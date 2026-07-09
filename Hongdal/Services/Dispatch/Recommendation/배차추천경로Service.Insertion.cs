namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed partial class 배차추천경로Service
    {
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
    }
}
