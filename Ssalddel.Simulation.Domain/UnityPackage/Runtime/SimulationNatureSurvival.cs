using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, SimulationNatureResourceNodeSnapshot>
            natureResourceNodes = new Dictionary<string, SimulationNatureResourceNodeSnapshot>(
                StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationNatureDroppedTimberSnapshot>
            natureDroppedTimber =
                new Dictionary<string, SimulationNatureDroppedTimberSnapshot>(
                    StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedNatureSurvivalCommand>
            appliedNatureSurvivalCommands =
                new Dictionary<string, AppliedNatureSurvivalCommand>(StringComparer.Ordinal);
        private readonly HashSet<int> natureEncounterEvaluatedCycleIndices = new HashSet<int>();
        private readonly Dictionary<string, long> natureCooperativeActors =
            new Dictionary<string, long>(StringComparer.Ordinal);
        private SimulationNatureSurvivalInitialStateRequest? natureSurvivalCreationState;
        private string natureSurvivalInitialPayloadKey = "none";
        private int natureCycleIndex;
        private int natureElapsedSecondsInCycle;
        private bool natureClockPaused;
        private string naturePauseReasonCode = string.Empty;
        private string natureCurrentH2StableId = string.Empty;
        private string natureCurrentH1StableId = string.Empty;
        private int natureNoiseEventCount;
        private bool naturePlayerInsideCabin;
        private bool natureSleeping;
        private string natureSelectedExpansionPlanCode = string.Empty;
        private bool natureDay2Ready;
        private string natureLinkedCombatStableId = string.Empty;
        private string natureLastCombatResultCode = string.Empty;
        private bool natureExpeditionPrepared;
        private string natureLastProtectedMaterialItemCode = string.Empty;
        private SimulationNatureActiveWorkSnapshot? natureActiveWork;
        private SimulationNatureCabinSnapshot natureCabin = new SimulationNatureCabinSnapshot();
        private SimulationNatureEncounterSnapshot? natureEncounter;

        public SimulationNatureSurvivalStateSnapshot GetNatureSurvivalState()
        {
            lock (gate)
            {
                return CreateNatureSurvivalStateSnapshot();
            }
        }

        /// <summary>
        /// Session revision, Nature 결과, 다음 선택과 행위 기록을 같은 잠금에서
        /// 복제한다. 각 요소를 따로 조회해 서로 다른 revision을 섞지 않는다.
        /// </summary>
        public SimulationNature표현관측Snapshot GetNature표현관측Snapshot()
        {
            lock (gate)
            {
                EnsureNatureSurvivalEnabled();
                return new SimulationNature표현관측Snapshot
                {
                    Session = CreateSnapshot(),
                    Nature = CreateNatureSurvivalStateSnapshot(),
                    BuildingProgression = GetAreaBuildingProgression(
                        Simulation영역건물발전Codes.Nature),
                    PlayerOpportunities = GetNaturePlayerOpportunities(),
                    ActionLedger = GetActionManifestationLedger(),
                };
            }
        }

        public SimulationNatureSurvivalStateSnapshot RegisterNatureCooperativeActor(
            string actorStableId, decimal inventoryCapacityUnits = 24m)
        {
            RequireStableId(actorStableId,
                "SimulationNatureCooperativeActorStableIdInvalid");
            if (inventoryCapacityUnits <= 0m)
                throw new SimulationContractException(
                    "SimulationNatureInventoryCapacityInvalid");
            lock (gate)
            {
                EnsureNatureSurvivalEnabled();
                var actor = actorStableId.Trim();
                if (string.Equals(actor,
                        natureSurvivalCreationState!.PlayerStableId,
                        StringComparison.Ordinal)
                    || natureCooperativeActors.ContainsKey(actor))
                    return CreateNatureSurvivalStateSnapshot();

                RegisterNatureCooperativeActorCore(actor,
                    inventoryCapacityUnits, Revision);
                natureSurvivalCreationState.CooperativeActors =
                    natureCooperativeActors.OrderBy(value => value.Key,
                            StringComparer.Ordinal)
                        .Select(value => new
                            SimulationNatureCooperativeActorInitialStateRequest
                            {
                                ActorStableId = value.Key,
                                InventoryCapacityUnits =
                                    worldInventoryPlayers[value.Key]
                                        .InventoryCapacityUnits,
                                RegisteredWorldRevision = value.Value,
                            }).ToArray();
                return CreateNatureSurvivalStateSnapshot();
            }
        }

        private void RegisterNatureCooperativeActorCore(string actor,
            decimal inventoryCapacityUnits, long registeredWorldRevision)
        {
            worldInventoryPlayers.Add(actor,
                    new SimulationWorldPlayerInventorySnapshot
                    {
                        PlayerStableId = actor,
                        InventoryCapacityUnits = inventoryCapacityUnits,
                        ManagedContainerStableIds = Array.Empty<string>(),
                    });
            AddNaturePlayerItem(actor,
                SimulationNatureSurvivalCodes.AxeItemCode, "기본 도끼", 1);
            if (!natureMindPlayers.ContainsKey(actor))
            {
                var mind = new NatureMindPlayerState
                {
                    PlayerStableId = actor,
                    InterpretationBandCode =
                        SimulationNatureMindCodes.MixedBand,
                };
                natureMindPlayers.Add(actor, mind);
                InitializeNaturePeriodState(mind);
            }
            natureCooperativeActors.Add(actor, registeredWorldRevision);
        }

        public Simulation플레이어기회Snapshot[] GetNaturePlayerOpportunities()
        {
            lock (gate)
            {
                EnsureNatureSurvivalEnabled();
                var requests = natureResourceNodes.Values
                    .Where(value => value.StateCode ==
                        SimulationNatureSurvivalCodes.Standing)
                    .OrderBy(value => value.ResourceNodeStableId,
                        StringComparer.Ordinal)
                    .Take(1)
                    .Select(value => new SimulationNatureSurvivalActionPreviewRequest
                    {
                        ObservedWorldRevision = Revision,
                        PlayerStableId = natureSurvivalCreationState!.PlayerStableId,
                        ActionCode = SimulationNatureSurvivalCodes.BeginHarvest,
                        TargetStableId = value.ResourceNodeStableId,
                    })
                    .Concat(natureDroppedTimber.Values
                        .Where(value => value.StateCode ==
                            SimulationNatureSurvivalCodes.DroppedTimberAvailable)
                        .OrderBy(value => value.DroppedTimberStableId,
                            StringComparer.Ordinal)
                        .Take(1)
                        .Select(value => OpportunityRequest(
                            SimulationNatureSurvivalCodes.CollectDroppedTimber,
                            value.DroppedTimberStableId)))
                    .Concat(new[]
                    {
                        OpportunityRequest(
                            SimulationNatureSurvivalCodes.AcquireHansBrokenAxe,
                            SimulationNatureSurvivalCodes
                                .HansBrokenAxePickupStableId),
                        OpportunityRequest(
                            SimulationNatureSurvivalCodes.RepairHansFarmFence,
                            SimulationNatureSurvivalCodes
                                .HansFarmFenceAggregateStableId),
                        OpportunityRequest(SimulationNatureSurvivalCodes.StoreAtCabin,
                            natureCabin.CabinStableId),
                        OpportunityRequest(
                            SimulationNatureSurvivalCodes.PrepareFieldSupply,
                            Simulation영역건물발전Codes.NatureWorkbenchBlueprint),
                    });

                var opportunities = requests.Select(request =>
                {
                    var preview = CreateNatureSurvivalActionPreview(request);
                    return new Simulation플레이어기회Snapshot
                    {
                        OpportunityStableId = "opportunity:nature:"
                            + preview.WorldInteractionId.ToLowerInvariant()
                            + ":" + preview.TargetStableId.ToLowerInvariant(),
                        PlayerActivityTrackCode = preview.PlayerActivityTrackCode,
                        PlayerFlowCode = preview.PlayerFlowCode,
                        NextPlayerFlowCode = preview.NextPlayerFlowCode,
                        CycleHandoffCode = preview.CycleHandoffCode,
                        WorldInteractionId = preview.WorldInteractionId,
                        WorldInteractionName = preview.WorldInteractionName,
                        WorldInteractionDisplayName =
                            preview.WorldInteractionDisplayName,
                        ResponsibilityKindCode = preview.ResponsibilityKindCode,
                        PrimaryOutcomeCode = preview.PrimaryOutcomeCode,
                        SingleResponsibilityAssessmentCode =
                            preview.SingleResponsibilityAssessmentCode,
                        ActionCode = preview.ActionCode,
                        TargetStableId = preview.TargetStableId,
                        Available = preview.CanConfirm,
                        BlockReasonCodes = preview.BlockReasonCodes.ToArray(),
                    };
                }).ToList();
                if (IsNatureR4)
                    opportunities.Add(
                        CreateNatureFieldSupplyDelegatedOpportunity());
                return opportunities.ToArray();
            }
        }

        public Simulation영역수요Snapshot[] GetNatureAreaNeeds()
        {
            lock (gate)
            {
                EnsureNatureSurvivalEnabled();
                return new[]
                {
                    AreaNeed("need:nature:field-supply:timber",
                        SimulationNatureSurvivalCodes.TimberItemCode,
                        SimulationNatureSurvivalCodes.FieldSupplyTimberCost,
                        NatureAvailableTimberQuantity()),
                    AreaNeed("need:nature:field-supply:rebuild-part",
                        SimulationNatureSurvivalCodes.RebuildPartItemCode,
                        SimulationNatureSurvivalCodes.FieldSupplyRebuildPartCost,
                        NaturePlayerItemQuantity(
                            SimulationNatureSurvivalCodes.RebuildPartItemCode)),
                };
            }
        }

        private SimulationNatureSurvivalActionPreviewRequest OpportunityRequest(
            string actionCode, string targetStableId)
            => new SimulationNatureSurvivalActionPreviewRequest
            {
                ObservedWorldRevision = Revision,
                PlayerStableId = natureSurvivalCreationState!.PlayerStableId,
                ActionCode = actionCode,
                TargetStableId = targetStableId,
            };

        private Simulation영역수요Snapshot AreaNeed(string needCode,
            string itemCode, int requiredQuantity, int availableQuantity)
            => new Simulation영역수요Snapshot
            {
                AreaSetStableId = natureSurvivalCreationState!.AreaSetStableId,
                NeedCode = needCode,
                RequiredItemCode = itemCode,
                RequiredQuantity = requiredQuantity,
                AvailableQuantity = availableQuantity,
                Satisfied = availableQuantity >= requiredQuantity,
            };

        public SimulationNatureSurvivalActionPreviewSnapshot PreviewNatureSurvivalAction(
            SimulationNatureSurvivalActionPreviewRequest request)
        {
            ValidateNatureSurvivalPreviewRequest(request);
            lock (gate)
            {
                return CreateNatureSurvivalActionPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmNatureSurvivalAction(
            SimulationNatureSurvivalCommandRequest request)
        {
            ValidateNatureSurvivalCommandRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildNatureSurvivalActionPayloadKey(request);
                if (appliedNatureSurvivalCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            SimulationNatureSurvivalCodes.CommandPayloadConflict);
                    return Clone(applied.Snapshot);
                }
                EnsureNatureSurvivalEnabled();
                if (HasDifferentKindCommand(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        SimulationNatureSurvivalCodes.ExpectedRevisionMismatch);

                var preview = CreateNatureSurvivalActionPreview(
                    new SimulationNatureSurvivalActionPreviewRequest
                    {
                        ObservedWorldRevision = request.ExpectedRevision,
                        PlayerStableId = request.PlayerStableId,
                        ActionCode = request.ActionCode,
                        TargetStableId = request.TargetStableId,
                        ChoiceCode = request.ChoiceCode,
                        LocalX = request.LocalX,
                        LocalZ = request.LocalZ,
                        YawDegrees = request.YawDegrees,
                    });
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(
                        preview.BlockReasonCodes.FirstOrDefault()
                        ?? SimulationNatureSurvivalCodes.ActionBlocked);

                ApplyNatureSurvivalAction(request);
                Revision++;
                AppendNatureSurvivalActionCommand(request);
                var snapshot = CreateSnapshot();
                appliedNatureSurvivalCommands.Add(commandId,
                    new AppliedNatureSurvivalCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        public 경영SimulationSessionSnapshot AdvanceNatureSurvivalClock(
            SimulationNatureSurvivalClockAdvanceRequest request)
        {
            ValidateNatureSurvivalClockRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildNatureSurvivalClockPayloadKey(request);
                if (appliedNatureSurvivalCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            SimulationNatureSurvivalCodes.CommandPayloadConflict);
                    return Clone(applied.Snapshot);
                }
                EnsureNatureSurvivalEnabled();
                if (HasDifferentKindCommand(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        SimulationNatureSurvivalCodes.ExpectedRevisionMismatch);

                var regeneratedNodeStableIds = Array.Empty<string>();
                var combatPaused = IsNatureR2
                    && natureEncounter?.StateCode ==
                        SimulationNatureSurvivalCodes.CombatActive;
                var soloPaused = string.Equals(hostedSessionModeCode,
                        SimulationHostedWorldCodes.Solo, StringComparison.Ordinal)
                    && IsNaturePauseReason(request.PauseReasonCode);
                natureClockPaused = soloPaused || combatPaused;
                naturePauseReasonCode = combatPaused
                    ? SimulationNatureSurvivalCodes.CombatActiveClockFrozen
                    : soloPaused ? NormalizeOptional(request.PauseReasonCode)
                    : string.Empty;
                if (!natureClockPaused && request.ElapsedRealtimeSeconds > 0)
                {
                    var previousSecond = natureElapsedSecondsInCycle;
                    var previousCycleIndex = natureCycleIndex;
                    var elapsedSeconds = NatureClockElapsedSeconds(request);
                    var projection = NatureSurvivalRules.AdvanceClock(
                        natureCycleIndex,
                        natureElapsedSecondsInCycle,
                        elapsedSeconds);
                    if (CurrentTick + projection.CompletedCycleCount > DurationTicks)
                        throw new SimulationConflictException(
                            SimulationNatureSurvivalCodes.DurationExceeded);

                    TryBeginNatureNpcFieldSupplyWork();
                    if (request.WorkInputHeld || IsNatureNpcFieldSupplyWork)
                        AdvanceNatureActiveWork(elapsedSeconds);
                    natureCycleIndex = projection.CycleIndex;
                    natureElapsedSecondsInCycle = projection.ElapsedSecondsInCycle;
                    AdvanceNatureLearningVisit();
                    regeneratedNodeStableIds = RegrowNatureResources();
                    TryTriggerFirstDuskEncounter(previousCycleIndex, previousSecond,
                        elapsedSeconds);
                    if (natureSleeping && natureElapsedSecondsInCycle >=
                        NatureSurvivalRules.NightEndsAtSecond)
                        natureSleeping = false;
                    if (projection.CompletedCycleCount > 0)
                        AdvanceWorldState(projection.CompletedCycleCount);
                }

                Revision++;
                AppendNatureSurvivalClockCommand(request);
                // v28 이전 저장자료의 명령 재생은 행위 원장을 새로 합성하지 않는다.
                // 현재 Session에서 이미 공통 행위/분야 원장을 사용한 경우에만
                // 같은 clock command와 revision에 자원 재생 결과를 덧붙인다.
                if (regeneratedNodeStableIds.Length > 0
                    && actionManifestationLedgerState != null
                    && playerDomainProfileState != null)
                    AppendNatureResourceRegenerationAction(
                        commandId, regeneratedNodeStableIds);
                var snapshot = CreateSnapshot();
                appliedNatureSurvivalCommands.Add(commandId,
                    new AppliedNatureSurvivalCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private void InitializeNatureSurvival(SimulationNatureSurvivalInitialStateRequest? request)
        {
            ValidateNatureSurvivalInitialState(request);
            natureSurvivalInitialPayloadKey = BuildNatureSurvivalInitialPayloadKey(request);
            natureSurvivalCreationState = CloneNatureSurvivalInitialState(request);
            if (request == null) return;

            natureCurrentH2StableId = request.SpawnH2StableId.Trim();
            natureCurrentH1StableId = request.SpawnH1StableId.Trim();
            natureCabin = new SimulationNatureCabinSnapshot
            {
                CabinStableId = "facility:nature-cabin",
                H2StableId = SimulationNatureSurvivalCodes.HomeH2StableId,
                H1StableId = SimulationNatureSurvivalCodes.CabinSiteH1StableId,
                StateCode = SimulationNatureSurvivalCodes.Planned,
                RequiredWorkSeconds = NatureSurvivalRules.CabinWorkSeconds,
                StorageCapacity = NatureSurvivalRules.CabinStorageCapacity,
            };
            InitializeAreaBuildingProgression(request);
            foreach (var node in request.ResourceNodes)
            {
                natureResourceNodes.Add(node.ResourceNodeStableId.Trim(),
                    new SimulationNatureResourceNodeSnapshot
                    {
                        ResourceNodeStableId = node.ResourceNodeStableId.Trim(),
                        H2StableId = node.H2StableId.Trim(),
                        H1StableId = node.H1StableId.Trim(),
                        LocalX = node.LocalX,
                        LocalZ = node.LocalZ,
                        StateCode = SimulationNatureSurvivalCodes.Standing,
                        RegrowsAtCycleIndex = -1,
                    });
            }
            EnsureNaturePlayerInventory(request);
            InitializeHansFarmFenceRestoration(request);
            foreach (var actor in request.CooperativeActors
                         .OrderBy(value => value.ActorStableId,
                             StringComparer.Ordinal))
                RegisterNatureCooperativeActorCore(
                    actor.ActorStableId.Trim(),
                    actor.InventoryCapacityUnits,
                    actor.RegisteredWorldRevision);
        }

        private void EnsureNaturePlayerInventory(SimulationNatureSurvivalInitialStateRequest request)
        {
            var playerId = request.PlayerStableId.Trim();
            if (!worldInventoryPlayers.ContainsKey(playerId))
            {
                worldInventoryPlayers.Add(playerId, new SimulationWorldPlayerInventorySnapshot
                {
                    PlayerStableId = playerId,
                    CurrentBuildingStableId = string.Empty,
                    InventoryCapacityUnits = request.InventoryCapacityUnits,
                    ManagedContainerStableIds = Array.Empty<string>(),
                });
            }
            if (request.StartsWithAxe)
                AddNaturePlayerItem(playerId, SimulationNatureSurvivalCodes.AxeItemCode,
                    "기본 도끼", 1);
        }

        private SimulationNatureSurvivalActionPreviewSnapshot CreateNatureSurvivalActionPreview(
            SimulationNatureSurvivalActionPreviewRequest request)
        {
            var reasons = new List<string>();
            if (natureSurvivalCreationState == null)
                reasons.Add(SimulationNatureSurvivalCodes.Disabled);
            if (request.ObservedWorldRevision != Revision)
                reasons.Add(SimulationNatureSurvivalCodes.ExpectedRevisionMismatch);
            var actorStableId = request.PlayerStableId.Trim();
            var isPrimaryActor = natureSurvivalCreationState != null
                && string.Equals(actorStableId,
                    natureSurvivalCreationState.PlayerStableId,
                    StringComparison.Ordinal);
            var isCooperativeActor = natureCooperativeActors.ContainsKey(
                actorStableId);
            if (!isPrimaryActor && !isCooperativeActor)
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);

            var action = request.ActionCode.Trim();
            var timber = NaturePlayerItemQuantity(SimulationNatureSurvivalCodes.TimberItemCode);
            SimulationNatureDroppedTimberSnapshot? targetDroppedTimber = null;
            var remainingInventoryCapacity = NatureRemainingInventoryCapacityUnits();
            var workSeconds = 0;
            if (action == SimulationNatureSurvivalCodes.AcquireAxe)
            {
                if (NormalizeOptional(request.TargetStableId)
                        != SimulationNatureSurvivalCodes.AxePickupStableId
                    || NaturePlayerHasItem(SimulationNatureSurvivalCodes.AxeItemCode))
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.BeginHarvest)
            {
                if (!natureResourceNodes.TryGetValue(
                    NormalizeOptional(request.TargetStableId), out var node))
                    reasons.Add(SimulationNatureSurvivalCodes.ResourceNodeNotFound);
                else if (node.StateCode != SimulationNatureSurvivalCodes.Standing)
                    reasons.Add(SimulationNatureSurvivalCodes.ResourceNodeUnavailable);
                if (!(isPrimaryActor
                        ? ActorHasEquippedCapability(actorStableId,
                            SimulationActorEquipmentCodes.Woodcutting)
                        : NatureActorHasItem(actorStableId,
                            SimulationNatureSurvivalCodes.AxeItemCode)))
                    reasons.Add(SimulationNatureSurvivalCodes.AxeRequired);
                if (natureActiveWork != null)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                var choice = NormalizeOptional(request.ChoiceCode);
                if (!string.IsNullOrEmpty(choice)
                    && choice != SimulationNatureSurvivalCodes.UseFieldSupplyPack)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                if (choice == SimulationNatureSurvivalCodes.UseFieldSupplyPack)
                {
                    if (!IsNatureR4 || NaturePlayerItemQuantity(
                            SimulationNatureSurvivalCodes.NatureFieldSupplyPackItemCode) < 1)
                        reasons.Add(SimulationNatureSurvivalCodes.FieldSupplyPackRequired);
                    if (natureExpeditionPrepared)
                        reasons.Add(SimulationNatureSurvivalCodes.ExpeditionAlreadyPrepared);
                }
                workSeconds = NatureSurvivalRules.HarvestWorkSeconds;
            }
            else if (isCooperativeActor)
            {
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.CollectDroppedTimber)
            {
                if (!IsNatureR5)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                if (!natureDroppedTimber.TryGetValue(
                        NormalizeOptional(request.TargetStableId),
                        out targetDroppedTimber))
                    reasons.Add(SimulationNatureSurvivalCodes.DroppedTimberNotFound);
                else if (targetDroppedTimber.StateCode !=
                    SimulationNatureSurvivalCodes.DroppedTimberAvailable)
                    reasons.Add(SimulationNatureSurvivalCodes.DroppedTimberUnavailable);
                if (targetDroppedTimber != null
                    && remainingInventoryCapacity < targetDroppedTimber.Quantity)
                    reasons.Add(SimulationWorldSurvivalInventoryCodes
                        .PlayerCapacityExceeded);
            }
            else if (action ==
                SimulationNatureSurvivalCodes.AcquireHansBrokenAxe
                || action == SimulationNatureSurvivalCodes.RepairHansFarmFence)
            {
                AppendHansFarmFencePreviewReasons(action,
                    NormalizeOptional(request.TargetStableId), reasons);
            }
            else if (action == SimulationNatureSurvivalCodes.PlaceCabinBlueprint)
            {
                if (natureCabin.StateCode != SimulationNatureSurvivalCodes.Planned
                    || Math.Abs(request.LocalX) > 12d || Math.Abs(request.LocalZ) > 12d)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.BeginCabinBuild)
            {
                if (natureCabin.StateCode != SimulationNatureSurvivalCodes.Building
                    || natureCabin.ReservedTimberQuantity > 0)
                    reasons.Add(SimulationNatureSurvivalCodes.CabinBlueprintRequired);
                if (timber < NatureSurvivalRules.CabinTimberCost)
                    reasons.Add(SimulationNatureSurvivalCodes.TimberInsufficient);
                if (natureActiveWork != null)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                workSeconds = NatureSurvivalRules.CabinWorkSeconds;
            }
            else if (action == SimulationNatureSurvivalCodes.ResolveEncounter)
            {
                if (natureEncounter == null
                    || (natureEncounter.StateCode != SimulationNatureSurvivalCodes.Pending
                        && natureEncounter.StateCode !=
                            SimulationNatureSurvivalCodes.CombatActive))
                    reasons.Add(SimulationNatureSurvivalCodes.EncounterNotPending);
                var choice = NormalizeOptional(request.ChoiceCode);
                var pendingChoice = choice == SimulationNatureSurvivalCodes.Fight
                    || choice == SimulationNatureSurvivalCodes.Retreat;
                var combatResult = choice == SimulationNatureSurvivalCodes.Victory
                    || choice == SimulationNatureSurvivalCodes.Defeat
                    || choice == SimulationNatureSurvivalCodes.Retreat;
                if (natureEncounter?.StateCode == SimulationNatureSurvivalCodes.Pending
                    && !pendingChoice)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                if (natureEncounter?.StateCode == SimulationNatureSurvivalCodes.CombatActive
                    && (!IsNatureR2 || !combatResult))
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.EnterCabin)
            {
                if (natureCabin.StateCode != SimulationNatureSurvivalCodes.Completed
                    || naturePlayerInsideCabin)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.LeaveCabin)
            {
                if (!naturePlayerInsideCabin)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.CancelActiveWork)
            {
                if (natureActiveWork == null)
                    reasons.Add(SimulationNatureSurvivalCodes.ActiveWorkRequired);
                else if (!string.IsNullOrWhiteSpace(request.TargetStableId)
                    && !string.Equals(request.TargetStableId.Trim(),
                        natureActiveWork.TargetStableId, StringComparison.Ordinal))
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.StoreAtCabin)
            {
                if (!IsNatureR2)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                if (natureCabin.StateCode != SimulationNatureSurvivalCodes.Completed)
                    reasons.Add(SimulationNatureSurvivalCodes.CabinRequired);
                if (!naturePlayerInsideCabin)
                    reasons.Add(SimulationNatureSurvivalCodes.CabinAccessRequired);
                if (timber <= 0)
                    reasons.Add(SimulationNatureSurvivalCodes.TimberNotCarried);
                if (NatureCabinStoredTimberQuantity() >= natureCabin.StorageCapacity)
                    reasons.Add(SimulationNatureSurvivalCodes.CabinStorageFull);
            }
            else if (action == SimulationNatureSurvivalCodes.SleepInCabin)
            {
                if (!IsNatureR2 || natureCabin.StateCode !=
                    SimulationNatureSurvivalCodes.Completed)
                    reasons.Add(SimulationNatureSurvivalCodes.CabinRequired);
                if (!naturePlayerInsideCabin)
                    reasons.Add(SimulationNatureSurvivalCodes.CabinAccessRequired);
                if (NatureCabinStoredTimberQuantity() <= 0)
                    reasons.Add(SimulationNatureSurvivalCodes
                        .CabinStoredResourceRequired);
                if (NatureSurvivalRules.PhaseAt(natureElapsedSecondsInCycle)
                    != NatureSurvivalClockPhaseCodes.Night)
                    reasons.Add(SimulationNatureSurvivalCodes.NightRequired);
                if (natureSleeping || natureEncounter?.StateCode ==
                    SimulationNatureSurvivalCodes.CombatActive)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.SelectExpansionPlan)
            {
                if (!IsNatureR2 || !KnownExpansionPlan(request.ChoiceCode))
                    reasons.Add(SimulationNatureSurvivalCodes.ExpansionPlanInvalid);
                if (!string.IsNullOrWhiteSpace(natureSelectedExpansionPlanCode))
                    reasons.Add(SimulationNatureSurvivalCodes.ExpansionPlanAlreadySelected);
                if (NatureSurvivalRules.PhaseAt(natureElapsedSecondsInCycle)
                    != NatureSurvivalClockPhaseCodes.Dawn)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }
            else if (action == SimulationNatureSurvivalCodes.BeginBuildingConstruction)
            {
                AppendNatureBuildingPreview(request, reasons, out var buildingBlueprint);
                if (buildingBlueprint != null)
                    workSeconds = buildingBlueprint.ConstructionSeconds;
            }
            else if (action == SimulationNatureSurvivalCodes.PrepareFieldSupply)
            {
                if (!IsNatureR4)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                if (!string.Equals(NormalizeOptional(request.TargetStableId),
                        Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                        StringComparison.Ordinal)
                    || !NatureWorkbenchOperational())
                    reasons.Add(SimulationNatureSurvivalCodes.WorkbenchRequired);
                if (!naturePlayerInsideCabin)
                    reasons.Add(SimulationNatureSurvivalCodes.CabinAccessRequired);
                if (natureActiveWork != null)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                if (NatureAvailableTimberQuantity() <
                    SimulationNatureSurvivalCodes.FieldSupplyTimberCost)
                    reasons.Add(SimulationNatureSurvivalCodes
                        .FieldSupplyTimberInsufficient);
                if (NaturePlayerItemQuantity(
                        SimulationNatureSurvivalCodes.RebuildPartItemCode) <
                    SimulationNatureSurvivalCodes.FieldSupplyRebuildPartCost)
                    reasons.Add(SimulationNatureSurvivalCodes
                        .FieldSupplyRebuildPartInsufficient);
                workSeconds = SimulationNatureSurvivalCodes.FieldSupplyCraftSeconds;
            }
            else
            {
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            }

            var spatial = ResolveNatureSurvivalSpatialEvidence(action);
            var targetStableId = action ==
                    SimulationNatureSurvivalCodes.CancelActiveWork
                    && natureActiveWork != null
                ? natureActiveWork.TargetStableId
                : NormalizeOptional(request.TargetStableId);
            var 세계상호작용Id =
                SimulationNatureSurvivalCodes.WorldInteractionIdForAction(action);

            return new SimulationNatureSurvivalActionPreviewSnapshot
            {
                SessionStableId = SessionStableId,
                WorldRevision = Revision,
                WorldInteractionId = 세계상호작용Id,
                WorldInteractionName = Simulation세계상호작용이름Catalog
                    .한국어기능명(세계상호작용Id),
                WorldInteractionDisplayName = Simulation세계상호작용이름Catalog
                    .한국어표시명(세계상호작용Id),
                ResponsibilityKindCode = Simulation세계상호작용이름Catalog
                    .책임종류코드(세계상호작용Id),
                PrimaryOutcomeCode = Simulation세계상호작용이름Catalog
                    .주요결과코드(세계상호작용Id),
                SingleResponsibilityAssessmentCode =
                    Simulation세계상호작용이름Catalog
                        .단일책임판정코드(세계상호작용Id),
                ActionCode = action,
                TargetStableId = targetStableId,
                PlayerActivityTrackCode =
                    SimulationNatureSurvivalCodes.PlayerActivityTrackCodeForAction(action),
                PlayerFlowCode =
                    SimulationNatureSurvivalCodes.PlayerFlowCodeForAction(action),
                NextPlayerFlowCode =
                    SimulationNatureSurvivalCodes.NextPlayerFlowCodeForAction(action),
                CycleHandoffCode =
                    SimulationNatureSurvivalCodes.CycleHandoffCodeForAction(action),
                CanConfirm = reasons.Count == 0,
                BlockReasonCodes = reasons.Distinct().ToArray(),
                RequiredTimberQuantity = action ==
                    SimulationNatureSurvivalCodes.BeginCabinBuild
                    ? NatureSurvivalRules.CabinTimberCost
                    : action == SimulationNatureSurvivalCodes.BeginBuildingConstruction
                        ? areaBuildingCatalog?.Blueprints.SingleOrDefault(value =>
                            value.BlueprintStableId == targetStableId)
                            ?.RequiredTimberQuantity ?? 0
                        : action == SimulationNatureSurvivalCodes.PrepareFieldSupply
                            ? SimulationNatureSurvivalCodes.FieldSupplyTimberCost
                            : action == SimulationNatureSurvivalCodes
                                .RepairHansFarmFence
                                ? NatureSurvivalRules
                                    .HansFarmFenceRepairTimberCost
                            : 0,
                AvailableTimberQuantity = action ==
                    SimulationNatureSurvivalCodes.BeginBuildingConstruction
                    || action == SimulationNatureSurvivalCodes.PrepareFieldSupply
                    || action == SimulationNatureSurvivalCodes.RepairHansFarmFence
                    ? NatureAvailableTimberQuantity() : timber,
                RequiredWorkSeconds = workSeconds,
                TransferableTimberQuantity = action ==
                    SimulationNatureSurvivalCodes.StoreAtCabin
                    ? Math.Max(0, Math.Min(timber, natureCabin.StorageCapacity
                        - NatureCabinStoredTimberQuantity())) : 0,
                CabinStoredTimberQuantity = NatureCabinStoredTimberQuantity(),
                CabinStorageCapacity = natureCabin.StorageCapacity,
                RequiredRebuildPartQuantity = action ==
                    SimulationNatureSurvivalCodes.BeginBuildingConstruction
                    ? areaBuildingCatalog?.Blueprints.SingleOrDefault(value =>
                        value.BlueprintStableId == targetStableId)
                        ?.RequiredRebuildPartQuantity ?? 0
                    : action == SimulationNatureSurvivalCodes.PrepareFieldSupply
                        ? SimulationNatureSurvivalCodes.FieldSupplyRebuildPartCost
                        : 0,
                AvailableRebuildPartQuantity = NaturePlayerItemQuantity(
                    SimulationNatureSurvivalCodes.RebuildPartItemCode),
                TargetDroppedTimberQuantity = targetDroppedTimber?.Quantity ?? 0,
                RemainingInventoryCapacityUnits = remainingInventoryCapacity,
                BuildingBlueprintStableId = action ==
                    SimulationNatureSurvivalCodes.BeginBuildingConstruction
                    ? targetStableId : string.Empty,
                SimulationOnly = true,
                IsOperationalState = false,
                SpatialEvidenceStateCode = spatial.StateCode,
                SpatialEvidenceReferenceIds = spatial.ReferenceIds,
            };
        }

        private void ApplyNatureSurvivalAction(SimulationNatureSurvivalCommandRequest request)
        {
            var action = request.ActionCode.Trim();
            if (action == SimulationNatureSurvivalCodes.AcquireAxe)
            {
                ApplyNatureAxeAcquisitionToActorEquipment();
            }
            else if (action == SimulationNatureSurvivalCodes.BeginHarvest)
            {
                if (NormalizeOptional(request.ChoiceCode) ==
                    SimulationNatureSurvivalCodes.UseFieldSupplyPack)
                {
                    ConsumeNaturePlayerItem(
                        SimulationNatureSurvivalCodes.NatureFieldSupplyPackItemCode, 1);
                    natureExpeditionPrepared = true;
                    natureLastProtectedMaterialItemCode = string.Empty;
                }
                natureActiveWork = new SimulationNatureActiveWorkSnapshot
                {
                    OriginCommandId = request.CommandId.Trim(),
                    ActorStableId = request.PlayerStableId.Trim(),
                    WorkKindCode = SimulationNatureSurvivalCodes.Harvest,
                    TargetStableId = NormalizeOptional(request.TargetStableId),
                    RequiredWorkSeconds = NatureSurvivalRules.HarvestWorkSeconds,
                };
                BeginNatureHarvestFocus(request);
            }
            else if (action == SimulationNatureSurvivalCodes.CollectDroppedTimber)
            {
                var droppedTimber = natureDroppedTimber[
                    NormalizeOptional(request.TargetStableId)];
                AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                    SimulationNatureSurvivalCodes.TimberItemCode, "통나무",
                    droppedTimber.Quantity);
                droppedTimber.StateCode =
                    SimulationNatureSurvivalCodes.DroppedTimberCollected;
                droppedTimber.CollectedWorldRevision = Revision + 1L;
                CompleteLatestWorldInteractionManifestation(
                    SimulationNatureSurvivalCodes
                        .CollectDroppedTimberWorldInteractionId,
                    new[] { "effect:nature:dropped-timber-collected" },
                    new[] { "TimberCollected", "DroppedTimberRemoved" },
                    Revision + 1L);
            }
            else if (action ==
                SimulationNatureSurvivalCodes.AcquireHansBrokenAxe
                || action == SimulationNatureSurvivalCodes.RepairHansFarmFence)
            {
                ApplyHansFarmFenceAction(action);
            }
            else if (action == SimulationNatureSurvivalCodes.PlaceCabinBlueprint)
            {
                natureCabin.StateCode = SimulationNatureSurvivalCodes.Building;
                natureCabin.LocalX = request.LocalX;
                natureCabin.LocalZ = request.LocalZ;
                natureCabin.YawDegrees = NormalizeYaw(request.YawDegrees);
            }
            else if (action == SimulationNatureSurvivalCodes.BeginCabinBuild)
            {
                ConsumeNaturePlayerItem(SimulationNatureSurvivalCodes.TimberItemCode,
                    NatureSurvivalRules.CabinTimberCost);
                natureCabin.ReservedTimberQuantity = NatureSurvivalRules.CabinTimberCost;
                natureActiveWork = new SimulationNatureActiveWorkSnapshot
                {
                    WorkKindCode = SimulationNatureSurvivalCodes.CabinBuild,
                    TargetStableId = natureCabin.CabinStableId,
                    RequiredWorkSeconds = NatureSurvivalRules.CabinWorkSeconds,
                    CompletedWorkSeconds = natureCabin.CompletedWorkSeconds,
                };
            }
            else if (action == SimulationNatureSurvivalCodes.ResolveEncounter)
            {
                var choice = NormalizeOptional(request.ChoiceCode);
                if (IsNatureR2 && natureEncounter!.StateCode ==
                    SimulationNatureSurvivalCodes.Pending
                    && choice == SimulationNatureSurvivalCodes.Fight)
                {
                    natureLinkedCombatStableId = "battle:nature:"
                        + natureEncounter.EncounterStableId;
                    natureEncounter.StateCode =
                        SimulationNatureSurvivalCodes.CombatActive;
                    natureEncounter.LinkedCombatStableId = natureLinkedCombatStableId;
                }
                else if (IsNatureR2 && natureEncounter!.StateCode ==
                    SimulationNatureSurvivalCodes.CombatActive)
                {
                    ApplyNatureCombatResult(choice,
                        request.AuthoritativeRewardBonusQuantity);
                }
                else
                {
                    natureEncounter!.StateCode = SimulationNatureSurvivalCodes.Resolved;
                    natureEncounter.ResolutionCode = choice;
                    if (choice == SimulationNatureSurvivalCodes.Retreat)
                    {
                        natureLastCombatResultCode = choice;
                        EndNatureExpedition();
                        ReturnNaturePlayerToHome();
                    }
                }
            }
            else if (action == SimulationNatureSurvivalCodes.EnterCabin)
            {
                naturePlayerInsideCabin = true;
                natureCurrentH1StableId = natureCabin.H1StableId;
            }
            else if (action == SimulationNatureSurvivalCodes.LeaveCabin)
            {
                naturePlayerInsideCabin = false;
                natureCurrentH1StableId = SimulationNatureSurvivalCodes.SafeClearingH1StableId;
            }
            else if (action == SimulationNatureSurvivalCodes.CancelActiveWork)
            {
                var cancelled = natureActiveWork!;
                var cancelledWorldInteractionId = cancelled.WorkKindCode ==
                    SimulationNatureSurvivalCodes.CabinBuild
                    ? SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId
                    : cancelled.WorkKindCode ==
                        Simulation영역건물발전Codes.ExpansionBuildWorkKind
                        ? Simulation영역건물발전Codes.ConstructionWorldInteractionId
                        : cancelled.WorkKindCode ==
                            SimulationNatureSurvivalCodes.FieldSupplyCraft
                            ? SimulationNatureSurvivalCodes
                                .PrepareFieldSupplyWorldInteractionId
                            : cancelled.WorkKindCode ==
                                SimulationNatureSurvivalCodes.FieldSupplyNpcCraft
                                ? SimulationNatureSurvivalCodes
                                    .PrepareFieldSupplyDelegatedWorldInteractionId
                            : SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId;
                if (cancelled.WorkKindCode == SimulationNatureSurvivalCodes.CabinBuild)
                {
                    if (natureCabin.ReservedTimberQuantity > 0)
                    {
                        AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                            SimulationNatureSurvivalCodes.TimberItemCode, "통나무",
                            natureCabin.ReservedTimberQuantity);
                    }
                    natureCabin.ReservedTimberQuantity = 0;
                    natureCabin.CompletedWorkSeconds = 0;
                }
                else if (cancelled.WorkKindCode ==
                    Simulation영역건물발전Codes.ExpansionBuildWorkKind)
                {
                    CancelNatureBuildingConstruction(cancelled);
                }
                else if (cancelled.WorkKindCode ==
                    SimulationNatureSurvivalCodes.FieldSupplyCraft
                    || cancelled.WorkKindCode ==
                    SimulationNatureSurvivalCodes.FieldSupplyNpcCraft)
                {
                    AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                        SimulationNatureSurvivalCodes.TimberItemCode, "통나무",
                        cancelled.ReservedTimberQuantity);
                    AddNaturePlayerItem(natureSurvivalCreationState.PlayerStableId,
                        SimulationNatureSurvivalCodes.RebuildPartItemCode, "재건 부품",
                        cancelled.ReservedRebuildPartQuantity);
                    if (cancelled.WorkKindCode ==
                        SimulationNatureSurvivalCodes.FieldSupplyNpcCraft)
                        CancelNatureNpcFieldSupplyWork();
                }
                CompleteLatestWorldInteractionManifestation(
                    cancelledWorldInteractionId,
                    new[] { "effect:nature:work-cancelled" },
                    new[] { "WorkCancelled" }, Revision + 1L);
                if (cancelled.WorkKindCode ==
                    SimulationNatureSurvivalCodes.Harvest)
                    VoidNatureHarvestFocus(cancelled.OriginCommandId);
                natureActiveWork = null;
            }
            else if (action == SimulationNatureSurvivalCodes.StoreAtCabin)
            {
                StoreNatureTimberAtCabin(request.CommandId.Trim());
            }
            else if (action == SimulationNatureSurvivalCodes.SleepInCabin)
            {
                natureSleeping = true;
            }
            else if (action == SimulationNatureSurvivalCodes.SelectExpansionPlan)
            {
                natureSelectedExpansionPlanCode = NormalizeOptional(request.ChoiceCode);
                natureDay2Ready = true;
            }
            else if (action == SimulationNatureSurvivalCodes.BeginBuildingConstruction)
            {
                BeginNatureBuildingConstruction(request);
            }
            else if (action == SimulationNatureSurvivalCodes.PrepareFieldSupply)
            {
                ConsumeNatureBuildingTimber(
                    SimulationNatureSurvivalCodes.FieldSupplyTimberCost);
                ConsumeNaturePlayerItem(
                    SimulationNatureSurvivalCodes.RebuildPartItemCode,
                    SimulationNatureSurvivalCodes.FieldSupplyRebuildPartCost);
                natureActiveWork = new SimulationNatureActiveWorkSnapshot
                {
                    WorkKindCode = SimulationNatureSurvivalCodes.FieldSupplyCraft,
                    TargetStableId = Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                    RequiredWorkSeconds =
                        SimulationNatureSurvivalCodes.FieldSupplyCraftSeconds,
                    ReservedTimberQuantity =
                        SimulationNatureSurvivalCodes.FieldSupplyTimberCost,
                    ReservedRebuildPartQuantity =
                        SimulationNatureSurvivalCodes.FieldSupplyRebuildPartCost,
                };
            }
        }

        private void AdvanceNatureActiveWork(int elapsedSeconds)
        {
            if (natureActiveWork == null) return;
            natureActiveWork.CompletedWorkSeconds = Math.Min(
                natureActiveWork.RequiredWorkSeconds,
                natureActiveWork.CompletedWorkSeconds + elapsedSeconds);
            if (natureActiveWork.CompletedWorkSeconds < natureActiveWork.RequiredWorkSeconds)
                return;

            if (natureActiveWork.WorkKindCode == SimulationNatureSurvivalCodes.Harvest)
            {
                var completedWork = CloneNatureActiveWork(natureActiveWork)!;
                var node = natureResourceNodes[natureActiveWork.TargetStableId];
                node.StateCode = SimulationNatureSurvivalCodes.Stump;
                node.RegrowsAtCycleIndex = natureCycleIndex
                    + NatureSurvivalRules.TreeRegrowthCycleCount;
                if (IsNatureR5)
                    CreateNatureDroppedTimber(node,
                        NatureSurvivalRules.HarvestTimberQuantity,
                        Revision + 1L);
                else
                    AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                        SimulationNatureSurvivalCodes.TimberItemCode, "통나무",
                        NatureSurvivalRules.HarvestTimberQuantity);
                natureNoiseEventCount++;
                CompleteLatestWorldInteractionManifestation(
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                    new[] { "effect:nature:harvest-completed" },
                    IsNatureR5
                        ? new[] { "DroppedTimberCreated", "ResourceNodeDepleted" }
                        : new[] { "TimberAdded", "ResourceNodeDepleted" },
                    Revision + 1L);
                CompleteNatureHarvestActionAndFocus(completedWork,
                    Revision + 1L);
            }
            else if (natureActiveWork.WorkKindCode == SimulationNatureSurvivalCodes.CabinBuild)
            {
                natureCabin.CompletedWorkSeconds = natureActiveWork.CompletedWorkSeconds;
                natureCabin.StateCode = SimulationNatureSurvivalCodes.Completed;
                natureCabin.ReservedTimberQuantity = 0;
                natureCabin.RecoveryAvailable = true;
                natureCabin.DefenseAvailable = true;
                if (IsNatureR2) EnsureNatureCabinStorage();
                natureNoiseEventCount++;
                CompleteLatestWorldInteractionManifestation(
                    SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId,
                    new[] { "effect:nature:cabin-completed" },
                    new[] { "CabinOperational" }, Revision + 1L);
            }
            else if (natureActiveWork.WorkKindCode ==
                Simulation영역건물발전Codes.ExpansionBuildWorkKind)
            {
                CompleteNatureBuildingConstruction();
            }
            else if (natureActiveWork.WorkKindCode ==
                SimulationNatureSurvivalCodes.FieldSupplyCraft
                || natureActiveWork.WorkKindCode ==
                SimulationNatureSurvivalCodes.FieldSupplyNpcCraft)
            {
                var delegated = natureActiveWork.WorkKindCode ==
                    SimulationNatureSurvivalCodes.FieldSupplyNpcCraft;
                AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                    SimulationNatureSurvivalCodes.NatureFieldSupplyPackItemCode,
                    "Nature 현장 보급 꾸러미", 1);
                CompleteLatestWorldInteractionManifestation(
                    delegated
                        ? SimulationNatureSurvivalCodes
                            .PrepareFieldSupplyDelegatedWorldInteractionId
                        : SimulationNatureSurvivalCodes
                            .PrepareFieldSupplyWorldInteractionId,
                    new[] { "effect:nature:field-supply-crafted" },
                    new[] { "NatureFieldSupplyPackAdded" }, Revision + 1L);
                if (delegated) CompleteNatureNpcFieldSupplyWork();
            }
            natureActiveWork = null;
        }

        private NatureSpatialEvidence ResolveNatureSurvivalSpatialEvidence(
            string actionCode)
        {
            var evidenceWorldInteractionId =
                SimulationNatureSurvivalCodes.WorldInteractionIdForAction(actionCode);
            if (actionCode == SimulationNatureSurvivalCodes.CancelActiveWork
                && natureActiveWork != null)
            {
                evidenceWorldInteractionId = natureActiveWork.WorkKindCode ==
                    SimulationNatureSurvivalCodes.CabinBuild
                    ? SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId
                    : natureActiveWork.WorkKindCode ==
                        Simulation영역건물발전Codes.ExpansionBuildWorkKind
                        ? Simulation영역건물발전Codes.ConstructionWorldInteractionId
                        : natureActiveWork.WorkKindCode ==
                            SimulationNatureSurvivalCodes.FieldSupplyCraft
                            ? SimulationNatureSurvivalCodes
                                .PrepareFieldSupplyWorldInteractionId
                            : natureActiveWork.WorkKindCode ==
                                SimulationNatureSurvivalCodes.FieldSupplyNpcCraft
                                ? SimulationNatureSurvivalCodes
                                    .PrepareFieldSupplyDelegatedWorldInteractionId
                            : SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId;
            }

            var spatialStableId =
                SimulationNatureSurvivalCodes.ActualE5SpatialStableId(
                    evidenceWorldInteractionId);
            if (!spatialDefinitions.TryGetValue(spatialStableId, out var definition))
            {
                return new NatureSpatialEvidence(
                    SimulationWorldInteractionSpatialEvidenceCodes.RequiredMissing,
                    new[]
                    {
                        "e9-wi-h:" + evidenceWorldInteractionId.ToLowerInvariant(),
                    });
            }

            var references = new[]
                {
                    definition.SpatialStableId,
                    definition.LandscapeGraphStableId,
                    definition.LandscapeNodeStableId,
                    "spatial-definition-sha256:" + definition.DefinitionHashSha256,
                }
                .Concat(definition.SourceStableIds)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            return new NatureSpatialEvidence(
                SimulationWorldInteractionSpatialEvidenceCodes.Bound,
                references);
        }

        private string[] RegrowNatureResources()
        {
            var regenerated = new List<string>();
            foreach (var node in natureResourceNodes.Values.OrderBy(
                         value => value.ResourceNodeStableId,
                         StringComparer.Ordinal))
            {
                if (node.StateCode == SimulationNatureSurvivalCodes.Stump
                    && node.RegrowsAtCycleIndex >= 0
                    && natureCycleIndex >= node.RegrowsAtCycleIndex)
                {
                    node.StateCode = SimulationNatureSurvivalCodes.Standing;
                    node.RegrowsAtCycleIndex = -1;
                    regenerated.Add(node.ResourceNodeStableId);
                }
            }
            return regenerated.ToArray();
        }

        private void AppendNatureResourceRegenerationAction(
            string commandId,
            string[] regeneratedNodeStableIds)
        {
            var regeneratedNodes = regeneratedNodeStableIds
                .Select(value => natureResourceNodes[value])
                .ToArray();
            var worldSystemStableId =
                "world-system:nature-resource-regeneration";
            AppendActionManifestationAndProgression(
                new Simulation행위발현Record
                {
                    WorldStableId = TerritoryStableId,
                    SessionStableId = SessionStableId,
                    PlayableLoopStableId =
                        "playable-loop:nature-night-day2.v1",
                    WorldInteractionId =
                        Simulation세계자원재생Codes.WorldInteractionId,
                    CommandId = commandId,
                    TriggerSourceCode =
                        SimulationWorldInteractionTriggerSourceCodes.WorldDerived,
                    InitiatorStableId = worldSystemStableId,
                    ActorStableId = worldSystemStableId,
                    ActorKindCode = "WorldSystem",
                    TargetStableIds = regeneratedNodeStableIds.ToArray(),
                    OutcomeStableId =
                        "outcome:resource-regeneration:" + commandId,
                    PrimaryOutcomeCode = "ResourceAvailabilityRestored",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = new[]
                    {
                        Simulation세계자원재생Codes.ResourceAvailabilityChanged,
                    },
                    영향공간StableIds = regeneratedNodes
                        .SelectMany(value => new[]
                        {
                            value.H2StableId,
                            value.H1StableId,
                        })
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray(),
                    SourceReferenceIds = new[]
                    {
                        "rule:" + Simulation세계자원재생Codes.RuleRevision,
                        "player-progression:not-applicable:" +
                        Simulation세계자원재생Codes
                            .PlayerProgressionNotApplicableReason,
                        "rule:nature-tree-regrowth-cycle-count:" +
                        NatureSurvivalRules.TreeRegrowthCycleCount.ToString(
                            CultureInfo.InvariantCulture),
                    }.Concat(RegenerationOriginCommandReferences(
                        regeneratedNodeStableIds)).ToArray(),
                    BeforeWorldRevision = Revision - 1,
                    AfterWorldRevision = Revision,
                    AppliedWorldTick = CurrentTick,
                    RuleRevision =
                        Simulation세계자원재생Codes.RuleRevision,
                },
                progressionPlayerStableId:
                    natureSurvivalCreationState!.PlayerStableId);
        }

        private string[] RegenerationOriginCommandReferences(
            string[] regeneratedNodeStableIds)
        {
            if (actionManifestationLedgerState == null)
                return Array.Empty<string>();
            var targets = regeneratedNodeStableIds.ToHashSet(StringComparer.Ordinal);
            return actionManifestationLedgerState.TailRecords
                .Where(value => value.WorldInteractionId ==
                                SimulationNatureSurvivalCodes
                                    .BeginHarvestWorldInteractionId
                                && value.PrimaryOutcomeCode == "HarvestCompleted"
                                && value.TargetStableIds.Any(target =>
                                    targets.Contains(target)))
                .Select(value => value.CommandId.EndsWith(":completed",
                        StringComparison.Ordinal)
                    ? value.CommandId[..^":completed".Length]
                    : value.CommandId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .Select(value => "origin-command:" + value)
                .ToArray();
        }

        private void TryTriggerFirstDuskEncounter(
            int previousCycleIndex,
            int previousSecond,
            int elapsedSeconds)
        {
            if ((natureEncounter != null && natureEncounter.StateCode !=
                    SimulationNatureSurvivalCodes.Resolved)
                || natureNoiseEventCount <= 0
                || !NatureSurvivalRules.CrossedIntoDusk(previousSecond, elapsedSeconds))
                return;
            var evaluatedCycle = previousSecond < NatureSurvivalRules.DaylightEndsAtSecond
                ? previousCycleIndex : previousCycleIndex + 1;
            if (!natureEncounterEvaluatedCycleIndices.Add(evaluatedCycle)) return;
            var triggers = IsNatureR2
                ? NatureSurvivalRules.ShouldTriggerDuskEncounter(
                    ScenarioSeed, SessionStableId, evaluatedCycle, natureNoiseEventCount)
                : NatureSurvivalRules.RollFirstDuskEncounter(
                    ScenarioSeed, SessionStableId, evaluatedCycle, natureNoiseEventCount);
            if (!triggers) return;

            var rawThreat = NatureSurvivalRules.NoiseThreatTier(natureNoiseEventCount);
            var cabinDefense = natureCabin.StateCode ==
                SimulationNatureSurvivalCodes.Completed;
            var effectiveThreat = NatureSurvivalRules.EffectiveThreatTier(
                natureNoiseEventCount, cabinDefense);

            natureEncounter = new SimulationNatureEncounterSnapshot
            {
                EncounterStableId = "nature-encounter:" + evaluatedCycle.ToString(
                    CultureInfo.InvariantCulture) + ":skeleton-placeholder",
                StateCode = SimulationNatureSurvivalCodes.Pending,
                ThreatPresentationCode = SimulationNatureSurvivalCodes.SkeletonPlaceholderCode,
                TriggeredCycleIndex = evaluatedCycle,
                CabinDefenseApplied = cabinDefense,
                RawThreatTier = rawThreat,
                EffectiveThreatTier = effectiveThreat,
                HostileCount = NatureSurvivalRules.EncounterHostileCount(
                    natureNoiseEventCount, cabinDefense),
            };
        }

        private SimulationNatureSurvivalStateSnapshot CreateNatureSurvivalStateSnapshot()
        {
            if (natureSurvivalCreationState == null)
                return new SimulationNatureSurvivalStateSnapshot();
            return new SimulationNatureSurvivalStateSnapshot
            {
                IsEnabled = true,
                ProfileRevision = natureSurvivalCreationState.ProfileRevision,
                PlayerStableId = natureSurvivalCreationState.PlayerStableId,
                AreaSetStableId = natureSurvivalCreationState.AreaSetStableId,
                H3StableId = natureSurvivalCreationState.H3StableId,
                CurrentH2StableId = natureCurrentH2StableId,
                CurrentH1StableId = natureCurrentH1StableId,
                CycleIndex = natureCycleIndex,
                ElapsedSecondsInCycle = natureElapsedSecondsInCycle,
                ClockPhaseCode = NatureSurvivalRules.PhaseAt(natureElapsedSecondsInCycle),
                ClockPaused = natureClockPaused,
                PauseReasonCode = naturePauseReasonCode,
                HasAxe = NaturePlayerHasItem(SimulationNatureSurvivalCodes.AxeItemCode),
                TimberQuantity = NaturePlayerItemQuantity(
                    SimulationNatureSurvivalCodes.TimberItemCode),
                StoredTimberQuantity = NatureCabinStoredTimberQuantity(),
                NoiseEventCount = natureNoiseEventCount,
                RawThreatTier = NatureSurvivalRules.NoiseThreatTier(
                    natureNoiseEventCount),
                EffectiveThreatTier = NatureSurvivalRules.EffectiveThreatTier(
                    natureNoiseEventCount, natureCabin.DefenseAvailable),
                RebuildPartQuantity = NaturePlayerItemQuantity(
                    SimulationNatureSurvivalCodes.RebuildPartItemCode),
                FieldSupplyPackQuantity = NaturePlayerItemQuantity(
                    SimulationNatureSurvivalCodes.NatureFieldSupplyPackItemCode),
                ExpeditionPrepared = natureExpeditionPrepared,
                LastProtectedMaterialItemCode =
                    natureLastProtectedMaterialItemCode,
                LinkedCombatStableId = natureLinkedCombatStableId,
                LastCombatResultCode = natureLastCombatResultCode,
                Sleeping = natureSleeping,
                SelectedExpansionPlanCode = natureSelectedExpansionPlanCode,
                Day2Ready = natureDay2Ready,
                BuildingProgression = CreateNatureBuildingProgressionSnapshot(),
                LearningVisit = CloneLearningVisit(natureLearningVisit),
                PlayerInsideCabin = naturePlayerInsideCabin,
                ResourceNodes = natureResourceNodes.Values
                    .OrderBy(value => value.ResourceNodeStableId, StringComparer.Ordinal)
                    .Select(CloneNatureResourceNode).ToArray(),
                DroppedTimber = natureDroppedTimber.Values
                    .OrderBy(value => value.DroppedTimberStableId,
                        StringComparer.Ordinal)
                    .Select(CloneNatureDroppedTimber).ToArray(),
                CooperativeActors = natureCooperativeActors
                    .OrderBy(value => value.Key, StringComparer.Ordinal)
                    .Select(value => new SimulationNatureCooperativeActorSnapshot
                    {
                        ActorStableId = value.Key,
                        InventoryCapacityUnits = worldInventoryPlayers[value.Key]
                            .InventoryCapacityUnits,
                        HasAxe = NatureActorHasItem(value.Key,
                            SimulationNatureSurvivalCodes.AxeItemCode),
                        TimberQuantity = NatureActorItemQuantity(value.Key,
                            SimulationNatureSurvivalCodes.TimberItemCode),
                        RegisteredWorldRevision = value.Value,
                    }).ToArray(),
                ActiveWork = CloneNatureActiveWork(natureActiveWork),
                ActiveFocusChallenge = CloneFocusChallenge(
                    natureActiveFocusChallenge),
                LastFocusResult = CloneFocusResult(natureLastFocusResult),
                Cabin = CloneNatureCabin(natureCabin),
                Encounter = CloneNatureEncounter(natureEncounter),
                HansFarmFenceRestoration =
                    CreateHansFarmFenceRestorationSnapshot(),
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private bool NaturePlayerHasItem(string itemCode)
            => NaturePlayerItemQuantity(itemCode) > 0;

        private bool NatureActorHasItem(string actorStableId, string itemCode)
            => NatureActorItemQuantity(actorStableId, itemCode) > 0;

        private int NatureActorItemQuantity(string actorStableId,
            string itemCode)
            => decimal.ToInt32(PlayerItemQuantity(actorStableId, itemCode,
                SimulationNatureSurvivalCodes.UnitEach));

        private int NaturePlayerItemQuantity(string itemCode)
        {
            if (natureSurvivalCreationState == null) return 0;
            var quantity = PlayerItemQuantity(natureSurvivalCreationState.PlayerStableId,
                itemCode, SimulationNatureSurvivalCodes.UnitEach);
            return decimal.ToInt32(quantity);
        }

        private void AddNaturePlayerItem(string playerStableId, string itemCode,
            string koreanName, int quantity)
        {
            var key = PlayerItemKey(playerStableId, itemCode,
                SimulationNatureSurvivalCodes.UnitEach);
            if (!worldInventoryPlayerItems.TryGetValue(key, out var item))
            {
                item = new SimulationWorldPlayerItemSnapshot
                {
                    ItemCode = itemCode,
                    KoreanName = koreanName,
                    UnitCode = SimulationNatureSurvivalCodes.UnitEach,
                };
                worldInventoryPlayerItems.Add(key, item);
            }
            item.Quantity += quantity;
        }

        private void ConsumeNaturePlayerItem(string itemCode, int quantity)
        {
            var playerId = natureSurvivalCreationState!.PlayerStableId;
            var key = PlayerItemKey(playerId, itemCode, SimulationNatureSurvivalCodes.UnitEach);
            if (!worldInventoryPlayerItems.TryGetValue(key, out var item)
                || item.Quantity < quantity)
                throw new SimulationConflictException(
                    SimulationNatureSurvivalCodes.TimberInsufficient);
            item.Quantity -= quantity;
        }

        private bool IsNatureR2 => natureSurvivalCreationState != null
            && SimulationNatureSurvivalCodes.IsR2(
                natureSurvivalCreationState.ProfileRevision);

        private bool IsNatureR4 => natureSurvivalCreationState != null
            && SimulationNatureSurvivalCodes.IsR4(
                natureSurvivalCreationState.ProfileRevision);

        private bool IsNatureR5 => natureSurvivalCreationState != null
            && SimulationNatureSurvivalCodes.IsR5(
                natureSurvivalCreationState.ProfileRevision);

        private bool IsNatureR6 => natureSurvivalCreationState != null
            && SimulationNatureSurvivalCodes.IsR6(
                natureSurvivalCreationState.ProfileRevision);

        private decimal NatureRemainingInventoryCapacityUnits()
        {
            if (natureSurvivalCreationState == null
                || !worldInventoryPlayers.TryGetValue(
                    natureSurvivalCreationState.PlayerStableId, out var player))
                return 0m;
            return Math.Max(0m, player.InventoryCapacityUnits
                - PlayerTotalQuantity(player.PlayerStableId));
        }

        private void CreateNatureDroppedTimber(
            SimulationNatureResourceNodeSnapshot node, int quantity,
            long createdWorldRevision)
        {
            var stableId = "drop:nature:timber:"
                + node.ResourceNodeStableId + ":cycle:" + natureCycleIndex
                    .ToString(CultureInfo.InvariantCulture);
            if (natureDroppedTimber.ContainsKey(stableId))
                throw new SimulationConflictException(
                    "SimulationNatureDroppedTimberDuplicate");
            natureDroppedTimber.Add(stableId,
                new SimulationNatureDroppedTimberSnapshot
                {
                    DroppedTimberStableId = stableId,
                    SourceResourceNodeStableId = node.ResourceNodeStableId,
                    H2StableId = node.H2StableId,
                    H1StableId = node.H1StableId,
                    LocalX = node.LocalX,
                    LocalZ = node.LocalZ,
                    Quantity = quantity,
                    UnitCode = SimulationNatureSurvivalCodes.UnitEach,
                    StateCode = SimulationNatureSurvivalCodes
                        .DroppedTimberAvailable,
                    CreatedWorldRevision = createdWorldRevision,
                });
        }

        private int NatureClockElapsedSeconds(
            SimulationNatureSurvivalClockAdvanceRequest request)
        {
            if (!natureSleeping) return request.ElapsedRealtimeSeconds;
            var remainingNight = Math.Max(0,
                NatureSurvivalRules.NightEndsAtSecond - natureElapsedSecondsInCycle);
            return Math.Min(remainingNight, checked(request.ElapsedRealtimeSeconds
                * NatureSurvivalRules.SleepNightTimeMultiplier));
        }

        private static bool KnownExpansionPlan(string choiceCode)
        {
            var choice = NormalizeOptional(choiceCode);
            return choice == SimulationNatureSurvivalCodes.Workbench
                || choice == SimulationNatureSurvivalCodes.StorageRack
                || choice == SimulationNatureSurvivalCodes.Palisade;
        }

        private void ApplyNatureCombatResult(string resultCode,
            int authoritativeRewardBonusQuantity)
        {
            if (natureEncounter == null || natureEncounter.StateCode !=
                SimulationNatureSurvivalCodes.CombatActive)
                throw new SimulationConflictException(
                    SimulationNatureSurvivalCodes.EncounterNotPending);

            if (authoritativeRewardBonusQuantity < 0
                || authoritativeRewardBonusQuantity > 2
                || resultCode != SimulationNatureSurvivalCodes.Victory
                   && authoritativeRewardBonusQuantity != 0)
                throw new SimulationContractException(
                    "SimulationNatureCombatRewardBonusInvalid");

            if (resultCode == SimulationNatureSurvivalCodes.Victory)
            {
                AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                    SimulationNatureSurvivalCodes.RebuildPartItemCode,
                    "재건 부품", Math.Max(1, natureEncounter.HostileCount)
                        + authoritativeRewardBonusQuantity);
            }
            else if (resultCode == SimulationNatureSurvivalCodes.Defeat)
            {
                natureLastProtectedMaterialItemCode =
                    LoseHalfOfCarriedNatureMaterials(natureExpeditionPrepared);
            }
            natureEncounter.StateCode = SimulationNatureSurvivalCodes.Resolved;
            natureEncounter.ResolutionCode = resultCode;
            natureLastCombatResultCode = resultCode;
            natureClockPaused = false;
            naturePauseReasonCode = string.Empty;
            EndNatureExpedition(clearLastProtectedMaterial: resultCode !=
                SimulationNatureSurvivalCodes.Defeat);
            ReturnNaturePlayerToHome();
        }

        private string LoseHalfOfCarriedNatureMaterials(bool protectOneStack)
        {
            var prefix = natureSurvivalCreationState!.PlayerStableId + "|";
            var carriedMaterials = worldInventoryPlayerItems.Where(value =>
                    value.Key.StartsWith(prefix, StringComparison.Ordinal)
                    && value.Value.ItemCode.StartsWith("material:",
                        StringComparison.Ordinal)
                    && decimal.Floor(value.Value.Quantity * .5m) > 0)
                .OrderBy(value => value.Value.ItemCode, StringComparer.Ordinal)
                .ToArray();
            var protectedItemCode = protectOneStack && carriedMaterials.Length > 0
                ? carriedMaterials[0].Value.ItemCode : string.Empty;
            foreach (var entry in carriedMaterials)
            {
                if (string.Equals(entry.Value.ItemCode, protectedItemCode,
                        StringComparison.Ordinal))
                    continue;
                entry.Value.Quantity -= decimal.Floor(entry.Value.Quantity * .5m);
            }
            return protectedItemCode;
        }

        private void EndNatureExpedition(bool clearLastProtectedMaterial = true)
        {
            natureExpeditionPrepared = false;
            if (clearLastProtectedMaterial)
                natureLastProtectedMaterialItemCode = string.Empty;
        }

        private bool NatureWorkbenchOperational()
            => natureBuildingNodes.TryGetValue(
                    Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                    out var workbench)
                && workbench.StateCode == Simulation영역건물발전Codes.Operational;

        private void ReturnNaturePlayerToHome()
        {
            natureCurrentH2StableId = SimulationNatureSurvivalCodes.HomeH2StableId;
            naturePlayerInsideCabin = natureCabin.StateCode ==
                SimulationNatureSurvivalCodes.Completed;
            natureCurrentH1StableId = naturePlayerInsideCabin
                ? natureCabin.H1StableId
                : SimulationNatureSurvivalCodes.SafeClearingH1StableId;
        }

        private void EnsureNatureCabinStorage()
        {
            var playerId = natureSurvivalCreationState!.PlayerStableId;
            if (string.IsNullOrWhiteSpace(worldInventoryRuleRevision))
                worldInventoryRuleRevision =
                    SimulationWorldSurvivalInventoryCodes.RuleRevision;
            if (!worldInventoryBuildings.ContainsKey(natureCabin.CabinStableId))
            {
                worldInventoryBuildings.Add(natureCabin.CabinStableId,
                    new SimulationWorldBuildingInteriorSnapshot
                    {
                        BuildingStableId = natureCabin.CabinStableId,
                        TileKey = natureCabin.H1StableId,
                        RegionStableId = SimulationNatureSurvivalCodes.AreaSetStableId,
                        BuildingEvidenceKindCode =
                            SimulationWorldSurvivalInventoryCodes.SimulationScenario,
                        SourceRecordStableId = natureCabin.CabinStableId,
                        InteriorSpaceStableId = natureCabin.H1StableId + ":interior",
                        InteriorEvidenceKindCode =
                            SimulationWorldSurvivalInventoryCodes.SimulationScenario,
                    });
            }
            if (!worldInventoryContainers.ContainsKey(
                SimulationNatureSurvivalCodes.CabinStorageContainerStableId))
            {
                worldInventoryContainers.Add(
                    SimulationNatureSurvivalCodes.CabinStorageContainerStableId,
                    new SimulationWorldContainerSnapshot
                    {
                        ContainerStableId = SimulationNatureSurvivalCodes
                            .CabinStorageContainerStableId,
                        BuildingStableId = natureCabin.CabinStableId,
                        InteriorSpaceStableId = natureCabin.H1StableId + ":interior",
                        AccessPolicyCode =
                            SimulationWorldSurvivalInventoryCodes.ManagerOnly,
                        CapacityUnits = natureCabin.StorageCapacity,
                        ManagerPlayerStableIds = new[] { playerId },
                        EvidenceKindCode =
                            SimulationWorldSurvivalInventoryCodes.SimulationScenario,
                    });
            }
            if (!worldInventoryItemStacks.ContainsKey(
                SimulationNatureSurvivalCodes.CabinStorageTimberStackStableId))
            {
                worldInventoryItemStacks.Add(
                    SimulationNatureSurvivalCodes.CabinStorageTimberStackStableId,
                    new SimulationWorldItemStackSnapshot
                    {
                        ItemStackStableId = SimulationNatureSurvivalCodes
                            .CabinStorageTimberStackStableId,
                        ContainerStableId = SimulationNatureSurvivalCodes
                            .CabinStorageContainerStableId,
                        ItemCode = SimulationNatureSurvivalCodes.TimberItemCode,
                        KoreanName = "통나무",
                        UnitCode = SimulationNatureSurvivalCodes.UnitEach,
                        BuildingItemRelationStableId = natureCabin.CabinStableId,
                        EvidenceKindCode =
                            SimulationWorldSurvivalInventoryCodes.SimulationScenario,
                    });
            }
            if (worldInventoryPlayers.TryGetValue(playerId, out var player)
                && !player.ManagedContainerStableIds.Contains(
                    SimulationNatureSurvivalCodes.CabinStorageContainerStableId,
                    StringComparer.Ordinal))
            {
                player.ManagedContainerStableIds = player.ManagedContainerStableIds
                    .Concat(new[]
                    {
                        SimulationNatureSurvivalCodes.CabinStorageContainerStableId,
                    }).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            }
        }

        private int NatureCabinStoredTimberQuantity()
            => worldInventoryItemStacks.TryGetValue(
                SimulationNatureSurvivalCodes.CabinStorageTimberStackStableId,
                out var stack) ? decimal.ToInt32(stack.Quantity) : 0;

        private void StoreNatureTimberAtCabin(string commandId)
        {
            EnsureNatureCabinStorage();
            var carried = NaturePlayerItemQuantity(
                SimulationNatureSurvivalCodes.TimberItemCode);
            var quantity = Math.Max(0, Math.Min(carried,
                natureCabin.StorageCapacity - NatureCabinStoredTimberQuantity()));
            if (quantity <= 0)
                throw new SimulationConflictException(
                    SimulationNatureSurvivalCodes.CabinStorageFull);

            ConsumeNaturePlayerItem(SimulationNatureSurvivalCodes.TimberItemCode,
                quantity);
            var stack = worldInventoryItemStacks[
                SimulationNatureSurvivalCodes.CabinStorageTimberStackStableId];
            stack.Quantity += quantity;
            worldInventoryTransfers.Add(new SimulationWorldItemTransferSnapshot
            {
                TransferStableId = "world-item-transfer:" + commandId,
                CommandId = commandId,
                PlayerStableId = natureSurvivalCreationState!.PlayerStableId,
                BuildingStableId = natureCabin.CabinStableId,
                SourceContainerStableId = "player-inventory:"
                    + natureSurvivalCreationState.PlayerStableId,
                SourceItemStackStableId = "player-item:"
                    + SimulationNatureSurvivalCodes.TimberItemCode,
                ItemCode = SimulationNatureSurvivalCodes.TimberItemCode,
                Quantity = quantity,
                UnitCode = SimulationNatureSurvivalCodes.UnitEach,
                AppliedWorldTick = CurrentTick,
                AppliedWorldRevision = Revision + 1L,
                EvidenceKindCode =
                    SimulationWorldSurvivalInventoryCodes.SimulationScenario,
                SimulationOnly = true,
            });
        }

        private void EnsureNatureSurvivalEnabled()
        {
            if (natureSurvivalCreationState == null)
                throw new SimulationConflictException(SimulationNatureSurvivalCodes.Disabled);
        }

        private bool HasAppliedNatureSurvivalCommand(string commandId)
            => appliedNatureSurvivalCommands.ContainsKey(commandId);

        private static bool IsNaturePauseReason(string pauseReasonCode)
            => string.Equals(pauseReasonCode?.Trim(), SimulationNatureSurvivalCodes.Menu,
                    StringComparison.Ordinal)
                || string.Equals(pauseReasonCode?.Trim(),
                    SimulationNatureSurvivalCodes.ApplicationInactive,
                    StringComparison.Ordinal);

        private static double NormalizeYaw(double yaw)
        {
            var normalized = yaw % 360d;
            return normalized < 0d ? normalized + 360d : normalized;
        }

        private static void ValidateNatureSurvivalInitialState(
            SimulationNatureSurvivalInitialStateRequest? request)
        {
            if (request == null) return;
            if (!string.Equals(request.ProfileRevision,
                    SimulationNatureSurvivalCodes.ProfileRevisionR1,
                    StringComparison.Ordinal)
                && !string.Equals(request.ProfileRevision,
                    SimulationNatureSurvivalCodes.ProfileRevisionR2,
                    StringComparison.Ordinal)
                && !string.Equals(request.ProfileRevision,
                    SimulationNatureSurvivalCodes.ProfileRevisionR3,
                    StringComparison.Ordinal)
                && !string.Equals(request.ProfileRevision,
                    SimulationNatureSurvivalCodes.ProfileRevisionR4,
                    StringComparison.Ordinal)
                && !string.Equals(request.ProfileRevision,
                    SimulationNatureSurvivalCodes.ProfileRevisionR5,
                    StringComparison.Ordinal)
                && !string.Equals(request.ProfileRevision,
                    SimulationNatureSurvivalCodes.ProfileRevisionR6,
                    StringComparison.Ordinal))
                throw new SimulationContractException("SimulationNatureSurvivalProfileUnsupported");
            RequireStableId(request.PlayerStableId, "SimulationNaturePlayerStableIdInvalid");
            RequireStableId(request.AreaSetStableId, "SimulationNatureAreaSetStableIdInvalid");
            RequireStableId(request.H3StableId, "SimulationNatureH3StableIdInvalid");
            RequireStableId(request.SpawnH2StableId, "SimulationNatureH2StableIdInvalid");
            RequireStableId(request.SpawnH1StableId, "SimulationNatureH1StableIdInvalid");
            if (request.InventoryCapacityUnits <= 0m)
                throw new SimulationContractException("SimulationNatureInventoryCapacityInvalid");
            Simulation집중판정Policy.Create(request.FocusAccessibilityModeCode);
            if (request.ResourceNodes == null || request.ResourceNodes.Length == 0)
                throw new SimulationContractException("SimulationNatureResourceNodesMissing");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var node in request.ResourceNodes)
            {
                RequireStableId(node.ResourceNodeStableId,
                    "SimulationNatureResourceNodeStableIdInvalid");
                RequireStableId(node.H2StableId, "SimulationNatureResourceH2Invalid");
                RequireStableId(node.H1StableId, "SimulationNatureResourceH1Invalid");
                if (!ids.Add(node.ResourceNodeStableId.Trim()))
                    throw new SimulationContractException("SimulationNatureResourceNodeDuplicate");
            }
            foreach (var actor in request.CooperativeActors
                ?? Array.Empty<SimulationNatureCooperativeActorInitialStateRequest>())
            {
                RequireStableId(actor.ActorStableId,
                    "SimulationNatureCooperativeActorStableIdInvalid");
                if (actor.InventoryCapacityUnits <= 0m
                    || string.Equals(actor.ActorStableId.Trim(),
                        request.PlayerStableId.Trim(), StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationNatureCooperativeActorInvalid");
            }
        }

        private static void ValidateNatureSurvivalPreviewRequest(
            SimulationNatureSurvivalActionPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.PlayerStableId, "SimulationNaturePlayerStableIdInvalid");
            RequireText(request.ActionCode, "SimulationNatureActionCodeMissing");
            if (request.ObservedWorldRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
        }

        internal static void ValidateNatureSurvivalCommandRequest(
            SimulationNatureSurvivalCommandRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            RequireStableId(request.PlayerStableId, "SimulationNaturePlayerStableIdInvalid");
            RequireText(request.ActionCode, "SimulationNatureActionCodeMissing");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
        }

        internal static void ValidateNatureSurvivalClockRequest(
            SimulationNatureSurvivalClockAdvanceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.ElapsedRealtimeSeconds < 0 || request.ElapsedRealtimeSeconds > 60)
                throw new SimulationContractException("SimulationNatureElapsedSecondsInvalid");
        }

        private static string BuildNatureSurvivalInitialPayloadKey(
            SimulationNatureSurvivalInitialStateRequest? request)
        {
            if (request == null) return "none";
            var key = string.Join("|", new[]
            {
                request.ProfileRevision.Trim(), request.PlayerStableId.Trim(),
                request.AreaSetStableId.Trim(), request.H3StableId.Trim(),
                request.SpawnH2StableId.Trim(), request.SpawnH1StableId.Trim(),
                request.InventoryCapacityUnits.ToString(CultureInfo.InvariantCulture),
                request.StartsWithAxe.ToString(),
                request.HansFarmFenceRestorationEnabled.ToString(),
                request.FocusAccessibilityModeCode,
                request.BuildingProgressionCatalog?.Revision ?? string.Empty,
                request.BuildingProgressionCatalog?.HashSha256 ?? string.Empty,
                string.Join(";", request.ResourceNodes
                    .OrderBy(value => value.ResourceNodeStableId, StringComparer.Ordinal)
                    .Select(value => string.Join(",", new[]
                    {
                        value.ResourceNodeStableId.Trim(), value.H2StableId.Trim(),
                        value.H1StableId.Trim(),
                        value.LocalX.ToString("R", CultureInfo.InvariantCulture),
                        value.LocalZ.ToString("R", CultureInfo.InvariantCulture),
                    }))),
            });
            if (request.CooperativeActors == null
                || request.CooperativeActors.Length == 0) return key;
            return key + "|" + string.Join(";", request.CooperativeActors
                .OrderBy(value => value.ActorStableId, StringComparer.Ordinal)
                .Select(value => string.Join(",", new[]
                {
                    value.ActorStableId.Trim(),
                    value.InventoryCapacityUnits.ToString(
                        CultureInfo.InvariantCulture),
                    value.RegisteredWorldRevision.ToString(
                        CultureInfo.InvariantCulture),
                })));
        }

        private static string BuildNatureSurvivalActionPayloadKey(
            SimulationNatureSurvivalCommandRequest request)
            => string.Join("|", new[]
            {
                request.PlayerStableId.Trim(), request.ActionCode.Trim(),
                NormalizeOptional(request.TargetStableId),
                NormalizeOptional(request.ChoiceCode),
                request.LocalX.ToString("R", CultureInfo.InvariantCulture),
                request.LocalZ.ToString("R", CultureInfo.InvariantCulture),
                request.YawDegrees.ToString("R", CultureInfo.InvariantCulture),
                request.ExpectedRevision.ToString(CultureInfo.InvariantCulture),
            });

        private static string BuildNatureSurvivalClockPayloadKey(
            SimulationNatureSurvivalClockAdvanceRequest request)
            => string.Join("|", new[]
            {
                request.ElapsedRealtimeSeconds.ToString(CultureInfo.InvariantCulture),
                request.WorkInputHeld.ToString(), NormalizeOptional(request.PauseReasonCode),
                request.ExpectedRevision.ToString(CultureInfo.InvariantCulture),
            });

        private static string NormalizeOptional(string? value)
            => value?.Trim() ?? string.Empty;

        internal static SimulationNatureSurvivalInitialStateRequest?
            CloneNatureSurvivalInitialState(SimulationNatureSurvivalInitialStateRequest? source)
            => source == null ? null : new SimulationNatureSurvivalInitialStateRequest
            {
                ProfileRevision = source.ProfileRevision,
                PlayerStableId = source.PlayerStableId,
                AreaSetStableId = source.AreaSetStableId,
                H3StableId = source.H3StableId,
                SpawnH2StableId = source.SpawnH2StableId,
                SpawnH1StableId = source.SpawnH1StableId,
                InventoryCapacityUnits = source.InventoryCapacityUnits,
                StartsWithAxe = source.StartsWithAxe,
                HansFarmFenceRestorationEnabled =
                    source.HansFarmFenceRestorationEnabled,
                FocusAccessibilityModeCode = source.FocusAccessibilityModeCode,
                BuildingProgressionCatalog = source.BuildingProgressionCatalog == null
                    ? null : Simulation영역건물발전Catalog.Clone(
                        source.BuildingProgressionCatalog),
                ResourceNodes = source.ResourceNodes.Select(value =>
                    new SimulationNatureResourceNodeInitialStateRequest
                    {
                        ResourceNodeStableId = value.ResourceNodeStableId,
                        H2StableId = value.H2StableId,
                        H1StableId = value.H1StableId,
                        LocalX = value.LocalX,
                        LocalZ = value.LocalZ,
                    }).ToArray(),
                CooperativeActors = (source.CooperativeActors
                    ?? Array.Empty<SimulationNatureCooperativeActorInitialStateRequest>())
                    .Select(value => new
                        SimulationNatureCooperativeActorInitialStateRequest
                        {
                            ActorStableId = value.ActorStableId,
                            InventoryCapacityUnits = value.InventoryCapacityUnits,
                            RegisteredWorldRevision =
                                value.RegisteredWorldRevision,
                        }).ToArray(),
            };

        internal static SimulationNatureSurvivalStateSnapshot CloneNatureSurvivalState(
            SimulationNatureSurvivalStateSnapshot source)
            => new SimulationNatureSurvivalStateSnapshot
            {
                IsEnabled = source.IsEnabled,
                ProfileRevision = source.ProfileRevision,
                PlayerStableId = source.PlayerStableId,
                AreaSetStableId = source.AreaSetStableId,
                H3StableId = source.H3StableId,
                CurrentH2StableId = source.CurrentH2StableId,
                CurrentH1StableId = source.CurrentH1StableId,
                CycleIndex = source.CycleIndex,
                ElapsedSecondsInCycle = source.ElapsedSecondsInCycle,
                ClockPhaseCode = source.ClockPhaseCode,
                ClockPaused = source.ClockPaused,
                PauseReasonCode = source.PauseReasonCode,
                HasAxe = source.HasAxe,
                TimberQuantity = source.TimberQuantity,
                StoredTimberQuantity = source.StoredTimberQuantity,
                NoiseEventCount = source.NoiseEventCount,
                RawThreatTier = source.RawThreatTier,
                EffectiveThreatTier = source.EffectiveThreatTier,
                RebuildPartQuantity = source.RebuildPartQuantity,
                FieldSupplyPackQuantity = source.FieldSupplyPackQuantity,
                ExpeditionPrepared = source.ExpeditionPrepared,
                LastProtectedMaterialItemCode =
                    source.LastProtectedMaterialItemCode,
                LinkedCombatStableId = source.LinkedCombatStableId,
                LastCombatResultCode = source.LastCombatResultCode,
                Sleeping = source.Sleeping,
                SelectedExpansionPlanCode = source.SelectedExpansionPlanCode,
                Day2Ready = source.Day2Ready,
                BuildingProgression = source.BuildingProgression == null ? null
                    : CloneAreaBuildingProgression(source.BuildingProgression),
                LearningVisit = CloneLearningVisit(source.LearningVisit),
                PlayerInsideCabin = source.PlayerInsideCabin,
                ResourceNodes = source.ResourceNodes.Select(CloneNatureResourceNode).ToArray(),
                DroppedTimber = source.DroppedTimber
                    .Select(CloneNatureDroppedTimber).ToArray(),
                CooperativeActors = (source.CooperativeActors
                    ?? Array.Empty<SimulationNatureCooperativeActorSnapshot>())
                    .Select(value => new SimulationNatureCooperativeActorSnapshot
                    {
                        ActorStableId = value.ActorStableId,
                        InventoryCapacityUnits = value.InventoryCapacityUnits,
                        HasAxe = value.HasAxe,
                        TimberQuantity = value.TimberQuantity,
                        RegisteredWorldRevision = value.RegisteredWorldRevision,
                    }).ToArray(),
                ActiveWork = CloneNatureActiveWork(source.ActiveWork),
                ActiveFocusChallenge = CloneFocusChallenge(
                    source.ActiveFocusChallenge),
                LastFocusResult = CloneFocusResult(source.LastFocusResult),
                Cabin = CloneNatureCabin(source.Cabin),
                Encounter = CloneNatureEncounter(source.Encounter),
                HansFarmFenceRestoration =
                    CloneHansFarmFenceRestoration(
                        source.HansFarmFenceRestoration),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private static SimulationNatureDroppedTimberSnapshot
            CloneNatureDroppedTimber(SimulationNatureDroppedTimberSnapshot source)
            => new SimulationNatureDroppedTimberSnapshot
            {
                DroppedTimberStableId = source.DroppedTimberStableId,
                SourceResourceNodeStableId = source.SourceResourceNodeStableId,
                H2StableId = source.H2StableId,
                H1StableId = source.H1StableId,
                LocalX = source.LocalX,
                LocalZ = source.LocalZ,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                StateCode = source.StateCode,
                CreatedWorldRevision = source.CreatedWorldRevision,
                CollectedWorldRevision = source.CollectedWorldRevision,
            };

        private static SimulationNatureResourceNodeSnapshot CloneNatureResourceNode(
            SimulationNatureResourceNodeSnapshot source)
            => new SimulationNatureResourceNodeSnapshot
            {
                ResourceNodeStableId = source.ResourceNodeStableId,
                H2StableId = source.H2StableId,
                H1StableId = source.H1StableId,
                LocalX = source.LocalX,
                LocalZ = source.LocalZ,
                StateCode = source.StateCode,
                RegrowsAtCycleIndex = source.RegrowsAtCycleIndex,
            };

        private static SimulationNatureActiveWorkSnapshot? CloneNatureActiveWork(
            SimulationNatureActiveWorkSnapshot? source)
            => source == null ? null : new SimulationNatureActiveWorkSnapshot
            {
                OriginCommandId = source.OriginCommandId,
                ActorStableId = source.ActorStableId,
                WorkKindCode = source.WorkKindCode,
                TargetStableId = source.TargetStableId,
                RequiredWorkSeconds = source.RequiredWorkSeconds,
                CompletedWorkSeconds = source.CompletedWorkSeconds,
                ReservedTimberQuantity = source.ReservedTimberQuantity,
                ReservedRebuildPartQuantity = source.ReservedRebuildPartQuantity,
            };

        private static SimulationNatureCabinSnapshot CloneNatureCabin(
            SimulationNatureCabinSnapshot source)
            => new SimulationNatureCabinSnapshot
            {
                CabinStableId = source.CabinStableId,
                H2StableId = source.H2StableId,
                H1StableId = source.H1StableId,
                StateCode = source.StateCode,
                LocalX = source.LocalX,
                LocalZ = source.LocalZ,
                YawDegrees = source.YawDegrees,
                ReservedTimberQuantity = source.ReservedTimberQuantity,
                CompletedWorkSeconds = source.CompletedWorkSeconds,
                RequiredWorkSeconds = source.RequiredWorkSeconds,
                StorageCapacity = source.StorageCapacity,
                RecoveryAvailable = source.RecoveryAvailable,
                DefenseAvailable = source.DefenseAvailable,
            };

        private static SimulationNatureEncounterSnapshot? CloneNatureEncounter(
            SimulationNatureEncounterSnapshot? source)
            => source == null ? null : new SimulationNatureEncounterSnapshot
            {
                EncounterStableId = source.EncounterStableId,
                StateCode = source.StateCode,
                ThreatPresentationCode = source.ThreatPresentationCode,
                TriggeredCycleIndex = source.TriggeredCycleIndex,
                ResolutionCode = source.ResolutionCode,
                CabinDefenseApplied = source.CabinDefenseApplied,
                RawThreatTier = source.RawThreatTier,
                EffectiveThreatTier = source.EffectiveThreatTier,
                HostileCount = source.HostileCount,
                LinkedCombatStableId = source.LinkedCombatStableId,
            };

        private sealed class AppliedNatureSurvivalCommand
        {
            public AppliedNatureSurvivalCommand(string payloadKey,
                경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }

        private sealed class NatureSpatialEvidence
        {
            public NatureSpatialEvidence(string stateCode, string[] referenceIds)
            {
                StateCode = stateCode;
                ReferenceIds = referenceIds;
            }

            public string StateCode { get; }
            public string[] ReferenceIds { get; }
        }
    }
}
