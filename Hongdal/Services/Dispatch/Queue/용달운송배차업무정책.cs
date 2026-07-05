using Microsoft.EntityFrameworkCore;
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
        private readonly I배차추천경로Service _routeService;
        private readonly I차량화물적합성Service _적합성Service;
        private readonly I배차추천판정Service _판정Service;
        private readonly I배차추천평가Service _평가Service;

        public 용달운송배차업무정책(
            HongdalContext db,
            IDriverLocationStore driverLocationStore,
            I배차추천경로Service routeService,
            I차량화물적합성Service 적합성Service,
            I배차추천판정Service 판정Service,
            I배차추천평가Service 평가Service)
        {
            _db = db;
            _driverLocationStore = driverLocationStore;
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

            var vehicles = await _db.용달기사.AsNoTracking()
                .Where(x => x.상태 == "활동중")
                .ToListAsync(cancellationToken);

            배차추천후보? bestCandidate = null;
            decimal bestDistance = decimal.MaxValue;

            foreach (var driver in vehicles)
            {
                if (!string.IsNullOrWhiteSpace(제외기사Id) && string.Equals(driver.기사Id, 제외기사Id, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!_driverLocationStore.TryGetLatest(driver.기사Id, out var location))
                {
                    continue;
                }

                var currentPoint = new 배차경로좌표(location.Latitude, location.Longitude);
                var distanceKm = _routeService.CalculateDistanceKm(currentPoint, pickupPoint);
                if (!distanceKm.HasValue)
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

                var score = 평가결과.추천점수 ?? 0m;
                if (bestCandidate is null
                    || score > bestCandidate.추천점수
                    || (score == bestCandidate.추천점수 && distanceKm.Value < bestDistance))
                {
                    bestCandidate = new 배차추천후보(driver.기사Id, score, 평가결과.추천사유);
                    bestDistance = distanceKm.Value;
                }
            }

            return bestCandidate;
        }
    }
}
