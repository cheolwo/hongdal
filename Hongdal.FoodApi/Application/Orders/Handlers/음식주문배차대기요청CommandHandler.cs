using Hongdal.Contracts.Food;
using Hongdal.FoodApi.Application;
using Hongdal.FoodApi.Application.Orders.Commands;
using Hongdal.FoodApi.Application.Orders.Events;
using Hongdal.FoodApi.Services;

namespace Hongdal.FoodApi.Application.Orders.Handlers;

public sealed class 음식주문배차대기요청CommandHandler : IFoodCommandHandler<음식주문배차대기요청Command, 음식주문응답?>
{
    private readonly 음식샘플Store _store;
    private readonly IFoodEventPublisher _eventPublisher;

    public 음식주문배차대기요청CommandHandler(음식샘플Store store, IFoodEventPublisher eventPublisher)
    {
        _store = store;
        _eventPublisher = eventPublisher;
    }

    public async Task<음식주문응답?> HandleAsync(음식주문배차대기요청Command command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.주문번호))
        {
            throw new ArgumentException("주문번호가 필요합니다.");
        }

        var order = _store.배차대기전환(command.주문번호);
        if (order is null)
        {
            return null;
        }

        await _eventPublisher.PublishAsync(
            new 음식주문배차대기요청됨Event(order, DateTime.UtcNow, Guid.NewGuid().ToString("N")),
            cancellationToken);

        return order;
    }
}
