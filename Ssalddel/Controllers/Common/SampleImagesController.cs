using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers;
using Ssalddel.Application.Images;
using 살뜰.Services.Images;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[ApiController]
[Authorize(Policy = "운영사용자전용")]
[Route("api/v1/sample-images")]
public sealed class SampleImagesController : ControllerBase
{
    private readonly I샘플이미지작업UseCase _useCase;

    public SampleImagesController(I샘플이미지작업UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> 작업목록(
        [FromQuery] string? 대상타입,
        [FromQuery] string? 이미지용도,
        [FromQuery] string? 상태,
        [FromQuery] bool? 샘플데이터여부,
        [FromQuery] string? 대상식별자,
        [FromQuery] int 최대건수 = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _useCase.작업목록Async(new 샘플이미지작업조회조건
        {
            대상타입 = 대상타입,
            이미지용도 = 이미지용도,
            상태 = 상태,
            샘플데이터여부 = 샘플데이터여부,
            대상식별자 = 대상식별자,
            최대건수 = 최대건수
        }, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("generate-missing")]
    public async Task<IActionResult> 누락이미지생성([FromBody] 누락샘플이미지생성요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.누락이미지생성Async(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{jobId:long}/retry")]
    public async Task<IActionResult> 작업재시도(long jobId, CancellationToken cancellationToken)
    {
        var result = await _useCase.작업재시도Async(jobId, cancellationToken);
        return this.ToActionResult(result);
    }
}
