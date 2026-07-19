using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Services.LogisticsProcessing.SalesOrders;
using Quartz;

namespace Ssalddel.Infrastructure.BackgroundJobs.SalesOrders;

[DisallowConcurrentExecution]
public sealed class DomesticSalesChannelOrderSyncJob : IJob
{
    private readonly ISalesChannelOrderSyncService _syncService;
    private readonly ILogger<DomesticSalesChannelOrderSyncJob> _logger;

    public DomesticSalesChannelOrderSyncJob(
        ISalesChannelOrderSyncService syncService,
        ILogger<DomesticSalesChannelOrderSyncJob> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var result = await _syncService.SyncAsync(CommerceChannelOrderSyncScopes.Domestic, context.CancellationToken);
        _logger.LogInformation(
            "Action={Action} SyncScope={SyncScope} AccountCount={AccountCount} FetchedOrderCount={FetchedOrderCount} CreatedOutboundCount={CreatedOutboundCount} SkippedOrderCount={SkippedOrderCount}",
            "SalesChannelOrdersSynced",
            result.SyncScope,
            result.AccountCount,
            result.FetchedOrderCount,
            result.CreatedOutboundCount,
            result.SkippedOrderCount);
    }
}
