using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Shipper;
using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Tests.Architecture;

public sealed class BaguaNavigationContractCompositionTests
{
    [Fact]
    public void 공용사방괘모델은_플랫폼URL문자열을직접소유하지않는다()
    {
        var model = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Models",
            "BaguaRoleTransitionPageModel.cs");
        var viewModel = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "ViewModels",
            "BaguaRoleTransitionPageViewModel.cs");

        Assert.Contains("BasePath = CommunityPageRoutes.Bagua", model);
        Assert.Contains("ShipperHomePageRoutes.DefaultSalesEntry", model);
        Assert.Contains("ShipperHomePageRoutes.WarehouseWorkspace", model);
        Assert.Contains("ShipperHomePageRoutes.DefaultTransportEntry", model);
        Assert.Contains("CommunityPageRoutes.GroupPurchase", model);
        Assert.DoesNotContain("\"/community", model);
        Assert.DoesNotContain("\"/shipper", model);
        Assert.Contains("커뮤니티경로 => CommunityPageRoutes.Home", viewModel);
    }

    [Fact]
    public void Web과모바일은_사방괘기본목적지route를모두제공한다()
    {
        AssertPageRoute(
            SalesOrderPageRoutes.Root,
            ("Ssalddel.WebApp", "Pages/ShipperSalesOrdersPage.razor"),
            ("SsalddelApp", "Components/Pages/SalesOrders.razor"));
        AssertPageRoute(
            ShipperHomePageRoutes.WarehouseWorkspace,
            ("Ssalddel.WebApp", "Pages/ShipperWarehouseWorkspacePage.razor"),
            ("SsalddelApp", "Components/Pages/WarehouseWorkspace.razor"));
        AssertPageRoute(
            ShipperRequestPageRoutes.Root,
            ("Ssalddel.WebApp", "Pages/ShipperRequestPage.razor"),
            ("SsalddelApp", "Components/Pages/ShipperRequestWizard.razor"));

        var webRoutes = Read("Ssalddel.WebApp", "Services", "ShipperRoutes.cs");
        var appRoutes = Read("SsalddelApp", "Services", "ShipperRoutes.cs");
        Assert.Contains("WarehouseWorkspace = ShipperHomePageRoutes.WarehouseWorkspace", webRoutes);
        Assert.Contains("WarehouseWorkspace = ShipperHomePageRoutes.WarehouseWorkspace", appRoutes);
    }

    private static void AssertPageRoute(
        string route,
        params (string Project, string RelativePath)[] pages)
    {
        foreach (var (project, relativePath) in pages)
        {
            Assert.Contains($"@page \"{route}\"", Read(project, relativePath));
        }
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
