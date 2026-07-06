using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Hongdal.Application.CommandProcessing;
using 홍달.도메인.공통;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차수락취소CommandHandler : IRequestHandler<배차수락취소Command, Result<배차수락취소결과>>
{
    private readonly HongdalContext _db;
    private readonly IPublisher _publisher;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;
    private readonly ILogger<배차수락취소CommandHandler> _logger;

    public 배차수락취소CommandHandler(
        HongdalContext db,
        IPublisher publisher,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사,
        ILogger<배차수락취소CommandHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _currentUserAccessor = currentUserAccessor;
        _권한검사 = 권한검사;
        _logger = logger;
    }

    public async Task<Result<배차수락취소결과>> Handle(배차수락취소Command request, CancellationToken cancellationToken)
    {
        if (!_권한검사.Try검증(_currentUserAccessor.UserId, _currentUserAccessor.Role, request.참여자Id, request.실행역할, out var 권한오류))
        {
            return Result.Fail<배차수락취소결과>(권한오류);
        }

        var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken);
        if (queue is null)
        {
            return Result.Fail<배차수락취소결과>("배차대기 데이터를 찾을 수 없습니다.");
        }

        var acceptedByDriver = string.Equals(queue.확정기사Id, request.기사Id, StringComparison.Ordinal)
                               || string.Equals(queue.현재추천대상기사Id, request.기사Id, StringComparison.Ordinal);
        var cancelableState = queue.상태 == 상태값.배차대기상태.확정 || queue.배차큐단계 == 상태값.배차큐단계.확정;
        if (!cancelableState || !acceptedByDriver)
        {
            return Result.Fail<배차수락취소결과>("수락 취소 가능한 배차가 아닙니다.");
        }

        var now = DateTime.UtcNow;
        try
        {
            await _publisher.Publish(
                new 배차수락취소됨Event(
                    request.기사Id,
                    request.RequestId,
                    request.사유,
                    now,
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락취소 사후처리 이벤트 발행 중 예외가 발생했습니다. RequestId={RequestId}", request.RequestId);
        }

        return Result.Ok(new 배차수락취소결과(request.RequestId, "수락 취소가 접수되었습니다."));
    }
}
