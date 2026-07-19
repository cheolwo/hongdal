using MediatR;
using Microsoft.Extensions.Logging;

namespace Ssalddel.Application.Driver.Transport;

public sealed class 운송상차지도착로그EventHandler : INotificationHandler<운송상차지도착됨Event>
{
    private readonly ILogger<운송상차지도착로그EventHandler> _logger;

    public 운송상차지도착로그EventHandler(ILogger<운송상차지도착로그EventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(운송상차지도착됨Event notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} TransportId={TransportId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "TransportArrivedPickup",
            notification.기사Id,
            notification.운송Id,
            notification.이전상태,
            notification.현재상태,
            "Success",
            notification.TraceId,
            notification.발생시각Utc);

        return Task.CompletedTask;
    }
}

public sealed class 운송상차완료로그EventHandler : INotificationHandler<운송상차완료됨Event>
{
    private readonly ILogger<운송상차완료로그EventHandler> _logger;

    public 운송상차완료로그EventHandler(ILogger<운송상차완료로그EventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(운송상차완료됨Event notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} TransportId={TransportId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "TransportPickupCompleted",
            notification.기사Id,
            notification.운송Id,
            notification.이전상태,
            notification.현재상태,
            "Success",
            notification.TraceId,
            notification.발생시각Utc);

        return Task.CompletedTask;
    }
}

public sealed class 운송하차지도착로그EventHandler : INotificationHandler<운송하차지도착됨Event>
{
    private readonly ILogger<운송하차지도착로그EventHandler> _logger;

    public 운송하차지도착로그EventHandler(ILogger<운송하차지도착로그EventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(운송하차지도착됨Event notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} TransportId={TransportId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "TransportArrivedDropoff",
            notification.기사Id,
            notification.운송Id,
            notification.이전상태,
            notification.현재상태,
            "Success",
            notification.TraceId,
            notification.발생시각Utc);

        return Task.CompletedTask;
    }
}
