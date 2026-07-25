using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Api,
    "공개 게시판 조회와 인증된 개설 신청·운영 검토 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "공개 조회와 관리자 검토 endpoint의 인증 경계를 분리합니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/boards")]
public sealed class 커뮤니티게시판Controller : CommunityControllerBase
{
    private readonly I커뮤니티게시판UseCase _useCase;

    public 커뮤니티게시판Controller(I커뮤니티게시판UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    [AllowAnonymous]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? appKey,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.목록Async(
            appKey,
            PlatformCommunityBoardRequestStatuses.Approved,
            includeReviewDetails: false,
            cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("requests")]
    [Authorize(Policy = "서버관리자전용")]
    [SsalddelApiContractName("ListRequests")]
    public async Task<IActionResult> 요청목록조회(
        [FromQuery] string? appKey,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.목록Async(
            appKey,
            status ?? PlatformCommunityBoardRequestStatuses.Pending,
            includeReviewDetails: true,
            cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize]
    [SsalddelApiContractName("Create")]
    public async Task<IActionResult> 생성(
        [FromBody] PlatformCommunityBoardCreateRequest request,
        CancellationToken cancellationToken)
    {
        var requesterUserId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(requesterUserId))
        {
            return Unauthorized();
        }

        var result = await _useCase.신청Async(
            request,
            requesterUserId,
            CurrentUserDisplayName(),
            cancellationToken);
        if (result.IsFailed)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(목록조회), new { appKey = result.Value.AppKey }, result.Value);
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = "서버관리자전용")]
    [SsalddelApiContractName("Approve")]
    public async Task<IActionResult> 승인(
        long id,
        [FromBody] PlatformCommunityBoardReviewRequest request,
        CancellationToken cancellationToken)
    {
        var reviewerUserId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(reviewerUserId))
        {
            return Unauthorized();
        }

        var result = await _useCase.승인Async(id, request, reviewerUserId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:long}/reject")]
    [Authorize(Policy = "서버관리자전용")]
    [SsalddelApiContractName("Reject")]
    public async Task<IActionResult> 거절(
        long id,
        [FromBody] PlatformCommunityBoardReviewRequest request,
        CancellationToken cancellationToken)
    {
        var reviewerUserId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(reviewerUserId))
        {
            return Unauthorized();
        }

        var result = await _useCase.반려Async(id, request, reviewerUserId, cancellationToken);
        return this.ToActionResult(result);
    }

    private string? CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub");

    private string CurrentUserDisplayName()
        => User.FindFirstValue("name")
           ?? User.Identity?.Name
           ?? "회원";
}
