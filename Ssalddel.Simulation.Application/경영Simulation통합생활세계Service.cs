using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// H5 정적 공간 위에서 제조·건설·편성·복구 명령을 같은 Session으로 조율한다.
    /// WI와 HTTP를 일대일로 만들지 않고 사용자 명령 진입점만 노출한다.
    /// </summary>
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
    }
}
