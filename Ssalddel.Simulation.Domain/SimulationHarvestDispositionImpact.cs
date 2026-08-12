using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string HarvestImpactPolicyRevision = "harvest-impact:fixture-r1";
        private const string HarvestImpactPolicySource = "source:fixture.harvest-impact-r1";
        private const decimal StorageShrinkageRate = 0.02m;
        private readonly Dictionary<string, AppliedHarvestDispositionImpactCommand> appliedHarvestImpactCommands =
            new Dictionary<string, AppliedHarvestDispositionImpactCommand>(StringComparer.Ordinal);

        public SimulationHarvestDispositionImpactPreviewSnapshot PreviewHarvestDispositionImpact(
            SimulationHarvestDispositionImpactPreviewRequest request)
        {
            ValidateHarvestDispositionImpactRequest(request);
            lock (gate)
            {
                return CreateHarvestDispositionImpactPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmHarvestDispositionImpact(
            SimulationHarvestDispositionImpactConfirmRequest request)
        {
            ValidateHarvestDispositionImpactConfirmRequest(request);
            lock (gate)
            {
                if (appliedCommands.ContainsKey(request.CommandId)
                    || appliedTurnClosingCommands.ContainsKey(request.CommandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                var payloadKey = BuildHarvestDispositionImpactPayloadKey(request.Impact);
                if (appliedHarvestImpactCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                if (appliedDecisionCommands.ContainsKey(request.CommandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");

                var impactPreview = CreateHarvestDispositionImpactPreview(request.Impact);
                if (impactPreview.CommonDecisionPreview.Decision.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException("SimulationDecisionPreviewBlocked");
                var commonRequest = CreateHarvestDispositionImpactDecisionRequest(request.Impact);
                var commonPreview = impactPreview.CommonDecisionPreview;
                if (CurrentTick + commonPreview.TaskPlan.DurationTicks > DurationTicks)
                    throw new SimulationConflictException("SimulationTaskDurationExceeded");
                if (decisions.ContainsKey(commonPreview.Decision.DecisionStableId))
                    throw new SimulationConflictException("SimulationDecisionStableIdConflict");
                if (tasks.ContainsKey(commonPreview.TaskPlan.TaskStableId))
                    throw new SimulationConflictException("SimulationTaskStableIdConflict");
                var effectCount = commonPreview.Decision.ExpectedCosts.Length
                    + commonPreview.Decision.ExpectedEffects.Length;
                for (var index = 1; index <= effectCount; index++)
                {
                    if (effects.ContainsKey(commonPreview.TaskPlan.TaskStableId + ":effect:"
                        + index.ToString(CultureInfo.InvariantCulture)))
                    {
                        throw new SimulationConflictException("SimulationEffectStableIdConflict");
                    }
                }
                var allocation = CreateHarvestLotAllocation(request.Impact, impactPreview);
                ReserveHarvestLotAllocation(allocation);
                var snapshot = ConfirmDecisionCore(
                    new SimulationDecisionConfirmRequest
                    {
                        CommandId = request.CommandId.Trim(),
                        ExpectedRevision = request.ExpectedRevision,
                        Preview = commonRequest,
                    },
                    true,
                    _ => AppendHarvestDispositionImpactConfirmCommand(request));
                appliedHarvestImpactCommands.Add(
                    request.CommandId.Trim(),
                    new AppliedHarvestDispositionImpactCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private bool HasAppliedHarvestDispositionImpactCommand(string commandId)
            => appliedHarvestImpactCommands.ContainsKey(commandId);

        internal static string BuildHarvestDispositionImpactPayloadKey(
            SimulationHarvestDispositionImpactPreviewRequest request)
            => string.Join("\u001e", new[]
            {
                request.DispositionDecisionStableId.Trim(),
                request.DispositionDecisionRevision.ToString(CultureInfo.InvariantCulture),
                request.HarvestLotStableId.Trim(),
                request.HarvestLotRevision.ToString(CultureInfo.InvariantCulture),
                request.ProductStableId.Trim(),
                request.Quantity.ToString(CultureInfo.InvariantCulture),
                request.UnitCode.Trim(),
                request.ChoiceCode.Trim(),
                request.NextWorkflowCode.Trim(),
                request.ActorStableId.Trim(),
                string.Join("\u001f", request.SourceStableIds.Select(value => value.Trim())
                    .OrderBy(value => value, StringComparer.Ordinal)),
            });

        private SimulationHarvestDispositionImpactPreviewSnapshot CreateHarvestDispositionImpactPreview(
            SimulationHarvestDispositionImpactPreviewRequest request)
        {
            var policy = HarvestDispositionImpactPolicy.For(request.ChoiceCode);
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException("SimulationSettlementRequiredForHarvestImpact");
            var commonRequest = CreateHarvestDispositionImpactDecisionRequest(
                request,
                policy,
                settlement,
                out var storageCandidate);
            return new SimulationHarvestDispositionImpactPreviewSnapshot
            {
                DispositionDecisionStableId = request.DispositionDecisionStableId.Trim(),
                DispositionDecisionRevision = request.DispositionDecisionRevision,
                ChoiceCode = request.ChoiceCode.Trim(),
                NextWorkflowCode = request.NextWorkflowCode.Trim(),
                HarvestLotStableId = request.HarvestLotStableId.Trim(),
                ProductStableId = request.ProductStableId.Trim(),
                Quantity = request.Quantity,
                SourceUnitCode = request.UnitCode.Trim(),
                CanonicalQuantityUnitCode = settlement.StorageUnitCode,
                RequiredLabor = policy.RequiredLabor,
                SimulationCost = policy.SimulationCost,
                ProjectedRevenue = policy.ProjectedRevenue,
                DurationTicks = policy.DurationTicks,
                FoodSecurityDaysBefore = settlement.FoodSecurityDays,
                FoodSecurityDaysCandidate = storageCandidate?.FoodSecurityDaysCandidate
                    ?? settlement.FoodSecurityDays,
                IsCandidateOnly = true,
                DoesNotApplySettlementState = true,
                PolicyRevision = HarvestImpactPolicyRevision,
                RiskCodes = policy.RiskCodes.ToArray(),
                BoundaryCodes = new[]
                {
                    "CandidateOnly",
                    "NoOperationalEffect",
                    "NoSettlementMutationBeforeConfirm",
                },
                StorageCandidate = storageCandidate,
                CommonDecisionPreview = CreateDecisionPreview(commonRequest),
            };
        }

        private SimulationDecisionPreviewRequest CreateHarvestDispositionImpactDecisionRequest(
            SimulationHarvestDispositionImpactPreviewRequest request)
        {
            var policy = HarvestDispositionImpactPolicy.For(request.ChoiceCode);
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException("SimulationSettlementRequiredForHarvestImpact");
            return CreateHarvestDispositionImpactDecisionRequest(
                request,
                policy,
                settlement,
                out _);
        }

        private SimulationDecisionPreviewRequest CreateHarvestDispositionImpactDecisionRequest(
            SimulationHarvestDispositionImpactPreviewRequest request,
            HarvestDispositionImpactPolicy policy,
            SimulationSettlementEconomySnapshot settlement,
            out SimulationReserveStorageCandidateSnapshot? storageCandidate)
        {
            var sourceIds = request.SourceStableIds
                .Select(value => value.Trim())
                .Concat(new[]
                {
                    request.HarvestLotStableId.Trim(),
                    request.DispositionDecisionStableId.Trim(),
                    BuildRevisionSourceStableId(
                        request.HarvestLotStableId,
                        request.HarvestLotRevision),
                    BuildRevisionSourceStableId(
                        request.DispositionDecisionStableId,
                        request.DispositionDecisionRevision),
                    HarvestImpactPolicySource,
                })
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var blocks = new List<string>();
            if (settlement.LaborAvailable < policy.RequiredLabor)
                blocks.Add("InsufficientLaborCapacity");
            if (settlement.TreasuryAvailable < policy.SimulationCost)
                blocks.Add("InsufficientTreasuryBalance");

            storageCandidate = null;
            if (request.ChoiceCode == SimulationHarvestDispositionChoiceCodes.ReserveStorage)
            {
                storageCandidate = CreateStorageCandidate(request, settlement, blocks);
            }

            var expectedCosts = new List<SimulationValueProjection>
            {
                Value(
                    "LaborReservationCandidate",
                    "ledger:simulation:" + SettlementStableId + ":labor-available",
                    settlement.LaborAvailable,
                    -policy.RequiredLabor,
                    "LaborUnit",
                    sourceIds),
                Value(
                    "SimulationCostCandidate",
                    "ledger:simulation:" + SettlementStableId + ":treasury",
                    settlement.TreasuryBalance,
                    -policy.SimulationCost,
                    settlement.CurrencyCode,
                    sourceIds),
            };
            var expectedEffects = CreateHarvestDispositionExpectedEffects(
                request,
                policy,
                settlement,
                storageCandidate,
                sourceIds);
            var facility = ResolveFacilityStableId(request.ChoiceCode, settlement);
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = request.DispositionDecisionStableId.Trim(),
                DecisionTypeCode = "HarvestDisposition",
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    request.HarvestLotStableId.Trim(),
                    request.ProductStableId.Trim(),
                    SettlementStableId,
                },
                ExpectedCosts = expectedCosts.ToArray(),
                ExpectedEffects = expectedEffects,
                Uncertainties = policy.RiskCodes.ToArray(),
                BlockReasonCodes = blocks
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                SourceStableIds = sourceIds,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:harvest-impact:"
                        + request.DispositionDecisionStableId.Trim(),
                    TaskTypeCode = request.ChoiceCode.Trim() + "Work",
                    FacilityStableId = facility,
                    AssignedCapacity = policy.RequiredLabor,
                    AssignedCapacityUnitCode = "LaborUnit",
                    DurationTicks = policy.DurationTicks,
                    InputLotStableIds = new[] { request.HarvestLotStableId.Trim() },
                    OutputCandidateCodes = new[] { request.NextWorkflowCode.Trim() },
                    SourceStableIds = sourceIds,
                },
            };
        }

        private SimulationValueProjection[] CreateHarvestDispositionExpectedEffects(
            SimulationHarvestDispositionImpactPreviewRequest request,
            HarvestDispositionImpactPolicy policy,
            SimulationSettlementEconomySnapshot settlement,
            SimulationReserveStorageCandidateSnapshot? storageCandidate,
            string[] sourceIds)
        {
            if (request.ChoiceCode == SimulationHarvestDispositionChoiceCodes.DirectOnlineSale)
            {
                var marketBefore = settlement.MarketSupplyByProduct
                    .Where(value => value.ProductStableId == request.ProductStableId.Trim()
                        && value.UnitCode == settlement.StorageUnitCode)
                    .Sum(value => value.Quantity);
                return AddProjectedRevenue(
                    new[]
                    {
                        Value(
                            "MarketSupplyCandidate",
                            "ledger:simulation:" + SettlementStableId + ":market:"
                                + request.ProductStableId.Trim(),
                            marketBefore,
                            request.Quantity,
                            settlement.StorageUnitCode,
                            sourceIds),
                    },
                    policy,
                    settlement,
                    sourceIds);
            }

            if (request.ChoiceCode == SimulationHarvestDispositionChoiceCodes.ReserveStorage)
            {
                var candidate = storageCandidate
                    ?? throw new SimulationContractException("SimulationReserveStorageCandidateMissing");
                var productReserveBefore = settlement.ReserveStockLots
                    .Where(value => value.ProductStableId == request.ProductStableId.Trim()
                        && value.UnitCode == settlement.StorageUnitCode)
                    .Sum(value => value.AvailableQuantity);
                return new[]
                {
                    Value(
                        "StorageOccupiedCandidate",
                        "ledger:simulation:" + SettlementStableId + ":storage-occupied",
                        settlement.StorageOccupied,
                        candidate.ExpectedStoredQuantity,
                        settlement.StorageUnitCode,
                        sourceIds),
                    Value(
                        "ReserveStockCandidate",
                        "ledger:simulation:" + SettlementStableId + ":reserve:"
                            + request.ProductStableId.Trim(),
                        productReserveBefore,
                        candidate.ExpectedStoredQuantity,
                        settlement.StorageUnitCode,
                        sourceIds),
                    Value(
                        "FoodReserveEquivalentCandidate",
                        "ledger:simulation:" + SettlementStableId + ":food-reserve-equivalent",
                        settlement.FoodReserveEquivalent,
                        candidate.FoodEquivalentAddedCandidate,
                        settlement.FoodEquivalentUnitCode,
                        sourceIds),
                    Value(
                        "FoodSecurityDaysCandidate",
                        "ledger:simulation:" + SettlementStableId + ":food-security-days",
                        settlement.FoodSecurityDays,
                        candidate.FoodSecurityDaysCandidate - settlement.FoodSecurityDays,
                        "Day",
                        sourceIds),
                };
            }

            return AddProjectedRevenue(
                new[]
                {
                    Value(
                        "HarvestLotOutboundAllocationCandidate",
                        "ledger:simulation:" + request.HarvestLotStableId.Trim(),
                        request.Quantity,
                        -request.Quantity,
                        settlement.StorageUnitCode,
                        sourceIds),
                },
                policy,
                settlement,
                sourceIds);
        }

        private static SimulationValueProjection[] AddProjectedRevenue(
            SimulationValueProjection[] existing,
            HarvestDispositionImpactPolicy policy,
            SimulationSettlementEconomySnapshot settlement,
            string[] sourceIds)
        {
            if (!policy.ProjectedRevenue.HasValue) return existing;
            return existing.Concat(new[]
            {
                Value(
                    "ProjectedTreasuryIncomeCandidate",
                    "ledger:simulation:" + settlement.SettlementStableId + ":treasury",
                    settlement.TreasuryBalance,
                    policy.ProjectedRevenue.Value,
                    settlement.CurrencyCode,
                    sourceIds),
            }).ToArray();
        }

        private static SimulationReserveStorageCandidateSnapshot CreateStorageCandidate(
            SimulationHarvestDispositionImpactPreviewRequest request,
            SimulationSettlementEconomySnapshot settlement,
            ICollection<string> blocks)
        {
            if (settlement.StorageAvailable < request.Quantity)
                blocks.Add("InsufficientStorageCapacity");
            var productLots = settlement.ReserveStockLots
                .Where(value => value.ProductStableId == request.ProductStableId.Trim()
                    && value.Quantity > 0m
                    && value.FoodEquivalentQuantity > 0m)
                .ToArray();
            decimal foodEquivalentRatio = 0m;
            if (productLots.Length == 0)
            {
                blocks.Add("FoodEquivalentBasisMissing");
            }
            else
            {
                foodEquivalentRatio = productLots.Sum(value => value.FoodEquivalentQuantity)
                    / productLots.Sum(value => value.Quantity);
            }

            var shrinkage = request.Quantity * StorageShrinkageRate;
            var stored = request.Quantity - shrinkage;
            var foodEquivalentAdded = stored * foodEquivalentRatio;
            var reserveCandidate = settlement.FoodReserveEquivalent + foodEquivalentAdded;
            var foodSecurityCandidate = reserveCandidate / settlement.FoodDemandPerTick;
            return new SimulationReserveStorageCandidateSnapshot
            {
                StorageFacilityStableId = ResolveFacilityStableId(
                    SimulationHarvestDispositionChoiceCodes.ReserveStorage,
                    settlement),
                StorageCapacity = settlement.StorageCapacity,
                StorageOccupiedBefore = settlement.StorageOccupied,
                StorageAvailableBefore = settlement.StorageAvailable,
                RequestedQuantity = request.Quantity,
                ExpectedShrinkageQuantity = shrinkage,
                ExpectedStoredQuantity = stored,
                FoodEquivalentAddedCandidate = foodEquivalentAdded,
                FoodReserveEquivalentBefore = settlement.FoodReserveEquivalent,
                FoodReserveEquivalentCandidate = reserveCandidate,
                FoodSecurityDaysBefore = settlement.FoodSecurityDays,
                FoodSecurityDaysCandidate = foodSecurityCandidate,
                ShrinkageRate = StorageShrinkageRate,
                QuantityUnitCode = settlement.StorageUnitCode,
                FoodEquivalentUnitCode = settlement.FoodEquivalentUnitCode,
                FoodEquivalentRuleRevision = settlement.FoodEquivalentRuleRevision,
                CandidateStockLotStableId = "stock-lot:candidate:"
                    + request.HarvestLotStableId.Trim() + ":reserve-storage",
            };
        }

        private static string ResolveFacilityStableId(
            string choiceCode,
            SimulationSettlementEconomySnapshot settlement)
        {
            var requiredType = choiceCode == SimulationHarvestDispositionChoiceCodes.DirectOnlineSale
                ? SimulationSettlementFacilityTypeCodes.Market
                : SimulationSettlementFacilityTypeCodes.Storage;
            var facilityStableId = settlement.Facilities
                .Where(value => value.FacilityTypeCode == requiredType)
                .OrderBy(value => value.FacilityStableId, StringComparer.Ordinal)
                .Select(value => value.FacilityStableId)
                .FirstOrDefault();
            if (string.IsNullOrWhiteSpace(facilityStableId))
                throw new SimulationContractException(
                    requiredType == SimulationSettlementFacilityTypeCodes.Market
                        ? "SimulationSettlementMarketFacilityRequiredForHarvestImpact"
                        : "SimulationSettlementStorageFacilityRequiredForHarvestImpact");
            return facilityStableId;
        }

        private static string BuildRevisionSourceStableId(string stableId, long revision)
            => "source-revision:" + stableId.Trim() + ":r" + revision;

        private static SimulationValueProjection Value(
            string valueTypeCode,
            string targetLedgerStableId,
            decimal before,
            decimal delta,
            string unitCode,
            string[] sourceIds)
            => new SimulationValueProjection
            {
                ValueTypeCode = valueTypeCode,
                TargetLedgerStableId = targetLedgerStableId,
                BeforeValue = before,
                Delta = delta,
                AfterValue = before + delta,
                UnitCode = unitCode,
                SourceStableIds = sourceIds.ToArray(),
            };

        private static void ValidateHarvestDispositionImpactConfirmRequest(
            SimulationHarvestDispositionImpactConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.Impact == null)
                throw new SimulationContractException("SimulationHarvestImpactMissing");
            ValidateHarvestDispositionImpactRequest(request.Impact);
        }

        internal static void ValidateHarvestDispositionImpactConfirmRequestForReplay(
            SimulationHarvestDispositionImpactConfirmRequest request)
            => ValidateHarvestDispositionImpactConfirmRequest(request);

        private sealed class AppliedHarvestDispositionImpactCommand
        {
            public AppliedHarvestDispositionImpactCommand(
                string payloadKey,
                경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }

        private static void ValidateHarvestDispositionImpactRequest(
            SimulationHarvestDispositionImpactPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(
                request.DispositionDecisionStableId,
                "SimulationHarvestDispositionDecisionStableIdInvalid");
            if (request.DispositionDecisionRevision <= 0)
                throw new SimulationContractException("SimulationHarvestDispositionDecisionRevisionInvalid");
            RequireStableId(request.HarvestLotStableId, "SimulationHarvestLotStableIdInvalid");
            if (request.HarvestLotRevision <= 0)
                throw new SimulationContractException("SimulationHarvestLotRevisionInvalid");
            RequireStableId(request.ProductStableId, "SimulationHarvestProductStableIdInvalid");
            if (request.Quantity <= 0m)
                throw new SimulationContractException("SimulationHarvestQuantityInvalid");
            if (request.UnitCode != "kg" && request.UnitCode != "KGM")
                throw new SimulationContractException("SimulationHarvestQuantityUnitInvalid");
            RequireStableId(request.ActorStableId, "SimulationDecisionActorStableIdInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationHarvestSourceStableIdsInvalid");
            if (!request.SourceStableIds.Any(value => string.Equals(
                value.Trim(),
                request.HarvestLotStableId.Trim(),
                StringComparison.Ordinal)))
                throw new SimulationContractException("SimulationHarvestLotSourceMissing");

            var expectedWorkflow = ExpectedWorkflow(request.ChoiceCode);
            if (!string.Equals(request.NextWorkflowCode, expectedWorkflow, StringComparison.Ordinal))
                throw new SimulationContractException("SimulationHarvestDispositionWorkflowMismatch");
        }

        private static string ExpectedWorkflow(string choiceCode)
        {
            switch (choiceCode)
            {
                case SimulationHarvestDispositionChoiceCodes.CooperativeShipment:
                    return SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate;
                case SimulationHarvestDispositionChoiceCodes.DirectOnlineSale:
                    return SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate;
                case SimulationHarvestDispositionChoiceCodes.ExportAgent:
                    return SimulationHarvestDispositionWorkflowCodes.ExportReadinessCandidate;
                case SimulationHarvestDispositionChoiceCodes.ReserveStorage:
                    return SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate;
                default:
                    throw new SimulationContractException("SimulationHarvestDispositionChoiceUnknown");
            }
        }

        private sealed class HarvestDispositionImpactPolicy
        {
            private HarvestDispositionImpactPolicy(
                decimal requiredLabor,
                decimal simulationCost,
                decimal? projectedRevenue,
                int durationTicks,
                params string[] riskCodes)
            {
                RequiredLabor = requiredLabor;
                SimulationCost = simulationCost;
                ProjectedRevenue = projectedRevenue;
                DurationTicks = durationTicks;
                RiskCodes = riskCodes;
            }

            public decimal RequiredLabor { get; }
            public decimal SimulationCost { get; }
            public decimal? ProjectedRevenue { get; }
            public int DurationTicks { get; }
            public string[] RiskCodes { get; }

            public static HarvestDispositionImpactPolicy For(string choiceCode)
            {
                switch (choiceCode)
                {
                    case SimulationHarvestDispositionChoiceCodes.CooperativeShipment:
                        return new HarvestDispositionImpactPolicy(
                            8m, 30_000m, 240_000m, 2, "CooperativeSettlementDelay");
                    case SimulationHarvestDispositionChoiceCodes.DirectOnlineSale:
                        return new HarvestDispositionImpactPolicy(
                            18m, 60_000m, 360_000m, 3, "UnsoldInventory");
                    case SimulationHarvestDispositionChoiceCodes.ExportAgent:
                        return new HarvestDispositionImpactPolicy(
                            24m, 90_000m, 450_000m, 4,
                            "InspectionRejection", "ExportHandoffDelay");
                    case SimulationHarvestDispositionChoiceCodes.ReserveStorage:
                        return new HarvestDispositionImpactPolicy(
                            6m, 15_000m, null, 1, "StorageShrinkage");
                    default:
                        throw new SimulationContractException("SimulationHarvestDispositionChoiceUnknown");
                }
            }
        }
    }
}
