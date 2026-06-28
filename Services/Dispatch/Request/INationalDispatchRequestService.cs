using Hongdal.Hubs;

namespace 홍달.Services.Dispatch.Request
{
    public interface INationalDispatchRequestService
    {
        Task<IReadOnlyList<DispatchRecommendationDto>> GetNationwideRequestsAsync(string driverId, CancellationToken cancellationToken = default);
    }
}



