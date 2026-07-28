using Ssalddel.Contracts.Food;
using Ssalddel.Client.Infrastructure.Security;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using RestaurantDeskApp.Options;

namespace RestaurantDeskApp.Services;

public sealed class 음식점주문SignalRClientService(
    IOptions<RestaurantDeskOptions> options,
    RestaurantAuthService authService,
    ClientAuthSession authSession) : I음식점주문SignalRClientService
{
    private const string ReceiveRestaurantOrderNotificationMethod = "ReceiveRestaurantOrderNotification";
    private const string ReceiveRestaurantOrderStatusChangedMethod = "ReceiveRestaurantOrderStatusChanged";
    private HubConnection? _connection;

    public event Func<음식점주문수신알림, Task>? 주문수신;

    public event Func<음식점주문상태변경알림, Task>? 주문상태변경;

    public event Func<string, Task>? 상태변경;

    public string 연결상태 => _connection?.State.ToString() ?? "Disconnected";

    public async Task 연결Async(CancellationToken cancellationToken = default)
    {
        var auth = await authService.EnsureAccessTokenAsync(
            cancellationToken: cancellationToken);
        if (!auth.IsSuccess)
        {
            throw new UnauthorizedAccessException(auth.ErrorMessage);
        }

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
            .WithUrl(
                BuildHubUri(),
                connectionOptions =>
                {
                    connectionOptions.AccessTokenProvider = async () =>
                    {
                        var result = await authService.EnsureAccessTokenAsync();
                        return result.IsSuccess ? authSession.AccessToken : null;
                    };
                })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<음식점주문수신알림>(
            ReceiveRestaurantOrderNotificationMethod,
            async notification => await Publish주문수신Async(notification));
        _connection.On<음식점주문상태변경알림>(
            ReceiveRestaurantOrderStatusChangedMethod,
            async notification => await Publish주문상태변경Async(notification));

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

        await _connection.InvokeAsync("JoinRestaurantOrders", cancellationToken);
    }

    private Uri BuildHubUri()
        => new(options.Value.GetServerBaseAddress(), "hubs/restaurant-orders");

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

    private async Task Publish주문상태변경Async(음식점주문상태변경알림 notification)
    {
        var handler = 주문상태변경;
        if (handler is null)
        {
            return;
        }

        foreach (Func<음식점주문상태변경알림, Task> callback in handler.GetInvocationList())
        {
            await callback(notification);
        }
    }
}
