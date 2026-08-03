using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Community;

public static class 커뮤니티세계지도Routes
{
    public const string ObservationApi = "api/v1/community/world-map/observations";
    public const string KoreaPriceDetail = "/information/kamis-domestic-price-comparison";
    public const string UnitedStatesPriceDetail = "/information/usda-us-price-comparison";
}

public static class 커뮤니티세계지도LayerCodes
{
    public const string RegionalCulture = "regional-culture";
    public const string PublicPrice = "public-price";
    public const string WholesaleMarket = "wholesale-market";
    public const string TraditionalMarketHub = "traditional-market-hub";
    public const string OverseasManufacturer = "overseas-manufacturer";
    public const string GyeonggiLivestockPublicEvidence = "gyeonggi-livestock-public-evidence";
    public const string TourismPublicEvidence = "tourism-public-evidence";
    public const string OnlinePricePublicEvidence = "online-price-public-evidence";
    public const string KosisStatisticalContext = "kosis-statistical-context";
    public const string ProcurementHandoff = "procurement-handoff";
    public const string ImportReadiness = "import-readiness";
    public const string TransportHandoff = "transport-handoff";
    public const string WarehouseInboundHandoff = "warehouse-inbound-handoff";
    public const string LearningChannel = "learning-channel";
    public const string ScriptureAndClassics = "scripture-classics";
}

public static class 커뮤니티세계지도EvidenceStatusCodes
{
    public const string Curated = "curated";
    public const string OfficialSourceLinked = "official-source-linked";
}

public static class 커뮤니티세계지도FreshnessCodes
{
    public const string Fresh = "fresh";
    public const string Stale = "stale";
    public const string Expired = "expired";
    public const string Unknown = "unknown";
}

public static class 커뮤니티세계지도RolePerspectiveCodes
{
    public const string Community = "community";
    public const string Orderer = "orderer";
    public const string SellerAndShipper = "seller-and-shipper";
    public const string TransportOperator = "transport-operator";
    public const string WarehouseManager = "warehouse-manager";
    public const string CustomsSpecialist = "customs-specialist";
    public const string PlatformOperator = "platform-operator";
}

public sealed record 커뮤니티세계지도RoleLayerProfile(
    string Code,
    string DisplayName,
    string Description,
    IReadOnlyList<string> RecommendedLayerCodes,
    string Boundary);

/// <summary>
/// 같은 공개 지도 자료를 사용자의 현재 역할에 맞는 기본 관점으로 배열합니다.
/// 이 catalog는 권한 판정이나 개인별 추천에 사용하지 않습니다.
/// </summary>
public static class 커뮤니티세계지도RoleLayerProfileCatalog
{
    public static IReadOnlyList<커뮤니티세계지도RoleLayerProfile> All { get; } =
    [
        new(
            커뮤니티세계지도RolePerspectiveCodes.Community,
            "생활·커뮤니티 참여자",
            "지역의 생활문화와 공개 가격부터 둘러보고 대화에 필요한 맥락을 확인합니다.",
            [
                커뮤니티세계지도LayerCodes.RegionalCulture,
                커뮤니티세계지도LayerCodes.TourismPublicEvidence,
                커뮤니티세계지도LayerCodes.PublicPrice,
                커뮤니티세계지도LayerCodes.KosisStatisticalContext
            ],
            "둘러본 사실을 관심·가입·거래 의사로 기록하지 않습니다."),
        new(
            커뮤니티세계지도RolePerspectiveCodes.Orderer,
            "주문자·구매 담당",
            "품목 가격, 조달 시장과 공동 수령 후보를 먼저 비교합니다.",
            [
                커뮤니티세계지도LayerCodes.ProcurementHandoff,
                커뮤니티세계지도LayerCodes.GyeonggiLivestockPublicEvidence,
                커뮤니티세계지도LayerCodes.OnlinePricePublicEvidence,
                커뮤니티세계지도LayerCodes.KosisStatisticalContext,
                커뮤니티세계지도LayerCodes.RegionalCulture
            ],
            "지도 조회만으로 주문·구매·참여 원장을 만들지 않습니다."),
        new(
            커뮤니티세계지도RolePerspectiveCodes.SellerAndShipper,
            "판매자·화주",
            "판매 시장, 해외 제조 근거와 가격 관측을 공급 판단 순서로 확인합니다.",
            [
                커뮤니티세계지도LayerCodes.ProcurementHandoff,
                커뮤니티세계지도LayerCodes.GyeonggiLivestockPublicEvidence,
                커뮤니티세계지도LayerCodes.ImportReadiness,
                커뮤니티세계지도LayerCodes.TransportHandoff
            ],
            "공개 업소 근거는 거래 가능 업체 추천이나 계약 상대 선정이 아닙니다."),
        new(
            커뮤니티세계지도RolePerspectiveCodes.TransportOperator,
            "기사·운송 담당",
            "공개된 시장과 공동 입고·수령 거점을 중심으로 지역 흐름을 살펴봅니다.",
            [
                커뮤니티세계지도LayerCodes.TransportHandoff
            ],
            "실시간 화물·배차·개인 위치·정확한 상하차 주소는 표시하지 않습니다."),
        new(
            커뮤니티세계지도RolePerspectiveCodes.WarehouseManager,
            "창고·거점 관리자",
            "입출고 연결 시장과 공동 거점, 가격 맥락을 먼저 확인합니다.",
            [
                커뮤니티세계지도LayerCodes.WarehouseInboundHandoff,
                커뮤니티세계지도LayerCodes.TransportHandoff
            ],
            "공개 지도에는 재고·처리능력의 비공개 운영 상태를 노출하지 않습니다."),
        new(
            커뮤니티세계지도RolePerspectiveCodes.CustomsSpecialist,
            "통관·무역 전문 역할",
            "해외 제조 근거와 국가별 가격·시장 맥락을 수입 준비 관점에서 확인합니다.",
            [
                커뮤니티세계지도LayerCodes.ImportReadiness,
                커뮤니티세계지도LayerCodes.ProcurementHandoff
            ],
            "공개 근거는 수입 적합성 판정, 신고 수임 또는 통관 완료 상태가 아닙니다."),
        new(
            커뮤니티세계지도RolePerspectiveCodes.PlatformOperator,
            "플랫폼 운영자",
            "모든 공개 레이어의 연결·준비 상태와 출처를 함께 점검합니다.",
            [
                커뮤니티세계지도LayerCodes.RegionalCulture,
                커뮤니티세계지도LayerCodes.PublicPrice,
                커뮤니티세계지도LayerCodes.WholesaleMarket,
                커뮤니티세계지도LayerCodes.TraditionalMarketHub,
                커뮤니티세계지도LayerCodes.OverseasManufacturer,
                커뮤니티세계지도LayerCodes.GyeonggiLivestockPublicEvidence,
                커뮤니티세계지도LayerCodes.TourismPublicEvidence,
                커뮤니티세계지도LayerCodes.OnlinePricePublicEvidence,
                커뮤니티세계지도LayerCodes.KosisStatisticalContext,
                커뮤니티세계지도LayerCodes.ProcurementHandoff,
                커뮤니티세계지도LayerCodes.ImportReadiness,
                커뮤니티세계지도LayerCodes.TransportHandoff,
                커뮤니티세계지도LayerCodes.WarehouseInboundHandoff
            ],
            "운영 관점도 개인 위치나 계약·주문·배차의 비공개 상태를 공개하지 않습니다.")
    ];

    public static 커뮤니티세계지도RoleLayerProfile Resolve(string? role)
    {
        var normalized = Normalize(role);
        var profileCode = ContainsAny(normalized, "서버관리자", "관리", "운영", "admin", "operator")
            ? 커뮤니티세계지도RolePerspectiveCodes.PlatformOperator
            : ContainsAny(normalized, "기사", "용달기사", "배달기사", "driver", "transport")
                ? 커뮤니티세계지도RolePerspectiveCodes.TransportOperator
                : ContainsAny(normalized, "화주", "판매자", "shipper", "seller")
                    ? 커뮤니티세계지도RolePerspectiveCodes.SellerAndShipper
                    : ContainsAny(normalized, "창고", "warehouse")
                        ? 커뮤니티세계지도RolePerspectiveCodes.WarehouseManager
                        : ContainsAny(normalized, "주문", "구매", "orderer", "buyer")
                            ? 커뮤니티세계지도RolePerspectiveCodes.Orderer
                            : ContainsAny(normalized, "관세", "통관", "customs")
                                ? 커뮤니티세계지도RolePerspectiveCodes.CustomsSpecialist
                                : 커뮤니티세계지도RolePerspectiveCodes.Community;

        return All.First(profile => string.Equals(profile.Code, profileCode, StringComparison.Ordinal));
    }

    private static bool ContainsAny(string text, params string[] candidates)
        => candidates.Any(candidate => text.Contains(
            Normalize(candidate),
            StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : text.Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Trim();
}

public sealed record 커뮤니티세계지도LayerDto(
    string Code,
    string DatasetCode,
    string DisplayName,
    string Description,
    string Color,
    string MarkerShape,
    string? LedgerTemplateKey = null,
    IReadOnlyList<string>? ObservationSourceLayerCodes = null);

public sealed record 커뮤니티세계지도MetricDto(
    string Code,
    string DisplayName,
    decimal Value,
    string Unit);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Contract,
    "한 개의 세계 지도에 표시할 분야별 공개 관측과 출처·시각·상세 경로를 전달",
    FlowOrder = 10,
    Effects = SsalddelCodeEffect.None,
    Boundary = "개인 정밀 위치와 결제·계약·배차 실행 상태는 포함하지 않습니다.")]
public sealed record 커뮤니티세계지도ObservationDto(
    string StableId,
    string DatasetCode,
    string LayerCode,
    string CountryCode,
    string CountryName,
    double Latitude,
    double Longitude,
    string Title,
    string Summary,
    string SourceName,
    DateTimeOffset? EvidenceAsOfUtc,
    string EvidenceStatusCode,
    string DetailHref,
    string? SourceHref = null,
    string? LocationPrecisionCode = null,
    string? MarketStageCode = null,
    string? MarkerStatusCode = null,
    decimal? ServiceRadiusKm = null,
    int? DailyCapacity = null,
    string? CommunityScopeKey = null,
    int? OrganizationCount = null,
    int? EvidenceCount = null,
    string? SourceDatasetKey = null,
    DateTimeOffset? SourceUpdatedAtUtc = null,
    DateTimeOffset? CollectedAtUtc = null,
    string? UpdateCycle = null,
    string? FreshnessCode = null,
    string? BoundaryNotice = null,
    IReadOnlyList<커뮤니티세계지도MetricDto>? Metrics = null);

public static class 커뮤니티세계지도위치정밀도Codes
{
    public const string AdministrativeRegionRepresentative =
        "administrative-region-representative";

    public const string CountryRepresentative = "country-representative";

    public const string OfficialPoint = "official-point";
}

public sealed record 커뮤니티세계지도SnapshotDto(
    string DatasetCode,
    string Revision,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<커뮤니티세계지도LayerDto> Layers,
    IReadOnlyList<커뮤니티세계지도ObservationDto> Observations);

public static class 커뮤니티세계지도LayerCatalog
{
    public static IReadOnlyList<커뮤니티세계지도LayerDto> All { get; } =
    [
        new(
            커뮤니티세계지도LayerCodes.RegionalCulture,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "지역 문화",
            "지역의 생활권·음식·특산물 맥락",
            "#176b4d",
            "circle"),
        new(
            커뮤니티세계지도LayerCodes.PublicPrice,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "가격·시장",
            "공식 가격 관측과 원 거래 단위",
            "#ef8f3c",
            "diamond"),
        new(
            커뮤니티세계지도LayerCodes.WholesaleMarket,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "도매시장",
            "한국 공영도매시장과 미국 USDA 터미널 시장 보고 위치",
            "#2f6fab",
            "market"),
        new(
            커뮤니티세계지도LayerCodes.TraditionalMarketHub,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "전통시장 거점",
            "운영 동의·현장 확인·지도 좌표 검증이 끝난 공동 입고·수령 거점",
            "#8a4b24",
            "hub"),
        new(
            커뮤니티세계지도LayerCodes.OverseasManufacturer,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "해외제조업소",
            "식약처 해외제조업소 근거를 검증된 행정권역 대표점에 집계",
            "#7b4ab0",
            "factory"),
        new(
            커뮤니티세계지도LayerCodes.GyeonggiLivestockPublicEvidence,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "경기 축산 공개근거",
            "가축사육업 인허가 원문을 검증된 행정구역 대표점의 영업상태별 집계로 표시",
            "#c87924",
            "livestock"),
        new(
            커뮤니티세계지도LayerCodes.TourismPublicEvidence,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "관광 공개지점",
            "한국관광공사가 제공한 공개 관광정보 좌표의 제한된 snapshot",
            "#148a8a",
            "tourism"),
        new(
            커뮤니티세계지도LayerCodes.OnlinePricePublicEvidence,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "온라인 수집가격",
            "웹 수집가격의 품목 연결 범위를 단위 미정렬 경계와 함께 표시",
            "#d05c42",
            "online-price"),
        new(
            커뮤니티세계지도LayerCodes.KosisStatisticalContext,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "KOSIS 물가 맥락",
            "전국 소비자물가지수를 기준월·단위와 함께 표시",
            "#5d63b8",
            "statistics"),
        new(
            커뮤니티세계지도LayerCodes.ProcurementHandoff,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "공동 조달·수령",
            "공동구매 원장의 가격 근거, 조달 시장과 공개 수령 거점을 한 관점으로 연결",
            "#a76320",
            "procurement",
            CommunityLedgerTemplateKeys.GroupPurchase,
            [
                커뮤니티세계지도LayerCodes.PublicPrice,
                커뮤니티세계지도LayerCodes.OnlinePricePublicEvidence,
                커뮤니티세계지도LayerCodes.KosisStatisticalContext,
                커뮤니티세계지도LayerCodes.WholesaleMarket,
                커뮤니티세계지도LayerCodes.TraditionalMarketHub
            ]),
        new(
            커뮤니티세계지도LayerCodes.ImportReadiness,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "수입 준비 근거",
            "수입 준비도 원장의 제품·원산지와 공식 확인 근거에 대응하는 공개 자료",
            "#a64f73",
            "readiness",
            CommunityLedgerTemplateKeys.MeatImportReadiness,
            [
                커뮤니티세계지도LayerCodes.OverseasManufacturer,
                커뮤니티세계지도LayerCodes.PublicPrice
            ]),
        new(
            커뮤니티세계지도LayerCodes.TransportHandoff,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "운송 인계 거점",
            "화물 운송 원장의 상하차 인계에 참고할 수 있는 공개 시장·공동 거점",
            "#315f8c",
            "route",
            CommunityLedgerTemplateKeys.CargoTransport,
            [
                커뮤니티세계지도LayerCodes.WholesaleMarket,
                커뮤니티세계지도LayerCodes.TraditionalMarketHub
            ]),
        new(
            커뮤니티세계지도LayerCodes.WarehouseInboundHandoff,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "공동 입고 인계",
            "창고 입고 원장의 납품·검수 인계 전에 확인하는 공개 시장·공동 거점",
            "#4f6b3a",
            "warehouse",
            CommunityLedgerTemplateKeys.WarehouseInbound,
            [
                커뮤니티세계지도LayerCodes.WholesaleMarket,
                커뮤니티세계지도LayerCodes.TraditionalMarketHub
            ]),
        new(
            커뮤니티세계지도LayerCodes.LearningChannel,
            CommunityPageRoutes.WorldMapNightLearningDataset,
            "생각·성찰 자료",
            "철학·심리·윤리·아이디어를 가볍게 알아차리는 공개 자료",
            "#6750a4",
            "circle"),
        new(
            커뮤니티세계지도LayerCodes.ScriptureAndClassics,
            CommunityPageRoutes.WorldMapNightLearningDataset,
            "경전·고전 자료",
            "종교 교육·마음챙김 관련 공개 자료를 가볍게 알아차림",
            "#b7791f",
            "diamond")
    ];

    public static IReadOnlyList<커뮤니티세계지도LayerDto> ForDataset(string datasetCode)
        => All.Where(layer => string.Equals(
                layer.DatasetCode,
                datasetCode,
                StringComparison.Ordinal))
            .ToArray();
}
