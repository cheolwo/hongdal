using Hongdal.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 홍달.Services.Options;

namespace Hongdal.Services.Orderer;

public interface IGroupPurchaseCommerceFulfillmentPlanStore
{
    Task<IReadOnlyList<GroupPurchaseCommerceFulfillmentPlanDto>> ListAsync(
        GroupPurchaseCommerceFulfillmentPlanQuery query,
        CancellationToken cancellationToken = default);

    Task<GroupPurchaseCommerceFulfillmentPlanDto?> GetAsync(
        string planId,
        CancellationToken cancellationToken = default);

    Task<GroupPurchaseCommerceFulfillmentPlanDto> UpsertAsync(
        GroupPurchaseCommerceFulfillmentPlanUpsertRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public sealed class MongoGroupPurchaseCommerceFulfillmentPlanStore : IGroupPurchaseCommerceFulfillmentPlanStore
{
    private const string CollectionName = "orderer_group_purchase_commerce_fulfillment_plans";
    private readonly IMongoCollection<GroupPurchaseCommerceFulfillmentPlanDocument> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public MongoGroupPurchaseCommerceFulfillmentPlanStore(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<GroupPurchaseCommerceFulfillmentPlanDocument>(CollectionName);
    }

    public async Task<IReadOnlyList<GroupPurchaseCommerceFulfillmentPlanDto>> ListAsync(
        GroupPurchaseCommerceFulfillmentPlanQuery query,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var items = await _collection
            .Find(BuildFilter(query))
            .SortByDescending(x => x.UpdatedAtUtc)
            .Limit(200)
            .ToListAsync(cancellationToken);

        return items.Select(ToDto).ToArray();
    }

    public async Task<GroupPurchaseCommerceFulfillmentPlanDto?> GetAsync(
        string planId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var normalized = NormalizeRequired(planId, "planId");
        var item = await _collection
            .Find(x => x.PlanIdNormalized == normalized)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : ToDto(item);
    }

    public async Task<GroupPurchaseCommerceFulfillmentPlanDto> UpsertAsync(
        GroupPurchaseCommerceFulfillmentPlanUpsertRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        Validate(request);

        var now = DateTime.UtcNow;
        var documentManagementNumberNormalized = NormalizeOptional(request.DocumentManagementNumber);
        var skuNormalized = NormalizeOptional(request.Sku);
        var inventoryLotCodeNormalized = NormalizeOptional(request.InventoryLotCode);
        var existing = await FindExistingAsync(request.PlanId, request.GroupPurchaseId, documentManagementNumberNormalized, skuNormalized, inventoryLotCodeNormalized, cancellationToken);
        var planId = string.IsNullOrWhiteSpace(request.PlanId)
            ? existing?.PlanId ?? ObjectId.GenerateNewId().ToString()
            : request.PlanId.Trim();

        var document = new GroupPurchaseCommerceFulfillmentPlanDocument
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            PlanId = planId,
            PlanIdNormalized = NormalizeRequired(planId, "planId"),
            GroupPurchaseId = request.GroupPurchaseId.Trim(),
            OrdererGroupScopeKey = request.OrdererGroupScopeKey.Trim(),
            OrdererGroupScopeName = request.OrdererGroupScopeName.Trim(),
            DocumentManagementNumber = request.DocumentManagementNumber.Trim(),
            DocumentManagementNumberNormalized = documentManagementNumberNormalized,
            UsePlatformLogisticsProxy = request.UsePlatformLogisticsProxy,
            LogisticsProxyCompanyName = request.LogisticsProxyCompanyName.Trim(),
            LogisticsProxySiteName = request.LogisticsProxySiteName.Trim(),
            WarehouseId = request.WarehouseId,
            WarehouseName = request.WarehouseName.Trim(),
            InboundRequestId = request.InboundRequestId,
            InboundProductId = request.InboundProductId,
            SalesProductId = request.SalesProductId,
            InventoryLotCode = request.InventoryLotCode.Trim(),
            InventoryLotCodeNormalized = inventoryLotCodeNormalized,
            Sku = request.Sku.Trim(),
            SkuNormalized = skuNormalized,
            ProductName = request.ProductName.Trim(),
            ExpectedInboundQuantity = Math.Max(0, request.ExpectedInboundQuantity),
            AvailableForMarketQuantity = Math.Max(0, request.AvailableForMarketQuantity),
            CurrentStatusCode = NormalizeStatus(request.CurrentStatusCode),
            InboundStatusCode = request.InboundStatusCode.Trim(),
            ListingStatusCode = request.ListingStatusCode.Trim(),
            OutboundBatchStatusCode = request.OutboundBatchStatusCode.Trim(),
            SalesChannels = request.SalesChannels.Select(ToDocument).ToArray(),
            SalesChannelTypes = request.SalesChannels.Select(x => NormalizeChannelType(x.ChannelType)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            OutboundBatchPolicyCode = request.OutboundBatchPolicyCode.Trim(),
            AdminMemo = request.AdminMemo.Trim(),
            UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            CreatedAtUtc = existing?.CreatedAtUtc ?? now,
            UpdatedAtUtc = now
        };

        await _collection.ReplaceOneAsync(
            x => x.PlanIdNormalized == document.PlanIdNormalized,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return ToDto(document);
    }

    private async Task<GroupPurchaseCommerceFulfillmentPlanDocument?> FindExistingAsync(
        string? planId,
        string groupPurchaseId,
        string documentManagementNumberNormalized,
        string skuNormalized,
        string inventoryLotCodeNormalized,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(planId))
        {
            var planIdNormalized = NormalizeRequired(planId, "planId");
            return await _collection.Find(x => x.PlanIdNormalized == planIdNormalized).FirstOrDefaultAsync(cancellationToken);
        }

        return await _collection
            .Find(x =>
                x.GroupPurchaseId == groupPurchaseId.Trim()
                && x.DocumentManagementNumberNormalized == documentManagementNumberNormalized
                && x.SkuNormalized == skuNormalized
                && x.InventoryLotCodeNormalized == inventoryLotCodeNormalized)
            .SortByDescending(x => x.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private FilterDefinition<GroupPurchaseCommerceFulfillmentPlanDocument> BuildFilter(
        GroupPurchaseCommerceFulfillmentPlanQuery query)
    {
        var builder = Builders<GroupPurchaseCommerceFulfillmentPlanDocument>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(query.GroupPurchaseId))
        {
            filter &= builder.Eq(x => x.GroupPurchaseId, query.GroupPurchaseId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.OrdererGroupScopeKey))
        {
            filter &= builder.Eq(x => x.OrdererGroupScopeKey, query.OrdererGroupScopeKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.DocumentManagementNumber))
        {
            filter &= builder.Eq(x => x.DocumentManagementNumberNormalized, NormalizeOptional(query.DocumentManagementNumber));
        }

        if (!string.IsNullOrWhiteSpace(query.CurrentStatusCode))
        {
            filter &= builder.Eq(x => x.CurrentStatusCode, NormalizeStatus(query.CurrentStatusCode));
        }

        if (!string.IsNullOrWhiteSpace(query.SalesChannelType))
        {
            filter &= builder.AnyEq(x => x.SalesChannelTypes, NormalizeChannelType(query.SalesChannelType));
        }

        if (query.WarehouseId.HasValue)
        {
            filter &= builder.Eq(x => x.WarehouseId, query.WarehouseId.Value);
        }

        if (query.InboundProductId.HasValue)
        {
            filter &= builder.Eq(x => x.InboundProductId, query.InboundProductId.Value);
        }

        if (query.UsePlatformLogisticsProxy.HasValue)
        {
            filter &= builder.Eq(x => x.UsePlatformLogisticsProxy, query.UsePlatformLogisticsProxy.Value);
        }

        return filter;
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (_indexesReady)
        {
            return;
        }

        await _indexLock.WaitAsync(cancellationToken);
        try
        {
            if (_indexesReady)
            {
                return;
            }

            var indexes = new[]
            {
                new CreateIndexModel<GroupPurchaseCommerceFulfillmentPlanDocument>(
                    Builders<GroupPurchaseCommerceFulfillmentPlanDocument>.IndexKeys.Ascending(x => x.PlanIdNormalized),
                    new CreateIndexOptions { Unique = true, Name = "ux_plan_id" }),
                new CreateIndexModel<GroupPurchaseCommerceFulfillmentPlanDocument>(
                    Builders<GroupPurchaseCommerceFulfillmentPlanDocument>.IndexKeys
                        .Ascending(x => x.GroupPurchaseId)
                        .Ascending(x => x.OrdererGroupScopeKey)
                        .Descending(x => x.UpdatedAtUtc),
                    new CreateIndexOptions { Name = "ix_group_purchase_scope_updated" }),
                new CreateIndexModel<GroupPurchaseCommerceFulfillmentPlanDocument>(
                    Builders<GroupPurchaseCommerceFulfillmentPlanDocument>.IndexKeys
                        .Ascending(x => x.DocumentManagementNumberNormalized)
                        .Ascending(x => x.SkuNormalized)
                        .Ascending(x => x.InventoryLotCodeNormalized),
                    new CreateIndexOptions { Name = "ix_document_sku_lot" }),
                new CreateIndexModel<GroupPurchaseCommerceFulfillmentPlanDocument>(
                    Builders<GroupPurchaseCommerceFulfillmentPlanDocument>.IndexKeys
                        .Ascending(x => x.CurrentStatusCode)
                        .Ascending(x => x.SalesChannelTypes)
                        .Descending(x => x.UpdatedAtUtc),
                    new CreateIndexOptions { Name = "ix_status_channel_updated" })
            };

            await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static void Validate(GroupPurchaseCommerceFulfillmentPlanUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GroupPurchaseId)) throw new InvalidOperationException("groupPurchaseId is required.");
        if (string.IsNullOrWhiteSpace(request.OrdererGroupScopeKey)) throw new InvalidOperationException("ordererGroupScopeKey is required.");
        if (string.IsNullOrWhiteSpace(request.Sku)) throw new InvalidOperationException("sku is required.");
        if (string.IsNullOrWhiteSpace(request.ProductName)) throw new InvalidOperationException("productName is required.");
        if (request.ExpectedInboundQuantity < 0) throw new InvalidOperationException("expectedInboundQuantity must be zero or greater.");
        if (request.AvailableForMarketQuantity < 0) throw new InvalidOperationException("availableForMarketQuantity must be zero or greater.");
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return NormalizeOptional(value);
    }

    private static string NormalizeOptional(string? value)
        => (value ?? string.Empty).Trim().Replace(" ", string.Empty).ToUpperInvariant();

    private static string NormalizeStatus(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, GroupPurchaseCommerceFulfillmentStatusCode.LogisticsProxySelected, StringComparison.OrdinalIgnoreCase)) return GroupPurchaseCommerceFulfillmentStatusCode.LogisticsProxySelected;
        if (string.Equals(normalized, GroupPurchaseCommerceFulfillmentStatusCode.InboundRequested, StringComparison.OrdinalIgnoreCase)) return GroupPurchaseCommerceFulfillmentStatusCode.InboundRequested;
        if (string.Equals(normalized, GroupPurchaseCommerceFulfillmentStatusCode.InboundCompleted, StringComparison.OrdinalIgnoreCase)) return GroupPurchaseCommerceFulfillmentStatusCode.InboundCompleted;
        if (string.Equals(normalized, GroupPurchaseCommerceFulfillmentStatusCode.SalesListingReady, StringComparison.OrdinalIgnoreCase)) return GroupPurchaseCommerceFulfillmentStatusCode.SalesListingReady;
        if (string.Equals(normalized, GroupPurchaseCommerceFulfillmentStatusCode.SalesChannelListed, StringComparison.OrdinalIgnoreCase)) return GroupPurchaseCommerceFulfillmentStatusCode.SalesChannelListed;
        if (string.Equals(normalized, GroupPurchaseCommerceFulfillmentStatusCode.OutboundBatchReady, StringComparison.OrdinalIgnoreCase)) return GroupPurchaseCommerceFulfillmentStatusCode.OutboundBatchReady;
        return string.Equals(normalized, GroupPurchaseCommerceFulfillmentStatusCode.Paused, StringComparison.OrdinalIgnoreCase)
            ? GroupPurchaseCommerceFulfillmentStatusCode.Paused
            : GroupPurchaseCommerceFulfillmentStatusCode.Draft;
    }

    private static string NormalizeChannelType(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, GroupPurchaseCommerceSalesChannelTypeCode.Coupang, StringComparison.OrdinalIgnoreCase)) return GroupPurchaseCommerceSalesChannelTypeCode.Coupang;
        return string.Equals(normalized, GroupPurchaseCommerceSalesChannelTypeCode.Other, StringComparison.OrdinalIgnoreCase)
            ? GroupPurchaseCommerceSalesChannelTypeCode.Other
            : GroupPurchaseCommerceSalesChannelTypeCode.NaverSmartStore;
    }

    private static GroupPurchaseCommerceSalesChannelPlanDocument ToDocument(
        GroupPurchaseCommerceSalesChannelPlanDto source)
        => new()
        {
            ChannelType = NormalizeChannelType(source.ChannelType),
            SalesChannelAccountId = source.SalesChannelAccountId,
            StoreName = source.StoreName.Trim(),
            ListingId = source.ListingId,
            ChannelProductNumber = source.ChannelProductNumber.Trim(),
            ListingStatusCode = source.ListingStatusCode.Trim(),
            ExternalProductUrl = source.ExternalProductUrl.Trim()
        };

    private static GroupPurchaseCommerceFulfillmentPlanDto ToDto(
        GroupPurchaseCommerceFulfillmentPlanDocument source)
        => new()
        {
            PlanId = source.PlanId,
            GroupPurchaseId = source.GroupPurchaseId,
            OrdererGroupScopeKey = source.OrdererGroupScopeKey,
            OrdererGroupScopeName = source.OrdererGroupScopeName,
            DocumentManagementNumber = source.DocumentManagementNumber,
            UsePlatformLogisticsProxy = source.UsePlatformLogisticsProxy,
            LogisticsProxyCompanyName = source.LogisticsProxyCompanyName,
            LogisticsProxySiteName = source.LogisticsProxySiteName,
            WarehouseId = source.WarehouseId,
            WarehouseName = source.WarehouseName,
            InboundRequestId = source.InboundRequestId,
            InboundProductId = source.InboundProductId,
            SalesProductId = source.SalesProductId,
            InventoryLotCode = source.InventoryLotCode,
            Sku = source.Sku,
            ProductName = source.ProductName,
            ExpectedInboundQuantity = source.ExpectedInboundQuantity,
            AvailableForMarketQuantity = source.AvailableForMarketQuantity,
            CurrentStatusCode = source.CurrentStatusCode,
            InboundStatusCode = source.InboundStatusCode,
            ListingStatusCode = source.ListingStatusCode,
            OutboundBatchStatusCode = source.OutboundBatchStatusCode,
            SalesChannels = source.SalesChannels.Select(ToDto).ToArray(),
            OutboundBatchPolicyCode = source.OutboundBatchPolicyCode,
            AdminMemo = source.AdminMemo,
            UpdatedBy = source.UpdatedBy,
            CreatedAtUtc = source.CreatedAtUtc,
            UpdatedAtUtc = source.UpdatedAtUtc
        };

    private static GroupPurchaseCommerceSalesChannelPlanDto ToDto(
        GroupPurchaseCommerceSalesChannelPlanDocument source)
        => new()
        {
            ChannelType = source.ChannelType,
            SalesChannelAccountId = source.SalesChannelAccountId,
            StoreName = source.StoreName,
            ListingId = source.ListingId,
            ChannelProductNumber = source.ChannelProductNumber,
            ListingStatusCode = source.ListingStatusCode,
            ExternalProductUrl = source.ExternalProductUrl
        };
}

public static class GroupPurchaseCommerceFulfillmentPlanProjection
{
    public static GroupPurchaseCommerceFulfillmentPlanPublicDto ToPublicDto(GroupPurchaseCommerceFulfillmentPlanDto source)
        => new()
        {
            GroupPurchaseId = source.GroupPurchaseId,
            OrdererGroupScopeKey = source.OrdererGroupScopeKey,
            OrdererGroupScopeName = source.OrdererGroupScopeName,
            DocumentManagementNumber = source.DocumentManagementNumber,
            UsePlatformLogisticsProxy = source.UsePlatformLogisticsProxy,
            LogisticsProxyCompanyName = source.LogisticsProxyCompanyName,
            LogisticsProxySiteName = source.LogisticsProxySiteName,
            WarehouseName = source.WarehouseName,
            InventoryLotCode = source.InventoryLotCode,
            Sku = source.Sku,
            ProductName = source.ProductName,
            ExpectedInboundQuantity = source.ExpectedInboundQuantity,
            AvailableForMarketQuantity = source.AvailableForMarketQuantity,
            CurrentStatusCode = source.CurrentStatusCode,
            InboundStatusCode = source.InboundStatusCode,
            ListingStatusCode = source.ListingStatusCode,
            OutboundBatchStatusCode = source.OutboundBatchStatusCode,
            SalesChannels = source.SalesChannels.Select(ToPublicDto).ToArray(),
            UpdatedAtUtc = source.UpdatedAtUtc
        };

    private static GroupPurchaseCommerceSalesChannelPlanPublicDto ToPublicDto(GroupPurchaseCommerceSalesChannelPlanDto source)
        => new()
        {
            ChannelType = source.ChannelType,
            StoreName = source.StoreName,
            ChannelProductNumber = source.ChannelProductNumber,
            ListingStatusCode = source.ListingStatusCode,
            ExternalProductUrl = source.ExternalProductUrl
        };
}

public sealed class GroupPurchaseCommerceFulfillmentPlanDocument
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public string PlanIdNormalized { get; set; } = string.Empty;
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public string DocumentManagementNumberNormalized { get; set; } = string.Empty;
    public bool UsePlatformLogisticsProxy { get; set; } = true;
    public string LogisticsProxyCompanyName { get; set; } = string.Empty;
    public string LogisticsProxySiteName { get; set; } = string.Empty;
    public long? WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public long? InboundRequestId { get; set; }
    public long? InboundProductId { get; set; }
    public long? SalesProductId { get; set; }
    public string InventoryLotCode { get; set; } = string.Empty;
    public string InventoryLotCodeNormalized { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string SkuNormalized { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int ExpectedInboundQuantity { get; set; }
    public int AvailableForMarketQuantity { get; set; }
    public string CurrentStatusCode { get; set; } = GroupPurchaseCommerceFulfillmentStatusCode.Draft;
    public string InboundStatusCode { get; set; } = string.Empty;
    public string ListingStatusCode { get; set; } = string.Empty;
    public string OutboundBatchStatusCode { get; set; } = string.Empty;
    public IReadOnlyList<GroupPurchaseCommerceSalesChannelPlanDocument> SalesChannels { get; set; } = [];
    public IReadOnlyList<string> SalesChannelTypes { get; set; } = [];
    public string OutboundBatchPolicyCode { get; set; } = string.Empty;
    public string AdminMemo { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class GroupPurchaseCommerceSalesChannelPlanDocument
{
    public string ChannelType { get; set; } = GroupPurchaseCommerceSalesChannelTypeCode.NaverSmartStore;
    public long? SalesChannelAccountId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public long? ListingId { get; set; }
    public string ChannelProductNumber { get; set; } = string.Empty;
    public string ListingStatusCode { get; set; } = string.Empty;
    public string ExternalProductUrl { get; set; } = string.Empty;
}
