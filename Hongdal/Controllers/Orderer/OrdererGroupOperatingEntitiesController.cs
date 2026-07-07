using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Orderer;

[ApiController]
[Route("api/v1/orderer/orderer-group-operating-entities")]
public sealed class OrdererGroupOperatingEntitiesController : ControllerBase
{
    private readonly IOrdererGroupOperatingEntityStore _store;

    public OrdererGroupOperatingEntitiesController(IOrdererGroupOperatingEntityStore store)
    {
        _store = store;
    }

    [HttpGet("{ordererGroupScopeKey}")]
    public async Task<IActionResult> Get(string ordererGroupScopeKey, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetByScopeKeyAsync(ordererGroupScopeKey, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("주문자 집단 운영 주체 프로필을 찾을 수 없습니다.")
                : Ok(OrdererGroupOperatingEntityProjection.ToPublicDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "주문자 집단 식별자가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
