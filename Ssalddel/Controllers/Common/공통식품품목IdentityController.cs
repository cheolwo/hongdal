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
    "출처별 품목코드 관계와 검토 상태를 보존한 공통 식품 품목 identity 공개 조회",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "공개 코드 관계이며 가격·재고·판매 가능성·세관 신고 세번을 확정하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommonFoodProductIdentity,
    SsalddelCodeLayer.Api,
    "공통 상품 stable ID와 KAMIS·HS·USDA AMS·농사로 코드 관계를 공개한다.",
    ContractType = typeof(I공통식품품목Identity조회UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "Candidate와 Unlinked 상태를 Confirmed로 승격하지 않으며 외부 기관 코드의 의미를 보존합니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route(공통식품품목IdentityRoutes.Api)]
[SsalddelApiContractName("CommonFoodProductIdentitiesController")]
public sealed class 공통식품품목IdentityController(
    I공통식품품목Identity조회UseCase useCase,
    I공통식품품목기존Data대조UseCase reconciliationUseCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<공통식품품목IdentityListResponse>> 목록조회(
        CancellationToken cancellationToken = default)
        => Ok(await useCase.목록조회Async(cancellationToken));

    [HttpGet("{canonicalProductStableId}")]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<공통식품품목IdentityResponse>> 단건조회(
        string canonicalProductStableId,
        CancellationToken cancellationToken = default)
    {
        var item = await useCase.단건조회Async(
            canonicalProductStableId,
            cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet(공통식품품목IdentityRoutes.ReconciliationPreview)]
    [SsalddelApiContractName("ReconciliationPreview")]
    public async Task<ActionResult<공통식품품목기존Data대조Response>> 기존Data대조Preview(
        [FromQuery] int? year,
        CancellationToken cancellationToken = default)
        => Ok(await reconciliationUseCase.PreviewAsync(
            year ?? DateTime.UtcNow.Year,
            cancellationToken));
}
