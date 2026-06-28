using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Hongdal.Hubs;
using 홍달.도메인.화물;
using 홍달.도메인.화주;
using 홍달.도메인.차량;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I배차추천Service
    {
        Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId);
        Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, 배차추천검색조건? criteria);
        Task<IReadOnlyList<DispatchRecommendationDto>> GetDrivingRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null);
        Task<IReadOnlyList<DispatchRecommendationDto>> GetIdleRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null);
        Task SendToDriverAsync(string driverId);
    }

    public sealed record 배차추천검색조건(decimal Latitude, decimal Longitude, decimal RadiusKm);

    public sealed class 배차추천Service : I배차추천Service
    {
        private readonly HongdalContext _db;
        private readonly IDriverLocationStore _driverLocationStore;
        private readonly IDriverRejectedRequestStore _rejectedRequestStore;
        private readonly IDriverRecommendationPushService _pushService;
        private readonly IDispatchRecommendationLogStore _logStore;
        private readonly IHubContext<DispatchRecommendationHub> _hubContext;
        private readonly I배차추천경로Service _routeService;
        private readonly I배차추천판정Service _판정Service;
        private readonly I배차추천평가Service _평가Service;
        private readonly I차량화물적합성Service _적합성Service;
        private readonly IOpinetAveragePriceService _averagePriceService;

        public 배차추천Service(
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
            IOpinetAveragePriceService averagePriceService,
            IGeocodingService geocodingService)
        {
            _db = db;
            _driverLocationStore = driverLocationStore;
            _rejectedRequestStore = rejectedRequestStore;
            _pushService = pushService;
            _logStore = logStore;
            _hubContext = hubContext;
            _routeService = routeService;
            _판정Service = 판정Service;
            _평가Service = 평가Service;
            _적합성Service = 적합성Service;
            _averagePriceService = averagePriceService;
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId)
        {
            return await GetRecommendationsAsync(driverId, null);
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, 배차추천검색조건? criteria)
        {
            var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
            var isDriving = string.Equals(driver?.운행상태, 상태값.기사운행상태.운행중, StringComparison.Ordinal);
            return isDriving
                ? await GetDrivingRecommendationsAsync(driverId, criteria)
                : await GetIdleRecommendationsAsync(driverId, criteria);
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetDrivingRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null)
        {
            return await GetRecommendationsCoreAsync(driverId, criteria, isDriving: true);
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetIdleRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null)
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
                var insertionResult = await _routeService.EstimateInsertionDelayAsync(originLocation, routeAnchorLocation, pickupPoint, dropoffPoint);
                var routeFromOriginToPickup = await _routeService.EstimateRouteAsync(originLocation, pickupPoint);
                var routeFromPickupToDropoff = await _routeService.EstimateRouteAsync(pickupPoint, dropoffPoint);
                var routeFromOriginToDropoff = await _routeService.EstimateRouteAsync(originLocation, dropoffPoint);

                var candidateDurationMinutes = SumMinutes(routeFromOriginToPickup?.Duration, routeFromPickupToDropoff?.Duration);
                var directDurationMinutes = routeFromOriginToDropoff?.Duration?.TotalMinutes;
                var additionalDelayMinutes = insertionResult?.기존배송지연분 ?? (candidateDurationMinutes.HasValue && directDurationMinutes.HasValue
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

                var 판단결과 = _판정Service.판정(request, additionalDelayMinutes, pickupWindowSlackMinutes, routeAnchorDistanceKm, fit);

                var emptyDistanceKm = routeFromOriginToPickup?.DistanceKm;
                var cargoDistanceKm = routeFromPickupToDropoff?.DistanceKm;
                var totalDistanceKm = SumDistance(emptyDistanceKm, cargoDistanceKm);
                var tollFare = SumMoney(routeFromOriginToPickup?.TollFare, routeFromPickupToDropoff?.TollFare);
                var estimatedFuelCost = EstimateFuelCost(totalDistanceKm, fuelEfficiencyKmPerLiter, fuelPricePerLiter);
                var estimatedRevenue = ResolveEstimatedRevenue(x.Request);
                var estimatedTotalCost = SumMoney(tollFare, estimatedFuelCost);
                var estimatedExtraProfit = estimatedRevenue.HasValue && estimatedTotalCost.HasValue
                    ? estimatedRevenue.Value - estimatedTotalCost.Value
                    : (decimal?)null;

                var 평가결과 = _평가Service.평가(
                    request,
                    판단결과,
                    estimatedExtraProfit,
                    additionalDelayMinutes,
                    routeAnchorDistanceKm,
                    candidateDurationMinutes,
                    pickupWindowSlackMinutes);

                result.Add(new DispatchRecommendationDto
                {
                    의뢰Id = x.Item.의뢰Id,
                    화물종류 = x.Request?.화물종류 ?? x.Item.픽업_도로명주소,
                    픽업지 = x.Item.픽업_도로명주소,
                    하차지 = x.Item.하차_도로명주소,
                    픽업_위도 = x.Item.픽업_위도,
                    픽업_경도 = x.Item.픽업_경도,
                    하차_위도 = x.Item.하차_위도,
                    하차_경도 = x.Item.하차_경도,
                    직선거리Km = x.CandidateDistanceKm.HasValue ? Math.Round((decimal)x.CandidateDistanceKm.Value, 2) : null,
                    픽업거리Km = emptyDistanceKm.HasValue ? Math.Round(emptyDistanceKm.Value, 2) : null,
                    공차거리Km = emptyDistanceKm.HasValue ? Math.Round(emptyDistanceKm.Value, 2) : null,
                    운송거리Km = cargoDistanceKm.HasValue ? Math.Round(cargoDistanceKm.Value, 2) : null,
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

        public async Task SendToDriverAsync(string driverId)
        {
            var recommendations = await GetRecommendationsAsync(driverId);
            await _hubContext.Clients.Group(GetDriverGroup(driverId)).SendAsync("ReceiveDispatchRecommendations", recommendations);
            await _pushService.SendAsync(driverId, recommendations);

            await _logStore.AppendAsync(new DispatchRecommendationLogEntry(
                driverId,
                DateTime.UtcNow,
                recommendations.Count,
                recommendations.Select(x => x.의뢰Id).ToList()));
        }

        private async Task<decimal?> ResolveFuelPricePerLiterAsync()
        {
            var items = await _averagePriceService.GetAveragePricesAsync();
            if (items.Count == 0)
            {
                return 1700m;
            }

            var diesel = items.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ProductName)
                                                   && x.ProductName.Contains("경유", StringComparison.OrdinalIgnoreCase)
                                                   && x.Price.HasValue);
            if (diesel?.Price is not null)
            {
                return diesel.Price.Value;
            }

            return items.FirstOrDefault(x => x.Price.HasValue)?.Price ?? 1700m;
        }

        private static decimal ResolveFuelEfficiencyKmPerLiter(string? vehicle)
        {
            if (string.IsNullOrWhiteSpace(vehicle))
            {
                return 7m;
            }

            if (vehicle.Contains("전기", StringComparison.OrdinalIgnoreCase))
            {
                return 999m;
            }

            if (vehicle.Contains("냉동", StringComparison.OrdinalIgnoreCase))
            {
                return 5.5m;
            }

            if (vehicle.Contains("5톤", StringComparison.OrdinalIgnoreCase) || vehicle.Contains("대형", StringComparison.OrdinalIgnoreCase))
            {
                return 4.5m;
            }

            if (vehicle.Contains("1톤", StringComparison.OrdinalIgnoreCase) || vehicle.Contains("카고", StringComparison.OrdinalIgnoreCase) || vehicle.Contains("용달", StringComparison.OrdinalIgnoreCase))
            {
                return 8m;
            }

            return 7m;
        }

        private static decimal? EstimateFuelCost(decimal? totalDistanceKm, decimal fuelEfficiencyKmPerLiter, decimal? fuelPricePerLiter)
        {
            if (!totalDistanceKm.HasValue || !fuelPricePerLiter.HasValue || fuelEfficiencyKmPerLiter <= 0)
            {
                return null;
            }

            return totalDistanceKm.Value / fuelEfficiencyKmPerLiter * fuelPricePerLiter.Value;
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

        private static decimal? SumDistance(params decimal?[] distances)
        {
            var values = distances.Where(x => x.HasValue).Select(x => x!.Value).ToList();
            return values.Count == 0 ? null : values.Sum();
        }

        private static decimal? SumMoney(params decimal?[] values)
        {
            var list = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
            return list.Count == 0 ? null : list.Sum();
        }

        private static decimal? SumMinutes(params TimeSpan?[] durations)
        {
            var values = durations.Where(x => x.HasValue).Select(x => (decimal)x!.Value.TotalMinutes).ToList();
            return values.Count == 0 ? null : values.Sum();
        }

        private static 배차경로좌표? CreatePoint(decimal? latitude, decimal? longitude)
        {
            return latitude.HasValue && longitude.HasValue ? new 배차경로좌표(latitude.Value, longitude.Value) : null;
        }

        private static string GetDriverGroup(string driverId) => $"driver-{driverId}";

    }
}



