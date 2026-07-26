using Quartz;
using 살뜰.Infrastructure.BackgroundJobs.DispatchQueue;
using 살뜰.Services.Payments;

namespace 살뜰.Infrastructure.BackgroundJobs.Payments;

[DisallowConcurrentExecution]
public sealed class 기사지급Outbox처리Job(
    I기사지급OutboxService outboxService,
    Microsoft.Extensions.Options.IOptions<배차큐배치작업Options> options,
    ILogger<기사지급Outbox처리Job> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var processed = await outboxService.대기항목처리Async(
            options.Value.처리배치크기,
            context.CancellationToken);
        logger.LogDebug(
            "Action={Action} ProcessedCount={ProcessedCount} OccurredAt={OccurredAt}",
            "DriverPayoutOutboxProcessed",
            processed,
            DateTime.UtcNow);
    }
}
