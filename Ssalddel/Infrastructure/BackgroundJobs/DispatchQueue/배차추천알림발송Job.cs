using Quartz;
using 살뜰.Services.Dispatch.Notification;

namespace 살뜰.Infrastructure.BackgroundJobs.DispatchQueue
{
    [DisallowConcurrentExecution]
    public sealed class 배차추천알림발송Job : IJob
    {
        private readonly I배차추천알림Service _notificationService;
        private readonly 배차큐배치작업Options _options;
        private readonly ILogger<배차추천알림발송Job> _logger;

        public 배차추천알림발송Job(
            I배차추천알림Service notificationService,
            Microsoft.Extensions.Options.IOptions<배차큐배치작업Options> options,
            ILogger<배차추천알림발송Job> logger)
        {
            _notificationService = notificationService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var processed = await _notificationService.대기알림발송Async(_options.처리배치크기, context.CancellationToken);
            _logger.LogDebug("Action={Action} ProcessedCount={ProcessedCount} OccurredAt={OccurredAt}",
                "DispatchRecommendationPushSent",
                processed,
                DateTime.UtcNow);
        }
    }
}
