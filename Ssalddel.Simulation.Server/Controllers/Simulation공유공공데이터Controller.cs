using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/public-data")]
public sealed class Simulation공유공공데이터Controller(
    ISimulation공유공공데이터조회Port reader) : ControllerBase
{
    [HttpGet("kamis-price-observations")]
    [ProducesResponseType(typeof(Simulation공유공공데이터조회결과), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<Simulation공유공공데이터조회결과>> Kamis가격관측조회Async(
        [FromQuery] string? itemName,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await reader.Kamis가격관측조회Async(
                itemName,
                limit,
                cancellationToken));
        }
        catch (InvalidOperationException error)
            when (error.Message == DisabledSimulation공유공공데이터Reader.ErrorCode)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new SimulationErrorResponse { ErrorCode = error.Message });
        }
    }
}
