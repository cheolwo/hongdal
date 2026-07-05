using Hongdal.Application.CommonContents.Queries;
using Hongdal.Contracts.CommonContents;
using 홍달.도메인.공통콘텐츠;

namespace Hongdal.Application.CommonContents.Handlers;

public sealed class 위젯콘텐츠조회QueryHandler : IRequestHandler<위젯콘텐츠조회Query, 홍달위젯콘텐츠Dto?>
{
    private readonly HongdalContext _db;

    public 위젯콘텐츠조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<홍달위젯콘텐츠Dto?> Handle(위젯콘텐츠조회Query request, CancellationToken cancellationToken)
    {
        var 현재시각 = DateTimeOffset.UtcNow;
        var 노출위치 = request.위치?.ToLowerInvariant() switch
        {
            "home" => 홍달노출위치.홈화면위젯,
            "lock" => 홍달노출위치.잠금화면위젯,
            _ => 홍달노출위치.홈화면위젯
        };

        var query = _db.홍달공통콘텐츠
            .AsNoTracking()
            .Where(x => x.활성화여부)
            .Where(x => (x.노출위치 & 노출위치) == 노출위치)
            .Where(x => x.노출시작시각 == null || x.노출시작시각 <= 현재시각)
            .Where(x => x.노출종료시각 == null || x.노출종료시각 >= 현재시각);

        query = request.역할?.ToLowerInvariant() switch
        {
            "driver" => query.Where(x => x.기사노출),
            "shipper" => query.Where(x => x.화주노출),
            "admin" => query.Where(x => x.운영자노출),
            _ => query.Where(x => x.기사노출)
        };

        var 콘텐츠 = await query
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return 콘텐츠?.ToWidgetDto();
    }
}