using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/world-stream/regions")]
public sealed class SimulationWorldRegionProjectionController(
    ISimulationWorld지역ProjectionReader reader) : ControllerBase
{
    [HttpGet("{regionStableId}")]
    [ProducesResponseType(typeof(SimulationWorldRegionProjectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<SimulationWorldRegionProjectionResponse>> Get(
        string regionStableId,
        CancellationToken cancellationToken)
    {
        var result = await reader.조회Async(regionStableId, cancellationToken);
        if (!result.파생Db사용가능)
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: SimulationWorldRegionProjectionCodes.DatabaseDisabled);
        return result.Projection is null
            ? NotFound()
            : Ok(result.Projection);
    }
}
