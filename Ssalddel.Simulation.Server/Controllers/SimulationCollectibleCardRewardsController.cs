using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}")]
public sealed class SimulationCollectibleCardRewardsController(
    SimulationCollectibleCardRewardService service) : ControllerBase
{
    [HttpGet("exploration-state")]
    [ProducesResponseType(typeof(SimulationWorldExplorationStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationWorldExplorationStateSnapshot> GetExploration(
        string sessionStableId, [FromQuery] string actorStableId)
        => Execute(() => service.GetExploration(sessionStableId, actorStableId));

    [HttpPost("tile-traversals/confirm")]
    [ProducesResponseType(typeof(SimulationTileTraversalConfirmResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationTileTraversalConfirmResponse> ConfirmTraversal(
        string sessionStableId,
        [FromBody] SimulationTileTraversalConfirmRequest request)
        => Execute(() => service.ConfirmTraversal(sessionStableId, request));

    [HttpGet("collectible-card-rewards")]
    [ProducesResponseType(typeof(SimulationCollectibleCardRewardStateSnapshot),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationCollectibleCardRewardStateSnapshot> GetRewards(
        string sessionStableId, [FromQuery] string actorStableId)
        => Execute(() => service.GetRewards(sessionStableId, actorStableId));

    [HttpPost("card-draw-opportunities/{opportunityStableId}/draw")]
    [ProducesResponseType(typeof(SimulationCollectibleCardDrawResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationCollectibleCardDrawResponse> Draw(
        string sessionStableId, string opportunityStableId,
        [FromBody] SimulationCollectibleCardDrawRequest request)
        => Execute(() => service.Draw(sessionStableId, opportunityStableId, request));

    [HttpPost("collectible-cards/{cardCopyStableId}/transfer")]
    [ProducesResponseType(typeof(SimulationCollectibleCardTransferResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SimulationCollectibleCardTransferResponse> Transfer(
        string sessionStableId, string cardCopyStableId,
        [FromBody] SimulationCollectibleCardTransferRequest request)
        => Execute(() => service.Transfer(sessionStableId, cardCopyStableId, request));

    private ActionResult<T> Execute<T>(Func<T> action)
    {
        try { return Ok(action()); }
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
