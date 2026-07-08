using Microsoft.EntityFrameworkCore;
using Hongdal;
using 홍달.도메인.공통;
using 홍달.Services.Dispatch.Engine;

namespace 홍달.Services.Dispatch.Queue
{
    public sealed class 배차추천후보선정Service : I배차추천후보선정Service
    {
        private readonly HongdalContext _db;
        private readonly IReadOnlyDictionary<int, I운송의뢰배차엔진> _engines;

        public 배차추천후보선정Service(
            HongdalContext db,
            IEnumerable<I운송의뢰배차엔진> engines)
        {
            _db = db;
            _engines = engines
                .GroupBy(x => x.배차업무유형)
                .ToDictionary(x => x.Key, x => x.First());
        }

        public async Task<배차추천후보?> 다음후보선정Async(string requestId, string? 제외기사Id = null, CancellationToken cancellationToken = default)
        {
            var queue = await _db.배차대기.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
            if (queue is null)
            {
                return null;
            }

            if (queue.배차큐단계 is 상태값.배차큐단계.확정 or 상태값.배차큐단계.종료)
            {
                return null;
            }

            if (queue.배차노출상태 == 상태값.배차노출상태.추천중
                && !string.IsNullOrWhiteSpace(queue.현재추천대상기사Id)
                && (!queue.추천만료시각.HasValue || queue.추천만료시각 > DateTime.UtcNow))
            {
                return null;
            }

            if (!_engines.TryGetValue(queue.배차업무유형, out var engine))
            {
                return null;
            }

            return await engine.다음후보선정Async(queue, 제외기사Id, cancellationToken);
        }
    }
}
