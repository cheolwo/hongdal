using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Contracts.Common.Content;

public sealed record CultureTransportContentPillar(
    string Key,
    string Version,
    string DisplayName,
    string WorkflowTag,
    string Purpose,
    IReadOnlyList<string> RequiredEvidence,
    string PublicationBoundary);

public static class CultureTransportContentCatalog
{
    public const string ProductName = SsalddelProductRoadmapCatalog.CultureTransportName;

    public const string CultureStoryKey = "culture-story";
    public const string PriceEvidenceKey = "price-evidence";
    public const string IndividualOrderIntentKey = "individual-order-intent";
    public const string SharedDemandKey = "shared-demand";
    public const string RouteReadinessKey = "route-readiness";

    public const string FoodCultureWorkflowTag = "문화교통 · 식문화";
    public const string PriceEvidenceWorkflowTag = "문화교통 · 가격 근거";
    public const string IndividualOrderIntentWorkflowTag = "문화교통 · 내 주문 의향";
    public const string SharedDemandWorkflowTag = "문화교통 · 함께 구하기";
    public const string RouteReadinessWorkflowTag = "문화교통 · 이동 준비";

    public static IReadOnlyList<CultureTransportContentPillar> All { get; } =
    [
        new(
            CultureStoryKey,
            SsalddelProductRoadmapCatalog.FoundationVersion,
            "음식과 사람의 문화",
            FoodCultureWorkflowTag,
            "공식 음식 메타데이터를 출발점으로 먹는 때, 관계, 지역 차이와 번역에서 빠지기 쉬운 맥락을 묻습니다.",
            ["공식 원문 링크", "제공기관", "자료 확인 시각", "권리 확인 상태"],
            "원문 레시피를 복제하거나 한 사람의 경험을 국가 전체로 일반화하지 않습니다."),
        new(
            PriceEvidenceKey,
            SsalddelProductRoadmapCatalog.FoundationVersion,
            "재료와 가격의 근거",
            PriceEvidenceWorkflowTag,
            "KAMIS와 USDA 같은 공공자료의 기준일, 지역, 시장 단계, 단위와 통화를 보존해 재료의 현재 맥락을 설명합니다.",
            ["원천", "기준일 또는 기준월", "지역", "시장 단계", "단위", "통화"],
            "관측값을 판매 권고, 확정 공동구매가 또는 서로 다른 시장의 직접 비교값으로 표현하지 않습니다."),
        new(
            IndividualOrderIntentKey,
            SsalddelProductRoadmapCatalog.IndividualOrderVersion,
            "내가 구하려는 것",
            IndividualOrderIntentWorkflowTag,
            "공개 근거에서 고른 상품의 수량, 수령 권역, 시간·보관 조건과 철회 가능한 주문 의향을 한 사람의 개별 원장으로 설명합니다.",
            ["상품·재료 근거", "희망 수량", "수령 권역", "시간·보관 조건", "철회 가능 상태"],
            "개별 원장을 결제·계약·배송 확정으로 표현하거나 같이 주문에 자동 포함하지 않습니다."),
        new(
            SharedDemandKey,
            SsalddelProductRoadmapCatalog.GroupPurchaseVersion,
            "함께 구하려는 마음",
            SharedDemandWorkflowTag,
            "같이 주문 참여에 동의한 개별주문의 공통 품목, 수령 권역, 시간창과 역할을 집계해 같이 주문 후보를 설명합니다.",
            ["포함된 개별 원장 수", "집계 수량", "수령 권역", "시간·보관 조건", "공동 참여 동의 상태"],
            "개별 원장을 임의로 합치거나 같이 주문 참여를 결제·계약으로 해석하지 않습니다."),
        new(
            RouteReadinessKey,
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            "재료가 이동하기 전의 준비",
            RouteReadinessWorkflowTag,
            "공급 근거, 포장·수량, 가격 기준, HS·HTS 후보와 포워더에게 확인할 질문으로 재료의 이동 준비를 설명합니다.",
            ["공급 근거", "포장·수량", "가격 기준", "HS·HTS 후보", "미확인 항목", "사람의 인계 대상"],
            "수입 가능성, 품목분류, 계약, 신고, 운송사 선정이나 선적을 자동 확정하지 않습니다.")
    ];

    public static CultureTransportContentPillar Find(string? key)
        => All.FirstOrDefault(item => string.Equals(
               item.Key,
               key?.Trim(),
               StringComparison.OrdinalIgnoreCase))
           ?? All[0];
}
