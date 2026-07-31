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
    "지역문화 3D 애니메이션 이미지의 검토 승인, 지역별 10장 진행 현황과 bounded 생성 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "서버관리자만 접근하며 승인되지 않은 프롬프트, Simulation 모드, 비활성 설정에서는 외부 비용 작업을 등록하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalCultureImagePrompt,
    SsalddelCodeLayer.Api,
    "지역문화 애니메이션 이미지 승인·진행 현황·다음 장면 생성 API",
    ContractType = typeof(I지역문화이미지생성관리UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead
              | SsalddelCodeEffect.PersistentWrite
              | SsalddelCodeEffect.NetworkCall
              | SsalddelCodeEffect.ThirdPartyApiCall
              | SsalddelCodeEffect.ObjectStorageWrite
              | SsalddelCodeEffect.MayIncurExternalCost,
    Boundary = "외부 생성은 명시적 검토 승인과 Operational·일일 한도·단일 활성 작업을 모두 요구합니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/information/regional-culture/image-generation")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("RegionalCultureImageGenerationController")]
public sealed class 지역문화이미지생성Controller(
    I지역문화이미지생성관리UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("Progress")]
    public async Task<ActionResult<RegionalCultureImageGenerationProgressResponse>> 진행현황(
        [FromQuery] string? countryCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await useCase.진행현황조회Async(countryCode, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "지역문화 이미지 진행 현황 조건을 확인해 주세요",
                exception.Message));
        }
    }

    [HttpPost("prompts/{regionKey}/approve")]
    [SsalddelApiContractName("ApprovePrompt")]
    public async Task<ActionResult<RegionalCultureImageGenerationApprovalResponse>> 생성승인(
        string regionKey,
        [FromBody] RegionalCultureImageGenerationApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await useCase.생성승인Async(
                regionKey,
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "지역문화 이미지 생성 승인 조건을 확인해 주세요",
                exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(CreateProblem(
                StatusCodes.Status404NotFound,
                "지역문화 이미지 프롬프트를 찾을 수 없습니다",
                exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem(
                StatusCodes.Status409Conflict,
                "현재 상태에서는 지역문화 이미지 생성을 승인할 수 없습니다",
                exception.Message));
        }
    }

    [HttpPost("next")]
    [SsalddelApiContractName("GenerateNext")]
    public async Task<ActionResult<RegionalCultureImageGenerationNextResponse>> 다음장면생성(
        [FromBody] RegionalCultureImageGenerationNextRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await useCase.다음장면생성Async(
            request,
            cancellationToken);
        return response.Accepted
            ? Accepted(response)
            : Conflict(response);
    }

    private static ProblemDetails CreateProblem(
        int status,
        string title,
        string detail)
        => new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };
}
