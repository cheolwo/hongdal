using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Hongdal.Hubs;
using 홍달.도메인.기사;

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

    public abstract class 배차추천Service
    {
        protected readonly HongdalContext _db;
        protected readonly IDriverLocationStore _driverLocationStore;
        protected readonly IDriverRejectedRequestStore _rejectedRequestStore;
        protected readonly IDriverRecommendationPushService _pushService;
        protected readonly IDispatchRecommendationLogStore _logStore;
        protected readonly IHubContext<DispatchRecommendationHub> _hubContext;
        protected readonly I배차추천경로Service _routeService;
        protected readonly I배차추천판정Service _판정Service;
        protected readonly I배차추천평가Service _평가Service;
        protected readonly I기사운송일정구성Service _기사운송일정구성Service;
        protected readonly I운송일정삽입평가Service _운송일정삽입평가Service;
        protected readonly IOpinetAveragePriceService _averagePriceService;

        protected const string 복귀지출처_오늘복귀지 = "오늘복귀지";
        protected const string 복귀지출처_기본복귀지 = "기본복귀지";
        protected const string 복귀지출처_없음 = "없음";

        protected 배차추천Service(
            HongdalContext db,
            IDriverLocationStore driverLocationStore,
            IDriverRejectedRequestStore rejectedRequestStore,
            IDriverRecommendationPushService pushService,
            IDispatchRecommendationLogStore logStore,
            IHubContext<DispatchRecommendationHub> hubContext,
            I배차추천경로Service routeService,
            I배차추천판정Service 판정Service,
            I배차추천평가Service 평가Service,
            I기사운송일정구성Service 기사운송일정구성Service,
            I운송일정삽입평가Service 운송일정삽입평가Service,
            IOpinetAveragePriceService averagePriceService)
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
            _기사운송일정구성Service = 기사운송일정구성Service;
            _운송일정삽입평가Service = 운송일정삽입평가Service;
            _averagePriceService = averagePriceService;
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId)
        {
            return await GetRecommendationsAsync(driverId, null);
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, 배차추천검색조건? criteria)
        {
            var isDriving = await IsDrivingAsync(driverId);
            return isDriving
                ? await GetDrivingRecommendationsAsync(driverId, criteria)
                : await GetIdleRecommendationsAsync(driverId, criteria);
        }

        protected abstract Task<bool> IsDrivingAsync(string driverId);
        public abstract Task<IReadOnlyList<DispatchRecommendationDto>> GetDrivingRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null);
        public abstract Task<IReadOnlyList<DispatchRecommendationDto>> GetIdleRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null);

        public virtual async Task SendToDriverAsync(string driverId)
        {
            var recommendations = await GetRecommendationsAsync(driverId);
            await _hubContext.Clients.Group(BuildDriverGroup(driverId)).SendAsync("ReceiveDispatchRecommendations", recommendations);
            await _pushService.SendAsync(driverId, recommendations);

            await _logStore.AppendAsync(new DispatchRecommendationLogEntry(
                driverId,
                DateTime.UtcNow,
                recommendations.Count,
                recommendations.Select(x => x.의뢰Id).ToList()));
        }

        protected async Task<decimal?> ResolveFuelPricePerLiterAsync()
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

        protected static decimal ResolveFuelEfficiencyKmPerLiter(string? vehicle)
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

        protected static decimal? EstimateFuelCost(decimal? totalDistanceKm, decimal fuelEfficiencyKmPerLiter, decimal? fuelPricePerLiter)
        {
            if (!totalDistanceKm.HasValue || !fuelPricePerLiter.HasValue || fuelEfficiencyKmPerLiter <= 0)
            {
                return null;
            }

            return totalDistanceKm.Value / fuelEfficiencyKmPerLiter * fuelPricePerLiter.Value;
        }

        protected static decimal? SumDistance(params decimal?[] distances)
        {
            var values = distances.Where(x => x.HasValue).Select(x => x!.Value).ToList();
            return values.Count == 0 ? null : values.Sum();
        }

        protected static decimal? SumMoney(params decimal?[] values)
        {
            var list = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
            return list.Count == 0 ? null : list.Sum();
        }

        protected static decimal? SumMinutes(params TimeSpan?[] durations)
        {
            var values = durations.Where(x => x.HasValue).Select(x => (decimal)x!.Value.TotalMinutes).ToList();
            return values.Count == 0 ? null : values.Sum();
        }

        protected static 배차경로좌표? CreatePoint(decimal? latitude, decimal? longitude)
        {
            return latitude.HasValue && longitude.HasValue ? new 배차경로좌표(latitude.Value, longitude.Value) : null;
        }

        protected async Task<복귀지결정결과> ResolveReturnDestinationAsync(string driverId, 용달기사? driver)
        {
            var currentShift = await _db.기사근무
                .AsNoTracking()
                .Where(x => x.기사Id == driverId)
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(currentShift?.오늘의복귀지주소)
                && currentShift.오늘의복귀지위도.HasValue
                && currentShift.오늘의복귀지경도.HasValue)
            {
                return new 복귀지결정결과(
                    currentShift.오늘의복귀지주소,
                    new 배차경로좌표(currentShift.오늘의복귀지위도.Value, currentShift.오늘의복귀지경도.Value),
                    string.IsNullOrWhiteSpace(currentShift.복귀지출처) ? 복귀지출처_오늘복귀지 : currentShift.복귀지출처,
                    true);
            }

            if (!string.IsNullOrWhiteSpace(driver?.기본복귀지주소)
                && driver.기본복귀지위도.HasValue
                && driver.기본복귀지경도.HasValue)
            {
                return new 복귀지결정결과(
                    driver.기본복귀지주소,
                    new 배차경로좌표(driver.기본복귀지위도.Value, driver.기본복귀지경도.Value),
                    복귀지출처_기본복귀지,
                    true);
            }

            return new 복귀지결정결과(null, null, 복귀지출처_없음, false);
        }

        protected virtual string BuildDriverGroup(string driverId) => $"driver-{driverId}";
    }
}



