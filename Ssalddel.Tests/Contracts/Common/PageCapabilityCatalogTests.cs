using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Tests.Contracts.Common;

public sealed class PageCapabilityCatalogTests
{
    [Fact]
    public void 페이지_키와_앱별_라우트_규칙은_중복되지_않는다()
    {
        var capabilities = SsalddelPageCapabilityCatalog.GetAll();

        Assert.NotEmpty(capabilities);
        Assert.DoesNotContain(
            capabilities.GroupBy(item => item.PageKey, StringComparer.Ordinal),
            group => group.Count() > 1);
        Assert.DoesNotContain(
            capabilities.GroupBy(
                item => $"{item.AppCode}|{item.MatchKind}|{item.RoutePattern}",
                StringComparer.OrdinalIgnoreCase),
            group => group.Count() > 1);
    }

    [Fact]
    public void 아홉_업무_흐름은_각각_대표_page_capability를_가진다()
    {
        var workflowCodes = SsalddelPageCapabilityCatalog.GetAll()
            .SelectMany(item => item.WorkflowCodes)
            .ToHashSet(StringComparer.Ordinal);
        var expected = new[]
        {
            "DomesticTransport",
            "WarehouseFulfillment",
            "CustomsAndTradeData",
            "GroupPurchaseImport",
            "SalesChannelFulfillment",
            "CommunityTrust",
            "HrParticipation",
            "FoodDelivery",
            "SsalddelMart"
        };

        Assert.All(expected, workflow => Assert.Contains(workflow, workflowCodes));
    }

    [Fact]
    public void 외부_효과가_있는_페이지는_운영_단계로_분류하지_않는다()
    {
        var unsafeLivePages = SsalddelPageCapabilityCatalog.GetAll()
            .Where(item => item.HasExternalEffects && item.Stage == PageCapabilityStage.Live)
            .Select(item => item.PageKey)
            .ToArray();

        Assert.Empty(unsafeLivePages);
    }

    [Fact]
    public void 창고입고예정페이지는_인증된서버조회Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.Warehouse,
            "/warehouse/inbounds/expected",
            out var capability);

        Assert.True(found);
        Assert.Equal("warehouse-app-expected-inbounds", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Contains("WarehouseFulfillmentWorkflow", capability.FeatureKeys);
    }

    [Fact]
    public void 창고작업보드는_인증된같은Id서버조회Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.Warehouse,
            "/work-board?inboundId=29",
            out var capability);

        Assert.True(found);
        Assert.Equal("warehouse-app-work-board", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Contains("WarehouseFulfillmentWorkflow", capability.FeatureKeys);
    }

    [Fact]
    public void 마트피킹페이지는_인증된영속작업조회Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.Warehouse,
            "/mart/picking?orderId=73",
            out var capability);

        Assert.True(found);
        Assert.Equal("warehouse-app-mart-picking", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("3.5", capability.IntroducedVersion);
        Assert.Contains("SsalddelMartWorkflow", capability.FeatureKeys);
        Assert.Contains("SsalddelMart", capability.WorkflowCodes);
    }

    [Theory]
    [InlineData("/warehouse/mart/picking?orderId=73", "web-warehouse-mart-picking")]
    [InlineData("/mart/picking?orderId=73", "web-mart-picking-alias")]
    public void 통합웹마트피킹경로도_같은읽기전용경계를사용한다(string route, string pageKey)
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            route,
            out var capability);

        Assert.True(found);
        Assert.Equal(pageKey, capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Contains("SsalddelMartWorkflow", capability.FeatureKeys);
    }

    [Fact]
    public void 공동구매공개페이지는_익명조회와로그인후저장을분리한Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            "/community/group-purchase?campaignId=ad1bcf9c-7e02-4dc4-a17a-96f9d4818f15",
            out var capability);

        Assert.True(found);
        Assert.Equal("community-group-purchase-public", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.Simulation, capability.Boundary);
        Assert.False(capability.RequiresAuthentication);
        Assert.True(capability.HasExternalEffects);
        Assert.Equal("0.0", capability.IntroducedVersion);
        Assert.Contains("CommunityTrustWorkflow", capability.FeatureKeys);
        Assert.Contains("CommunityTrust", capability.WorkflowCodes);
    }

    [Fact]
    public void 화주HS코드검토페이지는_인증된읽기전용Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            "/shipper/customs/hs-reviews?reviewId=42",
            out var capability);

        Assert.True(found);
        Assert.Equal("shipper-customs-hs-reviews", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("2.0", capability.IntroducedVersion);
        Assert.Contains("CustomsAndTradeDataWorkflow", capability.FeatureKeys);
        Assert.Contains("CustomsAndTradeData", capability.WorkflowCodes);
    }

    [Fact]
    public void 판매채널연결페이지는_사용자소유원장을저장하는Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            "/shipper/sales/channels?accountId=17",
            out var capability);

        Assert.True(found);
        Assert.Equal("shipper-sales-channels", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.PlatformPersistence, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("2.5", capability.IntroducedVersion);
        Assert.Contains("SalesChannelFulfillmentWorkflow", capability.FeatureKeys);
        Assert.Contains("SalesChannelFulfillment", capability.WorkflowCodes);
    }

    [Fact]
    public void 판매채널주문페이지는_영속출고후보를읽는Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            "/shipper/sales/orders?orderId=73",
            out var capability);

        Assert.True(found);
        Assert.Equal("shipper-sales-orders", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("2.5", capability.IntroducedVersion);
        Assert.Contains("SalesChannelFulfillmentWorkflow", capability.FeatureKeys);
        Assert.Contains("SalesChannelFulfillment", capability.WorkflowCodes);
    }

    [Fact]
    public void 음식점메뉴페이지는_익명공개조회Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.Orderer,
            "/food/restaurants?restaurantId=31",
            out var capability);

        Assert.True(found);
        Assert.Equal("orderer-food-restaurants", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.False(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("3.0", capability.IntroducedVersion);
        Assert.Contains("FoodDeliveryWorkflow", capability.FeatureKeys);
        Assert.Contains("FoodDelivery", capability.WorkflowCodes);
    }

    [Fact]
    public void 음식주문내역페이지는_인증된소유원장읽기Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.Orderer,
            "/orders?orderNo=FOOD-20260720-01",
            out var capability);

        Assert.True(found);
        Assert.Equal("orderer-food-orders", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("3.0", capability.IntroducedVersion);
        Assert.Contains("FoodDeliveryWorkflow", capability.FeatureKeys);
        Assert.Contains("FoodDelivery", capability.WorkflowCodes);
    }

    [Fact]
    public void 마트상품페이지는_익명공개투영조회Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.Orderer,
            "/food/mart?productId=41",
            out var capability);

        Assert.True(found);
        Assert.Equal("orderer-mart", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.False(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("3.5", capability.IntroducedVersion);
        Assert.Contains("SsalddelMartWorkflow", capability.FeatureKeys);
        Assert.Contains("SsalddelMart", capability.WorkflowCodes);
    }

    [Theory]
    [InlineData(SsalddelPageAppCodes.Orderer, "/food/mart/order?productId=41", "orderer-mart-order-request")]
    [InlineData(SsalddelPageAppCodes.IntegratedWeb, "/orderer/mart/order?requestId=116e0c45-8acd-4d24-b468-f522a127bbac", "web-orderer-mart-order-request")]
    public void 마트주문요청페이지는_인증된비구속플랫폼저장Beta로분류한다(
        string appCode,
        string route,
        string expectedPageKey)
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(appCode, route, out var capability);

        Assert.True(found);
        Assert.Equal(expectedPageKey, capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.PlatformPersistence, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("3.5", capability.IntroducedVersion);
        Assert.Contains("SsalddelMartWorkflow", capability.FeatureKeys);
        Assert.Contains("SsalddelMart", capability.WorkflowCodes);
    }

    [Fact]
    public void 통합웹마트상품페이지도_익명공개조회Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            "/orderer/mart?productId=41",
            out var capability);

        Assert.True(found);
        Assert.Equal("web-orderer-mart", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.False(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Contains("SsalddelMartWorkflow", capability.FeatureKeys);
    }

    [Fact]
    public void 인사역할검토홈은_인증된영속배정원장조회Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.HumanResources,
            "/?reviewId=2f4050b7-4784-44f7-a704-d895c4f2cc13",
            out var capability);

        Assert.True(found);
        Assert.Equal("human-resources-role-reviews", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("2.5", capability.IntroducedVersion);
        Assert.Contains("HrParticipationWorkflow", capability.FeatureKeys);
        Assert.Contains("HrParticipation", capability.WorkflowCodes);
    }

    [Fact]
    public void 커뮤니티역할지원은_인증된철회가능플랫폼저장Beta로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.IntegratedWeb,
            "/community/roles/apply",
            out var capability);

        Assert.True(found);
        Assert.Equal("community-role-application", capability.PageKey);
        Assert.Equal(PageCapabilityStage.Beta, capability.Stage);
        Assert.Equal(PageInteractionBoundary.PlatformPersistence, capability.Boundary);
        Assert.True(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
        Assert.Equal("2.5", capability.IntroducedVersion);
        Assert.Contains("HrParticipationWorkflow", capability.FeatureKeys);
        Assert.Contains("HrParticipation", capability.WorkflowCodes);
    }
}
