using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions")]
public sealed class Simulation타로객체반응Controller(
    Simulation타로객체반응PreviewService service) : ControllerBase
{
    [HttpPost("{sessionStableId}/tarot-object-reaction-previews")]
    [ProducesResponseType(
        typeof(Simulation타로객체반응PreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<Simulation타로객체반응PreviewSnapshot> Preview(
        string sessionStableId,
        [FromBody] Simulation타로객체반응PreviewRequest request)
    {
        try
        {
            return Ok(service.Preview(sessionStableId, request));
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
