using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Customs;

namespace Ssalddel.Contracts.Common.Community;

public static class CommunityBoardInformationConnectorStatuses
{
    public const string Implemented = "implemented";
    public const string ImplementedProjection = "implemented-projection";
    public const string Planned = "planned";
}

public static class CommunityBoardInformationBatchStatuses
{
    public const string Scheduled = "scheduled";
    public const string ReadyToSchedule = "ready-to-schedule";
    public const string OnDemand = "on-demand";
    public const string NotApplicable = "not-applicable";
    public const string Planned = "planned";
}

public static class CommunityBoardInformationPublicationPolicies
{
    public const string AutomatedBrief = "automated-brief";
    public const string EditorialReview = "editorial-review";
    public const string ReferenceOnly = "reference-only";
    public const string InternalOnly = "internal-only";
    public const string NoAutomaticPublication = "no-automatic-publication";
}

public static class CommunityBoardInformationBatchModuleKeys
{
    public const string AgriculturalFisheries = "agricultural-fisheries-public-data";
    public const string IngredientCompanyResearch = "official-food-ingredient-company-research";
    public const string OfficialFoodRecipeArchive = "official-food-recipe-archive";
    public const string CommunityEditorial = "community-editorial-publication";
    public const string CustomsPublicDataQuery = "customs-public-data-query";
    public const string TraditionalMarketSync = "traditional-market-sync";
    public const string PublicDataQuery = "public-data-query";
    public const string CommunityActivityProjection = "community-activity-projection";
    public const string PlannedConnector = "planned-connector";
}

public static class CommunityBoardInformationSourceKeys
{
    public const string TraditionalMarketStatus = "semas-traditional-market-status";
    public const string MfdsDomesticIngredientProducts = "mfds-domestic-product-ingredient-report";
    public const string MfdsImportedFoodLabels = "mfds-imported-food-korean-label";
    public const string MfdsOverseasManufacturers = "mfds-overseas-manufacturer";
    public const string CustomsCargoTracking = "customs-cargo-tracking";
    public const string RoadAddressSearch = "juso-road-address-search";
    public const string ApartmentComplexList = "kapt-apartment-complex-list";
    public const string HongikHakdangCards = "hongik-hakdang-card-archive";
    public const string CommunityActivityProjection = "community-activity-event-projection";
    public const string Reflection = "reflection";
    public const string ActivityDigest = "activity-digest";

    public const string PlannedOnlineShoppingStatistics =
        "planned-statistics-korea-online-shopping";
    public const string PlannedCustomsElectronicClearance =
        "planned-customs-electronic-clearance";
    public const string PlannedNationalLogisticsStatistics =
        "planned-national-logistics-statistics";
    public const string PlannedCargoSafetyGuidance =
        "planned-kosha-cargo-safety-guidance";
    public const string PlannedDeliveryFoodSafety =
        "planned-mfds-delivery-food-safety";
}

public static class CommunityBoardInformationPublicationSourceKeys
{
    public const string KamisPriceBrief = "kamis-price-brief";
    public const string UsdaNassPriceBrief = "usda-nass-price-brief";
    public const string ChinaImportedFoodRegionBrief = "china-imported-food-region-brief";
    public const string UnitedStatesImportedFoodStateBrief = "us-imported-food-state-brief";
    public const string WeeklyCountryProductComparison =
        "weekly-country-product-comparison";
    public const string CultureTransport = "culture-transport";
    public const string PrajnaCard = "prajna-card";
    public const string Reflection = "reflection";
    public const string ActivityDigest = "activity-digest";
}

public sealed record CommunityBoardInformationSourceRelation(
    string SourceKey,
    string Provider,
    string DisplayName,
    string SourceType,
    string ConnectorStatus,
    string BatchStatus,
    string UpdateCycle,
    string BatchModuleKey,
    string PublicationPolicy,
    string Purpose,
    string Limitations,
    IReadOnlyList<string> PublicationSourceKeys)
{
    public bool IsConnectorImplemented
        => ConnectorStatus is CommunityBoardInformationConnectorStatuses.Implemented
            or CommunityBoardInformationConnectorStatuses.ImplementedProjection;

    public bool HasPeriodicBatchModule
        => BatchStatus == CommunityBoardInformationBatchStatuses.Scheduled;

    public bool AllowsAutomaticPublication
        => PublicationPolicy == CommunityBoardInformationPublicationPolicies.AutomatedBrief;
}

public sealed record CommunityBoardInformationRelation(
    string BoardKey,
    string BoardDisplayName,
    IReadOnlyList<string> Topics,
    string PreferredCadence,
    IReadOnlyList<CommunityBoardInformationSourceRelation> Sources,
    string AutomationBoundary);

public sealed record CommunityBoardInformationBatchPlan(
    string SourceKey,
    string BatchModuleKey,
    string UpdateCycle,
    string? CanonicalBoardKey,
    IReadOnlyList<string> BoardKeys,
    IReadOnlyList<string> PublicationSourceKeys,
    bool AllowsAutomaticPublication,
    bool RequiresEditorialReview,
    bool RequiresExplicitActivation);

/// <summary>
/// 서버에 구현된 공공데이터·공식 자료·내부 공개 투영과 게시판의 관계를 안정 key로 연결합니다.
/// 이 카탈로그는 배치 등록 후보를 찾는 기준이며, 관계 자체가 자동 게시 승인을 뜻하지 않습니다.
/// </summary>
public static class CommunityBoardInformationRelationCatalog
{
    private const string DefaultEditorialBoundary =
        "자료를 후보로 수집해도 출처·기준시각·단위·지역·한계를 확인한 뒤 편집합니다.";
    private const string NoRecruitmentAutomationBoundary =
        "가격·공공자료는 참고 근거로만 표시하며 모집·판매·계약·상대 선택을 자동 확정하지 않습니다.";
    private const string NoPublicDataBoundary =
        "현재 연결할 공공데이터 배치가 없습니다. 사용자 글이나 보호 기록을 자료처럼 수집하지 않습니다.";

    private sealed record SourceDefinition(
        string SourceKey,
        string Provider,
        string DisplayName,
        string SourceType,
        string ConnectorStatus,
        string BatchStatus,
        string UpdateCycle,
        string BatchModuleKey,
        string Limitations,
        IReadOnlyList<string> PublicationSourceKeys);

    private static readonly IReadOnlyDictionary<string, SourceDefinition> Sources =
        CreateSourceDefinitions();

    public static IReadOnlyList<CommunityBoardInformationRelation> All { get; } =
    [
        Board(
            CommunityBoardKeys.Vow,
            ["개인·이웃의 바람", "공동 관심 발견"],
            "해당 없음",
            NoPublicDataBoundary),
        Board(
            CommunityBoardKeys.FreeLife,
            ["생활 정보", "살뜰 운영 성찰"],
            "주 2회",
            DefaultEditorialBoundary,
            Link(
                CommunityBoardInformationSourceKeys.Reflection,
                CommunityBoardInformationPublicationPolicies.AutomatedBrief,
                "살뜰의 공개·합의·기록 원칙을 짧은 시스템 성찰문으로 제공합니다.")),
        Board(
            CommunityBoardKeys.QuestionHelp,
            ["공공자료를 활용한 질문 답변"],
            "해당 없음",
            "현재 범용 질문 게시판에 직접 연결한 공공데이터 원천은 없습니다. 사용자 질문을 조사 배치 입력으로 자동 수집하지 않습니다."),
        Board(
            CommunityBoardKeys.InformationPrices,
            ["국내외 농수산물 가격", "수입식품 지역 근거", "HS·관세", "전통시장"],
            "매일·매월",
            "자동 요약은 명시된 원천만 허용하며 가격 권고, 원산지 확정, 통관 가능 판정으로 쓰지 않습니다.",
            Link(
                CommunityInformationSourceKeys.KamisPriceObservations,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "KAMIS 전용 원천 게시판에 한 번만 저장된 관측가격을 대표 안내로 연결합니다."),
            Link(
                CommunityInformationSourceKeys.UsdaNassPriceObservations,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "USDA 전용 원천 게시판에 한 번만 저장된 생산자 가격을 대표 안내로 연결합니다."),
            Link(
                CommunityInformationSourceKeys.AbsFoodPriceIndex,
                CommunityBoardInformationPublicationPolicies.EditorialReview,
                "호주 식품 소비자물가지수를 필요 시 비교 근거로 조회합니다."),
            Link(
                CommunityInformationSourceKeys.FishCooperativeGeneralStatistics,
                CommunityBoardInformationPublicationPolicies.EditorialReview,
                "수협 일반현황 통계를 수산 정보의 보조 근거로 조회합니다."),
            Link(
                CommunityBoardInformationSourceKeys.TraditionalMarketStatus,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "전통시장 코드·지역·공동물류시설 기준정보를 보강합니다."),
            Link(
                CommunityBoardInformationSourceKeys.MfdsImportedFoodLabels,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "MFDS 전용 원천 게시판의 중국 권역·미국 주별 누적을 대표 안내로 연결합니다."),
            Link(
                CommunityBoardInformationSourceKeys.MfdsOverseasManufacturers,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "MFDS 전용 원천 게시판의 해외제조업소 대조 근거를 대표 안내로 연결합니다."),
            Link(
                Hs공공데이터출처Keys.수입평균단가,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "HS 코드·국가·기간별 수입금액과 순중량으로 CIF 참고단가를 조회합니다."),
            Link(
                Hs공공데이터출처Keys.세관장확인대상물품,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "HSK별 확인 법령·승인기관·구비요건을 필요 시 조회합니다."),
            Link(
                Hs공공데이터출처Keys.관세환율,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "수입신고 과세가격 계산에 쓰는 주간 관세환율을 조회합니다.")),
        Board(
            CommunityBoardKeys.PeriodicDataKamis,
            ["KAMIS 농수산물 가격 원천", "조사일·품목·등급·단위"],
            "일별·월별",
            "KAMIS 정기 글의 단일 저장 위치입니다. 관련 게시판에는 글을 복제하지 않고 대표 안내 link만 표시합니다.",
            Link(
                CommunityInformationSourceKeys.KamisPriceObservations,
                CommunityBoardInformationPublicationPolicies.AutomatedBrief,
                "국내 농수산물 관측가격 정기 글을 이 게시판에 한 번만 누적합니다.")),
        Board(
            CommunityBoardKeys.PeriodicDataMfds,
            ["MFDS 수입식품·해외제조업소 원천", "중국 권역·미국 주별 근거"],
            "주별 조사·월별 게시 후보",
            "MFDS 정기 글의 단일 저장 위치입니다. 제조업소 소재지를 원재료 원산지로 확정하지 않습니다.",
            Link(
                CommunityBoardInformationSourceKeys.MfdsDomesticIngredientProducts,
                CommunityBoardInformationPublicationPolicies.EditorialReview,
                "국내 품목제조보고 원재료를 식재료·업체 근거 후보로 검토합니다."),
            Link(
                CommunityBoardInformationSourceKeys.MfdsImportedFoodLabels,
                CommunityBoardInformationPublicationPolicies.AutomatedBrief,
                "중국 권역과 미국 주별 제조업소 근거를 월별 정기 글로 한 번만 누적합니다."),
            Link(
                CommunityBoardInformationSourceKeys.MfdsOverseasManufacturers,
                CommunityBoardInformationPublicationPolicies.AutomatedBrief,
                "표시자료의 제조업소 이름·국가를 공식 시설 자료로 보조 대조합니다.")),
        Board(
            CommunityBoardKeys.PeriodicDataUsda,
            ["USDA NASS 생산자가격 원천", "기준월·품목·원 단위"],
            "월별",
            "USDA 정기 글의 단일 저장 위치입니다. 미국 생산자 가격을 국내 소매가나 개별 견적으로 해석하지 않습니다.",
            Link(
                CommunityInformationSourceKeys.UsdaNassPriceObservations,
                CommunityBoardInformationPublicationPolicies.AutomatedBrief,
                "미국 전국 생산자 수취가격 정기 글을 이 게시판에 한 번만 누적합니다.")),
        Board(
            CommunityBoardKeys.PeriodicDataCustomsImportUnitPrice,
            ["관세청 품목·국가별 수입 평균단가", "CIF 참고단가"],
            "요청 시·주기화 후보",
            "관세청 수입단가 글의 단일 저장 위치입니다. 세금·검역·통관·국내 물류비와 판매마진을 포함하지 않습니다.",
            Link(
                Hs공공데이터출처Keys.수입평균단가,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "품목·국가·기간별 수입금액과 순중량으로 CIF 참고단가를 조회·누적합니다.")),
        Board(
            CommunityBoardKeys.Food,
            ["공식 음식·재료", "조리와 문화 맥락", "식재료 가격 참고"],
            "주 2회",
            "공식 음식 자료는 승인된 메타데이터만 게시하고 원문 레시피를 복제하거나 국가 전체를 대표한다고 표현하지 않습니다.",
            RecipeLinks()
                .Append(Link(
                    CommunityInformationSourceKeys.KamisPriceObservations,
                    CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                    "식재료 가격 힌트의 국내 참고값으로 사용합니다."))
                .Append(Link(
                    CommunityBoardInformationSourceKeys.MfdsDomesticIngredientProducts,
                    CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                    "국내 품목제조보고 원재료와 제조업체 후보를 음식 재료 근거로 연결합니다."))
                .ToArray()),
        Board(
            CommunityBoardKeys.Cargo,
            ["통관 진행 참고", "HS 수입단가", "물류·안전 통계"],
            "필요 시·주 1회 후보",
            "조회 결과는 비구속 참고정보이며 자동 배차·운송계약·유상 추천·운임 확정에 사용하지 않습니다.",
            Link(
                CommunityBoardInformationSourceKeys.CustomsCargoTracking,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "사용자가 지정한 화물의 통관 진행상태를 필요 시 조회합니다."),
            Link(
                Hs공공데이터출처Keys.수입평균단가,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "품목·국가별 수입 규모와 CIF 참고단가를 운송 조건 검토에 보조합니다."),
            Link(
                CommunityBoardInformationSourceKeys.PlannedNationalLogisticsStatistics,
                CommunityBoardInformationPublicationPolicies.NoAutomaticPublication,
                "생활물류·운송량 통계 connector를 추가할 때 연결할 예정입니다."),
            Link(
                CommunityBoardInformationSourceKeys.PlannedCargoSafetyGuidance,
                CommunityBoardInformationPublicationPolicies.NoAutomaticPublication,
                "화물 고정·상하차 안전자료 connector를 추가할 때 연결할 예정입니다.")),
        Board(
            CommunityBoardKeys.Prajna,
            ["관리자 선별 카드"],
            "매일",
            "카드별 관리자의 명시적 공개 승인 없이는 자동 게시하지 않습니다.",
            Link(
                CommunityBoardInformationSourceKeys.HongikHakdangCards,
                CommunityBoardInformationPublicationPolicies.EditorialReview,
                "동기화한 카드 중 관리자가 반야 게시를 승인한 항목만 후보로 사용합니다.")),
        Board(
            CommunityBoardKeys.Participation,
            ["공동구매 수요의 가격 참고"],
            "필요 시",
            NoRecruitmentAutomationBoundary,
            Link(
                CommunityInformationSourceKeys.KamisPriceObservations,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "국내 식재료 수요 글의 가격 참고값만 제공합니다."),
            Link(
                CommunityInformationSourceKeys.UsdaNassPriceObservations,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "미국 생산자 가격을 해외 조달 검토의 보조값으로만 제공합니다.")),
        Board(
            CommunityBoardKeys.SalesSupply,
            ["공급 제안 가격·재료 근거"],
            "필요 시",
            NoRecruitmentAutomationBoundary,
            Link(
                CommunityInformationSourceKeys.KamisPriceObservations,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "공급자가 제시한 국내 농수산물 가격의 참고 관측값을 제공합니다."),
            Link(
                CommunityInformationSourceKeys.UsdaNassPriceObservations,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "미국 생산자 가격을 공급가나 판매가로 오인하지 않도록 단위와 함께 제공합니다."),
            Link(
                CommunityBoardInformationSourceKeys.MfdsDomesticIngredientProducts,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "원재료와 제조업체의 공식 보고 근거를 공급 후보 확인에 보조합니다.")),
        Board(
            CommunityBoardKeys.LedgerProgress,
            ["공개 가능한 업무 상태 투영"],
            "Event 발생 시",
            "공공데이터 배치가 아니라 권한 확인을 마친 Command·Event의 공개 가능한 상태만 투영합니다.",
            Link(
                CommunityBoardInformationSourceKeys.CommunityActivityProjection,
                CommunityBoardInformationPublicationPolicies.InternalOnly,
                "업무 원장의 비식별 상태 전이를 해당 업무 게시판에 연결합니다.")),
        Board(
            CommunityBoardKeys.CompletionReview,
            ["비식별 완료 원장 집계"],
            "매일",
            "사용자명·연락처·주소·금액·상품 세부값을 읽지 않고 공개 완료 기록의 건수만 집계합니다.",
            Link(
                CommunityBoardInformationSourceKeys.ActivityDigest,
                CommunityBoardInformationPublicationPolicies.AutomatedBrief,
                "전날 공개 가능한 완료 원장 게시 기록을 업무 태그별 건수로 요약합니다.")),
        Board(
            CommunityBoardKeys.NoticeGuide,
            ["운영 공지", "이용·개인정보·거래 안전 안내"],
            "운영자 필요 시",
            "운영자 공지는 공공데이터 배치로 만들지 않으며 정책 변경을 확인한 사람이 작성합니다."),
        Board(
            CommunityBoardKeys.ProductFeedback,
            ["사용자 개선 제안"],
            "해당 없음",
            NoPublicDataBoundary),

        WorkBoard(
            CommunityActivityBoardKeys.FoundationEvidence,
            ["농수산물 가격", "공식 음식·재료", "공공데이터 읽기"],
            "매일",
            PriceLinks()
                .Concat(RecipeLinks())
                .Append(Link(
                    CommunityBoardInformationSourceKeys.MfdsDomesticIngredientProducts,
                    CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                    "국내 원재료·제조업체 공식 보고 근거를 연결합니다."))
                .ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.IndividualDemand,
            ["식재료 가격 변화", "다품목 공동구매", "개별 원함"],
            "매일",
            PriceLinks()
                .Append(Link(
                    CommunityInformationSourceKeys.AbsFoodPriceIndex,
                    CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                    "호주 식품 물가지수를 해외 비교가 필요한 경우에만 조회합니다."))
                .Append(Link(
                    CommunityBoardInformationSourceKeys.RoadAddressSearch,
                    CommunityBoardInformationPublicationPolicies.InternalOnly,
                    "동의한 주소를 생활권 수요 집계 단위로 보정하며 상세주소는 공개하지 않습니다."))
                .Append(Link(
                    CommunityBoardInformationSourceKeys.ApartmentComplexList,
                    CommunityBoardInformationPublicationPolicies.InternalOnly,
                    "공동주택 단지 후보를 내부 기준정보로 확인하며 자동 가입에 쓰지 않습니다."))
                .ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.CollectiveLedger,
            ["공동구매 집단화", "공동 원장 운영", "B2B·B2C 수요"],
            "주 2회",
            Link(
                CommunityBoardInformationSourceKeys.CommunityActivityProjection,
                CommunityBoardInformationPublicationPolicies.InternalOnly,
                "합의된 공동 원장 상태를 비식별 공개 투영으로 연결합니다."),
            Link(
                CommunityBoardInformationSourceKeys.PlannedOnlineShoppingStatistics,
                CommunityBoardInformationPublicationPolicies.NoAutomaticPublication,
                "온라인쇼핑동향 connector 구현 뒤 시장 규모 참고자료로 연결할 예정입니다.")),
        WorkBoard(
            CommunityActivityBoardKeys.HsClassification,
            ["HS 품목분류", "식품 수입요건", "관세율표"],
            "필요 시",
            CustomsLinks().ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.CustomsDelegation,
            ["통관 의뢰", "관세사 수임", "전자문서 동의"],
            "주 1회 후보",
            Link(
                CommunityBoardInformationSourceKeys.PlannedCustomsElectronicClearance,
                CommunityBoardInformationPublicationPolicies.NoAutomaticPublication,
                "관세청 전자통관 안내 connector 구현 뒤 절차 변경 근거로 연결할 예정입니다.")),
        WorkBoard(
            CommunityActivityBoardKeys.CustomsProcess,
            ["수입신고", "검사", "관세 납부", "반출"],
            "매일·필요 시",
            CustomsLinks()
                .Append(Link(
                    CommunityBoardInformationSourceKeys.MfdsImportedFoodLabels,
                    CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                    "수입식품 표시·제조업소 후보를 통관 전 식품 확인 근거로 연결합니다."))
                .Append(Link(
                    CommunityBoardInformationSourceKeys.MfdsOverseasManufacturers,
                    CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                    "해외제조업소의 최신 등록·중단 상태를 보조 확인합니다."))
                .ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.TransportRequest,
            ["화물 운송 의뢰", "운송 조건", "적재 제약"],
            "주 2회 후보",
            TransportLinks("운송 요청 조건을 검토할 때").ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.DispatchDecision,
            ["배차 의사결정", "기사 참여", "운송 안전"],
            "주 2회 후보",
            TransportLinks("배차 후보를 사람이 검토할 때").ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.LoadingJourney,
            ["상차 안전", "화물 고정", "운행 점검"],
            "주 1회 후보",
            SafetyLinks("상차와 화물 고정").ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.DeliveryHandover,
            ["하차 안전", "인수 증빙", "화물 손상 확인"],
            "주 1회 후보",
            SafetyLinks("하차와 인수").ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.SellerWarehouseReceipt,
            ["판매자 출고", "주문자 입고", "검수 기준"],
            "주 1회 후보",
            TransportLinks("출고·입고 조건을 검토할 때").ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.WarehouseInbound,
            ["창고 입고", "수량·상태 검수", "적재 위치"],
            "주 1회 후보",
            SafetyLinks("창고 입고와 적재").ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.PickingHandover,
            ["피킹 정확도", "포장", "출고 인계"],
            "주 1회 후보",
            TransportLinks("피킹·포장·출고 조건을 검토할 때").ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.FoodOrderAcceptance,
            ["공식 음식 레시피", "식재료 수요", "음식점 주문 운영"],
            "매일",
            RecipeLinks().Concat(PriceLinks()).ToArray()),
        WorkBoard(
            CommunityActivityBoardKeys.FoodDeliveryHandoff,
            ["조리 완료 인계", "배달 픽업", "식품 온도 관리"],
            "주 1회 후보",
            Link(
                CommunityBoardInformationSourceKeys.PlannedDeliveryFoodSafety,
                CommunityBoardInformationPublicationPolicies.NoAutomaticPublication,
                "식약처 배달음식 안전정보 connector 구현 뒤 확인자료로 연결할 예정입니다.")),
        WorkBoard(
            CommunityActivityBoardKeys.MartFulfillment,
            ["마트 가격", "전통시장 기준정보", "피킹·포장", "즉시배송"],
            "매일",
            PriceLinks()
                .Append(Link(
                    CommunityBoardInformationSourceKeys.TraditionalMarketStatus,
                    CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                    "전통시장·공동물류시설 기준정보를 지역 공급 후보에 연결합니다."))
                .Concat(TransportLinks("마트 피킹·포장·배송 조건을 검토할 때"))
                .ToArray()),

        Board(
            CommunityBoardKeys.SafetyReport,
            ["보호 신고·분쟁 기록"],
            "해당 없음",
            "보호 기록은 공공데이터·조사 배치의 입력이나 자동 게시 원천으로 사용하지 않습니다.")
    ];

    public static CommunityBoardInformationRelation? Find(string? boardKeyOrName)
    {
        var board = CommunityBoardCatalog.Find(boardKeyOrName);
        return board is null
            ? null
            : All.FirstOrDefault(item =>
                string.Equals(item.BoardKey, board.Key, StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyList<CommunityBoardInformationSourceRelation> PeriodicBatchRelations()
        => All
            .SelectMany(board => board.Sources)
            .Where(source => source.HasPeriodicBatchModule)
            .GroupBy(source => new
            {
                source.SourceKey,
                source.BatchModuleKey
            })
            .Select(group => group.First())
            .OrderBy(source => source.BatchModuleKey)
            .ThenBy(source => source.SourceKey)
            .ToArray();

    public static IReadOnlyList<CommunityBoardInformationBatchPlan> PeriodicBatchPlans()
        => All
            .SelectMany(board => board.Sources.Select(source => new
            {
                board.BoardKey,
                Source = source
            }))
            .Where(item => item.Source.HasPeriodicBatchModule)
            .GroupBy(item => new
            {
                item.Source.SourceKey,
                item.Source.BatchModuleKey,
                item.Source.UpdateCycle
            })
            .Select(group => new CommunityBoardInformationBatchPlan(
                group.Key.SourceKey,
                group.Key.BatchModuleKey,
                group.Key.UpdateCycle,
                CommunityPeriodicDataBoardCatalog.CanonicalBoardKeyForSource(
                    group.Key.SourceKey),
                group
                    .Select(item => item.BoardKey)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(boardKey => boardKey)
                    .ToArray(),
                group
                    .SelectMany(item => item.Source.PublicationSourceKeys)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(sourceKey => sourceKey)
                    .ToArray(),
                AllowsAutomaticPublication: group.Any(item =>
                    item.Source.AllowsAutomaticPublication),
                RequiresEditorialReview: group.Any(item =>
                    item.Source.PublicationPolicy ==
                    CommunityBoardInformationPublicationPolicies.EditorialReview),
                RequiresExplicitActivation: true))
            .OrderBy(plan => plan.BatchModuleKey)
            .ThenBy(plan => plan.SourceKey)
            .ToArray();

    private static CommunityBoardInformationRelation WorkBoard(
        string boardKey,
        IReadOnlyList<string> topics,
        string cadence,
        params CommunityBoardInformationSourceRelation[] sources)
        => Board(
            boardKey,
            topics,
            cadence,
            "업무 게시판 자료는 설명·검토 근거이며 Command 승인, 계약, 배차, 결제 또는 상태 전이를 자동 실행하지 않습니다.",
            sources);

    private static CommunityBoardInformationRelation Board(
        string boardKey,
        IReadOnlyList<string> topics,
        string cadence,
        string automationBoundary,
        params CommunityBoardInformationSourceRelation[] sources)
    {
        var board = CommunityBoardCatalog.Find(boardKey)
                    ?? throw new InvalidOperationException($"게시판을 찾을 수 없습니다. BoardKey={boardKey}");
        return new(
            board.Key,
            board.DisplayName,
            topics,
            cadence,
            sources
                .GroupBy(source => source.SourceKey, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray(),
            automationBoundary);
    }

    private static CommunityBoardInformationSourceRelation Link(
        string sourceKey,
        string publicationPolicy,
        string purpose)
    {
        if (!Sources.TryGetValue(sourceKey, out var source))
        {
            throw new InvalidOperationException(
                $"게시판 정보 원천 정의를 찾을 수 없습니다. SourceKey={sourceKey}");
        }

        return new(
            source.SourceKey,
            source.Provider,
            source.DisplayName,
            source.SourceType,
            source.ConnectorStatus,
            source.BatchStatus,
            source.UpdateCycle,
            source.BatchModuleKey,
            publicationPolicy,
            purpose,
            source.Limitations,
            source.PublicationSourceKeys);
    }

    private static IReadOnlyList<CommunityBoardInformationSourceRelation> PriceLinks()
        =>
        [
            Link(
                CommunityInformationSourceKeys.KamisPriceObservations,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "국내 농수산물 관측가격을 조사일·품목·등급·단위와 함께 참고합니다."),
            Link(
                CommunityInformationSourceKeys.UsdaNassPriceObservations,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "미국 생산자 수취가격을 기준월·원 단위와 함께 참고합니다.")
        ];

    private static IReadOnlyList<CommunityBoardInformationSourceRelation> RecipeLinks()
        =>
        [
            Link(
                CommunityInformationSourceKeys.MfdsCookRecipes,
                CommunityBoardInformationPublicationPolicies.EditorialReview,
                "식약처 공식 조리정보를 음식·재료 후보로 검토합니다."),
            Link(
                CommunityInformationSourceKeys.RdaLocalFoodRecipes,
                CommunityBoardInformationPublicationPolicies.EditorialReview,
                "농촌진흥청 향토음식 자료를 지역 음식 후보로 검토합니다."),
            Link(
                CommunityInformationSourceKeys.MaffRegionalCuisineRecipes,
                CommunityBoardInformationPublicationPolicies.EditorialReview,
                "일본 농림수산성 향토요리 자료를 문화 맥락 후보로 검토합니다."),
            Link(
                CommunityInformationSourceKeys.NhsHealthierFamiliesRecipes,
                CommunityBoardInformationPublicationPolicies.EditorialReview,
                "영국 NHS 공개 조리 메타데이터를 음식 후보로 검토합니다.")
        ];

    private static IReadOnlyList<CommunityBoardInformationSourceRelation> CustomsLinks()
        =>
        [
            Link(
                Hs공공데이터출처Keys.수입평균단가,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "품목·국가별 CIF 수입 참고단가를 조회합니다."),
            Link(
                Hs공공데이터출처Keys.세관장확인대상물품,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "HSK별 세관장 확인 법령과 승인기관을 조회합니다."),
            Link(
                Hs공공데이터출처Keys.관세환율,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                "기준일의 주간 관세환율을 조회합니다.")
        ];

    private static IReadOnlyList<CommunityBoardInformationSourceRelation> TransportLinks(
        string context)
        =>
        [
            Link(
                CommunityBoardInformationSourceKeys.CustomsCargoTracking,
                CommunityBoardInformationPublicationPolicies.ReferenceOnly,
                $"{context} 지정 화물의 통관 진행상태를 필요 시 조회합니다."),
            Link(
                CommunityBoardInformationSourceKeys.PlannedNationalLogisticsStatistics,
                CommunityBoardInformationPublicationPolicies.NoAutomaticPublication,
                $"{context} 국가 물류통계 connector를 구현한 뒤 연결할 예정입니다.")
        ];

    private static IReadOnlyList<CommunityBoardInformationSourceRelation> SafetyLinks(
        string context)
        =>
        [
            Link(
                CommunityBoardInformationSourceKeys.PlannedCargoSafetyGuidance,
                CommunityBoardInformationPublicationPolicies.NoAutomaticPublication,
                $"{context} 산업안전 공식자료 connector를 구현한 뒤 연결할 예정입니다.")
        ];

    private static IReadOnlyDictionary<string, SourceDefinition> CreateSourceDefinitions()
    {
        var definitions = new[]
        {
            Source(
                CommunityInformationSourceKeys.KamisPriceObservations,
                "한국농수산식품유통공사",
                "KAMIS 농수산물 가격 관측",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.Scheduled,
                "일별·월별",
                CommunityBoardInformationBatchModuleKeys.AgriculturalFisheries,
                "관측 품목 일부이며 전체 시장 평균·판매 권고가 아닙니다.",
                CommunityBoardInformationPublicationSourceKeys.KamisPriceBrief,
                CommunityBoardInformationPublicationSourceKeys.WeeklyCountryProductComparison),
            Source(
                CommunityInformationSourceKeys.UsdaNassPriceObservations,
                "USDA NASS",
                "미국 생산자 수취가격",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.Scheduled,
                "월별",
                CommunityBoardInformationBatchModuleKeys.AgriculturalFisheries,
                "미국 전국 생산자 가격이며 소매가·한국 유통가·개별 견적이 아닙니다.",
                CommunityBoardInformationPublicationSourceKeys.UsdaNassPriceBrief,
                CommunityBoardInformationPublicationSourceKeys.WeeklyCountryProductComparison),
            Source(
                CommunityInformationSourceKeys.AbsFoodPriceIndex,
                "Australian Bureau of Statistics",
                "호주 식품 소비자물가지수",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.OnDemand,
                "분기",
                CommunityBoardInformationBatchModuleKeys.PublicDataQuery,
                "지수는 개별 품목 가격이나 한국 판매가격이 아닙니다."),
            Source(
                CommunityInformationSourceKeys.FishCooperativeGeneralStatistics,
                "수산업협동조합중앙회",
                "수협 일반현황 통계",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.OnDemand,
                "공표 주기 확인 필요",
                CommunityBoardInformationBatchModuleKeys.PublicDataQuery,
                "기관 일반현황이며 개별 수산물 시세나 거래조건이 아닙니다."),
            RecipeSource(
                CommunityInformationSourceKeys.MfdsCookRecipes,
                "식품의약품안전처",
                "식약처 조리식품 레시피"),
            RecipeSource(
                CommunityInformationSourceKeys.RdaLocalFoodRecipes,
                "농촌진흥청",
                "농촌진흥청 지역·향토음식"),
            RecipeSource(
                CommunityInformationSourceKeys.MaffRegionalCuisineRecipes,
                "일본 농림수산성",
                "일본 지역 향토요리"),
            RecipeSource(
                CommunityInformationSourceKeys.NhsHealthierFamiliesRecipes,
                "NHS",
                "NHS Healthier Families recipes"),
            Source(
                CommunityBoardInformationSourceKeys.TraditionalMarketStatus,
                "소상공인시장진흥공단",
                "전통시장 현황",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.ReadyToSchedule,
                "연간 기준자료",
                CommunityBoardInformationBatchModuleKeys.TraditionalMarketSync,
                "시설의 현재 운영 여부나 입점 사업자의 권한을 증명하지 않습니다."),
            Source(
                CommunityBoardInformationSourceKeys.MfdsDomesticIngredientProducts,
                "식품의약품안전처",
                "식품 품목제조보고 원재료",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.Scheduled,
                "주별",
                CommunityBoardInformationBatchModuleKeys.IngredientCompanyResearch,
                "보고 원재료와 업체 후보이며 현재 판매·공급 가능성을 확정하지 않습니다."),
            Source(
                CommunityBoardInformationSourceKeys.MfdsImportedFoodLabels,
                "식품의약품안전처",
                "수입식품 제품별 한글표시사항",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.Scheduled,
                "주별 조사·월별 게시 후보",
                CommunityBoardInformationBatchModuleKeys.IngredientCompanyResearch,
                "제조업소 소재지는 원재료 생산지나 법정 원산지가 아닙니다.",
                CommunityBoardInformationPublicationSourceKeys.ChinaImportedFoodRegionBrief,
                CommunityBoardInformationPublicationSourceKeys.UnitedStatesImportedFoodStateBrief),
            Source(
                CommunityBoardInformationSourceKeys.MfdsOverseasManufacturers,
                "식품의약품안전처",
                "수입식품 해외제조업소",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.Scheduled,
                "주별 조사·수입 전 재확인",
                CommunityBoardInformationBatchModuleKeys.IngredientCompanyResearch,
                "시설 상태는 바뀔 수 있고 이름·국가 대조는 동일 업체를 확정하지 않습니다.",
                CommunityBoardInformationPublicationSourceKeys.ChinaImportedFoodRegionBrief,
                CommunityBoardInformationPublicationSourceKeys.UnitedStatesImportedFoodStateBrief),
            Source(
                Hs공공데이터출처Keys.수입평균단가,
                "관세청",
                "품목별 국가별 수입 평균단가",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.OnDemand,
                "요청 시",
                CommunityBoardInformationBatchModuleKeys.CustomsPublicDataQuery,
                "CIF 통계단가는 세금·검역·통관·국내 물류비와 판매마진을 포함하지 않습니다."),
            Source(
                Hs공공데이터출처Keys.세관장확인대상물품,
                "관세청",
                "세관장확인대상물품",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.OnDemand,
                "요청 시·법령 변경 시 재확인",
                CommunityBoardInformationBatchModuleKeys.CustomsPublicDataQuery,
                "결과가 없더라도 수입요건이 없거나 통관 가능하다고 확정하지 않습니다."),
            Source(
                Hs공공데이터출처Keys.관세환율,
                "관세청",
                "주간 관세환율",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.OnDemand,
                "주별·요청 시",
                CommunityBoardInformationBatchModuleKeys.CustomsPublicDataQuery,
                "일반 환전 시세가 아니라 수입 과세가격 계산용 환율입니다."),
            Source(
                CommunityBoardInformationSourceKeys.CustomsCargoTracking,
                "관세청/공공데이터포털",
                "화물 통관 진행 정보",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.OnDemand,
                "요청 시",
                CommunityBoardInformationBatchModuleKeys.CustomsPublicDataQuery,
                "개별 화물 식별정보가 필요하므로 공개 피드나 무차별 배치에 사용하지 않습니다."),
            Source(
                CommunityBoardInformationSourceKeys.RoadAddressSearch,
                "행정안전부",
                "도로명주소 검색",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.OnDemand,
                "요청 시",
                CommunityBoardInformationBatchModuleKeys.PublicDataQuery,
                "동의한 주소 보정에만 사용하고 상세주소·동·호수를 공개하지 않습니다."),
            Source(
                CommunityBoardInformationSourceKeys.ApartmentComplexList,
                "국토교통부/K-apt",
                "공동주택 단지 목록",
                CommunityInformationSourceTypes.PublicData,
                CommunityBoardInformationBatchStatuses.OnDemand,
                "요청 시·동기화 시각 기록",
                CommunityBoardInformationBatchModuleKeys.PublicDataQuery,
                "단지 후보 확인용이며 주민의 가입·알림·상대 선택을 자동화하지 않습니다."),
            ProjectionSource(
                CommunityBoardInformationSourceKeys.HongikHakdangCards,
                "홍익학당 공개자료",
                "홍익학당 카드 보관본",
                CommunityBoardInformationBatchStatuses.Scheduled,
                "설정된 동기화 주기",
                CommunityBoardInformationBatchModuleKeys.CommunityEditorial,
                "출처·권리·공개 승인을 확인한 카드만 반야 게시 후보가 됩니다.",
                CommunityBoardInformationPublicationSourceKeys.PrajnaCard),
            ProjectionSource(
                CommunityBoardInformationSourceKeys.Reflection,
                "살뜰",
                "살뜰 운영 성찰문",
                CommunityBoardInformationBatchStatuses.Scheduled,
                "주 2회",
                CommunityBoardInformationBatchModuleKeys.CommunityEditorial,
                "외부 인물의 실제 인용문이 아니라 살뜰 원칙을 바탕으로 직접 작성한 글입니다.",
                CommunityBoardInformationPublicationSourceKeys.Reflection),
            ProjectionSource(
                CommunityBoardInformationSourceKeys.ActivityDigest,
                "살뜰 공개 원장 투영",
                "비식별 완료 원장 활동 요약",
                CommunityBoardInformationBatchStatuses.Scheduled,
                "매일",
                CommunityBoardInformationBatchModuleKeys.CommunityEditorial,
                "공개 완료 기록의 건수만 집계하며 거래액·매출·중개 실적이 아닙니다.",
                CommunityBoardInformationPublicationSourceKeys.ActivityDigest),
            ProjectionSource(
                CommunityBoardInformationSourceKeys.CommunityActivityProjection,
                "살뜰 Command/Event",
                "업무 원장 공개 상태 투영",
                CommunityBoardInformationBatchStatuses.NotApplicable,
                "Event 발생 시",
                CommunityBoardInformationBatchModuleKeys.CommunityActivityProjection,
                "권한과 공개 범위를 통과한 상태만 투영하며 원시 payload와 개인정보를 공개하지 않습니다."),
            PlannedSource(
                CommunityBoardInformationSourceKeys.PlannedOnlineShoppingStatistics,
                "통계청",
                "온라인쇼핑동향",
                "connector와 보관 계약이 아직 구현되지 않았습니다."),
            PlannedSource(
                CommunityBoardInformationSourceKeys.PlannedCustomsElectronicClearance,
                "관세청",
                "전자통관 절차 안내",
                "절차 변경을 구조화해 수집하는 connector가 아직 구현되지 않았습니다."),
            PlannedSource(
                CommunityBoardInformationSourceKeys.PlannedNationalLogisticsStatistics,
                "국가물류통합정보센터",
                "국가 물류·생활물류 통계",
                "공식 통계 connector와 게시 검토 기준이 아직 구현되지 않았습니다."),
            PlannedSource(
                CommunityBoardInformationSourceKeys.PlannedCargoSafetyGuidance,
                "한국산업안전보건공단",
                "화물·상하차·창고 안전자료",
                "문서 변경 감지와 편집 검토 connector가 아직 구현되지 않았습니다."),
            PlannedSource(
                CommunityBoardInformationSourceKeys.PlannedDeliveryFoodSafety,
                "식품의약품안전처",
                "배달음식 안전정보",
                "공식 안전자료 connector와 갱신 기준이 아직 구현되지 않았습니다.")
        };

        return definitions.ToDictionary(
            source => source.SourceKey,
            StringComparer.OrdinalIgnoreCase);
    }

    private static SourceDefinition RecipeSource(
        string sourceKey,
        string provider,
        string displayName)
        => Source(
            sourceKey,
            provider,
            displayName,
            CommunityInformationSourceTypes.PublicData,
            CommunityBoardInformationBatchStatuses.ReadyToSchedule,
            "원천별 갱신 주기",
            CommunityBoardInformationBatchModuleKeys.OfficialFoodRecipeArchive,
            "보관만으로 게시하지 않으며 대표 음식·권리·자동화 상태를 운영자가 승인해야 합니다.",
            CommunityBoardInformationPublicationSourceKeys.CultureTransport);

    private static SourceDefinition Source(
        string sourceKey,
        string provider,
        string displayName,
        string sourceType,
        string batchStatus,
        string updateCycle,
        string batchModuleKey,
        string limitations,
        params string[] publicationSourceKeys)
        => new(
            sourceKey,
            provider,
            displayName,
            sourceType,
            CommunityBoardInformationConnectorStatuses.Implemented,
            batchStatus,
            updateCycle,
            batchModuleKey,
            limitations,
            publicationSourceKeys);

    private static SourceDefinition ProjectionSource(
        string sourceKey,
        string provider,
        string displayName,
        string batchStatus,
        string updateCycle,
        string batchModuleKey,
        string limitations,
        params string[] publicationSourceKeys)
        => new(
            sourceKey,
            provider,
            displayName,
            "InternalProjection",
            CommunityBoardInformationConnectorStatuses.ImplementedProjection,
            batchStatus,
            updateCycle,
            batchModuleKey,
            limitations,
            publicationSourceKeys);

    private static SourceDefinition PlannedSource(
        string sourceKey,
        string provider,
        string displayName,
        string limitations)
        => new(
            sourceKey,
            provider,
            displayName,
            "PlannedOfficialSource",
            CommunityBoardInformationConnectorStatuses.Planned,
            CommunityBoardInformationBatchStatuses.Planned,
            "미정",
            CommunityBoardInformationBatchModuleKeys.PlannedConnector,
            limitations,
            []);
}
