using Hongdal.FoodApi.Application.DeliveryTickets;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.FoodApi.Controllers.Common;

[ApiController]
[Route("api/v1/food-delivery-tickets")]
public sealed class 음식배달권Controller : ControllerBase
{
    private readonly IFoodDeliveryTicketMemoryIndex _ticketIndex;
    private readonly IFoodDeliveryTicketRecommendationService _recommendationService;

    public 음식배달권Controller(
        IFoodDeliveryTicketMemoryIndex ticketIndex,
        IFoodDeliveryTicketRecommendationService recommendationService)
    {
        _ticketIndex = ticketIndex;
        _recommendationService = recommendationService;
    }

    [HttpGet("{ticketId}")]
    public ActionResult<FoodDeliveryTicket> 단건조회(string ticketId)
    {
        var ticket = _ticketIndex.GetById(ticketId);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpGet("pending")]
    public ActionResult<IReadOnlyList<FoodDeliveryTicket>> 지역대기조회(
        [FromQuery] string? region1,
        [FromQuery] string? region2,
        [FromQuery] string? region3,
        [FromQuery] int take = 20)
    {
        var region = new AddressRegionKey(region1 ?? string.Empty, region2 ?? string.Empty, region3 ?? string.Empty);
        return Ok(_ticketIndex.GetPendingByRegion(region, Math.Clamp(take, 1, 100)));
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<IReadOnlyList<FoodDeliveryTicketRecommendation>>> 추천조회(
        [FromQuery] decimal? driverLat,
        [FromQuery] decimal? driverLng,
        [FromQuery] string? region1,
        [FromQuery] string? region2,
        [FromQuery] string? region3,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _recommendationService.RecommendAsync(
            new FoodDeliveryTicketRecommendationRequest
            {
                DriverLat = driverLat,
                DriverLng = driverLng,
                Region1 = region1,
                Region2 = region2,
                Region3 = region3,
                Take = take
            },
            cancellationToken);

        return Ok(result);
    }
}
