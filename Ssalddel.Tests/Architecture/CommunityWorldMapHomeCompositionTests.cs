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
        Assert.Contains("커뮤니티세계지도Routes.KoreaPriceDetail", source);
        Assert.Contains("커뮤니티세계지도Routes.UnitedStatesPriceDetail", source);
        Assert.Contains("한국 가격 화면 보기", source);
        Assert.Contains("미국 가격 화면 보기", source);
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
    public void 세계지도는_낮업무와_밤배움의_서로다른데이터셋을선택한다()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");

        Assert.Contains("낮 · 생활과 업무", source);
        Assert.Contains("밤 · 알아차림과 성찰", source);
        Assert.Contains("가볍게 알아차려봐요", source);
        Assert.DoesNotContain("무엇이 있는지", source);
        Assert.Contains("알아차린 사실을 관심·동의로 간주하지 않으며", source);
        Assert.Contains("YouTube지식성찰채널Catalog.항목", source);
        Assert.Contains("ScriptureDecorationCatalog.Definitions", source);
        Assert.Contains("WorldMapNightLearningDataset", source);
        Assert.Contains("종교·철학·국적은 사용자 점수나 추천 순위에 쓰지 않습니다", source);
    }

    [Fact]
    public void 세계지도경로계약은_밤배움데이터셋을_공유가능한질의로유지한다()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.Contracts",
            "Common",
            "Community",
            "CommunityPageRoutes.cs");

        Assert.Contains("WorldMap = \"/community/home\"", source);
        Assert.Contains("WorldMapDayWorkDataset = \"day-work\"", source);
        Assert.Contains("WorldMapNightLearningDataset = \"night-learning\"", source);
        Assert.Contains("WorldMapFor", source);
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
        Assert.Contains(".world-community-home--night", source);
        Assert.Contains(".world-community-home__learning-grid", source);
    }

    [Fact]
    public void 세계지도는_Google지도한개에서_낮밤데이터레이어만교체한다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var scriptSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "js",
            "community-world-google-map.js");

        Assert.Equal(1, CountOccurrences(pageSource, "id=\"community-world-google-map\""));
        Assert.Contains("./js/community-world-google-map.js", pageSource);
        Assert.Contains("CurrentMapMarkers", pageSource);
        Assert.Contains("updateDataset", pageSource);
        Assert.Contains("new GoogleMap(element", scriptSource);
        Assert.Contains("instance.map.data", scriptSource);
        Assert.Contains("auth_referrer_policy", scriptSource);
        Assert.Contains("ssalddelRuntimeConfig", scriptSource);
        Assert.DoesNotContain("AIza", scriptSource);
    }

    [Fact]
    public void 세계지도는_분야별Layer와_사용자승인형새자료대기열을제공한다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var scriptSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "js",
            "community-world-google-map.js");

        Assert.Contains("지도에 표시할 분야", pageSource);
        Assert.Contains("새 자료 지도에 반영", pageSource);
        Assert.Contains("PollMapSnapshotsAsync", pageSource);
        Assert.Contains("TimeSpan.FromSeconds(30)", pageSource);
        Assert.Contains("_pendingSnapshot", pageSource);
        Assert.Contains("markerStyleFor", scriptSource);
        Assert.Contains("regional-culture", scriptSource);
        Assert.Contains("scripture-classics", scriptSource);
    }

    [Fact]
    public void 세계지도관측Api는_개인위치나업무실행없이_안정Revision을제공한다()
    {
        var contractSource = ReadRepositoryFile(
            "Ssalddel.Contracts",
            "Common",
            "Community",
            "커뮤니티세계지도Dtos.cs");
        var useCaseSource = ReadRepositoryFile(
            "Ssalddel",
            "Services",
            "Community",
            "커뮤니티세계지도조회UseCase.cs");

        Assert.Contains("StableId", contractSource);
        Assert.Contains("Revision", contractSource);
        Assert.Contains("SourceName", contractSource);
        Assert.Contains("EvidenceAsOfUtc", contractSource);
        Assert.Contains("SHA256.HashData", useCaseSource);
        Assert.Contains("결제·주문·계약·배차를 실행하지 않습니다", useCaseSource);
    }

    private static int CountOccurrences(string source, string value)
        => source.Split(value, StringSplitOptions.None).Length - 1;

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
