using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Contracts.Common.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Audit;
using 홍달.Services.Warehouse;

namespace Hongdal.Controllers.Common;

[ApiController]
[Authorize(Policy = "운영사용자전용")]
[Route("api/v1/warehouse-operations")]
public sealed class WarehouseOperationsController : ControllerBase
{
    private readonly IWarehouseOperationService _warehouseOperationService;
    private readonly I사용자행위로그Service _activityLogService;

    public WarehouseOperationsController(IWarehouseOperationService warehouseOperationService, I사용자행위로그Service activityLogService)
    {
        _warehouseOperationService = warehouseOperationService;
        _activityLogService = activityLogService;
    }

    [HttpGet("warehouses")]
    public async Task<ActionResult<창고목록응답>> 창고목록(CancellationToken cancellationToken)
    {
        return Ok(await _warehouseOperationService.GetWarehousesAsync(cancellationToken));
    }

    [HttpPost("warehouses")]
    public async Task<ActionResult<창고요약응답>> 창고생성([FromBody] 창고저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CreateWarehouseAsync(request, cancellationToken);
        await LogAsync("Warehouse", "Created", result.Id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("warehouses/{warehouseId:long}/users")]
    public async Task<ActionResult<창고사용자목록응답>> 창고사용자목록(long warehouseId, CancellationToken cancellationToken)
    {
        return Ok(await _warehouseOperationService.GetWarehouseUsersAsync(warehouseId, cancellationToken));
    }

    [HttpPost("warehouses/{warehouseId:long}/users")]
    public async Task<ActionResult<창고사용자항목응답>> 창고사용자추가(long warehouseId, [FromBody] 창고사용자저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.AddWarehouseUserAsync(warehouseId, request, cancellationToken);
        await LogAsync("WarehouseUser", "Added", result.Id, cancellationToken, $"{{\"warehouseId\":{warehouseId},\"userId\":\"{result.UserId}\"}}");
        return Ok(result);
    }

    [HttpGet("inbounds")]
    public async Task<ActionResult<입고요청목록응답>> 입고목록(CancellationToken cancellationToken)
    {
        return Ok(await _warehouseOperationService.GetInboundsAsync(cancellationToken));
    }

    [HttpPost("inbounds")]
    public async Task<ActionResult<입고요청항목응답>> 입고생성([FromBody] 입고요청저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CreateInboundAsync(request, cancellationToken);
        await LogAsync("Inbound", "Created", result.Id, cancellationToken, $"{{\"warehouseId\":{result.창고Id}}}");
        return Ok(result);
    }

    [HttpPost("inbounds/{inboundId:long}/complete")]
    public async Task<ActionResult<입고상품목록응답>> 입고완료(long inboundId, [FromBody] 입고완료요청 request, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CompleteInboundAsync(inboundId, request, cancellationToken);
        await LogAsync("Inbound", "Completed", inboundId, cancellationToken, $"{{\"createdItems\":{result.Items.Count}}}");
        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<재고목록응답>> 재고목록(CancellationToken cancellationToken)
    {
        return Ok(await _warehouseOperationService.GetInventoryAsync(cancellationToken));
    }

    [HttpPost("inventory/reconsignment")]
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
