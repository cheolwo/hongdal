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
        private readonly Dictionary<string, Simulation재배단위Snapshot> cultivationUnits =
            new Dictionary<string, Simulation재배단위Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, Simulation수확LotSnapshot> harvestLots =
            new Dictionary<string, Simulation수확LotSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, Simulation포장LotSnapshot> packageLots =
            new Dictionary<string, Simulation포장LotSnapshot>(StringComparer.Ordinal);
        private readonly List<FarmWorkOrderState> farmWorkOrders =
            new List<FarmWorkOrderState>();
        private readonly List<SimulationThreatEncounterSnapshot> threatEncounters =
            new List<SimulationThreatEncounterSnapshot>();
        private readonly List<SimulationSurvivalDayReportSnapshot> survivalDayReports =
            new List<SimulationSurvivalDayReportSnapshot>();
        private readonly Dictionary<string, AppliedFarmCommand> appliedFarmWorkCommands =
            new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedFarmCommand> appliedFarmWorkPlanCommands =
            new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedFarmCommand> appliedThreatResponseCommands =
            new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);

        private SimulationFarmSurvivalInitialStateRequest? farmSurvivalCreationState;
        private string farmSurvivalInitialPayloadKey = "none";
        private decimal farmSupplyUnits;
        private decimal farmRepairMaterialUnits;
        private decimal farmSeedUnits;
        private decimal farmWaterUnits;
        private decimal farmReservedSeedUnits;
        private decimal farmReservedWaterUnits;
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
                    request.AssignmentKindCode.Trim(),
                    request.PreferredSpatialStableId.Trim());
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
                    || appliedFarmWorkPlanCommands.ContainsKey(commandId)
                    || appliedThreatResponseCommands.ContainsKey(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");

                var preview = CreateFarmWorkPreview(
                    request.ActorStableId.Trim(),
                    request.TargetStableId.Trim(),
                    request.ActionCode.Trim(),
                    request.AssignmentKindCode.Trim(),
                    request.PreferredSpatialStableId.Trim());
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(
                        preview.BlockingReasonCodes.FirstOrDefault()
                        ?? "SimulationFarmWorkBlocked");

                ApplyFarmWorkPreview(commandId, preview);
                Revision++;
                AppendFarmWorkConfirmCommand(request);
                var state = CreateFarmSurvivalStateSnapshot();
                appliedFarmWorkCommands.Add(commandId,
                    new AppliedFarmCommand(payloadKey, CloneFarmSurvivalState(state)));
                return state;
            }
        }

        public SimulationFarmWorkPlanPreviewSnapshot PreviewFarmWorkPlan(
            SimulationFarmWorkPlanPreviewRequest request)
        {
            ValidateFarmWorkPlanPreviewRequest(request);
            lock (gate)
            {
                EnsureFarmSurvivalConfigured();
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");
                return CreateFarmWorkPlanPreview(request.ExpectedRevision, request.Items);
            }
        }

        public SimulationFarmSurvivalStateSnapshot ConfirmFarmWorkPlan(
            SimulationFarmWorkPlanConfirmRequest request)
        {
            ValidateFarmWorkPlanConfirmRequest(request);
            lock (gate)
            {
                EnsureFarmSurvivalConfigured();
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildFarmWorkPlanPayloadKey(request);
                if (appliedFarmWorkPlanCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey,
                        StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "SimulationCommandPayloadConflict");
                    return CloneFarmSurvivalState(applied.State);
                }

                if (HasDifferentKindCommand(commandId)
                    || appliedFarmWorkCommands.ContainsKey(commandId)
                    || appliedThreatResponseCommands.ContainsKey(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");

                var preview = CreateFarmWorkPlanPreview(
                    request.ExpectedRevision, request.Items);
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(
                        preview.BlockingReasonCodes.FirstOrDefault()
                        ?? "SimulationFarmWorkPlanBlocked");

                foreach (var item in preview.Items.OrderBy(value => value.Priority)
                    .ThenBy(value => value.PlanItemStableId, StringComparer.Ordinal))
                    ApplyFarmWorkPreview(commandId + ":" + item.PlanItemStableId, item.Work);

                Revision++;
                AppendFarmWorkPlanConfirmCommand(request);
                var state = CreateFarmSurvivalStateSnapshot();
                appliedFarmWorkPlanCommands.Add(commandId,
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
                var choice = request.ChoiceStableId.Trim();
                var isRaiderResponse = encounter.ThreatTypeCode ==
                        SimulationFarmSurvivalCodes.RaiderFaction
                    && encounter.StateCode ==
                        SimulationFarmSurvivalCodes.AwaitingResponse;
                var isSeasonalDefenseResponse = UsesScenicSeasonRule()
                    && encounter.ThreatTypeCode ==
                        SimulationFarmSurvivalCodes.ZombiePressure
                    && encounter.StateCode ==
                        SimulationFarmSurvivalCodes.AwaitingDefenseChoice;
                if (!isRaiderResponse && !isSeasonalDefenseResponse)
                    throw new SimulationConflictException(
                        "SimulationThreatResponseNotAllowed");

                if (isRaiderResponse && choice != SimulationFarmSurvivalCodes.Trade
                    && choice != SimulationFarmSurvivalCodes.Refuse
                    && choice != SimulationFarmSurvivalCodes.Deception)
                    throw new SimulationContractException(
                        "SimulationThreatChoiceInvalid");
                if (isSeasonalDefenseResponse
                    && choice != SimulationFarmSurvivalCodes.AutomaticDefense
                    && choice != SimulationFarmSurvivalCodes.DirectCombat)
                    throw new SimulationContractException(
                        "SimulationThreatChoiceInvalid");

                Revision++;
                encounter.SelectedChoiceStableId = choice;
                if (isRaiderResponse)
                {
                    ApplyRaiderChoice(encounter, choice);
                    encounter.StateCode = SimulationFarmSurvivalCodes.Resolved;
                    UpdateFarmThreatWorldEvent(encounter);
                }
                else if (choice == SimulationFarmSurvivalCodes.DirectCombat)
                {
                    PrepareZombieCombat(encounter);
                }
                else
                {
                    encounter.StateCode =
                        SimulationFarmSurvivalCodes.AwaitingAutoResolution;
                    encounter.PresentationKey =
                        SimulationFarmSurvivalCodes.SeasonalDefenseAutomaticPresentation;
                    UpdateFarmThreatWorldEvent(encounter);
                }
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
            farmSeedUnits = farmSurvivalCreationState.SeedUnits;
            farmWaterUnits = farmSurvivalCreationState.WaterUnits;
            foreach (var actor in farmSurvivalCreationState.Actors)
                farmActors.Add(actor.ActorStableId, new FarmActorState
                {
                    ActorStableId = actor.ActorStableId,
                    ActorKindCode = actor.ActorKindCode,
                    KoreanName = actor.KoreanName,
                    Health = actor.Health,
                    Stamina = actor.Stamina,
                    CapabilityCodes = actor.CapabilityCodes.ToArray(),
                });
            foreach (var tile in farmSurvivalCreationState.SoilTiles)
                farmSoilTiles.Add(tile.SoilTileStableId, new FarmSoilTileState
                {
                    SoilTileStableId = tile.SoilTileStableId,
                    GridX = tile.GridX,
                    GridY = tile.GridY,
                    StateCode = tile.StateCode,
                    PhysicalAreaSquareMeters = tile.PhysicalAreaSquareMeters,
                });
            foreach (var defense in farmSurvivalCreationState.Defenses)
                farmDefenses.Add(defense.DefenseStableId, new FarmDefenseState
                {
                    DefenseStableId = defense.DefenseStableId,
                    DefenseKindCode = defense.DefenseKindCode,
                    Durability = defense.Durability,
                    Prepared = defense.Prepared,
                });
            foreach (var unit in farmSurvivalCreationState.CultivationUnits)
            {
                var cloned = CloneCultivationUnit(unit);
                cultivationUnits.Add(cloned.CultivationUnitStableId, cloned);
            }
        }

        private void AdvanceFarmSurvival(int previousTick, int currentTick)
        {
            if (farmSurvivalCreationState == null) return;

            AdvanceFarmTacticalCombat(currentTick);

            foreach (var workOrder in farmWorkOrders.Where(value =>
                value.StatusCode == SimulationFarmSurvivalCodes.InProgress
                && value.CompletesWorldTick <= currentTick)
                .OrderBy(value => value.CompletesWorldTick)
                .ThenBy(value => value.WorkOrderStableId, StringComparer.Ordinal))
            {
                CompleteFarmWork(workOrder);
            }

            if (UsesScenicSeasonRule())
            {
                for (var tick = previousTick + 1; tick <= currentTick; tick++)
                    AdvanceScenicSeason(tick);
                return;
            }

            if (previousTick < 4 && currentTick >= 4)
                CreateZombieWarning();
            if (previousTick < 5 && currentTick >= 5)
            {
                if (UsesInteractiveCombatRule()) PrepareZombieCombat();
                else ResolveZombieEncounter();
                CreateRaiderApproach();
            }
            if (previousTick < 6 && currentTick >= 6)
            {
                if (UsesInteractiveCombatRule()) ExpireActiveFarmCombat();
            }
            if (currentTick >= 6 && !HasOpenTacticalOrderWindow()
                && threatEncounters.Any(value => value.ThreatTypeCode ==
                    SimulationFarmSurvivalCodes.ZombiePressure
                    && value.StateCode == SimulationFarmSurvivalCodes.Resolved))
                CreateDamageAssessmentReport();
        }

        private void AdvanceScenicSeason(int worldTick)
        {
            var chapterDay = worldTick % 28 + 1;
            var chapterNumber = worldTick / 28 + 1;
            if (chapterDay == 24)
                CreateSeasonalDefenseWarning(worldTick, chapterNumber);
            else if (chapterDay == 27)
                OpenSeasonalDefenseChoice(chapterNumber);
            else if (chapterDay == 28)
                ResolveSeasonalDefenseDeadline(worldTick, chapterNumber);
        }

        private SimulationFarmWorkPreviewSnapshot CreateFarmWorkPreview(
            string actorStableId,
            string targetStableId,
            string actionCode,
            string assignmentKindCode,
            string preferredSpatialStableId)
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
            var seedCost = 0m;
            var waterCost = 0m;
            var projectedQuantity = 0m;
            var projectedQuantityUnitCode = string.Empty;
            var projectedLotStableId = string.Empty;

            if (actionCode == SimulationFarmSurvivalCodes.Tilling)
            {
                if (!farmSoilTiles.TryGetValue(targetStableId, out var soil))
                    blocks.Add("SimulationFarmSoilTileNotFound");
                else if (soil.StateCode != SimulationFarmSurvivalCodes.Untilled)
                    blocks.Add("SimulationFarmSoilTileAlreadyTilled");
                if (spatialDefinitions.Count > 0)
                    RequireFarmActorCapability(actor,
                        SimulationFarmActorCapabilityCodes.FarmTilling, blocks);
            }
            else if (actionCode == SimulationFarmSurvivalCodes.Sowing)
            {
                if (!farmSoilTiles.TryGetValue(targetStableId, out var soil))
                    blocks.Add("SimulationFarmSoilTileNotFound");
                else if (soil.StateCode != SimulationFarmSurvivalCodes.Tilled)
                    blocks.Add("SimulationFarmSoilTileNotTilled");
                else if (cultivationUnits.Values.Any(value => value.TileStableId ==
                    targetStableId && value.StateCode != Simulation재배단위상태Codes.Harvested))
                    blocks.Add("SimulationFarmSoilTileAlreadySown");
                if (farmSurvivalCreationState!.PotatoProductionRule == null)
                    blocks.Add("SimulationPotatoProductionRuleUnavailable");
                seedCost = 1m;
                projectedQuantity = soil?.PhysicalAreaSquareMeters ?? 0m;
                projectedQuantityUnitCode = "MTK";
                projectedLotStableId = CultivationUnitStableId(targetStableId);
                RequireFarmActorCapability(actor,
                    SimulationFarmActorCapabilityCodes.FarmSowing, blocks);
            }
            else if (actionCode == SimulationFarmSurvivalCodes.CropCare)
            {
                if (!cultivationUnits.TryGetValue(targetStableId, out var cultivation))
                    blocks.Add("SimulationCultivationUnitNotFound");
                else if (cultivation.StateCode != Simulation재배단위상태Codes.Growing)
                    blocks.Add("SimulationCultivationUnitNotGrowing");
                waterCost = 1m;
                projectedQuantity = cultivation?.PhysicalAreaSquareMeters ?? 0m;
                projectedQuantityUnitCode = "MTK";
                projectedLotStableId = targetStableId;
                RequireFarmActorCapability(actor,
                    SimulationFarmActorCapabilityCodes.FarmCropCare, blocks);
            }
            else if (actionCode == SimulationFarmSurvivalCodes.Harvesting)
            {
                if (!cultivationUnits.TryGetValue(targetStableId, out var cultivation))
                    blocks.Add("SimulationCultivationUnitNotFound");
                else if (cultivation.StateCode != Simulation재배단위상태Codes.HarvestReady)
                    blocks.Add("SimulationCultivationUnitNotHarvestReady");
                else if (farmSurvivalCreationState!.PotatoProductionRule == null)
                    blocks.Add("SimulationPotatoProductionRuleUnavailable");
                else
                {
                    var production = CreatePotatoProductionPreview(
                        cultivation,
                        "preview:farm-harvest:" + cultivation.CultivationUnitStableId,
                        "preview-task:farm-harvest:" + cultivation.CultivationUnitStableId,
                        CurrentTick + duration);
                    projectedQuantity = production.ExpectedHarvestQuantityKilograms;
                    projectedQuantityUnitCode = "KGM";
                    projectedLotStableId = production.HarvestLotStableId;
                }
                RequireFarmActorCapability(actor, SimulationFarmActorCapabilityCodes.FarmHarvest,
                    blocks);
            }
            else if (actionCode == SimulationFarmSurvivalCodes.HarvestCollection)
            {
                if (!harvestLots.TryGetValue(targetStableId, out var harvestLot))
                    blocks.Add("SimulationHarvestLotNotFound");
                else if (harvestLot.StateCode != Simulation수확Lot상태Codes.HarvestedAtField)
                    blocks.Add("SimulationHarvestLotStateInvalid");
                else
                {
                    projectedQuantity = harvestLot.Quantity;
                    projectedQuantityUnitCode = harvestLot.UnitCode;
                    projectedLotStableId = harvestLot.HarvestLotStableId;
                }
                RequireFarmActorCapability(actor, SimulationFarmActorCapabilityCodes.FarmCollection,
                    blocks);
            }
            else if (actionCode == SimulationFarmSurvivalCodes.OutboundPacking)
            {
                if (!harvestLots.TryGetValue(targetStableId, out var harvestLot))
                    blocks.Add("SimulationHarvestLotNotFound");
                else if (harvestLot.StateCode != Simulation수확Lot상태Codes.CollectedAtYard)
                    blocks.Add("SimulationHarvestLotStateInvalid");
                else
                {
                    projectedQuantity = harvestLot.Quantity;
                    projectedQuantityUnitCode = harvestLot.UnitCode;
                    projectedLotStableId = PackageLotStableId(harvestLot.HarvestLotStableId);
                    if (!harvestLotAllocationIds.TryGetValue(
                        harvestLot.HarvestLotStableId,
                        out var allocationStableId))
                    {
                        blocks.Add("SimulationHarvestDispositionChoiceRequired");
                    }
                    else
                    {
                        var allocation = harvestLotAllocations[allocationStableId];
                        if (allocation.ChoiceCode
                            != SimulationHarvestDispositionChoiceCodes.CooperativeShipment)
                        {
                            blocks.Add(
                                "SimulationHarvestDispositionChoiceDoesNotUseOutboundPacking");
                        }
                        else if (allocation.StateCode
                            != SimulationHarvestLotAllocationStateCodes.Applied)
                        {
                            blocks.Add("SimulationHarvestDispositionChoiceNotApplied");
                        }
                    }
                }
                RequireFarmActorCapability(actor, SimulationFarmActorCapabilityCodes.FarmPacking,
                    blocks);
                if (settlementInitialState == null)
                    blocks.Add("SimulationSettlementRequiredForHarvestPacking");
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
            if (farmSeedUnits < seedCost)
                blocks.Add("SimulationFarmSeedInsufficient");
            if (farmWaterUnits < waterCost)
                blocks.Add("SimulationFarmWaterInsufficient");
            if (assignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated)
            {
                if (settlementInitialState == null)
                    blocks.Add("SimulationSettlementRequiredForNpcLabor");
                else if (settlementInitialState.LaborCapacityTotal
                    - settlementInitialState.LaborReserved < requiredLabor)
                    blocks.Add("SimulationSettlementLaborInsufficient");
            }

            var requiresSpatial = IsFarmWorldInteractionAction(actionCode)
                && (actionCode != SimulationFarmSurvivalCodes.Tilling
                    || spatialDefinitions.Count > 0);
            var spatial = requiresSpatial
                ? CreateFarmSpatialPreview(
                    farmSurvivalCreationState!.FarmBuildingStableId,
                    actionCode,
                    preferredSpatialStableId,
                    projectedQuantity,
                    projectedQuantityUnitCode)
                : null;
            if (spatial != null)
                blocks.AddRange(spatial.BlockReasonCodes);

            return new SimulationFarmWorkPreviewSnapshot
            {
                ActorStableId = actorStableId,
                TargetStableId = targetStableId,
                ActionCode = actionCode,
                AssignmentKindCode = assignmentKindCode,
                RequiredLabor = requiredLabor,
                StaminaCost = staminaCost,
                MaterialCost = materialCost,
                SeedCost = seedCost,
                WaterCost = waterCost,
                DurationTicks = duration,
                EstimatedCompletionWorldTick = CurrentTick + duration,
                PresentationKey = FarmWorkPresentationKey(actionCode),
                ProjectedQuantity = projectedQuantity,
                ProjectedQuantityUnitCode = projectedQuantityUnitCode,
                ProjectedLotStableId = projectedLotStableId,
                SpatialInteraction = spatial,
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.Distinct(StringComparer.Ordinal).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private SimulationFarmWorkPlanPreviewSnapshot CreateFarmWorkPlanPreview(
            long expectedRevision,
            SimulationFarmWorkPlanItemRequest[] items)
        {
            var ordered = items.OrderBy(value => value.Priority)
                .ThenBy(value => value.PlanItemStableId, StringComparer.Ordinal).ToArray();
            var itemPreviews = ordered.Select(value =>
            {
                var work = CreateFarmWorkPreview(
                    value.ActorStableId.Trim(), value.TargetStableId.Trim(),
                    value.ActionCode.Trim(), value.AssignmentKindCode.Trim(),
                    value.PreferredSpatialStableId.Trim());
                return new SimulationFarmWorkPlanItemPreviewSnapshot
                {
                    PlanItemStableId = value.PlanItemStableId.Trim(),
                    Priority = value.Priority,
                    Work = work,
                    BlockingReasonCodes = work.BlockingReasonCodes.ToArray(),
                };
            }).ToArray();
            var blocks = itemPreviews.SelectMany(value => value.BlockingReasonCodes)
                .ToList();

            if (itemPreviews.GroupBy(value => value.Work.ActorStableId,
                    StringComparer.Ordinal).Any(group => group.Count() > 1))
                blocks.Add("SimulationFarmWorkPlanActorConflict");
            if (itemPreviews.GroupBy(value => string.Join("|",
                    value.Work.ActionCode, value.Work.TargetStableId),
                    StringComparer.Ordinal).Any(group => group.Count() > 1))
                blocks.Add("SimulationFarmWorkPlanTargetConflict");

            var totalLabor = itemPreviews.Sum(value => value.Work.RequiredLabor);
            var totalMaterial = itemPreviews.Sum(value => value.Work.MaterialCost);
            var totalSeed = itemPreviews.Sum(value => value.Work.SeedCost);
            var totalWater = itemPreviews.Sum(value => value.Work.WaterCost);
            if (settlementInitialState != null
                && totalLabor > settlementInitialState.LaborCapacityTotal
                    - settlementInitialState.LaborReserved)
                blocks.Add("SimulationSettlementLaborInsufficient");
            if (totalMaterial > farmRepairMaterialUnits)
                blocks.Add("SimulationFarmRepairMaterialInsufficient");
            if (totalSeed > farmSeedUnits)
                blocks.Add("SimulationFarmSeedInsufficient");
            if (totalWater > farmWaterUnits)
                blocks.Add("SimulationFarmWaterInsufficient");

            foreach (var capacityGroup in itemPreviews
                .Where(value => value.Work.SpatialInteraction != null)
                .SelectMany(value => value.Work.SpatialInteraction!.RequiredCapacities
                    .Select(capacity => new
                    {
                        SpatialStableId = value.Work.SpatialInteraction!.SelectedSpatialStableId,
                        Capacity = capacity,
                    }))
                .GroupBy(value => string.Join("|", value.SpatialStableId,
                    value.Capacity.CapacityCode, value.Capacity.UnitCode),
                    StringComparer.Ordinal))
            {
                var first = capacityGroup.First();
                if (!spatialDefinitions.TryGetValue(first.SpatialStableId, out var definition)
                    || !spatialRuntimeStates.TryGetValue(first.SpatialStableId, out var runtime))
                {
                    blocks.Add(Simulation공간차단Codes.DefinitionUnavailable);
                    continue;
                }
                var baseCapacity = FindCapacity(definition.BaseCapacities, first.Capacity);
                var available = (baseCapacity?.Quantity ?? 0m)
                    - CapacityQuantity(runtime.OccupiedCapacities, first.Capacity)
                    - CapacityQuantity(runtime.ReservedCapacities, first.Capacity);
                if (capacityGroup.Sum(value => value.Capacity.Quantity) > available)
                    blocks.Add(first.Capacity.CapacityCode == Simulation공간용량Codes.WorkArea
                        ? Simulation공간차단Codes.ReservationConflict
                        : Simulation공간차단Codes.CapacityInsufficient);
            }

            var distinctBlocks = blocks.Distinct(StringComparer.Ordinal).ToArray();
            return new SimulationFarmWorkPlanPreviewSnapshot
            {
                ExpectedRevision = expectedRevision,
                Items = itemPreviews,
                TotalRequiredLabor = totalLabor,
                TotalStaminaCost = itemPreviews.Sum(value => value.Work.StaminaCost),
                EstimatedCompletionWorldTick = itemPreviews.Length == 0
                    ? CurrentTick
                    : itemPreviews.Max(value => value.Work.EstimatedCompletionWorldTick),
                CanConfirm = distinctBlocks.Length == 0,
                BlockingReasonCodes = distinctBlocks,
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private void ApplyFarmWorkPreview(
            string commandId,
            SimulationFarmWorkPreviewSnapshot preview)
        {
            if (preview.AssignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated)
                settlementInitialState!.LaborReserved += preview.RequiredLabor;
            farmRepairMaterialUnits -= preview.MaterialCost;
            farmSeedUnits -= preview.SeedCost;
            farmWaterUnits -= preview.WaterCost;
            farmReservedSeedUnits += preview.SeedCost;
            farmReservedWaterUnits += preview.WaterCost;

            var isSupplyWork = IsFarmWorldInteractionAction(preview.ActionCode)
                && preview.SpatialInteraction != null;
            var workOrder = new FarmWorkOrderState
            {
                WorkOrderStableId = isSupplyWork
                    ? "task:farm-supply:" + commandId
                    : "farm-work:" + commandId,
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
                SeedCost = preview.SeedCost,
                WaterCost = preview.WaterCost,
                PresentationKey = preview.PresentationKey,
                SelectedSpatialStableId = preview.SpatialInteraction?.SelectedSpatialStableId
                    ?? string.Empty,
                SpatialDefinitionRevision = preview.SpatialInteraction?.DefinitionRevision
                    ?? string.Empty,
                SpatialDefinitionHashSha256 = preview.SpatialInteraction?.DefinitionHashSha256
                    ?? string.Empty,
            };
            if (isSupplyWork)
                RegisterFarmSupplyDecisionAndTask(workOrder, preview);
            farmActors[preview.ActorStableId].ActiveWorkOrderStableId =
                workOrder.WorkOrderStableId;
            farmWorkOrders.Add(workOrder);
        }

        private void CompleteFarmWork(FarmWorkOrderState workOrder)
        {
            workOrder.StatusCode = SimulationFarmSurvivalCodes.Completed;
            var actor = farmActors[workOrder.ActorStableId];
            actor.Stamina = Math.Max(0m, actor.Stamina - workOrder.StaminaCost);
            actor.ActiveWorkOrderStableId = string.Empty;
            if (workOrder.AssignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated)
                settlementInitialState!.LaborReserved -= workOrder.ReservedLabor;
            farmReservedSeedUnits -= workOrder.SeedCost;
            farmReservedWaterUnits -= workOrder.WaterCost;

            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.Tilling)
            {
                farmSoilTiles[workOrder.TargetStableId].StateCode =
                    SimulationFarmSurvivalCodes.Tilled;
                EvaluateCollectibleRewardForFarmCompletion(workOrder);
                return;
            }
            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.Sowing)
            {
                var soil = farmSoilTiles[workOrder.TargetStableId];
                var rule = farmSurvivalCreationState!.PotatoProductionRule!;
                var cultivationStableId = CultivationUnitStableId(soil.SoilTileStableId);
                cultivationUnits.Add(cultivationStableId, new Simulation재배단위Snapshot
                {
                    CultivationUnitStableId = cultivationStableId,
                    Revision = 1,
                    TileStableId = soil.SoilTileStableId,
                    CultivationStableId = "cultivation:potato:" + soil.SoilTileStableId,
                    ProductStableId = rule.ProductStableId,
                    CropVariantStableId = rule.CropVariantStableId,
                    StateCode = Simulation재배단위상태Codes.Growing,
                    PhysicalAreaSquareMeters = soil.PhysicalAreaSquareMeters,
                    EffectiveCultivationAreaRatio = 1m,
                    SourceStableIds = new[]
                    {
                        soil.SoilTileStableId,
                        rule.RuleStableId,
                        workOrder.WorkOrderStableId,
                    },
                });
                return;
            }
            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.CropCare)
            {
                var cultivation = cultivationUnits[workOrder.TargetStableId];
                cultivation.StateCode = Simulation재배단위상태Codes.HarvestReady;
                cultivation.Revision++;
                return;
            }
            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.Harvesting)
            {
                var cultivation = cultivationUnits[workOrder.TargetStableId];
                var production = CreatePotatoProductionPreview(
                    cultivation,
                    "decision:farm-supply:" + workOrder.CommandId,
                    workOrder.WorkOrderStableId,
                    workOrder.CompletesWorldTick);
                cultivation.StateCode = Simulation재배단위상태Codes.Harvested;
                cultivation.Revision++;
                harvestLots.Add(production.HarvestLotStableId, new Simulation수확LotSnapshot
                {
                    HarvestLotStableId = production.HarvestLotStableId,
                    Revision = 1,
                    CultivationUnitStableId = cultivation.CultivationUnitStableId,
                    ProductStableId = cultivation.ProductStableId,
                    Quantity = production.ExpectedHarvestQuantityKilograms,
                    UnitCode = "KGM",
                    StateCode = Simulation수확Lot상태Codes.HarvestedAtField,
                    CausedByTaskStableId = workOrder.WorkOrderStableId,
                    CreatedWorldTick = workOrder.CompletesWorldTick,
                    SourceStableIds = production.PendingEffectBundle.SourceStableIds.ToArray(),
                });
                RegisterFarmHarvestExposureIncident(
                    production.HarvestLotStableId,
                    farmSurvivalCreationState!.FarmBuildingStableId,
                    workOrder.CompletesWorldTick);
                CompleteWorldInteractionManifestationForTask(
                    workOrder.WorkOrderStableId,
                    new[] { workOrder.WorkOrderStableId + ":production-effect" },
                    new[] { "HarvestLotCreated", "CultivationHarvested" },
                    WorldEventMutationRevision);
                return;
            }
            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.HarvestCollection)
            {
                var harvestLot = harvestLots[workOrder.TargetStableId];
                harvestLot.StateCode = Simulation수확Lot상태Codes.CollectedAtYard;
                harvestLot.Revision++;
                ObserveRegionalIncidentAction(workOrder.ActionCode,
                    harvestLot.HarvestLotStableId, workOrder.CompletesWorldTick);
                CompleteWorldInteractionManifestationForTask(
                    workOrder.WorkOrderStableId,
                    new[] { workOrder.WorkOrderStableId + ":collection-effect" },
                    new[] { "HarvestLotCollected" },
                    WorldEventMutationRevision);
                return;
            }
            if (workOrder.ActionCode == SimulationFarmSurvivalCodes.OutboundPacking)
            {
                var harvestLot = harvestLots[workOrder.TargetStableId];
                harvestLot.StateCode = Simulation수확Lot상태Codes.PackedForShipment;
                harvestLot.Revision++;
                var packageLotId = PackageLotStableId(harvestLot.HarvestLotStableId);
                if (!harvestLotAllocationIds.TryGetValue(
                    harvestLot.HarvestLotStableId,
                    out var allocationId))
                {
                    throw new SimulationConflictException(
                        "SimulationHarvestDispositionChoiceRequired");
                }
                var allocation = harvestLotAllocations[allocationId];
                if (allocation.ChoiceCode
                    != SimulationHarvestDispositionChoiceCodes.CooperativeShipment
                    || allocation.StateCode != SimulationHarvestLotAllocationStateCodes.Applied)
                {
                    throw new SimulationConflictException(
                        "SimulationHarvestDispositionChoiceInvalidForOutboundPacking");
                }
                packageLots.Add(packageLotId, new Simulation포장LotSnapshot
                {
                    PackageLotStableId = packageLotId,
                    Revision = 1,
                    HarvestLotStableId = harvestLot.HarvestLotStableId,
                    CargoStableId = CargoStableId(packageLotId),
                    SourceAllocationStableId = allocationId,
                    ProductStableId = harvestLot.ProductStableId,
                    Quantity = harvestLot.Quantity,
                    UnitCode = harvestLot.UnitCode,
                    StateCode = Simulation포장Lot상태Codes.PreparedForShipment,
                    CausedByTaskStableId = workOrder.WorkOrderStableId,
                    CreatedWorldTick = workOrder.CompletesWorldTick,
                    SourceStableIds = harvestLot.SourceStableIds
                        .Append(harvestLot.HarvestLotStableId)
                        .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                });
                ObserveRegionalIncidentAction(workOrder.ActionCode,
                    harvestLot.HarvestLotStableId, workOrder.CompletesWorldTick);
                CompleteWorldInteractionManifestationForTask(
                    workOrder.WorkOrderStableId,
                    new[] { workOrder.WorkOrderStableId + ":packing-effect" },
                    new[] { "PackageLotCreated", "CargoPrepared" },
                    WorldEventMutationRevision);
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

        private void RegisterFarmSupplyDecisionAndTask(
            FarmWorkOrderState workOrder,
            SimulationFarmWorkPreviewSnapshot preview)
        {
            var decisionId = "decision:farm-supply:" + workOrder.CommandId;
            if (decisions.ContainsKey(decisionId) || tasks.ContainsKey(workOrder.WorkOrderStableId))
                throw new SimulationConflictException("SimulationFarmSupplyStableIdConflict");
            var effectType = workOrder.ActionCode == SimulationFarmSurvivalCodes.Tilling
                ? "FarmSoilTilled"
                : workOrder.ActionCode == SimulationFarmSurvivalCodes.Sowing
                    ? "CultivationUnitCreated"
                    : workOrder.ActionCode == SimulationFarmSurvivalCodes.CropCare
                        ? "CultivationUnitHarvestReady"
                        : workOrder.ActionCode == SimulationFarmSurvivalCodes.Harvesting
                            ? "HarvestLotCreated"
                            : workOrder.ActionCode == SimulationFarmSurvivalCodes.HarvestCollection
                                ? "HarvestLotCollected"
                            : workOrder.ActionCode == SimulationFarmSurvivalCodes.OutboundPacking
                                ? "PackageLotCreated"
                                : "FacilityRepaired";
            var effectUnit = preview.ProjectedQuantityUnitCode.Length == 0
                ? "state" : preview.ProjectedQuantityUnitCode;
            var effectDelta = preview.ProjectedQuantity > 0m ? preview.ProjectedQuantity : 1m;
            var sources = new[]
            {
                workOrder.TargetStableId,
                workOrder.ActorStableId,
                farmSurvivalCreationState!.AreaStableId,
                preview.SpatialInteraction?.SelectedSpatialStableId ?? string.Empty,
            }.Where(value => value.Length > 0).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

            var decision = new SimulationDecisionSnapshot
            {
                DecisionStableId = decisionId,
                DecisionTypeCode = "FarmSupplyWork",
                StateCode = SimulationDecisionStateCodes.Confirmed,
                Revision = 1,
                SessionStableId = SessionStableId,
                FactionStableId = FactionStableId,
                TerritoryStableId = TerritoryStableId,
                SettlementStableId = SettlementStableId,
                ActorStableId = workOrder.ActorStableId,
                TargetStableIds = new[] { workOrder.TargetStableId },
                CreatedTick = CurrentTick,
                ConfirmedTick = CurrentTick,
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = effectType,
                        TargetLedgerStableId = preview.ProjectedLotStableId.Length == 0
                            ? workOrder.TargetStableId : preview.ProjectedLotStableId,
                        BeforeValue = 0m,
                        Delta = effectDelta,
                        AfterValue = effectDelta,
                        UnitCode = effectUnit,
                        SourceStableIds = sources,
                    },
                },
                SourceStableIds = sources,
            };
            var task = new SimulationTaskSnapshot
            {
                TaskStableId = workOrder.WorkOrderStableId,
                TaskTypeCode = "FarmSupplyWork",
                StateCode = SimulationTaskStateCodes.Scheduled,
                Revision = 1,
                CausedByDecisionStableId = decisionId,
                FacilityStableId = farmSurvivalCreationState.FarmBuildingStableId,
                ActionCode = workOrder.ActionCode,
                AssignedActorStableId = workOrder.ActorStableId,
                SelectedSpatialStableId = workOrder.SelectedSpatialStableId,
                SpatialDefinitionRevision = workOrder.SpatialDefinitionRevision,
                SpatialDefinitionHashSha256 = workOrder.SpatialDefinitionHashSha256,
                AssignedCapacity = preview.ProjectedQuantity,
                AssignedCapacityUnitCode = preview.ProjectedQuantityUnitCode,
                ScheduledStartTick = CurrentTick + 1,
                ExpectedEndTick = workOrder.CompletesWorldTick,
                InputLotStableIds = new[] { workOrder.TargetStableId },
                OutputCandidateCodes = new[] { effectType },
                SourceStableIds = sources,
            };
            var reservations = PrepareSimulationSpatialReservations(task);
            decisions.Add(decisionId, decision);
            tasks.Add(task.TaskStableId, task);
            effects.Add(task.TaskStableId + ":effect:1", new SimulationEffectRecord
            {
                EffectStableId = task.TaskStableId + ":effect:1",
                EffectTypeCode = effectType,
                StateCode = SimulationEffectStateCodes.Pending,
                Revision = 1,
                CausedByDecisionStableId = decisionId,
                CausedByTaskStableId = task.TaskStableId,
                TargetLedgerStableId = decision.ExpectedEffects[0].TargetLedgerStableId,
                BeforeValue = 0m,
                Delta = effectDelta,
                AfterValue = effectDelta,
                UnitCode = effectUnit,
                SourceStableIds = sources,
            });
            RegisterSimulationSpatialReservations(task, reservations);
        }

        private Simulation감자생산PreviewResult CreatePotatoProductionPreview(
            Simulation재배단위Snapshot cultivation,
            string decisionStableId,
            string taskStableId,
            int completedTick)
        {
            var rule = farmSurvivalCreationState!.PotatoProductionRule
                ?? throw new SimulationContractException("SimulationPotatoProductionRuleUnavailable");
            return new Simulation감자생산규칙().CreatePreview(new Simulation감자생산Request
            {
                EffectBundleStableId = taskStableId + ":production-effect",
                EffectLineStableId = taskStableId + ":production-effect:1",
                DecisionStableId = decisionStableId,
                DecisionStateCode = SimulationDecisionStateCodes.Confirmed,
                CompletedTaskStableId = taskStableId,
                TaskStateCode = SimulationTaskStateCodes.Completed,
                CompletedTick = completedTick,
                HarvestLotStableId = HarvestLotStableId(cultivation.CultivationUnitStableId),
                HarvestLedgerStableId = "ledger:farm-harvest:" + farmSurvivalCreationState.FarmBuildingStableId,
                EnvironmentSnapshotStableId = "environment:scenario:" + farmSurvivalCreationState.AreaStableId,
                EnvironmentFactor = 1m,
                InputFactor = 1m,
                FacilityFactor = 1m,
                LossFactor = 1m,
                CultivationUnit = CloneCultivationUnit(cultivation),
                Rule = ClonePotatoProductionRule(rule),
                SourceStableIds = new[]
                {
                    "scenario:farm-supply:300kg",
                    farmSurvivalCreationState.AreaStableId,
                },
            });
        }

        private static void RequireFarmActorCapability(
            FarmActorState? actor,
            string capabilityCode,
            List<string> blocks)
        {
            if (actor != null && !actor.CapabilityCodes.Contains(capabilityCode,
                    StringComparer.Ordinal))
                blocks.Add("SimulationFarmActorCapabilityMissing");
        }

        private static bool IsFarmSupplyAction(string actionCode)
            => actionCode == SimulationFarmSurvivalCodes.Harvesting
                || actionCode == SimulationFarmSurvivalCodes.HarvestCollection
                || actionCode == SimulationFarmSurvivalCodes.OutboundPacking;

        private static bool IsFarmWorldInteractionAction(string actionCode)
            => actionCode == SimulationFarmSurvivalCodes.Tilling
                || actionCode == SimulationFarmSurvivalCodes.Sowing
                || actionCode == SimulationFarmSurvivalCodes.CropCare
                || actionCode == SimulationFarmSurvivalCodes.FenceRepair
                || IsFarmSupplyAction(actionCode);

        private static string CultivationUnitStableId(string soilTileStableId)
            => "cultivation-unit:potato:" + soilTileStableId;

        private static string HarvestLotStableId(string cultivationUnitStableId)
            => "harvest-lot:" + cultivationUnitStableId;

        private static string PackageLotStableId(string harvestLotStableId)
            => "package-lot:" + harvestLotStableId;

        private static string CargoStableId(string packageLotStableId)
            => "cargo:" + packageLotStableId;

        private void CreateSeasonalDefenseWarning(
            int worldTick,
            int chapterNumber)
        {
            var encounterId = SeasonalDefenseEncounterId(chapterNumber);
            if (threatEncounters.Any(value => value.EncounterStableId == encounterId))
                return;
            var encounter = new SimulationThreatEncounterSnapshot
            {
                EncounterStableId = encounterId,
                ThreatTypeCode = SimulationFarmSurvivalCodes.ZombiePressure,
                ThreatUnitCount = 3,
                StateCode = SimulationFarmSurvivalCodes.Warning,
                OccurredWorldTick = worldTick,
                DecisionDeadlineWorldTick = ChapterStartTick(chapterNumber) + 27,
                PresentationKey =
                    SimulationFarmSurvivalCodes.SeasonalDefenseWarningPresentation,
                Recoverable = true,
            };
            threatEncounters.Add(encounter);
            RegisterFarmThreatWorldEvent(encounter,
                Array.Empty<SimulationWorldEventChoiceSnapshot>());
        }

        private void OpenSeasonalDefenseChoice(int chapterNumber)
        {
            var encounter = threatEncounters.SingleOrDefault(value =>
                value.EncounterStableId == SeasonalDefenseEncounterId(chapterNumber));
            if (encounter == null || encounter.StateCode !=
                    SimulationFarmSurvivalCodes.Warning)
                return;

            encounter.StateCode = SimulationFarmSurvivalCodes.AwaitingDefenseChoice;
            encounter.PresentationKey =
                SimulationFarmSurvivalCodes.SeasonalDefenseChoicePresentation;
            encounter.AvailableChoiceStableIds = new[]
            {
                SimulationFarmSurvivalCodes.AutomaticDefense,
                SimulationFarmSurvivalCodes.DirectCombat,
            };
            UpdateFarmThreatWorldEvent(encounter,
            new[]
            {
                ThreatChoice(SimulationFarmSurvivalCodes.AutomaticDefense, 1,
                    "자동 방어", "준비한 시설과 NPC의 방어 상태로 계절 사건을 해결한다."),
                ThreatChoice(SimulationFarmSurvivalCodes.DirectCombat, 2,
                    "직접 전투", "플레이어가 1인칭 전투에 참가해 방어 판정을 보완한다."),
            });
        }

        private void ResolveSeasonalDefenseDeadline(
            int worldTick,
            int chapterNumber)
        {
            var encounter = threatEncounters.SingleOrDefault(value =>
                value.EncounterStableId == SeasonalDefenseEncounterId(chapterNumber));
            if (encounter == null) return;

            if (encounter.StateCode ==
                SimulationFarmSurvivalCodes.AwaitingDefenseChoice)
            {
                encounter.SelectedChoiceStableId =
                    SimulationFarmSurvivalCodes.AutomaticDefense;
                encounter.StateCode =
                    SimulationFarmSurvivalCodes.AwaitingAutoResolution;
            }

            if (encounter.StateCode ==
                SimulationFarmSurvivalCodes.AwaitingAutoResolution)
            {
                ResolveZombieEncounter(encounter);
            }
            else if (encounter.StateCode == SimulationFarmSurvivalCodes.AwaitingCombat)
            {
                ExpireActiveFarmCombat();
                if (encounter.StateCode != SimulationFarmSurvivalCodes.Resolved)
                    ResolveZombieEncounter(encounter);
            }

            CreateSeasonalDefenseReport(worldTick + 1, encounter);
        }

        private void CreateSeasonalDefenseReport(
            int dayNumber,
            SimulationThreatEncounterSnapshot encounter)
        {
            if (survivalDayReports.Any(value => value.DayNumber == dayNumber)) return;
            survivalDayReports.Add(new SimulationSurvivalDayReportSnapshot
            {
                DayNumber = dayNumber,
                ReportCode = recoverableDamageUnits > 0m
                    ? "SeasonalRecoveryRequired" : "PeacefulSeasonSecured",
                KoreanSummary = recoverableDamageUnits > 0m
                    ? "계절 방어 뒤 복구 가능한 피해를 천천히 정리할 수 있습니다."
                    : "산책과 농장 생활을 이어갈 수 있도록 계절 방어를 마쳤습니다.",
                SourceStableIds = new[] { encounter.EncounterStableId },
            });
        }

        private string SeasonalDefenseEncounterId(int chapterNumber)
            => "encounter:seasonal-defense:" + SessionStableId
                + ":chapter-" + chapterNumber.ToString(CultureInfo.InvariantCulture);

        private static int ChapterStartTick(int chapterNumber)
            => (chapterNumber - 1) * 28;

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

            ResolveZombieEncounter(encounter);
        }

        private void ResolveZombieEncounter(
            SimulationThreatEncounterSnapshot encounter)
        {
            if (encounter.StateCode == SimulationFarmSurvivalCodes.Resolved)
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
                DayGoalCode = DayGoalCode(CurrentTick + 1, creation.RuleRevision),
                SupplyUnits = farmSupplyUnits,
                RepairMaterialUnits = farmRepairMaterialUnits,
                SeedUnits = farmSeedUnits,
                WaterUnits = farmWaterUnits,
                ReservedSeedUnits = farmReservedSeedUnits,
                ReservedWaterUnits = farmReservedWaterUnits,
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
                        CapabilityCodes = value.CapabilityCodes.ToArray(),
                    }).ToArray(),
                SoilTiles = farmSoilTiles.Values.OrderBy(value => value.SoilTileStableId,
                    StringComparer.Ordinal).Select(value => new SimulationFarmSoilTileSnapshot
                    {
                        SoilTileStableId = value.SoilTileStableId,
                        GridX = value.GridX,
                        GridY = value.GridY,
                        StateCode = value.StateCode,
                        PhysicalAreaSquareMeters = value.PhysicalAreaSquareMeters,
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
                CultivationUnits = cultivationUnits.Values.OrderBy(value =>
                    value.CultivationUnitStableId, StringComparer.Ordinal)
                    .Select(CloneCultivationUnit).ToArray(),
                HarvestLots = harvestLots.Values.OrderBy(value => value.HarvestLotStableId,
                    StringComparer.Ordinal).Select(CloneHarvestLot).ToArray(),
                PackageLots = packageLots.Values.OrderBy(value => value.PackageLotStableId,
                    StringComparer.Ordinal).Select(ClonePackageLot).ToArray(),
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
                        SeedCost = value.SeedCost,
                        WaterCost = value.WaterCost,
                        PresentationKey = value.PresentationKey,
                        SelectedSpatialStableId = value.SelectedSpatialStableId,
                        SpatialDefinitionRevision = value.SpatialDefinitionRevision,
                        SpatialDefinitionHashSha256 = value.SpatialDefinitionHashSha256,
                    }).ToArray(),
                Encounters = threatEncounters.Select(CloneThreatEncounter).ToArray(),
                DayReports = survivalDayReports.Select(CloneDayReport).ToArray(),
                Combat = CreateFarmCombatStateSnapshot(),
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
                SeedUnits = source.SeedUnits,
                WaterUnits = source.WaterUnits,
                ReservedSeedUnits = source.ReservedSeedUnits,
                ReservedWaterUnits = source.ReservedWaterUnits,
                RecoverableDamageUnits = source.RecoverableDamageUnits,
                Combat = CloneFarmCombatState(source.Combat),
                Actors = source.Actors.Select(value => new SimulationFarmActorSnapshot
                {
                    ActorStableId = value.ActorStableId,
                    ActorKindCode = value.ActorKindCode,
                    KoreanName = value.KoreanName,
                    Health = value.Health,
                    Stamina = value.Stamina,
                    Injured = value.Injured,
                    ActiveWorkOrderStableId = value.ActiveWorkOrderStableId,
                    CapabilityCodes = value.CapabilityCodes.ToArray(),
                }).ToArray(),
                SoilTiles = source.SoilTiles.Select(value => new SimulationFarmSoilTileSnapshot
                {
                    SoilTileStableId = value.SoilTileStableId,
                    GridX = value.GridX,
                    GridY = value.GridY,
                    StateCode = value.StateCode,
                    PhysicalAreaSquareMeters = value.PhysicalAreaSquareMeters,
                }).ToArray(),
                Defenses = source.Defenses.Select(value => new SimulationFarmDefenseSnapshot
                {
                    DefenseStableId = value.DefenseStableId,
                    DefenseKindCode = value.DefenseKindCode,
                    Durability = value.Durability,
                    Prepared = value.Prepared,
                    PresentationKey = value.PresentationKey,
                }).ToArray(),
                CultivationUnits = source.CultivationUnits.Select(CloneCultivationUnit).ToArray(),
                HarvestLots = source.HarvestLots.Select(CloneHarvestLot).ToArray(),
                PackageLots = source.PackageLots.Select(ClonePackageLot).ToArray(),
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
                    SeedCost = value.SeedCost,
                    WaterCost = value.WaterCost,
                    PresentationKey = value.PresentationKey,
                    SelectedSpatialStableId = value.SelectedSpatialStableId,
                    SpatialDefinitionRevision = value.SpatialDefinitionRevision,
                    SpatialDefinitionHashSha256 = value.SpatialDefinitionHashSha256,
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
                SeedUnits = source.SeedUnits,
                WaterUnits = source.WaterUnits,
                Actors = source.Actors.Select(value => new SimulationFarmActorInitialStateRequest
                {
                    ActorStableId = value.ActorStableId.Trim(),
                    ActorKindCode = value.ActorKindCode.Trim(),
                    KoreanName = value.KoreanName.Trim(),
                    Health = value.Health,
                    Stamina = value.Stamina,
                    CapabilityCodes = value.CapabilityCodes.Select(item => item.Trim())
                        .OrderBy(item => item, StringComparer.Ordinal).ToArray(),
                }).OrderBy(value => value.ActorStableId, StringComparer.Ordinal).ToArray(),
                SoilTiles = source.SoilTiles.Select(value =>
                    new SimulationFarmSoilTileInitialStateRequest
                    {
                        SoilTileStableId = value.SoilTileStableId.Trim(),
                        GridX = value.GridX,
                        GridY = value.GridY,
                        StateCode = value.StateCode.Trim(),
                        PhysicalAreaSquareMeters = value.PhysicalAreaSquareMeters,
                    }).OrderBy(value => value.SoilTileStableId, StringComparer.Ordinal).ToArray(),
                Defenses = source.Defenses.Select(value =>
                    new SimulationFarmDefenseInitialStateRequest
                    {
                        DefenseStableId = value.DefenseStableId.Trim(),
                        DefenseKindCode = value.DefenseKindCode.Trim(),
                        Durability = value.Durability,
                        Prepared = value.Prepared,
                    }).OrderBy(value => value.DefenseStableId, StringComparer.Ordinal).ToArray(),
                CultivationUnits = source.CultivationUnits.Select(CloneCultivationUnit)
                    .OrderBy(value => value.CultivationUnitStableId, StringComparer.Ordinal).ToArray(),
                PotatoProductionRule = source.PotatoProductionRule == null
                    ? null : ClonePotatoProductionRule(source.PotatoProductionRule),
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
            if (value.SeedUnits != 0m || value.WaterUnits != 0m)
            {
                AddFarmKey(key, value.SeedUnits);
                AddFarmKey(key, value.WaterUnits);
            }
            var hasFarmSupplyDefinition = value.CultivationUnits.Length > 0
                || value.PotatoProductionRule != null;
            foreach (var actor in value.Actors)
            {
                AddFarmKey(key, actor.ActorStableId);
                AddFarmKey(key, actor.ActorKindCode);
                AddFarmKey(key, actor.KoreanName);
                AddFarmKey(key, actor.Health);
                AddFarmKey(key, actor.Stamina);
                if (hasFarmSupplyDefinition)
                    foreach (var capability in actor.CapabilityCodes)
                        AddFarmKey(key, capability);
            }
            foreach (var soil in value.SoilTiles)
            {
                AddFarmKey(key, soil.SoilTileStableId);
                AddFarmKey(key, soil.GridX);
                AddFarmKey(key, soil.GridY);
                AddFarmKey(key, soil.StateCode);
                if (value.SeedUnits != 0m || value.WaterUnits != 0m)
                    AddFarmKey(key, soil.PhysicalAreaSquareMeters);
            }
            foreach (var defense in value.Defenses)
            {
                AddFarmKey(key, defense.DefenseStableId);
                AddFarmKey(key, defense.DefenseKindCode);
                AddFarmKey(key, defense.Durability);
                AddFarmKey(key, defense.Prepared);
            }
            if (hasFarmSupplyDefinition)
            {
                foreach (var cultivation in value.CultivationUnits)
                    AddCultivationUnitKey(key, cultivation);
                if (value.PotatoProductionRule != null)
                    AddPotatoProductionRuleKey(key, value.PotatoProductionRule);
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
            if (value.SeedUnits != 0m || value.WaterUnits != 0m
                || value.ReservedSeedUnits != 0m || value.ReservedWaterUnits != 0m)
            {
                AddFarmKey(key, value.SeedUnits);
                AddFarmKey(key, value.WaterUnits);
                AddFarmKey(key, value.ReservedSeedUnits);
                AddFarmKey(key, value.ReservedWaterUnits);
            }
            var hasFarmSupplyState = value.CultivationUnits.Length > 0
                || value.HarvestLots.Length > 0 || value.PackageLots.Length > 0;
            foreach (var actor in value.Actors)
            {
                AddFarmKey(key, actor.ActorStableId);
                AddFarmKey(key, actor.ActorKindCode);
                AddFarmKey(key, actor.KoreanName);
                AddFarmKey(key, actor.Health);
                AddFarmKey(key, actor.Stamina);
                AddFarmKey(key, actor.Injured);
                AddFarmKey(key, actor.ActiveWorkOrderStableId);
                if (hasFarmSupplyState)
                    foreach (var capability in actor.CapabilityCodes)
                        AddFarmKey(key, capability);
            }
            foreach (var soil in value.SoilTiles)
            {
                AddFarmKey(key, soil.SoilTileStableId);
                AddFarmKey(key, soil.GridX);
                AddFarmKey(key, soil.GridY);
                AddFarmKey(key, soil.StateCode);
                if (value.SeedUnits != 0m || value.WaterUnits != 0m
                    || value.ReservedSeedUnits != 0m || value.ReservedWaterUnits != 0m)
                    AddFarmKey(key, soil.PhysicalAreaSquareMeters);
            }
            foreach (var defense in value.Defenses)
            {
                AddFarmKey(key, defense.DefenseStableId);
                AddFarmKey(key, defense.DefenseKindCode);
                AddFarmKey(key, defense.Durability);
                AddFarmKey(key, defense.Prepared);
                AddFarmKey(key, defense.PresentationKey);
            }
            if (hasFarmSupplyState)
            {
                foreach (var cultivation in value.CultivationUnits)
                    AddCultivationUnitKey(key, cultivation);
                foreach (var harvestLot in value.HarvestLots)
                    AddHarvestLotKey(key, harvestLot);
                foreach (var packageLot in value.PackageLots)
                    AddPackageLotKey(key, packageLot);
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
                if (work.SeedCost != 0m || work.WaterCost != 0m)
                {
                    AddFarmKey(key, work.SeedCost);
                    AddFarmKey(key, work.WaterCost);
                }
                AddFarmKey(key, work.PresentationKey);
                if (hasFarmSupplyState)
                {
                    AddFarmKey(key, work.SelectedSpatialStableId);
                    AddFarmKey(key, work.SpatialDefinitionRevision);
                    AddFarmKey(key, work.SpatialDefinitionHashSha256);
                }
            }
            foreach (var encounter in value.Encounters)
            {
                AddFarmKey(key, encounter.EncounterStableId);
                AddFarmKey(key, encounter.ThreatTypeCode);
                AddFarmKey(key, encounter.ThreatUnitCount);
                AddFarmKey(key, encounter.StateCode);
                AddFarmKey(key, encounter.OccurredWorldTick);
                AddFarmKey(key, encounter.PresentationKey);
                if (value.RuleRevision ==
                    SimulationFarmSurvivalCodes.ScenicSeasonRuleRevision)
                {
                    foreach (var choice in encounter.AvailableChoiceStableIds
                        ?? Array.Empty<string>())
                        AddFarmKey(key, choice);
                    AddFarmKey(key, encounter.DecisionDeadlineWorldTick?.ToString(
                        CultureInfo.InvariantCulture) ?? string.Empty);
                }
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
            AddFarmKey(key, value.Combat.RuleRevision);
            foreach (var perspective in value.Combat.Perspectives)
            {
                AddFarmKey(key, perspective.ActorStableId);
                AddFarmKey(key, perspective.PerspectiveCode);
                AddFarmKey(key, perspective.PresentationKey);
            }
            foreach (var beat in value.Combat.Beats)
            {
                AddFarmKey(key, beat.BeatStableId);
                AddFarmKey(key, beat.EncounterStableId);
                AddFarmKey(key, beat.ActorStableId);
                AddFarmKey(key, beat.AppliedPerspectiveCode);
                AddFarmKey(key, beat.AttackPatternCode);
                AddFarmKey(key, beat.Sequence);
                AddFarmKey(key, beat.StartedWorldTick);
                AddFarmKey(key, beat.ImpactOffsetMs);
                AddFarmKey(key, beat.GuardWindowMs);
                AddFarmKey(key, beat.CounterWindowMs);
                AddFarmKey(key, beat.PerfectGuardWindowMs);
                AddFarmKey(key, beat.PerfectCounterWindowMs);
                AddFarmKey(key, beat.StateCode);
                AddFarmKey(key, beat.ReactionStableId);
                AddFarmKey(key, beat.PresentationKey);
            }
            foreach (var reaction in value.Combat.Reactions)
            {
                AddFarmKey(key, reaction.ReactionStableId);
                AddFarmKey(key, reaction.CommandId);
                AddFarmKey(key, reaction.BeatStableId);
                AddFarmKey(key, reaction.ActorStableId);
                AddFarmKey(key, reaction.ReactionActionCode);
                AddFarmKey(key, reaction.ReactionOffsetMs);
                AddFarmKey(key, reaction.TimingDeltaMs);
                AddFarmKey(key, reaction.GradeCode);
                AddFarmKey(key, reaction.ActorDamageUnits);
                AddFarmKey(key, reaction.DefenseResponseScore);
                AddFarmKey(key, reaction.ThreatStaggered);
                AddFarmKey(key, reaction.PresentationKey);
            }
            AddFarmTacticalCombatKey(key, value.Combat.Tactical);
            return key.ToString();
        }

        internal static void ValidateFarmSurvivalInitialState(
            SimulationFarmSurvivalInitialStateRequest? request)
        {
            if (request == null) return;
            if (request.RuleRevision != SimulationFarmSurvivalCodes.RuleRevision
                && request.RuleRevision !=
                    SimulationFarmSurvivalCodes.InteractiveCombatRuleRevision
                && request.RuleRevision !=
                    SimulationFarmSurvivalCodes.HeroTacticalCombatRuleRevision
                && request.RuleRevision !=
                    SimulationFarmSurvivalCodes.ScenicSeasonRuleRevision)
                throw new SimulationContractException(
                    "SimulationFarmSurvivalRuleUnsupported");
            RequireStableId(request.RegionStableId, "SimulationFarmRegionInvalid");
            RequireStableId(request.AreaStableId, "SimulationFarmAreaInvalid");
            RequireStableId(request.TileKey, "SimulationFarmTileInvalid");
            RequireStableId(request.FarmBuildingStableId,
                "SimulationFarmBuildingInvalid");
            if (request.SupplyUnits < 0m || request.RepairMaterialUnits < 0m
                || request.SeedUnits < 0m || request.WaterUnits < 0m
                || request.Actors == null || request.SoilTiles == null
                || request.Defenses == null || request.CultivationUnits == null
                || request.Actors.Length == 0)
                throw new SimulationContractException(
                    "SimulationFarmSurvivalInitialStateInvalid");

            EnsureUniqueFarmIds(request.Actors.Select(value => value.ActorStableId),
                "SimulationFarmActorInvalid");
            EnsureUniqueFarmIds(request.SoilTiles.Select(value => value.SoilTileStableId),
                "SimulationFarmSoilTileInvalid");
            EnsureUniqueFarmIds(request.Defenses.Select(value => value.DefenseStableId),
                "SimulationFarmDefenseInvalid");
            EnsureUniqueFarmIds(request.CultivationUnits.Select(value =>
                value.CultivationUnitStableId), "SimulationCultivationUnitInvalid");
            foreach (var actor in request.Actors)
            {
                if ((actor.ActorKindCode != SimulationFarmSurvivalCodes.Player
                        && actor.ActorKindCode != SimulationFarmSurvivalCodes.Npc)
                    || string.IsNullOrWhiteSpace(actor.KoreanName)
                    || actor.Health <= 0m || actor.Health > 100m
                    || actor.Stamina < 0m || actor.Stamina > 100m)
                    throw new SimulationContractException(
                        "SimulationFarmActorInvalid");
                EnsureUniqueFarmIds(actor.CapabilityCodes,
                    "SimulationFarmActorCapabilityInvalid");
            }
            if (!request.Actors.Any(value =>
                value.ActorKindCode == SimulationFarmSurvivalCodes.Player))
                throw new SimulationContractException(
                    "SimulationFarmPlayerMissing");
            if (request.RuleRevision ==
                    SimulationFarmSurvivalCodes.HeroTacticalCombatRuleRevision
                && !request.Actors.Any(value => value.ActorKindCode ==
                    SimulationFarmSurvivalCodes.Npc))
                throw new SimulationContractException(
                    "SimulationFarmTacticalNpcMissing");
            foreach (var soil in request.SoilTiles)
            {
                if (soil.StateCode != SimulationFarmSurvivalCodes.Untilled
                    && soil.StateCode != SimulationFarmSurvivalCodes.Tilled
                    || soil.PhysicalAreaSquareMeters <= 0m)
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
            foreach (var cultivation in request.CultivationUnits)
            {
                RequireStableId(cultivation.CultivationUnitStableId,
                    "SimulationCultivationUnitInvalid");
                RequireStableId(cultivation.TileStableId,
                    "SimulationCultivationTileStableIdInvalid");
                RequireStableId(cultivation.CultivationStableId,
                    "SimulationCultivationStableIdInvalid");
                RequireStableId(cultivation.ProductStableId,
                    "SimulationCultivationProductStableIdInvalid");
                RequireStableId(cultivation.CropVariantStableId,
                    "SimulationCultivationVariantStableIdInvalid");
                if (cultivation.Revision <= 0
                    || cultivation.StateCode != Simulation재배단위상태Codes.Growing
                        && cultivation.StateCode != Simulation재배단위상태Codes.HarvestReady
                        && cultivation.StateCode != Simulation재배단위상태Codes.Harvested
                    || cultivation.PhysicalAreaSquareMeters <= 0m
                    || cultivation.EffectiveCultivationAreaRatio <= 0m
                    || cultivation.EffectiveCultivationAreaRatio > 1m
                    || cultivation.SourceStableIds == null
                    || cultivation.SourceStableIds.Length == 0)
                    throw new SimulationContractException("SimulationCultivationUnitInvalid");
            }
            if (request.CultivationUnits.Length > 0)
            {
                var rule = request.PotatoProductionRule
                    ?? throw new SimulationContractException(
                        "SimulationPotatoProductionRuleUnavailable");
                RequireStableId(rule.RuleStableId, "SimulationPotatoProductionRuleInvalid");
                RequireStableId(rule.ProductStableId, "SimulationPotatoProductionRuleInvalid");
                RequireStableId(rule.CropVariantStableId, "SimulationPotatoProductionRuleInvalid");
                if (rule.RuleRevision <= 0 || rule.BaseYieldKilogramsPerSquareMeter <= 0m
                    || rule.SourceStableIds == null || rule.SourceStableIds.Length == 0)
                    throw new SimulationContractException("SimulationPotatoProductionRuleInvalid");
            }
        }

        internal static void ValidateFarmWorkConfirmRequest(
            SimulationFarmWorkConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            ValidateFarmWorkFields(request.ExpectedRevision, request.ActorStableId,
                request.TargetStableId, request.ActionCode, request.AssignmentKindCode);
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationSpatialStableIdInvalid");
        }

        internal static void ValidateFarmWorkPlanConfirmRequest(
            SimulationFarmWorkPlanConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            ValidateFarmWorkPlanItems(request.ExpectedRevision, request.Items);
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
        {
            var legacy = string.Join("|", request.ActorStableId.Trim(),
                request.TargetStableId.Trim(), request.ActionCode.Trim(),
                request.AssignmentKindCode.Trim());
            return string.IsNullOrWhiteSpace(request.PreferredSpatialStableId)
                ? legacy
                : legacy + "|" + request.PreferredSpatialStableId.Trim();
        }

        internal static string BuildFarmWorkPlanPayloadKey(
            SimulationFarmWorkPlanConfirmRequest request)
            => string.Join(";", request.Items.OrderBy(value => value.Priority)
                .ThenBy(value => value.PlanItemStableId, StringComparer.Ordinal)
                .Select(value => string.Join("|", value.PlanItemStableId.Trim(),
                    value.Priority.ToString(CultureInfo.InvariantCulture),
                    value.ActorStableId.Trim(), value.TargetStableId.Trim(),
                    value.ActionCode.Trim(), value.AssignmentKindCode.Trim(),
                    value.PreferredSpatialStableId.Trim())));

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
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationSpatialStableIdInvalid");
        }

        private static void ValidateFarmWorkPlanPreviewRequest(
            SimulationFarmWorkPlanPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateFarmWorkPlanItems(request.ExpectedRevision, request.Items);
        }

        private static void ValidateFarmWorkPlanItems(
            long expectedRevision,
            SimulationFarmWorkPlanItemRequest[] items)
        {
            if (expectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
            if (items == null || items.Length == 0 || items.Length > 32)
                throw new SimulationContractException("SimulationFarmWorkPlanItemsInvalid");
            var itemIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (item == null)
                    throw new SimulationContractException(
                        "SimulationFarmWorkPlanItemInvalid");
                RequireStableId(item.PlanItemStableId,
                    "SimulationFarmWorkPlanItemInvalid");
                if (!itemIds.Add(item.PlanItemStableId.Trim()))
                    throw new SimulationContractException(
                        "SimulationFarmWorkPlanItemDuplicate");
                if (item.Priority < 0)
                    throw new SimulationContractException(
                        "SimulationFarmWorkPlanPriorityInvalid");
                ValidateFarmWorkFields(expectedRevision, item.ActorStableId,
                    item.TargetStableId, item.ActionCode, item.AssignmentKindCode);
                if (!string.IsNullOrWhiteSpace(item.PreferredSpatialStableId))
                    RequireStableId(item.PreferredSpatialStableId,
                        "SimulationSpatialStableIdInvalid");
            }
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
                || appliedFarmWorkPlanCommands.ContainsKey(commandId)
                || appliedThreatResponseCommands.ContainsKey(commandId)
                || HasAppliedFarmCombatCommand(commandId);

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
                : actionCode == SimulationFarmSurvivalCodes.Sowing
                    ? SimulationFarmSurvivalCodes.FarmSowingPresentation
                : actionCode == SimulationFarmSurvivalCodes.CropCare
                    ? SimulationFarmSurvivalCodes.FarmCropCarePresentation
                : actionCode == SimulationFarmSurvivalCodes.Harvesting
                    ? SimulationFarmSurvivalCodes.FarmHarvestPresentation
                : actionCode == SimulationFarmSurvivalCodes.HarvestCollection
                    ? SimulationFarmSurvivalCodes.FarmCollectionPresentation
                : actionCode == SimulationFarmSurvivalCodes.OutboundPacking
                    ? SimulationFarmSurvivalCodes.FarmPackingPresentation
                : actionCode == SimulationFarmSurvivalCodes.MedicalRest
                    ? "survival.actor.recovery"
                : actionCode == SimulationFarmSurvivalCodes.GuardDuty
                    ? SimulationFarmSurvivalCodes.DuskDefense
                    : SimulationFarmSurvivalCodes.TacticalLabor;

        private static string DefensePresentationKey(string defenseKindCode)
            => "survival.defense." + defenseKindCode.ToLowerInvariant();

        private static string DayGoalCode(
            int dayNumber,
            string ruleRevision)
        {
            if (ruleRevision == SimulationFarmSurvivalCodes.ScenicSeasonRuleRevision)
            {
                var chapterDay = (dayNumber - 1) % 28 + 1;
                if (chapterDay == 24) return "SeasonalDefenseWarning";
                if (chapterDay == 25 || chapterDay == 26)
                    return "SeasonalDefensePreparation";
                if (chapterDay == 27) return "SeasonalDefenseChoice";
                if (chapterDay == 28) return "SeasonalRecovery";
                return "ScenicExploration";
            }

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
                AvailableChoiceStableIds = source.AvailableChoiceStableIds?
                    .ToArray() ?? Array.Empty<string>(),
                DecisionDeadlineWorldTick = source.DecisionDeadlineWorldTick,
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

        private static Simulation재배단위Snapshot CloneCultivationUnit(
            Simulation재배단위Snapshot source)
            => new Simulation재배단위Snapshot
            {
                CultivationUnitStableId = source.CultivationUnitStableId,
                Revision = source.Revision,
                TileStableId = source.TileStableId,
                CultivationStableId = source.CultivationStableId,
                ProductStableId = source.ProductStableId,
                CropVariantStableId = source.CropVariantStableId,
                StateCode = source.StateCode,
                PhysicalAreaSquareMeters = source.PhysicalAreaSquareMeters,
                EffectiveCultivationAreaRatio = source.EffectiveCultivationAreaRatio,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static Simulation감자생산RuleSnapshot ClonePotatoProductionRule(
            Simulation감자생산RuleSnapshot source)
            => new Simulation감자생산RuleSnapshot
            {
                RuleStableId = source.RuleStableId,
                RuleRevision = source.RuleRevision,
                SourceTypeCode = source.SourceTypeCode,
                ProductStableId = source.ProductStableId,
                CropVariantStableId = source.CropVariantStableId,
                BaseYieldKilogramsPerSquareMeter = source.BaseYieldKilogramsPerSquareMeter,
                MinimumEnvironmentFactor = source.MinimumEnvironmentFactor,
                MaximumEnvironmentFactor = source.MaximumEnvironmentFactor,
                MinimumInputFactor = source.MinimumInputFactor,
                MaximumInputFactor = source.MaximumInputFactor,
                MinimumFacilityFactor = source.MinimumFacilityFactor,
                MaximumFacilityFactor = source.MaximumFacilityFactor,
                MinimumLossFactor = source.MinimumLossFactor,
                MaximumLossFactor = source.MaximumLossFactor,
                SourceStableIds = source.SourceStableIds.ToArray(),
                Limitations = source.Limitations.ToArray(),
            };

        private static Simulation수확LotSnapshot CloneHarvestLot(Simulation수확LotSnapshot source)
            => new Simulation수확LotSnapshot
            {
                HarvestLotStableId = source.HarvestLotStableId,
                Revision = source.Revision,
                CultivationUnitStableId = source.CultivationUnitStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                StateCode = source.StateCode,
                CausedByTaskStableId = source.CausedByTaskStableId,
                CreatedWorldTick = source.CreatedWorldTick,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static Simulation포장LotSnapshot ClonePackageLot(Simulation포장LotSnapshot source)
            => new Simulation포장LotSnapshot
            {
                PackageLotStableId = source.PackageLotStableId,
                Revision = source.Revision,
                HarvestLotStableId = source.HarvestLotStableId,
                CargoStableId = source.CargoStableId,
                SourceAllocationStableId = source.SourceAllocationStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                StateCode = source.StateCode,
                CausedByTaskStableId = source.CausedByTaskStableId,
                CreatedWorldTick = source.CreatedWorldTick,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static void AddCultivationUnitKey(StringBuilder key, Simulation재배단위Snapshot value)
        {
            AddFarmKey(key, value.CultivationUnitStableId); AddFarmKey(key, value.Revision);
            AddFarmKey(key, value.TileStableId); AddFarmKey(key, value.CultivationStableId);
            AddFarmKey(key, value.ProductStableId); AddFarmKey(key, value.CropVariantStableId);
            AddFarmKey(key, value.StateCode); AddFarmKey(key, value.PhysicalAreaSquareMeters);
            AddFarmKey(key, value.EffectiveCultivationAreaRatio);
            foreach (var source in value.SourceStableIds) AddFarmKey(key, source);
        }

        private static void AddPotatoProductionRuleKey(StringBuilder key,
            Simulation감자생산RuleSnapshot value)
        {
            AddFarmKey(key, value.RuleStableId); AddFarmKey(key, value.RuleRevision);
            AddFarmKey(key, value.SourceTypeCode); AddFarmKey(key, value.ProductStableId);
            AddFarmKey(key, value.CropVariantStableId);
            AddFarmKey(key, value.BaseYieldKilogramsPerSquareMeter);
            AddFarmKey(key, value.MinimumEnvironmentFactor); AddFarmKey(key, value.MaximumEnvironmentFactor);
            AddFarmKey(key, value.MinimumInputFactor); AddFarmKey(key, value.MaximumInputFactor);
            AddFarmKey(key, value.MinimumFacilityFactor); AddFarmKey(key, value.MaximumFacilityFactor);
            AddFarmKey(key, value.MinimumLossFactor); AddFarmKey(key, value.MaximumLossFactor);
            foreach (var source in value.SourceStableIds) AddFarmKey(key, source);
            foreach (var limitation in value.Limitations) AddFarmKey(key, limitation);
        }

        private static void AddHarvestLotKey(StringBuilder key, Simulation수확LotSnapshot value)
        {
            AddFarmKey(key, value.HarvestLotStableId); AddFarmKey(key, value.Revision);
            AddFarmKey(key, value.CultivationUnitStableId); AddFarmKey(key, value.ProductStableId);
            AddFarmKey(key, value.Quantity); AddFarmKey(key, value.UnitCode);
            AddFarmKey(key, value.StateCode); AddFarmKey(key, value.CausedByTaskStableId);
            AddFarmKey(key, value.CreatedWorldTick);
            foreach (var source in value.SourceStableIds) AddFarmKey(key, source);
        }

        private static void AddPackageLotKey(StringBuilder key, Simulation포장LotSnapshot value)
        {
            AddFarmKey(key, value.PackageLotStableId); AddFarmKey(key, value.Revision);
            AddFarmKey(key, value.HarvestLotStableId); AddFarmKey(key, value.CargoStableId);
            AddFarmKey(key, value.SourceAllocationStableId);
            AddFarmKey(key, value.ProductStableId); AddFarmKey(key, value.Quantity);
            AddFarmKey(key, value.UnitCode); AddFarmKey(key, value.StateCode);
            AddFarmKey(key, value.CausedByTaskStableId); AddFarmKey(key, value.CreatedWorldTick);
            foreach (var source in value.SourceStableIds) AddFarmKey(key, source);
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
            public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        }

        private sealed class FarmSoilTileState
        {
            public string SoilTileStableId { get; set; } = string.Empty;
            public int GridX { get; set; }
            public int GridY { get; set; }
            public string StateCode { get; set; } = string.Empty;
            public decimal PhysicalAreaSquareMeters { get; set; }
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
            public decimal SeedCost { get; set; }
            public decimal WaterCost { get; set; }
            public string PresentationKey { get; set; } = string.Empty;
            public string SelectedSpatialStableId { get; set; } = string.Empty;
            public string SpatialDefinitionRevision { get; set; } = string.Empty;
            public string SpatialDefinitionHashSha256 { get; set; } = string.Empty;
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
