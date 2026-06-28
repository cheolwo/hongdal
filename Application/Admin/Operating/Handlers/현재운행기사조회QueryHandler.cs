using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed class 현재운행기사조회QueryHandler : IRequestHandler<현재운행기사조회Query, IReadOnlyList<현재운행기사응답>>
{
    private readonly HongdalContext _db;

    public 현재운행기사조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<현재운행기사응답>> Handle(현재운행기사조회Query request, CancellationToken cancellationToken)
    {
        var 요청 = request.Request ?? new 현재운행기사조회요청();

        var 운행상태 = string.IsNullOrWhiteSpace(요청.운행상태)
            ? 상태값.기사운행상태.운행중
            : 요청.운행상태.Trim();

        var 기사조회 = _db.용달기사
            .AsNoTracking()
            .Where(x => x.운행상태 == 운행상태);

        if (!string.IsNullOrWhiteSpace(요청.기사명검색어))
        {
            var 기사명검색어 = 요청.기사명검색어.Trim();
            기사조회 = 기사조회.Where(x => x.기사명.Contains(기사명검색어));
        }

        if (!string.IsNullOrWhiteSpace(요청.활동지역검색어))
        {
            var 활동지역검색어 = 요청.활동지역검색어.Trim();
            기사조회 = 기사조회.Where(x => x.주_활동지역.Contains(활동지역검색어));
        }

        return await 기사조회
            .OrderBy(x => x.기사명)
            .Select(x => new 현재운행기사응답
            {
                기사Id = x.기사Id,
                기사명 = x.기사명,
                연락처 = x.연락처,
                차량 = x.차량,
                주_활동지역 = x.주_활동지역,
                운행상태 = x.운행상태,
                최근근무시작시각 = _db.기사근무
                    .Where(s => s.기사Id == x.기사Id)
                    .OrderByDescending(s => s.시작시각)
                    .Select(s => s.시작시각)
                    .FirstOrDefault(),
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
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }
}
