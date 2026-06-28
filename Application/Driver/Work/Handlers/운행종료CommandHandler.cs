namespace Hongdal.Application.Driver.Work;

using Microsoft.Extensions.Logging;

public sealed class 운행종료CommandHandler : IRequestHandler<운행종료Command, Unit>
{
    private readonly HongdalContext _db;
    private readonly IDriverWorkQueueStore _driverWorkQueueStore;
    private readonly ILogger<운행종료CommandHandler> _logger;

    public 운행종료CommandHandler(HongdalContext db, IDriverWorkQueueStore driverWorkQueueStore, ILogger<운행종료CommandHandler> logger)
    {
        _db = db;
        _driverWorkQueueStore = driverWorkQueueStore;
        _logger = logger;
    }

    public async Task<Unit> Handle(운행종료Command request, CancellationToken cancellationToken)
    {
        var driver = await _db.용달기사.FirstOrDefaultAsync(x => x.기사Id == request.기사Id, cancellationToken)
            ?? throw new InvalidOperationException("용달기사 정보를 찾을 수 없습니다.");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        driver.운행상태 = 상태값.기사운행상태.대기;
        driver.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _driverWorkQueueStore.RemoveAsync(request.기사Id, cancellationToken);
        await tx.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Action={Action} DriverId={DriverId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DriverWorkStopped",
            request.기사Id,
            상태값.기사운행상태.운행중,
            driver.운행상태,
            "Success",
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            DateTime.UtcNow);

        return Unit.Value;
    }
}
