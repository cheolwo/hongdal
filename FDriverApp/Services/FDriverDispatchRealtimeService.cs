using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Ssalddel.Contracts.Common.Drivers;

namespace FDriverApp.Services;

public interface IFDriverDispatchRealtimeService
{
    event Func<int, Task>? RecommendationsReceived;
    event Action<string>? StatusChanged;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public sealed class FDriverDispatchRealtimeService : IFDriverDispatchRealtimeService, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IFDriverAuthSession _session;
    private readonly FDriverAuthApiService _authApi;
    private readonly SemaphoreSlim _connectionGate = new(1, 1);
    private HubConnection? _connection;

    public FDriverDispatchRealtimeService(
        HttpClient httpClient,
        IFDriverAuthSession session,
        FDriverAuthApiService authApi)
    {
        _httpClient = httpClient;
        _session = session;
        _authApi = authApi;
    }

    public event Func<int, Task>? RecommendationsReceived;
    public event Action<string>? StatusChanged;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _connectionGate.WaitAsync(cancellationToken);
        try
        {
            if (_connection?.State is HubConnectionState.Connected
                or HubConnectionState.Connecting
                or HubConnectionState.Reconnecting)
            {
                return;
            }

            if (_httpClient.BaseAddress is null)
            {
                NotifyStatus("실시간 배차 주소 확인 필요 · 30초 자동 조회 유지");
                return;
            }

            if (_connection is null)
            {
                _connection = BuildConnection();
            }

            try
            {
                using var connectionTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                connectionTimeout.CancelAfter(TimeSpan.FromSeconds(8));
                await _connection.StartAsync(connectionTimeout.Token);
                NotifyStatus("실시간 배차 연결됨 · 30초 자동 조회 보조");
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                NotifyStatus("실시간 배차 연결 지연 · 30초 자동 조회 유지");
            }
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

            try
            {
                await _connection.StopAsync(cancellationToken);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                // The polling fallback is already stopped by the page model.
            }

            NotifyStatus("실시간 배차 연결 종료");
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connectionGate.WaitAsync();
        try
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        finally
        {
            _connectionGate.Release();
            _connectionGate.Dispose();
        }
    }

    private HubConnection BuildConnection()
    {
        var hubUri = new Uri(
            _httpClient.BaseAddress!,
            DriverDispatchRealtimeContract.HubPath.TrimStart('/'));
        var connection = new HubConnectionBuilder()
            .WithUrl(
                hubUri,
                options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        var error = await _authApi.EnsureAccessTokenAsync();
                        return error is null ? _session.AccessToken : null;
                    };
                })
            .WithAutomaticReconnect(
            [
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30)
            ])
            .Build();

        connection.On<JsonElement>(
            DriverDispatchRealtimeContract.RecommendationsEvent,
            payload => NotifyRecommendationsAsync(
                payload.ValueKind == JsonValueKind.Array ? payload.GetArrayLength() : 0));
        connection.Reconnecting += _ =>
        {
            NotifyStatus("실시간 배차 재연결 중 · 30초 자동 조회 유지");
            return Task.CompletedTask;
        };
        connection.Reconnected += _ =>
        {
            NotifyStatus("실시간 배차 다시 연결됨 · 30초 자동 조회 보조");
            return Task.CompletedTask;
        };
        connection.Closed += _ =>
        {
            NotifyStatus("실시간 배차 연결 끊김 · 30초 자동 조회 유지");
            return Task.CompletedTask;
        };

        return connection;
    }

    private async Task NotifyRecommendationsAsync(int count)
    {
        var handlers = RecommendationsReceived;
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<Func<int, Task>>())
        {
            try
            {
                await handler(count);
            }
            catch
            {
                // One screen listener must not break the shared SignalR receive loop.
            }
        }
    }

    private void NotifyStatus(string status)
        => StatusChanged?.Invoke(status);
}
