using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class JapanImportedFoodManufacturerPrefectureClassifierTests
{
    [Theory]
    [InlineData("일본", "北海道", "", "JP-01")]
    [InlineData("JAPAN", "SHIZUOKA", "", "JP-22")]
    [InlineData("JP", "京都府", "", "JP-26")]
    [InlineData("日本", "", "大阪府大阪市北区", "JP-27")]
    [InlineData("JPN", "", "KAGOSHIMA, JAPAN", "JP-46")]
    public void 공식지역명과주소로_일본도도부현을분류한다(
        string countryName,
        string areaName,
        string address,
        string expectedRegionCode)
    {
        var result = JapanImportedFoodManufacturerPrefectureClassifier.Classify(
            countryName,
            areaName,
            address);

        Assert.NotNull(result);
        Assert.Equal(expectedRegionCode, result!.RegionCode);
        Assert.InRange(result.Confidence, 0.95m, 1m);
    }

    [Fact]
    public void 공식목록은_일본47개도도부현과안정코드를제공한다()
    {
        var definitions =
            JapanImportedFoodManufacturerPrefectureClassifier.Definitions;

        Assert.Equal(47, definitions.Count);
        Assert.Equal(47, definitions.Select(item => item.RegionCode).Distinct().Count());
        Assert.Contains(definitions, item => item.RegionCode == "JP-13" && item.KoreanName == "도쿄도");
        Assert.Contains(definitions, item => item.RegionCode == "JP-47" && item.KoreanName == "오키나와현");
    }

    [Fact]
    public void 국가만확인되면_도도부현을추정하지않는다()
    {
        var result = JapanImportedFoodManufacturerPrefectureClassifier.Classify(
            "일본",
            string.Empty,
            "JAPAN");

        Assert.NotNull(result);
        Assert.Equal(
            JapanImportedFoodManufacturerPrefectureRegionCodes.OtherOrUnclassified,
            result!.RegionCode);
        Assert.Equal(
            JapanImportedFoodManufacturerPrefectureMethodCodes.CountryOnly,
            result.ClassificationMethodCode);
        Assert.Equal(0.5m, result.Confidence);
    }

    [Theory]
    [InlineData("대한민국")]
    [InlineData("CHINA")]
    public void 일본이아닌제품에는_일본지역을부여하지않는다(string countryName)
    {
        var result = JapanImportedFoodManufacturerPrefectureClassifier.Classify(
            countryName,
            "TOKYO",
            "TOKYO, JAPAN");

        Assert.Null(result);
    }

    [Fact]
    public void 데이터소스카탈로그는_key필요여부와지역의미를명시한다()
    {
        var noKeySources = JapanRegionalDataSourceCatalog.All
            .Where(item => !item.RequiresApiKey)
            .Select(item => item.Key)
            .ToArray();
        var keyedSources = JapanRegionalDataSourceCatalog.All
            .Where(item => item.RequiresApiKey)
            .ToArray();

        Assert.Contains("maff-regional-cuisine", noKeySources);
        Assert.Contains("maff-gi-products", noKeySources);
        Assert.Contains("japan-customs-trade-statistics", noKeySources);
        Assert.Contains(
            keyedSources,
            item => item.Key == "e-stat-regional-production"
                    && item.ApiKeySetting.EndsWith(":AppId", StringComparison.Ordinal));
        Assert.Contains(
            keyedSources,
            item => item.Key == "korea-customs-hs-country-trade"
                    && item.ApiKeySetting.Contains(
                        "CustomsTradeStatistics",
                        StringComparison.Ordinal));
        Assert.All(
            JapanRegionalDataSourceCatalog.All,
            item => Assert.False(string.IsNullOrWhiteSpace(item.RegionMeaning)));
    }
}
