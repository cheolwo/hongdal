using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Ui.Common.Areas.App.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "커뮤니티 홈에서 공개 게시판과 명시적으로 여는 연결 도구 ViewModel을 조립",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.IndependentExecution,
    Boundary = "기본 진입은 게시판과 글이며 후속 업무 도구는 사용자 행동과 기능 플래그 뒤에서만 엽니다.")]
public sealed class PlatformCommunityHomePageViewModel : PageViewModelBase
{
    private bool _isEvidenceChartToolOpen;

    public PlatformCommunityHomePageViewModel(
        PlatformCommunityPublicBoardViewModel publicBoard,
        PlatformCommunityConnectedToolsViewModel connectedTools)
    {
        PublicBoard = 하위ViewModel등록(publicBoard, 수명소유: true);
        ConnectedTools = 하위ViewModel등록(connectedTools, 수명소유: true);
    }

    public PlatformCommunityPublicBoardViewModel PublicBoard { get; }
    public PlatformCommunityConnectedToolsViewModel ConnectedTools { get; }
    public CommunityPostComposerViewModel Composer => PublicBoard.Composer;
    public CommunityPostListPageViewModel PostList => PublicBoard.PostList;
    public PlatformCommunityHomeShellViewModel Shell => PublicBoard.Shell;
    public PlatformCommunityBoardWorkspaceViewModel Boards => PublicBoard.Boards;
    public PlatformCommunityPostEngagementViewModel Engagement => PublicBoard.Engagement;
    public PlatformCommunityLedgerPickerViewModel LedgerPicker => PublicBoard.LedgerPicker;
    public YouTubeFoodCommunityDiscoveryViewModel FoodDiscovery => ConnectedTools.FoodDiscovery;
    public PlatformCommunityDiagramWorkspaceViewModel DiagramWorkspace => ConnectedTools.DiagramWorkspace;
    public PlatformCommunityWishFlowViewModel WishFlow => ConnectedTools.WishFlow;
    public CommunityAuthoringEvidenceChartViewModel EvidenceChart => ConnectedTools.EvidenceChart;
    public PlatformCommunityWarehouseProxyViewModel WarehouseProxy => ConnectedTools.WarehouseProxy;
    public CommunityPostJourneyCollectionViewModel ActionJourneys => Engagement.Journeys;

    public bool IsEvidenceChartToolOpen
    {
        get => _isEvidenceChartToolOpen;
        private set => SetProperty(ref _isEvidenceChartToolOpen, value);
    }

    public void Configure(string appKey, string defaultRoleTag)
        => PublicBoard.Configure(appKey, defaultRoleTag);

    public void OpenEvidenceChartTool()
    {
        EvidenceChart.PrepareFromDraft(Composer.Draft.Title, Composer.Draft.Body);
        Composer.Open();
        IsEvidenceChartToolOpen = true;
        Composer.SetStatus(
            "주장을 뒷받침할 수치와 출처, 기준일, 해석과 한계를 함께 정리합니다.",
            CommunityComposerMessageKind.Info);
    }

    public void CloseEvidenceChartTool()
        => IsEvidenceChartToolOpen = false;

    public bool ApplyEvidenceChartToDraft()
    {
        var result = EvidenceChart.ApplyToDraft(Composer.Draft);
        if (!result.Succeeded)
        {
            Composer.SetStatus(result.Message, CommunityComposerMessageKind.Warning);
            return false;
        }

        Composer.Open();
        IsEvidenceChartToolOpen = false;
        Composer.SetStatus(
            $"{result.Message} 게시 뒤에도 같은 데이터로 그래프와 요약 통계가 표시됩니다.",
            CommunityComposerMessageKind.Success);
        return true;
    }

    public void ResetEvidenceChartTool()
    {
        EvidenceChart.Reset();
        IsEvidenceChartToolOpen = false;
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
        => await PublicBoard.LoadAsync(새로고침, cancellationToken);
}
