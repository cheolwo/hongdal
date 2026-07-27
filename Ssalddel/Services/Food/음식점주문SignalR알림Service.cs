using Ssalddel.Contracts.Food;
using Ssalddel.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ssalddel.Services.Food;

public sealed class 음식점주문SignalR알림Service(
    IHubContext<RestaurantOrderHub> hubContext,
    ILogger<음식점주문SignalR알림Service> logger) : I음식점주문실시간알림Service
{
    public async Task 신규주문알림발송Async(음식주문응답 order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        var notification = new 음식점주문수신알림
        {
            주문번호 = order.주문번호,
            음식점Id = order.음식점Id,
            고객명 = order.수령인정보.수령인명,
            메뉴요약 = BuildMenuSummary(order),
            상품목록 = order.상품목록.Select(item => new 음식주문상품Dto
            {
                상품명 = item.상품명,
                수량 = item.수량,
                단가 = item.단가
            }).ToArray(),
            주문금액 = order.총주문금액,
            상태 = order.상태,
            수신시각 = DateTimeOffset.UtcNow,
            제목 = "신규 음식 주문",
            본문 = $"{order.수령인정보.수령인명}님의 주문이 접수되었습니다."
        };

        await hubContext.Clients
            .Group(RestaurantOrderHub.BuildRestaurantGroup(order.음식점Id))
            .SendAsync(
                RestaurantOrderHub.ReceiveRestaurantOrderNotificationMethod,
                notification,
                cancellationToken);

        logger.LogInformation(
            "음식점 주문 SignalR 알림 발송. 주문번호={OrderNo}, 음식점Id={RestaurantId}, 금액={Amount}",
            order.주문번호,
            order.음식점Id,
            order.총주문금액);
    }

    private static string BuildMenuSummary(음식주문응답 order)
    {
        return string.Join(", ", order.상품목록.Select(x => $"{x.상품명} {x.수량}"));
    }
}
