using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public interface ISimulationBattleRuntimeProjectionProvider
    {
        SimulationBattleRelevantRuntimeProjectionSnapshot Create(string sessionStableId,
            string encounterScopeStableId, string areaRoleCode);
    }

    public sealed class SimulationBattleRuntimeProjectionProvider
        : ISimulationBattleRuntimeProjectionProvider
    {
        private readonly 경영SimulationSessionAccessor sessions;

        public SimulationBattleRuntimeProjectionProvider(경영SimulationSessionAccessor sessionAccessor)
            => sessions = sessionAccessor ?? throw new ArgumentNullException(nameof(sessionAccessor));

        public SimulationBattleRelevantRuntimeProjectionSnapshot Create(string sessionStableId,
            string encounterScopeStableId, string areaRoleCode)
            => sessions.Require(sessionStableId)
                .CreateBattleRelevantRuntimeProjectionForArea(encounterScopeStableId, areaRoleCode);
    }
}
