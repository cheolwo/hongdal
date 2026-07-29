using Ssalddel.Application.Food.Commands;
using Ssalddel.Application.Food.Events;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;
using MediatR;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 음식점주문수락CommandHandler(
    ISsalddelFoodOrderStore orderStore,
    IPublisher publisher) : IRequestHandler<음식점주문수락Command, 음식주문응답?>
{
    public async Task<음식주문응답?> Handle(음식점주문수락Command request, CancellationToken cancellationToken)
    {
        Validate(request);

        var actorUserId = NormalizeUserId(request.처리UserId)
            ?? throw new ArgumentException("인증된 음식점 처리 사용자 ID가 필요합니다.");
        var accepted = orderStore.음식점수락멱등(
            request.주문번호,
            request.Payload,
            actorUserId);
        if (accepted is null)
        {
            return null;
        }

        if (accepted.새로변경됨)
        {
            await publisher.Publish(
                new 음식점주문수락됨Event(
                    accepted.주문,
                    actorUserId,
                    DateTime.UtcNow,
                    Guid.NewGuid().ToString("N")),
                cancellationToken);
        }

        return orderStore.GetOrder(request.주문번호) ?? accepted.주문;
    }

    private static void Validate(음식점주문수락Command command)
    {
        if (string.IsNullOrWhiteSpace(command.주문번호))
        {
            throw new ArgumentException("주문번호가 필요합니다.");
        }

        if (command.Payload is null)
        {
            throw new ArgumentException("음식점 수락 요청 본문이 필요합니다.");
        }

        if (command.Payload.클라이언트요청Id == Guid.Empty)
        {
            throw new ArgumentException("음식점 주문 수락의 클라이언트 요청 ID가 필요합니다.");
        }
    }

    private static string? NormalizeUserId(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
