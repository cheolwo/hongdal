using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Services.Options;

namespace 살뜰.Services.External.Gemini;

public interface IImageGenerationProviderClient
{
    string Model { get; }

    bool IsEnabled { get; }

    Task<ImageGenerationProviderResult> GenerateAsync(
        ImageGenerationProviderRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record ImageGenerationProviderRequest(
    string Prompt,
    string AspectRatio,
    string? Resolution);

public sealed record ImageGenerationProviderResult(
    string ProviderTaskId,
    string Model,
    string ContentType,
    byte[] ImageBytes,
    string AuditJson);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityAuthoringImage,
    SsalddelCodeLayer.ExternalAdapter,
    "Google Gemini Nano Banana 이미지 생성 HTTP adapter",
    ContractType = typeof(IImageGenerationProviderClient),
    FlowOrder = 60,
    Effects = SsalddelCodeEffect.NetworkCall
              | SsalddelCodeEffect.ThirdPartyApiCall
              | SsalddelCodeEffect.MayIncurExternalCost,
    Boundary = "API key는 서버 설정에서만 읽고 Base64 원본 응답은 로그나 DB에 남기지 않으며 생성 호출은 외부 비용을 발생시킬 수 있습니다.")]
public sealed class NanoBananaImageGenerationClient(
    HttpClient httpClient,
    IOptions<GeminiImageOptions> options)
    : IImageGenerationProviderClient
{
    private const string TaskIdPrefix = "gemini-image:v1:";

    private static readonly HashSet<string> SupportedAspectRatios =
    [
        "auto",
        "1:1",
        "2:3",
        "3:2",
        "3:4",
        "4:3",
        "4:5",
        "5:4",
        "9:16",
        "16:9",
        "21:9"
    ];

    private static readonly HashSet<string> SupportedResolutions =
        ["512", "1K", "2K", "4K"];

    private readonly GeminiImageOptions _options = options.Value;

    public string Model => _options.Model;

    public bool IsEnabled =>
        _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<ImageGenerationProviderResult> GenerateAsync(
        ImageGenerationProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureEnabled();
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Prompt);

        var aspectRatio = NormalizeAspectRatio(request.AspectRatio);
        var resolution = NormalizeResolution(request.Resolution);
        var outputMimeType = NormalizeImageMimeType(_options.OutputMimeType);
        var responseFormat = new Dictionary<string, object?>
        {
            ["type"] = "image",
            ["mime_type"] = outputMimeType,
            ["image_size"] = resolution
        };
        if (aspectRatio != "auto")
        {
            responseFormat["aspect_ratio"] = aspectRatio;
        }

        var payload = new
        {
            model = _options.Model,
            input = new object[]
            {
                new
                {
                    type = "text",
                    text = request.Prompt.Trim()
                }
            },
            response_format = responseFormat
        };

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            _options.GeneratePath)
        {
            Content = JsonContent.Create(payload)
        };
        message.Headers.Add("x-goog-api-key", _options.ApiKey);

        using var response = await httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                BuildFailureMessage(response.StatusCode, responseText),
                inner: null,
                response.StatusCode);
        }

        var image = ParseGeneratedImage(responseText);
        EnsureImageSize(image.Bytes.Length);
        var taskId = TaskIdPrefix + Guid.NewGuid().ToString("N");
        var auditJson = JsonSerializer.Serialize(new
        {
            provider = "GoogleGemini",
            model = _options.Model,
            state = "success",
            contentType = image.ContentType,
            byteLength = image.Bytes.Length,
            aspectRatio,
            resolution
        });

        return new ImageGenerationProviderResult(
            taskId,
            _options.Model,
            image.ContentType,
            image.Bytes,
            auditJson);
    }

    private GeneratedImage ParseGeneratedImage(string responseText)
    {
        using var document = JsonDocument.Parse(responseText);
        if (!document.RootElement.TryGetProperty(
                "steps",
                out var steps)
            || steps.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                "Gemini 이미지 응답에 steps 배열이 없습니다.");
        }

        GeneratedImage? finalImage = null;
        foreach (var step in steps.EnumerateArray())
        {
            if (!step.TryGetProperty("type", out var stepType)
                || stepType.GetString() != "model_output"
                || !step.TryGetProperty("content", out var content)
                || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in content.EnumerateArray())
            {
                if (!part.TryGetProperty("type", out var contentType)
                    || contentType.GetString() != "image")
                {
                    continue;
                }

                var mimeType = NormalizeImageMimeType(
                    GetString(part, "mime_type", "mimeType"));
                var encoded = GetString(part, "data")
                    ?? throw new InvalidOperationException(
                        "Gemini 이미지 응답에 Base64 데이터가 없습니다.");
                try
                {
                    finalImage = new GeneratedImage(
                        mimeType,
                        Convert.FromBase64String(encoded));
                }
                catch (FormatException exception)
                {
                    throw new InvalidOperationException(
                        "Gemini 이미지 응답의 Base64 데이터가 올바르지 않습니다.",
                        exception);
                }
            }
        }

        return finalImage
               ?? throw new InvalidOperationException(
                   "Gemini 응답에서 생성된 이미지를 찾지 못했습니다.");
    }

    private void EnsureEnabled()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException(
                "Gemini Nano Banana 이미지 생성은 비활성화되어 있습니다.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "GeminiImage:ApiKey 서버 설정이 필요합니다.");
        }
    }

    private string NormalizeResolution(string? value)
    {
        var source = string.IsNullOrWhiteSpace(value)
                     || string.Equals(
                         value,
                         "provider-default",
                         StringComparison.OrdinalIgnoreCase)
            ? _options.DefaultResolution
            : value;
        var normalized = string.IsNullOrWhiteSpace(source)
            ? "1K"
            : source.Trim().ToUpperInvariant();
        return SupportedResolutions.Contains(normalized)
            ? normalized
            : throw new ArgumentException(
                "Gemini 이미지 크기는 512, 1K, 2K 또는 4K여야 합니다.",
                nameof(value));
    }

    private static string NormalizeAspectRatio(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? "auto"
            : value.Trim().ToLowerInvariant();
        return SupportedAspectRatios.Contains(normalized)
            ? normalized
            : throw new ArgumentException(
                "지원하지 않는 Gemini 이미지 화면 비율입니다.",
                nameof(value));
    }

    private static string NormalizeImageMimeType(string? value)
        => value?.Trim().ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => "image/jpeg",
            "image/png" => "image/png",
            "image/webp" => "image/webp",
            _ => throw new ArgumentException(
                "이미지는 JPEG, PNG 또는 WebP 형식이어야 합니다.")
        };

    private void EnsureImageSize(long size)
    {
        var limit = Math.Clamp(
            _options.MaxGeneratedImageBytes,
            1_048_576,
            100 * 1024 * 1024);
        if (size <= 0 || size > limit)
        {
            throw new InvalidOperationException(
                $"생성 이미지는 1바이트 이상 {limit / 1024 / 1024}MB 이하여야 합니다.");
        }
    }

    private static string? GetString(
        JsonElement element,
        params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.GetString();
            }
        }

        return null;
    }

    private static string BuildFailureMessage(
        HttpStatusCode statusCode,
        string payload)
    {
        var detail = TryReadErrorMessage(payload);
        var prefix = statusCode switch
        {
            HttpStatusCode.TooManyRequests =>
                "Gemini 이미지 API 할당량 또는 요청 속도 한도를 초과했습니다.",
            HttpStatusCode.Forbidden =>
                "Gemini 이미지 API 키 권한 또는 결제 설정을 확인해야 합니다.",
            HttpStatusCode.BadRequest =>
                "Gemini 이미지 API 요청이 올바르지 않습니다.",
            _ =>
                $"Gemini 이미지 API 응답에 실패했습니다. HTTP {(int)statusCode}"
        };
        return string.IsNullOrWhiteSpace(detail)
            ? prefix
            : $"{prefix} {detail}";
    }

    private static string? TryReadErrorMessage(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            if (!document.RootElement.TryGetProperty(
                    "error",
                    out var error)
                || !error.TryGetProperty("message", out var message))
            {
                return null;
            }

            var value = message.GetString()?.Trim();
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Length <= 500
                    ? value
                    : value[..500];
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record GeneratedImage(
        string ContentType,
        byte[] Bytes);
}
