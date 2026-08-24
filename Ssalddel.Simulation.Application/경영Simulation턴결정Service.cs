using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
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
