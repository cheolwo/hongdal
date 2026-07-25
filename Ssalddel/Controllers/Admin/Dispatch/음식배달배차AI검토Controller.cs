using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Admin.Dispatch;
using Ssalddel.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Dispatch.Coordination;

namespace Ssalddel.Controllers.Admin.Dispatch;

[SsalddelApiVersion(SsalddelProductVersion.V3_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/dispatch/food-delivery-ai-review")]
[SsalddelApiContractName("FoodDeliveryDispatchAIReviewController")]
public sealed class 음식배달배차AI검토Controller : ControllerBase
{
    private readonly IFoodDeliveryDispatchAIReviewService _음식배달배차AI검토Service;

    public 음식배달배차AI검토Controller(IFoodDeliveryDispatchAIReviewService 음식배달배차AI검토Service)
    {
        _음식배달배차AI검토Service = 음식배달배차AI검토Service;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<FoodDeliveryDispatchAIReviewWorkspaceDto>> 검토공간조회(CancellationToken cancellationToken)
    {
        return Ok(await _음식배달배차AI검토Service.GetWorkspaceAsync(cancellationToken));
    }

    [HttpPost("decisions")]
    [SsalddelApiContractName("RecordDecision")]
    public async Task<IActionResult> 결정기록(
        [FromBody] FoodDeliveryDispatchAIReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _음식배달배차AI검토Service.RecordDecisionAsync(request, ResolveUserName(), cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    private string? ResolveUserName()
    {
        return User.Identity?.Name
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue(ClaimTypes.Email);
    }
}
