using Hongdal.Contracts.Driver.Reservation;
using FluentResults;
using Hongdal.Application.CommandProcessing;

namespace Hongdal.Application.Driver.Reservation;

public sealed class 예약생성CommandHandler : IRequestHandler<예약생성Command, Result<기사예약응답>>
{
    private readonly HongdalContext _db;
    private readonly I배차추천Service _dispatchRecommendationService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;

    public 예약생성CommandHandler(
        HongdalContext db,
        I배차추천Service dispatchRecommendationService,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사)
    {
        _db = db;
        _dispatchRecommendationService = dispatchRecommendationService;
        _currentUserAccessor = currentUserAccessor;
        _권한검사 = 권한검사;
    }

    public async Task<Result<기사예약응답>> Handle(예약생성Command request, CancellationToken cancellationToken)
    {
        if (!_권한검사.Try검증(_currentUserAccessor.UserId, _currentUserAccessor.Role, request.참여자Id, request.실행역할, out var 권한오류))
        {
            return Result.Fail<기사예약응답>(권한오류);
        }

        var driver = await _db.용달기사.FirstOrDefaultAsync(x => x.기사Id == request.기사Id, cancellationToken);
        if (driver is null)
        {
            return Result.Fail<기사예약응답>("용달기사 정보를 찾을 수 없습니다.");
        }

        var shift = new 기사근무
        {
            기사Id = request.기사Id,
            시작모드 = request.시작모드,
            시작시각 = request.시작시각,
            시작위치 = request.시작위치,
            복귀지 = request.복귀지,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.기사근무.Add(shift);
        await _db.SaveChangesAsync(cancellationToken);

        await _dispatchRecommendationService.SendToDriverAsync(request.기사Id);

        return Result.Ok(new 기사예약응답
        {
            Id = shift.Id,
            DriverId = shift.기사Id,
            StartMode = shift.시작모드,
            StartTime = shift.시작시각,
            StartLocation = shift.시작위치,
            ReturnDestination = shift.복귀지
        });
    }
}
