using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/team-role-cards")]
public sealed class SimulationTeamRoleCardsController(
    SimulationTeamRoleCardService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SimulationTeamRoleCardStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTeamRoleCardStateSnapshot> Get(
        string sessionStableId,
        [FromQuery] string actorStableId)
        => Execute(() => service.Get(sessionStableId, actorStableId));

    [HttpPost("equip")]
    [ProducesResponseType(typeof(SimulationTeamRoleCardStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTeamRoleCardStateSnapshot> Equip(
        string sessionStableId,
        [FromBody] SimulationTeamRoleCardEquipRequest request)
        => Execute(() => service.Equip(sessionStableId, request));

    [HttpPost("activities/start")]
    [ProducesResponseType(typeof(SimulationTeamRoleCardStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTeamRoleCardStateSnapshot> StartActivity(
        string sessionStableId,
        [FromBody] SimulationTeamActivityStartRequest request)
        => Execute(() => service.StartActivity(sessionStableId, request));

    [HttpPost("activities/end")]
    [ProducesResponseType(typeof(SimulationTeamRoleCardStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTeamRoleCardStateSnapshot> EndActivity(
        string sessionStableId,
        [FromBody] SimulationTeamActivityEndRequest request)
        => Execute(() => service.EndActivity(sessionStableId, request));

    private ActionResult<SimulationTeamRoleCardStateSnapshot> Execute(
        Func<SimulationTeamRoleCardStateSnapshot> action)
    {
        try
        {
            return Ok(action());
        }
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
