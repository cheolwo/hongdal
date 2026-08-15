using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/world-inventory")]
public sealed class SimulationWorldSurvivalInventoryController(
    SimulationWorldSurvivalInventoryService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SimulationWorldInventorySnapshot), StatusCodes.Status200OK)]
    public ActionResult<SimulationWorldInventorySnapshot> Get(string sessionStableId)
        => Execute<SimulationWorldInventorySnapshot>(() => service.Get(sessionStableId));

    [HttpPost("item-acquisition-previews")]
    [ProducesResponseType(typeof(SimulationWorldItemAcquisitionPreviewSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationWorldItemAcquisitionPreviewSnapshot> PreviewAcquisition(
        string sessionStableId,
        [FromBody] SimulationWorldItemAcquisitionPreviewRequest request)
        => Execute<SimulationWorldItemAcquisitionPreviewSnapshot>(
            () => service.PreviewAcquisition(sessionStableId, request));

    [HttpPost("item-acquisitions/confirm")]
    [ProducesResponseType(typeof(SimulationWorldItemAcquisitionResultSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationWorldItemAcquisitionResultSnapshot> ConfirmAcquisition(
        string sessionStableId,
        [FromBody] SimulationWorldItemAcquisitionConfirmRequest request)
        => Execute<SimulationWorldItemAcquisitionResultSnapshot>(
            () => service.ConfirmAcquisition(sessionStableId, request));

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
