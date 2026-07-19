namespace Ssalddel.Contracts.Common.Warehouse;

public sealed class OutboundBatchPlanRequest
{
    public string OrderReference { get; set; } = string.Empty;

    public string SellerUserId { get; set; } = string.Empty;

    public string OrdererUserId { get; set; } = string.Empty;

    public string DestinationAddress { get; set; } = string.Empty;

    public decimal? DestinationLatitude { get; set; }

    public decimal? DestinationLongitude { get; set; }

    public IReadOnlyList<OutboundBatchPlanLineRequest> Lines { get; set; } = [];
}

public sealed class OutboundBatchPlanLineRequest
{
    public string LineKey { get; set; } = string.Empty;

    public long? SalesProductId { get; set; }

    public long? PreferredInboundProductId { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }
}

public sealed class OutboundBatchPlanResult
{
    public bool IsComplete { get; set; }

    public bool RequiresSplitShipment { get; set; }

    public string Message { get; set; } = string.Empty;

    public IReadOnlyList<OutboundBatchAllocation> Allocations { get; set; } = [];

    public IReadOnlyList<OutboundBatchUnallocatedLine> UnallocatedLines { get; set; } = [];
}

public sealed class OutboundBatchAllocation
{
    public string LineKey { get; set; } = string.Empty;

    public long? SalesProductId { get; set; }

    public long InboundProductId { get; set; }

    public long WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public bool IsServiceAreaMatched { get; set; }

    public decimal? EstimatedDistanceKm { get; set; }

    public decimal? EstimatedTransportCost { get; set; }

    public decimal SelectionScore { get; set; }

    public string SelectionReason { get; set; } = string.Empty;
}

public sealed class OutboundBatchUnallocatedLine
{
    public string LineKey { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int RequestedQuantity { get; set; }

    public int PlannedQuantity { get; set; }

    public string Reason { get; set; } = string.Empty;
}
