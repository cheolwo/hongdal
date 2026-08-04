using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Security;
using Ssalddel.Services.Community;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Participation,
    SsalddelModuleKind.Api,
    "공개 지도 observation을 사용자 확인형 질문과 커뮤니티 참여 흐름으로 연결",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "초안 생성은 저장하지 않고 확인 게시도 글만 저장하며 가원장과 실행 효과를 자동 생성하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Api,
    "지도 observation 기반 질문 초안과 확인 게시 HTTP 경계",
    ContractType = typeof(I커뮤니티세계지도질문UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "출처를 서버 snapshot에서 다시 확인하고 사용자 확인 뒤 게시글만 저장합니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route(커뮤니티세계지도Routes.ObservationApi)]
[SsalddelApiContractName("CommunityWorldMapObservationQuestionsController")]
public sealed class 커뮤니티세계지도질문Controller(
    I커뮤니티세계지도질문UseCase useCase) : CommunityControllerBase
{
    [HttpPost("{observationStableId}/question-draft")]
    [AllowAnonymous]
    [SsalddelApiContractName("BuildQuestionDraft")]
    public async Task<ActionResult<커뮤니티세계지도질문초안Response>> 질문초안생성(
        string observationStableId,
        [FromBody] 커뮤니티세계지도질문초안Request request,
        CancellationToken cancellationToken)
    {
        try
        {
            var draft = await useCase.초안생성Async(
                observationStableId,
                request,
                cancellationToken);
            return draft is null
                ? NotFoundProblem("지도 observation을 찾을 수 없습니다.")
                : Ok(draft);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "질문 초안 요청을 확인해 주세요.",
                detail: exception.Message);
        }
    }

    [HttpPost("{observationStableId}/questions")]
    [Authorize]
    [EnableRateLimiting(RequestRateLimitPolicyNames.CommunityMutation)]
    [SsalddelApiContractName("PublishQuestion")]
    public async Task<IActionResult> 질문게시(
        string observationStableId,
        [FromBody] 커뮤니티세계지도질문게시Request request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await useCase.게시Async(
                observationStableId,
                request,
                cancellationToken);
            return result.IsSuccess
                ? Created(result.Value.PostHref, result.Value)
                : this.ToActionResult(result);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "질문 게시 요청을 확인해 주세요.",
                detail: exception.Message);
        }
    }

    private ObjectResult NotFoundProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "지도 observation을 찾을 수 없습니다.",
            detail: detail);
}
