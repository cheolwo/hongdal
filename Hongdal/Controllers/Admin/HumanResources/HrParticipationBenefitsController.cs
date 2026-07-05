using Hongdal.Contracts.Common.Hr;
using Hongdal.Services.HumanResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.HumanResources;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/hr-participation-benefits")]
public sealed class HrParticipationBenefitsController : ControllerBase
{
    private readonly IHrParticipationBenefitRecordService _recordService;

    public HrParticipationBenefitsController(IHrParticipationBenefitRecordService recordService)
    {
        _recordService = recordService;
    }

    [HttpGet]
    public async Task<ActionResult<HrParticipationBenefitRecordListResponse>> List(
        [FromQuery] string? userId,
        [FromQuery] string? sourceType,
        CancellationToken cancellationToken)
    {
        var items = await _recordService.ListAsync(userId, sourceType, cancellationToken);
        return Ok(new HrParticipationBenefitRecordListResponse { Items = items });
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<HrParticipationBenefitRecordResponse>> Transfer(
        [FromBody] HrParticipationBenefitTransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var record = await _recordService.TransferAsync(request, cancellationToken);
            return CreatedAtAction(nameof(List), new { userId = record.UserId }, record);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
