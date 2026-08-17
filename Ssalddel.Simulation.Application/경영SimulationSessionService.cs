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

    // 기존 호출자를 보존하는 호환 Facade다. 새 Controller는 아래 기능별 Service를 직접 사용한다.
    public sealed class 경영SimulationSessionService
    {
        private readonly 경영SimulationSession생명주기Service lifecycle;
        private readonly 경영Simulation턴결정Service turnDecision;
        private readonly 경영Simulation물류창고Service logistics;
        private readonly 경영Simulation주문소비Service commerce;
        private readonly 경영Simulation수확수출Service harvestExport;

        public 경영SimulationSessionService(
            I경영SimulationSessionStore store,
            ISimulationSessionSaveStore saveStore,
            ISimulationBattleWorldReconciler? battleReconciler = null)
        {
            var sessions = new 경영SimulationSessionAccessor(store);
            lifecycle = new 경영SimulationSession생명주기Service(
                sessions,
                saveStore,
                battleReconciler);
            turnDecision = new 경영Simulation턴결정Service(sessions, battleReconciler);
            logistics = new 경영Simulation물류창고Service(sessions);
            commerce = new 경영Simulation주문소비Service(sessions);
            harvestExport = new 경영Simulation수확수출Service(sessions);
        }

        public 경영SimulationSessionSnapshot Create(경영SimulationSession생성Request request)
            => lifecycle.Create(request);
        public 경영SimulationSessionSnapshot Get(string sessionStableId)
            => lifecycle.Get(sessionStableId);
        public 경영SimulationSessionSnapshot Advance(
            string sessionStableId, 경영SimulationTick진행Request request)
            => lifecycle.Advance(sessionStableId, request);
        public SimulationSessionSavePackage Save(
            string sessionStableId, SimulationSessionSaveRequest request)
            => lifecycle.Save(sessionStableId, request);
        public SimulationSessionRestoreResult Restore(SimulationSessionRestoreRequest request)
            => lifecycle.Restore(request);

        public SimulationTurnClosingContextSnapshot GetTurnClosingContext(string sessionStableId)
            => turnDecision.GetTurnClosingContext(sessionStableId);
        public SimulationTurnClosingPreviewSnapshot PreviewTurnClosing(
            string sessionStableId, SimulationTurnClosingPreviewRequest request)
            => turnDecision.PreviewTurnClosing(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmTurnClosing(
            string sessionStableId, SimulationTurnClosingConfirmRequest request)
            => turnDecision.ConfirmTurnClosing(sessionStableId, request);
        public SimulationDecisionPreviewSnapshot PreviewDecision(
            string sessionStableId, SimulationDecisionPreviewRequest request)
            => turnDecision.PreviewDecision(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmDecision(
            string sessionStableId, SimulationDecisionConfirmRequest request)
            => turnDecision.ConfirmDecision(sessionStableId, request);
        public 경영SimulationSessionSnapshot CancelTask(
            string sessionStableId,
            string taskStableId,
            SimulationTaskCancelRequest request)
            => turnDecision.CancelTask(sessionStableId, taskStableId, request);
        public 경영SimulationSessionSnapshot UpdateNpcPolicy(
            string sessionStableId, SimulationNpcPolicyChangeRequest request)
            => turnDecision.UpdateNpcPolicy(sessionStableId, request);

        public SimulationIndividualOrderPreviewSnapshot PreviewIndividualOrder(
            string sessionStableId, SimulationIndividualOrderPreviewRequest request)
            => commerce.PreviewIndividualOrder(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmIndividualOrder(
            string sessionStableId, SimulationIndividualOrderConfirmRequest request)
            => commerce.ConfirmIndividualOrder(sessionStableId, request);
        public SimulationDecisionPreviewSnapshot PreviewIndividualOrderPickup(
            string sessionStableId, SimulationIndividualOrderPickupPreviewRequest request)
            => commerce.PreviewIndividualOrderPickup(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmIndividualOrderPickup(
            string sessionStableId, SimulationIndividualOrderPickupConfirmRequest request)
            => commerce.ConfirmIndividualOrderPickup(sessionStableId, request);
        public SimulationDecisionPreviewSnapshot PreviewIndividualOrderCancellation(
            string sessionStableId, SimulationIndividualOrderCancelRequest request)
            => commerce.PreviewIndividualOrderCancellation(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmIndividualOrderCancellation(
            string sessionStableId, SimulationIndividualOrderCancelRequest request)
            => commerce.ConfirmIndividualOrderCancellation(sessionStableId, request);
        public Simulation같이주문PreviewSnapshot PreviewGroupOrder(
            string sessionStableId, Simulation같이주문PreviewRequest request)
            => commerce.PreviewGroupOrder(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmGroupOrder(
            string sessionStableId, Simulation같이주문ConfirmRequest request)
            => commerce.ConfirmGroupOrder(sessionStableId, request);
        public Simulation음식배달PreviewSnapshot PreviewFoodDelivery(
            string sessionStableId, Simulation음식배달PreviewRequest request)
            => commerce.PreviewFoodDelivery(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmFoodDelivery(
            string sessionStableId, Simulation음식배달ConfirmRequest request)
            => commerce.ConfirmFoodDelivery(sessionStableId, request);
        public SimulationDecisionPreviewSnapshot PreviewFoodDeliveryReceipt(
            string sessionStableId, Simulation음식배달수령PreviewRequest request)
            => commerce.PreviewFoodDeliveryReceipt(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmFoodDeliveryReceipt(
            string sessionStableId, Simulation음식배달수령ConfirmRequest request)
            => commerce.ConfirmFoodDeliveryReceipt(sessionStableId, request);
        public Simulation시장소비PreviewSnapshot PreviewMarketConsumption(
            string sessionStableId, Simulation시장소비PreviewRequest request)
            => commerce.PreviewMarketConsumption(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmMarketConsumption(
            string sessionStableId, Simulation시장소비ConfirmRequest request)
            => commerce.ConfirmMarketConsumption(sessionStableId, request);

        public SimulationLogisticsMovementPreviewSnapshot PreviewLogisticsMovement(
            string sessionStableId, SimulationLogisticsMovementPreviewRequest request)
            => logistics.PreviewLogisticsMovement(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmLogisticsMovement(
            string sessionStableId, SimulationLogisticsMovementConfirmRequest request)
            => logistics.ConfirmLogisticsMovement(sessionStableId, request);
        public SimulationFreightDispatchPreviewSnapshot PreviewFreightDispatch(
            string sessionStableId, SimulationFreightDispatchPreviewRequest request)
            => logistics.PreviewFreightDispatch(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmFreightDispatch(
            string sessionStableId, SimulationFreightDispatchConfirmRequest request)
            => logistics.ConfirmFreightDispatch(sessionStableId, request);
        public SimulationFreightTransportPreviewSnapshot PreviewFreightTransport(
            string sessionStableId, SimulationFreightTransportPreviewRequest request)
            => logistics.PreviewFreightTransport(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmFreightTransport(
            string sessionStableId, SimulationFreightTransportConfirmRequest request)
            => logistics.ConfirmFreightTransport(sessionStableId, request);
        public SimulationDecisionPreviewSnapshot PreviewFreightReceipt(
            string sessionStableId, SimulationFreightReceiptPreviewRequest request)
            => logistics.PreviewFreightReceipt(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmFreightReceipt(
            string sessionStableId, SimulationFreightReceiptConfirmRequest request)
            => logistics.ConfirmFreightReceipt(sessionStableId, request);
        public SimulationDecisionPreviewSnapshot PreviewWarehousePutAway(
            string sessionStableId, SimulationWarehousePutAwayPreviewRequest request)
            => logistics.PreviewWarehousePutAway(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmWarehousePutAway(
            string sessionStableId, SimulationWarehousePutAwayConfirmRequest request)
            => logistics.ConfirmWarehousePutAway(sessionStableId, request);
        public SimulationDecisionPreviewSnapshot PreviewSupplyChainWork(
            string sessionStableId, SimulationSupplyChainWorkPreviewRequest request)
            => logistics.PreviewSupplyChainWork(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmSupplyChainWork(
            string sessionStableId, SimulationSupplyChainWorkConfirmRequest request)
            => logistics.ConfirmSupplyChainWork(sessionStableId, request);

        public SimulationHarvestDispositionImpactPreviewSnapshot PreviewHarvestDispositionImpact(
            string sessionStableId, SimulationHarvestDispositionImpactPreviewRequest request)
            => harvestExport.PreviewHarvestDispositionImpact(sessionStableId, request);
        public 경영SimulationSessionSnapshot ConfirmHarvestDispositionImpact(
            string sessionStableId, SimulationHarvestDispositionImpactConfirmRequest request)
            => harvestExport.ConfirmHarvestDispositionImpact(sessionStableId, request);
        public Simulation수출준비PreviewSnapshot Preview수출준비(
            string sessionStableId, Simulation수출준비PreviewRequest request)
            => harvestExport.Preview수출준비(sessionStableId, request);
        public 경영SimulationSessionSnapshot Confirm수출준비(
            string sessionStableId, Simulation수출준비ConfirmRequest request)
            => harvestExport.Confirm수출준비(sessionStableId, request);
        public Simulation수출준비PreviewSnapshot Preview수출재작업(
            string sessionStableId, Simulation수출재작업PreviewRequest request)
            => harvestExport.Preview수출재작업(sessionStableId, request);
        public 경영SimulationSessionSnapshot Confirm수출재작업(
            string sessionStableId, Simulation수출재작업ConfirmRequest request)
            => harvestExport.Confirm수출재작업(sessionStableId, request);
        public Simulation수출Cargo준비PreviewSnapshot Preview수출Cargo준비(
            string sessionStableId, Simulation수출Cargo준비PreviewRequest request)
            => harvestExport.Preview수출Cargo준비(sessionStableId, request);
        public 경영SimulationSessionSnapshot Confirm수출Cargo준비(
            string sessionStableId, Simulation수출Cargo준비ConfirmRequest request)
            => harvestExport.Confirm수출Cargo준비(sessionStableId, request);
        public Simulation수출Cargo인계PreviewSnapshot Preview수출Cargo인계(
            string sessionStableId, Simulation수출Cargo인계PreviewRequest request)
            => harvestExport.Preview수출Cargo인계(sessionStableId, request);
        public 경영SimulationSessionSnapshot Confirm수출Cargo인계(
            string sessionStableId, Simulation수출Cargo인계ConfirmRequest request)
            => harvestExport.Confirm수출Cargo인계(sessionStableId, request);
        public Simulation수출항만인수PreviewSnapshot Preview수출항만인수(
            string sessionStableId, Simulation수출항만인수PreviewRequest request)
            => harvestExport.Preview수출항만인수(sessionStableId, request);
        public 경영SimulationSessionSnapshot Confirm수출항만인수(
            string sessionStableId, Simulation수출항만인수ConfirmRequest request)
            => harvestExport.Confirm수출항만인수(sessionStableId, request);
        public Simulation수출준비성검토PreviewSnapshot Preview수출준비성검토(
            string sessionStableId, Simulation수출준비성검토PreviewRequest request)
            => harvestExport.Preview수출준비성검토(sessionStableId, request);
        public 경영SimulationSessionSnapshot Confirm수출준비성검토(
            string sessionStableId, Simulation수출준비성검토ConfirmRequest request)
            => harvestExport.Confirm수출준비성검토(sessionStableId, request);
        public Simulation수출선적계획PreviewSnapshot Preview수출선적계획(
            string sessionStableId, Simulation수출선적계획PreviewRequest request)
            => harvestExport.Preview수출선적계획(sessionStableId, request);
        public 경영SimulationSessionSnapshot Confirm수출선적계획(
            string sessionStableId, Simulation수출선적계획ConfirmRequest request)
            => harvestExport.Confirm수출선적계획(sessionStableId, request);
        public Simulation수출선적실행PreviewSnapshot Preview수출선적실행(
            string sessionStableId, Simulation수출선적실행PreviewRequest request)
            => harvestExport.Preview수출선적실행(sessionStableId, request);
        public 경영SimulationSessionSnapshot Confirm수출선적실행(
            string sessionStableId, Simulation수출선적실행ConfirmRequest request)
            => harvestExport.Confirm수출선적실행(sessionStableId, request);
        public Simulation수확판로결과Snapshot Get수확판로결과(
            string sessionStableId, string harvestLotStableId)
            => harvestExport.Get수확판로결과(sessionStableId, harvestLotStableId);
        public Simulation수확판로결과Snapshot[] Get수확판로결과목록(string sessionStableId)
            => harvestExport.Get수확판로결과목록(sessionStableId);
    }
}
