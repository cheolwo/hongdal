using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Application,
        "세션 생성·조회·Tick·저장·복원을 조율한다.",
        StepKey = "application.session-lifecycle",
        DependsOnStepKeys = new string[] { "api.session-lifecycle" },
        ExecutionStage = SsalddelCodeExecutionStage.Confirm,
        Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "실제 업무 상태를 만들지 않으며 기대 개정과 저장 자료 무결성을 통과한 Simulation 상태만 변경한다.")]
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSaveReplay,
        SsalddelCodeLayer.Application,
        "세션 저장·복원과 전투 저장 자료 결합을 조율한다.",
        StepKey = "application.save-replay",
        DependsOnStepKeys = new string[] { "api.save-replay" },
        ExecutionStage = SsalddelCodeExecutionStage.Persistence,
        Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "검증된 simulation-save.v1·v2 자료만 저장·복원하며 활성 세션을 임의로 덮어쓰지 않는다.")]
    public sealed class 경영SimulationSession생명주기Service
    {
        private readonly 경영SimulationSessionAccessor sessions;
        private readonly ISimulationSessionSaveStore saveStore;
        private readonly ISimulationBattleWorldReconciler? battleReconciler;

        public 경영SimulationSession생명주기Service(
            경영SimulationSessionAccessor sessions,
            ISimulationSessionSaveStore saveStore,
            ISimulationBattleWorldReconciler? battleReconciler = null)
        {
            this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            this.saveStore = saveStore ?? throw new ArgumentNullException(nameof(saveStore));
            this.battleReconciler = battleReconciler;
        }

        public 경영SimulationSessionSnapshot Create(경영SimulationSession생성Request request)
            => sessions.CreateOrGet(request).Snapshot();

        public 경영SimulationSessionSnapshot Get(string sessionStableId)
            => sessions.Require(sessionStableId).Snapshot();

        public 경영SimulationSessionSnapshot Advance(
            string sessionStableId,
            경영SimulationTick진행Request request)
        {
            battleReconciler?.EnsureWorldTickCanAdvance(sessionStableId);
            var snapshot = sessions.Require(sessionStableId).Advance(request);
            battleReconciler?.Reconcile(sessionStableId, snapshot);
            return snapshot;
        }

        public SimulationSessionSavePackage Save(
            string sessionStableId,
            SimulationSessionSaveRequest request)
        {
            var package = sessions.Require(sessionStableId).CreateSavePackage(request);
            if (battleReconciler != null)
            {
                package = SimulationSaveReplayCloner.WithBattles(
                    package,
                    battleReconciler.Capture(sessionStableId));
            }

            return saveStore.SaveOrGet(package);
        }

        public SimulationSessionRestoreResult Restore(SimulationSessionRestoreRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.SaveStableId))
                throw new SimulationContractException("SimulationSaveStableIdInvalid");
            var package = saveStore.Find(request.SaveStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSaveNotFound");
            var restored = SimulationSessionReplay.Restore(package);
            sessions.Restore(restored);
            battleReconciler?.Restore(package.SessionStableId, package.Battles);
            return new SimulationSessionRestoreResult
            {
                SaveStableId = package.SaveStableId,
                SchemaVersion = package.SchemaVersion,
                ReplayHash = package.ReplayHash,
                ReplayedCommandCount = package.CommandLog.Length,
                RestoredBattleCount = package.Battles.Length,
                Session = restored.Snapshot(),
            };
        }
    }
}
