namespace Hongdal.Application.Driver.Work;

public sealed class 현재근무조회QueryHandler : IRequestHandler<현재근무조회Query, 기사현재근무응답>
{
    private readonly HongdalContext _db;

    public 현재근무조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사현재근무응답> Handle(현재근무조회Query request, CancellationToken cancellationToken)
    {
        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == request.기사Id, cancellationToken)
            ?? throw new InvalidOperationException("용달기사 정보를 찾을 수 없습니다.");

        var currentShift = await _db.기사근무
            .AsNoTracking()
            .Where(x => x.기사Id == request.기사Id)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new 기사현재근무응답
        {
            ShiftId = currentShift?.Id,
            DriverId = request.기사Id,
            운행상태 = driver.운행상태,
            시작모드 = currentShift?.시작모드 ?? string.Empty,
            시작시각 = currentShift?.시작시각,
            시작위치 = currentShift?.시작위치 ?? string.Empty,
            복귀지 = currentShift?.복귀지,
            오늘의복귀지주소 = currentShift?.오늘의복귀지주소,
            오늘의복귀지위도 = currentShift?.오늘의복귀지위도,
            오늘의복귀지경도 = currentShift?.오늘의복귀지경도,
            복귀지출처 = currentShift?.복귀지출처
        };
    }
}
