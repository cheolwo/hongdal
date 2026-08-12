using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 수출선적계획DecisionTypeCode = "ExportShipmentPlan";
        private const string 수출선적계획DecisionPrefix = "decision:export-shipment-plan:";
        private readonly Dictionary<string, Simulation수출선적계획Snapshot> 수출선적계획원장 =
            new Dictionary<string, Simulation수출선적계획Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> 수출선적계획준비성검토연결 =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public Simulation수출선적계획PreviewSnapshot Preview수출선적계획(
            Simulation수출선적계획PreviewRequest request)
        {
            Validate수출선적계획Request(request);
            lock (gate)
            {
                return Create수출선적계획Preview(request);
            }
        }

        public 경영SimulationSessionSnapshot Confirm수출선적계획(
            Simulation수출선적계획ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Validate수출선적계획Request(request.Plan);
            return ConfirmDecision(new SimulationDecisionConfirmRequest
            {
                CommandId = request.CommandId.Trim(),
                ExpectedRevision = request.ExpectedRevision,
                Preview = Create수출선적계획DecisionRequest(request.Plan),
            });
        }

        private Simulation수출선적계획PreviewSnapshot Create수출선적계획Preview(
            Simulation수출선적계획PreviewRequest request)
        {
            var common = Create수출선적계획DecisionRequest(request);
            수출준비성검토원장.TryGetValue(
                request.SourceReadinessReviewStableId.Trim(), out var review);
            var totalCost = Create수출선적계획TotalCost(request);
            return new Simulation수출선적계획PreviewSnapshot
            {
                PlanStableId = request.PlanStableId.Trim(),
                SourceReadinessReviewStableId = request.SourceReadinessReviewStableId.Trim(),
                SourcePortReceiptStableId = review?.SourcePortReceiptStableId ?? string.Empty,
                CargoStableId = review?.CargoStableId ?? string.Empty,
                SourceAllocationStableId = review?.SourceAllocationStableId ?? string.Empty,
                HarvestLotStableId = review?.HarvestLotStableId ?? string.Empty,
                PackageLotStableId = review?.PackageLotStableId ?? string.Empty,
                ProductStableId = review?.ProductStableId ?? string.Empty,
                Quantity = review?.Quantity ?? 0m,
                UnitCode = review?.UnitCode ?? string.Empty,
                DestinationCountryCode = request.DestinationCountryCode.Trim(),
                DestinationMarketStableId = request.DestinationMarketStableId.Trim(),
                TransportModeCode = request.TransportModeCode.Trim(),
                PlanningFacilityStableId = request.PlanningFacilityStableId.Trim(),
                ExpectedGrossRevenue = request.ExpectedGrossRevenue,
                ExpectedTotalCost = totalCost,
                ExpectedNetRevenue = request.ExpectedGrossRevenue - totalCost,
                CurrencyCode = request.CurrencyCode.Trim(),
                EstimatedTransitTicks = request.EstimatedTransitTicks,
                RiskScore = request.RiskScore,
                RiskLevelCode = Create수출위험수준Code(request.RiskScore),
                IsCandidateOnly = true,
                DoesNotChangeTreasury = true,
                DoesNotCreateOperationalShipment = true,
                BoundaryCodes = 수출선적계획BoundaryCodes(),
                CommonDecisionPreview = CreateDecisionPreview(common),
            };
        }

        private SimulationDecisionPreviewRequest Create수출선적계획DecisionRequest(
            Simulation수출선적계획PreviewRequest request)
        {
            var planId = request.PlanStableId.Trim();
            var reviewId = request.SourceReadinessReviewStableId.Trim();
            var blocks = new List<string>();
            Simulation수출준비성검토Snapshot? review = null;
            if (!수출준비성검토원장.TryGetValue(reviewId, out review))
            {
                blocks.Add("ExportReadinessReviewNotFound");
            }
            else
            {
                if (review.StateCode != Simulation수출준비성검토상태Codes.ReadyCandidate)
                    blocks.Add("ExportReadinessReviewNotReady");
                if (review.ReviewingFacilityStableId != request.PlanningFacilityStableId.Trim())
                    blocks.Add("ExportShipmentPlanningFacilityMismatch");
                if (!harvestLotAllocations.TryGetValue(review.SourceAllocationStableId,
                        out var allocation)
                    || allocation.OutboundReservedQuantity < review.Quantity)
                    blocks.Add("ExportShipmentReservedQuantityMissing");
            }
            if (수출선적계획원장.ContainsKey(planId))
                blocks.Add("ExportShipmentPlanStableIdConflict");
            if (수출선적계획준비성검토연결.ContainsKey(reviewId))
                blocks.Add("ExportShipmentPlanAlreadyScheduled");
            if (settlementInitialState == null
                || !settlementInitialState.Facilities.Any(value =>
                    value.FacilityStableId == request.PlanningFacilityStableId.Trim()))
                blocks.Add("ExportShipmentPlanningFacilityNotFound");

            var sources = MergeSources(request.SourceStableIds, new[] { reviewId });
            var quantity = review?.Quantity ?? 1m;
            var unitCode = review?.UnitCode ?? "KGM";
            var totalCost = Create수출선적계획TotalCost(request);
            var currency = request.CurrencyCode.Trim();
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = 수출선적계획DecisionPrefix + planId,
                DecisionTypeCode = 수출선적계획DecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    planId,
                    reviewId,
                    request.DestinationMarketStableId.Trim(),
                    request.PlanningFacilityStableId.Trim(),
                    "country:" + request.DestinationCountryCode.Trim(),
                    "transport-mode:" + request.TransportModeCode.Trim(),
                },
                ExpectedCosts = new[]
                {
                    Create수출선적계획Projection("ExpectedInternationalLogisticsCost", planId,
                        request.ExpectedInternationalLogisticsCost, currency, sources),
                    Create수출선적계획Projection("ExpectedHandlingCost", planId,
                        request.ExpectedHandlingCost, currency, sources),
                    Create수출선적계획Projection("ExpectedOtherCost", planId,
                        request.ExpectedOtherCost, currency, sources),
                },
                ExpectedEffects = new[]
                {
                    Create수출선적계획Projection("ExpectedGrossRevenue", planId,
                        request.ExpectedGrossRevenue, currency, sources),
                    Create수출선적계획Projection("ExpectedNetRevenue", planId,
                        request.ExpectedGrossRevenue - totalCost, currency, sources),
                    Create수출선적계획Projection("PlannedExportQuantity", planId,
                        quantity, unitCode, sources),
                    Create수출선적계획Projection("EstimatedTransitTicks", planId,
                        request.EstimatedTransitTicks, "TICK", sources),
                    Create수출선적계획Projection("ExportPlanRiskScore", planId,
                        request.RiskScore, "SCORE", sources),
                },
                Uncertainties = new[]
                {
                    "Revenue, cost, transit time, and risk are simulation estimates, not quotes or guarantees.",
                    "Planning does not submit documents, reserve transport, clear customs, load cargo, or change treasury.",
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:export-shipment-plan:" + planId,
                    TaskTypeCode = 수출선적계획DecisionTypeCode,
                    FacilityStableId = request.PlanningFacilityStableId.Trim(),
                    AssignedCapacity = quantity,
                    AssignedCapacityUnitCode = unitCode,
                    DurationTicks = request.RequiredPlanningTicks,
                    InputLotStableIds = new[] { reviewId },
                    OutputCandidateCodes = new[] { "export-shipment-plan-candidate" },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation수출선적계획Snapshot? Prepare수출선적계획(
            SimulationDecisionPreviewRequest request,
            SimulationDecisionPreviewSnapshot preview)
        {
            if (request.DecisionTypeCode != 수출선적계획DecisionTypeCode) return null;
            var planId = request.DecisionStableId.Substring(수출선적계획DecisionPrefix.Length);
            var review = request.TargetStableIds
                .Select(value => 수출준비성검토원장.TryGetValue(value, out var found) ? found : null)
                .First(value => value != null)!;
            var countryTarget = request.TargetStableIds.Single(value =>
                value.StartsWith("country:", StringComparison.Ordinal));
            var modeTarget = request.TargetStableIds.Single(value =>
                value.StartsWith("transport-mode:", StringComparison.Ordinal));
            var market = request.TargetStableIds.Single(value =>
                value != planId && value != review.ReviewStableId
                    && value != review.ReviewingFacilityStableId
                    && value != countryTarget && value != modeTarget);
            var logisticsCost = Find수출선적계획Projection(
                request.ExpectedCosts, "ExpectedInternationalLogisticsCost").AfterValue;
            var handlingCost = Find수출선적계획Projection(
                request.ExpectedCosts, "ExpectedHandlingCost").AfterValue;
            var otherCost = Find수출선적계획Projection(
                request.ExpectedCosts, "ExpectedOtherCost").AfterValue;
            var grossRevenue = Find수출선적계획Projection(
                request.ExpectedEffects, "ExpectedGrossRevenue").AfterValue;
            var netRevenue = Find수출선적계획Projection(
                request.ExpectedEffects, "ExpectedNetRevenue").AfterValue;
            var country = countryTarget.Substring("country:".Length);
            var mode = modeTarget.Substring("transport-mode:".Length);
            var transitTicks = decimal.ToInt32(Find수출선적계획Projection(
                request.ExpectedEffects, "EstimatedTransitTicks").AfterValue);
            var riskScore = decimal.ToInt32(Find수출선적계획Projection(
                request.ExpectedEffects, "ExportPlanRiskScore").AfterValue);
            return new Simulation수출선적계획Snapshot
            {
                PlanStableId = planId,
                StateCode = Simulation수출선적계획상태Codes.Scheduled,
                Revision = 1,
                SourceReadinessReviewStableId = review.ReviewStableId,
                SourcePortReceiptStableId = review.SourcePortReceiptStableId,
                CargoStableId = review.CargoStableId,
                SourceAllocationStableId = review.SourceAllocationStableId,
                HarvestLotStableId = review.HarvestLotStableId,
                PackageLotStableId = review.PackageLotStableId,
                ProductStableId = review.ProductStableId,
                Quantity = review.Quantity,
                UnitCode = review.UnitCode,
                DestinationCountryCode = country,
                DestinationMarketStableId = market,
                TransportModeCode = mode,
                PlanningFacilityStableId = review.ReviewingFacilityStableId,
                ExpectedGrossRevenue = grossRevenue,
                ExpectedInternationalLogisticsCost = logisticsCost,
                ExpectedHandlingCost = handlingCost,
                ExpectedOtherCost = otherCost,
                ExpectedTotalCost = logisticsCost + handlingCost + otherCost,
                ExpectedNetRevenue = netRevenue,
                CurrencyCode = Find수출선적계획Projection(
                    request.ExpectedEffects, "ExpectedGrossRevenue").UnitCode,
                EstimatedTransitTicks = transitTicks,
                RiskScore = riskScore,
                RiskLevelCode = Create수출위험수준Code(riskScore),
                DecisionStableId = preview.Decision.DecisionStableId,
                TaskStableId = preview.TaskPlan.TaskStableId,
                RequiredPlanningTicks = request.Task.DurationTicks,
                ScheduledTick = CurrentTick,
                BoundaryCodes = 수출선적계획BoundaryCodes(),
                SourceStableIds = Copy(preview.Decision.SourceStableIds),
            };
        }

        private void Schedule수출선적계획(Simulation수출선적계획Snapshot? plan)
        {
            if (plan == null) return;
            수출선적계획원장.Add(plan.PlanStableId, plan);
            수출선적계획준비성검토연결.Add(
                plan.SourceReadinessReviewStableId, plan.PlanStableId);
        }

        private void Advance수출선적계획ForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var plan = 수출선적계획원장.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId
                    && value.StateCode == Simulation수출선적계획상태Codes.Scheduled);
            if (plan == null || currentTick < task.ExpectedEndTick) return;
            plan.StateCode = Simulation수출선적계획상태Codes.PlannedCandidate;
            plan.Revision++;
            plan.CompletedTick = task.ExpectedEndTick;
        }

        private Simulation수출선적계획Snapshot[] Create수출선적계획Snapshots()
            => 수출선적계획원장.Values
                .OrderBy(value => value.PlanStableId, StringComparer.Ordinal)
                .Select(Clone수출선적계획).ToArray();

        internal static Simulation수출선적계획Snapshot Clone수출선적계획(
            Simulation수출선적계획Snapshot source)
            => new Simulation수출선적계획Snapshot
            {
                PlanStableId = source.PlanStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                SourceReadinessReviewStableId = source.SourceReadinessReviewStableId,
                SourcePortReceiptStableId = source.SourcePortReceiptStableId,
                CargoStableId = source.CargoStableId,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                DestinationCountryCode = source.DestinationCountryCode,
                DestinationMarketStableId = source.DestinationMarketStableId,
                TransportModeCode = source.TransportModeCode,
                PlanningFacilityStableId = source.PlanningFacilityStableId,
                ExpectedGrossRevenue = source.ExpectedGrossRevenue,
                ExpectedInternationalLogisticsCost = source.ExpectedInternationalLogisticsCost,
                ExpectedHandlingCost = source.ExpectedHandlingCost,
                ExpectedOtherCost = source.ExpectedOtherCost,
                ExpectedTotalCost = source.ExpectedTotalCost,
                ExpectedNetRevenue = source.ExpectedNetRevenue,
                CurrencyCode = source.CurrencyCode,
                EstimatedTransitTicks = source.EstimatedTransitTicks,
                RiskScore = source.RiskScore,
                RiskLevelCode = source.RiskLevelCode,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                RequiredPlanningTicks = source.RequiredPlanningTicks,
                ScheduledTick = source.ScheduledTick,
                CompletedTick = source.CompletedTick,
                ExecutionStableId = source.ExecutionStableId,
                ExecutionCompletedTick = source.ExecutionCompletedTick,
                BoundaryCodes = Copy(source.BoundaryCodes),
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static decimal Create수출선적계획TotalCost(
            Simulation수출선적계획PreviewRequest request)
            => request.ExpectedInternationalLogisticsCost
                + request.ExpectedHandlingCost
                + request.ExpectedOtherCost;

        private static SimulationValueProjection Create수출선적계획Projection(
            string valueTypeCode,
            string planId,
            decimal value,
            string unitCode,
            string[] sources)
            => new SimulationValueProjection
            {
                ValueTypeCode = valueTypeCode,
                TargetLedgerStableId = planId,
                BeforeValue = 0m,
                Delta = value,
                AfterValue = value,
                UnitCode = unitCode,
                SourceStableIds = sources,
            };

        private static SimulationValueProjection Find수출선적계획Projection(
            SimulationValueProjection[] values,
            string valueTypeCode)
            => values.Single(value => value.ValueTypeCode == valueTypeCode);

        private static string Create수출위험수준Code(int riskScore)
        {
            if (riskScore <= 33) return Simulation수출위험수준Codes.Low;
            if (riskScore <= 66) return Simulation수출위험수준Codes.Medium;
            return Simulation수출위험수준Codes.High;
        }

        private static string[] 수출선적계획BoundaryCodes()
            => new[]
            {
                "SimulationOnly",
                "ShipmentPlanCandidateOnly",
                "EstimatedCommercialTermsOnly",
                "NoTreasuryMutation",
                "NoCarrierBooking",
                "NoExportDeclaration",
                "NoOfficialInspection",
                "NoQuarantineApproval",
                "NoCustomsClearance",
                "NoVesselLoading",
            };

        private static void Validate수출선적계획Request(
            Simulation수출선적계획PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.PlanStableId, "SimulationExportShipmentPlanStableIdInvalid");
            RequireStableId(request.SourceReadinessReviewStableId,
                "SimulationExportShipmentReadinessReviewStableIdInvalid");
            RequireStableId(request.DestinationCountryCode,
                "SimulationExportShipmentDestinationCountryCodeInvalid");
            RequireStableId(request.DestinationMarketStableId,
                "SimulationExportShipmentDestinationMarketStableIdInvalid");
            RequireStableId(request.PlanningFacilityStableId,
                "SimulationExportShipmentPlanningFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationExportShipmentActorStableIdInvalid");
            RequireStableId(request.CurrencyCode,
                "SimulationExportShipmentCurrencyCodeInvalid");
            if (request.TransportModeCode != Simulation수출운송방식Codes.Ocean
                && request.TransportModeCode != Simulation수출운송방식Codes.Air)
                throw new SimulationContractException("SimulationExportShipmentTransportModeInvalid");
            if (request.ExpectedGrossRevenue < 0m
                || request.ExpectedInternationalLogisticsCost < 0m
                || request.ExpectedHandlingCost < 0m
                || request.ExpectedOtherCost < 0m)
                throw new SimulationContractException("SimulationExportShipmentEstimateInvalid");
            if (request.EstimatedTransitTicks <= 0 || request.EstimatedTransitTicks > 365)
                throw new SimulationContractException("SimulationExportShipmentTransitTicksInvalid");
            if (request.RiskScore < 0 || request.RiskScore > 100)
                throw new SimulationContractException("SimulationExportShipmentRiskScoreInvalid");
            if (request.RequiredPlanningTicks <= 0 || request.RequiredPlanningTicks > 28)
                throw new SimulationContractException("SimulationExportShipmentPlanningTicksInvalid");
            var targets = new HashSet<string>(StringComparer.Ordinal)
            {
                request.PlanStableId.Trim(),
                request.SourceReadinessReviewStableId.Trim(),
                request.DestinationMarketStableId.Trim(),
                request.PlanningFacilityStableId.Trim(),
                "country:" + request.DestinationCountryCode.Trim(),
                "transport-mode:" + request.TransportModeCode.Trim(),
            };
            if (targets.Count != 6)
                throw new SimulationContractException("SimulationExportShipmentTargetsMustDiffer");
            ValidateIds(request.SourceStableIds, true,
                "SimulationExportShipmentSourceStableIdsInvalid");
        }
    }
}
