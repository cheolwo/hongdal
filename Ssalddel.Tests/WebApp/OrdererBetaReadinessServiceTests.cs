using System.Net;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class OrdererBetaReadinessServiceTests
{
    [Fact]
    public async Task CheckAsync_Ready응답이면_점검가능상태를_반환한다()
    {
        using var httpClient = new HttpClient(new StubHandler(HttpStatusCode.OK))
        {
            BaseAddress = new Uri("https://preview.example/")
        };
        var service = new OrdererBetaReadinessService(httpClient);

        var result = await service.CheckAsync();

        Assert.True(result.IsReady);
        Assert.Contains("응답", result.Message);
    }

    [Fact]
    public async Task CheckAsync_비정상응답이면_배포화면에서_경고할수있다()
    {
        using var httpClient = new HttpClient(new StubHandler(HttpStatusCode.ServiceUnavailable))
        {
            BaseAddress = new Uri("https://preview.example/")
        };
        var service = new OrdererBetaReadinessService(httpClient);

        var result = await service.CheckAsync();

        Assert.False(result.IsReady);
        Assert.Contains("503", result.Message);
    }

    private sealed class StubHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Assert.Equal("https://preview.example/health/ready", request.RequestUri?.ToString());
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
