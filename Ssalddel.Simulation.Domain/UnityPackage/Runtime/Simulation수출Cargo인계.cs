using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 수출Cargo인계DecisionTypeCode = "ExportCargoHandoff";
        private const string 수출Cargo인계DecisionPrefix = "decision:export-cargo-handoff:";
        private readonly Dictionary<string, Simulation수출Cargo인계Snapshot> 수출Cargo인계원장 =
            new Dictionary<string, Simulation수출Cargo인계Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> 수출Cargo인계출처연결 =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public Simulation수출Cargo인계PreviewSnapshot Preview수출Cargo인계(
            Simulation수출Cargo인계PreviewRequest request)
        {
            Validate수출Cargo인계Request(request);
            lock (gate)
            {
                return Create수출Cargo인계Preview(request);
            }
        }

        public 경영SimulationSessionSnapshot Confirm수출Cargo인계(
            Simulation수출Cargo인계ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Validate수출Cargo인계Request(request.Handoff);
            return ConfirmDecision(new SimulationDecisionConfirmRequest
            {
                CommandId = request.CommandId.Trim(),
                ExpectedRevision = request.ExpectedRevision,
                Preview = Create수출Cargo인계DecisionRequest(request.Handoff),
            });
        }

        private Simulation수출Cargo인계PreviewSnapshot Create수출Cargo인계Preview(
            Simulation수출Cargo인계PreviewRequest request)
        {
            var common = Create수출Cargo인계DecisionRequest(request);
            수출Cargo준비원장.TryGetValue(
                request.SourceCargoPreparationStableId.Trim(), out var cargo);
            return new Simulation수출Cargo인계PreviewSnapshot
            {
                HandoffStableId = request.HandoffStableId.Trim(),
                SourceCargoPreparationStableId = request.SourceCargoPreparationStableId.Trim(),
                SourceExportPreparationStableId = cargo?.SourceExportPreparationStableId ?? string.Empty,
                SourceAllocationStableId = cargo?.SourceAllocationStableId ?? string.Empty,
                HarvestLotStableId = cargo?.HarvestLotStableId ?? string.Empty,
                PackageLotStableId = cargo?.PackageLotStableId ?? string.Empty,
                ProductStableId = cargo?.ProductStableId ?? string.Empty,
                CargoStableId = cargo?.CargoStableId ?? string.Empty,
                Quantity = cargo?.Quantity ?? 0m,
                UnitCode = cargo?.UnitCode ?? string.Empty,
                ReceivingFacilityStableId = request.ReceivingFacilityStableId.Trim(),
                IsCandidateOnly = true,
                DoesNotCreateLogisticsMovement = true,
                BoundaryCodes = 수출Cargo인계BoundaryCodes(),
                CommonDecisionPreview = CreateDecisionPreview(common),
            };
        }

        private SimulationDecisionPreviewRequest Create수출Cargo인계DecisionRequest(
            Simulation수출Cargo인계PreviewRequest request)
        {
            var sourceId = request.SourceCargoPreparationStableId.Trim();
            var blocks = new List<string>();
            Simulation수출Cargo준비Snapshot? cargo = null;
            if (!수출Cargo준비원장.TryGetValue(sourceId, out cargo))
            {
                blocks.Add("SourceExportCargoPreparationNotFound");
            }
            else
            {
                if (cargo.StateCode != Simulation수출Cargo준비상태Codes.ReadyForHandoff)
                    blocks.Add("SourceExportCargoPreparationNotReady");
                if (cargo.DestinationFacilityStableId != request.ReceivingFacilityStableId.Trim())
                    blocks.Add("ExportCargoHandoffFacilityMismatch");
                if (!string.IsNullOrWhiteSpace(cargo.HandoffStableId))
                    blocks.Add("SourceExportCargoAlreadyHandedOff");
                if (!harvestLotAllocations.TryGetValue(cargo.SourceAllocationStableId, out var allocation)
                    || allocation.OutboundReservedQuantity < cargo.Quantity)
                    blocks.Add("SourceExportCargoQuantityNotReserved");
            }
            if (수출Cargo인계원장.ContainsKey(request.HandoffStableId.Trim()))
                blocks.Add("ExportCargoHandoffStableIdConflict");
            if (수출Cargo인계출처연결.ContainsKey(sourceId))
                blocks.Add("ExportCargoHandoffSourceAlreadyUsed");
            if (settlementInitialState == null
                || !settlementInitialState.Facilities.Any(value =>
                    value.FacilityStableId == request.ReceivingFacilityStableId.Trim()))
                blocks.Add("ExportCargoHandoffFacilityNotFound");

            var handoffId = request.HandoffStableId.Trim();
            var sources = MergeSources(request.SourceStableIds, new[] { sourceId });
            var quantity = cargo?.Quantity ?? 1m;
            var unitCode = cargo?.UnitCode ?? "KGM";
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = 수출Cargo인계DecisionPrefix + handoffId,
                DecisionTypeCode = 수출Cargo인계DecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    handoffId,
                    sourceId,
                    request.ReceivingFacilityStableId.Trim(),
                },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "ExportCargoHandoffRecordedQuantity",
                        TargetLedgerStableId = handoffId,
                        BeforeValue = 0m,
                        Delta = quantity,
                        AfterValue = quantity,
                        UnitCode = unitCode,
                        SourceStableIds = sources,
                    },
                },
                Uncertainties = new[]
                {
                    "Simulation handoff does not confirm an operational carrier, dispatch, or vehicle departure.",
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:export-cargo-handoff:" + handoffId,
                    TaskTypeCode = 수출Cargo인계DecisionTypeCode,
                    FacilityStableId = request.ReceivingFacilityStableId.Trim(),
                    AssignedCapacity = quantity,
                    AssignedCapacityUnitCode = unitCode,
                    DurationTicks = request.RequiredHandoffTicks,
                    InputLotStableIds = new[] { cargo?.CargoStableId ?? sourceId },
                    OutputCandidateCodes = new[]
                    {
                        "logistics-movement-decision-required",
                    },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation수출Cargo인계Snapshot? Prepare수출Cargo인계(
            SimulationDecisionPreviewRequest request,
            SimulationDecisionPreviewSnapshot preview)
        {
            if (request.DecisionTypeCode != 수출Cargo인계DecisionTypeCode) return null;
            var handoffId = request.DecisionStableId.Substring(수출Cargo인계DecisionPrefix.Length);
            var cargo = request.TargetStableIds
                .Select(value => 수출Cargo준비원장.TryGetValue(value, out var found) ? found : null)
                .First(value => value != null)!;
            var receivingFacility = request.TargetStableIds.Single(value =>
                value != handoffId && value != cargo.CargoPreparationStableId);
            return new Simulation수출Cargo인계Snapshot
            {
                HandoffStableId = handoffId,
                StateCode = Simulation수출Cargo인계상태Codes.Scheduled,
                Revision = 1,
                SourceCargoPreparationStableId = cargo.CargoPreparationStableId,
                SourceExportPreparationStableId = cargo.SourceExportPreparationStableId,
                SourceAllocationStableId = cargo.SourceAllocationStableId,
                HarvestLotStableId = cargo.HarvestLotStableId,
                PackageLotStableId = cargo.PackageLotStableId,
                ProductStableId = cargo.ProductStableId,
                CargoStableId = cargo.CargoStableId,
                Quantity = cargo.Quantity,
                UnitCode = cargo.UnitCode,
                ReceivingFacilityStableId = receivingFacility,
                DecisionStableId = preview.Decision.DecisionStableId,
                TaskStableId = preview.TaskPlan.TaskStableId,
                RequiredHandoffTicks = request.Task.DurationTicks,
                ScheduledTick = CurrentTick,
                BoundaryCodes = 수출Cargo인계BoundaryCodes(),
                SourceStableIds = Copy(preview.Decision.SourceStableIds),
            };
        }

        private void Schedule수출Cargo인계(Simulation수출Cargo인계Snapshot? handoff)
        {
            if (handoff == null) return;
            var cargo = 수출Cargo준비원장[handoff.SourceCargoPreparationStableId];
            cargo.StateCode = Simulation수출Cargo준비상태Codes.HandoffScheduled;
            cargo.HandoffStableId = handoff.HandoffStableId;
            cargo.Revision++;
            수출Cargo인계원장.Add(handoff.HandoffStableId, handoff);
            수출Cargo인계출처연결.Add(
                handoff.SourceCargoPreparationStableId, handoff.HandoffStableId);
        }

        private void Advance수출Cargo인계ForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var handoff = 수출Cargo인계원장.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId
                    && value.StateCode == Simulation수출Cargo인계상태Codes.Scheduled);
            if (handoff == null || currentTick < task.ExpectedEndTick) return;
            handoff.StateCode = Simulation수출Cargo인계상태Codes.HandedOffInSimulation;
            handoff.Revision++;
            handoff.CompletedTick = task.ExpectedEndTick;
            var cargo = 수출Cargo준비원장[handoff.SourceCargoPreparationStableId];
            cargo.StateCode = Simulation수출Cargo준비상태Codes.HandedOffInSimulation;
            cargo.HandoffCompletedTick = task.ExpectedEndTick;
            cargo.Revision++;
        }

        private Simulation수출Cargo인계Snapshot[] Create수출Cargo인계Snapshots()
            => 수출Cargo인계원장.Values
                .OrderBy(value => value.HandoffStableId, StringComparer.Ordinal)
                .Select(Clone수출Cargo인계).ToArray();

        internal static Simulation수출Cargo인계Snapshot Clone수출Cargo인계(
            Simulation수출Cargo인계Snapshot source)
            => new Simulation수출Cargo인계Snapshot
            {
                HandoffStableId = source.HandoffStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                SourceCargoPreparationStableId = source.SourceCargoPreparationStableId,
                SourceExportPreparationStableId = source.SourceExportPreparationStableId,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                CargoStableId = source.CargoStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                ReceivingFacilityStableId = source.ReceivingFacilityStableId,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                RequiredHandoffTicks = source.RequiredHandoffTicks,
                ScheduledTick = source.ScheduledTick,
                CompletedTick = source.CompletedTick,
                LogisticsMovementCargoStableId = source.LogisticsMovementCargoStableId,
                LogisticsMovementTaskStableId = source.LogisticsMovementTaskStableId,
                BoundaryCodes = Copy(source.BoundaryCodes),
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static string[] 수출Cargo인계BoundaryCodes()
            => new[]
            {
                "SimulationOnly",
                "NoOperationalCarrierHandoff",
                "NoOperationalDispatch",
                "NoVehicleAssignment",
                "NoVehicleDeparture",
                "LogisticsMovementRequiresSeparateDecision",
                "NoExportDeclaration",
                "NoCustomsClearance",
            };

        private static void Validate수출Cargo인계Request(
            Simulation수출Cargo인계PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.HandoffStableId, "SimulationExportCargoHandoffStableIdInvalid");
            RequireStableId(request.SourceCargoPreparationStableId,
                "SimulationExportCargoHandoffSourceStableIdInvalid");
            RequireStableId(request.ReceivingFacilityStableId,
                "SimulationExportCargoHandoffFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationExportCargoHandoffActorStableIdInvalid");
            var targets = new HashSet<string>(StringComparer.Ordinal)
            {
                request.HandoffStableId.Trim(),
                request.SourceCargoPreparationStableId.Trim(),
                request.ReceivingFacilityStableId.Trim(),
            };
            if (targets.Count != 3)
                throw new SimulationContractException("SimulationExportCargoHandoffTargetsMustDiffer");
            if (request.RequiredHandoffTicks <= 0 || request.RequiredHandoffTicks > 28)
                throw new SimulationContractException("SimulationExportCargoHandoffDurationInvalid");
            ValidateIds(request.SourceStableIds, true,
                "SimulationExportCargoHandoffSourceStableIdsInvalid");
        }
    }
}
