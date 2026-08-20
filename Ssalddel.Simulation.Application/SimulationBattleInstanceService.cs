using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public interface ISimulationBattleWorldReconciler
    {
        void Reconcile(string sessionStableId, 경영SimulationSessionSnapshot world);
        SimulationBattleSaveRecordSnapshot[] Capture(string sessionStableId);
        void Restore(string sessionStableId,
            SimulationBattleSaveRecordSnapshot[] records);
    }

    public interface ISimulationBattleInstanceStore
    {
        SimulationBattleInstanceState? Find(string battleStableId);
        SimulationBattleInstanceState[] FindBySession(string sessionStableId);
        bool CanReserve(string sessionStableId, string areaStableId,
            IEnumerable<string> resourceStableIds);
        SimulationBattleInstanceState CreateOrGet(SimulationBattleCreationContext context);
        SimulationBattleSaveRecordSnapshot[] CaptureSession(string sessionStableId);
        void RestoreSession(string sessionStableId,
            SimulationBattleSaveRecordSnapshot[] records);
    }

    public interface ISimulationBattleResourceLockReader
    {
        bool IsLocked(string sessionStableId, string resourceStableId);
    }

    public interface ISimulationBattleReservationReader
    {
        bool IsActorCommitted(string sessionStableId, string actorStableId);
        bool HasWorldTargetConflict(string sessionStableId,
            string worldEffectTargetStableId, string capabilityCode);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationParallelBattle,
        SsalddelCodeLayer.Application,
        "전투 Preview·Confirm·진행과 경영 World 합류를 조율한다.",
        StepKey = "application.battle",
        DependsOnStepKeys = new string[] { "api.battle" },
        ExecutionStage = SsalddelCodeExecutionStage.Confirm,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "전투 Tick과 경영 WorldTick을 분리하고 완료 결과만 안전한 WorldTick에 합류시킨다.")]
    public sealed class SimulationBattleInstanceService : ISimulationBattleWorldReconciler
    {
        private readonly I경영SimulationSessionStore sessionStore;
        private readonly ISimulationTeamObservationPolicyStore teamPolicyStore;
        private readonly ISimulationBattleInstanceStore battleStore;
        private readonly ISimulationBattlefieldDerivationService? battlefieldDerivationService;

        public SimulationBattleInstanceService(I경영SimulationSessionStore sessions,
            ISimulationTeamObservationPolicyStore policies,
            ISimulationBattleInstanceStore battles,
            ISimulationBattlefieldDerivationService? battlefieldDerivation = null)
        {
            sessionStore = sessions ?? throw new ArgumentNullException(nameof(sessions));
            teamPolicyStore = policies ?? throw new ArgumentNullException(nameof(policies));
            battleStore = battles ?? throw new ArgumentNullException(nameof(battles));
            battlefieldDerivationService = battlefieldDerivation;
        }

        public SimulationBattleCreatePreviewSnapshot PreviewCreate(string sessionStableId,
            SimulationBattleCreatePreviewRequest request)
        {
            ValidateCreatePreview(request);
            var session = FindSession(sessionStableId);
            var world = session.Snapshot();
            var policy = FindPolicy(sessionStableId, request.RequestingActorStableId);
            var farm = world.FarmSurvival;
            var natureEncounter = world.NatureThreat.Encounters.FirstOrDefault(value =>
                value.EncounterStableId == request.EncounterStableId.Trim());
            var blocks = new List<string>();
            if (battlefieldDerivationService == null
                && request.ExpectedWorldRevision != world.Revision)
                blocks.Add("SimulationExpectedRevisionMismatch");
            if ((farm == null || farm.IsOperationalState || !farm.SimulationOnly)
                && natureEncounter == null)
                blocks.Add("SimulationBattleFarmStateUnavailable");
            var encounter = farm?.Encounters.FirstOrDefault(value =>
                value.EncounterStableId == request.EncounterStableId.Trim());
            if (encounter == null && natureEncounter == null)
                blocks.Add("SimulationBattleEncounterNotFound");
            else if (encounter?.StateCode == SimulationFarmSurvivalCodes.Resolved
                || natureEncounter?.StateCode == SimulationRegionalIncidentCodes.Resolved)
                blocks.Add("SimulationBattleEncounterAlreadyResolved");

            var actorIds = farm?.Actors.Where(value =>
                    value.ActorKindCode == SimulationFarmSurvivalCodes.Npc)
                .Select(value => value.ActorStableId)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray()
                ?? world.NpcActors.Select(value => value.ActorStableId)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var initialActorCount = actorIds.Length == 0 ? 0 : Math.Max(1, actorIds.Length / 2);
            var initialActors = actorIds.Take(initialActorCount).ToArray();
            var reinforcementActors = actorIds.Skip(initialActorCount).ToArray();
            var initialResources = initialActors.Concat(new[]
                {
                    farm?.FarmBuildingStableId ?? string.Empty,
                    "battle-squad:initial:" + request.EncounterStableId.Trim(),
                }).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
            if (actorIds.Length == 0) blocks.Add("SimulationBattleAlliedForceUnavailable");
            var hostileStrength = encounter?.ThreatUnitCount
                ?? natureEncounter?.ThreatUnitCount ?? 0;
            if (hostileStrength <= 0)
                blocks.Add("SimulationBattleHostileForceUnavailable");
            var areaStableId = encounter != null ? farm?.AreaStableId ?? string.Empty
                : natureEncounter == null ? string.Empty
                    : "nature-route:" + natureEncounter.NatureRouteCode;
            if (!battleStore.CanReserve(sessionStableId.Trim(), areaStableId,
                initialResources)) blocks.Add("BattleResourceLocked");

            var derivation = new SimulationBattlefieldDerivationSnapshot();
            var roster = new SimulationBattleUnitRosterSnapshot();
            if (battlefieldDerivationService != null && hostileStrength > 0
                && !string.IsNullOrWhiteSpace(areaStableId))
            {
                derivation = battlefieldDerivationService.Derive(sessionStableId.Trim(),
                    request.EncounterStableId.Trim(), areaStableId, world.Revision,
                    natureEncounter != null);
                if (!derivation.CanConfirm)
                    blocks.AddRange(derivation.BlockingReasonCodes);
                roster = SimulationBattleUnitRosterBuilder.Build(
                    request.EncounterStableId.Trim(), farm?.Actors
                        ?? Array.Empty<SimulationFarmActorSnapshot>(), hostileStrength,
                    encounter?.ThreatTypeCode ?? "NatureThreat", world.TeamRoleCards);
                if (derivation.CanConfirm)
                    SimulationBattleUnitRosterBuilder.BindBattlefieldPlan(roster,
                        derivation.BattlefieldPlan.BattlefieldPlanHashSha256);
                if (!roster.Units.Any(value => value.SideCode ==
                    SimulationFarmTacticalCombatCodes.Allied))
                    blocks.Add("SimulationBattleAlliedUnitRosterUnavailable");
            }

            return new SimulationBattleCreatePreviewSnapshot
            {
                SessionStableId = world.SessionStableId,
                EncounterStableId = request.EncounterStableId.Trim(),
                AreaStableId = areaStableId,
                WorldRevision = world.Revision,
                WorldTick = world.CurrentTick,
                AlliedStrength = 1 + initialActors.Length
                    + (farm?.Defenses.Count(value => value.Prepared) ?? 0) * 2,
                HostileStrength = hostileStrength,
                InitialResourceStableIds = initialResources,
                ReinforcementCandidateStableIds = reinforcementActors,
                BattlefieldDerivation = derivation,
                UnitRoster = roster,
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.ToArray(),
                SimulationOnly = policy.SimulationOnly,
            };
        }

        public SimulationBattleInstanceSnapshot ConfirmCreate(string sessionStableId,
            SimulationBattleCreateConfirmRequest request)
        {
            ValidateCommand(request.CommandId);
            var usesSpatialHashContract = battlefieldDerivationService != null
                && (!string.IsNullOrWhiteSpace(
                        request.ExpectedBattleWorldContextHashSha256)
                    || !string.IsNullOrWhiteSpace(
                        request.ExpectedBattlefieldDerivationInputHashSha256));
            if (usesSpatialHashContract)
            {
                Require(request.ExpectedBattleWorldContextHashSha256,
                    "SimulationBattleWorldContextHashInvalid");
                Require(request.ExpectedBattlefieldDerivationInputHashSha256,
                    "SimulationBattleDerivationInputHashInvalid");
            }
            var existing = battleStore.Find("battle:" + request.CommandId.Trim());
            if (existing != null)
            {
                var snapshot = existing.Snapshot();
                if (snapshot.SessionStableId != sessionStableId.Trim()
                    || snapshot.EncounterStableId != request.EncounterStableId.Trim()
                    || (usesSpatialHashContract
                        && (snapshot.BattlefieldDerivation.WorldContext.ContextHashSha256 !=
                                request.ExpectedBattleWorldContextHashSha256.Trim()
                            || snapshot.BattlefieldDerivation
                                .BattlefieldDerivationInputHashSha256 != request
                                .ExpectedBattlefieldDerivationInputHashSha256.Trim()))
                    || !snapshot.Participants.Any(value => value.ActorStableId ==
                        request.RequestingActorStableId.Trim()
                        && value.ParticipationRoleCode == SimulationBattleInstanceCodes.Commander))
                    throw new SimulationConflictException("SimulationBattleCreatePayloadConflict");
                return snapshot;
            }
            var preview = PreviewCreate(sessionStableId, new SimulationBattleCreatePreviewRequest
            {
                ExpectedWorldRevision = request.ExpectedWorldRevision,
                EncounterStableId = request.EncounterStableId,
                RequestingActorStableId = request.RequestingActorStableId,
            });
            if (!preview.CanConfirm)
                throw new SimulationConflictException(preview.BlockingReasonCodes[0]);
            if (battlefieldDerivationService != null && !usesSpatialHashContract
                && request.ExpectedWorldRevision != preview.WorldRevision)
                throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
            if (usesSpatialHashContract)
            {
                if (!string.Equals(request.ExpectedBattleWorldContextHashSha256.Trim(),
                    preview.BattlefieldDerivation.WorldContext.ContextHashSha256,
                    StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "SimulationBattleWorldContextChanged");
                if (!string.Equals(
                    request.ExpectedBattlefieldDerivationInputHashSha256.Trim(),
                    preview.BattlefieldDerivation.BattlefieldDerivationInputHashSha256,
                    StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "SimulationBattleDerivationInputChanged");
            }
            var session = FindSession(sessionStableId).Snapshot();
            return battleStore.CreateOrGet(new SimulationBattleCreationContext
            {
                BattleStableId = "battle:" + request.CommandId.Trim(),
                SessionStableId = sessionStableId.Trim(),
                EncounterStableId = request.EncounterStableId.Trim(),
                AreaStableId = preview.AreaStableId,
                CommanderActorStableId = request.RequestingActorStableId.Trim(),
                StartedWorldTick = session.CurrentTick,
                StartedWorldRevision = session.Revision,
                ScenarioSeed = session.ScenarioSeed,
                AlliedStrength = preview.AlliedStrength,
                HostileStrength = preview.HostileStrength,
                InitialResourceStableIds = preview.InitialResourceStableIds,
                ReinforcementCandidateStableIds = preview.ReinforcementCandidateStableIds,
                BattlefieldDerivation = preview.BattlefieldDerivation,
                UnitRoster = preview.UnitRoster,
                CreateCommandId = request.CommandId.Trim(),
            }).Snapshot();
        }

        public SimulationBattleInstanceSnapshot[] List(string sessionStableId,
            string actorStableId)
        {
            FindPolicy(sessionStableId, actorStableId);
            var world = FindSession(sessionStableId).Snapshot();
            Reconcile(sessionStableId, world);
            return battleStore.FindBySession(sessionStableId.Trim())
                .Select(value => value.Snapshot()).ToArray();
        }

        public SimulationBattleInstanceSnapshot Get(string sessionStableId,
            string battleStableId, string actorStableId)
        {
            FindPolicy(sessionStableId, actorStableId);
            var battle = FindBattle(sessionStableId, battleStableId);
            var world = FindSession(sessionStableId).Snapshot();
            battle.Reconcile(world.CurrentTick, world.Revision);
            return battle.Snapshot();
        }

        public SimulationBattleInstanceSnapshot ConfirmParticipation(string sessionStableId,
            string battleStableId, SimulationBattleParticipationConfirmRequest request)
        {
            var policy = FindPolicy(sessionStableId, request.ActorStableId);
            if (request.ExpectedTeamPolicyRevision != policy.Revision)
                throw new SimulationConflictException("SimulationTeamPolicyRevisionMismatch");
            return FindBattle(sessionStableId, battleStableId).ConfirmParticipation(request);
        }

        public SimulationBattleDeploymentPreviewSnapshot PreviewDeployment(string sessionStableId,
            string battleStableId, SimulationBattleDeploymentPreviewRequest request)
        {
            FindPolicy(sessionStableId, request.ActorStableId);
            return FindBattle(sessionStableId, battleStableId).PreviewDeployment(request);
        }

        public SimulationBattleInstanceSnapshot ConfirmDeployment(string sessionStableId,
            string battleStableId, SimulationBattleDeploymentConfirmRequest request)
        {
            FindPolicy(sessionStableId, request.ActorStableId);
            return FindBattle(sessionStableId, battleStableId).ConfirmDeployment(request);
        }

        public SimulationBattleSupportPreviewSnapshot PreviewSupport(string sessionStableId,
            string battleStableId, SimulationBattleSupportPreviewRequest request)
        {
            FindPolicy(sessionStableId, request.RequestingActorStableId);
            var session = FindSession(sessionStableId);
            var world = session.Snapshot();
            var battle = FindBattle(sessionStableId, battleStableId);
            return battle.PreviewSupport(request, world.Revision,
                IsSupportSourceAvailable(session, battle, request.SupportCode,
                    request.SourceResourceStableId));
        }

        public SimulationBattleInstanceSnapshot ConfirmSupport(string sessionStableId,
            string battleStableId, SimulationBattleSupportConfirmRequest request)
        {
            FindPolicy(sessionStableId, request.RequestingActorStableId);
            var session = FindSession(sessionStableId);
            var world = session.Snapshot();
            var battle = FindBattle(sessionStableId, battleStableId);
            return battle.ConfirmSupport(request, world.CurrentTick, world.Revision,
                IsSupportSourceAvailable(session, battle, request.SupportCode,
                    request.SourceResourceStableId));
        }

        public SimulationBattleInstanceSnapshot Advance(string sessionStableId,
            string battleStableId, SimulationBattleAdvanceRequest request)
        {
            var session = FindSession(sessionStableId).Snapshot();
            var battle = FindBattle(sessionStableId, battleStableId);
            return battle.Advance(request, session.CurrentTick,
                HeroContribution(session, battle.EncounterStableId));
        }

        public SimulationBattleInstanceSnapshot ConfirmTacticalCommand(
            string sessionStableId, string battleStableId,
            SimulationBattleTacticalCommandConfirmRequest request)
        {
            FindPolicy(sessionStableId, request.RequestingActorStableId);
            return FindBattle(sessionStableId, battleStableId)
                .ConfirmTacticalCommand(request);
        }

        public void Reconcile(string sessionStableId, 경영SimulationSessionSnapshot world)
        {
            var session = FindSession(sessionStableId);
            foreach (var battle in battleStore.FindBySession(sessionStableId.Trim()))
            {
                var pending = battle.Snapshot();
                if (pending.PhaseCode != SimulationBattleInstanceCodes.Completed
                    || pending.Outcome == null
                    || world.CurrentTick <= pending.Outcome.CompletedWorldTick)
                    continue;
                session.ApplyBattleSemanticEffects(pending.BattleStableId,
                    pending.EncounterStableId, pending.Outcome,
                    pending.SemanticEffects.Where(value =>
                        value.ReconciliationStateCode ==
                            SimulationBattlefieldDerivationCodes.Pending));
                if (pending.Outcome.ResultCode == SimulationBattleInstanceCodes.Victory
                    && world.NatureThreat.Encounters.Any(value =>
                        value.EncounterStableId == pending.EncounterStableId))
                    session.ApplyNatureEncounterVictory(pending.BattleStableId,
                        pending.EncounterStableId);
                var current = session.Snapshot();
                battle.Reconcile(current.CurrentTick, current.Revision);
            }
        }

        public SimulationBattleSaveRecordSnapshot[] Capture(string sessionStableId)
            => battleStore.CaptureSession(sessionStableId.Trim());

        public void Restore(string sessionStableId,
            SimulationBattleSaveRecordSnapshot[] records)
            => battleStore.RestoreSession(sessionStableId.Trim(), records);

        private bool IsSupportSourceAvailable(경영SimulationSessionAggregate session,
            SimulationBattleInstanceState battle, string supportCode, string resourceStableId)
        {
            var id = resourceStableId.Trim();
            if (supportCode == SimulationBattleInstanceCodes.ReinforcementSquad)
                return battle.ReinforcementCandidateStableIds.Contains(id, StringComparer.Ordinal)
                    && battleStore.CanReserve(battle.SessionStableId, "support-only:" + battle.BattleStableId,
                        new[] { id });
            return session.GetWorldInventory().ContainerItemStacks.Any(value =>
                value.ItemStackStableId == id && value.Quantity > 0m)
                && battleStore.CanReserve(battle.SessionStableId, "support-only:" + battle.BattleStableId,
                    new[] { id });
        }

        private static int HeroContribution(경영SimulationSessionSnapshot session,
            string encounterStableId)
        {
            if (session.FarmSurvival == null) return 0;
            return session.FarmSurvival!.Combat.Reactions
                .Where(value => session.FarmSurvival.Combat.Beats.Any(beat =>
                    beat.BeatStableId == value.BeatStableId
                    && beat.EncounterStableId == encounterStableId))
                .Sum(value => value.DefenseResponseScore);
        }

        private 경영SimulationSessionAggregate FindSession(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdInvalid");
            return sessionStore.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
        }

        private SimulationBattleInstanceState FindBattle(string sessionStableId,
            string battleStableId)
        {
            if (string.IsNullOrWhiteSpace(battleStableId))
                throw new SimulationContractException("SimulationBattleStableIdInvalid");
            var battle = battleStore.Find(battleStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationBattleNotFound");
            if (battle.SessionStableId != sessionStableId.Trim())
                throw new SimulationNotFoundException("SimulationBattleNotFound");
            return battle;
        }

        private SimulationTeamObservationPolicySnapshot FindPolicy(string sessionStableId,
            string actorStableId)
        {
            if (string.IsNullOrWhiteSpace(actorStableId))
                throw new SimulationContractException("SimulationBattleActorInvalid");
            var policy = teamPolicyStore.FindForObserver(sessionStableId.Trim(), actorStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationTeamObservationPolicyNotFound");
            if (!policy.SimulationOnly || policy.IsOperationalState
                || !policy.MemberActorStableIds.Contains(actorStableId.Trim(), StringComparer.Ordinal))
                throw new SimulationConflictException("SimulationBattleTeamPolicyMismatch");
            return policy;
        }

        private static void ValidateCreatePreview(SimulationBattleCreatePreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedWorldRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Require(request.EncounterStableId, "SimulationBattleEncounterInvalid");
            Require(request.RequestingActorStableId, "SimulationBattleActorInvalid");
        }
        private static void ValidateCommand(string value) => Require(value, "SimulationCommandIdInvalid");
        private static void Require(string value, string code)
        {
            if (string.IsNullOrWhiteSpace(value) || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new SimulationContractException(code);
        }
    }
}
