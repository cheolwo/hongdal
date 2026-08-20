using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/actual-e5")]
public sealed class SimulationActualE5SessionsController(
    SimulationActualE5SessionCreationService service,
    SimulationAreaSetImmersionService immersion) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SimulationActualE5SessionCreateResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<SimulationActualE5SessionCreateResponse>> Create(
        [FromBody] SimulationActualE5SessionCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Create), result);
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (InvalidOperationException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.Message });
        }
    }

    [HttpPost("e7")]
    [ProducesResponseType(typeof(SimulationE7LaunchResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<SimulationE7LaunchResponse>> CreateForE7Validation(
        [FromBody] SimulationActualE5SessionCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var readiness = await immersion.RequireE7GateAsync(
                request.AreaSetStableId, cancellationToken);
            var session = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(CreateForE7Validation), new SimulationE7LaunchResponse
            {
                RuntimeValidationCompleted = false,
                ImmersionReadiness = readiness,
                SessionCreation = session,
            });
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (InvalidOperationException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.Message });
        }
    }
}
