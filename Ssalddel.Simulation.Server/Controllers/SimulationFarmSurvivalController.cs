using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/farm-survival")]
public sealed class SimulationFarmSurvivalController(
    SimulationFarmSurvivalService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SimulationFarmSurvivalStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFarmSurvivalStateSnapshot> Get(
        string sessionStableId)
        => Execute(() => service.Get(sessionStableId));

    [HttpPost("work/preview")]
    [ProducesResponseType(typeof(SimulationFarmWorkPreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFarmWorkPreviewSnapshot> PreviewWork(
        string sessionStableId,
        [FromBody] SimulationFarmWorkPreviewRequest request)
        => Execute(() => service.PreviewWork(sessionStableId, request));

    [HttpPost("work/confirm")]
    [ProducesResponseType(typeof(SimulationFarmSurvivalStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFarmSurvivalStateSnapshot> ConfirmWork(
        string sessionStableId,
        [FromBody] SimulationFarmWorkConfirmRequest request)
        => Execute(() => service.ConfirmWork(sessionStableId, request));

    [HttpPost("threat-responses/confirm")]
    [ProducesResponseType(typeof(SimulationFarmSurvivalStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFarmSurvivalStateSnapshot> ConfirmThreatResponse(
        string sessionStableId,
        [FromBody] SimulationThreatResponseConfirmRequest request)
        => Execute(() => service.ConfirmThreatResponse(sessionStableId, request));

    [HttpPost("combat/perspective/confirm")]
    [ProducesResponseType(typeof(SimulationFarmSurvivalStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFarmSurvivalStateSnapshot> ConfirmCombatPerspective(
        string sessionStableId,
        [FromBody] SimulationCombatPerspectiveConfirmRequest request)
        => Execute(() => service.ConfirmCombatPerspective(sessionStableId, request));

    [HttpPost("combat/beats/start")]
    [ProducesResponseType(typeof(SimulationFarmSurvivalStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFarmSurvivalStateSnapshot> StartCombatBeat(
        string sessionStableId,
        [FromBody] SimulationCombatBeatStartRequest request)
        => Execute(() => service.StartCombatBeat(sessionStableId, request));

    [HttpPost("combat/beats/{beatStableId}/react")]
    [ProducesResponseType(typeof(SimulationFarmSurvivalStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFarmSurvivalStateSnapshot> ConfirmCombatReaction(
        string sessionStableId,
        string beatStableId,
        [FromBody] SimulationCombatReactionConfirmRequest request)
    {
        if (request == null || !string.Equals(beatStableId, request.BeatStableId,
            StringComparison.Ordinal))
            return BadRequest(new SimulationErrorResponse
            {
                ErrorCode = "SimulationCombatBeatRouteMismatch",
            });
        return Execute(() => service.ConfirmCombatReaction(sessionStableId, request));
    }

    [HttpPost("combat/tactical-orders/preview")]
    [ProducesResponseType(typeof(SimulationTacticalOrderPreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTacticalOrderPreviewSnapshot> PreviewTacticalOrder(
        string sessionStableId,
        [FromBody] SimulationTacticalOrderPreviewRequest request)
        => Execute(() => service.PreviewTacticalOrder(sessionStableId, request));

    [HttpPost("combat/tactical-orders/{orderWindowStableId}/confirm")]
    [ProducesResponseType(typeof(SimulationFarmSurvivalStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationFarmSurvivalStateSnapshot> ConfirmTacticalOrder(
        string sessionStableId,
        string orderWindowStableId,
        [FromBody] SimulationTacticalOrderConfirmRequest request)
    {
        if (request == null || !string.Equals(orderWindowStableId,
            request.OrderWindowStableId, StringComparison.Ordinal))
            return BadRequest(new SimulationErrorResponse
            {
                ErrorCode = "SimulationTacticalOrderWindowRouteMismatch",
            });
        return Execute(() => service.ConfirmTacticalOrder(sessionStableId, request));
    }

    private ActionResult<T> Execute<T>(Func<T> action)
    {
        try
        {
            return Ok(action());
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
