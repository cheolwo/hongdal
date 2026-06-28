using Hongdal.Contracts.Driver.Transport;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송하차지도착CommandHandler : IRequestHandler<운송하차지도착Command, 기사운송상태변경응답>
{
    private readonly HongdalContext _db;
    private readonly ILogger<운송하차지도착CommandHandler> _logger;

    public 운송하차지도착CommandHandler(HongdalContext db, ILogger<운송하차지도착CommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<기사운송상태변경응답> Handle(운송하차지도착Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.배송_운송
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사_운송자 == request.기사Id, cancellationToken)
            ?? throw new InvalidOperationException("운송을 찾을 수 없습니다.");

        entity.상태 = "하차지도착";
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Action={Action} DriverId={DriverId} TransportId={TransportId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "TransportArrivedDropoff",
            request.기사Id,
            entity.Id,
            "상차완료",
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
