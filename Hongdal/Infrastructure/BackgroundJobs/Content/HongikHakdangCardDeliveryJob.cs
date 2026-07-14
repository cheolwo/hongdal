using Hongdal.Services.Content;
using Quartz;

namespace Hongdal.Infrastructure.BackgroundJobs.Content;

[DisallowConcurrentExecution]
public sealed class HongikHakdangCardDeliveryJob : IJob
{
    private readonly IHongikHakdangCardDeliveryService _service;
    private readonly ILogger<HongikHakdangCardDeliveryJob> _logger;

    public HongikHakdangCardDeliveryJob(
        IHongikHakdangCardDeliveryService service,
        ILogger<HongikHakdangCardDeliveryJob> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var result = await _service.RunDeliveryCycleAsync(context.CancellationToken);
        if (result.EnqueuedCount > 0 || result.ProcessedCount > 0)
        {
            _logger.LogInformation(
                "Action={Action} Enqueued={Enqueued} Processed={Processed} Succeeded={Succeeded} Failed={Failed}",
                "HongikHakdangCardDeliveryCycle",
                result.EnqueuedCount,
                result.ProcessedCount,
                result.SucceededCount,
                result.FailedCount);
        }
    }
}
