using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 플레이어 직접 노동과 NPC 위임, 농장 방어, 위협 대응을 Simulation 상태로만 확정한다.
    /// 실제 사람·사업체·건물에 감염, 약탈, 전투 의미를 부여하지 않는다.
    /// </summary>
    public sealed class SimulationFarmSurvivalService
    {
        private readonly I경영SimulationSessionStore store;

        public SimulationFarmSurvivalService(I경영SimulationSessionStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public SimulationFarmSurvivalStateSnapshot Get(string sessionStableId)
            => Find(sessionStableId).GetFarmSurvivalState();

        public SimulationFarmWorkPreviewSnapshot PreviewWork(
            string sessionStableId,
            SimulationFarmWorkPreviewRequest request)
            => Find(sessionStableId).PreviewFarmWork(request);

        public SimulationFarmSurvivalStateSnapshot ConfirmWork(
            string sessionStableId,
            SimulationFarmWorkConfirmRequest request)
            => Find(sessionStableId).ConfirmFarmWork(request);

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
    }
}
