using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public sealed class Simulation타로객체반응PreviewService
    {
        private readonly I경영SimulationSessionStore store;

        public Simulation타로객체반응PreviewService(I경영SimulationSessionStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public Simulation타로객체반응PreviewSnapshot Preview(
            string sessionStableId,
            Simulation타로객체반응PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview타로객체반응(request);
    }
}
