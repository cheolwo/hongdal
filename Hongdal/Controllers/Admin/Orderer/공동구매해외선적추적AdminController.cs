using System.Security.Claims;
using Hongdal.Application.Orderer;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.ApiMetadata;
using Hongdal.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Admin.Orderer;

[ApiController]
[HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/group-purchase-overseas-shipments")]
public sealed class 공동구매해외선적추적AdminController : ControllerBase
{
    private readonly I공동구매해외선적추적UseCase _useCase;

    public 공동구매해외선적추적AdminController(I공동구매해외선적추적UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? groupPurchaseId,
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? documentManagementNumber,
        [FromQuery] string? transportDocumentNumber,
        [FromQuery] string? currentStatusCode,
        CancellationToken cancellationToken = default)
    {
        var result = await _useCase.목록Async(new 공동구매해외선적추적조회조건
        {
            공동구매Id = groupPurchaseId,
            주문자집단배송권키 = ordererGroupScopeKey,
            문서관리번호 = documentManagementNumber,
            운송문서번호 = transportDocumentNumber,
            현재상태코드 = currentStatusCode
        }, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Get(
        [FromQuery] string documentManagementNumber,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.관리자조회Async(documentManagementNumber, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] 공동구매해외선적추적저장요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.저장Async(request, ResolveUserId(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("events")]
    public async Task<IActionResult> AppendEvent(
        [FromQuery] string documentManagementNumber,
        [FromBody] 공동구매해외선적추적이벤트추가요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.이벤트추가Async(documentManagementNumber, request, ResolveUserId(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("customs-sync")]
    public async Task<IActionResult> SyncCustoms(
        [FromBody] 공동구매해외선적통관동기화요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.통관동기화Async(request, ResolveUserId(), cancellationToken);
        return this.ToActionResult(result);
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
