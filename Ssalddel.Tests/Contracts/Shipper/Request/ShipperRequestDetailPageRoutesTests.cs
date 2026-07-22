using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Tests.Contracts.Shipper.Request;

public sealed class ShipperRequestDetailPageRoutesTests
{
    [Theory]
    [InlineData(ShipperRequestDetailScreenKind.Summary, "/shipper/request/REQ%2F2026%2001")]
    [InlineData(ShipperRequestDetailScreenKind.Timeline, "/shipper/request/REQ%2F2026%2001/timeline")]
    [InlineData(ShipperRequestDetailScreenKind.Payment, "/shipper/request/REQ%2F2026%2001/payment")]
    [InlineData(ShipperRequestDetailScreenKind.Proofs, "/shipper/request/REQ%2F2026%2001/proofs")]
    public void 상세Screen은_같은의뢰Id를_안전한StableRoute로사용한다(
        ShipperRequestDetailScreenKind screen,
        string expected)
    {
        Assert.Equal(expected, ShipperRequestDetailPageRoutes.PathFor(screen, "REQ/2026 01"));
    }

    [Fact]
    public void 상세Screen사이를이동해도_안전한복귀문맥을보존한다()
    {
        var context = new ShipperRequestDetailNavigationContext
        {
            ReturnPath = "/diagram?ledgerTemplate=cargo-transport&node=transport-request",
            Created = true
        };

        var summary = context.PathFor(ShipperRequestDetailScreenKind.Summary, "HD-WEB-001");
        var proofs = context.PathFor(ShipperRequestDetailScreenKind.Proofs, "HD-WEB-001");

        Assert.Equal(
            "/shipper/request/HD-WEB-001?created=true&from=%2Fdiagram%3FledgerTemplate%3Dcargo-transport%26node%3Dtransport-request",
            summary);
        Assert.Equal(
            "/shipper/request/HD-WEB-001/proofs?from=%2Fdiagram%3FledgerTemplate%3Dcargo-transport%26node%3Dtransport-request",
            proofs);
        Assert.Equal(context.ReturnPath, context.ResolveReturnPath("/shipper"));
    }

    [Theory]
    [InlineData("https://evil.example/diagram")]
    [InlineData("//evil.example/diagram")]
    [InlineData("javascript:alert(1)")]
    public void 외부복귀경로는_상세링크에전달하지않는다(string returnPath)
    {
        var context = new ShipperRequestDetailNavigationContext { ReturnPath = returnPath };

        Assert.Equal(
            "/shipper/request/HD-1/payment",
            context.PathFor(ShipperRequestDetailScreenKind.Payment, "HD-1"));
        Assert.Equal("/shipper", context.ResolveReturnPath("/shipper"));
    }

    [Fact]
    public void 빈의뢰Id는_route로만들지않는다()
    {
        Assert.Throws<ArgumentException>(() => ShipperRequestDetailPageRoutes.SummaryFor("  "));
    }
}
