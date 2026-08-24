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
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
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
