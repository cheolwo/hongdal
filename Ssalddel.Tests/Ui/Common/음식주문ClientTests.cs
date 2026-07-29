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

    [Fact]
    public async Task 등록Client는_선택메뉴와수령정보를보호Post로보낸다()
    {
        var response = new 음식주문응답 { 주문번호 = "FOOD-1" };
        var api = new RecordingJsonApiClient { Response = response };
        var client = new 주문자음식주문Client(api);
        var request = new 음식주문등록요청
        {
            음식점Id = 101,
            상품목록 = [new 음식주문상품Dto { 메뉴Id = 1001, 수량 = 2 }]
        };

        var result = await client.등록Async(request);

        Assert.Equal("FOOD-1", result.주문번호);
        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Equal("api/v1/food-orders", api.LastPath);
        Assert.Same(request, api.LastRequest);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 수령확인Client는_정확한주문번호의보호Post를사용한다()
    {
        var response = new 음식주문응답
        {
            주문번호 = "FOOD A/01",
            상태 = 음식주문상태코드.수령확인
        };
        var api = new RecordingJsonApiClient { Response = response };
        var client = new 주문자음식주문Client(api);
        var request = new 주문자음식주문수령확인요청
        {
            클라이언트요청Id = Guid.NewGuid(),
            확인메모 = "정상 수령"
        };

        var result = await client.수령확인Async("FOOD A/01", request);

        Assert.Equal(음식주문상태코드.수령확인, result.상태);
        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Equal(
            "api/v1/food-orders/FOOD%20A%2F01/receipt-confirmation",
            api.LastPath);
        Assert.Same(request, api.LastRequest);
        Assert.False(api.LastAllowNotFound);
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
