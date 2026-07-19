using Ssalddel.FoodApi.Application.Pricing;
using Ssalddel.FoodApi.Options;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.FoodApi.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/food-delivery-pricing-policy")]
public sealed class 음식배달요금정책Controller : ControllerBase
{
    private readonly IFoodDeliveryPricingPolicyStore _policyStore;

    public 음식배달요금정책Controller(IFoodDeliveryPricingPolicyStore policyStore)
    {
        _policyStore = policyStore;
    }

    [HttpGet]
    public ActionResult<FoodDeliveryPricingOptions> 조회()
    {
        return Ok(_policyStore.Get());
    }

    [HttpPut]
    public ActionResult<FoodDeliveryPricingOptions> 수정([FromBody] FoodDeliveryPricingOptions request)
    {
        return Ok(_policyStore.Update(request));
    }
}
