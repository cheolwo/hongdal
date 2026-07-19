using System.Security.Claims;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/group-purchase-commerce-fulfillment-plans")]
public sealed class 공동구매커머스이행계획AdminController : ControllerBase
{
    private readonly I공동구매커머스이행계획UseCase _useCase;

    public 공동구매커머스이행계획AdminController(I공동구매커머스이행계획UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? groupPurchaseId,
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? documentManagementNumber,
        [FromQuery] string? currentStatusCode,
        [FromQuery(Name = "salesChannelType")] string? 판매채널유형,
        [FromQuery] long? warehouseId,
        [FromQuery] long? inboundProductId,
        [FromQuery] bool? usePlatformLogisticsProxy,
        CancellationToken cancellationToken = default)
    {
        var result = await _useCase.목록조회Async(new 공동구매커머스이행계획조회조건
        {
            공동구매Id = groupPurchaseId,
            주문자집단배송권키 = ordererGroupScopeKey,
            문서관리번호 = documentManagementNumber,
            현재상태코드 = currentStatusCode,
            판매채널유형 = 판매채널유형,
            창고Id = warehouseId,
            입고상품Id = inboundProductId,
            플랫폼물류대행사용 = usePlatformLogisticsProxy
        }, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{planId}")]
    public async Task<IActionResult> Get(string planId, CancellationToken cancellationToken)
    {
        var result = await _useCase.단건조회Async(planId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("by-group-purchase/{groupPurchaseId}")]
    public async Task<IActionResult> ListByGroupPurchase(
        string groupPurchaseId,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.공동구매별목록조회Async(groupPurchaseId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] 공동구매커머스이행계획저장요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.저장Async(request, ResolveUserId(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{planId}/platform-domestic-transport-draft")]
    public async Task<IActionResult> 플랫폼국내운송초안생성(
        string planId,
        [FromBody] 공동구매플랫폼국내운송초안요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.플랫폼국내운송초안생성Async(planId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{planId}/platform-domestic-transport-dispatch-queue")]
    public async Task<IActionResult> 플랫폼국내운송배차대기생성(
        string planId,
        [FromBody] 공동구매플랫폼국내운송초안요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.플랫폼국내운송배차대기생성Async(planId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
