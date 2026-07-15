using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V0_0)]
[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/posts")]
public sealed class 커뮤니티게시글Controller : ControllerBase
{
    private readonly I커뮤니티게시글UseCase _useCase;
    private readonly I커뮤니티게시글음성조회Service _audioService;
    private readonly I게시글원장ContextService _원장ContextService;

    public 커뮤니티게시글Controller(
        I커뮤니티게시글UseCase useCase,
        I커뮤니티게시글음성조회Service audioService,
        I게시글원장ContextService 원장ContextService)
    {
        _useCase = useCase;
        _audioService = audioService;
        _원장ContextService = 원장ContextService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] string? appKey,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _useCase.목록Async(appKey, category, page, pageSize, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(
        [FromBody] PlatformCommunityPostCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.생성Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("my-ledgers")]
    [Authorize]
    public async Task<IActionResult> ListMyLedgers(
        [FromQuery] string? workflowTag,
        CancellationToken cancellationToken)
        => Ok(await _원장ContextService.연결가능원장목록조회Async(
            CurrentUserId(),
            workflowTag,
            cancellationToken));

    [HttpGet("shared-ledgers")]
    [AllowAnonymous]
    public async Task<IActionResult> ListSharedLedgers(
        [FromQuery] string? workflowTag,
        CancellationToken cancellationToken)
        => Ok(await _원장ContextService.공유원장목록조회Async(
            CurrentUserId(),
            workflowTag,
            cancellationToken));

    [HttpGet("ledgers/{ledgerId}/context")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLedgerContext(
        string ledgerId,
        CancellationToken cancellationToken)
    {
        var context = await _원장ContextService.조회Async(
            ledgerId,
            CurrentUserId(),
            cancellationToken);
        return context is null ? NotFound() : Ok(context);
    }

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        var result = await _useCase.상세Async(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:long}/audio")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAudio(long id, CancellationToken cancellationToken)
    {
        var audio = await _audioService.조회Async(
            id,
            CurrentUserId(),
            HttpContext.TraceIdentifier,
            cancellationToken);
        return audio is null ? NotFound() : Ok(audio);
    }

    [HttpGet("{id:long}/audio/segments/{sequence:int}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadAudio(
        long id,
        int sequence,
        CancellationToken cancellationToken)
    {
        var audio = await _audioService.다운로드Async(
            id,
            sequence,
            CurrentUserId(),
            HttpContext.TraceIdentifier,
            cancellationToken);
        return audio is null
            ? NotFound()
            : File(audio.Content, audio.ContentType, audio.FileName, enableRangeProcessing: true);
    }

    [HttpPost("{id:long}/attachments")]
    [AllowAnonymous]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadAttachment(
        long id,
        [FromForm] PlatformCommunityPostAttachmentUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return this.ToProblemActionResult("업로드할 이미지 파일을 선택해야 합니다.");
        }

        await using var stream = request.File.OpenReadStream();
        var command = new 커뮤니티게시글첨부업로드Command(
            request.Password,
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length);
        var result = await _useCase.첨부업로드Async(id, command, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] PlatformCommunityPostUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.수정Async(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:long}/operator-pin")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetOperatorPin(
        long id,
        [FromBody] PlatformCommunityPostOperatorPinRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.운영자고정Async(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:long}/recommendations")]
    [AllowAnonymous]
    public async Task<IActionResult> Recommend(
        long id,
        [FromBody] PlatformCommunityPostRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var fallbackKey = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        var result = await _useCase.추천Async(id, request, fallbackKey, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:long}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> ListComments(long id, CancellationToken cancellationToken)
    {
        var result = await _useCase.댓글목록Async(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:long}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateComment(
        long id,
        [FromBody] PlatformCommunityPostCommentCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.댓글작성Async(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:long}/comments/{commentId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteComment(
        long id,
        long commentId,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.댓글삭제Async(id, commentId, request, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpPost("comments/{commentId:long}/reports")]
    [AllowAnonymous]
    public async Task<IActionResult> ReportComment(long commentId, CancellationToken cancellationToken)
    {
        var result = await _useCase.댓글신고Async(commentId, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpPost("comments/{commentId:long}/operator-hidden")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetCommentOperatorHidden(
        long commentId,
        [FromBody] PlatformCommunityOperatorHiddenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.댓글운영자숨김Async(commentId, request, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpGet("attachments/{attachmentId:long}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> ListAttachmentComments(
        long attachmentId,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.첨부댓글목록Async(attachmentId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("attachments/{attachmentId:long}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAttachmentComment(
        long attachmentId,
        [FromBody] PlatformCommunityPostAttachmentCommentCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.첨부댓글작성Async(attachmentId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("attachments/{attachmentId:long}/comments/{commentId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteAttachmentComment(
        long attachmentId,
        long commentId,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.첨부댓글삭제Async(attachmentId, commentId, request, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpPost("attachments/comments/{commentId:long}/reports")]
    [AllowAnonymous]
    public async Task<IActionResult> ReportAttachmentComment(long commentId, CancellationToken cancellationToken)
    {
        var result = await _useCase.첨부댓글신고Async(commentId, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpPost("attachments/comments/{commentId:long}/operator-hidden")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetAttachmentCommentOperatorHidden(
        long commentId,
        [FromBody] PlatformCommunityOperatorHiddenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.첨부댓글운영자숨김Async(commentId, request, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(
        long id,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.삭제Async(id, request, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    private string? CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub");
}

public sealed class PlatformCommunityPostAttachmentUploadRequest
{
    public string Password { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}
