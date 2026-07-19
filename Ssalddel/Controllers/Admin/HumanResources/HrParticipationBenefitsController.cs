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
public sealed class HrParticipationBenefitsController : ControllerBase
{
    private readonly IHR참여운영UseCase _useCase;

    public HrParticipationBenefitsController(IHR참여운영UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? userId,
        [FromQuery] string? sourceType,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.참여혜택목록Async(userId, sourceType, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
        [FromBody] HrParticipationBenefitTransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.참여혜택전환Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(List), new { userId = result.Value.UserId }, result.Value)
            : this.ToActionResult(result);
    }
}
