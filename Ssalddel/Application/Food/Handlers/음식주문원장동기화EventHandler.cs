using Ssalddel.Application.Food.Events;
using Ssalddel.Services.Community;
using MediatR;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 음식주문원장동기화EventHandler :
    INotificationHandler<음식주문등록됨Event>,
    INotificationHandler<음식점주문수락됨Event>
{
    private readonly I음식마트원장동기화OutboxService _원장동기화OutboxService;
    private readonly ILogger<음식주문원장동기화EventHandler> _logger;

    public 음식주문원장동기화EventHandler(
        I음식마트원장동기화OutboxService 원장동기화OutboxService,
        ILogger<음식주문원장동기화EventHandler> logger)
    {
        _원장동기화OutboxService = 원장동기화OutboxService;
        _logger = logger;
    }

    public Task Handle(음식주문등록됨Event notification, CancellationToken cancellationToken)
        => 원장동기화Async(
            notification.주문,
            notification.주문.주문자UserId,
            notification.EventId,
            cancellationToken);

    public Task Handle(음식점주문수락됨Event notification, CancellationToken cancellationToken)
        => 원장동기화Async(
            notification.주문,
            notification.처리UserId ?? $"restaurant:{notification.주문.음식점Id}",
            notification.EventId,
            cancellationToken);

    private async Task 원장동기화Async(
        Ssalddel.Contracts.Food.음식주문응답 주문,
        string 변경자,
        string eventId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _원장동기화OutboxService.음식주문예약후즉시처리Async(
                주문,
                변경자,
                $"food-event:{eventId}",
                cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "음식 주문 변경 후 원장 동기화에 실패했습니다. 주문번호={주문번호}, EventId={EventId}",
                주문.주문번호,
                eventId);
        }
    }
}
