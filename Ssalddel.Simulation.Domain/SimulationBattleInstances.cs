using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class SimulationBattleCreationContext
    {
        public string BattleStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string CommanderActorStableId { get; set; } = string.Empty;
        public int StartedWorldTick { get; set; }
        public long StartedWorldRevision { get; set; }
        public int ScenarioSeed { get; set; }
        public int AlliedStrength { get; set; }
        public int HostileStrength { get; set; }
        public string CombatSpaceCode { get; set; } = SimulationLocalCombatCodes.DerivedBattlefield;
        public string EncounterScaleCode { get; set; } = SimulationLocalCombatCodes.Battlefield;
        public string ScalePolicyRevision { get; set; } = SimulationLocalCombatCodes.ScalePolicyRevision;
        public string[] ScaleReasonCodes { get; set; } = Array.Empty<string>();
        public SimulationLocalCombatWorldContextSnapshot LocalWorldContext { get; set; } = new();
        public string[] InitialResourceStableIds { get; set; } = Array.Empty<string>();
        public string[] ReinforcementCandidateStableIds { get; set; } = Array.Empty<string>();
        public SimulationBattlefieldDerivationSnapshot BattlefieldDerivation { get; set; } = new();
        public SimulationBattleUnitRosterSnapshot UnitRoster { get; set; } = new();
        public string CreateCommandId { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationParallelBattle,
        SsalddelCodeLayer.Domain,
        "독립 BattleTick·참가·배치·지원·결과 상태 전이를 소유한다.",
        StepKey = "domain.battle-state",
        DependsOnStepKeys = new string[] { "application.battle" },
        ExecutionStage = SsalddelCodeExecutionStage.Tick,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 40,
        Boundary = "전투 상태는 SimulationOnly이며 운영 자원이나 실제 인력을 잠그지 않는다.")]
    public sealed partial class SimulationBattleInstanceState
    {
        private readonly object gate = new object();
        private readonly SimulationBattleCreationContext context;
        private readonly List<SimulationBattleParticipantSnapshot> participants =
            new List<SimulationBattleParticipantSnapshot>();
        private readonly List<SimulationBattleResourceReservationSnapshot> reservations =
            new List<SimulationBattleResourceReservationSnapshot>();
        private readonly List<SimulationBattleSupportSnapshot> supports =
            new List<SimulationBattleSupportSnapshot>();
        private readonly List<SimulationBattleParticipationReservationSnapshot>
            participationReservations =
                new List<SimulationBattleParticipationReservationSnapshot>();
        private readonly List<SimulationBattleWorldTargetReservationSnapshot>
            worldTargetReservations =
                new List<SimulationBattleWorldTargetReservationSnapshot>();
        private readonly List<SimulationBattleSemanticEffectSnapshot> semanticEffects =
            new List<SimulationBattleSemanticEffectSnapshot>();
        private readonly Dictionary<string, AppliedCommand> commands =
            new Dictionary<string, AppliedCommand>(StringComparer.Ordinal);
        private readonly List<string> replayEvents = new List<string>();
        private SimulationBattleOutcomeSnapshot? outcome;
        private string phaseCode = SimulationBattleInstanceCodes.Deploying;
        private string deploymentCode = string.Empty;
        private long revision;
        private int combatTick;

        public SimulationBattleInstanceState(SimulationBattleCreationContext creation)
        {
            ValidateCreation(creation);
            context = CloneContext(creation);
            participants.Add(new SimulationBattleParticipantSnapshot
            {
                ActorStableId = creation.CommanderActorStableId.Trim(),
                ParticipationRoleCode = SimulationBattleInstanceCodes.Commander,
                CanControlWorldState = true,
                PresentationOnly = false,
            });
            foreach (var resource in creation.InitialResourceStableIds
                .Select(value => value.Trim()).Distinct(StringComparer.Ordinal))
            {
                reservations.Add(new SimulationBattleResourceReservationSnapshot
                {
                    ResourceStableId = resource,
                    ReservationKindCode = "InitialBattleResource",
                    SourceCommandId = creation.CreateCommandId.Trim(),
                });
            }
            foreach (var actor in context.UnitRoster.Units
                .Where(value => value.SideCode == SimulationFarmTacticalCombatCodes.Allied)
                .SelectMany(value => value.MemberActorStableIds)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal))
            {
                participationReservations.Add(
                    new SimulationBattleParticipationReservationSnapshot
                    {
                        ActorStableId = actor,
                        BattleStableId = context.BattleStableId,
                        ReservedWorldTick = context.StartedWorldTick,
                        EnteredBattleTick = 0,
                    });
            }
            foreach (var target in context.BattlefieldDerivation.WorldContext.Anchors
                .Where(value => !string.IsNullOrWhiteSpace(value.WorldEffectTargetStableId)
                    && value.PreservationPolicyCode !=
                        SimulationBattlefieldDerivationCodes.ContextOnly)
                .Select(value => value.WorldEffectTargetStableId)
                .Distinct(StringComparer.Ordinal))
            {
                worldTargetReservations.Add(
                    new SimulationBattleWorldTargetReservationSnapshot
                    {
                        WorldEffectTargetStableId = target,
                        BattleStableId = context.BattleStableId,
                        ReservedWorldTick = context.StartedWorldTick,
                        ConflictCapabilityCodes = new[]
                        {
                            "FacilityRelocation", "FacilityRepair", "FacilityRemoval",
                        },
                    });
            }
            InitializeLocalCombat();
            replayEvents.Add("create~" + Payload(creation));
        }

        public string BattleStableId => context.BattleStableId;
        public string SessionStableId => context.SessionStableId;
        public string EncounterStableId => context.EncounterStableId;
        public string AreaStableId => context.AreaStableId;
        public string[] ReinforcementCandidateStableIds =>
            context.ReinforcementCandidateStableIds.ToArray();

        public SimulationBattleInstanceSnapshot Snapshot()
        {
            lock (gate) return CreateSnapshot();
        }

        public SimulationBattleSaveRecordSnapshot CreateSaveRecord()
        {
            lock (gate)
            {
                var record = new SimulationBattleSaveRecordSnapshot
                {
                    Creation = ToCreationSnapshot(context),
                    State = CreateSnapshot(),
                    ReplayEvents = replayEvents.ToArray(),
                    AppliedCommands = commands.OrderBy(value => value.Key,
                            StringComparer.Ordinal)
                        .Select(value => new SimulationBattleAppliedCommandSnapshot
                        {
                            CommandId = value.Key,
                            Payload = value.Value.Payload,
                            Result = Clone(value.Value.Snapshot),
                        }).ToArray(),
                };
                record.IntegrityHashSha256 = CalculateSaveRecordIntegrityHash(record);
                return CloneSaveRecord(record);
            }
        }

        public static SimulationBattleInstanceState Restore(
            SimulationBattleSaveRecordSnapshot record)
        {
            ValidateSaveRecord(record);
            var restored = new SimulationBattleInstanceState(ToCreationContext(record.Creation));
            lock (restored.gate)
            {
                restored.participants.Clear();
                restored.participants.AddRange(record.State.Participants.Select(CloneParticipant));
                restored.reservations.Clear();
                restored.reservations.AddRange(record.State.ResourceReservations.Select(CloneReservation));
                restored.supports.Clear();
                restored.supports.AddRange(record.State.Supports.Select(CloneSupport));
                restored.participationReservations.Clear();
                restored.participationReservations.AddRange(record.State
                    .ParticipationReservations.Select(
                        SimulationBattlefieldSnapshotCloner.Participation));
                restored.worldTargetReservations.Clear();
                restored.worldTargetReservations.AddRange(record.State
                    .WorldTargetReservations.Select(
                        SimulationBattlefieldSnapshotCloner.WorldTarget));
                restored.semanticEffects.Clear();
                restored.semanticEffects.AddRange(record.State.SemanticEffects.Select(
                    SimulationBattlefieldSnapshotCloner.Effect));
                restored.outcome = record.State.Outcome == null
                    ? null : CloneOutcome(record.State.Outcome);
                restored.phaseCode = record.State.PhaseCode;
                restored.deploymentCode = record.State.DeploymentCode;
                restored.revision = record.State.BattleRevision;
                restored.combatTick = record.State.CombatTick;
                restored.RestoreLocalCombat(record.State.LocalCombat);
                restored.replayEvents.Clear();
                restored.replayEvents.AddRange(record.ReplayEvents);
                restored.commands.Clear();
                foreach (var command in record.AppliedCommands)
                    restored.commands.Add(command.CommandId,
                        new AppliedCommand(command.Payload, Clone(command.Result)));

                var snapshot = restored.CreateSnapshot();
                if (!string.Equals(snapshot.ReplayHashSha256,
                    record.State.ReplayHashSha256, StringComparison.Ordinal)
                    || snapshot.BattleRevision != record.State.BattleRevision
                    || snapshot.CombatTick != record.State.CombatTick
                    || !string.Equals(snapshot.PhaseCode, record.State.PhaseCode,
                        StringComparison.Ordinal))
                    throw new SimulationConflictException("SimulationBattleSaveReplayMismatch");
            }
            return restored;
        }

        public static void ValidateSaveRecord(SimulationBattleSaveRecordSnapshot record)
        {
            if (record == null || record.Creation == null || record.State == null
                || record.ReplayEvents == null || record.AppliedCommands == null
                || record.State.Participants == null
                || record.State.ResourceReservations == null
                || record.State.Supports == null
                || record.State.ParticipationReservations == null
                || record.State.WorldTargetReservations == null
                || record.State.SemanticEffects == null)
                throw new SimulationContractException("SimulationBattleSaveRecordInvalid");
            var context = ToCreationContext(record.Creation);
            ValidateCreation(context);
            if (record.State.BattleStableId != context.BattleStableId
                || record.State.SessionStableId != context.SessionStableId
                || record.State.EncounterStableId != context.EncounterStableId
                || record.State.AreaStableId != context.AreaStableId
                || record.State.StartedWorldTick != context.StartedWorldTick
                || record.State.StartedWorldRevision != context.StartedWorldRevision
                || record.State.ScenarioSeed != context.ScenarioSeed
                || record.State.BattlefieldDerivation.BattlefieldDerivationInputHashSha256
                    != context.BattlefieldDerivation.BattlefieldDerivationInputHashSha256
                || record.State.BattlefieldDerivation.BattlefieldPlan
                    .BattlefieldPlanHashSha256 != context.BattlefieldDerivation
                    .BattlefieldPlan.BattlefieldPlanHashSha256
                || record.State.UnitRoster.BattleUnitRosterHashSha256 !=
                    context.UnitRoster.BattleUnitRosterHashSha256
                || record.State.UnitRoster.CombatSeedHashSha256 !=
                    context.UnitRoster.CombatSeedHashSha256
                || record.State.IsOperationalState || !record.State.SimulationOnly
                || record.State.BattleRevision < 0 || record.State.CombatTick < 0
                || record.AppliedCommands.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.CommandId)
                    || value.Result == null
                    || value.Result.Participants == null
                    || value.Result.ResourceReservations == null
                    || value.Result.Supports == null
                    || value.Result.ParticipationReservations == null
                    || value.Result.WorldTargetReservations == null
                    || value.Result.SemanticEffects == null))
                throw new SimulationContractException("SimulationBattleSaveRecordInvalid");
            if (record.AppliedCommands.Select(value => value.CommandId)
                .Distinct(StringComparer.Ordinal).Count() != record.AppliedCommands.Length)
                throw new SimulationConflictException("SimulationBattleSaveCommandDuplicate");
            var expectedHash = CalculateSaveRecordIntegrityHash(record);
            if (!string.Equals(record.IntegrityHashSha256, expectedHash,
                StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationBattleSaveIntegrityMismatch");
        }

        public SimulationBattleInstanceSnapshot ConfirmParticipation(
            SimulationBattleParticipationConfirmRequest request)
        {
            ValidateParticipation(request);
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    string.Join("~", request.ActorStableId.Trim(),
                        request.ParticipationRoleCode.Trim(),
                        request.DelegatedSquadStableId.Trim(),
                        request.ExpectedTeamPolicyRevision.ToString(CultureInfo.InvariantCulture)),
                    () =>
                    {
                        if (phaseCode != SimulationBattleInstanceCodes.Deploying
                            && phaseCode != SimulationBattleInstanceCodes.Active)
                            throw new SimulationConflictException("SimulationBattleParticipationClosed");
                        if (participants.Any(value => value.ActorStableId == request.ActorStableId.Trim()))
                            throw new SimulationConflictException("SimulationBattleActorAlreadyParticipating");
                        if (request.ParticipationRoleCode == SimulationBattleInstanceCodes.DelegatedSquad
                            && !reservations.Any(value => value.ResourceStableId ==
                                request.DelegatedSquadStableId.Trim()))
                            throw new SimulationConflictException("SimulationBattleDelegatedSquadNotReserved");
                        participants.Add(new SimulationBattleParticipantSnapshot
                        {
                            ActorStableId = request.ActorStableId.Trim(),
                            ParticipationRoleCode = request.ParticipationRoleCode.Trim(),
                            DelegatedSquadStableId = request.DelegatedSquadStableId.Trim(),
                            CanControlWorldState = request.ParticipationRoleCode ==
                                SimulationBattleInstanceCodes.DelegatedSquad,
                            PresentationOnly = request.ParticipationRoleCode ==
                                SimulationBattleInstanceCodes.Spectator,
                        });
                    });
            }
        }

        public SimulationBattleDeploymentPreviewSnapshot PreviewDeployment(
            SimulationBattleDeploymentPreviewRequest request)
        {
            ValidateDeployment(request.ActorStableId, request.DeploymentCode,
                request.ExpectedBattleRevision);
            lock (gate)
            {
                var blocks = new List<string>();
                if (request.ExpectedBattleRevision != revision)
                    blocks.Add("SimulationBattleExpectedRevisionMismatch");
                if (phaseCode != SimulationBattleInstanceCodes.Deploying)
                    blocks.Add("SimulationBattleDeploymentClosed");
                if (!IsCommander(request.ActorStableId))
                    blocks.Add("SimulationBattleCommanderRequired");
                return new SimulationBattleDeploymentPreviewSnapshot
                {
                    BattleStableId = BattleStableId,
                    BattleRevision = revision,
                    ActorStableId = request.ActorStableId.Trim(),
                    DeploymentCode = request.DeploymentCode.Trim(),
                    ProjectedPreparednessBonus = DeploymentBonus(request.DeploymentCode),
                    CanConfirm = blocks.Count == 0,
                    BlockingReasonCodes = blocks.ToArray(),
                };
            }
        }

        public SimulationBattleInstanceSnapshot ConfirmDeployment(
            SimulationBattleDeploymentConfirmRequest request)
        {
            ValidateDeployment(request.ActorStableId, request.DeploymentCode,
                request.ExpectedBattleRevision);
            ValidateCommandId(request.CommandId);
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    request.ActorStableId.Trim() + "~" + request.DeploymentCode.Trim(),
                    () =>
                    {
                        var preview = PreviewDeployment(new SimulationBattleDeploymentPreviewRequest
                        {
                            ExpectedBattleRevision = request.ExpectedBattleRevision,
                            ActorStableId = request.ActorStableId,
                            DeploymentCode = request.DeploymentCode,
                        });
                        if (!preview.CanConfirm)
                            throw new SimulationConflictException(preview.BlockingReasonCodes[0]);
                        deploymentCode = request.DeploymentCode.Trim();
                        phaseCode = SimulationBattleInstanceCodes.Active;
                    });
            }
        }

        public SimulationBattleSupportPreviewSnapshot PreviewSupport(
            SimulationBattleSupportPreviewRequest request,
            long currentWorldRevision,
            bool sourceAvailable)
        {
            ValidateSupport(request.SupportCode, request.SourceResourceStableId,
                request.ExpectedWorldRevision, request.ExpectedBattleRevision);
            lock (gate)
            {
                var blocks = new List<string>();
                if (request.ExpectedBattleRevision != revision)
                    blocks.Add("SimulationBattleExpectedRevisionMismatch");
                if (request.ExpectedWorldRevision != currentWorldRevision)
                    blocks.Add("SimulationExpectedRevisionMismatch");
                if (phaseCode != SimulationBattleInstanceCodes.Deploying
                    && phaseCode != SimulationBattleInstanceCodes.Active)
                    blocks.Add("SimulationBattleSupportClosed");
                if (!sourceAvailable)
                    blocks.Add("SimulationBattleSupportSourceUnavailable");
                if (reservations.Any(value => value.ResourceStableId ==
                    request.SourceResourceStableId.Trim()
                    && value.StateCode == SimulationBattleInstanceCodes.Reserved))
                    blocks.Add("BattleResourceLocked");
                return new SimulationBattleSupportPreviewSnapshot
                {
                    BattleStableId = BattleStableId,
                    BattleRevision = revision,
                    WorldRevision = currentWorldRevision,
                    SupportCode = request.SupportCode.Trim(),
                    SourceResourceStableId = request.SourceResourceStableId.Trim(),
                    ArrivesCombatTick = Math.Min(SimulationBattleInstanceCodes.MaximumCombatTick,
                        combatTick + (request.SupportCode == SimulationBattleInstanceCodes.SupplyCrate
                            ? 300 : 600)),
                    ProjectedStrengthBonus = request.SupportCode ==
                        SimulationBattleInstanceCodes.SupplyCrate ? 8 : 12,
                    CanConfirm = blocks.Count == 0,
                    BlockingReasonCodes = blocks.ToArray(),
                };
            }
        }

        public SimulationBattleInstanceSnapshot ConfirmSupport(
            SimulationBattleSupportConfirmRequest request,
            int currentWorldTick,
            long currentWorldRevision,
            bool sourceAvailable)
        {
            ValidateCommandId(request.CommandId);
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    string.Join("~", request.ExpectedWorldRevision,
                        request.RequestingActorStableId.Trim(), request.SupportCode.Trim(),
                        request.SourceResourceStableId.Trim()),
                    () =>
                    {
                        var preview = PreviewSupport(new SimulationBattleSupportPreviewRequest
                        {
                            ExpectedWorldRevision = request.ExpectedWorldRevision,
                            ExpectedBattleRevision = request.ExpectedBattleRevision,
                            RequestingActorStableId = request.RequestingActorStableId,
                            SupportCode = request.SupportCode,
                            SourceResourceStableId = request.SourceResourceStableId,
                        }, currentWorldRevision, sourceAvailable);
                        if (!preview.CanConfirm)
                            throw new SimulationConflictException(preview.BlockingReasonCodes[0]);
                        supports.Add(new SimulationBattleSupportSnapshot
                        {
                            SupportStableId = "battle-support:" + request.CommandId.Trim(),
                            CommandId = request.CommandId.Trim(),
                            RequestingActorStableId = request.RequestingActorStableId.Trim(),
                            SupportCode = request.SupportCode.Trim(),
                            SourceResourceStableId = request.SourceResourceStableId.Trim(),
                            ConfirmedWorldTick = currentWorldTick,
                            ConfirmedWorldRevision = currentWorldRevision,
                            ArrivesCombatTick = preview.ArrivesCombatTick,
                            StrengthBonus = preview.ProjectedStrengthBonus,
                        });
                        reservations.Add(new SimulationBattleResourceReservationSnapshot
                        {
                            ResourceStableId = request.SourceResourceStableId.Trim(),
                            ReservationKindCode = request.SupportCode.Trim(),
                            SourceCommandId = request.CommandId.Trim(),
                        });
                    });
            }
        }

        public SimulationBattleInstanceSnapshot Advance(
            SimulationBattleAdvanceRequest request,
            int currentWorldTick,
            int heroContributionScore)
        {
            ValidateAdvance(request);
            lock (gate)
            {
                if (context.CombatSpaceCode == SimulationLocalCombatCodes.WorldLocal)
                    return AdvanceLocalCombat(request, currentWorldTick);
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    request.CombatTickCount.ToString(CultureInfo.InvariantCulture),
                    () =>
                    {
                        if (phaseCode != SimulationBattleInstanceCodes.Active)
                            throw new SimulationConflictException("SimulationBattleNotActive");
                        combatTick = Math.Min(SimulationBattleInstanceCodes.MaximumCombatTick,
                            combatTick + request.CombatTickCount);
                        foreach (var support in supports.Where(value =>
                            value.StateCode == SimulationBattleInstanceCodes.InTransit
                            && value.ArrivesCombatTick <= combatTick))
                            support.StateCode = SimulationBattleInstanceCodes.Arrived;
                        if (combatTick >= SimulationBattleInstanceCodes.MaximumCombatTick)
                            Resolve(currentWorldTick, heroContributionScore);
                    });
            }
        }

        public SimulationBattleInstanceSnapshot ConfirmTacticalCommand(
            SimulationBattleTacticalCommandConfirmRequest request)
        {
            ValidateTacticalCommand(request);
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    string.Join("~", request.RequestingActorStableId.Trim(),
                        request.UnitStableId.Trim(), request.CommandCode.Trim(),
                        request.TargetUnitStableId.Trim(), request.TargetXCentimeters,
                        request.TargetZCentimeters, request.FormationCode.Trim()),
                    () =>
                    {
                        if (phaseCode != SimulationBattleInstanceCodes.Active)
                            throw new SimulationConflictException(
                                "SimulationBattleNotActive");
                        if (!IsCommander(request.RequestingActorStableId))
                            throw new SimulationConflictException(
                                "SimulationBattleCommanderRequired");
                        var unit = context.UnitRoster.Units.FirstOrDefault(value =>
                            value.UnitStableId == request.UnitStableId.Trim()
                            && value.SideCode == SimulationFarmTacticalCombatCodes.Allied);
                        if (unit == null)
                            throw new SimulationConflictException(
                                "SimulationBattleUnitNotControllable");
                        if (request.CommandCode == SimulationBattlefieldDerivationCodes.Attack
                            && !context.UnitRoster.Units.Any(value => value.UnitStableId ==
                                request.TargetUnitStableId.Trim()
                                && value.SideCode == SimulationFarmTacticalCombatCodes.Hostile))
                            throw new SimulationConflictException(
                                "SimulationBattleTargetUnitUnavailable");
                    });
            }
        }

        public SimulationBattleInstanceSnapshot Reconcile(int worldTick, long worldRevision)
        {
            lock (gate)
            {
                if (phaseCode != SimulationBattleInstanceCodes.Completed || outcome == null
                    || worldTick <= outcome.CompletedWorldTick) return CreateSnapshot();
                outcome.ReconciliationStateCode = SimulationBattleInstanceCodes.Applied;
                outcome.AppliedWorldTick = worldTick;
                outcome.AppliedWorldRevision = worldRevision;
                phaseCode = SimulationBattleInstanceCodes.Reconciled;
                foreach (var reservation in reservations)
                    reservation.StateCode = SimulationBattleInstanceCodes.Released;
                foreach (var reservation in participationReservations)
                {
                    reservation.StateCode = SimulationBattlefieldDerivationCodes.Released;
                    reservation.ReleasedWorldTick = worldTick;
                }
                foreach (var reservation in worldTargetReservations)
                {
                    reservation.StateCode = SimulationBattlefieldDerivationCodes.Released;
                    reservation.ReleasedWorldTick = worldTick;
                }
                foreach (var effect in semanticEffects.Where(value =>
                    value.ReconciliationStateCode ==
                        SimulationBattlefieldDerivationCodes.Pending))
                {
                    effect.ReconciliationStateCode =
                        SimulationBattlefieldDerivationCodes.Applied;
                    effect.AppliedWorldTick = worldTick;
                    effect.AppliedWorldRevision = worldRevision;
                }
                foreach (var support in supports.Where(value =>
                    value.StateCode == SimulationBattleInstanceCodes.InTransit))
                    support.StateCode = SimulationBattleInstanceCodes.Returned;
                revision++;
                replayEvents.Add("reconcile~" + worldTick.ToString(CultureInfo.InvariantCulture)
                    + "~" + worldRevision.ToString(CultureInfo.InvariantCulture));
                return CreateSnapshot();
            }
        }

        private void Resolve(int worldTick, int heroContributionScore)
        {
            var arrivedSupport = supports.Where(value =>
                value.StateCode == SimulationBattleInstanceCodes.Arrived).Sum(value => value.StrengthBonus);
            var preparedness = DeploymentBonus(deploymentCode);
            var deterministicNoise = PositiveHash(context.ScenarioSeed + "~" +
                context.EncounterStableId) % 7;
            var alliedScore = context.AlliedStrength + arrivedSupport + preparedness
                + Math.Max(0, heroContributionScore) + deterministicNoise;
            var hostileScore = context.HostileStrength +
                PositiveHash(context.EncounterStableId + "~hostile") % 5;
            var victory = alliedScore >= hostileScore;
            var loss = victory ? Math.Max(0, hostileScore / 5 - arrivedSupport / 8)
                : Math.Max(1, hostileScore - alliedScore);
            outcome = new SimulationBattleOutcomeSnapshot
            {
                OutcomeStableId = "battle-outcome:" + BattleStableId,
                ResultCode = victory ? SimulationBattleInstanceCodes.Victory
                    : SimulationBattleInstanceCodes.Defeat,
                CompletedWorldTick = worldTick,
                AlliedStrengthDelta = -loss,
                RecoverableInjuryCount = victory ? loss : loss + 1,
                FacilityDamageUnits = victory ? loss : loss + 5m,
                SupplyLossUnits = victory ? Math.Max(0, loss - 1) : loss + 3m,
                SecurityDelta = victory ? 2 : -4,
                MoraleDelta = victory ? 3 : -5,
                UsedDeterministicAutoCommand = heroContributionScore <= 0,
            };
            semanticEffects.Clear();
            semanticEffects.AddRange(BuildSemanticEffects(outcome));
            phaseCode = SimulationBattleInstanceCodes.Completed;
        }

        private SimulationBattleInstanceSnapshot Apply(string commandId, long expectedRevision,
            string payload, Action action)
        {
            ValidateCommandId(commandId);
            var id = commandId.Trim();
            if (commands.TryGetValue(id, out var applied))
            {
                if (!string.Equals(applied.Payload, payload, StringComparison.Ordinal))
                    throw new SimulationConflictException("SimulationCommandPayloadConflict");
                return Clone(applied.Snapshot);
            }
            if (expectedRevision != revision)
                throw new SimulationConflictException("SimulationBattleExpectedRevisionMismatch");
            action();
            revision++;
            replayEvents.Add(id + "~" + payload);
            var snapshot = CreateSnapshot();
            commands.Add(id, new AppliedCommand(payload, Clone(snapshot)));
            return snapshot;
        }

        private SimulationBattleInstanceSnapshot CreateSnapshot()
        {
            var snapshot = new SimulationBattleInstanceSnapshot
            {
                BattleStableId = context.BattleStableId,
                SessionStableId = context.SessionStableId,
                EncounterStableId = context.EncounterStableId,
                AreaStableId = context.AreaStableId,
                CombatSpaceCode = context.CombatSpaceCode,
                EncounterScaleCode = context.EncounterScaleCode,
                ScalePolicyRevision = context.ScalePolicyRevision,
                ScaleReasonCodes = context.ScaleReasonCodes.ToArray(),
                PhaseCode = phaseCode,
                BattleRevision = revision,
                CombatTick = combatTick,
                StartedWorldTick = context.StartedWorldTick,
                StartedWorldRevision = context.StartedWorldRevision,
                AlliedStrength = context.AlliedStrength,
                HostileStrength = context.HostileStrength,
                ScenarioSeed = context.ScenarioSeed,
                DeploymentCode = deploymentCode,
                Participants = participants.Select(CloneParticipant).ToArray(),
                ResourceReservations = reservations.Select(CloneReservation).ToArray(),
                Supports = supports.Select(CloneSupport).ToArray(),
                BattlefieldDerivation = SimulationBattlefieldSnapshotCloner.Derivation(
                    context.BattlefieldDerivation),
                UnitRoster = SimulationBattlefieldSnapshotCloner.Roster(context.UnitRoster),
                LocalCombat = CreateLocalCombatSnapshot(),
                ParticipationReservations = participationReservations.Select(
                    SimulationBattlefieldSnapshotCloner.Participation).ToArray(),
                WorldTargetReservations = worldTargetReservations.Select(
                    SimulationBattlefieldSnapshotCloner.WorldTarget).ToArray(),
                SemanticEffects = semanticEffects.Select(
                    SimulationBattlefieldSnapshotCloner.Effect).ToArray(),
                Outcome = outcome == null ? null : CloneOutcome(outcome),
            };
            snapshot.ReplayHashSha256 = CalculateReplayHash(snapshot);
            return snapshot;
        }

        public static SimulationBattleInstanceSnapshot Clone(SimulationBattleInstanceSnapshot source)
            => new SimulationBattleInstanceSnapshot
            {
                BattleStableId = source.BattleStableId, SessionStableId = source.SessionStableId,
                EncounterStableId = source.EncounterStableId, AreaStableId = source.AreaStableId,
                RuleRevision = source.RuleRevision, PhaseCode = source.PhaseCode,
                CombatSpaceCode = source.CombatSpaceCode,
                EncounterScaleCode = source.EncounterScaleCode,
                ScalePolicyRevision = source.ScalePolicyRevision,
                ScaleReasonCodes = source.ScaleReasonCodes.ToArray(),
                BattleRevision = source.BattleRevision, CombatTick = source.CombatTick,
                StartedWorldTick = source.StartedWorldTick,
                StartedWorldRevision = source.StartedWorldRevision,
                AlliedStrength = source.AlliedStrength, HostileStrength = source.HostileStrength,
                ScenarioSeed = source.ScenarioSeed, DeploymentCode = source.DeploymentCode,
                Participants = source.Participants.Select(CloneParticipant).ToArray(),
                ResourceReservations = source.ResourceReservations.Select(CloneReservation).ToArray(),
                Supports = source.Supports.Select(CloneSupport).ToArray(),
                BattlefieldDerivation = SimulationBattlefieldSnapshotCloner.Derivation(
                    source.BattlefieldDerivation),
                UnitRoster = SimulationBattlefieldSnapshotCloner.Roster(source.UnitRoster),
                LocalCombat = CloneLocalCombat(source.LocalCombat),
                ParticipationReservations = source.ParticipationReservations.Select(
                    SimulationBattlefieldSnapshotCloner.Participation).ToArray(),
                WorldTargetReservations = source.WorldTargetReservations.Select(
                    SimulationBattlefieldSnapshotCloner.WorldTarget).ToArray(),
                SemanticEffects = source.SemanticEffects.Select(
                    SimulationBattlefieldSnapshotCloner.Effect).ToArray(),
                Outcome = source.Outcome == null ? null : CloneOutcome(source.Outcome),
                ReplayHashSha256 = source.ReplayHashSha256,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private SimulationBattleSemanticEffectSnapshot[] BuildSemanticEffects(
            SimulationBattleOutcomeSnapshot resolved)
        {
            var effects = new List<SimulationBattleSemanticEffectSnapshot>();
            var evidence = Math.Max(0, Math.Min(1000,
                (int)Math.Round(resolved.FacilityDamageUnits * 100m,
                    MidpointRounding.AwayFromZero)));
            foreach (var group in context.BattlefieldDerivation.WorldContext.Anchors
                .Where(value => !string.IsNullOrWhiteSpace(value.WorldEffectTargetStableId)
                    && value.PreservationPolicyCode !=
                        SimulationBattlefieldDerivationCodes.ContextOnly)
                .GroupBy(value => value.WorldEffectTargetStableId,
                    StringComparer.Ordinal))
            {
                var anchors = group.OrderBy(value => value.BattlefieldAnchorStableId,
                    StringComparer.Ordinal).ToArray();
                var effectCode = anchors.Any(value => value.AnchorTypeCodes.Contains(
                        SimulationBattlefieldDerivationCodes.Gate,
                        StringComparer.Ordinal))
                    ? SimulationBattlefieldDerivationCodes.GateCombatDamage
                    : SimulationBattlefieldDerivationCodes.FacilityCombatDamage;
                effects.Add(CreateSemanticEffect(group.Key, effectCode, evidence,
                    anchors.Select(value => value.BattlefieldAnchorStableId).ToArray(),
                    SimulationBattlefieldDerivationCodes.MaxSeverity));
            }

            var casualtyCount = Math.Min(resolved.RecoverableInjuryCount,
                context.UnitRoster.Units.Where(value => value.SideCode ==
                        SimulationFarmTacticalCombatCodes.Allied)
                    .SelectMany(value => value.MemberActorStableIds)
                    .Distinct(StringComparer.Ordinal).Count());
            foreach (var actor in context.UnitRoster.Units
                .Where(value => value.SideCode == SimulationFarmTacticalCombatCodes.Allied)
                .SelectMany(value => value.MemberActorStableIds)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).Take(casualtyCount))
            {
                effects.Add(CreateSemanticEffect(actor,
                    SimulationBattlefieldDerivationCodes.ActorCombatCasualty,
                    resolved.ResultCode == SimulationBattleInstanceCodes.Victory ? 500 : 800,
                    Array.Empty<string>(), SimulationBattlefieldDerivationCodes.SumCapped));
            }

            var objective = context.BattlefieldDerivation.WorldContext.Anchors.FirstOrDefault(
                value => value.AnchorTypeCodes.Contains(
                    SimulationBattlefieldDerivationCodes.Objective,
                    StringComparer.Ordinal));
            if (objective != null && !string.IsNullOrWhiteSpace(
                    objective.WorldEffectTargetStableId))
            {
                effects.Add(CreateSemanticEffect(objective.WorldEffectTargetStableId,
                    resolved.ResultCode == SimulationBattleInstanceCodes.Victory
                        ? SimulationBattlefieldDerivationCodes.ObjectiveSecured
                        : SimulationBattlefieldDerivationCodes.ObjectiveLost,
                    1000, new[] { objective.BattlefieldAnchorStableId },
                    SimulationBattlefieldDerivationCodes.MaxSeverity));
            }
            return effects.OrderBy(value => value.WorldEffectTargetStableId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.SemanticEffectCode, StringComparer.Ordinal).ToArray();
        }

        private SimulationBattleSemanticEffectSnapshot CreateSemanticEffect(
            string targetStableId, string effectCode, int evidence,
            string[] anchorIds, string aggregation)
        {
            var severity = evidence >= 900 ? SimulationBattlefieldDerivationCodes.Destroyed
                : evidence >= 650 ? SimulationBattlefieldDerivationCodes.Severe
                : evidence >= 300 ? SimulationBattlefieldDerivationCodes.Moderate
                : SimulationBattlefieldDerivationCodes.Light;
            var stableId = "battle-effect:" + StableHash(string.Join("|",
                BattleStableId, targetStableId, effectCode,
                string.Join(",", anchorIds.OrderBy(value => value,
                    StringComparer.Ordinal)))).Substring(0, 24);
            return new SimulationBattleSemanticEffectSnapshot
            {
                SemanticEffectStableId = stableId,
                BattleStableId = BattleStableId,
                BattlefieldAnchorStableIds = anchorIds,
                WorldEffectTargetStableId = targetStableId,
                SemanticEffectCode = effectCode,
                SeverityCode = severity,
                TacticalEvidencePermille = evidence,
                AggregationPolicyCode = aggregation,
                RuleRevision = "battle-world-effect.semantic.r1",
                WorldEffectApplicationKey = string.Join("|", BattleStableId,
                    targetStableId, effectCode, stableId),
            };
        }

        private static string StableHash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private string CalculateReplayHash(SimulationBattleInstanceSnapshot snapshot)
        {
            using var sha = SHA256.Create();
            var value = Payload(context) + "|" + string.Join("|", replayEvents)
                + "|" + snapshot.PhaseCode + "|" + snapshot.BattleRevision
                + "|" + snapshot.CombatTick;
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private bool IsCommander(string actorStableId) => participants.Any(value =>
            value.ActorStableId == actorStableId.Trim()
            && value.ParticipationRoleCode == SimulationBattleInstanceCodes.Commander);
        private static int DeploymentBonus(string code) =>
            code == SimulationBattleInstanceCodes.Defensive ? 6
            : code == SimulationBattleInstanceCodes.Forward ? 3 : 4;
        private static int PositiveHash(string value)
        {
            unchecked
            {
                var hash = 17;
                foreach (var ch in value) hash = hash * 31 + ch;
                return hash & int.MaxValue;
            }
        }

        private static void ValidateCreation(SimulationBattleCreationContext value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Require(value.BattleStableId, "SimulationBattleStableIdInvalid");
            Require(value.SessionStableId, "SimulationSessionStableIdInvalid");
            Require(value.EncounterStableId, "SimulationBattleEncounterInvalid");
            Require(value.AreaStableId, "SimulationBattleAreaInvalid");
            Require(value.CommanderActorStableId, "SimulationBattleCommanderInvalid");
            ValidateCommandId(value.CreateCommandId);
            if (value.AlliedStrength <= 0 || value.HostileStrength <= 0
                || value.InitialResourceStableIds == null
                || value.ReinforcementCandidateStableIds == null
                || value.ScaleReasonCodes == null || value.LocalWorldContext == null
                || value.BattlefieldDerivation == null || value.UnitRoster == null)
                throw new SimulationContractException("SimulationBattleCreationContextInvalid");
            if (!SimulationCombatScalePolicy.IsKnownSpace(value.CombatSpaceCode)
                || !SimulationCombatScalePolicy.IsKnownScale(value.EncounterScaleCode))
                throw new SimulationContractException("SimulationBattleCombatScaleInvalid");
            var spatial = !string.IsNullOrWhiteSpace(value.BattlefieldDerivation
                .BattlefieldDerivationInputHashSha256);
            if (spatial && (!value.BattlefieldDerivation.CanConfirm
                || string.IsNullOrWhiteSpace(value.BattlefieldDerivation.WorldContext
                    .ContextHashSha256)
                || string.IsNullOrWhiteSpace(value.BattlefieldDerivation.WorldContext
                    .AnchorSetHashSha256)
                || string.IsNullOrWhiteSpace(value.BattlefieldDerivation.BattlefieldPlan
                    .BattlefieldPlanHashSha256)
                || string.IsNullOrWhiteSpace(value.UnitRoster.BattleUnitRosterHashSha256)
                || string.IsNullOrWhiteSpace(value.UnitRoster.CombatSeedHashSha256)))
                throw new SimulationContractException(
                    "SimulationBattleSpatialCreationContextInvalid");
        }
        private static void ValidateParticipation(SimulationBattleParticipationConfirmRequest value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            ValidateCommandId(value.CommandId); Require(value.ActorStableId, "SimulationBattleActorInvalid");
            if (value.ExpectedBattleRevision < 0 || value.ExpectedTeamPolicyRevision < 0)
                throw new SimulationContractException("SimulationBattleRevisionInvalid");
            if (value.ParticipationRoleCode != SimulationBattleInstanceCodes.DelegatedSquad
                && value.ParticipationRoleCode != SimulationBattleInstanceCodes.Spectator)
                throw new SimulationContractException("SimulationBattleParticipationRoleInvalid");
            if (value.ParticipationRoleCode == SimulationBattleInstanceCodes.DelegatedSquad)
                Require(value.DelegatedSquadStableId, "SimulationBattleDelegatedSquadInvalid");
        }
        private static void ValidateDeployment(string actor, string code, long expected)
        {
            Require(actor, "SimulationBattleActorInvalid");
            if (expected < 0) throw new SimulationContractException("SimulationBattleRevisionInvalid");
            if (code != SimulationBattleInstanceCodes.Balanced
                && code != SimulationBattleInstanceCodes.Defensive
                && code != SimulationBattleInstanceCodes.Forward)
                throw new SimulationContractException("SimulationBattleDeploymentInvalid");
        }
        private static void ValidateSupport(string code, string resource, long world, long battle)
        {
            if (world < 0 || battle < 0)
                throw new SimulationContractException("SimulationBattleRevisionInvalid");
            Require(resource, "SimulationBattleSupportResourceInvalid");
            if (code != SimulationBattleInstanceCodes.SupplyCrate
                && code != SimulationBattleInstanceCodes.ReinforcementSquad)
                throw new SimulationContractException("SimulationBattleSupportCodeInvalid");
        }
        private static void ValidateAdvance(SimulationBattleAdvanceRequest value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            ValidateCommandId(value.CommandId);
            if (value.ExpectedBattleRevision < 0 || value.CombatTickCount <= 0
                || value.CombatTickCount > SimulationBattleInstanceCodes.MaximumCombatTick)
                throw new SimulationContractException("SimulationBattleAdvanceInvalid");
        }
        private static void ValidateTacticalCommand(
            SimulationBattleTacticalCommandConfirmRequest value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            ValidateCommandId(value.CommandId);
            Require(value.RequestingActorStableId, "SimulationBattleActorInvalid");
            Require(value.UnitStableId, "SimulationBattleUnitInvalid");
            if (value.ExpectedBattleRevision < 0
                || Math.Abs(value.TargetXCentimeters) > 25000
                || Math.Abs(value.TargetZCentimeters) > 25000)
                throw new SimulationContractException(
                    "SimulationBattleTacticalCommandInvalid");
            if (value.CommandCode != SimulationBattlefieldDerivationCodes.Move
                && value.CommandCode != SimulationBattlefieldDerivationCodes.Attack
                && value.CommandCode != SimulationBattlefieldDerivationCodes.Hold
                && value.CommandCode != SimulationBattlefieldDerivationCodes.Retreat
                && value.CommandCode != SimulationBattlefieldDerivationCodes.SetFormation)
                throw new SimulationContractException(
                    "SimulationBattleTacticalCommandInvalid");
            if (value.CommandCode == SimulationBattlefieldDerivationCodes.Attack)
                Require(value.TargetUnitStableId, "SimulationBattleTargetUnitInvalid");
            if (value.CommandCode == SimulationBattlefieldDerivationCodes.SetFormation)
                Require(value.FormationCode, "SimulationBattleFormationInvalid");
        }
        private static void ValidateCommandId(string value) =>
            Require(value, "SimulationCommandIdInvalid");
        private static void Require(string value, string code)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 180
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new SimulationContractException(code);
        }

        private static string Payload(SimulationBattleCreationContext value) => string.Join("~",
            value.BattleStableId.Trim(), value.SessionStableId.Trim(), value.EncounterStableId.Trim(),
            value.AreaStableId.Trim(), value.CommanderActorStableId.Trim(), value.StartedWorldTick,
            value.StartedWorldRevision, value.ScenarioSeed, value.AlliedStrength, value.HostileStrength,
            string.Join(",", value.InitialResourceStableIds.OrderBy(x => x, StringComparer.Ordinal)),
            string.Join(",", value.ReinforcementCandidateStableIds.OrderBy(x => x, StringComparer.Ordinal)),
            value.CombatSpaceCode, value.EncounterScaleCode, value.ScalePolicyRevision,
            string.Join(",", value.ScaleReasonCodes.OrderBy(x => x, StringComparer.Ordinal)),
            value.LocalWorldContext.ContextHashSha256,
            value.BattlefieldDerivation.BattlefieldDerivationInputHashSha256,
            value.BattlefieldDerivation.BattlefieldPlan.BattlefieldPlanHashSha256,
            value.UnitRoster.BattleUnitRosterHashSha256,
            value.UnitRoster.CardModifierHashSha256,
            value.UnitRoster.CombatSimulationRevision);
        private static SimulationBattleCreationContext CloneContext(SimulationBattleCreationContext value) => new()
        {
            BattleStableId = value.BattleStableId.Trim(), SessionStableId = value.SessionStableId.Trim(),
            EncounterStableId = value.EncounterStableId.Trim(), AreaStableId = value.AreaStableId.Trim(),
            CommanderActorStableId = value.CommanderActorStableId.Trim(), StartedWorldTick = value.StartedWorldTick,
            StartedWorldRevision = value.StartedWorldRevision, ScenarioSeed = value.ScenarioSeed,
            AlliedStrength = value.AlliedStrength, HostileStrength = value.HostileStrength,
            CombatSpaceCode = value.CombatSpaceCode,
            EncounterScaleCode = value.EncounterScaleCode,
            ScalePolicyRevision = value.ScalePolicyRevision,
            ScaleReasonCodes = value.ScaleReasonCodes.Select(x => x.Trim()).ToArray(),
            LocalWorldContext = CloneLocalWorldContext(value.LocalWorldContext),
            InitialResourceStableIds = value.InitialResourceStableIds.Select(x => x.Trim()).ToArray(),
            ReinforcementCandidateStableIds = value.ReinforcementCandidateStableIds.Select(x => x.Trim()).ToArray(),
            BattlefieldDerivation = SimulationBattlefieldSnapshotCloner.Derivation(
                value.BattlefieldDerivation),
            UnitRoster = SimulationBattlefieldSnapshotCloner.Roster(value.UnitRoster),
            CreateCommandId = value.CreateCommandId.Trim(),
        };
        private static SimulationBattleParticipantSnapshot CloneParticipant(SimulationBattleParticipantSnapshot v) => new()
        { ActorStableId = v.ActorStableId, ParticipationRoleCode = v.ParticipationRoleCode,
            DelegatedSquadStableId = v.DelegatedSquadStableId, CanControlWorldState = v.CanControlWorldState,
            PresentationOnly = v.PresentationOnly };
        private static SimulationBattleResourceReservationSnapshot CloneReservation(SimulationBattleResourceReservationSnapshot v) => new()
        { ResourceStableId = v.ResourceStableId, ReservationKindCode = v.ReservationKindCode,
            StateCode = v.StateCode, SourceCommandId = v.SourceCommandId };
        private static SimulationBattleSupportSnapshot CloneSupport(SimulationBattleSupportSnapshot v) => new()
        { SupportStableId = v.SupportStableId, CommandId = v.CommandId,
            RequestingActorStableId = v.RequestingActorStableId, SupportCode = v.SupportCode,
            SourceResourceStableId = v.SourceResourceStableId, ConfirmedWorldTick = v.ConfirmedWorldTick,
            ConfirmedWorldRevision = v.ConfirmedWorldRevision, ArrivesCombatTick = v.ArrivesCombatTick,
            StrengthBonus = v.StrengthBonus, StateCode = v.StateCode };
        private static SimulationBattleOutcomeSnapshot CloneOutcome(SimulationBattleOutcomeSnapshot v) => new()
        { OutcomeStableId = v.OutcomeStableId, ResultCode = v.ResultCode,
            CompletedWorldTick = v.CompletedWorldTick, AlliedStrengthDelta = v.AlliedStrengthDelta,
            RecoverableInjuryCount = v.RecoverableInjuryCount, FacilityDamageUnits = v.FacilityDamageUnits,
            SupplyLossUnits = v.SupplyLossUnits, SecurityDelta = v.SecurityDelta,
            MoraleDelta = v.MoraleDelta, UsedDeterministicAutoCommand = v.UsedDeterministicAutoCommand,
            ReconciliationStateCode = v.ReconciliationStateCode, AppliedWorldTick = v.AppliedWorldTick,
            AppliedWorldRevision = v.AppliedWorldRevision };

        public static SimulationBattleSaveRecordSnapshot CloneSaveRecord(
            SimulationBattleSaveRecordSnapshot source)
            => new SimulationBattleSaveRecordSnapshot
            {
                Creation = new SimulationBattleCreationSnapshot
                {
                    BattleStableId = source.Creation.BattleStableId,
                    SessionStableId = source.Creation.SessionStableId,
                    EncounterStableId = source.Creation.EncounterStableId,
                    AreaStableId = source.Creation.AreaStableId,
                    CommanderActorStableId = source.Creation.CommanderActorStableId,
                    StartedWorldTick = source.Creation.StartedWorldTick,
                    StartedWorldRevision = source.Creation.StartedWorldRevision,
                    ScenarioSeed = source.Creation.ScenarioSeed,
                    AlliedStrength = source.Creation.AlliedStrength,
                    HostileStrength = source.Creation.HostileStrength,
                    CombatSpaceCode = source.Creation.CombatSpaceCode,
                    EncounterScaleCode = source.Creation.EncounterScaleCode,
                    ScalePolicyRevision = source.Creation.ScalePolicyRevision,
                    ScaleReasonCodes = source.Creation.ScaleReasonCodes.ToArray(),
                    LocalWorldContext = CloneLocalWorldContext(
                        source.Creation.LocalWorldContext),
                    InitialResourceStableIds = source.Creation.InitialResourceStableIds.ToArray(),
                    ReinforcementCandidateStableIds = source.Creation
                        .ReinforcementCandidateStableIds.ToArray(),
                    BattlefieldDerivation = SimulationBattlefieldSnapshotCloner.Derivation(
                        source.Creation.BattlefieldDerivation),
                    UnitRoster = SimulationBattlefieldSnapshotCloner.Roster(
                        source.Creation.UnitRoster),
                    CreateCommandId = source.Creation.CreateCommandId,
                },
                State = Clone(source.State),
                ReplayEvents = source.ReplayEvents.ToArray(),
                AppliedCommands = source.AppliedCommands.Select(value =>
                    new SimulationBattleAppliedCommandSnapshot
                    {
                        CommandId = value.CommandId,
                        Payload = value.Payload,
                        Result = Clone(value.Result),
                    }).ToArray(),
                IntegrityHashSha256 = source.IntegrityHashSha256,
            };

        private static SimulationBattleCreationSnapshot ToCreationSnapshot(
            SimulationBattleCreationContext value) => new SimulationBattleCreationSnapshot
            {
                BattleStableId = value.BattleStableId,
                SessionStableId = value.SessionStableId,
                EncounterStableId = value.EncounterStableId,
                AreaStableId = value.AreaStableId,
                CommanderActorStableId = value.CommanderActorStableId,
                StartedWorldTick = value.StartedWorldTick,
                StartedWorldRevision = value.StartedWorldRevision,
                ScenarioSeed = value.ScenarioSeed,
                AlliedStrength = value.AlliedStrength,
                HostileStrength = value.HostileStrength,
                CombatSpaceCode = value.CombatSpaceCode,
                EncounterScaleCode = value.EncounterScaleCode,
                ScalePolicyRevision = value.ScalePolicyRevision,
                ScaleReasonCodes = value.ScaleReasonCodes.ToArray(),
                LocalWorldContext = CloneLocalWorldContext(value.LocalWorldContext),
                InitialResourceStableIds = value.InitialResourceStableIds.ToArray(),
                ReinforcementCandidateStableIds = value.ReinforcementCandidateStableIds.ToArray(),
                BattlefieldDerivation = SimulationBattlefieldSnapshotCloner.Derivation(
                    value.BattlefieldDerivation),
                UnitRoster = SimulationBattlefieldSnapshotCloner.Roster(value.UnitRoster),
                CreateCommandId = value.CreateCommandId,
            };

        private static SimulationBattleCreationContext ToCreationContext(
            SimulationBattleCreationSnapshot value)
        {
            if (value == null || value.InitialResourceStableIds == null
                || value.ReinforcementCandidateStableIds == null
                || value.BattlefieldDerivation == null || value.UnitRoster == null)
                throw new SimulationContractException("SimulationBattleSaveRecordInvalid");
            return new SimulationBattleCreationContext
            {
                BattleStableId = value.BattleStableId,
                SessionStableId = value.SessionStableId,
                EncounterStableId = value.EncounterStableId,
                AreaStableId = value.AreaStableId,
                CommanderActorStableId = value.CommanderActorStableId,
                StartedWorldTick = value.StartedWorldTick,
                StartedWorldRevision = value.StartedWorldRevision,
                ScenarioSeed = value.ScenarioSeed,
                AlliedStrength = value.AlliedStrength,
                HostileStrength = value.HostileStrength,
                CombatSpaceCode = value.CombatSpaceCode,
                EncounterScaleCode = value.EncounterScaleCode,
                ScalePolicyRevision = value.ScalePolicyRevision,
                ScaleReasonCodes = value.ScaleReasonCodes.ToArray(),
                LocalWorldContext = CloneLocalWorldContext(value.LocalWorldContext),
                InitialResourceStableIds = value.InitialResourceStableIds.ToArray(),
                ReinforcementCandidateStableIds = value.ReinforcementCandidateStableIds.ToArray(),
                BattlefieldDerivation = SimulationBattlefieldSnapshotCloner.Derivation(
                    value.BattlefieldDerivation),
                UnitRoster = SimulationBattlefieldSnapshotCloner.Roster(value.UnitRoster),
                CreateCommandId = value.CreateCommandId,
            };
        }

        private static string CalculateSaveRecordIntegrityHash(
            SimulationBattleSaveRecordSnapshot record)
        {
            var canonical = new StringBuilder();
            AddCanonical(canonical, "battle-save.v1");
            AddCreation(canonical, record.Creation);
            AddSnapshot(canonical, record.State);
            AddCanonical(canonical, record.ReplayEvents.Length);
            foreach (var value in record.ReplayEvents) AddCanonical(canonical, value);
            AddCanonical(canonical, record.AppliedCommands.Length);
            foreach (var command in record.AppliedCommands.OrderBy(value => value.CommandId,
                StringComparer.Ordinal))
            {
                AddCanonical(canonical, command.CommandId);
                AddCanonical(canonical, command.Payload);
                AddSnapshot(canonical, command.Result);
            }
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical.ToString())))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static void AddCreation(StringBuilder target,
            SimulationBattleCreationSnapshot value)
        {
            AddCanonical(target, value.BattleStableId);
            AddCanonical(target, value.SessionStableId);
            AddCanonical(target, value.EncounterStableId);
            AddCanonical(target, value.AreaStableId);
            AddCanonical(target, value.CommanderActorStableId);
            AddCanonical(target, value.StartedWorldTick);
            AddCanonical(target, value.StartedWorldRevision);
            AddCanonical(target, value.ScenarioSeed);
            AddCanonical(target, value.AlliedStrength);
            AddCanonical(target, value.HostileStrength);
            if (value.CombatSpaceCode == SimulationLocalCombatCodes.WorldLocal)
            {
                AddCanonical(target, value.CombatSpaceCode);
                AddCanonical(target, value.EncounterScaleCode);
                AddCanonical(target, value.ScalePolicyRevision);
                foreach (var reason in value.ScaleReasonCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) AddCanonical(target, reason);
                AddLocalWorldContextCanonical(target, value.LocalWorldContext);
            }
            AddCanonical(target, value.InitialResourceStableIds.Length);
            foreach (var id in value.InitialResourceStableIds) AddCanonical(target, id);
            AddCanonical(target, value.ReinforcementCandidateStableIds.Length);
            foreach (var id in value.ReinforcementCandidateStableIds) AddCanonical(target, id);
            AddCanonical(target, value.BattlefieldDerivation.WorldContext.ContextHashSha256);
            AddCanonical(target, value.BattlefieldDerivation.WorldContext.AnchorSetHashSha256);
            AddCanonical(target,
                value.BattlefieldDerivation.BattlefieldDerivationInputHashSha256);
            AddCanonical(target,
                value.BattlefieldDerivation.BattlefieldPlan.BattlefieldPlanHashSha256);
            AddCanonical(target, value.UnitRoster.CombatSimulationRevision);
            AddCanonical(target, value.UnitRoster.BattleUnitRosterHashSha256);
            AddCanonical(target, value.UnitRoster.CardModifierHashSha256);
            AddCanonical(target, value.UnitRoster.CombatSeedHashSha256);
            AddCanonical(target, value.CreateCommandId);
        }

        private static void AddSnapshot(StringBuilder target,
            SimulationBattleInstanceSnapshot value)
        {
            AddCanonical(target, value.BattleStableId);
            AddCanonical(target, value.SessionStableId);
            AddCanonical(target, value.EncounterStableId);
            AddCanonical(target, value.AreaStableId);
            AddCanonical(target, value.RuleRevision);
            if (value.CombatSpaceCode == SimulationLocalCombatCodes.WorldLocal)
            {
                AddCanonical(target, value.CombatSpaceCode);
                AddCanonical(target, value.EncounterScaleCode);
                AddCanonical(target, value.ScalePolicyRevision);
                foreach (var reason in value.ScaleReasonCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) AddCanonical(target, reason);
                AddLocalCombatCanonical(target, value.LocalCombat);
            }
            AddCanonical(target, value.PhaseCode);
            AddCanonical(target, value.BattleRevision);
            AddCanonical(target, value.CombatTick);
            AddCanonical(target, value.StartedWorldTick);
            AddCanonical(target, value.StartedWorldRevision);
            AddCanonical(target, value.AlliedStrength);
            AddCanonical(target, value.HostileStrength);
            AddCanonical(target, value.ScenarioSeed);
            AddCanonical(target, value.DeploymentCode);
            AddCanonical(target, value.Participants.Length);
            foreach (var participant in value.Participants)
            {
                AddCanonical(target, participant.ActorStableId);
                AddCanonical(target, participant.ParticipationRoleCode);
                AddCanonical(target, participant.DelegatedSquadStableId);
                AddCanonical(target, participant.CanControlWorldState);
                AddCanonical(target, participant.PresentationOnly);
            }
            AddCanonical(target, value.ResourceReservations.Length);
            foreach (var reservation in value.ResourceReservations)
            {
                AddCanonical(target, reservation.ResourceStableId);
                AddCanonical(target, reservation.ReservationKindCode);
                AddCanonical(target, reservation.StateCode);
                AddCanonical(target, reservation.SourceCommandId);
            }
            AddCanonical(target, value.Supports.Length);
            foreach (var support in value.Supports)
            {
                AddCanonical(target, support.SupportStableId);
                AddCanonical(target, support.CommandId);
                AddCanonical(target, support.RequestingActorStableId);
                AddCanonical(target, support.SupportCode);
                AddCanonical(target, support.SourceResourceStableId);
                AddCanonical(target, support.ConfirmedWorldTick);
                AddCanonical(target, support.ConfirmedWorldRevision);
                AddCanonical(target, support.ArrivesCombatTick);
                AddCanonical(target, support.StrengthBonus);
                AddCanonical(target, support.StateCode);
            }
            AddCanonical(target, value.BattlefieldDerivation.WorldContext.ContextHashSha256);
            AddCanonical(target, value.BattlefieldDerivation.WorldContext.AnchorSetHashSha256);
            AddCanonical(target,
                value.BattlefieldDerivation.BattlefieldDerivationInputHashSha256);
            AddCanonical(target,
                value.BattlefieldDerivation.BattlefieldPlan.BattlefieldPlanHashSha256);
            AddCanonical(target, value.UnitRoster.CombatSimulationRevision);
            AddCanonical(target, value.UnitRoster.BattleUnitRosterHashSha256);
            AddCanonical(target, value.UnitRoster.CardModifierHashSha256);
            AddCanonical(target, value.UnitRoster.CombatSeedHashSha256);
            AddCanonical(target, value.ParticipationReservations.Length);
            foreach (var reservation in value.ParticipationReservations)
            {
                AddCanonical(target, reservation.ActorStableId);
                AddCanonical(target, reservation.BattleStableId);
                AddCanonical(target, reservation.ReservedWorldTick);
                AddCanonical(target, reservation.EnteredBattleTick);
                AddCanonical(target, reservation.ReleasedWorldTick);
                AddCanonical(target, reservation.StateCode);
            }
            AddCanonical(target, value.WorldTargetReservations.Length);
            foreach (var reservation in value.WorldTargetReservations)
            {
                AddCanonical(target, reservation.WorldEffectTargetStableId);
                AddCanonical(target, reservation.BattleStableId);
                AddCanonical(target, reservation.ReservationKindCode);
                AddCanonical(target, reservation.ConflictCapabilityCodes.Length);
                foreach (var capability in reservation.ConflictCapabilityCodes)
                    AddCanonical(target, capability);
                AddCanonical(target, reservation.ReservedWorldTick);
                AddCanonical(target, reservation.ReleasedWorldTick);
                AddCanonical(target, reservation.StateCode);
            }
            AddCanonical(target, value.SemanticEffects.Length);
            foreach (var effect in value.SemanticEffects)
            {
                AddCanonical(target, effect.SemanticEffectStableId);
                AddCanonical(target, effect.BattleStableId);
                AddCanonical(target, effect.BattlefieldAnchorStableIds.Length);
                foreach (var anchor in effect.BattlefieldAnchorStableIds)
                    AddCanonical(target, anchor);
                AddCanonical(target, effect.WorldEffectTargetStableId);
                AddCanonical(target, effect.SemanticEffectCode);
                AddCanonical(target, effect.SeverityCode);
                AddCanonical(target, effect.TacticalEvidencePermille);
                AddCanonical(target, effect.AggregationPolicyCode);
                AddCanonical(target, effect.RuleRevision);
                AddCanonical(target, effect.ReconciliationStateCode);
                AddCanonical(target, effect.WorldEffectApplicationKey);
                AddCanonical(target, effect.AppliedWorldTick);
                AddCanonical(target, effect.AppliedWorldRevision);
            }
            AddCanonical(target, value.Outcome != null);
            if (value.Outcome != null)
            {
                AddCanonical(target, value.Outcome.OutcomeStableId);
                AddCanonical(target, value.Outcome.ResultCode);
                AddCanonical(target, value.Outcome.CompletedWorldTick);
                AddCanonical(target, value.Outcome.AlliedStrengthDelta);
                AddCanonical(target, value.Outcome.RecoverableInjuryCount);
                AddCanonical(target, value.Outcome.FacilityDamageUnits);
                AddCanonical(target, value.Outcome.SupplyLossUnits);
                AddCanonical(target, value.Outcome.SecurityDelta);
                AddCanonical(target, value.Outcome.MoraleDelta);
                AddCanonical(target, value.Outcome.UsedDeterministicAutoCommand);
                AddCanonical(target, value.Outcome.ReconciliationStateCode);
                AddCanonical(target, value.Outcome.AppliedWorldTick);
                AddCanonical(target, value.Outcome.AppliedWorldRevision);
            }
            AddCanonical(target, value.ReplayHashSha256);
            AddCanonical(target, value.SimulationOnly);
            AddCanonical(target, value.IsOperationalState);
        }

        private static void AddCanonical(StringBuilder target, object? value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture));
            target.Append(':');
            target.Append(text);
            target.Append('|');
        }

        private sealed class AppliedCommand
        {
            public AppliedCommand(string payload, SimulationBattleInstanceSnapshot snapshot)
            { Payload = payload; Snapshot = snapshot; }
            public string Payload { get; }
            public SimulationBattleInstanceSnapshot Snapshot { get; }
        }
    }
}
