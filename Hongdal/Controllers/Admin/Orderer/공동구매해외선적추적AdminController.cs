using System.Security.Claims;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.ApiMetadata;
using Hongdal.Filters;
using Hongdal.Services.Orderer;
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
    private readonly I공동구매해외선적추적저장소 _store;
    private readonly I공동구매해외선적통관동기화Service _customsSyncService;

    public 공동구매해외선적추적AdminController(
        I공동구매해외선적추적저장소 store,
        I공동구매해외선적통관동기화Service customsSyncService)
    {
        _store = store;
        _customsSyncService = customsSyncService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<공동구매해외선적추적Dto>>> List(
        [FromQuery] string? groupPurchaseId,
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? documentManagementNumber,
        [FromQuery] string? transportDocumentNumber,
        [FromQuery] string? currentStatusCode,
        CancellationToken cancellationToken = default)
    {
        var items = await _store.ListAsync(new 공동구매해외선적추적조회조건
        {
            공동구매Id = groupPurchaseId,
            주문자집단배송권키 = ordererGroupScopeKey,
            문서관리번호 = documentManagementNumber,
            운송문서번호 = transportDocumentNumber,
            현재상태코드 = currentStatusCode
        }, cancellationToken);

        return Ok(items);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Get(
        [FromQuery] string documentManagementNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetBy문서관리번호Async(documentManagementNumber, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("공동주문 해외 선적 추적 원장을 찾을 수 없습니다.")
                : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "문서관리번호가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] 공동구매해외선적추적저장요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.UpsertAsync(request, ResolveUserId(), cancellationToken);
            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "공동주문 해외 선적 원장 입력값이 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("events")]
    public async Task<IActionResult> AppendEvent(
        [FromQuery] string documentManagementNumber,
        [FromBody] 공동구매해외선적추적이벤트추가요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.AppendEventAsync(documentManagementNumber, request, ResolveUserId(), cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("공동주문 해외 선적 추적 원장을 찾을 수 없습니다.")
                : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "공동주문 해외 선적 이벤트 입력값이 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("customs-sync")]
    public async Task<IActionResult> SyncCustoms(
        [FromBody] 공동구매해외선적통관동기화요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _customsSyncService.SyncAsync(request, ResolveUserId(), cancellationToken);
            return result.선적 is null && !result.동기화됨
                ? this.ToNotFoundProblem(result.메시지)
                : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "공동주문 해외 선적 통관 동기화 입력값이 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
