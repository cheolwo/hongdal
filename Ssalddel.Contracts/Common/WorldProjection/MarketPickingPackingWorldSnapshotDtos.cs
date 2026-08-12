namespace Ssalddel.Contracts.Common.WorldProjection;

public static class MarketPickingPackingWorldSnapshotRoutes
{
    public const string AuthorizedSnapshot = "api/v1/warehouse-operations/mart/world/picking-packing";
}

public static class MarketWorldLocationMappingStateCodes
{
    public const string Mapped = "Mapped";
    public const string LocationUnmapped = "LocationUnmapped";
    public const string NotRequired = "NotRequired";
}

/// <summary>
/// 권한 범위 안의 마트 주문 피킹·포장 작업을 Unity 공간 표현에 필요한 최소 정보로 압축합니다.
/// 주문 참조번호, 주문자, 작업자, 주소, 연락처와 결제 정보는 포함하지 않습니다.
/// </summary>
public sealed class MarketPickingPackingWorldSnapshotResponse
{
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public long RevisionNumber { get; set; }
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public string WarehouseStableId { get; set; } = string.Empty;
    public int TotalOrderCount { get; set; }
    public bool IsTruncated { get; set; }
    public IReadOnlyList<MarketPickingPackingWorkflowResponse> Workflows { get; set; } = [];
    public IReadOnlyList<MarketOperatorInventoryShelfResponse> Shelves { get; set; } = [];
    public IReadOnlyList<MarketPickingPackingWorldTaskResponse> Tasks { get; set; } = [];
    public IReadOnlyList<MarketPickingPackingWorldNpcResponse> Npcs { get; set; } = [];
}

/// <summary>
/// 운영자에게만 보이는 주소 지정 가능한 재고 위치입니다.
/// 수량은 창고 기준 원장의 읽기 결과이며 Unity가 변경하거나 완료를 판정하지 않습니다.
/// </summary>
public sealed class MarketOperatorInventoryShelfResponse
{
    public string StableId { get; set; } = string.Empty;
    public string SeedbedObjectStableId { get; set; } = "seedbed-object:city.operator-inventory-shelf.a";
    public string WarehouseStableId { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string AccessScopeCode { get; set; } = "OperatorOnly";
    public string StateCode { get; set; } = string.Empty;
    public int TotalAvailableQuantity { get; set; }
    public int TotalReservedQuantity { get; set; }
    public IReadOnlyList<string> InventoryItemStableIds { get; set; } = [];
    public IReadOnlyList<string> ProductNames { get; set; } = [];
    public IReadOnlyList<string> ActiveTaskStableIds { get; set; } = [];
    public string PickApproachWaypointKey { get; set; } = string.Empty;
    public string PickPointWaypointKey { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public bool IsPresentationReady { get; set; }
}

public sealed class MarketPickingPackingWorkflowResponse
{
    public string StableId { get; set; } = string.Empty;
    public string OrderStateCode { get; set; } = string.Empty;
    public string CurrentStageCode { get; set; } = string.Empty;
    public int ProductLineCount { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
}

public sealed class MarketPickingPackingWorldTaskResponse
{
    public string StableId { get; set; } = string.Empty;
    public string WorkflowStableId { get; set; } = string.Empty;
    public string OrderLineStableId { get; set; } = string.Empty;
    public string InventoryItemStableId { get; set; } = string.Empty;
    public string PreviousTaskStableId { get; set; } = string.Empty;
    public string NextTaskStableId { get; set; } = string.Empty;
    public string TaskKindCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationMappingStateCode { get; set; } = string.Empty;
    public string ToteStableId { get; set; } = string.Empty;
    public string PackingStationWaypointKey { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
    public bool IsPresentationReady { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class MarketPickingPackingWorldNpcResponse
{
    public string StableId { get; set; } = string.Empty;
    public string SourceTaskStableId { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RouteCode { get; set; } = string.Empty;
    public string CurrentWaypointKey { get; set; } = string.Empty;
    public string DestinationWaypointKey { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
}
