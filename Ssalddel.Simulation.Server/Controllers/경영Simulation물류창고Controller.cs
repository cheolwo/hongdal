using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions")]
public sealed class 경영Simulation물류창고Controller(
    경영Simulation물류창고Service service) : ControllerBase
{
    [HttpPost("{sessionStableId}/logistics-movement-previews")]
    [ProducesResponseType(typeof(SimulationLogisticsMovementPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationLogisticsMovementPreviewSnapshot> PreviewLogisticsMovement(
        string sessionStableId,
        [FromBody] SimulationLogisticsMovementPreviewRequest request)
        => Ok(service.PreviewLogisticsMovement(sessionStableId, request));

    [HttpPost("{sessionStableId}/logistics-movements/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmLogisticsMovement(
        string sessionStableId,
        [FromBody] SimulationLogisticsMovementConfirmRequest request)
        => Ok(service.ConfirmLogisticsMovement(sessionStableId, request));

    [HttpPost("{sessionStableId}/freight-dispatch-previews")]
    [ProducesResponseType(typeof(SimulationFreightDispatchPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationFreightDispatchPreviewSnapshot> PreviewFreightDispatch(
        string sessionStableId,
        [FromBody] SimulationFreightDispatchPreviewRequest request)
        => Ok(service.PreviewFreightDispatch(sessionStableId, request));

    [HttpPost("{sessionStableId}/freight-dispatches/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmFreightDispatch(
        string sessionStableId,
        [FromBody] SimulationFreightDispatchConfirmRequest request)
        => Ok(service.ConfirmFreightDispatch(sessionStableId, request));

    [HttpPost("{sessionStableId}/freight-transport-previews")]
    [ProducesResponseType(typeof(SimulationFreightTransportPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationFreightTransportPreviewSnapshot> PreviewFreightTransport(
        string sessionStableId,
        [FromBody] SimulationFreightTransportPreviewRequest request)
        => Ok(service.PreviewFreightTransport(sessionStableId, request));

    [HttpPost("{sessionStableId}/freight-transports/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmFreightTransport(
        string sessionStableId,
        [FromBody] SimulationFreightTransportConfirmRequest request)
        => Ok(service.ConfirmFreightTransport(sessionStableId, request));

    [HttpPost("{sessionStableId}/freight-receipt-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewFreightReceipt(
        string sessionStableId,
        [FromBody] SimulationFreightReceiptPreviewRequest request)
        => Ok(service.PreviewFreightReceipt(sessionStableId, request));

    [HttpPost("{sessionStableId}/freight-receipts/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmFreightReceipt(
        string sessionStableId,
        [FromBody] SimulationFreightReceiptConfirmRequest request)
        => Ok(service.ConfirmFreightReceipt(sessionStableId, request));

    [HttpPost("{sessionStableId}/warehouse-put-away-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewWarehousePutAway(
        string sessionStableId,
        [FromBody] SimulationWarehousePutAwayPreviewRequest request)
        => Ok(service.PreviewWarehousePutAway(sessionStableId, request));

    [HttpPost("{sessionStableId}/warehouse-put-aways/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmWarehousePutAway(
        string sessionStableId,
        [FromBody] SimulationWarehousePutAwayConfirmRequest request)
        => Ok(service.ConfirmWarehousePutAway(sessionStableId, request));

    [HttpPost("{sessionStableId}/supply-chain-work-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewSupplyChainWork(
        string sessionStableId,
        [FromBody] SimulationSupplyChainWorkPreviewRequest request)
        => Ok(service.PreviewSupplyChainWork(sessionStableId, request));

    [HttpPost("{sessionStableId}/supply-chain-works/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmSupplyChainWork(
        string sessionStableId,
        [FromBody] SimulationSupplyChainWorkConfirmRequest request)
        => Ok(service.ConfirmSupplyChainWork(sessionStableId, request));
}
