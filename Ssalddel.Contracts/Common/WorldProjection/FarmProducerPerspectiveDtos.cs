using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.WorldProjection;

public static class FarmProducerPerspectiveRoutes
{
    public const string Producer =
        "api/v1/shipper/world/zones/farm/producer-perspective";
}

public static class FarmProducerStatusCodes
{
    public const string Operating = "Operating";
    public const string Inactive = "Inactive";
}

public static class FarmSensorConditionCodes
{
    public const string Normal = "Normal";
    public const string Dry = "Dry";
    public const string Critical = "Critical";
    public const string Waterlogged = "Waterlogged";
    public const string Unknown = "Unknown";
}

public sealed record FarmSensorObservationResponse(
    decimal Value,
    string UnitCode,
    DateTimeOffset ObservedAt,
    string FreshnessStatusCode,
    string ConditionCode,
    string AssessmentRuleRevision,
    string? EvidenceCardId,
    string? ConfidenceCode,
    string? Limitation);

public sealed record FarmSensorResponse(
    string StableId,
    long Revision,
    string SensorTypeCode,
    string StatusCode,
    FarmSensorObservationResponse? LatestObservation);

public sealed record FarmCultivationResponse(
    string StableId,
    long Revision,
    string CropName,
    string? CropReferenceStableId,
    string? CropReferenceSourceKey,
    string GrowthStatusCode,
    DateOnly? PlantedOn,
    DateOnly? ExpectedHarvestOn);

public sealed record FarmPlotResponse(
    string StableId,
    long Revision,
    string PlotName,
    string? SoilManagementProfileCode,
    IReadOnlyList<FarmCultivationResponse> Cultivations,
    IReadOnlyList<FarmSensorResponse> Sensors);

public sealed record FarmResponse(
    string StableId,
    long Revision,
    string FarmName,
    string StatusCode,
    IReadOnlyList<FarmPlotResponse> Plots);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Contract,
    "인증 생산자에게 자신이 소유한 농장·재배·센서의 서버 판정 결과만 World projection으로 제공한다.",
    FlowOrder = 10,
    Boundary = "소유자 ID, 좌표, 주소와 연락처를 제외한다. 공개 작물 기준과 운영 재배 상태를 별도 필드로 유지하며 Unity는 센서 원시값을 재판정하지 않는다.")]
public sealed record FarmProducerPerspectiveResponse(
    string StableId,
    long Revision,
    string AuthorizedRoleCode,
    string WorldZoneCode,
    string ViewerScopeCode,
    string SourceTypeCode,
    string AuthorizationDecisionId,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<FarmResponse> Farms,
    IReadOnlyList<NpcMovementResponse> Workers);
