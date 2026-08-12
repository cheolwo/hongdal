using System;
using System.Collections.Concurrent;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Infrastructure
{
    public sealed class InMemory경영SimulationSessionStore : I경영SimulationSessionStore
    {
        private readonly ConcurrentDictionary<string, 경영SimulationSessionAggregate> sessions =
            new ConcurrentDictionary<string, 경영SimulationSessionAggregate>(StringComparer.Ordinal);

        public 경영SimulationSessionAggregate CreateOrGet(경영SimulationSession생성Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var candidate = new 경영SimulationSessionAggregate(request);
            var session = sessions.GetOrAdd(candidate.SessionStableId, candidate);
            session.EnsureSameCreationRequest(request);
            return session;
        }

        public 경영SimulationSessionAggregate? Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId)) return null;
            return sessions.TryGetValue(sessionStableId, out var session) ? session : null;
        }

        public 경영SimulationSessionAggregate Restore(경영SimulationSessionAggregate session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (!sessions.TryAdd(session.SessionStableId, session))
                throw new SimulationConflictException("SimulationSessionAlreadyActive");
            return session;
        }
    }


    public sealed class InMemorySimulationSessionSaveStore : ISimulationSessionSaveStore
    {
        private readonly ConcurrentDictionary<string, SimulationSessionSavePackage> saves =
            new ConcurrentDictionary<string, SimulationSessionSavePackage>(StringComparer.Ordinal);

        public SimulationSessionSavePackage SaveOrGet(SimulationSessionSavePackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            var candidate = SimulationSaveReplayCloner.ClonePackage(package);
            var saved = saves.GetOrAdd(candidate.SaveStableId, candidate);
            if (!string.Equals(saved.SessionStableId, candidate.SessionStableId, StringComparison.Ordinal)
                || !string.Equals(saved.ReplayHash, candidate.ReplayHash, StringComparison.Ordinal))
            {
                throw new SimulationConflictException("SimulationSaveStableIdConflict");
            }

            return SimulationSaveReplayCloner.ClonePackage(saved);
        }

        public SimulationSessionSavePackage? Find(string saveStableId)
        {
            if (string.IsNullOrWhiteSpace(saveStableId)) return null;
            return saves.TryGetValue(saveStableId, out var package)
                ? SimulationSaveReplayCloner.ClonePackage(package)
                : null;
        }
    }
}
