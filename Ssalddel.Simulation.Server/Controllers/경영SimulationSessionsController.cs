using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions")]
public sealed class 경영SimulationSessionsController(
    경영SimulationSessionService service,
    SimulationWorldUIProjectionService? projectionService = null) : ControllerBase
{
    private readonly SimulationWorldUIProjectionService worldUiProjectionService =
        projectionService ?? new SimulationWorldUIProjectionService(service);

    [HttpPost]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status201Created)]
    public ActionResult<경영SimulationSessionSnapshot> Create(
        [FromBody] 경영SimulationSession생성Request request)
        => Execute(() =>
        {
            var result = service.Create(request);
            return CreatedAtAction(nameof(Get), new { sessionStableId = result.SessionStableId }, result);
        });

    [HttpGet("{sessionStableId}")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> Get(string sessionStableId)
        => Execute(() => Ok(service.Get(sessionStableId)));

    [HttpGet("{sessionStableId}/world-ui/surfaces/{surfaceStableId}")]
    [ProducesResponseType(typeof(SimulationWorldUIProjection), StatusCodes.Status200OK)]
    public ActionResult<SimulationWorldUIProjection> GetWorldUiSurface(
        string sessionStableId,
        string surfaceStableId)
        => ExecuteWorldUiProjection(() => Ok(
            worldUiProjectionService.Get(sessionStableId, surfaceStableId)));

    [HttpPost("{sessionStableId}/ticks")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> Advance(
        string sessionStableId,
        [FromBody] 경영SimulationTick진행Request request)
        => Execute(() => Ok(service.Advance(sessionStableId, request)));

    [HttpGet("{sessionStableId}/turn-closing-context")]
    [ProducesResponseType(typeof(SimulationTurnClosingContextSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationTurnClosingContextSnapshot> GetTurnClosingContext(
        string sessionStableId)
        => ExecuteTurnClosingContext(() => Ok(
            service.GetTurnClosingContext(sessionStableId)));

    [HttpPost("{sessionStableId}/turn-closing-previews")]
    [ProducesResponseType(typeof(SimulationTurnClosingPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationTurnClosingPreviewSnapshot> PreviewTurnClosing(
        string sessionStableId,
        [FromBody] SimulationTurnClosingPreviewRequest request)
        => ExecuteTurnClosingPreview(() => Ok(
            service.PreviewTurnClosing(sessionStableId, request)));

    [HttpPost("{sessionStableId}/turn-closings/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmTurnClosing(
        string sessionStableId,
        [FromBody] SimulationTurnClosingConfirmRequest request)
        => Execute(() => Ok(service.ConfirmTurnClosing(sessionStableId, request)));

    [HttpPost("{sessionStableId}/decision-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewDecision(
        string sessionStableId,
        [FromBody] SimulationDecisionPreviewRequest request)
        => ExecutePreview(() => Ok(service.PreviewDecision(sessionStableId, request)));

    [HttpPost("{sessionStableId}/decisions/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmDecision(
        string sessionStableId,
        [FromBody] SimulationDecisionConfirmRequest request)
        => Execute(() => Ok(service.ConfirmDecision(sessionStableId, request)));

    [HttpPost("{sessionStableId}/npc-policies")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> UpdateNpcPolicy(
        string sessionStableId,
        [FromBody] SimulationNpcPolicyChangeRequest request)
        => Execute(() => Ok(service.UpdateNpcPolicy(sessionStableId, request)));

    [HttpPost("{sessionStableId}/saves")]
    [ProducesResponseType(typeof(SimulationSessionSavePackage), StatusCodes.Status200OK)]
    public ActionResult<SimulationSessionSavePackage> Save(
        string sessionStableId,
        [FromBody] SimulationSessionSaveRequest request)
        => ExecuteSave(() => Ok(service.Save(sessionStableId, request)));

    [HttpPost("restores")]
    [ProducesResponseType(typeof(SimulationSessionRestoreResult), StatusCodes.Status200OK)]
    public ActionResult<SimulationSessionRestoreResult> Restore(
        [FromBody] SimulationSessionRestoreRequest request)
        => ExecuteRestore(() => Ok(service.Restore(request)));

    [HttpPost("{sessionStableId}/harvest-disposition-impact-previews")]
    [ProducesResponseType(
        typeof(SimulationHarvestDispositionImpactPreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationHarvestDispositionImpactPreviewSnapshot> PreviewHarvestImpact(
        string sessionStableId,
        [FromBody] SimulationHarvestDispositionImpactPreviewRequest request)
        => ExecuteHarvestImpact(() => Ok(
            service.PreviewHarvestDispositionImpact(sessionStableId, request)));

    [HttpPost("{sessionStableId}/harvest-disposition-impacts/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmHarvestImpact(
        string sessionStableId,
        [FromBody] SimulationHarvestDispositionImpactConfirmRequest request)
        => Execute(() => Ok(service.ConfirmHarvestDispositionImpact(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-preparation-previews")]
    [ProducesResponseType(typeof(Simulation수출준비PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수출준비PreviewSnapshot> 수출준비Preview(
        string sessionStableId,
        [FromBody] Simulation수출준비PreviewRequest request)
        => Execute수출준비(() => Ok(service.Preview수출준비(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-preparations/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> 수출준비Confirm(
        string sessionStableId,
        [FromBody] Simulation수출준비ConfirmRequest request)
        => Execute(() => Ok(service.Confirm수출준비(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-rework-previews")]
    [ProducesResponseType(typeof(Simulation수출준비PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수출준비PreviewSnapshot> 수출재작업Preview(
        string sessionStableId,
        [FromBody] Simulation수출재작업PreviewRequest request)
        => Execute수출준비(() => Ok(service.Preview수출재작업(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-reworks/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> 수출재작업Confirm(
        string sessionStableId,
        [FromBody] Simulation수출재작업ConfirmRequest request)
        => Execute(() => Ok(service.Confirm수출재작업(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-cargo-preparation-previews")]
    [ProducesResponseType(typeof(Simulation수출Cargo준비PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수출Cargo준비PreviewSnapshot> 수출Cargo준비Preview(
        string sessionStableId,
        [FromBody] Simulation수출Cargo준비PreviewRequest request)
        => Execute수출Cargo준비(() => Ok(service.Preview수출Cargo준비(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-cargo-preparations/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> 수출Cargo준비Confirm(
        string sessionStableId,
        [FromBody] Simulation수출Cargo준비ConfirmRequest request)
        => Execute(() => Ok(service.Confirm수출Cargo준비(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-cargo-handoff-previews")]
    [ProducesResponseType(typeof(Simulation수출Cargo인계PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수출Cargo인계PreviewSnapshot> 수출Cargo인계Preview(
        string sessionStableId,
        [FromBody] Simulation수출Cargo인계PreviewRequest request)
        => Execute수출Cargo인계(() => Ok(service.Preview수출Cargo인계(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-cargo-handoffs/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> 수출Cargo인계Confirm(
        string sessionStableId,
        [FromBody] Simulation수출Cargo인계ConfirmRequest request)
        => Execute(() => Ok(service.Confirm수출Cargo인계(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-port-receipt-previews")]
    [ProducesResponseType(typeof(Simulation수출항만인수PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수출항만인수PreviewSnapshot> 수출항만인수Preview(
        string sessionStableId,
        [FromBody] Simulation수출항만인수PreviewRequest request)
        => Execute수출항만인수(() => Ok(service.Preview수출항만인수(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-port-receipts/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> 수출항만인수Confirm(
        string sessionStableId,
        [FromBody] Simulation수출항만인수ConfirmRequest request)
        => Execute(() => Ok(service.Confirm수출항만인수(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-readiness-review-previews")]
    [ProducesResponseType(typeof(Simulation수출준비성검토PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수출준비성검토PreviewSnapshot> 수출준비성검토Preview(
        string sessionStableId,
        [FromBody] Simulation수출준비성검토PreviewRequest request)
        => Execute수출준비성검토(() => Ok(
            service.Preview수출준비성검토(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-readiness-reviews/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> 수출준비성검토Confirm(
        string sessionStableId,
        [FromBody] Simulation수출준비성검토ConfirmRequest request)
        => Execute(() => Ok(service.Confirm수출준비성검토(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-shipment-plan-previews")]
    [ProducesResponseType(typeof(Simulation수출선적계획PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수출선적계획PreviewSnapshot> 수출선적계획Preview(
        string sessionStableId,
        [FromBody] Simulation수출선적계획PreviewRequest request)
        => Execute수출선적계획(() => Ok(
            service.Preview수출선적계획(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-shipment-plans/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> 수출선적계획Confirm(
        string sessionStableId,
        [FromBody] Simulation수출선적계획ConfirmRequest request)
        => Execute(() => Ok(service.Confirm수출선적계획(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-shipment-execution-previews")]
    [ProducesResponseType(typeof(Simulation수출선적실행PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수출선적실행PreviewSnapshot> 수출선적실행Preview(
        string sessionStableId,
        [FromBody] Simulation수출선적실행PreviewRequest request)
        => Execute수출선적실행(() => Ok(
            service.Preview수출선적실행(sessionStableId, request)));

    [HttpPost("{sessionStableId}/export-shipment-executions/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> 수출선적실행Confirm(
        string sessionStableId,
        [FromBody] Simulation수출선적실행ConfirmRequest request)
        => Execute(() => Ok(service.Confirm수출선적실행(sessionStableId, request)));

    [HttpGet("{sessionStableId}/harvest-route-outcomes")]
    [ProducesResponseType(typeof(Simulation수확판로결과Snapshot[]), StatusCodes.Status200OK)]
    public ActionResult<Simulation수확판로결과Snapshot[]> 수확판로결과목록(
        string sessionStableId)
        => Execute수확판로결과목록(() => Ok(service.Get수확판로결과목록(sessionStableId)));

    [HttpGet("{sessionStableId}/harvest-route-outcomes/{harvestLotStableId}")]
    [ProducesResponseType(typeof(Simulation수확판로결과Snapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation수확판로결과Snapshot> 수확판로결과(
        string sessionStableId,
        string harvestLotStableId)
        => Execute수확판로결과(() => Ok(
            service.Get수확판로결과(sessionStableId, harvestLotStableId)));

    [HttpPost("{sessionStableId}/logistics-movement-previews")]
    [ProducesResponseType(
        typeof(SimulationLogisticsMovementPreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationLogisticsMovementPreviewSnapshot> PreviewLogisticsMovement(
        string sessionStableId,
        [FromBody] SimulationLogisticsMovementPreviewRequest request)
        => ExecuteLogisticsMovement(() => Ok(
            service.PreviewLogisticsMovement(sessionStableId, request)));

    [HttpPost("{sessionStableId}/logistics-movements/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmLogisticsMovement(
        string sessionStableId,
        [FromBody] SimulationLogisticsMovementConfirmRequest request)
        => Execute(() => Ok(service.ConfirmLogisticsMovement(sessionStableId, request)));

    [HttpPost("{sessionStableId}/freight-dispatch-previews")]
    [ProducesResponseType(
        typeof(SimulationFreightDispatchPreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFreightDispatchPreviewSnapshot> PreviewFreightDispatch(
        string sessionStableId,
        [FromBody] SimulationFreightDispatchPreviewRequest request)
        => ExecuteFreightDispatch(() => Ok(
            service.PreviewFreightDispatch(sessionStableId, request)));

    [HttpPost("{sessionStableId}/freight-dispatches/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmFreightDispatch(
        string sessionStableId,
        [FromBody] SimulationFreightDispatchConfirmRequest request)
        => Execute(() => Ok(service.ConfirmFreightDispatch(sessionStableId, request)));

    [HttpPost("{sessionStableId}/freight-transport-previews")]
    [ProducesResponseType(
        typeof(SimulationFreightTransportPreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFreightTransportPreviewSnapshot> PreviewFreightTransport(
        string sessionStableId,
        [FromBody] SimulationFreightTransportPreviewRequest request)
        => ExecuteFreightTransport(() => Ok(
            service.PreviewFreightTransport(sessionStableId, request)));

    [HttpPost("{sessionStableId}/freight-transports/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmFreightTransport(
        string sessionStableId,
        [FromBody] SimulationFreightTransportConfirmRequest request)
        => Execute(() => Ok(service.ConfirmFreightTransport(sessionStableId, request)));

    [HttpPost("{sessionStableId}/freight-receipt-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewFreightReceipt(
        string sessionStableId,
        [FromBody] SimulationFreightReceiptPreviewRequest request)
        => ExecutePreview(() => Ok(service.PreviewFreightReceipt(sessionStableId, request)));

    [HttpPost("{sessionStableId}/freight-receipts/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmFreightReceipt(
        string sessionStableId,
        [FromBody] SimulationFreightReceiptConfirmRequest request)
        => Execute(() => Ok(service.ConfirmFreightReceipt(sessionStableId, request)));

    [HttpPost("{sessionStableId}/warehouse-put-away-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewWarehousePutAway(
        string sessionStableId,
        [FromBody] SimulationWarehousePutAwayPreviewRequest request)
        => ExecutePreview(() => Ok(service.PreviewWarehousePutAway(sessionStableId, request)));

    [HttpPost("{sessionStableId}/warehouse-put-aways/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmWarehousePutAway(
        string sessionStableId,
        [FromBody] SimulationWarehousePutAwayConfirmRequest request)
        => Execute(() => Ok(service.ConfirmWarehousePutAway(sessionStableId, request)));

    [HttpPost("{sessionStableId}/group-order-previews")]
    [ProducesResponseType(typeof(Simulation같이주문PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation같이주문PreviewSnapshot> PreviewGroupOrder(
        string sessionStableId,
        [FromBody] Simulation같이주문PreviewRequest request)
        => ExecuteGroupOrder(() => Ok(service.PreviewGroupOrder(sessionStableId, request)));

    [HttpPost("{sessionStableId}/group-orders/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmGroupOrder(
        string sessionStableId,
        [FromBody] Simulation같이주문ConfirmRequest request)
        => Execute(() => Ok(service.ConfirmGroupOrder(sessionStableId, request)));

    [HttpPost("{sessionStableId}/food-delivery-previews")]
    [ProducesResponseType(typeof(Simulation음식배달PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation음식배달PreviewSnapshot> PreviewFoodDelivery(
        string sessionStableId,
        [FromBody] Simulation음식배달PreviewRequest request)
        => ExecuteFoodDelivery(() => Ok(service.PreviewFoodDelivery(sessionStableId, request)));

    [HttpPost("{sessionStableId}/food-deliveries/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmFoodDelivery(
        string sessionStableId,
        [FromBody] Simulation음식배달ConfirmRequest request)
        => Execute(() => Ok(service.ConfirmFoodDelivery(sessionStableId, request)));

    [HttpPost("{sessionStableId}/food-delivery-receipt-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewFoodDeliveryReceipt(
        string sessionStableId,
        [FromBody] Simulation음식배달수령PreviewRequest request)
        => ExecutePreview(() => Ok(service.PreviewFoodDeliveryReceipt(sessionStableId, request)));

    [HttpPost("{sessionStableId}/food-delivery-receipts/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmFoodDeliveryReceipt(
        string sessionStableId,
        [FromBody] Simulation음식배달수령ConfirmRequest request)
        => Execute(() => Ok(service.ConfirmFoodDeliveryReceipt(sessionStableId, request)));

    [HttpPost("{sessionStableId}/market-consumption-previews")]
    [ProducesResponseType(typeof(Simulation시장소비PreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<Simulation시장소비PreviewSnapshot> PreviewMarketConsumption(
        string sessionStableId,
        [FromBody] Simulation시장소비PreviewRequest request)
        => ExecuteMarketConsumption(() => Ok(
            service.PreviewMarketConsumption(sessionStableId, request)));

    [HttpPost("{sessionStableId}/market-consumptions/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmMarketConsumption(
        string sessionStableId,
        [FromBody] Simulation시장소비ConfirmRequest request)
        => Execute(() => Ok(service.ConfirmMarketConsumption(sessionStableId, request)));

    [HttpPost("{sessionStableId}/individual-order-previews")]
    [ProducesResponseType(
        typeof(SimulationIndividualOrderPreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationIndividualOrderPreviewSnapshot> PreviewIndividualOrder(
        string sessionStableId,
        [FromBody] SimulationIndividualOrderPreviewRequest request)
        => ExecuteIndividualOrder(() => Ok(
            service.PreviewIndividualOrder(sessionStableId, request)));

    [HttpPost("{sessionStableId}/individual-orders/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmIndividualOrder(
        string sessionStableId,
        [FromBody] SimulationIndividualOrderConfirmRequest request)
        => Execute(() => Ok(service.ConfirmIndividualOrder(sessionStableId, request)));

    [HttpPost("{sessionStableId}/individual-order-cancellation-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewIndividualOrderCancellation(
        string sessionStableId,
        [FromBody] SimulationIndividualOrderCancelRequest request)
        => ExecutePreview(() => Ok(
            service.PreviewIndividualOrderCancellation(sessionStableId, request)));

    [HttpPost("{sessionStableId}/individual-order-cancellations/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmIndividualOrderCancellation(
        string sessionStableId,
        [FromBody] SimulationIndividualOrderCancelRequest request)
        => Execute(() => Ok(
            service.ConfirmIndividualOrderCancellation(sessionStableId, request)));

    private ActionResult<경영SimulationSessionSnapshot> Execute(
        Func<ActionResult<경영SimulationSessionSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationDecisionPreviewSnapshot> ExecutePreview(
        Func<ActionResult<SimulationDecisionPreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationTurnClosingContextSnapshot> ExecuteTurnClosingContext(
        Func<ActionResult<SimulationTurnClosingContextSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationTurnClosingPreviewSnapshot> ExecuteTurnClosingPreview(
        Func<ActionResult<SimulationTurnClosingPreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수출준비PreviewSnapshot> Execute수출준비(
        Func<ActionResult<Simulation수출준비PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수출Cargo준비PreviewSnapshot> Execute수출Cargo준비(
        Func<ActionResult<Simulation수출Cargo준비PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수출Cargo인계PreviewSnapshot> Execute수출Cargo인계(
        Func<ActionResult<Simulation수출Cargo인계PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation음식배달PreviewSnapshot> ExecuteFoodDelivery(
        Func<ActionResult<Simulation음식배달PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation시장소비PreviewSnapshot> ExecuteMarketConsumption(
        Func<ActionResult<Simulation시장소비PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationSessionSavePackage> ExecuteSave(
        Func<ActionResult<SimulationSessionSavePackage>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationSessionRestoreResult> ExecuteRestore(
        Func<ActionResult<SimulationSessionRestoreResult>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationHarvestDispositionImpactPreviewSnapshot> ExecuteHarvestImpact(
        Func<ActionResult<SimulationHarvestDispositionImpactPreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationLogisticsMovementPreviewSnapshot> ExecuteLogisticsMovement(
        Func<ActionResult<SimulationLogisticsMovementPreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수출항만인수PreviewSnapshot> Execute수출항만인수(
        Func<ActionResult<Simulation수출항만인수PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수출준비성검토PreviewSnapshot> Execute수출준비성검토(
        Func<ActionResult<Simulation수출준비성검토PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수출선적계획PreviewSnapshot> Execute수출선적계획(
        Func<ActionResult<Simulation수출선적계획PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수출선적실행PreviewSnapshot> Execute수출선적실행(
        Func<ActionResult<Simulation수출선적실행PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수확판로결과Snapshot> Execute수확판로결과(
        Func<ActionResult<Simulation수확판로결과Snapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation수확판로결과Snapshot[]> Execute수확판로결과목록(
        Func<ActionResult<Simulation수확판로결과Snapshot[]>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationFreightTransportPreviewSnapshot> ExecuteFreightTransport(
        Func<ActionResult<SimulationFreightTransportPreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationWorldUIProjection> ExecuteWorldUiProjection(
        Func<ActionResult<SimulationWorldUIProjection>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationFreightDispatchPreviewSnapshot> ExecuteFreightDispatch(
        Func<ActionResult<SimulationFreightDispatchPreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<Simulation같이주문PreviewSnapshot> ExecuteGroupOrder(
        Func<ActionResult<Simulation같이주문PreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    private ActionResult<SimulationIndividualOrderPreviewSnapshot> ExecuteIndividualOrder(
        Func<ActionResult<SimulationIndividualOrderPreviewSnapshot>> action)
    {
        try
        {
            return action();
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }
}
