using Hongdal.Contracts.Driver.Reservation;

namespace Hongdal.Application.Driver.Reservation;

public sealed class 예약취소CommandHandler : IRequestHandler<예약취소Command, 기사예약취소응답>
{
    private readonly HongdalContext _db;

    public 예약취소CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사예약취소응답> Handle(예약취소Command request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var shift = await _db.기사근무
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사Id == request.기사Id, cancellationToken);

        if (shift is null)
        {
            throw new InvalidOperationException("예약을 찾을 수 없습니다.");
        }

        if (!string.Equals(shift.시작모드, "reserved", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("예약 건만 취소할 수 있습니다.");
        }

        if (shift.시작시각.HasValue && shift.시작시각.Value <= now)
        {
            throw new InvalidOperationException("이미 시작된 예약은 취소할 수 없습니다.");
        }

        _db.기사근무.Remove(shift);
        await _db.SaveChangesAsync(cancellationToken);

        return new 기사예약취소응답
        {
            Id = shift.Id,
            DriverId = shift.기사Id,
            Message = "예약이 취소되었습니다."
        };
    }
}
