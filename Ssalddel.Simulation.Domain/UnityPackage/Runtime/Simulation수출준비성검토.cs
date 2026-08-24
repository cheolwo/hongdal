using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 수출준비성검토DecisionTypeCode = "ExportReadinessReview";
        private const string 수출준비성검토DecisionPrefix = "decision:export-readiness-review:";
        private readonly Dictionary<string, Simulation수출준비성검토Snapshot> 수출준비성검토원장 =
            new Dictionary<string, Simulation수출준비성검토Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> 수출준비성검토항만인수연결 =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public Simulation수출준비성검토PreviewSnapshot Preview수출준비성검토(
            Simulation수출준비성검토PreviewRequest request)
        {
            Validate수출준비성검토Request(request);
            lock (gate)
            {
                return Create수출준비성검토Preview(request);
            }
        }

        public 경영SimulationSessionSnapshot Confirm수출준비성검토(
            Simulation수출준비성검토ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Validate수출준비성검토Request(request.Review);
            return ConfirmDecision(new SimulationDecisionConfirmRequest
            {
                CommandId = request.CommandId.Trim(),
                ExpectedRevision = request.ExpectedRevision,
                Preview = Create수출준비성검토DecisionRequest(request.Review),
            });
        }

        private Simulation수출준비성검토PreviewSnapshot Create수출준비성검토Preview(
            Simulation수출준비성검토PreviewRequest request)
        {
            var common = Create수출준비성검토DecisionRequest(request);
            수출항만인수원장.TryGetValue(request.SourcePortReceiptStableId.Trim(), out var receipt);
            var parent = FindLatest수출준비성검토(request.SourcePortReceiptStableId.Trim());
            var missing = Create수출준비성보완Codes(
                request.DocumentsPrepared, request.InspectionPreparationReady);
            return new Simulation수출준비성검토PreviewSnapshot
            {
                ReviewStableId = request.ReviewStableId.Trim(),
                SourcePortReceiptStableId = request.SourcePortReceiptStableId.Trim(),
                ParentReviewStableId = parent?.ReviewStableId,
                AttemptNumber = (parent?.AttemptNumber ?? 0) + 1,
                CargoStableId = receipt?.CargoStableId ?? string.Empty,
                SourceExportCargoHandoffStableId =
                    receipt?.SourceExportCargoHandoffStableId ?? string.Empty,
                SourceAllocationStableId = receipt?.SourceAllocationStableId ?? string.Empty,
                HarvestLotStableId = receipt?.HarvestLotStableId ?? string.Empty,
                PackageLotStableId = receipt?.PackageLotStableId ?? string.Empty,
                ProductStableId = receipt?.ProductStableId ?? string.Empty,
                Quantity = receipt?.Quantity ?? 0m,
                UnitCode = receipt?.UnitCode ?? string.Empty,
                ReviewingFacilityStableId = request.ReviewingFacilityStableId.Trim(),
                DocumentsPrepared = request.DocumentsPrepared,
                InspectionPreparationReady = request.InspectionPreparationReady,
                OutcomeCode = Create수출준비성검토결과Code(missing),
                MissingRequirementCodes = missing,
                IsCandidateOnly = true,
                DoesNotCreateOperationalExport = true,
                BoundaryCodes = 수출준비성검토BoundaryCodes(),
                CommonDecisionPreview = CreateDecisionPreview(common),
            };
        }

        private SimulationDecisionPreviewRequest Create수출준비성검토DecisionRequest(
            Simulation수출준비성검토PreviewRequest request)
        {
            var reviewId = request.ReviewStableId.Trim();
            var receiptId = request.SourcePortReceiptStableId.Trim();
            var blocks = new List<string>();
            Simulation수출항만인수Snapshot? receipt = null;
            if (!수출항만인수원장.TryGetValue(receiptId, out receipt))
            {
                blocks.Add("ExportPortReceiptNotFound");
            }
            else
            {
                if (receipt.StateCode != Simulation수출항만인수상태Codes.ReceivedAtPortStaging)
                    blocks.Add("ExportPortReceiptNotCompleted");
                if (receipt.ReceivingFacilityStableId != request.ReviewingFacilityStableId.Trim())
                    blocks.Add("ExportReadinessReviewFacilityMismatch");
                if (!harvestLotAllocations.TryGetValue(receipt.SourceAllocationStableId,
                        out var allocation)
                    || allocation.OutboundReservedQuantity < receipt.Quantity)
                    blocks.Add("ExportReadinessReservedQuantityMissing");
            }

            if (수출준비성검토원장.ContainsKey(reviewId))
                blocks.Add("ExportReadinessReviewStableIdConflict");
            var parent = FindLatest수출준비성검토(receiptId);
            if (parent?.StateCode == Simulation수출준비성검토상태Codes.Scheduled)
                blocks.Add("ExportReadinessReviewAlreadyScheduled");
            if (parent?.StateCode == Simulation수출준비성검토상태Codes.ReadyCandidate)
                blocks.Add("ExportReadinessAlreadyReady");
            if (settlementInitialState == null
                || !settlementInitialState.Facilities.Any(value =>
                    value.FacilityStableId == request.ReviewingFacilityStableId.Trim()))
                blocks.Add("ExportReadinessReviewFacilityNotFound");

            var sources = MergeSources(request.SourceStableIds, new[] { receiptId });
            if (!string.IsNullOrWhiteSpace(parent?.ReviewStableId))
                sources = MergeSources(sources, new[] { parent.ReviewStableId });
            var quantity = receipt?.Quantity ?? 1m;
            var unitCode = receipt?.UnitCode ?? "KGM";
            var missing = Create수출준비성보완Codes(
                request.DocumentsPrepared, request.InspectionPreparationReady);
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = 수출준비성검토DecisionPrefix + reviewId,
                DecisionTypeCode = 수출준비성검토DecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    reviewId,
                    receiptId,
                    request.ReviewingFacilityStableId.Trim(),
                },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    Create수출준비성Projection(
                        "ExportDocumentsPrepared", reviewId, request.DocumentsPrepared, sources),
                    Create수출준비성Projection(
                        "ExportInspectionPreparationReady", reviewId,
                        request.InspectionPreparationReady, sources),
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "ExportReadinessReviewedQuantity",
                        TargetLedgerStableId = reviewId,
                        BeforeValue = 0m,
                        Delta = quantity,
                        AfterValue = quantity,
                        UnitCode = unitCode,
                        SourceStableIds = sources,
                    },
                },
                Uncertainties = new[]
                {
                    "Readiness inputs are simulation assertions, not verified government records.",
                    "A ready candidate is not an export declaration, inspection approval, customs clearance, or loading authorization.",
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:export-readiness-review:" + reviewId,
                    TaskTypeCode = 수출준비성검토DecisionTypeCode,
                    FacilityStableId = request.ReviewingFacilityStableId.Trim(),
                    AssignedCapacity = quantity,
                    AssignedCapacityUnitCode = unitCode,
                    DurationTicks = request.RequiredReviewTicks,
                    InputLotStableIds = new[] { receiptId },
                    OutputCandidateCodes = missing.Length == 0
                        ? new[] { "export-readiness-candidate" }
                        : missing,
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation수출준비성검토Snapshot? Prepare수출준비성검토(
            SimulationDecisionPreviewRequest request,
            SimulationDecisionPreviewSnapshot preview)
        {
            if (request.DecisionTypeCode != 수출준비성검토DecisionTypeCode) return null;
            var reviewId = request.DecisionStableId.Substring(수출준비성검토DecisionPrefix.Length);
            var receipt = request.TargetStableIds
                .Select(value => 수출항만인수원장.TryGetValue(value, out var found) ? found : null)
                .First(value => value != null)!;
            var reviewingFacility = request.TargetStableIds.Single(value =>
                value != reviewId && value != receipt.ReceiptStableId);
            var parent = FindLatest수출준비성검토(receipt.ReceiptStableId);
            var documentsPrepared = Find수출준비성ProjectionValue(
                request, "ExportDocumentsPrepared");
            var inspectionReady = Find수출준비성ProjectionValue(
                request, "ExportInspectionPreparationReady");
            var missing = Create수출준비성보완Codes(documentsPrepared, inspectionReady);
            return new Simulation수출준비성검토Snapshot
            {
                ReviewStableId = reviewId,
                StateCode = Simulation수출준비성검토상태Codes.Scheduled,
                Revision = 1,
                SourcePortReceiptStableId = receipt.ReceiptStableId,
                ParentReviewStableId = parent?.ReviewStableId,
                AttemptNumber = (parent?.AttemptNumber ?? 0) + 1,
                CargoStableId = receipt.CargoStableId,
                SourceExportCargoHandoffStableId = receipt.SourceExportCargoHandoffStableId,
                SourceAllocationStableId = receipt.SourceAllocationStableId,
                HarvestLotStableId = receipt.HarvestLotStableId,
                PackageLotStableId = receipt.PackageLotStableId,
                ProductStableId = receipt.ProductStableId,
                Quantity = receipt.Quantity,
                UnitCode = receipt.UnitCode,
                ReviewingFacilityStableId = reviewingFacility,
                DocumentsPrepared = documentsPrepared,
                InspectionPreparationReady = inspectionReady,
                OutcomeCode = Create수출준비성검토결과Code(missing),
                MissingRequirementCodes = missing,
                DecisionStableId = preview.Decision.DecisionStableId,
                TaskStableId = preview.TaskPlan.TaskStableId,
                RequiredReviewTicks = request.Task.DurationTicks,
                ScheduledTick = CurrentTick,
                BoundaryCodes = 수출준비성검토BoundaryCodes(),
                SourceStableIds = Copy(preview.Decision.SourceStableIds),
            };
        }

        private void Schedule수출준비성검토(Simulation수출준비성검토Snapshot? review)
        {
            if (review == null) return;
            수출준비성검토원장.Add(review.ReviewStableId, review);
            수출준비성검토항만인수연결[review.SourcePortReceiptStableId] = review.ReviewStableId;
        }

        private void Advance수출준비성검토ForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var review = 수출준비성검토원장.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId
                    && value.StateCode == Simulation수출준비성검토상태Codes.Scheduled);
            if (review == null || currentTick < task.ExpectedEndTick) return;
            review.StateCode = review.OutcomeCode;
            review.Revision++;
            review.CompletedTick = task.ExpectedEndTick;
        }

        private Simulation수출준비성검토Snapshot? FindLatest수출준비성검토(string receiptId)
        {
            if (!수출준비성검토항만인수연결.TryGetValue(receiptId, out var reviewId)) return null;
            return 수출준비성검토원장[reviewId];
        }

        private Simulation수출준비성검토Snapshot[] Create수출준비성검토Snapshots()
            => 수출준비성검토원장.Values
                .OrderBy(value => value.ReviewStableId, StringComparer.Ordinal)
                .Select(Clone수출준비성검토).ToArray();

        internal static Simulation수출준비성검토Snapshot Clone수출준비성검토(
            Simulation수출준비성검토Snapshot source)
            => new Simulation수출준비성검토Snapshot
            {
                ReviewStableId = source.ReviewStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                SourcePortReceiptStableId = source.SourcePortReceiptStableId,
                ParentReviewStableId = source.ParentReviewStableId,
                AttemptNumber = source.AttemptNumber,
                CargoStableId = source.CargoStableId,
                SourceExportCargoHandoffStableId = source.SourceExportCargoHandoffStableId,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                ReviewingFacilityStableId = source.ReviewingFacilityStableId,
                DocumentsPrepared = source.DocumentsPrepared,
                InspectionPreparationReady = source.InspectionPreparationReady,
                OutcomeCode = source.OutcomeCode,
                MissingRequirementCodes = Copy(source.MissingRequirementCodes),
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                RequiredReviewTicks = source.RequiredReviewTicks,
                ScheduledTick = source.ScheduledTick,
                CompletedTick = source.CompletedTick,
                BoundaryCodes = Copy(source.BoundaryCodes),
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static SimulationValueProjection Create수출준비성Projection(
            string valueTypeCode,
            string reviewId,
            bool value,
            string[] sources)
            => new SimulationValueProjection
            {
                ValueTypeCode = valueTypeCode,
                TargetLedgerStableId = reviewId,
                BeforeValue = 0m,
                Delta = value ? 1m : 0m,
                AfterValue = value ? 1m : 0m,
                UnitCode = "BOOL",
                SourceStableIds = sources,
            };

        private static bool Find수출준비성ProjectionValue(
            SimulationDecisionPreviewRequest request,
            string valueTypeCode)
            => request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == valueTypeCode).AfterValue == 1m;

        private static string[] Create수출준비성보완Codes(
            bool documentsPrepared,
            bool inspectionPreparationReady)
        {
            var result = new List<string>();
            if (!documentsPrepared)
                result.Add(Simulation수출준비성보완Codes.DocumentsNotPrepared);
            if (!inspectionPreparationReady)
                result.Add(Simulation수출준비성보완Codes.InspectionPreparationNotReady);
            return result.ToArray();
        }

        private static string Create수출준비성검토결과Code(string[] missing)
            => missing.Length == 0
                ? Simulation수출준비성검토결과Codes.ReadyCandidate
                : Simulation수출준비성검토결과Codes.ActionRequired;

        private static string[] 수출준비성검토BoundaryCodes()
            => new[]
            {
                "SimulationOnly",
                "ReadinessReviewOnly",
                "SelfAssertedInputsOnly",
                "NoExportDeclaration",
                "NoOfficialInspection",
                "NoQuarantineApproval",
                "NoCustomsClearance",
                "NoVesselLoading",
            };

        private static void Validate수출준비성검토Request(
            Simulation수출준비성검토PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.ReviewStableId,
                "SimulationExportReadinessReviewStableIdInvalid");
            RequireStableId(request.SourcePortReceiptStableId,
                "SimulationExportReadinessPortReceiptStableIdInvalid");
            RequireStableId(request.ReviewingFacilityStableId,
                "SimulationExportReadinessFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId,
                "SimulationExportReadinessActorStableIdInvalid");
            var targets = new HashSet<string>(StringComparer.Ordinal)
            {
                request.ReviewStableId.Trim(),
                request.SourcePortReceiptStableId.Trim(),
                request.ReviewingFacilityStableId.Trim(),
            };
            if (targets.Count != 3)
                throw new SimulationContractException("SimulationExportReadinessTargetsMustDiffer");
            if (request.RequiredReviewTicks <= 0 || request.RequiredReviewTicks > 28)
                throw new SimulationContractException("SimulationExportReadinessDurationInvalid");
            ValidateIds(request.SourceStableIds, true,
                "SimulationExportReadinessSourceStableIdsInvalid");
        }
    }
}
