using MediatR;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차수락취소사후처리EventHandler : INotificationHandler<배차수락취소됨Event>
{
    private readonly 홍달.Services.Dispatch.Queue.I배차큐전환Service _queueTransitionService;
    private readonly ILogger<배차수락취소사후처리EventHandler> _logger;

    public 배차수락취소사후처리EventHandler(
        홍달.Services.Dispatch.Queue.I배차큐전환Service queueTransitionService,
        ILogger<배차수락취소사후처리EventHandler> logger)
    {
        _queueTransitionService = queueTransitionService;
        _logger = logger;
    }

    public async Task Handle(배차수락취소됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            await _queueTransitionService.배차수락취소처리Async(
                notification.의뢰Id,
                notification.기사Id,
                notification.사유,
                cancellationToken);
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
