using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public sealed class Simulation타로화물운송PreviewService
    {
        private readonly I경영SimulationSessionStore store;

        public Simulation타로화물운송PreviewService(I경영SimulationSessionStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public Simulation타로화물운송통합PreviewSnapshot Preview(
            string sessionStableId,
            Simulation타로화물운송PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview타로화물운송(request);
    }
}
