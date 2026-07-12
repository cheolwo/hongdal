using Hongdal.Application.Community;
using Hongdal.Application.Food.Events;
using Hongdal.Application.Food.Handlers;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Contracts.Food;
using Hongdal.Services.Community;
using Microsoft.Extensions.Logging.Abstractions;
using 홍달.Data;

namespace Hongdal.Tests.Application.Food;

public sealed class 음식주문경험치EventHandlerTests
{
    [Fact]
    public async Task Handle_음식점주문수락Event_음식주문수락경험치코드를기록한다()
    {
        var service = new FakeCommunityExperienceAwardService();
        var handler = new 음식주문경험치EventHandler(CreateRecorder(service));
        var occurredAt = new DateTime(2026, 7, 12, 4, 0, 0, DateTimeKind.Utc);

        await handler.Handle(
            new 음식점주문수락됨Event(
                new 음식주문응답
                {
                    주문번호 = "FOOD-1",
                    음식점Id = 77
                },
                "restaurant-user-1",
                occurredAt,
                "food-event-1"),
            CancellationToken.None);

        var request = Assert.Single(service.Requests);
        Assert.Equal("restaurant-user-1", request.UserId);
        Assert.Equal(역할명.음식점, request.RoleName);
        Assert.Equal(App식별자.RestaurantDeskApp, request.AppKey);
        Assert.Equal(CommunityLedgerExperienceEventCodes.FoodOrderAccepted, request.EventCode);
        Assert.Equal("FoodOrder", request.SourceKind);
        Assert.Equal("FOOD-1", request.SourceId);
        Assert.Equal("FOOD-1", request.SourceDisplayId);
        Assert.Equal("api/v1/food-orders/FOOD-1/restaurant-acceptance", request.Route);
        Assert.Equal("food-event-1", request.TraceId);
        Assert.Equal(occurredAt, request.OccurredAtUtc);
    }

    private sealed class FakeCommunityExperienceAwardService : ICommunityExperienceAwardService
    {
        public List<CommunityExperienceAwardRequest> Requests { get; } = [];

        public Task<CommunityExperienceAwardResult> RecordAsync(
            CommunityExperienceAwardRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(new CommunityExperienceAwardResult(
                true,
                request.EventCode,
                1,
                "ok"));
        }
    }

    private static CommunityExperienceEventRecorder CreateRecorder(
        ICommunityExperienceAwardService service)
        => new(service, NullLogger<CommunityExperienceEventRecorder>.Instance);
}
