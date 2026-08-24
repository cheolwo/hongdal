using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class 경영Simulation주문소비Service
    {
        private readonly 경영SimulationSessionAccessor sessions;

        public 경영Simulation주문소비Service(경영SimulationSessionAccessor sessions)
            => this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));

        public SimulationIndividualOrderPreviewSnapshot PreviewIndividualOrder(
            string sessionStableId,
            SimulationIndividualOrderPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewIndividualOrder(request);

        public 경영SimulationSessionSnapshot ConfirmIndividualOrder(
            string sessionStableId,
            SimulationIndividualOrderConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmIndividualOrder(request);

        public SimulationDecisionPreviewSnapshot PreviewIndividualOrderPickup(
            string sessionStableId,
            SimulationIndividualOrderPickupPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewIndividualOrderPickup(request);

        public 경영SimulationSessionSnapshot ConfirmIndividualOrderPickup(
            string sessionStableId,
            SimulationIndividualOrderPickupConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmIndividualOrderPickup(request);

        public SimulationDecisionPreviewSnapshot PreviewIndividualOrderCancellation(
            string sessionStableId,
            SimulationIndividualOrderCancelRequest request)
            => sessions.Require(sessionStableId).PreviewIndividualOrderCancellation(request);

        public 경영SimulationSessionSnapshot ConfirmIndividualOrderCancellation(
            string sessionStableId,
            SimulationIndividualOrderCancelRequest request)
            => sessions.Require(sessionStableId).ConfirmIndividualOrderCancellation(request);

        public Simulation같이주문PreviewSnapshot PreviewGroupOrder(
            string sessionStableId,
            Simulation같이주문PreviewRequest request)
            => sessions.Require(sessionStableId).PreviewGroupOrder(request);

        public 경영SimulationSessionSnapshot ConfirmGroupOrder(
            string sessionStableId,
            Simulation같이주문ConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmGroupOrder(request);

        public Simulation음식배달PreviewSnapshot PreviewFoodDelivery(
            string sessionStableId,
            Simulation음식배달PreviewRequest request)
            => sessions.Require(sessionStableId).PreviewFoodDelivery(request);

        public 경영SimulationSessionSnapshot ConfirmFoodDelivery(
            string sessionStableId,
            Simulation음식배달ConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmFoodDelivery(request);

        public SimulationDecisionPreviewSnapshot PreviewFoodDeliveryReceipt(
            string sessionStableId,
            Simulation음식배달수령PreviewRequest request)
            => sessions.Require(sessionStableId).PreviewFoodDeliveryReceipt(request);

        public 경영SimulationSessionSnapshot ConfirmFoodDeliveryReceipt(
            string sessionStableId,
            Simulation음식배달수령ConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmFoodDeliveryReceipt(request);

        public Simulation시장소비PreviewSnapshot PreviewMarketConsumption(
            string sessionStableId,
            Simulation시장소비PreviewRequest request)
            => sessions.Require(sessionStableId).PreviewMarketConsumption(request);

        public 경영SimulationSessionSnapshot ConfirmMarketConsumption(
            string sessionStableId,
            Simulation시장소비ConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmMarketConsumption(request);
    }
}
