using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Architecture;

public sealed class CommunityWorldMapHomeCompositionTests
{
    [Fact]
    public void ?멸퀎吏?꾨뒗_濡쒓렇?몄뿭?좎뿉留욌뒗Layer瑜쇨린蹂명솢?깊솕?섍퀬_?ㅻⅨ怨듦컻Layer?좏깮?꾩쑀吏?쒕떎()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains("@inject WebAuthSessionService AuthSession", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껽oleLayerProfileCatalog.Resolve(AuthSession.PrimaryRole)", pageSource);
        Assert.Contains("ActiveRoleProfile.RecommendedLayerCodes", pageSource);
        Assert.Contains("??븷 湲곕낯 ?덉씠??蹂듭썝", pageSource);
        Assert.Contains("異붽? 怨듦컻 ?먮즺", pageSource);
        Assert.Contains("world-community-home__role-profile", pageSource);
        Assert.Contains(".world-community-home__role-profile", styleSource);
        Assert.Contains(".is-role-optional", styleSource);
    }

    [Fact]
    public void ?멸퀎吏?꾨뒗_?먯옣援ъ“?먯꽌怨좊Ⅸ怨듦컻?멸퀎愿?먯쓣_湲곗〈怨듭떇Marker?먯쨷蹂듭뾾?댁뿰寃고븳??)
    {
        var contractSource = ReadRepositoryFile(
            "Ssalddel.Contracts",
            "Common",
            "Community",
            "而ㅻ??덊떚?멸퀎吏?껪tos.cs");
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains("ProcurementHandoff = \"procurement-handoff\"", contractSource);
        Assert.Contains("ImportReadiness = \"import-readiness\"", contractSource);
        Assert.Contains("TransportHandoff = \"transport-handoff\"", contractSource);
        Assert.Contains("WarehouseInboundHandoff = \"warehouse-inbound-handoff\"", contractSource);
        Assert.Contains("ObservationSourceLayerCodes", contractSource);
        Assert.Contains("CommunityLedgerTemplateKeys.MeatImportReadiness", contractSource);
        Assert.Contains("CommunityLedgerTemplateKeys.CargoTransport", contractSource);
        Assert.Contains("CommunityLedgerTemplateKeys.WarehouseInbound", contractSource);
        Assert.Contains("IsObservationSourceLayerVisible", pageSource);
        Assert.Contains("LayerHasVisibleSource", pageSource);
        Assert.Contains("layer.LedgerTemplateKey ?? (layer.Code switch", pageSource);
        Assert.Contains("?먯옣 愿??쨌 怨듦컻 ?덉씠??, pageSource);
        Assert.Contains(".world-community-home__layer-shape--route", styleSource);
        Assert.Contains(".world-community-home__layer-shape--warehouse", styleSource);
    }

    [Fact]
    public void ?멸퀎吏?꾨뒗_?댁쇅?쒖“?낆냼瑜??됱젙沅뚯뿭吏묎퀎Layer?怨듭옣?뷢arker濡쒗몴?쒗븳??)
    {
        var contractSource = ReadRepositoryFile(
            "Ssalddel.Contracts",
            "Common",
            "Community",
            "而ㅻ??덊떚?멸퀎吏?껪tos.cs");
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
        Assert.Contains("OverseasManufacturer = \"overseas-manufacturer\"", contractSource);
        Assert.Contains("?됱젙沅뚯뿭 ??쒖젏", pageSource);
        Assert.Contains("?щ즺 愿怨?洹쇨굅", pageSource);
        Assert.Contains("CommunityLedgerTemplateKeys.GroupImport", pageSource);
        Assert.Contains(".world-community-home__layer-shape--factory", styleSource);
        Assert.Contains("case \"overseas-manufacturer\"", scriptSource);
    }

    [Fact]
    public void 吏??쭏而ㅼ꽑?쒺anel?_吏??꽕紐낃낵寃?좎긽?쒓??덈뒗?앹꽦?대?吏瑜쇳븿猿섑몴?쒗븳??)
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains("@inject 吏??Ц?붿씠誘몄?Client RegionalImageClient", pageSource);
        Assert.Contains("regional-culture-one-each-v1", pageSource);
        Assert.Contains("SelectedRegionImage", pageSource);
        Assert.Contains("AI ?앹꽦 ?대?吏", pageSource);
        Assert.Contains("怨듭떇 湲곕줉쨌?먯궛吏쨌嫄곕옒 洹쇨굅媛 ?꾨떃?덈떎", pageSource);
        Assert.Contains("吏???대?吏瑜?遺덈윭?ㅼ? 紐삵뻽?듬땲??, pageSource);
        Assert.Contains("?ㅼ떆 ?쒕룄", pageSource);
        Assert.Contains(".world-community-home__region-visual", styleSource);
        Assert.Contains("aspect-ratio: 16 / 9", styleSource);
        Assert.Contains("width: min(440px", styleSource);
    }

    [Fact]
    public void 而ㅻ??덊떚Web?쒖옉?붾㈃?_怨듯넻硫붾돱?놁씠_?꾩껜?붾㈃吏?꾩?吏?꾩“?묓뙣?먮쭔?쒖떆?쒕떎()
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
        Assert.Contains("吏???곗씠?곗? ?덉씠??議곗옉", pageSource);
        Assert.Contains("HasSelectedCountry ? \"is-open\"", pageSource);
        Assert.Contains("?좏깮??吏???곸꽭 ?リ린", pageSource);
        Assert.Contains(".world-community-home__results", pageStyleSource);
        Assert.Contains(".world-community-home__results.is-open", pageStyleSource);
        Assert.Contains(".world-community-home__boundary", pageStyleSource);
        Assert.Contains("display: none", pageStyleSource);
        Assert.Contains("border-radius: 0", pageStyleSource);
    }

    [Fact]
    public void 而ㅻ??덊떚Web?쒖옉?붾㈃?_?멸퀎吏?꾩뿉??吏??옄猷뚮??좏깮?쒕떎()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");

        Assert.Contains("@page \"/community/home\"", source);
        Assert.Contains("<svg viewBox=\"0 0 1000 500\"", source);
        Assert.Contains("aria-controls=\"world-map-results\"", source);
        Assert.Contains("Google 吏??留덉빱 ?ㅻ낫???좏깮", source);
        Assert.Contains("@onchange=\"SelectMapFeatureFromKeyboard\"", source);
        Assert.Contains("@onkeydown=\"HandleResultsPanelKeyDown\"", source);
        Assert.Contains("_pendingFocusElementId = \"world-map-results\"", source);
        Assert.Contains("focusElement", ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "js",
            "community-world-google-map.js"));
        Assert.Contains("RegionalCultureSpecialtyCatalog.ForCountry", source);
        Assert.Contains("吏?꾨뒗 ?먮즺瑜?李얘린 ?꾪븳 媛쒕왂??, source);
    }

    [Fact]
    public void ?멸퀎吏?꾩꽑?앹?_援??_?덉씠??留덉빱_愿痢좸tableId瑜펁rl濡쒕났?먰븳??)
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var deepLink = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Services",
            "CommunityWorldMapDeepLink.cs");

        Assert.Contains("CommunityWorldMapDeepLink.CountryQueryKey", source);
        Assert.Contains("CommunityWorldMapDeepLink.LayersQueryKey", source);
        Assert.Contains("CommunityWorldMapDeepLink.MarkerQueryKey", source);
        Assert.Contains("CommunityWorldMapDeepLink.ObservationQueryKey", source);
        Assert.Contains("CommunityWorldMapDeepLink.SnapshotRevisionQueryKey", source);
        Assert.Contains("CommunityWorldMapDeepLink.SourceVersionQueryKey", source);
        Assert.Contains("ApplyDeepLinkParameters", source);
        Assert.Contains("SyncDeepLinkToUrl", source);
        Assert.Contains("Navigation.GetUriWithQueryParameters", source);
        Assert.Contains("SyncDeepLinkToUrl(replace: true)", source);
        Assert.Contains("public const string NoLayersValue = \"none\"", deepLink);
        Assert.Contains("NormalizeStableId", deepLink);
        Assert.Contains("EvidenceVersionNotice", source);
        Assert.Contains("寃뚯떆 ?댄썑 怨듦컻 洹쇨굅 媛깆떊", source);
    }

    [Fact]
    public void 吏?꾨쭏而ㅼ긽?몃뒗_怨듦컻異쒖쿂??좎꽑?꾨?利됱떆?쒖떆?섍퀬_媛寃⑹쭊?낆쓣媛寃⑸쭏而ㅻ줈?쒗븳?쒕떎()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");

        Assert.Contains("world-community-home__selection-summary", source);
        Assert.Contains("ObservationStatusLabel(selectedObservation)", source);
        Assert.Contains("EvidenceFreshnessLabel(selectedObservation.EvidenceAsOfUtc)", source);
        Assert.Contains("?먯옣쨌李몄뿬쨌二쇰Ц쨌怨꾩빟쨌諛곗감瑜?留뚮뱾吏 ?딆뒿?덈떎", source);
        Assert.Contains("SelectedObservation?.LayerCode ?? SelectedMapMarker?.LayerCode", source);
        Assert.Contains("SelectedCountry is not null && IsPriceMarketSelection", source);
        Assert.Contains("@if (IsPriceMarketSelection)", source);
        Assert.Contains("媛寃㈑룹떆??異쒖쿂", source);
        Assert.Contains("KAMIS? USDA AMS 異쒖쿂蹂?蹂닿린", source);
        Assert.Contains("USDA Agricultural Marketing Service (AMS)", source);
        Assert.Contains("USD 쨌 ???ъ옣?⑥쐞", source);
        Assert.Contains("_isPriceMarketCatalogOpen || IsPriceMarketSelection", source);
        Assert.Contains("?쒕줈 ?ㅻⅨ 援??쨌?쒖옣 ?④퀎??愿痢≪쓣 ?⑹궛?섏? ?딆뒿?덈떎", source);
        Assert.Contains("SelectedMapMarkerSummary", source);
        Assert.Contains("怨듦컻 ?쒖옣 ????꾩튂? ?쒖옣 ?④퀎", source);
        Assert.Contains("market.SourceName", source);
    }

    [Fact]
    public void ?멸퀎吏?꾨뒗_Google?ㅼ젣吏?꾩“?묎낵_吏?꾪삎Fallback吏?뺥몴?꾩쓣?쒓났?쒕떎()
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
        Assert.Contains("吏?꾪삎 媛쒕왂???쒖떆", pageSource);
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
    public void ?멸퀎吏?꾨뒗_臾명솕?먮즺?_援??蹂꾧?寃⑷렐嫄곕??④퍡?곌껐?쒕떎()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");

        Assert.Contains("RegionalCultureSpecialtyRoutes.DetailFor", source);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껽outes.KoreaPriceDetail", source);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껽outes.UnitedStatesPriceDetail", source);
        Assert.Contains("?쒓뎅 媛寃??붾㈃ 蹂닿린", source);
        Assert.Contains("誘멸뎅 媛寃??붾㈃ 蹂닿린", source);
        Assert.Contains("information/produce-price-comparison", source);
        Assert.Contains("AppRelative", source);
        Assert.Contains("異쒖쿂쨌湲곗? ?쒓컖쨌?듯솕쨌嫄곕옒 ?⑥쐞", source);
        Assert.Contains("?먮룞 媛?끒룹긽? 異붿쿇쨌二쇰Ц쨌?섏엯쨌諛곗감瑜?留뚮뱾吏 ?딆뒿?덈떎", source);
    }

    [Fact]
    public void 而ㅻ??덊떚??븷WebApp?_吏?꾩뿉?쒖뿰寃고븳媛寃⑺솕硫댁쓣?ы븿?쒕떎()
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
    public void ?듯빀Web吏?꾨뒗_?앺솢?낅Т怨듦컻?뺣낫留뚮끂異쒗븳??)
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");

        Assert.Contains("?앺솢쨌?낅Т ?듯빀 吏??, source);
        Assert.Contains("?앺솢쨌?낅Т 怨듦컻?뺣낫", source);
        Assert.Contains("private bool IsNightLearning => false", source);
        Assert.Contains("var requestedMode = MapDatasetMode.DayWork", source);
        Assert.DoesNotContain("class=\"world-community-home__dataset-switch\"", source);
        Assert.DoesNotContain("@onclick=\"ToggleDatasetAsync\"", source);
    }

    [Fact]
    public void ?멸퀎吏?꾧꼍濡쒓퀎?쎌?_諛ㅻ같??곗씠?곗뀑??怨듭쑀媛?ν븳吏덉쓽濡쒖쑀吏?쒕떎()
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
    public void 紐⑤컮?쇱뿉?쒕뒗_?멸퀎吏?꾧?_?섏씠吏媛?꾨땶吏?꾩쁺??븞?먯꽌留뚭?濡쒖씠?숉븳??)
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

    [Fact]
    public void ?멸퀎吏?꾨뒗_Google吏?꾪븳媛쒖뿉???앺솢?낅Т?곗씠?곕젅?댁뼱瑜쇨탳泥댄븳??)
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
    public void 媛쒕컻ReadOnlySimulation?_吏?꾩슂泥?컧?ъ벐湲곗?_HostedService瑜쇰퉬?쒖꽦?뷀븳??)
    {
        var programSource = ReadRepositoryFile("Ssalddel", "Program.cs");
        var middlewareSource = ReadRepositoryFile(
            "Ssalddel",
            "Middleware",
            "?ъ슜?먰뻾?꾨줈洹퇝iddleware.cs");

        Assert.Contains("executionOptions.DevelopmentReadOnly", programSource);
        Assert.Contains("builder.Services.RemoveAll<IHostedService>()", programSource);
        Assert.Contains("ShouldSkipDevelopmentMapReadAudit", middlewareSource);
        Assert.Contains("/api/v1/platform/runtime/google-maps", middlewareSource);
        Assert.Contains("/api/v1/community/world-map/observations", middlewareSource);
    }

    [Fact]
    public void Google吏?껧rowserKey??諛고룷?곗텧臾쇱뿉留뚯＜?낇븯怨??덉슜Origin?먯꽌留뚯냼鍮꾪븳??)
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
    public void ?듯빀Web吏?꾨뒗_留λ씫?녿뒗??갇Toggle?꾨끂異쒗븯吏?딅뒗??)
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        Assert.DoesNotContain("class=\"world-community-home__dataset-switch\"", pageSource);
        Assert.DoesNotContain("aria-label=\"@DatasetToggleAriaLabel\"", pageSource);
        Assert.Contains("IsNightLearning => false", pageSource);
    }

    [Fact]
    public void ?멸퀎吏?꾨뒗_遺꾩빞蹂껵ayer?_?붾㈃?대룞?녿뒗?먮룞?먮즺媛깆떊?꾩젣怨듯븳??)
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

        Assert.Contains("吏?꾩뿉 ?쒖떆??遺꾩빞", pageSource);
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
        Assert.Contains("tourism-public-evidence", scriptSource);
        Assert.Contains("online-price-public-evidence", scriptSource);
        Assert.Contains("kosis-statistical-context", scriptSource);
        Assert.Contains("scripture-classics", scriptSource);
    }

    [Fact]
    public void ?멸퀎吏?꾪븯?⑥?_?낅Т?곸뿭蹂꾩썝?쩣emplate?대??덉감瑜??묎퀬?쇱튇??)
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
        Assert.Contains("?먯옣 ?낅Т?먮쫫 ?ㅼ씠?닿렇??, pageSource);
        Assert.Contains("?덉씠???뚯븘李⑤┝ ?ㅼ씠?닿렇??, pageSource);
        Assert.Contains("?꾩옱 耳쒖쭊 ?덉씠?닿? ?놁뒿?덈떎", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.RegionalCulture", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.PublicPrice", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.WholesaleMarket", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.TraditionalMarketHub", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.TourismPublicEvidence", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.OnlinePricePublicEvidence", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.KosisStatisticalContext", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.LearningChannel", pageSource);
        Assert.Contains("而ㅻ??덊떚?멸퀎吏?껵ayerCodes.ScriptureAndClassics", pageSource);
        Assert.Contains("CommunityLedgerTemplateKeys.LocalSale", pageSource);
        Assert.Contains("CommunityLedgerTemplateKeys.GroupPurchase", pageSource);
        Assert.Contains("CommunityLedgerTemplateKeys.GroupImport", pageSource);
        Assert.Contains("CommunityLedgerTemplateCatalog.Find", pageSource);
        Assert.Contains("template.LedgerBlocks", pageSource);
        Assert.Contains("template.BlockRelations", pageSource);
        Assert.Contains("template.CompositionRules", pageSource);
        Assert.Contains("?낅Т ?곸뿭蹂?湲곗? ?먯옣 ?ъ쁺", pageSource);
        Assert.DoesNotContain("new(\"04\", \"媛?먯옣\"", pageSource);
        Assert.Contains("怨듦컻 梨꾨꼸", pageSource);
        Assert.Contains("怨듦컻 紐⑸줉", pageSource);

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
    public void ?멸퀎吏?꾩쥖?캰anel?_?낅Т?곸뿭怨쇱꽑?앹???쓣_媛숈??먯옣Template?먯뿰寃고븳??)
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains("?낅Т ?곸뿭怨??먯옣 ?곌껐", pageSource);
        Assert.Contains("world-community-home__layer-option-copy", pageSource);
        Assert.Contains("world-community-home__layer-ledger-summary", pageSource);
        Assert.Contains("{layerDiagram.DisplayName} ?곌껐", pageSource);
        Assert.Contains("SelectedDayLedgerDiagrams", pageSource);
        Assert.Contains("?좏깮 吏??쓽 ?낅Т ?곸뿭蹂??먯옣 ?곌껐", pageSource);
        Assert.Contains("?낅Т ?곸뿭蹂??먯옣", pageSource);
        Assert.Contains("?먯옣??留뚮뱾湲??꾩뿉 ?댄렣蹂대뒗 ?먮떒 ?먮즺", pageSource);
        Assert.Contains("ledger.Nodes.Take(3)", pageSource);
        Assert.Contains("?섎떒?먯꽌 ?먯옣 ?꾩껜 ?덉감 蹂닿린", pageSource);
        Assert.Contains("OpenDiagramPanel", pageSource);
        Assert.Contains(".Select(BuildLedgerWorkflowDiagram)", pageSource);
        Assert.Contains("LayerHasVisibleSource(layer, availableLayerCodes)", pageSource);

        Assert.Contains(".world-community-home__layer-option-copy", styleSource);
        Assert.Contains(".world-community-home__layer-ledger-summary", styleSource);
        Assert.Contains(".world-community-home__selected-ledgers", styleSource);
        Assert.Contains(".world-community-home__selected-ledger-list", styleSource);
    }

    [Fact]
    public void ?먯옣?곌껐Marker瑜쇱꽑?앺븯硫??섎떒?낅Т?먮쫫Diagram?꾩옄?숈쑝濡쒖뿰??)
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");

        Assert.Contains("OpenLedgerDiagramForMapFeature(observationId)", pageSource);
        Assert.Contains("private void OpenLedgerDiagramForMapFeature", pageSource);
        Assert.Contains("marker.LayerCode", pageSource);
        Assert.Contains("LayerHasVisibleSource(layer, availableSourceLayerCodes)", pageSource);
        Assert.Contains("if (hasSelectedLedgerLayer)", pageSource);
        Assert.Contains("_isDiagramPanelExpanded = true", pageSource);
    }

    [Fact]
    public void ?멸퀎吏?꾨뒗_?쒓뎅誘멸뎅媛쒕퀎?꾨ℓ?쒖옣??醫뚰몴?뺣??꾩?怨듭떇異쒖쿂寃쎄퀎濡쒗몴?쒗븳??)
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

        Assert.Equal(18, 而ㅻ??덊떚?꾨ℓ?쒖옣MapCatalog.All.Count);
        Assert.Equal(6, 而ㅻ??덊떚?꾨ℓ?쒖옣MapCatalog.ForCountry("KR").Count);
        Assert.Equal(12, 而ㅻ??덊떚?꾨ℓ?쒖옣MapCatalog.ForCountry("US").Count);
        Assert.All(而ㅻ??덊떚?꾨ℓ?쒖옣MapCatalog.ForCountry("KR"), market =>
            Assert.Equal(而ㅻ??덊떚?꾨ℓ?쒖옣?꾩튂?뺣??껩odes.?쒖옣??쒖젏, market.LocationPrecisionCode));
        Assert.All(而ㅻ??덊떚?꾨ℓ?쒖옣MapCatalog.ForCountry("US"), market =>
            Assert.Equal(而ㅻ??덊떚?꾨ℓ?쒖옣?꾩튂?뺣??껩odes.?꾩떆以묒떖?? market.LocationPrecisionCode));
        Assert.StartsWith("https://", 而ㅻ??덊떚?꾨ℓ?쒖옣MapCatalog.KoreaSourceHref, StringComparison.Ordinal);
        Assert.StartsWith("https://", 而ㅻ??덊떚?꾨ℓ?쒖옣MapCatalog.UnitedStatesSourceHref, StringComparison.Ordinal);

        Assert.Contains("GoogleMapMarker.ForMarket", pageSource);
        Assert.Contains("MapHotspotStyle", pageSource);
        Assert.Contains("aria-label=\"@($\"{country.Name} 쨌 {country.DataLabel}\")\"", pageSource);
        Assert.Contains("world-community-home__market-metadata", pageSource);
        Assert.Contains("怨듭떇 異쒖쿂", pageSource);
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
    public void 吏?꾨쭏而ㅼ삤瑜몄そ?대┃?_?몄떊泥?솕硫댁쑝濡??꾨낫臾몃㎘留뚯쟾?ы븳??)
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
        var routeSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Services",
            "CommunityMapApplicationRoutes.cs");
        var inboundSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "ShipperInboundRequestCreatePage.razor");
        var transportSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "ShipperRequestPage.razor");
        var orderSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "OrdererMartOrderRequestPage.razor");

        Assert.Contains("@oncontextmenu:preventDefault", pageSource);
        Assert.Contains("吏??留덉빱 ?좎껌 ?낅Т ?좏깮", pageSource);
        Assert.Contains("BuildChooserPath", routeSource);
        Assert.Contains("臾쇰쪟????좎껌", routeSource);
        Assert.Contains("?댁넚????좎껌", routeSource);
        Assert.Contains("媛쒕퀎 二쇰Ц ?좎껌", routeSource);
        Assert.Contains("addListener(\"contextmenu\"", scriptSource);
        Assert.DoesNotContain("addListener(\"rightclick\"", scriptSource);
        Assert.Contains("OpenMapApplicationsFromGoogleMap", scriptSource);
        Assert.Contains("data-map-application-marker-id", pageSource);
        Assert.Contains("OpenMapApplicationsFromKeyboardAsync", pageSource);
        Assert.Contains("OpenSelectedMapApplicationsAsync", pageSource);
        Assert.Contains("OpenMapApplicationChooser", pageSource);
        Assert.DoesNotContain("id=\"world-map-application-menu\"", pageSource);
        Assert.DoesNotContain("_pendingFocusElementId = \"world-map-application-menu\"", pageSource);
        Assert.DoesNotContain("aria-haspopup=\"dialog\"", pageSource);
        Assert.Contains("enableApplicationInteractions", scriptSource);
        Assert.Contains("event.shiftKey && event.key === \"F10\"", scriptSource);
        Assert.Contains("event.key === \"ContextMenu\"", scriptSource);
        Assert.Contains("addEventListener(\"pointerdown\"", scriptSource);
        Assert.Contains("}, 650);", scriptSource);
        Assert.Contains("OpenSelectedMapApplicationsFromKeyboard", scriptSource);
        Assert.Contains("CommunityMapApplicationRoutes.SourceCode", inboundSource);
        Assert.Contains("CommunityMapApplicationRoutes.SourceCode", transportSource);
        Assert.Contains("CommunityMapApplicationRoutes.SourceCode", orderSource);
        Assert.Contains("CommunityMapApplicationRoutes.ReturnToMarker", inboundSource);
        Assert.Contains("CommunityMapApplicationRoutes.ReturnToMarker", transportSource);
        Assert.Contains("CommunityMapApplicationRoutes.ReturnToMarker", orderSource);
        Assert.Contains("CommunityWorldMapDeepLink.LedgerQueryKey", pageSource);
        Assert.Contains("鍮꾧뎄??二쇰Ц ?섑뼢", orderSource);
    }

    [Fact]
    public void ?좏깮留덉빱?섎궡?먯옣Badge???몄쬆?쒕낯?몄“?뚮쭔?ъ슜?쒕떎()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var clientSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Services",
            "CommunityMapApplicationLedgerClient.cs");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains("@inject CommunityMapApplicationLedgerClient", pageSource);
        Assert.Contains("AuthSession.IsLoggedIn && _myMapLedgers.Count > 0", pageSource);
        Assert.Contains("PRIVATE 쨌 蹂몄씤?먭쾶留??쒖떆", pageSource);
        Assert.Contains("world-community-home__selected-marker-ledger", pageSource);
        Assert.Contains("world-community-home__my-ledgers", styleSource);
        Assert.Contains("world-community-home__selected-marker-ledger", styleSource);
        Assert.Contains("CommunityMapApplicationLedgerPresentation.For", pageSource);
        Assert.Contains("FindByMapMarkerAsync", pageSource);
        Assert.Contains("理쒖떊 ?곹깭 ?뺤씤", pageSource);
        Assert.Contains("RefreshMyMapLedgersAsync", pageSource);
        Assert.Contains("PollMapSnapshotsAsync", pageSource);
        Assert.Contains("await InvokeAsync(() => RefreshMyMapLedgersAsync(cancellationToken))", pageSource);
        Assert.Contains("by-map-marker", clientSource);
        Assert.Contains("message.Headers.Authorization", clientSource);
        Assert.Contains("if (!authSession.IsLoggedIn", clientSource);
        Assert.Contains("return [];", clientSource);
        Assert.DoesNotContain("AllowAnonymous", clientSource);
    }

    [Fact]
    public void ?멸퀎吏?꾧?痢좥pi??媛쒖씤?꾩튂?섏뾽臾댁떎?됱뾾???덉젙Revision?꾩젣怨듯븳??)
    {
        var contractSource = ReadRepositoryFile(
            "Ssalddel.Contracts",
            "Common",
            "Community",
            "而ㅻ??덊떚?멸퀎吏?껪tos.cs");
        var useCaseSource = ReadRepositoryFile(
            "Ssalddel",
            "Services",
            "Community",
            "而ㅻ??덊떚?멸퀎吏?꾩“?똗seCase.cs");

        Assert.Contains("StableId", contractSource);
        Assert.Contains("Revision", contractSource);
        Assert.Contains("SourceName", contractSource);
        Assert.Contains("EvidenceAsOfUtc", contractSource);
        Assert.Contains("SHA256.HashData", useCaseSource);
        Assert.Contains("寃곗젣쨌二쇰Ц쨌怨꾩빟쨌諛곗감瑜??ㅽ뻾?섏? ?딆뒿?덈떎", useCaseSource);
    }

    [Fact]
    public void ?댁뒪異쒖쿂留덉빱???몃줎?촄eed?곹깭?_蹂꾨룄怨듭떇Rss?꾨낫瑜쇨뎄遺꾪븳??)
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var clientSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Services",
            "而ㅻ??덊떚?멸퀎吏?껩lient.cs");
        var controllerSource = ReadRepositoryFile(
            "Ssalddel",
            "Controllers",
            "Common",
            "而ㅻ??덊떚?멸퀎吏?꾨돱?ㅽ썑蹂퀰ontroller.cs");

        Assert.Contains("RSS REVIEW 쨌 ?먮룞 寃뚯떆 ?놁쓬", pageSource);
        Assert.Contains("???댁뒪 異쒖쿂??湲곗궗 ?섏쭛 ?곹깭", pageSource);
        Assert.Contains("媛숈? 援????蹂꾨룄 怨듭떇?댁뒪 RSS ?좏깮", pageSource);
        Assert.Contains("?몃? ?ㅽ뙣瑜?鍮?湲곗궗 紐⑸줉?쇰줈 ?④린吏 ?딆뒿?덈떎", pageSource);
        Assert.Contains("?댁뒪?꾨낫議고쉶Async", clientSource);
        Assert.Contains("sourceKey", clientSource);
        Assert.Contains("news-candidates", controllerSource);
        Assert.Contains("SsalddelCodeEffect.PersistentRead", controllerSource);
        Assert.Contains("?댁쁺 寃???먯옣?먯꽌 ?뱀씤??snapshot留?議고쉶", controllerSource);
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

        throw new DirectoryNotFoundException("Ssalddel ??μ냼 猷⑦듃瑜?李얠? 紐삵뻽?듬땲??");
    }
}
