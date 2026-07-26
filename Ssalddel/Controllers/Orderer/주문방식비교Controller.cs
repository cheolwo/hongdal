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
[Route("api/v1/orderer/order-mode-comparisons")]
[SsalddelApiContractName("OrderModeComparisonController")]
public sealed class 주문방식비교Controller(
    I주문방식비교UseCase 주문방식비교UseCase) : OrdererControllerBase
{
    [HttpPost("preview")]
    [SsalddelApiContractName("Preview")]
    [ProducesResponseType(typeof(주문방식비교응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult 미리보기([FromBody] 주문방식비교요청 request)
    {
        try
        {
            return Ok(주문방식비교UseCase.비교(request));
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "주문 방식 비교 조건이 올바르지 않습니다.",
                detail: exception.Message);
        }
    }
}
