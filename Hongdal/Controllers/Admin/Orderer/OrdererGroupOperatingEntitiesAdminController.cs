using System.Security.Claims;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Orderer;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/orderer-group-operating-entities")]
public sealed class OrdererGroupOperatingEntitiesAdminController : ControllerBase
{
    private readonly IOrdererGroupOperatingEntityStore _store;

    public OrdererGroupOperatingEntitiesAdminController(IOrdererGroupOperatingEntityStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrdererGroupOperatingEntityDto>>> List(
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? entityType,
        [FromQuery] string? businessVerificationStatus,
        [FromQuery] bool? canActAsImporterOfRecord,
        [FromQuery] bool? canEmployWorkers,
        CancellationToken cancellationToken)
    {
        var items = await _store.ListAsync(new OrdererGroupOperatingEntityQuery
        {
            OrdererGroupScopeKey = ordererGroupScopeKey,
            EntityType = entityType,
            BusinessVerificationStatus = businessVerificationStatus,
            CanActAsImporterOfRecord = canActAsImporterOfRecord,
            CanEmployWorkers = canEmployWorkers
        }, cancellationToken);

        return Ok(items);
    }

    [HttpGet("{ordererGroupScopeKey}")]
    public async Task<IActionResult> Get(string ordererGroupScopeKey, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetByScopeKeyAsync(ordererGroupScopeKey, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("주문자 집단 운영 주체 프로필을 찾을 수 없습니다.")
                : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "주문자 집단 식별자가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] OrdererGroupOperatingEntityUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.UpsertAsync(request, ResolveUserId(), cancellationToken);
            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "주문자 집단 운영 주체 입력값이 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
