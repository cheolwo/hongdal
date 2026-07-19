using Quartz;
using 살뜰.Infrastructure.BackgroundJobs.DispatchQueue;
using 살뜰.Services.External.Customs;

namespace 살뜰.Infrastructure.BackgroundJobs.Customs;

[DisallowConcurrentExecution]
public sealed class 통관상태동기화Job : IJob
{
    private readonly 통관상태동기화Service _동기화Service;
    private readonly 배차큐배치작업Options _options;
    private readonly ILogger<통관상태동기화Job> _logger;

    public 통관상태동기화Job(
        통관상태동기화Service 동기화Service,
        Microsoft.Extensions.Options.IOptions<배차큐배치작업Options> options,
        ILogger<통관상태동기화Job> logger)
    {
        _동기화Service = 동기화Service;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var processed = await _동기화Service.동기화Async(_options.처리배치크기, context.CancellationToken);
        _logger.LogDebug("Action={Action} ProcessedCount={ProcessedCount} OccurredAt={OccurredAt}",
            "CustomsStatusSynced",
            processed,
            DateTime.UtcNow);
    }
}
