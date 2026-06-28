using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed class 관리자기사단건조회QueryHandler : IRequestHandler<관리자기사단건조회Query, 기사상세응답?>
{
    private readonly HongdalContext _db;

    public 관리자기사단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사상세응답?> Handle(관리자기사단건조회Query request, CancellationToken cancellationToken)
    {
        return await _db.용달기사
            .AsNoTracking()
            .Where(x => x.기사Id == request.DriverId)
            .Select(x => new 기사상세응답
            {
                기사Id = x.기사Id,
                기사명 = x.기사명,
                연락처 = x.연락처,
                차량 = x.차량,
                주_활동지역 = x.주_활동지역,
                운행상태 = x.운행상태,
                메모 = x.메모,
                등록일 = x.등록일,
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
