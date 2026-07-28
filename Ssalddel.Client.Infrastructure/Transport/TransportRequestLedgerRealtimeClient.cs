using Microsoft.AspNetCore.SignalR.Client;
using Ssalddel.Contracts.Common.Transport;

namespace Ssalddel.Client.Infrastructure.Transport;

public sealed class TransportRequestLedgerRealtimeClient : IAsyncDisposable
{
    private readonly Uri _baseAddress;
    private readonly Func<CancellationToken, Task<string?>> _accessTokenProvider;
    private readonly ITransportRequestLedgerObserver _observer;
    private readonly SemaphoreSlim _connectionGate = new(1, 1);
    private HubConnection? _connection;

    public TransportRequestLedgerRealtimeClient(
        Uri baseAddress,
        Func<CancellationToken, Task<string?>> accessTokenProvider,
        ITransportRequestLedgerObserver observer)
    {
        _baseAddress = baseAddress;
        _accessTokenProvider = accessTokenProvider;
        _observer = observer;
    }

    public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection?.State is HubConnectionState.Connected
            or HubConnectionState.Connecting
            or HubConnectionState.Reconnecting)
        {
            return;
        }

        await _connectionGate.WaitAsync(cancellationToken);
        try
        {
            if (_connection?.State is HubConnectionState.Connected
                or HubConnectionState.Connecting
                or HubConnectionState.Reconnecting)
            {
                return;
            }

            var token = await _accessTokenProvider(cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }

            var hubUri = new Uri(
                _baseAddress,
                TransportRequestLedgerRealtime.HubPath.TrimStart('/'));
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUri, options =>
                {
                    options.AccessTokenProvider = async () =>
                        await _accessTokenProvider(CancellationToken.None);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<TransportRequestLedgerChangedResponse>(
                TransportRequestLedgerRealtime.ChangedMethod,
                changed =>
                {
                    _observer.ObserveServerEvent(new TransportRequestLedgerServerEvent(
                        changed.RequestId,
                        changed.RequestStatus,
                        changed.PaymentStatus,
                        changed.DispatchStatus,
                        changed.SettlementStatus,
                        changed.TransportStatus,
                        changed.ChangedAtUtc,
                        changed.Source,
                        changed.EventType));
                    _observer.RequestRefresh(
                        changed.RequestId,
                        string.IsNullOrWhiteSpace(changed.EventType)
                            ? changed.Source
                            : $"{changed.Source}:{changed.EventType}");
                });

            await _connection.StartAsync(cancellationToken);
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _connectionGate.WaitAsync(cancellationToken);
        try
        {
            if (_connection is null)
            {
                return;
            }

            await _connection.StopAsync(cancellationToken);
            await _connection.DisposeAsync();
            _connection = null;
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _connectionGate.Dispose();
    }
}
