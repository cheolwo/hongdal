using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 공동수입준비주문자ClientTests
{
    [Fact]
    public async Task 조회는_원장경로와원천자동집단Query를인코딩하고_NotFound를허용한다()
    {
        var response = new 공동수입준비주문자조회응답 { 상품명 = "쌀" };
        var api = new RecordingApiClient(response);
        var client = new 공동수입준비주문자Client(api);

        var result = await client.조회Async("ledger/1", "group/1");

        Assert.Same(response, result);
        Assert.Equal(
            "api/v1/orderer/group-imports/ledger%2F1/readiness?autoGroupId=group%2F1",
            api.Path);
        Assert.True(api.AllowNotFound);
    }

    private sealed class RecordingApiClient(object response) : ISsalddelJsonApiClient
    {
        public string Path { get; private set; } = string.Empty;
        public bool AllowNotFound { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
        {
            Path = path;
            AllowNotFound = allowNotFound;
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
