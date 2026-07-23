using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Contracts.Common.Community;

public sealed record CommunityWorkBoardEditorialPlan(
    string BoardKey,
    IReadOnlyList<string> Topics,
    IReadOnlyList<string> ExecutableSourceKeys,
    IReadOnlyList<string> PlannedOfficialSources,
    string Cadence,
    bool RequiresEditorialReview);

/// <summary>
/// 정보 수집·편집 배치를 버전이 아니라 업무 게시판 key에 연결하는 기준입니다.
/// PlannedOfficialSources는 connector 구현 전 운영 후보이며 자동 fallback으로 사용하지 않습니다.
/// </summary>
public static class CommunityWorkBoardEditorialPlanCatalog
{
    private static readonly string[] PriceSources =
    [
        CommunityInformationSourceKeys.KamisPriceObservations,
        CommunityInformationSourceKeys.UsdaNassPriceObservations
    ];

    private static readonly string[] RecipeSources =
    [
        CommunityInformationSourceKeys.MfdsCookRecipes,
        CommunityInformationSourceKeys.RdaLocalFoodRecipes,
        CommunityInformationSourceKeys.MaffRegionalCuisineRecipes,
        CommunityInformationSourceKeys.NhsHealthierFamiliesRecipes
    ];

    public static IReadOnlyList<CommunityWorkBoardEditorialPlan> All { get; } =
    [
        Plan(CommunityActivityBoardKeys.FoundationEvidence,
            ["농수산물 가격", "공식 음식·재료", "공공데이터 읽기"],
            PriceSources.Concat(RecipeSources),
            ["KAMIS", "USDA NASS", "식품의약품안전처"],
            "매일"),
        Plan(CommunityActivityBoardKeys.IndividualDemand,
            ["식재료 가격 변화", "다품목 공동구매", "개별 원함"],
            PriceSources,
            ["KAMIS", "USDA NASS"],
            "매일"),
        Plan(CommunityActivityBoardKeys.CollectiveLedger,
            ["공동구매 집단화", "공동 원장 운영", "B2B·B2C 수요"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["통계청 온라인쇼핑동향"],
            "주 2회"),
        Plan(CommunityActivityBoardKeys.HsClassification,
            ["HS 품목분류", "식품 수입요건", "관세율표"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["관세청 품목분류 공개자료", "관세법령정보포털"],
            "주 1회"),
        Plan(CommunityActivityBoardKeys.CustomsDelegation,
            ["통관 의뢰", "관세사 수임", "전자문서 동의"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["관세청 전자통관 안내"],
            "주 1회"),
        Plan(CommunityActivityBoardKeys.CustomsProcess,
            ["수입신고", "검사", "관세 납부", "반출"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["관세청 수출입 통관 데이터", "식품안전나라 수입식품 정보"],
            "매일"),
        Plan(CommunityActivityBoardKeys.TransportRequest,
            ["화물 운송 의뢰", "운송 조건", "적재 제약"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["국가물류통합정보센터 물류통계"],
            "주 2회"),
        Plan(CommunityActivityBoardKeys.DispatchDecision,
            ["배차 의사결정", "기사 참여", "운송 안전"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["국가물류통합정보센터 물류통계"],
            "주 2회"),
        Plan(CommunityActivityBoardKeys.LoadingJourney,
            ["상차 안전", "화물 고정", "운행 점검"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["한국산업안전보건공단 화물운송 안전자료"],
            "주 1회"),
        Plan(CommunityActivityBoardKeys.DeliveryHandover,
            ["하차 안전", "인수 증빙", "화물 손상 확인"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["한국산업안전보건공단 하역 안전자료"],
            "주 1회"),
        Plan(CommunityActivityBoardKeys.SellerWarehouseReceipt,
            ["판매자 출고", "주문자 입고", "검수 기준"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["국가물류통합정보센터 생활물류 통계"],
            "주 1회"),
        Plan(CommunityActivityBoardKeys.WarehouseInbound,
            ["창고 입고", "수량·상태 검수", "적재 위치"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["한국산업안전보건공단 창고 안전자료"],
            "주 1회"),
        Plan(CommunityActivityBoardKeys.PickingHandover,
            ["피킹 정확도", "포장", "출고 인계"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["국가물류통합정보센터 물류시설 통계"],
            "주 1회"),
        Plan(CommunityActivityBoardKeys.FoodOrderAcceptance,
            ["공식 음식 레시피", "식재료 수요", "음식점 주문 운영"],
            RecipeSources.Concat(PriceSources),
            ["식품의약품안전처", "농촌진흥청"],
            "매일"),
        Plan(CommunityActivityBoardKeys.FoodDeliveryHandoff,
            ["조리 완료 인계", "배달 픽업", "식품 온도 관리"],
            [CommunityInformationSourceKeys.YouTubeChannelVideos],
            ["식품안전나라 배달음식 안전정보"],
            "주 1회"),
        Plan(CommunityActivityBoardKeys.MartFulfillment,
            ["마트 가격", "피킹·포장", "즉시배송"],
            PriceSources.Append(CommunityInformationSourceKeys.YouTubeChannelVideos),
            ["KAMIS", "국가물류통합정보센터 생활물류 통계"],
            "매일")
    ];

    public static CommunityWorkBoardEditorialPlan Find(string boardKey)
        => All.First(plan => string.Equals(plan.BoardKey, boardKey, StringComparison.OrdinalIgnoreCase));

    private static CommunityWorkBoardEditorialPlan Plan(
        string boardKey,
        IEnumerable<string> topics,
        IEnumerable<string> executableSourceKeys,
        IEnumerable<string> plannedOfficialSources,
        string cadence)
        => new(
            boardKey,
            topics.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            executableSourceKeys.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            plannedOfficialSources.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            cadence,
            RequiresEditorialReview: true);
}
