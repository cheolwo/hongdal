using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Mart;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Contracts.Common.Community;

public static class CommunityActivitySourceKinds
{
    public const string Command = "command";
    public const string Event = "event";

    public static string DisplayName(string sourceKind)
        => string.Equals(sourceKind, Command, StringComparison.OrdinalIgnoreCase)
            ? "Command"
            : "Event";
}

public static class CommunityActivityBoardKeys
{
    public const string Foundation = "activity-foundation";
    public const string GroupPurchase = "activity-group-purchase";
    public const string TradeReadiness = "activity-trade-readiness";
    public const string Transport = "activity-transport";
    public const string Fulfillment = "activity-fulfillment";
    public const string FoodDelivery = "activity-food-delivery";
    public const string Mart = "activity-mart";
}

public sealed record CommunityActivityPageDefinition(
    string Surface,
    string PageName,
    string Route,
    string Responsibility,
    bool IsWebRoute)
{
    public bool IsRouteTemplate
        => Route.Contains('{');

    public bool CanNavigateFromCommunityWeb
        => IsWebRoute
           && !IsRouteTemplate
           && Route.StartsWith("/", StringComparison.Ordinal);
}

public sealed record CommunityActivityBoardDefinition(
    string SourceKind,
    string SourceName,
    string ActivityDisplayName,
    string ProductVersion,
    string PublicActivitySummary,
    CommunityBoardDefinition Board)
{
    public SsalddelProductRoadmapStage RoadmapStage
        => SsalddelProductRoadmapCatalog.Find(ProductVersion);

    public string SourceKindDisplayName
        => CommunityActivitySourceKinds.DisplayName(SourceKind);
}

public sealed record CommunityActivityBoardBundleDefinition(
    string ProductVersion,
    CommunityBoardDefinition Board,
    IReadOnlyList<CommunityActivityBoardDefinition> Activities,
    IReadOnlyList<CommunityActivityPageDefinition> Pages)
{
    public const string MountainSymbol = "☶";
    public const string MountainName = "간";

    public SsalddelProductRoadmapStage RoadmapStage
        => SsalddelProductRoadmapCatalog.Find(ProductVersion);

    public int CommandCount
        => Activities.Count(activity => activity.SourceKind == CommunityActivitySourceKinds.Command);

    public int EventCount
        => Activities.Count(activity => activity.SourceKind == CommunityActivitySourceKinds.Event);
}

/// <summary>
/// 0.0~3.5의 공개 가능한 Command/Event를 버전 업무 게시판 일곱 개에 묶습니다.
/// Command/Event 하나를 게시판 하나로 만들지 않고, 같은 목적의 활동과 단일책임 페이지를 한 산으로 조망합니다.
/// </summary>
public static class CommunityActivityBoardCatalog
{
    public const string SurfaceMappingBoundary =
        "0.0~3.5의 공개 가능한 Command·Event는 일곱 개 버전 업무 게시판과 단일책임 페이지에 연결합니다.";

    public const string PrivacyBoundary =
        "사용자·업체 식별자, 연락처, 상세 주소, 위치, 금액, 결제 정보, 첨부와 원본 payload는 공개하지 않습니다.";

    public static IReadOnlyList<CommunityActivityBoardBundleDefinition> Bundles { get; } =
    [
        Bundle(
            SsalddelProductRoadmapCatalog.FoundationVersion,
            CommunityActivityBoardKeys.Foundation,
            "문화교통 0.0 · 커뮤니티 기반",
            "공개 콘텐츠·공공데이터와 커뮤니티 학습 Command를 함께 점검하는 게시판",
            sources:
            [
                Command(
                    "콘텐츠시청완료Command",
                    "activity-content-watch-completed",
                    "공개 콘텐츠 학습 완료",
                    "한 이웃이 공개 콘텐츠 학습 흐름을 완료했습니다.")
            ],
            pages:
            [
                WebPage("커뮤니티 게시판", CommunityPageRoutes.Boards, "공개 글 목록과 게시판 문맥 조회"),
                WebPage("공공데이터", "/information/public-data", "농수축산 가격과 공공 근거 조회"),
                WebPage("공식 음식 재료", "/information/food-ingredients", "공식 레시피·재료·가격 근거 조회")
            ]),

        Bundle(
            SsalddelProductRoadmapCatalog.GroupPurchaseVersion,
            CommunityActivityBoardKeys.GroupPurchase,
            "문화교통 1.0 · 공동구매",
            "개별 원함에서 주문자 집단화와 공동 원장 변경까지 점검하는 게시판",
            sources:
            [
                Event(
                    "커뮤니티원장변경됨Event",
                    "activity-collective-ledger-changed",
                    "공동 원장 변경",
                    "공동구매·공동수입 원장의 공개 가능한 진행 단계가 변경되었습니다.")
            ],
            pages:
            [
                WebPage("공동구매 둘러보기", CommunityPageRoutes.GroupPurchase, "공동구매 목록과 모집 흐름 조회"),
                WebPage("공동구매 수요", CommunityPageRoutes.GroupPurchaseDemand, "비구속 수요 등록 진입"),
                AppPage("OrdererApp", "내 원함 목록", GroupPurchasePageRoutes.WishesRoot, "개별 원함 원장 조회"),
                AppPage("OrdererApp", "자동 집단 목록", GroupPurchasePageRoutes.GroupsRoot, "집단화 결과 조회")
            ]),

        Bundle(
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            CommunityActivityBoardKeys.TradeReadiness,
            "문화교통 1.5 · 공급·무역 준비",
            "공급 근거, HS 검토와 통관 동의·수임·절차 Event를 함께 점검하는 게시판",
            sources:
            [
                Command(
                    "화주HsCode검토요청Command",
                    "activity-hs-review-requested",
                    "HS 코드 검토 요청",
                    "무역 준비 과정에서 HS 코드 검토 요청이 접수되었습니다."),
                Event(
                    "화주통관의뢰등록됨Event",
                    "activity-customs-request-created",
                    "통관 의뢰 등록",
                    "공동수입 준비 과정에서 통관 의뢰가 등록되었습니다."),
                Event(
                    "통관조회동의등록됨Event",
                    "activity-customs-consent-recorded",
                    "통관 조회 동의",
                    "통관 상태 조회를 위한 명시적 동의가 기록되었습니다."),
                Event(
                    "통관수임요청됨Event",
                    "activity-customs-agent-requested",
                    "통관 수임 요청",
                    "통관 절차를 맡을 전문 역할에 수임 요청이 전달되었습니다."),
                Event(
                    "통관절차생성됨Event",
                    "activity-customs-process-created",
                    "통관 절차 생성",
                    "수출입 이행을 위한 통관 절차가 생성되었습니다."),
                Event(
                    "통관상태변경감지됨Event",
                    "activity-customs-status-changed",
                    "통관 상태 변경",
                    "통관 절차의 상태 변경이 확인되었습니다.")
            ],
            pages:
            [
                WebPage("공동수입 준비", CommunityPageRoutes.GroupImport, "공동수입 원장 준비와 단계 확인"),
                WebPage("HS 검토함", "/shipper/customs/hs-reviews", "검토가 필요한 HS 후보 조회"),
                WebPage("개별수입 원장", "/orderer/ledgers/individual-import", "개별주문에서 확장된 수입 원장 조회"),
                WebPage("개별수출 원장", "/orderer/ledgers/individual-export", "개별 관계에서 확장된 수출 원장 조회"),
                WebPage("공동수출 원장", "/orderer/ledgers/group-export", "공동 수출 원장 조회"),
                AppPage("OrdererApp", "공동수입 단계 상세", GroupPurchasePageRoutes.ImportOverviewTemplate, "stable-ID 공동수입 상세")
            ]),

        Bundle(
            SsalddelProductRoadmapCatalog.TransportVersion,
            CommunityActivityBoardKeys.Transport,
            "살뜰 2.0 · 화물 운송",
            "운송 의뢰부터 배차, 상차, 하차와 인수 완료까지 점검하는 게시판",
            sources:
            [
                Command(
                    "의뢰생성Command",
                    "activity-transport-request-created",
                    "운송 의뢰 등록",
                    "국내 화물 이행을 위한 운송 의뢰가 등록되었습니다."),
                Event(
                    "배차수락됨Event",
                    "activity-dispatch-accepted",
                    "배차 수락",
                    "운송 의뢰의 배차가 수락되었습니다."),
                Event(
                    "배차거절됨Event",
                    "activity-dispatch-declined",
                    "배차 거절",
                    "운송 의뢰의 배차가 거절되어 다음 배차를 기다립니다."),
                Event(
                    "배차수락취소됨Event",
                    "activity-dispatch-acceptance-cancelled",
                    "배차 수락 취소",
                    "수락했던 배차가 취소되어 운송 배정 상태가 변경되었습니다."),
                Event(
                    "운송상차지도착됨Event",
                    "activity-pickup-arrived",
                    "상차지 도착",
                    "배차된 운송이 상차지 도착 단계에 들어갔습니다."),
                Event(
                    "운송상차완료됨Event",
                    "activity-loading-completed",
                    "상차 완료",
                    "배차된 운송의 상차 단계가 완료되었습니다."),
                Event(
                    "운송하차지도착됨Event",
                    "activity-dropoff-arrived",
                    "하차지 도착",
                    "운송이 하차지 도착 단계에 들어갔습니다."),
                Event(
                    "운송인수완료됨Event",
                    "activity-transport-handover-completed",
                    "운송 인수 완료",
                    "운송 물품의 인수 확인이 완료되었습니다.")
            ],
            pages:
            [
                WebPage("운송 의뢰 작성", "/shipper/request", "화물·운송·절차 입력"),
                WebPage("운송 의뢰 검토", "/shipper/request/review", "의뢰 전 최종 확인"),
                WebPage("기사 추천 목록", "/driver/recommendations", "비구속 배차 후보 조회"),
                WebPage("기사 현재 운송", "/driver/transports/current", "현재 운송 한 건의 상태 조회"),
                WebPage("기사 운송 이력", "/driver/transports/history", "완료·과거 운송 조회"),
                WebPage("운송 상세", "/shipper/request/{RequestId}", "stable-key 운송 의뢰 상세")
            ]),

        Bundle(
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            CommunityActivityBoardKeys.Fulfillment,
            "살뜰 2.5 · 창고·판매 이행",
            "판매자 출고에서 입고·검수·적재·피킹·출고 인계까지 점검하는 게시판",
            sources:
            [
                Event(
                    "판매자상품출고됨Event",
                    "activity-seller-shipment-released",
                    "판매자 출고",
                    "판매자가 물류 이행을 위해 상품을 출고했습니다."),
                Event(
                    "주문자상품입고완료됨Event",
                    "activity-orderer-receipt-completed",
                    "주문자 입고 확인",
                    "주문자가 상품 입고 완료를 확인했습니다."),
                Event(
                    "창고입고완료됨Event",
                    "activity-warehouse-inbound-completed",
                    "창고 입고 완료",
                    "창고 입고 등록이 완료되었습니다."),
                Event(
                    "창고입고검수완료됨Event",
                    "activity-warehouse-inspection-completed",
                    "입고 검수 완료",
                    "입고 상품의 수량·상태 검수가 완료되었습니다."),
                Event(
                    "창고적재위치배정됨Event",
                    "activity-warehouse-location-assigned",
                    "적재 위치 배정",
                    "입고 상품의 창고 적재 위치가 배정되었습니다."),
                Event(
                    "창고출고인계준비완료됨Event",
                    "activity-warehouse-handover-ready",
                    "출고 인계 준비",
                    "창고에서 다음 운송으로 인계할 출고 준비가 완료되었습니다."),
                Event(
                    "창고재위탁운송생성됨Event",
                    "activity-warehouse-transport-created",
                    "재위탁 운송 생성",
                    "창고 이행 뒤 이어질 재위탁 운송 의뢰가 생성되었습니다.")
            ],
            pages:
            [
                WebPage("입고 요청 목록", InboundRequestPageRoutes.Root, "입고 요청 목록 조회"),
                WebPage("창고 재고", "/warehouse/general/inventory", "접근 가능한 재고 조회"),
                WebPage("입고 검수", InboundInspectionPageRoutes.Root, "입고 검수 대상 목록"),
                WebPage("피킹 작업", PickingTaskPageRoutes.Root, "피킹 작업 목록"),
                WebPage("출고 인계 검토", "/warehouse/general/outbound-plan-review", "출고예정 원장 읽기 검토"),
                WebPage("판매 주문", SalesOrderPageRoutes.Root, "판매 주문 목록")
            ]),

        Bundle(
            SsalddelProductRoadmapCatalog.FoodDeliveryVersion,
            CommunityActivityBoardKeys.FoodDelivery,
            "살뜰 3.0 · 음식 주문·배달",
            "음식 주문 등록과 음식점 수락 뒤 조리·기사 인계 페이지를 점검하는 게시판",
            sources:
            [
                Event(
                    "음식주문등록됨Event",
                    "activity-food-order-created",
                    "음식 주문 등록",
                    "음식점 배달 흐름에 새 주문이 등록되었습니다."),
                Event(
                    "음식점주문수락됨Event",
                    "activity-restaurant-order-accepted",
                    "음식점 주문 수락",
                    "음식점이 주문을 수락해 조리·배달 준비가 시작되었습니다.")
            ],
            pages:
            [
                WebPage("음식 탐색", "/community/discover/food", "공개 음식·재료 탐색"),
                AppPage("OrdererApp", "음식 주문 내역", "/orders/food", "주문자의 음식 주문 목록·상세 진입"),
                AppPage("RestaurantDeskApp", "음식점 주문 수신함", "/orders", "음식점 주문 목록"),
                AppPage("RestaurantDeskApp", "음식점 주문 상세", "/orders/{OrderNo}", "정확한 주문번호 상세·수락"),
                AppPage("FDriverApp", "배달기사 홈", "home", "픽업·배송 기사 업무 진입")
            ]),

        Bundle(
            SsalddelProductRoadmapCatalog.MartVersion,
            CommunityActivityBoardKeys.Mart,
            "살뜰 3.5 · 마트·도심 물류",
            "마트 결제, 도심 피킹과 포장 완료 뒤 즉시배송 인계 준비를 점검하는 게시판",
            sources:
            [
                Event(
                    "주문결제완료됨Event",
                    "activity-mart-order-paid",
                    "마트 주문 결제",
                    "마트 주문의 결제 완료 사실이 물류 흐름에 전달되었습니다."),
                Event(
                    "창고피킹완료됨Event",
                    "activity-mart-picking-completed",
                    "상품 피킹 완료",
                    "주문 상품의 피킹 작업이 완료되었습니다."),
                Event(
                    "창고포장완료됨Event",
                    "activity-mart-packing-completed",
                    "상품 포장 완료",
                    "피킹된 주문 상품의 포장 작업이 완료되었습니다.")
            ],
            pages:
            [
                WebPage("마트 공개 상품", MartProductPageRoutes.Root, "공개 상품 목록"),
                WebPage("마트 피킹 주문", MartPickingPageRoutes.WebRoot, "접근 가능한 피킹 주문 목록"),
                WebPage("마트 업무 흐름", "/warehouse/mart/work-board", "입고·재고·피킹 책임별 진입"),
                WebPage("마트 주문 상세", MartPickingPageRoutes.WebDetailTemplate, "stable-ID 피킹·포장 주문 상세"),
                AppPage("OrdererApp", "마트 주문 요청", MartProductPageRoutes.OrderTemplate, "비구속 상품 주문 요청")
            ])
    ];

    public static IReadOnlyList<CommunityActivityBoardDefinition> All { get; } =
        Bundles.SelectMany(bundle => bundle.Activities).ToArray();

    public static IReadOnlyList<CommunityBoardDefinition> Boards { get; } =
        Bundles.Select(bundle => bundle.Board).ToArray();

    public static CommunityActivityBoardDefinition? FindSource(
        string? sourceKind,
        string? sourceName)
        => string.IsNullOrWhiteSpace(sourceName)
            ? null
            : All.FirstOrDefault(definition =>
                string.Equals(definition.SourceKind, sourceKind?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(definition.SourceName, sourceName.Trim(), StringComparison.Ordinal));

    public static CommunityActivityBoardBundleDefinition? FindBundle(string? boardKeyOrName)
        => string.IsNullOrWhiteSpace(boardKeyOrName)
            ? null
            : Bundles.FirstOrDefault(bundle =>
                IsSame(bundle.Board.Key, boardKeyOrName)
                || IsSame(bundle.Board.DisplayName, boardKeyOrName)
                || bundle.Board.LegacyCategoryNames.Any(alias => IsSame(alias, boardKeyOrName)));

    public static CommunityActivityBoardDefinition? FindBoard(string? boardKeyOrName)
        => FindBundle(boardKeyOrName)?.Activities.FirstOrDefault();

    public static bool IsActivityBoard(string? boardKeyOrName)
        => FindBundle(boardKeyOrName) is not null;

    private static CommunityActivityBoardBundleDefinition Bundle(
        string productVersion,
        string boardKey,
        string displayName,
        string description,
        IReadOnlyList<ActivitySourceSeed> sources,
        IReadOnlyList<CommunityActivityPageDefinition> pages)
    {
        var aliases = sources
            .SelectMany(source => new[] { source.LegacyBoardKey, source.ActivityDisplayName })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var board = new CommunityBoardDefinition(
            boardKey,
            displayName,
            description,
            CommunityBoardGroupCodes.ActivityRoadmap,
            "Command·Event 업무 산맥",
            IsUserCreatable: false,
            IsPublic: true,
            PostingAccessCode: CommunityBoardPostingAccessCodes.OperatorOnly,
            LegacyCategoryNames: aliases);
        var activities = sources
            .Select(source => new CommunityActivityBoardDefinition(
                source.SourceKind,
                source.SourceName,
                source.ActivityDisplayName,
                productVersion,
                source.PublicActivitySummary,
                board))
            .ToArray();
        return new CommunityActivityBoardBundleDefinition(
            productVersion,
            board,
            activities,
            pages);
    }

    private static ActivitySourceSeed Command(
        string sourceName,
        string legacyBoardKey,
        string activityDisplayName,
        string publicActivitySummary)
        => new(
            CommunityActivitySourceKinds.Command,
            sourceName,
            legacyBoardKey,
            activityDisplayName,
            publicActivitySummary);

    private static ActivitySourceSeed Event(
        string sourceName,
        string legacyBoardKey,
        string activityDisplayName,
        string publicActivitySummary)
        => new(
            CommunityActivitySourceKinds.Event,
            sourceName,
            legacyBoardKey,
            activityDisplayName,
            publicActivitySummary);

    private static CommunityActivityPageDefinition WebPage(
        string pageName,
        string route,
        string responsibility)
        => new("통합 Web", pageName, route, responsibility, IsWebRoute: true);

    private static CommunityActivityPageDefinition AppPage(
        string surface,
        string pageName,
        string route,
        string responsibility)
        => new(surface, pageName, route, responsibility, IsWebRoute: false);

    private static bool IsSame(string? left, string? right)
        => !string.IsNullOrWhiteSpace(left)
           && !string.IsNullOrWhiteSpace(right)
           && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);

    private sealed record ActivitySourceSeed(
        string SourceKind,
        string SourceName,
        string LegacyBoardKey,
        string ActivityDisplayName,
        string PublicActivitySummary);
}
