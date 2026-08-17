using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 창고적재수량EffectCode = "WarehousePutAwayQuantity";

        public SimulationDecisionPreviewSnapshot PreviewWarehousePutAway(
            SimulationWarehousePutAwayPreviewRequest request)
        {
            ValidateWarehousePutAwayRequest(request);
            lock (gate)
            {
                return CreateDecisionPreview(CreateWarehousePutAwayDecisionRequest(request, true));
            }
        }

        public 경영SimulationSessionSnapshot ConfirmWarehousePutAway(
            SimulationWarehousePutAwayConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateWarehousePutAwayRequest(request.PutAway);

            lock (gate)
            {
                var deterministicPreview = CreateWarehousePutAwayDecisionRequest(request.PutAway, false);
                if (!appliedDecisionCommands.ContainsKey(request.CommandId))
                {
                    var validationPreview = CreateWarehousePutAwayDecisionRequest(request.PutAway, true);
                    var block = validationPreview.BlockReasonCodes.FirstOrDefault();
                    if (block != null)
                        throw new SimulationConflictException(block);
                }
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = deterministicPreview,
                });
            }
        }

        private SimulationDecisionPreviewRequest CreateWarehousePutAwayDecisionRequest(
            SimulationWarehousePutAwayPreviewRequest request,
            bool includeValidationBlocks)
        {
            if (!npcFacilityInventories.TryGetValue(request.InventoryStableId.Trim(), out var inventory))
                throw new SimulationNotFoundException("SimulationWarehouseInventoryNotFound");

            var blocks = new List<string>();
            if (includeValidationBlocks)
            {
                if (inventory.Revision != request.InventoryRevision)
                    blocks.Add("SimulationWarehouseInventoryRevisionMismatch");
                if (!string.Equals(
                        inventory.StateCode,
                        SimulationNpcInventoryStateCodes.StorageEligible,
                        StringComparison.Ordinal))
                {
                    blocks.Add("SimulationWarehouseInventoryNotPutAwayPending");
                }
                if (HasActivePutAwayTask(inventory.InventoryStableId))
                    blocks.Add("SimulationWarehousePutAwayAlreadyScheduled");
            }

            var sources = MergeSources(
                request.SourceStableIds,
                new[]
                {
                    inventory.InventoryStableId,
                    PyeongchangSimulationWorldStableIds.창고적재규칙,
                });
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:warehouse-put-away:" + inventory.InventoryStableId,
                DecisionTypeCode = SimulationNpcActionCodes.WarehouseStorageMove,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    inventory.InventoryStableId,
                    inventory.LotStableId,
                    "warehouse-inventory-revision:" + request.InventoryRevision,
                },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = 창고적재수량EffectCode,
                        TargetLedgerStableId = inventory.InventoryStableId,
                        BeforeValue = 0m,
                        Delta = inventory.Quantity,
                        AfterValue = inventory.Quantity,
                        UnitCode = inventory.UnitCode,
                        SourceStableIds = sources,
                    },
                },
                BlockReasonCodes = blocks.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:warehouse-put-away:" + inventory.InventoryStableId,
                    TaskTypeCode = "WarehousePutAway",
                    FacilityStableId = inventory.FacilityStableId,
                    ActionCode = SimulationNpcActionCodes.WarehouseStorageMove,
                    AssignedActorStableId = request.ActorStableId.Trim(),
                    PreferredSpatialStableId = request.PreferredSpatialStableId.Trim(),
                    AssignedCapacity = inventory.Quantity,
                    AssignedCapacityUnitCode = inventory.UnitCode,
                    DurationTicks = request.PutAwayDurationTicks,
                    InputLotStableIds = new[] { inventory.LotStableId },
                    OutputCandidateCodes = new[] { SimulationNpcInventoryStateCodes.PutAwayCompleted },
                    SourceStableIds = sources,
                },
            };
        }

        private bool HasActivePutAwayTask(string inventoryStableId)
            => tasks.Values.Any(task =>
                string.Equals(task.ActionCode, SimulationNpcActionCodes.WarehouseStorageMove, StringComparison.Ordinal)
                && task.StateCode != SimulationTaskStateCodes.Completed
                && task.StateCode != SimulationTaskStateCodes.Cancelled
                && decisions.TryGetValue(task.CausedByDecisionStableId, out var decision)
                && decision.TargetStableIds.Contains(inventoryStableId, StringComparer.Ordinal));

        private static void ValidateWarehousePutAwayRequest(
            SimulationWarehousePutAwayPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.InventoryStableId, "SimulationWarehouseInventoryStableIdInvalid");
            if (request.InventoryRevision <= 0)
                throw new SimulationContractException("SimulationWarehouseInventoryRevisionInvalid");
            RequireStableId(request.ActorStableId, "SimulationActorStableIdInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationPreferredSpatialStableIdInvalid");
            if (request.PutAwayDurationTicks <= 0 || request.PutAwayDurationTicks > 7)
                throw new SimulationContractException("SimulationWarehousePutAwayDurationInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationWarehousePutAwaySourceStableIdsInvalid");
        }
    }
}
