using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.WebApp.Services;

public sealed class WebPlatformHomeWorkspaceNavigationResolver
    : IPlatformHomeWorkspaceNavigationResolver
{
    private const string DriverRecommendations = "/driver/recommendations";

    public string? ResolveEntryHref(PlatformHomeWorkspaceProfile workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        return workspace.LedgerTemplateKey switch
        {
            CommunityLedgerTemplateKeys.CargoTransport => ShipperRoutes.Request,
            CommunityLedgerTemplateKeys.FoodDelivery => DriverRecommendations,
            CommunityLedgerTemplateKeys.WarehouseOutbound => WarehouseManagerRoutes.OutboundWorkStart,
            CommunityLedgerTemplateKeys.WarehouseInbound => WarehouseManagerRoutes.InboundWorkStart,
            CommunityLedgerTemplateKeys.LocalSale => CommunityPageRoutes.Home,
            CommunityLedgerTemplateKeys.GroupPurchase => CommunityPageRoutes.GroupPurchase,
            CommunityLedgerTemplateKeys.GroupImport => CommunityPageRoutes.GroupImport,
            CommunityLedgerTemplateKeys.Errand => CommunityPageRoutes.Home,
            _ => null
        };
    }
}
