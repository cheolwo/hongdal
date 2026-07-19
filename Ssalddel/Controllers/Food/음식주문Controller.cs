using Ssalddel.ApiMetadata;
using Ssalddel.Application.Food;
using Ssalddel.Contracts.Food;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Food;

[SsalddelApiVersion(SsalddelProductVersion.V3_0, FeatureKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow, WorkflowKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.FoodDelivery)]
[ApiController]
[Route("api/v1/food-orders")]
public sealed class 음식주문Controller(I음식주문접수UseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<음식주문목록응답>> 목록조회(CancellationToken cancellationToken)
    {
        return Ok(await useCase.목록조회Async(cancellationToken));
    }

    [HttpGet("{orderNo}")]
    public async Task<ActionResult<음식주문응답>> 상세조회(string orderNo, CancellationToken cancellationToken)
    {
        var order = await useCase.상세조회Async(orderNo, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<음식주문응답>> 등록([FromBody] 음식주문등록요청 request, CancellationToken cancellationToken)
    {
        return Ok(await useCase.등록Async(request, cancellationToken));
    }

    [HttpPost("{orderNo}/restaurant-acceptance")]
    public async Task<ActionResult<음식주문응답>> 음식점수락(
        string orderNo,
        [FromBody] 음식점주문수락요청 request,
        CancellationToken cancellationToken)
    {
        var order = await useCase.음식점수락Async(orderNo, request, 현재사용자Id() ?? request.처리UserId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    private string? 현재사용자Id()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? User.Identity?.Name;
}
