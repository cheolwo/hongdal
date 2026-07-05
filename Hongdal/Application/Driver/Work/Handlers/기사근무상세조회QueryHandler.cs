using Hongdal.Contracts.Driver.Work;

namespace Hongdal.Application.Driver.Work;

public sealed class 기사근무상세조회QueryHandler : IRequestHandler<기사근무상세조회Query, 기사근무요약응답?>
{
    private readonly HongdalContext _db;

    public 기사근무상세조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사근무요약응답?> Handle(기사근무상세조회Query request, CancellationToken cancellationToken)
    {
        var currentShiftId = await _db.기사근무
            .AsNoTracking()
            .Where(x => x.기사Id == request.기사Id)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var shift = await _db.기사근무
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사Id == request.기사Id, cancellationToken);

        if (shift == null)
        {
            return null;
        }

        return new 기사근무요약응답
        {
            Id = shift.Id,
            DriverId = shift.기사Id,
            StartMode = shift.시작모드,
            StartTime = shift.시작시각,
            StartLocation = shift.시작위치,
            ReturnDestination = shift.복귀지,
            IsReserved = string.Equals(shift.시작모드, "reserved", StringComparison.OrdinalIgnoreCase),
            IsCurrent = currentShiftId.HasValue && currentShiftId.Value == shift.Id,
            UpdatedAt = shift.UpdatedAt
        };
    }
}
