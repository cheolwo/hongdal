using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Driver.Settlement;
using Ssalddel.Contracts.Driver.Settlement;
using 살뜰.도메인.공통;

namespace Ssalddel.Controllers.Driver.Settlement06;

[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[SsalddelApiAudience(SsalddelActor.Driver)]
[SsalddelApiCapability(SsalddelCapability.Settlement)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelApiOperation(SsalddelOperation.Manage)]
[ApiController]
[Authorize(Roles = 역할명.기사)]
[Route("api/v1/driver/settlement-account")]
public sealed class 기사정산계좌Controller : DriverControllerBase
{
    private readonly I기사정산계좌UseCase _useCase;

    public 기사정산계좌Controller(I기사정산계좌UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> 조회(CancellationToken cancellationToken)
    {
        var result = await _useCase.조회Async(현재기사Id(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> 저장(
        [FromBody] 기사정산계좌수정요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.저장Async(현재기사Id(), request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete]
    public async Task<IActionResult> 삭제(CancellationToken cancellationToken)
    {
        var result = await _useCase.삭제Async(현재기사Id(), cancellationToken);
        return this.ToNoContentActionResult(result);
    }
}
