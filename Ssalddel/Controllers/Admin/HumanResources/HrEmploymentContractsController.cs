using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Application.HumanResources;
using Ssalddel.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin.HumanResources;

[SsalddelApiVersion(SsalddelProductVersion.V2_5)]
[ApiController]
[Authorize(Roles = "서버관리자")]
[Route("api/v1/admin/hr-employment-contracts")]
public sealed class HrEmploymentContractsController : ControllerBase
{
    private readonly IHR참여운영UseCase _useCase;

    public HrEmploymentContractsController(IHR참여운영UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? workerUserId,
        [FromQuery] string? employerScopeType,
        [FromQuery] string? employerScopeId,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.고용계약목록Async(workerUserId, employerScopeType, employerScopeId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{contractId:guid}")]
    public async Task<IActionResult> Get(Guid contractId, CancellationToken cancellationToken)
    {
        var result = await _useCase.고용계약상세Async(contractId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDraft(
        [FromBody] HrEmploymentContractDraftRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.고용계약초안생성Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { contractId = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("{contractId:guid}/sign")]
    public async Task<IActionResult> Sign(
        Guid contractId,
        [FromBody] HrEmploymentContractSignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.고용계약서명Async(contractId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{contractId:guid}/payroll-schedules")]
    public async Task<IActionResult> CreatePayrollSchedules(
        Guid contractId,
        [FromBody] HrPayrollScheduleCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.급여스케줄생성Async(contractId, request, cancellationToken);
        return this.ToActionResult(result);
    }
}
