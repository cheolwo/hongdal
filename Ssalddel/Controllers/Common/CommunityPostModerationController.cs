using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ssalddel.Security;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Safety,
    SsalddelModuleKind.Api,
    "게시글 고정과 일반·첨부 댓글 신고·숨김 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.SafetyAndOperations,
    Boundary = "신고 접수는 공개하고 숨김·고정은 서버관리자 정책으로 제한합니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/posts")]
public sealed class 커뮤니티게시글운영Controller : ControllerBase
{
    private readonly I커뮤니티게시글운영UseCase _moderationUseCase;

    public 커뮤니티게시글운영Controller(I커뮤니티게시글운영UseCase moderationUseCase)
    {
        _moderationUseCase = moderationUseCase;
    }

    [HttpPost("{id:long}/operator-pin")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetOperatorPin(
        long id,
        [FromBody] PlatformCommunityPostOperatorPinRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _moderationUseCase.운영자고정Async(
            id,
            request,
            cancellationToken));

    [HttpPost("comments/{commentId:long}/reports")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    public async Task<IActionResult> ReportComment(
        long commentId,
        CancellationToken cancellationToken)
        => this.ToNoContentActionResult(await _moderationUseCase.댓글신고Async(
            commentId,
            cancellationToken));

    [HttpPost("comments/{commentId:long}/operator-hidden")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetCommentOperatorHidden(
        long commentId,
        [FromBody] PlatformCommunityOperatorHiddenRequest request,
        CancellationToken cancellationToken)
        => this.ToNoContentActionResult(await _moderationUseCase.댓글운영자숨김Async(
            commentId,
            request,
            cancellationToken));

    [HttpPost("attachments/comments/{commentId:long}/reports")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    public async Task<IActionResult> ReportAttachmentComment(
        long commentId,
        CancellationToken cancellationToken)
        => this.ToNoContentActionResult(await _moderationUseCase.첨부댓글신고Async(
            commentId,
            cancellationToken));

    [HttpPost("attachments/comments/{commentId:long}/operator-hidden")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetAttachmentCommentOperatorHidden(
        long commentId,
        [FromBody] PlatformCommunityOperatorHiddenRequest request,
        CancellationToken cancellationToken)
        => this.ToNoContentActionResult(await _moderationUseCase.첨부댓글운영자숨김Async(
            commentId,
            request,
            cancellationToken));
}
