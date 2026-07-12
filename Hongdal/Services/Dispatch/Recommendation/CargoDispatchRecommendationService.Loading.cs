using Microsoft.EntityFrameworkCore;
using 홍달.도메인.공통;
using 홍달.도메인.차량;
using 홍달.도메인.화물;
using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed partial class 화물배차추천Service
    {
        private async Task 추천만료정리Async(string driverId)
        {
            var activeQueue = await _db.운송원장
                .AsNoTracking()
                .Where(q => q.배차업무유형 == 상태값.배차업무유형.용달운송
                            && q.상태 == 상태값.배차대기상태.대기
                            && q.배차큐단계 == 상태값.배차큐단계.배차추천
                            && q.배차노출상태 == 상태값.배차노출상태.추천중
                            && q.현재추천대상기사Id == driverId)
                .OrderByDescending(q => q.추천시작시각)
                .FirstOrDefaultAsync();

            if (activeQueue is not null && activeQueue.추천만료시각.HasValue && activeQueue.추천만료시각 <= DateTime.UtcNow)
            {
                await _원장전환Service.추천만료처리Async(activeQueue.의뢰Id);
            }
        }

        private async Task<Dictionary<string, 화주운송의뢰>> LoadRequestMapAsync(IReadOnlyList<string> requestIds)
        {
            if (requestIds.Count == 0)
            {
                return new Dictionary<string, 화주운송의뢰>(StringComparer.Ordinal);
            }

            return await _db.화주운송의뢰
                .AsNoTracking()
                .Where(r => requestIds.Contains(r.의뢰Id))
                .ToDictionaryAsync(r => r.의뢰Id, StringComparer.Ordinal);
        }

        private async Task<Dictionary<string, 화물요구조건>> LoadCargoMapAsync(IReadOnlyList<string> requestIds)
        {
            if (requestIds.Count == 0)
            {
                return new Dictionary<string, 화물요구조건>(StringComparer.Ordinal);
            }

            return await _db.화물요구조건
                .AsNoTracking()
                .Where(r => requestIds.Contains(r.의뢰Id))
                .ToDictionaryAsync(r => r.의뢰Id, StringComparer.Ordinal);
        }

        private async Task<차량제원?> LoadVehicleSpecAsync(string? driverVehicle)
        {
            return driverVehicle is null
                ? null
                : await _db.차량제원
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.차량코드 == driverVehicle || x.차량명 == driverVehicle);
        }
    }
}
