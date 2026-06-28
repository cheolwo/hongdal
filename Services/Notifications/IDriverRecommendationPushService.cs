using Hongdal.Hubs;

namespace 홍달.Services.Notifications
{
    public interface IDriverRecommendationPushService
    {
        Task<bool> SendAsync(string driverId, IReadOnlyList<DispatchRecommendationDto> recommendations, CancellationToken cancellationToken = default);
    }
}

