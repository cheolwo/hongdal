using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace 홍달.Services.External.KieAi;

public interface IKieAiImageGenerationClient
{
    Task<KieAiCreateTaskResult> CreateTextToImageTaskAsync(KieAiCreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<KieAiTaskDetailResult> GetTaskDetailAsync(string taskId, CancellationToken cancellationToken = default);
    Task<Stream> DownloadImageAsync(string imageUrl, CancellationToken cancellationToken = default);
}

public sealed class KieAiImageGenerationClient : IKieAiImageGenerationClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly KieAiOptions _options;

    public KieAiImageGenerationClient(HttpClient httpClient, IOptions<KieAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<KieAiCreateTaskResult> CreateTextToImageTaskAsync(KieAiCreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();
        ApplyAuthorization();

        using var response = await _httpClient.PostAsJsonAsync(
            _options.CreateTaskPath,
            new KieAiCreateTaskHttpRequest(
                _options.Model,
                request.CallBackUrl,
                new KieAiTextToImageInput(request.Prompt, request.AspectRatio, request.Resolution)),
            cancellationToken);

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var payload = JsonSerializer.Deserialize<KieAiCreateTaskResponse>(rawJson, JsonOptions);

        if (!response.IsSuccessStatusCode || payload?.Data?.TaskId is null)
        {
            throw new InvalidOperationException($"Kie.AI task creation failed. Status={(int)response.StatusCode}, Body={rawJson}");
        }

        return new KieAiCreateTaskResult(payload.Data.TaskId, rawJson);
    }

    public async Task<KieAiTaskDetailResult> GetTaskDetailAsync(string taskId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            throw new ArgumentException("taskId is required.", nameof(taskId));
        }

        EnsureApiKey();
        ApplyAuthorization();

        var path = _options.GetTaskPathTemplate.Replace("{taskId}", Uri.EscapeDataString(taskId), StringComparison.Ordinal);
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Kie.AI task detail request failed. Status={(int)response.StatusCode}, Body={rawJson}");
        }

        var payload = JsonSerializer.Deserialize<KieAiTaskDetailResponse>(rawJson, JsonOptions);
        return KieAiTaskDetailResult.From(payload, rawJson);
    }

    public async Task<Stream> DownloadImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new ArgumentException("imageUrl is required.", nameof(imageUrl));
        }

        using var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var memory = new MemoryStream();
        await response.Content.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;
        return memory;
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("KieAi:ApiKey configuration is required.");
        }
    }

    private void ApplyAuthorization()
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }
}

public sealed record KieAiCreateTaskRequest(string Prompt, string AspectRatio, string Resolution, string? CallBackUrl);
public sealed record KieAiCreateTaskResult(string TaskId, string RawJson);

public sealed class KieAiTaskDetailResult
{
    public string? TaskId { get; init; }
    public string? Status { get; init; }
    public string? ImageUrl { get; init; }
    public string RawJson { get; init; } = string.Empty;
    public bool IsTerminal => string.Equals(Status, "success", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(Status, "failed", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(Status, "error", StringComparison.OrdinalIgnoreCase);
    public bool IsSuccess => string.Equals(Status, "success", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(Status, "succeeded", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(Status, "completed", StringComparison.OrdinalIgnoreCase);

    public static KieAiTaskDetailResult From(KieAiTaskDetailResponse? response, string rawJson)
    {
        var data = response?.Data;
        var imageUrl = data?.Result?.FirstOrDefault()?.Url
                       ?? data?.Images?.FirstOrDefault()?.Url
                       ?? data?.ImageUrl;

        return new KieAiTaskDetailResult
        {
            TaskId = data?.TaskId,
            Status = data?.Status ?? response?.Msg,
            ImageUrl = imageUrl,
            RawJson = rawJson
        };
    }
}

public sealed record KieAiCreateTaskHttpRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("callBackUrl")] string? CallBackUrl,
    [property: JsonPropertyName("input")] KieAiTextToImageInput Input);

public sealed record KieAiTextToImageInput(
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("aspect_ratio")] string AspectRatio,
    [property: JsonPropertyName("resolution")] string Resolution);

public sealed class KieAiCreateTaskResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    [JsonPropertyName("data")]
    public KieAiCreateTaskResponseData? Data { get; set; }
}

public sealed class KieAiCreateTaskResponseData
{
    [JsonPropertyName("taskId")]
    public string? TaskId { get; set; }
}

public sealed class KieAiTaskDetailResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    [JsonPropertyName("data")]
    public KieAiTaskDetailResponseData? Data { get; set; }
}

public sealed class KieAiTaskDetailResponseData
{
    [JsonPropertyName("taskId")]
    public string? TaskId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("result")]
    public List<KieAiTaskImageItem>? Result { get; set; }

    [JsonPropertyName("images")]
    public List<KieAiTaskImageItem>? Images { get; set; }
}

public sealed class KieAiTaskImageItem
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
