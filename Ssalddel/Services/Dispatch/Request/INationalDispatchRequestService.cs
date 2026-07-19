using Ssalddel.Hubs;

namespace 살뜰.Services.Dispatch.Request
{
    public interface INationalDispatchRequestService
    {
        Task<IReadOnlyList<DispatchRecommendationDto>> GetNationwideRequestsAsync(string driverId, CancellationToken cancellationToken = default);
    }
}



