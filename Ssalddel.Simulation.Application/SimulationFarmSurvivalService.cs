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
    public sealed class SimulationFarmSurvivalService
    {
        private readonly I경영SimulationSessionStore store;
        private readonly ISimulationBattleResourceLockReader? battleLocks;

        public SimulationFarmSurvivalService(I경영SimulationSessionStore store,
            ISimulationBattleResourceLockReader? battleResourceLocks = null)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            battleLocks = battleResourceLocks;
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
            return Find(sessionStableId).ConfirmFarmWork(request);
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
