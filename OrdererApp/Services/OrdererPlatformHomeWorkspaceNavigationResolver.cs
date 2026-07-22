using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace OrdererApp.Services;

public sealed class OrdererPlatformHomeWorkspaceNavigationResolver
    : IPlatformHomeWorkspaceNavigationResolver
{
    public string? ResolveEntryHref(PlatformHomeWorkspaceProfile workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        return workspace.LedgerTemplateKey switch
        {
            CommunityLedgerTemplateKeys.CargoTransport => OrdererRoutes.Cargo,
            CommunityLedgerTemplateKeys.FoodOrder => OrdererRoutes.Food,
            CommunityLedgerTemplateKeys.LocalSale => OrdererRoutes.Mart,
            CommunityLedgerTemplateKeys.GroupPurchase => OrdererRoutes.GroupPurchase,
            CommunityLedgerTemplateKeys.GroupImport => OrdererRoutes.GroupPurchase,
            _ => null
        };
    }
}
