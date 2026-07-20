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
[Route("api/v1/orderer/mart/products")]
public sealed class 마트공개상품Controller(I마트공개상품조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> 목록(
        [FromQuery] 마트공개상품목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.목록Async(request, cancellationToken));

    [HttpGet("{productId:long}")]
    public async Task<IActionResult> 상세(long productId, CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.상세Async(productId, cancellationToken));
}
