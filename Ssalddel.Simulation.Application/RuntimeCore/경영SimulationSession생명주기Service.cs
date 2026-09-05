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
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class 경영SimulationSession생명주기Service
    {
        private readonly 경영SimulationSessionAccessor sessions;
        private readonly ISimulationSessionSaveStore saveStore;
        private readonly ISimulationBattleWorldReconciler? battleReconciler;
        private readonly ISimulationHexagramCampaignAttemptStore? campaignAttempts;

        public 경영SimulationSession생명주기Service(
            경영SimulationSessionAccessor sessions,
            ISimulationSessionSaveStore saveStore,
            ISimulationBattleWorldReconciler? battleReconciler = null,
            ISimulationHexagramCampaignAttemptStore? campaignAttempts = null)
        {
            this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            this.saveStore = saveStore ?? throw new ArgumentNullException(nameof(saveStore));
            this.battleReconciler = battleReconciler;
            this.campaignAttempts = campaignAttempts;
        }

        public 경영SimulationSessionSnapshot Create(경영SimulationSession생성Request request,
            SimulationRealityContextSnapshot? frozenRealityContext = null)
            => sessions.CreateOrGet(request, frozenRealityContext).Snapshot();

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

            var saved = saveStore.SaveOrGet(package);
            campaignAttempts?.RegisterSave(saved.SaveStableId,
                saved.SessionStableId,
                saved.HexagramCampaign?.HexagramStableId ?? string.Empty,
                saved.HexagramCampaign?.AttemptOrdinal ?? 0);
            return saved;
        }

        public SimulationSessionRestoreResult Restore(SimulationSessionRestoreRequest request)
        {
            var (package, restored) = Replay(request);
            sessions.Restore(restored);
            battleReconciler?.Restore(package.SessionStableId, package.Battles);
            return Result(package, restored);
        }

        public SimulationSessionRestoreResult RestoreForCampaignRetry(
            SimulationSessionRestoreRequest request, long expectedCurrentRevision)
        {
            var (package, restored) = Replay(request, bypassAttemptGuard: true);
            sessions.ReplaceForCampaignRetry(restored, expectedCurrentRevision);
            battleReconciler?.Restore(package.SessionStableId, package.Battles);
            return Result(package, restored);
        }

        public SimulationSessionRestoreResult VerifyReplay(
            SimulationSessionRestoreRequest request)
        {
            var (package, restored) = Replay(request);
            return Result(package, restored);
        }

        private (SimulationSessionSavePackage Package,
            경영SimulationSessionAggregate Restored) Replay(
            SimulationSessionRestoreRequest request,
            bool bypassAttemptGuard = false)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.SaveStableId))
                throw new SimulationContractException("SimulationSaveStableIdInvalid");
            var package = saveStore.Find(request.SaveStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSaveNotFound");
            if (!bypassAttemptGuard)
                campaignAttempts?.EnsureRestoreAllowed(package.SaveStableId,
                    package.HexagramCampaign);
            return (package, SimulationSessionReplay.Restore(package));
        }

        private static SimulationSessionRestoreResult Result(
            SimulationSessionSavePackage package,
            경영SimulationSessionAggregate restored)
        {
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
