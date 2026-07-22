using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace WarehouseManagerApp.Services;

public sealed class WarehousePlatformHomeWorkspaceNavigationResolver
    : IPlatformHomeWorkspaceNavigationResolver
{
    public string? ResolveEntryHref(PlatformHomeWorkspaceProfile workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        return workspace.LedgerTemplateKey switch
        {
            CommunityLedgerTemplateKeys.CargoTransport => WarehouseManagerRoutes.TransportRequestDraft,
            CommunityLedgerTemplateKeys.WarehouseOutbound => WarehouseManagerRoutes.OutboundWorkStart,
            CommunityLedgerTemplateKeys.WarehouseInbound => WarehouseManagerRoutes.InboundWorkStart,
            CommunityLedgerTemplateKeys.LocalSale => WarehouseManagerRoutes.MartHome,
            CommunityLedgerTemplateKeys.GroupImport => WarehouseManagerRoutes.ImportArrival,
            _ => null
        };
    }
}
