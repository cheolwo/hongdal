using System.Text.Json;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class KamisPriceArchiveServiceTests
{
    [Theory]
    [InlineData("쓰가루(아오리)(10개)", "10개")]
    [InlineData("후지(1kg)", "1kg")]
    [InlineData("닭고기(1마리)", "1마리")]
    [InlineData("감자(수미)", "")]
    public void 종류명의마지막수량표시를_원포장표시로분리한다(
        string kindName,
        string expectedSourcePackage)
    {
        var result = KamisPriceUnitProvenanceParser.FromKindName(kindName);

        Assert.Equal(expectedSourcePackage, result.SourcePackageLabel);
        Assert.Equal("1kg", result.ComparisonUnit);
        Assert.Equal(
            KamisPriceUnitProvenanceParser.SourceKilogramConversionCode,
            result.PriceNormalizationCode);
        Assert.Contains("p_convert_kg_yn=Y", result.PriceNormalizationBasis);
    }

    [Theory]
    [InlineData("당일 (07/15)", "2026-07-15", "2026-07-15")]
    [InlineData("당일 (12/31)", "2026-01-01", "2025-12-31")]
    [InlineData("2026-07-14", "2026-07-15", "2026-07-14")]
    public void 조사일표시를_요청일기준_날짜로변환한다(
        string source,
        string requested,
        string expected)
    {
        var result = KamisPriceValueParser.ParseSurveyDate(source, DateOnly.Parse(requested));

        Assert.Equal(DateOnly.Parse(expected), result);
    }

    [Theory]
    [InlineData("6,744", 6744)]
    [InlineData(" 1,234.5 ", 1234.5)]
    public void 쉼표가있는_가격을_숫자로변환한다(string source, decimal expected)
    {
        Assert.Equal(expected, KamisPriceValueParser.ParsePrice(source));
    }

    [Theory]
    [InlineData("-")]
    [InlineData("")]
    public void 미제공가격은_null로변환한다(string source)
    {
        Assert.Null(KamisPriceValueParser.ParsePrice(source));
    }

    [Theory]
    [InlineData("2025", "07/17", "2025-07-17")]
    [InlineData("2026", "1/02", "2026-01-02")]
    public void 기간가격의_연도와_월일을_조사일로변환한다(
        string year,
        string monthDay,
        string expected)
    {
        var result = KamisPriceValueParser.ParsePeriodSurveyDate(year, monthDay);

        Assert.Equal(DateOnly.Parse(expected), result);
    }

    [Fact]
    public void 기간조회는_직전_1년_범위를허용한다()
    {
        KamisPriceRequestRules.ValidatePeriod(
            new DateOnly(2025, 7, 17),
            new DateOnly(2026, 7, 16));
    }

    [Fact]
    public void 기간조회가_1년을넘으면_거부한다()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            KamisPriceRequestRules.ValidatePeriod(
                new DateOnly(2025, 7, 17),
                new DateOnly(2026, 7, 17)));
    }

    [Fact]
    public async Task 기간조회는_공식Kamis품목매개변수명을사용한다()
    {
        await using var db = CreateDb();
        var client = new RecordingKamisClient();
        var options = Options.Create(new PublicDataOptions
        {
            Kamis = new KamisOptions
            {
                CertificationKey = "test-key",
                RequesterId = "test-id"
            }
        });
        var sut = new KamisPriceArchiveService(
            client,
            db,
            options,
            NullLogger<KamisPriceArchiveService>.Instance);

        var result = await sut.CollectPeriodPricesAsync(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 2));

        Assert.Equal(1, result.FetchedCount);
        var requestPath = Assert.Single(
            client.RequestPaths,
            path => path.Contains(
                "action=periodWholesaleProductList",
                StringComparison.Ordinal));
        Assert.Contains("p_itemcategorycode=100", requestPath, StringComparison.Ordinal);
        Assert.Contains("p_itemcode=111", requestPath, StringComparison.Ordinal);
        Assert.Contains("p_kindcode=01", requestPath, StringComparison.Ordinal);
        Assert.Contains("p_productrankcode=04", requestPath, StringComparison.Ordinal);
        Assert.Contains("p_countrycode=", requestPath, StringComparison.Ordinal);
        Assert.DoesNotContain("p_item_category_code", requestPath, StringComparison.Ordinal);
        Assert.DoesNotContain("p_item_code", requestPath, StringComparison.Ordinal);
        Assert.DoesNotContain("p_kind_code", requestPath, StringComparison.Ordinal);
        Assert.DoesNotContain("p_product_rank_code", requestPath, StringComparison.Ordinal);
    }

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private sealed class RecordingKamisClient : IKamisJsonClient
    {
        public List<string> RequestPaths { get; } = [];

        public Task<JsonDocument> GetDocumentAsync(
            string requestPath,
            CancellationToken cancellationToken = default)
        {
            RequestPaths.Add(requestPath);
            if (requestPath.Contains("action=productInfo", StringComparison.Ordinal))
            {
                return Task.FromResult(JsonDocument.Parse(
                    """
                    {
                      "error_code": "000",
                      "info": [
                        {
                          "itemcategorycode": "100",
                          "itemcategoryname": "식량작물",
                          "itemcode": "111",
                          "itemname": "쌀",
                          "kindcode": "01",
                          "kindname": "일반계",
                          "whole_productrankcode": "04",
                          "retail_productrankcode": ""
                        }
                      ]
                    }
                    """));
            }

            return Task.FromResult(JsonDocument.Parse(
                """
                {
                  "data": {
                    "error_code": "000",
                    "item": [
                      {
                        "itemname": "쌀",
                        "kindname": "일반계",
                        "countyname": "평균",
                        "yyyy": "2026",
                        "regday": "01/02",
                        "price": "2,500"
                      }
                    ]
                  }
                }
                """));
        }
    }
}
