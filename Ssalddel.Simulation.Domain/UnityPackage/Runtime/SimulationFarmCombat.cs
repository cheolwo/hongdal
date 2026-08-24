using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationFarmCombatInput,
        SsalddelCodeLayer.Domain,
        "전투 박자·타이밍 등급·피해·전술 기회를 결정적으로 판정한다.",
        StepKey = "domain.farm-combat",
        DependsOnStepKeys = new[] { "application.farm-combat" },
        FlowOrder = 40,
        ExecutionStage = SsalddelCodeExecutionStage.Tick,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        Effects = SsalddelCodeEffect.StateMutation,
        Boundary = "Unity가 제출한 판정 결과를 신뢰하지 않고 Session aggregate가 최종 결과를 확정한다.")]
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, string> combatPerspectives =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<SimulationCombatBeatSnapshot> combatBeats =
            new List<SimulationCombatBeatSnapshot>();
        private readonly List<SimulationCombatReactionSnapshot> combatReactions =
            new List<SimulationCombatReactionSnapshot>();
        private readonly Dictionary<string, AppliedFarmCommand>
            appliedCombatPerspectiveCommands =
                new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedFarmCommand> appliedCombatBeatCommands =
            new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedFarmCommand>
            appliedCombatReactionCommands =
                new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);

        public SimulationFarmSurvivalStateSnapshot ConfirmCombatPerspective(
            SimulationCombatPerspectiveConfirmRequest request)
        {
            ValidateCombatPerspectiveRequest(request);
            lock (gate)
            {
                EnsureInteractiveCombatConfigured();
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildCombatPerspectivePayloadKey(request);
                if (appliedCombatPerspectiveCommands.TryGetValue(commandId, out var applied))
                    return ResolveAppliedCombatCommand(applied, payloadKey);
                EnsureNewCombatCommand(commandId, request.ExpectedRevision);

                var actor = FindCombatPlayer(request.ActorStableId);
                if (combatBeats.Any(value => value.StateCode ==
                    SimulationFarmCombatCodes.Active))
                    throw new SimulationConflictException(
                        "SimulationCombatPerspectiveLocked");

                combatPerspectives[actor.ActorStableId] = request.PerspectiveCode.Trim();
                Revision++;
                AppendCombatPerspectiveConfirmCommand(request);
                return RememberCombatCommand(appliedCombatPerspectiveCommands,
                    commandId, payloadKey);
            }
        }

        public SimulationFarmSurvivalStateSnapshot StartCombatBeat(
            SimulationCombatBeatStartRequest request)
        {
            ValidateCombatBeatStartRequest(request);
            lock (gate)
            {
                EnsureInteractiveCombatConfigured();
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildCombatBeatPayloadKey(request);
                if (appliedCombatBeatCommands.TryGetValue(commandId, out var applied))
                    return ResolveAppliedCombatCommand(applied, payloadKey);
                EnsureNewCombatCommand(commandId, request.ExpectedRevision);

                var actor = FindCombatPlayer(request.ActorStableId);
                if (actor.Injured || actor.Health <= 0m)
                    throw new SimulationConflictException(
                        "SimulationCombatActorUnavailable");
                if (!combatPerspectives.TryGetValue(actor.ActorStableId,
                    out var perspective))
                    throw new SimulationConflictException(
                        "SimulationCombatPerspectiveRequired");
                if (combatBeats.Any(value => value.StateCode ==
                    SimulationFarmCombatCodes.Active))
                    throw new SimulationConflictException(
                        "SimulationCombatBeatAlreadyActive");
                if (HasOpenTacticalOrderWindow())
                    throw new SimulationConflictException(
                        "SimulationTacticalOrderWindowActive");

                var encounter = FindZombieCombatEncounter(request.EncounterStableId);
                if (encounter.StateCode != SimulationFarmSurvivalCodes.AwaitingCombat)
                    throw new SimulationConflictException(
                        "SimulationCombatEncounterNotReady");

                var sequence = combatBeats.Count(value => string.Equals(
                    value.EncounterStableId, encounter.EncounterStableId,
                    StringComparison.Ordinal)) + 1;
                var pattern = Math.Abs((long)ScenarioSeed + CurrentTick + sequence) % 2L == 0L
                    ? SimulationFarmCombatCodes.ZombieSwipe
                    : SimulationFarmCombatCodes.ZombieLunge;
                combatBeats.Add(new SimulationCombatBeatSnapshot
                {
                    BeatStableId = "combat-beat:" + encounter.EncounterStableId
                        + ":" + sequence.ToString(CultureInfo.InvariantCulture),
                    EncounterStableId = encounter.EncounterStableId,
                    ActorStableId = actor.ActorStableId,
                    AppliedPerspectiveCode = perspective,
                    AttackPatternCode = pattern,
                    Sequence = sequence,
                    StartedWorldTick = CurrentTick,
                    ImpactOffsetMs = SimulationFarmCombatCodes.ImpactOffsetMs,
                    GuardWindowMs = perspective ==
                        SimulationFarmCombatCodes.FirstPersonPrecision
                            ? SimulationFarmCombatCodes.FirstPersonGuardWindowMs
                            : SimulationFarmCombatCodes.ThirdPersonGuardWindowMs,
                    CounterWindowMs = perspective ==
                        SimulationFarmCombatCodes.FirstPersonPrecision
                            ? SimulationFarmCombatCodes.FirstPersonCounterWindowMs
                            : SimulationFarmCombatCodes.ThirdPersonCounterWindowMs,
                    PerfectGuardWindowMs =
                        SimulationFarmCombatCodes.PerfectGuardWindowMs,
                    PerfectCounterWindowMs =
                        SimulationFarmCombatCodes.PerfectCounterWindowMs,
                    StateCode = SimulationFarmCombatCodes.Active,
                    PresentationKey = "survival.combat.telegraph."
                        + pattern.ToLowerInvariant(),
                });

                Revision++;
                AppendCombatBeatStartCommand(request);
                return RememberCombatCommand(appliedCombatBeatCommands,
                    commandId, payloadKey);
            }
        }

        public SimulationFarmSurvivalStateSnapshot ConfirmCombatReaction(
            SimulationCombatReactionConfirmRequest request)
        {
            ValidateCombatReactionRequest(request);
            lock (gate)
            {
                EnsureInteractiveCombatConfigured();
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildCombatReactionPayloadKey(request);
                if (appliedCombatReactionCommands.TryGetValue(commandId, out var applied))
                    return ResolveAppliedCombatCommand(applied, payloadKey);
                EnsureNewCombatCommand(commandId, request.ExpectedRevision);

                var actor = FindCombatPlayer(request.ActorStableId);
                var beat = combatBeats.SingleOrDefault(value => string.Equals(
                    value.BeatStableId, request.BeatStableId.Trim(),
                    StringComparison.Ordinal))
                    ?? throw new SimulationNotFoundException(
                        "SimulationCombatBeatNotFound");
                if (beat.StateCode != SimulationFarmCombatCodes.Active)
                    throw new SimulationConflictException(
                        "SimulationCombatBeatAlreadyResolved");
                if (!string.Equals(beat.ActorStableId, actor.ActorStableId,
                    StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "SimulationCombatActorMismatch");

                var reaction = ResolveCombatReaction(beat, commandId,
                    request.ReactionActionCode.Trim(), request.ReactionOffsetMs);
                ApplyCombatReaction(beat, reaction);
                Revision++;
                AppendCombatReactionConfirmCommand(request);
                return RememberCombatCommand(appliedCombatReactionCommands,
                    commandId, payloadKey);
            }
        }

        private SimulationFarmSurvivalStateSnapshot ResolveAppliedCombatCommand(
            AppliedFarmCommand applied,
            string payloadKey)
        {
            if (!string.Equals(applied.PayloadKey, payloadKey,
                StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationCommandPayloadConflict");
            return CloneFarmSurvivalState(applied.State);
        }

        private void EnsureNewCombatCommand(string commandId, long expectedRevision)
        {
            if (HasDifferentKindCommand(commandId))
                throw new SimulationConflictException("SimulationCommandKindConflict");
            if (expectedRevision != Revision)
                throw new SimulationConflictException(
                    "SimulationExpectedRevisionMismatch");
        }

        private SimulationFarmSurvivalStateSnapshot RememberCombatCommand(
            IDictionary<string, AppliedFarmCommand> commands,
            string commandId,
            string payloadKey)
        {
            var state = CreateFarmSurvivalStateSnapshot();
            commands.Add(commandId,
                new AppliedFarmCommand(payloadKey, CloneFarmSurvivalState(state)));
            return state;
        }

        private FarmActorState FindCombatPlayer(string actorStableId)
        {
            if (!farmActors.TryGetValue(actorStableId.Trim(), out var actor))
                throw new SimulationNotFoundException("SimulationFarmActorNotFound");
            if (actor.ActorKindCode != SimulationFarmSurvivalCodes.Player)
                throw new SimulationConflictException(
                    "SimulationCombatPlayerActorRequired");
            return actor;
        }

        private SimulationThreatEncounterSnapshot FindZombieCombatEncounter(
            string encounterStableId)
        {
            var encounter = threatEncounters.SingleOrDefault(value => string.Equals(
                value.EncounterStableId, encounterStableId.Trim(),
                StringComparison.Ordinal))
                ?? throw new SimulationNotFoundException(
                    "SimulationThreatEncounterNotFound");
            if (encounter.ThreatTypeCode != SimulationFarmSurvivalCodes.ZombiePressure)
                throw new SimulationConflictException(
                    "SimulationCombatEncounterUnsupported");
            return encounter;
        }

        private static SimulationCombatReactionSnapshot ResolveCombatReaction(
            SimulationCombatBeatSnapshot beat,
            string commandId,
            string actionCode,
            int reactionOffsetMs)
        {
            var timingDelta = reactionOffsetMs - beat.ImpactOffsetMs;
            var absoluteDelta = Math.Abs(timingDelta);
            var perfectWindow = actionCode == SimulationFarmCombatCodes.Guard
                ? beat.PerfectGuardWindowMs : beat.PerfectCounterWindowMs;
            var allowedWindow = actionCode == SimulationFarmCombatCodes.Guard
                ? beat.GuardWindowMs : beat.CounterWindowMs;
            var grade = absoluteDelta <= perfectWindow
                ? SimulationFarmCombatCodes.Perfect
                : absoluteDelta <= allowedWindow
                    ? SimulationFarmCombatCodes.OnTime
                    : timingDelta < 0
                        ? SimulationFarmCombatCodes.Early
                        : SimulationFarmCombatCodes.Late;

            var perfect = grade == SimulationFarmCombatCodes.Perfect;
            var onTime = perfect || grade == SimulationFarmCombatCodes.OnTime;
            var counter = actionCode == SimulationFarmCombatCodes.Counter;
            return new SimulationCombatReactionSnapshot
            {
                ReactionStableId = "combat-reaction:" + beat.BeatStableId,
                CommandId = commandId,
                BeatStableId = beat.BeatStableId,
                ActorStableId = beat.ActorStableId,
                ReactionActionCode = actionCode,
                ReactionOffsetMs = reactionOffsetMs,
                TimingDeltaMs = timingDelta,
                GradeCode = grade,
                ActorDamageUnits = !onTime ? 10m
                    : counter || perfect ? 0m : 3m,
                DefenseResponseScore = !onTime ? 0 : counter && perfect ? 2 : 1,
                ThreatStaggered = counter && onTime,
                PresentationKey = "survival.combat.reaction."
                    + actionCode.ToLowerInvariant() + "." + grade.ToLowerInvariant(),
            };
        }

        private void ApplyCombatReaction(
            SimulationCombatBeatSnapshot beat,
            SimulationCombatReactionSnapshot reaction)
        {
            combatReactions.Add(reaction);
            beat.StateCode = SimulationFarmCombatCodes.Resolved;
            beat.ReactionStableId = reaction.ReactionStableId;
            if (reaction.ActorDamageUnits > 0m)
            {
                var actor = farmActors[beat.ActorStableId];
                actor.Health = Math.Max(1m, actor.Health - reaction.ActorDamageUnits);
                actor.Injured = true;
            }

            var encounter = FindZombieCombatEncounter(beat.EncounterStableId);
            if (UsesHeroTacticalCombatRule())
                OpenTacticalOrderWindow(beat, reaction, encounter);
            else
                ResolveZombieCombatEncounter(encounter, reaction.DefenseResponseScore,
                    reaction.ActorDamageUnits > 0m ? beat.ActorStableId : string.Empty);
        }

        private void PrepareZombieCombat()
        {
            var encounter = threatEncounters.SingleOrDefault(value =>
                value.ThreatTypeCode == SimulationFarmSurvivalCodes.ZombiePressure);
            if (encounter == null || encounter.StateCode == SimulationFarmSurvivalCodes.Resolved)
                return;
            PrepareZombieCombat(encounter);
        }

        private void PrepareZombieCombat(
            SimulationThreatEncounterSnapshot encounter)
        {
            if (encounter.StateCode == SimulationFarmSurvivalCodes.Resolved)
                return;
            encounter.StateCode = SimulationFarmSurvivalCodes.AwaitingCombat;
            encounter.PresentationKey = "survival.combat.ready";
            if (UsesHeroTacticalCombatRule()) PrepareTacticalFront(encounter);
            UpdateFarmThreatWorldEvent(encounter);
        }

        private void ExpireActiveFarmCombat()
        {
            var beat = combatBeats.SingleOrDefault(value =>
                value.StateCode == SimulationFarmCombatCodes.Active);
            if (beat == null) return;
            var reaction = new SimulationCombatReactionSnapshot
            {
                ReactionStableId = "combat-reaction:" + beat.BeatStableId,
                CommandId = "world-tick-expiration",
                BeatStableId = beat.BeatStableId,
                ActorStableId = beat.ActorStableId,
                ReactionActionCode = SimulationFarmCombatCodes.NoResponse,
                ReactionOffsetMs = SimulationFarmCombatCodes.MaximumReactionOffsetMs,
                TimingDeltaMs = SimulationFarmCombatCodes.MaximumReactionOffsetMs
                    - beat.ImpactOffsetMs,
                GradeCode = SimulationFarmCombatCodes.Expired,
                ActorDamageUnits = 10m,
                DefenseResponseScore = 0,
                ThreatStaggered = false,
                PresentationKey = "survival.combat.reaction.expired",
            };
            ApplyCombatReaction(beat, reaction);
        }

        private void ResolveZombieCombatEncounter(
            SimulationThreatEncounterSnapshot encounter,
            int responseScore,
            string injuredActorStableId)
        {
            if (DefensePreparednessScore() + responseScore >= 2)
            {
                encounter.OutcomeCode = SimulationFarmSurvivalCodes.DefenseSucceeded;
            }
            else
            {
                encounter.SupplyLossUnits = Math.Min(2m, farmSupplyUnits);
                farmSupplyUnits -= encounter.SupplyLossUnits;
                encounter.DamageUnits = 20m;
                recoverableDamageUnits += encounter.DamageUnits;
                var fence = farmDefenses.Values.FirstOrDefault(value =>
                    value.DefenseKindCode == SimulationFarmSurvivalCodes.Fence);
                if (fence != null)
                    fence.Durability = Math.Max(0m, fence.Durability
                        - encounter.DamageUnits);
                encounter.OutcomeCode = encounter.SupplyLossUnits > 0m
                    ? SimulationFarmSurvivalCodes.InventoryTaken
                    : SimulationFarmSurvivalCodes.FacilityDamaged;
            }
            encounter.InjuredActorStableId = injuredActorStableId;
            encounter.StateCode = SimulationFarmSurvivalCodes.Resolved;
            encounter.PresentationKey =
                SimulationFarmSurvivalCodes.DamageAssessmentPresentation;
            UpdateFarmThreatWorldEvent(encounter);
        }

        private SimulationFarmCombatStateSnapshot CreateFarmCombatStateSnapshot()
            => new SimulationFarmCombatStateSnapshot
            {
                Perspectives = combatPerspectives.OrderBy(value => value.Key,
                    StringComparer.Ordinal).Select(value =>
                        new SimulationCombatPerspectiveSnapshot
                        {
                            ActorStableId = value.Key,
                            PerspectiveCode = value.Value,
                            PresentationKey = value.Value ==
                                SimulationFarmCombatCodes.FirstPersonPrecision
                                    ? "survival.combat.view.first-person"
                                    : "survival.combat.view.third-person",
                        }).ToArray(),
                Beats = combatBeats.Select(CloneCombatBeat).ToArray(),
                Reactions = combatReactions.Select(CloneCombatReaction).ToArray(),
                Tactical = CreateFarmTacticalCombatStateSnapshot(),
                SimulationOnly = true,
                IsOperationalState = false,
            };

        internal static SimulationFarmCombatStateSnapshot CloneFarmCombatState(
            SimulationFarmCombatStateSnapshot source)
            => new SimulationFarmCombatStateSnapshot
            {
                RuleRevision = source.RuleRevision,
                Perspectives = source.Perspectives.Select(value =>
                    new SimulationCombatPerspectiveSnapshot
                    {
                        ActorStableId = value.ActorStableId,
                        PerspectiveCode = value.PerspectiveCode,
                        PresentationKey = value.PresentationKey,
                    }).ToArray(),
                Beats = source.Beats.Select(CloneCombatBeat).ToArray(),
                Reactions = source.Reactions.Select(CloneCombatReaction).ToArray(),
                Tactical = CloneFarmTacticalCombatState(source.Tactical),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private static SimulationCombatBeatSnapshot CloneCombatBeat(
            SimulationCombatBeatSnapshot value)
            => new SimulationCombatBeatSnapshot
            {
                BeatStableId = value.BeatStableId,
                EncounterStableId = value.EncounterStableId,
                ActorStableId = value.ActorStableId,
                AppliedPerspectiveCode = value.AppliedPerspectiveCode,
                AttackPatternCode = value.AttackPatternCode,
                Sequence = value.Sequence,
                StartedWorldTick = value.StartedWorldTick,
                ImpactOffsetMs = value.ImpactOffsetMs,
                GuardWindowMs = value.GuardWindowMs,
                CounterWindowMs = value.CounterWindowMs,
                PerfectGuardWindowMs = value.PerfectGuardWindowMs,
                PerfectCounterWindowMs = value.PerfectCounterWindowMs,
                StateCode = value.StateCode,
                ReactionStableId = value.ReactionStableId,
                PresentationKey = value.PresentationKey,
            };

        private static SimulationCombatReactionSnapshot CloneCombatReaction(
            SimulationCombatReactionSnapshot value)
            => new SimulationCombatReactionSnapshot
            {
                ReactionStableId = value.ReactionStableId,
                CommandId = value.CommandId,
                BeatStableId = value.BeatStableId,
                ActorStableId = value.ActorStableId,
                ReactionActionCode = value.ReactionActionCode,
                ReactionOffsetMs = value.ReactionOffsetMs,
                TimingDeltaMs = value.TimingDeltaMs,
                GradeCode = value.GradeCode,
                ActorDamageUnits = value.ActorDamageUnits,
                DefenseResponseScore = value.DefenseResponseScore,
                ThreatStaggered = value.ThreatStaggered,
                PresentationKey = value.PresentationKey,
            };

        private bool UsesInteractiveCombatRule()
            => farmSurvivalCreationState?.RuleRevision ==
                SimulationFarmSurvivalCodes.InteractiveCombatRuleRevision
                || UsesHeroTacticalCombatRule()
                || UsesScenicSeasonRule();

        private bool UsesScenicSeasonRule()
            => farmSurvivalCreationState?.RuleRevision ==
                SimulationFarmSurvivalCodes.ScenicSeasonRuleRevision;

        private bool UsesHeroTacticalCombatRule()
            => farmSurvivalCreationState?.RuleRevision ==
                SimulationFarmSurvivalCodes.HeroTacticalCombatRuleRevision;

        private void EnsureInteractiveCombatConfigured()
        {
            EnsureFarmSurvivalConfigured();
            if (!UsesInteractiveCombatRule())
                throw new SimulationConflictException(
                    "SimulationInteractiveCombatNotEnabled");
        }

        private bool HasAppliedFarmCombatCommand(string commandId)
            => appliedCombatPerspectiveCommands.ContainsKey(commandId)
                || appliedCombatBeatCommands.ContainsKey(commandId)
                || appliedCombatReactionCommands.ContainsKey(commandId)
                || appliedTacticalOrderCommands.ContainsKey(commandId);

        internal static void ValidateCombatPerspectiveRequest(
            SimulationCombatPerspectiveConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateCombatCommandFields(request.CommandId, request.ExpectedRevision,
                request.ActorStableId);
            if (request.PerspectiveCode != SimulationFarmCombatCodes.FirstPersonPrecision
                && request.PerspectiveCode !=
                    SimulationFarmCombatCodes.ThirdPersonAwareness)
                throw new SimulationContractException(
                    "SimulationCombatPerspectiveInvalid");
        }

        internal static void ValidateCombatBeatStartRequest(
            SimulationCombatBeatStartRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateCombatCommandFields(request.CommandId, request.ExpectedRevision,
                request.ActorStableId);
            RequireStableId(request.EncounterStableId,
                "SimulationThreatEncounterInvalid");
        }

        internal static void ValidateCombatReactionRequest(
            SimulationCombatReactionConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateCombatCommandFields(request.CommandId, request.ExpectedRevision,
                request.ActorStableId);
            RequireStableId(request.BeatStableId, "SimulationCombatBeatInvalid");
            if (request.ReactionActionCode != SimulationFarmCombatCodes.Guard
                && request.ReactionActionCode != SimulationFarmCombatCodes.Counter)
                throw new SimulationContractException(
                    "SimulationCombatReactionInvalid");
            if (request.ReactionOffsetMs < 0 || request.ReactionOffsetMs >
                SimulationFarmCombatCodes.MaximumReactionOffsetMs)
                throw new SimulationContractException(
                    "SimulationCombatReactionOffsetInvalid");
        }

        private static void ValidateCombatCommandFields(
            string commandId,
            long expectedRevision,
            string actorStableId)
        {
            RequireStableId(commandId, "SimulationCommandIdInvalid");
            if (expectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
            RequireStableId(actorStableId, "SimulationFarmActorInvalid");
        }

        internal static string BuildCombatPerspectivePayloadKey(
            SimulationCombatPerspectiveConfirmRequest request)
            => string.Join("|", request.ActorStableId.Trim(),
                request.PerspectiveCode.Trim());

        internal static string BuildCombatBeatPayloadKey(
            SimulationCombatBeatStartRequest request)
            => string.Join("|", request.EncounterStableId.Trim(),
                request.ActorStableId.Trim());

        internal static string BuildCombatReactionPayloadKey(
            SimulationCombatReactionConfirmRequest request)
            => string.Join("|", request.BeatStableId.Trim(),
                request.ActorStableId.Trim(), request.ReactionActionCode.Trim(),
                request.ReactionOffsetMs.ToString(CultureInfo.InvariantCulture));
    }
}
