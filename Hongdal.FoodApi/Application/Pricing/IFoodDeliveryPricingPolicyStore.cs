using Hongdal.FoodApi.Options;

namespace Hongdal.FoodApi.Application.Pricing;

public interface IFoodDeliveryPricingPolicyStore
{
    FoodDeliveryPricingOptions Get();
    FoodDeliveryPricingOptions Update(FoodDeliveryPricingOptions policy);
}
