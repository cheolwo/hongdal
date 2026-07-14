using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using 홍달.Services.External.PublicData;
using 홍달.Services.Options;

namespace Hongdal.Tests.Services.TraditionalMarkets;

public sealed class TraditionalMarketPublicDataClientTests
{
    [Fact]
    public async Task FetchPageAsync_공식응답을_시장과시설정보로변환한다()
    {
        Uri? requestUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestUri = request.RequestUri;
            return JsonResponse(
                """
                {
                  "currentCount": 1,
                  "data": [
                    {
                      "시장코드": "A001",
                      "시장명": "광장시장",
                      "시장 유형": "등록시장",
                      "지번주소": "서울특별시 종로구 예지동 6-1",
                      "도로명주소": "서울특별시 종로구 창경궁로 88",
                      "시도": "서울특별시",
                      "시군구": "종로구",
                      "아케이드 보유 여부": "Y",
                      "엘리베이터_에스컬레이터_보유여부": null,
                      "공동물류창고_보유여부": "N",
                      "시장전용 고객주차장_보유여부": "Y"
                    }
                  ],
                  "page": 1,
                  "perPage": 100,
                  "totalCount": 1393
                }
                """);
        });
        var sut = CreateClient(handler);

        var result = await sut.FetchPageAsync(1, 100, CancellationToken.None);

        var market = Assert.Single(result.Items);
        Assert.Equal(1393, result.TotalCount);
        Assert.Equal("A001", market.MarketCode);
        Assert.Equal("광장시장", market.Name);
        Assert.Equal("서울특별시", market.Province);
        Assert.True(market.Facilities.HasArcade);
        Assert.Null(market.Facilities.HasElevatorOrEscalator);
        Assert.False(market.Facilities.HasSharedLogisticsWarehouse);
        Assert.True(market.Facilities.HasDedicatedParking);
        Assert.Contains("page=1", requestUri!.Query);
        Assert.Contains("perPage=100", requestUri.Query);
        Assert.Contains("serviceKey=test-key", requestUri.Query);
    }

    private static TraditionalMarketPublicDataClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.odcloud.kr/")
        };
        return new TraditionalMarketPublicDataClient(
            httpClient,
            Options.Create(new PublicDataOptions
            {
                DataGoKrServiceKey = "test-key",
                TraditionalMarket = new TraditionalMarketOptions
                {
                    ApiPath = "/api/15052837/v1/test-dataset"
                }
            }));
    }

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(_responseFactory(request));
    }
}
