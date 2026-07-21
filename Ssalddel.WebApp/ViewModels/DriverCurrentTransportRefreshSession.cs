using Ssalddel.Client.Infrastructure.Transport;

namespace Ssalddel.WebApp.ViewModels;

public sealed class DriverCurrentTransportRefreshSession : IDisposable
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(15);

    private readonly ITransportRequestLedgerObserver _ledgerObserver;
    private readonly Func<string, bool> _matchesRequest;
    private readonly Func<bool> _canRefresh;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private bool _started;
    private bool _disposed;

    public DriverCurrentTransportRefreshSession(
        ITransportRequestLedgerObserver ledgerObserver,
        Func<string, bool> matchesRequest,
        Func<bool> canRefresh)
    {
        _ledgerObserver = ledgerObserver;
        _matchesRequest = matchesRequest;
        _canRefresh = canRefresh;
    }

    public event Action? RefreshRequested;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started)
        {
            return;
        }

        _started = true;
        _ledgerObserver.Changed += HandleLedgerChanged;
        _ledgerObserver.RefreshRequested += HandleLedgerRefreshRequested;
        _ = RunPollingAsync(_lifetimeCancellation.Token);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_started)
        {
            _ledgerObserver.Changed -= HandleLedgerChanged;
            _ledgerObserver.RefreshRequested -= HandleLedgerRefreshRequested;
        }

        _lifetimeCancellation.Cancel();
        _lifetimeCancellation.Dispose();
        RefreshRequested = null;
    }

    private void HandleLedgerChanged(TransportRequestLedgerChange change)
        => RequestRefreshWhenCurrent(change.RequestId);

    private void HandleLedgerRefreshRequested(TransportRequestLedgerRefreshRequest request)
        => RequestRefreshWhenCurrent(request.RequestId);

    private void RequestRefreshWhenCurrent(string requestId)
    {
        if (_matchesRequest(requestId))
        {
            RefreshRequested?.Invoke();
        }
    }

    private async Task RunPollingAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                if (_canRefresh())
                {
                    RefreshRequested?.Invoke();
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
