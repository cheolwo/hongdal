using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class 도심마트공급EngineWorkPolicyCodes
    {
        public const string CombinedInspectionPutAwayShelfAvailability =
            "CombinedInspectionPutAwayShelfAvailability";
    }

    public sealed class 도심마트공급경영SimulationEngineRule
    {
        public string RuleRevision { get; set; } = string.Empty;
        public int DurationTicks { get; set; } = 28;
        public decimal InitialInventoryQuantity { get; set; }
        public decimal InitialCash { get; set; }
        public decimal StorageCapacity { get; set; }
        public decimal ReceivingWorkCapacityPerTick { get; set; }
        public string WorkCapacityPolicyCode { get; set; } =
            도심마트공급EngineWorkPolicyCodes.CombinedInspectionPutAwayShelfAvailability;
        public string LimitationText { get; set; } = string.Empty;
        public int InitialInventoryShelfLifeTicks { get; set; }
        public 도심마트품질판매기한SimulationRule[] QualityShelfLives { get; set; } =
            Array.Empty<도심마트품질판매기한SimulationRule>();
        public 도심마트선택공급계획SimulationData[] SelectedSupplyPlans { get; set; } =
            Array.Empty<도심마트선택공급계획SimulationData>();
    }

    public sealed class 도심마트품질판매기한SimulationRule
    {
        public string QualityStandardCode { get; set; } = string.Empty;
        public int ShelfLifeTicks { get; set; }
    }

    public sealed class 도심마트선택공급계획SimulationData
    {
        public string ContractDraftStableId { get; set; } = string.Empty;
        public decimal WeeklyQuantity { get; set; }
        public decimal TransportCostPerDelivery { get; set; }
    }

    public sealed class 도심마트공급경영TickWorldState
    {
        public int Tick { get; set; }
        public decimal OpeningInventory { get; set; }
        public decimal DemandCreated { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public decimal FulfilledQuantity { get; set; }
        public decimal UnfulfilledClosedQuantity { get; set; }
        public decimal WasteQuantity { get; set; }
        public decimal ClosingInventory { get; set; }
        public decimal ReceivingWorkload { get; set; }
        public decimal PaymentDue { get; set; }
        public decimal PaymentPaid { get; set; }
        public decimal ClosingCash { get; set; }
    }

    public sealed class 도심마트납품SimulationResult
    {
        public string DeliveryStableId { get; set; } = string.Empty;
        public string ContractDraftStableId { get; set; } = string.Empty;
        public string SupplierStableId { get; set; } = string.Empty;
        public int PlannedTick { get; set; }
        public int ArrivalTick { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public int PaymentDueTick { get; set; }
        public decimal PaymentAmount { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트주문SimulationResult
    {
        public string OrderStableId { get; set; } = string.Empty;
        public string DemandSourceTypeCode { get; set; } = string.Empty;
        public int CreatedTick { get; set; }
        public int FulfillmentDueTick { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal FulfilledQuantity { get; set; }
        public decimal UnfulfilledQuantity { get; set; }
        public string StateCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트공급경영SimulationWorldState
    {
        public string StableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public string SimulationRevision { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal HardDemandQuantity { get; set; }
        public decimal FulfilledQuantity { get; set; }
        public decimal UnfulfilledQuantity { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal RejectedDeliveryQuantity { get; set; }
        public decimal WasteQuantity { get; set; }
        public decimal EndingInventoryQuantity { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal EndingCash { get; set; }
        public decimal OutstandingPaymentAmount { get; set; }
        public decimal ReceivingWorkload { get; set; }
        public SimulationDataLineage[] SourceLineage { get; set; } =
            Array.Empty<SimulationDataLineage>();
        public 도심마트공급경영TickWorldState[] Ticks { get; set; } =
            Array.Empty<도심마트공급경영TickWorldState>();
        public 도심마트납품SimulationResult[] Deliveries { get; set; } =
            Array.Empty<도심마트납품SimulationResult>();
        public 도심마트주문SimulationResult[] Orders { get; set; } =
            Array.Empty<도심마트주문SimulationResult>();
        public 도심마트공급처SimulationResult[] SupplierResults { get; set; } =
            Array.Empty<도심마트공급처SimulationResult>();
    }

    public sealed class 도심마트공급처SimulationResult
    {
        public string SupplierStableId { get; set; } = string.Empty;
        public decimal PlannedQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal AcceptedSupplyShareRate { get; set; }
        public decimal PurchaseCost { get; set; }
    }

    public sealed class 도심마트공급경영SimulationEngine
    {
        public 도심마트공급경영SimulationWorldState Run(
            도심마트공급경영SimulationDataSnapshot supply,
            도심마트주문SimulationDataSnapshot baseOrders,
            도심마트수요CompositionWorldState demand,
            도심마트공급경영SimulationEngineRule rule)
        {
            if (supply == null) throw new ArgumentNullException(nameof(supply));
            if (baseOrders == null) throw new ArgumentNullException(nameof(baseOrders));
            if (demand == null) throw new ArgumentNullException(nameof(demand));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            도심마트공급경영SimulationDataValidator.Validate(supply);
            도심마트공급경영SimulationDataValidator.Validate(baseOrders);
            ValidateInputs(supply, baseOrders, demand, rule);

            var orders = BuildOrders(baseOrders, demand);
            var deliveries = BuildDeliveries(supply, rule);
            var lots = new List<InventoryLot>();
            if (rule.InitialInventoryQuantity > 0m)
                lots.Add(new InventoryLot("inventory-lot:initial", rule.InitialInventoryQuantity,
                    rule.InitialInventoryShelfLifeTicks - 1));
            var payments = new List<Payment>();
            var ticks = new List<도심마트공급경영TickWorldState>();
            var cash = rule.InitialCash;

            for (var tick = 0; tick < rule.DurationTicks; tick++)
            {
                var tickState = new 도심마트공급경영TickWorldState
                {
                    Tick = tick,
                    OpeningInventory = Inventory(lots),
                    DemandCreated = orders.Where(order => order.CreatedTick == tick)
                        .Sum(order => order.RequestedQuantity),
                };
                tickState.FulfilledQuantity += Allocate(orders, lots, tick);

                var workRemaining = rule.ReceivingWorkCapacityPerTick;
                foreach (var delivery in deliveries.Where(value => value.ArrivalTick == tick)
                    .OrderBy(value => value.DeliveryStableId, StringComparer.Ordinal))
                {
                    tickState.DeliveredQuantity += delivery.PlannedQuantity;
                    var storageRemaining = Math.Max(0m, rule.StorageCapacity - Inventory(lots));
                    var accepted = Math.Min(delivery.PlannedQuantity,
                        Math.Min(workRemaining, storageRemaining));
                    delivery.AcceptedQuantity = accepted;
                    delivery.RejectedQuantity = delivery.PlannedQuantity - accepted;
                    tickState.AcceptedQuantity += accepted;
                    tickState.RejectedQuantity += delivery.RejectedQuantity;
                    tickState.ReceivingWorkload += accepted;
                    workRemaining -= accepted;
                    if (accepted <= 0m) continue;
                    var draft = supply.ContractDrafts.Single(value => value.ContractDraftStableId
                        == delivery.ContractDraftStableId);
                    lots.Add(new InventoryLot(
                        "inventory-lot:" + delivery.DeliveryStableId,
                        accepted,
                        tick + ShelfLife(rule, draft.QualityStandardCode) - 1));
                    var plan = rule.SelectedSupplyPlans.Single(value =>
                        value.ContractDraftStableId == draft.ContractDraftStableId);
                    delivery.PaymentAmount = accepted * draft.UnitPrice
                        + plan.TransportCostPerDelivery;
                    payments.Add(new Payment(delivery.PaymentDueTick, delivery.PaymentAmount));
                }

                tickState.FulfilledQuantity += Allocate(orders, lots, tick);
                foreach (var order in orders.Where(order => order.FulfillmentDueTick == tick
                    && order.FulfilledQuantity < order.RequestedQuantity))
                {
                    order.Closed = true;
                    tickState.UnfulfilledClosedQuantity +=
                        order.RequestedQuantity - order.FulfilledQuantity;
                }

                foreach (var lot in lots.Where(value => value.ExpiresAtTick <= tick && value.Quantity > 0m))
                {
                    tickState.WasteQuantity += lot.Quantity;
                    lot.Quantity = 0m;
                }

                foreach (var payment in payments.Where(value => value.DueTick == tick))
                {
                    tickState.PaymentDue += payment.Amount;
                    if (cash < payment.Amount) continue;
                    cash -= payment.Amount;
                    payment.Paid = true;
                    tickState.PaymentPaid += payment.Amount;
                }
                tickState.ClosingInventory = Inventory(lots);
                tickState.ClosingCash = cash;
                ticks.Add(tickState);
            }

            var orderResults = orders.OrderBy(value => value.CreatedTick)
                .ThenBy(value => value.OrderStableId, StringComparer.Ordinal)
                .Select(value => new 도심마트주문SimulationResult
                {
                    OrderStableId = value.OrderStableId,
                    DemandSourceTypeCode = value.SourceTypeCode,
                    CreatedTick = value.CreatedTick,
                    FulfillmentDueTick = value.FulfillmentDueTick,
                    RequestedQuantity = value.RequestedQuantity,
                    FulfilledQuantity = value.FulfilledQuantity,
                    UnfulfilledQuantity = value.RequestedQuantity - value.FulfilledQuantity,
                    StateCode = value.FulfilledQuantity == value.RequestedQuantity
                        ? SimulationOrderStateCodes.Fulfilled
                        : value.FulfilledQuantity > 0m
                            ? SimulationOrderStateCodes.PartiallyFulfilled
                            : SimulationOrderStateCodes.Unfulfilled,
                }).ToArray();
            var fulfilled = orderResults.Sum(value => value.FulfilledQuantity);
            var unfulfilled = orderResults.Sum(value => value.UnfulfilledQuantity);
            if (fulfilled + unfulfilled != demand.HardDemand)
                throw new SimulationContractException("SupplyEngineDemandConservationInvalid");

            var acceptedTotal = deliveries.Sum(value => value.AcceptedQuantity);
            var supplierResults = deliveries.GroupBy(value => value.SupplierStableId)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new 도심마트공급처SimulationResult
                {
                    SupplierStableId = group.Key,
                    PlannedQuantity = group.Sum(value => value.PlannedQuantity),
                    AcceptedQuantity = group.Sum(value => value.AcceptedQuantity),
                    AcceptedSupplyShareRate = acceptedTotal == 0m
                        ? 0m : group.Sum(value => value.AcceptedQuantity) / acceptedTotal,
                    PurchaseCost = group.Sum(value => value.PaymentAmount),
                }).ToArray();

            return new 도심마트공급경영SimulationWorldState
            {
                StableId = "supply-engine-result:" + demand.ScenarioStableId,
                SessionStableId = demand.SessionStableId,
                ScenarioStableId = demand.ScenarioStableId,
                SimulationRevision = "supply-engine:" + supply.DataRevision + ":"
                    + baseOrders.DataRevision + ":" + demand.InterpretationRevision + ":"
                    + rule.RuleRevision,
                ProductStableId = demand.ProductStableId,
                QuantityUnitCode = demand.QuantityUnitCode,
                CurrencyCode = supply.ContractDrafts.Select(value => value.CurrencyCode)
                    .Distinct(StringComparer.Ordinal).Single(),
                HardDemandQuantity = demand.HardDemand,
                FulfilledQuantity = fulfilled,
                UnfulfilledQuantity = unfulfilled,
                DeliveredQuantity = deliveries.Sum(value => value.AcceptedQuantity),
                RejectedDeliveryQuantity = deliveries.Sum(value => value.RejectedQuantity),
                WasteQuantity = ticks.Sum(value => value.WasteQuantity),
                EndingInventoryQuantity = Inventory(lots),
                PurchaseCost = deliveries.Sum(value => value.PaymentAmount),
                EndingCash = cash,
                OutstandingPaymentAmount = payments.Where(value => !value.Paid).Sum(value => value.Amount),
                ReceivingWorkload = ticks.Sum(value => value.ReceivingWorkload),
                SourceLineage = new[]
                {
                    Lineage(supply.SnapshotStableId, supply.DataRevision, rule.RuleRevision),
                    Lineage(baseOrders.SnapshotStableId, baseOrders.DataRevision, rule.RuleRevision),
                    Lineage(demand.StableId, demand.InterpretationRevision, rule.RuleRevision),
                },
                Ticks = ticks.ToArray(),
                Deliveries = deliveries.ToArray(),
                Orders = orderResults,
                SupplierResults = supplierResults,
            };
        }

        private static List<EngineOrder> BuildOrders(
            도심마트주문SimulationDataSnapshot baseOrders,
            도심마트수요CompositionWorldState demand)
        {
            var result = baseOrders.Orders.Select(value => new EngineOrder(
                value.OrderStableId, value.DemandSourceTypeCode, value.CreatedTick,
                value.FulfillmentDueTick, value.RequestedQuantity)).ToList();
            foreach (var component in demand.Components.Where(value => value.IsHardDemand
                && value.SourceTypeCode == SimulationDemandSourceTypeCodes.GroupConfirmedDemand))
            {
                result.Add(new EngineOrder(
                    "simulation-order:group-confirmed:" + component.SourceStableId,
                    component.SourceTypeCode,
                    component.StartsAtTick,
                    component.EndsAtTick,
                    component.Quantity));
            }
            if (result.Sum(value => value.RequestedQuantity) != demand.HardDemand)
                throw new SimulationContractException("SupplyEngineOrderDemandMismatch");
            return result;
        }

        private static List<도심마트납품SimulationResult> BuildDeliveries(
            도심마트공급경영SimulationDataSnapshot supply,
            도심마트공급경영SimulationEngineRule rule)
        {
            var result = new List<도심마트납품SimulationResult>();
            foreach (var plan in rule.SelectedSupplyPlans.OrderBy(
                value => value.ContractDraftStableId, StringComparer.Ordinal))
            {
                var draft = supply.ContractDrafts.Single(value =>
                    value.ContractDraftStableId == plan.ContractDraftStableId);
                for (var week = 0; week < 4; week++)
                {
                    var quantities = Split(plan.WeeklyQuantity, draft.DeliveriesPerWeek);
                    for (var index = 0; index < quantities.Length; index++)
                    {
                        var plannedTick = week * 7 + (index * 7 / draft.DeliveriesPerWeek);
                        var arrivalTick = plannedTick + draft.LeadTimeTicks;
                        if (arrivalTick >= rule.DurationTicks) continue;
                        result.Add(new 도심마트납품SimulationResult
                        {
                            DeliveryStableId = "delivery:" + draft.ContractDraftStableId + ":"
                                + week + ":" + index,
                            ContractDraftStableId = draft.ContractDraftStableId,
                            SupplierStableId = draft.SupplierStableId,
                            PlannedTick = plannedTick,
                            ArrivalTick = arrivalTick,
                            PlannedQuantity = quantities[index],
                            PaymentDueTick = arrivalTick + draft.PaymentDueTicks,
                            QuantityUnitCode = draft.QuantityUnitCode,
                            CurrencyCode = draft.CurrencyCode,
                        });
                    }
                }
            }
            return result;
        }

        private static decimal[] Split(decimal quantity, int count)
        {
            var unit = Math.Floor(quantity / count * 1000m) / 1000m;
            var values = Enumerable.Repeat(unit, count).ToArray();
            values[0] += quantity - values.Sum();
            return values;
        }

        private static decimal Allocate(List<EngineOrder> orders, List<InventoryLot> lots, int tick)
        {
            var fulfilled = 0m;
            foreach (var order in orders.Where(value => !value.Closed && value.CreatedTick <= tick)
                .OrderBy(value => value.FulfillmentDueTick)
                .ThenBy(value => value.OrderStableId, StringComparer.Ordinal))
            {
                var remaining = order.RequestedQuantity - order.FulfilledQuantity;
                foreach (var lot in lots.Where(value => value.Quantity > 0m)
                    .OrderBy(value => value.ExpiresAtTick)
                    .ThenBy(value => value.StableId, StringComparer.Ordinal))
                {
                    var quantity = Math.Min(remaining, lot.Quantity);
                    lot.Quantity -= quantity;
                    remaining -= quantity;
                    order.FulfilledQuantity += quantity;
                    fulfilled += quantity;
                    if (remaining == 0m) break;
                }
                if (order.FulfilledQuantity == order.RequestedQuantity) order.Closed = true;
            }
            return fulfilled;
        }

        private static void ValidateInputs(
            도심마트공급경영SimulationDataSnapshot supply,
            도심마트주문SimulationDataSnapshot orders,
            도심마트수요CompositionWorldState demand,
            도심마트공급경영SimulationEngineRule rule)
        {
            if (!new[] { supply.SessionStableId, orders.SessionStableId, demand.SessionStableId }
                .All(value => value == supply.SessionStableId))
                throw new SimulationContractException("SupplyEngineSessionMismatch");
            if (!new[] { supply.ScenarioStableId, orders.ScenarioStableId, demand.ScenarioStableId }
                .All(value => value == supply.ScenarioStableId))
                throw new SimulationContractException("SupplyEngineScenarioMismatch");
            if (string.IsNullOrWhiteSpace(rule.RuleRevision) || rule.DurationTicks != 28
                || rule.InitialInventoryQuantity < 0m || rule.InitialCash < 0m
                || rule.StorageCapacity <= 0m || rule.InitialInventoryQuantity > rule.StorageCapacity
                || rule.ReceivingWorkCapacityPerTick <= 0m || rule.InitialInventoryShelfLifeTicks <= 0)
                throw new SimulationContractException("SupplyEngineRuleInvalid");
            var planIds = rule.SelectedSupplyPlans.Select(value => value.ContractDraftStableId).ToArray();
            if (planIds.Length == 0 || planIds.Distinct(StringComparer.Ordinal).Count() != planIds.Length)
                throw new SimulationContractException("SupplyEngineSelectedPlanInvalid");
            foreach (var plan in rule.SelectedSupplyPlans)
            {
                var draft = supply.ContractDrafts.SingleOrDefault(value =>
                    value.ContractDraftStableId == plan.ContractDraftStableId);
                if (draft == null) throw new SimulationContractException("SupplyEngineContractDraftMissing");
                if (plan.WeeklyQuantity < draft.MinimumOrderQuantity
                    || plan.WeeklyQuantity > draft.BaseWeeklyQuantity + draft.MaximumAdditionalWeeklyQuantity)
                    throw new SimulationContractException("SupplyEngineWeeklyQuantityInvalid");
                if (plan.TransportCostPerDelivery < 0m)
                    throw new SimulationContractException("SupplyEngineTransportCostInvalid");
            }
            if (rule.WorkCapacityPolicyCode
                    != 도심마트공급EngineWorkPolicyCodes.CombinedInspectionPutAwayShelfAvailability
                || string.IsNullOrWhiteSpace(rule.LimitationText))
                throw new SimulationContractException("SupplyEngineWorkPolicyInvalid");
            var qualities = rule.QualityShelfLives.Select(value => value.QualityStandardCode).ToArray();
            if (qualities.Distinct(StringComparer.Ordinal).Count() != qualities.Length
                || rule.QualityShelfLives.Any(value => string.IsNullOrWhiteSpace(value.QualityStandardCode)
                    || value.ShelfLifeTicks <= 0)
                || supply.ContractDrafts.Where(value => planIds.Contains(value.ContractDraftStableId))
                    .Any(value => !qualities.Contains(value.QualityStandardCode)))
                throw new SimulationContractException("SupplyEngineShelfLifeRuleInvalid");
        }

        private static int ShelfLife(도심마트공급경영SimulationEngineRule rule, string quality)
            => rule.QualityShelfLives.Single(value => value.QualityStandardCode == quality).ShelfLifeTicks;

        private static decimal Inventory(IEnumerable<InventoryLot> lots) => lots.Sum(value => value.Quantity);

        private static SimulationDataLineage Lineage(string id, string revision, string ruleRevision)
            => new SimulationDataLineage
            {
                SourceStableId = id,
                SourceDataRevision = revision,
                RuleRevision = ruleRevision,
            };

        private sealed class InventoryLot
        {
            public InventoryLot(string stableId, decimal quantity, int expiresAtTick)
            {
                StableId = stableId;
                Quantity = quantity;
                ExpiresAtTick = expiresAtTick;
            }
            public string StableId { get; }
            public decimal Quantity { get; set; }
            public int ExpiresAtTick { get; }
        }

        private sealed class EngineOrder
        {
            public EngineOrder(string id, string source, int created, int due, decimal requested)
            {
                OrderStableId = id;
                SourceTypeCode = source;
                CreatedTick = created;
                FulfillmentDueTick = due;
                RequestedQuantity = requested;
            }
            public string OrderStableId { get; }
            public string SourceTypeCode { get; }
            public int CreatedTick { get; }
            public int FulfillmentDueTick { get; }
            public decimal RequestedQuantity { get; }
            public decimal FulfilledQuantity { get; set; }
            public bool Closed { get; set; }
        }

        private sealed class Payment
        {
            public Payment(int dueTick, decimal amount) { DueTick = dueTick; Amount = amount; }
            public int DueTick { get; }
            public decimal Amount { get; }
            public bool Paid { get; set; }
        }
    }

    public static class 도심마트감자공급경영SimulationEngineFixture
    {
        public static 도심마트공급경영SimulationEngineRule Rule()
            => new 도심마트공급경영SimulationEngineRule
            {
                RuleRevision = "potato-supply-engine-rule:1",
                DurationTicks = 28,
                InitialInventoryQuantity = 300m,
                InitialCash = 5_000_000m,
                StorageCapacity = 500m,
                ReceivingWorkCapacityPerTick = 60m,
                WorkCapacityPolicyCode =
                    도심마트공급EngineWorkPolicyCodes.CombinedInspectionPutAwayShelfAvailability,
                LimitationText = "첫 fixture는 검수·입고·진열 판매가능 전환 작업을 하나의 명시적 Tick capacity로 합산합니다.",
                InitialInventoryShelfLifeTicks = 8,
                QualityShelfLives = new[]
                {
                    new 도심마트품질판매기한SimulationRule { QualityStandardCode = "Fresh-A", ShelfLifeTicks = 9 },
                    new 도심마트품질판매기한SimulationRule { QualityStandardCode = "Standard-A", ShelfLifeTicks = 12 },
                    new 도심마트품질판매기한SimulationRule { QualityStandardCode = "Variable-Simulation", ShelfLifeTicks = 6 },
                },
                SelectedSupplyPlans = 도심마트감자공급SimulationFixture.CreateSupplySnapshot()
                    .ContractDrafts.Select(value => new 도심마트선택공급계획SimulationData
                    {
                        ContractDraftStableId = value.ContractDraftStableId,
                        WeeklyQuantity = value.BaseWeeklyQuantity,
                        TransportCostPerDelivery = value.SupplierStableId == "supplier:local-coop"
                            ? 5_000m
                            : value.SupplierStableId == "supplier:national-wholesaler"
                                ? 10_000m
                                : 0m,
                    }).ToArray(),
            };

        public static 도심마트공급경영SimulationWorldState Run()
            => new 도심마트공급경영SimulationEngine().Run(
                도심마트감자공급SimulationFixture.CreateSupplySnapshot(),
                도심마트감자기본방문주문SimulationFixture.Create(),
                도심마트감자수요CompositionSimulationFixture.Create(),
                Rule());
    }
}
