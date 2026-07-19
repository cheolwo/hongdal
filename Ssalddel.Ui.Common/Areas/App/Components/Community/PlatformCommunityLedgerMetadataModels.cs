using Ssalddel.Contracts.Common.Community;
using MudBlazor;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public sealed record PlatformCommunityLedgerApiSurfacePresentation(
    CommunityLedgerProcessingSurfaceResponse Surface,
    string Method,
    Color StatusColor,
    string StatusLabel,
    string ResolvedRoute,
    bool HasUnresolvedMetadata,
    bool HasMissingRouteParameters);
