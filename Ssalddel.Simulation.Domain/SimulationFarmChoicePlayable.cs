using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string FarmChoiceDecisionStableIdPrefix =
            "harvest-disposition:farm-choice:";

        public SimulationFarmChoiceContextSnapshot GetFarmChoiceContext()
        {
            lock (gate)
            {
                var settlement = CreateSettlementSnapshot()
                    ?? throw new SimulationContractException(
                        "SimulationSettlementRequiredForFarmChoice");
                var harvestLot = ResolveFarmChoiceHarvestLot(
                    settlement,
                    out var allocation);
                var appliedChoice = allocation == null
                    ? string.Empty
                    : ChoiceStableIdForCode(allocation.ChoiceCode);
                var resolved = allocation != null;
                var facts = CreateFarmChoiceFacts(settlement, harvestLot);
                return new SimulationFarmChoiceContextSnapshot
                {
                    SessionStableId = SessionStableId,
                    SituationStableId = SimulationFarmChoicePlayableCodes.SituationStableId,
                    SituationRevision = 1,
                    WorldRevision = Revision,
                    WorldTick = CurrentTick,
                    AreaSetStableId = SimulationFarmChoicePlayableCodes.AreaSetStableId,
                    ProductStableId = SimulationFarmChoicePlayableCodes.ProductStableId,
                    HarvestLotStableId = harvestLot?.HarvestLotStableId ?? string.Empty,
                    SituationStateCode = harvestLot == null
                        ? SimulationFarmChoicePlayableCodes.AwaitingHarvest
                        : resolved
                            ? SimulationFarmChoicePlayableCodes.ChoiceConfirmed
                            : SimulationFarmChoicePlayableCodes.AwaitingChoice,
                    AppliedChoiceStableId = appliedChoice,
                    IsSimulationOnly = true,
                    IsOperationalState = false,
                    Facts = facts,
                    Candidates = harvestLot == null
                        ? Array.Empty<SimulationFarmChoiceCandidateSnapshot>()
                        : CreateFarmChoiceCandidates(resolved, facts, harvestLot),
                };
            }
        }

        public SimulationFarmChoicePreviewSnapshot PreviewFarmChoice(
            SimulationFarmChoicePreviewRequest request)
        {
            ValidateFarmChoicePreviewRequest(request);
            lock (gate)
            {
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");
                var harvestLot = RequirePendingFarmChoiceHarvestLot();
                var impact = CreateHarvestDispositionImpactPreview(
                    CreateFarmChoiceImpactRequest(request.ChoiceStableId, harvestLot));
                return new SimulationFarmChoicePreviewSnapshot
                {
                    SituationStableId = SimulationFarmChoicePlayableCodes.SituationStableId,
                    ChoiceStableId = request.ChoiceStableId.Trim(),
                    BaseRevision = Revision,
                    IsCandidateOnly = true,
                    RequiresExplicitConfirm = true,
                    Impact = impact,
                };
            }
        }

        public 경영SimulationSessionSnapshot ConfirmFarmChoice(
            SimulationFarmChoiceConfirmRequest request)
        {
            ValidateFarmChoiceConfirmRequest(request);
            lock (gate)
            {
                var harvestLot = RequireFarmChoiceHarvestLotForConfirm();
                return ConfirmHarvestDispositionImpact(
                    new SimulationHarvestDispositionImpactConfirmRequest
                    {
                        CommandId = request.CommandId.Trim(),
                        ExpectedRevision = request.ExpectedRevision,
                        Impact = CreateFarmChoiceImpactRequest(
                            request.ChoiceStableId,
                            harvestLot),
                    });
            }
        }

        private SimulationFarmChoiceCandidateSnapshot[] CreateFarmChoiceCandidates(
            bool resolved,
            SimulationFarmChoiceFactSnapshot[] facts,
            Simulation수확LotSnapshot harvestLot)
            => new[]
            {
                CreateFarmChoiceCandidate(
                    SimulationFarmChoicePlayableCodes.ReserveStorageChoice,
                    "Capability", "비축 보관", "StorageCapacityObserved", resolved,
                    facts, facts[1], harvestLot),
                CreateFarmChoiceCandidate(
                    SimulationFarmChoicePlayableCodes.HubShipmentChoice,
                    "Logistics", "Hub 출하", "HubShipmentFacilityObserved", resolved,
                    facts, facts[2], harvestLot),
                CreateFarmChoiceCandidate(
                    SimulationFarmChoicePlayableCodes.TownDirectSaleChoice,
                    "Trade", "Town 직거래", "TownMarketFacilityObserved", resolved,
                    facts, facts[3], harvestLot),
            };

        private SimulationFarmChoiceCandidateSnapshot CreateFarmChoiceCandidate(
            string choiceStableId,
            string cardFunctionCode,
            string koreanDisplayName,
            string reasonCode,
            bool resolved,
            SimulationFarmChoiceFactSnapshot[] facts,
            SimulationFarmChoiceFactSnapshot capabilityFact,
            Simulation수확LotSnapshot harvestLot)
        {
            var mapping = MapChoice(choiceStableId);
            var blocks = Array.Empty<string>();
            try
            {
                var impact = CreateHarvestDispositionImpactPreview(
                    CreateFarmChoiceImpactRequest(choiceStableId, harvestLot));
                blocks = resolved
                    ? new[] { "FarmChoiceAlreadyConfirmed" }
                    : impact.CommonDecisionPreview.Decision.BlockReasonCodes;
            }
            catch (SimulationContractException exception)
            {
                blocks = new[] { exception.ErrorCode };
            }
            return new SimulationFarmChoiceCandidateSnapshot
            {
                ChoiceStableId = choiceStableId,
                ChoiceCode = mapping.ChoiceCode,
                CardFunctionCode = cardFunctionCode,
                KoreanDisplayName = koreanDisplayName,
                NextWorkflowCode = mapping.WorkflowCode,
                IsAvailable = blocks.Length == 0,
                BlockReasonCodes = blocks,
                CandidateReasons = new[]
                {
                    new SimulationFarmChoiceCandidateReasonSnapshot
                    {
                        ReasonCode = "PotatoHarvestCompleted",
                        SourceFactStableIds = new[] { facts[0].FactStableId },
                    },
                    new SimulationFarmChoiceCandidateReasonSnapshot
                    {
                        ReasonCode = reasonCode,
                        SourceFactStableIds = new[] { capabilityFact.FactStableId },
                    },
                },
            };
        }

        private SimulationFarmChoiceFactSnapshot[] CreateFarmChoiceFacts(
            SimulationSettlementEconomySnapshot settlement,
            Simulation수확LotSnapshot? harvestLot)
        {
            if (harvestLot == null)
            {
                return new[]
                {
                    new SimulationFarmChoiceFactSnapshot
                    {
                        FactStableId = "farm-fact:potato-harvest-pending.v1",
                        FactCode = "HarvestLotNotReady",
                        TargetStableId = SimulationFarmChoicePlayableCodes.AreaSetStableId,
                        ValueCode = "NoEligibleHarvestLot",
                        SourceStableIds = new[] { ScenarioDataRevision },
                    },
                };
            }

            var storageFacility = settlement.Facilities
                .Where(value => value.FacilityTypeCode
                    == SimulationSettlementFacilityTypeCodes.Storage)
                .OrderBy(value => value.FacilityStableId, StringComparer.Ordinal)
                .FirstOrDefault();
            var marketFacility = settlement.Facilities
                .Where(value => value.FacilityTypeCode
                    == SimulationSettlementFacilityTypeCodes.Market)
                .OrderBy(value => value.FacilityStableId, StringComparer.Ordinal)
                .FirstOrDefault();
            var storageSources = (storageFacility?.SourceStableIds
                    ?? Array.Empty<string>())
                .Concat(settlement.SourceStableIds)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var marketSources = (marketFacility?.SourceStableIds
                    ?? Array.Empty<string>())
                .Concat(settlement.SourceStableIds)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

            return new[]
            {
                new SimulationFarmChoiceFactSnapshot
                {
                    FactStableId = "farm-fact:potato-harvest-lot-ready:"
                        + harvestLot.HarvestLotStableId,
                    FactCode = "HarvestLotReady",
                    TargetStableId = harvestLot.HarvestLotStableId,
                    ValueCode = harvestLot.Quantity.ToString(CultureInfo.InvariantCulture)
                        + harvestLot.UnitCode,
                    SourceStableIds = harvestLot.SourceStableIds
                        .Append(harvestLot.HarvestLotStableId)
                        .Append(ScenarioDataRevision)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray(),
                },
                new SimulationFarmChoiceFactSnapshot
                {
                    FactStableId = "farm-fact:settlement-storage-capacity.v1",
                    FactCode = "SettlementStorageAvailable",
                    TargetStableId = SettlementStableId,
                    ValueCode = settlement.StorageAvailable
                        .ToString(CultureInfo.InvariantCulture)
                        + settlement.StorageUnitCode,
                    SourceStableIds = storageSources,
                },
                new SimulationFarmChoiceFactSnapshot
                {
                    FactStableId = "farm-fact:hub-shipment-facility.v1",
                    FactCode = "HubShipmentFacility",
                    TargetStableId = storageFacility?.FacilityStableId
                        ?? SettlementStableId,
                    ValueCode = storageFacility == null ? "Unavailable" : "Available",
                    SourceStableIds = storageSources,
                },
                new SimulationFarmChoiceFactSnapshot
                {
                    FactStableId = "farm-fact:town-market-facility.v1",
                    FactCode = "TownMarketFacility",
                    TargetStableId = marketFacility?.FacilityStableId
                        ?? SettlementStableId,
                    ValueCode = marketFacility == null ? "Unavailable" : "Available",
                    SourceStableIds = marketSources,
                },
            };
        }

        private Simulation수확LotSnapshot RequirePendingFarmChoiceHarvestLot()
        {
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException(
                    "SimulationSettlementRequiredForFarmChoice");
            var harvestLot = ResolveFarmChoiceHarvestLot(settlement, out var allocation)
                ?? throw new SimulationConflictException(
                    "SimulationFarmChoiceHarvestLotUnavailable");
            if (allocation != null)
                throw new SimulationConflictException(
                    "SimulationFarmChoiceAlreadyConfirmed");
            return harvestLot;
        }

        private Simulation수확LotSnapshot RequireFarmChoiceHarvestLotForConfirm()
        {
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException(
                    "SimulationSettlementRequiredForFarmChoice");
            return ResolveFarmChoiceHarvestLot(settlement, out _)
                ?? throw new SimulationConflictException(
                    "SimulationFarmChoiceHarvestLotUnavailable");
        }

        private Simulation수확LotSnapshot? ResolveFarmChoiceHarvestLot(
            SimulationSettlementEconomySnapshot settlement,
            out SimulationHarvestLotAllocationSnapshot? allocation)
        {
            var eligibleLots = harvestLots.Values
                .Where(value => string.Equals(
                    value.ProductStableId,
                    SimulationFarmChoicePlayableCodes.ProductStableId,
                    StringComparison.Ordinal))
                .Where(value => value.StateCode == Simulation수확Lot상태Codes.CollectedAtYard)
                .OrderBy(value => value.CreatedWorldTick)
                .ThenBy(value => value.HarvestLotStableId, StringComparer.Ordinal)
                .ToArray();
            foreach (var harvestLot in eligibleLots)
            {
                if (!settlement.HarvestLotAllocations.Any(value =>
                    value.HarvestLotStableId == harvestLot.HarvestLotStableId))
                {
                    allocation = null;
                    return harvestLot;
                }
            }

            foreach (var harvestLot in eligibleLots)
            {
                var resolved = settlement.HarvestLotAllocations.SingleOrDefault(value =>
                    value.HarvestLotStableId == harvestLot.HarvestLotStableId
                    && value.DecisionStableId.StartsWith(
                        FarmChoiceDecisionStableIdPrefix,
                        StringComparison.Ordinal));
                if (resolved != null)
                {
                    allocation = resolved;
                    return harvestLot;
                }
            }

            allocation = null;
            return null;
        }

        private SimulationHarvestDispositionImpactPreviewRequest
            CreateFarmChoiceImpactRequest(
                string choiceStableId,
                Simulation수확LotSnapshot harvestLot)
        {
            var mapping = MapChoice(choiceStableId);
            var actorStableId = farmWorkOrders
                .Where(value => value.WorkOrderStableId == harvestLot.CausedByTaskStableId)
                .Select(value => value.ActorStableId)
                .SingleOrDefault()
                ?? throw new SimulationContractException(
                    "SimulationFarmChoiceHarvestActorRequired");
            return new SimulationHarvestDispositionImpactPreviewRequest
            {
                DispositionDecisionStableId = FarmChoiceDecisionStableIdPrefix
                    + harvestLot.HarvestLotStableId,
                DispositionDecisionRevision = 1,
                HarvestLotStableId = harvestLot.HarvestLotStableId,
                HarvestLotRevision = harvestLot.Revision,
                ProductStableId = harvestLot.ProductStableId,
                Quantity = harvestLot.Quantity,
                UnitCode = harvestLot.UnitCode,
                ChoiceCode = mapping.ChoiceCode,
                NextWorkflowCode = mapping.WorkflowCode,
                ActorStableId = actorStableId,
                SourceStableIds = harvestLot.SourceStableIds
                    .Append(harvestLot.HarvestLotStableId)
                    .Append(SimulationFarmChoicePlayableCodes.SituationStableId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
            };
        }

        private void EnsureHarvestDispositionMatchesFarmLedger(
            SimulationHarvestDispositionImpactPreviewRequest request)
        {
            if (farmSurvivalCreationState == null)
                return;

            if (!harvestLots.TryGetValue(
                request.HarvestLotStableId.Trim(),
                out var harvestLot))
            {
                throw new SimulationConflictException(
                    "SimulationHarvestLotNotFoundInFarmLedger");
            }
            if (harvestLot.StateCode != Simulation수확Lot상태Codes.CollectedAtYard)
            {
                throw new SimulationConflictException(
                    "SimulationHarvestLotNotEligibleForDisposition");
            }
            if (request.HarvestLotRevision != harvestLot.Revision)
                throw new SimulationConflictException(
                    "SimulationHarvestLotRevisionMismatch");
            if (!string.Equals(
                request.ProductStableId.Trim(),
                harvestLot.ProductStableId,
                StringComparison.Ordinal))
            {
                throw new SimulationConflictException(
                    "SimulationHarvestLotProductMismatch");
            }
            if (request.Quantity != harvestLot.Quantity)
                throw new SimulationConflictException(
                    "SimulationHarvestLotQuantityMismatch");
            if (!string.Equals(
                request.UnitCode.Trim(),
                harvestLot.UnitCode,
                StringComparison.Ordinal))
            {
                throw new SimulationConflictException(
                    "SimulationHarvestLotUnitMismatch");
            }
            var harvestActorStableId = farmWorkOrders
                .Where(value => value.WorkOrderStableId == harvestLot.CausedByTaskStableId)
                .Select(value => value.ActorStableId)
                .SingleOrDefault();
            if (string.IsNullOrWhiteSpace(harvestActorStableId)
                || !string.Equals(
                    request.ActorStableId.Trim(),
                    harvestActorStableId,
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationHarvestLotActorMismatch");

            var requestSources = request.SourceStableIds
                .Select(value => value.Trim())
                .ToHashSet(StringComparer.Ordinal);
            if (harvestLot.SourceStableIds.Any(source => !requestSources.Contains(source)))
                throw new SimulationConflictException(
                    "SimulationHarvestLotSourceLineageMismatch");
        }

        private static (string ChoiceCode, string WorkflowCode) MapChoice(
            string choiceStableId)
            => choiceStableId.Trim() switch
            {
                SimulationFarmChoicePlayableCodes.ReserveStorageChoice => (
                    SimulationHarvestDispositionChoiceCodes.ReserveStorage,
                    SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate),
                SimulationFarmChoicePlayableCodes.HubShipmentChoice => (
                    SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
                    SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate),
                SimulationFarmChoicePlayableCodes.TownDirectSaleChoice => (
                    SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
                    SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate),
                _ => throw new SimulationContractException(
                    "SimulationFarmChoiceStableIdInvalid"),
            };

        private static string ChoiceStableIdForCode(string choiceCode)
            => choiceCode switch
            {
                SimulationHarvestDispositionChoiceCodes.ReserveStorage =>
                    SimulationFarmChoicePlayableCodes.ReserveStorageChoice,
                SimulationHarvestDispositionChoiceCodes.CooperativeShipment =>
                    SimulationFarmChoicePlayableCodes.HubShipmentChoice,
                SimulationHarvestDispositionChoiceCodes.DirectOnlineSale =>
                    SimulationFarmChoicePlayableCodes.TownDirectSaleChoice,
                _ => string.Empty,
            };

        private static void ValidateFarmChoicePreviewRequest(
            SimulationFarmChoicePreviewRequest request)
        {
            if (request == null)
                throw new SimulationContractException("SimulationFarmChoiceRequestMissing");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
            _ = MapChoice(request.ChoiceStableId ?? string.Empty);
        }

        private static void ValidateFarmChoiceConfirmRequest(
            SimulationFarmChoiceConfirmRequest request)
        {
            if (request == null)
                throw new SimulationContractException("SimulationFarmChoiceRequestMissing");
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
            _ = MapChoice(request.ChoiceStableId ?? string.Empty);
        }
    }
}
