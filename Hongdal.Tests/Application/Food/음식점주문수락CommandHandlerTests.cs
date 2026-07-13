using Hongdal.Application.Food.Commands;
using Hongdal.Application.Food.Events;
using Hongdal.Application.Food.Handlers;
using Hongdal.Contracts.Common.Participants;
using Hongdal.Contracts.Food;
using Hongdal.Services.Food;
using MediatR;

namespace Hongdal.Tests.Application.Food;

public sealed class 음식점주문수락CommandHandlerTests
{
    [Fact]
    public async Task Handle_처리UserId를_수락이벤트에전달한다()
    {
        var store = new InMemoryHongdalFoodOrderStore();
        var publisher = new RecordingPublisher();
        var handler = new 음식점주문수락CommandHandler(store, publisher);
        var order = store.AddOrder(CreateOrderRequest());

        var result = await handler.Handle(
            new 음식점주문수락Command(
                order.주문번호,
                new 음식점주문수락요청
                {
                    처리UserId = "body-restaurant-user",
                    음식점명 = "홍달분식",
                    음식점주소 = "서울특별시 마포구 월드컵북로 1",
                    즉시픽업가능여부 = true
                },
                " authenticated-restaurant-user "),
            CancellationToken.None);

        Assert.NotNull(result);
        var notification = Assert.IsType<음식점주문수락됨Event>(Assert.Single(publisher.Notifications));
        Assert.Equal("authenticated-restaurant-user", notification.처리UserId);
        Assert.Equal(order.주문번호, notification.주문.주문번호);
    }

    [Fact]
    public async Task Handle_명령UserId가없으면_요청본문의처리UserId를사용한다()
    {
        var store = new InMemoryHongdalFoodOrderStore();
        var publisher = new RecordingPublisher();
        var handler = new 음식점주문수락CommandHandler(store, publisher);
        var order = store.AddOrder(CreateOrderRequest());

        await handler.Handle(
            new 음식점주문수락Command(
                order.주문번호,
                new 음식점주문수락요청
                {
                    처리UserId = "body-restaurant-user",
                    음식점명 = "홍달분식",
                    음식점주소 = "서울특별시 마포구 월드컵북로 1"
                },
                null),
            CancellationToken.None);

        var notification = Assert.IsType<음식점주문수락됨Event>(Assert.Single(publisher.Notifications));
        Assert.Equal("body-restaurant-user", notification.처리UserId);
    }

    private static 음식주문등록요청 CreateOrderRequest()
        => new()
        {
            음식점Id = 42,
            주문자UserId = "orderer-1",
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = "홍길동",
                연락처 = "010-0000-0000",
                주소 = "서울특별시 마포구 양화로 10",
                상세주소 = "101호",
                주문자본인수령여부 = true
            },
            상품목록 =
            [
                new 음식주문상품Dto
                {
                    상품명 = "김밥",
                    수량 = 2,
                    단가 = 4500
                }
            ],
            결제수단 = "카드"
        };

    private sealed class RecordingPublisher : IPublisher
    {
        public List<object> Notifications { get; } = [];

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }
    }

}
