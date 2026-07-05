using System.Security.Claims;
using 홍달.Services.Audit;
using 홍달.Services.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Evidence04;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/documents")]
public sealed class 문서관리Controller : ControllerBase
{
    private readonly I문서관리Service _documentService;
    private readonly I사용자행위로그Service _activityLogService;

    public 문서관리Controller(I문서관리Service documentService, I사용자행위로그Service activityLogService)
    {
        _documentService = documentService;
        _activityLogService = activityLogService;
    }

    [HttpGet("policies")]
    public async Task<ActionResult<IReadOnlyList<문서정책요약응답>>> 정책목록조회(CancellationToken cancellationToken)
    {
        return Ok(await _documentService.GetPoliciesAsync(cancellationToken));
    }

    [HttpPut("policies/{documentCode}")]
    public async Task<ActionResult<문서정책요약응답>> 정책수정(string documentCode, [FromBody] 문서정책수정요청 request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentCode))
        {
            return BadRequest("documentCode is required");
        }

        var updated = await _documentService.UpdatePolicyAsync(documentCode.Trim(), request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<문서조회요약응답>>> 목록조회([FromQuery] string? documentCode, [FromQuery] string? requestId, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        return Ok(await _documentService.ListDocumentsAsync(documentCode, requestId, status, cancellationToken));
    }

    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<문서조회로그요약응답>>> 로그목록조회([FromQuery] long? documentId, CancellationToken cancellationToken)
    {
        return Ok(await _documentService.ListLogsAsync(documentId, cancellationToken));
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<문서조회요약응답>> 업로드([FromForm] 문서업로드요청 request, CancellationToken cancellationToken)
    {
        if (request?.File is null || request.File.Length <= 0)
        {
            return BadRequest("file is required");
        }

        await using var stream = request.File.OpenReadStream();
        var created = await _documentService.CreateDocumentAsync(new 문서생성요청
        {
            의뢰Id = request.의뢰Id,
            배송운송Id = request.배송운송Id,
            문서코드 = request.문서코드,
            문서명 = request.문서명,
            파일명 = request.File.FileName,
            ContentType = request.File.ContentType,
            암호화여부 = request.암호화여부,
            다운로드허용여부 = request.다운로드허용여부,
            생성자 = User.Identity?.Name
        }, stream, cancellationToken);

        return CreatedAtAction(nameof(문서다운로드), new { id = created!.Id }, created);
    }

    [HttpGet("{id:long}/download")]
    public async Task<IActionResult> 문서다운로드(long id, CancellationToken cancellationToken)
    {
        var result = await _documentService.DownloadAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        await _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = Hongdal.Contracts.Common.ViewSettings.App식별자.HongdalAdmin,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            UserName = User.Identity?.Name ?? string.Empty,
            RoleName = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            ActionType = "Document",
            ActionName = "Download",
            Route = Request.Path.Value ?? string.Empty,
            TraceId = HttpContext.TraceIdentifier,
            IsSuccess = true,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString(),
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = $"{{\"documentId\":{id}}}"
        }, cancellationToken);

        return File(result.내용, result.ContentType, result.파일명);
    }
}
