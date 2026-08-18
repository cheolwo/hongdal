using Microsoft.AspNetCore.Mvc;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/world-stream/lh")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.SimulationWorldStreaming,
    SsalddelCodeLayer.Api,
    "플레이어 L3 위치에 필요한 LH Cell 후보를 서버 Simulation 시각으로 Preview한다.",
    StepKey = "api.lh-world-preview",
    DependsOnStepKeys = new[] { "application.lh-world-preview" },
    ExecutionStage = SsalddelCodeExecutionStage.Preview,
    ReadsFrom = SsalddelCodeDataScope.SimulationState | SsalddelCodeDataScope.DerivedWorld,
    FlowOrder = 32,
    Boundary = "정확한 Transform을 받지 않고 양자화한 L3 Cell만 사용하며 Preview는 어떤 원장도 변경하지 않는다.")]
public sealed class SimulationLhWorldController(
    SimulationLhWorldService lhWorld,
    경영SimulationSessionService sessions) : ControllerBase
{
    [HttpPost("cells/preview")]
    [ProducesResponseType(typeof(SimulationLhCellPreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SimulationErrorResponse), StatusCodes.Status409Conflict)]
    public ActionResult<SimulationLhCellPreviewResponse> PreviewCells(
        [FromBody] SimulationLhCellPreviewRequest request)
    {
        var state = sessions.Get(request.SessionStableId);
        if (request.ExpectedWorldRevision != state.WorldContext.WorldRevision)
            return Conflict(new SimulationErrorResponse
            {
                ErrorCode = "SimulationExpectedRevisionMismatch",
            });
        try
        {
            return Ok(lhWorld.Preview(
                request,
                (state.WorldContext.GameDate.Date
                    - state.WorldContext.GameDateStartsOn.Date).Days + 1,
                state.WorldContext.WorldTick,
                state.WorldContext.WorldRevision));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new SimulationErrorResponse
            {
                ErrorCode = exception.Message.Split(':')[0],
            });
        }
    }
}
