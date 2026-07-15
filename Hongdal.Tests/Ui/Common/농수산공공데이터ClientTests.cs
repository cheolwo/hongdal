using System.Net;
using System.Text;
using Hongdal.Contracts.Common.AgriculturalFisheries;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Tests.Ui.Common;

public sealed class 농수산공공데이터ClientTests
{
    [Fact]
    public async Task 미국가격조회는_공식필터를전송하고_503응답본문도보존한다()
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent(
                    """
                    {
                      "success": false,
                      "statusCode": "NotConfigured",
                      "errorMessage": "USDA NASS Quick Stats API 키가 설정되지 않았습니다.",
                      "sourceKey": "usda-nass-quickstats",
                      "query": {
                        "commodity": "CATFISH",
                        "program": "SURVEY",
                        "yearFrom": 2023,
                        "yearTo": 2026
                      }
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = new 농수산공공데이터Client(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.hongdal.test/")
        });

        var result = await client.미국가격조회Async("catfish", "SURVEY", 2023, 2026);

        Assert.False(result.Success);
        Assert.Equal(미국농수산가격조회상태Codes.설정안됨, result.StatusCode);
        Assert.NotNull(requestedUri);
        var query = Uri.UnescapeDataString(requestedUri!.Query);
        Assert.Contains("commodity=catfish", query, StringComparison.Ordinal);
        Assert.Contains("statisticCategory=PRICE RECEIVED", query, StringComparison.Ordinal);
        Assert.Contains("aggregationLevel=NATIONAL", query, StringComparison.Ordinal);
        Assert.Contains("yearFrom=2023", query, StringComparison.Ordinal);
        Assert.Contains("yearTo=2026", query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 국내가격조회는_HS코드와조회기간을안전하게전송한다()
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "success": true,
                      "statusCode": "Complete",
                      "hsCode": "080810",
                      "summary": "사과 국내가격"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = new 농수산공공데이터Client(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.hongdal.test/")
        });

        var result = await client.국내가격조회Async("080810", lookbackDays: 45);

        Assert.True(result.Success);
        Assert.Equal("080810", result.HsCode);
        Assert.NotNull(requestedUri);
        Assert.Equal(
            "/api/v1/agricultural-fisheries/items/080810/domestic-price",
            requestedUri!.AbsolutePath);
        Assert.Contains("lookbackDays=31", requestedUri.Query, StringComparison.Ordinal);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
