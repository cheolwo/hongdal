namespace Ssalddel.FoodApi.Application.DeliveryTickets;

public sealed class FoodDeliveryTicketRecommendation
{
    public string TicketId { get; init; } = string.Empty;
    public string FoodOrderNo { get; init; } = string.Empty;
    public long RestaurantId { get; init; }
    public string PickupAddress { get; init; } = string.Empty;
    public string DropoffAddress { get; init; } = string.Empty;
    public string PickupRegion2Key { get; init; } = string.Empty;
    public string PickupRegion3Key { get; init; } = string.Empty;
    public decimal PriorityScore { get; init; }
    public double? DistanceKm { get; init; }
    public DateTime PickupReadyAtUtc { get; init; }
}
