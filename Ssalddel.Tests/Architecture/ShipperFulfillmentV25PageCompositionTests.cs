namespace Ssalddel.Tests.Architecture;

public sealed class ShipperFulfillmentV25PageCompositionTests
{
    [Fact]
    public void 화주재고route는_조회와_다음전용route이동만담당한다()
    {
        var path = WebPage("ShipperWarehouseInventoryPage.razor");
        var source = File.ReadAllText(path);

        Assert.True(File.ReadLines(path).Count() <= 40);
        Assert.Contains("<SsalddelWarehouseInventoryHub", source);
        Assert.Contains("SalesProductCreateForInventory", source);
        Assert.Contains("ReconsignmentOrdersForInventory", source);
        Assert.DoesNotContain("I상품등록Service", source);
        Assert.DoesNotContain("I채널출품Service", source);
        Assert.DoesNotContain("판매상품저장요청", source);
        Assert.DoesNotContain("CreateSaleAsync", source);
        Assert.DoesNotContain("MudNumericField", source);
    }

    [Fact]
    public void 판매상품과채널출품은_목록과생성route를각각분리한다()
    {
        var products = File.ReadAllText(WebPage("ShipperSalesProductsPage.razor"));
        var productCreate = File.ReadAllText(WebPage("ShipperSalesProductCreatePage.razor"));
        var listings = File.ReadAllText(WebPage("ShipperProductListingsPage.razor"));
        var listingCreate = File.ReadAllText(WebPage("ShipperSalesListingCreatePage.razor"));

        Assert.Contains("@page \"/shipper/sales/products\"", products);
        Assert.Contains("ShipperSalesProductsPageViewModel", products);
        Assert.DoesNotContain("상품생성Async", products);

        Assert.Contains("@page \"/shipper/sales/products/new\"", productCreate);
        Assert.Contains("ShipperSalesProductCreatePageViewModel", productCreate);
        Assert.DoesNotContain("I채널출품Service", productCreate);

        Assert.Contains("@page \"/shipper/sales/listings\"", listings);
        Assert.Contains("ShipperSalesListingsPageViewModel", listings);
        Assert.DoesNotContain("출품생성Async", listings);
        Assert.DoesNotContain("MudSelect", listings);

        Assert.Contains("@page \"/shipper/sales/listings/new\"", listingCreate);
        Assert.Contains("ShipperSalesListingCreatePageViewModel", listingCreate);
        Assert.Contains("외부 발행 없음", listingCreate);
    }

    [Fact]
    public void Web창고와판매는_운영Api실패를_고정샘플로대체하지않는다()
    {
        var warehouseService = File.ReadAllText(
            Path.Combine(
                FindRepositoryRoot(),
                "Ssalddel.WebApp",
                "Services",
                "WebShipperWarehouseWorkspaceService.cs"));
        var program = File.ReadAllText(
            Path.Combine(FindRepositoryRoot(), "Ssalddel.WebApp", "Program.cs"));

        Assert.Contains("ISsalddelJsonApiClient", warehouseService);
        Assert.Contains("api/v1/warehouse-operations", warehouseService);
        Assert.Contains("allowNotFound: false", warehouseService);
        Assert.DoesNotContain("shipper-web-demo", warehouseService);
        Assert.DoesNotContain("김포 물류 허브", warehouseService);
        Assert.False(File.Exists(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.WebApp",
            "Services",
            "WebShipperSalesWorkspaceService.cs")));
        Assert.DoesNotContain("WebShipperSalesWorkspaceService", program);
    }

    [Fact]
    public void 창고작업보드는_고정작업이아닌_목적별읽기허브다()
    {
        var path = WebPage("WarehouseWorkBoardPage.razor");
        var source = File.ReadAllText(path);

        Assert.Contains("<WebPageLinkGrid", source);
        Assert.Contains("읽기 전용 허브", source);
        Assert.Contains("WarehouseManagerRoutes.PickingBatch", source);
        Assert.DoesNotContain("INB-1001", source);
        Assert.DoesNotContain("NAVER-20260704", source);
        Assert.DoesNotContain("BoardItem", source);
        Assert.DoesNotContain("OnClick", source);
    }

    [Fact]
    public void Azure2점5override는_Operational과외부판매동기화를고정한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "deploy",
            "azure-vm",
            "compose.fulfillment-v25.override.yaml"));

        Assert.Contains("SsalddelExecution__Mode: Operational", source);
        Assert.Contains("VersionFeatureFlags__WarehouseFulfillmentWorkflow: \"true\"", source);
        Assert.Contains("VersionFeatureFlags__SalesChannelFulfillmentWorkflow: \"true\"", source);
        Assert.Contains("VersionFeatureFlags__HrParticipationWorkflow: \"true\"", source);
        Assert.Contains("SalesChannelOrderSync__Enabled: \"true\"", source);
        Assert.Contains("WorkRelationshipSnapshots__Enabled: \"false\"", source);
    }

    private static string WebPage(string fileName)
        => Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.WebApp",
            "Pages",
            fileName);

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
