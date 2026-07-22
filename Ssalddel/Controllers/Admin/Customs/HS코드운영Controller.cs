using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Customs;
using Ssalddel.Contracts.Admin.Customs;
using Ssalddel.Contracts.Common.ViewSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Admin.Customs;

[SsalddelApiVersion(SsalddelProductVersion.V1_5)]
[SsalddelApiWorkflow(SsalddelWorkflow.CustomsAndTradeData)]
[ApiController]
[Authorize(Policy = "HsCode운영자전용")]
[Route("api/v1/admin/hs-codes")]
public sealed class HS코드운영Controller : ControllerBase
{
    private readonly IHS코드운영UseCase _useCase;

    public HS코드운영Controller(IHS코드운영UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> 목록(
        [FromQuery] string? query,
        [FromQuery] int? businessCategory,
        [FromQuery] int? tagType,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _useCase.목록Async(query, businessCategory, tagType, includeInactive, page, pageSize, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{entryId:long}/business-category")]
    public async Task<IActionResult> 대분류수정(
        long entryId,
        [FromBody] AdminHsCodeBusinessCategoryUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.대분류수정Async(entryId, request, Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{entryId:long}/risk-tags")]
    public async Task<IActionResult> 태그저장(
        long entryId,
        [FromBody] AdminHsCodeRiskTagUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.태그저장Async(entryId, request, Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("risk-tags/{tagId:long}")]
    public async Task<IActionResult> 태그수정(
        long tagId,
        [FromBody] AdminHsCodeRiskTagUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.태그수정Async(tagId, request, Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    private HS코드운영자Context Context생성()
        => new(
            User.IsInRole(역할명.관세사) && !User.IsInRole(역할명.서버관리자),
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            User.Identity?.Name ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Request.Path.Value ?? string.Empty,
            HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Request.Headers.UserAgent.ToString());
}
