using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Admin.Restaurants;
using Ssalddel.Contracts.Admin.Restaurants;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin;

[SsalddelApiVersion(
    SsalddelProductVersion.V3_0,
    FeatureKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[RequireVersionFeature(VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/restaurants/operator-access")]
[SsalddelApiContractName("RestaurantOperatorAccessController")]
public sealed class 음식점운영자접근Controller(
    I음식점운영자접근관리UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 조회(
        [FromQuery] string userId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.조회Async(userId, cancellationToken));

    [HttpPut]
    [SsalddelApiContractName("Assign")]
    public async Task<IActionResult> 배정(
        [FromBody] 음식점운영자접근배정요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.배정Async(request, cancellationToken));

    [HttpDelete]
    [SsalddelApiContractName("Revoke")]
    public async Task<IActionResult> 해제(
        [FromBody] 음식점운영자접근배정요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.해제Async(request, cancellationToken));
}
