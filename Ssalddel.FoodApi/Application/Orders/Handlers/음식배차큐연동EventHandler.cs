using Ssalddel.FoodApi.Application;
using Ssalddel.FoodApi.Application.Orders.Events;
using Ssalddel.FoodApi.Services;

namespace Ssalddel.FoodApi.Application.Orders.Handlers;

public sealed class 음식배차큐연동EventHandler : IFoodEventHandler<음식주문배차대기요청됨Event>
{
    private readonly 음식샘플Store _store;
    private readonly I음식배차큐연동Service _dispatchIntegrationService;

    public 음식배차큐연동EventHandler(음식샘플Store store, I음식배차큐연동Service dispatchIntegrationService)
    {
        _store = store;
        _dispatchIntegrationService = dispatchIntegrationService;
    }

    public async Task HandleAsync(음식주문배차대기요청됨Event appEvent, CancellationToken cancellationToken = default)
    {
        var restaurant = _store.음식점조회(appEvent.주문.음식점Id);
        var pickupAddress = restaurant?.주소 ?? $"음식점:{appEvent.주문.음식점Id}";

        await _dispatchIntegrationService.배차대기생성요청Async(
            appEvent.주문,
            restaurant?.위도,
            restaurant?.경도,
            pickupAddress,
            cancellationToken);
    }
}
