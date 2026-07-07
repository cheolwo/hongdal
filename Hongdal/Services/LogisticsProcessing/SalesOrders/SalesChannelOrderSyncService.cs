using Hongdal.Contracts.Common.Sales;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Services.LogisticsProcessing.Warehouse;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Services.LogisticsProcessing.SalesOrders;

public interface ISalesChannelOrderSyncService
{
    Task<SalesChannelOrderSyncResult> SyncAsync(string syncScope, CancellationToken cancellationToken);
}

public sealed class SalesChannelOrderSyncService : ISalesChannelOrderSyncService
{
    private readonly HongdalContext _db;
    private readonly IReadOnlyList<ISalesChannelOrderFeedClient> _feedClients;
    private readonly IOutboundBatchEngine _outboundBatchEngine;
    private readonly SalesChannelOrderSyncOptions _options;
    private readonly ILogger<SalesChannelOrderSyncService> _logger;

    public SalesChannelOrderSyncService(
        HongdalContext db,
        IEnumerable<ISalesChannelOrderFeedClient> feedClients,
        IOutboundBatchEngine outboundBatchEngine,
        Microsoft.Extensions.Options.IOptions<SalesChannelOrderSyncOptions> options,
        ILogger<SalesChannelOrderSyncService> logger)
    {
        _db = db;
        _feedClients = feedClients.ToArray();
        _outboundBatchEngine = outboundBatchEngine;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SalesChannelOrderSyncResult> SyncAsync(string syncScope, CancellationToken cancellationToken)
    {
        var normalizedScope = NormalizeScope(syncScope);
        if (!_options.Enabled)
        {
            return new SalesChannelOrderSyncResult(normalizedScope, 0, 0, 0, 0);
        }

        var channelTypes = CommerceChannelOrderSyncScopes.GetChannelTypes(normalizedScope);
        var accounts = await _db.판매채널계정
            .Where(x => channelTypes.Contains(x.채널종류) && x.연결상태 != "해제")
            .OrderBy(x => x.마지막동기화일시 ?? DateTime.MinValue)
            .Take(Math.Max(1, _options.BatchSize))
            .ToListAsync(cancellationToken);

        var fetchedOrderCount = 0;
        var createdOutboundCount = 0;
        var skippedOrderCount = 0;
        var now = DateTime.UtcNow;

        foreach (var account in accounts)
        {
            var client = _feedClients.FirstOrDefault(x => x.CanFetch(account.채널종류, normalizedScope));

            if (client is null)
            {
                _logger.LogInformation(
                    "Action={Action} ChannelType={ChannelType} SyncScope={SyncScope} AccountId={AccountId}",
                    "SalesChannelOrderFeedClientMissing",
                    account.채널종류,
                    normalizedScope,
                    account.Id);
                continue;
            }

            var sinceUtc = account.마지막동기화일시 ?? now.AddMinutes(-Math.Max(1, _options.LookbackMinutes));
            var orders = await client.FetchOrdersAsync(account, sinceUtc, cancellationToken);
            fetchedOrderCount += orders.Count;

            foreach (var order in orders)
            {
                var created = await CreateWarehouseOutboundRequestsAsync(account, order, cancellationToken);
                if (created == 0)
                {
                    skippedOrderCount++;
                }
                else
                {
                    createdOutboundCount += created;
                }
            }

            account.마지막동기화일시 = now;
            account.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new SalesChannelOrderSyncResult(
            normalizedScope,
            accounts.Count,
            fetchedOrderCount,
            createdOutboundCount,
            skippedOrderCount);
    }

    private async Task<int> CreateWarehouseOutboundRequestsAsync(
        홍달.도메인.판매.판매채널계정 account,
        SalesChannelOrderFeedEntry order,
        CancellationToken cancellationToken)
    {
        if (order.Items.Count == 0 || string.IsNullOrWhiteSpace(order.ChannelOrderNo))
        {
            return 0;
        }

        var orderReference = CreateOrderReference(order.ChannelType, order.ChannelOrderNo);
        var now = DateTime.UtcNow;
        var lines = new List<OutboundBatchPlanLineRequest>();

        foreach (var item in order.Items.Where(x => x.Quantity > 0))
        {
            var listingQuery =
                from listing in _db.채널출품
                join product in _db.판매상품 on listing.판매상품Id equals product.Id
                join inboundItem in _db.입고상품 on product.입고상품Id equals inboundItem.Id
                where listing.판매채널계정Id == account.Id
                      && product.소유자UserId == account.UserId
                      && (product.판매SKU == item.Sku || listing.채널상품번호 == item.ChannelProductNo)
                select new { listing, product, inboundItem };

            var mapped = await listingQuery.FirstOrDefaultAsync(cancellationToken);
            if (mapped is null)
            {
                continue;
            }

            var alreadyExists = await _db.출고예정.AnyAsync(x =>
                x.주문참조번호 == orderReference
                && x.판매자UserId == account.UserId
                && x.SKU == mapped.product.판매SKU
                && x.상태 != 홍달.도메인.창고.출고상태.취소,
                cancellationToken);

            if (alreadyExists)
            {
                continue;
            }

            if (!Hongdal.Contracts.Common.Inbound.입고계약유형코드.CanSellToMarket(mapped.inboundItem.계약유형))
            {
                continue;
            }

            lines.Add(new OutboundBatchPlanLineRequest
            {
                LineKey = CreateLineKey(mapped.product.판매SKU, lines.Count + 1),
                SalesProductId = mapped.product.Id,
                PreferredInboundProductId = mapped.inboundItem.Id,
                Sku = mapped.product.판매SKU,
                ProductName = string.IsNullOrWhiteSpace(item.ProductName) ? mapped.product.대표상품명 : item.ProductName,
                Quantity = item.Quantity
            });
        }

        if (lines.Count == 0)
        {
            return 0;
        }

        var plan = await _outboundBatchEngine.PlanAsync(new OutboundBatchPlanRequest
        {
            OrderReference = orderReference,
            SellerUserId = account.UserId,
            OrdererUserId = string.IsNullOrWhiteSpace(order.BuyerName) ? order.RecipientName : order.BuyerName,
            DestinationAddress = order.RecipientAddress,
            Lines = lines
        }, cancellationToken);

        foreach (var allocation in plan.Allocations)
        {
            _db.출고예정.Add(new 홍달.도메인.창고.출고예정
            {
                주문참조번호 = orderReference,
                판매상품Id = allocation.SalesProductId,
                입고상품Id = allocation.InboundProductId,
                판매자UserId = account.UserId,
                주문자UserId = string.IsNullOrWhiteSpace(order.BuyerName) ? order.RecipientName : order.BuyerName,
                출고창고Id = allocation.WarehouseId,
                상품명 = allocation.ProductName,
                SKU = allocation.Sku,
                수량 = allocation.Quantity,
                상태 = 홍달.도메인.창고.출고상태.예정,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (plan.UnallocatedLines.Count > 0)
        {
            _logger.LogInformation(
                "Action={Action} OrderReference={OrderReference} UnallocatedLineCount={UnallocatedLineCount} Message={Message}",
                "OutboundBatchPlanPartiallyUnallocated",
                orderReference,
                plan.UnallocatedLines.Count,
                plan.Message);
        }

        return plan.Allocations.Count;
    }

    private static string NormalizeScope(string syncScope)
        => string.Equals(syncScope, CommerceChannelOrderSyncScopes.Overseas, StringComparison.OrdinalIgnoreCase)
            ? CommerceChannelOrderSyncScopes.Overseas
            : CommerceChannelOrderSyncScopes.Domestic;

    private static string CreateOrderReference(string channelType, string channelOrderNo)
        => $"{channelType.Trim()}:{channelOrderNo.Trim()}";

    private static string CreateLineKey(string sku, int sequence)
        => $"{sku.Trim()}#{sequence}";
}
