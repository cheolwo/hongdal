using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차수락CommandHandler : IRequestHandler<배차수락Command, Result<배차수락결과>>
{
    private readonly HongdalContext _db;
    private readonly IDispatchAcceptanceLogStore _acceptanceLogStore;
    private readonly ILogger<배차수락CommandHandler> _logger;

    public 배차수락CommandHandler(HongdalContext db, IDispatchAcceptanceLogStore acceptanceLogStore, ILogger<배차수락CommandHandler> logger)
    {
        _db = db;
        _acceptanceLogStore = acceptanceLogStore;
        _logger = logger;
    }

    public async Task<Result<배차수락결과>> Handle(배차수락Command request, CancellationToken cancellationToken)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken);
        if (queue is null)
        {
            return Result.Fail<배차수락결과>("배차대기 데이터를 찾을 수 없습니다.");
        }

        var dispatchRequest = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken);
        if (dispatchRequest is null)
        {
            return Result.Fail<배차수락결과>("운송의뢰 데이터를 찾을 수 없습니다.");
        }

        if (dispatchRequest.결제상태 != 상태값.결제상태.결제완료)
        {
            return Result.Fail<배차수락결과>("결제완료 의뢰만 수락할 수 있습니다.");
        }

        if (queue.상태 != 상태값.배차대기상태.대기)
        {
            return Result.Fail<배차수락결과>("이미 수락된 배차입니다.");
        }

        var now = DateTime.UtcNow;
        queue.상태 = 상태값.배차대기상태.확정;
        dispatchRequest.배차상태 = 상태값.배차상태.매칭중;
        dispatchRequest.UpdatedAt = now;
        queue.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        await _acceptanceLogStore.AppendAsync(new DispatchAcceptanceLogEntry(
            request.기사Id,
            dispatchRequest.화주Id,
            request.RequestId,
            now,
            queue.상태,
            dispatchRequest.배차상태,
            dispatchRequest.결제상태), cancellationToken);

        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} RequestId={RequestId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DispatchAccepted",
            request.기사Id,
            request.RequestId,
            상태값.배차대기상태.대기,
            queue.상태,
            "Success",
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            now);

        return Result.Ok(new 배차수락결과(request.RequestId, "수락되었습니다."));
    }
}
