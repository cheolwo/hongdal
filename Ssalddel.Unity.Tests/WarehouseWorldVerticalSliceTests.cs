using Ssalddel.Unity.Warehouse;

namespace Ssalddel.Tests.UnityData;

public sealed class WarehouseWorldVerticalSliceTests
{
    [Fact]
    public async Task AuthorizedApi는_Repository와UseCase를통해_재고작업Npc로_투영된다()
    {
        var client = new SequenceClient(Snapshot("revision-1"));
        var useCase = UseCase(client);

        var result = await useCase.실행Async(7);

        Assert.Equal("warehouse-zone:7", result.StableId);
        Assert.Equal(5, result.Objects.Length);
        Assert.Contains(result.Objects, item => item.Kind == "Inventory" && item.StableId == "warehouse-inventory:31");
        Assert.Contains(result.Objects, item => item.Kind == "Task" && item.CanExecute);
        Assert.Contains(result.Objects, item => item.Kind == "Npc" && item.LocationCode == "warehouse.storage-zone");
        Assert.Equal(WarehouseWorldApiRoutes.AuthorizedSnapshot, "api/v1/warehouse-operations/world/zones/warehouse");
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public void Mapper는_PutAway작업의_재고참조를_검증한다()
    {
        var source = Snapshot("revision-1");
        source.Tasks[0].InventoryItemStableId = "warehouse-inventory:missing";

        var error = Assert.Throws<InvalidOperationException>(() => new WarehouseWorldMapper().Map(source));

        Assert.Equal("WarehouseTaskInventoryUnknown:warehouse-putaway:31", error.Message);
    }

    [Fact]
    public void Mapper는_Npc의_작업참조와_중복StableId를_검증한다()
    {
        var unknownTask = Snapshot("revision-1");
        unknownTask.Npcs[0].SourceTaskStableId = "warehouse-putaway:missing";
        Assert.Equal("WarehouseNpcTaskUnknown:warehouse-npc:dock-worker:31",
            Assert.Throws<InvalidOperationException>(() => new WarehouseWorldMapper().Map(unknownTask)).Message);

        var duplicate = Snapshot("revision-1");
        duplicate.InventoryItems = new[] { duplicate.InventoryItems[0], duplicate.InventoryItems[0] };
        Assert.Equal("DuplicateWarehouseInventory:warehouse-inventory:31",
            Assert.Throws<InvalidOperationException>(() => new WarehouseWorldMapper().Map(duplicate)).Message);
    }

    [Fact]
    public async Task 최초실패는_빈상태이고_갱신실패는_마지막성공Snapshot을_유지한다()
    {
        var initial = new WarehouseWorldLoadCoordinator(UseCase(new SequenceClient(new InvalidOperationException("offline"))), new WarehouseWorldReconciler());
        var initialFailure = await initial.LoadAsync(7);
        var coordinator = new WarehouseWorldLoadCoordinator(UseCase(new SequenceClient(Snapshot("revision-1"), new InvalidOperationException("refresh-offline"))), new WarehouseWorldReconciler());
        var success = await coordinator.LoadAsync(7);
        var refreshFailure = await coordinator.LoadAsync(7);

        Assert.Equal(WarehouseWorldLoadStateCodes.InitialLoadError, initialFailure.StateCode);
        Assert.Null(initialFailure.Snapshot);
        Assert.Equal(WarehouseWorldLoadStateCodes.Success, success.StateCode);
        Assert.Equal(WarehouseWorldLoadStateCodes.RefreshError, refreshFailure.StateCode);
        Assert.Same(success.Snapshot, refreshFailure.Snapshot);
    }

    [Fact]
    public async Task 갱신은_StableId기준_추가갱신제거를_계산한다()
    {
        var first = Snapshot("revision-1");
        var second = Snapshot("revision-2");
        second.InventoryItems[0].AvailableQuantity = 9;
        second.Tasks = Array.Empty<WarehouseWorldTaskApiModel>();
        second.Npcs = Array.Empty<WarehouseWorldNpcApiModel>();
        second.InventoryItems = second.InventoryItems.Append(new WarehouseWorldInventoryItemApiModel
        {
            StableId = "warehouse-inventory:32", WarehouseStableId = "warehouse:7", WarehouseName = "도심 창고",
            ProductName = "양파", Sku = "ONION-10", AvailableQuantity = 4, Status = "적재완료",
            UpdatedAtUtc = DateTimeOffset.Parse("2026-08-08T02:00:00Z"),
        }).ToArray();
        var coordinator = new WarehouseWorldLoadCoordinator(UseCase(new SequenceClient(first, second)), new WarehouseWorldReconciler());

        await coordinator.LoadAsync(7);
        var refreshed = await coordinator.LoadAsync(7);

        Assert.Single(refreshed.Changes!.Added);
        Assert.Single(refreshed.Changes.Updated);
        Assert.Equal(4, refreshed.Changes.Removed.Length);
    }

    private static WarehouseWorldQueryUseCase UseCase(IWarehouseWorldApiClient client)
        => new(new WarehouseWorldApiRepository(client, new WarehouseWorldMapper()));

    private static WarehouseWorldSnapshotApiModel Snapshot(string revision)
        => new()
        {
            StableId = "warehouse-zone:7", Revision = revision, GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            TotalAvailableQuantity = 12, TotalReservedQuantity = 3,
            InventoryItems = new[]
            {
                new WarehouseWorldInventoryItemApiModel
                {
                    StableId = "warehouse-inventory:31", WarehouseStableId = "warehouse:7", WarehouseName = "도심 창고",
                    ProductName = "감자", Sku = "POTATO-20", OptionName = "20kg", AvailableQuantity = 12,
                    ReservedQuantity = 3, StorageLocation = "A-01", Status = "검수완료",
                    UpdatedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            },
            Tasks = new[]
            {
                new WarehouseWorldTaskApiModel
                {
                    StableId = "warehouse-putaway:31", WarehouseStableId = "warehouse:7", InventoryItemStableId = "warehouse-inventory:31",
                    TaskKind = "PutAway", ProductName = "감자", Sku = "POTATO-20", Quantity = 12, Status = "검수완료", CanExecute = true,
                    UpdatedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
                new WarehouseWorldTaskApiModel
                {
                    StableId = "warehouse-picking:abcdef", WarehouseStableId = "warehouse:7", TaskKind = "Picking",
                    ProductName = "감자", Sku = "POTATO-20", Quantity = 3, LocationCode = "A-01", Status = "대기", CanExecute = true,
                    UpdatedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            },
            Npcs = new[]
            {
                new WarehouseWorldNpcApiModel
                {
                    StableId = "warehouse-npc:dock-worker:31", WarehouseStableId = "warehouse:7", SourceTaskStableId = "warehouse-putaway:31",
                    RoleCode = "DockWorker", RouteCode = "warehouse.inbound-to-storage", CurrentWaypointKey = "warehouse.inbound-dock",
                    DestinationWaypointKey = "warehouse.storage-zone", ActivityCode = "PutAway",
                },
                new WarehouseWorldNpcApiModel
                {
                    StableId = "warehouse-npc:picker:abcdef", WarehouseStableId = "warehouse:7", SourceTaskStableId = "warehouse-picking:abcdef",
                    RoleCode = "Picker", RouteCode = "warehouse.rack-to-outbound", CurrentWaypointKey = "warehouse.rack-zone",
                    DestinationWaypointKey = "warehouse.outbound-staging", ActivityCode = "Picking",
                },
            },
        };

    private sealed class SequenceClient : IWarehouseWorldApiClient
    {
        private readonly Queue<object> responses;
        public SequenceClient(params object[] responses) => this.responses = new Queue<object>(responses);
        public int CallCount { get; private set; }
        public Task<WarehouseWorldSnapshotApiModel> GetAsync(long warehouseId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested(); CallCount++; var response = responses.Dequeue();
            return response is Exception error ? Task.FromException<WarehouseWorldSnapshotApiModel>(error) : Task.FromResult((WarehouseWorldSnapshotApiModel)response);
        }
    }
}
