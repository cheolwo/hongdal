using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommonContents;
using Ssalddel.Contracts.CommonContents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Admin.Content07;

[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[ApiController]
[Route("api/v1/admin/common-contents")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 공통콘텐츠관리Controller : ControllerBase
{
    private readonly I공통콘텐츠관리UseCase _useCase;

    public 공통콘텐츠관리Controller(I공통콘텐츠관리UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> 목록조회(CancellationToken cancellationToken)
    {
        var result = await _useCase.목록조회Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> 상세조회(long id, CancellationToken cancellationToken)
    {
        var result = await _useCase.상세조회Async(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> 등록([FromBody] 관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.등록Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(상세조회), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> 수정(long id, [FromBody] 관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.수정Async(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:long}/active")]
    public async Task<IActionResult> 활성화변경(long id, [FromQuery] bool enabled, CancellationToken cancellationToken)
    {
        var result = await _useCase.활성화변경Async(id, enabled, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpGet("reward-policies")]
    public async Task<IActionResult> 보상정책목록(CancellationToken cancellationToken)
    {
        var result = await _useCase.보상정책목록Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("reward-policies")]
    public async Task<IActionResult> 보상정책등록([FromBody] 공통콘텐츠보상정책Dto request, CancellationToken cancellationToken)
    {
        var result = await _useCase.보상정책등록Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(보상정책목록), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }
}
