using Ssalddel.Contracts.Common.Versioning;

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

public sealed record CommunityActivityBoardDefinition(
    string SourceKind,
    string SourceName,
    string ProductVersion,
    string PublicActivitySummary,
    CommunityBoardDefinition Board)
{
    public SsalddelProductRoadmapStage RoadmapStage
        => SsalddelProductRoadmapCatalog.Find(ProductVersion);

    public string SourceKindDisplayName
        => CommunityActivitySourceKinds.DisplayName(SourceKind);
}

public static class CommunityActivityBoardCatalog
{
    public const string SurfaceMappingBoundary =
        "관련 App·페이지는 2.0→2.5→3.0→3.5 단일책임 페이지화가 완료된 뒤 확정합니다.";

    public const string PrivacyBoundary =
        "사용자·업체 식별자, 연락처, 상세 주소, 위치, 금액, 결제 정보, 첨부와 원본 payload는 공개하지 않습니다.";

    public static IReadOnlyList<CommunityActivityBoardDefinition> All { get; } =
    [
        Command(
            "콘텐츠시청완료Command",
            SsalddelProductRoadmapCatalog.FoundationVersion,
            "activity-content-watch-completed",
            "공개 콘텐츠 학습 완료",
            "한 이웃이 공개 콘텐츠 학습 흐름을 완료했습니다."),

        Event(
            "커뮤니티원장변경됨Event",
            SsalddelProductRoadmapCatalog.GroupPurchaseVersion,
            "activity-collective-ledger-changed",
            "공동 원장 변경",
            "공동구매·공동수입 원장의 공개 가능한 진행 단계가 변경되었습니다."),

        Command(
            "화주HsCode검토요청Command",
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            "activity-hs-review-requested",
            "HS 코드 검토 요청",
            "무역 준비 과정에서 HS 코드 검토 요청이 접수되었습니다."),
        Event(
            "화주통관의뢰등록됨Event",
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            "activity-customs-request-created",
            "통관 의뢰 등록",
            "공동수입 준비 과정에서 통관 의뢰가 등록되었습니다."),
        Event(
            "통관조회동의등록됨Event",
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            "activity-customs-consent-recorded",
            "통관 조회 동의",
            "통관 상태 조회를 위한 명시적 동의가 기록되었습니다."),
        Event(
            "통관수임요청됨Event",
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            "activity-customs-agent-requested",
            "통관 수임 요청",
            "통관 절차를 맡을 전문 역할에 수임 요청이 전달되었습니다."),
        Event(
            "통관절차생성됨Event",
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            "activity-customs-process-created",
            "통관 절차 생성",
            "수출입 이행을 위한 통관 절차가 생성되었습니다."),
        Event(
            "통관상태변경감지됨Event",
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            "activity-customs-status-changed",
            "통관 상태 변경",
            "통관 절차의 상태 변경이 확인되었습니다."),

        Command(
            "의뢰생성Command",
            SsalddelProductRoadmapCatalog.TransportVersion,
            "activity-transport-request-created",
            "운송 의뢰 등록",
            "국내 화물 이행을 위한 운송 의뢰가 등록되었습니다."),
        Event(
            "배차수락됨Event",
            SsalddelProductRoadmapCatalog.TransportVersion,
            "activity-dispatch-accepted",
            "배차 수락",
            "운송 의뢰의 배차가 수락되었습니다."),
        Event(
            "배차거절됨Event",
            SsalddelProductRoadmapCatalog.TransportVersion,
            "activity-dispatch-declined",
            "배차 거절",
            "운송 의뢰의 배차가 거절되어 다음 배차를 기다립니다."),
        Event(
            "배차수락취소됨Event",
            SsalddelProductRoadmapCatalog.TransportVersion,
            "activity-dispatch-acceptance-cancelled",
            "배차 수락 취소",
            "수락했던 배차가 취소되어 운송 배정 상태가 변경되었습니다."),
        Event(
            "운송상차지도착됨Event",
            SsalddelProductRoadmapCatalog.TransportVersion,
            "activity-pickup-arrived",
            "상차지 도착",
            "배차된 운송이 상차지 도착 단계에 들어갔습니다."),
        Event(
            "운송상차완료됨Event",
            SsalddelProductRoadmapCatalog.TransportVersion,
            "activity-loading-completed",
            "상차 완료",
            "배차된 운송의 상차 단계가 완료되었습니다."),
        Event(
            "운송하차지도착됨Event",
            SsalddelProductRoadmapCatalog.TransportVersion,
            "activity-dropoff-arrived",
            "하차지 도착",
            "운송이 하차지 도착 단계에 들어갔습니다."),
        Event(
            "운송인수완료됨Event",
            SsalddelProductRoadmapCatalog.TransportVersion,
            "activity-transport-handover-completed",
            "운송 인수 완료",
            "운송 물품의 인수 확인이 완료되었습니다."),

        Event(
            "판매자상품출고됨Event",
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            "activity-seller-shipment-released",
            "판매자 출고",
            "판매자가 물류 이행을 위해 상품을 출고했습니다."),
        Event(
            "주문자상품입고완료됨Event",
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            "activity-orderer-receipt-completed",
            "주문자 입고 확인",
            "주문자가 상품 입고 완료를 확인했습니다."),
        Event(
            "창고입고완료됨Event",
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            "activity-warehouse-inbound-completed",
            "창고 입고 완료",
            "창고 입고 등록이 완료되었습니다."),
        Event(
            "창고입고검수완료됨Event",
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            "activity-warehouse-inspection-completed",
            "입고 검수 완료",
            "입고 상품의 수량·상태 검수가 완료되었습니다."),
        Event(
            "창고적재위치배정됨Event",
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            "activity-warehouse-location-assigned",
            "적재 위치 배정",
            "입고 상품의 창고 적재 위치가 배정되었습니다."),
        Event(
            "창고출고인계준비완료됨Event",
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            "activity-warehouse-handover-ready",
            "출고 인계 준비",
            "창고에서 다음 운송으로 인계할 출고 준비가 완료되었습니다."),
        Event(
            "창고재위탁운송생성됨Event",
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            "activity-warehouse-transport-created",
            "재위탁 운송 생성",
            "창고 이행 뒤 이어질 재위탁 운송 의뢰가 생성되었습니다."),

        Event(
            "음식주문등록됨Event",
            SsalddelProductRoadmapCatalog.FoodDeliveryVersion,
            "activity-food-order-created",
            "음식 주문 등록",
            "음식점 배달 흐름에 새 주문이 등록되었습니다."),
        Event(
            "음식점주문수락됨Event",
            SsalddelProductRoadmapCatalog.FoodDeliveryVersion,
            "activity-restaurant-order-accepted",
            "음식점 주문 수락",
            "음식점이 주문을 수락해 조리·배달 준비가 시작되었습니다."),

        Event(
            "주문결제완료됨Event",
            SsalddelProductRoadmapCatalog.MartVersion,
            "activity-mart-order-paid",
            "마트 주문 결제",
            "마트 주문의 결제 완료 사실이 물류 흐름에 전달되었습니다."),
        Event(
            "창고피킹완료됨Event",
            SsalddelProductRoadmapCatalog.MartVersion,
            "activity-mart-picking-completed",
            "상품 피킹 완료",
            "주문 상품의 피킹 작업이 완료되었습니다."),
        Event(
            "창고포장완료됨Event",
            SsalddelProductRoadmapCatalog.MartVersion,
            "activity-mart-packing-completed",
            "상품 포장 완료",
            "피킹된 주문 상품의 포장 작업이 완료되었습니다.")
    ];

    public static IReadOnlyList<CommunityBoardDefinition> Boards { get; } =
        All.Select(definition => definition.Board).ToArray();

    public static CommunityActivityBoardDefinition? FindSource(
        string? sourceKind,
        string? sourceName)
        => string.IsNullOrWhiteSpace(sourceName)
            ? null
            : All.FirstOrDefault(definition =>
                string.Equals(definition.SourceKind, sourceKind?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(definition.SourceName, sourceName.Trim(), StringComparison.Ordinal));

    public static CommunityActivityBoardDefinition? FindBoard(string? boardKeyOrName)
        => string.IsNullOrWhiteSpace(boardKeyOrName)
            ? null
            : All.FirstOrDefault(definition =>
                string.Equals(definition.Board.Key, boardKeyOrName.Trim(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    definition.Board.DisplayName,
                    boardKeyOrName.Trim(),
                    StringComparison.OrdinalIgnoreCase));

    public static bool IsActivityBoard(string? boardKeyOrName)
        => FindBoard(boardKeyOrName) is not null;

    private static CommunityActivityBoardDefinition Command(
        string sourceName,
        string productVersion,
        string boardKey,
        string displayName,
        string publicActivitySummary)
        => Create(
            CommunityActivitySourceKinds.Command,
            sourceName,
            productVersion,
            boardKey,
            displayName,
            publicActivitySummary);

    private static CommunityActivityBoardDefinition Event(
        string sourceName,
        string productVersion,
        string boardKey,
        string displayName,
        string publicActivitySummary)
        => Create(
            CommunityActivitySourceKinds.Event,
            sourceName,
            productVersion,
            boardKey,
            displayName,
            publicActivitySummary);

    private static CommunityActivityBoardDefinition Create(
        string sourceKind,
        string sourceName,
        string productVersion,
        string boardKey,
        string displayName,
        string publicActivitySummary)
    {
        var roadmapStage = SsalddelProductRoadmapCatalog.Find(productVersion);
        return new CommunityActivityBoardDefinition(
            sourceKind,
            sourceName,
            productVersion,
            publicActivitySummary,
            new CommunityBoardDefinition(
                boardKey,
                displayName,
                $"{publicActivitySummary} 원본 업무 정보는 공개하지 않고 발생 사실만 자동 기록합니다.",
                $"activity-{productVersion.Replace('.', '-')}",
                roadmapStage.FullDisplayName,
                IsUserCreatable: false,
                IsPublic: true,
                PostingAccessCode: CommunityBoardPostingAccessCodes.OperatorOnly,
                LegacyCategoryNames: []));
    }

}
