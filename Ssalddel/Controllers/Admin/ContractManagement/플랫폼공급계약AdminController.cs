using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.ContractManagement;
using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.ContractManagement;

[SsalddelApiVersion(
    SsalddelProductVersion.V3_5,
    FeatureKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelApiCapability(SsalddelCapability.OrderParticipation)]
[SsalddelApiAudience(SsalddelActor.PlatformOperator)]
[SsalddelApiOperation(SsalddelOperation.Manage)]
[RequireVersionFeature(VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/supply-brokerage")]
[SsalddelApiContractName("PlatformSupplyBrokerageAdminController")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
    SsalddelCodeLayer.Api,
    "공급조건 계약 활성화와 공급자 응답 증거 기록 API를 제공합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(플랫폼공급계약등록요청),
    FlowOrder = 70,
    Boundary = "플랫폼 운영자는 공급조건 계약과 공급자 응답 증거만 관리하며 거래 당사자의 결제·재고를 실행하지 않습니다.")]
public sealed class 플랫폼공급계약AdminController(
    I플랫폼공급계약관리UseCase useCase) : ControllerBase
{
    [HttpPost("agreements")]
    [SsalddelApiContractName("CreateSupplyAgreementDraft")]
    public async Task<IActionResult> 계약등록(
        [FromBody] 플랫폼공급계약등록요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.등록Async(request, cancellationToken));

    [HttpPost("agreements/{agreementId:guid}/activation")]
    [SsalddelApiContractName("ActivateSupplyAgreement")]
    public async Task<IActionResult> 계약활성화(
        Guid agreementId,
        [FromBody] 플랫폼공급계약활성화요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.활성화Async(
            agreementId,
            request,
            cancellationToken));

    [HttpPost("orders/{orderId:guid}/supplier-response")]
    [SsalddelApiContractName("RecordSupplierOrderResponse")]
    public async Task<IActionResult> 공급자응답기록(
        Guid orderId,
        [FromBody] 개별공급발주공급자응답기록요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.공급자응답기록Async(
            orderId,
            request,
            cancellationToken));
}
