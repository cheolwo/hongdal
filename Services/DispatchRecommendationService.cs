using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Hongdal.Hubs;
using 홍달.Data;
using 홍달.도메인.공통;

namespace 홍달.Services
{
    public interface IDispatchRecommendationService
    {
        Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId);
        Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, DispatchRecommendationSearchCriteria? criteria);
        Task SendToDriverAsync(string driverId);
    }

    public sealed record DispatchRecommendationSearchCriteria(decimal Latitude, decimal Longitude, decimal RadiusKm);

    public sealed class DispatchRecommendationService : IDispatchRecommendationService
    {
        private readonly HongdalContext _db;
        private readonly IDriverLocationStore _driverLocationStore;
        private readonly IDriverRejectedRequestStore _rejectedRequestStore;
        private readonly IDriverRecommendationPushService _pushService;
        private readonly IDispatchRecommendationLogStore _logStore;
        private readonly IHubContext<DispatchRecommendationHub> _hubContext;
        private readonly IRouteDistanceService _routeDistanceService;

        public DispatchRecommendationService(
            HongdalContext db,
            IDriverLocationStore driverLocationStore,
            IDriverRejectedRequestStore rejectedRequestStore,
            IDriverRecommendationPushService pushService,
            IDispatchRecommendationLogStore logStore,
            IHubContext<DispatchRecommendationHub> hubContext,
            IRouteDistanceService routeDistanceService)
        {
            _db = db;
            _driverLocationStore = driverLocationStore;
            _rejectedRequestStore = rejectedRequestStore;
            _pushService = pushService;
            _logStore = logStore;
            _hubContext = hubContext;
            _routeDistanceService = routeDistanceService;
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId)
        {
            return await GetRecommendationsAsync(driverId, null);
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, DispatchRecommendationSearchCriteria? criteria)
        {
            _driverLocationStore.TryGetLatest(driverId, out var currentLocation);
            var rejectedRequestIds = await _rejectedRequestStore.GetRejectedRequestIdsAsync(driverId);
            var rejectedRequestIdSet = rejectedRequestIds.Count > 0
                ? new HashSet<string>(rejectedRequestIds, StringComparer.Ordinal)
                : null;

            var items = await _db.배차대기
                .Where(q => q.상태 == 상태값.배차대기상태.대기)
                .ToListAsync();

            var requestIds = items.Select(q => q.의뢰Id).Distinct().ToList();
            var requestMap = requestIds.Count == 0
                ? new Dictionary<string, 홍달.도메인.화주.화주운송의뢰>(StringComparer.Ordinal)
                : await _db.화주운송의뢰
                    .AsNoTracking()
                    .Where(r => requestIds.Contains(r.의뢰Id))
                    .ToDictionaryAsync(r => r.의뢰Id, StringComparer.Ordinal);

            var hasSearchCriteria = criteria is not null && criteria.RadiusKm > 0;
            var searchLatitude = hasSearchCriteria ? criteria!.Latitude : currentLocation?.위도;
            var searchLongitude = hasSearchCriteria ? criteria!.Longitude : currentLocation?.경도;
            var radiusKm = hasSearchCriteria ? criteria!.RadiusKm : (decimal?)null;

            var filtered = items
                .Where(q => rejectedRequestIdSet == null || !rejectedRequestIdSet.Contains(q.의뢰Id))
                .Select(q => new
                {
                    Item = q,
                    Request = requestMap.TryGetValue(q.의뢰Id, out var request) ? request : null,
                    CandidateDistanceKm = searchLatitude.HasValue && searchLongitude.HasValue && q.픽업_위도.HasValue && q.픽업_경도.HasValue
                        ? CalculateDistanceKm(
                            (double)searchLatitude.Value,
                            (double)searchLongitude.Value,
                            (double)q.픽업_위도.Value,
                            (double)q.픽업_경도.Value)
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
                decimal? routeDistanceKm = null;
                if (x.Item.픽업_위도.HasValue && x.Item.픽업_경도.HasValue && searchLatitude.HasValue && searchLongitude.HasValue)
                {
                    routeDistanceKm = await _routeDistanceService.GetDrivingDistanceKmAsync(
                        searchLatitude.Value,
                        searchLongitude.Value,
                        x.Item.픽업_위도.Value,
                        x.Item.픽업_경도.Value);
                }

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
                    주행거리Km = routeDistanceKm,
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

        private static double CalculateDistanceKm(double sourceLat, double sourceLng, double targetLat, double targetLng)
        {
            const double earthRadiusKm = 6371.0;
            var dLat = ToRadians(targetLat - sourceLat);
            var dLng = ToRadians(targetLng - sourceLng);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(ToRadians(sourceLat)) * Math.Cos(ToRadians(targetLat))
                    * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private static double ToRadians(double angle) => angle * Math.PI / 180.0;

        private static string GetDriverGroup(string driverId) => $"driver-{driverId}";
    }
}
