using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 공동구매내원함ClientTests
{
    [Fact]
    public async Task 내원함목록은_로그인주문자전용_GET경로로조회한다()
    {
        var response = new 공동구매내원함목록응답
        {
            전체건수 = 1,
            활성건수 = 1
        };
        var api = new RecordingApiClient(response);
        var client = new 공동구매내원함Client(api);
        using var cancellation = new CancellationTokenSource();

        var result = await client.내원함목록조회Async(cancellation.Token);

        Assert.Same(response, result);
        Assert.Equal("api/v1/orderer/group-purchase-wishes/me", api.Path);
        Assert.Equal("내 공동구매 원함 목록 조회", api.OperationName);
        Assert.True(api.AllowNotFound);
        Assert.Equal(cancellation.Token, api.CancellationToken);
    }

    private sealed class RecordingApiClient(object response) : ISsalddelJsonApiClient
    {
        public string Path { get; private set; } = string.Empty;
        public string OperationName { get; private set; } = string.Empty;
        public bool AllowNotFound { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
        {
            Path = path;
            OperationName = operationName;
            AllowNotFound = allowNotFound;
            CancellationToken = cancellationToken;
            return Task.FromResult((TResponse?)response);
        }

        public Task<TResponse?> SendAsync<TResponse>(
            HttpMethod method,
            string path,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TResponse?> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SendAsync(
            HttpMethod method,
            string path,
            string operationName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SendAsync<TRequest>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
