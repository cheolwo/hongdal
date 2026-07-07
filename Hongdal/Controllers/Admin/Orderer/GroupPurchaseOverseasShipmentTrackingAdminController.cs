using System.Security.Claims;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Orderer;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/group-purchase-overseas-shipments")]
public sealed class GroupPurchaseOverseasShipmentTrackingAdminController : ControllerBase
{
    private readonly IGroupPurchaseOverseasShipmentTrackingStore _store;
    private readonly IGroupPurchaseOverseasShipmentCustomsSyncService _customsSyncService;

    public GroupPurchaseOverseasShipmentTrackingAdminController(
        IGroupPurchaseOverseasShipmentTrackingStore store,
        IGroupPurchaseOverseasShipmentCustomsSyncService customsSyncService)
    {
        _store = store;
        _customsSyncService = customsSyncService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GroupPurchaseOverseasShipmentTrackingDto>>> List(
        [FromQuery] string? groupPurchaseId,
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? documentManagementNumber,
        [FromQuery] string? transportDocumentNumber,
        [FromQuery] string? currentStatusCode,
        CancellationToken cancellationToken = default)
    {
        var items = await _store.ListAsync(new GroupPurchaseOverseasShipmentTrackingQuery
        {
            GroupPurchaseId = groupPurchaseId,
            OrdererGroupScopeKey = ordererGroupScopeKey,
            DocumentManagementNumber = documentManagementNumber,
            TransportDocumentNumber = transportDocumentNumber,
            CurrentStatusCode = currentStatusCode
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
            var item = await _store.GetByDocumentManagementNumberAsync(documentManagementNumber, cancellationToken);
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
        [FromBody] GroupPurchaseOverseasShipmentTrackingUpsertRequest request,
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
        [FromBody] GroupPurchaseOverseasShipmentTrackingEventAppendRequest request,
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
        [FromBody] GroupPurchaseOverseasShipmentCustomsSyncRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _customsSyncService.SyncAsync(request, ResolveUserId(), cancellationToken);
            return result.Shipment is null && !result.Synced
                ? this.ToNotFoundProblem(result.Message)
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
