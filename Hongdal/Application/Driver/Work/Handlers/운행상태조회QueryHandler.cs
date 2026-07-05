namespace Hongdal.Application.Driver.Work;

public sealed class 운행상태조회QueryHandler : IRequestHandler<운행상태조회Query, 기사운행상태응답>
{
    private readonly HongdalContext _db;

    public 운행상태조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사운행상태응답> Handle(운행상태조회Query request, CancellationToken cancellationToken)
    {
        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == request.기사Id, cancellationToken)
            ?? throw new InvalidOperationException("용달기사 정보를 찾을 수 없습니다.");

        return new 기사운행상태응답
        {
            DriverId = request.기사Id,
            Status = driver.운행상태,
            UpdatedAt = driver.UpdatedAt
        };
    }
}
