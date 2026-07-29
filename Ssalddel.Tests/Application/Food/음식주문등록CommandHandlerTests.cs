using MediatR;
using Ssalddel.Application.Food.Commands;
using Ssalddel.Application.Food.Events;
using Ssalddel.Application.Food.Handlers;
using Ssalddel.Contracts.Common.Participants;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;

namespace Ssalddel.Tests.Application.Food;

public sealed class 음식주문등록CommandHandlerTests
{
    [Fact]
    public async Task 같은클라이언트요청재시도는_주문등록Event를다시발행하지않는다()
    {
        var store = new InMemorySsalddelFoodOrderStore();
        var publisher = new RecordingPublisher();
        var handler = new 음식주문등록CommandHandler(
            store,
            new PassThroughMenuValidationService(),
            publisher);
        var request = CreateRequest();

        var first = await handler.Handle(
            new 음식주문등록Command(request),
            CancellationToken.None);
        var retried = await handler.Handle(
            new 음식주문등록Command(request),
            CancellationToken.None);

        Assert.Equal(first.주문번호, retried.주문번호);
        Assert.Single(publisher.Notifications);
        Assert.IsType<음식주문등록됨Event>(publisher.Notifications[0]);
    }

    private static 음식주문등록요청 CreateRequest()
        => new()
        {
            클라이언트요청Id = Guid.NewGuid(),
            음식점Id = 101,
            주문자UserId = "orderer-1",
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = "주문자",
                연락처 = "010-1234-5678",
                주소 = "서울특별시 중구 세종대로 1"
            },
            상품목록 =
            [
                new 음식주문상품Dto
                {
                    메뉴Id = 1001,
                    상품명 = "살뜰김밥",
                    수량 = 2,
                    단가 = 4_500
                }
            ]
        };

    private sealed class PassThroughMenuValidationService : I음식주문메뉴검증Service
    {
        public Task<음식주문등록요청> 서버기준요청생성Async(
            음식주문등록요청 request,
            CancellationToken cancellationToken)
            => Task.FromResult(request);
    }

    private sealed class RecordingPublisher : IPublisher
    {
        public List<object> Notifications { get; } = [];

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }
    }
}
