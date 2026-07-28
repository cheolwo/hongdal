using Ssalddel.Contracts.Food;
using RestaurantDeskApp.Models.Restaurant;

namespace RestaurantDeskApp.Services;

public interface I음식점주문SignalRClientService : IAsyncDisposable
{
    event Func<음식점주문수신알림, Task>? 주문수신;

    event Func<음식점주문상태변경알림, Task>? 주문상태변경;

    event Func<음식점실시간연결상태변경, Task>? 상태변경;

    event Func<Task>? 재연결후재조회요청;

    음식점실시간연결상태 연결상태 { get; }

    Task 연결Async(CancellationToken cancellationToken = default);

    Task 연결해제Async();
}
