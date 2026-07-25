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
[SsalddelApiContractName("HrEmploymentContractsController")]
public sealed class 고용계약Controller : ControllerBase
{
    private readonly IHR참여운영UseCase _고용계약UseCase;

    public 고용계약Controller(IHR참여운영UseCase 고용계약UseCase)
    {
        _고용계약UseCase = 고용계약UseCase;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? workerUserId,
        [FromQuery] string? employerScopeType,
        [FromQuery] string? employerScopeId,
        CancellationToken cancellationToken)
    {
        var result = await _고용계약UseCase.고용계약목록Async(workerUserId, employerScopeType, employerScopeId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{contractId:guid}")]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 상세조회(Guid contractId, CancellationToken cancellationToken)
    {
        var result = await _고용계약UseCase.고용계약상세Async(contractId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [SsalddelApiContractName("CreateDraft")]
    public async Task<IActionResult> 초안생성(
        [FromBody] HrEmploymentContractDraftRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _고용계약UseCase.고용계약초안생성Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(상세조회), new { contractId = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("{contractId:guid}/sign")]
    [SsalddelApiContractName("Sign")]
    public async Task<IActionResult> 서명(
        Guid contractId,
        [FromBody] HrEmploymentContractSignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _고용계약UseCase.고용계약서명Async(contractId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{contractId:guid}/payroll-schedules")]
    [SsalddelApiContractName("CreatePayrollSchedules")]
    public async Task<IActionResult> 급여일정생성(
        Guid contractId,
        [FromBody] HrPayrollScheduleCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _고용계약UseCase.급여스케줄생성Async(contractId, request, cancellationToken);
        return this.ToActionResult(result);
    }
}
