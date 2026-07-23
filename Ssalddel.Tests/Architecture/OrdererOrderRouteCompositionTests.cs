namespace Ssalddel.Tests.Architecture;

public sealed class OrdererOrderRouteCompositionTests
{
    [Fact]
    public void 주문목록route는_개별주문목록만_조립하고_음식주문을_별도route로_보존한다()
    {
        var orderHistory = ReadOrdererPage("OrderHistory.razor");
        var historyScreen = ReadOrdererOrderComponent("OrdererIndividualOrderLedgerHistoryScreen.razor");
        var foodHistory = ReadOrdererPage("FoodOrderHistory.razor");

        Assert.Contains("@page \"/orders\"", orderHistory);
        Assert.Contains("<OrdererIndividualOrderLedgerHistoryScreen", orderHistory);
        Assert.DoesNotContain("<OrdererIndividualOrderLedgerList", orderHistory);
        Assert.DoesNotContain("<OrdererFoodOrderWorkspace", orderHistory);
        Assert.Contains("<OrdererIndividualOrderLedgerList", historyScreen);
        Assert.Contains("<OrdererOrderAccessGate", historyScreen);
        Assert.Contains("Href=\"/orders/food\"", historyScreen);

        Assert.Contains("@page \"/orders/food\"", foodHistory);
        Assert.Contains("<OrdererFoodOrderWorkspace", foodHistory);
        Assert.Contains("/orders/food?orderNo=", foodHistory);
    }

    [Fact]
    public void 개별주문상세route는_안정원장Id로_보호형상세한건만_조회한다()
    {
        var detail = ReadOrdererPage("OrderDetail.razor");
        var detailScreen = ReadOrdererOrderComponent("OrdererIndividualOrderLedgerDetailScreen.razor");
        var detailPanel = ReadOrdererOrderComponent("OrdererIndividualOrderLedgerDetail.razor");

        Assert.Contains("@page \"/orders/{OrderLedgerId}\"", detail);
        Assert.Contains("<OrdererIndividualOrderLedgerDetailScreen", detail);
        Assert.DoesNotContain("@inject 주문조회ViewModel", detail);
        Assert.DoesNotContain("OrderLedger.조회Async", detail);

        Assert.Contains("<OrdererOrderAccessGate", detailScreen);
        Assert.Contains("<OrdererIndividualOrderLedgerDetail", detailScreen);
        Assert.Contains("OrderLedgerId=\"@OrderLedgerId\"", detailScreen);

        Assert.Contains("@inject 주문조회ViewModel OrderLedger", detailPanel);
        Assert.Contains("주문원장보기코드.주문자보호", detailPanel);
        Assert.Contains("OrderLedger.조회Async", detailPanel);
        Assert.DoesNotContain("주문원장역할조회Async", detailPanel);
        Assert.DoesNotContain("OrdererFoodOrderWorkspace", detail);
    }

    [Fact]
    public void 개별주문목록은_주문자관점Api와_안정원장Id선택만_담당한다()
    {
        var component = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "OrdererApp",
            "Components",
            "Orders",
            "OrdererIndividualOrderLedgerList.razor"));

        Assert.Contains("@inject 주문자개별주문ViewModel Orders", component);
        Assert.Contains("Orders.조회Async", component);
        Assert.Contains("목록정렬방향.내림차순", component);
        Assert.Contains("OrderSelected.InvokeAsync(orderLedgerId)", component);
        Assert.DoesNotContain("주문원장보호조회Async", component);
        Assert.DoesNotContain("OrdererFoodOrderWorkspace", component);
    }

    [Fact]
    public void 주문화면인증은_재사용gate가_복원과로그인을_단독담당한다()
    {
        var gate = ReadOrdererOrderComponent("OrdererOrderAccessGate.razor");
        var orderHistory = ReadOrdererPage("OrderHistory.razor");
        var orderDetail = ReadOrdererPage("OrderDetail.razor");

        Assert.Contains("@inject 주문자앱인증ViewModel Authentication", gate);
        Assert.Contains("Authentication.복원Async()", gate);
        Assert.Contains("<Ssalddel공통로그인Panel", gate);
        Assert.Contains("@Authorized", gate);

        Assert.DoesNotContain("@inject 주문자앱인증ViewModel", orderHistory);
        Assert.DoesNotContain("@inject 주문자앱인증ViewModel", orderDetail);
        Assert.DoesNotContain("<Ssalddel공통로그인Panel", orderHistory);
        Assert.DoesNotContain("<Ssalddel공통로그인Panel", orderDetail);
    }

    private static string ReadOrdererPage(string fileName)
        => File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "OrdererApp",
            "Components",
            "Pages",
            fileName));

    private static string ReadOrdererOrderComponent(string fileName)
        => File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "OrdererApp",
            "Components",
            "Orders",
            fileName));

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
