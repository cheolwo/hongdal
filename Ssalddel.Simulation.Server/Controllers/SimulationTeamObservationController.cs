using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/team-observation")]
public sealed class SimulationTeamObservationController(
    SimulationTeamObservationService service) : ControllerBase
{
    [HttpPost("access/preview")]
    [ProducesResponseType(typeof(SimulationTeamObservationAccessResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SimulationErrorResponse),
        StatusCodes.Status404NotFound)]
    public ActionResult<SimulationTeamObservationAccessResponse> PreviewAccess(
        string sessionStableId,
        [FromBody] SimulationTeamObservationAccessRequest request)
    {
        try
        {
            return Ok(service.Evaluate(sessionStableId, request));
        }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    [HttpPost("sessions/start")]
    [ProducesResponseType(typeof(SimulationTeamObservationSessionResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTeamObservationSessionResponse> Start(
        string sessionStableId,
        [FromBody] SimulationTeamObservationSessionStartRequest request)
        => Execute(() => service.Start(sessionStableId, request));

    [HttpGet("sessions/{observationSessionStableId}/frame")]
    [ProducesResponseType(typeof(SimulationTeamObservationFrameResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTeamObservationFrameResponse> Frame(
        string sessionStableId,
        string observationSessionStableId)
        => Execute(() => service.GetFrame(sessionStableId,
            observationSessionStableId));

    [HttpPost("sessions/{observationSessionStableId}/end")]
    [ProducesResponseType(typeof(SimulationTeamObservationSessionResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTeamObservationSessionResponse> End(
        string sessionStableId,
        string observationSessionStableId,
        [FromBody] SimulationTeamObservationSessionEndRequest request)
        => Execute(() => service.End(sessionStableId,
            observationSessionStableId, request));

    [HttpGet("targets/{targetActorStableId}/observers")]
    [ProducesResponseType(typeof(SimulationTeamObserverIndicatorResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTeamObserverIndicatorResponse> Observers(
        string sessionStableId,
        string targetActorStableId)
        => Execute(() => service.GetObservers(sessionStableId,
            targetActorStableId));

    private ActionResult<T> Execute<T>(Func<T> action)
    {
        try
        {
            return Ok(action());
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
