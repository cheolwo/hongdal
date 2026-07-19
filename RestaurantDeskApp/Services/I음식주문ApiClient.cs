using Ssalddel.Contracts.Food;

namespace RestaurantDeskApp.Services;

public interface I음식주문ApiClient
{
    Task<IReadOnlyList<음식주문응답>> 주문목록조회Async(CancellationToken cancellationToken = default);

    Task<음식주문응답?> 주문상세조회Async(string 주문번호, CancellationToken cancellationToken = default);
}
