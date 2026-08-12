using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Mart;
using 살뜰.도메인.창고;

namespace Ssalddel.Application.Mart;

public sealed class 마트피킹포장WorldProjector
{
    private const string OperatorHome = "market.operator.home";
    private const string PackingQueue = "market.packing:queue";
    private const string PackingInput = "market.packing:station-01:input";
    private const string PackingPackage = "market.packing:station-01:package";

    public MarketPickingPackingWorldSnapshotResponse Project(
        long warehouseId,
        int totalOrderCount,
        int maximumOrders,
        IReadOnlyList<마트피킹주문상세응답> orders,
        WarehouseWorldSnapshotResponse warehouseSnapshot,
        DateTimeOffset generatedAtUtc)
    {
        var scopedOrders = orders
            .Where(order => order.작업목록.Any(task => task.창고Id == warehouseId))
            .ToArray();
        var taskStates = scopedOrders
            .SelectMany(order => order.작업목록)
            .Where(task => task.창고Id == warehouseId)
            .Where(task => !string.IsNullOrWhiteSpace(task.작업Key))
            .GroupBy(task => task.작업Key, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.First().상태,
                StringComparer.Ordinal);
        var tasks = scopedOrders
            .SelectMany(order => order.작업목록
                .Where(task => task.창고Id == warehouseId)
                .Select(task => MapTask(order, task, taskStates)))
            .OrderBy(task => task.StableId, StringComparer.Ordinal)
            .ToArray();
        var npcs = tasks
            .Where(ShouldCreateNpc)
            .Select(MapNpc)
            .OrderBy(npc => npc.StableId, StringComparer.Ordinal)
            .ToArray();
        var workflows = scopedOrders
            .Select(order => MapWorkflow(order, warehouseId))
            .OrderBy(workflow => workflow.StableId, StringComparer.Ordinal)
            .ToArray();
        var shelves = MapShelves(warehouseId, warehouseSnapshot, tasks);

        return new MarketPickingPackingWorldSnapshotResponse
        {
            StableId = $"market-picking-packing-zone:{warehouseId}",
            Revision = ComputeRevision(workflows, shelves, tasks),
            RevisionNumber = ComputeRevisionNumber(scopedOrders, shelves, tasks),
            GeneratedAtUtc = generatedAtUtc,
            WarehouseStableId = $"warehouse:{warehouseId}",
            TotalOrderCount = totalOrderCount,
            IsTruncated = totalOrderCount > maximumOrders,
            Workflows = workflows,
            Shelves = shelves,
            Tasks = tasks,
            Npcs = npcs
        };
    }

    private static MarketPickingPackingWorkflowResponse MapWorkflow(
        마트피킹주문상세응답 order,
        long warehouseId)
    {
        var scopedTasks = order.작업목록
            .Where(task => task.창고Id == warehouseId)
            .ToArray();
        return new()
        {
            StableId = StableId("market-order-workflow", order.주문참조번호),
            OrderStateCode = order.주문상태,
            CurrentStageCode = order.현재단계 ?? string.Empty,
            ProductLineCount = scopedTasks
                .Select(task => task.라인Key)
                .Where(lineKey => !string.IsNullOrWhiteSpace(lineKey))
                .Distinct(StringComparer.Ordinal)
                .Count(),
            TaskCount = scopedTasks.Length,
            CompletedTaskCount = scopedTasks.Count(
                task => task.상태 == 피킹포장작업상태.완료)
        };
    }

    private static MarketPickingPackingWorldTaskResponse MapTask(
        마트피킹주문상세응답 order,
        마트피킹작업응답 task,
        IReadOnlyDictionary<string, string> taskStates)
    {
        var taskKind = ResolveTaskKind(task.작업유형);
        var picking = taskKind == "Picking";
        var packing = taskKind == "Packing";
        var location = picking
            ? FirstValue(task.적재대코드, task.보관위치코드)
            : string.Empty;
        var locationMapped = !picking || !string.IsNullOrWhiteSpace(location);
        var previousCompleted = !string.IsNullOrWhiteSpace(task.이전작업Key)
            && taskStates.TryGetValue(task.이전작업Key, out var previousState)
            && previousState == 피킹포장작업상태.완료;
        var activity = ResolveActivity(
            task,
            taskKind,
            locationMapped,
            previousCompleted);

        return new MarketPickingPackingWorldTaskResponse
        {
            StableId = StableId("market-fulfillment-task", task.작업Key),
            WorkflowStableId = StableId("market-order-workflow", order.주문참조번호),
            OrderLineStableId = StableId(
                "market-order-line",
                $"{order.주문참조번호}|{task.라인Key}"),
            InventoryItemStableId = task.입고상품Id is > 0
                ? $"warehouse-inventory:{task.입고상품Id.Value}"
                : string.Empty,
            PreviousTaskStableId = StableIdOrEmpty(
                "market-fulfillment-task",
                task.이전작업Key),
            NextTaskStableId = StableIdOrEmpty(
                "market-fulfillment-task",
                task.다음작업Key),
            TaskKindCode = taskKind,
            ProductName = task.상품명,
            Sku = task.SKU,
            Quantity = task.수량,
            LocationCode = location,
            LocationMappingStateCode = picking
                ? locationMapped
                    ? MarketWorldLocationMappingStateCodes.Mapped
                    : MarketWorldLocationMappingStateCodes.LocationUnmapped
                : MarketWorldLocationMappingStateCodes.NotRequired,
            ToteStableId = StableId(
                "market-tote",
                FirstValue(task.묶음바코드, order.주문참조번호)),
            PackingStationWaypointKey = PackingInput,
            StatusCode = task.상태,
            ActivityCode = activity,
            IsPresentationReady = taskKind != "Unsupported"
                && locationMapped
                && activity is not "AwaitingPicking" and not "Cancelled",
            UpdatedAtUtc = AsUtc(task.수정일시Utc)
        };
    }

    private static MarketOperatorInventoryShelfResponse[] MapShelves(
        long warehouseId,
        WarehouseWorldSnapshotResponse warehouseSnapshot,
        IReadOnlyList<MarketPickingPackingWorldTaskResponse> tasks)
    {
        var warehouseStableId = $"warehouse:{warehouseId}";
        return warehouseSnapshot.InventoryItems
            .Where(item => item.WarehouseStableId == warehouseStableId)
            .Where(item => !string.IsNullOrWhiteSpace(item.StorageLocation))
            .GroupBy(item => item.StorageLocation.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var inventoryIds = group
                    .Select(item => item.StableId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToArray();
                var activeTasks = tasks
                    .Where(task => task.TaskKindCode == "Picking")
                    .Where(task => task.ActivityCode is not "Cancelled" and not "Picked")
                    .Where(task => inventoryIds.Contains(task.InventoryItemStableId, StringComparer.Ordinal)
                        || string.Equals(task.LocationCode, group.Key, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(task => task.StableId, StringComparer.Ordinal)
                    .ToArray();
                var segment = WaypointSegment(group.Key);

                return new MarketOperatorInventoryShelfResponse
                {
                    StableId = StableId("market-inventory-shelf", $"{warehouseId}|{group.Key.ToUpperInvariant()}"),
                    WarehouseStableId = warehouseStableId,
                    LocationCode = group.Key,
                    StateCode = ResolveShelfState(group, activeTasks),
                    TotalAvailableQuantity = group.Sum(item => item.AvailableQuantity),
                    TotalReservedQuantity = group.Sum(item => item.ReservedQuantity),
                    InventoryItemStableIds = inventoryIds,
                    ProductNames = group.Select(item => item.ProductName)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(name => name, StringComparer.Ordinal)
                        .ToArray(),
                    ActiveTaskStableIds = activeTasks.Select(task => task.StableId).ToArray(),
                    PickApproachWaypointKey = $"market.rack:{segment}:approach",
                    PickPointWaypointKey = $"market.rack:{segment}:pick",
                    UpdatedAtUtc = group.Select(item => item.UpdatedAtUtc).DefaultIfEmpty().Max(),
                    IsPresentationReady = true
                };
            })
            .OrderBy(shelf => shelf.LocationCode, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string ResolveShelfState(
        IEnumerable<WarehouseWorldInventoryItemResponse> inventory,
        IReadOnlyList<MarketPickingPackingWorldTaskResponse> activeTasks)
    {
        if (activeTasks.Any(task => task.ActivityCode == "PickingInProgress")) return "PickingInProgress";
        if (activeTasks.Any(task => task.ActivityCode == "PickingReady")) return "PickingReady";
        if (activeTasks.Any(task => task.ActivityCode == "MovingToPacking")) return "PickCompleted";

        var available = inventory.Sum(item => item.AvailableQuantity);
        var reserved = inventory.Sum(item => item.ReservedQuantity);
        if (available == 0 && reserved > 0) return "ReservedOnly";
        if (available == 0 && reserved == 0) return "Depleted";
        return "Available";
    }

    private static string ResolveTaskKind(string taskType)
        => taskType switch
        {
            피킹포장작업유형.피킹 => "Picking",
            피킹포장작업유형.포장 => "Packing",
            _ => "Unsupported"
        };

    private static string ResolveActivity(
        마트피킹작업응답 task,
        string taskKind,
        bool locationMapped,
        bool previousCompleted)
    {
        if (task.상태 == 피킹포장작업상태.취소)
        {
            return "Cancelled";
        }

        if (taskKind == "Picking")
        {
            if (!locationMapped) return "LocationUnmapped";
            return task.상태 switch
            {
                피킹포장작업상태.대기 => "PickingReady",
                피킹포장작업상태.진행중 => "PickingInProgress",
                피킹포장작업상태.완료 when !string.IsNullOrWhiteSpace(task.다음작업Key) =>
                    "MovingToPacking",
                피킹포장작업상태.완료 => "Picked",
                _ => "PickingUnavailable"
            };
        }

        if (taskKind == "Packing")
        {
            return task.상태 switch
            {
                피킹포장작업상태.대기 when previousCompleted => "PackingReady",
                피킹포장작업상태.대기 => "AwaitingPicking",
                피킹포장작업상태.진행중 => "PackingInProgress",
                피킹포장작업상태.완료 => "HandoffReady",
                _ => "PackingUnavailable"
            };
        }

        return "TaskKindUnsupported";
    }

    private static bool ShouldCreateNpc(MarketPickingPackingWorldTaskResponse task)
        => task.ActivityCode is "PickingReady"
            or "PickingInProgress"
            or "MovingToPacking"
            or "PackingReady"
            or "PackingInProgress";

    private static MarketPickingPackingWorldNpcResponse MapNpc(
        MarketPickingPackingWorldTaskResponse task)
    {
        var rackSegment = WaypointSegment(task.LocationCode);
        var (current, destination, route) = task.ActivityCode switch
        {
            "PickingReady" => (
                OperatorHome,
                $"market.rack:{rackSegment}:approach",
                "market.operator-to-rack"),
            "PickingInProgress" => (
                $"market.rack:{rackSegment}:approach",
                $"market.rack:{rackSegment}:pick",
                "market.rack-picking"),
            "MovingToPacking" => (
                $"market.rack:{rackSegment}:pick",
                PackingInput,
                "market.rack-to-packing"),
            "PackingReady" => (
                PackingQueue,
                PackingInput,
                "market.packing-queue-to-station"),
            "PackingInProgress" => (
                PackingInput,
                PackingPackage,
                "market.packing-station"),
            _ => (OperatorHome, OperatorHome, "market.operator-idle")
        };

        return new MarketPickingPackingWorldNpcResponse
        {
            StableId = StableId("market-npc", task.StableId),
            SourceTaskStableId = task.StableId,
            RoleCode = task.TaskKindCode == "Picking" ? "Picker" : "Packer",
            RouteCode = route,
            CurrentWaypointKey = current,
            DestinationWaypointKey = destination,
            ActivityCode = task.ActivityCode
        };
    }

    private static string ComputeRevision(
        IReadOnlyList<MarketPickingPackingWorkflowResponse> workflows,
        IReadOnlyList<MarketOperatorInventoryShelfResponse> shelves,
        IReadOnlyList<MarketPickingPackingWorldTaskResponse> tasks)
    {
        var values = workflows.Select(item =>
                $"{item.StableId}:{item.OrderStateCode}:{item.CurrentStageCode}:"
                + $"{item.ProductLineCount}:{item.TaskCount}:{item.CompletedTaskCount}")
            .Concat(shelves.Select(item =>
                $"{item.StableId}:{item.StateCode}:{item.TotalAvailableQuantity}:"
                + $"{item.TotalReservedQuantity}:{item.LocationCode}:{item.UpdatedAtUtc:O}"))
            .Concat(tasks.Select(item =>
                $"{item.StableId}:{item.StatusCode}:{item.ActivityCode}:"
                + $"{item.InventoryItemStableId}:{item.PreviousTaskStableId}:"
                + $"{item.NextTaskStableId}:{item.LocationCode}:{item.ToteStableId}:"
                + $"{item.UpdatedAtUtc:O}"))
            .OrderBy(value => value, StringComparer.Ordinal);
        return Hash(string.Join("|", values));
    }

    private static long ComputeRevisionNumber(
        IReadOnlyList<마트피킹주문상세응답> orders,
        IReadOnlyList<MarketOperatorInventoryShelfResponse> shelves,
        IReadOnlyList<MarketPickingPackingWorldTaskResponse> tasks)
        => orders.Select(order => AsUtc(order.수정일시Utc).UtcDateTime.Ticks)
            .Concat(shelves.Select(shelf => shelf.UpdatedAtUtc.UtcDateTime.Ticks))
            .Concat(tasks.Select(task => task.UpdatedAtUtc.UtcDateTime.Ticks))
            .DefaultIfEmpty(0)
            .Max();

    private static string StableId(string prefix, string value)
        => $"{prefix}:{Hash(value)[..32]}";

    private static string StableIdOrEmpty(string prefix, string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : StableId(prefix, value.Trim());

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant();

    private static string FirstValue(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
            ?? string.Empty;

    private static string WaypointSegment(string value)
    {
        var normalized = new string(value.Trim().ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) || character is '-' or '_'
                ? character
                : '-')
            .ToArray());
        return string.IsNullOrWhiteSpace(normalized) ? "unmapped" : normalized;
    }

    private static DateTimeOffset AsUtc(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return new DateTimeOffset(utc);
    }
}
