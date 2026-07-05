using Hongdal.Contracts.Common.Sales;
using Hongdal.Services.LogisticsProcessing.SalesOrders;
using Quartz;

namespace Hongdal.Infrastructure.BackgroundJobs.SalesOrders;

[DisallowConcurrentExecution]
public sealed class OverseasSalesChannelOrderSyncJob : IJob
{
    private readonly ISalesChannelOrderSyncService _syncService;
    private readonly ILogger<OverseasSalesChannelOrderSyncJob> _logger;

    public OverseasSalesChannelOrderSyncJob(
        ISalesChannelOrderSyncService syncService,
        ILogger<OverseasSalesChannelOrderSyncJob> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var result = await _syncService.SyncAsync(CommerceChannelOrderSyncScopes.Overseas, context.CancellationToken);
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
