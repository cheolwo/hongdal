using System.Security.Claims;
using FluentResults;
using Ssalddel.Application.Evidence;
using Ssalddel.Controllers;
using 살뜰.Services.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin.Evidence04;

[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/documents")]
public sealed class 문서관리Controller : ControllerBase
{
    private readonly I문서관리UseCase _useCase;

    public 문서관리Controller(I문서관리UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("policies")]
    public async Task<IActionResult> 정책목록조회(CancellationToken cancellationToken)
    {
        var result = await _useCase.정책목록조회Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("policies/{documentCode}")]
    public async Task<IActionResult> 정책수정(string documentCode, [FromBody] 문서정책수정요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.정책수정Async(documentCode, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> 목록조회([FromQuery] string? documentCode, [FromQuery] string? requestId, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await _useCase.목록조회Async(documentCode, requestId, status, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("logs")]
    public async Task<IActionResult> 로그목록조회([FromQuery] long? documentId, CancellationToken cancellationToken)
    {
        var result = await _useCase.로그목록조회Async(documentId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> 업로드([FromForm] 문서업로드요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.업로드Async(new 문서업로드Command(
            request?.File,
            request?.의뢰Id,
            request?.운송원장Id,
            request?.문서코드,
            request?.문서명,
            request?.암호화여부,
            request?.다운로드허용여부,
            User.Identity?.Name), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(문서다운로드), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("{id:long}/download")]
    public async Task<IActionResult> 문서다운로드(long id, CancellationToken cancellationToken)
    {
        var result = await _useCase.다운로드Async(id, new 문서다운로드Context(
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            User.Identity?.Name ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Request.Path.Value ?? string.Empty,
            HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Request.Headers.UserAgent.ToString()), cancellationToken);

        return result.IsSuccess
            ? File(result.Value.내용, result.Value.ContentType, result.Value.파일명)
            : this.ToActionResult(result);
    }
}
