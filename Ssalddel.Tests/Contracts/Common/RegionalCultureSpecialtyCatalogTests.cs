using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Tests.Contracts.Common;

public sealed class RegionalCultureSpecialtyCatalogTests
{
    [Fact]
    public void 지역탐색은_미국주와_중국현재행정구역및문화권을_구분한다()
    {
        Assert.Equal("/community/regions", RegionalCultureSpecialtyRoutes.Browse);
        Assert.Contains(
            RegionalCultureSpecialtyCatalog.All,
            item => item.CountryCode == "US" && item.RegionType == "현재 주");
        Assert.Contains(
            RegionalCultureSpecialtyCatalog.All,
            item => item.Key == "cn-shandong" && item.RegionType == "현재 성");
        Assert.Contains(
            RegionalCultureSpecialtyCatalog.All,
            item => item.Key == "cn-liaodong" && item.RegionType == "역사·지리권");
        Assert.Contains(
            RegionalCultureSpecialtyCatalog.All,
            item => item.Key == "cn-south-yangtze" && item.RegionType == "넓은 문화·지리권");
    }

    [Fact]
    public void 모든지역은_문화질문_특산물_근거경계를_함께제공한다()
    {
        Assert.All(
            RegionalCultureSpecialtyCatalog.All,
            region =>
            {
                Assert.NotEmpty(region.CultureQuestions);
                Assert.NotEmpty(region.Specialties);
                Assert.NotEmpty(region.EvidenceBoundary);
            });

        Assert.Contains(
            "현재 행정구역명이 아닙니다",
            RegionalCultureSpecialtyCatalog.All.Single(item => item.Key == "cn-liaodong").EvidenceBoundary,
            StringComparison.Ordinal);
        Assert.Contains(
            "매우 넓습니다",
            RegionalCultureSpecialtyCatalog.All.Single(item => item.Key == "cn-south-yangtze").EvidenceBoundary,
            StringComparison.Ordinal);
    }

    [Fact]
    public void 국가필터는_선택한국가의지역만반환한다()
    {
        var unitedStates = RegionalCultureSpecialtyCatalog.ForCountry("us");
        var china = RegionalCultureSpecialtyCatalog.ForCountry(" CN ");

        Assert.All(unitedStates, item => Assert.Equal("US", item.CountryCode));
        Assert.All(china, item => Assert.Equal("CN", item.CountryCode));
        Assert.Equal(RegionalCultureSpecialtyCatalog.All, RegionalCultureSpecialtyCatalog.ForCountry(null));
    }
}
