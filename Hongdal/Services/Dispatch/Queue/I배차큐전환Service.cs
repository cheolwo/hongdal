using System.Threading;
using System.Threading.Tasks;

namespace 홍달.Services.Dispatch.Queue
{
    public interface I배차큐전환Service
    {
        Task 계획배차에서추천으로전환Async(string requestId, CancellationToken cancellationToken = default);
        Task 추천대기처리Async(string requestId, CancellationToken cancellationToken = default);
        Task 추천시작Async(string requestId, string driverId, int? timeoutSeconds = null, CancellationToken cancellationToken = default);
        Task 추천거절처리Async(string requestId, string driverId, CancellationToken cancellationToken = default);
        Task 추천만료처리Async(string requestId, CancellationToken cancellationToken = default);
        Task 공개배차로전환Async(string requestId, CancellationToken cancellationToken = default);
        Task 배차확정처리Async(string requestId, string driverId, CancellationToken cancellationToken = default);
    }
}
