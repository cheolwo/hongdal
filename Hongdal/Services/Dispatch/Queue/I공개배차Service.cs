using System.Threading;
using System.Threading.Tasks;
using Hongdal.Hubs;

namespace 홍달.Services.Dispatch.Queue
{
    public interface I공개배차Service
    {
        Task<IReadOnlyList<DispatchRecommendationDto>> GetPublicDispatchesAsync(string driverId, CancellationToken cancellationToken = default);
    }
}
