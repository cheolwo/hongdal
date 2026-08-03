using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Tests.Ui.Common;

public sealed class RegionalAgriculturalMapPresentationTests
{
    [Fact]
    public void Web과Maui는_같은한국미국지역지도Workspace를사용한다()
    {
        var webPage = Read("Ssalddel.WebApp", "Pages", "KoreaAgriculturalMapPage.razor");
        var appPage = Read("SsalddelApp", "Components", "Pages", "KoreaAgriculturalMapPage.razor");
        var component = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "KoreaAgriculturalMapWorkspace.razor");

        Assert.Contains("@page \"/information/korea-agricultural-map\"", webPage);
        Assert.Contains("@page \"/information/regional-agricultural-map\"", webPage);
        Assert.Contains("<KoreaAgriculturalMapWorkspace InitialCountryCode=", webPage);
        Assert.Contains("InitialContentLayerKey=", webPage);
        Assert.Contains("SupplyParameterFromQuery(Name = \"layer\")", webPage);
        Assert.Contains("@page \"/information/korea-agricultural-map\"", appPage);
        Assert.Contains("@page \"/information/regional-agricultural-map\"", appPage);
        Assert.Contains("<KoreaAgriculturalMapWorkspace InitialCountryCode=", appPage);
        Assert.Contains("InitialContentLayerKey=", appPage);
        Assert.Contains("SupplyParameterFromQuery(Name = \"layer\")", appPage);
        Assert.Contains("공개 정보 지도 레이어", component);
        Assert.Contains("지도 콘텐츠 레이어 계층", component);
        Assert.Contains("홍익학당 철학·영상", component);
        Assert.Contains("검증된 홍익학당 지리 기록이 없습니다", component);
        Assert.Contains("수산·바다 구역", component);
        Assert.Contains("공식 어획구역 바다 타일", component);
        Assert.Contains("korea-agri-map__ocean-tile", component);
        Assert.Contains("애니메이션도 실시간 변화나 이동을 뜻하지 않습니다", component);
        Assert.Contains("prefers-reduced-motion", Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "KoreaAgriculturalMapWorkspace.razor.css"));
        Assert.Contains("RegionalAgriculturalMapCountryCodes.UnitedStates", component);
        Assert.Contains("관계 레이어 선택", component);
        Assert.Contains("Shipping Point는 원산지로 단정하지 않습니다", component);
        Assert.Contains("ViewModel.Markers", component);
        Assert.Contains("실제 농장·창고·개인의 위치", component);
        Assert.Contains("가격 게시판", component);
        Assert.Contains("현재 표시할 검증된 지역 마커가 없습니다", component);
        Assert.Contains("다시 시도", component);
    }

    [Fact]
    public void 커뮤니티역할WebApp은_한국지도페이지를_역할빌드에포함한다()
    {
        var props = Read("eng", "web-role-app", "Ssalddel.RoleWebApp.props");
        var publicData = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "농수산공공데이터Workspace.razor");

        Assert.Contains("KoreaAgriculturalMapPage.razor", props);
        Assert.Contains("RegionalAgriculturalMapRoutes.ForCountry", publicData);
    }

    [Fact]
    public void 공통지역지도와_기존한국주소는_읽기전용PageCapability를가진다()
    {
        Assert.True(SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            RegionalAgriculturalMapRoutes.ForCountry(RegionalAgriculturalMapCountryCodes.UnitedStates),
            out var regional));
        Assert.Equal("regional-agricultural-map", regional.PageKey);
        Assert.Equal(PageInteractionBoundary.ReadOnly, regional.Boundary);

        Assert.True(SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            RegionalAgriculturalMapRoutes.KoreaMap,
            out var legacy));
        Assert.Equal("korea-regional-agricultural-map-legacy", legacy.PageKey);
    }

    private static string Read(params string[] segments)
        => File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

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
