using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Tests.Architecture;

public sealed class FoodDeliveryV30PageCompositionTests
{
    [Fact]
    public void 음식점운영홈은_주문수락구현을포함하지않는다()
    {
        var source = Read("RestaurantDeskApp", "Components/Pages/Home.razor");

        Assert.Contains("@page \"/\"", source);
        Assert.Contains("/orders", source);
        Assert.DoesNotContain("주문수락후전표준비Async", source);
        Assert.DoesNotContain("SimulateOrderAlertAsync", source);
    }

    [Fact]
    public void 음식점주문은_수신함과정확한주문번호상세로분리한다()
    {
        var inbox = Read("RestaurantDeskApp", "Components/Pages/OrderInbox.razor");
        var detail = Read("RestaurantDeskApp", "Components/Pages/OrderDetail.razor");

        Assert.Contains("@page \"/orders\"", inbox);
        Assert.Contains("상세 보기", inbox);
        Assert.DoesNotContain("주문수락후전표준비Async", inbox);
        Assert.Contains("@page \"/orders/{OrderNo}\"", detail);
        Assert.Contains("주문수락후전표준비Async", detail);
    }

    [Fact]
    public void 음식점Api실패는_샘플주문으로대체하지않는다()
    {
        var client = Read("RestaurantDeskApp", "Services/Ssalddel음식주문Client.cs");
        var desk = Read("RestaurantDeskApp", "Services/음식점주문DeskService.cs");

        Assert.DoesNotContain("RestaurantDeskSampleService", client);
        Assert.DoesNotContain("sampleService", desk);
        Assert.Contains("/restaurant-acceptance", client);
        Assert.Contains("PostAsJsonAsync", client);
    }

    [Fact]
    public void 음식배달3_0미리보기는_Operational에서3_5를켜지않는다()
    {
        var compose = Read("deploy", "azure-vm/compose.food-delivery-v30.override.yaml");

        Assert.Contains("SsalddelExecution__Mode: Operational", compose);
        Assert.Contains("VersionFeatureFlags__FoodDeliveryWorkflow: \"true\"", compose);
        Assert.Contains("VersionFeatureFlags__SsalddelMartWorkflow: \"false\"", compose);
    }

    [Fact]
    public void 음식배달기사반복항목Command는_기사업무ViewModel을명시한다()
    {
        var page = Read("FDriverApp", "Pages/MainPage.xaml");

        Assert.DoesNotContain("BindingContext.AcceptBundleCommand", page);
        Assert.DoesNotContain("BindingContext.SelectTicketCommand", page);
        Assert.DoesNotContain("BindingContext.AcceptTicketCommand", page);
        Assert.Contains("x:DataType='pageModels:MainPageModel'", page);
        Assert.Contains("AncestorType={x:Type pageModels:MainPageModel}", page);
    }

    [Theory]
    [InlineData(SsalddelPageAppCodes.Orderer, "/food", "orderer-food-home", PageInteractionBoundary.ReadOnly)]
    [InlineData(SsalddelPageAppCodes.RestaurantDesk, "/orders", "restaurant-order-inbox", PageInteractionBoundary.ReadOnly)]
    [InlineData(SsalddelPageAppCodes.RestaurantDesk, "/orders/FOOD-20260723-01", "restaurant-order-detail", PageInteractionBoundary.PlatformPersistence)]
    [InlineData(SsalddelPageAppCodes.FoodDeliveryDriver, "/food-delivery/open/dispatch", "food-driver-workspace-launch", PageInteractionBoundary.PlatformPersistence)]
    public void 음식배달3_0페이지는_책임별Capability를가진다(
        string appCode,
        string route,
        string expectedPageKey,
        PageInteractionBoundary expectedBoundary)
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(appCode, route, out var capability);

        Assert.True(found);
        Assert.Equal(expectedPageKey, capability.PageKey);
        Assert.Equal("3.0", capability.IntroducedVersion);
        Assert.Equal(expectedBoundary, capability.Boundary);
        Assert.Contains("FoodDeliveryWorkflow", capability.FeatureKeys);
        Assert.Contains("FoodDelivery", capability.WorkflowCodes);
    }

    private static string Read(string project, string relativePath)
        => File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Ssalddel.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("저장소 루트를 찾을 수 없습니다.");
    }
}
