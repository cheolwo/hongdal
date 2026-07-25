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
[SsalddelApiContractName("HrRoleApplicationsController")]
public sealed class 인사역할지원Controller(
    IHR역할지원조회UseCase queryUseCase,
    IHR역할지원CommandUseCase commandUseCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("MyApplications")]
    public async Task<IActionResult> 내지원목록조회(CancellationToken cancellationToken)
        => this.ToActionResult(await queryUseCase.내지원목록Async(cancellationToken));

    [HttpPost]
    [SsalddelApiContractName("Submit")]
    public async Task<IActionResult> 지원(
        [FromBody] HrRoleApplicationSubmitRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await commandUseCase.제출Async(request, cancellationToken));

    [HttpPost("{applicationId:guid}/withdraw")]
    [SsalddelApiContractName("Withdraw")]
    public async Task<IActionResult> 지원철회(
        Guid applicationId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await commandUseCase.철회Async(applicationId, cancellationToken));
}
