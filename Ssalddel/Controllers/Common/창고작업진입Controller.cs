using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Filters;
using Ssalddel.Security;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(
    SsalddelProductVersion.V2_5,
    FeatureKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.WarehouseFulfillment)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Warehouse)]
[SsalddelApiAudience(SsalddelActor.WarehouseManager)]
[SsalddelApiCapability(SsalddelCapability.WarehouseFulfillment)]
[SsalddelApiOperation(SsalddelOperation.Execute)]
[RequireVersionFeature(VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[ApiController]
[Authorize(Policy = "운영사용자전용")]
[Route("api/v1/warehouse-operations/work-entry")]
public sealed class 창고작업진입Controller(
    I창고작업진입UseCase useCase) : ControllerBase
{
    [HttpPost("verify")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInboundOperator,
        HrDetailedRoleCodes.WarehouseInventoryOperator,
        HrDetailedRoleCodes.WarehouseDispatchOperator,
        HrDetailedRoleCodes.ShippingAgencyOperator)]
    public async Task<IActionResult> 확인(
        [FromBody] 창고작업진입확인요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.확인Async(request, cancellationToken));
}
