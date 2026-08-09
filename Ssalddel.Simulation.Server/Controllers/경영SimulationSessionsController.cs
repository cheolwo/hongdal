using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions")]
public sealed class 경영SimulationSessionsController(
    경영SimulationSessionService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status201Created)]
    public ActionResult<경영SimulationSessionSnapshot> Create(
        [FromBody] 경영SimulationSession생성Request request)
        => Execute(() =>
        {
            var result = service.Create(request);
            return CreatedAtAction(nameof(Get), new { sessionStableId = result.SessionStableId }, result);
        });

    [HttpGet("{sessionStableId}")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> Get(string sessionStableId)
        => Execute(() => Ok(service.Get(sessionStableId)));

    [HttpPost("{sessionStableId}/ticks")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> Advance(
        string sessionStableId,
        [FromBody] 경영SimulationTick진행Request request)
        => Execute(() => Ok(service.Advance(sessionStableId, request)));

    private ActionResult<경영SimulationSessionSnapshot> Execute(
        Func<ActionResult<경영SimulationSessionSnapshot>> action)
    {
        try
        {
            return action();
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
