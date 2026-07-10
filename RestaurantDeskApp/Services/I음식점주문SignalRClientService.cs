using Hongdal.Contracts.Food;

namespace RestaurantDeskApp.Services;

public interface I음식점주문SignalRClientService : IAsyncDisposable
{
    event Func<음식점주문수신알림, Task>? 주문수신;

    event Func<string, Task>? 상태변경;

    string 연결상태 { get; }

    Task 연결Async(long restaurantId, CancellationToken cancellationToken = default);

    Task 연결해제Async();
}
