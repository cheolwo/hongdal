using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed record PlatformCommunityCommandResult(
    bool Succeeded,
    string? Message = null,
    CommunityComposerMessageKind MessageKind = CommunityComposerMessageKind.Info,
    bool RefreshPosts = false);

public sealed record PlatformCommunityLedgerReuseResult(
    PlatformCommunityCommandResult Command,
    커뮤니티원장재사용Response? ReusedLedger = null);
