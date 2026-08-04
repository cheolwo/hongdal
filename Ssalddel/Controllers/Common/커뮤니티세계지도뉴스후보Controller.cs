using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Community;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Api,
    "지도 뉴스 출처와 명시 선택한 별도 공식뉴스 RSS 검토 후보를 연결",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "언론사 자체 feed와 같은 국가의 정부기관 RSS를 구분하고 자동 게시하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Api,
    "뉴스 출처 마커별 RSS 지원 상태와 명시 선택한 검토 후보를 조회",
    ContractType = typeof(I커뮤니티세계지도뉴스후보UseCase),
    FlowOrder = 35,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "공개 요청은 RSS를 직접 호출하지 않고 운영 검토 원장에서 승인된 snapshot만 조회합니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route(커뮤니티세계지도Routes.ObservationApi)]
[SsalddelApiContractName("CommunityWorldMapNewsCandidatesController")]
public sealed class 커뮤니티세계지도뉴스후보Controller(
    I커뮤니티세계지도뉴스후보UseCase useCase) : ControllerBase
{
    [HttpGet("{observationStableId}/news-candidates")]
    [SsalddelApiContractName("GetNewsCandidates")]
    public async Task<ActionResult<커뮤니티세계지도뉴스후보Response>> 조회(
        string observationStableId,
        [FromQuery] string? sourceKey,
        [FromQuery] int take = 8,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await useCase.조회Async(
                observationStableId,
                sourceKey,
                take,
                cancellationToken);
            return response is null
                ? NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "뉴스 출처 마커를 찾을 수 없습니다."
                })
                : Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "뉴스 후보 sourceKey를 확인해 주세요.",
                Detail = exception.Message
            });
        }
    }
}
