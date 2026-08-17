using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions")]
public sealed class 경영SimulationWorldUiController(
    SimulationWorldUIProjectionService projectionService) : ControllerBase
{
    [HttpGet("{sessionStableId}/world-ui/surfaces/{surfaceStableId}")]
    [ProducesResponseType(typeof(SimulationWorldUIProjection), StatusCodes.Status200OK)]
    public ActionResult<SimulationWorldUIProjection> GetWorldUiSurface(
        string sessionStableId,
        string surfaceStableId)
        => Ok(projectionService.Get(sessionStableId, surfaceStableId));
}
