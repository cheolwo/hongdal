using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/world-events")]
public sealed class SimulationWorldEventsController(
    SimulationWorldEventProjectionService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SimulationWorldEventProjectionSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationWorldEventProjectionSnapshot> GetChanges(
        string sessionStableId,
        [FromQuery] long afterWorldRevision = -1)
    {
        try
        {
            return Ok(service.GetChanges(sessionStableId, afterWorldRevision));
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
