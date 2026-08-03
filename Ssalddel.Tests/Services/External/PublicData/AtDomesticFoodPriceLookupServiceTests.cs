using System.Net;
using System.Text;
using Ssalddel.Contracts.Common.PublicData;
using Microsoft.Extensions.Options;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class AtDomesticFoodPriceLookupServiceTests
{
    [Fact]
    public async Task 국내가격조회_허용품종의최근조사일가격을도소매별로평균한다()
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
                      "header": { "resultCode": "00", "resultMsg": "NORMAL SERVICE" },
                      "body": {
                        "items": {
                          "item": [
                            { "exmn_ymd": "20260713", "se_cd": "01", "item_nm": "양파", "vrty_cd": "00", "vrty_nm": "양파", "exmn_dd_cnvs_prc": "9,000" },
                            { "exmn_ymd": "20260714", "se_cd": "01", "item_nm": "양파", "vrty_cd": "00", "vrty_nm": "양파", "exmn_dd_cnvs_prc": "10,000" },
                            { "exmn_ymd": "20260714", "se_cd": "01", "item_nm": "양파", "vrty_cd": "02", "vrty_nm": "햇양파", "exmn_dd_cnvs_prc": "12,000" },
                            { "exmn_ymd": "20260714", "se_cd": "01", "item_nm": "양파", "vrty_cd": "10", "vrty_nm": "수입", "exmn_dd_cnvs_prc": "5,000" },
                            { "exmn_ymd": "20260712", "se_cd": "02", "item_nm": "양파", "vrty_cd": "00", "vrty_nm": "양파", "exmn_dd_cnvs_prc": "7,000" },
                            { "exmn_ymd": "20260714", "se_cd": "02", "item_nm": "양파", "vrty_cd": "00", "vrty_nm": "양파", "exmn_dd_cnvs_prc": "8,000" },
                            { "exmn_ymd": "20260714", "se_cd": "02", "item_nm": "양파", "vrty_cd": "02", "vrty_nm": "햇양파", "exmn_dd_cnvs_prc": "10,000" }
                          ]
                        }
                      }
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var service = new AtDomesticFoodPriceLookupService(
            new HttpClient(handler) { BaseAddress = new Uri("https://apis.data.go.kr/") },
            Options.Create(new PublicDataOptions
            {
                AtFoodPrices = new AtFoodPricesOptions
                {
                    ServiceKey = "test-key",
                    DailyPricePath = "/B552845/perDay/price"
                }
            }));

        var result = await service.LookupAsync(new AtDomesticFoodPriceRequest
        {
            CategoryCode = "200",
            ItemCode = "245",
            StartDate = "20260701",
            EndDate = "20260714",
            VarietyCodes = ["00", "02"],
            ExcludedNameTokens = ["수입"]
        });

        Assert.True(result.Success);
        Assert.Equal("양파", result.ItemName);
        Assert.NotNull(result.Retail);
        Assert.Equal("20260714", result.Retail!.LatestSurveyDate);
        Assert.Equal(11_000m, result.Retail.AverageKrwPerKg);
        Assert.Equal(2, result.Retail.SampleCount);
        Assert.NotNull(result.Wholesale);
        Assert.Equal("20260714", result.Wholesale!.LatestSurveyDate);
        Assert.Equal(9_000m, result.Wholesale.AverageKrwPerKg);
        Assert.Equal(2, result.Wholesale.SampleCount);

        Assert.NotNull(requestedUri);
        var query = Uri.UnescapeDataString(requestedUri!.Query);
        Assert.Contains("cond[exmn_ymd::GTE]=20260701", query, StringComparison.Ordinal);
        Assert.Contains("cond[exmn_ymd::LTE]=20260714", query, StringComparison.Ordinal);
        Assert.Contains("cond[ctgry_cd::EQ]=200", query, StringComparison.Ordinal);
        Assert.Contains("cond[item_cd::EQ]=245", query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 고등어는_도매와소매의서로다른품종코드로_각단계가격을조회한다()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                  "header": { "resultCode": "00", "resultMsg": "NORMAL SERVICE" },
                  "body": {
                    "items": {
                      "item": [
                        { "exmn_ymd": "20260801", "se_cd": "01", "item_nm": "고등어", "vrty_cd": "05", "vrty_nm": "국산 신선 냉장", "exmn_dd_cnvs_prc": "4,000" },
                        { "exmn_ymd": "20260801", "se_cd": "01", "item_nm": "고등어", "vrty_cd": "08", "vrty_nm": "수입산 염장", "exmn_dd_cnvs_prc": "3,000" },
                        { "exmn_ymd": "20260801", "se_cd": "02", "item_nm": "고등어", "vrty_cd": "01", "vrty_nm": "생선", "exmn_dd_cnvs_prc": "5,000" },
                        { "exmn_ymd": "20260801", "se_cd": "02", "item_nm": "고등어", "vrty_cd": "02", "vrty_nm": "냉동", "exmn_dd_cnvs_prc": "6,000" },
                        { "exmn_ymd": "20260801", "se_cd": "02", "item_nm": "고등어", "vrty_cd": "04", "vrty_nm": "냉동 수입", "exmn_dd_cnvs_prc": "2,000" }
                      ]
                    }
                  }
                }
                """,
                Encoding.UTF8,
                "application/json")
        });
        var service = new AtDomesticFoodPriceLookupService(
            new HttpClient(handler) { BaseAddress = new Uri("https://apis.data.go.kr/") },
            Options.Create(new PublicDataOptions
            {
                AtFoodPrices = new AtFoodPricesOptions
                {
                    ServiceKey = "test-key",
                    DailyPricePath = "/B552845/perDay/price"
                }
            }));

        var result = await service.LookupAsync(new AtDomesticFoodPriceRequest
        {
            CategoryCode = "600",
            ItemCode = "611",
            StartDate = "20260801",
            EndDate = "20260801",
            VarietyCodes = ["05"],
            WholesaleVarietyCodes = ["01", "02"],
            RetailVarietyCodes = ["05"],
            ExcludedNameTokens = ["수입"]
        });

        Assert.True(result.Success);
        Assert.Equal(4_000m, result.Retail?.AverageKrwPerKg);
        Assert.Equal(5_500m, result.Wholesale?.AverageKrwPerKg);
        Assert.Equal(2, result.Wholesale?.SampleCount);
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
