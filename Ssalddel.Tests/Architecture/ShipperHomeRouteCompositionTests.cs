using Ssalddel.Contracts.Shipper;

namespace Ssalddel.Tests.Architecture;

public sealed class ShipperHomeRouteCompositionTests
{
    [Fact]
    public void Web과모바일은_같은화주허브route와공용Screen을사용한다()
    {
        var web = Read("Ssalddel.WebApp", "Pages", "ShipperHome.razor");
        var app = Read("SsalddelApp", "Components", "Pages", "Home.razor");
        var appShell = Read("SsalddelApp", "Components", "Shared", "ShipperHomeAppShell.razor");

        Assert.Contains($"@page \"{ShipperHomePageRoutes.Root}\"", web);
        Assert.Contains($"@page \"{ShipperHomePageRoutes.Root}\"", app);
        Assert.Contains("<ShipperHomeScreen", web);
        Assert.Contains("<ShipperHomeAppShell", app);
        Assert.Contains("<ShipperHomeScreen", appShell);
        Assert.True(app.Split('\n').Length <= 5);
    }

    [Fact]
    public void 공용Screen은_플랫폼서비스와route를소유하지않는다()
    {
        var screen = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Shipper",
            "ShipperHomeScreen.razor");

        Assert.DoesNotContain("@page", screen);
        Assert.DoesNotContain("Ssalddel.WebApp", screen);
        Assert.DoesNotContain("SsalddelApp", screen);
        Assert.DoesNotContain("IShipperOperationsService", screen);
        Assert.DoesNotContain("WebAuthSessionService", screen);
        Assert.Contains("FeatureMetadataAvailable", screen);
        Assert.Contains("IsFeatureEnabled", screen);
        Assert.Contains("0.0 기본 비활성", screen);
    }

    [Fact]
    public void RoutePage는_API조회와업무상태계산을하지않는다()
    {
        var web = Read("Ssalddel.WebApp", "Pages", "ShipperHome.razor");
        var app = Read("SsalddelApp", "Components", "Pages", "Home.razor");

        foreach (var source in new[] { web, app })
        {
            Assert.DoesNotContain("HttpClient", source);
            Assert.DoesNotContain("GetAsync<", source);
            Assert.DoesNotContain("GetRequestsAsync", source);
            Assert.DoesNotContain("try", source);
            Assert.DoesNotContain("catch", source);
        }
    }

    [Fact]
    public void 비활성workflow의업무API는_공용client에서호출되지않는다()
    {
        var client = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Services",
            "ShipperHomeDashboardClient.cs");

        Assert.Contains("if (!IsEnabled(flags, ShipperHomeFeatureKeys.DomesticTransport))", client);
        Assert.Contains("if (!IsEnabled(flags, ShipperHomeFeatureKeys.WarehouseFulfillment))", client);
        Assert.Contains("return [];", client);
        Assert.Contains("PageInteractionBoundary.ReadOnly", Read(
            "Ssalddel.Contracts",
            "Common",
            "Versioning",
            "PageCapabilityDtos.cs"));
    }

    [Fact]
    public void 공용화주허브는_모바일단일열과48px행동을가진다()
    {
        var css = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Shipper",
            "ShipperHomeScreen.razor.css");

        Assert.Contains("@media (max-width: 720px)", css);
        Assert.Contains("grid-template-columns: minmax(0, 1fr)", css);
        Assert.Contains("min-height: 48px", css);
        Assert.Contains("overflow-wrap: anywhere", css);
    }

    [Fact]
    public void Web전문도구목록은_공용허브아래에보존된다()
    {
        var directory = Read(
            "Ssalddel.WebApp",
            "Pages",
            "ShipperHome",
            "ShipperHomeWebToolDirectory.razor");

        Assert.Contains("ShipperRoutes.RequestBulk", directory);
        Assert.Contains("ShipperRoutes.PublicCargo", directory);
        Assert.Contains("ShipperRoutes.WarehouseScan", directory);
        Assert.Contains("ShipperRoutes.SalesChannels", directory);
        Assert.Contains("ShipperRoutes.CustomsHsReviews", directory);
    }

    [Fact]
    public void Web공통header는_모바일에서짧은제목과48px언어행동을사용한다()
    {
        var layout = Read("Ssalddel.WebApp", "Layout", "MainLayout.razor");
        var appCss = Read("Ssalddel.WebApp", "wwwroot", "css", "app.css");
        var languageSwitcher = Read("Ssalddel.WebApp", "Shared", "WebLanguageSwitcher.razor");

        Assert.Contains("web-app-title__full", layout);
        Assert.Contains("web-app-title__compact", layout);
        Assert.Contains("@media (max-width: 720px)", appCss);
        Assert.Contains(".web-app-title__compact", appCss);
        Assert.Contains("min-width: 48px", languageSwitcher);
        Assert.Contains("min-height: 48px", languageSwitcher);
    }

    private static string Read(params string[] segments)
        => File.ReadAllText(Path.Combine(new[] { FindRepositoryRoot() }.Concat(segments).ToArray()));

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
