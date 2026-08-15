using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public sealed class SimulationWorldEventProjectionService
    {
        private readonly I경영SimulationSessionStore store;

        public SimulationWorldEventProjectionService(I경영SimulationSessionStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public SimulationWorldEventProjectionSnapshot GetChanges(
            string sessionStableId,
            long afterWorldRevision)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdInvalid");
            var session = store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
            return session.GetWorldEvents(afterWorldRevision);
        }
    }
}
