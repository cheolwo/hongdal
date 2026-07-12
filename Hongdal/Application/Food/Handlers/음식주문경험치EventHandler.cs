using Hongdal.Application.Community;
using Hongdal.Application.Food.Events;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Services.Community;
using 홍달.Data;

namespace Hongdal.Application.Food.Handlers;

public sealed class 음식주문경험치EventHandler : INotificationHandler<음식점주문수락됨Event>
{
    private readonly ICommunityExperienceEventRecorder _experienceEventRecorder;

    public 음식주문경험치EventHandler(ICommunityExperienceEventRecorder experienceEventRecorder)
    {
        _experienceEventRecorder = experienceEventRecorder;
    }

    public Task Handle(음식점주문수락됨Event notification, CancellationToken cancellationToken)
    {
        var order = notification.주문;
        return RecordAsync(
            notification.처리UserId ?? string.Empty,
            역할명.음식점,
            CommunityLedgerExperienceEventCodes.FoodOrderAccepted,
            "FoodOrder",
            order.주문번호,
            order.주문번호,
            $"api/v1/food-orders/{order.주문번호}/restaurant-acceptance",
            notification.EventId,
            notification.발생시각Utc,
            cancellationToken);
    }

    private Task RecordAsync(
        string userId,
        string roleName,
        string eventCode,
        string sourceKind,
        string sourceId,
        string sourceDisplayId,
        string route,
        string traceId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken)
        => _experienceEventRecorder.RecordAsync(
            new CommunityExperienceAwardRequest(
                userId,
                roleName,
                eventCode,
                sourceKind,
                sourceId,
                sourceDisplayId,
                route,
                traceId,
                occurredAtUtc,
                App식별자.RestaurantDeskApp),
            "음식 주문",
            cancellationToken);
}
