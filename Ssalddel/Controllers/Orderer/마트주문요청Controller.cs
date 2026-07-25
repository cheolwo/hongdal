using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Mart;
using Ssalddel.Contracts.Mart;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[SsalddelApiVersion(
    SsalddelProductVersion.V3_5,
    FeatureKey = VersionFeatureFlagKeys.SsalddelMartWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.SsalddelMartWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.SsalddelMart)]
[RequireVersionFeature(VersionFeatureFlagKeys.SsalddelMartWorkflow)]
[ApiController]
[Authorize]
[Route("api/v1/orderer/mart/order-requests")]
public sealed class 마트주문요청Controller(
    I마트주문요청조회UseCase queryUseCase,
    I마트주문요청작성UseCase commandUseCase) : OrdererControllerBase
{
    [HttpGet("{orderRequestId:guid}")]
    public async Task<IActionResult> 상세(Guid orderRequestId, CancellationToken cancellationToken)
        => this.ToActionResult(await queryUseCase.상세Async(orderRequestId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> 등록(
        [FromBody] 마트주문요청등록요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await commandUseCase.등록Async(request, cancellationToken));
}
