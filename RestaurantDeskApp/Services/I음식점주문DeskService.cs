using RestaurantDeskApp.Models.Restaurant;

namespace RestaurantDeskApp.Services;

public interface I음식점주문DeskService
{
    Task<IReadOnlyList<음식점주문DeskItem>> 주문목록조회Async(CancellationToken cancellationToken = default);

    Task<음식점주문DeskItem?> 주문조회Async(string 주문번호, CancellationToken cancellationToken = default);

    Task<음식점주문DeskItem> 주문알림수신Async(음식점주문수신Payload payload, CancellationToken cancellationToken = default);

    Task<음식점주문수락결과> 주문수락후전표준비Async(string 주문번호, CancellationToken cancellationToken = default);

    Task<음식점주문수락결과> 주문수락후전표준비Async(
        string 주문번호,
        int 조리예상분,
        CancellationToken cancellationToken = default);

    Task 전표출력완료Async(string 주문번호, CancellationToken cancellationToken = default);
}
