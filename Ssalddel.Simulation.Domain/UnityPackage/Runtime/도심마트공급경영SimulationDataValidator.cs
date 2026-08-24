using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
    public static class 도심마트공급경영SimulationDataValidator
    {
        private static readonly HashSet<string> OrderStates = new HashSet<string>(StringComparer.Ordinal)
        {
            SimulationOrderStateCodes.Pending,
            SimulationOrderStateCodes.Allocated,
            SimulationOrderStateCodes.PartiallyFulfilled,
            SimulationOrderStateCodes.Fulfilled,
            SimulationOrderStateCodes.Unfulfilled,
            SimulationOrderStateCodes.Cancelled,
        };

        private static readonly HashSet<string> AllocationStates = new HashSet<string>(StringComparer.Ordinal)
        {
            SimulationOrderAllocationStateCodes.Reserved,
            SimulationOrderAllocationStateCodes.Consumed,
            SimulationOrderAllocationStateCodes.Released,
        };

        private static readonly HashSet<string> DemandSourceTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            SimulationDemandSourceTypeCodes.BaseScenarioDemand,
            SimulationDemandSourceTypeCodes.GroupConfirmedDemand,
        };

        public static void Validate(도심마트공급경영SimulationDataSnapshot snapshot)
        {
            ValidateHeader(snapshot);
            var suppliers = NotNull(snapshot.Suppliers, "SupplySuppliersMissing");
            var offers = NotNull(snapshot.Offers, "SupplyOffersMissing");
            var drafts = NotNull(snapshot.ContractDrafts, "SupplyContractDraftsMissing");
            EnsureUnique(suppliers.Select(x => x.SupplierStableId), "SupplySupplierStableIdDuplicate");
            EnsureUnique(offers.Select(x => x.OfferStableId), "SupplyOfferStableIdDuplicate");
            EnsureUnique(drafts.Select(x => x.ContractDraftStableId), "SupplyContractDraftStableIdDuplicate");

            var supplierIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var supplier in suppliers)
            {
                RequireStableId(supplier.SupplierStableId, "SupplySupplierStableIdInvalid");
                RequireText(supplier.DisplayName, "SupplySupplierDisplayNameMissing");
                RequireText(supplier.SupplierTypeCode, "SupplySupplierTypeMissing");
                RequireQuantity(supplier.MaximumWeeklyQuantity, "SupplySupplierMaximumQuantityInvalid");
                RequireText(supplier.QuantityUnitCode, "SupplySupplierQuantityUnitMissing");
                supplierIds.Add(supplier.SupplierStableId);
            }

            var offerById = new Dictionary<string, 도심마트공급제안SimulationData>(StringComparer.Ordinal);
            foreach (var offer in offers)
            {
                RequireStableId(offer.OfferStableId, "SupplyOfferStableIdInvalid");
                if (!supplierIds.Contains(offer.SupplierStableId)) Fail("SupplyOfferSupplierMissing");
                ValidateCommercialTerms(offer.ProductStableId, offer.UnitPrice, offer.CurrencyCode,
                    offer.MinimumOrderQuantity, offer.MaximumWeeklyQuantity, offer.QuantityUnitCode,
                    offer.DeliveriesPerWeek, offer.LeadTimeTicks, offer.PaymentDueTicks,
                    offer.QualityStandardCode, "SupplyOffer");
                RequireText(offer.OfferRevision, "SupplyOfferRevisionMissing");
                offerById.Add(offer.OfferStableId, offer);
            }

            foreach (var draft in drafts)
            {
                RequireStableId(draft.ContractDraftStableId, "SupplyContractDraftStableIdInvalid");
                if (!offerById.TryGetValue(draft.SourceOfferStableId, out var sourceOffer)) Fail("SupplyContractDraftOfferMissing");
                if (!supplierIds.Contains(draft.SupplierStableId)) Fail("SupplyContractDraftSupplierMissing");
                if (!string.Equals(draft.SupplierStableId, sourceOffer.SupplierStableId, StringComparison.Ordinal)
                    || !string.Equals(draft.ProductStableId, sourceOffer.ProductStableId, StringComparison.Ordinal)
                    || !string.Equals(draft.CurrencyCode, sourceOffer.CurrencyCode, StringComparison.Ordinal)
                    || !string.Equals(draft.QuantityUnitCode, sourceOffer.QuantityUnitCode, StringComparison.Ordinal))
                    Fail("SupplyContractDraftOfferBoundaryMismatch");
                ValidateCommercialTerms(draft.ProductStableId, draft.UnitPrice, draft.CurrencyCode,
                    draft.MinimumOrderQuantity, draft.BaseWeeklyQuantity + draft.MaximumAdditionalWeeklyQuantity,
                    draft.QuantityUnitCode, draft.DeliveriesPerWeek, draft.LeadTimeTicks,
                    draft.PaymentDueTicks, draft.QualityStandardCode, "SupplyContractDraft");
                if (draft.BaseWeeklyQuantity < draft.MinimumOrderQuantity) Fail("SupplyContractDraftMinimumQuantityNotMet");
                if (draft.MaximumAdditionalWeeklyQuantity < 0) Fail("SupplyContractDraftAdditionalQuantityInvalid");
                if (draft.StartsAtTick < 0 || draft.EndsAtTick < draft.StartsAtTick) Fail("SupplyContractDraftPeriodInvalid");
                RequireText(draft.DraftRevision, "SupplyContractDraftRevisionMissing");
            }
        }

        public static void Validate(도심마트수요시나리오DataSnapshot snapshot)
        {
            ValidateHeader(snapshot);
            RequireStableId(snapshot.RegionStableId, "DemandRegionStableIdInvalid");
            RequireText(snapshot.PopulationBasisRevision, "DemandPopulationBasisRevisionMissing");
            RequireText(snapshot.DemandRuleRevision, "DemandRuleRevisionMissing");
            RequireRate(snapshot.ProductSelectionRate, "DemandProductSelectionRateMissingOrInvalid");
            RequireRate(snapshot.SimulationMarketShareRate, "DemandMarketShareRateMissingOrInvalid");
            RequireText(snapshot.SeasonAssumptionCode, "DemandSeasonAssumptionMissing");
            RequireText(snapshot.EventAssumptionCode, "DemandEventAssumptionMissing");
            RequireText(snapshot.LimitationText, "DemandLimitationMissing");
            var segments = NotNull(snapshot.DemandSegments, "DemandSegmentsMissing");
            EnsureUnique(segments.Select(x => x.DemandSegmentStableId), "DemandSegmentStableIdDuplicate");
            foreach (var segment in segments)
            {
                RequireStableId(segment.DemandSegmentStableId, "DemandSegmentStableIdInvalid");
                RequireStableId(segment.ProductStableId, "DemandProductStableIdInvalid");
                if (segment.StartsAtTick < 0 || segment.EndsAtTick < segment.StartsAtTick) Fail("DemandSegmentPeriodInvalid");
                if (segment.MinimumQuantity < 0
                    || segment.ExpectedQuantity < segment.MinimumQuantity
                    || segment.MaximumQuantity < segment.ExpectedQuantity)
                    Fail("DemandSegmentQuantityRangeInvalid");
                RequireText(segment.QuantityUnitCode, "DemandQuantityUnitMissing");
            }
        }

        public static void Validate(도심마트주문자집단수요SimulationDataSnapshot snapshot)
        {
            ValidateHeader(snapshot);
            var groups = NotNull(snapshot.Groups, "OrdererGroupDemandGroupsMissing");
            EnsureUnique(groups.Select(x => x.OrdererGroupStableId), "OrdererGroupStableIdDuplicate");
            EnsureUnique(groups.Select(x => x.DemandRequestStableId), "OrdererGroupDemandRequestStableIdDuplicate");
            EnsureUnique(groups.Select(x => x.Representative?.RepresentativeStableId ?? string.Empty),
                "OrdererGroupRepresentativeStableIdDuplicate");
            EnsureUnique(groups.Select(x => x.Representative?.NpcStableId ?? string.Empty),
                "OrdererGroupRepresentativeNpcStableIdDuplicate");
            EnsureUnique(groups.Select(x => x.Representative?.RepresentativeVisitStableId ?? string.Empty),
                "OrdererGroupRepresentativeVisitStableIdDuplicate");

            foreach (var group in groups)
            {
                RequireStableId(group.OrdererGroupStableId, "OrdererGroupStableIdInvalid");
                RequireStableId(group.DemandRequestStableId, "OrdererGroupDemandRequestStableIdInvalid");
                if (!string.Equals(group.GroupContextCode,
                        도심마트주문자집단ContextCodes.ResidentialCommunity,
                        StringComparison.Ordinal))
                    Fail("OrdererGroupContextInvalid");
                RequireStableId(group.ProductStableId, "OrdererGroupProductStableIdInvalid");
                if (!string.Equals(group.StateCode,
                        도심마트주문자집단StateCodes.MemberConfirmationPending,
                        StringComparison.Ordinal))
                    Fail("OrdererGroupStateInvalid");
                if (group.IntentParticipantCount <= 0
                    || group.ConfirmedParticipantCount < 0
                    || group.ConfirmedParticipantCount > group.IntentParticipantCount)
                    Fail("OrdererGroupParticipantCountInvalid");
                if (group.IntentQuantity <= 0
                    || group.ConfirmedQuantity < 0
                    || group.ConfirmedQuantity > group.IntentQuantity
                    || (group.ConfirmedParticipantCount > 0 && group.ConfirmedQuantity <= 0))
                    Fail("OrdererGroupQuantityInvalid");
                RequireText(group.QuantityUnitCode, "OrdererGroupQuantityUnitMissing");
                if (group.RequestedFulfillmentStartsAtTick < 0
                    || group.RequestedFulfillmentEndsAtTick < group.RequestedFulfillmentStartsAtTick)
                    Fail("OrdererGroupFulfillmentWindowInvalid");
                RequireStableId(group.RequestedPickupPointStableId,
                    "OrdererGroupPickupPointStableIdInvalid");
                if (!string.Equals(group.PickupPointStateCode,
                        도심마트공동수령지StateCodes.Candidate,
                        StringComparison.Ordinal))
                    Fail("OrdererGroupPickupPointStateInvalid");

                var representative = group.Representative;
                if (representative == null) Fail("OrdererGroupRepresentativeMissing");
                RequireStableId(representative.RepresentativeStableId,
                    "OrdererGroupRepresentativeStableIdInvalid");
                if (!string.Equals(representative.SocialContextCode,
                        도심마트대표SocialContextCodes.ResidentialCommunityRepresentative,
                        StringComparison.Ordinal))
                    Fail("OrdererGroupRepresentativeSocialContextInvalid");
                RequireText(representative.DisplayLabel, "OrdererGroupRepresentativeDisplayLabelMissing");
                if (!string.Equals(representative.CanonicalRoleCode,
                        도심마트대표CanonicalRoleCodes.GroupPurchaseRepresentative,
                        StringComparison.Ordinal))
                    Fail("OrdererGroupRepresentativeCanonicalRoleInvalid");
                if (!string.Equals(representative.RoleStateCode,
                        도심마트대표RoleStateCodes.AssignedSimulatedCoordinator,
                        StringComparison.Ordinal))
                    Fail("OrdererGroupRepresentativeRoleStateInvalid");
                RequireStableId(representative.NpcStableId,
                    "OrdererGroupRepresentativeNpcStableIdInvalid");
                RequireStableId(representative.RepresentativeVisitStableId,
                    "OrdererGroupRepresentativeVisitStableIdInvalid");
            }
        }

        public static void Validate(도심마트주문SimulationDataSnapshot snapshot)
        {
            ValidateHeader(snapshot);
            RequireText(snapshot.DemandScenarioDataRevision, "OrderDemandRevisionMissing");
            RequireText(snapshot.GenerationRuleRevision, "OrderGenerationRuleRevisionMissing");
            if (!string.Equals(snapshot.QuantityBasisCode,
                    도심마트주문생성수량기준코드.ExpectedQuantity,
                    StringComparison.Ordinal))
                Fail("OrderQuantityBasisInvalid");
            if (!string.Equals(snapshot.SplitStrategyCode,
                    도심마트주문분할전략코드.EvenSplitWithSeededRemainder,
                    StringComparison.Ordinal))
                Fail("OrderSplitStrategyInvalid");
            RequireText(snapshot.GenerationLimitationText, "OrderGenerationLimitationMissing");
            var orders = NotNull(snapshot.Orders, "SimulationOrdersMissing");
            var allocations = NotNull(snapshot.Allocations, "SimulationOrderAllocationsMissing");
            EnsureUnique(orders.Select(x => x.OrderStableId), "SimulationOrderStableIdDuplicate");
            EnsureUnique(allocations.Select(x => x.AllocationStableId), "SimulationOrderAllocationStableIdDuplicate");

            var orderById = new Dictionary<string, 도심마트주문SimulationData>(StringComparer.Ordinal);
            foreach (var order in orders)
            {
                RequireStableId(order.OrderStableId, "SimulationOrderStableIdInvalid");
                RequireStableId(order.SourceDemandSegmentStableId, "SimulationOrderDemandSegmentInvalid");
                if (!DemandSourceTypes.Contains(order.DemandSourceTypeCode))
                    Fail("SimulationOrderDemandSourceTypeInvalid");
                RequireStableId(order.ProductStableId, "SimulationOrderProductInvalid");
                RequireStableId(order.RegionStableId, "SimulationOrderRegionInvalid");
                if (order.CreatedTick < 0 || order.FulfillmentDueTick < order.CreatedTick) Fail("SimulationOrderPeriodInvalid");
                if (order.RequestedQuantity <= 0 || order.AllocatedQuantity < 0
                    || order.FulfilledQuantity < 0 || order.UnfulfilledQuantity < 0
                    || order.AllocatedQuantity > order.RequestedQuantity
                    || order.FulfilledQuantity + order.UnfulfilledQuantity > order.RequestedQuantity)
                    Fail("SimulationOrderQuantityInvariantInvalid");
                RequireText(order.QuantityUnitCode, "SimulationOrderQuantityUnitMissing");
                if (!OrderStates.Contains(order.StateCode)) Fail("SimulationOrderStateInvalid");
                orderById.Add(order.OrderStableId, order);
            }

            var activeAllocatedByOrder = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var allocation in allocations)
            {
                RequireStableId(allocation.AllocationStableId, "SimulationOrderAllocationStableIdInvalid");
                RequireStableId(allocation.InventoryStableId, "SimulationOrderInventoryStableIdInvalid");
                if (!orderById.TryGetValue(allocation.OrderStableId, out var order)) Fail("SimulationOrderAllocationOrderMissing");
                if (allocation.Quantity <= 0 || allocation.AllocationRevision < 0) Fail("SimulationOrderAllocationQuantityInvalid");
                RequireText(allocation.QuantityUnitCode, "SimulationOrderAllocationUnitMissing");
                if (!string.Equals(allocation.QuantityUnitCode, order.QuantityUnitCode, StringComparison.Ordinal))
                    Fail("SimulationOrderAllocationUnitMismatch");
                if (!AllocationStates.Contains(allocation.StateCode)) Fail("SimulationOrderAllocationStateInvalid");
                if (!string.Equals(allocation.StateCode, SimulationOrderAllocationStateCodes.Released, StringComparison.Ordinal))
                {
                    activeAllocatedByOrder.TryGetValue(order.OrderStableId, out var current);
                    activeAllocatedByOrder[order.OrderStableId] = current + allocation.Quantity;
                }
            }

            foreach (var order in orders)
            {
                activeAllocatedByOrder.TryGetValue(order.OrderStableId, out var allocated);
                if (allocated != order.AllocatedQuantity) Fail("SimulationOrderAllocationTotalMismatch");
            }
        }

        private static void ValidateHeader(도심마트SimulationDataSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            RequireStableId(snapshot.SnapshotStableId, "SimulationSnapshotStableIdInvalid");
            RequireStableId(snapshot.SessionStableId, "SimulationSessionStableIdInvalid");
            RequireStableId(snapshot.ScenarioStableId, "SimulationScenarioStableIdInvalid");
            RequireText(snapshot.DataRevision, "SimulationDataRevisionMissing");
            if (snapshot.AsOfTick < 0) Fail("SimulationAsOfTickInvalid");
            if (!string.Equals(snapshot.ModeCode, SimulationModeCodes.Simulation, StringComparison.Ordinal)
                || snapshot.IsOperationalState)
                Fail("SimulationOperationalBoundaryViolation");
            var lineage = NotNull(snapshot.SourceLineage, "SimulationSourceLineageMissing");
            if (lineage.Length == 0) Fail("SimulationSourceLineageMissing");
            foreach (var source in lineage)
            {
                if (source == null) Fail("SimulationSourceLineageInvalid");
                RequireStableId(source.SourceStableId, "SimulationSourceStableIdInvalid");
                RequireText(source.SourceDataRevision, "SimulationSourceDataRevisionMissing");
                RequireText(source.RuleRevision, "SimulationSourceRuleRevisionMissing");
            }
        }

        private static void ValidateCommercialTerms(string productStableId, decimal unitPrice,
            string currencyCode, decimal minimumQuantity, decimal maximumQuantity, string quantityUnitCode,
            int deliveriesPerWeek, int leadTimeTicks, int paymentDueTicks, string qualityStandardCode, string prefix)
        {
            RequireStableId(productStableId, prefix + "ProductStableIdInvalid");
            if (unitPrice <= 0) Fail(prefix + "UnitPriceInvalid");
            RequireText(currencyCode, prefix + "CurrencyMissing");
            if (minimumQuantity < 0 || maximumQuantity < minimumQuantity) Fail(prefix + "QuantityRangeInvalid");
            RequireText(quantityUnitCode, prefix + "QuantityUnitMissing");
            if (deliveriesPerWeek <= 0 || deliveriesPerWeek > 7) Fail(prefix + "DeliveryFrequencyInvalid");
            if (leadTimeTicks < 0 || paymentDueTicks < 0) Fail(prefix + "TimingInvalid");
            RequireText(qualityStandardCode, prefix + "QualityStandardMissing");
        }

        private static T[] NotNull<T>(T[]? values, string errorCode)
        {
            if (values == null) Fail(errorCode);
            return values!;
        }

        private static void EnsureUnique(IEnumerable<string> values, string errorCode)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
                if (!ids.Add(value ?? string.Empty)) Fail(errorCode);
        }

        private static void RequireRate(decimal? value, string errorCode)
        {
            if (!value.HasValue || value.Value < 0 || value.Value > 1) Fail(errorCode);
        }

        private static void RequireQuantity(decimal value, string errorCode)
        {
            if (value < 0) Fail(errorCode);
        }

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                Fail(errorCode);
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)) Fail(errorCode);
        }

        [DoesNotReturn]
        private static void Fail(string errorCode)
            => throw new SimulationContractException(errorCode);
    }
}
