using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string IndividualOrderPickupDecisionTypeCode =
            "IndividualOrderPickup";
        private const string ResidentOrderPickupActionCode = "ResidentOrderPickup";

        public SimulationDecisionPreviewSnapshot PreviewIndividualOrderPickup(
            SimulationIndividualOrderPickupPreviewRequest request)
        {
            ValidateIndividualOrderPickup(request);
            lock (gate)
            {
                return CreateDecisionPreview(CreateIndividualOrderPickupDecision(
                    request, true));
            }
        }

        public 경영SimulationSessionSnapshot ConfirmIndividualOrderPickup(
            SimulationIndividualOrderPickupConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateIndividualOrderPickup(request.Pickup);
            lock (gate)
            {
                var validation = CreateIndividualOrderPickupDecision(request.Pickup, true);
                var block = validation.BlockReasonCodes.FirstOrDefault();
                if (block != null && !appliedDecisionCommands.ContainsKey(request.CommandId))
                    throw new SimulationConflictException(block);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateIndividualOrderPickupDecision(request.Pickup, false),
                });
            }
        }

        private SimulationDecisionPreviewRequest CreateIndividualOrderPickupDecision(
            SimulationIndividualOrderPickupPreviewRequest request,
            bool includeValidationBlocks)
        {
            if (!individualOrders.TryGetValue(request.OrderStableId.Trim(), out var order))
                throw new SimulationNotFoundException("SimulationIndividualOrderNotFound");
            var blocks = new List<string>();
            if (includeValidationBlocks)
            {
                if (order.Revision != request.OrderRevision)
                    blocks.Add("SimulationIndividualOrderRevisionMismatch");
                if (order.StateCode != SimulationIndividualOrderStateCodes.ReadyForPickup)
                    blocks.Add("SimulationIndividualOrderNotReadyForPickup");
                if (order.ActorStableId != request.ActorStableId.Trim())
                    blocks.Add("SimulationIndividualOrderActorMismatch");
                if (!string.IsNullOrWhiteSpace(order.PickupTaskStableId))
                    blocks.Add("SimulationIndividualOrderPickupAlreadyScheduled");
            }
            var sources = MergeSources(request.SourceStableIds, order.SourceStableIds);
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:individual-order-pickup:" + order.OrderStableId,
                DecisionTypeCode = IndividualOrderPickupDecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { order.OrderStableId, order.ProductStableId },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = SimulationIndividualOrderStateCodes.Fulfilled,
                        TargetLedgerStableId = order.OrderStableId,
                        BeforeValue = 0m,
                        Delta = order.FulfilledQuantity,
                        AfterValue = order.FulfilledQuantity,
                        UnitCode = order.UnitCode,
                        SourceStableIds = sources,
                    },
                },
                BlockReasonCodes = blocks.OrderBy(value => value,
                    StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:individual-order-pickup:" + order.OrderStableId,
                    TaskTypeCode = "IndividualOrderPickup",
                    FacilityStableId = order.MarketFacilityStableId,
                    ActionCode = ResidentOrderPickupActionCode,
                    AssignedActorStableId = order.ActorStableId,
                    PreferredSpatialStableId = request.PreferredSpatialStableId.Trim(),
                    AssignedCapacity = order.FulfilledQuantity,
                    AssignedCapacityUnitCode = order.UnitCode,
                    DurationTicks = request.PickupDurationTicks,
                    InputLotStableIds = new[] { "reservation:" + order.OrderStableId },
                    OutputCandidateCodes = new[] { SimulationIndividualOrderStateCodes.Fulfilled },
                    SourceStableIds = sources,
                },
            };
        }

        private SimulationIndividualOrderSnapshot? PrepareIndividualOrderPickup(
            SimulationDecisionPreviewRequest request)
        {
            if (request.DecisionTypeCode != IndividualOrderPickupDecisionTypeCode)
                return null;
            var orderId = request.TargetStableIds.Single(value =>
                value.StartsWith("order:", StringComparison.Ordinal));
            if (!individualOrders.TryGetValue(orderId, out var order))
                throw new SimulationNotFoundException("SimulationIndividualOrderNotFound");
            if (order.StateCode != SimulationIndividualOrderStateCodes.ReadyForPickup)
                throw new SimulationConflictException("SimulationIndividualOrderNotReadyForPickup");
            if (!string.IsNullOrWhiteSpace(order.PickupTaskStableId))
                throw new SimulationConflictException(
                    "SimulationIndividualOrderPickupAlreadyScheduled");
            return order;
        }

        private static void ScheduleIndividualOrderPickup(
            SimulationIndividualOrderSnapshot? order,
            SimulationDecisionSnapshot decision,
            SimulationTaskSnapshot task)
        {
            if (order == null) return;
            order.StateCode = SimulationIndividualOrderStateCodes.PickupScheduled;
            order.PickupDecisionStableId = decision.DecisionStableId;
            order.PickupTaskStableId = task.TaskStableId;
            order.Revision++;
        }

        private void ApplyIndividualOrderPickupForTask(
            SimulationTaskSnapshot task,
            int appliedTick)
        {
            var order = individualOrders.Values.FirstOrDefault(value =>
                value.PickupTaskStableId == task.TaskStableId
                && value.StateCode == SimulationIndividualOrderStateCodes.PickupScheduled);
            if (order == null) return;
            order.StateCode = SimulationIndividualOrderStateCodes.Fulfilled;
            order.FulfilledTick = appliedTick;
            order.Revision++;
        }

        private void CancelIndividualOrderPickupForTask(SimulationTaskSnapshot task)
        {
            var order = individualOrders.Values.FirstOrDefault(value =>
                value.PickupTaskStableId == task.TaskStableId
                && value.StateCode == SimulationIndividualOrderStateCodes.PickupScheduled);
            if (order == null) return;
            order.StateCode = SimulationIndividualOrderStateCodes.ReadyForPickup;
            order.PickupDecisionStableId = null;
            order.PickupTaskStableId = null;
            order.Revision++;
        }

        private static void ValidateIndividualOrderPickup(
            SimulationIndividualOrderPickupPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.OrderStableId,
                "SimulationIndividualOrderStableIdInvalid");
            if (request.OrderRevision <= 0)
                throw new SimulationContractException(
                    "SimulationIndividualOrderRevisionInvalid");
            RequireStableId(request.ActorStableId,
                "SimulationIndividualOrderActorStableIdInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationPreferredSpatialStableIdInvalid");
            if (request.PickupDurationTicks <= 0 || request.PickupDurationTicks > 7)
                throw new SimulationContractException(
                    "SimulationIndividualOrderPickupDurationInvalid");
            ValidateIds(request.SourceStableIds, true,
                "SimulationIndividualOrderPickupSourceStableIdsInvalid");
        }
    }
}
