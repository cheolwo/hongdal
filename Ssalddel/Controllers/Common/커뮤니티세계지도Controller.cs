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
    "한 개의 커뮤니티 세계 지도에 표시할 분야별 공개 관측의 읽기 전용 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "개인 위치와 결제·계약·배차 상태를 공개하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Api,
    "낮·밤 dataset별 지도 관측 snapshot을 공개 조회",
    ContractType = typeof(I커뮤니티세계지도조회UseCase),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.None,
    Boundary = "읽기 전용이며 외부 API 호출이나 업무 실행 효과를 만들지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route(커뮤니티세계지도Routes.ObservationApi)]
[SsalddelApiContractName("CommunityWorldMapObservationsController")]
public sealed class 커뮤니티세계지도Controller(
    I커뮤니티세계지도조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("GetSnapshot")]
    public async Task<ActionResult<커뮤니티세계지도SnapshotDto>> 조회(
        [FromQuery] string? dataset = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await useCase.조회Async(dataset, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "세계 지도 dataset을 확인해 주세요",
                Detail = exception.Message
            });
        }
    }
}
