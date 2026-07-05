using Hongdal.Contracts.Common.Sales;
using 홍달.도메인.판매;

namespace Hongdal.Services.LogisticsProcessing.SalesOrders;

public sealed class EmptySalesChannelOrderFeedClient : ISalesChannelOrderFeedClient
{
    public bool CanFetch(string channelType, string syncScope)
    {
        return CommerceChannelOrderSyncScopes
            .GetChannelTypes(syncScope)
            .Any(x => string.Equals(x, channelType, StringComparison.OrdinalIgnoreCase));
    }

    public Task<IReadOnlyList<SalesChannelOrderFeedEntry>> FetchOrdersAsync(
        판매채널계정 account,
        DateTime? sinceUtc,
        CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<SalesChannelOrderFeedEntry>>([]);
}
