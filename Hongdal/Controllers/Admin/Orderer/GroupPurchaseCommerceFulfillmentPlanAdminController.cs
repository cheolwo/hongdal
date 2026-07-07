using System.Security.Claims;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Orderer;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/group-purchase-commerce-fulfillment-plans")]
public sealed class GroupPurchaseCommerceFulfillmentPlanAdminController : ControllerBase
{
    private readonly IGroupPurchaseCommerceFulfillmentPlanStore _store;

    public GroupPurchaseCommerceFulfillmentPlanAdminController(IGroupPurchaseCommerceFulfillmentPlanStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GroupPurchaseCommerceFulfillmentPlanDto>>> List(
        [FromQuery] string? groupPurchaseId,
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? documentManagementNumber,
        [FromQuery] string? currentStatusCode,
        [FromQuery] string? salesChannelType,
        [FromQuery] long? warehouseId,
        [FromQuery] long? inboundProductId,
        [FromQuery] bool? usePlatformLogisticsProxy,
        CancellationToken cancellationToken = default)
    {
        var items = await _store.ListAsync(new GroupPurchaseCommerceFulfillmentPlanQuery
        {
            GroupPurchaseId = groupPurchaseId,
            OrdererGroupScopeKey = ordererGroupScopeKey,
            DocumentManagementNumber = documentManagementNumber,
            CurrentStatusCode = currentStatusCode,
            SalesChannelType = salesChannelType,
            WarehouseId = warehouseId,
            InboundProductId = inboundProductId,
            UsePlatformLogisticsProxy = usePlatformLogisticsProxy
        }, cancellationToken);

        return Ok(items);
    }

    [HttpGet("{planId}")]
    public async Task<IActionResult> Get(string planId, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetAsync(planId, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("공동주문 커머스 풀필먼트 플랜을 찾을 수 없습니다.")
                : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "공동주문 커머스 풀필먼트 플랜 식별자가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("by-group-purchase/{groupPurchaseId}")]
    public async Task<ActionResult<IReadOnlyList<GroupPurchaseCommerceFulfillmentPlanDto>>> ListByGroupPurchase(
        string groupPurchaseId,
        CancellationToken cancellationToken)
    {
        var items = await _store.ListAsync(new GroupPurchaseCommerceFulfillmentPlanQuery
        {
            GroupPurchaseId = groupPurchaseId
        }, cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] GroupPurchaseCommerceFulfillmentPlanUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.UpsertAsync(request, ResolveUserId(), cancellationToken);
            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "공동주문 커머스 풀필먼트 플랜 입력값이 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
