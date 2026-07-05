using Hongdal.Contracts.Food;

namespace Hongdal.FoodApi.Application.DeliveryTickets;

public sealed class FoodDeliveryTicket
{
    public string TicketId { get; init; } = string.Empty;
    public string FoodOrderNo { get; init; } = string.Empty;
    public long RestaurantId { get; init; }
    public string OrdererUserId { get; init; } = string.Empty;
    public string PickupAddress { get; init; } = string.Empty;
    public string DropoffAddress { get; init; } = string.Empty;
    public AddressRegionKey PickupRegion { get; init; } = AddressRegionKey.Empty;
    public AddressRegionKey DropoffRegion { get; init; } = AddressRegionKey.Empty;
    public decimal? PickupLat { get; init; }
    public decimal? PickupLng { get; init; }
    public decimal? DropoffLat { get; init; }
    public decimal? DropoffLng { get; init; }
    public string Status { get; set; } = FoodDeliveryTicketStatus.Pending;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime PickupReadyAtUtc { get; init; }
    public decimal PriorityScore { get; init; }
    public 음식주문응답 SourceOrder { get; init; } = new();
}

public static class FoodDeliveryTicketStatus
{
    public const string Pending = "대기";
    public const string Assigned = "기사배정";
    public const string PickedUp = "픽업완료";
    public const string Delivered = "배달완료";
    public const string Canceled = "취소";
}
