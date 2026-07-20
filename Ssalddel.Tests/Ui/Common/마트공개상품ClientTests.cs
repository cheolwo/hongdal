using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 마트공개상품ClientTests
{
    [Fact]
    public async Task 목록Client는_검색조건을인코딩하고404를숨기지않는다()
    {
        var api = new RecordingJsonApiClient { Response = new 마트공개상품목록응답() };
        var client = new 마트공개상품Client(api);

        await client.목록Async(new 마트공개상품목록조회요청
        {
            검색어 = "생수 & 생활",
            판매가능만 = true,
            Page = 2,
            PageSize = 12
        });

        Assert.Equal(
            "api/v1/orderer/mart/products?판매가능만=true&page=2&pageSize=12&검색어=%EC%83%9D%EC%88%98%20%26%20%EC%83%9D%ED%99%9C",
            api.LastPath);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 상세Client는_정확한ProductId경로에서404만없음으로허용한다()
    {
        var api = new RecordingJsonApiClient { Response = null };
        var client = new 마트공개상품Client(api);

        var result = await client.상세Async(41);

        Assert.Null(result);
        Assert.Equal("api/v1/orderer/mart/products/41", api.LastPath);
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
