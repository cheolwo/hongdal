using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Mart;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Contracts.Mart;
using Ssalddel.Filters;
using Ssalddel.Security;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(
    SsalddelProductVersion.V3_5,
    FeatureKey = VersionFeatureFlagKeys.SsalddelMartWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.SsalddelMartWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.SsalddelMart)]
[RequireVersionFeature(VersionFeatureFlagKeys.SsalddelMartWorkflow)]
[ApiController]
[Authorize(Policy = "운영사용자전용")]
[RequireHrRole(
    HrDetailedRoleCodes.WarehouseManager,
    HrDetailedRoleCodes.WarehouseInventoryOperator,
    HrDetailedRoleCodes.WarehouseDispatchOperator)]
[Route("api/v1/warehouse-operations/mart/picking-orders")]
public sealed class 마트피킹Controller(I마트피킹조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> 목록(
        [FromQuery] 마트피킹주문목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.목록Async(request, cancellationToken));

    [HttpGet("{orderId:long}")]
    public async Task<IActionResult> 상세(long orderId, CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.상세Async(orderId, cancellationToken));
}
