using Quartz;
using 살뜰.Infrastructure.BackgroundJobs.DispatchQueue;
using 살뜰.Services.Payments;

namespace 살뜰.Infrastructure.BackgroundJobs.Payments;

[DisallowConcurrentExecution]
public sealed class 결제승인완료Outbox발행Job : IJob
{
    private readonly I결제승인완료OutboxService _outboxService;
    private readonly 배차큐배치작업Options _options;
    private readonly ILogger<결제승인완료Outbox발행Job> _logger;

    public 결제승인완료Outbox발행Job(
        I결제승인완료OutboxService outboxService,
        Microsoft.Extensions.Options.IOptions<배차큐배치작업Options> options,
        ILogger<결제승인완료Outbox발행Job> logger)
    {
        _outboxService = outboxService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var processed = await _outboxService.대기이벤트발행Async(_options.처리배치크기, context.CancellationToken);
        _logger.LogDebug("Action={Action} ProcessedCount={ProcessedCount} OccurredAt={OccurredAt}",
            "PaymentApprovedOutboxPublished",
            processed,
            DateTime.UtcNow);
    }
}
