using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, FarmActorState> farmActors =
            new Dictionary<string, FarmActorState>(StringComparer.Ordinal);
        private readonly Dictionary<string, FarmSoilTileState> farmSoilTiles =
            new Dictionary<string, FarmSoilTileState>(StringComparer.Ordinal);
        private readonly Dictionary<string, FarmDefenseState> farmDefenses =
            new Dictionary<string, FarmDefenseState>(StringComparer.Ordinal);
        private readonly List<FarmWorkOrderState> farmWorkOrders =
            new List<FarmWorkOrderState>();
        private readonly List<SimulationThreatEncounterSnapshot> threatEncounters =
            new List<SimulationThreatEncounterSnapshot>();
        private readonly List<SimulationSurvivalDayReportSnapshot> survivalDayReports =
            new List<SimulationSurvivalDayReportSnapshot>();
        private readonly Dictionary<string, AppliedFarmCommand> appliedFarmWorkCommands =
            new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedFarmCommand> appliedThreatResponseCommands =
            new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);

        private SimulationFarmSurvivalInitialStateRequest? farmSurvivalCreationState;
        private string farmSurvivalInitialPayloadKey = "none";
        private decimal farmSupplyUnits;
        private decimal farmRepairMaterialUnits;
        private decimal recoverableDamageUnits;

        public SimulationFarmSurvivalStateSnapshot GetFarmSurvivalState()
        {
            lock (gate)
            {
                EnsureFarmSurvivalConfigured();
                return CreateFarmSurvivalStateSnapshot();
            }
        }

        public SimulationFarmWorkPreviewSnapshot PreviewFarmWork(
            SimulationFarmWorkPreviewRequest request)
        {
            ValidateFarmWorkPreviewRequest(request);
            lock (gate)
            {
                EnsureFarmSurvivalConfigured();
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");
                return CreateFarmWorkPreview(
                    request.ActorStableId.Trim(),
                    request.TargetStableId.Trim(),
                    request.ActionCode.Trim(),
                    request.AssignmentKindCode.Trim());
            }
        }

        public SimulationFarmSurvivalStateSnapshot ConfirmFarmWork(
            SimulationFarmWorkConfirmRequest request)
        {
            ValidateFarmWorkConfirmRequest(request);
            lock (gate)
            {
                EnsureFarmSurvivalConfigured();
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildFarmWorkPayloadKey(request);
                if (appliedFarmWorkCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey,
                        StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "SimulationCommandPayloadConflict");
                    return CloneFarmSurvivalState(applied.State);
                }

                if (HasDifferentKindCommand(commandId)
                    || appliedThreatResponseCommands.ContainsKey(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");

                var preview = CreateFarmWorkPreview(
                    request.ActorStableId.Trim(),
                    request.TargetStableId.Trim(),
                    request.ActionCode.Trim(),
                    request.AssignmentKindCode.Trim());
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(
                        preview.BlockingReasonCodes.FirstOrDefault()
                        ?? "SimulationFarmWorkBlocked");

                if (preview.AssignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated)
                    settlementInitialState!.LaborReserved += preview.RequiredLabor;
                farmRepairMaterialUnits -= preview.MaterialCost;

                var workOrder = new FarmWorkOrderState
                {
                    WorkOrderStableId = "farm-work:" + commandId,
                    CommandId = commandId,
                    ActorStableId = preview.ActorStableId,
                    TargetStableId = preview.TargetStableId,
                    ActionCode = preview.ActionCode,
                    AssignmentKindCode = preview.AssignmentKindCode,
                    StatusCode = SimulationFarmSurvivalCodes.InProgress,
                    StartedWorldTick = CurrentTick,
                    CompletesWorldTick = preview.EstimatedCompletionWorldTick,
                    ReservedLabor = preview.RequiredLabor,
                    StaminaCost = preview.StaminaCost,
                    MaterialCost = preview.MaterialCost,
                    PresentationKey = preview.PresentationKey,
                };
                farmActors[preview.ActorStableId].ActiveWorkOrderStableId =
                    workOrder.WorkOrderStableId;
                farmWorkOrders.Add(workOrder);
                Revision++;
                AppendFarmWorkConfirmCommand(request);
                var state = CreateFarmSurvivalStateSnapshot();
                appliedFarmWorkCommands.Add(commandId,
                    new AppliedFarmCommand(payloadKey, CloneFarmSurvivalState(state)));
                return state;
            }
        }

        public SimulationFarmSurvivalStateSnapshot ConfirmThreatResponse(
            SimulationThreatResponseConfirmRequest request)
        {
            ValidateThreatResponseRequest(request);
            lock (gate)
            {
                EnsureFarmSurvivalConfigured();
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildThreatResponsePayloadKey(request);
                if (appliedThreatResponseCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey,
                        StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "SimulationCommandPayloadConflict");
                    return CloneFarmSurvivalState(applied.State);
                }

                if (HasDifferentKindCommand(commandId)
                    || appliedFarmWorkCommands.ContainsKey(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");
                if (!farmActors.ContainsKey(request.ActorStableId.Trim()))
                    throw new SimulationNotFoundException(
                        "SimulationFarmActorNotFound");

                var encounter = threatEncounters.SingleOrDefault(value =>
                    string.Equals(value.EncounterStableId,
                        request.EncounterStableId.Trim(), StringComparison.Ordinal))
                    ?? throw new SimulationNotFoundException(
                        "SimulationThreatEncounterNotFound");
                if (encounter.ThreatTypeCode != SimulationFarmSurvivalCodes.RaiderFaction
                    || encounter.StateCode != SimulationFarmSurvivalCodes.AwaitingResponse)
                    throw new SimulationConflictException(
                        "SimulationThreatResponseNotAllowed");

                var choice = request.ChoiceStableId.Trim();
                if (choice != SimulationFarmSurvivalCodes.Trade
                    && choice != SimulationFarmSurvivalCodes.Refuse
                    && choice != SimulationFarmSurvivalCodes.Deception)
                    throw new SimulationContractException(
                        "SimulationThreatChoiceInvalid");

                Revision++;
                ApplyRaiderChoice(encounter, choice);
                encounter.SelectedChoiceStableId = choice;
                encounter.StateCode = SimulationFarmSurvivalCodes.Resolved;
                UpdateFarmThreatWorldEvent(encounter);
                AppendThreatResponseConfirmCommand(request);
                var state = CreateFarmSurvivalStateSnapshot();
                appliedThreatResponseCommands.Add(commandId,
                    new AppliedFarmCommand(payloadKey, CloneFarmSurvivalState(state)));
                return state;
            }
        }

        private void InitializeFarmSurvival(
            SimulationFarmSurvivalInitialStateRequest? request)
        {
            ValidateFarmSurvivalInitialState(request);
            farmSurvivalInitialPayloadKey = BuildFarmSurvivalPayloadKey(request);
            farmSurvivalCreationState = CloneFarmSurvivalInitialState(request);
            if (farmSurvivalCreationState == null) return;

            farmSupplyUnits = farmSurvivalCreationState.SupplyUnits;
            farmRepairMaterialUnits = farmSurvivalCreationState.RepairMaterialUnits;
            foreach (var actor in farmSurvivalCreationState.Actors)
                farmActors.Add(actor.ActorStableId, new FarmActorState
                {
                    ActorStableId = actor.ActorStableId,
                    ActorKindCode = actor.ActorKindCode,
                    KoreanName = actor.KoreanName,
                    Health = actor.Health,
                    Stamina = actor.Stamina,
                });
            foreach (var tile in farmSurvivalCreationState.SoilTiles)
                farmSoilTiles.Add(tile.SoilTileStableId, new FarmSoilTileState
                {
                    SoilTileStableId = tile.SoilTileStableId,
                    GridX = tile.GridX,
                    GridY = tile.GridY,
                    StateCode = tile.StateCode,
                });
            foreach (var defense in farmSurvivalCreationState.Defenses)
                farmDefenses.Add(defense.DefenseStableId, new FarmDefenseState
                {
                    DefenseStableId = defense.DefenseStableId,
                    DefenseKindCode = defense.DefenseKindCode,
                    Durability = defense.Durability,
                    Prepared = defense.Prepared,
                });
        }

        private void AdvanceFarmSurvival(int previousTick, int currentTick)
        {
            if (farmSurvivalCreationState == null) return;

            foreach (var workOrder in farmWorkOrders.Where(value =>
                value.StatusCode == SimulationFarmSurvivalCodes.InProgress
                && value.CompletesWorldTick <= currentTick)
                .OrderBy(value => value.CompletesWorldTick)
                .ThenBy(value => value.WorkOrderStableId, StringComparer.Ordinal))
            {
                CompleteFarmWork(workOrder);
            }

            if (previousTick < 4 && currentTick >= 4)
                CreateZombieWarning();
            if (previousTick < 5 && currentTick >= 5)
            {
                ResolveZombieEncounter();
                CreateRaiderApproach();
            }
            if (previousTick < 6 && currentTick >= 6)
                CreateDamageAssessmentReport();
        }

        private SimulationFarmWorkPreviewSnapshot CreateFarmWorkPreview(
            string actorStableId,
            string targetStableId,
            string actionCode,
            string assignmentKindCode)
        {
            var blocks = new List<string>();
            if (!farmActors.TryGetValue(actorStableId, out var actor))
                blocks.Add("SimulationFarmActorNotFound");
            if (assignmentKindCode != SimulationFarmSurvivalCodes.PlayerDirect
                && assignmentKindCode != SimulationFarmSurvivalCodes.NpcDelegated)
                blocks.Add("SimulationFarmAssignmentKindInvalid");
            if (actor != null
                && ((assignmentKindCode == SimulationFarmSurvivalCodes.PlayerDirect
                        && actor.ActorKindCode != SimulationFarmSurvivalCodes.Player)
                    || (assignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated
                        && actor.ActorKindCode != SimulationFarmSurvivalCodes.Npc)))
                blocks.Add("SimulationFarmActorAssignmentMismatch");
            if (actor != null && !string.IsNullOrWhiteSpace(
                actor.ActiveWorkOrderStableId))
                blocks.Add("SimulationFarmActorBusy");

            var duration = assignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated
                ? 2 : 1;
            var requiredLabor = assignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated
                ? 3m : 0m;
            var staminaCost = assignmentKindCode == SimulationFarmSurvivalCodes.PlayerDirect
                ? 15m : 8m;
            var materialCost = 0m;

            if (actionCode == SimulationFarmSurvivalCodes.Tilling)
            {
                if (!farmSoilTiles.TryGetValue(targetStableId, out var soil))
                    blocks.Add("SimulationFarmSoilTileNotFound");
                else if (soil.StateCode != SimulationFarmSurvivalCodes.Untilled)
                    blocks.Add("SimulationFarmSoilTileAlreadyTilled");
            }
            else if (actionCode == SimulationFarmSurvivalCodes.MedicalRest)
            {
                if (!farmActors.TryGetValue(targetStableId, out var recoveringActor))
                    blocks.Add("SimulationFarmActorNotFound");
                else if (!string.Equals(actorStableId, targetStableId,
                    StringComparison.Ordinal))
                    blocks.Add("SimulationFarmMedicalRestActorMismatch");
                else if (!recoveringActor.Injured)
                    blocks.Add("SimulationFarmActorNotInjured");
                staminaCost = 0m;
                materialCost = 1m;
                duration = 1;
            }
            else if (actionCode == SimulationFarmSurvivalCodes.FenceRepair
                || actionCode == SimulationFarmSurvivalCodes.StorageSecuring
                || actionCode == SimulationFarmSurvivalCodes.LightingPreparation
                || actionCode == SimulationFarmSurvivalCodes.GuardDuty)
            {
                if (!farmDefenses.TryGetValue(targetStableId, out var defense))
                    blocks.Add("SimulationFarmDefenseNotFound");
                else if (!MatchesDefenseAction(actionCode, defense.DefenseKindCode))
                    blocks.Add("SimulationFarmDefenseActionMismatch");
                materialCost = actionCode == SimulationFarmSurvivalCodes.GuardDuty ? 0m : 1m;
                staminaCost = assignmentKindCode == SimulationFarmSurvivalCodes.PlayerDirect
                    ? 10m : 6m;
            }
            else
            {
                blocks.Add("SimulationFarmActionInvalid");
            }

            if (actor != null && actor.Stamina < staminaCost)
                blocks.Add("SimulationFarmActorStaminaInsufficient");
            if (farmRepairMaterialUnits < materialCost)
                blocks.Add("SimulationFarmRepairMaterialInsufficient");
            if (assignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated)
            {
                if (settlementInitialState == null)
                    blocks.Add("SimulationSettlementRequiredForNpcLabor");
                else if (settlementInitialState.LaborCapacityTotal
                    - settlementInitialState.LaborReserved < requiredLabor)
                    blocks.Add("SimulationSettlementLaborInsufficient");
            }

            return new SimulationFarmWorkPreviewSnapshot
            {
                ActorStableId = actorStableId,
                TargetStableId = targetStableId,
                ActionCode = actionCode,
                AssignmentKindCode = assignmentKindCode,
                RequiredLabor = requiredLabor,
                StaminaCost = staminaCost,
                MaterialCost = materialCost,
                DurationTicks = duration,
                EstimatedCompletionWorldTick = CurrentTick + duration,
                PresentationKey = FarmWorkPresentationKey(actionCode),
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.Distinct(StringComparer.Ordinal).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private void CompleteFarmWork(FarmWorkOrderState workOrder)
        {
            workOrder.StatusCode = SimulationFarmSurvivalCodes.Completed;
            var actor = farmActors[workOrder.ActorStableId];
            actor.Stamina = Math.Max(0m, actor.Stamina - workOrder.StaminaCost);
            actor.ActiveWorkOrderStableId = string.Empty;
            if (workOrder.AssignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated)
                settlementInitialState!.LaborReserved -= workOrder.ReservedLabor;

            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.Tilling)
            {
                farmSoilTiles[workOrder.TargetStableId].StateCode =
                    SimulationFarmSurvivalCodes.Tilled;
                EvaluateCollectibleRewardForFarmCompletion(workOrder);
                return;
            }
            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.MedicalRest)
            {
                actor.Health = Math.Min(100m, actor.Health + 20m);
                actor.Injured = false;
                return;
            }

            var defense = farmDefenses[workOrder.TargetStableId];
            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.FenceRepair)
            {
                var repaired = Math.Min(25m, 100m - defense.Durability);
                defense.Durability += repaired;
                recoverableDamageUnits = Math.Max(0m,
                    recoverableDamageUnits - repaired);
            }
            defense.Prepared = true;
        }

        private void CreateZombieWarning()
        {
            if (threatEncounters.Any(value =>
                value.ThreatTypeCode == SimulationFarmSurvivalCodes.ZombiePressure)) return;
            var encounter = new SimulationThreatEncounterSnapshot
            {
                EncounterStableId = "encounter:zombie:" + SessionStableId + ":day-5",
                ThreatTypeCode = SimulationFarmSurvivalCodes.ZombiePressure,
                ThreatUnitCount = 3,
                StateCode = SimulationFarmSurvivalCodes.Warning,
                OccurredWorldTick = 4,
                PresentationKey = SimulationFarmSurvivalCodes.ZombieWarningPresentation,
                Recoverable = true,
            };
            threatEncounters.Add(encounter);
            RegisterFarmThreatWorldEvent(encounter, Array.Empty<SimulationWorldEventChoiceSnapshot>());
        }

        private void ResolveZombieEncounter()
        {
            var encounter = threatEncounters.SingleOrDefault(value =>
                value.ThreatTypeCode == SimulationFarmSurvivalCodes.ZombiePressure);
            if (encounter == null || encounter.StateCode == SimulationFarmSurvivalCodes.Resolved)
                return;

            var score = DefensePreparednessScore();
            if (score >= 2)
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
                    fence.Durability = Math.Max(0m, fence.Durability - encounter.DamageUnits);
                var injured = farmActors.Values.OrderBy(value => value.ActorStableId,
                    StringComparer.Ordinal).FirstOrDefault();
                if (injured != null && ScenarioSeed % 2 != 0)
                {
                    injured.Health = Math.Max(1m, injured.Health - 15m);
                    injured.Injured = true;
                    encounter.InjuredActorStableId = injured.ActorStableId;
                }
                encounter.OutcomeCode = encounter.SupplyLossUnits > 0m
                    ? SimulationFarmSurvivalCodes.InventoryTaken
                    : SimulationFarmSurvivalCodes.FacilityDamaged;
            }
            encounter.StateCode = SimulationFarmSurvivalCodes.Resolved;
            encounter.PresentationKey =
                SimulationFarmSurvivalCodes.DamageAssessmentPresentation;
            UpdateFarmThreatWorldEvent(encounter);
        }

        private void CreateRaiderApproach()
        {
            if (threatEncounters.Any(value =>
                value.ThreatTypeCode == SimulationFarmSurvivalCodes.RaiderFaction)) return;
            var encounter = new SimulationThreatEncounterSnapshot
            {
                EncounterStableId = "encounter:raider:" + SessionStableId + ":day-6",
                ThreatTypeCode = SimulationFarmSurvivalCodes.RaiderFaction,
                ThreatUnitCount = 1,
                StateCode = SimulationFarmSurvivalCodes.AwaitingResponse,
                OccurredWorldTick = 5,
                PresentationKey = SimulationFarmSurvivalCodes.RaiderApproachPresentation,
                Recoverable = true,
            };
            threatEncounters.Add(encounter);
            RegisterFarmThreatWorldEvent(encounter, new[]
            {
                ThreatChoice(SimulationFarmSurvivalCodes.Trade, 1,
                    "교환한다", "식량 일부를 내주고 충돌을 피한다."),
                ThreatChoice(SimulationFarmSurvivalCodes.Refuse, 2,
                    "거절한다", "준비한 방어 시설로 물러나게 한다."),
                ThreatChoice(SimulationFarmSurvivalCodes.Deception, 3,
                    "속임수를 쓴다", "위험을 감수하고 보급품을 지킨다."),
            });
        }

        private void ApplyRaiderChoice(
            SimulationThreatEncounterSnapshot encounter,
            string choice)
        {
            if (choice == SimulationFarmSurvivalCodes.Trade)
            {
                encounter.SupplyLossUnits = Math.Min(2m, farmSupplyUnits);
                farmSupplyUnits -= encounter.SupplyLossUnits;
                encounter.OutcomeCode = SimulationFarmSurvivalCodes.TradeCompleted;
                return;
            }

            var succeeds = choice == SimulationFarmSurvivalCodes.Refuse
                ? DefensePreparednessScore() >= 2
                : Math.Abs(ScenarioSeed + CurrentTick) % 2 == 0;
            if (succeeds)
            {
                encounter.OutcomeCode = SimulationFarmSurvivalCodes.ThreatRepelled;
                return;
            }

            encounter.SupplyLossUnits = Math.Min(1m, farmSupplyUnits);
            farmSupplyUnits -= encounter.SupplyLossUnits;
            encounter.DamageUnits = 10m;
            recoverableDamageUnits += encounter.DamageUnits;
            encounter.OutcomeCode = encounter.SupplyLossUnits > 0m
                ? SimulationFarmSurvivalCodes.InventoryTaken
                : SimulationFarmSurvivalCodes.FacilityDamaged;
        }

        private void CreateDamageAssessmentReport()
        {
            if (survivalDayReports.Any(value => value.DayNumber == 7)) return;
            var encounterIds = threatEncounters.Select(value => value.EncounterStableId)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            survivalDayReports.Add(new SimulationSurvivalDayReportSnapshot
            {
                DayNumber = 7,
                ReportCode = recoverableDamageUnits > 0m
                    ? "RepairRequired" : "FarmSecured",
                KoreanSummary = recoverableDamageUnits > 0m
                    ? "복구 가능한 시설 피해가 남아 있습니다."
                    : "첫 주 방어와 농장 준비를 마쳤습니다.",
                SourceStableIds = encounterIds,
            });
        }

        private int DefensePreparednessScore()
            => farmDefenses.Values.Count(value => value.Prepared
                && value.Durability > 0m);

        private SimulationFarmSurvivalStateSnapshot CreateFarmSurvivalStateSnapshot()
        {
            EnsureFarmSurvivalConfigured();
            var creation = farmSurvivalCreationState!;
            return new SimulationFarmSurvivalStateSnapshot
            {
                SessionStableId = SessionStableId,
                WorldRevision = Revision,
                WorldTick = CurrentTick,
                DayNumber = CurrentTick + 1,
                ChapterDayNumber = CurrentTick % 28 + 1,
                RuleRevision = creation.RuleRevision,
                RegionStableId = creation.RegionStableId,
                AreaStableId = creation.AreaStableId,
                TileKey = creation.TileKey,
                FarmBuildingStableId = creation.FarmBuildingStableId,
                DayGoalCode = DayGoalCode(CurrentTick + 1),
                SupplyUnits = farmSupplyUnits,
                RepairMaterialUnits = farmRepairMaterialUnits,
                RecoverableDamageUnits = recoverableDamageUnits,
                Actors = farmActors.Values.OrderBy(value => value.ActorStableId,
                    StringComparer.Ordinal).Select(value => new SimulationFarmActorSnapshot
                    {
                        ActorStableId = value.ActorStableId,
                        ActorKindCode = value.ActorKindCode,
                        KoreanName = value.KoreanName,
                        Health = value.Health,
                        Stamina = value.Stamina,
                        Injured = value.Injured,
                        ActiveWorkOrderStableId = value.ActiveWorkOrderStableId,
                    }).ToArray(),
                SoilTiles = farmSoilTiles.Values.OrderBy(value => value.SoilTileStableId,
                    StringComparer.Ordinal).Select(value => new SimulationFarmSoilTileSnapshot
                    {
                        SoilTileStableId = value.SoilTileStableId,
                        GridX = value.GridX,
                        GridY = value.GridY,
                        StateCode = value.StateCode,
                    }).ToArray(),
                Defenses = farmDefenses.Values.OrderBy(value => value.DefenseStableId,
                    StringComparer.Ordinal).Select(value => new SimulationFarmDefenseSnapshot
                    {
                        DefenseStableId = value.DefenseStableId,
                        DefenseKindCode = value.DefenseKindCode,
                        Durability = value.Durability,
                        Prepared = value.Prepared,
                        PresentationKey = DefensePresentationKey(value.DefenseKindCode),
                    }).ToArray(),
                WorkOrders = farmWorkOrders.OrderBy(value => value.WorkOrderStableId,
                    StringComparer.Ordinal).Select(value => new SimulationFarmWorkOrderSnapshot
                    {
                        WorkOrderStableId = value.WorkOrderStableId,
                        CommandId = value.CommandId,
                        ActorStableId = value.ActorStableId,
                        TargetStableId = value.TargetStableId,
                        ActionCode = value.ActionCode,
                        AssignmentKindCode = value.AssignmentKindCode,
                        StatusCode = value.StatusCode,
                        StartedWorldTick = value.StartedWorldTick,
                        CompletesWorldTick = value.CompletesWorldTick,
                        ReservedLabor = value.ReservedLabor,
                        StaminaCost = value.StaminaCost,
                        MaterialCost = value.MaterialCost,
                        PresentationKey = value.PresentationKey,
                    }).ToArray(),
                Encounters = threatEncounters.Select(CloneThreatEncounter).ToArray(),
                DayReports = survivalDayReports.Select(CloneDayReport).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        internal static SimulationFarmSurvivalStateSnapshot?
            CloneFarmSurvivalStateOrNull(SimulationFarmSurvivalStateSnapshot? source)
            => source == null ? null : CloneFarmSurvivalState(source);

        internal static SimulationFarmSurvivalStateSnapshot CloneFarmSurvivalState(
            SimulationFarmSurvivalStateSnapshot source)
            => new SimulationFarmSurvivalStateSnapshot
            {
                SessionStableId = source.SessionStableId,
                WorldRevision = source.WorldRevision,
                WorldTick = source.WorldTick,
                DayNumber = source.DayNumber,
                ChapterDayNumber = source.ChapterDayNumber,
                RuleRevision = source.RuleRevision,
                RegionStableId = source.RegionStableId,
                AreaStableId = source.AreaStableId,
                TileKey = source.TileKey,
                FarmBuildingStableId = source.FarmBuildingStableId,
                DayGoalCode = source.DayGoalCode,
                SupplyUnits = source.SupplyUnits,
                RepairMaterialUnits = source.RepairMaterialUnits,
                RecoverableDamageUnits = source.RecoverableDamageUnits,
                Actors = source.Actors.Select(value => new SimulationFarmActorSnapshot
                {
                    ActorStableId = value.ActorStableId,
                    ActorKindCode = value.ActorKindCode,
                    KoreanName = value.KoreanName,
                    Health = value.Health,
                    Stamina = value.Stamina,
                    Injured = value.Injured,
                    ActiveWorkOrderStableId = value.ActiveWorkOrderStableId,
                }).ToArray(),
                SoilTiles = source.SoilTiles.Select(value => new SimulationFarmSoilTileSnapshot
                {
                    SoilTileStableId = value.SoilTileStableId,
                    GridX = value.GridX,
                    GridY = value.GridY,
                    StateCode = value.StateCode,
                }).ToArray(),
                Defenses = source.Defenses.Select(value => new SimulationFarmDefenseSnapshot
                {
                    DefenseStableId = value.DefenseStableId,
                    DefenseKindCode = value.DefenseKindCode,
                    Durability = value.Durability,
                    Prepared = value.Prepared,
                    PresentationKey = value.PresentationKey,
                }).ToArray(),
                WorkOrders = source.WorkOrders.Select(value => new SimulationFarmWorkOrderSnapshot
                {
                    WorkOrderStableId = value.WorkOrderStableId,
                    CommandId = value.CommandId,
                    ActorStableId = value.ActorStableId,
                    TargetStableId = value.TargetStableId,
                    ActionCode = value.ActionCode,
                    AssignmentKindCode = value.AssignmentKindCode,
                    StatusCode = value.StatusCode,
                    StartedWorldTick = value.StartedWorldTick,
                    CompletesWorldTick = value.CompletesWorldTick,
                    ReservedLabor = value.ReservedLabor,
                    StaminaCost = value.StaminaCost,
                    MaterialCost = value.MaterialCost,
                    PresentationKey = value.PresentationKey,
                }).ToArray(),
                Encounters = source.Encounters.Select(CloneThreatEncounter).ToArray(),
                DayReports = source.DayReports.Select(CloneDayReport).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        internal static SimulationFarmSurvivalInitialStateRequest?
            CloneFarmSurvivalInitialState(SimulationFarmSurvivalInitialStateRequest? source)
        {
            if (source == null) return null;
            return new SimulationFarmSurvivalInitialStateRequest
            {
                RuleRevision = source.RuleRevision.Trim(),
                RegionStableId = source.RegionStableId.Trim(),
                AreaStableId = source.AreaStableId.Trim(),
                TileKey = source.TileKey.Trim(),
                FarmBuildingStableId = source.FarmBuildingStableId.Trim(),
                SupplyUnits = source.SupplyUnits,
                RepairMaterialUnits = source.RepairMaterialUnits,
                Actors = source.Actors.Select(value => new SimulationFarmActorInitialStateRequest
                {
                    ActorStableId = value.ActorStableId.Trim(),
                    ActorKindCode = value.ActorKindCode.Trim(),
                    KoreanName = value.KoreanName.Trim(),
                    Health = value.Health,
                    Stamina = value.Stamina,
                }).OrderBy(value => value.ActorStableId, StringComparer.Ordinal).ToArray(),
                SoilTiles = source.SoilTiles.Select(value =>
                    new SimulationFarmSoilTileInitialStateRequest
                    {
                        SoilTileStableId = value.SoilTileStableId.Trim(),
                        GridX = value.GridX,
                        GridY = value.GridY,
                        StateCode = value.StateCode.Trim(),
                    }).OrderBy(value => value.SoilTileStableId, StringComparer.Ordinal).ToArray(),
                Defenses = source.Defenses.Select(value =>
                    new SimulationFarmDefenseInitialStateRequest
                    {
                        DefenseStableId = value.DefenseStableId.Trim(),
                        DefenseKindCode = value.DefenseKindCode.Trim(),
                        Durability = value.Durability,
                        Prepared = value.Prepared,
                    }).OrderBy(value => value.DefenseStableId, StringComparer.Ordinal).ToArray(),
            };
        }

        internal static string BuildFarmSurvivalPayloadKey(
            SimulationFarmSurvivalInitialStateRequest? source)
        {
            if (source == null) return "none";
            var value = CloneFarmSurvivalInitialState(source)!;
            var key = new StringBuilder();
            AddFarmKey(key, value.RuleRevision);
            AddFarmKey(key, value.RegionStableId);
            AddFarmKey(key, value.AreaStableId);
            AddFarmKey(key, value.TileKey);
            AddFarmKey(key, value.FarmBuildingStableId);
            AddFarmKey(key, value.SupplyUnits);
            AddFarmKey(key, value.RepairMaterialUnits);
            foreach (var actor in value.Actors)
            {
                AddFarmKey(key, actor.ActorStableId);
                AddFarmKey(key, actor.ActorKindCode);
                AddFarmKey(key, actor.KoreanName);
                AddFarmKey(key, actor.Health);
                AddFarmKey(key, actor.Stamina);
            }
            foreach (var soil in value.SoilTiles)
            {
                AddFarmKey(key, soil.SoilTileStableId);
                AddFarmKey(key, soil.GridX);
                AddFarmKey(key, soil.GridY);
                AddFarmKey(key, soil.StateCode);
            }
            foreach (var defense in value.Defenses)
            {
                AddFarmKey(key, defense.DefenseStableId);
                AddFarmKey(key, defense.DefenseKindCode);
                AddFarmKey(key, defense.Durability);
                AddFarmKey(key, defense.Prepared);
            }
            return key.ToString();
        }

        internal static string BuildFarmSurvivalStatePayloadKey(
            SimulationFarmSurvivalStateSnapshot? value)
        {
            if (value == null) return "none";
            var key = new StringBuilder();
            AddFarmKey(key, value.SessionStableId);
            AddFarmKey(key, value.WorldRevision);
            AddFarmKey(key, value.WorldTick);
            AddFarmKey(key, value.DayNumber);
            AddFarmKey(key, value.ChapterDayNumber);
            AddFarmKey(key, value.RuleRevision);
            AddFarmKey(key, value.RegionStableId);
            AddFarmKey(key, value.AreaStableId);
            AddFarmKey(key, value.TileKey);
            AddFarmKey(key, value.FarmBuildingStableId);
            AddFarmKey(key, value.DayGoalCode);
            AddFarmKey(key, value.SupplyUnits);
            AddFarmKey(key, value.RepairMaterialUnits);
            AddFarmKey(key, value.RecoverableDamageUnits);
            foreach (var actor in value.Actors)
            {
                AddFarmKey(key, actor.ActorStableId);
                AddFarmKey(key, actor.ActorKindCode);
                AddFarmKey(key, actor.KoreanName);
                AddFarmKey(key, actor.Health);
                AddFarmKey(key, actor.Stamina);
                AddFarmKey(key, actor.Injured);
                AddFarmKey(key, actor.ActiveWorkOrderStableId);
            }
            foreach (var soil in value.SoilTiles)
            {
                AddFarmKey(key, soil.SoilTileStableId);
                AddFarmKey(key, soil.GridX);
                AddFarmKey(key, soil.GridY);
                AddFarmKey(key, soil.StateCode);
            }
            foreach (var defense in value.Defenses)
            {
                AddFarmKey(key, defense.DefenseStableId);
                AddFarmKey(key, defense.DefenseKindCode);
                AddFarmKey(key, defense.Durability);
                AddFarmKey(key, defense.Prepared);
                AddFarmKey(key, defense.PresentationKey);
            }
            foreach (var work in value.WorkOrders)
            {
                AddFarmKey(key, work.WorkOrderStableId);
                AddFarmKey(key, work.CommandId);
                AddFarmKey(key, work.ActorStableId);
                AddFarmKey(key, work.TargetStableId);
                AddFarmKey(key, work.ActionCode);
                AddFarmKey(key, work.AssignmentKindCode);
                AddFarmKey(key, work.StatusCode);
                AddFarmKey(key, work.StartedWorldTick);
                AddFarmKey(key, work.CompletesWorldTick);
                AddFarmKey(key, work.ReservedLabor);
                AddFarmKey(key, work.StaminaCost);
                AddFarmKey(key, work.MaterialCost);
                AddFarmKey(key, work.PresentationKey);
            }
            foreach (var encounter in value.Encounters)
            {
                AddFarmKey(key, encounter.EncounterStableId);
                AddFarmKey(key, encounter.ThreatTypeCode);
                AddFarmKey(key, encounter.ThreatUnitCount);
                AddFarmKey(key, encounter.StateCode);
                AddFarmKey(key, encounter.OccurredWorldTick);
                AddFarmKey(key, encounter.PresentationKey);
                AddFarmKey(key, encounter.SelectedChoiceStableId);
                AddFarmKey(key, encounter.OutcomeCode);
                AddFarmKey(key, encounter.SupplyLossUnits);
                AddFarmKey(key, encounter.DamageUnits);
                AddFarmKey(key, encounter.InjuredActorStableId);
                AddFarmKey(key, encounter.Recoverable);
            }
            foreach (var report in value.DayReports)
            {
                AddFarmKey(key, report.DayNumber);
                AddFarmKey(key, report.ReportCode);
                AddFarmKey(key, report.KoreanSummary);
                foreach (var id in report.SourceStableIds)
                    AddFarmKey(key, id);
            }
            return key.ToString();
        }

        internal static void ValidateFarmSurvivalInitialState(
            SimulationFarmSurvivalInitialStateRequest? request)
        {
            if (request == null) return;
            if (request.RuleRevision != SimulationFarmSurvivalCodes.RuleRevision)
                throw new SimulationContractException(
                    "SimulationFarmSurvivalRuleUnsupported");
            RequireStableId(request.RegionStableId, "SimulationFarmRegionInvalid");
            RequireStableId(request.AreaStableId, "SimulationFarmAreaInvalid");
            RequireStableId(request.TileKey, "SimulationFarmTileInvalid");
            RequireStableId(request.FarmBuildingStableId,
                "SimulationFarmBuildingInvalid");
            if (request.SupplyUnits < 0m || request.RepairMaterialUnits < 0m
                || request.Actors == null || request.SoilTiles == null
                || request.Defenses == null || request.Actors.Length == 0)
                throw new SimulationContractException(
                    "SimulationFarmSurvivalInitialStateInvalid");

            EnsureUniqueFarmIds(request.Actors.Select(value => value.ActorStableId),
                "SimulationFarmActorInvalid");
            EnsureUniqueFarmIds(request.SoilTiles.Select(value => value.SoilTileStableId),
                "SimulationFarmSoilTileInvalid");
            EnsureUniqueFarmIds(request.Defenses.Select(value => value.DefenseStableId),
                "SimulationFarmDefenseInvalid");
            foreach (var actor in request.Actors)
            {
                if ((actor.ActorKindCode != SimulationFarmSurvivalCodes.Player
                        && actor.ActorKindCode != SimulationFarmSurvivalCodes.Npc)
                    || string.IsNullOrWhiteSpace(actor.KoreanName)
                    || actor.Health <= 0m || actor.Health > 100m
                    || actor.Stamina < 0m || actor.Stamina > 100m)
                    throw new SimulationContractException(
                        "SimulationFarmActorInvalid");
            }
            if (!request.Actors.Any(value =>
                value.ActorKindCode == SimulationFarmSurvivalCodes.Player))
                throw new SimulationContractException(
                    "SimulationFarmPlayerMissing");
            foreach (var soil in request.SoilTiles)
            {
                if (soil.StateCode != SimulationFarmSurvivalCodes.Untilled
                    && soil.StateCode != SimulationFarmSurvivalCodes.Tilled)
                    throw new SimulationContractException(
                        "SimulationFarmSoilStateInvalid");
            }
            foreach (var defense in request.Defenses)
            {
                if (!IsDefenseKind(defense.DefenseKindCode)
                    || defense.Durability < 0m || defense.Durability > 100m)
                    throw new SimulationContractException(
                        "SimulationFarmDefenseInvalid");
            }
        }

        internal static void ValidateFarmWorkConfirmRequest(
            SimulationFarmWorkConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            ValidateFarmWorkFields(request.ExpectedRevision, request.ActorStableId,
                request.TargetStableId, request.ActionCode, request.AssignmentKindCode);
        }

        internal static void ValidateThreatResponseRequest(
            SimulationThreatResponseConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
            RequireStableId(request.EncounterStableId,
                "SimulationThreatEncounterInvalid");
            RequireStableId(request.ActorStableId,
                "SimulationFarmActorInvalid");
            RequireStableId(request.ChoiceStableId,
                "SimulationThreatChoiceInvalid");
        }

        internal static string BuildFarmWorkPayloadKey(
            SimulationFarmWorkConfirmRequest request)
            => string.Join("|", request.ActorStableId.Trim(),
                request.TargetStableId.Trim(), request.ActionCode.Trim(),
                request.AssignmentKindCode.Trim());

        internal static string BuildThreatResponsePayloadKey(
            SimulationThreatResponseConfirmRequest request)
            => string.Join("|", request.EncounterStableId.Trim(),
                request.ActorStableId.Trim(), request.ChoiceStableId.Trim());

        private static void ValidateFarmWorkPreviewRequest(
            SimulationFarmWorkPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateFarmWorkFields(request.ExpectedRevision, request.ActorStableId,
                request.TargetStableId, request.ActionCode, request.AssignmentKindCode);
        }

        private static void ValidateFarmWorkFields(
            long expectedRevision,
            string actorStableId,
            string targetStableId,
            string actionCode,
            string assignmentKindCode)
        {
            if (expectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
            RequireStableId(actorStableId, "SimulationFarmActorInvalid");
            RequireStableId(targetStableId, "SimulationFarmTargetInvalid");
            RequireStableId(actionCode, "SimulationFarmActionInvalid");
            RequireStableId(assignmentKindCode,
                "SimulationFarmAssignmentKindInvalid");
        }

        private void EnsureFarmSurvivalConfigured()
        {
            if (farmSurvivalCreationState == null)
                throw new SimulationConflictException(
                    "SimulationFarmSurvivalNotConfigured");
        }

        private bool HasAppliedFarmSurvivalCommand(string commandId)
            => appliedFarmWorkCommands.ContainsKey(commandId)
                || appliedThreatResponseCommands.ContainsKey(commandId);

        private static void EnsureUniqueFarmIds(
            IEnumerable<string> values,
            string errorCode)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                RequireStableId(value, errorCode);
                if (!ids.Add(value.Trim()))
                    throw new SimulationContractException(errorCode);
            }
        }

        private static bool IsDefenseKind(string value)
            => value == SimulationFarmSurvivalCodes.Fence
                || value == SimulationFarmSurvivalCodes.StorageLock
                || value == SimulationFarmSurvivalCodes.Lighting
                || value == SimulationFarmSurvivalCodes.GuardPost;

        private static bool MatchesDefenseAction(string actionCode, string defenseKindCode)
            => (actionCode == SimulationFarmSurvivalCodes.FenceRepair
                    && defenseKindCode == SimulationFarmSurvivalCodes.Fence)
                || (actionCode == SimulationFarmSurvivalCodes.StorageSecuring
                    && defenseKindCode == SimulationFarmSurvivalCodes.StorageLock)
                || (actionCode == SimulationFarmSurvivalCodes.LightingPreparation
                    && defenseKindCode == SimulationFarmSurvivalCodes.Lighting)
                || (actionCode == SimulationFarmSurvivalCodes.GuardDuty
                    && defenseKindCode == SimulationFarmSurvivalCodes.GuardPost);

        private static string FarmWorkPresentationKey(string actionCode)
            => actionCode == SimulationFarmSurvivalCodes.Tilling
                ? SimulationFarmSurvivalCodes.DayFarm
                : actionCode == SimulationFarmSurvivalCodes.MedicalRest
                    ? "survival.actor.recovery"
                : actionCode == SimulationFarmSurvivalCodes.GuardDuty
                    ? SimulationFarmSurvivalCodes.DuskDefense
                    : SimulationFarmSurvivalCodes.TacticalLabor;

        private static string DefensePresentationKey(string defenseKindCode)
            => "survival.defense." + defenseKindCode.ToLowerInvariant();

        private static string DayGoalCode(int dayNumber)
        {
            switch (dayNumber)
            {
                case 1: return "DirectAndDelegatedTilling";
                case 2: return "DefensePriority";
                case 3: return "ExternalExpeditionTarot";
                case 4: return "AdjacentTileExploration";
                case 5: return "ZombieDefense";
                case 6: return "RaiderNegotiation";
                case 7: return "RepairAndReport";
                default: return "SeasonalSurvival";
            }
        }

        private static SimulationWorldEventChoiceSnapshot ThreatChoice(
            string stableId,
            int order,
            string title,
            string summary)
            => new SimulationWorldEventChoiceSnapshot
            {
                ChoiceStableId = stableId,
                DisplayOrder = order,
                KoreanTitle = title,
                KoreanSummary = summary,
            };

        private static SimulationThreatEncounterSnapshot CloneThreatEncounter(
            SimulationThreatEncounterSnapshot source)
            => new SimulationThreatEncounterSnapshot
            {
                EncounterStableId = source.EncounterStableId,
                ThreatTypeCode = source.ThreatTypeCode,
                ThreatUnitCount = source.ThreatUnitCount,
                StateCode = source.StateCode,
                OccurredWorldTick = source.OccurredWorldTick,
                PresentationKey = source.PresentationKey,
                SelectedChoiceStableId = source.SelectedChoiceStableId,
                OutcomeCode = source.OutcomeCode,
                SupplyLossUnits = source.SupplyLossUnits,
                DamageUnits = source.DamageUnits,
                InjuredActorStableId = source.InjuredActorStableId,
                Recoverable = source.Recoverable,
            };

        private static SimulationSurvivalDayReportSnapshot CloneDayReport(
            SimulationSurvivalDayReportSnapshot source)
            => new SimulationSurvivalDayReportSnapshot
            {
                DayNumber = source.DayNumber,
                ReportCode = source.ReportCode,
                KoreanSummary = source.KoreanSummary,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static void AddFarmKey(StringBuilder target, object value)
        {
            var text = value is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString();
            target.Append(text?.Length ?? 0).Append(':').Append(text).Append('|');
        }

        private sealed class FarmActorState
        {
            public string ActorStableId { get; set; } = string.Empty;
            public string ActorKindCode { get; set; } = string.Empty;
            public string KoreanName { get; set; } = string.Empty;
            public decimal Health { get; set; }
            public decimal Stamina { get; set; }
            public bool Injured { get; set; }
            public string ActiveWorkOrderStableId { get; set; } = string.Empty;
        }

        private sealed class FarmSoilTileState
        {
            public string SoilTileStableId { get; set; } = string.Empty;
            public int GridX { get; set; }
            public int GridY { get; set; }
            public string StateCode { get; set; } = string.Empty;
        }

        private sealed class FarmDefenseState
        {
            public string DefenseStableId { get; set; } = string.Empty;
            public string DefenseKindCode { get; set; } = string.Empty;
            public decimal Durability { get; set; }
            public bool Prepared { get; set; }
        }

        private sealed class FarmWorkOrderState
        {
            public string WorkOrderStableId { get; set; } = string.Empty;
            public string CommandId { get; set; } = string.Empty;
            public string ActorStableId { get; set; } = string.Empty;
            public string TargetStableId { get; set; } = string.Empty;
            public string ActionCode { get; set; } = string.Empty;
            public string AssignmentKindCode { get; set; } = string.Empty;
            public string StatusCode { get; set; } = string.Empty;
            public int StartedWorldTick { get; set; }
            public int CompletesWorldTick { get; set; }
            public decimal ReservedLabor { get; set; }
            public decimal StaminaCost { get; set; }
            public decimal MaterialCost { get; set; }
            public string PresentationKey { get; set; } = string.Empty;
        }

        private sealed class AppliedFarmCommand
        {
            public AppliedFarmCommand(
                string payloadKey,
                SimulationFarmSurvivalStateSnapshot state)
            {
                PayloadKey = payloadKey;
                State = state;
            }

            public string PayloadKey { get; }
            public SimulationFarmSurvivalStateSnapshot State { get; }
        }
    }
}
