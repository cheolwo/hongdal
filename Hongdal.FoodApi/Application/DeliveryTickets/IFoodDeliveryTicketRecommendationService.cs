namespace Hongdal.FoodApi.Application.DeliveryTickets;

public interface IFoodDeliveryTicketRecommendationService
{
    Task<IReadOnlyList<FoodDeliveryTicketRecommendation>> RecommendAsync(
        FoodDeliveryTicketRecommendationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class FoodDeliveryTicketRecommendationRequest
{
    public decimal? DriverLat { get; init; }
    public decimal? DriverLng { get; init; }
    public string? Region1 { get; init; }
    public string? Region2 { get; init; }
    public string? Region3 { get; init; }
    public int Take { get; init; } = 20;
}
