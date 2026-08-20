using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/integrated-world")]
public sealed class SimulationIntegratedWorldController(
    경영Simulation통합생활세계Service service) : ControllerBase
{
    [HttpPost("previews")]
    [ProducesResponseType(typeof(SimulationIntegratedWorldPreviewSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationIntegratedWorldPreviewSnapshot> Preview(
        string sessionStableId,
        [FromBody] SimulationIntegratedWorldCommandRequest request)
        => Execute<SimulationIntegratedWorldPreviewSnapshot>(
            () => Ok(service.Preview(sessionStableId, request)));

    [HttpPost("commands")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> Confirm(
        string sessionStableId,
        [FromBody] SimulationIntegratedWorldCommandRequest request)
        => Execute<경영SimulationSessionSnapshot>(
            () => Ok(service.Confirm(sessionStableId, request)));

    private ActionResult<T> Execute<T>(Func<ActionResult<T>> action)
    {
        try { return action(); }
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
