using MediatR;
using Microsoft.Extensions.Logging;

namespace Ssalddel.Application.Driver.DispatchAction;

public sealed class 배차수락취소사후처리EventHandler : INotificationHandler<배차수락취소됨Event>
{
    private readonly 살뜰.Services.Dispatch.Queue.I배차대기원장전환Service _원장전환Service;
    private readonly ILogger<배차수락취소사후처리EventHandler> _logger;

    public 배차수락취소사후처리EventHandler(
        살뜰.Services.Dispatch.Queue.I배차대기원장전환Service 원장전환Service,
        ILogger<배차수락취소사후처리EventHandler> logger)
    {
        _원장전환Service = 원장전환Service;
        _logger = logger;
    }

    public async Task Handle(배차수락취소됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            var 전환결과 = await _원장전환Service.배차수락취소처리Async(
                notification.의뢰Id,
                notification.기사Id,
                notification.사유,
                cancellationToken);
            if (!전환결과.전환여부)
            {
                _logger.LogDebug(
                    "배차수락취소 사후처리 원장 전환이 생략되었습니다. RequestId={RequestId} DriverId={DriverId} ResultCode={ResultCode} Message={Message}",
                    notification.의뢰Id,
                    notification.기사Id,
                    전환결과.결과코드,
                    전환결과.메시지);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락취소 사후처리 중 큐전환 예외가 발생했습니다. RequestId={RequestId}", notification.의뢰Id);
        }

        _logger.LogInformation(
            "Action={Action} DriverId={DriverId} RequestId={RequestId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DispatchAcceptanceCanceled",
            notification.기사Id,
            notification.의뢰Id,
            "RedispatchRequired",
            string.IsNullOrWhiteSpace(notification.사유) ? "No reason provided" : notification.사유,
            notification.TraceId,
            notification.발생시각Utc);
    }
}
