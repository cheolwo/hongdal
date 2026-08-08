using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.WorldProjection;

public static class NpcMovementRoutes
{
    public const string DriverUrbanLogisticsCenter =
        RolePerspectiveRoutes.DriverUrbanLogisticsCenter + "/npc-movement";

    public const string DriverWarehouseHandoff =
        "api/v1/driver/world/workflows/warehouse-handoff";
}

public static class NpcMovementSourceTypeCodes
{
    public const string OperationalProjection = "OperationalProjection";
}

public static class NpcMovementStateCodes
{
    public const string Moving = "Moving";
    public const string PerformingAction = "PerformingAction";
}

public static class CargoHandoffStateCodes
{
    public const string InTransit = "InTransit";
    public const string ArrivedAtWarehouse = "ArrivedAtWarehouse";
    public const string ReceivingCompleted = "ReceivingCompleted";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Contract,
    "운영 업무 상태를 Unity semantic waypoint 기반 NPC 이동 의도로 전달한다.",
    FlowOrder = 40,
    Boundary = "Unity 좌표와 개인정보를 포함하지 않으며 NPC 도착은 canonical 업무 완료를 의미하지 않는다.")]
public sealed class NpcMovementResponse
{
    public string StableId { get; set; } = string.Empty;

    public long Revision { get; set; }

    public string NpcStableId { get; set; } = string.Empty;

    public string ActorRoleCode { get; set; } = string.Empty;

    public string WorldZoneCode { get; set; } = string.Empty;

    public string RouteCode { get; set; } = string.Empty;

    public string CurrentWaypointKey { get; set; } = string.Empty;

    public string DestinationWaypointKey { get; set; } = string.Empty;

    public string MovementStateCode { get; set; } = string.Empty;

    public string ArrivalActionCode { get; set; } = string.Empty;

    public string SourceTypeCode { get; set; } = string.Empty;

    public string CanonicalTaskStableId { get; set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; set; }
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Contract,
    "운송 중인 화물이 창고 NPC에게 인계되는 read-only World workflow를 제공한다.",
    FlowOrder = 40,
    Boundary = "운송과 입고의 canonical 상태만 결합하며 NPC 도착 또는 animation으로 하차·입고 완료를 만들지 않는다.")]
public sealed class CargoWarehouseHandoffResponse
{
    public string StableId { get; set; } = string.Empty;

    public long Revision { get; set; }

    public string HandoffStateCode { get; set; } = string.Empty;

    public string CargoStableId { get; set; } = string.Empty;

    public string TransportTaskStableId { get; set; } = string.Empty;

    public string InboundTaskStableId { get; set; } = string.Empty;

    public IReadOnlyList<NpcMovementResponse> Movements { get; set; } = [];

    public DateTimeOffset GeneratedAt { get; set; }
}
