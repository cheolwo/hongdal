using Hongdal.Contracts.Common.Hr;
using Hongdal.Services.HumanResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.HumanResources;

[ApiController]
[Authorize(Roles = "서버관리자")]
[Route("api/v1/admin/hr-employment-contracts")]
public sealed class HrEmploymentContractsController : ControllerBase
{
    private readonly IHrEmploymentContractService _contractService;

    public HrEmploymentContractsController(IHrEmploymentContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<ActionResult<HrEmploymentContractListResponse>> List(
        [FromQuery] string? workerUserId,
        [FromQuery] string? employerScopeType,
        [FromQuery] string? employerScopeId,
        CancellationToken cancellationToken)
    {
        var items = await _contractService.ListAsync(workerUserId, employerScopeType, employerScopeId, cancellationToken);
        return Ok(new HrEmploymentContractListResponse { Items = items });
    }

    [HttpGet("{contractId:guid}")]
    public async Task<ActionResult<HrEmploymentContractResponse>> Get(Guid contractId, CancellationToken cancellationToken)
    {
        var contract = await _contractService.GetAsync(contractId, cancellationToken);
        return contract is null ? NotFound() : Ok(contract);
    }

    [HttpPost]
    public async Task<ActionResult<HrEmploymentContractResponse>> CreateDraft(
        [FromBody] HrEmploymentContractDraftRequest request,
        CancellationToken cancellationToken)
    {
        var contract = await _contractService.CreateDraftAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { contractId = contract.Id }, contract);
    }

    [HttpPost("{contractId:guid}/sign")]
    public async Task<ActionResult<HrEmploymentContractResponse>> Sign(
        Guid contractId,
        [FromBody] HrEmploymentContractSignRequest request,
        CancellationToken cancellationToken)
    {
        var contract = await _contractService.SignAsync(contractId, request.SignedByUserId, cancellationToken);
        return Ok(contract);
    }

    [HttpPost("{contractId:guid}/payroll-schedules")]
    public async Task<ActionResult<HrPayrollScheduleListResponse>> CreatePayrollSchedules(
        Guid contractId,
        [FromBody] HrPayrollScheduleCreateRequest request,
        CancellationToken cancellationToken)
    {
        var schedules = await _contractService.CreatePayrollSchedulesAsync(
            contractId,
            request.ScheduleStartDate,
            request.ScheduleEndDate,
            cancellationToken);

        return Ok(new HrPayrollScheduleListResponse { Items = schedules });
    }
}
