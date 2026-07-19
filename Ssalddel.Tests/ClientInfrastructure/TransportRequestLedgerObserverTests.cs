using Ssalddel.Client.Infrastructure.Transport;

namespace Ssalddel.Tests.ClientInfrastructure;

public sealed class TransportRequestLedgerObserverTests
{
    [Fact]
    public void ObserveServerEvent_uses_transport_status_when_dispatch_status_is_empty()
    {
        var observer = new TransportRequestLedgerObserver();
        TransportRequestLedgerChange? observed = null;
        observer.Changed += change => observed = change;

        var initialChanged = observer.Observe(
            new TransportRequestLedgerSnapshot(
                "REQ-1",
                "생성됨",
                "결제대기",
                "배차확정",
                "청구대기",
                DateTimeOffset.UtcNow.AddMinutes(-1),
                "Initial"),
            "Initial");

        var serverChanged = observer.ObserveServerEvent(
            new TransportRequestLedgerServerEvent(
                "REQ-1",
                "생성됨",
                "결제대기",
                null,
                "청구대기",
                "상차완료",
                DateTimeOffset.UtcNow,
                "ServerLedger",
                "운송상차완료됨Event"));

        Assert.False(initialChanged);
        Assert.True(serverChanged);
        Assert.NotNull(observed);
        Assert.Equal("배차확정", observed!.Previous.DispatchStatus);
        Assert.Equal("상차완료", observed.Current.DispatchStatus);
        Assert.Equal("ServerLedger:운송상차완료됨Event", observed.Reason);
    }

    [Fact]
    public void ObserveServerEvent_returns_false_when_request_id_is_empty()
    {
        var observer = new TransportRequestLedgerObserver();

        var changed = observer.ObserveServerEvent(
            new TransportRequestLedgerServerEvent(
                string.Empty,
                null,
                null,
                null,
                null,
                "상차완료",
                DateTimeOffset.UtcNow,
                "ServerLedger"));

        Assert.False(changed);
    }
}
