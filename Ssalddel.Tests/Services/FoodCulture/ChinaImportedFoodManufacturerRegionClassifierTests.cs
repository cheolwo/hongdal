using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class ChinaImportedFoodManufacturerRegionClassifierTests
{
    [Theory]
    [InlineData(
        "중국",
        "LIAONING",
        "",
        ChinaImportedFoodManufacturerRegionCodes.LiaoningLiaodong,
        ChinaImportedFoodManufacturerRegionMethodCodes.OfficialAreaProvince)]
    [InlineData(
        "CHINA",
        "",
        "NO. 10, QINGDAO, SHANDONG",
        ChinaImportedFoodManufacturerRegionCodes.Shandong,
        ChinaImportedFoodManufacturerRegionMethodCodes.FacilityAddressProvince)]
    [InlineData(
        "중국",
        "",
        "NINGBO CITY",
        ChinaImportedFoodManufacturerRegionCodes.LowerYangtzeJiangnan,
        ChinaImportedFoodManufacturerRegionMethodCodes.FacilityAddressCity)]
    public void 공식지역명과제조업소주소로_세운영권역을분류한다(
        string countryName,
        string areaName,
        string address,
        string expectedRegionCode,
        string expectedMethodCode)
    {
        var result = ChinaImportedFoodManufacturerRegionClassifier.Classify(
            countryName,
            areaName,
            address);

        Assert.NotNull(result);
        Assert.Equal(expectedRegionCode, result!.RegionCode);
        Assert.Equal(expectedMethodCode, result.ClassificationMethodCode);
        Assert.InRange(result.Confidence, 0.9m, 1m);
    }

    [Fact]
    public void 중국이지만지역근거가없으면_기타미분류로보존한다()
    {
        var result = ChinaImportedFoodManufacturerRegionClassifier.Classify(
            "중국",
            string.Empty,
            "ADDRESS WITHOUT A RECOGNIZED REGION");

        Assert.NotNull(result);
        Assert.Equal(
            ChinaImportedFoodManufacturerRegionCodes.OtherOrUnclassified,
            result!.RegionCode);
        Assert.Equal(
            ChinaImportedFoodManufacturerRegionMethodCodes.CountryOnly,
            result.ClassificationMethodCode);
        Assert.Equal(0.5m, result.Confidence);
    }

    [Theory]
    [InlineData("미국")]
    [InlineData("대만")]
    [InlineData("TAIWAN")]
    public void 중국본토가아닌국가는_중국권역을부여하지않는다(string countryName)
    {
        var result = ChinaImportedFoodManufacturerRegionClassifier.Classify(
            countryName,
            "SHANDONG",
            "QINGDAO");

        Assert.Null(result);
    }
}
