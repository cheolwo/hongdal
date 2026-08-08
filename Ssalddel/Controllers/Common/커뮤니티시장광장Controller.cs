using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.Community;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Api,
    "Unity 커뮤니티·시장 광장의 공개 read-only aggregate HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "공개 게시판·게시글·비식별 활동·허용된 원장 요약만 반환한다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Api,
    "커뮤니티 시장 광장 공개 snapshot 조회",
    ContractType = typeof(CommunityMarketSquareSnapshotResponse),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.None)]
[ApiController]
[AllowAnonymous]
[Route(CommunityMarketSquareRoutes.PublicSnapshot)]
public sealed class 커뮤니티시장광장Controller(
    I커뮤니티시장광장조회UseCase useCase) : CommunityControllerBase
{
    [HttpGet]
    public async Task<IActionResult> 조회(
        [FromQuery] string? appKey,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.조회Async(appKey, cancellationToken));
}
