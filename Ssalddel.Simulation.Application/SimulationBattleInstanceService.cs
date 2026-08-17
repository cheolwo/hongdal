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

        public SimulationBattleInstanceService(I경영SimulationSessionStore sessions,
            ISimulationTeamObservationPolicyStore policies,
            ISimulationBattleInstanceStore battles)
        {
            sessionStore = sessions ?? throw new ArgumentNullException(nameof(sessions));
            teamPolicyStore = policies ?? throw new ArgumentNullException(nameof(policies));
            battleStore = battles ?? throw new ArgumentNullException(nameof(battles));
        }

        public SimulationBattleCreatePreviewSnapshot PreviewCreate(string sessionStableId,
            SimulationBattleCreatePreviewRequest request)
        {
            ValidateCreatePreview(request);
            var session = FindSession(sessionStableId);
            var world = session.Snapshot();
            var policy = FindPolicy(sessionStableId, request.RequestingActorStableId);
            var farm = world.FarmSurvival;
            var blocks = new List<string>();
            if (request.ExpectedWorldRevision != world.Revision)
                blocks.Add("SimulationExpectedRevisionMismatch");
            if (farm == null || farm.IsOperationalState || !farm.SimulationOnly)
                blocks.Add("SimulationBattleFarmStateUnavailable");
            var encounter = farm?.Encounters.FirstOrDefault(value =>
                value.EncounterStableId == request.EncounterStableId.Trim());
            if (encounter == null)
                blocks.Add("SimulationBattleEncounterNotFound");
            else if (encounter.StateCode == SimulationFarmSurvivalCodes.Resolved)
                blocks.Add("SimulationBattleEncounterAlreadyResolved");

            var actors = farm?.Actors.Where(value =>
                    value.ActorKindCode == SimulationFarmSurvivalCodes.Npc)
                .OrderBy(value => value.ActorStableId, StringComparer.Ordinal).ToArray()
                ?? Array.Empty<SimulationFarmActorSnapshot>();
            var initialActorCount = actors.Length == 0 ? 0 : Math.Max(1, actors.Length / 2);
            var initialActors = actors.Take(initialActorCount).Select(value => value.ActorStableId).ToArray();
            var reinforcementActors = actors.Skip(initialActorCount).Select(value => value.ActorStableId).ToArray();
            var initialResources = farm == null ? Array.Empty<string>() : initialActors
                .Concat(new[]
                {
                    farm.FarmBuildingStableId,
                    "battle-squad:initial:" + request.EncounterStableId.Trim(),
                }).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
            if (actors.Length == 0) blocks.Add("SimulationBattleAlliedForceUnavailable");
            if (encounter != null && encounter.ThreatUnitCount <= 0)
                blocks.Add("SimulationBattleHostileForceUnavailable");
            if (farm != null && !battleStore.CanReserve(sessionStableId.Trim(), farm.AreaStableId,
                initialResources)) blocks.Add("BattleResourceLocked");

            return new SimulationBattleCreatePreviewSnapshot
            {
                SessionStableId = world.SessionStableId,
                EncounterStableId = request.EncounterStableId.Trim(),
                AreaStableId = farm?.AreaStableId ?? string.Empty,
                WorldRevision = world.Revision,
                WorldTick = world.CurrentTick,
                AlliedStrength = 1 + initialActors.Length
                    + (farm?.Defenses.Count(value => value.Prepared) ?? 0) * 2,
                HostileStrength = encounter?.ThreatUnitCount ?? 0,
                InitialResourceStableIds = initialResources,
                ReinforcementCandidateStableIds = reinforcementActors,
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.ToArray(),
                SimulationOnly = policy.SimulationOnly,
            };
        }

        public SimulationBattleInstanceSnapshot ConfirmCreate(string sessionStableId,
            SimulationBattleCreateConfirmRequest request)
        {
            ValidateCommand(request.CommandId);
            var existing = battleStore.Find("battle:" + request.CommandId.Trim());
            if (existing != null)
            {
                var snapshot = existing.Snapshot();
                if (snapshot.SessionStableId != sessionStableId.Trim()
                    || snapshot.EncounterStableId != request.EncounterStableId.Trim()
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

        public void Reconcile(string sessionStableId, 경영SimulationSessionSnapshot world)
        {
            foreach (var battle in battleStore.FindBySession(sessionStableId.Trim()))
                battle.Reconcile(world.CurrentTick, world.Revision);
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
