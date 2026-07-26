using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Content;

namespace Ssalddel.Controllers.Admin.Content07;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.Api,
    "지역문화 이미지 조사 초안과 생성 프롬프트의 관리자 조회 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "조회만 제공하며 이미지 생성·비용 발생·게시글 첨부를 실행하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalCultureImagePrompt,
    SsalddelCodeLayer.Api,
    "관리자에게 지역문화 이미지 프롬프트 목록과 상세를 제공",
    ContractType = typeof(I지역문화이미지Prompt조회UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "서버관리자 정책을 요구하고 ResearchDraft와 근거 재검토 필요 상태를 숨기지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/information/regional-culture/image-prompts")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("RegionalCultureImagePromptsController")]
public sealed class 지역문화이미지PromptController(
    I지역문화이미지Prompt조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<RegionalCultureImagePromptListResponse>> 목록조회(
        [FromQuery] string? countryCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await useCase.목록조회Async(countryCode, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(exception.Message));
        }
    }

    [HttpGet("{regionKey}")]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<RegionalCultureImagePromptDto>> 상세조회(
        string regionKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await useCase.상세조회Async(regionKey, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(exception.Message));
        }
    }

    private static ProblemDetails CreateProblem(string detail)
        => new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "지역문화 이미지 프롬프트 조회 조건을 확인해 주세요",
            Detail = detail
        };
}
