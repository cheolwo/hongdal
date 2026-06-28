using Hongdal.Contracts.Driver.Reservation;

namespace Hongdal.Application.Driver.Reservation;

public sealed class 예약목록조회QueryHandler : IRequestHandler<예약목록조회Query, IReadOnlyList<기사예약목록응답>>
{
    private readonly HongdalContext _db;

    public 예약목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<기사예약목록응답>> Handle(예약목록조회Query request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var items = await _db.기사근무
            .AsNoTracking()
            .Where(x => x.기사Id == request.기사Id && string.Equals(x.시작모드, "reserved", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.시작시각)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new 기사예약목록응답
            {
                Id = x.Id,
                DriverId = x.기사Id,
                StartMode = x.시작모드,
                StartTime = x.시작시각,
                StartLocation = x.시작위치,
                ReturnDestination = x.복귀지,
                IsFuture = x.시작시각.HasValue && x.시작시각.Value > now
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
