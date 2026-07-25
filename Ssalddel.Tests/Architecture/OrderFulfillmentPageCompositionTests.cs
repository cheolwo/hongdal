using Ssalddel.Contracts.Common.Sales;

namespace Ssalddel.Tests.Architecture;

public sealed class OrderFulfillmentPageCompositionTests
{
    [Fact]
    public void 판매주문이행_root는_Command탭이아닌_목표별허브다()
    {
        var source = ReadPage("OrderFulfillment.razor");
        var navigation = Read("SsalddelApp", "Components", "Layout", "NavMenu.razor");

        Assert.Contains($"@page \"{OrderFulfillmentSimulationPageRoutes.Root}\"", source);
        Assert.Contains("<OrderFulfillmentRouteFrame", source);
        Assert.Contains("<OrderFulfillmentSummary", source);
        Assert.Contains("Destinations", source);
        Assert.DoesNotContain("<MudTabs", source);
        Assert.DoesNotContain("<OrderFulfillmentPickingPanel", source);
        Assert.DoesNotContain("<OrderFulfillmentPackingPanel", source);
        Assert.DoesNotContain("<OrderFulfillmentRestockPolicyPanel", source);
        Assert.DoesNotContain("ICommerceOrderFulfillmentService", source);
        Assert.Contains("주문 이행 Simulation", navigation);
        Assert.Contains("ShipperRoutes.OrderFulfillment", navigation);
    }

    [Theory]
    [InlineData("OrderFulfillmentSamples.razor", OrderFulfillmentSimulationPageRoutes.Samples, "판매채널 주문 동기화")]
    [InlineData("OrderFulfillmentOrders.razor", OrderFulfillmentSimulationPageRoutes.Orders, "<OrderFulfillmentOrderListPanel")]
    [InlineData("OrderFulfillmentOrderDetail.razor", OrderFulfillmentSimulationPageRoutes.OrderDetailTemplate, "<OrderFulfillmentOrderDetailPanel")]
    [InlineData("OrderFulfillmentInventory.razor", OrderFulfillmentSimulationPageRoutes.Inventory, "<OrderFulfillmentInventoryPanel")]
    [InlineData("OrderFulfillmentPicking.razor", OrderFulfillmentSimulationPageRoutes.Picking, "<OrderFulfillmentPickingListPanel")]
    [InlineData("OrderFulfillmentPickingTask.razor", OrderFulfillmentSimulationPageRoutes.PickingTaskTemplate, "<OrderFulfillmentPickingPanel")]
    [InlineData("OrderFulfillmentPacking.razor", OrderFulfillmentSimulationPageRoutes.Packing, "<OrderFulfillmentPackingListPanel")]
    [InlineData("OrderFulfillmentPackingTask.razor", OrderFulfillmentSimulationPageRoutes.PackingTaskTemplate, "<OrderFulfillmentPackingPanel")]
    [InlineData("OrderFulfillmentRestockPolicy.razor", OrderFulfillmentSimulationPageRoutes.RestockPolicy, "<OrderFulfillmentRestockPolicyPanel")]
    public void 사용자목표는_각각_독립RoutePage를가진다(string fileName, string route, string expectedScreen)
    {
        var source = ReadPage(fileName);

        Assert.Contains($"@page \"{route}\"", source);
        Assert.Contains("<OrderFulfillmentRouteFrame", source);
        Assert.Contains(expectedScreen, source);
        Assert.DoesNotContain("<MudTabs", source);
    }

    [Fact]
    public void 피킹과포장Command는_stableTaskId화면에서만_같은원장을재조회한다()
    {
        var pickingPage = ReadPage("OrderFulfillmentPickingTask.razor.cs");
        var packingPage = ReadPage("OrderFulfillmentPackingTask.razor.cs");
        var pickingPanel = ReadComponent("OrderFulfillmentPickingPanel.razor");
        var packingPanel = ReadComponent("OrderFulfillmentPackingPanel.razor");
        var viewModels = Read(
            "SsalddelApp",
            "ViewModels",
            "Shipper",
            "OrderFulfillmentPageViewModels.cs");

        Assert.Contains("Picking.선택작업Id = TaskId", pickingPage);
        Assert.Contains("OnParametersSetAsync", pickingPage);
        Assert.Contains("ViewModel.피킹스캔Async", pickingPage);
        Assert.Contains("item.Id == TaskId", packingPage);
        Assert.Contains("OnParametersSetAsync", packingPage);
        Assert.Contains("ViewModel.포장시작Async", packingPage);
        Assert.DoesNotContain("<MudSelect", pickingPanel);
        Assert.Contains("SelectedTask", packingPanel);
        Assert.Contains("실행후새로고침Async", viewModels);
        Assert.DoesNotContain("선택작업Id ??=", viewModels);
    }

    [Fact]
    public void Simulation주문목록은_stableKey상세와_필터복귀문맥을사용한다()
    {
        var listPage = ReadPage("OrderFulfillmentOrders.razor.cs");
        var detailPage = ReadPage("OrderFulfillmentOrderDetail.razor.cs");
        var listPanel = ReadComponent("OrderFulfillmentOrderListPanel.razor");

        Assert.Contains("FulfillmentOrderNavigationContext.Parse", listPage);
        Assert.Contains(".DetailPath(order.채널종류, order.채널주문번호)", listPage);
        Assert.Contains("TryDecodeOrderKey", detailPage);
        Assert.Contains("OnParametersSetAsync", detailPage);
        Assert.Contains("Read.주문선택(ChannelType, ChannelOrderNo)", detailPage);
        Assert.Contains("Href=\"@DetailHref(order)\"", listPanel);
        Assert.DoesNotContain("Read.주문선택(order.Key)", listPanel);
    }

    [Fact]
    public void 판매주문이행_조회와_Command는_독립_ViewModel로_분리한다()
    {
        var source = Read(
            "SsalddelApp",
            "ViewModels",
            "Shipper",
            "OrderFulfillmentPageViewModels.cs");

        Assert.Contains("class OrderFulfillmentReadViewModel", source);
        Assert.Contains("class OrderFulfillmentSimulationViewModel", source);
        Assert.Contains("class OrderFulfillmentRestockPolicyViewModel", source);
        Assert.Contains("class OrderFulfillmentPickingViewModel", source);
        Assert.Contains("class OrderFulfillmentPackingViewModel", source);
        Assert.Contains("class OrderFulfillmentPageViewModel", source);
        Assert.Contains("Task.WhenAll", source);
    }

    [Theory]
    [InlineData("OrderFulfillmentRouteFrame.razor")]
    [InlineData("OrderFulfillmentRouteFrame.razor.css")]
    [InlineData("OrderFulfillmentLoadState.razor")]
    [InlineData("OrderFulfillmentSummary.razor")]
    [InlineData("OrderFulfillmentOrderListPanel.razor")]
    [InlineData("OrderFulfillmentOrderDetailPanel.razor")]
    [InlineData("OrderFulfillmentInventoryPanel.razor")]
    [InlineData("OrderFulfillmentPickingListPanel.razor")]
    [InlineData("OrderFulfillmentPickingPanel.razor")]
    [InlineData("OrderFulfillmentPackingListPanel.razor")]
    [InlineData("OrderFulfillmentPackingPanel.razor")]
    [InlineData("OrderFulfillmentRestockPolicyPanel.razor")]
    [InlineData("OrderFulfillmentPresentation.cs")]
    public void 판매주문이행_화면책임은_전용파일로_존재한다(string fileName)
    {
        var path = Path.Combine(FindComponentDirectory(), fileName);

        Assert.True(File.Exists(path), $"판매 주문 이행 전용 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(path));
    }

    [Fact]
    public void Simulation_경계와_개인정보_비노출을_화면에_고정한다()
    {
        var frame = ReadComponent("OrderFulfillmentRouteFrame.razor");
        var picking = ReadComponent("OrderFulfillmentPickingPanel.razor");
        var packing = ReadComponent("OrderFulfillmentPackingPanel.razor");
        var navigationScript = Read("Ssalddel.Ui.Common", "wwwroot", "js", "pageNavigation.js");
        var directory = FindComponentDirectory();
        var visibleComponents = string.Join('\n', Directory.GetFiles(directory, "*.razor").Select(File.ReadAllText));

        Assert.Contains("로컬 메모리 Simulation", frame);
        Assert.Contains("외부 주문 수집", frame);
        Assert.Contains("실제 재고 예약·차감", frame);
        Assert.Contains("HtmlTag=\"h1\"", frame);
        Assert.Contains("scrollToPageTop", frame);
        Assert.Contains("export function scrollToPageTop", navigationScript);
        Assert.Contains("수령인·주소는 피킹 화면에 표시하지 않습니다", picking);
        Assert.Contains("수령인·주소는 포장 화면에 표시하지 않습니다", packing);
        Assert.DoesNotContain("RecipientName", visibleComponents);
        Assert.DoesNotContain("RecipientAddress", visibleComponents);
    }

    [Fact]
    public void 모바일화면은_단일열과_48px주행동을_고정한다()
    {
        var rootCss = ReadPage("OrderFulfillment.razor.css");
        var frameCss = ReadComponent("OrderFulfillmentRouteFrame.razor.css");
        var pickingCss = ReadComponent("OrderFulfillmentPickingPanel.razor.css");
        var packingCss = ReadComponent("OrderFulfillmentPackingPanel.razor.css");

        Assert.Contains("@media (max-width: 720px)", rootCss);
        Assert.Contains("grid-template-columns: minmax(0, 1fr)", rootCss);
        Assert.Contains("@media (max-width: 720px)", frameCss);
        Assert.Contains("min-height: 48px", frameCss);
        Assert.Contains("grid-template-columns: 1fr", pickingCss);
        Assert.Contains("min-height: 48px", pickingCss);
        Assert.Contains("min-height: 48px", packingCss);
    }

    [Fact]
    public void 판매주문이행_PageViewModel은_DI에_명시적으로_등록한다()
    {
        var source = Read("SsalddelApp", "Services", "ShipperSalesModule.cs");

        Assert.Contains("AddTransient<OrderFulfillmentReadViewModel>", source);
        Assert.Contains("AddTransient<OrderFulfillmentSimulationViewModel>", source);
        Assert.Contains("AddTransient<OrderFulfillmentPageViewModel>", source);
    }

    private static string ReadPage(string fileName)
        => Read("SsalddelApp", "Components", "Pages", fileName);

    private static string ReadComponent(string fileName)
        => File.ReadAllText(Path.Combine(FindComponentDirectory(), fileName));

    private static string Read(params string[] segments)
        => File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

    private static string FindComponentDirectory()
        => Path.Combine(
            FindRepositoryRoot(),
            "SsalddelApp",
            "Components",
            "Pages",
            "OrderFulfillmentComponents");

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
