using System.Security.Claims;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.AgriculturalFisheries.ImportReadiness;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[ApiController]
[HongdalApiVersion(HongdalProductVersion.V0_0)]
[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[Route("api/v1/community/posts/{postId:long}/opportunities")]
public sealed class CommunityPostOpportunitiesController : ControllerBase
{
    private readonly ICommunityPostOpportunityService _service;

    public CommunityPostOpportunitiesController(ICommunityPostOpportunityService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<CommunityPostOpportunityListResponse>> Get(
        long postId,
        [FromQuery] string? displayLanguage,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(postId, displayLanguage, cancellationToken);
        return result is null
            ? NotFoundProblem("커뮤니티 게시글을 찾을 수 없습니다.")
            : Ok(result);
    }

    [HttpPost("meat-import-readiness/start")]
    [Authorize]
    public async Task<ActionResult<StartCommunityMeatImportReadinessResponse>> StartMeatImportReadiness(
        long postId,
        [FromBody] StartCommunityMeatImportReadinessRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.StartMeatImportReadinessAsync(
                postId,
                request,
                CurrentUserId(),
                CurrentDisplayName(),
                cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundProblem(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "이 게시글에서 정보 협업을 시작할 권한이 없습니다.",
                detail: ex.Message);
        }
        catch (CommunityPostOpportunityConflictException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "게시글 원장 연결이 충돌했습니다.",
                detail: ex.Message);
        }
        catch (MeatImportReadinessConcurrencyException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "육류 수입 준비 정보가 이미 변경되었습니다.",
                detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "정보 협업 시작 요청이 올바르지 않습니다.",
                detail: ex.Message);
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? string.Empty;

    private string CurrentDisplayName()
        => User.Identity?.Name
           ?? User.FindFirstValue("name")
           ?? "참여자";

    private ObjectResult NotFoundProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "커뮤니티 게시글을 찾을 수 없습니다.",
            detail: detail);
}
