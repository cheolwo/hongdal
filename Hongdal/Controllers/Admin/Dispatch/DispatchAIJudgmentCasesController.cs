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
[Route("api/v1/admin/dispatch/ai-judgment-cases")]
public sealed class DispatchAIJudgmentCasesController : ControllerBase
{
    private readonly I배차AI판단사례LedgerStore _store;

    public DispatchAIJudgmentCasesController(I배차AI판단사례LedgerStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<DispatchAIJudgmentCaseCatalogDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _store.GetCatalogAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] DispatchAIJudgmentCaseCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _store.CreateAsync(request, ResolveUserName(), cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("suggestions/{suggestionKey}/promote")]
    public async Task<IActionResult> PromoteSuggestion(
        string suggestionKey,
        [FromBody] DispatchAIJudgmentCasePromoteSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _store.PromoteSuggestionAsync(suggestionKey, request, ResolveUserName(), cancellationToken));
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
