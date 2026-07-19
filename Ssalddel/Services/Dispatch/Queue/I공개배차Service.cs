using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Hubs;

namespace 살뜰.Services.Dispatch.Queue
{
    public interface I공개배차Service
    {
        Task<IReadOnlyList<DispatchRecommendationDto>> GetPublicDispatchesAsync(string driverId, CancellationToken cancellationToken = default);
    }
}
