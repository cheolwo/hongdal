namespace Hongdal.Client.Infrastructure.Transport;

public sealed record TransportRequestLedgerSnapshot(
    string RequestId,
    string? RequestStatus,
    string? PaymentStatus,
    string? DispatchStatus,
    string? SettlementStatus,
    DateTimeOffset ObservedAtUtc,
    string Source)
{
    public bool HasSameStateAs(TransportRequestLedgerSnapshot other)
        => string.Equals(RequestStatus, other.RequestStatus, StringComparison.Ordinal)
           && string.Equals(PaymentStatus, other.PaymentStatus, StringComparison.Ordinal)
           && string.Equals(DispatchStatus, other.DispatchStatus, StringComparison.Ordinal)
           && string.Equals(SettlementStatus, other.SettlementStatus, StringComparison.Ordinal);
}

public sealed record TransportRequestLedgerChange(
    string RequestId,
    TransportRequestLedgerSnapshot Previous,
    TransportRequestLedgerSnapshot Current,
    string Reason);

public sealed record TransportRequestLedgerRefreshRequest(
    string RequestId,
    string Reason,
    DateTimeOffset RequestedAtUtc);

public interface ITransportRequestLedgerObserver
{
    event Action<TransportRequestLedgerChange>? Changed;

    event Action<TransportRequestLedgerRefreshRequest>? RefreshRequested;

    TransportRequestLedgerSnapshot? GetSnapshot(string requestId);

    bool Observe(TransportRequestLedgerSnapshot snapshot, string reason);

    void RequestRefresh(string requestId, string reason);
}

public sealed class TransportRequestLedgerObserver : ITransportRequestLedgerObserver
{
    private readonly object _gate = new();
    private readonly Dictionary<string, TransportRequestLedgerSnapshot> _snapshots = new(StringComparer.OrdinalIgnoreCase);

    public event Action<TransportRequestLedgerChange>? Changed;

    public event Action<TransportRequestLedgerRefreshRequest>? RefreshRequested;

    public TransportRequestLedgerSnapshot? GetSnapshot(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return null;
        }

        lock (_gate)
        {
            return _snapshots.TryGetValue(requestId.Trim(), out var snapshot) ? snapshot : null;
        }
    }

    public bool Observe(TransportRequestLedgerSnapshot snapshot, string reason)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (string.IsNullOrWhiteSpace(snapshot.RequestId))
        {
            return false;
        }

        TransportRequestLedgerChange? change = null;
        var normalized = snapshot.RequestId.Trim();
        snapshot = snapshot with { RequestId = normalized };

        lock (_gate)
        {
            if (_snapshots.TryGetValue(normalized, out var previous)
                && !previous.HasSameStateAs(snapshot))
            {
                change = new TransportRequestLedgerChange(
                    normalized,
                    previous,
                    snapshot,
                    string.IsNullOrWhiteSpace(reason) ? "Observed" : reason.Trim());
            }

            _snapshots[normalized] = snapshot;
        }

        if (change is null)
        {
            return false;
        }

        Changed?.Invoke(change);
        return true;
    }

    public void RequestRefresh(string requestId, string reason)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return;
        }

        RefreshRequested?.Invoke(new TransportRequestLedgerRefreshRequest(
            requestId.Trim(),
            string.IsNullOrWhiteSpace(reason) ? "RefreshRequested" : reason.Trim(),
            DateTimeOffset.UtcNow));
    }
}
