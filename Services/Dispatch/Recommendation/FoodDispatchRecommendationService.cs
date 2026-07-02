using Microsoft.AspNetCore.SignalR;
using Hongdal.Hubs;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I음식배차추천Service
    {
        Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null);
    }

    public sealed class 음식배차추천Service : 배차추천Service, I음식배차추천Service
    {
        public 음식배차추천Service(
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
        }

        public Task<IReadOnlyList<DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null)
        {
            return base.GetRecommendationsAsync(driverId, criteria);
        }

        protected override Task<bool> IsDrivingAsync(string driverId)
        {
            return Task.FromResult(false);
        }

        public override Task<IReadOnlyList<DispatchRecommendationDto>> GetDrivingRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null)
        {
            return Task.FromResult<IReadOnlyList<DispatchRecommendationDto>>(Array.Empty<DispatchRecommendationDto>());
        }

        public override Task<IReadOnlyList<DispatchRecommendationDto>> GetIdleRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null)
        {
            return Task.FromResult<IReadOnlyList<DispatchRecommendationDto>>(Array.Empty<DispatchRecommendationDto>());
        }
    }
}
