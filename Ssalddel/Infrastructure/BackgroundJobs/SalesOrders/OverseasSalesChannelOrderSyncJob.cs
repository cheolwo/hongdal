using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Infrastructure.BackgroundJobs;
using Ssalddel.Services.LogisticsProcessing.SalesOrders;
using Quartz;

namespace Ssalddel.Infrastructure.BackgroundJobs.SalesOrders;

[DisallowConcurrentExecution]
public sealed class OverseasSalesChannelOrderSyncJob : IJob
{
    private readonly ISalesChannelOrderSyncService _syncService;
    private readonly ISsalddelBackgroundJobActivationPolicy _activationPolicy;
    private readonly ILogger<OverseasSalesChannelOrderSyncJob> _logger;

    public OverseasSalesChannelOrderSyncJob(
        ISalesChannelOrderSyncService syncService,
        ISsalddelBackgroundJobActivationPolicy activationPolicy,
        ILogger<OverseasSalesChannelOrderSyncJob> logger)
    {
        _syncService = syncService;
        _activationPolicy = activationPolicy;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var activation = _activationPolicy.Evaluate(
            SsalddelBackgroundWorkloadKeys.SalesChannelOrderSync);
        if (!activation.IsEnabled)
        {
            _logger.LogDebug(
                "Action={Action} SyncScope={SyncScope} ActivationCode={ActivationCode} FeatureKey={FeatureKey}",
                "SalesChannelOrderSyncSkipped",
                CommerceChannelOrderSyncScopes.Overseas,
                activation.Code,
                activation.FeatureKey);
            return;
        }

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
