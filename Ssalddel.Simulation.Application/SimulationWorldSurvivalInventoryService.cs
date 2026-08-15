using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 같은 Simulation Session 원장에서 건물 컨테이너 재고와 플레이어 소지품을 읽고
    /// 이동시킨다. 운영 재고 DB나 공공데이터 원본을 변경하지 않는다.
    /// </summary>
    public sealed class SimulationWorldSurvivalInventoryService
    {
        private readonly I경영SimulationSessionStore store;

        public SimulationWorldSurvivalInventoryService(I경영SimulationSessionStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public SimulationWorldInventorySnapshot Get(string sessionStableId)
            => Find(sessionStableId).GetWorldInventory();

        public SimulationWorldItemAcquisitionPreviewSnapshot PreviewAcquisition(
            string sessionStableId,
            SimulationWorldItemAcquisitionPreviewRequest request)
            => Find(sessionStableId).PreviewWorldItemAcquisition(request);

        public SimulationWorldItemAcquisitionResultSnapshot ConfirmAcquisition(
            string sessionStableId,
            SimulationWorldItemAcquisitionConfirmRequest request)
            => Find(sessionStableId).ConfirmWorldItemAcquisition(request);

        private 경영SimulationSessionAggregate Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdInvalid");
            return store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
        }
    }
}
