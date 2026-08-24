using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class Simulation타로객체반응PreviewService
    {
        private readonly I경영SimulationSessionStore store;

        public Simulation타로객체반응PreviewService(I경영SimulationSessionStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public Simulation타로객체반응PreviewSnapshot Preview(
            string sessionStableId,
            Simulation타로객체반응PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview타로객체반응(request);
    }
}
