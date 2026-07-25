using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Ssalddel.Security;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Api,
    "게시글 조회·발행·음성·번역·원장 문맥 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "공개 읽기와 게시글 작성자 명령만 연결하며 첨부·댓글 참여·운영 심의는 별도 Controller가 처리합니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/posts")]
public sealed class 커뮤니티게시글Controller : CommunityControllerBase
{
    private readonly I커뮤니티게시글조회UseCase _readUseCase;
    private readonly I커뮤니티게시글조회수기록UseCase _viewCountUseCase;
    private readonly I커뮤니티게시글발행UseCase _publishingUseCase;
    private readonly I커뮤니티게시글음성조회Service _audioService;
    private readonly ICommunityPostTranslationService _translationService;
    private readonly I게시글원장선택조회Service _ledgerSelectionService;
    private readonly I게시글원장표시ContextService _ledgerContextService;

    public 커뮤니티게시글Controller(
        I커뮤니티게시글조회UseCase readUseCase,
        I커뮤니티게시글조회수기록UseCase viewCountUseCase,
        I커뮤니티게시글발행UseCase publishingUseCase,
        I커뮤니티게시글음성조회Service audioService,
        ICommunityPostTranslationService translationService,
        I게시글원장선택조회Service ledgerSelectionService,
        I게시글원장표시ContextService ledgerContextService)
    {
        _readUseCase = readUseCase;
        _viewCountUseCase = viewCountUseCase;
        _publishingUseCase = publishingUseCase;
        _audioService = audioService;
        _translationService = translationService;
        _ledgerSelectionService = ledgerSelectionService;
        _ledgerContextService = ledgerContextService;
    }

    [HttpGet]
    [AllowAnonymous]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? appKey,
        [FromQuery] string? category,
        [FromQuery] string? boardKey,
        [FromQuery] string? workflowTag,
        [FromQuery] string? roleTag,
        [FromQuery] string? periodicVisibility,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _readUseCase.목록Async(
            appKey,
            category,
            boardKey,
            workflowTag,
            roleTag,
            page,
            pageSize,
            cancellationToken,
            periodicVisibility);
        return this.ToActionResult(result);
    }

    [HttpGet("board-summaries")]
    [AllowAnonymous]
    [SsalddelApiContractName("ListBoardSummaries")]
    public async Task<IActionResult> 게시판요약목록조회(
        [FromQuery] string? appKey,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _readUseCase.게시판요약목록Async(appKey, cancellationToken));

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("Create")]
    public async Task<IActionResult> 생성(
        [FromBody] PlatformCommunityPostCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _publishingUseCase.생성Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(상세조회), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("my-ledgers")]
    [Authorize]
    [SsalddelApiContractName("ListMyLedgers")]
    public async Task<IActionResult> 내원장목록조회(
        [FromQuery] string? workflowTag,
        CancellationToken cancellationToken)
        => Ok(await _ledgerSelectionService.연결가능원장목록조회Async(
            CurrentUserId(),
            workflowTag,
            cancellationToken));

    [HttpGet("shared-ledgers")]
    [AllowAnonymous]
    [SsalddelApiContractName("ListSharedLedgers")]
    public async Task<IActionResult> 공유원장목록조회(
        [FromQuery] string? workflowTag,
        CancellationToken cancellationToken)
        => Ok(await _ledgerSelectionService.공유원장목록조회Async(
            CurrentUserId(),
            workflowTag,
            cancellationToken));

    [HttpGet("ledgers/{ledgerId}/context")]
    [AllowAnonymous]
    [SsalddelApiContractName("GetLedgerContext")]
    public async Task<IActionResult> 원장문맥조회(
        string ledgerId,
        CancellationToken cancellationToken)
    {
        var context = await _ledgerContextService.조회Async(
            ledgerId,
            CurrentUserId(),
            cancellationToken);
        return context is null ? NotFound() : Ok(context);
    }

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 상세조회(long id, CancellationToken cancellationToken)
    {
        await _viewCountUseCase.조회기록Async(id, cancellationToken);
        return this.ToActionResult(await _readUseCase.상세Async(id, cancellationToken));
    }

    [HttpPost("{id:long}/translations/{targetLanguageCode}")]
    [AllowAnonymous]
    [SsalddelApiContractName("Translate")]
    public async Task<IActionResult> 번역(
        long id,
        string targetLanguageCode,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _translationService.GetOrCreateAsync(
            id,
            targetLanguageCode,
            cancellationToken));

    [HttpGet("{id:long}/audio")]
    [AllowAnonymous]
    [SsalddelApiContractName("GetAudio")]
    public async Task<IActionResult> 음성조회(long id, CancellationToken cancellationToken)
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
    [SsalddelApiContractName("DownloadAudio")]
    public async Task<IActionResult> 음성다운로드(
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

    [HttpPut("{id:long}")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("Update")]
    public async Task<IActionResult> 수정(
        long id,
        [FromBody] PlatformCommunityPostUpdateRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _publishingUseCase.수정Async(id, request, cancellationToken));

    [HttpDelete("{id:long}")]
    [AllowAnonymous]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("Delete")]
    public async Task<IActionResult> 삭제(
        long id,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
        => this.ToNoContentActionResult(await _publishingUseCase.삭제Async(
            id,
            request,
            cancellationToken));

    private string? CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub");
}
