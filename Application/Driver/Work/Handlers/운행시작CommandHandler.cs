using Hongdal.Contracts.Driver.Work;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.Work;

public sealed class 운행시작CommandHandler : IRequestHandler<운행시작Command, Result<기사운행시작응답>>
{
    private readonly HongdalContext _db;
    private readonly I배차추천Service _dispatchRecommendationService;
    private readonly IDriverWorkQueueStore _driverWorkQueueStore;
    private readonly ILogger<운행시작CommandHandler> _logger;

    public 운행시작CommandHandler(
        HongdalContext db,
        I배차추천Service dispatchRecommendationService,
        IDriverWorkQueueStore driverWorkQueueStore,
        ILogger<운행시작CommandHandler> logger)
    {
        _db = db;
        _dispatchRecommendationService = dispatchRecommendationService;
        _driverWorkQueueStore = driverWorkQueueStore;
        _logger = logger;
    }

    public async Task<Result<기사운행시작응답>> Handle(운행시작Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.시작모드))
        {
            return Result.Fail<기사운행시작응답>("시작모드가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.시작위치))
        {
            return Result.Fail<기사운행시작응답>("시작위치가 필요합니다.");
        }

        var driver = await _db.용달기사.FirstOrDefaultAsync(x => x.기사Id == request.기사Id, cancellationToken);
        if (driver is null)
        {
            return Result.Fail<기사운행시작응답>("용달기사 정보를 찾을 수 없습니다.");
        }

        var shift = new 기사근무
        {
            기사Id = request.기사Id,
            시작모드 = request.시작모드,
            시작시각 = request.시작시각 ?? DateTime.UtcNow,
            시작위치 = request.시작위치,
            복귀지 = request.복귀지,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        driver.운행상태 = 상태값.기사운행상태.운행중;
        driver.UpdatedAt = DateTime.UtcNow;
        _db.기사근무.Add(shift);
        await _db.SaveChangesAsync(cancellationToken);
        await _driverWorkQueueStore.UpsertAsync(new DriverWorkQueueEntry(
            request.기사Id,
            shift.Id,
            shift.CreatedAt,
            shift.시작모드,
            shift.시작위치,
            shift.복귀지), cancellationToken);
        await tx.CommitAsync(cancellationToken);

        await _dispatchRecommendationService.SendToDriverAsync(request.기사Id);

        _logger.LogInformation(
            "Action={Action} DriverId={DriverId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DriverWorkStarted",
            request.기사Id,
            상태값.기사운행상태.대기,
            driver.운행상태,
            "Success",
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            DateTime.UtcNow);

        return Result.Ok(new 기사운행시작응답
        {
            DriverId = request.기사Id,
            Status = driver.운행상태,
            ShiftId = shift.Id,
            StartedAt = shift.시작시각
        });
    }
}
