using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Content;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Api,
    "홍익학당 철학·영상 지도 레이어의 공개 원천·지리 검증 상태 조회 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "검증되지 않은 개인·장소·추정 위치를 공개하지 않고, 지리 레코드가 없으면 명시적인 빈 레이어만 반환합니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.HongikAcademyContentMap,
    SsalddelCodeLayer.Api,
    "홍익학당 철학·영상 지도 레이어를 읽기 전용으로 공개",
    ContractType = typeof(I홍익학당철학영상MapLayer조회UseCase),
    FlowOrder = 40,
    Boundary = "콘텐츠 추천, 외부 영상 호출, 위치 추론·저장, 개인 데이터 공개를 실행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route(HongikAcademyContentMapRoutes.LayerApi)]
[SsalddelApiContractName("HongikAcademyContentMapLayerController")]
public sealed class 홍익학당철학영상MapController(
    I홍익학당철학영상MapLayer조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("GetLayer")]
    public async Task<ActionResult<HongikAcademyContentMapLayerResponse>> 레이어조회(
        CancellationToken cancellationToken = default)
        => Ok(await useCase.조회Async(cancellationToken));
}
