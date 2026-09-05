using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/hexagram-campaign")]
public sealed class SimulationHexagramCampaignController(
    SimulationHexagramCampaignService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SimulationHexagramCampaignStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationHexagramCampaignStateSnapshot> Get(
        string sessionStableId)
        => Execute(() => service.Get(sessionStableId));

    [HttpPost("enter")]
    public ActionResult<SimulationHexagramCampaignStateSnapshot> Enter(
        string sessionStableId,
        [FromBody] SimulationHexagramCampaignEnterRequest request)
        => Execute(() => service.Enter(sessionStableId, request));

    [HttpPost("line/complete")]
    public ActionResult<SimulationHexagramCampaignStateSnapshot> CompleteLine(
        string sessionStableId,
        [FromBody] SimulationHexagramCampaignLineCompleteRequest request)
        => Execute(() => service.CompleteLine(sessionStableId, request));

    [HttpPost("setback")]
    public ActionResult<SimulationHexagramCampaignStateSnapshot> Setback(
        string sessionStableId,
        [FromBody] SimulationHexagramCampaignSetbackRequest request)
        => Execute(() => service.RecordSetback(sessionStableId, request));

    [HttpPost("fail")]
    public ActionResult<SimulationHexagramCampaignStateSnapshot> Fail(
        string sessionStableId,
        [FromBody] SimulationHexagramCampaignFailureRequest request)
        => Execute(() => service.Fail(sessionStableId, request));

    [HttpPost("complete")]
    public ActionResult<SimulationHexagramCampaignStateSnapshot> Complete(
        string sessionStableId,
        [FromBody] SimulationHexagramCampaignCompleteRequest request)
        => Execute(() => service.Complete(sessionStableId, request));

    private ActionResult<T> Execute<T>(Func<T> action)
    {
        try { return Ok(action()); }
        catch (SimulationContractException error)
        {
            return BadRequest(new SimulationErrorResponse
                { ErrorCode = error.ErrorCode });
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse
                { ErrorCode = error.ErrorCode });
        }
        catch (SimulationConflictException error)
        {
            return Conflict(new SimulationErrorResponse
                { ErrorCode = error.ErrorCode });
        }
    }
}
