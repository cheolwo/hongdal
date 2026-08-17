using System;
using System.Linq;
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
        private readonly ISimulationBattleResourceLockReader? battleLocks;

        public SimulationWorldSurvivalInventoryService(I경영SimulationSessionStore store,
            ISimulationBattleResourceLockReader? battleResourceLocks = null)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            battleLocks = battleResourceLocks;
        }

        public SimulationWorldInventorySnapshot Get(string sessionStableId)
            => Find(sessionStableId).GetWorldInventory();

        public SimulationWorldItemAcquisitionPreviewSnapshot PreviewAcquisition(
            string sessionStableId,
            SimulationWorldItemAcquisitionPreviewRequest request)
        {
            var preview = Find(sessionStableId).PreviewWorldItemAcquisition(request);
            if (battleLocks?.IsLocked(sessionStableId, request.ItemStackStableId) == true)
            {
                preview.CanConfirm = false;
                preview.EligibilityStateCode = SimulationWorldSurvivalInventoryCodes.Blocked;
                preview.BlockReasonCodes = preview.BlockReasonCodes
                    .Concat(new[] { "BattleResourceLocked" }).Distinct().ToArray();
            }
            return preview;
        }

        public SimulationWorldItemAcquisitionResultSnapshot ConfirmAcquisition(
            string sessionStableId,
            SimulationWorldItemAcquisitionConfirmRequest request)
        {
            if (battleLocks?.IsLocked(sessionStableId, request.ItemStackStableId) == true)
                throw new SimulationConflictException("BattleResourceLocked");
            return Find(sessionStableId).ConfirmWorldItemAcquisition(request);
        }

        private 경영SimulationSessionAggregate Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdInvalid");
            return store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
        }
    }
}
