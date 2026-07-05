using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.기사;
using 홍달.Services.External.Google;
using 홍달.Services.External.Naver;
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

        public 배차추천경로Service(
            HongdalContext db,
            IDriverWorkQueueStore driverWorkQueueStore,
            IGeocodingService geocodingService,
            INaverCloudDirectionsService routeService)
        {
            _db = db;
            _driverWorkQueueStore = driverWorkQueueStore;
            _geocodingService = geocodingService;
            _routeService = routeService;
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

            var route = await _routeService.GetDrivingRouteAsync(origin.Latitude, origin.Longitude, destination.Latitude, destination.Longitude);
            return route is null ? null : new 배차경로예상결과(route.DistanceKm, route.Duration, route.TollFare);
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
    }
}