using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 마트피킹ClientTests
{
    [Fact]
    public async Task 목록Client는_검색창고상태와페이징을인코딩하고404를숨기지않는다()
    {
        var api = new RecordingJsonApiClient { Response = new 마트피킹주문목록응답() };
        var client = new 마트피킹Client(api);

        await client.목록Async(new 마트피킹주문목록조회요청
        {
            검색어 = "생수 & 휴지",
            창고Id = 17,
            작업상태 = "진행중",
            Page = 2,
            PageSize = 12
        });

        Assert.Equal(
            "api/v1/warehouse-operations/mart/picking-orders?page=2&pageSize=12&검색어=%EC%83%9D%EC%88%98%20%26%20%ED%9C%B4%EC%A7%80&창고Id=17&작업상태=%EC%A7%84%ED%96%89%EC%A4%91",
            api.LastPath);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 상세Client는_정확한OrderId경로에서404만없음으로허용한다()
    {
        var api = new RecordingJsonApiClient { Response = null };
        var client = new 마트피킹Client(api);

        var result = await client.상세Async(73);

        Assert.Null(result);
        Assert.Equal("api/v1/warehouse-operations/mart/picking-orders/73", api.LastPath);
        Assert.True(api.LastAllowNotFound);
    }

    private sealed class RecordingJsonApiClient : ISsalddelJsonApiClient
    {
        public object? Response { get; init; }
        public string? LastPath { get; private set; }
        public bool LastAllowNotFound { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(string path, string operationName, bool allowNotFound = true, CancellationToken cancellationToken = default)
        {
            LastPath = path;
            LastAllowNotFound = allowNotFound;
            return Task.FromResult((TResponse?)Response);
        }

        public Task<TResponse?> SendAsync<TResponse>(HttpMethod method, string path, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TResponse?> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest request, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SendAsync(HttpMethod method, string path, string operationName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SendAsync<TRequest>(HttpMethod method, string path, TRequest request, string operationName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
