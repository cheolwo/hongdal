using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, SimulationLogisticsMovementSnapshot> logisticsMovements =
            new Dictionary<string, SimulationLogisticsMovementSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> logisticsMovementSourceAllocations =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedLogisticsMovementCommand> appliedLogisticsMovementCommands =
            new Dictionary<string, AppliedLogisticsMovementCommand>(StringComparer.Ordinal);

        public SimulationLogisticsMovementPreviewSnapshot PreviewLogisticsMovement(
            SimulationLogisticsMovementPreviewRequest request)
        {
            ValidateLogisticsMovementRequest(request);
            lock (gate)
            {
                return CreateLogisticsMovementPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmLogisticsMovement(
            SimulationLogisticsMovementConfirmRequest request)
        {
            ValidateLogisticsMovementConfirmRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildLogisticsMovementPayloadKey(request.Movement);
                if (appliedLogisticsMovementCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                if (appliedCommands.ContainsKey(commandId)
                    || appliedDecisionCommands.ContainsKey(commandId)
                    || appliedHarvestImpactCommands.ContainsKey(commandId))
                {
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                }
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");

                var preview = CreateLogisticsMovementPreview(request.Movement);
                var commonRequest = CreateLogisticsMovementDecisionRequest(request.Movement);
                if (preview.CommonDecisionPreview.Decision.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException("SimulationDecisionPreviewBlocked");
                var commonPreview = preview.CommonDecisionPreview;
                if (CurrentTick + commonPreview.TaskPlan.DurationTicks > DurationTicks)
                    throw new SimulationConflictException("SimulationTaskDurationExceeded");
                if (decisions.ContainsKey(commonPreview.Decision.DecisionStableId))
                    throw new SimulationConflictException("SimulationDecisionStableIdConflict");
                if (tasks.ContainsKey(commonPreview.TaskPlan.TaskStableId))
                    throw new SimulationConflictException("SimulationTaskStableIdConflict");
                for (var index = 1; index <= commonPreview.Decision.ExpectedEffects.Length; index++)
                {
                    var effectId = commonPreview.TaskPlan.TaskStableId + ":effect:"
                        + index.ToString(CultureInfo.InvariantCulture);
                    if (effects.ContainsKey(effectId))
                        throw new SimulationConflictException("SimulationEffectStableIdConflict");
                }
                if (logisticsMovements.ContainsKey(preview.CargoStableId))
                    throw new SimulationConflictException("SimulationCargoStableIdConflict");
                if (logisticsMovementSourceAllocations.ContainsKey(preview.SourceAllocationStableId))
                    throw new SimulationConflictException("SimulationLogisticsSourceAlreadyAllocated");
                if (request.Movement.FreightTransport != null
                    && freightTransports.ContainsKey(request.Movement.FreightTransport.TransportRequestStableId.Trim()))
                    throw new SimulationConflictException("SimulationFreightTransportStableIdConflict");

                var allocation = harvestLotAllocations[preview.SourceAllocationStableId];
                allocation.OutboundReservedQuantity += preview.Quantity;
                allocation.AvailableQuantity = allocation.Quantity - allocation.OutboundReservedQuantity;

                var movement = CreateLogisticsMovementSnapshot(request.Movement, preview);
                logisticsMovements.Add(movement.CargoStableId, movement);
                logisticsMovementSourceAllocations.Add(
                    movement.SourceAllocationStableId,
                    movement.CargoStableId);
                if (request.Movement.FreightTransport != null)
                {
                    var freight = CreateFreightTransportSnapshot(request.Movement, movement);
                    freightTransports.Add(freight.TransportRequestStableId, freight);
                }

                var snapshot = ConfirmDecisionCore(
                    new SimulationDecisionConfirmRequest
                    {
                        CommandId = commandId,
                        ExpectedRevision = request.ExpectedRevision,
                        Preview = commonRequest,
                    },
                    false,
                    _ => AppendLogisticsMovementConfirmCommand(request));
                appliedLogisticsMovementCommands.Add(
                    commandId,
                    new AppliedLogisticsMovementCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private SimulationLogisticsMovementPreviewSnapshot CreateLogisticsMovementPreview(
            SimulationLogisticsMovementPreviewRequest request)
        {
            var commonRequest = CreateLogisticsMovementDecisionRequest(request);
            return new SimulationLogisticsMovementPreviewSnapshot
            {
                CargoStableId = request.CargoStableId.Trim(),
                CargoRevision = request.CargoRevision,
                SourceAllocationStableId = request.SourceAllocationStableId.Trim(),
                RouteStableId = request.RouteStableId.Trim(),
                OriginFacilityStableId = request.OriginFacilityStableId.Trim(),
                DestinationFacilityStableId = request.DestinationFacilityStableId.Trim(),
                Quantity = request.Quantity,
                UnitCode = request.UnitCode.Trim(),
                RequiredRouteTicks = request.RequiredRouteTicks,
                IsCandidateOnly = true,
                DoesNotApplySettlementState = true,
                DestinationStockCandidateStableId = DestinationCandidateId(request.CargoStableId),
                BoundaryCodes = new[]
                {
                    "CandidateOnly",
                    "NoOperationalDispatch",
                    "VehicleAnimationIsPresentationOnly",
                    "DestinationStockRequiresReceivingDecision",
                },
                CommonDecisionPreview = CreateDecisionPreview(commonRequest),
            };
        }

        private SimulationDecisionPreviewRequest CreateLogisticsMovementDecisionRequest(
            SimulationLogisticsMovementPreviewRequest request)
        {
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException("SimulationSettlementRequiredForLogisticsMovement");
            var sourceAllocationId = request.SourceAllocationStableId.Trim();
            var blocks = new List<string>();
            if (!harvestLotAllocations.TryGetValue(sourceAllocationId, out var allocation))
            {
                blocks.Add("SourceAllocationNotFound");
            }
            else
            {
                if (allocation.StateCode != SimulationHarvestLotAllocationStateCodes.Applied)
                    blocks.Add("SourceAllocationNotApplied");
                if (!string.Equals(allocation.HarvestLotStableId, request.HarvestLotStableId.Trim(), StringComparison.Ordinal)
                    || !string.Equals(allocation.ProductStableId, request.ProductStableId.Trim(), StringComparison.Ordinal)
                    || !string.Equals(allocation.UnitCode, request.UnitCode.Trim(), StringComparison.Ordinal))
                {
                    blocks.Add("SourceAllocationLineageMismatch");
                }
                if (allocation.AvailableQuantity < request.Quantity)
                    blocks.Add("SourceAllocationQuantityExceeded");
            }
            if (logisticsMovements.ContainsKey(request.CargoStableId.Trim()))
                blocks.Add("CargoAlreadyScheduled");
            if (logisticsMovementSourceAllocations.ContainsKey(sourceAllocationId))
                blocks.Add("SourceAllocationAlreadyScheduled");
            if (!settlement.Facilities.Any(value => value.FacilityStableId == request.OriginFacilityStableId.Trim()))
                blocks.Add("OriginFacilityNotFound");
            if (!settlement.Facilities.Any(value => value.FacilityStableId == request.DestinationFacilityStableId.Trim()))
                blocks.Add("DestinationFacilityNotFound");
            if (request.FreightTransport != null)
            {
                var binding = request.FreightTransport;
                if (binding.VehicleCapacity < request.Quantity)
                    blocks.Add("FreightVehicleCapacityExceeded");
                if (!string.Equals(binding.VehicleCapacityUnitCode.Trim(), request.UnitCode.Trim(), StringComparison.Ordinal))
                    blocks.Add("FreightVehicleCapacityUnitMismatch");
                if (freightTransports.ContainsKey(binding.TransportRequestStableId.Trim()))
                    blocks.Add("FreightTransportAlreadyScheduled");
            }

            var sources = request.SourceStableIds
                .Select(value => value.Trim())
                .Concat(new[]
                {
                    request.CargoStableId.Trim(),
                    request.HarvestLotStableId.Trim(),
                    request.PackageLotStableId.Trim(),
                    sourceAllocationId,
                    request.RouteStableId.Trim(),
                })
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var availableBefore = allocation?.AvailableQuantity ?? 0m;
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:logistics:" + request.CargoStableId.Trim(),
                DecisionTypeCode = "LogisticsMovement",
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    request.CargoStableId.Trim(),
                    request.OriginFacilityStableId.Trim(),
                    request.DestinationFacilityStableId.Trim(),
                },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "CargoArrivalCandidate",
                        TargetLedgerStableId = DestinationCandidateId(request.CargoStableId),
                        BeforeValue = 0m,
                        Delta = request.Quantity,
                        AfterValue = request.Quantity,
                        UnitCode = request.UnitCode.Trim(),
                        SourceStableIds = sources,
                    },
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "SourceStockReservation",
                        TargetLedgerStableId = sourceAllocationId,
                        BeforeValue = availableBefore,
                        Delta = -request.Quantity,
                        AfterValue = availableBefore - request.Quantity,
                        UnitCode = request.UnitCode.Trim(),
                        SourceStableIds = sources,
                    },
                },
                Uncertainties = new[] { "ReceivingDecisionPending" },
                BlockReasonCodes = blocks.Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:logistics:" + request.CargoStableId.Trim(),
                    TaskTypeCode = "CargoRouteMovement",
                    FacilityStableId = request.OriginFacilityStableId.Trim(),
                    AssignedCapacity = request.Quantity,
                    AssignedCapacityUnitCode = request.UnitCode.Trim(),
                    DurationTicks = request.RequiredRouteTicks,
                    InputLotStableIds = new[]
                    {
                        request.HarvestLotStableId.Trim(),
                        request.PackageLotStableId.Trim(),
                        request.CargoStableId.Trim(),
                    },
                    OutputCandidateCodes = new[] { "DestinationStockCandidate" },
                    SourceStableIds = sources,
                },
            };
        }

        private SimulationLogisticsMovementSnapshot CreateLogisticsMovementSnapshot(
            SimulationLogisticsMovementPreviewRequest request,
            SimulationLogisticsMovementPreviewSnapshot preview)
            => new SimulationLogisticsMovementSnapshot
            {
                CargoStableId = preview.CargoStableId,
                CargoRevision = request.CargoRevision,
                StateCode = SimulationLogisticsMovementStateCodes.Reserved,
                Revision = 1,
                SourceAllocationStableId = preview.SourceAllocationStableId,
                HarvestLotStableId = request.HarvestLotStableId.Trim(),
                PackageLotStableId = request.PackageLotStableId.Trim(),
                ProductStableId = request.ProductStableId.Trim(),
                Quantity = request.Quantity,
                ReservedQuantity = request.Quantity,
                UnitCode = request.UnitCode.Trim(),
                RouteStableId = preview.RouteStableId,
                OriginFacilityStableId = preview.OriginFacilityStableId,
                DestinationFacilityStableId = preview.DestinationFacilityStableId,
                DecisionStableId = preview.CommonDecisionPreview.Decision.DecisionStableId,
                TaskStableId = preview.CommonDecisionPreview.TaskPlan.TaskStableId,
                RequiredRouteTicks = preview.RequiredRouteTicks,
                ReservedTick = CurrentTick,
                DestinationStockCandidateStableId = preview.DestinationStockCandidateStableId,
                SourceStableIds = Copy(preview.CommonDecisionPreview.Decision.SourceStableIds),
            };

        private void AdvanceLogisticsMovementForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var movement = logisticsMovements.Values.FirstOrDefault(
                value => value.TaskStableId == task.TaskStableId
                    && value.StateCode != SimulationLogisticsMovementStateCodes.ArrivedAtDestination);
            if (movement == null || currentTick < task.ScheduledStartTick) return;

            var completed = Math.Min(
                movement.RequiredRouteTicks,
                currentTick - task.ScheduledStartTick + 1);
            if (movement.StateCode == SimulationLogisticsMovementStateCodes.Reserved)
            {
                movement.StateCode = SimulationLogisticsMovementStateCodes.InTransit;
                movement.DepartedTick = task.ScheduledStartTick;
                movement.Revision++;
            }
            if (completed != movement.CompletedRouteTicks)
            {
                movement.CompletedRouteTicks = completed;
                movement.Revision++;
            }
            if (currentTick >= task.ExpectedEndTick)
            {
                movement.StateCode = SimulationLogisticsMovementStateCodes.ArrivedAtDestination;
                movement.ArrivedTick = task.ExpectedEndTick;
                movement.Revision++;
            }
            AdvanceFreightTransportForMovement(movement, currentTick);
        }

        private SimulationLogisticsMovementSnapshot[] CreateLogisticsMovementSnapshots()
            => logisticsMovements.Values.OrderBy(value => value.CargoStableId, StringComparer.Ordinal)
                .Select(CloneLogisticsMovement).ToArray();

        internal static SimulationLogisticsMovementSnapshot CloneLogisticsMovement(
            SimulationLogisticsMovementSnapshot source)
            => new SimulationLogisticsMovementSnapshot
            {
                CargoStableId = source.CargoStableId,
                CargoRevision = source.CargoRevision,
                StateCode = source.StateCode,
                Revision = source.Revision,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                ReservedQuantity = source.ReservedQuantity,
                UnitCode = source.UnitCode,
                RouteStableId = source.RouteStableId,
                OriginFacilityStableId = source.OriginFacilityStableId,
                DestinationFacilityStableId = source.DestinationFacilityStableId,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                RequiredRouteTicks = source.RequiredRouteTicks,
                CompletedRouteTicks = source.CompletedRouteTicks,
                ReservedTick = source.ReservedTick,
                DepartedTick = source.DepartedTick,
                ArrivedTick = source.ArrivedTick,
                DestinationStockCandidateStableId = source.DestinationStockCandidateStableId,
                SourceStableIds = Copy(source.SourceStableIds),
            };

        internal static string BuildLogisticsMovementPayloadKey(
            SimulationLogisticsMovementPreviewRequest request)
            => string.Join("\u001e", new[]
            {
                request.CargoStableId.Trim(),
                request.CargoRevision.ToString(CultureInfo.InvariantCulture),
                request.SourceAllocationStableId.Trim(),
                request.HarvestLotStableId.Trim(),
                request.PackageLotStableId.Trim(),
                request.ProductStableId.Trim(),
                request.Quantity.ToString(CultureInfo.InvariantCulture),
                request.UnitCode.Trim(),
                request.RouteStableId.Trim(),
                request.OriginFacilityStableId.Trim(),
                request.DestinationFacilityStableId.Trim(),
                request.ActorStableId.Trim(),
                request.RequiredRouteTicks.ToString(CultureInfo.InvariantCulture),
                BuildFreightTransportBindingPayloadKey(request.FreightTransport),
                string.Join("\u001f", request.SourceStableIds.Select(value => value.Trim())
                    .OrderBy(value => value, StringComparer.Ordinal)),
            });

        internal static void ValidateLogisticsMovementConfirmRequestForReplay(
            SimulationLogisticsMovementConfirmRequest request)
            => ValidateLogisticsMovementConfirmRequest(request);

        private static void ValidateLogisticsMovementConfirmRequest(
            SimulationLogisticsMovementConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateLogisticsMovementRequest(request.Movement);
        }

        private static void ValidateLogisticsMovementRequest(
            SimulationLogisticsMovementPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CargoStableId, "SimulationCargoStableIdInvalid");
            if (request.CargoRevision <= 0)
                throw new SimulationContractException("SimulationCargoRevisionInvalid");
            RequireStableId(request.SourceAllocationStableId, "SimulationSourceAllocationStableIdInvalid");
            RequireStableId(request.HarvestLotStableId, "SimulationHarvestLotStableIdInvalid");
            RequireStableId(request.PackageLotStableId, "SimulationPackageLotStableIdInvalid");
            RequireStableId(request.ProductStableId, "SimulationProductStableIdInvalid");
            if (request.Quantity <= 0m)
                throw new SimulationContractException("SimulationLogisticsQuantityInvalid");
            RequireText(request.UnitCode, "SimulationLogisticsUnitCodeMissing");
            RequireStableId(request.RouteStableId, "SimulationRouteStableIdInvalid");
            RequireStableId(request.OriginFacilityStableId, "SimulationOriginFacilityStableIdInvalid");
            RequireStableId(request.DestinationFacilityStableId, "SimulationDestinationFacilityStableIdInvalid");
            if (request.OriginFacilityStableId.Trim() == request.DestinationFacilityStableId.Trim())
                throw new SimulationContractException("SimulationLogisticsRouteEndpointsEqual");
            RequireStableId(request.ActorStableId, "SimulationActorStableIdInvalid");
            if (request.RequiredRouteTicks <= 0 || request.RequiredRouteTicks > 30)
                throw new SimulationContractException("SimulationRouteTicksInvalid");
            if (request.FreightTransport != null)
                ValidateFreightTransportBinding(request.FreightTransport, request.UnitCode);
            ValidateIds(request.SourceStableIds, true, "SimulationLogisticsSourceStableIdsInvalid");
        }

        private static string BuildFreightTransportBindingPayloadKey(
            SimulationFreightTransportBindingRequest? request)
            => request == null ? string.Empty : string.Join("\u001f", new[]
            {
                request.TransportRequestStableId.Trim(),
                request.DispatchOfferStableId.Trim(),
                request.CarrierCandidateStableId.Trim(),
                request.VehicleStableId.Trim(),
                request.VehicleCapacity.ToString(CultureInfo.InvariantCulture),
                request.VehicleCapacityUnitCode.Trim(),
            });

        private static string DestinationCandidateId(string cargoStableId)
            => "stock-candidate:arrival:" + cargoStableId.Trim();

        private sealed class AppliedLogisticsMovementCommand
        {
            public AppliedLogisticsMovementCommand(
                string payloadKey,
                경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
