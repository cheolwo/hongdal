using Ssalddel.Application.Food.Commands;
using Ssalddel.Application.Food.Events;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;
using MediatR;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 음식주문등록CommandHandler(
    ISsalddelFoodOrderStore orderStore,
    IPublisher publisher) : IRequestHandler<음식주문등록Command, 음식주문응답>
{
    public async Task<음식주문응답> Handle(음식주문등록Command request, CancellationToken cancellationToken)
    {
        Validate(request.Payload);

        var order = orderStore.AddOrder(request.Payload);

        await publisher.Publish(
            new 음식주문등록됨Event(order, DateTime.UtcNow, Guid.NewGuid().ToString("N")),
            cancellationToken);

        return orderStore.GetOrder(order.주문번호) ?? order;
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
