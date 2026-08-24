using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// H5 정적 공간 위에서 제조·건설·편성·복구 명령을 같은 Session으로 조율한다.
    /// WI와 HTTP를 일대일로 만들지 않고 사용자 명령 진입점만 노출한다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class 경영Simulation통합생활세계Service
    {
        private readonly 경영SimulationSessionAccessor sessions;

        public 경영Simulation통합생활세계Service(경영SimulationSessionAccessor sessionAccessor)
            => sessions = sessionAccessor ?? throw new ArgumentNullException(nameof(sessionAccessor));

        public SimulationIntegratedWorldPreviewSnapshot Preview(string sessionStableId,
            SimulationIntegratedWorldCommandRequest request)
            => sessions.Require(sessionStableId).PreviewIntegratedWorldCommand(request);

        public 경영SimulationSessionSnapshot Confirm(string sessionStableId,
            SimulationIntegratedWorldCommandRequest request)
            => sessions.Require(sessionStableId).ConfirmIntegratedWorldCommand(request);

        public SimulationFarmConstructionPlacementPreviewSnapshot PreviewFarmPlacement(
            string sessionStableId,
            SimulationFarmConstructionPlacementPreviewRequest request)
            => sessions.Require(sessionStableId)
                .PreviewFarmConstructionPlacement(request);

        public 경영SimulationSessionSnapshot ConfirmFarmPlacement(
            string sessionStableId,
            SimulationFarmConstructionPlacementConfirmRequest request)
            => sessions.Require(sessionStableId)
                .ConfirmFarmConstructionPlacement(request);
    }
}
