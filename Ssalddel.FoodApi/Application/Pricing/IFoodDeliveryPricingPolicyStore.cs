using Ssalddel.FoodApi.Options;

namespace Ssalddel.FoodApi.Application.Pricing;

public interface IFoodDeliveryPricingPolicyStore
{
    FoodDeliveryPricingOptions Get();
    FoodDeliveryPricingOptions Update(FoodDeliveryPricingOptions policy);
}
