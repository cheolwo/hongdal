using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 수출선적실행DecisionTypeCode = "ExportShipmentExecution";
        private const string 수출선적실행DecisionPrefix = "decision:export-shipment-execution:";
        private readonly Dictionary<string, Simulation수출선적실행Snapshot> 수출선적실행원장 =
            new Dictionary<string, Simulation수출선적실행Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> 수출선적실행계획연결 =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public Simulation수출선적실행PreviewSnapshot Preview수출선적실행(
            Simulation수출선적실행PreviewRequest request)
        {
            Validate수출선적실행Request(request);
            lock (gate)
            {
                return Create수출선적실행Preview(request);
            }
        }

        public 경영SimulationSessionSnapshot Confirm수출선적실행(
            Simulation수출선적실행ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Validate수출선적실행Request(request.Execution);
            return ConfirmDecision(new SimulationDecisionConfirmRequest
            {
                CommandId = request.CommandId.Trim(),
                ExpectedRevision = request.ExpectedRevision,
                Preview = Create수출선적실행DecisionRequest(request.Execution),
            });
        }

        private Simulation수출선적실행PreviewSnapshot Create수출선적실행Preview(
            Simulation수출선적실행PreviewRequest request)
        {
            var common = Create수출선적실행DecisionRequest(request);
            수출선적계획원장.TryGetValue(request.SourceShipmentPlanStableId.Trim(), out var plan);
            var settlement = CreateSettlementSnapshot();
            var projectedRevenue = Find수출선적기존예상매출(plan);
            var successDelta = plan == null
                ? 0m
                : plan.ExpectedNetRevenue - projectedRevenue;
            var lossDelta = plan == null
                ? 0m
                : -plan.ExpectedTotalCost - projectedRevenue;
            var treasury = settlement?.TreasuryBalance ?? 0m;
            return new Simulation수출선적실행PreviewSnapshot
            {
                ExecutionStableId = request.ExecutionStableId.Trim(),
                SourceShipmentPlanStableId = request.SourceShipmentPlanStableId.Trim(),
                SourceReadinessReviewStableId = plan?.SourceReadinessReviewStableId ?? string.Empty,
                SourcePortReceiptStableId = plan?.SourcePortReceiptStableId ?? string.Empty,
                CargoStableId = plan?.CargoStableId ?? string.Empty,
                SourceAllocationStableId = plan?.SourceAllocationStableId ?? string.Empty,
                HarvestLotStableId = plan?.HarvestLotStableId ?? string.Empty,
                PackageLotStableId = plan?.PackageLotStableId ?? string.Empty,
                ProductStableId = plan?.ProductStableId ?? string.Empty,
                Quantity = plan?.Quantity ?? 0m,
                UnitCode = plan?.UnitCode ?? string.Empty,
                DestinationCountryCode = plan?.DestinationCountryCode ?? string.Empty,
                DestinationMarketStableId = plan?.DestinationMarketStableId ?? string.Empty,
                TransportModeCode = plan?.TransportModeCode ?? string.Empty,
                EstimatedTransitTicks = plan?.EstimatedTransitTicks ?? 0,
                RiskScore = plan?.RiskScore ?? 0,
                SuccessProbabilityPercent = plan == null ? 0m : 100m - plan.RiskScore,
                TreasuryBefore = treasury,
                PreviouslyRecognizedProjectedRevenue = projectedRevenue,
                SuccessTreasuryDeltaCandidate = successDelta,
                LossTreasuryDeltaCandidate = lossDelta,
                SuccessTreasuryAfterCandidate = treasury + successDelta,
                LossTreasuryAfterCandidate = treasury + lossDelta,
                RequiredLossCapacityReservation = Math.Max(0m, -lossDelta),
                CurrencyCode = plan?.CurrencyCode ?? string.Empty,
                IsCandidateOnly = true,
                OutcomeHiddenUntilCompletion = true,
                DoesNotCreateOperationalShipment = true,
                BoundaryCodes = 수출선적실행BoundaryCodes(),
                CommonDecisionPreview = CreateDecisionPreview(common),
            };
        }

        private SimulationDecisionPreviewRequest Create수출선적실행DecisionRequest(
            Simulation수출선적실행PreviewRequest request)
        {
            var executionId = request.ExecutionStableId.Trim();
            var planId = request.SourceShipmentPlanStableId.Trim();
            var blocks = new List<string>();
            Simulation수출선적계획Snapshot? plan = null;
            if (!수출선적계획원장.TryGetValue(planId, out plan))
            {
                blocks.Add("ExportShipmentPlanNotFound");
            }
            else
            {
                if (plan.StateCode != Simulation수출선적계획상태Codes.PlannedCandidate)
                    blocks.Add("ExportShipmentPlanNotCompleted");
                if (plan.PlanningFacilityStableId != request.ExecutionFacilityStableId.Trim())
                    blocks.Add("ExportShipmentExecutionFacilityMismatch");
                if (!string.IsNullOrWhiteSpace(plan.ExecutionStableId))
                    blocks.Add("ExportShipmentPlanAlreadyExecuted");
                if (!harvestLotAllocations.TryGetValue(plan.SourceAllocationStableId,
                        out var allocation)
                    || allocation.OutboundReservedQuantity < plan.Quantity)
                    blocks.Add("ExportShipmentReservedQuantityMissing");
                if (settlementInitialState == null
                    || plan.CurrencyCode != settlementInitialState.CurrencyCode)
                    blocks.Add("ExportShipmentCurrencyMismatch");
                else
                {
                    var projectedRevenue = Find수출선적기존예상매출(plan);
                    var lossDelta = -plan.ExpectedTotalCost - projectedRevenue;
                    var requiredReservation = Math.Max(0m, -lossDelta);
                    var treasuryAvailable = settlementInitialState.TreasuryBalance
                        - settlementTreasuryReserved;
                    if (treasuryAvailable < requiredReservation)
                        blocks.Add("ExportShipmentLossCapacityInsufficient");
                }
            }
            if (수출선적실행원장.ContainsKey(executionId))
                blocks.Add("ExportShipmentExecutionStableIdConflict");
            if (수출선적실행계획연결.ContainsKey(planId))
                blocks.Add("ExportShipmentExecutionAlreadyScheduled");
            if (settlementInitialState == null
                || !settlementInitialState.Facilities.Any(value =>
                    value.FacilityStableId == request.ExecutionFacilityStableId.Trim()))
                blocks.Add("ExportShipmentExecutionFacilityNotFound");

            var sources = MergeSources(request.SourceStableIds, new[] { planId });
            var quantity = plan?.Quantity ?? 1m;
            var unitCode = plan?.UnitCode ?? "KGM";
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = 수출선적실행DecisionPrefix + executionId,
                DecisionTypeCode = 수출선적실행DecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    executionId,
                    planId,
                    request.ExecutionFacilityStableId.Trim(),
                },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "ExportShipmentSimulationQuantity",
                        TargetLedgerStableId = executionId,
                        BeforeValue = 0m,
                        Delta = quantity,
                        AfterValue = quantity,
                        UnitCode = unitCode,
                        SourceStableIds = sources,
                    },
                },
                Uncertainties = new[]
                {
                    "The outcome remains hidden until the simulation transit task completes.",
                    "The result is deterministic from the scenario seed and execution stable ID, not an operational forecast.",
                    "Simulation execution does not book transport, submit declarations, clear customs, or load real cargo.",
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:export-shipment-execution:" + executionId,
                    TaskTypeCode = 수출선적실행DecisionTypeCode,
                    FacilityStableId = request.ExecutionFacilityStableId.Trim(),
                    AssignedCapacity = quantity,
                    AssignedCapacityUnitCode = unitCode,
                    DurationTicks = plan?.EstimatedTransitTicks ?? 1,
                    InputLotStableIds = new[] { planId },
                    OutputCandidateCodes = new[]
                    {
                        "delivered-or-disrupted-simulation-outcome",
                    },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation수출선적실행Snapshot? Prepare수출선적실행(
            SimulationDecisionPreviewRequest request,
            SimulationDecisionPreviewSnapshot preview)
        {
            if (request.DecisionTypeCode != 수출선적실행DecisionTypeCode) return null;
            var executionId = request.DecisionStableId.Substring(수출선적실행DecisionPrefix.Length);
            var plan = request.TargetStableIds
                .Select(value => 수출선적계획원장.TryGetValue(value, out var found) ? found : null)
                .First(value => value != null)!;
            var projectedRevenue = Find수출선적기존예상매출(plan);
            var successDelta = plan.ExpectedNetRevenue - projectedRevenue;
            var lossDelta = -plan.ExpectedTotalCost - projectedRevenue;
            return new Simulation수출선적실행Snapshot
            {
                ExecutionStableId = executionId,
                StateCode = Simulation수출선적실행상태Codes.Scheduled,
                Revision = 1,
                OutcomeCode = Simulation수출선적결과Codes.Pending,
                SourceShipmentPlanStableId = plan.PlanStableId,
                SourceReadinessReviewStableId = plan.SourceReadinessReviewStableId,
                SourcePortReceiptStableId = plan.SourcePortReceiptStableId,
                CargoStableId = plan.CargoStableId,
                SourceAllocationStableId = plan.SourceAllocationStableId,
                HarvestLotStableId = plan.HarvestLotStableId,
                PackageLotStableId = plan.PackageLotStableId,
                ProductStableId = plan.ProductStableId,
                Quantity = plan.Quantity,
                UnitCode = plan.UnitCode,
                DestinationCountryCode = plan.DestinationCountryCode,
                DestinationMarketStableId = plan.DestinationMarketStableId,
                TransportModeCode = plan.TransportModeCode,
                ExecutionFacilityStableId = plan.PlanningFacilityStableId,
                EstimatedTransitTicks = plan.EstimatedTransitTicks,
                RiskScore = plan.RiskScore,
                SuccessProbabilityPercent = 100m - plan.RiskScore,
                ExpectedGrossRevenue = plan.ExpectedGrossRevenue,
                ExpectedTotalCost = plan.ExpectedTotalCost,
                PreviouslyRecognizedProjectedRevenue = projectedRevenue,
                SuccessTreasuryDeltaCandidate = successDelta,
                LossTreasuryDeltaCandidate = lossDelta,
                RequiredLossCapacityReservation = Math.Max(0m, -lossDelta),
                CurrencyCode = plan.CurrencyCode,
                DecisionStableId = preview.Decision.DecisionStableId,
                TaskStableId = preview.TaskPlan.TaskStableId,
                ScheduledTick = CurrentTick,
                BoundaryCodes = 수출선적실행BoundaryCodes(),
                SourceStableIds = Copy(preview.Decision.SourceStableIds),
            };
        }

        private void Schedule수출선적실행(Simulation수출선적실행Snapshot? execution)
        {
            if (execution == null) return;
            if (settlementInitialState == null)
                throw new SimulationContractException("SimulationSettlementRequiredForExportShipment");
            settlementTreasuryReserved += execution.RequiredLossCapacityReservation;
            var plan = 수출선적계획원장[execution.SourceShipmentPlanStableId];
            plan.ExecutionStableId = execution.ExecutionStableId;
            plan.Revision++;
            수출선적실행원장.Add(execution.ExecutionStableId, execution);
            수출선적실행계획연결.Add(plan.PlanStableId, execution.ExecutionStableId);
        }

        private void Advance수출선적실행ForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var execution = 수출선적실행원장.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId
                    && (value.StateCode == Simulation수출선적실행상태Codes.Scheduled
                        || value.StateCode == Simulation수출선적실행상태Codes.InTransit));
            if (execution == null || currentTick < task.ScheduledStartTick) return;
            if (execution.DepartedTick == null)
            {
                execution.DepartedTick = task.ScheduledStartTick;
                execution.StateCode = Simulation수출선적실행상태Codes.InTransit;
                execution.Revision++;
            }
            if (currentTick < task.ExpectedEndTick) return;
            Complete수출선적실행(execution, task);
        }

        private void Complete수출선적실행(
            Simulation수출선적실행Snapshot execution,
            SimulationTaskSnapshot task)
        {
            if (settlementInitialState == null)
                throw new SimulationContractException("SimulationSettlementRequiredForExportShipment");
            var outcomeRoll = Create수출선적결과Roll(execution.ExecutionStableId);
            var delivered = outcomeRoll > execution.RiskScore;
            var treasuryDelta = delivered
                ? execution.SuccessTreasuryDeltaCandidate
                : execution.LossTreasuryDeltaCandidate;
            var treasuryBefore = settlementInitialState.TreasuryBalance;
            var treasuryAfter = treasuryBefore + treasuryDelta;
            if (treasuryAfter < 0m)
                throw new SimulationConflictException("ExportShipmentTreasuryWouldBecomeNegative");

            settlementTreasuryReserved -= execution.RequiredLossCapacityReservation;
            settlementInitialState.TreasuryBalance = treasuryAfter;
            var allocation = harvestLotAllocations[execution.SourceAllocationStableId];
            allocation.OutboundReservedQuantity -= execution.Quantity;
            execution.OutcomeRoll = outcomeRoll;
            execution.OutcomeCode = delivered
                ? Simulation수출선적결과Codes.Delivered
                : Simulation수출선적결과Codes.DisruptedWithLoss;
            execution.StateCode = delivered
                ? Simulation수출선적실행상태Codes.DeliveredInSimulation
                : Simulation수출선적실행상태Codes.DisruptedWithLossInSimulation;
            execution.DeliveredQuantity = delivered ? execution.Quantity : 0m;
            execution.LostQuantity = delivered ? 0m : execution.Quantity;
            execution.AppliedTreasuryDelta = treasuryDelta;
            execution.TreasuryBeforeApplication = treasuryBefore;
            execution.TreasuryAfterApplication = treasuryAfter;
            execution.CompletedTick = task.ExpectedEndTick;
            execution.Revision++;

            var plan = 수출선적계획원장[execution.SourceShipmentPlanStableId];
            plan.ExecutionCompletedTick = task.ExpectedEndTick;
            plan.Revision++;
            Add수출선적결과Effect(execution, task, treasuryBefore, treasuryAfter);
        }

        private void Add수출선적결과Effect(
            Simulation수출선적실행Snapshot execution,
            SimulationTaskSnapshot task,
            decimal treasuryBefore,
            decimal treasuryAfter)
        {
            var effectId = task.TaskStableId + ":outcome-effect:1";
            if (effects.ContainsKey(effectId))
                throw new SimulationConflictException("SimulationEffectStableIdConflict");
            effects.Add(effectId, new SimulationEffectRecord
            {
                EffectStableId = effectId,
                EffectTypeCode = execution.OutcomeCode == Simulation수출선적결과Codes.Delivered
                    ? "ExportShipmentDeliveredTreasuryReconciliation"
                    : "ExportShipmentLossTreasuryReconciliation",
                StateCode = SimulationEffectStateCodes.Applied,
                Revision = 1,
                CausedByDecisionStableId = execution.DecisionStableId,
                CausedByTaskStableId = task.TaskStableId,
                TargetLedgerStableId = "ledger:simulation:" + SettlementStableId + ":treasury",
                BeforeValue = treasuryBefore,
                Delta = execution.AppliedTreasuryDelta!.Value,
                AfterValue = treasuryAfter,
                UnitCode = execution.CurrencyCode,
                AppliedTick = task.ExpectedEndTick,
                SourceStableIds = Copy(execution.SourceStableIds),
            });
        }

        private decimal Find수출선적기존예상매출(Simulation수출선적계획Snapshot? plan)
        {
            if (plan == null) return 0m;
            return harvestLotAllocations.TryGetValue(plan.SourceAllocationStableId, out var allocation)
                ? allocation.ProjectedRevenue ?? 0m
                : 0m;
        }

        private int Create수출선적결과Roll(string executionStableId)
        {
            unchecked
            {
                uint hash = 2166136261;
                for (var shift = 0; shift < 32; shift += 8)
                {
                    hash ^= (byte)(ScenarioSeed >> shift);
                    hash *= 16777619;
                }
                foreach (var character in executionStableId)
                {
                    hash ^= character;
                    hash *= 16777619;
                }
                return (int)(hash % 100) + 1;
            }
        }

        private Simulation수출선적실행Snapshot[] Create수출선적실행Snapshots()
            => 수출선적실행원장.Values
                .OrderBy(value => value.ExecutionStableId, StringComparer.Ordinal)
                .Select(Clone수출선적실행).ToArray();

        internal static Simulation수출선적실행Snapshot Clone수출선적실행(
            Simulation수출선적실행Snapshot source)
            => new Simulation수출선적실행Snapshot
            {
                ExecutionStableId = source.ExecutionStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                OutcomeCode = source.OutcomeCode,
                OutcomeRoll = source.OutcomeRoll,
                SourceShipmentPlanStableId = source.SourceShipmentPlanStableId,
                SourceReadinessReviewStableId = source.SourceReadinessReviewStableId,
                SourcePortReceiptStableId = source.SourcePortReceiptStableId,
                CargoStableId = source.CargoStableId,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                DeliveredQuantity = source.DeliveredQuantity,
                LostQuantity = source.LostQuantity,
                UnitCode = source.UnitCode,
                DestinationCountryCode = source.DestinationCountryCode,
                DestinationMarketStableId = source.DestinationMarketStableId,
                TransportModeCode = source.TransportModeCode,
                ExecutionFacilityStableId = source.ExecutionFacilityStableId,
                EstimatedTransitTicks = source.EstimatedTransitTicks,
                RiskScore = source.RiskScore,
                SuccessProbabilityPercent = source.SuccessProbabilityPercent,
                ExpectedGrossRevenue = source.ExpectedGrossRevenue,
                ExpectedTotalCost = source.ExpectedTotalCost,
                PreviouslyRecognizedProjectedRevenue = source.PreviouslyRecognizedProjectedRevenue,
                SuccessTreasuryDeltaCandidate = source.SuccessTreasuryDeltaCandidate,
                LossTreasuryDeltaCandidate = source.LossTreasuryDeltaCandidate,
                RequiredLossCapacityReservation = source.RequiredLossCapacityReservation,
                AppliedTreasuryDelta = source.AppliedTreasuryDelta,
                TreasuryBeforeApplication = source.TreasuryBeforeApplication,
                TreasuryAfterApplication = source.TreasuryAfterApplication,
                CurrencyCode = source.CurrencyCode,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                ScheduledTick = source.ScheduledTick,
                DepartedTick = source.DepartedTick,
                CompletedTick = source.CompletedTick,
                BoundaryCodes = Copy(source.BoundaryCodes),
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static string[] 수출선적실행BoundaryCodes()
            => new[]
            {
                "SimulationOnly",
                "DeterministicScenarioOutcomeOnly",
                "NoCarrierBooking",
                "NoExportDeclaration",
                "NoOfficialInspection",
                "NoQuarantineApproval",
                "NoCustomsClearance",
                "NoVesselLoading",
                "NoOperationalSettlement",
            };

        private static void Validate수출선적실행Request(
            Simulation수출선적실행PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.ExecutionStableId,
                "SimulationExportShipmentExecutionStableIdInvalid");
            RequireStableId(request.SourceShipmentPlanStableId,
                "SimulationExportShipmentExecutionPlanStableIdInvalid");
            RequireStableId(request.ExecutionFacilityStableId,
                "SimulationExportShipmentExecutionFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId,
                "SimulationExportShipmentExecutionActorStableIdInvalid");
            var targets = new HashSet<string>(StringComparer.Ordinal)
            {
                request.ExecutionStableId.Trim(),
                request.SourceShipmentPlanStableId.Trim(),
                request.ExecutionFacilityStableId.Trim(),
            };
            if (targets.Count != 3)
                throw new SimulationContractException("SimulationExportShipmentExecutionTargetsMustDiffer");
            ValidateIds(request.SourceStableIds, true,
                "SimulationExportShipmentExecutionSourceStableIdsInvalid");
        }
    }
}
