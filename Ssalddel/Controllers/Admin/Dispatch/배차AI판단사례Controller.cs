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
[Route("api/v1/admin/dispatch/ai-judgment-cases")]
[SsalddelApiContractName("DispatchAIJudgmentCasesController")]
public sealed class 배차AI판단사례Controller : ControllerBase
{
    private readonly I배차AI판단사례LedgerStore _배차AI판단사례Store;

    public 배차AI판단사례Controller(I배차AI판단사례LedgerStore 배차AI판단사례Store)
    {
        _배차AI판단사례Store = 배차AI판단사례Store;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<DispatchAIJudgmentCaseCatalogDto>> 목록조회(CancellationToken cancellationToken)
    {
        return Ok(await _배차AI판단사례Store.GetCatalogAsync(cancellationToken));
    }

    [HttpPost]
    [SsalddelApiContractName("Create")]
    public async Task<IActionResult> 생성(
        [FromBody] DispatchAIJudgmentCaseCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _배차AI판단사례Store.CreateAsync(request, ResolveUserName(), cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("suggestions/{suggestionKey}/promote")]
    [SsalddelApiContractName("PromoteSuggestion")]
    public async Task<IActionResult> 제안승격(
        string suggestionKey,
        [FromBody] DispatchAIJudgmentCasePromoteSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _배차AI판단사례Store.PromoteSuggestionAsync(suggestionKey, request, ResolveUserName(), cancellationToken));
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
