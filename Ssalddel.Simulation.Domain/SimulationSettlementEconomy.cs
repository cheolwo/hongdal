using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private decimal settlementTreasuryReserved;
        private decimal settlementStorageReserved;
        private readonly Dictionary<string, SimulationHarvestLotAllocationSnapshot> harvestLotAllocations =
            new Dictionary<string, SimulationHarvestLotAllocationSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> harvestLotAllocationIds =
            new Dictionary<string, string>(StringComparer.Ordinal);

        private void InitializeSettlementEconomy()
        {
            settlementTreasuryReserved = 0m;
            settlementStorageReserved = 0m;
            harvestLotAllocations.Clear();
            harvestLotAllocationIds.Clear();
        }

        private SimulationHarvestLotAllocationSnapshot CreateHarvestLotAllocation(
            SimulationHarvestDispositionImpactPreviewRequest request,
            SimulationHarvestDispositionImpactPreviewSnapshot preview)
        {
            var lotId = request.HarvestLotStableId.Trim();
            if (harvestLotAllocationIds.ContainsKey(lotId))
                throw new SimulationConflictException("SimulationHarvestLotAlreadyAllocated");
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException("SimulationSettlementRequiredForHarvestImpact");
            if (settlement.LaborAvailable < preview.RequiredLabor)
                throw new SimulationConflictException("SimulationSettlementLaborCapacityExceeded");
            if (settlement.TreasuryAvailable < preview.SimulationCost)
                throw new SimulationConflictException("SimulationSettlementTreasuryCapacityExceeded");
            if (preview.StorageCandidate != null
                && settlement.StorageAvailable < preview.StorageCandidate.ExpectedStoredQuantity)
            {
                throw new SimulationConflictException("SimulationSettlementStorageCapacityExceeded");
            }

            return new SimulationHarvestLotAllocationSnapshot
            {
                AllocationStableId = "allocation:harvest-lot:" + lotId,
                HarvestLotStableId = lotId,
                HarvestLotRevision = request.HarvestLotRevision,
                ProductStableId = request.ProductStableId.Trim(),
                Quantity = request.Quantity,
                UnitCode = preview.CanonicalQuantityUnitCode,
                ChoiceCode = request.ChoiceCode.Trim(),
                NextWorkflowCode = request.NextWorkflowCode.Trim(),
                DecisionStableId = preview.CommonDecisionPreview.Decision.DecisionStableId,
                TaskStableId = preview.CommonDecisionPreview.TaskPlan.TaskStableId,
                FacilityStableId = preview.CommonDecisionPreview.TaskPlan.FacilityStableId,
                RequiredLabor = preview.RequiredLabor,
                TreasuryCost = preview.SimulationCost,
                ProjectedRevenue = preview.ProjectedRevenue,
                StateCode = SimulationHarvestLotAllocationStateCodes.Reserved,
                ReservedTick = CurrentTick,
                ReserveStockLotStableId = preview.StorageCandidate?.CandidateStockLotStableId,
                StoredQuantity = preview.StorageCandidate?.ExpectedStoredQuantity ?? 0m,
                FoodEquivalentQuantity = preview.StorageCandidate?.FoodEquivalentAddedCandidate ?? 0m,
                OutboundReservedQuantity = 0m,
                AvailableQuantity = request.Quantity,
                SourceStableIds = Copy(preview.CommonDecisionPreview.Decision.SourceStableIds),
            };
        }

        private void ReserveHarvestLotAllocation(SimulationHarvestLotAllocationSnapshot allocation)
        {
            if (settlementInitialState == null)
                throw new SimulationContractException("SimulationSettlementRequiredForHarvestImpact");
            settlementInitialState.LaborReserved += allocation.RequiredLabor;
            settlementTreasuryReserved += allocation.TreasuryCost;
            settlementStorageReserved += allocation.StoredQuantity;
            harvestLotAllocations.Add(allocation.AllocationStableId, allocation);
            harvestLotAllocationIds.Add(allocation.HarvestLotStableId, allocation.AllocationStableId);
        }

        private void ApplySettlementEconomyForTask(SimulationTaskSnapshot task, int appliedTick)
        {
            var allocation = harvestLotAllocations.Values.FirstOrDefault(
                value => value.TaskStableId == task.TaskStableId
                    && value.StateCode == SimulationHarvestLotAllocationStateCodes.Reserved);
            if (allocation == null) return;
            if (settlementInitialState == null)
                throw new SimulationContractException("SimulationSettlementRequiredForHarvestImpact");

            settlementInitialState.LaborReserved -= allocation.RequiredLabor;
            settlementTreasuryReserved -= allocation.TreasuryCost;
            settlementStorageReserved -= allocation.StoredQuantity;
            settlementInitialState.TreasuryBalance -= allocation.TreasuryCost;
            if (allocation.ProjectedRevenue.HasValue)
                settlementInitialState.TreasuryBalance += allocation.ProjectedRevenue.Value;

            if (allocation.ChoiceCode == SimulationHarvestDispositionChoiceCodes.DirectOnlineSale)
                ApplyMarketSupply(allocation);
            else if (allocation.ChoiceCode == SimulationHarvestDispositionChoiceCodes.ReserveStorage)
                ApplyReserveStorage(allocation);

            allocation.StateCode = SimulationHarvestLotAllocationStateCodes.Applied;
            allocation.AppliedTick = appliedTick;
        }

        private void ApplyMarketSupply(SimulationHarvestLotAllocationSnapshot allocation)
        {
            var existing = settlementInitialState!.MarketSupplyByProduct.FirstOrDefault(
                value => value.ProductStableId == allocation.ProductStableId
                    && value.UnitCode == allocation.UnitCode);
            if (existing != null)
            {
                existing.Quantity += allocation.Quantity;
                existing.SourceStableIds = MergeSources(existing.SourceStableIds, allocation.SourceStableIds);
                return;
            }
            settlementInitialState.MarketSupplyByProduct = settlementInitialState.MarketSupplyByProduct
                .Concat(new[]
                {
                    new SimulationMarketSupplyRequest
                    {
                        ProductStableId = allocation.ProductStableId,
                        Quantity = allocation.Quantity,
                        UnitCode = allocation.UnitCode,
                        SourceStableIds = Copy(allocation.SourceStableIds),
                    },
                })
                .OrderBy(value => value.ProductStableId, StringComparer.Ordinal)
                .ToArray();
        }

        private void ApplyReserveStorage(SimulationHarvestLotAllocationSnapshot allocation)
        {
            if (string.IsNullOrWhiteSpace(allocation.ReserveStockLotStableId))
                throw new SimulationContractException("SimulationReserveStockLotStableIdMissing");
            if (settlementInitialState!.ReserveStockLots.Any(
                value => value.StockLotStableId == allocation.ReserveStockLotStableId))
            {
                throw new SimulationConflictException("SimulationReserveStockLotStableIdConflict");
            }
            settlementInitialState.StorageOccupied += allocation.StoredQuantity;
            settlementInitialState.ReserveStockLots = settlementInitialState.ReserveStockLots
                .Concat(new[]
                {
                    new SimulationReserveStockLotRequest
                    {
                        StockLotStableId = allocation.ReserveStockLotStableId,
                        ProductStableId = allocation.ProductStableId,
                        StorageFacilityStableId = allocation.FacilityStableId,
                        Quantity = allocation.StoredQuantity,
                        UnitCode = allocation.UnitCode,
                        FoodEquivalentQuantity = allocation.FoodEquivalentQuantity,
                        SourceStableIds = Copy(allocation.SourceStableIds),
                    },
                })
                .OrderBy(value => value.StockLotStableId, StringComparer.Ordinal)
                .ToArray();
        }

        private SimulationHarvestLotAllocationSnapshot[] CreateHarvestLotAllocationSnapshots()
            => harvestLotAllocations.Values
                .OrderBy(value => value.AllocationStableId, StringComparer.Ordinal)
                .Select(CloneHarvestLotAllocation)
                .ToArray();

        internal static SimulationHarvestLotAllocationSnapshot CloneHarvestLotAllocation(
            SimulationHarvestLotAllocationSnapshot source)
            => new SimulationHarvestLotAllocationSnapshot
            {
                AllocationStableId = source.AllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                HarvestLotRevision = source.HarvestLotRevision,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                ChoiceCode = source.ChoiceCode,
                NextWorkflowCode = source.NextWorkflowCode,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                FacilityStableId = source.FacilityStableId,
                RequiredLabor = source.RequiredLabor,
                TreasuryCost = source.TreasuryCost,
                ProjectedRevenue = source.ProjectedRevenue,
                StateCode = source.StateCode,
                ReservedTick = source.ReservedTick,
                AppliedTick = source.AppliedTick,
                ReserveStockLotStableId = source.ReserveStockLotStableId,
                StoredQuantity = source.StoredQuantity,
                FoodEquivalentQuantity = source.FoodEquivalentQuantity,
                OutboundReservedQuantity = source.OutboundReservedQuantity,
                AvailableQuantity = source.AvailableQuantity,
                SourceStableIds = Copy(source.SourceStableIds),
            };
    }
}
