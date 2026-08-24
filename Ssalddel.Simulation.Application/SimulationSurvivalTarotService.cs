using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 생존 위기와 주기 기회를 조회하고 안전 거점 전원 합의를 확정한다.
    /// 카드 효과 수치는 클라이언트가 아니라 Session aggregate가 결정한다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class SimulationSurvivalTarotService
    {
        private readonly I경영SimulationSessionStore store;

        public SimulationSurvivalTarotService(I경영SimulationSessionStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public SimulationSurvivalTarotStateSnapshot Get(string sessionStableId)
            => Find(sessionStableId).GetSurvivalTarotState();

        public SimulationSurvivalTarotCommandResultSnapshot ConfirmResponse(
            string sessionStableId,
            SimulationSurvivalTarotResponseConfirmRequest request)
            => Find(sessionStableId).ConfirmSurvivalTarotResponse(request);

        public SimulationSurvivalTarotCommandResultSnapshot ConfirmResolution(
            string sessionStableId,
            SimulationSurvivalTarotResolutionConfirmRequest request)
            => Find(sessionStableId).ConfirmSurvivalTarotResolution(request);

        private 경영SimulationSessionAggregate Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdInvalid");
            return store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
        }
    }
}
