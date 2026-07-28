using Quartz;
using Ssalddel.Services.Community;
using 살뜰.Infrastructure.BackgroundJobs.DispatchQueue;

namespace Ssalddel.Infrastructure.BackgroundJobs.Community;

[DisallowConcurrentExecution]
public sealed class 음식마트원장동기화Outbox처리Job(
    I음식마트원장동기화OutboxService outboxService,
    Microsoft.Extensions.Options.IOptions<배차큐배치작업Options> options,
    ILogger<음식마트원장동기화Outbox처리Job> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var processed = await outboxService.대기항목처리Async(
            options.Value.처리배치크기,
            context.CancellationToken);
        logger.LogDebug(
            "음식/마트 원장 동기화 Outbox 처리 완료. 처리건수={ProcessedCount}",
            processed);
    }
}
