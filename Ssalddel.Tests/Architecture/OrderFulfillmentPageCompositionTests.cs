namespace Ssalddel.Tests.Architecture;

public sealed class OrderFulfillmentPageCompositionTests
{
    [Fact]
    public void 판매주문이행_라우트는_상태영역과_workflow만_조립한다()
    {
        var pagePath = Path.Combine(FindRepositoryRoot(), "SsalddelApp", "Components", "Pages", "OrderFulfillment.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 80);
        Assert.Contains("<OrderFulfillmentHeader", source);
        Assert.Contains("<OrderFulfillmentLoadState", source);
        Assert.Contains("<OrderFulfillmentSummary", source);
        Assert.Contains("<OrderFulfillmentOrderListPanel", source);
        Assert.Contains("<OrderFulfillmentOrderDetailPanel", source);
        Assert.Contains("<OrderFulfillmentInventoryPanel", source);
        Assert.Contains("<OrderFulfillmentPickingPanel", source);
        Assert.Contains("<OrderFulfillmentPackingPanel", source);
        Assert.Contains("<OrderFulfillmentRestockPolicyPanel", source);
        Assert.DoesNotContain("<MudTable", source);
        Assert.DoesNotContain("ICommerceOrderFulfillmentService", source);
        Assert.DoesNotContain("ICommerceOrderSampleFeedService", source);
        Assert.DoesNotContain("@code", source);
    }

    [Fact]
    public void 판매주문이행_조회와_Command는_독립_ViewModel로_분리한다()
    {
        var path = Path.Combine(FindRepositoryRoot(), "SsalddelApp", "ViewModels", "Shipper", "OrderFulfillmentPageViewModels.cs");
        var source = File.ReadAllText(path);

        Assert.Contains("class OrderFulfillmentReadViewModel", source);
        Assert.Contains("class OrderFulfillmentSimulationViewModel", source);
        Assert.Contains("class OrderFulfillmentRestockPolicyViewModel", source);
        Assert.Contains("class OrderFulfillmentPickingViewModel", source);
        Assert.Contains("class OrderFulfillmentPackingViewModel", source);
        Assert.Contains("class OrderFulfillmentPageViewModel", source);
        Assert.Contains("Task.WhenAll", source);
        Assert.DoesNotContain("선택작업Id ??=", source);
    }

    [Theory]
    [InlineData("OrderFulfillmentHeader.razor")]
    [InlineData("OrderFulfillmentHeader.razor.css")]
    [InlineData("OrderFulfillmentLoadState.razor")]
    [InlineData("OrderFulfillmentLoadState.razor.css")]
    [InlineData("OrderFulfillmentSummary.razor")]
    [InlineData("OrderFulfillmentSummary.razor.css")]
    [InlineData("OrderFulfillmentOrderListPanel.razor")]
    [InlineData("OrderFulfillmentOrderListPanel.razor.css")]
    [InlineData("OrderFulfillmentOrderDetailPanel.razor")]
    [InlineData("OrderFulfillmentOrderDetailPanel.razor.css")]
    [InlineData("OrderFulfillmentInventoryPanel.razor")]
    [InlineData("OrderFulfillmentInventoryPanel.razor.css")]
    [InlineData("OrderFulfillmentPickingPanel.razor")]
    [InlineData("OrderFulfillmentPickingPanel.razor.css")]
    [InlineData("OrderFulfillmentPackingPanel.razor")]
    [InlineData("OrderFulfillmentPackingPanel.razor.css")]
    [InlineData("OrderFulfillmentRestockPolicyPanel.razor")]
    [InlineData("OrderFulfillmentRestockPolicyPanel.razor.css")]
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
        var directory = FindComponentDirectory();
        var header = File.ReadAllText(Path.Combine(directory, "OrderFulfillmentHeader.razor"));
        var picking = File.ReadAllText(Path.Combine(directory, "OrderFulfillmentPickingPanel.razor"));
        var packing = File.ReadAllText(Path.Combine(directory, "OrderFulfillmentPackingPanel.razor"));
        var visibleComponents = string.Join('\n', Directory.GetFiles(directory, "*.razor").Select(File.ReadAllText));

        Assert.Contains("로컬 메모리 Simulation", header);
        Assert.Contains("외부 주문 수집", header);
        Assert.Contains("실제 재고 예약·차감", header);
        Assert.Contains("수령인·주소는 피킹 화면에 표시하지 않습니다", picking);
        Assert.Contains("수령인·주소는 포장 목록에 표시하지 않습니다", packing);
        Assert.DoesNotContain("RecipientName", visibleComponents);
        Assert.DoesNotContain("RecipientAddress", visibleComponents);
    }

    [Fact]
    public void 판매주문이행_화면은_좁은폭에서_단일열로_전환한다()
    {
        var pages = Path.Combine(FindRepositoryRoot(), "SsalddelApp", "Components", "Pages");
        var rootCss = File.ReadAllText(Path.Combine(pages, "OrderFulfillment.razor.css"));
        var listCss = File.ReadAllText(Path.Combine(FindComponentDirectory(), "OrderFulfillmentOrderListPanel.razor.css"));
        var pickingCss = File.ReadAllText(Path.Combine(FindComponentDirectory(), "OrderFulfillmentPickingPanel.razor.css"));

        Assert.Contains("grid-template-columns: minmax(0, 1fr)", rootCss);
        Assert.Contains(".order-fulfillment-shell > *", rootCss);
        Assert.Contains("@media (max-width: 720px)", rootCss);
        Assert.Contains("grid-template-columns: 1fr", rootCss);
        Assert.Contains("@media (max-width: 720px)", listCss);
        Assert.Contains("grid-template-columns: 1fr", listCss);
        Assert.Contains("@media (max-width: 720px)", pickingCss);
        Assert.Contains("grid-template-columns: 1fr", pickingCss);
    }

    [Fact]
    public void 판매주문이행_PageViewModel은_DI에_명시적으로_등록한다()
    {
        var modulePath = Path.Combine(FindRepositoryRoot(), "SsalddelApp", "Services", "ShipperSalesModule.cs");
        var source = File.ReadAllText(modulePath);

        Assert.Contains("AddTransient<OrderFulfillmentReadViewModel>", source);
        Assert.Contains("AddTransient<OrderFulfillmentSimulationViewModel>", source);
        Assert.Contains("AddTransient<OrderFulfillmentPageViewModel>", source);
    }

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
