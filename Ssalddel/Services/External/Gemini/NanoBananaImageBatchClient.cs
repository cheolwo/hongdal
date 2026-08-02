using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Services.Options;

namespace 살뜰.Services.External.Gemini;

public interface IAppContextImageBatchProviderClient
{
    string Model { get; }

    bool IsEnabled { get; }

    AppContextImageBatchCostEstimate Estimate(
        IReadOnlyList<AppContextImageBatchRequestItem> items);

    Task<AppContextImageBatchSubmission> SubmitAsync(
        string displayName,
        IReadOnlyList<AppContextImageBatchRequestItem> items,
        CancellationToken cancellationToken = default);

    Task<AppContextImageBatchStatus> GetAsync(
        string jobName,
        IReadOnlyList<string> expectedKeys,
        CancellationToken cancellationToken = default);
}

public sealed record AppContextImageBatchRequestItem(
    string Key,
    string Prompt,
    string AspectRatio,
    string Resolution);

public sealed record AppContextImageBatchCostEstimate(
    string Model,
    int ItemCount,
    decimal EstimatedOutputUsd,
    string PricingReferenceDate);

public sealed record AppContextImageBatchSubmission(
    string JobName,
    string InputFileName,
    string State,
    AppContextImageBatchCostEstimate CostEstimate);

public sealed record AppContextImageBatchResultItem(
    string Key,
    string? MimeType,
    byte[]? Bytes,
    string? Error);

public sealed record AppContextImageBatchStatus(
    string JobName,
    string State,
    string? OutputFileName,
    string? Error,
    IReadOnlyList<AppContextImageBatchResultItem> Results);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.AppContextImageBatch,
    SsalddelCodeLayer.ExternalAdapter,
    "Google Gemini Nano Banana 앱 문맥 이미지 JSONL Batch adapter",
    ContractType = typeof(IAppContextImageBatchProviderClient),
    FlowOrder = 60,
    Effects = SsalddelCodeEffect.NetworkCall
              | SsalddelCodeEffect.ThirdPartyApiCall
              | SsalddelCodeEffect.MayIncurExternalCost,
    Boundary = "API key는 서버 설정에서만 읽고 프롬프트 승인과 비용 확인 없이 호출하지 않으며 Base64 결과를 로그에 남기지 않습니다.")]
public sealed class NanoBananaImageBatchClient(
    HttpClient httpClient,
    IOptions<GeminiImageBatchOptions> options)
    : IAppContextImageBatchProviderClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> SupportedAspectRatios =
    [
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

    private readonly GeminiImageBatchOptions _options = options.Value;

    public string Model => _options.Model;

    public bool IsEnabled =>
        _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.ApiKey);

    public AppContextImageBatchCostEstimate Estimate(
        IReadOnlyList<AppContextImageBatchRequestItem> items)
    {
        ValidateItems(items);
        var unitPrice = Math.Max(
            0m,
            _options.EstimatedOutputUsdPerImage);
        return new AppContextImageBatchCostEstimate(
            _options.Model,
            items.Count,
            decimal.Round(unitPrice * items.Count, 4),
            string.IsNullOrWhiteSpace(_options.PricingReferenceDate)
                ? "unverified"
                : _options.PricingReferenceDate.Trim());
    }

    public async Task<AppContextImageBatchSubmission> SubmitAsync(
        string displayName,
        IReadOnlyList<AppContextImageBatchRequestItem> items,
        CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        var costEstimate = Estimate(items);
        var jsonLines = BuildJsonLines(items);
        if (jsonLines.Length > Math.Clamp(
                _options.MaxInputFileBytes,
                1 * 1024 * 1024,
                2_000_000_000))
        {
            throw new InvalidOperationException(
                "Gemini Batch JSONL 입력 파일이 설정된 최대 크기를 초과했습니다.");
        }

        var inputFileName = await UploadInputFileAsync(
            displayName,
            jsonLines,
            cancellationToken);
        var payload = new
        {
            batch = new
            {
                display_name = displayName.Trim(),
                input_config = new
                {
                    file_name = inputFileName
                }
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"models/{Uri.EscapeDataString(_options.Model)}:batchGenerateContent")
        {
            Content = JsonContent.Create(payload)
        };
        AddApiKey(request);
        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(
            cancellationToken);
        EnsureSuccess(response, responseText, "Batch 작업 제출");

        using var document = JsonDocument.Parse(responseText);
        var root = document.RootElement;
        var jobName = GetString(root, "name")
                      ?? GetNestedString(root, "metadata", "name")
                      ?? throw new InvalidOperationException(
                          "Gemini Batch 응답에 작업 이름이 없습니다.");
        return new AppContextImageBatchSubmission(
            jobName,
            inputFileName,
            ReadState(root) ?? "JOB_STATE_PENDING",
            costEstimate);
    }

    public async Task<AppContextImageBatchStatus> GetAsync(
        string jobName,
        IReadOnlyList<string> expectedKeys,
        CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        ValidateExpectedKeys(expectedKeys);
        var normalizedName = NormalizeResourceName(jobName, "batches/");
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            normalizedName);
        AddApiKey(request);
        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(
            cancellationToken);
        EnsureSuccess(response, responseText, "Batch 상태 조회");

        using var document = JsonDocument.Parse(responseText);
        var root = document.RootElement;
        var state = ReadState(root) ?? "JOB_STATE_UNSPECIFIED";
        var outputFileName =
            GetNestedString(root, "dest", "fileName")
            ?? GetNestedString(root, "dest", "file_name")
            ?? GetNestedString(root, "output", "responsesFile")
            ?? GetNestedString(root, "output", "responses_file")
            ?? GetNestedString(root, "response", "responsesFile")
            ?? GetNestedString(root, "response", "responses_file")
            ?? GetNestedString(
                root,
                "metadata",
                "output",
                "responsesFile")
            ?? GetNestedString(
                root,
                "metadata",
                "output",
                "responses_file");
        var error = ReadError(root);
        if (!IsSucceededState(state))
        {
            return new AppContextImageBatchStatus(
                normalizedName,
                state,
                outputFileName,
                error,
                []);
        }

        if (string.IsNullOrWhiteSpace(outputFileName))
        {
            throw new InvalidOperationException(
                "완료된 Gemini Batch 응답에 결과 파일이 없습니다.");
        }

        var resultBytes = await DownloadResultFileAsync(
            outputFileName,
            cancellationToken);
        return new AppContextImageBatchStatus(
            normalizedName,
            state,
            outputFileName,
            error,
            ParseResultJsonLines(resultBytes, expectedKeys));
    }

    internal byte[] BuildJsonLines(
        IReadOnlyList<AppContextImageBatchRequestItem> items)
    {
        ValidateItems(items);
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            leaveOpen: true);
        foreach (var item in items)
        {
            var line = new
            {
                key = item.Key.Trim(),
                request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new
                                {
                                    text = item.Prompt.Trim()
                                }
                            }
                        }
                    },
                    generation_config = new
                    {
                        response_modalities = new[] { "TEXT", "IMAGE" },
                        image_config = new
                        {
                            aspect_ratio = NormalizeAspectRatio(
                                item.AspectRatio),
                            image_size = NormalizeResolution(
                                item.Resolution)
                        }
                    }
                }
            };
            writer.WriteLine(JsonSerializer.Serialize(line, JsonOptions));
        }

        writer.Flush();
        return stream.ToArray();
    }

    internal IReadOnlyList<AppContextImageBatchResultItem>
        ParseResultJsonLines(
            byte[] jsonLines,
            IReadOnlyList<string> expectedKeys)
    {
        ArgumentNullException.ThrowIfNull(jsonLines);
        ValidateExpectedKeys(expectedKeys);
        var lines = Encoding.UTF8.GetString(jsonLines).Split(
            ['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries);
        var results = new List<AppContextImageBatchResultItem>(
            expectedKeys.Count);
        for (var index = 0; index < lines.Length; index++)
        {
            using var document = JsonDocument.Parse(lines[index]);
            var root = document.RootElement;
            var key = GetString(root, "key")
                      ?? GetNestedString(root, "metadata", "key")
                      ?? (index < expectedKeys.Count
                          ? expectedKeys[index]
                          : $"response-{index + 1}");
            if (TryReadLineError(root, out var lineError))
            {
                results.Add(new(key, null, null, lineError));
                continue;
            }

            var response = root.TryGetProperty(
                    "response",
                    out var responseElement)
                ? responseElement
                : root;
            var image = ReadGeneratedImage(response);
            results.Add(image is null
                ? new(
                    key,
                    null,
                    null,
                    "Gemini Batch 결과에서 생성 이미지를 찾지 못했습니다.")
                : new(
                    key,
                    image.Value.MimeType,
                    image.Value.Bytes,
                    null));
        }

        return results;
    }

    private async Task<string> UploadInputFileAsync(
        string displayName,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        var baseUri = new Uri(_options.BaseUrl, UriKind.Absolute);
        using var startRequest = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(baseUri, "/upload/v1beta/files"));
        AddApiKey(startRequest);
        startRequest.Headers.Add("X-Goog-Upload-Protocol", "resumable");
        startRequest.Headers.Add("X-Goog-Upload-Command", "start");
        startRequest.Headers.Add(
            "X-Goog-Upload-Header-Content-Length",
            bytes.Length.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        startRequest.Headers.Add(
            "X-Goog-Upload-Header-Content-Type",
            "application/jsonl");
        startRequest.Content = JsonContent.Create(new
        {
            file = new
            {
                display_name = displayName.Trim()
            }
        });

        using var startResponse = await httpClient.SendAsync(
            startRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var startBody = await startResponse.Content.ReadAsStringAsync(
            cancellationToken);
        EnsureSuccess(startResponse, startBody, "Batch 입력 파일 업로드 준비");
        if (!startResponse.Headers.TryGetValues(
                "X-Goog-Upload-URL",
                out var uploadUrls)
            || !Uri.TryCreate(
                uploadUrls.FirstOrDefault(),
                UriKind.Absolute,
                out var uploadUri)
            || uploadUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Gemini Files API가 HTTPS 업로드 URL을 반환하지 않았습니다.");
        }

        using var uploadRequest = new HttpRequestMessage(
            HttpMethod.Post,
            uploadUri);
        AddApiKey(uploadRequest);
        uploadRequest.Headers.Add("X-Goog-Upload-Offset", "0");
        uploadRequest.Headers.Add(
            "X-Goog-Upload-Command",
            "upload, finalize");
        uploadRequest.Content = new ByteArrayContent(bytes);
        uploadRequest.Content.Headers.ContentType =
            new MediaTypeHeaderValue("application/jsonl");
        using var uploadResponse = await httpClient.SendAsync(
            uploadRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync(
            cancellationToken);
        EnsureSuccess(uploadResponse, uploadBody, "Batch 입력 파일 업로드");
        using var document = JsonDocument.Parse(uploadBody);
        return GetNestedString(document.RootElement, "file", "name")
               ?? GetString(document.RootElement, "name")
               ?? throw new InvalidOperationException(
                   "Gemini Files API 응답에 파일 이름이 없습니다.");
    }

    private async Task<byte[]> DownloadResultFileAsync(
        string fileName,
        CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeResourceName(fileName, "files/");
        using var metadataRequest = new HttpRequestMessage(
            HttpMethod.Get,
            normalizedName);
        AddApiKey(metadataRequest);
        using var metadataResponse = await httpClient.SendAsync(
            metadataRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var metadataBody = await metadataResponse.Content.ReadAsStringAsync(
            cancellationToken);
        EnsureSuccess(
            metadataResponse,
            metadataBody,
            "Batch 결과 파일 정보 조회");

        using var document = JsonDocument.Parse(metadataBody);
        var downloadUriText = GetString(document.RootElement, "downloadUri")
                              ?? GetString(
                                  document.RootElement,
                                  "download_uri")
                              ?? throw new InvalidOperationException(
                                  "Gemini Batch 결과 파일에 다운로드 주소가 없습니다.");
        if (!Uri.TryCreate(
                downloadUriText,
                UriKind.Absolute,
                out var downloadUri)
            || downloadUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Gemini Batch 결과 다운로드 주소가 올바르지 않습니다.");
        }

        using var downloadRequest = new HttpRequestMessage(
            HttpMethod.Get,
            downloadUri);
        AddApiKey(downloadRequest);
        using var downloadResponse = await httpClient.SendAsync(
            downloadRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var bytes = await downloadResponse.Content.ReadAsByteArrayAsync(
            cancellationToken);
        EnsureSuccess(
            downloadResponse,
            string.Empty,
            "Batch 결과 파일 다운로드");
        return bytes;
    }

    private void ValidateItems(
        IReadOnlyList<AppContextImageBatchRequestItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var maximum = Math.Clamp(_options.MaxItemsPerBatch, 1, 1_000);
        if (items.Count is 0 || items.Count > maximum)
        {
            throw new ArgumentException(
                $"Gemini 이미지 Batch는 1개 이상 {maximum}개 이하 항목이어야 합니다.",
                nameof(items));
        }

        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Key);
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Prompt);
            if (!keys.Add(item.Key.Trim()))
            {
                throw new ArgumentException(
                    "Gemini 이미지 Batch key는 중복될 수 없습니다.",
                    nameof(items));
            }

            _ = NormalizeAspectRatio(item.AspectRatio);
            _ = NormalizeResolution(item.Resolution);
        }
    }

    private void ValidateExpectedKeys(IReadOnlyList<string> expectedKeys)
    {
        ArgumentNullException.ThrowIfNull(expectedKeys);
        if (expectedKeys.Count is 0
            || expectedKeys.Count > Math.Clamp(
                _options.MaxItemsPerBatch,
                1,
                1_000)
            || expectedKeys.Any(string.IsNullOrWhiteSpace)
            || expectedKeys.Distinct(StringComparer.Ordinal).Count()
            != expectedKeys.Count)
        {
            throw new ArgumentException(
                "Gemini Batch 결과 key 목록이 올바르지 않습니다.",
                nameof(expectedKeys));
        }
    }

    private string NormalizeResolution(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? "1K"
            : value.Trim().ToUpperInvariant();
        if (!SupportedResolutions.Contains(normalized))
        {
            throw new ArgumentException(
                "Gemini 이미지 크기는 512, 1K, 2K 또는 4K여야 합니다.",
                nameof(value));
        }

        if (_options.Model.Contains(
                "flash-lite-image",
                StringComparison.OrdinalIgnoreCase)
            && normalized != "1K")
        {
            throw new ArgumentException(
                "Gemini Flash Lite Image Batch는 1K 해상도만 지원합니다.",
                nameof(value));
        }

        return normalized;
    }

    private static string NormalizeAspectRatio(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        return SupportedAspectRatios.Contains(normalized)
            ? normalized
            : throw new ArgumentException(
                "지원하지 않는 Gemini 이미지 화면 비율입니다.",
                nameof(value));
    }

    private (string MimeType, byte[] Bytes)? ReadGeneratedImage(
        JsonElement response)
    {
        if (!response.TryGetProperty("candidates", out var candidates)
            || candidates.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var candidate in candidates.EnumerateArray())
        {
            if (!candidate.TryGetProperty("content", out var content)
                || !content.TryGetProperty("parts", out var parts)
                || parts.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in parts.EnumerateArray())
            {
                var inlineData = part.TryGetProperty(
                        "inlineData",
                        out var camelInlineData)
                    ? camelInlineData
                    : part.TryGetProperty(
                        "inline_data",
                        out var snakeInlineData)
                        ? snakeInlineData
                        : default;
                if (inlineData.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var mimeType = GetString(inlineData, "mimeType")
                               ?? GetString(inlineData, "mime_type");
                var encoded = GetString(inlineData, "data");
                if (mimeType is null || encoded is null)
                {
                    continue;
                }

                byte[] bytes;
                try
                {
                    bytes = Convert.FromBase64String(encoded);
                }
                catch (FormatException exception)
                {
                    throw new InvalidOperationException(
                        "Gemini Batch 이미지 Base64가 올바르지 않습니다.",
                        exception);
                }

                var maximumBytes = Math.Clamp(
                    _options.MaxGeneratedImageBytes,
                    1_048_576,
                    100 * 1024 * 1024);
                if (bytes.Length is 0 || bytes.Length > maximumBytes)
                {
                    throw new InvalidOperationException(
                        "Gemini Batch 이미지가 설정된 최대 크기를 초과했습니다.");
                }

                return (NormalizeMimeType(mimeType), bytes);
            }
        }

        return null;
    }

    private static bool TryReadLineError(
        JsonElement root,
        out string error)
    {
        if (root.TryGetProperty("error", out var errorElement))
        {
            error = LimitDetail(
                GetString(errorElement, "message")
                ?? errorElement.ToString());
            return true;
        }

        error = string.Empty;
        return false;
    }

    private void EnsureEnabled()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException(
                "Gemini Nano Banana 앱 이미지 Batch가 비활성화되어 있습니다.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "GeminiImageBatch:ApiKey 서버 설정이 필요합니다.");
        }
    }

    private void AddApiKey(HttpRequestMessage request)
        => request.Headers.Add("x-goog-api-key", _options.ApiKey);

    private static void EnsureSuccess(
        HttpResponseMessage response,
        string responseText,
        string operation)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = TryReadErrorMessage(responseText);
        var prefix = response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests =>
                "Gemini Batch 할당량 또는 속도 한도를 초과했습니다.",
            HttpStatusCode.Forbidden =>
                "Gemini Batch API key 권한과 결제 설정을 확인해야 합니다.",
            HttpStatusCode.Unauthorized =>
                "Gemini Batch API key가 올바르지 않습니다.",
            HttpStatusCode.BadRequest =>
                "Gemini Batch 요청이 올바르지 않습니다.",
            _ => $"{operation}에 실패했습니다."
        };
        throw new HttpRequestException(
            string.IsNullOrWhiteSpace(detail)
                ? prefix
                : $"{prefix} {detail}",
            null,
            response.StatusCode);
    }

    private static string? TryReadErrorMessage(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            var detail = GetNestedString(
                             document.RootElement,
                             "error",
                             "message")
                         ?? GetString(document.RootElement, "message");
            return string.IsNullOrWhiteSpace(detail)
                ? null
                : LimitDetail(detail);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string LimitDetail(string value)
    {
        var normalized = value.Trim();
        return normalized.Length <= 500
            ? normalized
            : normalized[..500];
    }

    private static string NormalizeResourceName(
        string value,
        string requiredPrefix)
    {
        var normalized = value?.Trim().TrimStart('/') ?? string.Empty;
        if (normalized.Length == 0
            || !normalized.StartsWith(
                requiredPrefix,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Gemini 리소스 이름은 {requiredPrefix}로 시작해야 합니다.",
                nameof(value));
        }

        return normalized;
    }

    private static string NormalizeMimeType(string value)
        => value?.Trim().ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => "image/jpeg",
            "image/png" => "image/png",
            "image/webp" => "image/webp",
            _ => throw new ArgumentException(
                "Gemini Batch 결과 이미지는 JPEG, PNG 또는 WebP여야 합니다.")
        };

    private static string? ReadState(JsonElement root)
        => GetString(root, "state")
           ?? GetNestedString(root, "metadata", "state")
           ?? GetNestedString(root, "batch", "state");

    private static bool IsSucceededState(string state)
        => state.Equals(
               "JOB_STATE_SUCCEEDED",
               StringComparison.OrdinalIgnoreCase)
           || state.Equals(
               "BATCH_STATE_SUCCEEDED",
               StringComparison.OrdinalIgnoreCase);

    private static string? ReadError(JsonElement root)
    {
        if (!root.TryGetProperty("error", out var error))
        {
            return null;
        }

        return LimitDetail(
            GetString(error, "message")
            ?? error.ToString());
    }

    private static string? GetString(
        JsonElement element,
        string propertyName)
        => element.ValueKind == JsonValueKind.Object
           && element.TryGetProperty(propertyName, out var property)
           && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string? GetNestedString(
        JsonElement element,
        string parentPropertyName,
        string childPropertyName)
        => element.ValueKind == JsonValueKind.Object
           && element.TryGetProperty(parentPropertyName, out var parent)
            ? GetString(parent, childPropertyName)
            : null;

    private static string? GetNestedString(
        JsonElement element,
        string parentPropertyName,
        string nestedPropertyName,
        string childPropertyName)
        => element.ValueKind == JsonValueKind.Object
           && element.TryGetProperty(parentPropertyName, out var parent)
           && parent.ValueKind == JsonValueKind.Object
           && parent.TryGetProperty(nestedPropertyName, out var nested)
            ? GetString(nested, childPropertyName)
            : null;
}
