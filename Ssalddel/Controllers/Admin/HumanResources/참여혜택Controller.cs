using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Application.HumanResources;
using Ssalddel.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin.HumanResources;

[SsalddelApiVersion(SsalddelProductVersion.V2_5)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/hr-participation-benefits")]
[SsalddelApiContractName("HrParticipationBenefitsController")]
public sealed class 참여혜택Controller : ControllerBase
{
    private readonly IHR참여운영UseCase _참여혜택UseCase;

    public 참여혜택Controller(IHR참여운영UseCase 참여혜택UseCase)
    {
        _참여혜택UseCase = 참여혜택UseCase;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? userId,
        [FromQuery] string? sourceType,
        CancellationToken cancellationToken)
    {
        var result = await _참여혜택UseCase.참여혜택목록Async(userId, sourceType, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("transfer")]
    [SsalddelApiContractName("Transfer")]
    public async Task<IActionResult> 혜택이전(
        [FromBody] HrParticipationBenefitTransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _참여혜택UseCase.참여혜택전환Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(목록조회), new { userId = result.Value.UserId }, result.Value)
            : this.ToActionResult(result);
    }
}
