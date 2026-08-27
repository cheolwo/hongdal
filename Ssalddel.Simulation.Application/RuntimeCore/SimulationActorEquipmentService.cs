using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "물품 획득과 장착 상태 변경의 Preview·Confirm 실행 경계를 제공한다.",
        Boundary = "장착 상태와 파생 능력은 Simulation Core가 결정하며 표현 객체가 권위를 변경하지 않는다.",
        SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행)]
    public sealed class SimulationActorEquipmentService
    {
        private readonly I경영SimulationSessionStore store;

        public SimulationActorEquipmentService(I경영SimulationSessionStore store)
            => this.store = store ?? throw new ArgumentNullException(nameof(store));

        public SimulationActorEquipmentStateSnapshot Get(string sessionStableId)
            => Find(sessionStableId).GetActorEquipmentState();

        public SimulationActorItemAcquirePreviewSnapshot PreviewAcquire(
            string sessionStableId, SimulationActorItemAcquirePreviewRequest request)
            => Find(sessionStableId).PreviewActorItemAcquire(request);

        public SimulationActorEquipmentStateSnapshot ConfirmAcquire(
            string sessionStableId, SimulationActorItemAcquireConfirmRequest request)
            => Find(sessionStableId).ConfirmActorItemAcquire(request);

        public SimulationActorEquipmentChangePreviewSnapshot PreviewChange(
            string sessionStableId,
            SimulationActorEquipmentChangePreviewRequest request)
            => Find(sessionStableId).PreviewActorEquipmentChange(request);

        public SimulationActorEquipmentStateSnapshot ConfirmChange(
            string sessionStableId,
            SimulationActorEquipmentChangeConfirmRequest request)
            => Find(sessionStableId).ConfirmActorEquipmentChange(request);

        private 경영SimulationSessionAggregate Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException(
                    "SimulationSessionStableIdInvalid");
            return store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException(
                    "SimulationSessionNotFound");
        }
    }
}
