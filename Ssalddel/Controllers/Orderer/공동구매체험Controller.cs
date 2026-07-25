using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[AllowAnonymous]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_0,
    FeatureKey = VersionFeatureFlagKeys.GroupPurchasePracticeWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.GroupPurchasePracticeWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchasePracticeWorkflow)]
[Route("api/v1/orderer/group-purchase-practice")]
public sealed class 공동구매체험Controller(I공동구매체험Service 체험Service) : OrdererControllerBase
{
    [HttpGet("scenarios")]
    [ProducesResponseType(typeof(IReadOnlyList<공동구매체험시나리오응답>), StatusCodes.Status200OK)]
    public IActionResult 시나리오목록()
        => Ok(체험Service.시나리오목록());

    [HttpPost("simulate")]
    [ProducesResponseType(typeof(공동구매체험응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult 시뮬레이션([FromBody] 공동구매체험요청 request)
    {
        try
        {
            return Ok(체험Service.시뮬레이션(request));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "공동구매 연습 조건을 확인해 주세요.",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
