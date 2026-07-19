using Ssalddel.FoodApi.Application;
using Ssalddel.FoodApi.Application.Orders.Events;
using Ssalddel.FoodApi.Services;

namespace Ssalddel.FoodApi.Application.Orders.Handlers;

public sealed class 음식점신규주문알림EventHandler : IFoodEventHandler<음식주문등록됨Event>
{
    private readonly 음식샘플Store _store;
    private readonly ILogger<음식점신규주문알림EventHandler> _logger;

    public 음식점신규주문알림EventHandler(음식샘플Store store, ILogger<음식점신규주문알림EventHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task HandleAsync(음식주문등록됨Event appEvent, CancellationToken cancellationToken = default)
    {
        var restaurant = _store.음식점조회(appEvent.주문.음식점Id);
        _logger.LogInformation(
            "음식점 신규 주문 알림 생성. EventId={EventId}, 주문번호={OrderNo}, 음식점Id={RestaurantId}, 음식점명={RestaurantName}, 주문자={OrdererId}",
            appEvent.EventId,
            appEvent.주문.주문번호,
            appEvent.주문.음식점Id,
            restaurant?.상호명 ?? string.Empty,
            appEvent.주문.주문자UserId);

        return Task.CompletedTask;
    }
}
