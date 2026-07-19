using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ssalddel.Application.CommandProcessing;
using 살뜰.도메인.공통;
using 살뜰.Services.Dispatch.Queue;

namespace Ssalddel.Application.Driver.DispatchAction;

public sealed class 배차수락취소CommandHandler : IRequestHandler<배차수락취소Command, Result<배차수락취소결과>>
{
    private readonly SsalddelContext _db;
    private readonly I배차대기원장전환Service _원장전환Service;
    private readonly IPublisher _publisher;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;
    private readonly ILogger<배차수락취소CommandHandler> _logger;

    public 배차수락취소CommandHandler(
        SsalddelContext db,
        I배차대기원장전환Service 원장전환Service,
        IPublisher publisher,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사,
        ILogger<배차수락취소CommandHandler> logger)
    {
        _db = db;
        _원장전환Service = 원장전환Service;
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

        var queue = await _db.운송원장.FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken);
        if (queue is null)
        {
            return Result.Fail<배차수락취소결과>("배차대기 데이터를 찾을 수 없습니다.");
        }

        if (!배차응답가능정책.수락취소가능(queue, request.기사Id))
        {
            return Result.Fail<배차수락취소결과>("수락 취소 가능한 배차가 아닙니다.");
        }

        var now = DateTime.UtcNow;
        try
        {
            var 전환결과 = await _원장전환Service.배차수락취소처리Async(
                request.RequestId,
                request.기사Id,
                request.사유,
                cancellationToken);
            if (!전환결과.전환여부)
            {
                _logger.LogWarning(
                    "배차수락취소 원장 전환이 적용되지 않았습니다. RequestId={RequestId} DriverId={DriverId} ResultCode={ResultCode} Message={Message}",
                    request.RequestId,
                    request.기사Id,
                    전환결과.결과코드,
                    전환결과.메시지);
                return Result.Fail<배차수락취소결과>("수락 취소 가능한 배차가 아닙니다.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락취소 필수 처리 중 예외가 발생했습니다. RequestId={RequestId} DriverId={DriverId}", request.RequestId, request.기사Id);
            return Result.Fail<배차수락취소결과>("수락 취소 처리 중 오류가 발생했습니다. 잠시 후 다시 시도해 주세요.");
        }

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
