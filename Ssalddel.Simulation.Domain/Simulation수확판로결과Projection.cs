using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private static readonly string[] 수확판로ChoiceCodes =
        {
            SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
            SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
            SimulationHarvestDispositionChoiceCodes.ReserveStorage,
            SimulationHarvestDispositionChoiceCodes.ExportAgent,
        };

        public Simulation수확판로결과Snapshot Get수확판로결과(string harvestLotStableId)
        {
            RequireStableId(harvestLotStableId, "SimulationHarvestRouteOutcomeLotStableIdInvalid");
            lock (gate)
            {
                var allocation = harvestLotAllocations.Values.SingleOrDefault(value =>
                    value.HarvestLotStableId == harvestLotStableId.Trim())
                    ?? throw new SimulationNotFoundException("SimulationHarvestRouteOutcomeNotFound");
                return Create수확판로결과(allocation);
            }
        }

        public Simulation수확판로결과Snapshot[] Get수확판로결과목록()
        {
            lock (gate)
            {
                return harvestLotAllocations.Values
                    .OrderBy(value => value.HarvestLotStableId, StringComparer.Ordinal)
                    .Select(Create수확판로결과)
                    .ToArray();
            }
        }

        private Simulation수확판로결과Snapshot Create수확판로결과(
            SimulationHarvestLotAllocationSnapshot allocation)
        {
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException("SimulationSettlementRequiredForHarvestRouteOutcome");
            var routes = 수확판로ChoiceCodes
                .Select(choice => Create수확판로선택지결과(allocation, settlement, choice))
                .ToArray();
            var relatedSources = routes.SelectMany(value => value.SourceStableIds)
                .Concat(allocation.SourceStableIds)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            return new Simulation수확판로결과Snapshot
            {
                SessionStableId = SessionStableId,
                WorldTick = CurrentTick,
                WorldRevision = Revision,
                SettlementStableId = SettlementStableId,
                AllocationStableId = allocation.AllocationStableId,
                HarvestLotStableId = allocation.HarvestLotStableId,
                HarvestLotRevision = allocation.HarvestLotRevision,
                ProductStableId = allocation.ProductStableId,
                Quantity = allocation.Quantity,
                UnitCode = allocation.UnitCode,
                SelectedChoiceCode = allocation.ChoiceCode,
                AllocationStateCode = allocation.StateCode,
                CurrentTreasuryBalance = settlement.TreasuryBalance,
                CurrencyCode = settlement.CurrencyCode,
                CurrentProductMarketSupplyQuantity = settlement.MarketSupplyByProduct
                    .Where(value => value.ProductStableId == allocation.ProductStableId
                        && value.UnitCode == allocation.UnitCode)
                    .Sum(value => value.Quantity),
                CurrentProductReserveQuantity = settlement.ReserveStockLots
                    .Where(value => value.ProductStableId == allocation.ProductStableId
                        && value.UnitCode == allocation.UnitCode)
                    .Sum(value => value.AvailableQuantity),
                Routes = routes,
                BoundaryCodes = new[]
                {
                    "SimulationOnly",
                    "ProjectionOnly",
                    "NoStateMutation",
                    "SelectedRouteOnlyHasActualOutcome",
                    "NoOperationalEffect",
                },
                SourceStableIds = relatedSources,
            };
        }

        private Simulation수확판로선택지결과Snapshot Create수확판로선택지결과(
            SimulationHarvestLotAllocationSnapshot allocation,
            SimulationSettlementEconomySnapshot settlement,
            string choiceCode)
        {
            if (allocation.ChoiceCode != choiceCode)
            {
                return new Simulation수확판로선택지결과Snapshot
                {
                    ChoiceCode = choiceCode,
                    SelectionStateCode = Simulation수확판로선택상태Codes.NotSelected,
                    IsSelected = false,
                    CurrentStageCode = Simulation수확판로단계Codes.NotSelected,
                    Quantity = allocation.Quantity,
                    RemainingQuantity = allocation.Quantity,
                    CurrencyCode = settlement.CurrencyCode,
                    RiskCodes = Create수확판로기본RiskCodes(choiceCode),
                    SourceStableIds = Copy(allocation.SourceStableIds),
                };
            }

            var initialTreasuryDelta = allocation.StateCode == SimulationHarvestLotAllocationStateCodes.Applied
                ? (allocation.ProjectedRevenue ?? 0m) - allocation.TreasuryCost
                : 0m;
            var result = new Simulation수확판로선택지결과Snapshot
            {
                ChoiceCode = choiceCode,
                SelectionStateCode = Simulation수확판로선택상태Codes.Selected,
                IsSelected = true,
                CurrentStageCode = allocation.StateCode == SimulationHarvestLotAllocationStateCodes.Applied
                    ? allocation.NextWorkflowCode
                    : Simulation수확판로단계Codes.DispositionTaskScheduled,
                SourceStateCode = allocation.StateCode,
                Quantity = allocation.Quantity,
                RemainingQuantity = allocation.Quantity,
                OutboundReservedQuantity = allocation.OutboundReservedQuantity,
                RecognizedTreasuryDelta = initialTreasuryDelta,
                CurrencyCode = settlement.CurrencyCode,
                RiskCodes = Create수확판로기본RiskCodes(choiceCode),
                RelatedStableIds = new[]
                {
                    allocation.AllocationStableId,
                    allocation.DecisionStableId,
                    allocation.TaskStableId,
                },
                SourceStableIds = Copy(allocation.SourceStableIds),
            };

            if (choiceCode == SimulationHarvestDispositionChoiceCodes.CooperativeShipment)
                Apply조합판로결과(result, allocation);
            else if (choiceCode == SimulationHarvestDispositionChoiceCodes.DirectOnlineSale)
                Apply직판판로결과(result, allocation);
            else if (choiceCode == SimulationHarvestDispositionChoiceCodes.ReserveStorage)
                Apply보관판로결과(result, allocation, settlement);
            else if (choiceCode == SimulationHarvestDispositionChoiceCodes.ExportAgent)
                Apply수출판로결과(result, allocation);
            return result;
        }

        private void Apply조합판로결과(
            Simulation수확판로선택지결과Snapshot result,
            SimulationHarvestLotAllocationSnapshot allocation)
        {
            var movement = logisticsMovements.Values.FirstOrDefault(value =>
                value.SourceAllocationStableId == allocation.AllocationStableId);
            if (movement == null)
            {
                result.CurrentStageCode = allocation.StateCode == SimulationHarvestLotAllocationStateCodes.Applied
                    ? Simulation수확판로단계Codes.CooperativeIntakeCandidate
                    : result.CurrentStageCode;
                return;
            }
            result.SourceStateCode = movement.StateCode;
            result.RelatedStableIds = MergeSources(result.RelatedStableIds,
                new[] { movement.CargoStableId, movement.TaskStableId, movement.RouteStableId });
            result.SourceStableIds = MergeSources(result.SourceStableIds, movement.SourceStableIds);
            if (movement.StateCode == SimulationLogisticsMovementStateCodes.ArrivedAtDestination)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.CooperativeCargoArrived;
                result.ResolvedQuantity = movement.Quantity;
                result.RemainingQuantity = Math.Max(0m, allocation.Quantity - movement.Quantity);
            }
            else if (movement.StateCode == SimulationLogisticsMovementStateCodes.InTransit)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.CooperativeCargoInTransit;
            }
            else
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.CooperativeCargoReserved;
            }
        }

        private static void Apply직판판로결과(
            Simulation수확판로선택지결과Snapshot result,
            SimulationHarvestLotAllocationSnapshot allocation)
        {
            if (allocation.StateCode != SimulationHarvestLotAllocationStateCodes.Applied) return;
            result.CurrentStageCode = Simulation수확판로단계Codes.DirectMarketSupplyAvailable;
            result.MarketSuppliedQuantity = allocation.Quantity;
            result.ResolvedQuantity = allocation.Quantity;
            result.RemainingQuantity = 0m;
        }

        private static void Apply보관판로결과(
            Simulation수확판로선택지결과Snapshot result,
            SimulationHarvestLotAllocationSnapshot allocation,
            SimulationSettlementEconomySnapshot settlement)
        {
            if (allocation.StateCode != SimulationHarvestLotAllocationStateCodes.Applied
                || string.IsNullOrWhiteSpace(allocation.ReserveStockLotStableId)) return;
            var stock = settlement.ReserveStockLots.FirstOrDefault(value =>
                value.StockLotStableId == allocation.ReserveStockLotStableId);
            if (stock == null) return;
            result.CurrentStageCode = Simulation수확판로단계Codes.ReserveStored;
            result.StoredQuantity = stock.Quantity;
            result.ResolvedQuantity = stock.Quantity;
            result.RemainingQuantity = Math.Max(0m, allocation.Quantity - stock.Quantity);
            result.RelatedStableIds = MergeSources(result.RelatedStableIds,
                new[] { stock.StockLotStableId, stock.StorageFacilityStableId });
            result.SourceStableIds = MergeSources(result.SourceStableIds, stock.SourceStableIds);
        }

        private void Apply수출판로결과(
            Simulation수확판로선택지결과Snapshot result,
            SimulationHarvestLotAllocationSnapshot allocation)
        {
            var relatedIds = new List<string>(result.RelatedStableIds);
            var sources = new List<string>(result.SourceStableIds);
            var preparation = 수출준비원장.Values
                .Where(value => value.SourceAllocationStableId == allocation.AllocationStableId)
                .OrderByDescending(value => value.AttemptNumber)
                .FirstOrDefault();
            if (preparation != null)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.ExportPreparation;
                result.SourceStateCode = preparation.StateCode;
                Add수확판로Lineage(relatedIds, sources,
                    preparation.PreparationStableId, preparation.SourceStableIds);
            }

            var cargoPreparation = 수출Cargo준비원장.Values.FirstOrDefault(value =>
                value.SourceAllocationStableId == allocation.AllocationStableId);
            if (cargoPreparation != null)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.ExportCargoPreparation;
                result.SourceStateCode = cargoPreparation.StateCode;
                Add수확판로Lineage(relatedIds, sources,
                    cargoPreparation.CargoPreparationStableId, cargoPreparation.SourceStableIds);
            }

            var handoff = 수출Cargo인계원장.Values.FirstOrDefault(value =>
                value.SourceAllocationStableId == allocation.AllocationStableId);
            if (handoff != null)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.ExportCargoHandoff;
                result.SourceStateCode = handoff.StateCode;
                Add수확판로Lineage(relatedIds, sources,
                    handoff.HandoffStableId, handoff.SourceStableIds);
            }

            var movement = logisticsMovements.Values.FirstOrDefault(value =>
                value.SourceAllocationStableId == allocation.AllocationStableId
                    && !string.IsNullOrWhiteSpace(value.SourceExportCargoHandoffStableId));
            if (movement != null)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.ExportPortMovement;
                result.SourceStateCode = movement.StateCode;
                Add수확판로Lineage(relatedIds, sources,
                    movement.CargoStableId, movement.SourceStableIds);
            }

            var receipt = 수출항만인수원장.Values.FirstOrDefault(value =>
                value.SourceAllocationStableId == allocation.AllocationStableId);
            if (receipt != null)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.ExportPortReceipt;
                result.SourceStateCode = receipt.StateCode;
                Add수확판로Lineage(relatedIds, sources,
                    receipt.ReceiptStableId, receipt.SourceStableIds);
            }

            var review = 수출준비성검토원장.Values
                .Where(value => value.SourceAllocationStableId == allocation.AllocationStableId)
                .OrderByDescending(value => value.AttemptNumber)
                .FirstOrDefault();
            if (review != null)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.ExportReadinessReview;
                result.SourceStateCode = review.StateCode;
                result.RiskCodes = MergeSources(result.RiskCodes, review.MissingRequirementCodes);
                result.RiskResultCode = review.OutcomeCode;
                Add수확판로Lineage(relatedIds, sources,
                    review.ReviewStableId, review.SourceStableIds);
            }

            var plan = 수출선적계획원장.Values.FirstOrDefault(value =>
                value.SourceAllocationStableId == allocation.AllocationStableId);
            if (plan != null)
            {
                result.CurrentStageCode = Simulation수확판로단계Codes.ExportShipmentPlan;
                result.SourceStateCode = plan.StateCode;
                result.RiskCodes = MergeSources(result.RiskCodes,
                    new[] { "ShipmentRisk:" + plan.RiskLevelCode });
                Add수확판로Lineage(relatedIds, sources, plan.PlanStableId, plan.SourceStableIds);
            }

            var execution = 수출선적실행원장.Values.FirstOrDefault(value =>
                value.SourceAllocationStableId == allocation.AllocationStableId);
            if (execution != null)
            {
                result.SourceStateCode = execution.StateCode;
                result.RiskResultCode = execution.OutcomeCode;
                result.RecognizedTreasuryDelta += execution.AppliedTreasuryDelta ?? 0m;
                result.ExportDeliveredQuantity = execution.DeliveredQuantity;
                result.ExportLostQuantity = execution.LostQuantity;
                result.ResolvedQuantity = execution.DeliveredQuantity + execution.LostQuantity;
                result.RemainingQuantity = Math.Max(0m, allocation.Quantity - result.ResolvedQuantity);
                result.CurrentStageCode = Create수출실행판로단계Code(execution.StateCode);
                result.RiskCodes = MergeSources(result.RiskCodes,
                    new[] { "ShipmentOutcome:" + execution.OutcomeCode });
                Add수확판로Lineage(relatedIds, sources,
                    execution.ExecutionStableId, execution.SourceStableIds);
            }

            result.OutboundReservedQuantity = allocation.OutboundReservedQuantity;
            result.RelatedStableIds = relatedIds.Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            result.SourceStableIds = sources.Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
        }

        private static string Create수출실행판로단계Code(string executionStateCode)
        {
            if (executionStateCode == Simulation수출선적실행상태Codes.DeliveredInSimulation)
                return Simulation수확판로단계Codes.ExportDelivered;
            if (executionStateCode
                == Simulation수출선적실행상태Codes.DisruptedWithLossInSimulation)
                return Simulation수확판로단계Codes.ExportDisruptedWithLoss;
            if (executionStateCode == Simulation수출선적실행상태Codes.InTransit)
                return Simulation수확판로단계Codes.ExportShipmentInTransit;
            return Simulation수확판로단계Codes.ExportShipmentScheduled;
        }

        private static string[] Create수확판로기본RiskCodes(string choiceCode)
        {
            if (choiceCode == SimulationHarvestDispositionChoiceCodes.CooperativeShipment)
                return new[] { "CooperativeSettlementDelay" };
            if (choiceCode == SimulationHarvestDispositionChoiceCodes.DirectOnlineSale)
                return new[] { "UnsoldInventory" };
            if (choiceCode == SimulationHarvestDispositionChoiceCodes.ExportAgent)
                return new[] { "InspectionRejection", "ExportHandoffDelay" };
            if (choiceCode == SimulationHarvestDispositionChoiceCodes.ReserveStorage)
                return new[] { "StorageShrinkage" };
            return Array.Empty<string>();
        }

        private static void Add수확판로Lineage(
            ICollection<string> relatedIds,
            ICollection<string> sources,
            string stableId,
            IEnumerable<string> sourceIds)
        {
            relatedIds.Add(stableId);
            foreach (var sourceId in sourceIds) sources.Add(sourceId);
        }
    }
}
