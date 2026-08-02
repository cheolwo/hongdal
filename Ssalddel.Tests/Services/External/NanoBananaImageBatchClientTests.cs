using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using 살뜰.Services.External.Gemini;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External;

public sealed class NanoBananaImageBatchClientTests
{
    private static readonly byte[] ImageBytes = [1, 2, 3, 4, 5];

    [Fact]
    public void BuildJsonLines_AllowsVariableItemsAndUsesGenerateContentImageConfig()
    {
        var handler = new RecordingHandler([]);
        var client = CreateClient(handler);

        var jsonLines = Encoding.UTF8.GetString(client.BuildJsonLines(
        [
            Item("community-shipper--scene-01", "16:9"),
            Item("community-shipper--scene-02", "4:3")
        ]));
        var lines = jsonLines.Split(
            ['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        using var first = JsonDocument.Parse(lines[0]);
        var root = first.RootElement;
        Assert.Equal(
            "community-shipper--scene-01",
            root.GetProperty("key").GetString());
        var config = root.GetProperty("request")
            .GetProperty("generation_config");
        Assert.Equal(
            ["TEXT", "IMAGE"],
            config.GetProperty("response_modalities")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray());
        var image = config.GetProperty("image_config");
        Assert.Equal("16:9", image.GetProperty("aspect_ratio").GetString());
        Assert.Equal("1K", image.GetProperty("image_size").GetString());
    }

    [Fact]
    public void Estimate_UsesConfiguredBatchUnitPrice()
    {
        var client = CreateClient(new RecordingHandler([]));

        var estimate = client.Estimate(
        [
            Item("pack--scene-01", "1:1"),
            Item("pack--scene-02", "3:4"),
            Item("pack--scene-03", "4:3")
        ]);

        Assert.Equal("gemini-3.1-flash-lite-image", estimate.Model);
        Assert.Equal(3, estimate.ItemCount);
        Assert.Equal(0.0504m, estimate.EstimatedOutputUsd);
        Assert.Equal("2026-08-01", estimate.PricingReferenceDate);
    }

    [Fact]
    public async Task SubmitAsync_UploadsJsonlAndCreatesBatchWithoutLoggingKey()
    {
        var start = JsonResponse("{}");
        start.Headers.Add(
            "X-Goog-Upload-URL",
            "https://upload.example.test/session/1");
        var handler = new RecordingHandler(
        [
            start,
            JsonResponse("""{"file":{"name":"files/input-1"}}"""),
            JsonResponse(
                """{"name":"batches/job-1","metadata":{"state":"JOB_STATE_PENDING"}}""")
        ]);
        var client = CreateClient(handler);

        var result = await client.SubmitAsync(
            "ssalddel-community-shipper-v1",
            [Item("community-shipper--scene-01", "16:9")]);

        Assert.Equal("batches/job-1", result.JobName);
        Assert.Equal("files/input-1", result.InputFileName);
        Assert.Equal("JOB_STATE_PENDING", result.State);
        Assert.Equal(3, handler.Requests.Count);
        Assert.All(
            handler.Requests,
            request => Assert.Equal("test-api-key", request.ApiKey));
        Assert.Equal(
            "/v1beta/models/gemini-3.1-flash-lite-image:batchGenerateContent",
            handler.Requests[2].Uri.AbsolutePath);
        Assert.DoesNotContain(
            "test-api-key",
            string.Join("\n", handler.Requests.Select(request => request.Body)));
    }

    [Fact]
    public async Task GetAsync_DownloadsResultJsonlAndParsesImage()
    {
        var resultLine = JsonSerializer.Serialize(new
        {
            key = "pack--scene-01",
            response = new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = "image/png",
                                        data = Convert.ToBase64String(ImageBytes)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });
        var handler = new RecordingHandler(
        [
            JsonResponse(
                """{"name":"batches/job-1","metadata":{"state":"BATCH_STATE_SUCCEEDED","output":{"responsesFile":"files/result-1"}}}"""),
            JsonResponse(
                """{"name":"files/result-1","downloadUri":"https://download.example.test/result-1"}"""),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes(resultLine))
            }
        ]);
        var client = CreateClient(handler);

        var status = await client.GetAsync(
            "batches/job-1",
            ["pack--scene-01"]);

        Assert.Equal("BATCH_STATE_SUCCEEDED", status.State);
        var result = Assert.Single(status.Results);
        Assert.Equal("pack--scene-01", result.Key);
        Assert.Equal("image/png", result.MimeType);
        Assert.Equal(ImageBytes, result.Bytes);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task SubmitAsync_WhenDisabled_DoesNotCallProvider()
    {
        var handler = new RecordingHandler([]);
        var client = CreateClient(handler, enabled: false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.SubmitAsync(
                "disabled",
                [Item("pack--scene-01", "1:1")]));

        Assert.Contains("비활성화", exception.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public void Estimate_RejectsDuplicateKeysAndLiteNon1KResolution()
    {
        var client = CreateClient(new RecordingHandler([]));

        Assert.Throws<ArgumentException>(() => client.Estimate(
        [
            Item("duplicate", "1:1"),
            Item("duplicate", "16:9")
        ]));
        Assert.Throws<ArgumentException>(() => client.Estimate(
        [
            new AppContextImageBatchRequestItem(
                "pack--scene-01",
                "A detailed safe application context scene without text or logos",
                "16:9",
                "2K")
        ]));
    }

    private static AppContextImageBatchRequestItem Item(
        string key,
        string aspectRatio)
        => new(
            key,
            "A detailed safe application context scene without readable text, logos, personal data or evidence-like documents",
            aspectRatio,
            "1K");

    private static NanoBananaImageBatchClient CreateClient(
        HttpMessageHandler handler,
        bool enabled = true)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(
                "https://generativelanguage.googleapis.com/v1beta/")
        };
        return new NanoBananaImageBatchClient(
            httpClient,
            Options.Create(new GeminiImageBatchOptions
            {
                Enabled = enabled,
                ApiKey = "test-api-key",
                Model = "gemini-3.1-flash-lite-image",
                MaxItemsPerBatch = 50,
                EstimatedOutputUsdPerImage = 0.0168m,
                PricingReferenceDate = "2026-08-01"
            }));
    }

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class RecordingHandler(
        IEnumerable<HttpResponseMessage> responses)
        : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri!,
                request.Headers.TryGetValues(
                    "x-goog-api-key",
                    out var values)
                    ? values.SingleOrDefault()
                    : null,
                request.Content is null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync(
                        cancellationToken)));
            return _responses.Count > 0
                ? _responses.Dequeue()
                : throw new InvalidOperationException(
                    "테스트 HTTP 응답이 준비되지 않았습니다.");
        }
    }

    private sealed record RecordedRequest(
        HttpMethod Method,
        Uri Uri,
        string? ApiKey,
        string Body);
}
