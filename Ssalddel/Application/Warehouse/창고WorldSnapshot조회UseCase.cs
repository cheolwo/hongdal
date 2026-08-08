using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Application.Warehouse;

public interface I창고WorldSnapshot조회UseCase
{
    Task<Result<WarehouseWorldSnapshotResponse>> 조회Async(
        long? warehouseId,
        CancellationToken cancellationToken);
}

public sealed class 창고WorldSnapshot조회UseCase(
    I재고현황UseCase inventoryReader,
    I적재작업UseCase putAwayReader,
    I피킹작업UseCase pickingReader) : I창고WorldSnapshot조회UseCase
{
    private const int MaximumItems = 50;

    public async Task<Result<WarehouseWorldSnapshotResponse>> 조회Async(
        long? warehouseId,
        CancellationToken cancellationToken)
    {
        if (warehouseId is <= 0)
        {
            return Result.Fail<WarehouseWorldSnapshotResponse>("WarehouseIdInvalid");
        }

        var inventoryResult = await inventoryReader.목록Async(new 창고재고현황목록조회요청
        {
            WarehouseId = warehouseId,
            Status = 창고재고조회상태코드.전체,
            Page = 0,
            PageSize = MaximumItems,
        }, cancellationToken);
        if (inventoryResult.IsFailed) return Failure(inventoryResult.Errors);

        var putAwayResult = await putAwayReader.목록Async(new 적재작업목록조회요청
        {
            WarehouseId = warehouseId,
            Status = 적재작업조회상태코드.대기,
            Page = 0,
            PageSize = MaximumItems,
        }, cancellationToken);
        if (putAwayResult.IsFailed) return Failure(putAwayResult.Errors);

        var pickingResult = await pickingReader.목록Async(new 피킹작업목록조회요청
        {
            WarehouseId = warehouseId,
            Status = 피킹작업조회상태코드.전체,
            Page = 0,
            PageSize = MaximumItems,
        }, cancellationToken);
        if (pickingResult.IsFailed) return Failure(pickingResult.Errors);

        var inventory = inventoryResult.Value.Items.Select(MapInventory).ToArray();
        var putAwayTasks = putAwayResult.Value.Items.Select(MapPutAway).ToArray();
        var pickingTasks = pickingResult.Value.Items.Select(MapPicking).ToArray();
        var tasks = putAwayTasks.Concat(pickingTasks).ToArray();
        var npcs = tasks
            .Where(task => task.Status is not "완료")
            .Select(MapNpc)
            .ToArray();

        return Result.Ok(new WarehouseWorldSnapshotResponse
        {
            StableId = warehouseId.HasValue
                ? $"warehouse-zone:{warehouseId.Value}"
                : "warehouse-zone:authorized",
            Revision = ComputeRevision(inventory, tasks),
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            TotalAvailableQuantity = inventoryResult.Value.TotalAvailableQuantity,
            TotalReservedQuantity = inventoryResult.Value.TotalReservedQuantity,
            UnassignedLocationCount = inventoryResult.Value.UnassignedLocationCount,
            InventoryItems = inventory,
            Tasks = tasks,
            Npcs = npcs,
        });
    }

    private static WarehouseWorldInventoryItemResponse MapInventory(창고재고현황목록항목응답 source)
        => new()
        {
            StableId = $"warehouse-inventory:{source.InboundItemId}",
            WarehouseStableId = $"warehouse:{source.WarehouseId}",
            WarehouseName = source.WarehouseName,
            ProductName = source.ProductName,
            Sku = source.Sku,
            OptionName = source.OptionName,
            AvailableQuantity = source.AvailableQuantity,
            ReservedQuantity = source.ReservedQuantity,
            StorageLocation = source.StorageLocation,
            Status = source.Status,
            HasCommunityLedger = source.HasCommunityLedger,
            UpdatedAtUtc = AsUtc(source.UpdatedAtUtc),
        };

    private static WarehouseWorldTaskResponse MapPutAway(적재작업목록항목응답 source)
        => new()
        {
            StableId = $"warehouse-putaway:{source.InboundItemId}",
            WarehouseStableId = $"warehouse:{source.WarehouseId}",
            InventoryItemStableId = $"warehouse-inventory:{source.InboundItemId}",
            TaskKind = "PutAway",
            ProductName = source.ProductName,
            Sku = source.Sku,
            Quantity = source.AvailableQuantity,
            LocationCode = source.StorageLocation,
            Status = source.InventoryStatus,
            CanExecute = source.CanPutAway,
            UpdatedAtUtc = AsUtc(source.UpdatedAtUtc),
        };

    private static WarehouseWorldTaskResponse MapPicking(피킹작업목록항목응답 source)
        => new()
        {
            StableId = "warehouse-picking:" + StableSuffix(source.TaskKey),
            WarehouseStableId = $"warehouse:{source.WarehouseId}",
            TaskKind = "Picking",
            ProductName = source.ProductName,
            Sku = source.Sku,
            Quantity = source.Quantity,
            LocationCode = source.RackCode,
            Status = source.Status,
            CanExecute = source.Status is "대기" or "진행중",
            UpdatedAtUtc = AsUtc(source.UpdatedAtUtc),
        };

    private static WarehouseWorldNpcResponse MapNpc(WarehouseWorldTaskResponse task)
    {
        var putAway = task.TaskKind == "PutAway";
        return new WarehouseWorldNpcResponse
        {
            StableId = "warehouse-npc:" + (putAway ? "dock-worker:" : "picker:") + StableSuffix(task.StableId),
            WarehouseStableId = task.WarehouseStableId,
            SourceTaskStableId = task.StableId,
            RoleCode = putAway ? "DockWorker" : "Picker",
            RouteCode = putAway ? "warehouse.inbound-to-storage" : "warehouse.rack-to-outbound",
            CurrentWaypointKey = putAway ? "warehouse.inbound-dock" : "warehouse.rack-zone",
            DestinationWaypointKey = putAway ? "warehouse.storage-zone" : "warehouse.outbound-staging",
            ActivityCode = putAway ? "PutAway" : "Picking",
        };
    }

    private static DateTimeOffset AsUtc(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return new DateTimeOffset(utc);
    }

    private static string StableSuffix(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant()[..16];

    private static string ComputeRevision(
        IReadOnlyList<WarehouseWorldInventoryItemResponse> inventory,
        IReadOnlyList<WarehouseWorldTaskResponse> tasks)
    {
        var values = inventory.Select(item => $"{item.StableId}:{item.AvailableQuantity}:{item.ReservedQuantity}:{item.StorageLocation}:{item.Status}:{item.UpdatedAtUtc:O}")
            .Concat(tasks.Select(item => $"{item.StableId}:{item.Status}:{item.LocationCode}:{item.UpdatedAtUtc:O}"))
            .OrderBy(value => value, StringComparer.Ordinal);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", values))))
            .ToLowerInvariant();
    }

    private static Result<WarehouseWorldSnapshotResponse> Failure(IEnumerable<IError> errors)
        => Result.Fail<WarehouseWorldSnapshotResponse>(string.Join(", ", errors.Select(error => error.Message)));
}
