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
    "한국·미국 행정구역 기반 농수산물 가격 마커의 공개 조회 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "정확한 생산자·출하자·사업체 위치를 노출하지 않고 검증된 공식 지역 기준점만 반환합니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Api,
    "국가·관계·품목·기간별 농수산물 지도 마커를 읽기 전용으로 공개",
    ContractType = typeof(I지역농수산MapMarker조회UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "가격 관측과 지역 근거를 조회할 뿐 외부 API 호출·거래·알림을 실행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route("api/v1/community/regional-map/markers")]
[SsalddelApiContractName("RegionalAgriculturalMapMarkersController")]
public sealed class 지역농수산MapController(
    I지역농수산MapMarker조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<RegionalAgriculturalMapMarkerListResponse>> 목록조회(
        [FromQuery] RegionalAgriculturalMapMarkerQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await useCase.조회Async(query, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "지역 농수산물 마커 조회 조건을 확인해 주세요",
                Detail = exception.Message
            });
        }
    }
}
