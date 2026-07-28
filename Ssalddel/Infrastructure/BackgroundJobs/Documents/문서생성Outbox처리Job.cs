using Quartz;
using 살뜰.Infrastructure.BackgroundJobs.DispatchQueue;
using 살뜰.Services.Documents;

namespace 살뜰.Infrastructure.BackgroundJobs.Documents;

[DisallowConcurrentExecution]
public sealed class 문서생성Outbox처리Job : IJob
{
    private readonly I문서생성OutboxService _outboxService;
    private readonly 배차큐배치작업Options _options;
    private readonly ILogger<문서생성Outbox처리Job> _logger;

    public 문서생성Outbox처리Job(
        I문서생성OutboxService outboxService,
        Microsoft.Extensions.Options.IOptions<배차큐배치작업Options> options,
        ILogger<문서생성Outbox처리Job> logger)
    {
        _outboxService = outboxService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var processed = await _outboxService.대기문서생성Async(
            _options.처리배치크기,
            context.CancellationToken);
        _logger.LogDebug(
            "Action={Action} ProcessedCount={ProcessedCount} OccurredAt={OccurredAt}",
            "DocumentGenerationOutboxProcessed",
            processed,
            DateTime.UtcNow);
    }
}
