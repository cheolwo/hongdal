using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Hongdal;
using 홍달.도메인.공통;
using 홍달.도메인.배차;
using 홍달.Services.Dispatch.Notification;

namespace 홍달.Services.Dispatch.Queue
{
    public sealed class 배차큐전환Service : I배차큐전환Service
    {
        private readonly HongdalContext _db;
        private readonly 배차큐정책Options _options;
        private readonly I배차추천후보선정Service _candidateSelectionService;
        private readonly I배차추천알림Service _recommendationNotificationService;
        private readonly I국내화물운송기사상태Service _국내화물운송기사상태Service;

        public 배차큐전환Service(
            HongdalContext db,
            IOptions<배차큐정책Options> options,
            I배차추천후보선정Service candidateSelectionService,
            I배차추천알림Service recommendationNotificationService,
            I국내화물운송기사상태Service 국내화물운송기사상태Service)
        {
            _db = db;
            _options = options.Value;
            _candidateSelectionService = candidateSelectionService;
            _recommendationNotificationService = recommendationNotificationService;
            _국내화물운송기사상태Service = 국내화물운송기사상태Service;
        }

        public async Task 계획배차에서추천으로전환Async(string requestId, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null) return;
            if (queue.상태 != 상태값.배차대기상태.대기) return;

            queue.배차큐단계 = 상태값.배차큐단계.배차추천;
            queue.배차노출상태 = 상태값.배차노출상태.추천대기;
            queue.계획배차시도횟수++;
            queue.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            await 추천거절후다음후보로진행Async(queue, excludeDriverId: null, cancellationToken);
        }

        public async Task 추천대기처리Async(string requestId, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null) return;
            if (queue.상태 != 상태값.배차대기상태.대기) return;

            if (queue.배차큐단계 != 상태값.배차큐단계.배차추천 || queue.배차노출상태 != 상태값.배차노출상태.추천대기)
            {
                return;
            }

            await 추천거절후다음후보로진행Async(queue, queue.마지막거절기사Id, cancellationToken);
        }

        public async Task 추천시작Async(string requestId, string driverId, int? timeoutSeconds = null, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null) return;
            if (queue.상태 != 상태값.배차대기상태.대기) return;

            await 시작Async(queue, driverId, timeoutSeconds, cancellationToken);
        }

        public async Task 추천거절처리Async(string requestId, string driverId, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null) return;
            if (queue.상태 != 상태값.배차대기상태.대기) return;

            // only process if the driver was indeed the current recommended
            if (!string.Equals(queue.현재추천대상기사Id, driverId, StringComparison.Ordinal))
            {
                return;
            }

            queue.배차큐단계 = 상태값.배차큐단계.배차추천;
            queue.배차노출상태 = 상태값.배차노출상태.추천거절;
            queue.마지막거절기사Id = driverId;
            queue.현재추천대상기사Id = null;
            queue.추천시작시각 = null;
            queue.추천만료시각 = null;
            queue.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            await 추천거절후다음후보로진행Async(queue, driverId, cancellationToken);
        }

        public async Task 추천만료처리Async(string requestId, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null) return;
            if (queue.상태 != 상태값.배차대기상태.대기) return;

            if (!queue.추천만료시각.HasValue || queue.추천만료시각 > DateTime.UtcNow) return;

            var expiredDriverId = queue.현재추천대상기사Id;

            queue.배차큐단계 = 상태값.배차큐단계.배차추천;
            queue.배차노출상태 = 상태값.배차노출상태.추천만료;
            queue.현재추천대상기사Id = null;
            queue.추천시작시각 = null;
            queue.추천만료시각 = null;
            queue.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            await 추천거절후다음후보로진행Async(queue, expiredDriverId, cancellationToken);
        }

        public async Task 공개배차로전환Async(string requestId, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null) return;
            if (queue.상태 != 상태값.배차대기상태.대기) return;

            queue.배차큐단계 = 상태값.배차큐단계.공개배차;
            queue.배차노출상태 = 상태값.배차노출상태.공개중;
            queue.공개전환시각 = DateTime.UtcNow;
            queue.현재추천대상기사Id = null;
            queue.추천시작시각 = null;
            queue.추천만료시각 = null;
            queue.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task 배차확정처리Async(string requestId, string driverId, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null) return;

            queue.상태 = 상태값.배차대기상태.확정;
            queue.배차큐단계 = 상태값.배차큐단계.확정;
            queue.배차노출상태 = 상태값.배차노출상태.확정;
            queue.확정기사Id = driverId;
            queue.현재추천대상기사Id = null;
            queue.추천시작시각 = null;
            queue.추천만료시각 = null;
            queue.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task 배차수락취소처리Async(string requestId, string driverId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null) return;

            var assignedToDriver = string.Equals(queue.확정기사Id, driverId, StringComparison.Ordinal)
                                   || string.Equals(queue.현재추천대상기사Id, driverId, StringComparison.Ordinal);
            if ((queue.확정기사Id is not null || queue.현재추천대상기사Id is not null) && !assignedToDriver)
            {
                return;
            }

            var dispatchRequest = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (dispatchRequest is not null)
            {
                dispatchRequest.배차상태 = 상태값.배차상태.매칭중;
                dispatchRequest.UpdatedAt = DateTime.UtcNow;
            }

            queue.상태 = 상태값.배차대기상태.대기;
            queue.배차큐단계 = 상태값.배차큐단계.배차추천;
            queue.배차노출상태 = 상태값.배차노출상태.추천대기;
            queue.마지막거절기사Id = driverId;
            queue.확정기사Id = null;
            queue.현재추천대상기사Id = null;
            queue.추천시작시각 = null;
            queue.추천만료시각 = null;
            queue.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            await 추천거절후다음후보로진행Async(queue, driverId, cancellationToken);
        }

        private async Task 시작Async(배차대기 queue, string driverId, int? timeoutSeconds, CancellationToken cancellationToken)
        {
            if (queue.상태 != 상태값.배차대기상태.대기)
            {
                return;
            }

            if (queue.배차노출상태 == 상태값.배차노출상태.추천중
                && !string.IsNullOrWhiteSpace(queue.현재추천대상기사Id)
                && (!queue.추천만료시각.HasValue || queue.추천만료시각 > DateTime.UtcNow))
            {
                return;
            }

            queue.배차큐단계 = 상태값.배차큐단계.배차추천;
            queue.배차노출상태 = 상태값.배차노출상태.추천중;
            queue.현재추천대상기사Id = driverId;
            queue.추천라운드 = queue.추천라운드 + 1;
            queue.추천시작시각 = DateTime.UtcNow;
            var ttl = TimeSpan.FromSeconds(timeoutSeconds ?? _options.추천유지시간초);
            queue.추천만료시각 = queue.추천시작시각.Value.Add(ttl);
            queue.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            await _recommendationNotificationService.추천알림요청생성Async(
                queue.Id,
                queue.의뢰Id,
                driverId,
                queue.추천라운드,
                cancellationToken);

            await _국내화물운송기사상태Service.추천기록Async(
                driverId,
                queue.추천시작시각.Value,
                cancellationToken);
        }

        private async Task 추천거절후다음후보로진행Async(배차대기 queue, string? excludeDriverId, CancellationToken cancellationToken)
        {
            if (queue.상태 != 상태값.배차대기상태.대기)
            {
                return;
            }

            if (queue.배차큐단계 == 상태값.배차큐단계.공개배차)
            {
                return;
            }

            if (queue.추천라운드 >= _options.최대추천라운드)
            {
                await 공개배차로전환Async(queue.의뢰Id, cancellationToken);
                return;
            }

            var candidate = await _candidateSelectionService.다음후보선정Async(queue.의뢰Id, excludeDriverId, cancellationToken);
            if (candidate is null)
            {
                queue.배차노출상태 = 상태값.배차노출상태.추천후보없음;
                queue.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                await 공개배차로전환Async(queue.의뢰Id, cancellationToken);
                return;
            }

            await 시작Async(queue, candidate.DriverId, null, cancellationToken);
        }
    }
}
