using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using MudBlazor;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private WorkspaceSurface? workspaceSurface;

    private WorkspaceSurface Workspace
        => workspaceSurface ??= new(this);

    public sealed class WorkspaceSurface
    {
        private readonly PlatformCommunityHome owner;

        internal WorkspaceSurface(PlatformCommunityHome owner)
        {
            this.owner = owner;
        }

        public bool IsBoardLoading => owner.isBoardLoading;

        public bool IsSavingBoardRequest => owner.isSavingBoardRequest;

        public bool CanManagePosts => owner.CanManageCommunityPosts;

        public IReadOnlyList<PlatformCommunityBoardResponse> ApprovedBoards => owner.approvedBoards;

        public IReadOnlyList<PlatformCommunityBoardResponse> PendingBoards => owner.pendingBoardRequests;

        public PlatformCommunityBoardForm BoardForm => owner.boardForm;

        public Dictionary<long, string> BoardReviewMemos => owner.boardReviewMemo;

        public IReadOnlyList<PlatformHomeWorkspaceProfile> Workspaces => owner.UnifiedWorkspaces;

        public LedgerDraftSurface LedgerDraft => owner.LedgerDraft;

        public CommunityPostComposerViewModel Composer => owner.Composer;

        public IReadOnlyList<string> BoardCategories => owner.BoardCategoryOptions;

        public IReadOnlyList<string> RoleTags => owner.RoleTagOptions;

        public CommunityWorkClassificationResponse? SelectedWorkClassification
            => owner.SelectedWorkClassification;

        public bool SelectedWorkFeatureEnabled => owner.SelectedWorkFeatureEnabled;

        public bool IsLedgerPickerLoading => owner.isMyLedgersLoading;

        public string? StatusMessage => owner.statusMessage;

        public Severity StatusSeverity => owner.statusSeverity;

        public bool IsEvidenceChartOpen => owner.ViewModel.IsEvidenceChartToolOpen;

        public string? SelectedLedgerId => owner.form.커뮤니티원장Id;

        public IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse> Ledgers => owner.myLedgers;

        public 커뮤니티원장공개설정Response? SharingSettings => owner.ledgerSharingSettings;

        public bool IsLedgerSharingSaving => owner.isLedgerSharingSaving;

        public CommunityAuthoringEvidenceChartViewModel EvidenceChart => owner.EvidenceChart;

        public IReadOnlyList<PlatformHomeQuickAction> QuickActions => owner.QuickActions;

        public IReadOnlyList<string> OperatingNotes => PlatformCommunityHome.OperatingNotes;

        public void SelectBoard(string title) => owner.SelectBoard(title);

        public Task SaveBoardRequestAsync() => owner.SaveBoardRequestAsync();

        public Task ApproveBoardAsync(PlatformCommunityBoardResponse board)
            => owner.ReviewBoardAsync(board, true);

        public Task RejectBoardAsync(PlatformCommunityBoardResponse board)
            => owner.ReviewBoardAsync(board, false);

        public void PrepareLedgerDraft(string templateKey)
            => owner.PrepareLedgerCommunityDraft(templateKey);

        public Task OpenLedgerPickerAsync() => owner.OpenLedgerPickerAsync();

        public void OpenEvidenceChart() => owner.OpenEvidenceChartTool();

        public void CloseComposer() => owner.CloseCompose();

        public void CancelEdit() => owner.CancelEdit();

        public Task HandleSavedAsync(CommunityPostComposerSaveResult result)
            => owner.HandleComposerSavedAsync(result);

        public Task OpenLedgerSharingSettingsAsync() => owner.OpenLedgerSharingSettingsAsync();

        public void ClearSelectedLedger() => owner.ClearSelectedLedger();

        public void CloseLedgerSharingSettings() => owner.CloseLedgerSharingSettings();

        public Task SaveLedgerSharingSettingsAsync() => owner.SaveLedgerSharingSettingsAsync();

        public void CloseEvidenceChart() => owner.CloseEvidenceChartTool();

        public void ApplyEvidenceChart() => owner.ApplyEvidenceChartToDraft();
    }
}
