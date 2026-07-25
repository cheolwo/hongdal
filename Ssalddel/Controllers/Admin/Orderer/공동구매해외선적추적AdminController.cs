using System.Security.Claims;
using Ssalddel.Application.Orderer;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_0, FeatureKey = VersionFeatureFlagKeys.DomesticTransportWorkflow, WorkflowKey = VersionFeatureFlagKeys.DomesticTransportWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.DomesticTransportWorkflow)]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/group-purchase-overseas-shipments")]
public sealed class 공동구매해외선적추적AdminController : ControllerBase
{
    private readonly I공동구매해외선적추적UseCase _해외선적추적UseCase;

    public 공동구매해외선적추적AdminController(I공동구매해외선적추적UseCase 해외선적추적UseCase)
    {
        _해외선적추적UseCase = 해외선적추적UseCase;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? groupPurchaseId,
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? documentManagementNumber,
        [FromQuery] string? transportDocumentNumber,
        [FromQuery] string? currentStatusCode,
        CancellationToken cancellationToken = default)
    {
        var result = await _해외선적추적UseCase.목록Async(new 공동구매해외선적추적조회조건
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
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 상세조회(
        [FromQuery] string documentManagementNumber,
        CancellationToken cancellationToken)
    {
        var result = await _해외선적추적UseCase.관리자조회Async(documentManagementNumber, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [SsalddelApiContractName("Upsert")]
    public async Task<IActionResult> 등록또는수정(
        [FromBody] 공동구매해외선적추적저장요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _해외선적추적UseCase.저장Async(request, ResolveUserId(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("events")]
    [SsalddelApiContractName("AppendEvent")]
    public async Task<IActionResult> Event추가(
        [FromQuery] string documentManagementNumber,
        [FromBody] 공동구매해외선적추적이벤트추가요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _해외선적추적UseCase.이벤트추가Async(documentManagementNumber, request, ResolveUserId(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("customs-sync")]
    [SsalddelApiContractName("SyncCustoms")]
    public async Task<IActionResult> 세관동기화(
        [FromBody] 공동구매해외선적통관동기화요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _해외선적추적UseCase.통관동기화Async(request, ResolveUserId(), cancellationToken);
        return this.ToActionResult(result);
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
