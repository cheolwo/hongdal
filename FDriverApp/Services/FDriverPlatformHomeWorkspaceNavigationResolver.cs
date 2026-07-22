using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace FDriverApp.Services;

public sealed class FDriverPlatformHomeWorkspaceNavigationResolver
    : IPlatformHomeWorkspaceNavigationResolver
{
    private const string Workspace = "/food-delivery/open/workspace";

    public string? ResolveEntryHref(PlatformHomeWorkspaceProfile workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        return workspace.LedgerTemplateKey == CommunityLedgerTemplateKeys.FoodDelivery
            ? Workspace
            : null;
    }
}
