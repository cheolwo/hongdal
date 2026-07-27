using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Drivers;
using 살뜰.Data;
using 살뜰.도메인.설정;
using 살뜰.Services.Notifications;
using 살뜰.Services.Storage.Local;

namespace 살뜰.Services.Dispatch.Notification
{
    public sealed class 배차추천알림Service : I배차추천알림Service
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private const string 상태_대기 = "Pending";
        private const string 상태_성공 = "Succeeded";
        private const string 상태_실패 = "Failed";

        private readonly SsalddelContext _db;
        private readonly IDriverPushTokenStore _pushTokenStore;
        private readonly IFcmPushService _fcmPushService;
        private readonly ILogger<배차추천알림Service> _logger;

        public 배차추천알림Service(
            SsalddelContext db,
            IDriverPushTokenStore pushTokenStore,
            IFcmPushService fcmPushService,
            ILogger<배차추천알림Service> logger)
        {
            _db = db;
            _pushTokenStore = pushTokenStore;
            _fcmPushService = fcmPushService;
            _logger = logger;
        }

        public async Task 추천알림요청생성Async(long 배차대기Id, string 의뢰Id, string 기사Id, int 추천라운드, CancellationToken cancellationToken = default)
        {
            if (배차대기Id <= 0 || string.IsNullOrWhiteSpace(의뢰Id) || string.IsNullOrWhiteSpace(기사Id))
            {
                return;
            }

            var exists = await _db.배차추천알림Outbox
                .AnyAsync(x => x.배차대기Id == 배차대기Id
                               && x.기사Id == 기사Id
                               && x.추천라운드 == 추천라운드,
                    cancellationToken);
            if (exists)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var dataJson = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["type"] = 기사배차추천알림계약.현재유형,
                ["dispatchWaitingId"] = 배차대기Id.ToString(),
                ["requestId"] = 의뢰Id,
                ["recommendationRound"] = 추천라운드.ToString()
            }, JsonOptions);

            _db.배차추천알림Outbox.Add(new 배차추천알림Outbox
            {
                배차대기Id = 배차대기Id,
                의뢰Id = 의뢰Id,
                기사Id = 기사Id,
                추천라운드 = 추천라운드,
                제목 = "새로운 배차 추천",
                본문 = "근처 운송의뢰가 도착했습니다.",
                DataJson = dataJson,
                발송상태 = 상태_대기,
                CreatedAt = now,
                UpdatedAt = now
            });

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> 대기알림발송Async(int take = 100, CancellationToken cancellationToken = default)
        {
            var pendingItems = await _db.배차추천알림Outbox
                .Where(x => x.발송상태 == 상태_대기)
                .OrderBy(x => x.CreatedAt)
                .Take(take)
                .ToListAsync(cancellationToken);

            if (pendingItems.Count == 0)
            {
                return 0;
            }

            var processed = 0;
            foreach (var item in pendingItems)
            {
                cancellationToken.ThrowIfCancellationRequested();
                processed++;

                var now = DateTime.UtcNow;
                item.시도횟수 += 1;
                item.마지막시도시각 = now;
                item.UpdatedAt = now;

                try
                {
                    var token = await _pushTokenStore.GetAsync(item.기사Id, cancellationToken);
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        item.발송상태 = 상태_실패;
                        _logger.LogWarning("Action={Action} DriverId={DriverId} OutboxId={OutboxId} Result={Result} Reason={Reason}",
                            "DispatchRecommendationPush",
                            item.기사Id,
                            item.Id,
                            "Failed",
                            "No push token");
                        continue;
                    }

                    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(item.DataJson, JsonOptions)
                               ?? new Dictionary<string, string>();

                    var sent = await _fcmPushService.SendToTokenAsync(
                        token,
                        item.제목,
                        item.본문,
                        data,
                        cancellationToken);

                    item.발송상태 = sent ? 상태_성공 : 상태_실패;
                }
                catch (Exception ex)
                {
                    item.발송상태 = 상태_실패;
                    _logger.LogWarning(ex, "배차추천 알림 발송 실패. OutboxId={OutboxId} DriverId={DriverId}", item.Id, item.기사Id);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            return processed;
        }
    }
}
