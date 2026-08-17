using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Server.Controllers;

[ApiController]
[Route("api/simulation/v1/sessions/{sessionStableId}/battles")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.SimulationParallelBattle,
    SsalddelCodeLayer.Api,
    "병렬 전투 조회·Preview·Confirm·진행 HTTP 경계를 제공한다.",
    StepKey = "api.battle",
    DependsOnStepKeys = new string[] { "contract.battle-preview" },
    ExecutionStage = SsalddelCodeExecutionStage.Confirm,
    Effects = SsalddelCodeEffect.StateMutation,
    ReadsFrom = SsalddelCodeDataScope.SimulationState,
    WritesTo = SsalddelCodeDataScope.SimulationState,
    FlowOrder = 20,
    Boundary = "클라이언트가 보낸 안정 ID와 예상 개정만 받아 서버 규칙으로 전투 상태를 확정한다.")]
public sealed class SimulationBattlesController(SimulationBattleInstanceService service)
    : ControllerBase
{
    [HttpGet]
    public ActionResult<SimulationBattleInstanceSnapshot[]> List(string sessionStableId,
        [FromQuery] string actorStableId)
        => Execute(() => service.List(sessionStableId, actorStableId));

    [HttpGet("{battleStableId}")]
    public ActionResult<SimulationBattleInstanceSnapshot> Get(string sessionStableId,
        string battleStableId, [FromQuery] string actorStableId)
        => Execute(() => service.Get(sessionStableId, battleStableId, actorStableId));

    [HttpPost("previews")]
    public ActionResult<SimulationBattleCreatePreviewSnapshot> PreviewCreate(
        string sessionStableId, [FromBody] SimulationBattleCreatePreviewRequest request)
        => Execute(() => service.PreviewCreate(sessionStableId, request));

    [HttpPost("confirm")]
    public ActionResult<SimulationBattleInstanceSnapshot> ConfirmCreate(
        string sessionStableId, [FromBody] SimulationBattleCreateConfirmRequest request)
        => Execute(() => service.ConfirmCreate(sessionStableId, request));

    [HttpPost("{battleStableId}/participants/confirm")]
    public ActionResult<SimulationBattleInstanceSnapshot> ConfirmParticipation(
        string sessionStableId, string battleStableId,
        [FromBody] SimulationBattleParticipationConfirmRequest request)
        => Execute(() => service.ConfirmParticipation(sessionStableId, battleStableId, request));

    [HttpPost("{battleStableId}/deployments/preview")]
    public ActionResult<SimulationBattleDeploymentPreviewSnapshot> PreviewDeployment(
        string sessionStableId, string battleStableId,
        [FromBody] SimulationBattleDeploymentPreviewRequest request)
        => Execute(() => service.PreviewDeployment(sessionStableId, battleStableId, request));

    [HttpPost("{battleStableId}/deployments/confirm")]
    public ActionResult<SimulationBattleInstanceSnapshot> ConfirmDeployment(
        string sessionStableId, string battleStableId,
        [FromBody] SimulationBattleDeploymentConfirmRequest request)
        => Execute(() => service.ConfirmDeployment(sessionStableId, battleStableId, request));

    [HttpPost("{battleStableId}/support-previews")]
    public ActionResult<SimulationBattleSupportPreviewSnapshot> PreviewSupport(
        string sessionStableId, string battleStableId,
        [FromBody] SimulationBattleSupportPreviewRequest request)
        => Execute(() => service.PreviewSupport(sessionStableId, battleStableId, request));

    [HttpPost("{battleStableId}/supports/confirm")]
    public ActionResult<SimulationBattleInstanceSnapshot> ConfirmSupport(
        string sessionStableId, string battleStableId,
        [FromBody] SimulationBattleSupportConfirmRequest request)
        => Execute(() => service.ConfirmSupport(sessionStableId, battleStableId, request));

    [HttpPost("{battleStableId}/ticks")]
    public ActionResult<SimulationBattleInstanceSnapshot> Advance(
        string sessionStableId, string battleStableId,
        [FromBody] SimulationBattleAdvanceRequest request)
        => Execute(() => service.Advance(sessionStableId, battleStableId, request));

    private ActionResult<T> Execute<T>(Func<T> action)
    {
        try { return Ok(action()); }
        catch (SimulationContractException error)
        { return BadRequest(new SimulationErrorResponse { ErrorCode = error.ErrorCode }); }
        catch (SimulationNotFoundException error)
        { return NotFound(new SimulationErrorResponse { ErrorCode = error.ErrorCode }); }
        catch (SimulationConflictException error)
        { return Conflict(new SimulationErrorResponse { ErrorCode = error.ErrorCode }); }
    }
}
