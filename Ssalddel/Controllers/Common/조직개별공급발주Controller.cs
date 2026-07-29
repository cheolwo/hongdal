using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.ContractManagement;
using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(
    SsalddelProductVersion.V3_5,
    FeatureKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelApiCapability(SsalddelCapability.ProductDiscovery)]
[SsalddelApiCapability(SsalddelCapability.OrderParticipation)]
[SsalddelApiAudience(SsalddelActor.Restaurant)]
[SsalddelApiAudience(SsalddelActor.MartOperator)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelApiOperation(SsalddelOperation.Request)]
[SsalddelApiOperation(SsalddelOperation.Record)]
[RequireVersionFeature(VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[ApiController]
[Authorize]
[Route("api/v1/supply-brokerage")]
[SsalddelApiContractName("OrganizationSupplyBrokerageController")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
    SsalddelCodeLayer.Api,
    "음식점과 살들마트의 공급계약 이용등록 및 개별 발주 중개 API를 제공합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(개별공급발주등록요청),
    FlowOrder = 80,
    Boundary = "서버 Claim으로 확인된 음식점·살들마트만 자기 명의 발주를 제출하며 조직 ID를 요청 본문에서 선택하지 않습니다.")]
public sealed class 조직개별공급발주Controller(
    I조직개별공급발주UseCase useCase) : ControllerBase
{
    [HttpGet("agreements")]
    [SsalddelApiContractName("GetAvailableSupplyAgreements")]
    public async Task<IActionResult> 이용가능계약조회(
        [FromQuery] string organizationTypeCode,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.이용가능계약조회Async(
            organizationTypeCode,
            cancellationToken));

    [HttpPost("agreements/{agreementId:guid}/participations")]
    [SsalddelApiContractName("RegisterAgreementParticipation")]
    public async Task<IActionResult> 공급계약이용등록(
        Guid agreementId,
        [FromBody] 공급계약이용등록요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.공급계약이용등록Async(
            agreementId,
            request,
            cancellationToken));

    [HttpGet("orders")]
    [SsalddelApiContractName("GetOrganizationSupplyOrders")]
    public async Task<IActionResult> 발주목록조회(
        [FromQuery] 개별공급발주목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.발주목록조회Async(request, cancellationToken));

    [HttpPost("orders")]
    [SsalddelApiContractName("SubmitOrganizationSupplyOrder")]
    public async Task<IActionResult> 발주등록(
        [FromBody] 개별공급발주등록요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.발주등록Async(request, cancellationToken));

    [HttpPost("orders/{orderId:guid}/withdrawal")]
    [SsalddelApiContractName("WithdrawOrganizationSupplyOrder")]
    public async Task<IActionResult> 발주철회(
        Guid orderId,
        [FromBody] 개별공급발주철회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.발주철회Async(
            orderId,
            request,
            cancellationToken));
}
