using System;
using System.Collections.Concurrent;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public interface ISimulationHexagramCampaignAttemptStore
    {
        int RegisterEntry(string sessionStableId, string hexagramStableId,
            string entrySaveStableId);
        int NextAttempt(string sessionStableId, string hexagramStableId);
        void RegisterSave(string saveStableId, string sessionStableId,
            string hexagramStableId, int attemptOrdinal);
        void EnsureRestoreAllowed(string saveStableId,
            SimulationHexagramCampaignStateSnapshot? packageState);
    }

    public sealed class InMemorySimulationHexagramCampaignAttemptStore
        : ISimulationHexagramCampaignAttemptStore
    {
        private readonly ConcurrentDictionary<string, int> attempts = new(
            StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, SaveAttempt> saves = new(
            StringComparer.Ordinal);

        public int RegisterEntry(string sessionStableId, string hexagramStableId,
            string entrySaveStableId)
        {
            var key = Key(sessionStableId, hexagramStableId);
            var attempt = attempts.AddOrUpdate(key, 1,
                (_, current) => Math.Max(1, current));
            saves[entrySaveStableId] = new SaveAttempt(key, 1);
            return attempt;
        }

        public int NextAttempt(string sessionStableId, string hexagramStableId)
            => attempts.AddOrUpdate(Key(sessionStableId, hexagramStableId), 2,
                (_, current) => checked(current + 1));

        public void RegisterSave(string saveStableId, string sessionStableId,
            string hexagramStableId, int attemptOrdinal)
        {
            if (string.IsNullOrWhiteSpace(hexagramStableId)
                || attemptOrdinal <= 0) return;
            var key = Key(sessionStableId, hexagramStableId);
            attempts.AddOrUpdate(key, attemptOrdinal,
                (_, current) => Math.Max(current, attemptOrdinal));
            saves[saveStableId] = new SaveAttempt(key, attemptOrdinal);
        }

        public void EnsureRestoreAllowed(string saveStableId,
            SimulationHexagramCampaignStateSnapshot? packageState)
        {
            if (packageState == null
                || string.IsNullOrWhiteSpace(packageState.HexagramStableId)
                || packageState.AttemptOrdinal <= 0) return;
            var key = saves.TryGetValue(saveStableId, out var recorded)
                ? recorded.Key
                : Key(string.Empty, packageState.HexagramStableId);
            if (attempts.TryGetValue(key, out var current)
                && packageState.AttemptOrdinal < current)
                throw new SimulationConflictException(
                    "HexagramCampaignSaveAttemptInvalidated");
        }

        private static string Key(string sessionStableId, string hexagramStableId)
            => (sessionStableId ?? string.Empty).Trim() + "|"
                + (hexagramStableId ?? string.Empty).Trim();

        private sealed record SaveAttempt(string Key, int AttemptOrdinal);
    }

    public sealed class SimulationHexagramCampaignService
    {
        private readonly 경영SimulationSessionAccessor sessions;
        private readonly 경영SimulationSession생명주기Service lifecycle;
        private readonly ISimulationHexagramCampaignAttemptStore attempts;

        public SimulationHexagramCampaignService(
            경영SimulationSessionAccessor sessions,
            경영SimulationSession생명주기Service lifecycle,
            ISimulationHexagramCampaignAttemptStore attempts)
        {
            this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
            this.attempts = attempts ?? throw new ArgumentNullException(nameof(attempts));
        }

        public SimulationHexagramCampaignStateSnapshot Get(string sessionStableId)
            => sessions.Require(sessionStableId).GetHexagramCampaignState();

        public SimulationHexagramCampaignStateSnapshot Enter(
            string sessionStableId, SimulationHexagramCampaignEnterRequest request)
        {
            var entrySaveStableId = "simulation-save:campaign-entry:"
                + sessionStableId.Trim() + ":" + request.HexagramStableId.Trim();
            var state = sessions.Require(sessionStableId)
                .BeginHexagramCampaign(request, entrySaveStableId);
            var package = lifecycle.Save(sessionStableId,
                new SimulationSessionSaveRequest
                {
                    SaveStableId = entrySaveStableId,
                    ExpectedRevision = state.EntryWorldRevision,
                });
            attempts.RegisterEntry(sessionStableId, state.HexagramStableId,
                package.SaveStableId);
            return state;
        }

        public SimulationHexagramCampaignStateSnapshot CompleteLine(
            string sessionStableId,
            SimulationHexagramCampaignLineCompleteRequest request)
            => sessions.Require(sessionStableId).CompleteHexagramLine(request);

        public SimulationHexagramCampaignStateSnapshot RecordSetback(
            string sessionStableId,
            SimulationHexagramCampaignSetbackRequest request)
            => sessions.Require(sessionStableId).RecordHexagramSetback(request);

        public SimulationHexagramCampaignStateSnapshot Fail(
            string sessionStableId,
            SimulationHexagramCampaignFailureRequest request)
        {
            var current = sessions.Require(sessionStableId);
            var entrySaveStableId = current.ValidateHexagramCampaignFailure(request);
            var before = current.GetHexagramCampaignState();
            var nextAttempt = attempts.NextAttempt(sessionStableId,
                before.HexagramStableId);
            lifecycle.RestoreForCampaignRetry(new SimulationSessionRestoreRequest
            {
                SaveStableId = entrySaveStableId,
            }, request.ExpectedRevision);
            return sessions.Require(sessionStableId)
                .RestartHexagramCampaign(request, nextAttempt);
        }

        public SimulationHexagramCampaignStateSnapshot Complete(
            string sessionStableId,
            SimulationHexagramCampaignCompleteRequest request)
            => sessions.Require(sessionStableId)
                .CompleteHexagramCampaign(request);
    }
}
