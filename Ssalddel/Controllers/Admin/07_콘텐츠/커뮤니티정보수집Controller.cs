using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Community;
using Ssalddel.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Admin.Content07;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.Api,
    "운영자 자료 수집·SNS 조사·AI 초안 작업공간 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "서버관리자만 사용하며 외부 자료와 AI 초안은 검토 전 자동 게시하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/information")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("CommunityInformationCollectionController")]
public sealed class 커뮤니티정보수집Controller : ControllerBase
{
    private readonly ICommunityInformationCollectionService _커뮤니티정보수집Service;
    private readonly IYouTubeSocialContextWorkspaceService _socialContextWorkspaceService;
    private readonly ICommunityAuthoringAiDraftService _aiDraftService;

    public 커뮤니티정보수집Controller(
        ICommunityInformationCollectionService 커뮤니티정보수집Service,
        IYouTubeSocialContextWorkspaceService socialContextWorkspaceService,
        ICommunityAuthoringAiDraftService aiDraftService)
    {
        _커뮤니티정보수집Service = 커뮤니티정보수집Service;
        _socialContextWorkspaceService = socialContextWorkspaceService;
        _aiDraftService = aiDraftService;
    }

    [HttpGet("sources")]
    [SsalddelApiContractName("GetSources")]
    public ActionResult<IReadOnlyList<CommunityInformationSourceDto>> 정보출처목록조회()
        => Ok(_커뮤니티정보수집Service.GetSources());

    [HttpGet("board-relations")]
    [SsalddelApiContractName("GetBoardRelations")]
    public ActionResult<IReadOnlyList<CommunityBoardInformationRelation>> 게시판연결목록조회(
        [FromQuery] string? boardKey)
    {
        if (string.IsNullOrWhiteSpace(boardKey))
        {
            return Ok(CommunityBoardInformationRelationCatalog.All);
        }

        var relation = CommunityBoardInformationRelationCatalog.Find(boardKey);
        return relation is null
            ? NotFound(CreateProblem(404, $"게시판 정보 관계를 찾을 수 없습니다. BoardKey={boardKey}"))
            : Ok(new[] { relation });
    }

    [HttpGet("board-relations/batch-plans")]
    [SsalddelApiContractName("GetBoardRelationBatchPlans")]
    public ActionResult<IReadOnlyList<CommunityBoardInformationBatchPlan>>
        게시판연결일괄계획조회()
        => Ok(CommunityBoardInformationRelationCatalog.PeriodicBatchPlans());

    [HttpGet("candidates")]
    [SsalddelApiContractName("GetCandidates")]
    public async Task<ActionResult<CommunityInformationCollectionResponse>> 정보후보목록조회(
        [FromQuery] CommunityInformationCollectionQuery query,
        CancellationToken cancellationToken)
        => Ok(await _커뮤니티정보수집Service.ReadAsync(query, cancellationToken));

    [HttpPost("authoring/ai-drafts")]
    [SsalddelApiContractName("GenerateAiDraft")]
    public async Task<ActionResult<CommunityAuthoringAiDraftResponse>> AI초안생성(
        [FromBody] CommunityAuthoringAiDraftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _aiDraftService.GenerateAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(400, exception.Message));
        }
    }

    [HttpGet("social-media/sources")]
    [SsalddelApiContractName("GetSocialMediaSources")]
    public ActionResult<IReadOnlyList<SocialMediaResearchSourceDto>> SocialMedia출처목록조회()
        => Ok(_socialContextWorkspaceService.GetSources());

    [HttpPost("youtube-social-context/draft")]
    [HttpPost("youtube-social-context/workspaces/research")]
    [SsalddelApiContractName("BuildYouTubeSocialContextDraft")]
    public async Task<ActionResult<YouTubeSocialContextResearchResponse>> YouTube사회문맥초안생성(
        [FromBody] YouTubeSocialContextResearchRequest request,
        CancellationToken cancellationToken)
        => Ok(await _socialContextWorkspaceService.ResearchAndSaveAsync(
            request,
            CurrentUserId(),
            CurrentDisplayName(),
            cancellationToken));

    [HttpGet("youtube-social-context/workspaces")]
    [SsalddelApiContractName("GetYouTubeSocialContextWorkspaces")]
    public async Task<ActionResult<IReadOnlyList<YouTubeSocialContextWorkspaceSummaryDto>>> YouTube사회문맥작업공간목록조회(
        [FromQuery] string? status,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _socialContextWorkspaceService.ListAsync(status, take, cancellationToken));

    [HttpGet("youtube-social-context/workspaces/by-video/{videoId}")]
    [SsalddelApiContractName("GetYouTubeSocialContextWorkspaceByVideo")]
    public async Task<ActionResult<YouTubeSocialContextWorkspaceDto>> YouTube영상별사회문맥작업공간조회(
        string videoId,
        CancellationToken cancellationToken)
    {
        var workspace = await _socialContextWorkspaceService.GetByVideoIdAsync(videoId, cancellationToken);
        return workspace is null ? NotFound() : Ok(workspace);
    }

    [HttpGet("youtube-social-context/workspaces/{workspaceId}")]
    [SsalddelApiContractName("GetYouTubeSocialContextWorkspace")]
    public async Task<ActionResult<YouTubeSocialContextWorkspaceDto>> YouTube사회문맥작업공간조회(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var workspace = await _socialContextWorkspaceService.GetAsync(workspaceId, cancellationToken);
        return workspace is null ? NotFound() : Ok(workspace);
    }

    [HttpPut("youtube-social-context/workspaces/{workspaceId}/draft")]
    [SsalddelApiContractName("UpdateYouTubeSocialContextWorkspaceDraft")]
    public async Task<ActionResult<YouTubeSocialContextWorkspaceDto>> YouTube사회문맥작업공간초안수정(
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
    [SsalddelApiContractName("LinkYouTubeSocialContextPublication")]
    public async Task<ActionResult<YouTubeSocialContextWorkspaceDto>> YouTube사회문맥게시연결(
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
           ?? "살뜰 운영자";

    private static ProblemDetails CreateProblem(int status, string detail)
        => new()
        {
            Status = status,
            Title = status switch
            {
                400 => "글쓰기 요청을 확인해 주세요",
                409 => "작업공간 변경 충돌",
                _ => "작업공간을 찾을 수 없음"
            },
            Detail = detail
        };
}
