using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class UnitedStatesImportedFoodManufacturerRegionClassifierTests
{
    [Theory]
    [InlineData(
        "미국",
        "CALIFORNIA",
        "",
        "US-CA",
        UnitedStatesImportedFoodManufacturerRegionMethodCodes.OfficialAreaName)]
    [InlineData(
        "UNITED STATES",
        "",
        "416 E. SOUTH AVE, FOWLER, CA - 93625",
        "US-CA",
        UnitedStatesImportedFoodManufacturerRegionMethodCodes
            .FacilityAddressPostalCode)]
    [InlineData(
        "US",
        "",
        "5166 S. SPERRY RD. DENAIR CA95316",
        "US-CA",
        UnitedStatesImportedFoodManufacturerRegionMethodCodes
            .FacilityAddressPostalCode)]
    [InlineData(
        "미국",
        "",
        "SEATTLE, WA 98101 USA",
        "US-WA",
        UnitedStatesImportedFoodManufacturerRegionMethodCodes
            .FacilityAddressPostalCode)]
    [InlineData(
        "미국",
        "",
        "PORTLAND, OREGON 97205",
        "US-OR",
        UnitedStatesImportedFoodManufacturerRegionMethodCodes
            .FacilityAddressStateName)]
    [InlineData(
        "미국",
        "DISTRICT OF COLUMBIA",
        "",
        "US-DC",
        UnitedStatesImportedFoodManufacturerRegionMethodCodes.OfficialAreaName)]
    public void 공식지역명과주소로_미국주연방구를분류한다(
        string countryName,
        string areaName,
        string address,
        string expectedRegionCode,
        string expectedMethod)
    {
        var result = UnitedStatesImportedFoodManufacturerRegionClassifier.Classify(
            countryName,
            areaName,
            address);

        Assert.NotNull(result);
        Assert.Equal(expectedRegionCode, result!.RegionCode);
        Assert.Equal(expectedMethod, result.ClassificationMethodCode);
        Assert.InRange(result.Confidence, 0.9m, 1m);
    }

    [Fact]
    public void 미국제품행의제조업소주소가중국이면_주를강제배정하지않는다()
    {
        var result = UnitedStatesImportedFoodManufacturerRegionClassifier.Classify(
            "미국",
            string.Empty,
            "NO. 18 HEFEI STREET, YANTAI, SHANDONG, CHINA");

        Assert.NotNull(result);
        Assert.Equal(
            UnitedStatesImportedFoodManufacturerRegionCodes.OtherOrUnclassified,
            result!.RegionCode);
        Assert.Equal(
            UnitedStatesImportedFoodManufacturerRegionMethodCodes
                .ExplicitForeignFacilityAddress,
            result.ClassificationMethodCode);
        Assert.Equal(0.4m, result.Confidence);
    }

    [Fact]
    public void 도시명만있으면_추측하지않고미분류로보존한다()
    {
        var result = UnitedStatesImportedFoodManufacturerRegionClassifier.Classify(
            "미국",
            string.Empty,
            "1050 S. DIAMOND ST, STOCKTON");

        Assert.NotNull(result);
        Assert.Equal(
            UnitedStatesImportedFoodManufacturerRegionCodes.OtherOrUnclassified,
            result!.RegionCode);
        Assert.Equal(
            UnitedStatesImportedFoodManufacturerRegionMethodCodes.CountryOnly,
            result.ClassificationMethodCode);
    }

    [Theory]
    [InlineData("캐나다")]
    [InlineData("TAIWAN")]
    public void 미국이아닌제품국가는_미국주를부여하지않는다(string countryName)
    {
        var result = UnitedStatesImportedFoodManufacturerRegionClassifier.Classify(
            countryName,
            "CALIFORNIA",
            "LOS ANGELES, CA 90001");

        Assert.Null(result);
    }

    [Fact]
    public void 공식목록은_50개주와연방구미국령을분리한다()
    {
        Assert.Equal(
            50,
            UnitedStatesImportedFoodManufacturerRegionClassifier.Definitions
                .Count(area => area.IsState));
        Assert.Single(
            UnitedStatesImportedFoodManufacturerRegionClassifier.Definitions,
            area => area.IsDistrict);
        Assert.Equal(
            5,
            UnitedStatesImportedFoodManufacturerRegionClassifier.Definitions
                .Count(area => area.IsTerritory));
    }
}
