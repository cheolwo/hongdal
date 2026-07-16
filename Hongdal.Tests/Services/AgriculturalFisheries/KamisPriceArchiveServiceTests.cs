using Hongdal.Services.AgriculturalFisheries.Information;

namespace Hongdal.Tests.Services.AgriculturalFisheries;

public sealed class KamisPriceArchiveServiceTests
{
    [Theory]
    [InlineData("당일 (07/15)", "2026-07-15", "2026-07-15")]
    [InlineData("당일 (12/31)", "2026-01-01", "2025-12-31")]
    [InlineData("2026-07-14", "2026-07-15", "2026-07-14")]
    public void 조사일표시를_요청일기준_날짜로변환한다(
        string source,
        string requested,
        string expected)
    {
        var result = KamisPriceArchiveService.ParseSurveyDate(source, DateOnly.Parse(requested));

        Assert.Equal(DateOnly.Parse(expected), result);
    }

    [Theory]
    [InlineData("6,744", 6744)]
    [InlineData(" 1,234.5 ", 1234.5)]
    public void 쉼표가있는_가격을_숫자로변환한다(string source, decimal expected)
    {
        Assert.Equal(expected, KamisPriceArchiveService.ParsePrice(source));
    }

    [Theory]
    [InlineData("-")]
    [InlineData("")]
    public void 미제공가격은_null로변환한다(string source)
    {
        Assert.Null(KamisPriceArchiveService.ParsePrice(source));
    }
}
