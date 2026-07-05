using Hongdal.FoodApi.Application;
using Hongdal.FoodApi.Application.DeliveryTickets;
using Hongdal.FoodApi.Application.Orders.Events;
using Hongdal.FoodApi.Services;

namespace Hongdal.FoodApi.Application.Orders.Handlers;

public sealed class 음식배달권인덱싱EventHandler : IFoodEventHandler<음식주문배차대기요청됨Event>
{
    private readonly 음식샘플Store _store;
    private readonly IKakao좌표변환Service _kakaoAddressService;
    private readonly IFoodDeliveryTicketMemoryIndex _ticketIndex;
    private readonly ILogger<음식배달권인덱싱EventHandler> _logger;

    public 음식배달권인덱싱EventHandler(
        음식샘플Store store,
        IKakao좌표변환Service kakaoAddressService,
        IFoodDeliveryTicketMemoryIndex ticketIndex,
        ILogger<음식배달권인덱싱EventHandler> logger)
    {
        _store = store;
        _kakaoAddressService = kakaoAddressService;
        _ticketIndex = ticketIndex;
        _logger = logger;
    }

    public async Task HandleAsync(음식주문배차대기요청됨Event appEvent, CancellationToken cancellationToken = default)
    {
        var restaurant = _store.음식점조회(appEvent.주문.음식점Id);
        var pickupAddress = restaurant?.주소 ?? $"음식점:{appEvent.주문.음식점Id}";
        var dropoffAddress = appEvent.주문.수령인정보.주소;
        var pickupKakao = await _kakaoAddressService.주소정보조회Async(pickupAddress, cancellationToken);
        var dropoffKakao = await _kakaoAddressService.주소정보조회Async(dropoffAddress, cancellationToken);
        var now = DateTime.UtcNow;

        var ticket = new FoodDeliveryTicket
        {
            TicketId = appEvent.주문.주문번호,
            FoodOrderNo = appEvent.주문.주문번호,
            RestaurantId = appEvent.주문.음식점Id,
            OrdererUserId = appEvent.주문.주문자UserId,
            PickupAddress = pickupAddress,
            DropoffAddress = dropoffAddress,
            PickupRegion = ResolveRegion(pickupKakao, pickupAddress),
            DropoffRegion = ResolveRegion(dropoffKakao, dropoffAddress),
            PickupLat = pickupKakao?.위도 ?? restaurant?.위도,
            PickupLng = pickupKakao?.경도 ?? restaurant?.경도,
            DropoffLat = dropoffKakao?.위도,
            DropoffLng = dropoffKakao?.경도,
            CreatedAtUtc = now,
            PickupReadyAtUtc = now,
            PriorityScore = CalculatePriorityScore(appEvent.주문.CreatedAt, now),
            SourceOrder = appEvent.주문
        };

        _ticketIndex.AddOrUpdate(ticket);

        _logger.LogInformation(
            "음식 배달권 메모리 인덱싱 완료. TicketId={TicketId}, Region2={Region2}, Region3={Region3}",
            ticket.TicketId,
            ticket.PickupRegion.Region2Key,
            ticket.PickupRegion.Region3Key);

    }

    private static decimal CalculatePriorityScore(DateTime orderCreatedAtUtc, DateTime nowUtc)
    {
        var waitingMinutes = Math.Max(0, (nowUtc - orderCreatedAtUtc).TotalMinutes);
        return 100m + (decimal)Math.Min(waitingMinutes, 60);
    }

    private static AddressRegionKey ResolveRegion(Kakao주소정보? kakao, string fallbackAddress)
    {
        if (kakao is not null
            && (!string.IsNullOrWhiteSpace(kakao.Region2) || !string.IsNullOrWhiteSpace(kakao.Region3)))
        {
            return AddressRegionKey.FromKakao(kakao.Region1, kakao.Region2, kakao.Region3);
        }

        return AddressRegionKey.FromAddress(fallbackAddress);
    }
}
