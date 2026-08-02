using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Architecture;

public sealed class CommunityWorldMapHomeCompositionTests
{
    [Fact]
    public void 커뮤니티Web시작화면은_공통메뉴없이_전체화면지도와지도조작패널만표시한다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var pageStyleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");
        var layoutSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Layout",
            "MapOnlyLayout.razor");

        Assert.Contains("@layout MapOnlyLayout", pageSource);
        Assert.Contains("world-community-home__map-fallback", pageSource);
        Assert.Contains("preserveAspectRatio=\"xMidYMid meet\"", pageSource);
        Assert.Contains("@Body", layoutSource);
        Assert.DoesNotContain("NavMenu", layoutSource);
        Assert.DoesNotContain("MudAppBar", layoutSource);
        Assert.Contains("height: 100dvh", pageStyleSource);
        Assert.Contains("world-community-home__map-controls", pageSource);
        Assert.Contains("지도 데이터와 레이어 조작", pageSource);
        Assert.Contains("HasSelectedCountry ? \"is-open\"", pageSource);
        Assert.Contains("선택한 지역 상세 닫기", pageSource);
        Assert.Contains(".world-community-home__results", pageStyleSource);
        Assert.Contains(".world-community-home__results.is-open", pageStyleSource);
        Assert.Contains(".world-community-home__boundary", pageStyleSource);
        Assert.Contains("display: none", pageStyleSource);
        Assert.Contains("border-radius: 0", pageStyleSource);
    }

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
    public void 세계지도는_Google실제지도조작과_지도형Fallback지형표현을제공한다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");
        var scriptSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "js",
            "community-world-google-map.js");

        Assert.Contains("world-community-home__ocean", pageSource);
        Assert.Contains("world-community-home__country-boundaries", pageSource);
        Assert.Contains("world-community-home__water-labels", pageSource);
        Assert.Contains("world-community-home__coordinate-labels", pageSource);
        Assert.Contains("world-community-home__compass", pageSource);
        Assert.Contains("world-community-home__scale", pageSource);
        Assert.Contains("지도형 개략도 표시", pageSource);
        Assert.Contains("url(#world-map-land)", styleSource);
        Assert.Contains(".world-community-home--night .world-community-home__ocean", styleSource);

        Assert.Contains("mapTypeId: \"roadmap\"", scriptSource);
        Assert.Contains("mapTypeControl: true", scriptSource);
        Assert.Contains("[\"roadmap\", \"terrain\", \"satellite\"]", scriptSource);
        Assert.Contains("scaleControl: true", scriptSource);
        Assert.Contains("streetViewControl: true", scriptSource);
        Assert.Contains("zoomControl: true", scriptSource);
        Assert.Contains("gestureHandling: \"greedy\"", scriptSource);
        Assert.Contains("mapViewportPadding", scriptSource);
        Assert.Contains("selectedScale", scriptSource);
        Assert.DoesNotContain("case \"public-price\":\n            return { color: \"#ef8f3c\", path: \"M 0,-10 10,0 0,10 -10,0 z\" };", scriptSource);
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
        Assert.Contains("Layout\\MapOnlyLayout.razor", source);
        Assert.Contains("Layout\\MapOnlyLayout.razor.css", source);
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
        Assert.Contains("suppliedRuntimeConfig", scriptSource);
        Assert.Contains("GoogleMapsRuntimeClient.TryGetAsync", pageSource);
        Assert.DoesNotContain("AIza", scriptSource);
    }

    [Fact]
    public void Google지도BrowserKey는_배포산출물에만주입하고_허용Origin에서만소비한다()
    {
        var indexSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "index.html");
        var runtimeConfigSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "runtime-config.js");
        var mapScriptSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "js",
            "community-world-google-map.js");
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var injectionScriptSource = ReadRepositoryFile(
            "eng",
            "inject-web-runtime-config.ps1");

        var runtimeConfigIndex = indexSource.IndexOf("runtime-config.js", StringComparison.Ordinal);
        var blazorBootIndex = indexSource.IndexOf("_framework/blazor.webassembly", StringComparison.Ordinal);

        Assert.True(runtimeConfigIndex >= 0);
        Assert.True(blazorBootIndex > runtimeConfigIndex);
        Assert.Contains("strict-origin-when-cross-origin", indexSource);
        Assert.Contains("ssalddelRuntimeConfig ??= {}", runtimeConfigSource);
        Assert.DoesNotContain("googleMapsBrowserApiKey", runtimeConfigSource);
        Assert.DoesNotContain("AIza", runtimeConfigSource);

        Assert.Contains("consumeRuntimeValue", mapScriptSource);
        Assert.Contains("googleMapsAllowedOrigins", mapScriptSource);
        Assert.Contains("blocked-origin", mapScriptSource);
        Assert.True(
            mapScriptSource.IndexOf("isRuntimeOriginAllowed(runtimeConfig)", StringComparison.Ordinal)
            < mapScriptSource.IndexOf("consumeRuntimeValue(runtimeConfig", StringComparison.Ordinal));
        Assert.Contains("delete runtimeConfig[configName]", mapScriptSource);
        Assert.DoesNotContain("ssalddel-google-maps-browser-key", mapScriptSource);
        Assert.DoesNotContain("document.querySelector(`meta", mapScriptSource);
        Assert.Contains("strict-origin-when-cross-origin", mapScriptSource);
        Assert.Contains("script.remove()", mapScriptSource);
        Assert.Contains("GoogleMapState.BlockedOrigin", pageSource);

        Assert.Contains("SSALDDEL_GOOGLE_MAPS_BROWSER_API_KEY", injectionScriptSource);
        Assert.Contains("GoogleMaps:BrowserApiKey", injectionScriptSource);
        Assert.DoesNotContain("GoogleMaps:UnifiedApiKey", injectionScriptSource);
        Assert.Contains("Refusing to inject", injectionScriptSource);
        Assert.Contains("AllowLoopback", injectionScriptSource);
        Assert.Contains("Do not mix loopback and remote origins", injectionScriptSource);
        Assert.Contains("must not contain user information", injectionScriptSource);
        Assert.Contains("UriSchemeHttps", injectionScriptSource);
        Assert.Contains("uri.IsLoopback", injectionScriptSource);
        Assert.Contains("ConvertTo-Json -Compress", injectionScriptSource);
        Assert.Contains("runtime-config.js?v=", injectionScriptSource);
    }

    [Fact]
    public void 지도낮밤선택은_현재상태를표시하는_단일Toggle이다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Equal(1, CountOccurrences(pageSource, "class=\"world-community-home__dataset-switch\""));
        Assert.Contains("@onclick=\"ToggleDatasetAsync\"", pageSource);
        Assert.Contains("aria-label=\"@DatasetToggleAriaLabel\"", pageSource);
        Assert.Contains("현재 낮 · 생활과 업무", pageSource);
        Assert.Contains("현재 밤 · 알아차림과 성찰", pageSource);
        Assert.Contains("SelectDataset(IsNightLearning ? MapDatasetMode.DayWork : MapDatasetMode.NightLearning)", pageSource);
        Assert.DoesNotContain("@onclick=\"() => SelectDataset(MapDatasetMode.DayWork)\"", pageSource);
        Assert.DoesNotContain("@onclick=\"() => SelectDataset(MapDatasetMode.NightLearning)\"", pageSource);
        Assert.Contains("grid-template-columns: 1fr", styleSource);
    }

    [Fact]
    public void 세계지도는_분야별Layer와_화면이동없는자동자료갱신을제공한다()
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
        Assert.Contains("PollMapSnapshotsAsync", pageSource);
        Assert.Contains("TimeSpan.FromSeconds(30)", pageSource);
        Assert.Contains("preserveViewport: true", pageSource);
        Assert.DoesNotContain("_pendingSnapshot", pageSource);
        Assert.Contains("preserveViewport = false", scriptSource);
        Assert.Contains("!preserveViewport", scriptSource);
        Assert.Contains("markerStyleFor", scriptSource);
        Assert.Contains("regional-culture", scriptSource);
        Assert.Contains("wholesale-market", scriptSource);
        Assert.Contains("traditional-market-hub", scriptSource);
        Assert.Contains("scripture-classics", scriptSource);
    }

    [Fact]
    public void 세계지도하단은_업무영역별원장Template내부절차를_접고펼친다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains("id=\"world-map-layer-diagrams\"", pageSource);
        Assert.Contains("aria-controls=\"world-map-layer-diagram-content\"", pageSource);
        Assert.Contains("_isDiagramPanelExpanded", pageSource);
        Assert.Contains("ActiveLayerDiagrams", pageSource);
        Assert.Contains("원장 업무흐름 다이어그램", pageSource);
        Assert.Contains("레이어 알아차림 다이어그램", pageSource);
        Assert.Contains("현재 켜진 레이어가 없습니다", pageSource);
        Assert.Contains("커뮤니티세계지도LayerCodes.RegionalCulture", pageSource);
        Assert.Contains("커뮤니티세계지도LayerCodes.PublicPrice", pageSource);
        Assert.Contains("커뮤니티세계지도LayerCodes.WholesaleMarket", pageSource);
        Assert.Contains("커뮤니티세계지도LayerCodes.TraditionalMarketHub", pageSource);
        Assert.Contains("커뮤니티세계지도LayerCodes.LearningChannel", pageSource);
        Assert.Contains("커뮤니티세계지도LayerCodes.ScriptureAndClassics", pageSource);
        Assert.Contains("CommunityLedgerTemplateKeys.LocalSale", pageSource);
        Assert.Contains("CommunityLedgerTemplateKeys.GroupPurchase", pageSource);
        Assert.Contains("CommunityLedgerTemplateKeys.GroupImport", pageSource);
        Assert.Contains("CommunityLedgerTemplateCatalog.Find", pageSource);
        Assert.Contains("template.LedgerBlocks", pageSource);
        Assert.Contains("template.BlockRelations", pageSource);
        Assert.Contains("template.CompositionRules", pageSource);
        Assert.Contains("업무 영역별 기준 원장 투영", pageSource);
        Assert.DoesNotContain("new(\"04\", \"가원장\"", pageSource);
        Assert.Contains("공개 채널", pageSource);
        Assert.Contains("공개 목록", pageSource);

        foreach (var templateKey in new[]
                 {
                     CommunityLedgerTemplateKeys.LocalSale,
                     CommunityLedgerTemplateKeys.GroupPurchase,
                     CommunityLedgerTemplateKeys.GroupImport
                 })
        {
            var template = CommunityLedgerTemplateCatalog.Find(templateKey);
            Assert.NotEmpty(template.LedgerBlocks);
            Assert.NotEmpty(template.BlockRelations);
        }

        Assert.Contains(".world-community-home__diagram-panel", styleSource);
        Assert.Contains(".world-community-home__diagram-panel.is-expanded", styleSource);
        Assert.Contains(".world-community-home__diagram-track", styleSource);
        Assert.Contains(".world-community-home__diagram-panel.has-results", styleSource);
        Assert.Contains("prefers-reduced-motion: reduce", styleSource);
    }

    [Fact]
    public void 세계지도좌우Panel은_업무영역과선택지역을_같은원장Template에연결한다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains("업무 영역과 원장 연결", pageSource);
        Assert.Contains("world-community-home__layer-option-copy", pageSource);
        Assert.Contains("world-community-home__layer-ledger-summary", pageSource);
        Assert.Contains("{layerDiagram.DisplayName} 연결", pageSource);
        Assert.Contains("SelectedDayLedgerDiagrams", pageSource);
        Assert.Contains("선택 지역의 업무 영역별 원장 연결", pageSource);
        Assert.Contains("업무 영역별 원장", pageSource);
        Assert.Contains("원장을 만들기 전에 살펴보는 판단 자료", pageSource);
        Assert.Contains("ledger.Nodes.Take(3)", pageSource);
        Assert.Contains("하단에서 원장 전체 절차 보기", pageSource);
        Assert.Contains("OpenDiagramPanel", pageSource);
        Assert.Contains(".Select(BuildLedgerWorkflowDiagram)", pageSource);
        Assert.Contains("availableLayerCodes.Contains(layer.Code)", pageSource);

        Assert.Contains(".world-community-home__layer-option-copy", styleSource);
        Assert.Contains(".world-community-home__layer-ledger-summary", styleSource);
        Assert.Contains(".world-community-home__selected-ledgers", styleSource);
        Assert.Contains(".world-community-home__selected-ledger-list", styleSource);
    }

    [Fact]
    public void 세계지도는_한국미국개별도매시장을_좌표정밀도와공식출처경계로표시한다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");
        var scriptSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "js",
            "community-world-google-map.js");

        Assert.Equal(18, 커뮤니티도매시장MapCatalog.All.Count);
        Assert.Equal(6, 커뮤니티도매시장MapCatalog.ForCountry("KR").Count);
        Assert.Equal(12, 커뮤니티도매시장MapCatalog.ForCountry("US").Count);
        Assert.All(커뮤니티도매시장MapCatalog.ForCountry("KR"), market =>
            Assert.Equal(커뮤니티도매시장위치정밀도Codes.시장대표점, market.LocationPrecisionCode));
        Assert.All(커뮤니티도매시장MapCatalog.ForCountry("US"), market =>
            Assert.Equal(커뮤니티도매시장위치정밀도Codes.도시중심점, market.LocationPrecisionCode));
        Assert.StartsWith("https://", 커뮤니티도매시장MapCatalog.KoreaSourceHref, StringComparison.Ordinal);
        Assert.StartsWith("https://", 커뮤니티도매시장MapCatalog.UnitedStatesSourceHref, StringComparison.Ordinal);

        Assert.Contains("GoogleMapMarker.ForMarket", pageSource);
        Assert.Contains("MapHotspotStyle", pageSource);
        Assert.Contains("aria-label=\"@($\"{country.Name} · {country.DataLabel}\")\"", pageSource);
        Assert.Contains("world-community-home__market-metadata", pageSource);
        Assert.Contains("공식 출처", pageSource);
        Assert.Contains("ConsolidateLedgerDiagrams", pageSource);
        Assert.Contains(".world-community-home__hotspot--market", styleSource);
        Assert.Contains(".world-community-home__layer-shape--market", styleSource);
        Assert.Contains("case \"wholesale-market\"", scriptSource);
        Assert.Contains("case \"traditional-market-hub\"", scriptSource);
        Assert.Contains("? first.Id", pageSource);
        Assert.Contains("SelectMapFeatureFromGoogleMap", scriptSource);
        Assert.Contains("selectedMarkerId", scriptSource);
        Assert.Contains("focusSelection(instance)", scriptSource);
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
