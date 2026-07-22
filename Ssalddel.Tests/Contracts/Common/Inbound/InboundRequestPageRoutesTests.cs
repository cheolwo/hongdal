using Ssalddel.Contracts.Common.Inbound;

namespace Ssalddel.Tests.Contracts.Common.Inbound;

public sealed class InboundRequestPageRoutesTests
{
    [Fact]
    public void StableIdRoute를_화면별로생성한다()
    {
        Assert.Equal("/shipper/inbound/requests/42", InboundRequestPageRoutes.DetailFor(42));
        Assert.Equal("/shipper/inbound/requests/42/complete", InboundRequestPageRoutes.CompleteFor(42));
        Assert.Throws<ArgumentOutOfRangeException>(() => InboundRequestPageRoutes.DetailFor(0));
    }

    [Fact]
    public void 다이어그램초안은_신규신청Route에서왕복한다()
    {
        var context = new InboundRequestNavigationContext
        {
            From = "/diagram?node=inbound",
            Source = "diagram-warehouse-proxy",
            WarehouseId = 9,
            WarehouseName = "도심 생활물류센터",
            SupplierCode = "SUP-9",
            SupplierName = "공동주문 공급처",
            OrderReference = "ORDER-9",
            ExpectedArrivalDate = new DateTime(2026, 7, 23),
            ContractType = 입고계약유형코드.마켓풀필먼트,
            ContractCommissionRate = 7.5m,
            NodeTitle = "공동주문 입고"
        };

        var path = context.PathFor(InboundRequestScreenKind.Create);
        var restored = InboundRequestNavigationContext.Parse(path);

        Assert.StartsWith("/shipper/inbound/requests/new?", path, StringComparison.Ordinal);
        Assert.Equal(context.From, restored.From);
        Assert.Equal(context.WarehouseId, restored.WarehouseId);
        Assert.Equal(context.SupplierName, restored.SupplierName);
        Assert.Equal(context.ExpectedArrivalDate, restored.ExpectedArrivalDate);
        Assert.Equal(context.ContractCommissionRate, restored.ContractCommissionRate);
        Assert.Equal(context.NodeTitle, restored.NodeTitle);
    }

    [Fact]
    public void 외부복귀경로는_버리고안전한LocalPath만보존한다()
    {
        var unsafeContext = InboundRequestNavigationContext.Parse(
            "/shipper/inbound/requests/new?from=https%3A%2F%2Fevil.example%2Fsteal");
        var safeContext = InboundRequestNavigationContext.Parse(
            "/shipper/inbound/requests/new?from=%2Fdiagram%3Fnode%3Dinbound");

        Assert.Null(unsafeContext.From);
        Assert.Equal("/shipper", unsafeContext.ResolveReturnPath("/shipper"));
        Assert.Equal("/diagram?node=inbound", safeContext.ResolveReturnPath("/shipper"));
    }

    [Fact]
    public void Created표시는_상세Route에만추가한다()
    {
        var context = new InboundRequestNavigationContext { Created = true };

        Assert.Equal("/shipper/inbound/requests/17?created=true", context.PathFor(InboundRequestScreenKind.Detail, 17));
        Assert.Equal("/shipper/inbound/requests/17/complete", context.PathFor(InboundRequestScreenKind.Complete, 17));
    }
}
