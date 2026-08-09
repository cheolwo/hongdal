namespace Ssalddel.Contracts.Common.WorldProjection;

public static class WarehouseWorldSnapshotRoutes
{
    public const string AuthorizedSnapshot = "api/v1/warehouse-operations/world/zones/warehouse";
}

/// <summary>
/// 현재 창고 관리자의 접근 범위 안에 있는 재고와 작업을 Unity World 표현에 필요한 최소 정보로 압축합니다.
/// 작업자 식별자, 연락처, 주문 참조, 계약·정산 정보는 포함하지 않습니다.
/// </summary>
public sealed class WarehouseWorldSnapshotResponse
{
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public int TotalAvailableQuantity { get; set; }
    public int TotalReservedQuantity { get; set; }
    public int UnassignedLocationCount { get; set; }
    public IReadOnlyList<WarehouseWorldInventoryItemResponse> InventoryItems { get; set; } = [];
    public IReadOnlyList<WarehouseWorldTaskResponse> Tasks { get; set; } = [];
    public IReadOnlyList<WarehouseWorldNpcResponse> Npcs { get; set; } = [];
    public IReadOnlyList<CargoWarehouseHandoffResponse> InboundHandoffs { get; set; } = [];
}

public sealed class WarehouseWorldInventoryItemResponse
{
    public string StableId { get; set; } = string.Empty;
    public string WarehouseStableId { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool HasCommunityLedger { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class WarehouseWorldTaskResponse
{
    public string StableId { get; set; } = string.Empty;
    public string WarehouseStableId { get; set; } = string.Empty;
    public string InventoryItemStableId { get; set; } = string.Empty;
    public string CanonicalTaskStableId { get; set; } = string.Empty;
    public string TaskKind { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool CanExecute { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class WarehouseWorldNpcResponse
{
    public string StableId { get; set; } = string.Empty;
    public string WarehouseStableId { get; set; } = string.Empty;
    public string SourceTaskStableId { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RouteCode { get; set; } = string.Empty;
    public string CurrentWaypointKey { get; set; } = string.Empty;
    public string DestinationWaypointKey { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
}
