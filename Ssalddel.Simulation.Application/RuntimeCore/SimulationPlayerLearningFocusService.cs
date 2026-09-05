using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "LocalProcess와 RemoteHost가 공유하는 NPC 학습중점 조회·미리보기·확정 경계다.",
        Boundary = "Application은 카드 효과를 다시 계산하지 않고 Session Core를 호출한다.")]
    public sealed class SimulationPlayerLearningFocusService
    {
        private readonly I경영SimulationSessionStore sessionStore;

        public SimulationPlayerLearningFocusService(
            I경영SimulationSessionStore simulationSessionStore)
        {
            sessionStore = simulationSessionStore
                ?? throw new ArgumentNullException(nameof(simulationSessionStore));
        }

        public Simulation학습중점ProjectionSnapshot Get(
            string sessionStableId,
            string playerStableId)
        {
            var session = Find(sessionStableId);
            var projection = session.GetLearningFocusProjection();
            EnsurePlayer(projection.PlayerStableId, playerStableId);
            return projection;
        }

        public Simulation학습중점PreviewSnapshot Preview(
            string sessionStableId,
            Simulation학습중점ChangeRequest request)
            => Find(sessionStableId).PreviewLearningFocusChange(request);

        public Simulation학습중점StateSnapshot Confirm(
            string sessionStableId,
            Simulation학습중점ChangeRequest request)
            => Find(sessionStableId).ConfirmLearningFocusChange(request);

        private 경영SimulationSessionAggregate Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException(
                    "SimulationSessionStableIdInvalid");
            return sessionStore.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException(
                    "SimulationSessionNotFound");
        }

        private static void EnsurePlayer(string expected, string supplied)
        {
            if (!string.Equals(expected, supplied?.Trim(),
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningFocusPlayerMismatch");
        }
    }
}
