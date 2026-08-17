using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public sealed class 경영Simulation턴결정Service
    {
        private readonly 경영SimulationSessionAccessor sessions;
        private readonly ISimulationBattleWorldReconciler? battleReconciler;

        public 경영Simulation턴결정Service(
            경영SimulationSessionAccessor sessions,
            ISimulationBattleWorldReconciler? battleReconciler = null)
        {
            this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            this.battleReconciler = battleReconciler;
        }

        public SimulationTurnClosingContextSnapshot GetTurnClosingContext(string sessionStableId)
            => sessions.Require(sessionStableId).GetTurnClosingContext();

        public SimulationTurnClosingPreviewSnapshot PreviewTurnClosing(
            string sessionStableId,
            SimulationTurnClosingPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewTurnClosing(request);

        public 경영SimulationSessionSnapshot ConfirmTurnClosing(
            string sessionStableId,
            SimulationTurnClosingConfirmRequest request)
        {
            var snapshot = sessions.Require(sessionStableId).ConfirmTurnClosing(request);
            battleReconciler?.Reconcile(sessionStableId, snapshot);
            return snapshot;
        }

        public SimulationDecisionPreviewSnapshot PreviewDecision(
            string sessionStableId,
            SimulationDecisionPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewDecision(request);

        public 경영SimulationSessionSnapshot ConfirmDecision(
            string sessionStableId,
            SimulationDecisionConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmDecision(request);

        public 경영SimulationSessionSnapshot CancelTask(
            string sessionStableId,
            string taskStableId,
            SimulationTaskCancelRequest request)
            => sessions.Require(sessionStableId).CancelTask(taskStableId, request);

        public 경영SimulationSessionSnapshot UpdateNpcPolicy(
            string sessionStableId,
            SimulationNpcPolicyChangeRequest request)
            => sessions.Require(sessionStableId).UpdateNpcPolicy(request);
    }
}
