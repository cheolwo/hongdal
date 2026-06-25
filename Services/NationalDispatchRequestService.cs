using Microsoft.EntityFrameworkCore;
using Hongdal.Hubs;
using 홍달.Data;
using 홍달.도메인.공통;

namespace 홍달.Services
{
    public sealed class NationalDispatchRequestService : INationalDispatchRequestService
    {
        private readonly HongdalContext _db;
        private readonly IDriverRejectedRequestStore _rejectedRequestStore;

        public NationalDispatchRequestService(HongdalContext db, IDriverRejectedRequestStore rejectedRequestStore)
        {
            _db = db;
            _rejectedRequestStore = rejectedRequestStore;
        }

        public async Task<IReadOnlyList<DispatchRecommendationDto>> GetNationwideRequestsAsync(string driverId, CancellationToken cancellationToken = default)
        {
            var rejectedRequestIds = await _rejectedRequestStore.GetRejectedRequestIdsAsync(driverId, cancellationToken);
            var rejectedRequestIdSet = rejectedRequestIds.Count > 0
                ? new HashSet<string>(rejectedRequestIds, StringComparer.Ordinal)
                : null;

            var items = await _db.배차대기
                .AsNoTracking()
                .Where(q => q.상태 == 상태값.배차대기상태.대기)
                .OrderByDescending(q => q.CreatedAt)
                .Take(100)
                .ToListAsync(cancellationToken);

            var requestIds = items.Select(q => q.의뢰Id).Distinct().ToList();
            var requestMap = requestIds.Count == 0
                ? new Dictionary<string, 홍달.도메인.화주.화주운송의뢰>(StringComparer.Ordinal)
                : await _db.화주운송의뢰
                    .AsNoTracking()
                    .Where(r => requestIds.Contains(r.의뢰Id))
                    .ToDictionaryAsync(r => r.의뢰Id, StringComparer.Ordinal, cancellationToken);

            return items
                .Where(q => rejectedRequestIdSet == null || !rejectedRequestIdSet.Contains(q.의뢰Id))
                .Select(q => new DispatchRecommendationDto
                {
                    의뢰Id = q.의뢰Id,
                    화물종류 = requestMap.TryGetValue(q.의뢰Id, out var request) ? request.화물종류 : q.픽업_도로명주소,
                    픽업지 = q.픽업_도로명주소,
                    하차지 = q.하차_도로명주소,
                    픽업_위도 = q.픽업_위도,
                    픽업_경도 = q.픽업_경도,
                    하차_위도 = q.하차_위도,
                    하차_경도 = q.하차_경도,
                    픽업거리Km = null,
                    상태 = q.상태,
                    배차상태 = 상태값.배차상태.대기
                })
                .ToList();
        }
    }
}
