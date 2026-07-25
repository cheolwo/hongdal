using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Admin.Dispatch;
using Ssalddel.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Dispatch.Coordination;

namespace Ssalddel.Controllers.Admin.Dispatch;

[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/dispatch/domestic-cargo-ai-review")]
[SsalddelApiContractName("DomesticCargoDispatchAIReviewController")]
public sealed class 국내화물배차AI검토Controller : ControllerBase
{
    private readonly IDomesticCargoDispatchAIReviewService _국내화물배차AI검토Service;

    public 국내화물배차AI검토Controller(IDomesticCargoDispatchAIReviewService 국내화물배차AI검토Service)
    {
        _국내화물배차AI검토Service = 국내화물배차AI검토Service;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<DomesticCargoDispatchAIReviewWorkspaceDto>> 검토공간조회(CancellationToken cancellationToken)
    {
        return Ok(await _국내화물배차AI검토Service.GetWorkspaceAsync(cancellationToken));
    }

    [HttpPost("decisions")]
    [SsalddelApiContractName("RecordDecision")]
    public async Task<IActionResult> 결정기록(
        [FromBody] DomesticCargoDispatchAIReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _국내화물배차AI검토Service.RecordDecisionAsync(request, ResolveUserName(), cancellationToken));
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
