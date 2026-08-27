using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, SimulationItemDefinitionSnapshot>
            actorItemDefinitions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationOwnedItemInstanceSnapshot>
            actorItemInstances = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> actorEquipmentSlots =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedActorEquipmentCommand>
            appliedActorEquipmentCommands = new(StringComparer.Ordinal);
        private SimulationActorEquipmentInitialStateRequest? actorEquipmentCreationState;
        private string actorEquipmentInitialPayloadKey = "none";
        private long actorEquipmentRevision;
        private bool actorEquipmentLegacyBridge;

        public SimulationActorEquipmentStateSnapshot GetActorEquipmentState()
        {
            lock (gate)
                return CreateActorEquipmentStateSnapshot();
        }

        public SimulationActorItemAcquirePreviewSnapshot PreviewActorItemAcquire(
            SimulationActorItemAcquirePreviewRequest request)
        {
            ValidateActorItemAcquirePreviewRequest(request);
            lock (gate)
                return CreateActorItemAcquirePreview(request);
        }

        public SimulationActorEquipmentStateSnapshot ConfirmActorItemAcquire(
            SimulationActorItemAcquireConfirmRequest request)
        {
            ValidateActorItemAcquireConfirmRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildActorItemAcquirePayloadKey(request);
                if (appliedActorEquipmentCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey,
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            SimulationActorEquipmentCodes.CommandPayloadConflict);
                    return CloneActorEquipmentState(applied.Snapshot);
                }
                if (HasDifferentKindCommand(commandId))
                    throw new SimulationConflictException(
                        "SimulationCommandKindConflict");

                var preview = CreateActorItemAcquirePreview(new()
                {
                    ObservedEquipmentRevision = request.ExpectedEquipmentRevision,
                    ActorStableId = request.ActorStableId,
                    ItemInstanceStableId = request.ItemInstanceStableId,
                    SpecializationWorldInteractionId =
                        request.SpecializationWorldInteractionId,
                });
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(
                        preview.BlockReasonCodes.First());

                AcquireActorItemInstance(request.ItemInstanceStableId.Trim(),
                    autoEquipMainHand: false);
                actorEquipmentRevision++;
                AppendActorItemAcquireCommand(request);
                var snapshot = CreateActorEquipmentStateSnapshot();
                appliedActorEquipmentCommands.Add(commandId,
                    new AppliedActorEquipmentCommand(payloadKey,
                        CloneActorEquipmentState(snapshot)));
                return snapshot;
            }
        }

        public SimulationActorEquipmentChangePreviewSnapshot
            PreviewActorEquipmentChange(
                SimulationActorEquipmentChangePreviewRequest request)
        {
            ValidateActorEquipmentChangePreviewRequest(request);
            lock (gate)
                return CreateActorEquipmentChangePreview(request);
        }

        public SimulationActorEquipmentStateSnapshot ConfirmActorEquipmentChange(
            SimulationActorEquipmentChangeConfirmRequest request)
        {
            ValidateActorEquipmentChangeConfirmRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildActorEquipmentChangePayloadKey(request);
                if (appliedActorEquipmentCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey,
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            SimulationActorEquipmentCodes.CommandPayloadConflict);
                    return CloneActorEquipmentState(applied.Snapshot);
                }
                if (HasDifferentKindCommand(commandId))
                    throw new SimulationConflictException(
                        "SimulationCommandKindConflict");

                var preview = CreateActorEquipmentChangePreview(new()
                {
                    ObservedEquipmentRevision = request.ExpectedEquipmentRevision,
                    ActorStableId = request.ActorStableId,
                    OperationCode = request.OperationCode,
                    ItemInstanceStableId = request.ItemInstanceStableId,
                    SlotCode = request.SlotCode,
                    SwapItemInstanceStableId = request.SwapItemInstanceStableId,
                    SpecializationWorldInteractionId =
                        request.SpecializationWorldInteractionId,
                });
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(
                        preview.BlockReasonCodes.First());

                ApplyActorEquipmentChange(request);
                actorEquipmentRevision++;
                AppendActorEquipmentChangeCommand(request);
                var snapshot = CreateActorEquipmentStateSnapshot();
                appliedActorEquipmentCommands.Add(commandId,
                    new AppliedActorEquipmentCommand(payloadKey,
                        CloneActorEquipmentState(snapshot)));
                return snapshot;
            }
        }

        internal bool ActorHasEquippedCapability(string actorStableId,
            string capabilityCode)
        {
            if (actorEquipmentCreationState == null
                || !string.Equals(actorEquipmentCreationState.ActorStableId,
                    actorStableId, StringComparison.Ordinal))
                return false;
            return EquippedCapabilityCodes().Contains(capabilityCode,
                StringComparer.Ordinal);
        }

        private bool HasAppliedActorEquipmentCommand(string commandId)
            => appliedActorEquipmentCommands.ContainsKey(commandId);

        private void InitializeActorEquipment(
            SimulationActorEquipmentInitialStateRequest? request)
        {
            actorEquipmentInitialPayloadKey =
                BuildActorEquipmentInitialPayloadKey(request);
            actorEquipmentCreationState = CloneActorEquipmentInitialState(request);
            actorEquipmentLegacyBridge = (request == null
                && natureSurvivalCreationState != null)
                || request?.LegacyAutoEquipCompatibility == true;
            if (actorEquipmentCreationState == null)
            {
                if (natureSurvivalCreationState == null) return;
                actorEquipmentCreationState = new SimulationActorEquipmentInitialStateRequest
                {
                    RuleRevision = SimulationActorEquipmentCodes.RuleRevision,
                    ActorStableId = natureSurvivalCreationState.PlayerStableId,
                    LegacyAutoEquipCompatibility = true,
                };
            }

            AddBuiltInActorItemDefinitions();
            foreach (var definition in actorEquipmentCreationState.ItemDefinitions)
                actorItemDefinitions[definition.ItemDefinitionStableId.Trim()] =
                    CloneItemDefinition(definition);
            foreach (var slot in ActorEquipmentSlotCodes())
                actorEquipmentSlots[slot] = string.Empty;
            foreach (var initial in actorEquipmentCreationState.ItemInstances)
                AddActorItemInstance(initial);

            if (natureSurvivalCreationState != null
                && string.Equals(actorEquipmentCreationState.ActorStableId,
                    natureSurvivalCreationState.PlayerStableId,
                    StringComparison.Ordinal))
            {
                var hasAxe = NaturePlayerHasItem(
                    SimulationNatureSurvivalCodes.AxeItemCode);
                if (!actorItemInstances.Values.Any(value =>
                        value.ItemDefinitionStableId ==
                        SimulationActorEquipmentCodes.AxeDefinitionStableId))
                {
                    AddActorItemInstance(new SimulationOwnedItemInstanceInitialState
                    {
                        ItemInstanceStableId =
                            SimulationNatureSurvivalCodes.AxePickupStableId,
                        ItemDefinitionStableId =
                            SimulationActorEquipmentCodes.AxeDefinitionStableId,
                        LocationCode = hasAxe
                            ? SimulationActorEquipmentCodes.Equipped
                            : SimulationActorEquipmentCodes.WorldPickup,
                        SlotCode = hasAxe
                            ? SimulationActorEquipmentCodes.MainHand : string.Empty,
                        SourceSpatialStableId =
                            SimulationNatureSurvivalCodes.AxePickupStableId,
                    });
                }
            }
        }

        private void AddBuiltInActorItemDefinitions()
        {
            AddBuiltInActorItemDefinition(
                SimulationActorEquipmentCodes.AxeDefinitionStableId,
                SimulationNatureSurvivalCodes.AxeItemCode, "기본 도끼",
                SimulationActorEquipmentCodes.Woodcutting,
                "Tool.Axe.Basic");
            AddBuiltInActorItemDefinition(
                SimulationActorEquipmentCodes.ShovelDefinitionStableId,
                "tool:shovel.basic", "기본 삽",
                SimulationActorEquipmentCodes.TerrainGrading,
                "Tool.Shovel.Basic");
            AddBuiltInActorItemDefinition(
                SimulationActorEquipmentCodes.PickaxeDefinitionStableId,
                "tool:pickaxe.basic", "기본 곡괭이",
                SimulationActorEquipmentCodes.Mining,
                "Tool.Pickaxe.Basic");
        }

        private void AddBuiltInActorItemDefinition(string definitionStableId,
            string itemCode, string koreanName, string capabilityCode,
            string visualKey)
        {
            actorItemDefinitions[definitionStableId] = new()
            {
                ItemDefinitionStableId = definitionStableId,
                ItemCode = itemCode,
                KoreanName = koreanName,
                Stackable = false,
                InventoryCapacityUnits = 1,
                AllowedSlotCodes = new[] { SimulationActorEquipmentCodes.MainHand },
                CapabilityCodes = new[] { capabilityCode },
                VisualKey = visualKey,
            };
        }

        private void AddActorItemInstance(
            SimulationOwnedItemInstanceInitialState initial)
        {
            var instanceId = initial.ItemInstanceStableId.Trim();
            var definitionId = initial.ItemDefinitionStableId.Trim();
            if (string.IsNullOrWhiteSpace(instanceId)
                || !actorItemDefinitions.TryGetValue(definitionId, out var definition))
                throw new SimulationContractException(
                    SimulationActorEquipmentCodes.ItemInstanceNotFound);
            if (actorItemInstances.ContainsKey(instanceId))
                throw new SimulationContractException(
                    SimulationActorEquipmentCodes.ItemInstanceNotFound);

            var location = initial.LocationCode.Trim();
            var slot = initial.SlotCode.Trim();
            if (location == SimulationActorEquipmentCodes.Equipped)
            {
                if (!definition.AllowedSlotCodes.Contains(slot,
                        StringComparer.Ordinal)
                    || !actorEquipmentSlots.TryGetValue(slot, out var occupied)
                    || !string.IsNullOrEmpty(occupied))
                    throw new SimulationContractException(
                        SimulationActorEquipmentCodes.SlotNotAllowed);
                actorEquipmentSlots[slot] = instanceId;
            }

            actorItemInstances.Add(instanceId, new()
            {
                ItemInstanceStableId = instanceId,
                ItemDefinitionStableId = definitionId,
                ItemCode = definition.ItemCode,
                KoreanName = definition.KoreanName,
                LocationCode = location,
                SlotCode = slot,
                SourceSpatialStableId = initial.SourceSpatialStableId.Trim(),
                VisualKey = definition.VisualKey,
            });
        }

        private SimulationActorItemAcquirePreviewSnapshot
            CreateActorItemAcquirePreview(
                SimulationActorItemAcquirePreviewRequest request)
        {
            var reasons = CommonActorEquipmentBlockReasons(
                request.ObservedEquipmentRevision, request.ActorStableId);
            if (!actorItemInstances.TryGetValue(
                    request.ItemInstanceStableId.Trim(), out var item))
                reasons.Add(SimulationActorEquipmentCodes.ItemInstanceNotFound);
            else if (item.LocationCode != SimulationActorEquipmentCodes.WorldPickup)
                reasons.Add(SimulationActorEquipmentCodes.ItemNotInWorld);
            return new()
            {
                SpecializationWorldInteractionId =
                    request.SpecializationWorldInteractionId.Trim(),
                ObservedEquipmentRevision = request.ObservedEquipmentRevision,
                ActorStableId = request.ActorStableId.Trim(),
                ItemInstanceStableId = request.ItemInstanceStableId.Trim(),
                CanConfirm = reasons.Count == 0,
                BlockReasonCodes = reasons.Distinct(StringComparer.Ordinal).ToArray(),
            };
        }

        private SimulationActorEquipmentChangePreviewSnapshot
            CreateActorEquipmentChangePreview(
                SimulationActorEquipmentChangePreviewRequest request)
        {
            var reasons = CommonActorEquipmentBlockReasons(
                request.ObservedEquipmentRevision, request.ActorStableId);
            var operation = request.OperationCode.Trim();
            var instanceId = request.ItemInstanceStableId.Trim();
            var slot = request.SlotCode.Trim();
            if (!actorItemInstances.TryGetValue(instanceId, out var item))
                reasons.Add(SimulationActorEquipmentCodes.ItemInstanceNotFound);
            else if (operation == SimulationActorEquipmentCodes.Equip
                     || operation == SimulationActorEquipmentCodes.Swap)
            {
                if (item.LocationCode != SimulationActorEquipmentCodes.Inventory)
                    reasons.Add(SimulationActorEquipmentCodes.ItemNotInInventory);
                if (!actorItemDefinitions[item.ItemDefinitionStableId]
                        .AllowedSlotCodes.Contains(slot, StringComparer.Ordinal))
                    reasons.Add(SimulationActorEquipmentCodes.SlotNotAllowed);
                else if (actorEquipmentSlots.TryGetValue(slot, out var occupied)
                         && operation == SimulationActorEquipmentCodes.Equip
                         && !string.IsNullOrEmpty(occupied))
                    reasons.Add(SimulationActorEquipmentCodes.SlotOccupied);
                else if (operation == SimulationActorEquipmentCodes.Swap
                         && (string.IsNullOrEmpty(occupied)
                             || !string.IsNullOrWhiteSpace(
                                 request.SwapItemInstanceStableId)
                             && occupied != request.SwapItemInstanceStableId.Trim()))
                    reasons.Add(SimulationActorEquipmentCodes.ItemNotEquipped);
            }
            else if (operation == SimulationActorEquipmentCodes.Unequip)
            {
                if (item.LocationCode != SimulationActorEquipmentCodes.Equipped
                    || (!string.IsNullOrEmpty(slot) && item.SlotCode != slot))
                    reasons.Add(SimulationActorEquipmentCodes.ItemNotEquipped);
            }
            else
                reasons.Add(SimulationActorEquipmentCodes.OperationNotSupported);

            return new()
            {
                SpecializationWorldInteractionId =
                    request.SpecializationWorldInteractionId.Trim(),
                ObservedEquipmentRevision = request.ObservedEquipmentRevision,
                ActorStableId = request.ActorStableId.Trim(),
                OperationCode = operation,
                ItemInstanceStableId = instanceId,
                SlotCode = slot,
                CanConfirm = reasons.Count == 0,
                BlockReasonCodes = reasons.Distinct(StringComparer.Ordinal).ToArray(),
            };
        }

        private List<string> CommonActorEquipmentBlockReasons(long observedRevision,
            string actorStableId)
        {
            var reasons = new List<string>();
            if (actorEquipmentCreationState == null)
                reasons.Add(SimulationActorEquipmentCodes.Disabled);
            else if (!string.Equals(actorEquipmentCreationState.ActorStableId,
                         actorStableId.Trim(), StringComparison.Ordinal))
                reasons.Add(SimulationActorEquipmentCodes.ActorMismatch);
            if (observedRevision != actorEquipmentRevision)
                reasons.Add(SimulationActorEquipmentCodes.ExpectedRevisionMismatch);
            return reasons;
        }

        private void AcquireActorItemInstance(string instanceId,
            bool autoEquipMainHand)
        {
            var item = actorItemInstances[instanceId];
            item.LocationCode = autoEquipMainHand
                ? SimulationActorEquipmentCodes.Equipped
                : SimulationActorEquipmentCodes.Inventory;
            item.SlotCode = autoEquipMainHand
                ? SimulationActorEquipmentCodes.MainHand : string.Empty;
            if (autoEquipMainHand)
                actorEquipmentSlots[SimulationActorEquipmentCodes.MainHand] =
                    instanceId;
            EnsureActorAggregateInventoryItem(item);
        }

        private void ApplyActorEquipmentChange(
            SimulationActorEquipmentChangeConfirmRequest request)
        {
            var item = actorItemInstances[request.ItemInstanceStableId.Trim()];
            var operation = request.OperationCode.Trim();
            if (operation == SimulationActorEquipmentCodes.Unequip)
            {
                actorEquipmentSlots[item.SlotCode] = string.Empty;
                item.LocationCode = SimulationActorEquipmentCodes.Inventory;
                item.SlotCode = string.Empty;
                return;
            }

            var slot = request.SlotCode.Trim();
            if (operation == SimulationActorEquipmentCodes.Swap)
            {
                var previous = actorItemInstances[actorEquipmentSlots[slot]];
                previous.LocationCode = SimulationActorEquipmentCodes.Inventory;
                previous.SlotCode = string.Empty;
            }
            actorEquipmentSlots[slot] = item.ItemInstanceStableId;
            item.LocationCode = SimulationActorEquipmentCodes.Equipped;
            item.SlotCode = slot;
        }

        private void EnsureActorAggregateInventoryItem(
            SimulationOwnedItemInstanceSnapshot item)
        {
            if (actorEquipmentCreationState == null
                || !worldInventoryPlayers.ContainsKey(
                    actorEquipmentCreationState.ActorStableId)) return;
            var key = PlayerItemKey(actorEquipmentCreationState.ActorStableId,
                item.ItemCode, SimulationNatureSurvivalCodes.UnitEach);
            if (worldInventoryPlayerItems.ContainsKey(key)) return;
            worldInventoryPlayerItems.Add(key, new SimulationWorldPlayerItemSnapshot
            {
                ItemCode = item.ItemCode,
                KoreanName = item.KoreanName,
                Quantity = 1,
                UnitCode = SimulationNatureSurvivalCodes.UnitEach,
            });
        }

        private void ApplyNatureAxeAcquisitionToActorEquipment()
        {
            if (actorEquipmentCreationState == null)
            {
                AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                    SimulationNatureSurvivalCodes.AxeItemCode, "기본 도끼", 1);
                return;
            }
            var axe = actorItemInstances.Values.Single(value =>
                value.ItemDefinitionStableId ==
                SimulationActorEquipmentCodes.AxeDefinitionStableId);
            AcquireActorItemInstance(axe.ItemInstanceStableId,
                autoEquipMainHand: actorEquipmentLegacyBridge);
            actorEquipmentRevision++;
        }

        private string[] EquippedCapabilityCodes()
            => actorItemInstances.Values
                .Where(value => value.LocationCode ==
                    SimulationActorEquipmentCodes.Equipped)
                .SelectMany(value => actorItemDefinitions[
                    value.ItemDefinitionStableId].CapabilityCodes)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

        private SimulationActorEquipmentStateSnapshot
            CreateActorEquipmentStateSnapshot()
        {
            if (actorEquipmentCreationState == null)
                return new SimulationActorEquipmentStateSnapshot();
            var snapshot = new SimulationActorEquipmentStateSnapshot
            {
                IsEnabled = true,
                RuleRevision = actorEquipmentCreationState.RuleRevision,
                ActorStableId = actorEquipmentCreationState.ActorStableId,
                EquipmentRevision = actorEquipmentRevision,
                ItemDefinitions = actorItemDefinitions.Values
                    .OrderBy(value => value.ItemDefinitionStableId,
                        StringComparer.Ordinal)
                    .Select(CloneItemDefinition).ToArray(),
                ItemInstances = actorItemInstances.Values
                    .OrderBy(value => value.ItemInstanceStableId,
                        StringComparer.Ordinal)
                    .Select(CloneActorItemInstance).ToArray(),
                Slots = actorEquipmentSlots
                    .OrderBy(value => value.Key, StringComparer.Ordinal)
                    .Select(value => new SimulationEquipmentSlotSnapshot
                    {
                        SlotCode = value.Key,
                        EquippedItemInstanceStableId = value.Value,
                    }).ToArray(),
                CapabilityCodes = EquippedCapabilityCodes(),
            };
            snapshot.StateHashSha256 = CalculateActorEquipmentStateHash(snapshot);
            return snapshot;
        }

        internal void RestoreActorEquipmentState(
            SimulationActorEquipmentStateSnapshot? snapshot)
        {
            if (snapshot == null || !snapshot.IsEnabled) return;
            actorEquipmentCreationState = new()
            {
                RuleRevision = snapshot.RuleRevision,
                ActorStableId = snapshot.ActorStableId,
                ItemDefinitions = snapshot.ItemDefinitions.Select(
                    CloneItemDefinition).ToArray(),
                ItemInstances = Array.Empty<SimulationOwnedItemInstanceInitialState>(),
            };
            actorItemDefinitions.Clear();
            actorItemInstances.Clear();
            actorEquipmentSlots.Clear();
            foreach (var definition in snapshot.ItemDefinitions)
                actorItemDefinitions[definition.ItemDefinitionStableId] =
                    CloneItemDefinition(definition);
            foreach (var slot in snapshot.Slots)
                actorEquipmentSlots[slot.SlotCode] =
                    slot.EquippedItemInstanceStableId;
            foreach (var item in snapshot.ItemInstances)
                actorItemInstances[item.ItemInstanceStableId] =
                    CloneActorItemInstance(item);
            actorEquipmentRevision = snapshot.EquipmentRevision;
        }

        internal static SimulationActorEquipmentInitialStateRequest?
            CloneActorEquipmentInitialState(
                SimulationActorEquipmentInitialStateRequest? source)
            => source == null ? null : new()
            {
                RuleRevision = source.RuleRevision,
                ActorStableId = source.ActorStableId,
                LegacyAutoEquipCompatibility =
                    source.LegacyAutoEquipCompatibility,
                ItemDefinitions = source.ItemDefinitions.Select(
                    CloneItemDefinition).ToArray(),
                ItemInstances = source.ItemInstances.Select(value =>
                    new SimulationOwnedItemInstanceInitialState
                    {
                        ItemInstanceStableId = value.ItemInstanceStableId,
                        ItemDefinitionStableId = value.ItemDefinitionStableId,
                        LocationCode = value.LocationCode,
                        SlotCode = value.SlotCode,
                        SourceSpatialStableId = value.SourceSpatialStableId,
                    }).ToArray(),
            };

        internal static SimulationActorEquipmentStateSnapshot
            CloneActorEquipmentState(SimulationActorEquipmentStateSnapshot source)
            => new()
            {
                IsEnabled = source.IsEnabled,
                RuleRevision = source.RuleRevision,
                ActorStableId = source.ActorStableId,
                EquipmentRevision = source.EquipmentRevision,
                ItemDefinitions = source.ItemDefinitions.Select(
                    CloneItemDefinition).ToArray(),
                ItemInstances = source.ItemInstances.Select(
                    CloneActorItemInstance).ToArray(),
                Slots = source.Slots.Select(value => new SimulationEquipmentSlotSnapshot
                {
                    SlotCode = value.SlotCode,
                    EquippedItemInstanceStableId = value.EquippedItemInstanceStableId,
                }).ToArray(),
                CapabilityCodes = source.CapabilityCodes.ToArray(),
                StateHashSha256 = source.StateHashSha256,
            };

        private static SimulationItemDefinitionSnapshot CloneItemDefinition(
            SimulationItemDefinitionSnapshot source)
            => new()
            {
                ItemDefinitionStableId = source.ItemDefinitionStableId,
                ItemCode = source.ItemCode,
                KoreanName = source.KoreanName,
                Stackable = source.Stackable,
                InventoryCapacityUnits = source.InventoryCapacityUnits,
                AllowedSlotCodes = source.AllowedSlotCodes.ToArray(),
                CapabilityCodes = source.CapabilityCodes.ToArray(),
                VisualKey = source.VisualKey,
            };

        private static SimulationOwnedItemInstanceSnapshot CloneActorItemInstance(
            SimulationOwnedItemInstanceSnapshot source)
            => new()
            {
                ItemInstanceStableId = source.ItemInstanceStableId,
                ItemDefinitionStableId = source.ItemDefinitionStableId,
                ItemCode = source.ItemCode,
                KoreanName = source.KoreanName,
                LocationCode = source.LocationCode,
                SlotCode = source.SlotCode,
                SourceSpatialStableId = source.SourceSpatialStableId,
                VisualKey = source.VisualKey,
            };

        internal static string CalculateActorEquipmentStateHash(
            SimulationActorEquipmentStateSnapshot snapshot)
        {
            var canonical = new StringBuilder()
                .Append(snapshot.RuleRevision).Append('|')
                .Append(snapshot.ActorStableId).Append('|')
                .Append(snapshot.EquipmentRevision);
            foreach (var definition in snapshot.ItemDefinitions)
                canonical.Append("|D:").Append(definition.ItemDefinitionStableId)
                    .Append(':').Append(definition.ItemCode).Append(':')
                    .Append(string.Join(',', definition.AllowedSlotCodes
                        .OrderBy(value => value, StringComparer.Ordinal)))
                    .Append(':').Append(string.Join(',', definition.CapabilityCodes
                        .OrderBy(value => value, StringComparer.Ordinal)))
                    .Append(':').Append(definition.VisualKey);
            foreach (var item in snapshot.ItemInstances)
                canonical.Append("|I:").Append(item.ItemInstanceStableId)
                    .Append(':').Append(item.ItemDefinitionStableId).Append(':')
                    .Append(item.LocationCode).Append(':').Append(item.SlotCode)
                    .Append(':').Append(item.SourceSpatialStableId);
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical.ToString())))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string[] ActorEquipmentSlotCodes()
            => new[]
            {
                SimulationActorEquipmentCodes.MainHand,
                SimulationActorEquipmentCodes.OffHand,
                SimulationActorEquipmentCodes.Head,
                SimulationActorEquipmentCodes.Body,
                SimulationActorEquipmentCodes.Legs,
                SimulationActorEquipmentCodes.Feet,
                SimulationActorEquipmentCodes.Back,
                SimulationActorEquipmentCodes.Accessory,
            };

        internal static void ValidateActorItemAcquirePreviewRequest(
            SimulationActorItemAcquirePreviewRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ActorStableId)
                || string.IsNullOrWhiteSpace(request.ItemInstanceStableId))
                throw new SimulationContractException("ActorItemAcquireRequestInvalid");
        }

        internal static void ValidateActorItemAcquireConfirmRequest(
            SimulationActorItemAcquireConfirmRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommandId))
                throw new SimulationContractException("ActorItemAcquireRequestInvalid");
            ValidateActorItemAcquirePreviewRequest(new()
            {
                ActorStableId = request.ActorStableId,
                ItemInstanceStableId = request.ItemInstanceStableId,
            });
        }

        internal static void ValidateActorEquipmentChangePreviewRequest(
            SimulationActorEquipmentChangePreviewRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ActorStableId)
                || string.IsNullOrWhiteSpace(request.OperationCode)
                || string.IsNullOrWhiteSpace(request.ItemInstanceStableId))
                throw new SimulationContractException(
                    "ActorEquipmentChangeRequestInvalid");
        }

        internal static void ValidateActorEquipmentChangeConfirmRequest(
            SimulationActorEquipmentChangeConfirmRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommandId))
                throw new SimulationContractException(
                    "ActorEquipmentChangeRequestInvalid");
            ValidateActorEquipmentChangePreviewRequest(new()
            {
                ActorStableId = request.ActorStableId,
                OperationCode = request.OperationCode,
                ItemInstanceStableId = request.ItemInstanceStableId,
            });
        }

        private static string BuildActorItemAcquirePayloadKey(
            SimulationActorItemAcquireConfirmRequest request)
            => string.Join('|', request.ExpectedEquipmentRevision,
                request.ActorStableId.Trim(), request.ItemInstanceStableId.Trim(),
                request.SpecializationWorldInteractionId.Trim());

        private static string BuildActorEquipmentChangePayloadKey(
            SimulationActorEquipmentChangeConfirmRequest request)
            => string.Join('|', request.ExpectedEquipmentRevision,
                request.ActorStableId.Trim(), request.OperationCode.Trim(),
                request.ItemInstanceStableId.Trim(), request.SlotCode.Trim(),
                request.SwapItemInstanceStableId.Trim(),
                request.SpecializationWorldInteractionId.Trim());

        private static string BuildActorEquipmentInitialPayloadKey(
            SimulationActorEquipmentInitialStateRequest? request)
        {
            if (request == null) return "none";
            return string.Join("|", new[]
            {
                request.RuleRevision.Trim(),
                request.ActorStableId.Trim(),
                request.LegacyAutoEquipCompatibility.ToString(),
                string.Join(";", request.ItemDefinitions
                    .OrderBy(value => value.ItemDefinitionStableId,
                        StringComparer.Ordinal)
                    .Select(value => string.Join(",", new[]
                    {
                        value.ItemDefinitionStableId.Trim(),
                        value.ItemCode.Trim(),
                        value.KoreanName.Trim(),
                        value.Stackable.ToString(),
                        value.InventoryCapacityUnits.ToString(
                            CultureInfo.InvariantCulture),
                        string.Join("+", value.AllowedSlotCodes
                            .OrderBy(code => code, StringComparer.Ordinal)),
                        string.Join("+", value.CapabilityCodes
                            .OrderBy(code => code, StringComparer.Ordinal)),
                        value.VisualKey.Trim(),
                    }))),
                string.Join(";", request.ItemInstances
                    .OrderBy(value => value.ItemInstanceStableId,
                        StringComparer.Ordinal)
                    .Select(value => string.Join(",", new[]
                    {
                        value.ItemInstanceStableId.Trim(),
                        value.ItemDefinitionStableId.Trim(),
                        value.LocationCode.Trim(),
                        value.SlotCode.Trim(),
                        value.SourceSpatialStableId.Trim(),
                    }))),
            });
        }

        private sealed class AppliedActorEquipmentCommand
        {
            public AppliedActorEquipmentCommand(string payloadKey,
                SimulationActorEquipmentStateSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public SimulationActorEquipmentStateSnapshot Snapshot { get; }
        }
    }
}
