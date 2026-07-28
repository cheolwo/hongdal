using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
public sealed class 음식점탐색Controller(
    I음식점탐색조회UseCase 음식점탐색UseCase,
    I음식점리뷰UseCase 음식점리뷰UseCase) : OrdererControllerBase
{
    [HttpGet("categories")]
    public async Task<IActionResult> 카테고리목록(CancellationToken cancellationToken)
        => this.ToActionResult(await 음식점탐색UseCase.카테고리목록Async(cancellationToken));

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

    [HttpGet("{restaurantId:long}/reviews")]
    public async Task<IActionResult> 리뷰목록(long restaurantId, CancellationToken cancellationToken)
        => this.ToActionResult(await 음식점리뷰UseCase.목록Async(restaurantId, cancellationToken));

    [HttpPost("{restaurantId:long}/reviews")]
    [Authorize]
    public async Task<IActionResult> 리뷰등록(
        long restaurantId,
        [FromBody] 음식점리뷰등록요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await 음식점리뷰UseCase.등록Async(
            restaurantId,
            request,
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("주문자 인증 정보를 확인할 수 없습니다."),
            cancellationToken));
}
