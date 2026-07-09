using Microsoft.EntityFrameworkCore;
using 홍달.도메인.기사;
using 홍달.Services.Storage.Local;

namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed partial class 배차추천경로Service
    {
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
    }
}
