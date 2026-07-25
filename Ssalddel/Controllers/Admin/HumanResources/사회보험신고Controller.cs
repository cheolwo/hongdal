using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Application.HumanResources;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.HumanResources;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.HrParticipationWorkflow, WorkflowKey = VersionFeatureFlagKeys.HrParticipationWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.HrParticipation)]
[RequireVersionFeature(VersionFeatureFlagKeys.HrParticipationWorkflow)]
[Authorize(Policy = "?쒕쾭愿由ъ옄?꾩슜")]
[Route("api/v1/admin/hr-social-insurance-filings")]
[SsalddelApiContractName("SocialInsuranceFilingsController")]
public sealed class 사회보험신고Controller : ControllerBase
{
    private readonly I사회보험신고UseCase _사회보험신고UseCase;

    public 사회보험신고Controller(I사회보험신고UseCase 사회보험신고UseCase)
    {
        _사회보험신고UseCase = 사회보험신고UseCase;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? workerUserId,
        [FromQuery] string? employerScopeType,
        [FromQuery] string? employerScopeId,
        [FromQuery] string? filingStatus,
        CancellationToken cancellationToken)
    {
        var result = await _사회보험신고UseCase.목록Async(
            workerUserId,
            employerScopeType,
            employerScopeId,
            filingStatus,
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 상세조회(Guid id, CancellationToken cancellationToken)
    {
        var result = await _사회보험신고UseCase.상세Async(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("assess")]
    [SsalddelApiContractName("Assess")]
    public async Task<IActionResult> 평가(
        [FromBody] SocialInsuranceEligibilityAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _사회보험신고UseCase.가입요건평가Async(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [SsalddelApiContractName("CreatePlan")]
    public async Task<IActionResult> 계획생성(
        [FromBody] SocialInsuranceFilingPlanCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _사회보험신고UseCase.계획생성Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(상세조회), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}/status")]
    [SsalddelApiContractName("UpdateStatus")]
    public async Task<IActionResult> 상태수정(
        Guid id,
        [FromBody] SocialInsuranceFilingStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _사회보험신고UseCase.상태수정Async(id, request, cancellationToken);
        return this.ToActionResult(result);
    }
}
