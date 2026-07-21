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

    [Fact]
    public async Task 후기Client는_정확한상품하위경로에Post하고404를숨기지않는다()
    {
        var response = new 마트공개상품구매후기응답 { 게시글Id = 92, 제목 = "구매 후기" };
        var api = new RecordingJsonApiClient { Response = response };
        var client = new 마트공개상품후기Client(api);
        var request = new 마트공개상품구매후기작성요청
        {
            작성자표시명 = "구매자",
            글비밀번호 = "1234",
            제목 = "구매 후기",
            본문 = "좋았습니다."
        };

        var result = await client.작성Async(41, request);

        Assert.Same(response, result);
        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Equal("api/v1/orderer/mart/products/41/reviews", api.LastPath);
        Assert.False(api.LastAllowNotFound);
        Assert.Same(request, api.LastRequest);
    }

    private sealed class RecordingJsonApiClient : ISsalddelJsonApiClient
    {
        public object? Response { get; init; }
        public string? LastPath { get; private set; }
        public bool LastAllowNotFound { get; private set; }
        public HttpMethod? LastMethod { get; private set; }
        public object? LastRequest { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(string path, string operationName, bool allowNotFound = true, CancellationToken cancellationToken = default)
        {
            LastPath = path;
            LastAllowNotFound = allowNotFound;
            return Task.FromResult((TResponse?)Response);
        }

        public Task<TResponse?> SendAsync<TResponse>(HttpMethod method, string path, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<TResponse?> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
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
