using Hongdal.Contracts.Food;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using RestaurantDeskApp.Options;

namespace RestaurantDeskApp.Services;

public sealed class 음식점주문SignalRClientService(IOptions<RestaurantDeskOptions> options) : I음식점주문SignalRClientService
{
    private const string ReceiveRestaurantOrderNotificationMethod = "ReceiveRestaurantOrderNotification";
    private HubConnection? _connection;
    private long _restaurantId;

    public event Func<음식점주문수신알림, Task>? 주문수신;

    public event Func<string, Task>? 상태변경;

    public string 연결상태 => _connection?.State.ToString() ?? "Disconnected";

    public async Task 연결Async(long restaurantId, CancellationToken cancellationToken = default)
    {
        if (restaurantId <= 0)
        {
            throw new InvalidOperationException("음식점Id가 필요합니다.");
        }

        _restaurantId = restaurantId;
        if (_connection is not null)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await JoinRestaurantGroupAsync(cancellationToken);
                await Publish상태Async("음식점 주문 허브에 이미 연결되어 있습니다.");
                return;
            }

            await _connection.DisposeAsync();
            _connection = null;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(BuildHubUri())
            .WithAutomaticReconnect()
            .Build();

        _connection.On<음식점주문수신알림>(
            ReceiveRestaurantOrderNotificationMethod,
            async notification => await Publish주문수신Async(notification));

        _connection.Reconnecting += async error =>
        {
            await Publish상태Async(error is null
                ? "음식점 주문 허브 재연결을 시도합니다."
                : $"음식점 주문 허브 연결이 끊겼습니다. 재연결을 시도합니다. {error.Message}");
        };

        _connection.Reconnected += async _ =>
        {
            await JoinRestaurantGroupAsync(CancellationToken.None);
            await Publish상태Async("음식점 주문 허브에 다시 연결되었습니다.");
        };

        _connection.Closed += async error =>
        {
            await Publish상태Async(error is null
                ? "음식점 주문 허브 연결이 종료되었습니다."
                : $"음식점 주문 허브 연결이 종료되었습니다. {error.Message}");
        };

        await _connection.StartAsync(cancellationToken);
        await JoinRestaurantGroupAsync(cancellationToken);
        await Publish상태Async("음식점 주문 허브에 연결되었습니다.");
    }

    public async Task 연결해제Async()
    {
        if (_connection is null)
        {
            return;
        }

        await _connection.DisposeAsync();
        _connection = null;
    }

    public async ValueTask DisposeAsync()
    {
        await 연결해제Async();
    }

    private async Task JoinRestaurantGroupAsync(CancellationToken cancellationToken)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            return;
        }

        await _connection.InvokeAsync("JoinRestaurantOrders", _restaurantId, cancellationToken);
    }

    private Uri BuildHubUri()
    {
        var baseUrl = string.IsNullOrWhiteSpace(options.Value.ServerBaseUrl)
            ? "https://localhost:7117/"
            : options.Value.ServerBaseUrl.Trim();

        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        return new Uri(new Uri(baseUrl), "hubs/restaurant-orders");
    }

    private async Task Publish주문수신Async(음식점주문수신알림 notification)
    {
        var handler = 주문수신;
        if (handler is null)
        {
            return;
        }

        foreach (Func<음식점주문수신알림, Task> callback in handler.GetInvocationList())
        {
            await callback(notification);
        }
    }

    private async Task Publish상태Async(string message)
    {
        var handler = 상태변경;
        if (handler is null)
        {
            return;
        }

        foreach (Func<string, Task> callback in handler.GetInvocationList())
        {
            await callback(message);
        }
    }
}
