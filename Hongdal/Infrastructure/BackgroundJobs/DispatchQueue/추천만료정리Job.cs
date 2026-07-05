using Microsoft.EntityFrameworkCore;
using Quartz;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.Services.Dispatch.Queue;

namespace 홍달.Infrastructure.BackgroundJobs.DispatchQueue
{
    [DisallowConcurrentExecution]
    public sealed class 추천만료정리Job : IJob
    {
        private readonly HongdalContext _db;
        private readonly I배차큐전환Service _queueTransitionService;
        private readonly 배차큐배치작업Options _options;
        private readonly ILogger<추천만료정리Job> _logger;

        public 추천만료정리Job(
            HongdalContext db,
            I배차큐전환Service queueTransitionService,
            Microsoft.Extensions.Options.IOptions<배차큐배치작업Options> options,
            ILogger<추천만료정리Job> logger)
        {
            _db = db;
            _queueTransitionService = queueTransitionService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cancellationToken = context.CancellationToken;
            var now = DateTime.UtcNow;

            var expiredRequestIds = await _db.배차대기.AsNoTracking()
                .Where(x => x.상태 == 상태값.배차대기상태.대기
                            && x.배차큐단계 == 상태값.배차큐단계.배차추천
                            && x.배차노출상태 == 상태값.배차노출상태.추천중
                            && x.추천만료시각.HasValue
                            && x.추천만료시각 <= now)
                .OrderBy(x => x.추천만료시각)
                .Select(x => x.의뢰Id)
                .Take(_options.처리배치크기)
                .ToListAsync(cancellationToken);

            foreach (var requestId in expiredRequestIds)
            {
                await _queueTransitionService.추천만료처리Async(requestId, cancellationToken);
            }

            _logger.LogDebug("Action={Action} ExpiredCount={ExpiredCount} OccurredAt={OccurredAt}",
                "DispatchRecommendationExpiredCleaned",
                expiredRequestIds.Count,
                now);
        }
    }
}
