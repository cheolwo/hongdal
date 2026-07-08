using Quartz;
using 홍달.Infrastructure.BackgroundJobs.DispatchQueue;
using 홍달.Services.Notifications;
using Microsoft.Extensions.Options;

namespace 홍달.Infrastructure.BackgroundJobs.Notifications;

[DisallowConcurrentExecution]
public sealed class Command알림Outbox발송Job : IJob
{
    private readonly ICommand알림Outbox발송Service _notificationService;
    private readonly 배차큐배치작업Options _options;
    private readonly ILogger<Command알림Outbox발송Job> _logger;

    public Command알림Outbox발송Job(
        ICommand알림Outbox발송Service notificationService,
        IOptions<배차큐배치작업Options> options,
        ILogger<Command알림Outbox발송Job> logger)
    {
        _notificationService = notificationService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var processed = await _notificationService.대기알림발송Async(_options.처리배치크기, context.CancellationToken);
        if (processed > 0)
        {
            _logger.LogInformation("Command 알림 Outbox 발송 처리 완료. 처리건수={Count}", processed);
        }
    }
}
