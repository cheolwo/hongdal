using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private SimulationSettlementInitialStateRequest? settlementInitialState;
        private SimulationSettlementInitialStateRequest? settlementCreationState;

        private string SettlementPayloadKey { get; set; } = "none";

        private void InitializeSettlement(SimulationSettlementInitialStateRequest? request)
        {
            settlementCreationState = CloneSettlementRequest(request);
            settlementInitialState = CloneSettlementRequest(request);
            InitializeSettlementEconomy();
            SettlementPayloadKey = BuildSettlementPayloadKey(settlementInitialState);
        }

        private SimulationSettlementEconomySnapshot? CreateSettlementSnapshot()
        {
            if (settlementInitialState == null) return null;

            var availableFoodEquivalent = settlementInitialState.ReserveStockLots.Sum(
                value => value.FoodEquivalentQuantity
                    - value.OutboundReservedFoodEquivalentQuantity);
            var foodDemand = settlementInitialState.PopulationFoodDemandPerTick
                + settlementInitialState.GarrisonFoodDemandPerTick;
            return new SimulationSettlementEconomySnapshot
            {
                SettlementStableId = SettlementStableId,
                WorldTick = CurrentTick,
                Revision = Revision,
                RuleRevision = RuleRevision,
                TreasuryBalance = settlementInitialState.TreasuryBalance,
                TreasuryReserved = settlementTreasuryReserved,
                TreasuryAvailable = settlementInitialState.TreasuryBalance - settlementTreasuryReserved,
                CurrencyCode = settlementInitialState.CurrencyCode,
                LaborCapacityTotal = settlementInitialState.LaborCapacityTotal,
                LaborReserved = settlementInitialState.LaborReserved,
                LaborAvailable = settlementInitialState.LaborCapacityTotal
                    - settlementInitialState.LaborReserved,
                StorageCapacity = settlementInitialState.StorageCapacity,
                StorageOccupied = settlementInitialState.StorageOccupied,
                StorageReserved = settlementStorageReserved,
                StorageAvailable = settlementInitialState.StorageCapacity
                    - settlementInitialState.StorageOccupied
                    - settlementStorageReserved,
                StorageUnitCode = settlementInitialState.StorageUnitCode,
                PopulationCount = settlementInitialState.PopulationCount,
                PopulationFoodDemandPerTick = settlementInitialState.PopulationFoodDemandPerTick,
                GarrisonCount = settlementInitialState.GarrisonCount,
                GarrisonFoodDemandPerTick = settlementInitialState.GarrisonFoodDemandPerTick,
                FoodReserveEquivalent = availableFoodEquivalent,
                FoodDemandPerTick = foodDemand,
                FoodSecurityDays = availableFoodEquivalent / foodDemand,
                FoodEquivalentUnitCode = settlementInitialState.FoodEquivalentUnitCode,
                FoodEquivalentRuleRevision = settlementInitialState.FoodEquivalentRuleRevision,
                Districts = settlementInitialState.Districts.Select(value =>
                    new SimulationSettlementDistrictSnapshot
                    {
                        DistrictStableId = value.DistrictStableId,
                        DistrictTypeCode = value.DistrictTypeCode,
                        SourceStableIds = Copy(value.SourceStableIds),
                    }).ToArray(),
                Facilities = settlementInitialState.Facilities.Select(value =>
                    new SimulationSettlementFacilitySnapshot
                    {
                        FacilityStableId = value.FacilityStableId,
                        FacilityTypeCode = value.FacilityTypeCode,
                        DistrictStableId = value.DistrictStableId,
                        SourceStableIds = Copy(value.SourceStableIds),
                    }).ToArray(),
                MarketSupplyByProduct = settlementInitialState.MarketSupplyByProduct.Select(value =>
                    new SimulationMarketSupplySnapshot
                    {
                        ProductStableId = value.ProductStableId,
                        Quantity = value.Quantity,
                        UnitCode = value.UnitCode,
                        SourceStableIds = Copy(value.SourceStableIds),
                    }).ToArray(),
                ResidentConsumptionByProduct = CreateResidentConsumptionSummaries(),
                ReserveStockLots = settlementInitialState.ReserveStockLots.Select(value =>
                    new SimulationReserveStockLotSnapshot
                    {
                        StockLotStableId = value.StockLotStableId,
                        ProductStableId = value.ProductStableId,
                        StorageFacilityStableId = value.StorageFacilityStableId,
                        Quantity = value.Quantity,
                        OutboundReservedQuantity = value.OutboundReservedQuantity,
                        AvailableQuantity = value.Quantity - value.OutboundReservedQuantity,
                        UnitCode = value.UnitCode,
                        FoodEquivalentQuantity = value.FoodEquivalentQuantity,
                        OutboundReservedFoodEquivalentQuantity =
                            value.OutboundReservedFoodEquivalentQuantity,
                        AvailableFoodEquivalentQuantity = value.FoodEquivalentQuantity
                            - value.OutboundReservedFoodEquivalentQuantity,
                        SourceStableIds = Copy(value.SourceStableIds),
                    }).ToArray(),
                HarvestLotAllocations = CreateHarvestLotAllocationSnapshots(),
                ActiveTaskStableIds = tasks.Values
                    .Where(value => value.StateCode != SimulationTaskStateCodes.Completed)
                    .Select(value => value.TaskStableId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                SourceStableIds = Copy(settlementInitialState.SourceStableIds),
            };
        }

        internal static void ValidateSettlementInitialState(
            SimulationSettlementInitialStateRequest? request,
            string settlementStableId)
        {
            if (request == null) return;
            RequireStableId(settlementStableId, "SimulationSettlementStableIdInvalid");
            if (request.TreasuryBalance < 0m)
                throw new SimulationContractException("SimulationSettlementTreasuryInvalid");
            RequireStableId(request.CurrencyCode, "SimulationSettlementCurrencyCodeInvalid");
            if (request.LaborCapacityTotal <= 0m
                || request.LaborReserved < 0m
                || request.LaborReserved > request.LaborCapacityTotal)
            {
                throw new SimulationContractException("SimulationSettlementLaborCapacityInvalid");
            }
            if (request.StorageCapacity <= 0m
                || request.StorageOccupied < 0m
                || request.StorageOccupied > request.StorageCapacity)
            {
                throw new SimulationContractException("SimulationSettlementStorageCapacityInvalid");
            }
            RequireStableId(request.StorageUnitCode, "SimulationSettlementStorageUnitCodeInvalid");
            if (request.PopulationCount <= 0 || request.PopulationFoodDemandPerTick <= 0m)
                throw new SimulationContractException("SimulationSettlementPopulationDemandInvalid");
            if (request.GarrisonCount < 0
                || request.GarrisonFoodDemandPerTick < 0m
                || (request.GarrisonCount == 0 && request.GarrisonFoodDemandPerTick != 0m)
                || (request.GarrisonCount > 0 && request.GarrisonFoodDemandPerTick <= 0m))
            {
                throw new SimulationContractException("SimulationSettlementGarrisonDemandInvalid");
            }
            RequireStableId(
                request.FoodEquivalentUnitCode,
                "SimulationFoodEquivalentUnitCodeInvalid");
            RequireText(
                request.FoodEquivalentRuleRevision,
                "SimulationFoodEquivalentRuleRevisionMissing");
            ValidateIds(request.SourceStableIds, true, "SimulationSettlementSourceStableIdsInvalid");

            if (request.Districts == null || request.Districts.Length == 0)
                throw new SimulationContractException("SimulationSettlementDistrictsInvalid");
            var districtIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var district in request.Districts)
            {
                if (district == null)
                    throw new SimulationContractException("SimulationSettlementDistrictsInvalid");
                RequireStableId(district.DistrictStableId, "SimulationSettlementDistrictStableIdInvalid");
                RequireStableId(district.DistrictTypeCode, "SimulationSettlementDistrictTypeCodeInvalid");
                ValidateIds(
                    district.SourceStableIds,
                    true,
                    "SimulationSettlementDistrictSourceStableIdsInvalid");
                if (!districtIds.Add(district.DistrictStableId.Trim()))
                    throw new SimulationContractException("SimulationSettlementDistrictStableIdDuplicate");
            }

            if (request.Facilities == null || request.Facilities.Length == 0)
                throw new SimulationContractException("SimulationSettlementFacilitiesInvalid");
            var facilityIds = new HashSet<string>(StringComparer.Ordinal);
            var facilityTypes = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var facility in request.Facilities)
            {
                if (facility == null)
                    throw new SimulationContractException("SimulationSettlementFacilitiesInvalid");
                RequireStableId(facility.FacilityStableId, "SimulationSettlementFacilityStableIdInvalid");
                RequireStableId(facility.FacilityTypeCode, "SimulationSettlementFacilityTypeCodeInvalid");
                RequireStableId(facility.DistrictStableId, "SimulationSettlementFacilityDistrictStableIdInvalid");
                ValidateIds(
                    facility.SourceStableIds,
                    true,
                    "SimulationSettlementFacilitySourceStableIdsInvalid");
                if (!districtIds.Contains(facility.DistrictStableId.Trim()))
                    throw new SimulationContractException("SimulationSettlementFacilityDistrictNotFound");
                var facilityId = facility.FacilityStableId.Trim();
                if (!facilityIds.Add(facilityId))
                    throw new SimulationContractException("SimulationSettlementFacilityStableIdDuplicate");
                facilityTypes.Add(facilityId, facility.FacilityTypeCode.Trim());
            }
            if (!facilityTypes.Values.Contains(SimulationSettlementFacilityTypeCodes.Storage)
                || !facilityTypes.Values.Contains(SimulationSettlementFacilityTypeCodes.Market))
            {
                throw new SimulationContractException("SimulationSettlementRequiredFacilityMissing");
            }

            if (request.MarketSupplyByProduct == null)
                throw new SimulationContractException("SimulationSettlementMarketSupplyInvalid");
            var marketProducts = new HashSet<string>(StringComparer.Ordinal);
            foreach (var supply in request.MarketSupplyByProduct)
            {
                if (supply == null)
                    throw new SimulationContractException("SimulationSettlementMarketSupplyInvalid");
                RequireStableId(supply.ProductStableId, "SimulationSettlementMarketProductStableIdInvalid");
                RequireStableId(supply.UnitCode, "SimulationSettlementMarketSupplyUnitCodeInvalid");
                ValidateIds(
                    supply.SourceStableIds,
                    true,
                    "SimulationSettlementMarketSupplySourceStableIdsInvalid");
                if (supply.Quantity < 0m)
                    throw new SimulationContractException("SimulationSettlementMarketSupplyQuantityInvalid");
                if (!marketProducts.Add(supply.ProductStableId.Trim()))
                    throw new SimulationContractException("SimulationSettlementMarketProductDuplicate");
            }

            if (request.ReserveStockLots == null)
                throw new SimulationContractException("SimulationSettlementReserveStockLotsInvalid");
            var stockLotIds = new HashSet<string>(StringComparer.Ordinal);
            decimal reserveStorageQuantity = 0m;
            foreach (var lot in request.ReserveStockLots)
            {
                if (lot == null)
                    throw new SimulationContractException("SimulationSettlementReserveStockLotsInvalid");
                RequireStableId(lot.StockLotStableId, "SimulationSettlementStockLotStableIdInvalid");
                RequireStableId(lot.ProductStableId, "SimulationSettlementStockProductStableIdInvalid");
                RequireStableId(
                    lot.StorageFacilityStableId,
                    "SimulationSettlementStockStorageFacilityStableIdInvalid");
                RequireStableId(lot.UnitCode, "SimulationSettlementStockUnitCodeInvalid");
                ValidateIds(
                    lot.SourceStableIds,
                    true,
                    "SimulationSettlementStockSourceStableIdsInvalid");
                if (!facilityTypes.TryGetValue(lot.StorageFacilityStableId.Trim(), out var facilityType)
                    || facilityType != SimulationSettlementFacilityTypeCodes.Storage)
                {
                    throw new SimulationContractException("SimulationSettlementStockStorageFacilityInvalid");
                }
                if (!string.Equals(lot.UnitCode.Trim(), request.StorageUnitCode.Trim(), StringComparison.Ordinal))
                    throw new SimulationContractException("SimulationSettlementStockStorageUnitMismatch");
                if (lot.Quantity <= 0m
                    || lot.OutboundReservedQuantity < 0m
                    || lot.OutboundReservedQuantity > lot.Quantity)
                {
                    throw new SimulationContractException("SimulationSettlementStockQuantityInvalid");
                }
                if (lot.FoodEquivalentQuantity < 0m
                    || lot.OutboundReservedFoodEquivalentQuantity < 0m
                    || lot.OutboundReservedFoodEquivalentQuantity > lot.FoodEquivalentQuantity)
                {
                    throw new SimulationContractException("SimulationSettlementFoodEquivalentQuantityInvalid");
                }
                if (!stockLotIds.Add(lot.StockLotStableId.Trim()))
                    throw new SimulationContractException("SimulationSettlementStockLotStableIdDuplicate");
                reserveStorageQuantity += lot.Quantity;
            }
            if (reserveStorageQuantity > request.StorageOccupied)
                throw new SimulationContractException("SimulationSettlementReserveExceedsStorageOccupied");
        }

        internal static string BuildSettlementPayloadKey(
            SimulationSettlementInitialStateRequest? request)
        {
            if (request == null) return "none";
            var normalized = CloneSettlementRequest(request)!;
            var key = new StringBuilder();
            AddKey(key, normalized.TreasuryBalance);
            AddKey(key, normalized.CurrencyCode);
            AddKey(key, normalized.LaborCapacityTotal);
            AddKey(key, normalized.LaborReserved);
            AddKey(key, normalized.StorageCapacity);
            AddKey(key, normalized.StorageOccupied);
            AddKey(key, normalized.StorageUnitCode);
            AddKey(key, normalized.PopulationCount);
            AddKey(key, normalized.PopulationFoodDemandPerTick);
            AddKey(key, normalized.GarrisonCount);
            AddKey(key, normalized.GarrisonFoodDemandPerTick);
            AddKey(key, normalized.FoodEquivalentUnitCode);
            AddKey(key, normalized.FoodEquivalentRuleRevision);
            AddKey(key, normalized.SourceStableIds);
            foreach (var district in normalized.Districts)
            {
                AddKey(key, district.DistrictStableId);
                AddKey(key, district.DistrictTypeCode);
                AddKey(key, district.SourceStableIds);
            }
            foreach (var facility in normalized.Facilities)
            {
                AddKey(key, facility.FacilityStableId);
                AddKey(key, facility.FacilityTypeCode);
                AddKey(key, facility.DistrictStableId);
                AddKey(key, facility.SourceStableIds);
            }
            foreach (var supply in normalized.MarketSupplyByProduct)
            {
                AddKey(key, supply.ProductStableId);
                AddKey(key, supply.Quantity);
                AddKey(key, supply.UnitCode);
                AddKey(key, supply.SourceStableIds);
            }
            foreach (var lot in normalized.ReserveStockLots)
            {
                AddKey(key, lot.StockLotStableId);
                AddKey(key, lot.ProductStableId);
                AddKey(key, lot.StorageFacilityStableId);
                AddKey(key, lot.Quantity);
                AddKey(key, lot.OutboundReservedQuantity);
                AddKey(key, lot.UnitCode);
                AddKey(key, lot.FoodEquivalentQuantity);
                AddKey(key, lot.OutboundReservedFoodEquivalentQuantity);
                AddKey(key, lot.SourceStableIds);
            }
            return key.ToString();
        }

        internal static SimulationSettlementInitialStateRequest? CloneSettlementRequest(
            SimulationSettlementInitialStateRequest? source)
        {
            if (source == null) return null;
            return new SimulationSettlementInitialStateRequest
            {
                TreasuryBalance = source.TreasuryBalance,
                CurrencyCode = source.CurrencyCode.Trim(),
                LaborCapacityTotal = source.LaborCapacityTotal,
                LaborReserved = source.LaborReserved,
                StorageCapacity = source.StorageCapacity,
                StorageOccupied = source.StorageOccupied,
                StorageUnitCode = source.StorageUnitCode.Trim(),
                PopulationCount = source.PopulationCount,
                PopulationFoodDemandPerTick = source.PopulationFoodDemandPerTick,
                GarrisonCount = source.GarrisonCount,
                GarrisonFoodDemandPerTick = source.GarrisonFoodDemandPerTick,
                FoodEquivalentUnitCode = source.FoodEquivalentUnitCode.Trim(),
                FoodEquivalentRuleRevision = source.FoodEquivalentRuleRevision.Trim(),
                SourceStableIds = NormalizeIds(source.SourceStableIds),
                Districts = source.Districts.Select(value => new SimulationSettlementDistrictRequest
                {
                    DistrictStableId = value.DistrictStableId.Trim(),
                    DistrictTypeCode = value.DistrictTypeCode.Trim(),
                    SourceStableIds = NormalizeIds(value.SourceStableIds),
                }).OrderBy(value => value.DistrictStableId, StringComparer.Ordinal).ToArray(),
                Facilities = source.Facilities.Select(value => new SimulationSettlementFacilityRequest
                {
                    FacilityStableId = value.FacilityStableId.Trim(),
                    FacilityTypeCode = value.FacilityTypeCode.Trim(),
                    DistrictStableId = value.DistrictStableId.Trim(),
                    SourceStableIds = NormalizeIds(value.SourceStableIds),
                }).OrderBy(value => value.FacilityStableId, StringComparer.Ordinal).ToArray(),
                MarketSupplyByProduct = source.MarketSupplyByProduct.Select(value =>
                    new SimulationMarketSupplyRequest
                    {
                        ProductStableId = value.ProductStableId.Trim(),
                        Quantity = value.Quantity,
                        UnitCode = value.UnitCode.Trim(),
                        SourceStableIds = NormalizeIds(value.SourceStableIds),
                    }).OrderBy(value => value.ProductStableId, StringComparer.Ordinal).ToArray(),
                ReserveStockLots = source.ReserveStockLots.Select(value =>
                    new SimulationReserveStockLotRequest
                    {
                        StockLotStableId = value.StockLotStableId.Trim(),
                        ProductStableId = value.ProductStableId.Trim(),
                        StorageFacilityStableId = value.StorageFacilityStableId.Trim(),
                        Quantity = value.Quantity,
                        OutboundReservedQuantity = value.OutboundReservedQuantity,
                        UnitCode = value.UnitCode.Trim(),
                        FoodEquivalentQuantity = value.FoodEquivalentQuantity,
                        OutboundReservedFoodEquivalentQuantity =
                            value.OutboundReservedFoodEquivalentQuantity,
                        SourceStableIds = NormalizeIds(value.SourceStableIds),
                    }).OrderBy(value => value.StockLotStableId, StringComparer.Ordinal).ToArray(),
            };
        }

        internal static SimulationSettlementEconomySnapshot? CloneSettlementSnapshot(
            SimulationSettlementEconomySnapshot? source)
        {
            if (source == null) return null;
            return new SimulationSettlementEconomySnapshot
            {
                SettlementStableId = source.SettlementStableId,
                WorldTick = source.WorldTick,
                Revision = source.Revision,
                RuleRevision = source.RuleRevision,
                TreasuryBalance = source.TreasuryBalance,
                TreasuryReserved = source.TreasuryReserved,
                TreasuryAvailable = source.TreasuryAvailable,
                CurrencyCode = source.CurrencyCode,
                LaborCapacityTotal = source.LaborCapacityTotal,
                LaborReserved = source.LaborReserved,
                LaborAvailable = source.LaborAvailable,
                StorageCapacity = source.StorageCapacity,
                StorageOccupied = source.StorageOccupied,
                StorageReserved = source.StorageReserved,
                StorageAvailable = source.StorageAvailable,
                StorageUnitCode = source.StorageUnitCode,
                PopulationCount = source.PopulationCount,
                PopulationFoodDemandPerTick = source.PopulationFoodDemandPerTick,
                GarrisonCount = source.GarrisonCount,
                GarrisonFoodDemandPerTick = source.GarrisonFoodDemandPerTick,
                FoodReserveEquivalent = source.FoodReserveEquivalent,
                FoodDemandPerTick = source.FoodDemandPerTick,
                FoodSecurityDays = source.FoodSecurityDays,
                FoodEquivalentUnitCode = source.FoodEquivalentUnitCode,
                FoodEquivalentRuleRevision = source.FoodEquivalentRuleRevision,
                FoodSecurityFormulaCode = source.FoodSecurityFormulaCode,
                Districts = source.Districts.Select(value => new SimulationSettlementDistrictSnapshot
                {
                    DistrictStableId = value.DistrictStableId,
                    DistrictTypeCode = value.DistrictTypeCode,
                    SourceStableIds = Copy(value.SourceStableIds),
                }).ToArray(),
                Facilities = source.Facilities.Select(value => new SimulationSettlementFacilitySnapshot
                {
                    FacilityStableId = value.FacilityStableId,
                    FacilityTypeCode = value.FacilityTypeCode,
                    DistrictStableId = value.DistrictStableId,
                    SourceStableIds = Copy(value.SourceStableIds),
                }).ToArray(),
                MarketSupplyByProduct = source.MarketSupplyByProduct.Select(value =>
                    new SimulationMarketSupplySnapshot
                    {
                        ProductStableId = value.ProductStableId,
                        Quantity = value.Quantity,
                        UnitCode = value.UnitCode,
                        SourceStableIds = Copy(value.SourceStableIds),
                    }).ToArray(),
                ResidentConsumptionByProduct = source.ResidentConsumptionByProduct.Select(value =>
                    new SimulationResidentConsumptionSummarySnapshot
                    {
                        ProductStableId = value.ProductStableId,
                        Quantity = value.Quantity,
                        UnitCode = value.UnitCode,
                        ConsumptionCount = value.ConsumptionCount,
                        SourceStableIds = Copy(value.SourceStableIds),
                    }).ToArray(),
                HarvestLotAllocations = source.HarvestLotAllocations
                    .Select(CloneHarvestLotAllocation)
                    .ToArray(),
                ReserveStockLots = source.ReserveStockLots.Select(value =>
                    new SimulationReserveStockLotSnapshot
                    {
                        StockLotStableId = value.StockLotStableId,
                        ProductStableId = value.ProductStableId,
                        StorageFacilityStableId = value.StorageFacilityStableId,
                        Quantity = value.Quantity,
                        OutboundReservedQuantity = value.OutboundReservedQuantity,
                        AvailableQuantity = value.AvailableQuantity,
                        UnitCode = value.UnitCode,
                        FoodEquivalentQuantity = value.FoodEquivalentQuantity,
                        OutboundReservedFoodEquivalentQuantity =
                            value.OutboundReservedFoodEquivalentQuantity,
                        AvailableFoodEquivalentQuantity = value.AvailableFoodEquivalentQuantity,
                        SourceStableIds = Copy(value.SourceStableIds),
                    }).ToArray(),
                ActiveTaskStableIds = Copy(source.ActiveTaskStableIds),
                SourceStableIds = Copy(source.SourceStableIds),
            };
        }

        private static void AddKey(StringBuilder target, object value)
        {
            var text = value is string[] values
                ? string.Join("\u001f", values)
                : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture));
            target.Append(':');
            target.Append(text);
            target.Append('|');
        }
    }
}
