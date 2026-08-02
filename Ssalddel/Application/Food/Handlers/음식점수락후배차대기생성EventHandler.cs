using Ssalddel.Application.Food.Events;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Community;
using Ssalddel.Services.Food;
using MediatR;
using 살뜰.도메인.공통;
using 살뜰.Services.Dispatch.Engine;
using 살뜰.Services.Dispatch.Queue;
using 살뜰.Services.Transport;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 음식점수락후배차대기생성EventHandler(
    I운송의뢰배차대기Service dispatchQueueService,
    I운송원장Mongo동기화Service transportLedgerSync,
    I음식마트원장동기화OutboxService foodMartLedgerOutbox,
    I음식점주문실시간알림Service restaurantNotification,
    ITransportRequestLedgerRealtimeService transportLedgerRealtimeService,
    ISsalddelFoodOrderStore orderStore,
    IKakao좌표변환Service kakaoGeoService,
    SsalddelContext db,
    ILogger<음식점수락후배차대기생성EventHandler> logger) : INotificationHandler<음식점주문수락됨Event>
{
    public async Task Handle(음식점주문수락됨Event notification, CancellationToken cancellationToken)
    {
        var order = notification.주문;
        var pickupAddress = ResolvePickupAddress(order);
        var dropoffAddress = order.수령인정보.주소;
        var pickupCoordinate = await ResolveCoordinateAsync(order.음식점위도, order.음식점경도, pickupAddress, cancellationToken);
        var dropoffCoordinate = await ResolveCoordinateAsync(null, null, dropoffAddress, cancellationToken);

        var target = new 출고예정운송대상
        {
            원천유형 = 출고예정운송대상원천유형.음식주문,
            원천참조번호 = order.주문번호,
            운송의뢰Id = order.주문번호,
            표시명 = FoodOrderSampleData.BuildMenuSummary(order.상품목록),
            판매자UserId = $"restaurant:{order.음식점Id}",
            주문자UserId = order.주문자UserId,
            상차주소 = pickupAddress,
            상차위도 = pickupCoordinate?.위도,
            상차경도 = pickupCoordinate?.경도,
            하차주소 = dropoffAddress,
            하차위도 = dropoffCoordinate?.위도,
            하차경도 = dropoffCoordinate?.경도,
            온도조건 = "음식",
            파손주의 = true,
            Lines = order.상품목록.Select((item, index) => new 출고예정운송대상라인
            {
                LineKey = $"{order.주문번호}-{index + 1}",
                Sku = item.상품명,
                ProductName = item.상품명,
                Quantity = item.수량
            }).ToArray()
        };

        var queue = await dispatchQueueService.생성또는조회Async(
            target,
            new 운송의뢰배차대기생성옵션
            {
                의뢰Id = order.주문번호,
                화주Id = $"restaurant:{order.음식점Id}",
                배차업무유형 = 상태값.배차업무유형.음식배달,
                원본의뢰유형 = 운송의뢰배차원천유형.음식점주문,
                원본의뢰Id = order.주문번호,
                픽업상세주소 = order.음식점상세주소,
                하차상세주소 = order.수령인정보.상세주소,
                상태 = 상태값.배차대기상태.대기
            },
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await transportLedgerSync.운송실행투영동기화Async(queue, $"restaurant:{order.음식점Id}", cancellationToken);
        var updatedOrder = orderStore.배차대기반영(order.주문번호, queue.Id, DateTime.UtcNow);
        if (updatedOrder is not null)
        {
            await foodMartLedgerOutbox.음식주문예약후즉시처리Async(
                updatedOrder,
                $"restaurant:{order.음식점Id}",
                $"food-dispatch:{notification.EventId}",
                cancellationToken);
            await transportLedgerRealtimeService.PublishAsync(
                order.주문번호,
                nameof(음식점주문수락됨Event),
                cancellationToken);
            await NotifyRestaurantAsync(updatedOrder, cancellationToken);
        }

        logger.LogInformation(
            "음식점 주문 수락 후 배차대기 생성 완료. EventId={EventId}, 주문번호={OrderNo}, 배차대기Id={DispatchWaitId}",
            notification.EventId,
            order.주문번호,
            queue.Id);
    }

    private async Task NotifyRestaurantAsync(
        음식주문응답 order,
        CancellationToken cancellationToken)
    {
        try
        {
            await restaurantNotification.주문상태변경알림발송Async(
                order,
                "음식점 수락 후 기사 배차를 시작했습니다.",
                cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                ex,
                "음식점 수락 후 배차 상태 실시간 알림에 실패했습니다. 주문번호={OrderNo}",
                order.주문번호);
        }
    }

    private async Task<(decimal 위도, decimal 경도)?> ResolveCoordinateAsync(
        decimal? latitude,
        decimal? longitude,
        string address,
        CancellationToken cancellationToken)
    {
        if (latitude.HasValue && longitude.HasValue)
        {
            return (latitude.Value, longitude.Value);
        }

        try
        {
            var info = await kakaoGeoService.주소정보조회Async(address, cancellationToken);
            if (info?.위도 is decimal resolvedLatitude && info.경도 is decimal resolvedLongitude)
            {
                return (resolvedLatitude, resolvedLongitude);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "음식 주문 배차 좌표 조회 실패. Address={Address}", address);
        }

        return null;
    }

    private static string ResolvePickupAddress(음식주문응답 order)
    {
        if (!string.IsNullOrWhiteSpace(order.음식점주소))
        {
            return order.음식점주소.Trim();
        }

        return $"음식점:{order.음식점Id}";
    }
}
