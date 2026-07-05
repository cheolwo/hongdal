using Hongdal.Contracts.Driver.Reservation;

namespace Hongdal.Application.Driver.Reservation;

public sealed class 예약상세조회QueryHandler : IRequestHandler<예약상세조회Query, 기사예약응답>
{
    private readonly HongdalContext _db;

    public 예약상세조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사예약응답> Handle(예약상세조회Query request, CancellationToken cancellationToken)
    {
        var shift = await _db.기사근무.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (shift is null || shift.기사Id != request.기사Id)
        {
            throw new InvalidOperationException("예약을 찾을 수 없습니다.");
        }

        return new 기사예약응답
        {
            Id = shift.Id,
            DriverId = shift.기사Id,
            StartMode = shift.시작모드,
            StartTime = shift.시작시각,
            StartLocation = shift.시작위치,
            ReturnDestination = shift.복귀지
        };
    }
}
