using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 플레이어 직접 노동과 NPC 위임, 농장 방어, 위협 대응을 Simulation 상태로만 확정한다.
    /// 실제 사람·사업체·건물에 감염, 약탈, 전투 의미를 부여하지 않는다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationFarmCombatInput,
        SsalddelCodeLayer.Application,
        "전투 입력을 현재 Simulation Session aggregate에 전달한다.",
        StepKey = "application.farm-combat",
        DependsOnStepKeys = new[] { "api.farm-combat" },
        FlowOrder = 30,
        ExecutionStage = SsalddelCodeExecutionStage.Confirm,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        Effects = SsalddelCodeEffect.StateMutation,
        Boundary = "운영 업무 상태가 아니라 Simulation Session 상태만 변경한다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행)]
    public sealed class SimulationFarmSurvivalService
    {
        private readonly I경영SimulationSessionStore store;
        private readonly ISimulationBattleResourceLockReader? battleLocks;
        private readonly I세계상호작용실행Pipeline worldInteractions;

        public SimulationFarmSurvivalService(I경영SimulationSessionStore store,
            ISimulationBattleResourceLockReader? battleResourceLocks = null,
            I세계상호작용실행Pipeline? worldInteractionPipeline = null)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            battleLocks = battleResourceLocks;
            worldInteractions = worldInteractionPipeline
                ?? new 세계상호작용실행Pipeline();
        }

        public SimulationFarmSurvivalStateSnapshot Get(string sessionStableId)
            => Find(sessionStableId).GetFarmSurvivalState();

        public SimulationFarmWorkPreviewSnapshot PreviewWork(
            string sessionStableId,
            SimulationFarmWorkPreviewRequest request)
        {
            var preview = Find(sessionStableId).PreviewFarmWork(request);
            if (IsLocked(sessionStableId, request.ActorStableId)
                || IsLocked(sessionStableId, request.TargetStableId))
            {
                preview.CanConfirm = false;
                preview.BlockingReasonCodes = preview.BlockingReasonCodes
                    .Concat(new[] { "BattleResourceLocked" }).Distinct().ToArray();
            }
            return preview;
        }

        public SimulationFarmSurvivalStateSnapshot ConfirmWork(
            string sessionStableId,
            SimulationFarmWorkConfirmRequest request)
        {
            if (IsLocked(sessionStableId, request.ActorStableId)
                || IsLocked(sessionStableId, request.TargetStableId))
                throw new SimulationConflictException("BattleResourceLocked");
            var worldInteractionId = request.ActionCode switch
            {
                SimulationFarmSurvivalCodes.Harvesting => "WI-FARM-04",
                SimulationFarmSurvivalCodes.HarvestCollection => "WI-FARM-05",
                SimulationFarmSurvivalCodes.OutboundPacking => "WI-FARM-06",
                _ => string.Empty,
            };
            var aggregate = Find(sessionStableId);
            if (worldInteractionId.Length == 0)
                return aggregate.ConfirmFarmWork(request);
            var preview = aggregate.PreviewFarmWork(
                new SimulationFarmWorkPreviewRequest
                {
                    ExpectedRevision = request.ExpectedRevision,
                    ActorStableId = request.ActorStableId,
                    TargetStableId = request.TargetStableId,
                    ActionCode = request.ActionCode,
                    AssignmentKindCode = request.AssignmentKindCode,
                    PreferredSpatialStableId = request.PreferredSpatialStableId,
                });
            var successor = worldInteractionId switch
            {
                "WI-FARM-04" => "WI-FARM-05",
                "WI-FARM-05" => "WI-FARM-06",
                _ => "FarmInternalStorageOrNextProductionCycle",
            };
            var context = new 세계상호작용실행Context
                {
                    WorldInteractionId = worldInteractionId,
                    CommandId = request.CommandId,
                    InitiatorStableId = request.ActorStableId,
                    ActorStableId = request.ActorStableId,
                    TargetStableId = request.TargetStableId,
                    SourceReferenceIds = new[]
                    {
                        request.TargetStableId, request.ActionCode,
                    },
                    TimeReferenceId = "simulation-time:world-tick",
                    PlayableLoopStableId = worldInteractionId == "WI-FARM-04"
                        ? "playable-loop:farm-crop-cycle.v1"
                        : "playable-loop:farm-pack-store-return.v1",
                    AuthorityLocationCode = "RemoteHost",
                    SpatialEvidenceStateCode =
                        SimulationWorldInteractionSpatialEvidenceCodes.Bound,
                    SpatialEvidenceReferenceIds = new[]
                    {
                        "e9-wi-h:" + worldInteractionId.ToLowerInvariant(),
                    },
                    TaskOrEffectReferenceIds = new[]
                    {
                        "task:farm-supply:" + request.CommandId,
                    },
                    // 작업 시작은 아직 세계 결과가 아니다. 실제 결과 상태는
                    // WorldTick에서 효과가 적용될 때 기존 E5 기록에 결속한다.
                    ResultStateCodes = Array.Empty<string>(),
                    SuccessorOrReturnCodes = new[] { successor },
                    PrimaryOutcomeCode = request.ActionCode + ":TaskStarted",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = new[]
                    {
                        Simulation행위변화의미Codes.재고변경,
                        Simulation행위변화의미Codes.실외배치변경,
                    },
                    SpatialRevision = aggregate.SpatialCompositionRuleRevision,
                };
            worldInteractions.RecordPreview(context, aggregate.Revision,
                preview.CanConfirm, preview.BlockingReasonCodes);
            return worldInteractions.ExecutePlayerDriven(aggregate, context,
                () => aggregate.ConfirmFarmWork(request));
        }

        public SimulationFarmWorkPlanPreviewSnapshot PreviewWorkPlan(
            string sessionStableId,
            SimulationFarmWorkPlanPreviewRequest request)
        {
            var preview = Find(sessionStableId).PreviewFarmWorkPlan(request);
            if (request.Items.Any(value => IsLocked(sessionStableId,
                    value.ActorStableId) || IsLocked(sessionStableId,
                    value.TargetStableId)))
            {
                preview.CanConfirm = false;
                preview.BlockingReasonCodes = preview.BlockingReasonCodes
                    .Concat(new[] { "BattleResourceLocked" }).Distinct().ToArray();
            }
            return preview;
        }

        public SimulationFarmSurvivalStateSnapshot ConfirmWorkPlan(
            string sessionStableId,
            SimulationFarmWorkPlanConfirmRequest request)
        {
            if (request.Items.Any(value => IsLocked(sessionStableId,
                    value.ActorStableId) || IsLocked(sessionStableId,
                    value.TargetStableId)))
                throw new SimulationConflictException("BattleResourceLocked");
            return Find(sessionStableId).ConfirmFarmWorkPlan(request);
        }

        public SimulationFarmSurvivalStateSnapshot ConfirmThreatResponse(
            string sessionStableId,
            SimulationThreatResponseConfirmRequest request)
            => Find(sessionStableId).ConfirmThreatResponse(request);

        public SimulationFarmSurvivalStateSnapshot ConfirmCombatPerspective(
            string sessionStableId,
            SimulationCombatPerspectiveConfirmRequest request)
            => Find(sessionStableId).ConfirmCombatPerspective(request);

        public SimulationFarmSurvivalStateSnapshot StartCombatBeat(
            string sessionStableId,
            SimulationCombatBeatStartRequest request)
            => Find(sessionStableId).StartCombatBeat(request);

        public SimulationFarmSurvivalStateSnapshot ConfirmCombatReaction(
            string sessionStableId,
            SimulationCombatReactionConfirmRequest request)
            => Find(sessionStableId).ConfirmCombatReaction(request);

        public SimulationTacticalOrderPreviewSnapshot PreviewTacticalOrder(
            string sessionStableId,
            SimulationTacticalOrderPreviewRequest request)
            => Find(sessionStableId).PreviewTacticalOrder(request);

        public SimulationFarmSurvivalStateSnapshot ConfirmTacticalOrder(
            string sessionStableId,
            SimulationTacticalOrderConfirmRequest request)
            => Find(sessionStableId).ConfirmTacticalOrder(request);

        private 경영SimulationSessionAggregate Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException(
                    "SimulationSessionStableIdInvalid");
            return store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
        }

        private bool IsLocked(string sessionStableId, string resourceStableId)
            => battleLocks?.IsLocked(sessionStableId, resourceStableId) == true;
    }
}
