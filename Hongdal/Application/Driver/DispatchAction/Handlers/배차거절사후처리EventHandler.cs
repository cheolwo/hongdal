using MediatR;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차거절사후처리EventHandler : INotificationHandler<배차거절됨Event>
{
    private readonly 홍달.Services.Dispatch.Queue.I배차큐전환Service _queueTransitionService;
    private readonly ILogger<배차거절사후처리EventHandler> _logger;

    public 배차거절사후처리EventHandler(
        홍달.Services.Dispatch.Queue.I배차큐전환Service queueTransitionService,
        ILogger<배차거절사후처리EventHandler> logger)
    {
        _queueTransitionService = queueTransitionService;
        _logger = logger;
    }

    public async Task Handle(배차거절됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            await _queueTransitionService.추천거절처리Async(notification.의뢰Id, notification.기사Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차거절 사후처리 중 큐전환 예외가 발생했습니다. RequestId={RequestId}", notification.의뢰Id);
        }

        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} RequestId={RequestId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DispatchRejected",
            notification.기사Id,
            notification.의뢰Id,
            "Success",
            "Driver rejected recommendation",
            notification.TraceId,
            notification.발생시각Utc);
    }
}
