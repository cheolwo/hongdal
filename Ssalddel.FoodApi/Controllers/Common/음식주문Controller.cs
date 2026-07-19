using Ssalddel.Contracts.Food;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.FoodApi.Application;
using Ssalddel.FoodApi.Application.Orders.Commands;
using Ssalddel.FoodApi.Services;

namespace Ssalddel.FoodApi.Controllers.Common;

[ApiController]
[Route("api/v1/food-orders")]
[Obsolete("Food order endpoints are moving to the main Ssalddel server. Use Ssalddel.Controllers.Food endpoints.", false)]
public sealed class 음식주문Controller : ControllerBase
{
    private readonly 음식샘플Store _store;
    private readonly IFoodCommandHandler<음식주문등록Command, 음식주문응답> _createOrderHandler;
    private readonly IFoodCommandHandler<음식주문배차대기요청Command, 음식주문응답?> _requestDispatchHandler;

    public 음식주문Controller(
        음식샘플Store store,
        IFoodCommandHandler<음식주문등록Command, 음식주문응답> createOrderHandler,
        IFoodCommandHandler<음식주문배차대기요청Command, 음식주문응답?> requestDispatchHandler)
    {
        _store = store;
        _createOrderHandler = createOrderHandler;
        _requestDispatchHandler = requestDispatchHandler;
    }

    [HttpGet]
    public ActionResult<음식주문목록응답> 목록조회()
    {
        return Ok(_store.GetOrders());
    }

    [HttpPost]
    public async Task<ActionResult<음식주문응답>> 등록([FromBody] 음식주문등록요청 request, CancellationToken cancellationToken)
    {
        return Ok(await _createOrderHandler.HandleAsync(new 음식주문등록Command(request), cancellationToken));
    }

    [HttpPost("{orderNo}/dispatch-wait")]
    public async Task<IActionResult> 배차대기전환(string orderNo, CancellationToken cancellationToken)
    {
        var order = await _requestDispatchHandler.HandleAsync(new 음식주문배차대기요청Command(orderNo), cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}
