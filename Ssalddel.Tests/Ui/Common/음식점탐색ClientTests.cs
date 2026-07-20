using Ssalddel.Contracts.Restaurants;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 음식점탐색ClientTests
{
    [Fact]
    public async Task 목록Client는_권역과검색조건을인코딩하고404를숨기지않는다()
    {
        var api = new RecordingJsonApiClient { Response = new 음식점공개목록응답() };
        var client = new 음식점공개Client(api);

        await client.목록Async(new 음식점공개목록조회요청
        {
            배달권키 = "bjd-sigungu:11500",
            반경Km = 7.5m,
            검색어 = "김밥 & 분식",
            주문가능만 = true,
            Page = 2,
            PageSize = 12
        });

        Assert.Equal(
            "api/v1/orderer/restaurants?배달권키=bjd-sigungu%3A11500&반경Km=7.5&주문가능만=true&page=2&pageSize=12&검색어=%EA%B9%80%EB%B0%A5%20%26%20%EB%B6%84%EC%8B%9D",
            api.LastPath);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 상세Client는_정확한RestaurantId경로에서404만없음으로허용한다()
    {
        var api = new RecordingJsonApiClient { Response = null };
        var client = new 음식점공개Client(api);

        var result = await client.상세Async(31);

        Assert.Null(result);
        Assert.Equal("api/v1/orderer/restaurants/31", api.LastPath);
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
