namespace Hongdal.Application.Driver.Work;

public sealed class 운행상태조회QueryHandler : IRequestHandler<운행상태조회Query, 기사운행상태응답>
{
    private readonly HongdalContext _db;
    private readonly I국내화물운송기사상태Store _국내화물운송기사상태Store;

    public 운행상태조회QueryHandler(
        HongdalContext db,
        I국내화물운송기사상태Store 국내화물운송기사상태Store)
    {
        _db = db;
        _국내화물운송기사상태Store = 국내화물운송기사상태Store;
    }

    public async Task<기사운행상태응답> Handle(운행상태조회Query request, CancellationToken cancellationToken)
    {
        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == request.기사Id, cancellationToken)
            ?? throw new InvalidOperationException("용달기사 정보를 찾을 수 없습니다.");
        var osState = await _국내화물운송기사상태Store.GetAsync(request.기사Id, cancellationToken);

        return new 기사운행상태응답
        {
            DriverId = request.기사Id,
            Status = driver.운행상태,
            UpdatedAt = driver.UpdatedAt,
            현재위도 = osState?.Latitude,
            현재경도 = osState?.Longitude,
            최근위치수신시각 = osState?.위치수신시각Utc,
            Aging점수 = osState?.Aging점수,
            Aging기준시각 = osState?.Aging기준시각Utc,
            복귀콜선호 = osState?.복귀콜선호
        };
    }
}
