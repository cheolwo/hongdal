using System.Net;
using System.Text.Json;
using Ssalddel.Client.Infrastructure.Notifications;

namespace Ssalddel.Tests.ClientInfrastructure;

public sealed class SsalddelMobilePushInstallationClientTests
{
    [Fact]
    public async Task 토큰공급자가비어있으면_서버를호출하지않고안전하게건너뛴다()
    {
        var handler = new RecordingHandler();
        var client = new SsalddelMobilePushInstallationClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") },
            new NullSsalddelMobilePushTokenProvider(),
            () => "access-token");

        var result = await client.EnsureRegisteredAsync();

        Assert.Equal(SsalddelMobilePushRegistrationState.TokenNotAvailable, result.State);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task 토큰과인증이있으면_공통설치Endpoint에보호요청을보낸다()
    {
        var handler = new RecordingHandler();
        var client = new SsalddelMobilePushInstallationClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") },
            new TestPushTokenProvider(),
            () => "access-token");

        var result = await client.EnsureRegisteredAsync();

        Assert.Equal(SsalddelMobilePushRegistrationState.Registered, result.State);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(HttpMethod.Put, handler.LastRequestMethod);
        Assert.Equal(
            "/api/v1/mobile/push/installations",
            handler.LastRequestUri?.AbsolutePath);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("access-token", handler.AuthorizationParameter);
        Assert.Equal("ssalddel.shipper", handler.PayloadAppKey);
    }

    private sealed class TestPushTokenProvider : ISsalddelMobilePushTokenProvider
    {
        public Task<SsalddelMobilePushTokenSnapshot?> GetCurrentAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<SsalddelMobilePushTokenSnapshot?>(new(
                "installation-1",
                "ssalddel.shipper",
                "android",
                "push-token",
                "1.0.0",
                "test-device"));
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public HttpMethod? LastRequestMethod { get; private set; }
        public Uri? LastRequestUri { get; private set; }
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        public string? PayloadAppKey { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequestMethod = request.Method;
            LastRequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;

            if (request.Content is not null)
            {
                using var payload = JsonDocument.Parse(
                    await request.Content.ReadAsStringAsync(cancellationToken));
                PayloadAppKey = payload.RootElement.GetProperty("appKey").GetString();
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
