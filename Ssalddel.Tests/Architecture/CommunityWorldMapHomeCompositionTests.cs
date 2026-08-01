namespace Ssalddel.Tests.Architecture;

public sealed class CommunityWorldMapHomeCompositionTests
{
    [Fact]
    public void 커뮤니티Web시작화면은_세계지도에서_지역자료를선택한다()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");

        Assert.Contains("@page \"/community/home\"", source);
        Assert.Contains("<svg viewBox=\"0 0 1000 500\"", source);
        Assert.Contains("aria-controls=\"world-map-results\"", source);
        Assert.Contains("RegionalCultureSpecialtyCatalog.ForCountry", source);
        Assert.Contains("지도는 자료를 찾기 위한 개략도", source);
    }

    [Fact]
    public void 세계지도는_문화자료와_국가별가격근거를함께연결한다()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");

        Assert.Contains("RegionalCultureSpecialtyRoutes.DetailFor", source);
        Assert.Contains("information/kamis-domestic-price-comparison", source);
        Assert.Contains("information/usda-us-price-comparison", source);
        Assert.Contains("information/produce-price-comparison", source);
        Assert.Contains("AppRelative", source);
        Assert.Contains("출처·기준 시각·통화·거래 단위", source);
        Assert.Contains("자동 가입·상대 추천·주문·수입·배차를 만들지 않습니다", source);
    }

    [Fact]
    public void 커뮤니티역할WebApp은_지도에서연결한가격화면을포함한다()
    {
        var source = ReadRepositoryFile(
            "eng",
            "web-role-app",
            "Ssalddel.RoleWebApp.props");

        Assert.Contains("PublicDataInformationPage.razor", source);
        Assert.Contains("KamisDomesticPriceComparisonPage.razor", source);
        Assert.Contains("ProduceRegionalPriceComparisonPage.razor", source);
        Assert.Contains("UsdaUnitedStatesPriceComparisonPage.razor", source);
    }

    [Fact]
    public void 모바일에서는_세계지도가_페이지가아닌지도영역안에서만가로이동한다()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains(".world-community-home > *", source);
        Assert.Contains("min-width: 0", source);
        Assert.Contains(".world-community-home__map-scroll", source);
        Assert.Contains("overflow-x: auto", source);
    }

    private static string ReadRepositoryFile(params string[] relativePath)
        => File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. relativePath]));

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
