using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Hongdal.Hubs;
using 홍달.도메인.화물;
using 홍달.도메인.화주;
using 홍달.도메인.차량;

namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed class 화물배차추천Service : 배차추천Service, I배차추천Service
    {
        private readonly I차량화물적합성Service _적합성Service;

        public 화물배차추천Service(
            HongdalContext db,
            IDriverLocationStore driverLocationStore,
            IDriverRejectedRequestStore rejectedRequestStore,
            IDriverRecommendationPushService pushService,
            IDispatchRecommendationLogStore logStore,
            IHubContext<DispatchRecommendationHub> hubContext,
            I배차추천경로Service routeService,
            I배차추천판정Service 판정Service,
            I배차추천평가Service 평가Service,
            I차량화물적합성Service 적합성Service,
            I기사운송일정구성Service 기사운송일정구성Service,
            I운송일정삽입평가Service 운송일정삽입평가Service,
            IOpinetAveragePriceService averagePriceService,
            IGeocodingService geocodingService)
            : base(
                db,
                driverLocationStore,
                rejectedRequestStore,
                pushService,
                logStore,
                hubContext,
                routeService,
                판정Service,
                평가Service,
                기사운송일정구성Service,
                운송일정삽입평가Service,
                averagePriceService)
        {
            _적합성Service = 적합성Service;
        }

        protected override async Task<bool> IsDrivingAsync(string driverId)
        {
            var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
            return string.Equals(driver?.운행상태, 상태값.기사운행상태.운행중, StringComparison.Ordinal);
        }

        public override async Task<IReadOnlyList<DispatchRecommendationDto>> GetDrivingRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null)
        {
            return await GetRecommendationsCoreAsync(driverId, criteria, isDriving: true);
        }

        public override async Task<IReadOnlyList<DispatchRecommendationDto>> GetIdleRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null)
        {
            return await GetRecommendationsCoreAsync(driverId, criteria, isDriving: false);
        }

        private async Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsCoreAsync(string driverId, 배차추천검색조건? criteria, bool isDriving)
        {
            var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
            _driverLocationStore.TryGetLatest(driverId, out DriverLocationSnapshot? currentLocation);
            var rejectedRequestIds = await _rejectedRequestStore.GetRejectedRequestIdsAsync(driverId);
            var rejectedRequestIdSet = rejectedRequestIds.Count > 0
                ? new HashSet<string>(rejectedRequestIds, StringComparer.Ordinal)
                : null;

            var originLocation = isDriving
                ? await _routeService.ResolveRouteAnchorLocationAsync(driverId, driver, currentLocation)
                : await _routeService.ResolveOriginLocationAsync(driverId, driver, currentLocation, criteria);
            var routeAnchorLocation = isDriving ? originLocation : null;
            var currentSchedulePlan = isDriving
                ? await _기사운송일정구성Service.구성Async(driverId, originLocation)
                : null;
            var returnDestination = await ResolveReturnDestinationAsync(driverId, driver);
            var fuelPricePerLiter = await ResolveFuelPricePerLiterAsync();
            var fuelEfficiencyKmPerLiter = ResolveFuelEfficiencyKmPerLiter(driver?.차량);

            var items = await _db.배차대기
                .Where(q => q.상태 == 상태값.배차대기상태.대기)
                .ToListAsync();

            var requestIds = items.Select(q => q.의뢰Id).Distinct().ToList();
            Dictionary<string, 화주운송의뢰> requestMap;
            if (requestIds.Count == 0)
            {
                requestMap = new Dictionary<string, 화주운송의뢰>(StringComparer.Ordinal);
            }
            else
            {
                requestMap = await _db.화주운송의뢰
                    .AsNoTracking()
                    .Where(r => requestIds.Contains(r.의뢰Id))
                    .ToDictionaryAsync(r => r.의뢰Id, StringComparer.Ordinal);
            }

            Dictionary<string, 화물요구조건> cargoMap;
            if (requestIds.Count == 0)
            {
                cargoMap = new Dictionary<string, 화물요구조건>(StringComparer.Ordinal);
            }
            else
            {
                cargoMap = await _db.화물요구조건
                    .AsNoTracking()
                    .Where(r => requestIds.Contains(r.의뢰Id))
                    .ToDictionaryAsync(r => r.의뢰Id, StringComparer.Ordinal);
            }

            var driverVehicle = driver?.차량;
            var vehicleSpec = driverVehicle is null
                ? null
                : await _db.차량제원
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.차량코드 == driverVehicle || x.차량명 == driverVehicle);

            var hasSearchCriteria = criteria is not null && criteria.RadiusKm > 0;
            var searchLatitude = hasSearchCriteria ? criteria!.Latitude : originLocation?.Latitude ?? currentLocation?.Latitude;
            var searchLongitude = hasSearchCriteria ? criteria!.Longitude : originLocation?.Longitude ?? currentLocation?.Longitude;
            var radiusKm = hasSearchCriteria ? criteria!.RadiusKm : (decimal?)null;

            var filtered = items
                .Where(q => rejectedRequestIdSet == null || !rejectedRequestIdSet.Contains(q.의뢰Id))
                .Select(q => new
                {
                    Item = q,
                    Request = requestMap.TryGetValue(q.의뢰Id, out var request) ? request : null,
                    CargoRequirement = cargoMap.TryGetValue(q.의뢰Id, out var cargoRequirement) ? cargoRequirement : null,
                    CandidateDistanceKm = searchLatitude.HasValue && searchLongitude.HasValue && q.픽업_위도.HasValue && q.픽업_경도.HasValue
                        ? (double?)_routeService.CalculateDistanceKm(
                            new 배차경로좌표(searchLatitude.Value, searchLongitude.Value),
                            new 배차경로좌표(q.픽업_위도.Value, q.픽업_경도.Value))
                        : (double?)null
                })
                .OrderBy(x => x.CandidateDistanceKm.HasValue ? 0 : 1)
                .ThenBy(x => x.CandidateDistanceKm ?? double.MaxValue)
                .ThenBy(x => x.Item.CreatedAt)
                .Where(x => !radiusKm.HasValue || !x.CandidateDistanceKm.HasValue || x.CandidateDistanceKm.Value <= (double)radiusKm.Value)
                .Take(10)
                .ToList();

            var result = new List<DispatchRecommendationDto>(filtered.Count);
            foreach (var x in filtered)
            {
                var request = x.Request;
                var cargo = x.CargoRequirement;
                var pickupPoint = CreatePoint(x.Item.픽업_위도, x.Item.픽업_경도);
                var dropoffPoint = CreatePoint(x.Item.하차_위도, x.Item.하차_경도);
                var fit = request is null ? null : _적합성Service.판정(vehicleSpec, request, cargo);
                var scheduleEvaluation = isDriving && request is not null && currentSchedulePlan is not null
                    ? await _운송일정삽입평가Service.평가Async(currentSchedulePlan, request)
                    : null;
                var insertionResult = await _routeService.EstimateInsertionDelayAsync(originLocation, routeAnchorLocation, pickupPoint, dropoffPoint);
                var routeFromOriginToPickup = await _routeService.EstimateRouteAsync(originLocation, pickupPoint);
                var routeFromPickupToDropoff = await _routeService.EstimateRouteAsync(pickupPoint, dropoffPoint);
                var routeFromOriginToDropoff = await _routeService.EstimateRouteAsync(originLocation, dropoffPoint);
                var routeFromDropoffToReturn = await _routeService.EstimateRouteAsync(dropoffPoint, returnDestination.좌표);
                var routeFromOriginToReturn = await _routeService.EstimateRouteAsync(originLocation, returnDestination.좌표);

                var candidateDurationMinutes = SumMinutes(routeFromOriginToPickup?.Duration, routeFromPickupToDropoff?.Duration);
                var directDurationMinutes = routeFromOriginToDropoff?.Duration?.TotalMinutes;
                var additionalDelayMinutes = scheduleEvaluation?.총추가지연분
                    ?? insertionResult?.기존배송지연분
                    ?? (candidateDurationMinutes.HasValue && directDurationMinutes.HasValue
                    ? Math.Max(0m, candidateDurationMinutes.Value - (decimal)directDurationMinutes.Value)
                    : (decimal?)null);

                decimal? routeAnchorDistanceKm = null;
                if (routeAnchorLocation is not null && pickupPoint is not null)
                {
                    routeAnchorDistanceKm = Math.Round(_routeService.CalculateDistanceKm(routeAnchorLocation, pickupPoint) ?? 0m, 2);
                }

                var routeAnchorRoute = await _routeService.EstimateRouteAsync(routeAnchorLocation, pickupPoint);
                var pickupArrivalMinutes = routeAnchorRoute?.Duration?.TotalMinutes ?? routeFromOriginToPickup?.Duration?.TotalMinutes;
                decimal? pickupWindowSlackMinutes = null;
                var pickupWindowEnd = request?.픽업_시간창_종료일시;
                if (pickupWindowEnd.HasValue && pickupArrivalMinutes.HasValue)
                {
                    pickupWindowSlackMinutes = (decimal)((pickupWindowEnd.Value - DateTime.UtcNow).TotalMinutes - pickupArrivalMinutes.Value);
                }

                var 판단결과 = _판정Service.판정(request, additionalDelayMinutes, pickupWindowSlackMinutes, routeAnchorDistanceKm, fit, scheduleEvaluation);

                var emptyDistanceKm = routeFromOriginToPickup?.DistanceKm;
                var cargoDistanceKm = routeFromPickupToDropoff?.DistanceKm;
                var returnDistanceKm = routeFromDropoffToReturn?.DistanceKm;
                var directReturnDistanceKm = routeFromOriginToReturn?.DistanceKm;
                var totalEmptyDistanceKm = SumDistance(emptyDistanceKm, returnDistanceKm);
                var returnDetourDistanceKm = returnDistanceKm.HasValue && directReturnDistanceKm.HasValue
                    ? Math.Round(returnDistanceKm.Value - directReturnDistanceKm.Value, 2)
                    : (decimal?)null;
                var totalDistanceKm = SumDistance(emptyDistanceKm, cargoDistanceKm, returnDistanceKm);
                var tollFare = SumMoney(routeFromOriginToPickup?.TollFare, routeFromPickupToDropoff?.TollFare);
                var estimatedFuelCost = EstimateFuelCost(totalDistanceKm, fuelEfficiencyKmPerLiter, fuelPricePerLiter);
                var estimatedRevenue = ResolveEstimatedRevenue(request);
                var estimatedTotalCost = SumMoney(tollFare, estimatedFuelCost);
                var estimatedExtraProfit = estimatedRevenue.HasValue && estimatedTotalCost.HasValue
                    ? estimatedRevenue.Value - estimatedTotalCost.Value
                    : (decimal?)null;

                var 평가결과 = _평가Service.평가(
                    request,
                    판단결과,
                    scheduleEvaluation,
                    estimatedExtraProfit,
                    additionalDelayMinutes,
                    routeAnchorDistanceKm,
                    candidateDurationMinutes,
                    pickupWindowSlackMinutes,
                    returnDetourDistanceKm,
                    returnDestination.복귀지기준사용됨,
                    returnDestination.출처);

                result.Add(new DispatchRecommendationDto
                {
                    의뢰Id = x.Item.의뢰Id,
                    화물종류 = request?.화물종류 ?? x.Item.픽업_도로명주소,
                    픽업지 = x.Item.픽업_도로명주소,
                    하차지 = x.Item.하차_도로명주소,
                    픽업_위도 = x.Item.픽업_위도,
                    픽업_경도 = x.Item.픽업_경도,
                    하차_위도 = x.Item.하차_위도,
                    하차_경도 = x.Item.하차_경도,
                    직선거리Km = x.CandidateDistanceKm.HasValue ? Math.Round((decimal)x.CandidateDistanceKm.Value, 2) : null,
                    픽업거리Km = emptyDistanceKm.HasValue ? Math.Round(emptyDistanceKm.Value, 2) : null,
                    공차거리Km = totalEmptyDistanceKm.HasValue ? Math.Round(totalEmptyDistanceKm.Value, 2) : null,
                    운송거리Km = cargoDistanceKm.HasValue ? Math.Round(cargoDistanceKm.Value, 2) : null,
                    복귀예상거리Km = returnDistanceKm.HasValue ? Math.Round(returnDistanceKm.Value, 2) : null,
                    지금바로복귀거리Km = directReturnDistanceKm.HasValue ? Math.Round(directReturnDistanceKm.Value, 2) : null,
                    복귀우회증가거리Km = returnDetourDistanceKm,
                    총공차거리Km = totalEmptyDistanceKm.HasValue ? Math.Round(totalEmptyDistanceKm.Value, 2) : null,
                    주행거리Km = totalDistanceKm.HasValue ? Math.Round(totalDistanceKm.Value, 2) : null,
                    예상톨비 = tollFare.HasValue ? Math.Round(tollFare.Value, 0) : null,
                    예상연료비 = estimatedFuelCost.HasValue ? Math.Round(estimatedFuelCost.Value, 0) : null,
                    예상총비용 = estimatedTotalCost.HasValue ? Math.Round(estimatedTotalCost.Value, 0) : null,
                    예상수익 = estimatedRevenue.HasValue ? Math.Round(estimatedRevenue.Value, 0) : null,
                    예상추가순이익 = estimatedExtraProfit.HasValue ? Math.Round(estimatedExtraProfit.Value, 0) : null,
                    분당추가수익 = estimatedExtraProfit.HasValue && candidateDurationMinutes.HasValue && candidateDurationMinutes.Value > 0
                        ? Math.Round(estimatedExtraProfit.Value / candidateDurationMinutes.Value, 0)
                        : null,
                    추천유형 = 판단결과.추천유형,
                    추가예상시간분 = candidateDurationMinutes.HasValue ? Math.Round(candidateDurationMinutes.Value, 0) : null,
                    기존배송지연분 = additionalDelayMinutes.HasValue ? Math.Round(additionalDelayMinutes.Value, 0) : null,
                    기존경로거리Km = insertionResult?.기존경로거리Km,
                    삽입경로거리Km = insertionResult?.삽입경로거리Km,
                    기존경로소요시간분 = insertionResult?.기존경로소요시간분,
                    삽입경로소요시간분 = insertionResult?.삽입경로소요시간분,
                    삽입추가톨비 = insertionResult?.삽입추가톨비,
                    추천점수 = 평가결과.추천점수,
                    추천사유 = 평가결과.추천사유,
                    일정삽입가능여부 = scheduleEvaluation?.삽입가능여부 ?? !isDriving,
                    전체일정완수가능여부 = scheduleEvaluation?.전체완수가능여부 ?? true,
                    최적삽입인덱스 = scheduleEvaluation?.최적삽입인덱스,
                    최대시간위반분 = scheduleEvaluation?.최대시간위반분,
                    일정위반사유 = scheduleEvaluation?.위반사유 ?? Array.Empty<string>(),
                    복귀지기준추천여부 = returnDestination.복귀지기준사용됨,
                    복귀지출처 = returnDestination.출처,
                    복귀추천사유 = 평가결과.복귀추천사유,
                    배지 = 평가결과.배지,
                    경고 = fit is null ? 평가결과.경고 : 평가결과.경고.Concat(fit.경고).ToArray(),
                    차량적합여부 = fit?.적합여부 ?? true,
                    차량부적합사유 = fit?.부적합사유 ?? Array.Empty<string>(),
                    차량경고 = fit?.경고 ?? Array.Empty<string>(),
                    상태 = x.Item.상태,
                    배차상태 = 상태값.배차상태.대기
                });
            }

            return result;
        }

        private static decimal? ResolveEstimatedRevenue(화주운송의뢰? request)
        {
            if (request is null)
            {
                return null;
            }

            if (request.최종운임.HasValue)
            {
                return request.최종운임.Value;
            }

            return request.결제예정금액.HasValue ? request.결제예정금액.Value : null;
        }
    }
}
