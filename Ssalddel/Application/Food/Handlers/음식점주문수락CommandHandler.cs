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

        var accepted = orderStore.음식점수락(request.주문번호, request.Payload);
        if (accepted is null)
        {
            return null;
        }

        await publisher.Publish(
            new 음식점주문수락됨Event(
                accepted,
                NormalizeUserId(request.처리UserId) ?? NormalizeUserId(request.Payload.처리UserId),
                DateTime.UtcNow,
                Guid.NewGuid().ToString("N")),
            cancellationToken);

        return orderStore.GetOrder(request.주문번호) ?? accepted;
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
    }

    private static string? NormalizeUserId(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
