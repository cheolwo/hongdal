using System.Security.Claims;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Admin.Dispatch;
using Hongdal.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Dispatch.Coordination;

namespace Hongdal.Controllers.Admin.Dispatch;

[HongdalApiVersion(HongdalProductVersion.V3_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/dispatch/food-delivery-ai-review")]
public sealed class FoodDeliveryDispatchAIReviewController : ControllerBase
{
    private readonly IFoodDeliveryDispatchAIReviewService _service;

    public FoodDeliveryDispatchAIReviewController(IFoodDeliveryDispatchAIReviewService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<FoodDeliveryDispatchAIReviewWorkspaceDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetWorkspaceAsync(cancellationToken));
    }

    [HttpPost("decisions")]
    public async Task<IActionResult> RecordDecision(
        [FromBody] FoodDeliveryDispatchAIReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.RecordDecisionAsync(request, ResolveUserName(), cancellationToken));
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
