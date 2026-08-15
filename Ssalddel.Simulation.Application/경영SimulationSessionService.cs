using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public interface I경영SimulationSessionStore
    {
        경영SimulationSessionAggregate CreateOrGet(경영SimulationSession생성Request request);
        경영SimulationSessionAggregate? Find(string sessionStableId);
        경영SimulationSessionAggregate Restore(경영SimulationSessionAggregate session);
    }


    public interface ISimulationSessionSaveStore
    {
        SimulationSessionSavePackage SaveOrGet(SimulationSessionSavePackage package);
        SimulationSessionSavePackage? Find(string saveStableId);
    }

    public sealed class 경영SimulationSessionService
    {
        private readonly I경영SimulationSessionStore store;
        private readonly ISimulationSessionSaveStore saveStore;

        public 경영SimulationSessionService(
            I경영SimulationSessionStore store,
            ISimulationSessionSaveStore saveStore)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.saveStore = saveStore ?? throw new ArgumentNullException(nameof(saveStore));
        }

        public 경영SimulationSessionSnapshot Create(경영SimulationSession생성Request request)
            => store.CreateOrGet(request).Snapshot();

        public 경영SimulationSessionSnapshot Get(string sessionStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Snapshot();

        public 경영SimulationSessionSnapshot Advance(
            string sessionStableId,
            경영SimulationTick진행Request request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Advance(request);

        public SimulationTurnClosingContextSnapshot GetTurnClosingContext(
            string sessionStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .GetTurnClosingContext();

        public SimulationTurnClosingPreviewSnapshot PreviewTurnClosing(
            string sessionStableId,
            SimulationTurnClosingPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewTurnClosing(request);

        public 경영SimulationSessionSnapshot ConfirmTurnClosing(
            string sessionStableId,
            SimulationTurnClosingConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmTurnClosing(request);

        public SimulationDecisionPreviewSnapshot PreviewDecision(
            string sessionStableId,
            SimulationDecisionPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewDecision(request);

        public 경영SimulationSessionSnapshot ConfirmDecision(
            string sessionStableId,
            SimulationDecisionConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmDecision(request);

        public 경영SimulationSessionSnapshot UpdateNpcPolicy(
            string sessionStableId,
            SimulationNpcPolicyChangeRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .UpdateNpcPolicy(request);

        public SimulationIndividualOrderPreviewSnapshot PreviewIndividualOrder(
            string sessionStableId,
            SimulationIndividualOrderPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewIndividualOrder(request);

        public 경영SimulationSessionSnapshot ConfirmIndividualOrder(
            string sessionStableId,
            SimulationIndividualOrderConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmIndividualOrder(request);

        public SimulationDecisionPreviewSnapshot PreviewIndividualOrderCancellation(
            string sessionStableId,
            SimulationIndividualOrderCancelRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewIndividualOrderCancellation(request);

        public 경영SimulationSessionSnapshot ConfirmIndividualOrderCancellation(
            string sessionStableId,
            SimulationIndividualOrderCancelRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmIndividualOrderCancellation(request);

        public SimulationLogisticsMovementPreviewSnapshot PreviewLogisticsMovement(
            string sessionStableId,
            SimulationLogisticsMovementPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewLogisticsMovement(request);

        public 경영SimulationSessionSnapshot ConfirmLogisticsMovement(
            string sessionStableId,
            SimulationLogisticsMovementConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmLogisticsMovement(request);

        public SimulationFreightDispatchPreviewSnapshot PreviewFreightDispatch(
            string sessionStableId,
            SimulationFreightDispatchPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFreightDispatch(request);

        public 경영SimulationSessionSnapshot ConfirmFreightDispatch(
            string sessionStableId,
            SimulationFreightDispatchConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFreightDispatch(request);

        public SimulationFreightTransportPreviewSnapshot PreviewFreightTransport(
            string sessionStableId,
            SimulationFreightTransportPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFreightTransport(request);

        public 경영SimulationSessionSnapshot ConfirmFreightTransport(
            string sessionStableId,
            SimulationFreightTransportConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFreightTransport(request);

        public SimulationDecisionPreviewSnapshot PreviewFreightReceipt(
            string sessionStableId,
            SimulationFreightReceiptPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFreightReceipt(request);

        public 경영SimulationSessionSnapshot ConfirmFreightReceipt(
            string sessionStableId,
            SimulationFreightReceiptConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFreightReceipt(request);

        public SimulationDecisionPreviewSnapshot PreviewWarehousePutAway(
            string sessionStableId,
            SimulationWarehousePutAwayPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewWarehousePutAway(request);

        public 경영SimulationSessionSnapshot ConfirmWarehousePutAway(
            string sessionStableId,
            SimulationWarehousePutAwayConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmWarehousePutAway(request);

        public Simulation같이주문PreviewSnapshot PreviewGroupOrder(
            string sessionStableId,
            Simulation같이주문PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewGroupOrder(request);

        public 경영SimulationSessionSnapshot ConfirmGroupOrder(
            string sessionStableId,
            Simulation같이주문ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmGroupOrder(request);

        public Simulation음식배달PreviewSnapshot PreviewFoodDelivery(
            string sessionStableId,
            Simulation음식배달PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFoodDelivery(request);

        public 경영SimulationSessionSnapshot ConfirmFoodDelivery(
            string sessionStableId,
            Simulation음식배달ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFoodDelivery(request);

        public SimulationDecisionPreviewSnapshot PreviewFoodDeliveryReceipt(
            string sessionStableId,
            Simulation음식배달수령PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFoodDeliveryReceipt(request);

        public 경영SimulationSessionSnapshot ConfirmFoodDeliveryReceipt(
            string sessionStableId,
            Simulation음식배달수령ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFoodDeliveryReceipt(request);

        public Simulation시장소비PreviewSnapshot PreviewMarketConsumption(
            string sessionStableId,
            Simulation시장소비PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewMarketConsumption(request);

        public 경영SimulationSessionSnapshot ConfirmMarketConsumption(
            string sessionStableId,
            Simulation시장소비ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmMarketConsumption(request);

        public SimulationSessionSavePackage Save(
            string sessionStableId,
            SimulationSessionSaveRequest request)
        {
            var session = store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
            return saveStore.SaveOrGet(session.CreateSavePackage(request));
        }

        public SimulationSessionRestoreResult Restore(SimulationSessionRestoreRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.SaveStableId))
                throw new SimulationContractException("SimulationSaveStableIdInvalid");
            var package = saveStore.Find(request.SaveStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSaveNotFound");
            var restored = SimulationSessionReplay.Restore(package);
            store.Restore(restored);
            return new SimulationSessionRestoreResult
            {
                SaveStableId = package.SaveStableId,
                SchemaVersion = package.SchemaVersion,
                ReplayHash = package.ReplayHash,
                ReplayedCommandCount = package.CommandLog.Length,
                Session = restored.Snapshot(),
            };
        }

        public SimulationHarvestDispositionImpactPreviewSnapshot PreviewHarvestDispositionImpact(
            string sessionStableId,
            SimulationHarvestDispositionImpactPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewHarvestDispositionImpact(request);

        public 경영SimulationSessionSnapshot ConfirmHarvestDispositionImpact(
            string sessionStableId,
            SimulationHarvestDispositionImpactConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmHarvestDispositionImpact(request);

        public Simulation수출준비PreviewSnapshot Preview수출준비(
            string sessionStableId,
            Simulation수출준비PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출준비(request);

        public 경영SimulationSessionSnapshot Confirm수출준비(
            string sessionStableId,
            Simulation수출준비ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출준비(request);

        public Simulation수출준비PreviewSnapshot Preview수출재작업(
            string sessionStableId,
            Simulation수출재작업PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출재작업(request);

        public 경영SimulationSessionSnapshot Confirm수출재작업(
            string sessionStableId,
            Simulation수출재작업ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출재작업(request);

        public Simulation수출Cargo준비PreviewSnapshot Preview수출Cargo준비(
            string sessionStableId,
            Simulation수출Cargo준비PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출Cargo준비(request);

        public 경영SimulationSessionSnapshot Confirm수출Cargo준비(
            string sessionStableId,
            Simulation수출Cargo준비ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출Cargo준비(request);

        public Simulation수출Cargo인계PreviewSnapshot Preview수출Cargo인계(
            string sessionStableId,
            Simulation수출Cargo인계PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출Cargo인계(request);

        public 경영SimulationSessionSnapshot Confirm수출Cargo인계(
            string sessionStableId,
            Simulation수출Cargo인계ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출Cargo인계(request);

        public Simulation수출항만인수PreviewSnapshot Preview수출항만인수(
            string sessionStableId,
            Simulation수출항만인수PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출항만인수(request);

        public 경영SimulationSessionSnapshot Confirm수출항만인수(
            string sessionStableId,
            Simulation수출항만인수ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출항만인수(request);

        public Simulation수출준비성검토PreviewSnapshot Preview수출준비성검토(
            string sessionStableId,
            Simulation수출준비성검토PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출준비성검토(request);

        public 경영SimulationSessionSnapshot Confirm수출준비성검토(
            string sessionStableId,
            Simulation수출준비성검토ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출준비성검토(request);

        public Simulation수출선적계획PreviewSnapshot Preview수출선적계획(
            string sessionStableId,
            Simulation수출선적계획PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출선적계획(request);

        public 경영SimulationSessionSnapshot Confirm수출선적계획(
            string sessionStableId,
            Simulation수출선적계획ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출선적계획(request);

        public Simulation수출선적실행PreviewSnapshot Preview수출선적실행(
            string sessionStableId,
            Simulation수출선적실행PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출선적실행(request);

        public 경영SimulationSessionSnapshot Confirm수출선적실행(
            string sessionStableId,
            Simulation수출선적실행ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출선적실행(request);

        public Simulation수확판로결과Snapshot Get수확판로결과(
            string sessionStableId,
            string harvestLotStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Get수확판로결과(harvestLotStableId);

        public Simulation수확판로결과Snapshot[] Get수확판로결과목록(string sessionStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Get수확판로결과목록();
    }
}
