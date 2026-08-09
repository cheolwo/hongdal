using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Application.Driver.Transport;

namespace Ssalddel.Application.WorldProjection;

/// <summary>
/// 기사 관점과 창고 관점이 동일한 운송·입고 원장 상태를 동일한 stable ID 관계로 투영하도록 합니다.
/// 좌표, 주소, 연락처, 운임과 주문 식별자는 만들지 않습니다.
/// </summary>
public static class CargoWarehouseHandoffProjectionBuilder
{
    private const string TransportNetworkZone = "transport-network";
    private const string WarehouseZone = "warehouse";

    public static CargoWarehouseHandoffResponse? Build(
        long transportId,
        string transportStatus,
        DateTime transportUpdatedAt,
        long inboundId,
        string inboundStatus,
        DateTime inboundUpdatedAt,
        DateTimeOffset generatedAt)
    {
        var workflow = ResolveWorkflow(transportStatus, inboundStatus);
        if (workflow is null)
        {
            return null;
        }

        var revision = Math.Max(transportUpdatedAt.Ticks, inboundUpdatedAt.Ticks);
        return new CargoWarehouseHandoffResponse
        {
            StableId = $"cargo-handoff:transport-{transportId}.inbound-{inboundId}",
            Revision = revision,
            HandoffStateCode = workflow.Value.StateCode,
            CargoStableId = $"cargo:transport-{transportId}",
            TransportTaskStableId = $"transport-task:{transportId}",
            InboundTaskStableId = $"inbound-task:{inboundId}",
            Movements = workflow.Value.Movements
                .Select(item => Movement(transportId, inboundId, revision, generatedAt, item))
                .ToArray(),
            GeneratedAt = generatedAt,
        };
    }

    private static Workflow? ResolveWorkflow(string transportStatus, string inboundStatus)
    {
        if (transportStatus is 기사운송상태코드.상차완료 or 기사운송상태코드.운송중)
        {
            return new Workflow(CargoHandoffStateCodes.InTransit,
            [
                new MovementIntent("transporter", RolePerspectiveRoleCodes.Transporter,
                    TransportNetworkZone, "transport-network-hub-delivery",
                    "network.logistics-center", "network.warehouse",
                    NpcMovementStateCodes.Moving, "arrive-at-warehouse")
            ]);
        }

        if (!string.Equals(transportStatus, 기사운송상태코드.하차지도착, StringComparison.Ordinal))
        {
            return null;
        }

        if (string.Equals(inboundStatus, 입고상태코드.완료, StringComparison.Ordinal))
        {
            return new Workflow(CargoHandoffStateCodes.ReceivingCompleted,
            [
                new MovementIntent("transporter", RolePerspectiveRoleCodes.Transporter,
                    WarehouseZone, "warehouse-transporter-dropoff",
                    "warehouse.inbound-dock", "warehouse.vehicle-exit",
                    NpcMovementStateCodes.Moving, "depart-warehouse"),
                new MovementIntent("inbound-worker", "WarehouseInboundWorker",
                    WarehouseZone, "warehouse-inbound-worker-handoff",
                    "warehouse.inspection-zone", "warehouse.storage-zone",
                    NpcMovementStateCodes.Moving, "store-cargo")
            ]);
        }

        return new Workflow(CargoHandoffStateCodes.ArrivedAtWarehouse,
        [
            new MovementIntent("transporter", RolePerspectiveRoleCodes.Transporter,
                WarehouseZone, "warehouse-transporter-dropoff",
                "warehouse.approach", "warehouse.inbound-dock",
                NpcMovementStateCodes.Moving, "open-cargo-door"),
            new MovementIntent("inbound-worker", "WarehouseInboundWorker",
                WarehouseZone, "warehouse-inbound-worker-handoff",
                "warehouse.staff-entry", "warehouse.inbound-dock",
                NpcMovementStateCodes.Moving, "unload-cargo")
        ]);
    }

    private static NpcMovementResponse Movement(
        long transportId,
        long inboundId,
        long revision,
        DateTimeOffset generatedAt,
        MovementIntent intent)
        => new()
        {
            StableId = $"npc-movement:{intent.ActorKey}.transport-{transportId}.inbound-{inboundId}",
            Revision = revision,
            NpcStableId = intent.ActorKey == "transporter"
                ? $"npc:transport-driver.{transportId}"
                : $"npc:warehouse-inbound-worker.{inboundId}",
            ActorRoleCode = intent.RoleCode,
            WorldZoneCode = intent.ZoneCode,
            RouteCode = intent.RouteCode,
            CurrentWaypointKey = intent.CurrentWaypoint,
            DestinationWaypointKey = intent.DestinationWaypoint,
            MovementStateCode = intent.StateCode,
            ArrivalActionCode = intent.ArrivalAction,
            SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection,
            CanonicalTaskStableId = intent.ActorKey == "transporter"
                ? $"transport-task:{transportId}"
                : $"inbound-task:{inboundId}",
            GeneratedAt = generatedAt,
        };

    private readonly record struct Workflow(
        string StateCode,
        IReadOnlyList<MovementIntent> Movements);

    private readonly record struct MovementIntent(
        string ActorKey,
        string RoleCode,
        string ZoneCode,
        string RouteCode,
        string CurrentWaypoint,
        string DestinationWaypoint,
        string StateCode,
        string ArrivalAction);
}
