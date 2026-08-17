using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public sealed class 경영Simulation물류창고Service
    {
        private readonly 경영SimulationSessionAccessor sessions;

        public 경영Simulation물류창고Service(경영SimulationSessionAccessor sessions)
            => this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));

        public SimulationLogisticsMovementPreviewSnapshot PreviewLogisticsMovement(
            string sessionStableId,
            SimulationLogisticsMovementPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewLogisticsMovement(request);

        public 경영SimulationSessionSnapshot ConfirmLogisticsMovement(
            string sessionStableId,
            SimulationLogisticsMovementConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmLogisticsMovement(request);

        public SimulationFreightDispatchPreviewSnapshot PreviewFreightDispatch(
            string sessionStableId,
            SimulationFreightDispatchPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewFreightDispatch(request);

        public 경영SimulationSessionSnapshot ConfirmFreightDispatch(
            string sessionStableId,
            SimulationFreightDispatchConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmFreightDispatch(request);

        public SimulationFreightTransportPreviewSnapshot PreviewFreightTransport(
            string sessionStableId,
            SimulationFreightTransportPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewFreightTransport(request);

        public 경영SimulationSessionSnapshot ConfirmFreightTransport(
            string sessionStableId,
            SimulationFreightTransportConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmFreightTransport(request);

        public SimulationDecisionPreviewSnapshot PreviewFreightReceipt(
            string sessionStableId,
            SimulationFreightReceiptPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewFreightReceipt(request);

        public 경영SimulationSessionSnapshot ConfirmFreightReceipt(
            string sessionStableId,
            SimulationFreightReceiptConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmFreightReceipt(request);

        public SimulationDecisionPreviewSnapshot PreviewWarehousePutAway(
            string sessionStableId,
            SimulationWarehousePutAwayPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewWarehousePutAway(request);

        public 경영SimulationSessionSnapshot ConfirmWarehousePutAway(
            string sessionStableId,
            SimulationWarehousePutAwayConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmWarehousePutAway(request);

        public SimulationDecisionPreviewSnapshot PreviewSupplyChainWork(
            string sessionStableId,
            SimulationSupplyChainWorkPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewSupplyChainWork(request);

        public 경영SimulationSessionSnapshot ConfirmSupplyChainWork(
            string sessionStableId,
            SimulationSupplyChainWorkConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmSupplyChainWork(request);
    }
}
