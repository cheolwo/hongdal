using MediatR;
using Ssalddel.Application.Food.Events;
using Ssalddel.Services.Food;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 음식점주문진행SignalR알림EventHandler(
    I음식점주문실시간알림Service notificationService)
    : INotificationHandler<음식점주문진행변경됨Event>
{
    public Task Handle(
        음식점주문진행변경됨Event notification,
        CancellationToken cancellationToken)
        => notificationService.주문상태변경알림발송Async(
            notification.주문,
            notification.변경사유,
            cancellationToken);
}
