using MediatR;
using Ssalddel.Application.Food.Commands;
using Ssalddel.Application.Food.Events;
using Ssalddel.Application.Food.Handlers;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;

namespace Ssalddel.Tests.Application.Food;

public sealed class 주문자음식주문수령확인CommandHandlerTests
{
    [Fact]
    public async Task 수령확인은_주문자와메모를이벤트에남기고_같은요청은재발행하지않는다()
    {
        var store = new ReceiptStore();
        var publisher = new RecordingPublisher();
        var handler = new 주문자음식주문수령확인CommandHandler(store, publisher);
        var command = new 주문자음식주문수령확인Command(
            "FOOD-RECEIPT",
            new 주문자음식주문수령확인요청
            {
                클라이언트요청Id = Guid.NewGuid(),
                확인메모 = "문 앞에서 정상 수령"
            },
            " orderer-1 ");

        var first = await handler.Handle(command, CancellationToken.None);
        var retried = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(음식주문상태코드.수령확인, first?.상태);
        Assert.Equal(음식주문상태코드.수령확인, retried?.상태);
        var notification = Assert.IsType<주문자음식주문수령확인됨Event>(
            Assert.Single(publisher.Notifications));
        Assert.Equal("orderer-1", notification.주문자UserId);
        Assert.Equal("문 앞에서 정상 수령", notification.확인메모);
    }

    private sealed class ReceiptStore : ISsalddelFoodOrderStore
    {
        private readonly 음식주문응답 _order = new()
        {
            주문번호 = "FOOD-RECEIPT",
            주문자UserId = "orderer-1",
            상태 = 음식주문상태코드.전달완료,
            배차상태 = 음식주문배차상태코드.배달완료
        };
        private Guid? _processedRequestId;

        public 음식주문목록응답 GetOrders() => new() { Items = [_order] };
        public 음식주문응답? GetOrder(string orderNo)
            => orderNo == _order.주문번호 ? _order : null;
        public 음식주문응답 AddOrder(음식주문등록요청 request) => throw new NotSupportedException();
        public 음식주문응답? 음식점수락(string orderNo, 음식점주문수락요청 request)
            => throw new NotSupportedException();
        public 음식주문응답? 배차대기반영(string orderNo, long dispatchWaitId, DateTime dispatchRequestedAtUtc)
            => throw new NotSupportedException();

        public 음식주문변경결과? 주문자수령확인(
            string orderNo,
            주문자음식주문수령확인요청 request,
            string 주문자UserId)
        {
            if (orderNo != _order.주문번호 || 주문자UserId != _order.주문자UserId)
            {
                return null;
            }

            if (_processedRequestId == request.클라이언트요청Id)
            {
                return new 음식주문변경결과(_order, false);
            }

            _processedRequestId = request.클라이언트요청Id;
            _order.상태 = 음식주문상태코드.수령확인;
            return new 음식주문변경결과(_order, true);
        }
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
