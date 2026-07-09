namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed partial class 배차추천경로Service
    {
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
