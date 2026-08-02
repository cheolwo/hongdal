using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using 살뜰.Services.Versioning;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(
    SsalddelProductVersion.V0_0,
    FeatureKey = VersionFeatureFlagKeys.CommunityTrustWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CommunityTrustWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/activity-paid-details")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityActivityPaidDetail,
    SsalddelCodeLayer.Api,
    "유료 활동 상세 등록, 미리보기, 구매 상태와 구매 후 본문 조회 HTTP 계약을 제공합니다.",
    ContractType = typeof(I커뮤니티활동유료상세UseCase),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "구매와 본문 조회는 인증이 필요하고 FakePG 확인은 실행 모드 경계를 따릅니다.")]
public sealed class 커뮤니티활동유료상세Controller(I커뮤니티활동유료상세UseCase useCase)
    : CommunityControllerBase
{
    [HttpPost]
    [Authorize]
    [SsalddelApiContractName("CreatePaidActivityDetail")]
    public async Task<IActionResult> 등록(
        [FromBody] 커뮤니티활동유료상세등록Request request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.등록Async(request, cancellationToken));

    [HttpGet("{detailId}")]
    [AllowAnonymous]
    [SsalddelApiContractName("GetPaidActivityDetailPreview")]
    public async Task<IActionResult> 미리보기조회(
        [FromRoute] string detailId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.조회Async(detailId, false, cancellationToken));

    [HttpGet("posts/{postId:long}")]
    [AllowAnonymous]
    [SsalddelApiContractName("GetPaidActivityDetailByPost")]
    public async Task<IActionResult> 게시글별조회(
        [FromRoute] long postId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.게시글별조회Async(postId, cancellationToken));

    [HttpGet("{detailId}/content")]
    [Authorize]
    [SsalddelApiContractName("GetPurchasedActivityDetailContent")]
    public async Task<IActionResult> 상세내용조회(
        [FromRoute] string detailId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.조회Async(detailId, true, cancellationToken));

    [HttpGet("entitlements/me")]
    [Authorize]
    [SsalddelApiContractName("GetMyPaidActivityDetailEntitlements")]
    public async Task<IActionResult> 내열람권조회(CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.내열람권목록Async(cancellationToken));

    [HttpGet("purchases/me")]
    [Authorize]
    [SsalddelApiContractName("GetMyPaidActivityDetailPurchases")]
    public async Task<IActionResult> 내구매목록조회(CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.내구매목록Async(cancellationToken));

    [HttpGet("purchases/{purchaseId}")]
    [Authorize]
    [SsalddelApiContractName("GetPaidActivityDetailPurchase")]
    public async Task<IActionResult> 구매조회(
        [FromRoute] string purchaseId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.구매조회Async(purchaseId, cancellationToken));

    [HttpPost("{detailId}/fake-pg/confirm")]
    [Authorize]
    [SsalddelApiContractName("ConfirmPaidActivityDetailFakePg")]
    public async Task<IActionResult> 모의결제확인(
        [FromRoute] string detailId,
        [FromBody] 커뮤니티활동상세FakePg결제승인Request request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.페이크결제승인Async(detailId, request, cancellationToken));
}
