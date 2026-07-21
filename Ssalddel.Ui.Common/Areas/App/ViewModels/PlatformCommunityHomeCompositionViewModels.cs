namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 공개 게시판의 목록, 글쓰기, 게시판 탐색과 게시글 참여 기능을 한 경계로 조립합니다.
/// </summary>
public sealed class PlatformCommunityPublicBoardViewModel : 조립ViewModelBase
{
    public PlatformCommunityPublicBoardViewModel(
        CommunityPostComposerViewModel composer,
        CommunityPostListPageViewModel postList,
        PlatformCommunityHomeShellViewModel shell,
        PlatformCommunityBoardWorkspaceViewModel boards,
        PlatformCommunityPostEngagementViewModel engagement,
        PlatformCommunityLedgerPickerViewModel ledgerPicker)
    {
        Composer = 하위ViewModel등록(composer, 수명소유: true);
        PostList = 하위ViewModel등록(postList, 수명소유: true);
        Shell = 하위ViewModel등록(shell, 수명소유: true);
        Boards = 하위ViewModel등록(boards, 수명소유: true);
        Engagement = 하위ViewModel등록(engagement, 수명소유: true);
        LedgerPicker = 하위ViewModel등록(ledgerPicker, 수명소유: true);
    }

    public CommunityPostComposerViewModel Composer { get; }
    public CommunityPostListPageViewModel PostList { get; }
    public PlatformCommunityHomeShellViewModel Shell { get; }
    public PlatformCommunityBoardWorkspaceViewModel Boards { get; }
    public PlatformCommunityPostEngagementViewModel Engagement { get; }
    public PlatformCommunityLedgerPickerViewModel LedgerPicker { get; }

    public void Configure(string appKey, string defaultRoleTag)
    {
        Composer.Configure(appKey, defaultRoleTag);
        PostList.Configure(appKey);
    }

    public async Task LoadAsync(bool refresh, CancellationToken cancellationToken)
    {
        var loaded = refresh
            ? await PostList.새로고침Async(cancellationToken)
            : await PostList.초기화Async(cancellationToken);
        if (!loaded)
        {
            throw new InvalidOperationException(
                PostList.오류메시지 ?? "커뮤니티 게시글 목록을 불러오지 못했습니다.");
        }
    }
}

/// <summary>
/// 게시판에서 사용자가 명시적으로 이동하거나 여는 원장·다이어그램·업무 연결 도구를 조립합니다.
/// 공개 글 목록의 기본 책임과 분리하여 후속 도구가 페이지 초기화를 지배하지 않게 합니다.
/// </summary>
public sealed class PlatformCommunityConnectedToolsViewModel : 조립ViewModelBase
{
    public PlatformCommunityConnectedToolsViewModel(
        YouTubeFoodCommunityDiscoveryViewModel foodDiscovery,
        PlatformCommunityDiagramWorkspaceViewModel diagramWorkspace,
        PlatformCommunityWishFlowViewModel wishFlow,
        CommunityAuthoringEvidenceChartViewModel evidenceChart,
        PlatformCommunityWarehouseProxyViewModel warehouseProxy)
    {
        FoodDiscovery = 하위ViewModel등록(foodDiscovery, 수명소유: true);
        DiagramWorkspace = 하위ViewModel등록(diagramWorkspace, 수명소유: true);
        WishFlow = 하위ViewModel등록(wishFlow, 수명소유: true);
        EvidenceChart = 하위ViewModel등록(evidenceChart, 수명소유: true);
        WarehouseProxy = 하위ViewModel등록(warehouseProxy, 수명소유: true);
    }

    public YouTubeFoodCommunityDiscoveryViewModel FoodDiscovery { get; }
    public PlatformCommunityDiagramWorkspaceViewModel DiagramWorkspace { get; }
    public PlatformCommunityWishFlowViewModel WishFlow { get; }
    public CommunityAuthoringEvidenceChartViewModel EvidenceChart { get; }
    public PlatformCommunityWarehouseProxyViewModel WarehouseProxy { get; }
}
