namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I운행중배차추천Service
    {
        Task<IReadOnlyList<Hongdal.Hubs.DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null);
    }

    public sealed class 운행중배차추천Service : I운행중배차추천Service
    {
        private readonly I배차추천Service _배차추천Service;

        public 운행중배차추천Service(I배차추천Service 배차추천Service)
        {
            _배차추천Service = 배차추천Service;
        }

        public Task<IReadOnlyList<Hongdal.Hubs.DispatchRecommendationDto>> GetRecommendationsAsync(string driverId, 배차추천검색조건? criteria = null)
        {
            return _배차추천Service.GetDrivingRecommendationsAsync(driverId, criteria);
        }
    }
}