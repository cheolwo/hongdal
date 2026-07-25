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
[SsalddelApiContractName("HrRoleReviewsController")]
public sealed class 인사역할검토Controller(IHR역할검토조회UseCase 인사역할검토UseCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] HrRoleReviewListRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await 인사역할검토UseCase.목록Async(request, cancellationToken));

    [HttpGet("{reviewId:guid}")]
    [SsalddelApiContractName("Detail")]
    public async Task<IActionResult> 상세조회(
        Guid reviewId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await 인사역할검토UseCase.상세Async(reviewId, cancellationToken));
}
