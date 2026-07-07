using Hongdal.Contracts.Common.Hr;
using Hongdal.Controllers;
using Hongdal.ApiMetadata;
using Hongdal.Filters;
using Hongdal.Services.HumanResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Admin.HumanResources;

[ApiController]
[HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.HrParticipationWorkflow, WorkflowKey = VersionFeatureFlagKeys.HrParticipationWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.HrParticipation)]
[RequireVersionFeature(VersionFeatureFlagKeys.HrParticipationWorkflow)]
[Authorize(Policy = "?쒕쾭愿由ъ옄?꾩슜")]
[Route("api/v1/admin/hr-social-insurance-filings")]
public sealed class SocialInsuranceFilingsController : ControllerBase
{
    private readonly ISocialInsuranceFilingService _filingService;

    public SocialInsuranceFilingsController(ISocialInsuranceFilingService filingService)
    {
        _filingService = filingService;
    }

    [HttpGet]
    public async Task<ActionResult<SocialInsuranceFilingPlanListResponse>> List(
        [FromQuery] string? workerUserId,
        [FromQuery] string? employerScopeType,
        [FromQuery] string? employerScopeId,
        [FromQuery] string? filingStatus,
        CancellationToken cancellationToken)
    {
        var items = await _filingService.ListAsync(
            workerUserId,
            employerScopeType,
            employerScopeId,
            filingStatus,
            cancellationToken);

        return Ok(new SocialInsuranceFilingPlanListResponse { Items = items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var plan = await _filingService.GetAsync(id, cancellationToken);
        return plan is null
            ? this.ToNotFoundProblem("Social insurance filing plan was not found.")
            : Ok(plan);
    }

    [HttpPost("assess")]
    public async Task<IActionResult> Assess(
        [FromBody] SocialInsuranceEligibilityAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assessment = await _filingService.AssessAsync(request, cancellationToken);
            return Ok(assessment);
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlan(
        [FromBody] SocialInsuranceFilingPlanCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _filingService.CreatePlanAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = plan.Id }, plan);
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] SocialInsuranceFilingStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _filingService.UpdateStatusAsync(id, request, cancellationToken);
            return Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return this.ToNotFoundProblem(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }
}
