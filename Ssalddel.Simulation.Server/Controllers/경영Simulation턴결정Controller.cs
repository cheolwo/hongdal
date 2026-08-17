using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions")]
public sealed class 경영Simulation턴결정Controller(
    경영Simulation턴결정Service service) : ControllerBase
{
    [HttpGet("{sessionStableId}/turn-closing-context")]
    [ProducesResponseType(typeof(SimulationTurnClosingContextSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationTurnClosingContextSnapshot> GetTurnClosingContext(
        string sessionStableId)
        => Ok(service.GetTurnClosingContext(sessionStableId));

    [HttpPost("{sessionStableId}/turn-closing-previews")]
    [ProducesResponseType(typeof(SimulationTurnClosingPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationTurnClosingPreviewSnapshot> PreviewTurnClosing(
        string sessionStableId,
        [FromBody] SimulationTurnClosingPreviewRequest request)
        => Ok(service.PreviewTurnClosing(sessionStableId, request));

    [HttpPost("{sessionStableId}/turn-closings/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmTurnClosing(
        string sessionStableId,
        [FromBody] SimulationTurnClosingConfirmRequest request)
        => Ok(service.ConfirmTurnClosing(sessionStableId, request));

    [HttpPost("{sessionStableId}/decision-previews")]
    [ProducesResponseType(typeof(SimulationDecisionPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationDecisionPreviewSnapshot> PreviewDecision(
        string sessionStableId,
        [FromBody] SimulationDecisionPreviewRequest request)
        => Ok(service.PreviewDecision(sessionStableId, request));

    [HttpPost("{sessionStableId}/decisions/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmDecision(
        string sessionStableId,
        [FromBody] SimulationDecisionConfirmRequest request)
        => Ok(service.ConfirmDecision(sessionStableId, request));

    [HttpPost("{sessionStableId}/tasks/{taskStableId}/cancel")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> CancelTask(
        string sessionStableId,
        string taskStableId,
        [FromBody] SimulationTaskCancelRequest request)
        => Ok(service.CancelTask(sessionStableId, taskStableId, request));

    [HttpPost("{sessionStableId}/npc-policies")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> UpdateNpcPolicy(
        string sessionStableId,
        [FromBody] SimulationNpcPolicyChangeRequest request)
        => Ok(service.UpdateNpcPolicy(sessionStableId, request));
}
