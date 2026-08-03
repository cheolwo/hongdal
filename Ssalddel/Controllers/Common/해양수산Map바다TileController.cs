using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Api,
    "해양수산부 어획구역을 바다별 공개 지도 타일로 제공",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "좌표 없는 원천을 실제 어획 위치로 보이지 않고 출처와 개략 배치 한계를 함께 반환합니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Api,
    "공식 어획구역 카탈로그의 바다별 개략 타일 공개 조회",
    ContractType = typeof(I해양수산Map바다Tile조회UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.ThirdPartyApiCall,
    Boundary = "읽기 요청 때 공식 파일을 단기 캐시로 수집할 뿐 거래·알림·운영 실행을 만들지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route(RegionalAgriculturalMapRoutes.OceanTileApi)]
[SsalddelApiContractName("MarineFishingAreaOceanTilesController")]
public sealed class 해양수산Map바다TileController(
    I해양수산Map바다Tile조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<MarineFishingAreaOceanTileResponse>> 목록조회(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await useCase.조회Async(cancellationToken));
        }
        catch (Exception exception) when (
            exception is HttpRequestException or InvalidDataException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "해양수산부 어획구역 데이터를 수집하지 못했습니다",
                Detail = exception.Message
            });
        }
    }
}
