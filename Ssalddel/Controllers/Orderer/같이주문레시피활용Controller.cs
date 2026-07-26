using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[SsalddelApiVersion(
    SsalddelProductVersion.V0_5,
    FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.OrdererGroupCommerce)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[Route("api/v1/orderer/order-mode-comparisons/recipe-uses")]
[SsalddelApiContractName("OrderModeRecipeUsesController")]
public sealed class 같이주문레시피활용Controller(
    I같이주문레시피활용UseCase useCase) : OrdererControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("GetRecipeUses")]
    [ProducesResponseType(typeof(같이주문레시피활용응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> 조회(
        [FromQuery] 같이주문레시피활용조회요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await useCase.조회Async(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "레시피 활용 조회 조건이 올바르지 않습니다.",
                detail: exception.Message);
        }
    }
}
