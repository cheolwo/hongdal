using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, 적용된작업취소Command> appliedTaskCancelCommands =
            new Dictionary<string, 적용된작업취소Command>(StringComparer.Ordinal);

        public 경영SimulationSessionSnapshot CancelTask(
            string taskStableId,
            SimulationTaskCancelRequest request)
        {
            ValidateTaskCancel(taskStableId, request);
            lock (gate)
            {
                if (HasAppliedDecisionCommand(request.CommandId)
                    && !HasAppliedTaskCancelCommand(request.CommandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                var payloadKey = BuildTaskCancelPayloadKey(taskStableId, request);
                if (appliedTaskCancelCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (applied.PayloadKey != payloadKey)
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                if (!tasks.TryGetValue(taskStableId.Trim(), out var task))
                    throw new SimulationNotFoundException("SimulationTaskNotFound");
                if (task.StateCode != SimulationTaskStateCodes.Scheduled
                    && task.StateCode != SimulationTaskStateCodes.Blocked)
                    throw new SimulationConflictException("SimulationTaskCancellationNotAllowed");

                task.StateCode = SimulationTaskStateCodes.Cancelled;
                task.Revision++;
                task.ActualEndTick = CurrentTick;
                if (decisions.TryGetValue(task.CausedByDecisionStableId, out var decision))
                {
                    decision.StateCode = SimulationDecisionStateCodes.Cancelled;
                    decision.Revision++;
                }
                foreach (var effect in effects.Values.Where(value =>
                    value.CausedByTaskStableId == task.TaskStableId
                    && value.StateCode == SimulationEffectStateCodes.Pending))
                {
                    effect.StateCode = SimulationEffectStateCodes.Cancelled;
                    effect.Revision++;
                }

                CancelSimulationSpatialReservationsForTask(task);
                CancelFarmWorkForTask(task);
                CancelSupplyChainWorkForTask(task);
                CancelIndividualOrderPickupForTask(task);
                CancelNpcAssignmentForTask(task);
                CancelPendingInspectionInventoryForTask(task);
                CancelFreightReceiptForTask(task);
                Revision++;
                AppendTaskCancelCommand(taskStableId, request);
                var snapshot = CreateSnapshot();
                appliedTaskCancelCommands.Add(request.CommandId,
                    new 적용된작업취소Command(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private void CancelSimulationSpatialReservationsForTask(SimulationTaskSnapshot task)
        {
            var reservations = spatialReservations.Values.Where(value =>
                    value.TaskStableId == task.TaskStableId
                    && value.StatusCode == Simulation공간예약상태Codes.Reserved)
                .ToArray();
            if (reservations.Length == 0) return;
            foreach (var group in reservations.GroupBy(value => value.SpatialStableId,
                StringComparer.Ordinal))
            {
                var runtime = spatialRuntimeStates[group.Key];
                foreach (var reservation in group)
                {
                    AddCapacity(runtime.ReservedCapacities, reservation.ReservationKindCode,
                        -reservation.Quantity, reservation.UnitCode);
                    reservation.StatusCode = Simulation공간예약상태Codes.Cancelled;
                    reservation.ReleasedAtTick = CurrentTick;
                    reservation.FinalizedRevision = Revision + 1;
                }
                runtime.ActiveTaskStableIds = runtime.ActiveTaskStableIds.Where(value =>
                    value != task.TaskStableId).ToArray();
                runtime.Revision++;
            }
        }

        private void CancelFarmWorkForTask(SimulationTaskSnapshot task)
        {
            var workOrder = farmWorkOrders.FirstOrDefault(value =>
                value.WorkOrderStableId == task.TaskStableId
                && value.StatusCode == SimulationFarmSurvivalCodes.InProgress);
            if (workOrder == null) return;
            workOrder.StatusCode = SimulationTaskStateCodes.Cancelled;
            if (farmActors.TryGetValue(workOrder.ActorStableId, out var actor))
                actor.ActiveWorkOrderStableId = string.Empty;
            if (workOrder.AssignmentKindCode == SimulationFarmSurvivalCodes.NpcDelegated
                && settlementInitialState != null)
                settlementInitialState.LaborReserved -= workOrder.ReservedLabor;
            farmReservedSeedUnits -= workOrder.SeedCost;
            farmReservedWaterUnits -= workOrder.WaterCost;
            farmSeedUnits += workOrder.SeedCost;
            farmWaterUnits += workOrder.WaterCost;
        }

        private void CancelNpcAssignmentForTask(SimulationTaskSnapshot task)
        {
            var assignment = npcTaskAssignments.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId);
            if (assignment == null) return;
            assignment.PhaseCode = SimulationNpcActionPhaseCodes.Cancelled;
            assignment.CompletedTick = CurrentTick;
            assignment.Revision++;
        }

        private void CancelPendingInspectionInventoryForTask(SimulationTaskSnapshot task)
        {
            if (task.ActionCode != SimulationNpcActionCodes.WarehouseInboundInspection) return;
            var ids = npcFacilityInventories.Values.Where(value =>
                    value.SourceTaskStableId == task.TaskStableId
                    && value.StateCode == SimulationNpcInventoryStateCodes.PendingInspection)
                .Select(value => value.InventoryStableId).ToArray();
            foreach (var id in ids) npcFacilityInventories.Remove(id);
        }

        private void CancelFreightReceiptForTask(SimulationTaskSnapshot task)
        {
            var freight = freightTransports.Values.FirstOrDefault(value =>
                value.ReceiptTaskStableId == task.TaskStableId);
            if (freight == null) return;
            freight.ReceiptDecisionStableId = null;
            freight.ReceiptTaskStableId = null;
            freight.Revision++;
        }

        private bool HasAppliedTaskCancelCommand(string commandId)
            => appliedTaskCancelCommands.ContainsKey(commandId);

        internal static string BuildTaskCancelPayloadKey(
            string taskStableId,
            SimulationTaskCancelRequest request)
            => string.Join("\u001e", taskStableId.Trim(), request.ReasonCode.Trim(),
                request.ExpectedRevision.ToString(CultureInfo.InvariantCulture));

        internal static void ValidateTaskCancel(
            string taskStableId,
            SimulationTaskCancelRequest request)
        {
            RequireStableId(taskStableId, "SimulationTaskStableIdInvalid");
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            RequireStableId(request.ReasonCode, "SimulationTaskCancelReasonInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
        }

        private sealed class 적용된작업취소Command
        {
            public 적용된작업취소Command(string payloadKey, 경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
