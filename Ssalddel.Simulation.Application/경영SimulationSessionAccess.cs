using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public sealed class 경영SimulationSessionAccessor
    {
        private readonly I경영SimulationSessionStore store;

        public 경영SimulationSessionAccessor(I경영SimulationSessionStore store)
            => this.store = store ?? throw new ArgumentNullException(nameof(store));

        public 경영SimulationSessionAggregate CreateOrGet(
            경영SimulationSession생성Request request)
            => store.CreateOrGet(request);

        public 경영SimulationSessionAggregate Require(string sessionStableId)
            => store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");

        public 경영SimulationSessionAggregate Restore(경영SimulationSessionAggregate session)
            => store.Restore(session);
    }
}
