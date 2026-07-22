using Ssalddel.Contracts.Common.Sales;

namespace Ssalddel.Tests.Contracts.Common.Sales;

public sealed class SalesOrderPageRoutesTests
{
    [Fact]
    public void StableIdDetailRoute_RequiresPositiveOrderId()
    {
        Assert.Equal("/shipper/sales/orders/73", SalesOrderPageRoutes.DetailFor(73));
        Assert.Throws<ArgumentOutOfRangeException>(() => SalesOrderPageRoutes.DetailFor(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SalesOrderPageRoutes.DetailFor(-1));
    }

    [Fact]
    public void Context_RoundTripsListStateAndSafeReturnPath()
    {
        var listContext = new SalesOrderNavigationContext()
            .WithListState(" 냉장 세트 ", CommerceChannelOrderSyncScopes.Domestic, "출고예정", 3);
        var listPath = listContext.PathFor(SalesOrderScreenKind.List);
        var detailPath = new SalesOrderNavigationContext { From = listPath }
            .PathFor(SalesOrderScreenKind.Detail, 73);
        var parsedList = SalesOrderNavigationContext.Parse(listPath);
        var parsedDetail = SalesOrderNavigationContext.Parse(detailPath);

        Assert.Contains("q=", listPath, StringComparison.Ordinal);
        Assert.Equal("냉장 세트", parsedList.Search);
        Assert.Equal(CommerceChannelOrderSyncScopes.Domestic, parsedList.SyncScope);
        Assert.Equal("출고예정", parsedList.Status);
        Assert.Equal(3, parsedList.Page);
        Assert.Equal(listPath, parsedDetail.ResolveReturnPath());
    }

    [Theory]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    [InlineData("/safe\\redirect")]
    [InlineData("/%2f%2fevil.example")]
    public void Context_DropsUnsafeReturnPath(string from)
    {
        var parsed = SalesOrderNavigationContext.Parse(
            $"/shipper/sales/orders/73?from={Uri.EscapeDataString(from)}");

        Assert.Null(parsed.From);
        Assert.Equal(SalesOrderPageRoutes.Root, parsed.ResolveReturnPath());
    }

    [Fact]
    public void Context_NormalizesInvalidPageAndSupportsLegacyQueryNames()
    {
        var parsed = SalesOrderNavigationContext.Parse(
            "/shipper/sales/orders?search=%EB%83%89%EC%9E%A5&syncScope=Domestic&page=-2");

        Assert.Equal("냉장", parsed.Search);
        Assert.Equal("Domestic", parsed.SyncScope);
        Assert.Equal(1, parsed.Page);
        Assert.Equal(SalesOrderPageRoutes.Root, new SalesOrderNavigationContext().PathFor(SalesOrderScreenKind.List));
    }

    [Fact]
    public void FulfillmentRoutes_UseStableTaskIdsAndRejectInvalidIds()
    {
        Assert.Equal(
            "/shipper/sales/fulfillment/picking/17",
            OrderFulfillmentSimulationPageRoutes.PickingTaskFor(17));
        Assert.Equal(
            "/shipper/sales/fulfillment/packing/23",
            OrderFulfillmentSimulationPageRoutes.PackingTaskFor(23));
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderFulfillmentSimulationPageRoutes.PickingTaskFor(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderFulfillmentSimulationPageRoutes.PackingTaskFor(-1));
    }

    [Fact]
    public void FulfillmentOrderRoute_RoundTripsOpaqueUnicodeKey()
    {
        var route = OrderFulfillmentSimulationPageRoutes.OrderDetailFor(
            "Smart/Store",
            "주문 2026-07/22+A");
        var orderKey = route[(route.LastIndexOf('/') + 1)..];

        var decoded = OrderFulfillmentSimulationPageRoutes.TryDecodeOrderKey(
            orderKey,
            out var channelType,
            out var channelOrderNo);

        Assert.True(decoded);
        Assert.Equal("Smart/Store", channelType);
        Assert.Equal("주문 2026-07/22+A", channelOrderNo);
        Assert.DoesNotContain("Smart/Store", route, StringComparison.Ordinal);
        Assert.DoesNotContain("주문", route, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-base64!")]
    [InlineData("YQ")]
    public void FulfillmentOrderRoute_DoesNotGuessInvalidKeys(string? orderKey)
    {
        var decoded = OrderFulfillmentSimulationPageRoutes.TryDecodeOrderKey(
            orderKey,
            out var channelType,
            out var channelOrderNo);

        Assert.False(decoded);
        Assert.Empty(channelType);
        Assert.Empty(channelOrderNo);
    }

    [Fact]
    public void FulfillmentOrderContext_PreservesFiltersInSafeReturnPath()
    {
        var context = new FulfillmentOrderNavigationContext()
            .WithListState(" 냉장 세트 ", "국내", "출고대기");
        var listPath = context.ListPath();
        var detailPath = context.DetailPath(CommerceChannelKeys.SmartStore, "ORDER-73");
        var parsedList = FulfillmentOrderNavigationContext.Parse(listPath);
        var parsedDetail = FulfillmentOrderNavigationContext.Parse(detailPath);

        Assert.Equal("냉장 세트", parsedList.Search);
        Assert.Equal("국내", parsedList.Scope);
        Assert.Equal("출고대기", parsedList.Status);
        Assert.Equal(listPath, parsedDetail.ResolveReturnPath());
    }
}
