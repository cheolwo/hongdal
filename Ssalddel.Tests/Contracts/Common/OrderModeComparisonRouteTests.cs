using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Tests.Contracts.Common;

public sealed class OrderModeComparisonRouteTests
{
    [Fact]
    public void 비교화면은_상품상세와구분되는안정된경로를사용한다()
    {
        Assert.Equal(
            "/group-purchase/compare/apple-5kg",
            GroupPurchasePageRoutes.OrderModeComparisonFor("apple-5kg"));
        Assert.Equal(
            "/group-purchase/compare/{ProductId}",
            GroupPurchasePageRoutes.OrderModeComparisonTemplate);
        Assert.Equal(
            GroupPurchaseScreenKind.OrderModeComparison,
            Enum.Parse<GroupPurchaseScreenKind>("OrderModeComparison"));
    }

    [Fact]
    public void 비교화면은_0점5공개읽기전용Capability로분류한다()
    {
        var found = SsalddelPageCapabilityCatalog.TryResolve(
            SsalddelPageAppCodes.Orderer,
            "/group-purchase/compare/apple-5kg",
            out var capability);

        Assert.True(found);
        Assert.Equal("orderer-order-mode-comparison", capability.PageKey);
        Assert.Equal(
            SsalddelProductRoadmapCatalog.IndividualOrderVersion,
            capability.IntroducedVersion);
        Assert.Equal(PageInteractionBoundary.ReadOnly, capability.Boundary);
        Assert.False(capability.RequiresAuthentication);
        Assert.False(capability.HasExternalEffects);
    }
}
