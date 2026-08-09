using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class SimulationContractException : InvalidOperationException
    {
        public SimulationContractException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }

    public sealed class SimulationNotFoundException : InvalidOperationException
    {
        public SimulationNotFoundException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }

    public sealed class SimulationConflictException : InvalidOperationException
    {
        public SimulationConflictException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }

    public sealed class 경영SimulationSessionAggregate
    {
        private readonly object gate = new object();
        private readonly Dictionary<string, 적용된TickCommand> appliedCommands =
            new Dictionary<string, 적용된TickCommand>(StringComparer.Ordinal);

        public 경영SimulationSessionAggregate(경영SimulationSession생성Request request)
        {
            ValidateCreate(request);
            SessionStableId = "simulation-session:" + request.ClientRequestId.ToString("N");
            ClientRequestId = request.ClientRequestId;
            ScenarioStableId = request.ScenarioStableId.Trim();
            ScenarioDataRevision = request.ScenarioDataRevision.Trim();
            ScenarioSeed = request.ScenarioSeed;
            RuleRevision = request.RuleRevision.Trim();
            DurationTicks = request.DurationTicks;
        }

        public string SessionStableId { get; }
        public Guid ClientRequestId { get; }
        public string ScenarioStableId { get; }
        public string ScenarioDataRevision { get; }
        public int ScenarioSeed { get; }
        public string RuleRevision { get; }
        public int CurrentTick { get; private set; }
        public int DurationTicks { get; }
        public long Revision { get; private set; }

        public 경영SimulationSessionSnapshot Snapshot()
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }

        public 경영SimulationSessionSnapshot Advance(경영SimulationTick진행Request request)
        {
            ValidateAdvance(request);
            lock (gate)
            {
                if (appliedCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (applied.TickCount != request.TickCount)
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }

                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                if (CurrentTick + request.TickCount > DurationTicks)
                    throw new SimulationConflictException("SimulationDurationExceeded");

                CurrentTick += request.TickCount;
                Revision++;
                var snapshot = CreateSnapshot();
                appliedCommands.Add(
                    request.CommandId,
                    new 적용된TickCommand(request.TickCount, Clone(snapshot)));
                return snapshot;
            }
        }

        public void EnsureSameCreationRequest(경영SimulationSession생성Request request)
        {
            ValidateCreate(request);
            if (ClientRequestId != request.ClientRequestId
                || !string.Equals(ScenarioStableId, request.ScenarioStableId.Trim(), StringComparison.Ordinal)
                || !string.Equals(ScenarioDataRevision, request.ScenarioDataRevision.Trim(), StringComparison.Ordinal)
                || ScenarioSeed != request.ScenarioSeed
                || !string.Equals(RuleRevision, request.RuleRevision.Trim(), StringComparison.Ordinal)
                || DurationTicks != request.DurationTicks)
            {
                throw new SimulationConflictException("SimulationCreateRequestPayloadConflict");
            }
        }

        private 경영SimulationSessionSnapshot CreateSnapshot()
            => new 경영SimulationSessionSnapshot
            {
                SessionStableId = SessionStableId,
                ClientRequestId = ClientRequestId,
                ScenarioStableId = ScenarioStableId,
                ScenarioDataRevision = ScenarioDataRevision,
                ScenarioSeed = ScenarioSeed,
                RuleRevision = RuleRevision,
                CurrentTick = CurrentTick,
                DurationTicks = DurationTicks,
                Revision = Revision,
                IsCompleted = CurrentTick == DurationTicks,
                ModeCode = SimulationModeCodes.Simulation,
                IsOperationalState = false,
            };

        private static 경영SimulationSessionSnapshot Clone(경영SimulationSessionSnapshot source)
            => new 경영SimulationSessionSnapshot
            {
                SessionStableId = source.SessionStableId,
                ClientRequestId = source.ClientRequestId,
                ScenarioStableId = source.ScenarioStableId,
                ScenarioDataRevision = source.ScenarioDataRevision,
                ScenarioSeed = source.ScenarioSeed,
                RuleRevision = source.RuleRevision,
                CurrentTick = source.CurrentTick,
                DurationTicks = source.DurationTicks,
                Revision = source.Revision,
                IsCompleted = source.IsCompleted,
                ModeCode = source.ModeCode,
                IsOperationalState = source.IsOperationalState,
            };

        private static void ValidateCreate(경영SimulationSession생성Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ClientRequestId == Guid.Empty)
                throw new SimulationContractException("SimulationClientRequestIdMissing");
            RequireStableId(request.ScenarioStableId, "SimulationScenarioStableIdInvalid");
            RequireText(request.ScenarioDataRevision, "SimulationScenarioDataRevisionMissing");
            RequireText(request.RuleRevision, "SimulationRuleRevisionMissing");
            if (request.DurationTicks <= 0 || request.DurationTicks > 365)
                throw new SimulationContractException("SimulationDurationTicksInvalid");
        }

        private static void ValidateAdvance(경영SimulationTick진행Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.TickCount <= 0 || request.TickCount > 28)
                throw new SimulationContractException("SimulationTickCountInvalid");
        }

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                throw new SimulationContractException(errorCode);
            }
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(errorCode);
        }

        private sealed class 적용된TickCommand
        {
            public 적용된TickCommand(int tickCount, 경영SimulationSessionSnapshot snapshot)
            {
                TickCount = tickCount;
                Snapshot = snapshot;
            }

            public int TickCount { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }

    public interface I경영SimulationSessionStore
    {
        경영SimulationSessionAggregate CreateOrGet(경영SimulationSession생성Request request);
        경영SimulationSessionAggregate? Find(string sessionStableId);
    }

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
    }

    public sealed class 경영SimulationSessionService
    {
        private readonly I경영SimulationSessionStore store;

        public 경영SimulationSessionService(I경영SimulationSessionStore store)
            => this.store = store ?? throw new ArgumentNullException(nameof(store));

        public 경영SimulationSessionSnapshot Create(경영SimulationSession생성Request request)
            => store.CreateOrGet(request).Snapshot();

        public 경영SimulationSessionSnapshot Get(string sessionStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Snapshot();

        public 경영SimulationSessionSnapshot Advance(
            string sessionStableId,
            경영SimulationTick진행Request request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Advance(request);
    }
}
