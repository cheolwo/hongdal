using System;
using System.Collections.Concurrent;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Infrastructure
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Infrastructure,
        "활성 Simulation 세션을 프로세스 수명 동안 보관한다.",
        StepKey = "infrastructure.session-store",
        DependsOnStepKeys = new string[] { "domain.session-aggregate" },
        ExecutionStage = SsalddelCodeExecutionStage.Persistence,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 50,
        Boundary = "프로세스 내부 저장소이며 durable 저장이나 다중 인스턴스 동기화를 보장하지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Server 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "운영 상태와 Simulation 상태의 권위 경계를 유지한다.")]
    public sealed class InMemory경영SimulationSessionStore : I경영SimulationSessionStore
    {
        private readonly ConcurrentDictionary<string, 경영SimulationSessionAggregate> sessions =
            new ConcurrentDictionary<string, 경영SimulationSessionAggregate>(StringComparer.Ordinal);

        public 경영SimulationSessionAggregate CreateOrGet(
            경영SimulationSession생성Request request,
            SimulationRealityContextSnapshot? frozenRealityContext = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var candidate = new 경영SimulationSessionAggregate(request, frozenRealityContext);
            var session = sessions.GetOrAdd(candidate.SessionStableId, candidate);
            session.EnsureSameCreationRequest(request, frozenRealityContext);
            return session;
        }

        public 경영SimulationSessionAggregate? Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId)) return null;
            return sessions.TryGetValue(sessionStableId, out var session) ? session : null;
        }

        public 경영SimulationSessionAggregate Restore(경영SimulationSessionAggregate session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (!sessions.TryAdd(session.SessionStableId, session))
                throw new SimulationConflictException("SimulationSessionAlreadyActive");
            return session;
        }

        public 경영SimulationSessionAggregate ReplaceForCampaignRetry(
            경영SimulationSessionAggregate session, long expectedCurrentRevision)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            while (true)
            {
                if (!sessions.TryGetValue(session.SessionStableId, out var current))
                    throw new SimulationNotFoundException("SimulationSessionNotFound");
                if (current.Revision != expectedCurrentRevision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");
                if (sessions.TryUpdate(session.SessionStableId, session, current))
                    return session;
            }
        }
    }


    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
        "결정성·Save/Replay 또는 회귀 검증 책임을 제공한다.",
        Boundary = "저장 구현 존재만으로 상위 E 증거를 승격하지 않는다.")]
    public sealed class InMemorySimulationSessionSaveStore : ISimulationSessionSaveStore
    {
        private readonly ConcurrentDictionary<string, SimulationSessionSavePackage> saves =
            new ConcurrentDictionary<string, SimulationSessionSavePackage>(StringComparer.Ordinal);

        public SimulationSessionSavePackage SaveOrGet(SimulationSessionSavePackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            var candidate = SimulationSaveReplayCloner.ClonePackage(package);
            var saved = saves.GetOrAdd(candidate.SaveStableId, candidate);
            if (!string.Equals(saved.SessionStableId, candidate.SessionStableId, StringComparison.Ordinal)
                || !string.Equals(saved.ReplayHash, candidate.ReplayHash, StringComparison.Ordinal))
            {
                throw new SimulationConflictException("SimulationSaveStableIdConflict");
            }

            return SimulationSaveReplayCloner.ClonePackage(saved);
        }

        public SimulationSessionSavePackage? Find(string saveStableId)
        {
            if (string.IsNullOrWhiteSpace(saveStableId)) return null;
            return saves.TryGetValue(saveStableId, out var package)
                ? SimulationSaveReplayCloner.ClonePackage(package)
                : null;
        }
    }
}
