using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public SimulationDecisionPreviewSnapshot PreviewSupplyChainWork(
            SimulationSupplyChainWorkPreviewRequest request)
        {
            ValidateSupplyChainWork(request);
            lock (gate)
            {
                return CreateDecisionPreview(CreateSupplyChainDecisionRequest(request, true));
            }
        }

        public 경영SimulationSessionSnapshot ConfirmSupplyChainWork(
            SimulationSupplyChainWorkConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateSupplyChainWork(request.Work);
            lock (gate)
            {
                if (IsNpcRoutineControlEnabled
                    && string.Equals(request.Work.ActionCode,
                        SimulationSupplyChainActionCodes.WarehouseOutboundFlow,
                        StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "SimulationNpcRoutineDirectControlForbidden");
                var validation = CreateSupplyChainDecisionRequest(request.Work, true);
                var block = validation.BlockReasonCodes.FirstOrDefault();
                if (block != null && !appliedDecisionCommands.ContainsKey(request.CommandId))
                    throw new SimulationConflictException(block);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateSupplyChainDecisionRequest(request.Work, false),
                });
            }
        }

        private SimulationDecisionPreviewRequest CreateSupplyChainDecisionRequest(
            SimulationSupplyChainWorkPreviewRequest request,
            bool includeValidationBlocks)
        {
            if (!npcFacilityInventories.TryGetValue(request.InventoryStableId.Trim(),
                out var inventory))
                throw new SimulationNotFoundException("SimulationSupplyChainInventoryNotFound");

            var actionCode = request.ActionCode.Trim();
            var expectedState = RequiredInventoryState(actionCode);
            var completionState = CompletedInventoryState(actionCode);
            var blocks = new List<string>();
            if (includeValidationBlocks)
            {
                if (inventory.Revision != request.InventoryRevision)
                    blocks.Add("SimulationSupplyChainInventoryRevisionMismatch");
                if (!string.Equals(inventory.StateCode, expectedState,
                    StringComparison.Ordinal))
                    blocks.Add("SimulationSupplyChainInventoryStateInvalid");
                if (!npcActors.ContainsKey(request.ActorStableId.Trim()))
                    blocks.Add("SimulationSupplyChainActorNotFound");
                if (HasActiveSupplyChainTask(inventory.InventoryStableId))
                    blocks.Add("SimulationSupplyChainWorkAlreadyScheduled");
                if (actionCode == SimulationSupplyChainActionCodes.WarehouseOutboundFlow
                    && string.IsNullOrWhiteSpace(inventory.ProductStableId))
                    blocks.Add("SimulationSupplyChainInventoryProductMissing");
            }

            var sources = MergeSources(request.SourceStableIds,
                MergeSources(inventory.SourceStableIds,
                    new[] { inventory.InventoryStableId, "rule:simulation-supply-chain.r1" }));
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:supply-chain:" + actionCode.ToLowerInvariant()
                    + ":" + inventory.InventoryStableId,
                DecisionTypeCode = "SupplyChainWork",
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    inventory.InventoryStableId,
                    inventory.LotStableId,
                    "inventory-revision:" + request.InventoryRevision,
                },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = completionState,
                        TargetLedgerStableId = inventory.InventoryStableId,
                        BeforeValue = 0m,
                        Delta = inventory.Quantity,
                        AfterValue = inventory.Quantity,
                        UnitCode = inventory.UnitCode,
                        SourceStableIds = sources,
                    },
                },
                BlockReasonCodes = blocks.OrderBy(value => value,
                    StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:supply-chain:" + actionCode.ToLowerInvariant()
                        + ":" + inventory.InventoryStableId,
                    TaskTypeCode = "SupplyChainWork",
                    FacilityStableId = inventory.FacilityStableId,
                    ActionCode = actionCode,
                    AssignedActorStableId = request.ActorStableId.Trim(),
                    PreferredSpatialStableId = request.PreferredSpatialStableId.Trim(),
                    AssignedCapacity = inventory.Quantity,
                    AssignedCapacityUnitCode = inventory.UnitCode,
                    DurationTicks = request.DurationTicks,
                    InputLotStableIds = new[] { inventory.LotStableId },
                    OutputCandidateCodes = new[] { completionState },
                    SourceStableIds = sources,
                },
            };
        }

        private SimulationNpcFacilityInventorySnapshot? PrepareSupplyChainWork(
            SimulationDecisionPreviewRequest request)
        {
            if (!string.Equals(request.DecisionTypeCode, "SupplyChainWork",
                StringComparison.Ordinal)) return null;
            var inventoryId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("npc-inventory:", StringComparison.Ordinal))
                ?? throw new SimulationContractException(
                    "SimulationSupplyChainInventoryStableIdInvalid");
            if (!npcFacilityInventories.TryGetValue(inventoryId, out var inventory))
                throw new SimulationNotFoundException("SimulationSupplyChainInventoryNotFound");
            if (!string.Equals(inventory.StateCode,
                RequiredInventoryState(request.Task.ActionCode), StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationSupplyChainInventoryStateInvalid");
            if (HasActiveSupplyChainTask(inventoryId))
                throw new SimulationConflictException("SimulationSupplyChainWorkAlreadyScheduled");
            return inventory;
        }

        private void ScheduleSupplyChainWork(
            SimulationNpcFacilityInventorySnapshot? inventory,
            SimulationTaskSnapshot task)
        {
            if (inventory == null) return;
            if (task.ActionCode == SimulationSupplyChainActionCodes.WarehouseOutboundFlow)
            {
                inventory.StateCode = SimulationNpcInventoryStateCodes.OutboundRequested;
                inventory.UpdatedTick = CurrentTick;
                inventory.Revision++;
                inventory.SourceStableIds = MergeSources(inventory.SourceStableIds,
                    task.SourceStableIds);
            }
        }

        private void AdvanceSupplyChainWorkForTask(
            SimulationTaskSnapshot task,
            int currentTick)
        {
            if (!IsSupplyChainAction(task.ActionCode)) return;
            var inventory = FindSupplyChainInventory(task);
            if (inventory == null) return;

            if (task.ActionCode == SimulationSupplyChainActionCodes.WarehouseOutboundFlow)
            {
                if (currentTick >= task.ScheduledStartTick
                    && inventory.StateCode == SimulationNpcInventoryStateCodes.OutboundRequested)
                {
                    inventory.StateCode = SimulationNpcInventoryStateCodes.Picked;
                    inventory.UpdatedTick = currentTick;
                    inventory.Revision++;
                    ObserveNpcRoutineSupplyChainTransition(task, inventory,
                        "WI-HUB-04");
                }
                if (currentTick >= task.ExpectedEndTick
                    && inventory.StateCode == SimulationNpcInventoryStateCodes.Picked)
                {
                    inventory.StateCode = SimulationNpcInventoryStateCodes.OutboundReady;
                    inventory.UpdatedTick = task.ExpectedEndTick;
                    inventory.Revision++;
                    RegisterWarehouseOutboundAllocation(inventory, task);
                    ObserveNpcRoutineSupplyChainTransition(task, inventory,
                        "WI-HUB-05");
                }
                return;
            }

            if (currentTick < task.ExpectedEndTick) return;
            inventory.StateCode = CompletedInventoryState(task.ActionCode);
            inventory.UpdatedTick = task.ExpectedEndTick;
            inventory.Revision++;
            inventory.SourceStableIds = MergeSources(inventory.SourceStableIds,
                task.SourceStableIds);
        }

        private void CancelSupplyChainWorkForTask(SimulationTaskSnapshot task)
        {
            if (task.ActionCode != SimulationSupplyChainActionCodes.WarehouseOutboundFlow)
                return;
            var inventory = FindSupplyChainInventory(task);
            if (inventory == null
                || inventory.StateCode != SimulationNpcInventoryStateCodes.OutboundRequested
                    && inventory.StateCode != SimulationNpcInventoryStateCodes.Picked)
                return;
            inventory.StateCode = SimulationNpcInventoryStateCodes.PutAwayCompleted;
            inventory.UpdatedTick = CurrentTick;
            inventory.Revision++;
        }

        private void RegisterWarehouseOutboundAllocation(
            SimulationNpcFacilityInventorySnapshot inventory,
            SimulationTaskSnapshot task)
        {
            var allocationId = "allocation:warehouse-outbound:" + inventory.InventoryStableId;
            if (harvestLotAllocations.ContainsKey(allocationId)) return;
            var lotId = "warehouse-lot:" + inventory.InventoryStableId;
            harvestLotAllocations.Add(allocationId, new SimulationHarvestLotAllocationSnapshot
            {
                AllocationStableId = allocationId,
                HarvestLotStableId = lotId,
                HarvestLotRevision = inventory.Revision,
                ProductStableId = inventory.ProductStableId,
                Quantity = inventory.Quantity,
                UnitCode = inventory.UnitCode,
                ChoiceCode = SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
                NextWorkflowCode = SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate,
                DecisionStableId = task.CausedByDecisionStableId,
                TaskStableId = task.TaskStableId,
                FacilityStableId = inventory.FacilityStableId,
                StateCode = SimulationHarvestLotAllocationStateCodes.Applied,
                ReservedTick = task.ScheduledStartTick,
                AppliedTick = task.ExpectedEndTick,
                AvailableQuantity = inventory.Quantity,
                SourceStableIds = MergeSources(inventory.SourceStableIds,
                    new[] { inventory.InventoryStableId, task.TaskStableId }),
            });
        }

        private SimulationNpcFacilityInventorySnapshot? FindSupplyChainInventory(
            SimulationTaskSnapshot task)
        {
            if (!decisions.TryGetValue(task.CausedByDecisionStableId, out var decision))
                return null;
            var inventoryId = decision.TargetStableIds.FirstOrDefault(value =>
                value.StartsWith("npc-inventory:", StringComparison.Ordinal));
            return inventoryId != null
                && npcFacilityInventories.TryGetValue(inventoryId, out var inventory)
                    ? inventory : null;
        }

        private bool HasActiveSupplyChainTask(string inventoryStableId)
            => tasks.Values.Any(task => IsSupplyChainAction(task.ActionCode)
                && task.StateCode != SimulationTaskStateCodes.Completed
                && task.StateCode != SimulationTaskStateCodes.Cancelled
                && decisions.TryGetValue(task.CausedByDecisionStableId, out var decision)
                && decision.TargetStableIds.Contains(inventoryStableId,
                    StringComparer.Ordinal));

        private static bool IsSupplyChainAction(string actionCode)
            => actionCode == SimulationSupplyChainActionCodes.WarehouseOutboundFlow
                || actionCode == SimulationSupplyChainActionCodes.MarketInspection
                || actionCode == SimulationSupplyChainActionCodes.MarketBackroomPutAway
                || actionCode == SimulationSupplyChainActionCodes.MarketDisplayReplenishment;

        private static string RequiredInventoryState(string actionCode)
            => actionCode == SimulationSupplyChainActionCodes.WarehouseOutboundFlow
                ? SimulationNpcInventoryStateCodes.PutAwayCompleted
                : actionCode == SimulationSupplyChainActionCodes.MarketInspection
                    ? SimulationNpcInventoryStateCodes.MarketReceived
                    : actionCode == SimulationSupplyChainActionCodes.MarketBackroomPutAway
                        ? SimulationNpcInventoryStateCodes.MarketStorageEligible
                        : actionCode == SimulationSupplyChainActionCodes.MarketDisplayReplenishment
                            ? SimulationNpcInventoryStateCodes.MarketBackroomStored
                            : throw new SimulationContractException(
                                "SimulationSupplyChainActionInvalid");

        private static string CompletedInventoryState(string actionCode)
            => actionCode == SimulationSupplyChainActionCodes.WarehouseOutboundFlow
                ? SimulationNpcInventoryStateCodes.OutboundReady
                : actionCode == SimulationSupplyChainActionCodes.MarketInspection
                    ? SimulationNpcInventoryStateCodes.MarketStorageEligible
                    : actionCode == SimulationSupplyChainActionCodes.MarketBackroomPutAway
                        ? SimulationNpcInventoryStateCodes.MarketBackroomStored
                        : actionCode == SimulationSupplyChainActionCodes.MarketDisplayReplenishment
                            ? SimulationNpcInventoryStateCodes.Displayed
                            : throw new SimulationContractException(
                                "SimulationSupplyChainActionInvalid");

        private static void ValidateSupplyChainWork(
            SimulationSupplyChainWorkPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.InventoryStableId,
                "SimulationSupplyChainInventoryStableIdInvalid");
            if (request.InventoryRevision <= 0)
                throw new SimulationContractException(
                    "SimulationSupplyChainInventoryRevisionInvalid");
            RequireStableId(request.ActorStableId, "SimulationActorStableIdInvalid");
            RequiredInventoryState(request.ActionCode.Trim());
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationPreferredSpatialStableIdInvalid");
            if (request.DurationTicks <= 0 || request.DurationTicks > 7)
                throw new SimulationContractException(
                    "SimulationSupplyChainWorkDurationInvalid");
            ValidateIds(request.SourceStableIds, true,
                "SimulationSupplyChainSourceStableIdsInvalid");
        }

        private bool IsMarketFacility(string facilityStableId)
            => settlementInitialState?.Facilities.Any(value =>
                value.FacilityStableId == facilityStableId
                && value.FacilityTypeCode == SimulationSettlementFacilityTypeCodes.Market)
                == true;
    }
}
