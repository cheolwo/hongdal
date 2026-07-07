namespace Hongdal.Contracts.Common.Orderer;

public static class GroupPurchaseCommerceFulfillmentStatusCode
{
    public const string Draft = "Draft";
    public const string LogisticsProxySelected = "LogisticsProxySelected";
    public const string InboundRequested = "InboundRequested";
    public const string InboundCompleted = "InboundCompleted";
    public const string SalesListingReady = "SalesListingReady";
    public const string SalesChannelListed = "SalesChannelListed";
    public const string OutboundBatchReady = "OutboundBatchReady";
    public const string Paused = "Paused";
}

public static class GroupPurchaseCommerceSalesChannelTypeCode
{
    public const string NaverSmartStore = "NaverSmartStore";
    public const string Coupang = "Coupang";
    public const string Other = "Other";
}

public sealed class GroupPurchaseCommerceFulfillmentPlanQuery
{
    public string? GroupPurchaseId { get; set; }
    public string? OrdererGroupScopeKey { get; set; }
    public string? DocumentManagementNumber { get; set; }
    public string? CurrentStatusCode { get; set; }
    public string? SalesChannelType { get; set; }
    public long? WarehouseId { get; set; }
    public long? InboundProductId { get; set; }
    public bool? UsePlatformLogisticsProxy { get; set; }
}

public sealed class GroupPurchaseCommerceFulfillmentPlanDto
{
    public string PlanId { get; set; } = string.Empty;
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public bool UsePlatformLogisticsProxy { get; set; } = true;
    public string LogisticsProxyCompanyName { get; set; } = string.Empty;
    public string LogisticsProxySiteName { get; set; } = string.Empty;
    public long? WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public long? InboundRequestId { get; set; }
    public long? InboundProductId { get; set; }
    public long? SalesProductId { get; set; }
    public string InventoryLotCode { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int ExpectedInboundQuantity { get; set; }
    public int AvailableForMarketQuantity { get; set; }
    public string CurrentStatusCode { get; set; } = GroupPurchaseCommerceFulfillmentStatusCode.Draft;
    public string InboundStatusCode { get; set; } = string.Empty;
    public string ListingStatusCode { get; set; } = string.Empty;
    public string OutboundBatchStatusCode { get; set; } = string.Empty;
    public IReadOnlyList<GroupPurchaseCommerceSalesChannelPlanDto> SalesChannels { get; set; } = [];
    public string OutboundBatchPolicyCode { get; set; } = string.Empty;
    public string AdminMemo { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class GroupPurchaseCommerceFulfillmentPlanPublicDto
{
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public bool UsePlatformLogisticsProxy { get; set; }
    public string LogisticsProxyCompanyName { get; set; } = string.Empty;
    public string LogisticsProxySiteName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string InventoryLotCode { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int ExpectedInboundQuantity { get; set; }
    public int AvailableForMarketQuantity { get; set; }
    public string CurrentStatusCode { get; set; } = string.Empty;
    public string InboundStatusCode { get; set; } = string.Empty;
    public string ListingStatusCode { get; set; } = string.Empty;
    public string OutboundBatchStatusCode { get; set; } = string.Empty;
    public IReadOnlyList<GroupPurchaseCommerceSalesChannelPlanPublicDto> SalesChannels { get; set; } = [];
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class GroupPurchaseCommerceSalesChannelPlanDto
{
    public string ChannelType { get; set; } = GroupPurchaseCommerceSalesChannelTypeCode.NaverSmartStore;
    public long? SalesChannelAccountId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public long? ListingId { get; set; }
    public string ChannelProductNumber { get; set; } = string.Empty;
    public string ListingStatusCode { get; set; } = string.Empty;
    public string ExternalProductUrl { get; set; } = string.Empty;
}

public sealed class GroupPurchaseCommerceSalesChannelPlanPublicDto
{
    public string ChannelType { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string ChannelProductNumber { get; set; } = string.Empty;
    public string ListingStatusCode { get; set; } = string.Empty;
    public string ExternalProductUrl { get; set; } = string.Empty;
}

public sealed class GroupPurchaseCommerceFulfillmentPlanUpsertRequest
{
    public string? PlanId { get; set; }
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public bool UsePlatformLogisticsProxy { get; set; } = true;
    public string LogisticsProxyCompanyName { get; set; } = string.Empty;
    public string LogisticsProxySiteName { get; set; } = string.Empty;
    public long? WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public long? InboundRequestId { get; set; }
    public long? InboundProductId { get; set; }
    public long? SalesProductId { get; set; }
    public string InventoryLotCode { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int ExpectedInboundQuantity { get; set; }
    public int AvailableForMarketQuantity { get; set; }
    public string CurrentStatusCode { get; set; } = GroupPurchaseCommerceFulfillmentStatusCode.Draft;
    public string InboundStatusCode { get; set; } = string.Empty;
    public string ListingStatusCode { get; set; } = string.Empty;
    public string OutboundBatchStatusCode { get; set; } = string.Empty;
    public IReadOnlyList<GroupPurchaseCommerceSalesChannelPlanDto> SalesChannels { get; set; } = [];
    public string OutboundBatchPolicyCode { get; set; } = string.Empty;
    public string AdminMemo { get; set; } = string.Empty;
}
