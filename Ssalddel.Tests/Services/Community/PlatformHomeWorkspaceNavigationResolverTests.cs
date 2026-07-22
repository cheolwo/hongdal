using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.Services.Community;

public sealed class PlatformHomeWorkspaceNavigationResolverTests
{
    [Fact]
    public void WebResolver_OnlyReturnsRoutesThatWebProvides()
    {
        var resolver = new WebPlatformHomeWorkspaceNavigationResolver();

        Assert.Equal(
            ShipperRoutes.Request,
            Resolve(resolver, CommunityLedgerTemplateKeys.CargoTransport));
        Assert.Equal(
            "/driver/recommendations",
            Resolve(resolver, CommunityLedgerTemplateKeys.FoodDelivery));
        Assert.Equal(
            WarehouseManagerRoutes.OutboundWorkStart,
            Resolve(resolver, CommunityLedgerTemplateKeys.WarehouseOutbound));
        Assert.Equal(
            WarehouseManagerRoutes.InboundWorkStart,
            Resolve(resolver, CommunityLedgerTemplateKeys.WarehouseInbound));
        Assert.Equal(
            CommunityPageRoutes.GroupPurchase,
            Resolve(resolver, CommunityLedgerTemplateKeys.GroupPurchase));
        Assert.Equal(
            CommunityPageRoutes.GroupImport,
            Resolve(resolver, CommunityLedgerTemplateKeys.GroupImport));
        Assert.Null(Resolve(resolver, CommunityLedgerTemplateKeys.FoodOrder));
    }

    [Fact]
    public void UnsupportedResolver_NeverInventsAHostRoute()
    {
        var resolver = new UnsupportedPlatformHomeWorkspaceNavigationResolver();

        Assert.All(
            PlatformHomeWorkspaceCatalog.DefaultWorkspaces,
            workspace => Assert.Null(resolver.ResolveEntryHref(workspace)));
    }

    private static string? Resolve(
        IPlatformHomeWorkspaceNavigationResolver resolver,
        string ledgerTemplateKey)
        => resolver.ResolveEntryHref(Assert.Single(
            PlatformHomeWorkspaceCatalog.DefaultWorkspaces,
            workspace => workspace.LedgerTemplateKey == ledgerTemplateKey));
}
