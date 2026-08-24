using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 수출준비DecisionTypeCode = "ExportPreparation";
        private readonly Dictionary<string, Simulation수출준비Snapshot> 수출준비원장 =
            new Dictionary<string, Simulation수출준비Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> 수출준비배분연결 =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public Simulation수출준비PreviewSnapshot Preview수출준비(
            Simulation수출준비PreviewRequest request)
        {
            Validate수출준비Request(request);
            lock (gate)
            {
                return Create수출준비Preview(request);
            }
        }

        public 경영SimulationSessionSnapshot Confirm수출준비(
            Simulation수출준비ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Validate수출준비Request(request.Preparation);
            return ConfirmDecision(new SimulationDecisionConfirmRequest
            {
                CommandId = request.CommandId.Trim(),
                ExpectedRevision = request.ExpectedRevision,
                Preview = Create수출준비DecisionRequest(request.Preparation),
            });
        }

        public Simulation수출준비PreviewSnapshot Preview수출재작업(
            Simulation수출재작업PreviewRequest request)
        {
            Validate수출재작업Request(request);
            lock (gate)
            {
                return Create수출준비Preview(Create수출재작업준비Request(request));
            }
        }

        public 경영SimulationSessionSnapshot Confirm수출재작업(
            Simulation수출재작업ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Validate수출재작업Request(request.Rework);
            lock (gate)
            {
                var preparation = Create수출재작업준비Request(request.Rework);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId.Trim(),
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = Create수출준비DecisionRequest(preparation),
                });
            }
        }

        private Simulation수출준비PreviewSnapshot Create수출준비Preview(
            Simulation수출준비PreviewRequest request)
        {
            var commonRequest = Create수출준비DecisionRequest(request);
            var previous = Find이전수출준비(request.PreviousPreparationStableId);
            return new Simulation수출준비PreviewSnapshot
            {
                PreparationStableId = request.PreparationStableId.Trim(),
                RootPreparationStableId = previous?.RootPreparationStableId
                    ?? request.PreparationStableId.Trim(),
                PreviousPreparationStableId = previous?.PreparationStableId,
                AttemptNumber = (previous?.AttemptNumber ?? 0) + 1,
                IsReworkAttempt = previous != null,
                SourceAllocationStableId = request.SourceAllocationStableId.Trim(),
                Quantity = request.Quantity,
                UnitCode = request.UnitCode.Trim(),
                InspectionOutcomeCode = request.InspectionOutcomeCode.Trim(),
                PackageLotCandidateStableId = PackageLotCandidateId(request.PreparationStableId),
                HandoffCandidateStableId = HandoffCandidateId(request.PreparationStableId),
                IsCandidateOnly = true,
                DoesNotCreateOperationalExport = true,
                BoundaryCodes = new[]
                {
                    "SimulationOnly",
                    "NoExportDeclaration",
                    "NoCustomsClearance",
                    "NoTradeContract",
                    "NoOperationalCarrierHandoff",
                    "NoOperationalSettlement",
                },
                CommonDecisionPreview = CreateDecisionPreview(commonRequest),
            };
        }

        private SimulationDecisionPreviewRequest Create수출준비DecisionRequest(
            Simulation수출준비PreviewRequest request)
        {
            var allocationId = request.SourceAllocationStableId.Trim();
            var blocks = new List<string>();
            if (!harvestLotAllocations.TryGetValue(allocationId, out var allocation))
            {
                blocks.Add("SourceAllocationNotFound");
            }
            else
            {
                if (allocation.StateCode != SimulationHarvestLotAllocationStateCodes.Applied)
                    blocks.Add("SourceAllocationNotApplied");
                if (allocation.ChoiceCode != SimulationHarvestDispositionChoiceCodes.ExportAgent
                    || allocation.NextWorkflowCode != SimulationHarvestDispositionWorkflowCodes.ExportReadinessCandidate)
                    blocks.Add("SourceAllocationNotExportReadiness");
                if (allocation.AvailableQuantity < request.Quantity)
                    blocks.Add("SourceAllocationQuantityExceeded");
                if (!string.Equals(allocation.UnitCode, request.UnitCode.Trim(), StringComparison.Ordinal))
                    blocks.Add("SourceAllocationUnitMismatch");
            }
            if (수출준비원장.ContainsKey(request.PreparationStableId.Trim()))
                blocks.Add("ExportPreparationStableIdConflict");
            if (수출준비배분연결.ContainsKey(allocationId))
                blocks.Add("ExportPreparationSourceAlreadyReserved");
            var previous = Find이전수출준비(request.PreviousPreparationStableId);
            if (!string.IsNullOrWhiteSpace(request.PreviousPreparationStableId))
            {
                if (previous == null)
                    blocks.Add("PreviousExportPreparationNotFound");
                else
                {
                    if (previous.StateCode != Simulation수출준비상태Codes.ReworkRequired
                        || !previous.CanRetry)
                        blocks.Add("PreviousExportPreparationNotRetryable");
                    if (previous.SourceAllocationStableId != allocationId
                        || previous.Quantity != request.Quantity
                        || previous.UnitCode != request.UnitCode.Trim())
                        blocks.Add("PreviousExportPreparationLineageMismatch");
                }
            }
            if (settlementInitialState == null
                || !settlementInitialState.Facilities.Any(value =>
                    value.FacilityStableId == request.PackingFacilityStableId.Trim()))
                blocks.Add("PackingFacilityNotFound");
            if (settlementInitialState == null
                || !settlementInitialState.Facilities.Any(value =>
                    value.FacilityStableId == request.HandoffFacilityStableId.Trim()))
                blocks.Add("HandoffFacilityNotFound");

            var preparationId = request.PreparationStableId.Trim();
            var sources = MergeSources(request.SourceStableIds, new[] { allocationId });
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:export-preparation:" + preparationId,
                DecisionTypeCode = 수출준비DecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                    {
                        preparationId,
                        allocationId,
                        request.HandoffFacilityStableId.Trim(),
                    }
                    .Concat(string.IsNullOrWhiteSpace(request.PreviousPreparationStableId)
                        ? Array.Empty<string>()
                        : new[] { request.PreviousPreparationStableId.Trim() })
                    .ToArray(),
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "ExportPreparationProcessedQuantity",
                        TargetLedgerStableId = preparationId,
                        BeforeValue = 0m,
                        Delta = request.Quantity,
                        AfterValue = request.Quantity,
                        UnitCode = request.UnitCode.Trim(),
                        SourceStableIds = sources,
                    },
                },
                Uncertainties = new[]
                {
                    "Inspection outcome is a simulation input, not an official inspection result.",
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:export-preparation:" + preparationId,
                    TaskTypeCode = 수출준비DecisionTypeCode,
                    FacilityStableId = request.PackingFacilityStableId.Trim(),
                    AssignedCapacity = request.Quantity,
                    AssignedCapacityUnitCode = request.UnitCode.Trim(),
                    DurationTicks = request.PackagingTicks + request.InspectionTicks,
                    InputLotStableIds = new[] { allocationId },
                    OutputCandidateCodes = new[]
                    {
                        request.InspectionOutcomeCode.Trim(),
                        HandoffCandidateId(preparationId),
                        request.FailureReasonCode?.Trim() ?? "NoFailureReason",
                        request.PackagingTicks.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation수출준비Snapshot? Prepare수출준비(
            SimulationDecisionPreviewRequest request,
            SimulationDecisionPreviewSnapshot preview)
        {
            if (request.DecisionTypeCode != 수출준비DecisionTypeCode) return null;
            var allocationId = request.Task.InputLotStableIds.Single();
            if (!harvestLotAllocations.TryGetValue(allocationId, out var allocation))
                throw new SimulationConflictException("SimulationExportSourceAllocationNotFound");
            if (수출준비배분연결.ContainsKey(allocationId))
                throw new SimulationConflictException("SimulationExportSourceAlreadyReserved");
            if (allocation.AvailableQuantity < request.Task.AssignedCapacity)
                throw new SimulationConflictException("SimulationExportSourceQuantityExceeded");

            const string decisionPrefix = "decision:export-preparation:";
            var preparationId = request.DecisionStableId.Substring(decisionPrefix.Length);
            var previous = request.TargetStableIds
                .Where(value => value != preparationId)
                .Select(value => 수출준비원장.TryGetValue(value, out var found) ? found : null)
                .FirstOrDefault(value => value != null);
            var handoffFacilityId = request.TargetStableIds.Single(value =>
                value != allocationId && value != preparationId
                    && value != previous?.PreparationStableId);
            var outcome = request.Task.OutputCandidateCodes.Single(value =>
                value == Simulation수출준비검사결과Codes.Passed
                || value == Simulation수출준비검사결과Codes.Failed);
            var packagingTicks = int.Parse(request.Task.OutputCandidateCodes.Single(value =>
                int.TryParse(value, out _)), System.Globalization.CultureInfo.InvariantCulture);
            var failureReason = request.Task.OutputCandidateCodes.Single(value =>
                value != outcome && value != HandoffCandidateId(preparationId)
                    && !int.TryParse(value, out _));
            return new Simulation수출준비Snapshot
            {
                PreparationStableId = preparationId,
                RootPreparationStableId = previous?.RootPreparationStableId ?? preparationId,
                PreviousPreparationStableId = previous?.PreparationStableId,
                AttemptNumber = (previous?.AttemptNumber ?? 0) + 1,
                IsReworkAttempt = previous != null,
                StateCode = previous == null
                    ? Simulation수출준비상태Codes.Scheduled
                    : Simulation수출준비상태Codes.ReworkScheduled,
                Revision = 1,
                SourceAllocationStableId = allocationId,
                HarvestLotStableId = allocation.HarvestLotStableId,
                ProductStableId = allocation.ProductStableId,
                Quantity = request.Task.AssignedCapacity,
                UnitCode = request.Task.AssignedCapacityUnitCode,
                PackingFacilityStableId = request.Task.FacilityStableId,
                HandoffFacilityStableId = handoffFacilityId,
                PackageLotCandidateStableId = PackageLotCandidateId(preparationId),
                HandoffCandidateStableId = HandoffCandidateId(preparationId),
                InspectionOutcomeCode = outcome,
                FailureReasonCode = failureReason == "NoFailureReason" ? null : failureReason,
                DecisionStableId = preview.Decision.DecisionStableId,
                TaskStableId = preview.TaskPlan.TaskStableId,
                PackagingTicks = packagingTicks,
                InspectionTicks = request.Task.DurationTicks - packagingTicks,
                ReservedTick = CurrentTick,
                SourceStableIds = Copy(preview.Decision.SourceStableIds),
            };
        }

        private void Schedule수출준비(Simulation수출준비Snapshot? preparation)
        {
            if (preparation == null) return;
            var allocation = harvestLotAllocations[preparation.SourceAllocationStableId];
            allocation.OutboundReservedQuantity += preparation.Quantity;
            allocation.AvailableQuantity = allocation.Quantity - allocation.OutboundReservedQuantity;
            if (preparation.PreviousPreparationStableId != null)
            {
                var previous = 수출준비원장[preparation.PreviousPreparationStableId];
                previous.CanRetry = false;
                previous.Revision++;
            }
            수출준비원장.Add(preparation.PreparationStableId, preparation);
            수출준비배분연결.Add(preparation.SourceAllocationStableId, preparation.PreparationStableId);
        }

        private void Advance수출준비ForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var preparation = 수출준비원장.Values.FirstOrDefault(value => value.TaskStableId == task.TaskStableId);
            if (preparation == null
                || preparation.StateCode == Simulation수출준비상태Codes.HandoffCandidateReady
                || preparation.StateCode == Simulation수출준비상태Codes.ReworkRequired)
                return;

            var packagedTick = task.ScheduledStartTick + preparation.PackagingTicks - 1;
            if (currentTick < packagedTick)
            {
                Set수출준비State(
                    preparation,
                    preparation.IsReworkAttempt
                        ? Simulation수출준비상태Codes.Reworking
                        : Simulation수출준비상태Codes.Packaging);
                return;
            }

            if (!preparation.PackagedTick.HasValue)
                preparation.PackagedTick = packagedTick;
            if (currentTick < task.ExpectedEndTick)
            {
                Set수출준비State(preparation, Simulation수출준비상태Codes.Inspection);
                return;
            }

            preparation.InspectedTick = task.ExpectedEndTick;
            if (preparation.InspectionOutcomeCode == Simulation수출준비검사결과Codes.Passed)
            {
                preparation.HandoffCandidateReadyTick = task.ExpectedEndTick;
                Set수출준비State(preparation, Simulation수출준비상태Codes.HandoffCandidateReady);
            }
            else
            {
                var allocation = harvestLotAllocations[preparation.SourceAllocationStableId];
                allocation.OutboundReservedQuantity -= preparation.Quantity;
                allocation.AvailableQuantity = allocation.Quantity - allocation.OutboundReservedQuantity;
                수출준비배분연결.Remove(preparation.SourceAllocationStableId);
                preparation.CanRetry = true;
                Set수출준비State(preparation, Simulation수출준비상태Codes.ReworkRequired);
            }
        }

        private static void Set수출준비State(Simulation수출준비Snapshot preparation, string stateCode)
        {
            if (preparation.StateCode == stateCode) return;
            preparation.StateCode = stateCode;
            preparation.Revision++;
        }

        private Simulation수출준비Snapshot[] Create수출준비Snapshots()
            => 수출준비원장.Values.OrderBy(value => value.PreparationStableId, StringComparer.Ordinal)
                .Select(Clone수출준비).ToArray();

        internal static Simulation수출준비Snapshot Clone수출준비(Simulation수출준비Snapshot source)
            => new Simulation수출준비Snapshot
            {
                PreparationStableId = source.PreparationStableId,
                RootPreparationStableId = source.RootPreparationStableId,
                PreviousPreparationStableId = source.PreviousPreparationStableId,
                AttemptNumber = source.AttemptNumber,
                IsReworkAttempt = source.IsReworkAttempt,
                StateCode = source.StateCode,
                Revision = source.Revision,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                PackingFacilityStableId = source.PackingFacilityStableId,
                HandoffFacilityStableId = source.HandoffFacilityStableId,
                PackageLotCandidateStableId = source.PackageLotCandidateStableId,
                HandoffCandidateStableId = source.HandoffCandidateStableId,
                InspectionOutcomeCode = source.InspectionOutcomeCode,
                FailureReasonCode = source.FailureReasonCode,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                PackagingTicks = source.PackagingTicks,
                InspectionTicks = source.InspectionTicks,
                ReservedTick = source.ReservedTick,
                PackagedTick = source.PackagedTick,
                InspectedTick = source.InspectedTick,
                HandoffCandidateReadyTick = source.HandoffCandidateReadyTick,
                CanRetry = source.CanRetry,
                CargoPreparationStableId = source.CargoPreparationStableId,
                CargoStableId = source.CargoStableId,
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static string PackageLotCandidateId(string preparationId)
            => "package-lot-candidate:export:" + preparationId.Trim();

        private static string HandoffCandidateId(string preparationId)
            => "handoff-candidate:export:" + preparationId.Trim();

        private Simulation수출준비Snapshot? Find이전수출준비(string? preparationStableId)
        {
            if (string.IsNullOrWhiteSpace(preparationStableId)) return null;
            return 수출준비원장.TryGetValue(preparationStableId.Trim(), out var preparation)
                ? preparation
                : null;
        }

        private Simulation수출준비PreviewRequest Create수출재작업준비Request(
            Simulation수출재작업PreviewRequest request)
        {
            var failedId = request.FailedPreparationStableId.Trim();
            if (!수출준비원장.TryGetValue(failedId, out var failed))
                throw new SimulationNotFoundException("SimulationExportPreparationNotFound");
            return new Simulation수출준비PreviewRequest
            {
                PreparationStableId = request.RetryPreparationStableId.Trim(),
                PreviousPreparationStableId = failedId,
                SourceAllocationStableId = failed.SourceAllocationStableId,
                Quantity = failed.Quantity,
                UnitCode = failed.UnitCode,
                PackingFacilityStableId = request.ReworkFacilityStableId.Trim(),
                HandoffFacilityStableId = request.HandoffFacilityStableId.Trim(),
                ActorStableId = request.ActorStableId.Trim(),
                PackagingTicks = request.ReworkTicks,
                InspectionTicks = request.InspectionTicks,
                InspectionOutcomeCode = request.InspectionOutcomeCode.Trim(),
                FailureReasonCode = request.FailureReasonCode?.Trim(),
                SourceStableIds = MergeSources(
                    request.SourceStableIds,
                    MergeSources(failed.SourceStableIds, new[] { failedId })),
            };
        }

        private static void Validate수출준비Request(Simulation수출준비PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.PreparationStableId, "SimulationExportPreparationStableIdInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreviousPreparationStableId))
                RequireStableId(request.PreviousPreparationStableId,
                    "SimulationExportPreviousPreparationStableIdInvalid");
            RequireStableId(request.SourceAllocationStableId, "SimulationExportSourceAllocationStableIdInvalid");
            RequireStableId(request.PackingFacilityStableId, "SimulationExportPackingFacilityStableIdInvalid");
            RequireStableId(request.HandoffFacilityStableId, "SimulationExportHandoffFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationExportActorStableIdInvalid");
            if (request.PackingFacilityStableId.Trim() == request.HandoffFacilityStableId.Trim())
                throw new SimulationContractException("SimulationExportFacilitiesMustDiffer");
            if (request.Quantity <= 0m)
                throw new SimulationContractException("SimulationExportQuantityInvalid");
            RequireStableId(request.UnitCode, "SimulationExportUnitCodeInvalid");
            if (request.PackagingTicks <= 0 || request.InspectionTicks <= 0
                || request.PackagingTicks + request.InspectionTicks > 28)
                throw new SimulationContractException("SimulationExportDurationInvalid");
            if (request.InspectionOutcomeCode != Simulation수출준비검사결과Codes.Passed
                && request.InspectionOutcomeCode != Simulation수출준비검사결과Codes.Failed)
                throw new SimulationContractException("SimulationExportInspectionOutcomeInvalid");
            if (request.InspectionOutcomeCode == Simulation수출준비검사결과Codes.Failed)
                RequireStableId(request.FailureReasonCode ?? string.Empty, "SimulationExportFailureReasonMissing");
            ValidateIds(request.SourceStableIds, true, "SimulationExportSourceStableIdsInvalid");
        }

        private static void Validate수출재작업Request(Simulation수출재작업PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.FailedPreparationStableId,
                "SimulationExportFailedPreparationStableIdInvalid");
            RequireStableId(request.RetryPreparationStableId,
                "SimulationExportRetryPreparationStableIdInvalid");
            if (request.FailedPreparationStableId.Trim() == request.RetryPreparationStableId.Trim())
                throw new SimulationContractException("SimulationExportRetryPreparationMustBeNew");
            RequireStableId(request.ReworkFacilityStableId,
                "SimulationExportReworkFacilityStableIdInvalid");
            RequireStableId(request.HandoffFacilityStableId,
                "SimulationExportHandoffFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationExportActorStableIdInvalid");
            if (request.ReworkFacilityStableId.Trim() == request.HandoffFacilityStableId.Trim())
                throw new SimulationContractException("SimulationExportFacilitiesMustDiffer");
            if (request.ReworkTicks <= 0 || request.InspectionTicks <= 0
                || request.ReworkTicks + request.InspectionTicks > 28)
                throw new SimulationContractException("SimulationExportReworkDurationInvalid");
            if (request.InspectionOutcomeCode != Simulation수출준비검사결과Codes.Passed
                && request.InspectionOutcomeCode != Simulation수출준비검사결과Codes.Failed)
                throw new SimulationContractException("SimulationExportInspectionOutcomeInvalid");
            if (request.InspectionOutcomeCode == Simulation수출준비검사결과Codes.Failed)
                RequireStableId(request.FailureReasonCode ?? string.Empty,
                    "SimulationExportFailureReasonMissing");
            ValidateIds(request.SourceStableIds, true, "SimulationExportSourceStableIdsInvalid");
        }
    }
}
