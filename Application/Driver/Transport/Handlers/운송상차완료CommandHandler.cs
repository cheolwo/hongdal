using Hongdal.Contracts.Driver.Transport;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송상차완료CommandHandler : IRequestHandler<운송상차완료Command, Result<기사운송상태변경응답>>
{
    private readonly HongdalContext _db;
    private readonly I기사운송상태전이Service _상태전이Service;
    private readonly ILogger<운송상차완료CommandHandler> _logger;

    public 운송상차완료CommandHandler(HongdalContext db, I기사운송상태전이Service 상태전이Service, ILogger<운송상차완료CommandHandler> logger)
    {
        _db = db;
        _상태전이Service = 상태전이Service;
        _logger = logger;
    }

    public async Task<Result<기사운송상태변경응답>> Handle(운송상차완료Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.배송_운송
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사_운송자 == request.기사Id, cancellationToken);
        if (entity is null)
        {
            return Result.Fail<기사운송상태변경응답>("운송을 찾을 수 없습니다.");
        }

        var 이전상태 = entity.상태;
        var now = DateTime.UtcNow;
        var 상태변경 = _상태전이Service.상태변경(entity, "상차완료", now);
        if (상태변경.IsFailed)
        {
            return Result.Fail<기사운송상태변경응답>(상태변경.Errors.Select(x => x.Message));
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} TransportId={TransportId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "TransportPickupCompleted",
            request.기사Id,
            entity.Id,
            이전상태,
            entity.상태,
            "Success",
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            now);

        return Result.Ok(new 기사운송상태변경응답
        {
            Id = entity.Id,
            운송번호 = entity.운송번호,
            상태 = entity.상태,
            UpdatedAt = entity.UpdatedAt
        });
    }
}
