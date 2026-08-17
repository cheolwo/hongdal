using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
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
