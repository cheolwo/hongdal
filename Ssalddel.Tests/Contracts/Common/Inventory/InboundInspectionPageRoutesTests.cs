using Ssalddel.Contracts.Common.Inventory;

namespace Ssalddel.Tests.Contracts.Common.Inventory;

public sealed class InboundInspectionPageRoutesTests
{
    [Fact]
    public void StableIdRoutes_RequirePositiveInboundItemId()
    {
        Assert.Equal("/work/inbound/inspection/71", InboundInspectionPageRoutes.DetailFor(71));
        Assert.Equal("/work/inbound/inspection/71/record", InboundInspectionPageRoutes.RecordFor(71));
        Assert.Throws<ArgumentOutOfRangeException>(() => InboundInspectionPageRoutes.DetailFor(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => InboundInspectionPageRoutes.RecordFor(-1));
    }

    [Fact]
    public void Context_RoundTripsListStateAcrossDetailAndRecord()
    {
        var context = new InboundInspectionNavigationContext
        {
            From = "/diagram?node=inbound-71",
            Search = "감자 SKU",
            Status = 입고검수조회상태코드.완료,
            Page = 2
        };

        var detail = context.PathFor(InboundInspectionScreenKind.Detail, 71);
        var parsed = InboundInspectionNavigationContext.Parse(detail);

        Assert.StartsWith("/work/inbound/inspection/71?", detail, StringComparison.Ordinal);
        Assert.Equal(context.From, parsed.From);
        Assert.Equal(context.Search, parsed.Search);
        Assert.Equal(context.Status, parsed.Status);
        Assert.Equal(2, parsed.Page);
        Assert.StartsWith("/work/inbound/inspection/71/record?", parsed.PathFor(InboundInspectionScreenKind.Record, 71), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    [InlineData("/safe\\redirect")]
    [InlineData("/%2f%2fevil.example")]
    public void Context_DropsUnsafeReturnPath(string from)
    {
        var parsed = InboundInspectionNavigationContext.Parse(
            $"/work/inbound/inspection?from={Uri.EscapeDataString(from)}");

        Assert.Null(parsed.From);
        Assert.Equal("/work-board", parsed.ResolveReturnPath("/work-board"));
    }

    [Fact]
    public void Context_NormalizesInvalidFilterValues()
    {
        var parsed = InboundInspectionNavigationContext.Parse(
            "/work/inbound/inspection?status=unknown&page=-3&search=%E0%A4%A");

        Assert.Equal(입고검수조회상태코드.대기, parsed.Status);
        Assert.Equal(0, parsed.Page);
        Assert.NotNull(parsed.Search);
        Assert.Equal(InboundInspectionPageRoutes.Root, new InboundInspectionNavigationContext().PathFor(InboundInspectionScreenKind.List));
    }
}
