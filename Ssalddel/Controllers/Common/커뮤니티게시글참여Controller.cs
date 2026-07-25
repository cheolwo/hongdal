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
    SsalddelCommunityV0ModuleKeys.Participation,
    SsalddelModuleKind.Api,
    "게시글 추천과 일반·첨부 댓글 참여 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "자발적 추천·댓글과 비밀번호 기반 본인 삭제만 연결하며 운영자 심의를 수행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/posts")]
public sealed class 커뮤니티게시글참여Controller : CommunityControllerBase
{
    private readonly I커뮤니티게시글참여UseCase _participationUseCase;

    public 커뮤니티게시글참여Controller(I커뮤니티게시글참여UseCase participationUseCase)
    {
        _participationUseCase = participationUseCase;
    }

    [HttpPost("{id:long}/recommendations")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("Recommend")]
    public async Task<IActionResult> 추천(
        long id,
        [FromBody] PlatformCommunityPostRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var fallbackKey = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return this.ToActionResult(await _participationUseCase.추천Async(
            id,
            request,
            fallbackKey,
            cancellationToken));
    }

    [HttpGet("{id:long}/comments")]
    [AllowAnonymous]
    [SsalddelApiContractName("ListComments")]
    public async Task<IActionResult> 댓글목록조회(long id, CancellationToken cancellationToken)
        => this.ToActionResult(await _participationUseCase.댓글목록Async(id, cancellationToken));

    [HttpPost("{id:long}/comments")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("CreateComment")]
    public async Task<IActionResult> 댓글생성(
        long id,
        [FromBody] PlatformCommunityPostCommentCreateRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _participationUseCase.댓글작성Async(
            id,
            request,
            cancellationToken));

    [HttpDelete("{id:long}/comments/{commentId:long}")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("DeleteComment")]
    public async Task<IActionResult> 댓글삭제(
        long id,
        long commentId,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
        => this.ToNoContentActionResult(await _participationUseCase.댓글삭제Async(
            id,
            commentId,
            request,
            cancellationToken));

    [HttpGet("attachments/{attachmentId:long}/comments")]
    [AllowAnonymous]
    [SsalddelApiContractName("ListAttachmentComments")]
    public async Task<IActionResult> 첨부댓글목록조회(
        long attachmentId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _participationUseCase.첨부댓글목록Async(
            attachmentId,
            cancellationToken));

    [HttpPost("attachments/{attachmentId:long}/comments")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("CreateAttachmentComment")]
    public async Task<IActionResult> 첨부댓글생성(
        long attachmentId,
        [FromBody] PlatformCommunityPostAttachmentCommentCreateRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _participationUseCase.첨부댓글작성Async(
            attachmentId,
            request,
            cancellationToken));

    [HttpDelete("attachments/{attachmentId:long}/comments/{commentId:long}")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("DeleteAttachmentComment")]
    public async Task<IActionResult> 첨부댓글삭제(
        long attachmentId,
        long commentId,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
        => this.ToNoContentActionResult(await _participationUseCase.첨부댓글삭제Async(
            attachmentId,
            commentId,
            request,
            cancellationToken));
}
