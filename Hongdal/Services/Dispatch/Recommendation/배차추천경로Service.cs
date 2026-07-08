using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 홍달.Data;
using 홍달.도메인.기사;
using 홍달.Services.External.Google;
using 홍달.Services.External.Naver;
using 홍달.Services.Options;
using 홍달.Services.Storage.Local;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I배차추천경로Service
    {
        Task<배차경로좌표?> ResolveOriginLocationAsync(string driverId, 용달기사? driver, DriverLocationSnapshot? currentLocation, 배차추천검색조건? criteria);
        Task<배차경로좌표?> ResolveRouteAnchorLocationAsync(string driverId, 용달기사? driver, DriverLocationSnapshot? currentLocation);
        Task<배차경로예상결과?> EstimateRouteAsync(배차경로좌표? origin, 배차경로좌표? destination);
        Task<배차삽입경로예상결과?> EstimateInsertionDelayAsync(배차경로좌표? origin, 배차경로좌표? routeAnchor, 배차경로좌표? pickup, 배차경로좌표? dropoff);
        decimal? CalculateDistanceKm(배차경로좌표 source, 배차경로좌표 target);
    }

    public sealed class 배차추천경로Service : I배차추천경로Service
    {
        private readonly HongdalContext _db;
        private readonly IDriverWorkQueueStore _driverWorkQueueStore;
        private readonly IGeocodingService _geocodingService;
        private readonly INaverCloudDirectionsService _routeService;
        private readonly NaverCloudDirectionsOptions _routeOptions;
        private readonly ILogger<배차추천경로Service> _logger;
        private readonly Dictionary<배차경로CacheKey, Task<배차경로예상결과?>> _routeEstimateCache = [];

        public 배차추천경로Service(
            HongdalContext db,
            IDriverWorkQueueStore driverWorkQueueStore,
            IGeocodingService geocodingService,
            INaverCloudDirectionsService routeService,
            IOptions<NaverCloudDirectionsOptions> routeOptions,
            ILogger<배차추천경로Service> logger)
        {
            _db = db;
            _driverWorkQueueStore = driverWorkQueueStore;
            _geocodingService = geocodingService;
            _routeService = routeService;
            _routeOptions = routeOptions.Value;
            _logger = logger;
        }

        public async Task<배차경로좌표?> ResolveOriginLocationAsync(string driverId, 용달기사? driver, DriverLocationSnapshot? currentLocation, 배차추천검색조건? criteria)
        {
            if (criteria is not null && criteria.RadiusKm > 0)
            {
                return new 배차경로좌표(criteria.Latitude, criteria.Longitude);
            }

            if (currentLocation is not null)
            {
                return new 배차경로좌표(currentLocation.Latitude, currentLocation.Longitude);
            }

            if (driver is not null)
            {
                var lastDispatch = await _db.기사배차
                    .AsNoTracking()
                    .Where(x => x.용달기사_id == driver.Id || x.기사Id == driver.Id)
                    .OrderByDescending(x => x.배차완료시각)
                    .ThenByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(lastDispatch?.배송지))
                {
                    var geocoded = await _geocodingService.GeocodeAsync(lastDispatch.배송지);
                    if (geocoded.HasValue)
                    {
                        return new 배차경로좌표(geocoded.Value.lat, geocoded.Value.lng);
                    }
                }
            }

            var workQueue = await _driverWorkQueueStore.SnapshotAsync();
            var queueItem = workQueue.FirstOrDefault(x => string.Equals(x.DriverId, driverId, StringComparison.Ordinal));
            if (queueItem is not null && !string.IsNullOrWhiteSpace(queueItem.StartLocation))
            {
                var geocoded = await _geocodingService.GeocodeAsync(queueItem.StartLocation);
                if (geocoded.HasValue)
                {
                    return new 배차경로좌표(geocoded.Value.lat, geocoded.Value.lng);
                }
            }

            return null;
        }

        public async Task<배차경로좌표?> ResolveRouteAnchorLocationAsync(string driverId, 용달기사? driver, DriverLocationSnapshot? currentLocation)
        {
            if (currentLocation is not null)
            {
                return new 배차경로좌표(currentLocation.Latitude, currentLocation.Longitude);
            }

            if (driver is not null)
            {
                var lastDispatch = await _db.기사배차
                    .AsNoTracking()
                    .Where(x => x.용달기사_id == driver.Id || x.기사Id == driver.Id)
                    .OrderByDescending(x => x.배차완료시각)
                    .ThenByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(lastDispatch?.배송지))
                {
                    var geocoded = await _geocodingService.GeocodeAsync(lastDispatch.배송지);
                    if (geocoded.HasValue)
                    {
                        return new 배차경로좌표(geocoded.Value.lat, geocoded.Value.lng);
                    }
                }
            }

            var workQueue = await _driverWorkQueueStore.SnapshotAsync();
            var queueItem = workQueue.FirstOrDefault(x => string.Equals(x.DriverId, driverId, StringComparison.Ordinal));
            if (queueItem is not null && !string.IsNullOrWhiteSpace(queueItem.ReturnDestination))
            {
                var geocoded = await _geocodingService.GeocodeAsync(queueItem.ReturnDestination);
                if (geocoded.HasValue)
                {
                    return new 배차경로좌표(geocoded.Value.lat, geocoded.Value.lng);
                }
            }

            return null;
        }

        public async Task<배차경로예상결과?> EstimateRouteAsync(배차경로좌표? origin, 배차경로좌표? destination)
        {
            if (origin is null || destination is null)
            {
                return null;
            }

            var cacheKey = 배차경로CacheKey.Create(origin, destination);
            if (_routeEstimateCache.TryGetValue(cacheKey, out var cachedTask))
            {
                return await cachedTask;
            }

            var task = EstimateRouteCoreAsync(origin, destination);
            _routeEstimateCache[cacheKey] = task;

            try
            {
                return await task;
            }
            catch
            {
                _routeEstimateCache.Remove(cacheKey);
                throw;
            }
        }

        private async Task<배차경로예상결과?> EstimateRouteCoreAsync(배차경로좌표 origin, 배차경로좌표 destination)
        {
            try
            {
                var route = await _routeService.GetDrivingRouteAsync(origin.Latitude, origin.Longitude, destination.Latitude, destination.Longitude);
                if (route?.Duration is not null || route?.DistanceKm is not null)
                {
                    return new 배차경로예상결과(route.DistanceKm, route.Duration, route.TollFare);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "네이버 경로 API 호출에 실패해 fallback 경로 추정을 사용합니다. Origin={OriginLat},{OriginLng} Destination={DestinationLat},{DestinationLng}",
                    origin.Latitude,
                    origin.Longitude,
                    destination.Latitude,
                    destination.Longitude);
            }

            return EstimateFallbackRoute(origin, destination);
        }

        public async Task<배차삽입경로예상결과?> EstimateInsertionDelayAsync(배차경로좌표? origin, 배차경로좌표? routeAnchor, 배차경로좌표? pickup, 배차경로좌표? dropoff)
        {
            if (origin is null || pickup is null)
            {
                return null;
            }

            var baseRoute = await EstimateRouteAsync(origin, routeAnchor ?? dropoff);
            var insertedPickupRoute = await EstimateRouteAsync(origin, pickup);

            if (routeAnchor is null)
            {
                var insertedDropoffRoute = await EstimateRouteAsync(pickup, dropoff);
                if (insertedPickupRoute is null || insertedDropoffRoute is null)
                {
                    return null;
                }

                var insertedDistanceKm = SumMoney(insertedPickupRoute.DistanceKm, insertedDropoffRoute.DistanceKm);
                var insertedDurationMinutes = SumMinutes(insertedPickupRoute.Duration, insertedDropoffRoute.Duration);
                var baseDurationMinutes = baseRoute?.Duration?.TotalMinutes;
                var baseDistanceKm = baseRoute?.DistanceKm;
                var delayMinutes = insertedDurationMinutes.HasValue && baseDurationMinutes.HasValue
                    ? Math.Max(0m, insertedDurationMinutes.Value - (decimal)baseDurationMinutes.Value)
                    : (decimal?)null;

                return new 배차삽입경로예상결과(
                    baseDistanceKm,
                    insertedDistanceKm,
                    baseDurationMinutes.HasValue ? (decimal?)baseDurationMinutes.Value : null,
                    insertedDurationMinutes,
                    delayMinutes,
                    SumMoney(insertedPickupRoute.TollFare, insertedDropoffRoute.TollFare));
            }

            var insertedAnchorRoute = await EstimateRouteAsync(pickup, routeAnchor);
            var insertedDropoffAfterAnchorRoute = await EstimateRouteAsync(routeAnchor, dropoff);

            if (baseRoute is null || insertedPickupRoute is null || insertedAnchorRoute is null || insertedDropoffAfterAnchorRoute is null)
            {
                return null;
            }

            var baseDuration = baseRoute.Duration?.TotalMinutes;
            var insertedDuration = SumMinutes(insertedPickupRoute.Duration, insertedAnchorRoute.Duration, insertedDropoffAfterAnchorRoute.Duration);
            var delay = insertedDuration.HasValue && baseDuration.HasValue
                ? Math.Max(0m, insertedDuration.Value - (decimal)baseDuration.Value)
                : (decimal?)null;

            return new 배차삽입경로예상결과(
                baseRoute.DistanceKm,
                SumMoney(insertedPickupRoute.DistanceKm, insertedAnchorRoute.DistanceKm, insertedDropoffAfterAnchorRoute.DistanceKm),
                baseDuration.HasValue ? (decimal?)baseDuration.Value : null,
                insertedDuration,
                delay,
                SumMoney(insertedPickupRoute.TollFare, insertedAnchorRoute.TollFare, insertedDropoffAfterAnchorRoute.TollFare));
        }

        public decimal? CalculateDistanceKm(배차경로좌표 source, 배차경로좌표 target)
        {
            const double earthRadiusKm = 6371.0;
            var dLat = ToRadians((double)target.Latitude - (double)source.Latitude);
            var dLng = ToRadians((double)target.Longitude - (double)source.Longitude);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(ToRadians((double)source.Latitude)) * Math.Cos(ToRadians((double)target.Latitude))
                    * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (decimal)(earthRadiusKm * c);
        }

        private 배차경로예상결과? EstimateFallbackRoute(배차경로좌표 origin, 배차경로좌표 destination)
        {
            if (!_routeOptions.EnableFallbackRouteEstimate)
            {
                return null;
            }

            var directDistanceKm = CalculateDistanceKm(origin, destination);
            if (!directDistanceKm.HasValue)
            {
                return null;
            }

            var multiplier = _routeOptions.FallbackDistanceMultiplier <= 0m
                ? 1.25m
                : _routeOptions.FallbackDistanceMultiplier;
            var averageSpeedKmH = _routeOptions.FallbackAverageSpeedKmH <= 0m
                ? 45m
                : _routeOptions.FallbackAverageSpeedKmH;
            var estimatedDistanceKm = Math.Round(directDistanceKm.Value * multiplier, 2);
            var estimatedMinutes = estimatedDistanceKm == 0m
                ? 0m
                : estimatedDistanceKm / averageSpeedKmH * 60m;

            return new 배차경로예상결과(
                estimatedDistanceKm,
                TimeSpan.FromMinutes((double)estimatedMinutes),
                null);
        }

        private static decimal? SumMinutes(params TimeSpan?[] durations)
        {
            var values = durations.Where(x => x.HasValue).Select(x => (decimal)x!.Value.TotalMinutes).ToList();
            return values.Count == 0 ? null : values.Sum();
        }

        private static decimal? SumMoney(params decimal?[] values)
        {
            var list = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
            return list.Count == 0 ? null : list.Sum();
        }

        private static double ToRadians(double angle) => angle * Math.PI / 180.0;

        private readonly record struct 배차경로CacheKey(
            decimal OriginLatitude,
            decimal OriginLongitude,
            decimal DestinationLatitude,
            decimal DestinationLongitude)
        {
            public static 배차경로CacheKey Create(배차경로좌표 origin, 배차경로좌표 destination)
                => new(origin.Latitude, origin.Longitude, destination.Latitude, destination.Longitude);
        }
    }
}
