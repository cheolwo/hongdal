using Ssalddel.Unity.Warehouse;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class WarehouseDataFlowMigrationTests
{
    [Fact]
    public async Task QueryUseCase는_DataRepository다음에_Interpreter를실행한다()
    {
        var client = new SingleSnapshotClient(Snapshot("revision-1"));
        var repository = new WarehouseApiDataRepository(client, new WarehouseDataMapper());
        var useCase = new WarehouseWorldQueryUseCase(repository, new WarehouseWorldInterpreter());

        var result = await useCase.실행Async(7);

        Assert.Equal(1, client.CallCount);
        Assert.Equal("revision-1", result.Lineage!.Inputs.Items[0].Revision);
        Assert.Contains(result.Objects, item => item.Kind == "Inventory");
    }

    [Fact]
    public void DataMapper와Interpreter는_사실과World의미와Revision을분리한다()
    {
        var api = Snapshot("revision-1");
        var data = new WarehouseDataMapper().Map(api);
        var interpreted = new WarehouseWorldInterpreter().Interpret(data);
        var compatible = new WarehouseWorldMapper().Map(api);

        Assert.Equal("revision-1", data.DataRevision);
        Assert.Equal("A-01", data.InventoryItems[0].StorageLocation);
        Assert.Equal("Inventory", interpreted.Objects[0].Kind);
        Assert.Equal("A-01", interpreted.Objects[0].LocationCode);
        Assert.NotNull(interpreted.Lineage);
        Assert.Equal("revision-1", interpreted.Lineage!.Inputs.Items[0].Revision);
        Assert.StartsWith("interpretation:", interpreted.Lineage.InterpretationRevision);
        Assert.Equal(interpreted.Lineage.InterpretationRevision, compatible.Lineage!.InterpretationRevision);
        Assert.Equal(interpreted.Objects.Select(item => item.StableId), compatible.Objects.Select(item => item.StableId));
    }

    [Fact]
    public void DataRevision변경은_InterpretationRevision을변경한다()
    {
        var mapper = new WarehouseDataMapper();
        var interpreter = new WarehouseWorldInterpreter();

        var first = interpreter.Interpret(mapper.Map(Snapshot("revision-1")));
        var second = interpreter.Interpret(mapper.Map(Snapshot("revision-2")));

        Assert.NotEqual(first.Lineage!.InterpretationRevision, second.Lineage!.InterpretationRevision);
    }

    [Fact]
    public void Presenter는_위치관계상세와PresentationRevision을_View밖에서결정한다()
    {
        var world = new WarehouseWorldMapper().Map(Snapshot("revision-1"));
        var presenter = new WarehousePresenter(
            new WarehouseLocationResolver(),
            new WarehouseRelationResolver());
        var result = new WarehouseWorldLoadResult
        {
            StateCode = WarehouseWorldLoadStateCodes.Success,
            Snapshot = world,
            Changes = new WarehouseWorldReconciler().Reconcile(
                Array.Empty<WarehouseWorldObject>(),
                world.Objects),
        };

        var presentation = presenter.Present(result);
        var inventory = presentation.Snapshot!.Items.Single(item => item.Kind == "Inventory");

        Assert.Equal(WarehouseLocationSocketKeys.RackZone, inventory.SocketKey);
        Assert.Equal(
            new[] { "warehouse-putaway:31", "warehouse-npc:dock-worker:31" },
            inventory.RelatedStableIds);
        Assert.Contains("물리 팔레트 수가 아님", inventory.DetailText);
        Assert.StartsWith("presentation:", presentation.Snapshot.PresentationRevision);
        Assert.Equal(world.Lineage!.InterpretationRevision, presentation.Snapshot.InterpretationRevision);
        Assert.Equal("재고 12 · 예약 3", presentation.StatusMessage);
    }

    private static WarehouseWorldSnapshotApiModel Snapshot(string revision)
        => new()
        {
            StableId = "warehouse-zone:7",
            Revision = revision,
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            TotalAvailableQuantity = 12,
            TotalReservedQuantity = 3,
            InventoryItems = new[]
            {
                new WarehouseWorldInventoryItemApiModel
                {
                    StableId = "warehouse-inventory:31",
                    WarehouseStableId = "warehouse:7",
                    WarehouseName = "도심 창고",
                    ProductName = "감자",
                    Sku = "POTATO-20",
                    OptionName = "20kg",
                    AvailableQuantity = 12,
                    ReservedQuantity = 3,
                    StorageLocation = "A-01",
                    Status = "검수완료",
                    UpdatedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            },
            Tasks = new[]
            {
                new WarehouseWorldTaskApiModel
                {
                    StableId = "warehouse-putaway:31",
                    WarehouseStableId = "warehouse:7",
                    InventoryItemStableId = "warehouse-inventory:31",
                    TaskKind = "PutAway",
                    ProductName = "감자",
                    Sku = "POTATO-20",
                    Quantity = 12,
                    Status = "검수완료",
                    CanExecute = true,
                    UpdatedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            },
            Npcs = new[]
            {
                new WarehouseWorldNpcApiModel
                {
                    StableId = "warehouse-npc:dock-worker:31",
                    WarehouseStableId = "warehouse:7",
                    SourceTaskStableId = "warehouse-putaway:31",
                    RoleCode = "DockWorker",
                    RouteCode = "warehouse-inbound",
                    CurrentWaypointKey = "warehouse.inbound-dock",
                    DestinationWaypointKey = "warehouse.storage-zone",
                    ActivityCode = "PutAway",
                },
            },
        };

    private sealed class SingleSnapshotClient : IWarehouseWorldApiClient
    {
        private readonly WarehouseWorldSnapshotApiModel snapshot;

        public SingleSnapshotClient(WarehouseWorldSnapshotApiModel snapshot)
            => this.snapshot = snapshot;

        public int CallCount { get; private set; }

        public Task<WarehouseWorldSnapshotApiModel> GetAsync(
            long warehouseId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(snapshot);
        }
    }
}
