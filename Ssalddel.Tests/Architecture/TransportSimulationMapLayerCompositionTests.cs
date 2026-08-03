namespace Ssalddel.Tests.Architecture;

public sealed class TransportSimulationMapLayerCompositionTests
{
    [Fact]
    public void 지도형홈은_시뮬레이션경고토글범례와_SvgFallback을제공한다()
    {
        var pageSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor");
        var styleSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRoleHomePage.razor.css");

        Assert.Contains("시뮬레이션/교육용/비실시간", pageSource);
        Assert.Contains("실제 운행 추적·배차 확정·개인 위치·타 조직 운영정보가 아닙니다", pageSource);
        Assert.Contains("ToggleSimulationMode", pageSource);
        Assert.Contains("ToggleSimulationAnimation", pageSource);
        Assert.Contains("고정 예시의 상태·출처·기준 보기", pageSource);
        Assert.Contains("world-community-home__simulation-fallback", pageSource);
        Assert.Contains("FallbackSimulationPath", pageSource);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", styleSource);
    }

    [Fact]
    public void Google지도는_소수객체CanvasAdapter와_향후Renderer등록경계를사용한다()
    {
        var mapScriptSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "js",
            "community-world-google-map.js");
        var simulationScriptSource = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "wwwroot",
            "js",
            "transport-simulation-map-layer.js");

        Assert.Contains("updateTransportSimulationLayer", mapScriptSource);
        Assert.Contains("transportSimulationLayer?.dispose()", mapScriptSource);
        Assert.Contains("registerTransportSimulationRenderer", simulationScriptSource);
        Assert.Contains("extends google.maps.OverlayView", simulationScriptSource);
        Assert.Contains("visibleObjectLimitForZoom", simulationScriptSource);
        Assert.Contains("routeTouchesViewport", simulationScriptSource);
        Assert.Contains("canvasOffsetX", simulationScriptSource);
        Assert.Contains("paneBounds.left - mapBounds.left", simulationScriptSource);
        Assert.Contains("requestAnimationFrame", simulationScriptSource);
        Assert.Contains("prefers-reduced-motion: reduce", simulationScriptSource);
        Assert.Contains("document.hidden", simulationScriptSource);
        Assert.Contains("sourceKindCode === \"simulated-fixture\"", simulationScriptSource);
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
