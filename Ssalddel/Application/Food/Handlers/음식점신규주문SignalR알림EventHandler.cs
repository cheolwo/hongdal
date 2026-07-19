using Ssalddel.Application.Food.Events;
using Ssalddel.Services.Food;
using MediatR;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 음식점신규주문SignalR알림EventHandler(
    I음식점주문실시간알림Service notificationService,
    ILogger<음식점신규주문SignalR알림EventHandler> logger) : INotificationHandler<음식주문등록됨Event>
{
    public async Task Handle(음식주문등록됨Event notification, CancellationToken cancellationToken)
    {
        await notificationService.신규주문알림발송Async(notification.주문, cancellationToken);
        logger.LogInformation(
            "음식 주문 등록 이벤트 후속처리 완료. EventId={EventId}, 주문번호={OrderNo}, 음식점Id={RestaurantId}",
            notification.EventId,
            notification.주문.주문번호,
            notification.주문.음식점Id);
    }
}
