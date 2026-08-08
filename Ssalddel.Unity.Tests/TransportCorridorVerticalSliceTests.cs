using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.Transport;

namespace Ssalddel.Tests.UnityData;

public sealed class TransportCorridorVerticalSliceTests
{
    [Fact]
    public async Task 운송중_화물인계를_TruckCorridor로_투영한다()
    {
        var useCase = UseCase(Handoff(CargoHandoffStateCodes.InTransit));

        var result = await useCase.실행Async();

        Assert.NotNull(result);
        Assert.Equal("truck-projection:cargo:transport-71", result.Truck.StableId);
        Assert.Equal("network.logistics-center", result.Truck.CurrentNodeKey);
        Assert.Equal("network.warehouse", result.Truck.DestinationNodeKey);
        Assert.Equal("transport-task:71", result.Truck.CanonicalTaskStableId);
    }

    [Fact]
    public async Task 창고도착이후에는_TransportCorridor를_비활성화한다()
    {
        Assert.Null(await UseCase(Handoff(CargoHandoffStateCodes.ArrivedAtWarehouse)).실행Async());
        Assert.Null(await UseCase(null).실행Async());
    }

    [Fact]
    public void Projector는_운송중인데_TransporterMovement가_없으면_거부한다()
    {
        var handoff = Map(Handoff(CargoHandoffStateCodes.InTransit)!);
        handoff.Movements = Array.Empty<NpcMovementSnapshot>();

        Assert.Equal("TransportCorridorMovementMissing",
            Assert.Throws<InvalidOperationException>(() => new TransportCorridorProjector().Project(handoff)).Message);
    }

    [Fact]
    public void TruckApplicator는_StableId와_revision을_검증한다()
    {
        var snapshot = new TransportCorridorProjector().Project(Map(Handoff(CargoHandoffStateCodes.InTransit)!))!;
        var target = new Target(snapshot.Truck.StableId);
        var applicator = new TruckMovementApplicator();

        Assert.True(applicator.Apply(snapshot, target));
        snapshot.Revision--;
        Assert.False(applicator.Apply(snapshot, target));
        Assert.Throws<InvalidOperationException>(() => applicator.Apply(snapshot, new Target("truck-projection:other")));
    }

    private static TransportCorridorQueryUseCase UseCase(CargoWarehouseHandoffApiModel? response)
    {
        var repository = new CargoWarehouseHandoffApiRepository(new Client(response), new CargoWarehouseHandoffMapper(new NpcMovementMapper()));
        return new TransportCorridorQueryUseCase(new CargoWarehouseHandoffQueryUseCase(repository), new TransportCorridorProjector());
    }

    private static CargoWarehouseHandoffSnapshot Map(CargoWarehouseHandoffApiModel source)
        => new CargoWarehouseHandoffMapper(new NpcMovementMapper()).Map(source);

    private static CargoWarehouseHandoffApiModel? Handoff(string state)
    {
        var transit = state == CargoHandoffStateCodes.InTransit;
        return new CargoWarehouseHandoffApiModel
        {
            StableId = "cargo-handoff:transport-71.inbound-91", Revision = 5, HandoffStateCode = state,
            CargoStableId = "cargo:transport-71", TransportTaskStableId = "transport-task:71", InboundTaskStableId = "inbound-task:91",
            GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            Movements = transit
                ? new[] { Movement("npc:transport-driver.71", "Transporter", "transport-network", "transport-network-hub-delivery", "network.logistics-center", "network.warehouse", "transport-task:71") }
                : new[]
                {
                    Movement("npc:transport-driver.71", "Transporter", "warehouse", "warehouse-transporter-dropoff", "warehouse.approach", "warehouse.inbound-dock", "transport-task:71"),
                    Movement("npc:warehouse-inbound-worker.91", "WarehouseInboundWorker", "warehouse", "warehouse-inbound-worker-handoff", "warehouse.staff-entry", "warehouse.inbound-dock", "inbound-task:91"),
                },
        };
    }

    private static NpcMovementApiModel Movement(string npc, string role, string zone, string route, string current, string destination, string task)
        => new() { StableId = "npc-movement:" + npc.Replace(':', '-'), Revision = 5, NpcStableId = npc, ActorRoleCode = role, WorldZoneCode = zone, RouteCode = route, CurrentWaypointKey = current, DestinationWaypointKey = destination, MovementStateCode = NpcMovementStateCodes.Moving, ArrivalActionCode = "arrive", SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection, CanonicalTaskStableId = task, GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z") };

    private sealed class Client(CargoWarehouseHandoffApiModel? response) : ICargoWarehouseHandoffApiClient
    { public Task<CargoWarehouseHandoffApiModel?> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(response); }
    private sealed class Target(string stableId) : ITruckMovementTarget
    { public string TruckStableId { get; } = stableId; public TruckMovementSnapshot? Last { get; private set; } public void ApplyTruckMovement(TruckMovementSnapshot movement) => Last = movement; }
}
