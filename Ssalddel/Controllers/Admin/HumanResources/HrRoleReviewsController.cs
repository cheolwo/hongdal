using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.HumanResources;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Controllers;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.HumanResources;

[SsalddelApiVersion(
    SsalddelProductVersion.V2_5,
    FeatureKey = VersionFeatureFlagKeys.HrParticipationWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.HrParticipationWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.HrParticipation)]
[RequireVersionFeature(VersionFeatureFlagKeys.HrParticipationWorkflow)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/hr-role-reviews")]
public sealed class HrRoleReviewsController(IHR역할검토조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] HrRoleReviewListRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.목록Async(request, cancellationToken));

    [HttpGet("{reviewId:guid}")]
    public async Task<IActionResult> Detail(
        Guid reviewId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.상세Async(reviewId, cancellationToken));
}
