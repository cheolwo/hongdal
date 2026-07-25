namespace Ssalddel.Tests.Architecture;

public sealed class RegionalCultureSpecialtyPageCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/RegionalCultureSpecialtyPage.razor")]
    [InlineData("SsalddelApp", "Components/Pages/RegionalCultureSpecialtyPage.razor")]
    public void 지역문화특산물route는_공용탐색화면을조립한다(string project, string relativePath)
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

        Assert.Contains("@page \"/community/regions\"", source);
        Assert.Contains("<RegionalCultureSpecialtyBrowse", source);
    }

    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/RegionalCultureSpecialtyDetailPage.razor")]
    [InlineData("SsalddelApp", "Components/Pages/RegionalCultureSpecialtyDetailPage.razor")]
    public void 지역문화상세route는_regionKey로_공용허브를조립한다(string project, string relativePath)
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

        Assert.Contains("@page \"/community/regions/{RegionKey}\"", source);
        Assert.Contains("<RegionalCultureSpecialtyDetail", source);
        Assert.Contains("RegionKey=\"@RegionKey\"", source);
    }

    [Fact]
    public void 공용탐색화면은_문화_특산물_근거경계와_후속탐색을함께보여준다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "RegionalCultureSpecialtyBrowse.razor"));

        Assert.Contains("문화를 알아가는 질문", source);
        Assert.Contains("특산물 탐색", source);
        Assert.Contains("EvidenceBoundary", source);
        Assert.Contains("공식 음식·재료 탐색으로 이동", source);
        Assert.Contains("주문·참여·수입이 만들어지지 않습니다", source);
        Assert.Contains("@inherits MvvmComponentBase<지역문화특산물목록PageViewModel>", source);
    }

    [Fact]
    public void 지역문화상세화면은_전용PageViewModel을사용한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "RegionalCultureSpecialtyDetail.razor"));

        Assert.Contains("@inherits MvvmComponentBase<지역문화특산물상세PageViewModel>", source);
        Assert.Contains("ViewModel.Configure(RegionKey)", source);
    }

    [Fact]
    public void Maui지역탐색은_Figma01모바일Shell과_간결한카드표현을사용한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "SsalddelApp",
            "Components",
            "Pages",
            "RegionalCultureSpecialtyPage.razor"));

        Assert.Contains("@layout CommunityMobileLayout", source);
        Assert.Contains("CommunityMobilePresentation=\"true\"", source);
    }

    [Fact]
    public void 지역카탈로그는_생성이미지와_안정적인상품key를제공한다()
    {
        var catalog = Ssalddel.Contracts.Common.Content.RegionalCultureSpecialtyCatalog.All;

        Assert.Equal(6, catalog.Count);
        Assert.All(catalog, region =>
        {
            Assert.StartsWith("_content/Ssalddel.Ui.Common/images/regions/", region.HeroImagePath);
            Assert.Contains("생성 일러스트", region.HeroImageAlt);
            Assert.NotEmpty(region.Specialties);
            Assert.All(region.Specialties, specialty => Assert.False(string.IsNullOrWhiteSpace(specialty.Key)));
        });
    }

    [Fact]
    public void 지역상세경로와_가격비교경로는_regionKey와_productKey를유지한다()
    {
        var routes = typeof(Ssalddel.Contracts.Common.Content.RegionalCultureSpecialtyRoutes);

        Assert.Equal(
            "/community/regions/cn-shandong",
            Ssalddel.Contracts.Common.Content.RegionalCultureSpecialtyRoutes.DetailFor("cn-shandong"));
        Assert.Equal(
            "/information/produce-price-comparison?regionKey=cn-shandong&productKey=apple",
            Ssalddel.Contracts.Common.Content.RegionalCultureSpecialtyRoutes.PriceComparisonFor("cn-shandong", "apple"));
        Assert.NotNull(routes);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
