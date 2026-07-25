using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Food;
using Ssalddel.Contracts.Restaurants;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[SsalddelApiVersion(
    SsalddelProductVersion.V3_0,
    FeatureKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.FoodDelivery)]
[RequireVersionFeature(VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[ApiController]
[Route("api/v1/orderer/restaurants")]
public sealed class 음식점탐색Controller(I음식점탐색조회UseCase 음식점탐색UseCase) : OrdererControllerBase
{
    [HttpGet("service-areas")]
    public async Task<IActionResult> 권역목록(CancellationToken cancellationToken)
        => this.ToActionResult(await 음식점탐색UseCase.권역목록Async(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> 목록(
        [FromQuery] 음식점공개목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await 음식점탐색UseCase.목록Async(request, cancellationToken));

    [HttpGet("{restaurantId:long}")]
    public async Task<IActionResult> 상세(long restaurantId, CancellationToken cancellationToken)
        => this.ToActionResult(await 음식점탐색UseCase.상세Async(restaurantId, cancellationToken));
}
