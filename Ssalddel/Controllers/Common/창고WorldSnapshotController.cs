using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Security;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[SsalddelApiIntroducedIn(SsalddelProductVersion.V2_5)]
[SsalddelApiFeature(VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[SsalddelApiCapability(SsalddelCapability.InventoryManagement)]
[SsalddelApiCapability(SsalddelCapability.WarehouseFulfillment)]
[SsalddelApiAudience(SsalddelActor.WarehouseManager)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelApiWorkflow(SsalddelWorkflow.WarehouseFulfillment)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Warehouse)]
[ApiController]
[Authorize(Policy = "운영사용자전용")]
[RequireHrRole(HrDetailedRoleCodes.WarehouseManager)]
[Route(WarehouseWorldSnapshotRoutes.AuthorizedSnapshot)]
public sealed class 창고WorldSnapshotController(
    I창고WorldSnapshot조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> 조회(
        [FromQuery] long? warehouseId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.조회Async(warehouseId, cancellationToken));
}
