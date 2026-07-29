using MediatR;
using Ssalddel.Application.Food.Commands;
using Ssalddel.Application.Food.Events;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 주문자음식주문수령확인CommandHandler(
    ISsalddelFoodOrderStore orderStore,
    IPublisher publisher)
    : IRequestHandler<주문자음식주문수령확인Command, 음식주문응답?>
{
    public async Task<음식주문응답?> Handle(
        주문자음식주문수령확인Command request,
        CancellationToken cancellationToken)
    {
        Validate(request);

        var changed = orderStore.주문자수령확인(
            request.주문번호,
            request.Payload,
            request.주문자UserId.Trim());
        if (changed is null)
        {
            return null;
        }

        if (changed.새로변경됨)
        {
            await publisher.Publish(
                new 주문자음식주문수령확인됨Event(
                    changed.주문,
                    request.주문자UserId.Trim(),
                    request.Payload.확인메모?.Trim() ?? string.Empty,
                    DateTime.UtcNow,
                    Guid.NewGuid().ToString("N")),
                cancellationToken);
        }

        return orderStore.GetOrder(request.주문번호) ?? changed.주문;
    }

    private static void Validate(주문자음식주문수령확인Command command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.주문번호);
        ArgumentNullException.ThrowIfNull(command.Payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.주문자UserId);

        if (command.Payload.클라이언트요청Id == Guid.Empty)
        {
            throw new ArgumentException("주문자 수령 확인의 클라이언트 요청 ID가 필요합니다.");
        }

        if (command.Payload.확인메모?.Length > 500)
        {
            throw new ArgumentException("수령 확인 메모는 500자 이내로 입력해 주세요.");
        }
    }
}
