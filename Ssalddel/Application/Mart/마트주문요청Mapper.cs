using Ssalddel.Contracts.Mart;
using 살뜰.도메인.마트;

namespace Ssalddel.Application.Mart;

internal static class 마트주문요청Mapper
{
    internal static 마트주문요청응답 ToResponse(마트주문요청 request)
        => new()
        {
            주문요청Id = request.Id,
            공개상품Id = request.공개상품Id,
            상품명 = request.상품명Snapshot,
            판매단위 = request.판매단위Snapshot,
            단가 = request.단가Snapshot,
            수량 = request.수량,
            합계 = request.합계Snapshot,
            통화 = request.통화,
            제출시판매가능수량 = request.제출시판매가능수량,
            재고기준시각Utc = request.재고기준시각Utc,
            상태코드 = request.상태코드,
            상태명 = 마트주문요청상태코드.표시명(request.상태코드),
            안내버전 = request.안내버전,
            제출일시Utc = request.CreatedAtUtc,
            재고예약됨 = false,
            결제됨 = false
        };
}
