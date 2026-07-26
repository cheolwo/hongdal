using System.Net;
using System.Text;
using Ssalddel.Contracts.Common.PublicData;
using Microsoft.Extensions.Options;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class HsCountryTradeUnitPriceLookupServiceTests
{
    [Fact]
    public async Task 수입평균단가조회_신고금액과중량의가중평균을원화로환산한다()
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
                      "response": {
                        "body": {
                          "items": {
                            "item": [
                              {
                                "year": "2026.01",
                                "statKor": "볶은 커피",
                                "statCd": "CN",
                                "hsCd": "0901210000",
                                "impWgt": "100",
                                "impDlr": "200",
                                "expWgt": "20",
                                "expDlr": "100"
                              },
                              {
                                "year": "2026.02",
                                "statKor": "볶은 커피",
                                "statCd": "CN",
                                "hsCd": "0901210000",
                                "impWgt": "300",
                                "impDlr": "900",
                                "expWgt": "80",
                                "expDlr": "500"
                              }
                            ]
                          }
                        }
                      }
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://apis.data.go.kr/")
        };
        var service = new HsCountryTradeUnitPriceLookupService(
            httpClient,
            Options.Create(new PublicDataOptions
            {
                CustomsTradeStatistics = new CustomsTradeStatisticsOptions
                {
                    ServiceKey = "test-key",
                    HsCountryMonthlyPath = "/1220000/nitemtrade/getNitemtradeList"
                }
            }));

        var result = await service.SimulateImportUnitPriceAsync(new HsCountryMonthlyTradeUnitPriceRequest
        {
            InternalProductCode = "PRD-COFFEE-001",
            ProductName = "유기농 볶은 커피 원두 1kg",
            HsCode = "0901.21",
            HsCodeScheme = "HS",
            NationalTariffCodeScheme = "HTSUS",
            NationalTariffCode = "0901.21.0015",
            CountryCode = "cn",
            Month = "202603",
            LookbackMonths = 3,
            ExpectedFxRateKrwPerUsd = 1350m
        });

        Assert.True(result.Success);
        Assert.Equal("PRD-COFFEE-001", result.InternalProductCode);
        Assert.Equal("유기농 볶은 커피 원두 1kg", result.ProductName);
        Assert.Equal("090121", result.HsCode);
        Assert.Equal("HS", result.HsCodeScheme);
        Assert.Equal("HTSUS", result.NationalTariffCodeScheme);
        Assert.Equal("0901210015", result.NationalTariffCode);
        Assert.Equal("CN", result.CountryCode);
        Assert.Equal("202601", result.StartMonth);
        Assert.Equal("202603", result.EndMonth);
        Assert.Equal(400m, result.TotalImportWeightKg);
        Assert.Equal(1100m, result.TotalImportValueUsd);
        Assert.Equal(2.75m, result.AverageImportUnitValueUsdPerKg);
        Assert.Equal(3713m, result.AverageImportUnitValueKrwPerKg);
        Assert.Equal(100m, result.TotalExportWeightKg);
        Assert.Equal(600m, result.TotalExportValueUsd);
        Assert.Equal(6m, result.AverageExportUnitValueUsdPerKg);
        Assert.Equal(8100m, result.AverageExportUnitValueKrwPerKg);
        Assert.Equal("CIF customs value", result.ImportValueBasis);
        Assert.Equal("FOB declared value", result.ExportValueBasis);
        Assert.True(result.IsStatisticalUnitValue);
        Assert.False(result.IsLandedCost);
        Assert.Equal("볶은 커피", result.MonthlyItems[0].ProductName);
        Assert.Equal("0901210000", result.MonthlyItems[0].HsCode);
        Assert.Equal("HS", result.MonthlyItems[0].HsCodeScheme);
        Assert.Equal("CN", result.MonthlyItems[0].CountryCode);
        Assert.Equal("202601", result.MonthlyItems[0].Month);
        Assert.NotNull(requestedUri);
        Assert.Contains("strtYymm=202601", requestedUri!.Query, StringComparison.Ordinal);
        Assert.Contains("endYymm=202603", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("hsSgn=090121", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("cntyCd=CN", requestedUri.Query, StringComparison.Ordinal);
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
