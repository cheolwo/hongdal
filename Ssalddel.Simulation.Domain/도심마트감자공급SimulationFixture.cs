using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class 도심마트감자공급SimulationFixture
    {
        public const string ScenarioStableId = "scenario:urban-market-potato-4w";
        public const string ProductStableId = "product:potato";
        public const string QuantityUnitCode = "kg";
        public const string CurrencyCode = "KRW";

        public static 도심마트공급경영SimulationDataSnapshot CreateSupplySnapshot(
            string sessionStableId = "simulation-session:potato-fixture")
        {
            var snapshot = new 도심마트공급경영SimulationDataSnapshot
            {
                SnapshotStableId = "supply-snapshot:potato-4w:1",
                SessionStableId = sessionStableId,
                ScenarioStableId = ScenarioStableId,
                DataRevision = "simulation-fixture:supply-potato-4w:1",
                AsOfTick = 0,
                ModeCode = SimulationModeCodes.Simulation,
                IsOperationalState = false,
                SourceLineage = new[]
                {
                    new SimulationDataLineage
                    {
                        SourceStableId = "fixture:urban-market-potato-suppliers",
                        SourceDataRevision = "fixture:urban-market-potato-suppliers:1",
                        RuleRevision = "supply-fixture-rule:1",
                    },
                },
                Suppliers = Suppliers(),
                Offers = Offers(),
                ContractDrafts = ContractDrafts(),
            };

            도심마트공급경영SimulationDataValidator.Validate(snapshot);
            return snapshot;
        }

        private static 도심마트공급처SimulationData[] Suppliers()
            => new[]
            {
                Supplier("supplier:local-coop", "지역 생산자 협동조합", "LocalCooperative", 120m),
                Supplier("supplier:national-wholesaler", "대형 도매 공급처", "NationalWholesaler", 300m),
                Supplier("supplier:spot-market", "Simulation 현물 시장", "SpotMarket", 80m),
            };

        private static 도심마트공급제안SimulationData[] Offers()
            => new[]
            {
                Offer("offer:local-coop:potato:1", "supplier:local-coop", 1850m, 10m, 120m, 2, 2, 14, "Fresh-A"),
                Offer("offer:national-wholesaler:potato:1", "supplier:national-wholesaler", 1600m, 50m, 300m, 1, 4, 7, "Standard-A"),
                Offer("offer:spot-market:potato:1", "supplier:spot-market", 2300m, 1m, 80m, 7, 0, 0, "Variable-Simulation"),
            };

        private static 도심마트공급계약안SimulationData[] ContractDrafts()
            => new[]
            {
                Draft("contract-draft:local-coop:potato:1", "offer:local-coop:potato:1",
                    "supplier:local-coop", 1850m, 10m, 70m, 30m, 2, 2, 14, "Fresh-A"),
                Draft("contract-draft:national-wholesaler:potato:1", "offer:national-wholesaler:potato:1",
                    "supplier:national-wholesaler", 1600m, 50m, 50m, 100m, 1, 4, 7, "Standard-A"),
                Draft("contract-draft:spot-market:potato:1", "offer:spot-market:potato:1",
                    "supplier:spot-market", 2300m, 1m, 10m, 70m, 7, 0, 0, "Variable-Simulation"),
            };

        private static 도심마트공급처SimulationData Supplier(
            string stableId,
            string displayName,
            string supplierTypeCode,
            decimal maximumWeeklyQuantity)
            => new 도심마트공급처SimulationData
            {
                SupplierStableId = stableId,
                DisplayName = displayName,
                SupplierTypeCode = supplierTypeCode,
                MaximumWeeklyQuantity = maximumWeeklyQuantity,
                QuantityUnitCode = QuantityUnitCode,
            };

        private static 도심마트공급제안SimulationData Offer(
            string stableId,
            string supplierStableId,
            decimal unitPrice,
            decimal minimumOrderQuantity,
            decimal maximumWeeklyQuantity,
            int deliveriesPerWeek,
            int leadTimeTicks,
            int paymentDueTicks,
            string qualityStandardCode)
            => new 도심마트공급제안SimulationData
            {
                OfferStableId = stableId,
                SupplierStableId = supplierStableId,
                ProductStableId = ProductStableId,
                UnitPrice = unitPrice,
                CurrencyCode = CurrencyCode,
                MinimumOrderQuantity = minimumOrderQuantity,
                MaximumWeeklyQuantity = maximumWeeklyQuantity,
                QuantityUnitCode = QuantityUnitCode,
                DeliveriesPerWeek = deliveriesPerWeek,
                LeadTimeTicks = leadTimeTicks,
                PaymentDueTicks = paymentDueTicks,
                QualityStandardCode = qualityStandardCode,
                OfferRevision = stableId + ":revision:1",
            };

        private static 도심마트공급계약안SimulationData Draft(
            string stableId,
            string offerStableId,
            string supplierStableId,
            decimal unitPrice,
            decimal minimumOrderQuantity,
            decimal baseWeeklyQuantity,
            decimal maximumAdditionalWeeklyQuantity,
            int deliveriesPerWeek,
            int leadTimeTicks,
            int paymentDueTicks,
            string qualityStandardCode)
            => new 도심마트공급계약안SimulationData
            {
                ContractDraftStableId = stableId,
                SourceOfferStableId = offerStableId,
                SupplierStableId = supplierStableId,
                ProductStableId = ProductStableId,
                UnitPrice = unitPrice,
                CurrencyCode = CurrencyCode,
                MinimumOrderQuantity = minimumOrderQuantity,
                BaseWeeklyQuantity = baseWeeklyQuantity,
                MaximumAdditionalWeeklyQuantity = maximumAdditionalWeeklyQuantity,
                QuantityUnitCode = QuantityUnitCode,
                DeliveriesPerWeek = deliveriesPerWeek,
                LeadTimeTicks = leadTimeTicks,
                PaymentDueTicks = paymentDueTicks,
                QualityStandardCode = qualityStandardCode,
                StartsAtTick = 0,
                EndsAtTick = 27,
                DraftRevision = stableId + ":revision:1",
            };
    }
}
