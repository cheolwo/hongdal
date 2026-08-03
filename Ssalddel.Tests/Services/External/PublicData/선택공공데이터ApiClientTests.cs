using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 선택공공데이터ApiClientTests
{
    [Fact]
    public void 국문관광정보Client_15개허용오퍼레이션을제공한다()
    {
        var sut = new 국문관광정보공공데이터Client(
            CreateHttpClient(new StubHttpMessageHandler(_ => JsonResponse("{}"))),
            CreateOptions("stored-key"));

        Assert.Equal(15, sut.Apis.Count);
        Assert.All(sut.Apis, api =>
            Assert.StartsWith("/B551011/KorService2/", api.DefaultOperationPath, StringComparison.Ordinal));
        Assert.Contains(sut.Apis, api => api.Key == "detail-image");
        Assert.Contains(sut.Apis, api => api.Key == "pet-tour");
    }

    [Theory]
    [InlineData("item-list", "serviceKey=stored-key", "ServiceKey=caller-key")]
    [InlineData("price-observation", "ServiceKey=stored-key", "serviceKey=caller-key")]
    public async Task 온라인가격Client_오퍼레이션별인증키이름을지키고호출자키를무시한다(
        string apiKey,
        string expected,
        string rejected)
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse("{}");
        });
        var sut = new 온라인가격공공데이터Client(CreateHttpClient(handler), CreateOptions("stored-key"));

        var result = await sut.QueryAsync(new 공공데이터포털업무ApiRequest
        {
            ApiKey = apiKey,
            Parameters = new Dictionary<string, string?>
            {
                ["serviceKey"] = "caller-key",
                ["ServiceKey"] = "caller-key",
                ["pageNo"] = "1"
            }
        });

        Assert.True(result.Success);
        Assert.NotNull(requestedUri);
        Assert.Contains(expected, requestedUri!.Query, StringComparison.Ordinal);
        Assert.DoesNotContain(rejected, requestedUri.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("caller-key", requestedUri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Kosis비교자료Client_승인된지표정보경로와저장된인증키를사용한다()
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse("{}");
        });
        var sut = new Kosis비교자료공공데이터Client(CreateHttpClient(handler), CreateOptions("stored-key"));

        var result = await sut.QueryAsync(new 공공데이터포털업무ApiRequest
        {
            ApiKey = "indicator-by-name",
            Parameters = new Dictionary<string, string?> { ["indNm"] = "소비자물가" }
        });

        Assert.True(result.Success);
        Assert.Equal(6, sut.Apis.Count);
        Assert.Equal("/1240000/IndicatorService/IndListSearchRequest", requestedUri!.AbsolutePath);
        Assert.Contains("serviceKey=stored-key", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("indNm=", requestedUri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Kosis비교자료Client_별도승인된통계자료조회경로를제공한다()
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse("{}");
        });
        var sut = new Kosis비교자료공공데이터Client(CreateHttpClient(handler), CreateOptions("stored-key"));

        var result = await sut.QueryAsync(new 공공데이터포털업무ApiRequest
        {
            ApiKey = "statistics-data",
            Parameters = new Dictionary<string, string?>
            {
                ["orgId"] = "101",
                ["tblId"] = "DT_1J22001"
            }
        });

        Assert.True(result.Success);
        Assert.Equal("/1240000/statisticsData/getStatisticsData", requestedUri!.AbsolutePath);
        Assert.Contains("serviceKey=stored-key", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("orgId=101", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("tblId=DT_1J22001", requestedUri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 산지위판가격Client_별도키가없으면외부호출하지않는다()
    {
        var requested = false;
        var handler = new StubHttpMessageHandler(_ =>
        {
            requested = true;
            return JsonResponse("{}");
        });
        var sut = CreateAuctionClient(handler, new MafraFisheriesAuctionOptions
        {
            BaseUrl = "https://relay.example.test"
        });

        var result = await sut.QueryAsync(new 수산물산지위판가격Request
        {
            CollectionDate = new DateOnly(2015, 6, 30)
        });

        Assert.False(result.Success);
        Assert.False(requested);
        Assert.Contains("ApiKey", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 산지위판가격Client_Http원천은기본적으로차단한다()
    {
        var requested = false;
        var handler = new StubHttpMessageHandler(_ =>
        {
            requested = true;
            return JsonResponse("{}");
        });
        var sut = CreateAuctionClient(handler, new MafraFisheriesAuctionOptions
        {
            ApiKey = "separate-key",
            BaseUrl = "http://211.237.50.150:7080"
        });

        var result = await sut.QueryAsync(new 수산물산지위판가격Request
        {
            CollectionDate = new DateOnly(2015, 6, 30)
        });

        Assert.False(result.Success);
        Assert.False(requested);
        Assert.Contains("HTTPS", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 산지위판가격Client_역사관측값과단위를매핑한다()
    {
        Uri? requestedUri = null;
        var json = """
            {
              "Grid_20151125000000000310_1": {
                "totalCnt": "1",
                "row": [{
                  "COLCT_DE": "20150630",
                  "CNSGSLE_ASSC_CODE": "A01",
                  "NSO_KDFSH_CODE": "011",
                  "SUHYUP_PRDLST_CODE": "F001",
                  "SBID_TIME": "07",
                  "KDFSH_NM": "고등어",
                  "TOT_QY": "1,200.5",
                  "TOT_DLAMT": "33",
                  "TOT_PRIC": "123000",
                  "TOP_PRIC": "12000",
                  "LWET_PRIC": "8000",
                  "AVRG_PRIC": "10250",
                  "UNIT_NM": "kg",
                  "FRMLC_NM": "상자",
                  "MG_NM": "대",
                  "QLITY_NM": "상",
                  "MTC_NM": "부산공동어시장"
                }]
              }
            }
            """;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse(json);
        });
        var sut = CreateAuctionClient(handler, new MafraFisheriesAuctionOptions
        {
            ApiKey = "separate-key",
            BaseUrl = "https://relay.example.test"
        });

        var result = await sut.QueryAsync(new 수산물산지위판가격Request
        {
            CollectionDate = new DateOnly(2015, 6, 30),
            MarketName = "부산공동어시장"
        });

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalCount);
        var item = Assert.Single(result.Items);
        Assert.Equal("고등어", item.FishSpeciesName);
        Assert.Equal(1200.5m, item.TotalQuantity);
        Assert.Equal(10250m, item.AveragePriceKrw);
        Assert.Equal("kg", item.UnitName);
        Assert.Equal(new DateOnly(1999, 1, 1), result.CoverageStart);
        Assert.Equal(new DateOnly(2016, 1, 19), result.CoverageEnd);
        Assert.Contains("COLCT_DE=20150630", requestedUri!.Query, StringComparison.Ordinal);
        Assert.Contains("MTC_NM=", requestedUri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 산지위판가격Client_수록기간밖의날짜는호출하지않는다()
    {
        var requested = false;
        var handler = new StubHttpMessageHandler(_ =>
        {
            requested = true;
            return JsonResponse("{}");
        });
        var sut = CreateAuctionClient(handler, new MafraFisheriesAuctionOptions
        {
            ApiKey = "separate-key",
            BaseUrl = "https://relay.example.test"
        });

        var result = await sut.QueryAsync(new 수산물산지위판가격Request
        {
            CollectionDate = new DateOnly(2026, 8, 3)
        });

        Assert.False(result.Success);
        Assert.False(requested);
        Assert.Contains("2016-01-19", result.ErrorMessage, StringComparison.Ordinal);
    }

    private static Mafra수산물산지위판가격Client CreateAuctionClient(
        HttpMessageHandler handler,
        MafraFisheriesAuctionOptions auctionOptions)
    {
        var options = new PublicDataOptions { FisheriesAuction = auctionOptions };
        return new Mafra수산물산지위판가격Client(
            new HttpClient(handler) { BaseAddress = new Uri(auctionOptions.BaseUrl.TrimEnd('/') + "/") },
            Options.Create(options),
            TimeProvider.System);
    }

    private static IOptions<PublicDataOptions> CreateOptions(string serviceKey)
        => Options.Create(new PublicDataOptions { DataGoKrServiceKey = serviceKey });

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("https://apis.data.go.kr/") };

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
