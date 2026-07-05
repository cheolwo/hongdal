using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed class 관리자기사목록조회QueryHandler : IRequestHandler<관리자기사목록조회Query, IReadOnlyList<기사목록응답>>
{
    private readonly HongdalContext _db;

    public 관리자기사목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<기사목록응답>> Handle(관리자기사목록조회Query request, CancellationToken cancellationToken)
    {
        var query = _db.용달기사.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.운행상태))
        {
            var status = request.운행상태.Trim();
            query = query.Where(x => x.운행상태 == status);
        }

        if (!string.IsNullOrWhiteSpace(request.차량종류))
        {
            var vehicle = request.차량종류.Trim();
            query = query.Where(x => x.차량 == vehicle);
        }

        if (!string.IsNullOrWhiteSpace(request.활동지역검색어))
        {
            var keyword = request.활동지역검색어.Trim();
            query = query.Where(x => x.주_활동지역.Contains(keyword));
        }

        var items = await query
            .OrderBy(x => x.기사명)
            .Select(x => new 기사목록응답
            {
                기사Id = x.기사Id,
                기사명 = x.기사명,
                연락처 = x.연락처,
                차량 = x.차량,
                주_활동지역 = x.주_활동지역,
                운행상태 = x.운행상태,
                최근위도 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (decimal?)l.위도)
                    .FirstOrDefault(),
                최근경도 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (decimal?)l.경도)
                    .FirstOrDefault(),
                최근위치기록시각 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (DateTime?)l.기록시각)
                    .FirstOrDefault(),
                배차건수 = _db.기사배차.Count(d => d.용달기사_id == x.Id || d.기사Id == x.Id)
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
