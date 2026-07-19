using Ssalddel.Contracts.Food;
using Ssalddel.FoodApi.Application;
using Ssalddel.FoodApi.Application.Orders.Commands;
using Ssalddel.FoodApi.Application.Orders.Events;
using Ssalddel.FoodApi.Services;

namespace Ssalddel.FoodApi.Application.Orders.Handlers;

public sealed class 음식주문등록CommandHandler : IFoodCommandHandler<음식주문등록Command, 음식주문응답>
{
    private readonly 음식샘플Store _store;
    private readonly IFoodEventPublisher _eventPublisher;

    public 음식주문등록CommandHandler(음식샘플Store store, IFoodEventPublisher eventPublisher)
    {
        _store = store;
        _eventPublisher = eventPublisher;
    }

    public async Task<음식주문응답> HandleAsync(음식주문등록Command command, CancellationToken cancellationToken = default)
    {
        Validate(command.Payload);

        var order = _store.AddOrder(command.Payload);
        await _eventPublisher.PublishAsync(
            new 음식주문등록됨Event(order, DateTime.UtcNow, Guid.NewGuid().ToString("N")),
            cancellationToken);

        return order;
    }

    private static void Validate(음식주문등록요청 request)
    {
        if (request.음식점Id <= 0)
        {
            throw new ArgumentException("음식점Id가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.주문자UserId))
        {
            throw new ArgumentException("주문자UserId가 필요합니다.");
        }

        if (request.상품목록.Count == 0)
        {
            throw new ArgumentException("상품목록이 필요합니다.");
        }

        if (request.상품목록.Any(x => string.IsNullOrWhiteSpace(x.상품명) || x.수량 <= 0 || x.단가 < 0))
        {
            throw new ArgumentException("상품명, 수량, 단가를 확인해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(request.수령인정보.주소))
        {
            throw new ArgumentException("수령지 주소가 필요합니다.");
        }
    }
}
