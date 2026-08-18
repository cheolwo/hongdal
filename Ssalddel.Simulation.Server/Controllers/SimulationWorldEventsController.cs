using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/world-events")]
public sealed class SimulationWorldEventsController(
    SimulationWorldEventProjectionService service,
    SimulationRegionalIncidentService incidentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SimulationWorldEventProjectionSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationWorldEventProjectionSnapshot> GetChanges(
        string sessionStableId,
        [FromQuery] long afterWorldRevision = -1)
    {
        try
        {
            return Ok(service.GetChanges(sessionStableId, afterWorldRevision));
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

    [HttpPost("{eventStableId}/response-previews")]
    [ProducesResponseType(typeof(SimulationRegionalIncidentResponsePreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationRegionalIncidentResponsePreviewSnapshot> PreviewResponse(
        string sessionStableId,
        string eventStableId,
        [FromBody] SimulationRegionalIncidentResponsePreviewRequest request)
    {
        try
        {
            return Ok(incidentService.Preview(sessionStableId, eventStableId, request));
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

    [HttpPost("{eventStableId}/responses/confirm")]
    [ProducesResponseType(typeof(경영SimulationSessionSnapshot), StatusCodes.Status200OK)]
    public ActionResult<경영SimulationSessionSnapshot> ConfirmResponse(
        string sessionStableId,
        string eventStableId,
        [FromBody] SimulationRegionalIncidentResponseConfirmRequest request)
    {
        try
        {
            return Ok(incidentService.Confirm(sessionStableId, eventStableId, request));
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
