using MediatR;
using Ssalddel.Application.Food.Commands;
using Ssalddel.Application.Food.Events;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 음식점주문진행변경CommandHandler(
    ISsalddelFoodOrderStore orderStore,
    IPublisher publisher) : IRequestHandler<음식점주문진행변경Command, 음식주문응답?>
{
    public async Task<음식주문응답?> Handle(
        음식점주문진행변경Command request,
        CancellationToken cancellationToken)
    {
        Validate(request);

        var changed = orderStore.음식점진행변경(
            request.주문번호,
            request.Payload,
            request.처리UserId.Trim());
        if (changed is null)
        {
            return null;
        }

        if (changed.새로변경됨)
        {
            var history = changed.주문.상태이력
                .OrderByDescending(item => item.전이시각Utc)
                .FirstOrDefault(item => item.클라이언트요청Id == request.Payload.클라이언트요청Id);
            await publisher.Publish(
                new 음식점주문진행변경됨Event(
                    changed.주문,
                    request.처리UserId.Trim(),
                    request.Payload.작업.Trim(),
                    history?.사유 ?? request.Payload.사유.Trim(),
                    DateTime.UtcNow,
                    Guid.NewGuid().ToString("N")),
                cancellationToken);
        }

        return orderStore.GetOrder(request.주문번호) ?? changed.주문;
    }

    private static void Validate(음식점주문진행변경Command command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.주문번호);
        ArgumentNullException.ThrowIfNull(command.Payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.처리UserId);

        if (command.Payload.클라이언트요청Id == Guid.Empty)
        {
            throw new ArgumentException("음식점 진행 변경의 클라이언트 요청 ID가 필요합니다.");
        }

        if (!음식점주문진행작업코드.지원여부(command.Payload.작업))
        {
            throw new ArgumentException("지원하지 않는 음식점 주문 진행 작업입니다.");
        }
    }
}
