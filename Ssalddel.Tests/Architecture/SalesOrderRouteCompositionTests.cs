using Ssalddel.Contracts.Common.Sales;

namespace Ssalddel.Tests.Architecture;

public sealed class SalesOrderRouteCompositionTests
{
    [Fact]
    public void Web과_모바일은_같은_영속주문_List_Detail_route를_사용한다()
    {
        var root = FindRepositoryRoot();
        var webList = Read(root, "Ssalddel.WebApp", "Pages", "ShipperSalesOrdersPage.razor");
        var webDetail = Read(root, "Ssalddel.WebApp", "Pages", "ShipperSalesOrderDetailPage.razor");
        var appList = Read(root, "SsalddelApp", "Components", "Pages", "SalesOrders.razor");
        var appDetail = Read(root, "SsalddelApp", "Components", "Pages", "SalesOrderDetail.razor");

        Assert.Contains($"@page \"{SalesOrderPageRoutes.Root}\"", webList);
        Assert.Contains($"@page \"{SalesOrderPageRoutes.Root}\"", appList);
        Assert.Contains($"@page \"{SalesOrderPageRoutes.DetailTemplate}\"", webDetail);
        Assert.Contains($"@page \"{SalesOrderPageRoutes.DetailTemplate}\"", appDetail);
        Assert.Contains("ShipperSalesOrderWorkspaceMode.List", webList);
        Assert.Contains("ShipperSalesOrderWorkspaceMode.List", appList);
        Assert.Contains("ShipperSalesOrderWorkspaceMode.Detail", webDetail);
        Assert.Contains("ShipperSalesOrderWorkspaceMode.Detail", appDetail);
    }

    [Fact]
    public void 모바일_Simulation_작업공간은_영속주문_route와_분리한다()
    {
        var root = FindRepositoryRoot();
        var simulationPage = Read(root, "SsalddelApp", "Components", "Pages", "OrderFulfillment.razor");
        var routes = Read(root, "SsalddelApp", "Services", "ShipperRoutes.cs");

        Assert.Contains($"@page \"{SalesOrderPageRoutes.FulfillmentRoot}\"", simulationPage);
        Assert.DoesNotContain($"@page \"{SalesOrderPageRoutes.Root}\"", simulationPage);
        Assert.Contains("SalesOrders = SalesOrderPageRoutes.Root", routes);
        Assert.Contains("OrderFulfillment = SalesOrderPageRoutes.FulfillmentRoot", routes);
    }

    [Fact]
    public void 공통주문_workspace는_List와_Detail을_독립적으로_조회한다()
    {
        var root = FindRepositoryRoot();
        var source = Read(root, "Ssalddel.Ui.Common", "Areas", "App", "Components", "Sales", "ShipperSalesOrderWorkspace.razor.cs");
        var css = Read(root, "Ssalddel.Ui.Common", "Areas", "App", "Components", "Sales", "ShipperSalesOrderWorkspace.razor.css");

        Assert.Contains("ShowList", source);
        Assert.Contains("ShowDetail", source);
        Assert.Contains("var listTask = ShowList", source);
        Assert.Contains("List.페이지조회Async", source);
        Assert.Contains("ShowDetail && OrderId", source);
        Assert.Contains("SalesOrderNavigationContext", source);
        Assert.Contains("min-height: 48px", css);
    }

    private static string Read(string root, params string[] segments)
        => File.ReadAllText(Path.Combine([root, .. segments]));

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
