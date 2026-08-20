using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/world-stream/area-sets")]
public sealed class SimulationAreaSetImmersionController(
    SimulationAreaSetImmersionService service) : ControllerBase
{
    [HttpGet("{areaSetStableId}/immersion-readiness")]
    [ProducesResponseType(typeof(SimulationAreaSetImmersionReadinessResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<SimulationAreaSetImmersionReadinessResponse>> Get(
        string areaSetStableId, CancellationToken cancellationToken)
    {
        var result = await service.ReadAsync(areaSetStableId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}
