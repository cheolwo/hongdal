using Ssalddel.Ui.Common.Areas.App.Models;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface IPlatformHomeWorkspaceNavigationResolver
{
    string? ResolveEntryHref(PlatformHomeWorkspaceProfile workspace);
}

public sealed class UnsupportedPlatformHomeWorkspaceNavigationResolver
    : IPlatformHomeWorkspaceNavigationResolver
{
    public string? ResolveEntryHref(PlatformHomeWorkspaceProfile workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        return null;
    }
}
