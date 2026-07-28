using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Sales;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Seller;

[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.CustomsAndTradeData)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Customs)]
[SsalddelApiCapability(SsalddelCapability.TradePreparation)]
[SsalddelApiOperation(SsalddelOperation.Manage)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.Api,
    "해외 판매자의 한국 수입식품용 실제 제조시설 등록 준비 원장을 조회하고 저장합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(해외판매자식품시설응답),
    FlowOrder = 36,
    Boundary = "식약처·세관·수출국 정부에 등록이나 신고를 전송하지 않습니다.")]
[ApiController]
[Authorize(Roles = 역할명.화주 + "," + 역할명.판매자 + "," + 역할명.서버관리자)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/seller/foreign-food-facilities")]
public sealed class 해외판매자식품시설Controller(I해외판매자식품시설UseCase useCase)
    : ShipperControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(해외판매자식품시설목록응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 목록(CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.목록Async(
            CurrentUserId(),
            User.IsInRole(역할명.서버관리자),
            cancellationToken));

    [HttpGet("{profileId}")]
    [ProducesResponseType(typeof(해외판매자식품시설응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 조회(string profileId, CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.조회Async(
            profileId,
            CurrentUserId(),
            User.IsInRole(역할명.서버관리자),
            cancellationToken));

    [HttpPut("{profileId}")]
    [ProducesResponseType(typeof(해외판매자식품시설응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 저장(
        string profileId,
        [FromBody] 해외판매자식품시설저장요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.저장Async(
            profileId,
            request,
            CurrentUserId(),
            User.IsInRole(역할명.서버관리자),
            cancellationToken));

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? string.Empty;
}
