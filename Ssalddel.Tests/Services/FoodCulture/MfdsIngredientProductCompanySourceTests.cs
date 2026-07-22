using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Services.FoodCulture;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class MfdsIngredientProductCompanySourceTests
{
    [Fact]
    public async Task SearchAsync_WhenProviderReturnsHtml_ExplainsConfigurationFailure()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "<html><title>Access denied</title></html>",
                    Encoding.UTF8,
                    "text/html")
            });
        var options = new PublicDataOptions
        {
            MfdsIngredientCompanies = new MfdsIngredientCompanyOptions
            {
                ApiKey = "test-key",
                BaseUrl = "https://openapi.foodsafetykorea.go.kr",
                ServiceId = "C002"
            }
        };
        var source = new MfdsIngredientProductCompanySource(
            new HttpClient(handler) { BaseAddress = new Uri($"{options.MfdsIngredientCompanies.BaseUrl}/") },
            Options.Create(options));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => source.SearchAsync("참깨", 20));

        Assert.Contains("JSON이 아닌 응답", exception.Message, StringComparison.Ordinal);
        Assert.Contains("text/html", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("서비스 권한", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task C002원천은_재료명으로_국내업소와품목제조근거를조회한다()
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
                      "C002": {
                        "total_count": "1",
                        "row": [
                          {
                            "LCNS_NO": "20010000001",
                            "BSSH_NM": "공식식품 주식회사",
                            "PRDLST_REPORT_NO": "20010000001123",
                            "PRMS_DT": "20260701",
                            "PRDLST_NM": "참깨 소스",
                            "PRDLST_DCNM": "소스",
                            "RAWMTRL_NM": "참깨, 정제수, 소금",
                            "RAWMTRL_ORDNO": "1",
                            "CHNG_DT": "20260720"
                          }
                        ]
                      }
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var options = new PublicDataOptions
        {
            MfdsIngredientCompanies = new MfdsIngredientCompanyOptions
            {
                ApiKey = "test-key",
                BaseUrl = "https://openapi.foodsafetykorea.go.kr",
                ServiceId = "C002",
                PageSize = 20
            }
        };
        var source = new MfdsIngredientProductCompanySource(
            new HttpClient(handler) { BaseAddress = new Uri($"{options.MfdsIngredientCompanies.BaseUrl}/") },
            Options.Create(options));

        var result = await source.SearchAsync(" 참깨 ", 100);

        var record = Assert.Single(result);
        Assert.Equal("공식식품 주식회사", record.OrganizationName);
        Assert.Equal("20010000001", record.LicenseNumber);
        Assert.Equal("참깨 소스", record.ProductName);
        Assert.Contains("참깨", record.RawIngredientText, StringComparison.Ordinal);
        Assert.NotNull(requestedUri);
        Assert.Contains(
            "/api/test-key/C002/json/1/20/RAWMTRL_NM=참깨",
            Uri.UnescapeDataString(requestedUri!.AbsolutePath),
            StringComparison.Ordinal);
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
