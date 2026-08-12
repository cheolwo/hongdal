using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Mart;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Contracts.Common.WorldProjection;
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
[Route(MarketPickingPackingWorldSnapshotRoutes.AuthorizedSnapshot)]
public sealed class 마트피킹포장WorldController(
    I마트피킹포장World조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> 조회(
        [FromQuery] long? warehouseId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.조회Async(warehouseId, cancellationToken));
}
