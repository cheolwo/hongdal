using Ssalddel.Contracts.Food;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 음식주문ClientTests
{
    [Fact]
    public async Task 목록Client는_검색상태와페이지를인코딩하고404를숨기지않는다()
    {
        var api = new RecordingJsonApiClient { Response = new 주문자음식주문목록응답() };
        var client = new 주문자음식주문Client(api);

        await client.목록Async(new 주문자음식주문목록조회요청
        {
            검색어 = "김밥 & 분식",
            상태 = 음식주문상태코드.조리중,
            Page = 2,
            PageSize = 12
        });

        Assert.Equal(
            "api/v1/food-orders?page=2&pageSize=12&검색어=%EA%B9%80%EB%B0%A5%20%26%20%EB%B6%84%EC%8B%9D&상태=%EC%A1%B0%EB%A6%AC%EC%A4%91",
            api.LastPath);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 상세Client는_정확한OrderNo경로에서404만없음으로허용한다()
    {
        var api = new RecordingJsonApiClient { Response = null };
        var client = new 주문자음식주문Client(api);

        var result = await client.상세Async("FOOD A/01");

        Assert.Null(result);
        Assert.Equal("api/v1/food-orders/FOOD%20A%2F01", api.LastPath);
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
