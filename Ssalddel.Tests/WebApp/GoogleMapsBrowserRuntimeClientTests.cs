using System.Net;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Platform;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class GoogleMapsBrowserRuntimeClientTests
{
    [Fact]
    public async Task 한Scope에서_Runtime설정을한번만요청한다()
    {
        var handler = new CountingHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7117/")
        };
        var client = new GoogleMapsBrowserRuntimeClient(httpClient);

        var first = await client.TryGetAsync();
        var second = await client.TryGetAsync();

        Assert.NotNull(first);
        Assert.Same(first, second);
        Assert.Equal(1, handler.RequestCount);
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new GoogleMapsBrowserRuntimeResponse
                {
                    BrowserApiKey = "AIza" + new string('a', 35),
                    AllowedOrigins = ["http://localhost:5238"]
                })
            });
        }
    }
}
