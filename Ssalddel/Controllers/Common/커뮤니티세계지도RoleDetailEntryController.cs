using System.Security.Claims;
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
    "공개 지도에서 인증된 역할 앱 작업대로 인계하는 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "개별 업무 데이터는 반환하지 않으며 상세 API의 원장 범위 검증을 대체하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Api,
    "인증된 역할 claim에 맞는 상세 작업대 진입 경로를 조회",
    ContractType = typeof(I커뮤니티세계지도RoleDetailEntryUseCase),
    FlowOrder = 34,
    Effects = SsalddelCodeEffect.None,
    Boundary = "공개 지도에는 개인 위치·연락처·거래처·재고·계약·개별 배정 상태를 추가하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Authorize]
[Route(커뮤니티세계지도Routes.RoleDetailEntryApi)]
[SsalddelApiContractName("CommunityWorldMapRoleDetailEntryController")]
public sealed class 커뮤니티세계지도RoleDetailEntryController(
    I커뮤니티세계지도RoleDetailEntryUseCase useCase) : ControllerBase
{
    [HttpPost]
    [SsalddelApiContractName("ResolveRoleDetailEntry")]
    public ActionResult<커뮤니티세계지도RoleDetailEntryResponse> Resolve(
        [FromBody] 커뮤니티세계지도RoleDetailEntryRequest request)
    {
        var response = useCase.Resolve(
            request.EntryCode,
            User.FindFirstValue(ClaimTypes.Role));

        return response is null ? Forbid() : Ok(response);
    }
}
