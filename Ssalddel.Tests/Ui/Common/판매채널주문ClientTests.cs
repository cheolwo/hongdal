using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 판매채널주문ClientTests
{
    [Fact]
    public async Task 목록Client는_검색조건을인코딩하고404를빈목록으로숨기지않는다()
    {
        var api = new RecordingJsonApiClient { Response = new 판매채널주문목록응답() };
        var client = new 판매채널주문Client(api);

        await client.목록조회Async(new 판매채널주문목록조회요청
        {
            Page = 1,
            PageSize = 25,
            Search = "캠핑 & 의자",
            SyncScope = CommerceChannelOrderSyncScopes.Overseas,
            Status = "출고예정"
        });

        Assert.Equal(
            "api/v1/sales-channels/orders?page=1&pageSize=25&search=%EC%BA%A0%ED%95%91%20%26%20%EC%9D%98%EC%9E%90&syncScope=Overseas&status=%EC%B6%9C%EA%B3%A0%EC%98%88%EC%A0%95",
            api.LastPath);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 상세Client는_정확한OrderId경로에서404만없음으로허용한다()
    {
        var api = new RecordingJsonApiClient { Response = null };
        var client = new 판매채널주문Client(api);

        var result = await client.상세조회Async(73);

        Assert.Null(result);
        Assert.Equal("api/v1/sales-channels/orders/73", api.LastPath);
        Assert.True(api.LastAllowNotFound);
    }

    private sealed class RecordingJsonApiClient : ISsalddelJsonApiClient
    {
        public object? Response { get; init; }
        public string? LastPath { get; private set; }
        public bool LastAllowNotFound { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
        {
            LastPath = path;
            LastAllowNotFound = allowNotFound;
            return Task.FromResult((TResponse?)Response);
        }

        public Task<TResponse?> SendAsync<TResponse>(HttpMethod method, string path, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TResponse?> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest request, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SendAsync(HttpMethod method, string path, string operationName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SendAsync<TRequest>(HttpMethod method, string path, TRequest request, string operationName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
