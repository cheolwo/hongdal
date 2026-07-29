using MediatR;
using Ssalddel.Application.Food.Events;
using Ssalddel.Services.Food;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 주문자음식주문수령확인SignalR알림EventHandler(
    I음식점주문실시간알림Service notificationService)
    : INotificationHandler<주문자음식주문수령확인됨Event>
{
    public Task Handle(
        주문자음식주문수령확인됨Event notification,
        CancellationToken cancellationToken)
        => notificationService.주문상태변경알림발송Async(
            notification.주문,
            "주문자가 음식 수령을 확인했습니다.",
            cancellationToken);
}
