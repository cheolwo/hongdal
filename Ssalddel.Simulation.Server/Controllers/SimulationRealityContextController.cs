using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/reality-context")]
public sealed class SimulationRealityContextController(
    SimulationRealityContextService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SimulationRealityContextPlayerProjectionResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationRealityContextPlayerProjectionResponse> Get(
        string sessionStableId,
        [FromQuery] bool includeSourceDetails = false)
    {
        try
        {
            return Ok(service.ReadPlayerProjection(sessionStableId,
                includeSourceDetails));
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }
}
