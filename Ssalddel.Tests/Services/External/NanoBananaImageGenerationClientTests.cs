using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Xunit;
using 살뜰.Services.External.Gemini;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External;

public sealed class NanoBananaImageGenerationClientTests
{
    private static readonly byte[] ImageBytes = [1, 2, 3, 4, 5];

    [Fact]
    public async Task GenerateAsync_UsesGeminiInteractionContractAndParsesImage()
    {
        var handler = new RecordingHandler(_ => ImageResponse("image/png"));
        var client = CreateClient(handler);

        var result = await client.GenerateAsync(
            new ImageGenerationProviderRequest(
                "A community gathering around a shared table",
                "3:2",
                "2K"));

        Assert.Equal(ImageBytes, result.ImageBytes);
        Assert.Equal("image/png", result.ContentType);
        Assert.Equal("gemini-3.1-flash-image", result.Model);
        Assert.StartsWith("gemini-image:v1:", result.ProviderTaskId);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/v1beta/interactions", handler.RequestUri?.AbsolutePath);
        Assert.Equal("test-api-key", handler.ApiKey);

        using var document = JsonDocument.Parse(handler.Body!);
        var root = document.RootElement;
        Assert.Equal("gemini-3.1-flash-image", root.GetProperty("model").GetString());
        Assert.Equal(
            "A community gathering around a shared table",
            root.GetProperty("input")[0].GetProperty("text").GetString());
        var format = root.GetProperty("response_format");
        Assert.Equal("image/jpeg", format.GetProperty("mime_type").GetString());
        Assert.Equal("2K", format.GetProperty("image_size").GetString());
        Assert.Equal("3:2", format.GetProperty("aspect_ratio").GetString());
        Assert.DoesNotContain(Convert.ToBase64String(ImageBytes), result.AuditJson);
    }

    [Fact]
    public async Task GenerateAsync_UsesDefaultResolutionAndOmitsAutoAspectRatio()
    {
        var handler = new RecordingHandler(_ => ImageResponse("image/jpeg"));
        var client = CreateClient(handler);

        await client.GenerateAsync(
            new ImageGenerationProviderRequest(
                "A neighborhood group preparing a shared food order",
                "auto",
                "provider-default"));

        using var document = JsonDocument.Parse(handler.Body!);
        var format = document.RootElement.GetProperty("response_format");
        Assert.Equal("1K", format.GetProperty("image_size").GetString());
        Assert.False(format.TryGetProperty("aspect_ratio", out _));
    }

    [Fact]
    public async Task GenerateAsync_WhenDisabled_DoesNotCallProvider()
    {
        var handler = new RecordingHandler(_ => ImageResponse("image/png"));
        var client = CreateClient(handler, enabled: false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GenerateAsync(
                new ImageGenerationProviderRequest("prompt", "1:1", "1K")));

        Assert.Contains("비활성화", exception.Message);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_WhenImageIsMissing_ReturnsClearError()
    {
        var handler = new RecordingHandler(_ => JsonResponse(
            """
            {"steps":[{"type":"model_output","content":[{"type":"text","text":"no image"}]}]}
            """));
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GenerateAsync(
                new ImageGenerationProviderRequest("prompt", "1:1", "1K")));

        Assert.Contains("생성된 이미지를 찾지 못했습니다", exception.Message);
    }

    private static NanoBananaImageGenerationClient CreateClient(
        HttpMessageHandler handler,
        bool enabled = true)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(
                "https://generativelanguage.googleapis.com/v1beta/")
        };
        return new NanoBananaImageGenerationClient(
            httpClient,
            Options.Create(new GeminiImageOptions
            {
                Enabled = enabled,
                ApiKey = "test-api-key",
                Model = "gemini-3.1-flash-image",
                GeneratePath = "interactions",
                DefaultResolution = "1K",
                OutputMimeType = "image/jpeg"
            }));
    }

    private static HttpResponseMessage ImageResponse(string contentType)
        => JsonResponse(
            $$"""
            {
              "steps": [
                {
                  "type": "model_output",
                  "content": [
                    {
                      "type": "image",
                      "mime_type": "{{contentType}}",
                      "data": "{{Convert.ToBase64String(ImageBytes)}}"
                    }
                  ]
                }
              ]
            }
            """);

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? ApiKey { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            ApiKey = request.Headers.TryGetValues(
                    "x-goog-api-key",
                    out var values)
                ? values.SingleOrDefault()
                : null;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return responseFactory(request);
        }
    }
}
