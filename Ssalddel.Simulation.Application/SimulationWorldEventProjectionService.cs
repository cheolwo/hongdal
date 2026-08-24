using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class SimulationWorldEventProjectionService
    {
        private readonly I경영SimulationSessionStore store;

        public SimulationWorldEventProjectionService(I경영SimulationSessionStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public SimulationWorldEventProjectionSnapshot GetChanges(
            string sessionStableId,
            long afterWorldRevision)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdInvalid");
            var session = store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
            return session.GetWorldEvents(afterWorldRevision);
        }
    }
}
