using MediatR;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차수락사후처리EventHandler : INotificationHandler<배차수락됨Event>
{
    private readonly IDispatchAcceptanceLogStore _acceptanceLogStore;
    private readonly 홍달.Services.Dispatch.Queue.I배차큐전환Service _queueTransitionService;
    private readonly ILogger<배차수락사후처리EventHandler> _logger;

    public 배차수락사후처리EventHandler(
        IDispatchAcceptanceLogStore acceptanceLogStore,
        홍달.Services.Dispatch.Queue.I배차큐전환Service queueTransitionService,
        ILogger<배차수락사후처리EventHandler> logger)
    {
        _acceptanceLogStore = acceptanceLogStore;
        _queueTransitionService = queueTransitionService;
        _logger = logger;
    }

    public async Task Handle(배차수락됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            await _acceptanceLogStore.AppendAsync(new DispatchAcceptanceLogEntry(
                notification.기사Id,
                notification.화주Id,
                notification.의뢰Id,
                notification.발생시각Utc,
                notification.배차대기상태,
                notification.의뢰배차상태,
                notification.의뢰결제상태), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락 사후처리 중 수락로그 적재 예외가 발생했습니다. RequestId={RequestId}", notification.의뢰Id);
        }

        try
        {
            await _queueTransitionService.배차확정처리Async(notification.의뢰Id, notification.기사Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락 사후처리 중 큐전환 예외가 발생했습니다. RequestId={RequestId}", notification.의뢰Id);
        }

        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} RequestId={RequestId} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DispatchAccepted",
            notification.기사Id,
            notification.의뢰Id,
            notification.배차대기상태,
            "Success",
            notification.TraceId,
            notification.발생시각Utc);
    }
}
