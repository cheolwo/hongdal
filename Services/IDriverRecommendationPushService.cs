using Hongdal.Hubs;

namespace 홍달.Services
{
    public interface IDriverRecommendationPushService
    {
        Task<bool> SendAsync(string driverId, IReadOnlyList<DispatchRecommendationDto> recommendations, CancellationToken cancellationToken = default);
    }
}
