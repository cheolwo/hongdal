using Hongdal.Contracts.Driver.Transport;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송상차완료CommandHandler : IRequestHandler<운송상차완료Command, 기사운송상태변경응답>
{
    private readonly HongdalContext _db;
    private readonly ILogger<운송상차완료CommandHandler> _logger;

    public 운송상차완료CommandHandler(HongdalContext db, ILogger<운송상차완료CommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<기사운송상태변경응답> Handle(운송상차완료Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.배송_운송
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사_운송자 == request.기사Id, cancellationToken)
            ?? throw new InvalidOperationException("운송을 찾을 수 없습니다.");

        entity.상태 = "상차완료";
        entity.출발_픽업 ??= DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Action={Action} DriverId={DriverId} TransportId={TransportId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "TransportPickupCompleted",
            request.기사Id,
            entity.Id,
            "상차지도착",
            entity.상태,
            "Success",
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            DateTime.UtcNow);

        return new 기사운송상태변경응답
        {
            Id = entity.Id,
            운송번호 = entity.운송번호,
            상태 = entity.상태,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
