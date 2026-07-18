using System.Security.Claims;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Content;
using Hongdal.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Content07;

[HongdalApiVersion(HongdalProductVersion.V0_0)]
[ApiController]
[Route("api/v1/admin/content/information")]
[Authorize(Policy = "서버관리자전용")]
public sealed class CommunityInformationCollectionController : ControllerBase
{
    private readonly ICommunityInformationCollectionService _service;
    private readonly IYouTubeSocialContextWorkspaceService _socialContextWorkspaceService;

    public CommunityInformationCollectionController(
        ICommunityInformationCollectionService service,
        IYouTubeSocialContextWorkspaceService socialContextWorkspaceService)
    {
        _service = service;
        _socialContextWorkspaceService = socialContextWorkspaceService;
    }

    [HttpGet("sources")]
    public ActionResult<IReadOnlyList<CommunityInformationSourceDto>> GetSources()
        => Ok(_service.GetSources());

    [HttpGet("candidates")]
    public async Task<ActionResult<CommunityInformationCollectionResponse>> GetCandidates(
        [FromQuery] CommunityInformationCollectionQuery query,
        CancellationToken cancellationToken)
        => Ok(await _service.ReadAsync(query, cancellationToken));

    [HttpGet("social-media/sources")]
    public ActionResult<IReadOnlyList<SocialMediaResearchSourceDto>> GetSocialMediaSources()
        => Ok(_socialContextWorkspaceService.GetSources());

    [HttpPost("youtube-social-context/draft")]
    [HttpPost("youtube-social-context/workspaces/research")]
    public async Task<ActionResult<YouTubeSocialContextResearchResponse>> BuildYouTubeSocialContextDraft(
        [FromBody] YouTubeSocialContextResearchRequest request,
        CancellationToken cancellationToken)
        => Ok(await _socialContextWorkspaceService.ResearchAndSaveAsync(
            request,
            CurrentUserId(),
            CurrentDisplayName(),
            cancellationToken));

    [HttpGet("youtube-social-context/workspaces")]
    public async Task<ActionResult<IReadOnlyList<YouTubeSocialContextWorkspaceSummaryDto>>> GetYouTubeSocialContextWorkspaces(
        [FromQuery] string? status,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _socialContextWorkspaceService.ListAsync(status, take, cancellationToken));

    [HttpGet("youtube-social-context/workspaces/by-video/{videoId}")]
    public async Task<ActionResult<YouTubeSocialContextWorkspaceDto>> GetYouTubeSocialContextWorkspaceByVideo(
        string videoId,
        CancellationToken cancellationToken)
    {
        var workspace = await _socialContextWorkspaceService.GetByVideoIdAsync(videoId, cancellationToken);
        return workspace is null ? NotFound() : Ok(workspace);
    }

    [HttpGet("youtube-social-context/workspaces/{workspaceId}")]
    public async Task<ActionResult<YouTubeSocialContextWorkspaceDto>> GetYouTubeSocialContextWorkspace(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var workspace = await _socialContextWorkspaceService.GetAsync(workspaceId, cancellationToken);
        return workspace is null ? NotFound() : Ok(workspace);
    }

    [HttpPut("youtube-social-context/workspaces/{workspaceId}/draft")]
    public async Task<ActionResult<YouTubeSocialContextWorkspaceDto>> UpdateYouTubeSocialContextWorkspaceDraft(
        string workspaceId,
        [FromBody] YouTubeSocialContextWorkspaceDraftUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _socialContextWorkspaceService.UpdateDraftAsync(
                workspaceId,
                request,
                CurrentUserId(),
                CurrentDisplayName(),
                cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(CreateProblem(404, exception.Message));
        }
        catch (YouTubeSocialContextWorkspaceConcurrencyException exception)
        {
            return Conflict(CreateProblem(409, exception.Message));
        }
    }

    [HttpPost("youtube-social-context/workspaces/{workspaceId}/publication-links")]
    public async Task<ActionResult<YouTubeSocialContextWorkspaceDto>> LinkYouTubeSocialContextPublication(
        string workspaceId,
        [FromBody] YouTubeSocialContextPublicationLinkRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _socialContextWorkspaceService.LinkPublicationAsync(
                workspaceId,
                request,
                CurrentUserId(),
                CurrentDisplayName(),
                cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(CreateProblem(404, exception.Message));
        }
        catch (YouTubeSocialContextWorkspaceConcurrencyException exception)
        {
            return Conflict(CreateProblem(409, exception.Message));
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? User.Identity?.Name
           ?? "server-admin";

    private string CurrentDisplayName()
        => User.FindFirstValue("name")
           ?? User.Identity?.Name
           ?? "홍달 운영자";

    private static ProblemDetails CreateProblem(int status, string detail)
        => new()
        {
            Status = status,
            Title = status == 409 ? "작업공간 변경 충돌" : "작업공간을 찾을 수 없음",
            Detail = detail
        };
}
