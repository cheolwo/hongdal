namespace Ssalddel.FoodApi.Application.Pricing;

public interface IFoodDeliveryPricingService
{
    FoodDeliveryFareQuote Quote(int distanceMeters);
}
