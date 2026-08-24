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
        private readonly Dictionary<string, AppliedNatureSurvivalCommand>
            appliedNatureSurvivalCommands =
                new Dictionary<string, AppliedNatureSurvivalCommand>(StringComparer.Ordinal);
        private readonly HashSet<int> natureEncounterEvaluatedCycleIndices = new HashSet<int>();
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

                var soloPaused = string.Equals(hostedSessionModeCode,
                        SimulationHostedWorldCodes.Solo, StringComparison.Ordinal)
                    && IsNaturePauseReason(request.PauseReasonCode);
                natureClockPaused = soloPaused;
                naturePauseReasonCode = soloPaused
                    ? NormalizeOptional(request.PauseReasonCode) : string.Empty;
                if (!soloPaused && request.ElapsedRealtimeSeconds > 0)
                {
                    var previousSecond = natureElapsedSecondsInCycle;
                    var previousCycleIndex = natureCycleIndex;
                    var projection = NatureSurvivalRules.AdvanceClock(
                        natureCycleIndex,
                        natureElapsedSecondsInCycle,
                        request.ElapsedRealtimeSeconds);
                    if (CurrentTick + projection.CompletedCycleCount > DurationTicks)
                        throw new SimulationConflictException(
                            SimulationNatureSurvivalCodes.DurationExceeded);

                    if (request.WorkInputHeld)
                        AdvanceNatureActiveWork(request.ElapsedRealtimeSeconds);
                    natureCycleIndex = projection.CycleIndex;
                    natureElapsedSecondsInCycle = projection.ElapsedSecondsInCycle;
                    RegrowNatureResources();
                    TryTriggerFirstDuskEncounter(previousCycleIndex, previousSecond,
                        request.ElapsedRealtimeSeconds);
                    if (projection.CompletedCycleCount > 0)
                        AdvanceWorldState(projection.CompletedCycleCount);
                }

                Revision++;
                AppendNatureSurvivalClockCommand(request);
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
            if (natureSurvivalCreationState != null
                && !string.Equals(request.PlayerStableId.Trim(),
                    natureSurvivalCreationState.PlayerStableId, StringComparison.Ordinal))
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);

            var action = request.ActionCode.Trim();
            var timber = NaturePlayerItemQuantity(SimulationNatureSurvivalCodes.TimberItemCode);
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
                if (!NaturePlayerHasItem(SimulationNatureSurvivalCodes.AxeItemCode))
                    reasons.Add(SimulationNatureSurvivalCodes.AxeRequired);
                if (natureActiveWork != null)
                    reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                workSeconds = NatureSurvivalRules.HarvestWorkSeconds;
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
                    || natureEncounter.StateCode != SimulationNatureSurvivalCodes.Pending)
                    reasons.Add(SimulationNatureSurvivalCodes.EncounterNotPending);
                if (NormalizeOptional(request.ChoiceCode)
                        != SimulationNatureSurvivalCodes.Fight
                    && NormalizeOptional(request.ChoiceCode)
                        != SimulationNatureSurvivalCodes.Retreat)
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

            return new SimulationNatureSurvivalActionPreviewSnapshot
            {
                SessionStableId = SessionStableId,
                WorldRevision = Revision,
                WorldInteractionId =
                    SimulationNatureSurvivalCodes.WorldInteractionIdForAction(action),
                ActionCode = action,
                TargetStableId = targetStableId,
                CanConfirm = reasons.Count == 0,
                BlockReasonCodes = reasons.Distinct().ToArray(),
                RequiredTimberQuantity = action == SimulationNatureSurvivalCodes.BeginCabinBuild
                    ? NatureSurvivalRules.CabinTimberCost : 0,
                AvailableTimberQuantity = timber,
                RequiredWorkSeconds = workSeconds,
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
                AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                    SimulationNatureSurvivalCodes.AxeItemCode, "기본 도끼", 1);
            }
            else if (action == SimulationNatureSurvivalCodes.BeginHarvest)
            {
                natureActiveWork = new SimulationNatureActiveWorkSnapshot
                {
                    WorkKindCode = SimulationNatureSurvivalCodes.Harvest,
                    TargetStableId = NormalizeOptional(request.TargetStableId),
                    RequiredWorkSeconds = NatureSurvivalRules.HarvestWorkSeconds,
                };
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
                natureEncounter!.StateCode = SimulationNatureSurvivalCodes.Resolved;
                natureEncounter.ResolutionCode = NormalizeOptional(request.ChoiceCode);
                if (NormalizeOptional(request.ChoiceCode)
                    == SimulationNatureSurvivalCodes.Retreat)
                {
                    natureCurrentH2StableId = SimulationNatureSurvivalCodes.HomeH2StableId;
                    natureCurrentH1StableId = SimulationNatureSurvivalCodes.SafeClearingH1StableId;
                    naturePlayerInsideCabin = natureCabin.StateCode
                        == SimulationNatureSurvivalCodes.Completed;
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
                CompleteLatestWorldInteractionManifestation(
                    cancelledWorldInteractionId,
                    new[] { "effect:nature:work-cancelled" },
                    new[] { "WorkCancelled" }, Revision + 1L);
                natureActiveWork = null;
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
                var node = natureResourceNodes[natureActiveWork.TargetStableId];
                node.StateCode = SimulationNatureSurvivalCodes.Stump;
                node.RegrowsAtCycleIndex = natureCycleIndex
                    + NatureSurvivalRules.TreeRegrowthCycleCount;
                AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                    SimulationNatureSurvivalCodes.TimberItemCode, "통나무",
                    NatureSurvivalRules.HarvestTimberQuantity);
                natureNoiseEventCount++;
                CompleteLatestWorldInteractionManifestation(
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                    new[] { "effect:nature:harvest-completed" },
                    new[] { "TimberAdded", "ResourceNodeDepleted" }, Revision + 1L);
            }
            else if (natureActiveWork.WorkKindCode == SimulationNatureSurvivalCodes.CabinBuild)
            {
                natureCabin.CompletedWorkSeconds = natureActiveWork.CompletedWorkSeconds;
                natureCabin.StateCode = SimulationNatureSurvivalCodes.Completed;
                natureCabin.ReservedTimberQuantity = 0;
                natureCabin.RecoveryAvailable = true;
                natureCabin.DefenseAvailable = true;
                natureNoiseEventCount++;
                CompleteLatestWorldInteractionManifestation(
                    SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId,
                    new[] { "effect:nature:cabin-completed" },
                    new[] { "CabinOperational" }, Revision + 1L);
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

        private void RegrowNatureResources()
        {
            foreach (var node in natureResourceNodes.Values)
            {
                if (node.StateCode == SimulationNatureSurvivalCodes.Stump
                    && node.RegrowsAtCycleIndex >= 0
                    && natureCycleIndex >= node.RegrowsAtCycleIndex)
                {
                    node.StateCode = SimulationNatureSurvivalCodes.Standing;
                    node.RegrowsAtCycleIndex = -1;
                }
            }
        }

        private void TryTriggerFirstDuskEncounter(
            int previousCycleIndex,
            int previousSecond,
            int elapsedSeconds)
        {
            if (natureEncounter != null || natureNoiseEventCount <= 0
                || !NatureSurvivalRules.CrossedIntoDusk(previousSecond, elapsedSeconds))
                return;
            var evaluatedCycle = previousSecond < NatureSurvivalRules.DaylightEndsAtSecond
                ? previousCycleIndex : previousCycleIndex + 1;
            if (!natureEncounterEvaluatedCycleIndices.Add(evaluatedCycle)) return;
            if (!NatureSurvivalRules.RollFirstDuskEncounter(
                ScenarioSeed, SessionStableId, evaluatedCycle, natureNoiseEventCount)) return;

            natureEncounter = new SimulationNatureEncounterSnapshot
            {
                EncounterStableId = "nature-encounter:" + evaluatedCycle.ToString(
                    CultureInfo.InvariantCulture) + ":skeleton-placeholder",
                StateCode = SimulationNatureSurvivalCodes.Pending,
                ThreatPresentationCode = SimulationNatureSurvivalCodes.SkeletonPlaceholderCode,
                TriggeredCycleIndex = evaluatedCycle,
                CabinDefenseApplied = natureCabin.StateCode
                    == SimulationNatureSurvivalCodes.Completed,
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
                NoiseEventCount = natureNoiseEventCount,
                PlayerInsideCabin = naturePlayerInsideCabin,
                ResourceNodes = natureResourceNodes.Values
                    .OrderBy(value => value.ResourceNodeStableId, StringComparer.Ordinal)
                    .Select(CloneNatureResourceNode).ToArray(),
                ActiveWork = CloneNatureActiveWork(natureActiveWork),
                Cabin = CloneNatureCabin(natureCabin),
                Encounter = CloneNatureEncounter(natureEncounter),
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private bool NaturePlayerHasItem(string itemCode)
            => NaturePlayerItemQuantity(itemCode) > 0;

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
                SimulationNatureSurvivalCodes.ProfileRevision, StringComparison.Ordinal))
                throw new SimulationContractException("SimulationNatureSurvivalProfileUnsupported");
            RequireStableId(request.PlayerStableId, "SimulationNaturePlayerStableIdInvalid");
            RequireStableId(request.AreaSetStableId, "SimulationNatureAreaSetStableIdInvalid");
            RequireStableId(request.H3StableId, "SimulationNatureH3StableIdInvalid");
            RequireStableId(request.SpawnH2StableId, "SimulationNatureH2StableIdInvalid");
            RequireStableId(request.SpawnH1StableId, "SimulationNatureH1StableIdInvalid");
            if (request.InventoryCapacityUnits <= 0m)
                throw new SimulationContractException("SimulationNatureInventoryCapacityInvalid");
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
            return string.Join("|", new[]
            {
                request.ProfileRevision.Trim(), request.PlayerStableId.Trim(),
                request.AreaSetStableId.Trim(), request.H3StableId.Trim(),
                request.SpawnH2StableId.Trim(), request.SpawnH1StableId.Trim(),
                request.InventoryCapacityUnits.ToString(CultureInfo.InvariantCulture),
                request.StartsWithAxe.ToString(),
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
                ResourceNodes = source.ResourceNodes.Select(value =>
                    new SimulationNatureResourceNodeInitialStateRequest
                    {
                        ResourceNodeStableId = value.ResourceNodeStableId,
                        H2StableId = value.H2StableId,
                        H1StableId = value.H1StableId,
                        LocalX = value.LocalX,
                        LocalZ = value.LocalZ,
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
                NoiseEventCount = source.NoiseEventCount,
                PlayerInsideCabin = source.PlayerInsideCabin,
                ResourceNodes = source.ResourceNodes.Select(CloneNatureResourceNode).ToArray(),
                ActiveWork = CloneNatureActiveWork(source.ActiveWork),
                Cabin = CloneNatureCabin(source.Cabin),
                Encounter = CloneNatureEncounter(source.Encounter),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
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
                WorkKindCode = source.WorkKindCode,
                TargetStableId = source.TargetStableId,
                RequiredWorkSeconds = source.RequiredWorkSeconds,
                CompletedWorkSeconds = source.CompletedWorkSeconds,
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
