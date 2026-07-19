using Ssalddel.Hubs;

namespace 살뜰.Services.Notifications
{
    public interface IDriverRecommendationPushService
    {
        Task<bool> SendAsync(string driverId, IReadOnlyList<DispatchRecommendationDto> recommendations, CancellationToken cancellationToken = default);
    }
}

