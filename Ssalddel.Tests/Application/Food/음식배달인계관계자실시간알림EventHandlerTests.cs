using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.Food.Events;
using Ssalddel.Application.Food.Handlers;
using 살뜰.Services.Transport;

namespace Ssalddel.Tests.Application.Food;

public sealed class 음식배달인계관계자실시간알림EventHandlerTests
{
    [Fact]
    public async Task Handle_주문번호와상태로관계자원장재조회를요청한다()
    {
        var realtime = new RecordingRealtimeService();
        var handler = new 음식배달인계관계자실시간알림EventHandler(
            realtime,
            NullLogger<음식배달인계관계자실시간알림EventHandler>.Instance);

        await handler.Handle(
            new 음식배달인계상태변경됨Event(
                "delivery-101",
                "FOOD-101",
                "픽업완료",
                DateTime.UtcNow,
                "event-101"),
            CancellationToken.None);

        Assert.Equal("FOOD-101", realtime.RequestId);
        Assert.Equal("픽업완료", realtime.EventType);
    }

    [Fact]
    public async Task Handle_주문번호가없으면관계자원장재조회를생략한다()
    {
        var realtime = new RecordingRealtimeService();
        var handler = new 음식배달인계관계자실시간알림EventHandler(
            realtime,
            NullLogger<음식배달인계관계자실시간알림EventHandler>.Instance);

        await handler.Handle(
            new 음식배달인계상태변경됨Event(
                "delivery-102",
                " ",
                "전달완료",
                DateTime.UtcNow,
                "event-102"),
            CancellationToken.None);

        Assert.Null(realtime.RequestId);
    }

    private sealed class RecordingRealtimeService : ITransportRequestLedgerRealtimeService
    {
        public string? RequestId { get; private set; }

        public string? EventType { get; private set; }

        public Task PublishAsync(
            string requestId,
            string eventType,
            CancellationToken cancellationToken = default)
        {
            RequestId = requestId;
            EventType = eventType;
            return Task.CompletedTask;
        }
    }
}
