using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.Warehouse;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class WarehouseInboundHandoffVerticalSliceTests
{
    private static readonly DateTimeOffset GeneratedAt =
        DateTimeOffset.Parse("2026-08-08T15:00:00+09:00");

    [Fact]
    public void Dock도착은_차량화물운송자입고작업자를_같은Canonical관계로결합한다()
    {
        var snapshot = Map(CargoHandoffStateCodes.ArrivedAtWarehouse);
        var relation = "inbound-task:91";
        var related = snapshot.Objects
            .Where(item => item.CanonicalRelationStableId == relation)
            .ToArray();

        Assert.Contains(related, item => item.Kind == "Vehicle" && item.LocationCode == WarehouseLocationSocketKeys.InboundDock);
        Assert.Contains(related, item => item.Kind == "Cargo" && item.LocationCode == WarehouseLocationSocketKeys.InboundDock);
        Assert.Contains(related, item => item.Kind == "Task" && item.StableId == "warehouse-putaway:32");
        Assert.Contains(related, item => item.Kind == "Npc" && item.Title == "DockWorker" && item.LocationCode == WarehouseLocationSocketKeys.InboundDock);
        Assert.Contains(related, item => item.Kind == "Npc" && item.Title == "Transporter");

        var selection = new WarehouseRelationResolver().Select(snapshot, "cargo:transport-71");
        Assert.Contains(selection.Related, item => item.Kind == "Vehicle");
        Assert.Contains(selection.Related, item => item.StableId == "warehouse-putaway:32");
        Assert.Contains(selection.Related, item => item.Title == "DockWorker");
    }

    [Fact]
    public void 입고완료는_화물을Storage로_차량을Exit로_입고작업자를Storage경로로옮긴다()
    {
        var snapshot = Map(CargoHandoffStateCodes.ReceivingCompleted);

        Assert.Equal(WarehouseLocationSocketKeys.StorageZone,
            snapshot.Objects.Single(item => item.Kind == "Cargo").LocationCode);
        Assert.Equal(WarehouseLocationSocketKeys.VehicleExit,
            snapshot.Objects.Single(item => item.Kind == "Vehicle").LocationCode);
        var dockWorker = snapshot.Objects.Single(item => item.StableId == "warehouse-npc:dock-worker:32");
        Assert.Equal(WarehouseLocationSocketKeys.InspectionZone, dockWorker.CurrentLocationCode);
        Assert.Equal(WarehouseLocationSocketKeys.StorageZone, dockWorker.LocationCode);
    }

    [Fact]
    public void 운송중은_창고Approach에예정점유를표시하고_창고Npc를추가하지않는다()
    {
        var snapshot = Map(CargoHandoffStateCodes.InTransit);

        Assert.Equal(WarehouseLocationSocketKeys.Approach,
            snapshot.Objects.Single(item => item.Kind == "Cargo").LocationCode);
        Assert.Equal(WarehouseLocationSocketKeys.Approach,
            snapshot.Objects.Single(item => item.Kind == "Vehicle").LocationCode);
        Assert.DoesNotContain(snapshot.Objects, item => item.StableId == "npc:transport-driver.71");
    }

    [Fact]
    public void Handoff가있으면_PutAway의Canonical입고참조가필수다()
    {
        var source = Api(CargoHandoffStateCodes.ArrivedAtWarehouse);
        source.Tasks[0].CanonicalTaskStableId = string.Empty;

        var error = Assert.Throws<InvalidOperationException>(() => new WarehouseWorldMapper().Map(source));

        Assert.Equal("WarehouseTaskCanonicalReferenceInvalid:warehouse-putaway:32", error.Message);
    }

    private static WarehouseWorldSnapshot Map(string state)
        => new WarehouseWorldMapper().Map(Api(state));

    private static WarehouseWorldSnapshotApiModel Api(string state)
        => new()
        {
            StableId = "warehouse-zone:7",
            Revision = "warehouse-revision-1",
            GeneratedAtUtc = GeneratedAt,
            TotalAvailableQuantity = 4,
            InventoryItems =
            [
                new WarehouseWorldInventoryItemApiModel
                {
                    StableId = "warehouse-inventory:32",
                    WarehouseStableId = "warehouse:7",
                    WarehouseName = "도심 창고",
                    ProductName = "양파",
                    Sku = "ONION-10",
                    AvailableQuantity = 4,
                    Status = "검수완료",
                    UpdatedAtUtc = GeneratedAt,
                },
            ],
            Tasks =
            [
                new WarehouseWorldTaskApiModel
                {
                    StableId = "warehouse-putaway:32",
                    WarehouseStableId = "warehouse:7",
                    InventoryItemStableId = "warehouse-inventory:32",
                    CanonicalTaskStableId = "inbound-task:91",
                    TaskKind = "PutAway",
                    ProductName = "양파",
                    Sku = "ONION-10",
                    Quantity = 4,
                    Status = "검수완료",
                    CanExecute = true,
                    UpdatedAtUtc = GeneratedAt,
                },
            ],
            Npcs =
            [
                new WarehouseWorldNpcApiModel
                {
                    StableId = "warehouse-npc:dock-worker:32",
                    WarehouseStableId = "warehouse:7",
                    SourceTaskStableId = "warehouse-putaway:32",
                    RoleCode = "DockWorker",
                    RouteCode = "warehouse.inbound-to-storage",
                    CurrentWaypointKey = WarehouseLocationSocketKeys.InboundDock,
                    DestinationWaypointKey = WarehouseLocationSocketKeys.StorageZone,
                    ActivityCode = "PutAway",
                },
            ],
            InboundHandoffs = [Handoff(state)],
        };

    private static CargoWarehouseHandoffApiModel Handoff(string state)
    {
        var inTransit = state == CargoHandoffStateCodes.InTransit;
        var completed = state == CargoHandoffStateCodes.ReceivingCompleted;
        var movements = inTransit
            ? new[]
            {
                Movement("npc:transport-driver.71", "Transporter", "transport-network",
                    "network.logistics-center", "network.warehouse", "transport-task:71", "arrive-at-warehouse"),
            }
            : new[]
            {
                Movement("npc:transport-driver.71", "Transporter", "warehouse",
                    completed ? "warehouse.inbound-dock" : "warehouse.approach",
                    completed ? "warehouse.vehicle-exit" : "warehouse.inbound-dock",
                    "transport-task:71", completed ? "depart-warehouse" : "open-cargo-door"),
                Movement("npc:warehouse-inbound-worker.91", "WarehouseInboundWorker", "warehouse",
                    completed ? "warehouse.inspection-zone" : "warehouse.staff-entry",
                    completed ? "warehouse.storage-zone" : "warehouse.inbound-dock",
                    "inbound-task:91", completed ? "store-cargo" : "unload-cargo"),
            };
        return new CargoWarehouseHandoffApiModel
        {
            StableId = "cargo-handoff:transport-71.inbound-91",
            Revision = completed ? 3 : inTransit ? 1 : 2,
            HandoffStateCode = state,
            CargoStableId = "cargo:transport-71",
            TransportTaskStableId = "transport-task:71",
            InboundTaskStableId = "inbound-task:91",
            Movements = movements,
            GeneratedAt = GeneratedAt,
        };
    }

    private static NpcMovementApiModel Movement(
        string npcStableId,
        string role,
        string zone,
        string current,
        string destination,
        string task,
        string action)
        => new()
        {
            StableId = "npc-movement:" + npcStableId.Replace(':', '-'),
            Revision = 1,
            NpcStableId = npcStableId,
            ActorRoleCode = role,
            WorldZoneCode = zone,
            RouteCode = zone == "transport-network"
                ? "transport-network-hub-delivery"
                : role == "Transporter"
                    ? "warehouse-transporter-dropoff"
                    : "warehouse-inbound-worker-handoff",
            CurrentWaypointKey = current,
            DestinationWaypointKey = destination,
            MovementStateCode = NpcMovementStateCodes.Moving,
            ArrivalActionCode = action,
            SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection,
            CanonicalTaskStableId = task,
            GeneratedAt = GeneratedAt,
        };
}
