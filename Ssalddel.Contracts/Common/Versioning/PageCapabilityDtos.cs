using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Contracts.Common.Versioning;

public enum PageCapabilityMatchKind
{
    Exact,
    Prefix
}

public enum PageCapabilityStage
{
    Live,
    Beta,
    Experience,
    Preparing
}

public enum PageInteractionBoundary
{
    ReadOnly,
    PlatformPersistence,
    Simulation
}

public static class SsalddelPageAppCodes
{
    public const string IntegratedWeb = "Ssalddel.WebApp";
    public const string Admin = "SsalddelAdmin";
    public const string Driver = "DriverApp";
    public const string Orderer = "OrdererApp";
    public const string Shipper = "SsalddelApp";
    public const string Warehouse = "WarehouseManagerApp";
    public const string HumanResources = "HumanResourcesManagerApp";
}

public sealed record SsalddelPageCapabilityRule(
    string PageKey,
    string AppCode,
    string RoutePattern,
    PageCapabilityMatchKind MatchKind,
    PageCapabilityStage Stage,
    PageInteractionBoundary Boundary,
    bool RequiresAuthentication,
    bool HasExternalEffects,
    string IntroducedVersion,
    IReadOnlyList<string> FeatureKeys,
    IReadOnlyList<string> WorkflowCodes,
    string Notice);

public sealed class PageCapabilityDto
{
    public string PageKey { get; init; } = string.Empty;

    public string AppCode { get; init; } = string.Empty;

    public string RoutePattern { get; init; } = string.Empty;

    public string MatchKindCode { get; init; } = string.Empty;

    public string StageCode { get; init; } = string.Empty;

    public string StageName { get; init; } = string.Empty;

    public string BoundaryCode { get; init; } = string.Empty;

    public string BoundaryName { get; init; } = string.Empty;

    public bool RequiresAuthentication { get; init; }

    public bool HasExternalEffects { get; init; }

    public string IntroducedVersion { get; init; } = string.Empty;

    public IReadOnlyList<string> FeatureKeys { get; init; } = [];

    public bool IsFeatureEnabled { get; init; }

    public IReadOnlyList<string> WorkflowCodes { get; init; } = [];

    public string Notice { get; init; } = string.Empty;
}

public static class PageCapabilityLabels
{
    public static string StageName(PageCapabilityStage stage)
        => stage switch
        {
            PageCapabilityStage.Live => "운영",
            PageCapabilityStage.Beta => "베타",
            PageCapabilityStage.Experience => "체험",
            PageCapabilityStage.Preparing => "준비 중",
            _ => "준비 중"
        };

    public static string BoundaryName(PageInteractionBoundary boundary)
        => boundary switch
        {
            PageInteractionBoundary.ReadOnly => "조회",
            PageInteractionBoundary.PlatformPersistence => "플랫폼 저장",
            PageInteractionBoundary.Simulation => "Simulation",
            _ => "상태 확인"
        };
}

/// <summary>
/// 통합 웹의 실제 라우트 계열과 여러 역할 앱의 대표 진입점을 같은 기준으로 설명합니다.
/// 기능 플래그는 노출 가능성을, 실행 경계는 외부 효과 허용 여부를 각각 따로 표현합니다.
/// </summary>
public static class SsalddelPageCapabilityCatalog
{
    private const string Community = "CommunityTrustWorkflow";
    private const string DomesticTransport = "DomesticTransportWorkflow";
    private const string Warehouse = "WarehouseFulfillmentWorkflow";
    private const string Customs = "CustomsAndTradeDataWorkflow";
    private const string GroupPurchase = "GroupPurchaseImportWorkflow";
    private const string Sales = "SalesChannelFulfillmentWorkflow";
    private const string Hr = "HrParticipationWorkflow";
    private const string Food = "FoodDeliveryWorkflow";
    private const string Mart = "SsalddelMartWorkflow";

    private static readonly IReadOnlyList<SsalddelPageCapabilityRule> Items =
    [
        Exact("web-home", SsalddelPageAppCodes.IntegratedWeb, "/", PageCapabilityStage.Live,
            PageInteractionBoundary.ReadOnly, false, "0.0",
            "커뮤니티와 업무 도구를 한곳에서 찾는 통합 시작점입니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("web-login", SsalddelPageAppCodes.IntegratedWeb, "/login", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0",
            "회원가입은 선택이며, 가입할 때만 개인정보 수집·이용에 동의합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("web-not-found", SsalddelPageAppCodes.IntegratedWeb, "/not-found", PageCapabilityStage.Live,
            PageInteractionBoundary.ReadOnly, false, "0.0", "찾을 수 없는 주소를 안전하게 안내합니다."),

        Exact("community-home", SsalddelPageAppCodes.IntegratedWeb, "/community", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0",
            "로그인하지 않아도 공개 글을 읽고 익명 글과 댓글을 등록할 수 있습니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-localized-home-template", SsalddelPageAppCodes.IntegratedWeb, "/{LanguageSegment}/community", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0",
            "국가 문맥과 분리된 화면 언어 경로에서 같은 공개 커뮤니티 기능을 제공합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-korean-home", SsalddelPageAppCodes.IntegratedWeb, "/ko/community", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0",
            "한국어 시스템 UI에서 공개 커뮤니티와 익명 참여를 제공합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-english-home", SsalddelPageAppCodes.IntegratedWeb, "/en/community", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0",
            "영문 시스템 UI에서 공개 커뮤니티를 제공하며 사용자 글은 원문을 유지합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-categories", SsalddelPageAppCodes.IntegratedWeb, "/community/categories", PageCapabilityStage.Live,
            PageInteractionBoundary.ReadOnly, false, "0.0",
            "공개 커뮤니티 게시판 분류와 각 게시판 진입점을 읽기 전용으로 조회합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-board-management", SsalddelPageAppCodes.IntegratedWeb, "/community/boards/manage", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0", "사용자가 게시판 개설을 신청하고 권한이 있는 운영자가 검토합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Prefix("community-boards", SsalddelPageAppCodes.IntegratedWeb, "/community/boards", PageCapabilityStage.Live,
            PageInteractionBoundary.ReadOnly, false, "0.0", "공개 게시판과 게시판별 글을 조회합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Prefix("community-write", SsalddelPageAppCodes.IntegratedWeb, "/community/write", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0", "회원 또는 익명 작성자로 커뮤니티 글을 저장합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Prefix("community-posts", SsalddelPageAppCodes.IntegratedWeb, "/community/posts", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0", "공개 글과 익명 댓글을 조회하고 저장합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-workspace", SsalddelPageAppCodes.IntegratedWeb, "/community/workspace", PageCapabilityStage.Live,
            PageInteractionBoundary.ReadOnly, false, "0.0", "게시판에서 합의한 필요를 업무·원장·다이어그램 화면으로 연결하는 허브입니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-ledger-draft", SsalddelPageAppCodes.IntegratedWeb, "/community/ledgers/new", PageCapabilityStage.Live,
            PageInteractionBoundary.PlatformPersistence, false, "0.0", "당사자가 합의할 조건과 담당 흐름을 공동 원장 초안으로 정리합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-group-purchase-public", SsalddelPageAppCodes.IntegratedWeb, "/community/group-purchase", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, false, "0.0",
            "공개 공동구매 목록만 조회하며 선택한 모집은 campaign ID 상세 route에서 엽니다.",
            false, [Community], ["CommunityTrust"]),
        Exact("community-group-purchase-create", SsalddelPageAppCodes.IntegratedWeb, "/community/group-purchase/new", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "0.0",
            "상품·수량·수령 조건을 비구속 공동구매 제안과 수요 투표로 저장하며 결제나 계약을 확정하지 않습니다.",
            true, [Community, GroupPurchase], ["CommunityTrust"]),
        Prefix("community-group-purchase", SsalddelPageAppCodes.IntegratedWeb, "/community/group-purchase", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, false, "0.0", "campaign ID의 공개 상세를 읽고 참여·합의 Command는 화면과 서버에서 별도로 인증하며 실제 결제·계약·자동 배차는 실행하지 않습니다.", true,
            [Community, GroupPurchase], ["CommunityTrust"]),
        Prefix("community-group-import", SsalddelPageAppCodes.IntegratedWeb, "/community/group-import", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "2.5", "공동수입의 공급·물류·비용 연결 구조를 Simulation으로 살펴봅니다.", true,
            [GroupPurchase], ["GroupPurchaseImport", "CustomsAndTradeData"]),
        Prefix("community-global-trade", SsalddelPageAppCodes.IntegratedWeb, "/community/global-trade", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, false, "2.0", "해외 공급자와 국내 참여자의 공개 대화 흐름을 체험합니다.", false,
            [Customs], ["CustomsAndTradeData", "CommunityTrust"]),
        Prefix("community-actions", SsalddelPageAppCodes.IntegratedWeb, "/community/actions", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "0.0", "관심과 참여를 명시적으로 나누며 실제 실행 전 단계까지만 검증합니다.", false,
            [Community], ["CommunityTrust"]),
        Prefix("community-bagua", SsalddelPageAppCodes.IntegratedWeb, "/community/bagua", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, false, "0.0", "대화에서 공동행동으로 이어지는 다이어그램 전환을 체험합니다.", false,
            [Community], ["CommunityTrust"]),
        Prefix("community-discover", SsalddelPageAppCodes.IntegratedWeb, "/community/discover", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, false, "0.0", "공개 정보에 근거한 주변 후보를 조회합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Prefix("community-safety", SsalddelPageAppCodes.IntegratedWeb, "/community/safety", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "0.0", "신고와 안전 조치 흐름을 로그인 사용자 기준으로 검증합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Exact("community-role-application", SsalddelPageAppCodes.IntegratedWeb, "/community/roles/apply", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "2.5",
            "자발적인 역할 관심을 최소 정보로 저장하고 본인이 철회하며, 역할 배정·채용·계약·보수는 실행하지 않습니다.",
            featureKeys: [Hr], workflowCodes: ["HrParticipation", "CommunityTrust"]),
        Prefix("community-personal", SsalddelPageAppCodes.IntegratedWeb, "/community/me", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "0.0", "내 글·참여·원장·알림과 개인 설정을 관리합니다.",
            featureKeys: [Community], workflowCodes: ["CommunityTrust"]),
        Prefix("community-decorations", SsalddelPageAppCodes.IntegratedWeb, "/community/decorations", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "0.0", "꾸미기와 FakePG 흐름은 Simulation 범위에서만 체험합니다.", true,
            [Community], ["CommunityTrust"]),

        Prefix("public-data", SsalddelPageAppCodes.IntegratedWeb, "/information/public-data", PageCapabilityStage.Live,
            PageInteractionBoundary.ReadOnly, false, "0.0", "출처와 기준 시각을 함께 표시하는 공개 정보 조회 화면입니다."),
        Prefix("public-price-comparison", SsalddelPageAppCodes.IntegratedWeb, "/information/agricultural-fisheries-price-comparison", PageCapabilityStage.Live,
            PageInteractionBoundary.ReadOnly, false, "0.0", "공개 가격 자료의 출처·단위·기준 시각을 비교합니다."),
        Exact("official-food-ingredients", SsalddelPageAppCodes.IntegratedWeb, "/information/food-ingredients", PageCapabilityStage.Live,
            PageInteractionBoundary.ReadOnly, false, "0.0", "공식 레시피의 표준 재료, 출처가 확인된 공공가격과 실제 관련 레시피를 조회합니다."),

        Exact("global-home", SsalddelPageAppCodes.IntegratedWeb, "/global", PageCapabilityStage.Experience,
            PageInteractionBoundary.ReadOnly, false, "2.0", "해외 상품과 공급 조건을 공개 탐색합니다.",
            featureKeys: [Customs], workflowCodes: ["CustomsAndTradeData"]),
        Prefix("global-products", SsalddelPageAppCodes.IntegratedWeb, "/global/products", PageCapabilityStage.Experience,
            PageInteractionBoundary.ReadOnly, false, "2.0", "샘플 상품의 상세 정보와 수입 검토 항목을 조회합니다.",
            featureKeys: [Customs], workflowCodes: ["CustomsAndTradeData"]),
        Prefix("global-suppliers", SsalddelPageAppCodes.IntegratedWeb, "/global/suppliers", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, false, "2.0", "공급자 제출 내용은 체험 세션에서만 다룹니다.", false,
            [Customs], ["CustomsAndTradeData"]),
        Prefix("global-import-requests", SsalddelPageAppCodes.IntegratedWeb, "/global/import-requests", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "2.5", "수입 요청을 Simulation 원장으로 검토합니다.", true,
            [GroupPurchase, Customs], ["GroupPurchaseImport", "CustomsAndTradeData"]),
        Prefix("global-orders", SsalddelPageAppCodes.IntegratedWeb, "/global/orders", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "2.5", "수입 주문 원장의 연결 구조를 체험합니다.", true,
            [GroupPurchase, Customs], ["GroupPurchaseImport", "CustomsAndTradeData"]),

        Prefix("shipper-public-cargo", SsalddelPageAppCodes.IntegratedWeb, "/shipper/public-cargo", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, false, "1.0", "공개 설정된 화물 정보만 조회합니다.",
            featureKeys: [DomesticTransport], workflowCodes: ["DomesticTransport"]),
        Prefix("shipper-payment-status", SsalddelPageAppCodes.IntegratedWeb, "/shipper/request/payment-status", PageCapabilityStage.Preparing,
            PageInteractionBoundary.Simulation, true, "1.0", "실결제와 정산은 비활성이고 FakePG 상태만 검증합니다.", true,
            [DomesticTransport], ["DomesticTransport"]),
        Prefix("shipper-request", SsalddelPageAppCodes.IntegratedWeb, "/shipper/request", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.0", "운송 의뢰 저장을 검증하되 자동 배차·계약 확정은 실행하지 않습니다.", true,
            [DomesticTransport], ["DomesticTransport"]),
        Prefix("shipper-inbound", SsalddelPageAppCodes.IntegratedWeb, "/shipper/inbound", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5", "입고와 재고 전환 흐름을 Simulation 데이터로 검증합니다.", true,
            [Warehouse], ["WarehouseFulfillment"]),
        Prefix("shipper-warehouse", SsalddelPageAppCodes.IntegratedWeb, "/shipper/warehouse", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5", "화주 관점의 창고·재고 흐름을 Simulation으로 검증합니다.", true,
            [Warehouse], ["WarehouseFulfillment"]),
        Prefix("shipper-exploration", SsalddelPageAppCodes.IntegratedWeb, "/shipper/exploration", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.0", "운행 가능성 문의를 계약 확정과 분리해 검증합니다.", true,
            [DomesticTransport], ["DomesticTransport"]),
        Prefix("shipper-reconsignment", SsalddelPageAppCodes.IntegratedWeb, "/shipper/reconsignment", PageCapabilityStage.Preparing,
            PageInteractionBoundary.Simulation, true, "1.5", "재위탁 운송은 실제 배차 없이 Simulation으로만 준비합니다.", true,
            [Warehouse, DomesticTransport], ["WarehouseFulfillment", "DomesticTransport"]),
        Exact("shipper-sales-channels", SsalddelPageAppCodes.IntegratedWeb, "/shipper/sales/channels", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "2.5",
            "사용자 소유 판매채널 연결 준비 원장을 조회·저장하며 외부 인증·발행·주문 동기화는 실행하지 않습니다.", false,
            [Sales], ["SalesChannelFulfillment"]),
        Exact("shipper-sales-orders", SsalddelPageAppCodes.IntegratedWeb, "/shipper/sales/orders", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "2.5",
            "재고 예약과 함께 영속된 사용자 소유 판매채널 주문 출고 후보만 조회하며 외부 주문 수집과 출고 실행은 하지 않습니다.", false,
            [Sales], ["SalesChannelFulfillment"]),
        Prefix("shipper-sales", SsalddelPageAppCodes.IntegratedWeb, "/shipper/sales", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "2.5", "외부 판매채널 발행 없이 판매·출고 화면을 체험합니다.", true,
            [Sales], ["SalesChannelFulfillment"]),
        Exact("shipper-customs-hs-reviews", SsalddelPageAppCodes.IntegratedWeb, "/shipper/customs/hs-reviews", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "2.0",
            "활성 HS 코드 원장과 공개 동의된 근거를 조회하며 품목분류·세율·신고를 확정하지 않습니다.", false,
            [Customs], ["CustomsAndTradeData"]),
        Prefix("shipper-customs", SsalddelPageAppCodes.IntegratedWeb, "/shipper/customs", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "2.0", "HS 코드 후보와 검토 상태를 체험하며 전문 판단을 확정하지 않습니다.", true,
            [Customs], ["CustomsAndTradeData"]),
        Prefix("shipper-international", SsalddelPageAppCodes.IntegratedWeb, "/shipper/international", PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "2.0", "FCL/LCL 비용 판단을 Simulation으로 비교합니다.", true,
            [Customs], ["CustomsAndTradeData"]),
        Prefix("shipper-settings", SsalddelPageAppCodes.IntegratedWeb, "/shipper/settings", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "1.0", "사용자별 화면·프로필 설정을 관리합니다.",
            featureKeys: [DomesticTransport], workflowCodes: ["DomesticTransport"]),
        Prefix("shipper-home", SsalddelPageAppCodes.IntegratedWeb, "/shipper", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.0", "화주·판매자 업무는 통합 베타에서 Simulation 경계를 유지합니다.", true,
            [DomesticTransport], ["DomesticTransport"]),
        Prefix("dispatch-address", SsalddelPageAppCodes.IntegratedWeb, "/dispatch", PageCapabilityStage.Preparing,
            PageInteractionBoundary.Simulation, true, "1.0", "주소 입력은 검증할 수 있지만 자동 배차는 비활성입니다.", true,
            [DomesticTransport], ["DomesticTransport"]),

        Prefix("driver-recommendations", SsalddelPageAppCodes.IntegratedWeb, "/driver/recommendations", PageCapabilityStage.Preparing,
            PageInteractionBoundary.Simulation, true, "1.0", "추천 후보를 표시하지만 자동 배차나 계약 확정은 실행하지 않습니다.", true,
            [DomesticTransport, Food], ["DomesticTransport", "FoodDelivery"]),
        Prefix("driver-transports", SsalddelPageAppCodes.IntegratedWeb, "/driver/transports", PageCapabilityStage.Preparing,
            PageInteractionBoundary.Simulation, true, "1.0", "운송 단계 전환은 실제 운송과 분리된 Simulation입니다.", true,
            [DomesticTransport, Food], ["DomesticTransport", "FoodDelivery"]),
        Prefix("driver-settlements", SsalddelPageAppCodes.IntegratedWeb, "/driver/settlements", PageCapabilityStage.Preparing,
            PageInteractionBoundary.Simulation, true, "1.0", "실정산은 비활성이고 화면 흐름만 준비합니다.", true,
            [DomesticTransport], ["DomesticTransport"]),
        Prefix("driver-work", SsalddelPageAppCodes.IntegratedWeb, "/driver/work", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.0", "기사 운행 설정과 시작 흐름을 Simulation으로 검증합니다.", true,
            [DomesticTransport], ["DomesticTransport"]),
        Prefix("driver-proof", SsalddelPageAppCodes.IntegratedWeb, "/driver/transport/proof", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.0", "상·하차 증빙 화면을 실제 운송과 분리해 검증합니다.", true,
            [DomesticTransport], ["DomesticTransport"]),
        Prefix("driver-account", SsalddelPageAppCodes.IntegratedWeb, "/driver/account", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "1.0", "민감정보 마스킹과 계좌 설정 흐름을 검증합니다.",
            featureKeys: [DomesticTransport], workflowCodes: ["DomesticTransport"]),
        Prefix("driver-profile", SsalddelPageAppCodes.IntegratedWeb, "/driver/me", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "1.0", "기사 개인 설정을 관리합니다.",
            featureKeys: [DomesticTransport], workflowCodes: ["DomesticTransport"]),
        Prefix("driver-notifications", SsalddelPageAppCodes.IntegratedWeb, "/driver/notifications", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "1.0", "기사 알림과 알림 설정을 관리합니다.",
            featureKeys: [DomesticTransport], workflowCodes: ["DomesticTransport"]),
        Prefix("driver-settings", SsalddelPageAppCodes.IntegratedWeb, "/driver/settings", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "1.0", "기사 화면 설정을 관리합니다.",
            featureKeys: [DomesticTransport], workflowCodes: ["DomesticTransport"]),
        Prefix("driver-home", SsalddelPageAppCodes.IntegratedWeb, "/driver", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.0", "기사 업무 현황을 통합 베타에서 조회합니다.",
            featureKeys: [DomesticTransport], workflowCodes: ["DomesticTransport"]),

        Exact("web-warehouse-mart-picking", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/mart/picking", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "3.5", "로그인 계정이 접근할 수 있는 영속 마트 주문과 피킹·포장 작업만 조회합니다.",
            featureKeys: [Mart], workflowCodes: ["SsalddelMart"]),
        Exact("web-warehouse-inbound-products", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/work/inbound/products", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "1.5",
            "정확한 상품 바코드로 입고예정을 조회하고 멱등 현장 반입 요청을 저장한 뒤 같은 ID로 다시 조회합니다. 검수·입고완료·재고 생성은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-inbound-products-alias", SsalddelPageAppCodes.IntegratedWeb, "/work/inbound/products", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "1.5",
            "통합 웹의 입고상품 수령 호환 경로입니다. 입고예정 요청만 저장하며 검수·입고완료·재고 생성은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-inbound-inspection", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/work/inbound/inspection", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "접근 가능한 입고 완료 상품의 검수 결과를 서버 원장에 저장하고 같은 ID를 다시 조회합니다. 적재·출고·운송·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-inbound-inspection-alias", SsalddelPageAppCodes.IntegratedWeb, "/work/inbound/inspection", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "통합 웹의 입고 검수 호환 경로입니다. 서버 Simulation 검수만 저장하며 적재·출고·운송·정산은 자동 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-inventory-overview", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/general/inventory", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.5",
            "현재 계정의 창고 소유·배정 범위에서 최소 재고 목록과 명시한 입고상품 근거만 조회합니다. 사용자 ID와 계약·정산 내용은 노출하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-inventory-overview-alias", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/inventory", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.5",
            "통합 웹의 창고 재고 현황 호환 경로이며 창고 접근 범위의 최소 재고 정보만 읽고 계약·정산 내용은 노출하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-put-away-task", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/work/inbound/put-away", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "검수 완료 재고 한 건의 확인된 보관 위치만 저장하고 같은 입고상품 ID를 다시 조회합니다. 위치 이동·출고·운송·계약·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-put-away-task-alias", SsalddelPageAppCodes.IntegratedWeb, "/work/inbound/put-away", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "통합 웹의 적재 작업 호환 경로입니다. 검수 완료 재고의 위치 확정만 Simulation으로 저장합니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-packing-task", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/work/outbound/packing", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "적재 완료 재고의 전체 가용수량 포장 사실만 저장하고 같은 입고상품 ID를 다시 조회합니다. 재고 차감·출고·운송·계약·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-packing-task-alias", SsalddelPageAppCodes.IntegratedWeb, "/work/outbound/packing", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "통합 웹의 포장 작업 호환 경로입니다. 적재 완료 재고의 출고 준비 포장 사실만 Simulation으로 저장합니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-outbound-handoff", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/general/transport-handoff", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "포장 완료 재고의 전체 가용수량을 출고예정 원장에 한 번만 기록합니다. 재고 예약·차감, 운송의뢰·배차·결제·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-outbound-handoff-alias", SsalddelPageAppCodes.IntegratedWeb, "/work/outbound/handoff", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "통합 웹의 출고 인계 준비 호환 경로입니다. 출고예정 원장 준비만 저장하고 실제 운송 효과는 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-outbound-plan-review", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/general/outbound-plan-review", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.5",
            "준비된 출고예정 원장의 포장·수량·출발지 근거와 운송의뢰 입력 필요 항목을 읽기 전용으로 검토합니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-outbound-plan-review-alias", SsalddelPageAppCodes.IntegratedWeb, "/work/outbound/plans", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.5",
            "통합 웹의 출고예정 운송 전 검토 호환 경로입니다. 출고예정·재고·운송 상태를 변경하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-transport-request-draft", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/general/transport-request-draft", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "검토를 통과한 출고예정의 하차지·희망 일정·차량 조건을 로컬 초안으로만 구성하며 서버 상태를 변경하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-transport-request-draft-alias", SsalddelPageAppCodes.IntegratedWeb, "/work/outbound/transport-request-draft", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "통합 웹의 운송의뢰 로컬 초안 호환 경로입니다. 재고 예약·운송 생성·배차·결제를 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-picking-task", SsalddelPageAppCodes.IntegratedWeb, "/warehouse/work/picking-batch", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "접근 가능한 피킹 작업의 대기·진행중·완료 상태만 서버에 저장하고 같은 작업 Key를 다시 조회합니다. 재고·포장·출고·운송·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("web-warehouse-picking-task-alias", SsalddelPageAppCodes.IntegratedWeb, "/work/picking-batch", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "통합 웹의 피킹 작업 호환 경로입니다. 서버 Simulation 피킹 상태만 저장하며 재고·포장·출고·운송·정산은 자동 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Prefix("warehouse", SsalddelPageAppCodes.IntegratedWeb, "/warehouse", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5", "입고·검수·피킹·포장을 실제 보관 계약과 분리해 검증합니다.", true,
            [Warehouse, Mart], ["WarehouseFulfillment", "SsalddelMart"]),
        Prefix("warehouse-work-alias", SsalddelPageAppCodes.IntegratedWeb, "/work", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5", "창고 현장 호환 경로이며 Simulation 경계를 유지합니다.", true,
            [Warehouse, Mart], ["WarehouseFulfillment", "SsalddelMart"]),
        Exact("warehouse-work-board-alias", SsalddelPageAppCodes.IntegratedWeb, "/work-board", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5", "창고 작업 보드 호환 경로이며 Simulation 경계를 유지합니다.", true,
            [Warehouse], ["WarehouseFulfillment"]),
        Exact("warehouse-app-expected-inbounds", SsalddelPageAppCodes.Warehouse, "/warehouse/inbounds/expected", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.5", "창고 역할과 HR 세부 역할을 확인한 뒤 서버의 입고 예정 목록을 조회합니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-work-board", SsalddelPageAppCodes.Warehouse, "/work-board", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.5", "선택한 입고 요청을 같은 ID로 다시 조회하고 서버 상태에 따른 다음 업무를 확인합니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-inbound-products", SsalddelPageAppCodes.Warehouse, "/work/inbound/products", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "1.5",
            "정확한 상품 바코드로 입고예정을 조회하고 멱등 현장 반입 요청을 저장한 뒤 같은 ID로 다시 조회합니다. 검수·입고완료·재고 생성은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-inbound-inspection", SsalddelPageAppCodes.Warehouse, "/work/inbound/inspection", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "접근 가능한 입고 완료 상품의 검수 결과를 서버 원장에 저장하고 같은 ID를 다시 조회합니다. 적재·출고·운송·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-inventory-overview", SsalddelPageAppCodes.Warehouse, "/warehouse/general/inventory", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.5",
            "현재 계정의 창고 소유·배정 범위에서 최소 재고 목록과 명시한 입고상품 근거만 조회합니다. 사용자 ID와 계약·정산 내용은 노출하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-put-away-task", SsalddelPageAppCodes.Warehouse, "/work/inbound/put-away", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "검수 완료 재고 한 건의 확인된 보관 위치만 저장하고 같은 입고상품 ID를 다시 조회합니다. 위치 이동·출고·운송·계약·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-packing-task", SsalddelPageAppCodes.Warehouse, "/work/outbound/packing", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "적재 완료 재고의 전체 가용수량 포장 사실만 저장하고 같은 입고상품 ID를 다시 조회합니다. 재고 차감·출고·운송·계약·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-outbound-handoff", SsalddelPageAppCodes.Warehouse, "/warehouse/general/transport-handoff", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "포장 완료 재고의 전체 가용수량을 출고예정 원장에 한 번만 기록합니다. 재고 예약·차감, 운송의뢰·배차·결제·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-outbound-plan-review", SsalddelPageAppCodes.Warehouse, "/warehouse/general/outbound-plan-review", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.5",
            "준비된 출고예정 원장의 포장·수량·출발지 근거와 운송의뢰 입력 필요 항목을 읽기 전용으로 검토합니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-transport-request-draft", SsalddelPageAppCodes.Warehouse, "/warehouse/general/transport-request-draft", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "검토를 통과한 출고예정의 하차지·희망 일정·차량 조건을 로컬 초안으로만 구성하며 서버 상태를 변경하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-picking-task", SsalddelPageAppCodes.Warehouse, "/work/picking-batch", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5",
            "접근 가능한 피킹 작업의 대기·진행중·완료 상태만 서버에 저장하고 같은 작업 Key를 다시 조회합니다. 재고·포장·출고·운송·정산은 실행하지 않습니다.",
            featureKeys: [Warehouse], workflowCodes: ["WarehouseFulfillment"]),
        Exact("warehouse-app-mart-picking", SsalddelPageAppCodes.Warehouse, "/mart/picking", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "3.5", "접근 가능한 창고의 영속 마트 주문과 피킹·포장 작업을 조회하며 상태는 변경하지 않습니다.",
            featureKeys: [Mart], workflowCodes: ["SsalddelMart"]),
        Exact("web-mart-picking-alias", SsalddelPageAppCodes.IntegratedWeb, "/mart/picking", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "3.5", "통합 웹의 마트 피킹 호환 경로이며 영속 작업을 읽기 전용으로 조회합니다.",
            featureKeys: [Mart], workflowCodes: ["SsalddelMart"]),
        Prefix("mart-work", SsalddelPageAppCodes.IntegratedWeb, "/mart", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "3.5", "도심 마트 작업을 Simulation 데이터로 검증합니다.", true,
            [Mart], ["SsalddelMart"]),
        Exact("warehouse-scan-alias", SsalddelPageAppCodes.IntegratedWeb, "/scan", PageCapabilityStage.Beta,
            PageInteractionBoundary.Simulation, true, "1.5", "브라우저 스캔 입력을 Simulation으로 검증합니다.", true,
            [Warehouse], ["WarehouseFulfillment"]),

        Exact("web-orderer-mart-order-request", SsalddelPageAppCodes.IntegratedWeb, "/orderer/mart/order", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "3.5",
            "한 공개 상품의 비구속 주문 요청을 멱등 저장하고 같은 ID를 다시 조회하며 재고·결제·출고 원장은 변경하지 않습니다.", false,
            [Mart], ["SsalddelMart"]),
        Exact("web-orderer-mart", SsalddelPageAppCodes.IntegratedWeb, "/orderer/mart", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, false, "3.5",
            "내부 창고 원장과 분리된 공개 상품과 판매 가능 수량 투영을 익명으로 조회합니다.", false,
            [Mart], ["SsalddelMart"]),
        Prefix("orderer-home", SsalddelPageAppCodes.IntegratedWeb, "/orderer", PageCapabilityStage.Preparing,
            PageInteractionBoundary.Simulation, true, "2.5", "공동주문 집단화와 실제 결제 연결 전 화면 골격을 준비합니다.", true,
            [GroupPurchase], ["GroupPurchaseImport"]),
        Prefix("document-tools", SsalddelPageAppCodes.IntegratedWeb, "/tools", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "1.0", "업무 문서와 식별자 출력을 생성 전 미리 확인합니다."),
        Exact("diagram", SsalddelPageAppCodes.IntegratedWeb, CommunityPageRoutes.Diagram, PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "0.0", "공동 원장과 업무 노드의 연결 구조를 체험합니다.", false,
            [Community], ["CommunityTrust"]),
        Exact("shipper-diagram", SsalddelPageAppCodes.Shipper, CommunityPageRoutes.Diagram, PageCapabilityStage.Experience,
            PageInteractionBoundary.Simulation, true, "0.0", "Web과 같은 공용 Screen에서 공동 원장과 업무 노드의 연결 구조를 탐색합니다.", false,
            [Community], ["CommunityTrust"]),

        Exact("admin-hr-dashboard", SsalddelPageAppCodes.Admin, "/dashboard", PageCapabilityStage.Preparing,
            PageInteractionBoundary.Simulation, true, "2.5", "역할·계약·4대보험 신고 준비를 실제 외부 신고와 분리해 검증합니다.", true,
            [Hr], ["HrParticipation"]),
        Exact("human-resources-role-reviews", SsalddelPageAppCodes.HumanResources, "/", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "2.5",
            "서버관리자가 영속 HR 역할 지원·철회와 배정·해제 원장을 검색하고 정확한 검토 ID 상세를 조회합니다.",
            featureKeys: [Hr], workflowCodes: ["HrParticipation"]),
        Exact("orderer-food-restaurants", SsalddelPageAppCodes.Orderer, "/food/restaurants", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, false, "3.0",
            "사용자가 선택한 공개 행정권역 기준점과 반경으로 영속된 공개 음식점·메뉴를 조회합니다.",
            featureKeys: [Food], workflowCodes: ["FoodDelivery"]),
        Exact("orderer-food-orders", SsalddelPageAppCodes.Orderer, "/orders", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, true, "3.0",
            "로그인한 주문자가 소유한 영속 음식 주문 목록과 정확한 주문번호 상세만 조회합니다.", false,
            [Food], ["FoodDelivery"]),
        Exact("orderer-mart", SsalddelPageAppCodes.Orderer, "/food/mart", PageCapabilityStage.Beta,
            PageInteractionBoundary.ReadOnly, false, "3.5",
            "내부 창고 원장과 분리해 영속된 공개 상품·판매 가능 수량 투영을 조회하며 장바구니·주문·피킹·배송을 실행하지 않습니다.", false,
            [Mart], ["SsalddelMart"]),
        Exact("orderer-mart-order-request", SsalddelPageAppCodes.Orderer, "/food/mart/order", PageCapabilityStage.Beta,
            PageInteractionBoundary.PlatformPersistence, true, "3.5",
            "한 공개 상품의 비구속 주문 요청을 멱등 저장하고 같은 ID를 다시 조회하며 재고·결제·출고 원장은 변경하지 않습니다.", false,
            [Mart], ["SsalddelMart"])
    ];

    public static IReadOnlyList<SsalddelPageCapabilityRule> GetAll() => Items;

    public static bool TryResolve(string appCode, string? route, out SsalddelPageCapabilityRule rule)
    {
        var normalizedRoute = NormalizeRoute(route);
        var match = Items
            .Where(item => string.Equals(item.AppCode, appCode, StringComparison.Ordinal))
            .Where(item => Matches(item, normalizedRoute))
            .OrderBy(item => item.MatchKind == PageCapabilityMatchKind.Exact ? 0 : 1)
            .ThenByDescending(item => item.RoutePattern.Length)
            .FirstOrDefault();

        if (match is null)
        {
            rule = default!;
            return false;
        }

        rule = match;
        return true;
    }

    private static bool Matches(SsalddelPageCapabilityRule rule, string route)
        => rule.MatchKind == PageCapabilityMatchKind.Exact
            ? string.Equals(rule.RoutePattern, route, StringComparison.OrdinalIgnoreCase)
            : string.Equals(rule.RoutePattern, route, StringComparison.OrdinalIgnoreCase)
              || route.StartsWith($"{rule.RoutePattern}/", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeRoute(string? route)
    {
        var value = route?.Trim() ?? string.Empty;
        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
        {
            value = absoluteUri.AbsolutePath;
        }

        value = value.Split('?', '#')[0].Trim();
        if (string.IsNullOrEmpty(value))
        {
            return "/";
        }

        value = $"/{value.Trim('/')}";
        return value.Length == 1 ? value : value.TrimEnd('/');
    }

    private static SsalddelPageCapabilityRule Exact(
        string pageKey,
        string appCode,
        string route,
        PageCapabilityStage stage,
        PageInteractionBoundary boundary,
        bool requiresAuthentication,
        string introducedVersion,
        string notice,
        bool hasExternalEffects = false,
        string[]? featureKeys = null,
        string[]? workflowCodes = null)
        => Rule(pageKey, appCode, route, PageCapabilityMatchKind.Exact, stage, boundary,
            requiresAuthentication, introducedVersion, notice, hasExternalEffects, featureKeys, workflowCodes);

    private static SsalddelPageCapabilityRule Prefix(
        string pageKey,
        string appCode,
        string route,
        PageCapabilityStage stage,
        PageInteractionBoundary boundary,
        bool requiresAuthentication,
        string introducedVersion,
        string notice,
        bool hasExternalEffects = false,
        string[]? featureKeys = null,
        string[]? workflowCodes = null)
        => Rule(pageKey, appCode, route, PageCapabilityMatchKind.Prefix, stage, boundary,
            requiresAuthentication, introducedVersion, notice, hasExternalEffects, featureKeys, workflowCodes);

    private static SsalddelPageCapabilityRule Rule(
        string pageKey,
        string appCode,
        string route,
        PageCapabilityMatchKind matchKind,
        PageCapabilityStage stage,
        PageInteractionBoundary boundary,
        bool requiresAuthentication,
        string introducedVersion,
        string notice,
        bool hasExternalEffects,
        string[]? featureKeys,
        string[]? workflowCodes)
        => new(
            pageKey,
            appCode,
            NormalizeRoute(route),
            matchKind,
            stage,
            boundary,
            requiresAuthentication,
            hasExternalEffects,
            introducedVersion,
            featureKeys ?? [],
            workflowCodes ?? [],
            notice);
}
