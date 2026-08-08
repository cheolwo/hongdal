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
    "농사로 출처가 보존된 작물 기준정보의 공개 조회 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "공개 작목기술 분류이며 개별 농장의 재배 상태·위치·생산량·재고를 반환하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CropReferenceInformation,
    SsalddelCodeLayer.Api,
    "농사로 작목기술 주분류를 Unity와 Web이 재사용할 typed projection으로 공개",
    ContractType = typeof(I작물기준정보분류조회UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.ThirdPartyApiCall,
    Boundary = "외부 공개정보를 실시간 조회할 뿐 운영 농장 원장이나 거래 상태를 생성하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route(CropReferenceRoutes.CategoryApi)]
[SsalddelApiContractName("CropReferenceCategoriesController")]
public sealed class 작물기준정보Controller(
    I작물기준정보분류조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<CropReferenceCategoryListResponse>> 목록조회(
        CancellationToken cancellationToken = default)
        => Ok(await useCase.조회Async(cancellationToken));
}
