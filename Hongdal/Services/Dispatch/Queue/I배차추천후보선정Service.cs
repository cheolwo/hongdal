using System.Threading;
using System.Threading.Tasks;

namespace 홍달.Services.Dispatch.Queue
{
    public interface I배차추천후보선정Service
    {
        Task<배차추천후보?> 다음후보선정Async(string requestId, string? 제외기사Id = null, CancellationToken cancellationToken = default);
    }

    public sealed record 배차추천후보(string DriverId, decimal 추천점수, string 추천사유);
}
