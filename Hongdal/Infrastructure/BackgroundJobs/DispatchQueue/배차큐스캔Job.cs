using Microsoft.EntityFrameworkCore;
using Quartz;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.Services.Dispatch.Queue;

namespace 홍달.Infrastructure.BackgroundJobs.DispatchQueue
{
    [DisallowConcurrentExecution]
    public sealed class 배차큐스캔Job : IJob
    {
        private readonly HongdalContext _db;
        private readonly I배차대기원장전환Service _원장전환Service;
        private readonly 배차큐배치작업Options _options;
        private readonly ILogger<배차큐스캔Job> _logger;

        public 배차큐스캔Job(
            HongdalContext db,
            I배차대기원장전환Service 원장전환Service,
            Microsoft.Extensions.Options.IOptions<배차큐배치작업Options> options,
            ILogger<배차큐스캔Job> logger)
        {
            _db = db;
            _원장전환Service = 원장전환Service;
            _options = options.Value;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cancellationToken = context.CancellationToken;

            var plannedIds = await _db.배차대기.AsNoTracking()
                .Where(x => x.상태 == 상태값.배차대기상태.대기
                            && x.배차큐단계 == 상태값.배차큐단계.계획배차
                            && x.배차노출상태 == 상태값.배차노출상태.계획대기)
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.의뢰Id)
                .Take(_options.처리배치크기)
                .ToListAsync(cancellationToken);

            foreach (var requestId in plannedIds)
            {
                await _원장전환Service.계획배차에서추천으로전환Async(requestId, cancellationToken);
            }

            var recommendWaitingIds = await _db.배차대기.AsNoTracking()
                .Where(x => x.상태 == 상태값.배차대기상태.대기
                            && x.배차큐단계 == 상태값.배차큐단계.배차추천
                            && x.배차노출상태 == 상태값.배차노출상태.추천대기
                            && x.현재추천대상기사Id == null)
                .OrderBy(x => x.UpdatedAt)
                .Select(x => x.의뢰Id)
                .Take(_options.처리배치크기)
                .ToListAsync(cancellationToken);

            foreach (var requestId in recommendWaitingIds)
            {
                await _원장전환Service.추천대기처리Async(requestId, cancellationToken);
            }

            var cargoWaitingCount = await _db.배차대기.AsNoTracking()
                .CountAsync(x => x.상태 == 상태값.배차대기상태.대기
                                 && x.배차업무유형 == 상태값.배차업무유형.용달운송
                                 && x.배차큐단계 == 상태값.배차큐단계.배차추천
                                 && x.배차노출상태 == 상태값.배차노출상태.추천대기,
                    cancellationToken);

            var foodWaitingCount = await _db.배차대기.AsNoTracking()
                .CountAsync(x => x.상태 == 상태값.배차대기상태.대기
                                 && x.배차업무유형 == 상태값.배차업무유형.음식배달
                                 && x.배차큐단계 == 상태값.배차큐단계.배차추천
                                 && x.배차노출상태 == 상태값.배차노출상태.추천대기,
                    cancellationToken);

            _logger.LogDebug("Action={Action} PlannedCount={PlannedCount} RecommendWaitingCount={RecommendWaitingCount} CargoWaitingCount={CargoWaitingCount} FoodWaitingCount={FoodWaitingCount} OccurredAt={OccurredAt}",
                "DispatchQueueScanned",
                plannedIds.Count,
                recommendWaitingIds.Count,
                cargoWaitingCount,
                foodWaitingCount,
                DateTime.UtcNow);
        }
    }
}
