using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public Simulation타로객체반응PreviewSnapshot Preview타로객체반응(
            Simulation타로객체반응PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.DrawStableId, "SimulationTarotDrawStableIdInvalid");

            lock (gate)
            {
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                var draw = CreateTarotDraw();
                if (draw.DrawStableId != request.DrawStableId.Trim())
                    throw new SimulationConflictException("SimulationTarotDrawUnavailable");

                var states = CreateCurrentO6ObjectStates();
                var calculator = new Simulation타로배치객체반응계산기();
                var reactions = draw.Offers
                    .Select(offer => calculator.Calculate(offer, states))
                    .ToArray();
                return new Simulation타로객체반응PreviewSnapshot
                {
                    PreviewStableId = "tarot-object-reaction-preview:"
                        + SessionStableId + ":" + draw.TurnNumber,
                    BaseRevision = Revision,
                    TurnNumber = draw.TurnNumber,
                    DrawStableId = draw.DrawStableId,
                    ObjectCatalogRevision =
                        Simulation타로배치객체반응계산기.ObjectCatalogRevision,
                    IsCandidateOnly = true,
                    DoesNotMutateSession = true,
                    CardReactions = reactions,
                    HighlightObjectStableIds = reactions
                        .SelectMany(value => value.HighlightObjectStableIds)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray(),
                    SourceStableIds = states
                        .SelectMany(value => value.StateSourceStableIds)
                        .Concat(new[]
                        {
                            ScenarioStableId,
                            draw.DrawStableId,
                            Simulation타로배치객체반응계산기.ObjectCatalogRevision,
                        })
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray(),
                };
            }
        }

        private Simulation타로객체상태Snapshot[] CreateCurrentO6ObjectStates()
        {
            var states = Simulation타로배치객체반응계산기
                .CreateEmptyO6ObjectStates()
                .ToDictionary(value => value.ObjectStableId, StringComparer.Ordinal);
            var allocations = harvestLotAllocations.Values
                .Where(value => value.AvailableQuantity > 0m
                    || value.OutboundReservedQuantity > 0m)
                .ToArray();
            var allocationSources = allocations.SelectMany(value => value.SourceStableIds)
                .Concat(allocations.Select(value => value.AllocationStableId));
            SetObjectState(states, SimulationO6배치객체StableIds.HarvestBox,
                allocations.Length > 0, allocationSources);
            SetObjectState(states, SimulationO6배치객체StableIds.FarmCrate,
                allocations.Length > 0, allocationSources);

            var freightSources = freightTransports.Values
                .SelectMany(value => value.SourceStableIds)
                .Concat(freightTransports.Keys);
            SetObjectState(states, SimulationO6배치객체StableIds.DeliveryTruck,
                freightTransports.Count > 0, freightSources);

            var logisticsSources = logisticsMovements.Values
                .SelectMany(value => value.SourceStableIds)
                .Concat(logisticsMovements.Keys)
                .Concat(freightSources);
            var hasLogisticsState = logisticsMovements.Count > 0
                || freightTransports.Count > 0;
            SetObjectState(states, SimulationO6배치객체StableIds.CargoPallet,
                hasLogisticsState, logisticsSources);
            SetObjectState(states, SimulationO6배치객체StableIds.HubGate,
                hasLogisticsState, logisticsSources);

            var marketSupplies = settlementInitialState?.MarketSupplyByProduct
                .Where(value => value.Quantity > 0m).ToArray()
                ?? Array.Empty<SimulationMarketSupplyRequest>();
            SetObjectState(states, SimulationO6배치객체StableIds.Market,
                marketSupplies.Length > 0,
                marketSupplies.SelectMany(value => value.SourceStableIds)
                    .Concat(marketSupplies.Select(value => value.ProductStableId)));

            var groupSources = groupOrders.Values
                .SelectMany(value => value.SourceStableIds)
                .Concat(groupOrders.Keys);
            SetObjectState(states, SimulationO6배치객체StableIds.GroupCart,
                groupOrders.Count > 0, groupSources);
            return states.Values
                .OrderBy(value => value.ObjectStableId, StringComparer.Ordinal)
                .ToArray();
        }

        private static void SetObjectState(
            IDictionary<string, Simulation타로객체상태Snapshot> states,
            string objectStableId,
            bool hasRelevantState,
            IEnumerable<string> sourceStableIds)
        {
            var state = states[objectStableId];
            state.HasRelevantState = hasRelevantState;
            state.StateSourceStableIds = hasRelevantState
                ? sourceStableIds.Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray()
                : Array.Empty<string>();
        }
    }
}
