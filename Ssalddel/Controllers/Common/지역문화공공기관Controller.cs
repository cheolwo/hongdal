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
    "지역문화 공공기관과 공식 데이터 원천의 공개 조회 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "기관 연결은 정보 확인 경로이며 정부 보증·문화 대표성·민원 접수를 대신하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalCulturePublicInstitution,
    SsalddelCodeLayer.Api,
    "지역문화 공공기관 원천을 국가와 관할 단계로 공개 조회",
    ContractType = typeof(I지역문화공공기관Source조회UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "읽기 전용이며 외부 기관 호출·민원 제출·개인정보 수집을 실행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route("api/v1/community/regional-culture/public-institutions")]
[SsalddelApiContractName("RegionalCulturePublicInstitutionsController")]
public sealed class 지역문화공공기관Controller(
    I지역문화공공기관Source조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<RegionalCulturePublicInstitutionSourceListResponse>> 목록조회(
        [FromQuery] string? countryCode = null,
        [FromQuery] string? jurisdictionLevelCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await useCase.목록조회Async(
                countryCode,
                jurisdictionLevelCode,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "지역문화 공공기관 조회 조건을 확인해 주세요",
                Detail = exception.Message
            });
        }
    }
}
