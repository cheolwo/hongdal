using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 수출항만인수DecisionTypeCode = "ExportPortReceiving";
        private const string 수출항만인수DecisionPrefix = "decision:export-port-receiving:";
        private readonly Dictionary<string, Simulation수출항만인수Snapshot> 수출항만인수원장 =
            new Dictionary<string, Simulation수출항만인수Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> 수출항만인수Cargo연결 =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public Simulation수출항만인수PreviewSnapshot Preview수출항만인수(
            Simulation수출항만인수PreviewRequest request)
        {
            Validate수출항만인수Request(request);
            lock (gate)
            {
                return Create수출항만인수Preview(request);
            }
        }

        public 경영SimulationSessionSnapshot Confirm수출항만인수(
            Simulation수출항만인수ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Validate수출항만인수Request(request.Receipt);
            return ConfirmDecision(new SimulationDecisionConfirmRequest
            {
                CommandId = request.CommandId.Trim(),
                ExpectedRevision = request.ExpectedRevision,
                Preview = Create수출항만인수DecisionRequest(request.Receipt),
            });
        }

        private Simulation수출항만인수PreviewSnapshot Create수출항만인수Preview(
            Simulation수출항만인수PreviewRequest request)
        {
            var common = Create수출항만인수DecisionRequest(request);
            logisticsMovements.TryGetValue(request.CargoStableId.Trim(), out var movement);
            return new Simulation수출항만인수PreviewSnapshot
            {
                ReceiptStableId = request.ReceiptStableId.Trim(),
                CargoStableId = request.CargoStableId.Trim(),
                SourceExportCargoHandoffStableId =
                    movement?.SourceExportCargoHandoffStableId ?? string.Empty,
                SourceAllocationStableId = movement?.SourceAllocationStableId ?? string.Empty,
                HarvestLotStableId = movement?.HarvestLotStableId ?? string.Empty,
                PackageLotStableId = movement?.PackageLotStableId ?? string.Empty,
                ProductStableId = movement?.ProductStableId ?? string.Empty,
                Quantity = movement?.Quantity ?? 0m,
                UnitCode = movement?.UnitCode ?? string.Empty,
                ReceivingFacilityStableId = request.ReceivingFacilityStableId.Trim(),
                IsCandidateOnly = true,
                DoesNotCreateCustomsOperation = true,
                BoundaryCodes = 수출항만인수BoundaryCodes(),
                CommonDecisionPreview = CreateDecisionPreview(common),
            };
        }

        private SimulationDecisionPreviewRequest Create수출항만인수DecisionRequest(
            Simulation수출항만인수PreviewRequest request)
        {
            var cargoId = request.CargoStableId.Trim();
            var blocks = new List<string>();
            SimulationLogisticsMovementSnapshot? movement = null;
            if (!logisticsMovements.TryGetValue(cargoId, out movement))
            {
                blocks.Add("ExportPortCargoMovementNotFound");
            }
            else
            {
                if (movement.StateCode != SimulationLogisticsMovementStateCodes.ArrivedAtDestination)
                    blocks.Add("ExportPortCargoNotArrived");
                if (string.IsNullOrWhiteSpace(movement.SourceExportCargoHandoffStableId))
                    blocks.Add("ExportPortCargoHandoffLineageMissing");
                if (movement.DestinationFacilityStableId
                    != request.ReceivingFacilityStableId.Trim())
                    blocks.Add("ExportPortReceivingFacilityMismatch");
                if (!string.IsNullOrWhiteSpace(movement.DestinationReceiptStableId))
                    blocks.Add("ExportPortCargoAlreadyReceived");
                if (!harvestLotAllocations.TryGetValue(movement.SourceAllocationStableId,
                        out var allocation)
                    || allocation.OutboundReservedQuantity < movement.Quantity)
                    blocks.Add("ExportPortCargoReservedQuantityMissing");
            }
            if (수출항만인수원장.ContainsKey(request.ReceiptStableId.Trim()))
                blocks.Add("ExportPortReceiptStableIdConflict");
            if (수출항만인수Cargo연결.ContainsKey(cargoId))
                blocks.Add("ExportPortCargoReceiptAlreadyScheduled");
            if (settlementInitialState == null
                || !settlementInitialState.Facilities.Any(value =>
                    value.FacilityStableId == request.ReceivingFacilityStableId.Trim()))
                blocks.Add("ExportPortReceivingFacilityNotFound");

            var receiptId = request.ReceiptStableId.Trim();
            var sources = MergeSources(request.SourceStableIds, new[] { cargoId });
            if (!string.IsNullOrWhiteSpace(movement?.SourceExportCargoHandoffStableId))
                sources = MergeSources(sources, new[] { movement.SourceExportCargoHandoffStableId! });
            var quantity = movement?.Quantity ?? 1m;
            var unitCode = movement?.UnitCode ?? "KGM";
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = 수출항만인수DecisionPrefix + receiptId,
                DecisionTypeCode = 수출항만인수DecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    receiptId,
                    cargoId,
                    request.ReceivingFacilityStableId.Trim(),
                },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "ExportPortReceivedQuantity",
                        TargetLedgerStableId = receiptId,
                        BeforeValue = 0m,
                        Delta = quantity,
                        AfterValue = quantity,
                        UnitCode = unitCode,
                        SourceStableIds = sources,
                    },
                },
                Uncertainties = new[]
                {
                    "Port staging receipt does not create an export declaration, inspection, customs clearance, or loading operation.",
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:export-port-receiving:" + receiptId,
                    TaskTypeCode = 수출항만인수DecisionTypeCode,
                    FacilityStableId = request.ReceivingFacilityStableId.Trim(),
                    AssignedCapacity = quantity,
                    AssignedCapacityUnitCode = unitCode,
                    DurationTicks = request.RequiredReceivingTicks,
                    InputLotStableIds = new[] { cargoId },
                    OutputCandidateCodes = new[]
                    {
                        "export-readiness-review-required",
                    },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation수출항만인수Snapshot? Prepare수출항만인수(
            SimulationDecisionPreviewRequest request,
            SimulationDecisionPreviewSnapshot preview)
        {
            if (request.DecisionTypeCode != 수출항만인수DecisionTypeCode) return null;
            var receiptId = request.DecisionStableId.Substring(수출항만인수DecisionPrefix.Length);
            var movement = request.TargetStableIds
                .Select(value => logisticsMovements.TryGetValue(value, out var found) ? found : null)
                .First(value => value != null)!;
            var receivingFacility = request.TargetStableIds.Single(value =>
                value != receiptId && value != movement.CargoStableId);
            return new Simulation수출항만인수Snapshot
            {
                ReceiptStableId = receiptId,
                StateCode = Simulation수출항만인수상태Codes.Scheduled,
                Revision = 1,
                CargoStableId = movement.CargoStableId,
                SourceExportCargoHandoffStableId = movement.SourceExportCargoHandoffStableId!,
                SourceAllocationStableId = movement.SourceAllocationStableId,
                HarvestLotStableId = movement.HarvestLotStableId,
                PackageLotStableId = movement.PackageLotStableId,
                ProductStableId = movement.ProductStableId,
                Quantity = movement.Quantity,
                UnitCode = movement.UnitCode,
                ReceivingFacilityStableId = receivingFacility,
                DecisionStableId = preview.Decision.DecisionStableId,
                TaskStableId = preview.TaskPlan.TaskStableId,
                RequiredReceivingTicks = request.Task.DurationTicks,
                ScheduledTick = CurrentTick,
                BoundaryCodes = 수출항만인수BoundaryCodes(),
                SourceStableIds = Copy(preview.Decision.SourceStableIds),
            };
        }

        private void Schedule수출항만인수(Simulation수출항만인수Snapshot? receipt)
        {
            if (receipt == null) return;
            var movement = logisticsMovements[receipt.CargoStableId];
            movement.DestinationReceiptStableId = receipt.ReceiptStableId;
            movement.Revision++;
            수출항만인수원장.Add(receipt.ReceiptStableId, receipt);
            수출항만인수Cargo연결.Add(receipt.CargoStableId, receipt.ReceiptStableId);
        }

        private void Advance수출항만인수ForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var receipt = 수출항만인수원장.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId
                    && value.StateCode == Simulation수출항만인수상태Codes.Scheduled);
            if (receipt == null || currentTick < task.ExpectedEndTick) return;
            receipt.StateCode = Simulation수출항만인수상태Codes.ReceivedAtPortStaging;
            receipt.Revision++;
            receipt.CompletedTick = task.ExpectedEndTick;
            var movement = logisticsMovements[receipt.CargoStableId];
            movement.DestinationReceiptCompletedTick = task.ExpectedEndTick;
            movement.Revision++;
        }

        private Simulation수출항만인수Snapshot[] Create수출항만인수Snapshots()
            => 수출항만인수원장.Values
                .OrderBy(value => value.ReceiptStableId, StringComparer.Ordinal)
                .Select(Clone수출항만인수).ToArray();

        internal static Simulation수출항만인수Snapshot Clone수출항만인수(
            Simulation수출항만인수Snapshot source)
            => new Simulation수출항만인수Snapshot
            {
                ReceiptStableId = source.ReceiptStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                CargoStableId = source.CargoStableId,
                SourceExportCargoHandoffStableId = source.SourceExportCargoHandoffStableId,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                ReceivingFacilityStableId = source.ReceivingFacilityStableId,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                RequiredReceivingTicks = source.RequiredReceivingTicks,
                ScheduledTick = source.ScheduledTick,
                CompletedTick = source.CompletedTick,
                BoundaryCodes = Copy(source.BoundaryCodes),
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static string[] 수출항만인수BoundaryCodes()
            => new[]
            {
                "SimulationOnly",
                "PortStagingReceiptOnly",
                "NoExportDeclaration",
                "NoOfficialInspection",
                "NoCustomsClearance",
                "NoVesselLoading",
                "ExportReadinessRequiresSeparateDecision",
            };

        private static void Validate수출항만인수Request(
            Simulation수출항만인수PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.ReceiptStableId, "SimulationExportPortReceiptStableIdInvalid");
            RequireStableId(request.CargoStableId, "SimulationExportPortCargoStableIdInvalid");
            RequireStableId(request.ReceivingFacilityStableId,
                "SimulationExportPortReceivingFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationExportPortActorStableIdInvalid");
            var targets = new HashSet<string>(StringComparer.Ordinal)
            {
                request.ReceiptStableId.Trim(),
                request.CargoStableId.Trim(),
                request.ReceivingFacilityStableId.Trim(),
            };
            if (targets.Count != 3)
                throw new SimulationContractException("SimulationExportPortReceiptTargetsMustDiffer");
            if (request.RequiredReceivingTicks <= 0 || request.RequiredReceivingTicks > 28)
                throw new SimulationContractException("SimulationExportPortReceivingDurationInvalid");
            ValidateIds(request.SourceStableIds, true,
                "SimulationExportPortReceiptSourceStableIdsInvalid");
        }
    }
}
