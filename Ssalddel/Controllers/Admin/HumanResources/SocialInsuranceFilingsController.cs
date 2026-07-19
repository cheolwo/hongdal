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
public sealed class SocialInsuranceFilingsController : ControllerBase
{
    private readonly I사회보험신고UseCase _useCase;

    public SocialInsuranceFilingsController(I사회보험신고UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? workerUserId,
        [FromQuery] string? employerScopeType,
        [FromQuery] string? employerScopeId,
        [FromQuery] string? filingStatus,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.목록Async(
            workerUserId,
            employerScopeType,
            employerScopeId,
            filingStatus,
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _useCase.상세Async(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("assess")]
    public async Task<IActionResult> Assess(
        [FromBody] SocialInsuranceEligibilityAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.가입요건평가Async(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlan(
        [FromBody] SocialInsuranceFilingPlanCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.계획생성Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] SocialInsuranceFilingStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.상태수정Async(id, request, cancellationToken);
        return this.ToActionResult(result);
    }
}
