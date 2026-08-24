using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class 경영Simulation수확수출Service
    {
        private readonly 경영SimulationSessionAccessor sessions;

        public 경영Simulation수확수출Service(경영SimulationSessionAccessor sessions)
            => this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));

        public SimulationHarvestDispositionImpactPreviewSnapshot PreviewHarvestDispositionImpact(
            string sessionStableId,
            SimulationHarvestDispositionImpactPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewHarvestDispositionImpact(request);

        public 경영SimulationSessionSnapshot ConfirmHarvestDispositionImpact(
            string sessionStableId,
            SimulationHarvestDispositionImpactConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmHarvestDispositionImpact(request);

        public SimulationFarmChoiceContextSnapshot GetFarmChoiceContext(
            string sessionStableId)
            => sessions.Require(sessionStableId).GetFarmChoiceContext();

        public SimulationFarmChoicePreviewSnapshot PreviewFarmChoice(
            string sessionStableId,
            SimulationFarmChoicePreviewRequest request)
            => sessions.Require(sessionStableId).PreviewFarmChoice(request);

        public 경영SimulationSessionSnapshot ConfirmFarmChoice(
            string sessionStableId,
            SimulationFarmChoiceConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmFarmChoice(request);

        public Simulation수출준비PreviewSnapshot Preview수출준비(
            string sessionStableId,
            Simulation수출준비PreviewRequest request)
            => sessions.Require(sessionStableId).Preview수출준비(request);

        public 경영SimulationSessionSnapshot Confirm수출준비(
            string sessionStableId,
            Simulation수출준비ConfirmRequest request)
            => sessions.Require(sessionStableId).Confirm수출준비(request);

        public Simulation수출준비PreviewSnapshot Preview수출재작업(
            string sessionStableId,
            Simulation수출재작업PreviewRequest request)
            => sessions.Require(sessionStableId).Preview수출재작업(request);

        public 경영SimulationSessionSnapshot Confirm수출재작업(
            string sessionStableId,
            Simulation수출재작업ConfirmRequest request)
            => sessions.Require(sessionStableId).Confirm수출재작업(request);

        public Simulation수출Cargo준비PreviewSnapshot Preview수출Cargo준비(
            string sessionStableId,
            Simulation수출Cargo준비PreviewRequest request)
            => sessions.Require(sessionStableId).Preview수출Cargo준비(request);

        public 경영SimulationSessionSnapshot Confirm수출Cargo준비(
            string sessionStableId,
            Simulation수출Cargo준비ConfirmRequest request)
            => sessions.Require(sessionStableId).Confirm수출Cargo준비(request);

        public Simulation수출Cargo인계PreviewSnapshot Preview수출Cargo인계(
            string sessionStableId,
            Simulation수출Cargo인계PreviewRequest request)
            => sessions.Require(sessionStableId).Preview수출Cargo인계(request);

        public 경영SimulationSessionSnapshot Confirm수출Cargo인계(
            string sessionStableId,
            Simulation수출Cargo인계ConfirmRequest request)
            => sessions.Require(sessionStableId).Confirm수출Cargo인계(request);

        public Simulation수출항만인수PreviewSnapshot Preview수출항만인수(
            string sessionStableId,
            Simulation수출항만인수PreviewRequest request)
            => sessions.Require(sessionStableId).Preview수출항만인수(request);

        public 경영SimulationSessionSnapshot Confirm수출항만인수(
            string sessionStableId,
            Simulation수출항만인수ConfirmRequest request)
            => sessions.Require(sessionStableId).Confirm수출항만인수(request);

        public Simulation수출준비성검토PreviewSnapshot Preview수출준비성검토(
            string sessionStableId,
            Simulation수출준비성검토PreviewRequest request)
            => sessions.Require(sessionStableId).Preview수출준비성검토(request);

        public 경영SimulationSessionSnapshot Confirm수출준비성검토(
            string sessionStableId,
            Simulation수출준비성검토ConfirmRequest request)
            => sessions.Require(sessionStableId).Confirm수출준비성검토(request);

        public Simulation수출선적계획PreviewSnapshot Preview수출선적계획(
            string sessionStableId,
            Simulation수출선적계획PreviewRequest request)
            => sessions.Require(sessionStableId).Preview수출선적계획(request);

        public 경영SimulationSessionSnapshot Confirm수출선적계획(
            string sessionStableId,
            Simulation수출선적계획ConfirmRequest request)
            => sessions.Require(sessionStableId).Confirm수출선적계획(request);

        public Simulation수출선적실행PreviewSnapshot Preview수출선적실행(
            string sessionStableId,
            Simulation수출선적실행PreviewRequest request)
            => sessions.Require(sessionStableId).Preview수출선적실행(request);

        public 경영SimulationSessionSnapshot Confirm수출선적실행(
            string sessionStableId,
            Simulation수출선적실행ConfirmRequest request)
            => sessions.Require(sessionStableId).Confirm수출선적실행(request);

        public Simulation수확판로결과Snapshot Get수확판로결과(
            string sessionStableId,
            string harvestLotStableId)
            => sessions.Require(sessionStableId).Get수확판로결과(harvestLotStableId);

        public Simulation수확판로결과Snapshot[] Get수확판로결과목록(
            string sessionStableId)
            => sessions.Require(sessionStableId).Get수확판로결과목록();
    }
}
