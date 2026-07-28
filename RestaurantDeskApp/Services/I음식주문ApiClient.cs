using Ssalddel.Contracts.Food;

namespace RestaurantDeskApp.Services;

public interface I음식주문ApiClient
{
    Task<음식점주문수신함응답> 주문목록조회Async(
        음식점주문수신함조회요청 request,
        CancellationToken cancellationToken = default);

    Task<음식주문응답?> 주문상세조회Async(string 주문번호, CancellationToken cancellationToken = default);

    Task<음식주문응답?> 음식점수락Async(
        string 주문번호,
        음식점주문수락요청 request,
        CancellationToken cancellationToken = default);
}
