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
    public const string SharedDemandKey = "shared-demand";
    public const string RouteReadinessKey = "route-readiness";

    public const string FoodCultureWorkflowTag = "문화교통 · 식문화";
    public const string PriceEvidenceWorkflowTag = "문화교통 · 가격 근거";
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
            SharedDemandKey,
            SsalddelProductRoadmapCatalog.GroupPurchaseVersion,
            "함께 구하려는 마음",
            SharedDemandWorkflowTag,
            "문화와 재료에 대한 관심이 생겼을 때 비구속 수요, 여러 재료, 수령 권역과 역할을 공개적으로 설명합니다.",
            ["품목", "희망 수량", "수령 권역", "시간·보관 조건", "철회 가능 상태"],
            "글을 주문·결제·계약으로 해석하지 않고 참여자를 자동 가입시키지 않습니다."),
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
