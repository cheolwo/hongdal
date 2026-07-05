using 홍달.도메인.판매;

namespace Hongdal.Services.LogisticsProcessing.SalesOrders;

public sealed record SalesChannelOrderFeedItem(
    string ChannelProductNo,
    string Sku,
    string ProductName,
    int Quantity);

public sealed record SalesChannelOrderFeedEntry(
    string ChannelType,
    string ChannelOrderNo,
    string BuyerName,
    string RecipientName,
    string RecipientAddress,
    DateTime OrderedAtUtc,
    IReadOnlyList<SalesChannelOrderFeedItem> Items);

public interface ISalesChannelOrderFeedClient
{
    bool CanFetch(string channelType, string syncScope);

    Task<IReadOnlyList<SalesChannelOrderFeedEntry>> FetchOrdersAsync(
        판매채널계정 account,
        DateTime? sinceUtc,
        CancellationToken cancellationToken);
}

public sealed record SalesChannelOrderSyncResult(
    string SyncScope,
    int AccountCount,
    int FetchedOrderCount,
    int CreatedOutboundCount,
    int SkippedOrderCount);
