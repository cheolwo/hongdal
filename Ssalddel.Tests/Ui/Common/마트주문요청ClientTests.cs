using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 마트주문요청ClientTests
{
    [Fact]
    public async Task 등록Client는_보호된고정경로로요청을전송한다()
    {
        var response = new 마트주문요청응답 { 주문요청Id = Guid.NewGuid() };
        var api = new RecordingJsonApiClient { Response = response };
        var client = new 마트주문요청Client(api);
        var request = new 마트주문요청등록요청 { 클라이언트요청Id = Guid.NewGuid() };

        var result = await client.등록Async(request);

        Assert.Same(response, result);
        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Equal("api/v1/orderer/mart/order-requests", api.LastPath);
        Assert.Same(request, api.LastRequest);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 상세Client는_정확한요청Id의404만없음으로허용한다()
    {
        var requestId = Guid.NewGuid();
        var api = new RecordingJsonApiClient { Response = null };
        var client = new 마트주문요청Client(api);

        var result = await client.상세Async(requestId);

        Assert.Null(result);
        Assert.Equal($"api/v1/orderer/mart/order-requests/{requestId:D}", api.LastPath);
        Assert.True(api.LastAllowNotFound);
    }

    private sealed class RecordingJsonApiClient : ISsalddelJsonApiClient
    {
        public object? Response { get; init; }
        public HttpMethod? LastMethod { get; private set; }
        public string? LastPath { get; private set; }
        public object? LastRequest { get; private set; }
        public bool LastAllowNotFound { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(string path, string operationName, bool allowNotFound = true, CancellationToken cancellationToken = default)
        {
            LastMethod = HttpMethod.Get;
            LastPath = path;
            LastAllowNotFound = allowNotFound;
            return Task.FromResult((TResponse?)Response);
        }

        public Task<TResponse?> SendAsync<TResponse>(HttpMethod method, string path, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<TResponse?> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest request, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default)
        {
            LastMethod = method;
            LastPath = path;
            LastRequest = request;
            LastAllowNotFound = allowNotFound;
            return Task.FromResult((TResponse?)Response);
        }

        public Task SendAsync(HttpMethod method, string path, string operationName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SendAsync<TRequest>(HttpMethod method, string path, TRequest request, string operationName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
