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
