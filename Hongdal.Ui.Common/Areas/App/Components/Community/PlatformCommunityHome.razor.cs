using System.Globalization;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Versioning;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Ui.Common.Areas.App.Models;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    [Parameter]
    public bool BoardIndexOnly { get; set; }

    [Parameter]
    public bool ListOnly { get; set; }

    [Parameter]
    public string? InitialBoard { get; set; }

    [Parameter]
    public bool StartInComposeMode { get; set; }

    [Parameter]
    public string AppName { get; set; } = "Hongdal";

    [Parameter]
    public string RoleLabel { get; set; } = "플랫폼 구성원";

    [Parameter]
    public string AppKey { get; set; } = "platform";

    [Parameter]
    public IReadOnlyList<PlatformHomeQuickAction> QuickActions { get; set; } = [];

    [Parameter]
    public IReadOnlyList<PlatformHomeWorkspaceProfile> Workspaces { get; set; } = [];

    [Parameter]
    public IReadOnlyList<HongdalCardinalNavigationOption> CardinalNavigationOptions { get; set; } = [];

    [Parameter]
    public bool ShowPrajnaUpayaNavigator { get; set; }

    [Parameter]
    public string UpayaLabel { get; set; } = "방편";

    [Parameter]
    public string PrajnaLabel { get; set; } = "정보";

    [Parameter]
    public string? UpayaHref { get; set; }

    [Parameter]
    public string? PrajnaHref { get; set; }

    [Parameter]
    public bool BaguaCenterOpensCommunity { get; set; }

    [Parameter]
    public bool BaguaCenterShowsTaegeuk { get; set; } = true;

    [Parameter]
    public string? BaguaCenterSymbol { get; set; } = "☶";

    [Parameter]
    public string BaguaCenterTrigramName { get; set; } = "간";

    [Parameter]
    public string BaguaCenterDestinationLabel { get; set; } = "커뮤니티";

    [Parameter]
    public bool UseBaguaRoleTransitionPages { get; set; }

    [Parameter]
    public string? BaguaPerspectiveRoleCode { get; set; }

    [Parameter]
    public bool CanManageCommunityPosts { get; set; }

    [Parameter]
    public bool ShowRealtimeBest { get; set; }

    [Parameter]
    public bool UseCompactHomeSummary { get; set; }

    [Parameter]
    public RenderFragment? CommunityModeContent { get; set; }

    [Parameter]
    public RenderFragment? WorkModeContent { get; set; }

    [Parameter]
    public string? QueryLedgerTemplateKey { get; set; }

    [Parameter]
    public string? QueryDiagramMode { get; set; }

    [Parameter]
    public long? QueryPostId { get; set; }

    [Parameter]
    public string? QuerySeedPostTitle { get; set; }

    [Parameter]
    public string? QueryBoardName { get; set; }

    private string boardIndexSearchText = string.Empty;

    private PlatformCommunityHomeShellViewModel Shell => ViewModel.Shell;
    private PlatformCommunityBoardWorkspaceViewModel Boards => ViewModel.Boards;
    private PlatformCommunityPostEngagementViewModel Engagement => ViewModel.Engagement;
    private PlatformCommunityLedgerPickerViewModel LedgerPicker => ViewModel.LedgerPicker;
    private PlatformCommunityDiagramWorkspaceViewModel DiagramWorkspace => ViewModel.DiagramWorkspace;
    private PlatformCommunityDiagramChatViewModel DiagramChat => DiagramWorkspace.Chat;
    private PlatformCommunityDiagramCanvasViewModel DiagramCanvas => DiagramWorkspace.Canvas;
    private List<PlatformCommunityPostLedgerChoiceResponse> myLedgers => LedgerPicker.Items;
    private List<PlatformCommunityBoardResponse> approvedBoards => Boards.ApprovedBoards;
    private List<PlatformCommunityBoardResponse> pendingBoardRequests => Boards.PendingBoardRequests;
    private PlatformCommunityBoardForm boardForm => Boards.Form;
    private Dictionary<long, CommunityPostOpportunityListResponse> postOpportunities => Engagement.Opportunities;
    private HashSet<long> pendingPostParticipationIds => Engagement.PendingPostParticipationIds;
    private Dictionary<long, string> boardReviewMemo => Boards.ReviewMemos;
    private Dictionary<string, string> 원장블록입력값 => DiagramWorkspace.LedgerBlockValues;
    private Dictionary<string, string> diagramFormValues => DiagramWorkspace.FormValues;
    private Dictionary<string, string> 원장Api경로변수값 => DiagramWorkspace.ApiPathParameterValues;
    private readonly Dictionary<string, WorkflowApiEndpointDto> apiEndpointMetadata = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> featureFlagStates = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<원장블록노드> 팔레트원장블록노드목록 = [];
    private List<string> diagramNodeOrder => DiagramCanvas.NodeOrder;
    private List<원장블록연결선> customDiagramEdges => DiagramCanvas.CustomEdges;
    private readonly List<다이어그램창고대행후보> 창고대행후보목록 = [];
    private Dictionary<string, string> diagramEdgeLabels => DiagramCanvas.EdgeLabels;
    private Dictionary<string, DiagramEdgeStyleKind> diagramEdgeStyles => DiagramCanvas.EdgeStyles;
    private static readonly IReadOnlyList<DiagramConnectionHandleKind> DiagramConnectionHandles =
    [
        DiagramConnectionHandleKind.Top,
        DiagramConnectionHandleKind.Right,
        DiagramConnectionHandleKind.Bottom,
        DiagramConnectionHandleKind.Left
    ];
    private static readonly IReadOnlyList<DiagramEdgeStyleKind> DiagramEdgeStyleOptions =
    [
        DiagramEdgeStyleKind.Curve,
        DiagramEdgeStyleKind.Straight,
        DiagramEdgeStyleKind.Elbow
    ];
    private const string DiagramLayerStructure = "structure";
    private const string DiagramLayerProcedure = "procedure";
    private const string DiagramLayerRole = "role";
    private const string DiagramLayerState = "state";
    private const string DiagramLayerEvidence = "evidence";
    private const string DiagramLayerRisk = "risk";
    private const string DiagramLayerApi = "api";
    private HashSet<string> hiddenDiagramLayerKeys => DiagramCanvas.HiddenLayerKeys;
    private static readonly IReadOnlyList<다이어그램레이어정의> 다이어그램레이어정의s =
    [
        new(DiagramLayerStructure, "구조", "원장 노드 자체입니다. 캔버스의 기준이므로 항상 표시됩니다.", 10, 10, Icons.Material.Filled.Hub, Color.Primary, true, true),
        new(DiagramLayerProcedure, "절차", "노드 사이의 화살표와 흐름 의미입니다.", 20, 20, Icons.Material.Filled.AccountTree, Color.Secondary, true),
        new(DiagramLayerRole, "역할", "각 노드를 주로 처리하는 참여자 역할입니다.", 30, 30, Icons.Material.Filled.PushPin, Color.Info, true),
        new(DiagramLayerState, "상태", "완료, 현재 단계, 대기 상태입니다.", 40, 40, Icons.Material.Filled.Schedule, Color.Success, true),
        new(DiagramLayerEvidence, "증빙", "필수 확인, 사진, 수령, 정산 근거입니다.", 50, 50, Icons.Material.Filled.FactCheck, Color.Warning, true),
        new(DiagramLayerRisk, "리스크", "신고, 분쟁, 오류, 보류처럼 다른 표시를 덮어야 하는 신호입니다.", 60, 60, Icons.Material.Filled.HelpOutline, Color.Error, true),
        new(DiagramLayerApi, "API", "노드에서 호출 가능한 처리 표면과 기존 API 경로입니다.", 70, 35, Icons.Material.Filled.OpenInNew, Color.Info, true)
    ];
    private static readonly IReadOnlyList<string> 창고대행입고계약유형목록 =
    [
        입고계약유형코드.보관대행,
        입고계약유형코드.위탁판매,
        입고계약유형코드.마켓풀필먼트,
        입고계약유형코드.수입통관풀필먼트
    ];
    private static readonly IReadOnlyList<현재원장컨텍스트> 현재원장스냅샷목록 =
    [
        new(
            "ledger-inbound-001",
            "스마트스토어 봄 재고 입고",
            CommunityLedgerTemplateKeys.WarehouseInbound,
            "검수 대기",
            "오늘 10:20",
            "스마트스토어 판매 상품을 공유 창고에 입고해서 보관하고 싶어요.",
            "공급처 한빛상사, 다음 주 화요일 오전 도착 예정, 검수 후 A-01 선반 보관. 입고 이상 사진은 선택으로 남깁니다.",
            "입고 예정, 검수, 보관 위치가 이미 잡힌 창고 입고 원장입니다.",
            new Dictionary<string, string>
            {
                ["참여자"] = "입고 요청자: 알뜰상점 / 납품자: 한빛상사 / 입고 검수자: 공유창고 담당자",
                ["입고"] = "SKU-BOM-001 외 3종, 총 42개 입고 예정",
                ["납품"] = "다음 주 화요일 오전 도착 예정, 택배 송장 확인 대기",
                ["검수"] = "수량, 파손, 누락 확인 후 이상 사진 선택 첨부",
                ["보관"] = "A-01 선반 우선 배정, 냉장 필요 없음",
                ["마감"] = "검수 완료 후 재고 전환"
            }),
        new(
            "ledger-mart-002",
            "동네 생수 묶음 즉시배송",
            CommunityLedgerTemplateKeys.HongdalMart,
            "피킹 준비",
            "어제 18:05",
            "동네 사람들이 같이 주문한 생수를 가까운 재고에서 꺼내 포장하고 바로 배송하고 싶어요.",
            "주문자 4명, 도심 창고 재고 12묶음, 피킹 담당자와 배달자 필요. 포장 완료 뒤 기사 픽업으로 넘깁니다.",
            "알뜰살뜰 마트 주문, 도심 재고, 피킹/포장, 배송이 하나로 이어지는 원장입니다.",
            new Dictionary<string, string>
            {
                ["참여자"] = "주문자 4명 / 마트 피킹 담당자 1명 / 배달자 미정",
                ["주문"] = "생수 2L 6입 12묶음, 오늘 저녁 공동 수령",
                ["재고"] = "도심 창고 B-02 재고 우선 사용",
                ["피킹"] = "묶음 수량 확인 후 박스 단위로 피킹",
                ["포장"] = "묶음 라벨 부착 후 포장 완료 처리",
                ["픽업"] = "포장 완료 뒤 근거리 기사 픽업",
                ["전달"] = "공동 수령 장소: 역 앞 상가 1층"
            }),
        new(
            "ledger-cargo-003",
            "중고 책장 운송 요청",
            CommunityLedgerTemplateKeys.CargoTransport,
            "기사 확인 필요",
            "3일 전",
            "중고 책장을 상차지에서 하차지까지 옮기고 상차/하차 확인을 남기고 싶어요.",
            "상차지는 파주, 하차지는 은평구. 엘리베이터 여부와 수작업비 확인이 필요합니다.",
            "상차지, 하차지, 화물 조건, 증빙 확인이 필요한 화물 운송 원장입니다.",
            new Dictionary<string, string>
            {
                ["참여자"] = "요청자: 익명 판매자 / 운반자: 기사님 배정 전 / 수령 확인자: 구매자",
                ["상차"] = "파주시 야당동, 아파트 지하주차장 진입 가능 여부 확인 필요",
                ["하차"] = "은평구 응암동, 엘리베이터 있음",
                ["화물"] = "중고 책장 1개, 약 180cm, 포장 없음",
                ["증빙"] = "상차 전 사진, 하차 후 수령 확인 사진 선택",
                ["정산"] = "하차 완료 후 결제 표시 예정"
            })
    ];

    private CommunityPostComposerViewModel Composer => ViewModel.Composer;
    private YouTubeFoodCommunityDiscoveryViewModel FoodDiscovery => ViewModel.FoodDiscovery;
    private IReadOnlyList<PlatformCommunityPostResponse> posts => ViewModel.PostList.Items;
    private List<IBrowserFile> selectedFiles => Composer.SelectedFiles;
    private CommunityPostComposerDraftViewModel form => Composer.Draft;
    private ElementReference diagramCanvasElement;
    private IJSObjectReference? diagramJsModule;
    private string selectedLedgerTemplateKey
    {
        get => DiagramWorkspace.SelectedLedgerTemplateKey;
        set => DiagramWorkspace.SelectedLedgerTemplateKey = value;
    }
    private string selectedBoardFilter
    {
        get => ViewModel.PostList.SelectedBoard;
        set => ViewModel.PostList.SelectedBoard = value;
    }

    private string selectedForumListFilter
    {
        get => ViewModel.PostList.SelectedListFilter;
        set => ViewModel.PostList.SelectedListFilter = value;
    }

    private CommunityPostViewMode communityPostViewMode
    {
        get => ViewModel.PostList.ViewMode;
        set => ViewModel.PostList.ViewMode = value;
    }

    private string communityPostSearchText
    {
        get => ViewModel.PostList.SearchText;
        set => ViewModel.PostList.SearchText = value;
    }
    private string ledgerPickerSearchText
    {
        get => LedgerPicker.SearchText;
        set => LedgerPicker.SearchText = value;
    }
    private string ledgerPickerScope
    {
        get => LedgerPicker.Scope;
        set => LedgerPicker.Scope = value;
    }
    private string? pendingLedgerId
    {
        get => LedgerPicker.PendingLedgerId;
        set => LedgerPicker.PendingLedgerId = value;
    }
    private long? selectedForumPostId
    {
        get => ViewModel.PostList.SelectedPostId;
        set => ViewModel.PostList.SelectedPostId = value;
    }
    private string? selectedForumSeedPostTitle
    {
        get => Engagement.SelectedSeedPostTitle;
        set => Engagement.SelectedSeedPostTitle = value;
    }
    private string? 선택현재원장Id;
    private string? 선택원장블록노드제목
    {
        get => DiagramCanvas.SelectedNodeTitle;
        set => DiagramCanvas.SelectedNodeTitle = value;
    }
    private string? connectionStartNodeTitle
    {
        get => DiagramCanvas.ConnectionStartNodeTitle;
        set => DiagramCanvas.ConnectionStartNodeTitle = value;
    }
    private string? diagramConnectionMessage
    {
        get => DiagramCanvas.ConnectionMessage;
        set => DiagramCanvas.ConnectionMessage = value;
    }
    private string? selectedDiagramEdgeId
    {
        get => DiagramCanvas.SelectedEdgeId;
        set => DiagramCanvas.SelectedEdgeId = value;
    }
    private string? 선택창고대행후보키;
    private 원장블록노드? nodeDetailPanelNode;
    private 원장블록노드? 창고대행신청노드;
    private bool isDiagramEdgeOptionDockCollapsed
    {
        get => DiagramCanvas.IsEdgeOptionDockCollapsed;
        set => DiagramCanvas.IsEdgeOptionDockCollapsed = value;
    }
    private bool 창고대행후보목록로딩중;
    private bool 창고대행신청제출중;
    private DiagramHandleDrag? activeDiagramHandleDrag
    {
        get => DiagramCanvas.ActiveHandleDrag;
        set => DiagramCanvas.ActiveHandleDrag = value;
    }
    private DiagramDragPoint? diagramDragPointer
    {
        get => DiagramCanvas.DragPointer;
        set => DiagramCanvas.DragPointer = value;
    }
    private DiagramSnapshotDto? sharedLedgerDiagramSnapshot;
    private bool diagramHandleDragMoved
    {
        get => DiagramCanvas.HandleDragMoved;
        set => DiagramCanvas.HandleDragMoved = value;
    }
    private bool suppressNextDiagramHandleClick
    {
        get => DiagramCanvas.SuppressNextHandleClick;
        set => DiagramCanvas.SuppressNextHandleClick = value;
    }
    private string newConnectionLabel
    {
        get => DiagramCanvas.NewConnectionLabel;
        set => DiagramCanvas.NewConnectionLabel = value;
    }
    private string warehouseProxySupplierCode = string.Empty;
    private string warehouseProxySupplierName = string.Empty;
    private string warehouseProxyOrderReference = string.Empty;
    private DateTime? warehouseProxyExpectedArrivalDate = DateTime.Today.AddDays(1);
    private string warehouseProxyContractNo = string.Empty;
    private string 창고대행계약유형 = 입고계약유형코드.보관대행;
    private string warehouseProxyContractCounterpartyName = string.Empty;
    private string warehouseProxyContractSettlementType = "보관료/작업비 협의";
    private decimal warehouseProxyContractCommissionRate;
    private decimal warehouseProxyContractDailyStorageFee;
    private string 창고대행메모 = string.Empty;
    private string 원함입력 = string.Empty;
    private string 원함조건입력 = string.Empty;
    private CommunityLedgerFlowAnalysisResponse? 원함분석결과;
    private long? editingPostId
    {
        get => Composer.EditingPostId;
        set => Composer.EditingPostId = value;
    }
    private bool isLoading
    {
        get => Shell.IsLoading;
        set => Shell.IsLoading = value;
    }
    private bool isBoardLoading
    {
        get => Boards.IsLoading;
        set => Boards.IsLoading = value;
    }
    private bool isApiEndpointMetadataLoading;
    private bool isMyLedgersLoading
    {
        get => LedgerPicker.IsLoading;
        set => LedgerPicker.IsLoading = value;
    }
    private bool isLedgerPickerOpen
    {
        get => LedgerPicker.IsPickerOpen;
        set => LedgerPicker.IsPickerOpen = value;
    }
    private bool isLedgerDetailOpen
    {
        get => LedgerPicker.IsDetailOpen;
        set => LedgerPicker.IsDetailOpen = value;
    }
    private bool isOrderLedgerHierarchyOpen
    {
        get => LedgerPicker.IsHierarchyOpen;
        set => LedgerPicker.IsHierarchyOpen = value;
    }
    private bool isLedgerDetailLoading
    {
        get => LedgerPicker.IsDetailLoading;
        set => LedgerPicker.IsDetailLoading = value;
    }
    private bool ledgerDetailOpenedFromHierarchy
    {
        get => LedgerPicker.DetailOpenedFromHierarchy;
        set => LedgerPicker.DetailOpenedFromHierarchy = value;
    }
    private bool isLedgerSharingSaving
    {
        get => LedgerPicker.IsSharingSaving;
        set => LedgerPicker.IsSharingSaving = value;
    }
    private bool isSharedLedgerReusing
    {
        get => LedgerPicker.IsSharedLedgerReusing;
        set => LedgerPicker.IsSharedLedgerReusing = value;
    }
    private bool isSavingBoardRequest
    {
        get => Boards.IsSavingRequest;
        set => Boards.IsSavingRequest = value;
    }
    private bool isWorkMode
    {
        get => Shell.IsWorkMode;
        set => Shell.IsWorkMode = value;
    }
    private bool isCompactHomeSummary
    {
        get => Shell.IsCompactHomeSummary;
        set => Shell.IsCompactHomeSummary = value;
    }
    private bool isComposeOpen
    {
        get => Composer.IsOpen;
        set => Composer.IsOpen = value;
    }

    private bool isBaguaNavigatorOpen
    {
        get => Shell.IsBaguaNavigatorOpen;
        set => Shell.IsBaguaNavigatorOpen = value;
    }
    private bool isBaguaDockDragging;
    private bool hasBaguaDockDragMoved;
    private double baguaDockDragStartX;
    private string? statusMessage
    {
        get => Shell.StatusMessage;
        set => Shell.StatusMessage = value;
    }
    private string? myLedgerLoadMessage
    {
        get => LedgerPicker.LoadMessage;
        set => LedgerPicker.LoadMessage = value;
    }
    private string? ledgerDetailErrorMessage
    {
        get => LedgerPicker.DetailErrorMessage;
        set => LedgerPicker.DetailErrorMessage = value;
    }
    private 커뮤니티원장공개설정Response? ledgerSharingSettings
    {
        get => LedgerPicker.SharingSettings;
        set => LedgerPicker.SharingSettings = value;
    }
    private PlatformCommunityPostLedgerContextResponse? ledgerDetailContext
    {
        get => LedgerPicker.DetailContext;
        set => LedgerPicker.DetailContext = value;
    }
    private PlatformCommunityPostLedgerContextResponse? orderLedgerHierarchyContext
    {
        get => LedgerPicker.HierarchyContext;
        set => LedgerPicker.HierarchyContext = value;
    }
    private string? 창고대행신청알림문구;
    private string? 원장전송결과메시지;
    private Severity statusSeverity
    {
        get => Shell.StatusKind switch
        {
            CommunityComposerMessageKind.Success => Severity.Success,
            CommunityComposerMessageKind.Warning => Severity.Warning,
            CommunityComposerMessageKind.Error => Severity.Error,
            _ => Severity.Info
        };
        set => Shell.StatusKind = value switch
        {
            Severity.Success => CommunityComposerMessageKind.Success,
            Severity.Warning => CommunityComposerMessageKind.Warning,
            Severity.Error => CommunityComposerMessageKind.Error,
            _ => CommunityComposerMessageKind.Info
        };
    }
    private Severity diagramConnectionSeverity
    {
        get => DiagramCanvas.ConnectionMessageKind switch
        {
            CommunityComposerMessageKind.Success => Severity.Success,
            CommunityComposerMessageKind.Warning => Severity.Warning,
            CommunityComposerMessageKind.Error => Severity.Error,
            _ => Severity.Info
        };
        set => DiagramCanvas.ConnectionMessageKind = value switch
        {
            Severity.Success => CommunityComposerMessageKind.Success,
            Severity.Warning => CommunityComposerMessageKind.Warning,
            Severity.Error => CommunityComposerMessageKind.Error,
            _ => CommunityComposerMessageKind.Info
        };
    }
    private Severity 창고대행신청알림수준 = Severity.Info;
    private Severity 원장전송결과Severity = Severity.Info;
    private bool isDisposed;
    private static readonly IReadOnlyList<string> ForumListFilterOptions = ["전체글", "추천글", "공지"];
    private static readonly IReadOnlyList<string> LedgerPickerScopes = ["전체", "내 원장", "공개 원장"];

    private bool IsDiagramMode => DiagramPalette.IsDiagramMode;

    private bool IsCompactHomeSummaryVisible
        => UseCompactHomeSummary && isCompactHomeSummary && !isWorkMode && !IsDiagramMode;

    private string HomeRootClass => isLedgerPickerOpen || isLedgerDetailOpen || isOrderLedgerHierarchyOpen
        ? "py-4 platform-community-home platform-home--ledger-picker"
        : IsDiagramMode
        ? "py-4 platform-community-home platform-home--diagram"
        : isWorkMode
        ? "py-4 platform-community-home platform-home--work"
        : IsCompactHomeSummaryVisible
        ? "py-4 platform-community-home platform-home--summary"
        : "py-4 platform-community-home platform-home--community";

    private string CommunityGridClass => IsDiagramMode
        ? "platform-community-main-grid platform-community-main-grid--diagram"
        : isWorkMode
        ? "platform-community-main-grid platform-home-section-hidden"
        : "platform-community-main-grid";

    private string BuildDiagramEdgeDockClass(원장블록연결선? selectedDiagramEdge)
    {
        var state = isDiagramEdgeOptionDockCollapsed
            ? " platform-ledger-edge-options-dock--collapsed"
            : " platform-ledger-edge-options-dock--expanded";
        var empty = selectedDiagramEdge is null
            ? " platform-ledger-edge-options-dock--empty"
            : string.Empty;
        return $"platform-ledger-edge-options-dock{state}{empty}";
    }

    private string BuildDiagramEdgeDockToggleClass()
        => isDiagramEdgeOptionDockCollapsed
            ? "platform-ledger-edge-dock-toggle platform-ledger-edge-dock-toggle--collapsed"
            : "platform-ledger-edge-dock-toggle platform-ledger-edge-dock-toggle--expanded";

    private string BuildDiagramEdgeDockToggleIcon()
        => isDiagramEdgeOptionDockCollapsed
            ? Icons.Material.Filled.ChevronLeft
            : Icons.Material.Filled.ChevronRight;

    private string BuildDiagramEdgeDockToggleLabel()
        => isDiagramEdgeOptionDockCollapsed ? "선 옵션 열기" : "선 옵션 접기";

    private void ToggleDiagramEdgeOptionDock()
    {
        isDiagramEdgeOptionDockCollapsed = !isDiagramEdgeOptionDockCollapsed;
    }

    private void CollapseDiagramEdgeOptionDock()
    {
        isDiagramEdgeOptionDockCollapsed = true;
    }

    private string WorkPanelClass => isWorkMode && !IsDiagramMode
        ? "pa-4 platform-work-panel"
        : "pa-4 platform-work-panel platform-home-section-hidden";

    private string CurrentModeLabel => IsDiagramMode ? "다이어그램 모드" : isWorkMode ? "업무 모드" : "커뮤니티 모드";

    private Color CurrentModeColor => IsDiagramMode ? Color.Secondary : isWorkMode ? Color.Success : Color.Primary;

    private IReadOnlyList<string> CommunityBoardOptions
        => new[] { "전체" }
            .Concat(BoardCategoryOptions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private IReadOnlyList<string> VisibleBoardIndexOptions
        => CommunityBoardOptions
            .Where(board => !string.Equals(board, "전체", StringComparison.OrdinalIgnoreCase))
            .Where(board => string.IsNullOrWhiteSpace(boardIndexSearchText)
                || board.Contains(boardIndexSearchText.Trim(), StringComparison.OrdinalIgnoreCase)
                || ResolveCommunityBoardDescription(board).Contains(
                    boardIndexSearchText.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private IReadOnlyList<PlatformCommunityPostResponse> VisiblePosts
        => ViewModel.PostList.VisibleItems;

    private IReadOnlyList<CommunitySeedPost> VisibleSeedPosts
        => SeedPosts
            .Where(post => string.Equals(selectedBoardFilter, "전체", StringComparison.OrdinalIgnoreCase)
                || string.Equals(post.Category, selectedBoardFilter, StringComparison.OrdinalIgnoreCase))
            .Where(MatchesForumListFilter)
            .Where(MatchesCommunityPostSearch)
            .ToArray();

    private int ForumVisiblePostCount => VisiblePosts.Count + VisibleSeedPosts.Count;

    private IReadOnlyList<공통홈베스트글요약> 실시간베스트글
        => posts
            .Select(post => new 공통홈베스트글요약(
                post.Id,
                null,
                post.Title,
                post.Category,
                DisplayPostNickname(post),
                post.RecommendationCount,
                post.CommentCount,
                post.IsTrending,
                post.LastEngagedAtUtc ?? post.CreatedAtUtc))
            .Concat(SeedPosts.Select(post => new 공통홈베스트글요약(
                null,
                post.Title,
                post.Title,
                post.Category,
                post.Author,
                post.RecommendationCount,
                post.CommentCount,
                false,
                DateTime.MinValue)))
            .OrderByDescending(post => post.실시간인기)
            .ThenByDescending(post => (post.추천수 * 3) + (post.댓글수 * 2))
            .ThenByDescending(post => post.최근활동일시)
            .Take(3)
            .ToArray();

    private int 공통홈전체글수 => posts.Count + SeedPosts.Count;

    private IReadOnlyList<string> 공통홈게시판명목록
        => CommunityBoardOptions
            .Where(board => !string.Equals(board, "전체", StringComparison.OrdinalIgnoreCase))
            .Take(4)
            .ToArray();

    private int 공통홈보유상품수
        => DecorationState.Products.Count(DecorationState.IsProductOwned);

    private string 공통홈현재테마명
        => DecorationState.Products.FirstOrDefault(product =>
               string.Equals(
                   product.PackKey,
                   DecorationState.ActiveHomeThemePackKey,
                   StringComparison.OrdinalIgnoreCase))?.Title
           ?? "홍달 기본 홈";

    private IReadOnlyList<string> 공통홈추천상품명목록
        => DecorationState.Products
            .OrderByDescending(DecorationState.IsProductActive)
            .ThenByDescending(product => !DecorationState.IsProductOwned(product))
            .Select(product => product.Title)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .ToArray();

    private string CurrentCommunityBoardTitle
        => string.Equals(selectedBoardFilter, "전체", StringComparison.OrdinalIgnoreCase)
            ? "살뜰 게시판"
            : $"{selectedBoardFilter} 게시판";

    private string CurrentCommunityBoardDescription
        => string.Equals(selectedBoardFilter, "전체", StringComparison.OrdinalIgnoreCase)
            ? "질문과 경험을 나누고, 필요한 일은 원장과 업무 흐름으로 이어가는 공간입니다."
            : ResolveCommunityBoardDescription(selectedBoardFilter);

}
