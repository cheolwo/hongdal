using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Driver.Transport;

namespace Ssalddel.Application.Driver.Transport;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Application,
    "현재 기사의 운송과 연계 입고를 운송 NPC·창고 NPC 화물 인계 workflow로 투영한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(CargoWarehouseHandoffResponse),
    FlowOrder = 50,
    Boundary = "배정 운송과 연결된 입고만 조회하며 주소·연락처·상품 상세와 Unity 좌표를 반환하지 않는다.")]
public sealed class 기사창고화물인계조회QueryHandler
    : IRequestHandler<기사창고화물인계조회Query, CargoWarehouseHandoffResponse?>
{
    private const string TransportNetworkZone = "transport-network";
    private const string WarehouseZone = "warehouse";

    private readonly IRequestHandler<운송현재조회Query, 기사운송요약응답?> currentTransportReader;
    private readonly IRequestHandler<운송연계입고조회Query, 운송연계입고Projection?> inboundReader;

    public 기사창고화물인계조회QueryHandler(
        IRequestHandler<운송현재조회Query, 기사운송요약응답?> currentTransportReader,
        IRequestHandler<운송연계입고조회Query, 운송연계입고Projection?> inboundReader)
    {
        this.currentTransportReader = currentTransportReader;
        this.inboundReader = inboundReader;
    }

    public async Task<CargoWarehouseHandoffResponse?> Handle(
        기사창고화물인계조회Query request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.기사Id);

        var transport = await currentTransportReader.Handle(
            new 운송현재조회Query(request.기사Id), cancellationToken);
        if (transport is null)
        {
            return null;
        }

        var inbound = await inboundReader.Handle(
            new 운송연계입고조회Query(transport.운송번호), cancellationToken);
        if (inbound is null)
        {
            return null;
        }

        var workflow = ResolveWorkflow(transport, inbound);
        if (workflow is null)
        {
            return null;
        }

        var revision = Math.Max(transport.UpdatedAt.Ticks, inbound.UpdatedAt.Ticks);
        var generatedAt = DateTimeOffset.UtcNow;
        return new CargoWarehouseHandoffResponse
        {
            StableId = $"cargo-handoff:transport-{transport.Id}.inbound-{inbound.Id}",
            Revision = revision,
            HandoffStateCode = workflow.Value.StateCode,
            CargoStableId = $"cargo:transport-{transport.Id}",
            TransportTaskStableId = $"transport-task:{transport.Id}",
            InboundTaskStableId = $"inbound-task:{inbound.Id}",
            Movements = workflow.Value.Movements
                .Select(item => Movement(transport.Id, inbound.Id, revision, generatedAt, item))
                .ToArray(),
            GeneratedAt = generatedAt,
        };
    }

    private static Workflow? ResolveWorkflow(
        기사운송요약응답 transport,
        운송연계입고Projection inbound)
    {
        if (transport.상태 is 기사운송상태코드.상차완료 or 기사운송상태코드.운송중)
        {
            return new Workflow(CargoHandoffStateCodes.InTransit,
            [
                new MovementIntent("transporter", RolePerspectiveRoleCodes.Transporter,
                    TransportNetworkZone, "transport-network-hub-delivery",
                    "network.logistics-center", "network.warehouse",
                    NpcMovementStateCodes.Moving, "arrive-at-warehouse")
            ]);
        }

        if (!string.Equals(transport.상태, 기사운송상태코드.하차지도착, StringComparison.Ordinal))
        {
            return null;
        }

        if (string.Equals(inbound.상태, 입고상태코드.완료, StringComparison.Ordinal))
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
    {
        var canonicalTask = intent.ActorKey == "transporter"
            ? $"transport-task:{transportId}"
            : $"inbound-task:{inboundId}";

        return new NpcMovementResponse
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
            CanonicalTaskStableId = canonicalTask,
            GeneratedAt = generatedAt,
        };
    }

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
