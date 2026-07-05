using Hongdal.FoodApi.Application.Pricing;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.FoodApi.Controllers.Common;

[ApiController]
[Route("api/v1/food-delivery-pricing")]
public sealed class 음식배달요금Controller : ControllerBase
{
    private readonly IFoodDeliveryPricingService _pricingService;

    public 음식배달요금Controller(IFoodDeliveryPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    [HttpGet("quote")]
    public ActionResult<FoodDeliveryFareQuote> 견적([FromQuery] int distanceMeters)
    {
        return Ok(_pricingService.Quote(distanceMeters));
    }
}
