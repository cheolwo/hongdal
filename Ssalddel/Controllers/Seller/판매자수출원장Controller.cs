using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Seller;

[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.CustomsAndTradeData)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Customs)]
[SsalddelApiCapability(SsalddelCapability.TradePreparation)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.Api,
    "판매자 또는 화주가 본인이 소유하거나 참여한 개별수출·공동수출 준비 원장을 조회합니다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(판매자수출원장목록응답),
    FlowOrder = 35,
    Boundary = "수출 신고·계약·결제·포워더 전송을 실행하지 않고 준비 원장만 조회합니다.")]
[ApiController]
[Authorize(Roles = 역할명.화주 + "," + 역할명.판매자 + "," + 역할명.서버관리자)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/seller/export-ledgers")]
public sealed class 판매자수출원장Controller(I무역확장원장UseCase useCase) : ShipperControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(판매자수출원장목록응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 목록(
        [FromQuery] 판매자수출원장목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.판매자수출목록조회Async(
            request,
            CurrentUserId(),
            User.IsInRole(역할명.서버관리자),
            cancellationToken));

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? string.Empty;
}
