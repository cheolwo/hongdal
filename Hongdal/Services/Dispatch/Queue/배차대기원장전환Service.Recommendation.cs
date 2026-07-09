using 홍달.도메인.공통;
using 홍달.도메인.배차;

namespace 홍달.Services.Dispatch.Queue
{
    public sealed partial class 배차대기원장전환Service
    {
        private async Task<배차대기원장전환결과> 시작Async(배차대기 queue, string driverId, int? timeoutSeconds, CancellationToken cancellationToken)
        {
            if (queue.상태 != 상태값.배차대기상태.대기)
            {
                return 대기상태아님(queue, driverId);
            }

            if (queue.배차노출상태 == 상태값.배차노출상태.추천중
                && !string.IsNullOrWhiteSpace(queue.현재추천대상기사Id)
                && (!queue.추천만료시각.HasValue || queue.추천만료시각 > DateTime.UtcNow))
            {
                return 전환안됨(
                    queue,
                    배차대기원장전환결과코드.이미추천중,
                    "아직 만료되지 않은 추천중 상태라 새 추천을 시작하지 않았습니다.",
                    driverId);
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

            return 전환됨(
                queue,
                배차대기원장전환결과코드.추천시작됨,
                "배차대기를 특정 기사 추천중 상태로 전환했습니다.",
                driverId);
        }

        private async Task<배차대기원장전환결과> 추천거절후다음후보로진행Async(배차대기 queue, string? excludeDriverId, CancellationToken cancellationToken)
        {
            if (queue.상태 != 상태값.배차대기상태.대기)
            {
                return 대기상태아님(queue, excludeDriverId);
            }

            if (queue.배차큐단계 == 상태값.배차큐단계.공개배차)
            {
                return 전환안됨(
                    queue,
                    배차대기원장전환결과코드.단계불일치,
                    "이미 공개배차 단계라 다음 추천 후보를 찾지 않았습니다.",
                    excludeDriverId);
            }

            if (queue.추천라운드 >= _options.최대추천라운드)
            {
                return await 공개배차로전환Async(queue.의뢰Id, cancellationToken);
            }

            var candidate = await _candidateSelectionService.다음후보선정Async(queue.의뢰Id, excludeDriverId, cancellationToken);
            if (candidate is null)
            {
                queue.배차노출상태 = 상태값.배차노출상태.추천후보없음;
                queue.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                var 공개전환 = await 공개배차로전환Async(queue.의뢰Id, cancellationToken);
                return 공개전환.전환여부
                    ? 배차대기원장전환결과.전환됨(
                        queue.의뢰Id,
                        배차대기원장전환결과코드.후보없음,
                        "추천 후보가 없어 공개배차로 전환했습니다.",
                        excludeDriverId)
                    : 공개전환;
            }

            return await 시작Async(queue, candidate.DriverId, null, cancellationToken);
        }
    }
}
