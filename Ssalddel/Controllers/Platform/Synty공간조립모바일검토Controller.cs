using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.WorldProjection;

namespace Ssalddel.Controllers.Platform;

[SsalddelApiVersion(SsalddelProductVersion.V3_5)]
[SsalddelApiAudience(SsalddelActor.PlatformOperator)]
[SsalddelApiContractName("SyntyCompositionMobileReviewController")]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route(Synty공간조립모바일검토Routes.Base)]
public sealed class Synty공간조립모바일검토Controller(
    ISynty공간조립모바일검토Service service,
    ISynty공간조립검토촬영업로드Service captureUploadService) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("GetSyntyCompositionMobileReviewInbox")]
    public async Task<ActionResult<Synty공간조립검토함Response>> 검토함조회(
        [FromQuery] string? batchStableId,
        [FromQuery] string? reviewStateCode,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await service.검토함조회Async(
                batchStableId,
                reviewStateCode,
                take,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(400, exception.Message));
        }
    }

    [HttpPost("capture-uploads")]
    [RequestSizeLimit(Synty공간조립검토촬영업로드Service.MaximumPngBytes + 256_000)]
    [SsalddelApiContractName("UploadSyntyCompositionReviewCapture")]
    public async Task<ActionResult<Synty공간조립검토촬영업로드Response>> 촬영업로드(
        [FromForm] Synty공간조립검토촬영업로드Request request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await captureUploadService.업로드Async(
                new Synty공간조립검토촬영업로드Command(
                    request?.File,
                    request?.BatchStableId,
                    request?.ReviewItemStableId,
                    request?.CaptureStableId,
                    request?.ViewCode,
                    request?.CaptureBundleHash,
                    request?.ParentCaptureBundleHash,
                    request?.SourceCompositionHash,
                    request?.ExpectedReviewItemRevision ?? -1,
                    request?.RenderingProfileHash,
                    request?.ImageSha256,
                    request?.Width ?? 0,
                    request?.Height ?? 0),
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(400, exception.Message));
        }
    }

    [HttpPost("batches")]
    [SsalddelApiContractName("RegisterSyntyCompositionReviewBatch")]
    public async Task<ActionResult<Synty공간조립검토Batch등록Response>> 촬영Batch등록(
        [FromBody] Synty공간조립검토Batch등록Request request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.Batch등록Async(request, cancellationToken));
        }
        catch (Synty공간조립검토ConcurrencyException exception)
        {
            return Conflict(CreateProblem(409, exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(400, exception.Message));
        }
    }

    [HttpPost("items/{reviewItemStableId}/decisions")]
    [SsalddelApiContractName("RecordSyntyCompositionMobileReviewDecision")]
    public async Task<ActionResult<Synty공간조립검토항목Dto>> 검토결정기록(
        string reviewItemStableId,
        [FromBody] Synty공간조립검토결정Request request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.결정기록Async(
                reviewItemStableId,
                request,
                CurrentUserId(),
                CurrentDisplayName(),
                cancellationToken));
        }
        catch (Synty공간조립검토ConcurrencyException exception)
        {
            return Conflict(CreateProblem(409, exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(CreateProblem(404, exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(400, exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return UnprocessableEntity(CreateProblem(422, exception.Message));
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");

    private string CurrentDisplayName()
        => User.FindFirstValue("name")
           ?? User.Identity?.Name
           ?? "살뜰 공간 검토자";

    private static ProblemDetails CreateProblem(int status, string detail)
        => new()
        {
            Status = status,
            Title = status switch
            {
                400 => "공간 조립 검토 요청을 확인해 주세요",
                404 => "검토할 공간 조립물을 찾을 수 없음",
                409 => "공간 조립 검토 원장 변경 충돌",
                422 => "현재 상태에서는 검토할 수 없음",
                _ => "공간 조립 검토 요청 실패"
            },
            Detail = detail
        };
}

public sealed class Synty공간조립검토촬영업로드Request
{
    public IFormFile? File { get; set; }
    public string BatchStableId { get; set; } = string.Empty;
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string CaptureStableId { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string CaptureBundleHash { get; set; } = string.Empty;
    public string ParentCaptureBundleHash { get; set; } = string.Empty;
    public string SourceCompositionHash { get; set; } = string.Empty;
    public long ExpectedReviewItemRevision { get; set; }
    public string RenderingProfileHash { get; set; } = string.Empty;
    public string ImageSha256 { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}
