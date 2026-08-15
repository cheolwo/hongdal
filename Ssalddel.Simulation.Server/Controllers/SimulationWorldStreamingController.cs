using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/world-stream")]
public sealed class SimulationWorldStreamingController(
    SimulationWorldStreamingService service,
    SimulationWorldExplorationService exploration) : ControllerBase
{
    [HttpGet("recipes/{recipeId}")]
    [ProducesResponseType(typeof(SimulationWorldStreamRecipeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<SimulationWorldStreamRecipeResponse> Recipe(string recipeId)
        => service.TryGetRecipe(recipeId, out var value)
            ? Ok(value)
            : NotFound(new SimulationErrorResponse { ErrorCode = "SimulationWorldStreamRecipeNotFound" });

    [HttpGet("tiles/{tileKey}/manifest")]
    [ProducesResponseType(typeof(SimulationWorldTileStreamManifestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<SimulationWorldTileStreamManifestResponse> Manifest(string tileKey)
        => service.TryGetManifest(tileKey, out var value)
            ? Ok(value)
            : NotFound(new SimulationErrorResponse { ErrorCode = "SimulationWorldStreamTileNotFound" });

    [HttpGet("tiles/{tileKey}/artifacts/{layerCode}")]
    [ProducesResponseType(typeof(SimulationWorldTileArtifactDescriptorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<SimulationWorldTileArtifactDescriptorResponse> Artifact(
        string tileKey, string layerCode)
        => service.TryGetArtifact(tileKey, layerCode, out var value)
            ? Ok(value)
            : NotFound(new SimulationErrorResponse { ErrorCode = "SimulationWorldStreamArtifactNotFound" });

    [HttpGet("tiles/{tileKey}/activities")]
    [ProducesResponseType(typeof(SimulationWorldTileActivityProjectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<SimulationWorldTileActivityProjectionResponse> Activities(string tileKey)
        => service.TryGetActivities(tileKey, out var value)
            ? Ok(value)
            : NotFound(new SimulationErrorResponse { ErrorCode = "SimulationWorldStreamTileNotFound" });

    [HttpGet("tiles/{tileKey}/objects")]
    [ProducesResponseType(typeof(SimulationWorldTileObjectProjectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<SimulationWorldTileObjectProjectionResponse> Objects(string tileKey)
        => service.TryGetObjects(tileKey, out var value)
            ? Ok(value)
            : NotFound(new SimulationErrorResponse { ErrorCode = "SimulationWorldStreamTileNotFound" });

    [HttpGet("tiles/{tileKey}/building-item-rules")]
    [ProducesResponseType(typeof(SimulationWorldBuildingItemRulePackageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<SimulationWorldBuildingItemRulePackageResponse> BuildingItemRules(
        string tileKey)
    {
        try
        {
            return Ok(exploration.GetBuildingItemRules(tileKey));
        }
        catch (SimulationNotFoundException error)
        {
            return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode });
        }
    }

    [HttpPost("sessions/{sessionStableId}/tiles/{tileKey}/building-item-eligibility-preview")]
    [ProducesResponseType(typeof(SimulationWorldBuildingItemEligibilityPreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<SimulationWorldBuildingItemEligibilityPreviewResponse> PreviewEligibility(
        string sessionStableId,
        string tileKey,
        [FromBody] SimulationWorldBuildingItemEligibilityPreviewRequest request)
    {
        try
        {
            return Ok(exploration.PreviewEligibility(sessionStableId, tileKey, request));
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
}
