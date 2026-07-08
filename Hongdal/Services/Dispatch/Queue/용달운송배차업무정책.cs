using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Hongdal;
using 홍달.도메인.기사;
using 홍달.도메인.화물;
using 홍달.Services.Storage.Local;

namespace 홍달.Services.Dispatch.Queue
{
    public sealed class 용달운송배차업무정책 : I배차업무정책
    {
        private readonly HongdalContext _db;
        private readonly IDriverLocationStore _driverLocationStore;
        private readonly I국내화물운송기사상태Store _국내화물운송기사상태Store;
        private readonly 배차큐정책Options _options;
        private readonly I배차추천경로Service _routeService;
        private readonly I차량화물적합성Service _적합성Service;
        private readonly I배차추천판정Service _판정Service;
        private readonly I배차추천평가Service _평가Service;

        public 용달운송배차업무정책(
            HongdalContext db,
            IDriverLocationStore driverLocationStore,
            I국내화물운송기사상태Store 국내화물운송기사상태Store,
            IOptions<배차큐정책Options> options,
            I배차추천경로Service routeService,
            I차량화물적합성Service 적합성Service,
            I배차추천판정Service 판정Service,
            I배차추천평가Service 평가Service)
        {
            _db = db;
            _driverLocationStore = driverLocationStore;
            _국내화물운송기사상태Store = 국내화물운송기사상태Store;
            _options = options.Value;
            _routeService = routeService;
            _적합성Service = 적합성Service;
            _판정Service = 판정Service;
            _평가Service = 평가Service;
        }

        public int 배차업무유형 => 홍달.도메인.공통.상태값.배차업무유형.용달운송;

        public async Task<배차추천후보?> 다음후보선정Async(홍달.도메인.배차.배차대기 queue, string? 제외기사Id = null, CancellationToken cancellationToken = default)
        {
            var request = await _db.화주운송의뢰.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == queue.의뢰Id, cancellationToken);
            if (request is null)
            {
                return null;
            }

            var cargoRequirement = await _db.화물요구조건.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == queue.의뢰Id, cancellationToken);
            var pickupPoint = queue.픽업_위도.HasValue && queue.픽업_경도.HasValue
                ? new 배차경로좌표(queue.픽업_위도.Value, queue.픽업_경도.Value)
                : null;

            if (pickupPoint is null)
            {
                return null;
            }

            var candidateStates = await _국내화물운송기사상태Store.위치반경조회Async(
                pickupPoint.Latitude,
                pickupPoint.Longitude,
                Math.Max(1m, _options.기사후보검색반경Km),
                Math.Max(1, _options.기사후보최대조회수),
                cancellationToken);
            candidateStates = await 원거리지원후보병합Async(candidateStates, pickupPoint, request, cancellationToken);
            if (candidateStates.Count == 0)
            {
                return null;
            }

            var candidateDriverIds = candidateStates
                .Select(x => x.DriverId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var vehicles = await _db.용달기사.AsNoTracking()
                .Where(x => candidateDriverIds.Contains(x.기사Id) && x.상태 == "활동중")
                .ToDictionaryAsync(x => x.기사Id, StringComparer.Ordinal, cancellationToken);
            var now = DateTime.UtcNow;

            배차추천후보? bestCandidate = null;
            decimal bestDistance = decimal.MaxValue;

            foreach (var osState in candidateStates)
            {
                if (!vehicles.TryGetValue(osState.DriverId, out var driver))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(제외기사Id) && string.Equals(driver.기사Id, 제외기사Id, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.Equals(osState.운행상태, 상태값.기사운행상태.운행중, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                DriverLocationSnapshot location;
                if (osState.Latitude.HasValue && osState.Longitude.HasValue)
                {
                    location = new DriverLocationSnapshot(
                        driver.기사Id,
                        osState.Latitude.Value,
                        osState.Longitude.Value,
                        osState.AccuracyM,
                        osState.운행상태,
                        osState.위치기록시각Utc ?? now,
                        osState.위치수신시각Utc ?? now);
                }
                else if (!_driverLocationStore.TryGetLatest(driver.기사Id, out location))
                {
                    continue;
                }

                var currentPoint = new 배차경로좌표(location.Latitude, location.Longitude);
                var distanceKm = _routeService.CalculateDistanceKm(currentPoint, pickupPoint);
                if (!distanceKm.HasValue)
                {
                    continue;
                }

                if (!상차접근허용(osState, distanceKm.Value, request))
                {
                    continue;
                }

                var vehicleSpec = await _db.차량제원.AsNoTracking().FirstOrDefaultAsync(x => x.차량코드 == driver.차량 || x.차량명 == driver.차량, cancellationToken);
                var fit = _적합성Service.판정(vehicleSpec, request, cargoRequirement);
                if (!fit.적합여부)
                {
                    continue;
                }

                var 판단결과 = _판정Service.판정(request, null, null, distanceKm, fit, null);
                var 평가결과 = _평가Service.평가(
                    request,
                    판단결과,
                    null,
                    null,
                    null,
                    distanceKm,
                    null,
                    null,
                    null,
                    false,
                    null);

                var agingScore = 기사대기Aging점수정책.계산(osState.Aging기준시각Utc, now);
                var score = (평가결과.추천점수 ?? 0m) + agingScore;
                var reason = agingScore > 0m
                    ? $"{평가결과.추천사유} · 기사대기보정 +{agingScore:0}"
                    : 평가결과.추천사유;

                if (bestCandidate is null
                    || score > bestCandidate.추천점수
                    || (score == bestCandidate.추천점수 && distanceKm.Value < bestDistance))
                {
                    bestCandidate = new 배차추천후보(driver.기사Id, score, reason);
                    bestDistance = distanceKm.Value;
                }
            }

            return bestCandidate;
        }

        private async Task<IReadOnlyList<국내화물운송기사상태Snapshot>> 원거리지원후보병합Async(
            IReadOnlyList<국내화물운송기사상태Snapshot> geoCandidates,
            배차경로좌표 pickupPoint,
            화주운송의뢰 request,
            CancellationToken cancellationToken)
        {
            var result = geoCandidates
                .GroupBy(x => x.DriverId, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);

            var activeCandidates = await _국내화물운송기사상태Store.활성기사조회Async(
                Math.Max(1, _options.원거리지원후보최대조회수),
                cancellationToken);

            foreach (var state in activeCandidates)
            {
                if (result.ContainsKey(state.DriverId)
                    || !state.상차접근허용반경Km.HasValue
                    || !state.Latitude.HasValue
                    || !state.Longitude.HasValue)
                {
                    continue;
                }

                var distanceKm = _routeService.CalculateDistanceKm(
                    new 배차경로좌표(state.Latitude.Value, state.Longitude.Value),
                    pickupPoint);
                if (!distanceKm.HasValue || !상차접근허용(state, distanceKm.Value, request))
                {
                    continue;
                }

                result[state.DriverId] = state;
            }

            return result.Values.ToList();
        }

        private bool 상차접근허용(국내화물운송기사상태Snapshot state, decimal distanceKm, 화주운송의뢰 request)
        {
            var baseRadius = Math.Max(1m, _options.기사후보검색반경Km);
            if (distanceKm <= baseRadius)
            {
                return true;
            }

            var driverAllowedRadius = state.상차접근허용반경Km ?? baseRadius;
            var maxRadius = Math.Max(baseRadius, _options.원거리상차접근최대반경Km);
            var allowedRadius = Math.Min(driverAllowedRadius, maxRadius);
            if (distanceKm > allowedRadius)
            {
                return false;
            }

            return 상차시간창도착가능(distanceKm, request);
        }

        private bool 상차시간창도착가능(decimal distanceKm, 화주운송의뢰 request)
        {
            var pickupWindowEnd = request.픽업_시간창_종료일시;
            if (pickupWindowEnd <= DateTime.UtcNow)
            {
                return false;
            }

            var speed = Math.Max(1m, _options.원거리상차평균속도KmH);
            var estimatedMinutes = distanceKm / speed * 60m;
            var remainingMinutes = (decimal)(pickupWindowEnd - DateTime.UtcNow).TotalMinutes;
            return remainingMinutes >= estimatedMinutes + Math.Max(0m, _options.원거리상차도착여유분);
        }

        private async Task<IReadOnlyDictionary<string, DateTime>> 최근추천상호작용시각조회Async(
            IEnumerable<string> driverIds,
            CancellationToken cancellationToken)
        {
            var ids = driverIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (ids.Length == 0)
            {
                return new Dictionary<string, DateTime>(StringComparer.Ordinal);
            }

            var interactions = await _db.배차대기
                .AsNoTracking()
                .Where(x =>
                    (x.현재추천대상기사Id != null && ids.Contains(x.현재추천대상기사Id)) ||
                    (x.확정기사Id != null && ids.Contains(x.확정기사Id)) ||
                    (x.마지막거절기사Id != null && ids.Contains(x.마지막거절기사Id)))
                .Select(x => new
                {
                    x.현재추천대상기사Id,
                    x.확정기사Id,
                    x.마지막거절기사Id,
                    기준시각 = x.추천시작시각 ?? x.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<string, DateTime>(StringComparer.Ordinal);
            foreach (var interaction in interactions)
            {
                AddLatest(result, interaction.현재추천대상기사Id, interaction.기준시각);
                AddLatest(result, interaction.확정기사Id, interaction.기준시각);
                AddLatest(result, interaction.마지막거절기사Id, interaction.기준시각);
            }

            return result;
        }

        private static decimal 기사대기Aging점수계산(
            용달기사 driver,
            IReadOnlyDictionary<string, DateTime> driverLastRecommendationTimes,
            DateTime now)
        {
            var basis = driverLastRecommendationTimes.TryGetValue(driver.기사Id, out var lastRecommendationAt)
                ? lastRecommendationAt
                : driver.등록일 ?? driver.CreatedAt;
            return 기사대기Aging점수정책.계산(basis, now);
        }

        private static void AddLatest(Dictionary<string, DateTime> target, string? driverId, DateTime value)
        {
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return;
            }

            if (!target.TryGetValue(driverId, out var existing) || value > existing)
            {
                target[driverId] = value;
            }
        }
    }
}
