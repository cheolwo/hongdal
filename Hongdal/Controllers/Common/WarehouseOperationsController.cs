using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Hr;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Contracts.Common.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.Security;
using 홍달.Services.Audit;
using Hongdal.Services.LogisticsProcessing.Warehouse;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_5)]
[HongdalApiWorkflow(HongdalWorkflow.WarehouseFulfillment)]
[ApiController]
[Authorize(Policy = "운영사용자전용")]
[Route("api/v1/warehouse-operations")]
public sealed class WarehouseOperationsController : ControllerBase
{
    private readonly IWarehouseOperationService _warehouseOperationService;
    private readonly I사용자행위로그Service _activityLogService;

    public WarehouseOperationsController(
        IWarehouseOperationService warehouseOperationService,
        I사용자행위로그Service activityLogService)
    {
        _warehouseOperationService = warehouseOperationService;
        _activityLogService = activityLogService;
    }

    [HttpGet("warehouses")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInboundOperator,
        HrDetailedRoleCodes.WarehouseInventoryOperator,
        HrDetailedRoleCodes.WarehouseDispatchOperator)]
    public async Task<ActionResult<창고목록응답>> 창고목록(CancellationToken cancellationToken)
    {
        return Ok(await _warehouseOperationService.GetWarehousesAsync(cancellationToken));
    }

    [HttpPost("warehouses")]
    [RequireHrRole(HrDetailedRoleCodes.WarehouseManager)]
    public async Task<ActionResult<창고요약응답>> 창고생성([FromBody] 창고저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CreateWarehouseAsync(request, cancellationToken);
        await LogAsync("Warehouse", "Created", result.Id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("warehouses/{warehouseId:long}/users")]
    [RequireHrRole(HrDetailedRoleCodes.WarehouseManager)]
    public async Task<ActionResult<창고사용자목록응답>> 창고사용자목록(long warehouseId, CancellationToken cancellationToken)
    {
        return Ok(await _warehouseOperationService.GetWarehouseUsersAsync(warehouseId, cancellationToken));
    }

    [HttpPost("warehouses/{warehouseId:long}/users")]
    [RequireHrRole(HrDetailedRoleCodes.WarehouseManager)]
    public async Task<ActionResult<창고사용자항목응답>> 창고사용자추가(long warehouseId, [FromBody] 창고사용자저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.AddWarehouseUserAsync(warehouseId, request, cancellationToken);
        await LogAsync("WarehouseUser", "Added", result.Id, cancellationToken, $"{{\"warehouseId\":{warehouseId},\"userId\":\"{result.UserId}\"}}");
        return Ok(result);
    }

    [HttpGet("inbounds")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInboundOperator)]
    public async Task<ActionResult<입고요청목록응답>> 입고목록(CancellationToken cancellationToken)
    {
        return Ok(await _warehouseOperationService.GetInboundsAsync(cancellationToken));
    }

    [HttpPost("inbounds")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInboundOperator)]
    public async Task<ActionResult<입고요청항목응답>> 입고생성([FromBody] 입고요청저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CreateInboundAsync(request, cancellationToken);
        await LogAsync("Inbound", "Created", result.Id, cancellationToken, $"{{\"warehouseId\":{result.창고Id}}}");
        return Ok(result);
    }

    [HttpPost("inbounds/{inboundId:long}/complete")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInboundOperator)]
    public async Task<ActionResult<입고상품목록응답>> 입고완료(long inboundId, [FromBody] 입고완료요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CompleteInboundAsync(inboundId, request, cancellationToken);
        await LogAsync("Inbound", "Completed", inboundId, cancellationToken, $"{{\"createdItems\":{result.Items.Count}}}");
        return Ok(result);
    }

    [HttpGet("inventory")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInventoryOperator)]
    public async Task<ActionResult<재고목록응답>> 재고목록(CancellationToken cancellationToken)
    {
        return Ok(await _warehouseOperationService.GetInventoryAsync(cancellationToken));
    }

    [HttpPost("inventory/{inboundItemId:long}/inspect")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInboundOperator)]
    public async Task<ActionResult<창고작업결과응답>> 입고검수(long inboundItemId, [FromBody] 입고검수요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.InspectInboundItemAsync(inboundItemId, request, cancellationToken);
        await LogAsync("WarehouseWork", "InboundInspected", inboundItemId, cancellationToken, $"{{\"available\":{result.가용수량},\"defect\":{result.불량수량}}}");
        return Ok(result);
    }

    [HttpPost("inventory/{inboundItemId:long}/put-away")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInventoryOperator)]
    public async Task<ActionResult<창고작업결과응답>> 적재위치배정(long inboundItemId, [FromBody] 적재위치배정요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.PutAwayInventoryItemAsync(inboundItemId, request, cancellationToken);
        await LogAsync("WarehouseWork", "PutAwayCompleted", inboundItemId, cancellationToken, $"{{\"location\":\"{result.보관위치}\"}}");
        return Ok(result);
    }

    [HttpPost("inventory/{inboundItemId:long}/pack")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseDispatchOperator)]
    public async Task<ActionResult<창고작업결과응답>> 포장작업(long inboundItemId, [FromBody] 포장작업요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.PackInventoryItemAsync(inboundItemId, request, cancellationToken);
        await LogAsync("WarehouseWork", "Packed", inboundItemId, cancellationToken, $"{{\"quantity\":{request.포장수량}}}");
        return Ok(result);
    }

    [HttpPost("inventory/reconsignment")]
    [RequireHrRole(
        HrDetailedRoleCodes.WarehouseManager,
        HrDetailedRoleCodes.WarehouseInventoryOperator,
        HrDetailedRoleCodes.WarehouseDispatchOperator)]
    public async Task<ActionResult<화주운송의뢰응답>> 재위탁운송생성([FromBody] 재고운송의뢰생성요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CreateReconsignmentRequestAsync(request, cancellationToken);
        await LogAsync("Reconsignment", "Created", 0, cancellationToken, $"{{\"requestId\":\"{result.의뢰Id}\",\"inventoryItemId\":{request.입고상품Id},\"quantity\":{request.요청수량}}}");
        return Ok(result);
    }

    private async Task LogAsync(string actionType, string actionName, long entityId, CancellationToken cancellationToken, string? metadataJson = null)
    {
        await _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = Request.Headers["X-App-Key"].ToString(),
            UserId = User.Identity?.Name ?? string.Empty,
            UserName = User.Identity?.Name ?? string.Empty,
            RoleName = User.Claims.FirstOrDefault(x => x.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
            ActionType = actionType,
            ActionName = actionName,
            Route = Request.Path.Value ?? string.Empty,
            TraceId = HttpContext.TraceIdentifier,
            IsSuccess = true,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString(),
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = metadataJson ?? $"{{\"entityId\":{entityId}}}"
        }, cancellationToken);
    }

}
