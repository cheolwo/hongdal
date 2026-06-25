using Hongdal.Hubs;

namespace 홍달.Services
{
    public interface INationalDispatchRequestService
    {
        Task<IReadOnlyList<DispatchRecommendationDto>> GetNationwideRequestsAsync(string driverId, CancellationToken cancellationToken = default);
    }
}
