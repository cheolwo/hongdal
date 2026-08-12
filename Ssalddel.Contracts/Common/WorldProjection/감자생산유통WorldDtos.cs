using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.WorldProjection;

public static class 감자생산유통WorldRoutes
{
    // 먼저 구현된 Unity client와의 transport 호환을 위해 기존 URI를 유지한다.
    public const string 조회 = "api/v1/common/world/slices/potato-journey";
}

public static class 감자생산유통SourceModeCodes
{
    public const string OperationalProjection = "OperationalProjection";
    public const string SimulationFixture = "SimulationFixture";
}

public static class 감자생산유통LinkageStatusCodes
{
    public const string CanonicalLinked = "CanonicalLinked";
    public const string SimulationLinked = "SimulationLinked";
    public const string ProductOnly = "ProductOnly";
    public const string Unverified = "Unverified";
    public const string Unavailable = "Unavailable";
}

public static class 감자가격관측StatusCodes
{
    public const string Ready = "Ready";
    public const string MappingRequired = "MappingRequired";
    public const string DataUnavailable = "DataUnavailable";
}

public sealed class 감자생산유통World조회요청
{
    public string? CultivationStableId { get; init; }

    public string? ReferenceDate { get; init; }

    public int LookbackDays { get; init; } = 14;
}

public sealed record 감자생산유통SourceLineageResponse(
    string SourceKey,
    string SourceStableId,
    string SourceRevision,
    DateTimeOffset? ObservedAt,
    string SourceModeCode);

public sealed record 감자상품WorldResponse(
    string ProductStableId,
    string DisplayName,
    string HsPrefix,
    string MappingQualityCode,
    string MappingQualityLabel,
    string MappingEvidence,
    bool InformationOnly);

public sealed record 감자가격구간WorldResponse(
    string MarketStageCode,
    string MarketStageLabel,
    decimal AverageKrwPerKg,
    decimal MinimumKrwPerKg,
    decimal MaximumKrwPerKg,
    int SampleCount,
    string LatestSurveyDate);

public sealed record 감자가격관측WorldResponse(
    string StatusCode,
    string HsCode,
    string UnitCode,
    string CurrencyCode,
    string DataSource,
    string StartDate,
    string EndDate,
    감자가격구간WorldResponse? Wholesale,
    감자가격구간WorldResponse? Retail,
    IReadOnlyList<string> Notices,
    bool InformationOnly);

public sealed record 감자재배WorldResponse(
    string FarmStableId,
    long FarmRevision,
    string PlotStableId,
    long PlotRevision,
    string CultivationStableId,
    long CultivationRevision,
    string CropName,
    string? CropReferenceStableId,
    string? CropReferenceSourceKey,
    string GrowthStatusCode,
    DateOnly? PlantedOn,
    DateOnly? ExpectedHarvestOn,
    string ProductLinkageStatusCode,
    IReadOnlyList<FarmSensorResponse> Sensors);

public sealed record 감자화물WorldResponse(
    string CargoStableId,
    string TransportTaskStableId,
    string InboundTaskStableId,
    string HandoffStateCode);

public sealed record 감자WarehouseWorldResponse(
    string WarehouseStableId,
    string InventoryStableId,
    string TaskStableId,
    string StatusCode,
    int? AuthorizedQuantity);

public sealed record 감자MarketWorldResponse(
    string PublicProductStableId,
    decimal SalePrice,
    int AvailableQuantity,
    DateTimeOffset InventoryObservedAt);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PotatoProductionDistributionWorld,
    SsalddelCodeLayer.Contract,
    "감자 상품·가격과 명시적으로 연결된 Farm·화물·창고·마트 source를 하나의 읽기 전용 World slice로 전달한다.",
    FlowOrder = 10,
    Boundary = "source별 revision과 linkage를 보존한다. 상품명이나 Synty asset 이름으로 운영 원장을 연결하지 않는다.")]
public sealed record 감자생산유통WorldResponse(
    string StableId,
    string Revision,
    DateTimeOffset GeneratedAt,
    string AuthorizedRoleCode,
    string ViewerScopeCode,
    string AuthorizationDecisionId,
    string SourceModeCode,
    string LinkageStatusCode,
    감자상품WorldResponse Product,
    감자재배WorldResponse? Farm,
    감자가격관측WorldResponse DomesticPrice,
    감자화물WorldResponse? CargoJourney,
    감자WarehouseWorldResponse? Warehouse,
    감자MarketWorldResponse? Market,
    IReadOnlyList<감자생산유통SourceLineageResponse> SourceLineage,
    IReadOnlyList<string> Limitations,
    bool IsReadOnly);
