using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.HumanResources;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Controllers;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(
    SsalddelProductVersion.V2_5,
    FeatureKey = VersionFeatureFlagKeys.HrParticipationWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.HrParticipationWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.HrParticipation)]
[RequireVersionFeature(VersionFeatureFlagKeys.HrParticipationWorkflow)]
[ApiController]
[Authorize]
[Route("api/v1/hr/role-applications")]
public sealed class HrRoleApplicationsController(
    IHR역할지원조회UseCase queryUseCase,
    IHR역할지원CommandUseCase commandUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> MyApplications(CancellationToken cancellationToken)
        => this.ToActionResult(await queryUseCase.내지원목록Async(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] HrRoleApplicationSubmitRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await commandUseCase.제출Async(request, cancellationToken));

    [HttpPost("{applicationId:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(
        Guid applicationId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await commandUseCase.철회Async(applicationId, cancellationToken));
}
