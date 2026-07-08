using Hongdal.ApiMetadata;
using Hongdal.Application.Audit;
using Hongdal.Contracts.Admin.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/activity-logs")]
public sealed class 사용자행위로그Controller : ControllerBase
{
    private readonly I사용자행위로그조회UseCase _useCase;

    public 사용자행위로그Controller(I사용자행위로그조회UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> 조회([FromQuery] 사용자행위로그검색요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.조회Async(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> 상세(long id, CancellationToken cancellationToken)
    {
        var result = await _useCase.상세Async(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("trace/{traceId}")]
    public async Task<IActionResult> Trace조회(string traceId, CancellationToken cancellationToken)
    {
        var result = await _useCase.Trace조회Async(traceId, cancellationToken);
        return this.ToActionResult(result);
    }
}
