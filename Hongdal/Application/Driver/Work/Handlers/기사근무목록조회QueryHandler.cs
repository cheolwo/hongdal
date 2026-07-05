using Hongdal.Contracts.Driver.Work;

namespace Hongdal.Application.Driver.Work;

public sealed class 기사근무목록조회QueryHandler : IRequestHandler<기사근무목록조회Query, IReadOnlyList<기사근무요약응답>>
{
    private readonly HongdalContext _db;

    public 기사근무목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<기사근무요약응답>> Handle(기사근무목록조회Query request, CancellationToken cancellationToken)
    {
        var currentShiftId = await 현재근무Id조회Async(request.기사Id, cancellationToken);

        return await _db.기사근무
            .AsNoTracking()
            .Where(x => x.기사Id == request.기사Id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new 기사근무요약응답
            {
                Id = x.Id,
                DriverId = x.기사Id,
                StartMode = x.시작모드,
                StartTime = x.시작시각,
                StartLocation = x.시작위치,
                ReturnDestination = x.복귀지,
                IsReserved = string.Equals(x.시작모드, "reserved", StringComparison.OrdinalIgnoreCase),
                IsCurrent = currentShiftId.HasValue && currentShiftId.Value == x.Id,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<long?> 현재근무Id조회Async(string 기사Id, CancellationToken cancellationToken)
    {
        return await _db.기사근무
            .AsNoTracking()
            .Where(x => x.기사Id == 기사Id)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
