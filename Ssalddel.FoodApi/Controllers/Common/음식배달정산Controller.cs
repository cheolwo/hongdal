using Ssalddel.FoodApi.Application;
using Ssalddel.FoodApi.Application.DeliveryTickets.Commands;
using Ssalddel.FoodApi.Application.Settlements;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.FoodApi.Controllers.Common;

[ApiController]
[Route("api/v1/food-delivery-settlements")]
public sealed class 음식배달정산Controller : ControllerBase
{
    private readonly IFoodCommandHandler<음식배달완료정산반영Command, FoodDeliverySettlementApplyResult> _completeHandler;
    private readonly IFoodDeliverySettlementStore _settlementStore;

    public 음식배달정산Controller(
        IFoodCommandHandler<음식배달완료정산반영Command, FoodDeliverySettlementApplyResult> completeHandler,
        IFoodDeliverySettlementStore settlementStore)
    {
        _completeHandler = completeHandler;
        _settlementStore = settlementStore;
    }

    [HttpPost("complete")]
    public async Task<ActionResult<FoodDeliverySettlementApplyResult>> 배달완료정산반영(
        [FromBody] 음식배달완료정산반영요청 request,
        CancellationToken cancellationToken)
    {
        var result = await _completeHandler.HandleAsync(
            new 음식배달완료정산반영Command(
                request.TicketId,
                request.DriverId,
                request.ActualDistanceMeters,
                request.CompletedAtUtc),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("drivers/{driverId}/daily")]
    public ActionResult<FoodDeliverySettlementSummary> 일별조회(string driverId, [FromQuery] DateOnly? date)
    {
        return Ok(_settlementStore.GetDaily(driverId, date ?? DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [HttpGet("drivers/{driverId}/weekly")]
    public ActionResult<FoodDeliverySettlementSummary> 주별조회(string driverId, [FromQuery] DateOnly? date)
    {
        return Ok(_settlementStore.GetWeekly(driverId, date ?? DateOnly.FromDateTime(DateTime.UtcNow)));
    }
}

public sealed class 음식배달완료정산반영요청
{
    public string TicketId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public int? ActualDistanceMeters { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
