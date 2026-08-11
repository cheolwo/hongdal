using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 주민소비수량EffectCode = "ResidentConsumptionQuantity";
        private const string 시장재고수렴EffectCode = "MarketSupplyReconciliation";
        private readonly Dictionary<string, Simulation시장소비Snapshot> marketConsumptions =
            new Dictionary<string, Simulation시장소비Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, 적용된시장소비Command> appliedMarketConsumptionCommands =
            new Dictionary<string, 적용된시장소비Command>(StringComparer.Ordinal);

        public Simulation시장소비PreviewSnapshot PreviewMarketConsumption(
            Simulation시장소비PreviewRequest request)
        {
            ValidateMarketConsumptionRequest(request);
            lock (gate) return CreateMarketConsumptionPreview(request);
        }

        public 경영SimulationSessionSnapshot ConfirmMarketConsumption(
            Simulation시장소비ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateMarketConsumptionRequest(request.Consumption);
            lock (gate)
            {
                var payloadKey = BuildMarketConsumptionPayloadKey(request.Consumption);
                if (appliedMarketConsumptionCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                var preview = CreateMarketConsumptionPreview(request.Consumption);
                var snapshot = ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateMarketConsumptionDecisionRequest(request.Consumption, preview),
                });
                appliedMarketConsumptionCommands.Add(request.CommandId,
                    new 적용된시장소비Command(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private Simulation시장소비PreviewSnapshot CreateMarketConsumptionPreview(
            Simulation시장소비PreviewRequest request)
        {
            if (!individualOrders.TryGetValue(request.OrderStableId.Trim(), out var order))
                throw new SimulationNotFoundException("SimulationIndividualOrderNotFound");
            var reservation = stockReservations.Values.SingleOrDefault(value =>
                value.OrderStableId == order.OrderStableId);
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException("SimulationSettlementRequiredForMarketConsumption");
            var supply = settlement.MarketSupplyByProduct.SingleOrDefault(value =>
                value.ProductStableId == order.ProductStableId && value.UnitCode == order.UnitCode);
            var blocks = new List<string>();
            if (order.Revision != request.OrderRevision)
                blocks.Add("SimulationIndividualOrderRevisionMismatch");
            if (!string.Equals(order.ActorStableId, request.ActorStableId.Trim(), StringComparison.Ordinal))
                blocks.Add("SimulationIndividualOrderActorMismatch");
            if (order.StateCode != SimulationIndividualOrderStateCodes.ReadyForPickup)
                blocks.Add("SimulationIndividualOrderNotReadyForConsumption");
            if (reservation == null
                || reservation.StateCode != SimulationStockReservationStateCodes.Consumed)
                blocks.Add("SimulationStockReservationNotConsumed");
            if (reservation != null && (reservation.Quantity != order.FulfilledQuantity
                || reservation.UnitCode != order.UnitCode))
                blocks.Add("SimulationMarketConsumptionQuantityImbalance");
            if (supply == null)
                blocks.Add("SimulationMarketSupplyNotFound");
            if (marketConsumptions.ContainsKey(request.ConsumptionStableId.Trim()))
                blocks.Add("SimulationMarketConsumptionStableIdConflict");
            if (marketConsumptions.Values.Any(value => value.OrderStableId == order.OrderStableId))
                blocks.Add("SimulationIndividualOrderAlreadyConsumed");
            var sources = MergeSources(request.SourceStableIds,
                MergeSources(order.SourceStableIds, reservation?.SourceStableIds ?? Array.Empty<string>()));
            var remaining = supply?.Quantity ?? 0m;
            var preview = new Simulation시장소비PreviewSnapshot
            {
                ConsumptionStableId = request.ConsumptionStableId.Trim(),
                OrderStableId = order.OrderStableId,
                ProductStableId = order.ProductStableId,
                MarketFacilityStableId = order.MarketFacilityStableId,
                ConsumptionQuantity = order.FulfilledQuantity,
                UnitCode = order.UnitCode,
                MarketSupplyAfterOrderFulfillment = remaining,
                MarketSupplyAfterConsumption = remaining,
                AdditionalMarketSupplyDeductionRequired = false,
                BlockReasonCodes = blocks.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
            };
            preview.CommonDecisionPreview = CreateDecisionPreview(
                CreateMarketConsumptionDecisionRequest(request, preview));
            return preview;
        }

        private static SimulationDecisionPreviewRequest CreateMarketConsumptionDecisionRequest(
            Simulation시장소비PreviewRequest request,
            Simulation시장소비PreviewSnapshot preview)
        {
            var sources = Copy(preview.SourceStableIds);
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:market-consumption:" + preview.ConsumptionStableId,
                DecisionTypeCode = Simulation시장소비DecisionTypeCodes.주민수령소비,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    preview.ConsumptionStableId,
                    preview.OrderStableId,
                    preview.ProductStableId,
                },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = 주민소비수량EffectCode,
                        TargetLedgerStableId = preview.ConsumptionStableId,
                        BeforeValue = 0m,
                        Delta = preview.ConsumptionQuantity,
                        AfterValue = preview.ConsumptionQuantity,
                        UnitCode = preview.UnitCode,
                        SourceStableIds = sources,
                    },
                    new SimulationValueProjection
                    {
                        ValueTypeCode = 시장재고수렴EffectCode,
                        TargetLedgerStableId = "market-supply:" + preview.ProductStableId,
                        BeforeValue = preview.MarketSupplyAfterOrderFulfillment,
                        Delta = 0m,
                        AfterValue = preview.MarketSupplyAfterConsumption,
                        UnitCode = preview.UnitCode,
                        SourceStableIds = sources,
                    },
                },
                Uncertainties = new[] { "MarketSupplyAlreadyDeductedAtOrderFulfillment" },
                BlockReasonCodes = Copy(preview.BlockReasonCodes),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:market-consumption:" + preview.ConsumptionStableId,
                    TaskTypeCode = "MarketResidentConsumption",
                    FacilityStableId = preview.MarketFacilityStableId,
                    AssignedCapacity = preview.ConsumptionQuantity,
                    AssignedCapacityUnitCode = preview.UnitCode,
                    DurationTicks = request.ConsumptionDurationTicks,
                    InputLotStableIds = new[] { "reservation:" + preview.OrderStableId },
                    OutputCandidateCodes = new[] { Simulation시장소비StateCodes.Consumed },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation시장소비Snapshot? PrepareMarketConsumption(
            SimulationDecisionPreviewRequest request)
        {
            if (!string.Equals(request.DecisionTypeCode,
                Simulation시장소비DecisionTypeCodes.주민수령소비, StringComparison.Ordinal)) return null;
            var consumptionId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("market-consumption:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationMarketConsumptionStableIdInvalid");
            var orderId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("order:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationIndividualOrderStableIdInvalid");
            if (!individualOrders.TryGetValue(orderId, out var order))
                throw new SimulationNotFoundException("SimulationIndividualOrderNotFound");
            if (order.StateCode != SimulationIndividualOrderStateCodes.ReadyForPickup)
                throw new SimulationConflictException("SimulationIndividualOrderNotReadyForConsumption");
            if (!string.Equals(order.ActorStableId, request.ActorStableId, StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationIndividualOrderActorMismatch");
            if (marketConsumptions.ContainsKey(consumptionId)
                || marketConsumptions.Values.Any(value => value.OrderStableId == orderId))
                throw new SimulationConflictException("SimulationIndividualOrderAlreadyConsumed");
            var reservation = stockReservations.Values.SingleOrDefault(value =>
                value.OrderStableId == orderId
                && value.StateCode == SimulationStockReservationStateCodes.Consumed)
                ?? throw new SimulationConflictException("SimulationStockReservationNotConsumed");
            var consumption = request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == 주민소비수량EffectCode);
            var reconciliation = request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == 시장재고수렴EffectCode);
            if (consumption.AfterValue != order.FulfilledQuantity
                || consumption.UnitCode != order.UnitCode
                || reservation.Quantity != order.FulfilledQuantity)
                throw new SimulationConflictException("SimulationMarketConsumptionQuantityImbalance");
            return new Simulation시장소비Snapshot
            {
                ConsumptionStableId = consumptionId,
                OrderStableId = order.OrderStableId,
                ReservationStableId = reservation.ReservationStableId,
                ActorStableId = order.ActorStableId,
                ProductStableId = order.ProductStableId,
                MarketFacilityStableId = order.MarketFacilityStableId,
                Quantity = consumption.AfterValue,
                UnitCode = consumption.UnitCode,
                StateCode = Simulation시장소비StateCodes.Scheduled,
                Revision = 1,
                ScheduledTick = CurrentTick,
                MarketSupplyAfterOrderFulfillment = reconciliation.BeforeValue,
                AdditionalMarketSupplyDeductionApplied = false,
                SourceStableIds = MergeSources(request.SourceStableIds, order.SourceStableIds),
            };
        }

        private void ScheduleMarketConsumption(
            Simulation시장소비Snapshot? consumption,
            SimulationDecisionSnapshot decision,
            SimulationTaskSnapshot task)
        {
            if (consumption == null) return;
            var order = individualOrders[consumption.OrderStableId];
            consumption.DecisionStableId = decision.DecisionStableId;
            consumption.TaskStableId = task.TaskStableId;
            marketConsumptions.Add(consumption.ConsumptionStableId, consumption);
            order.StateCode = SimulationIndividualOrderStateCodes.ConsumptionScheduled;
            order.ConsumptionDecisionStableId = decision.DecisionStableId;
            order.ConsumptionTaskStableId = task.TaskStableId;
            order.Revision++;
        }

        private void ApplyMarketConsumptionForTask(SimulationTaskSnapshot task, int appliedTick)
        {
            var consumption = marketConsumptions.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId
                && value.StateCode == Simulation시장소비StateCodes.Scheduled);
            if (consumption == null) return;
            var order = individualOrders[consumption.OrderStableId];
            var supply = settlementInitialState!.MarketSupplyByProduct.Single(value =>
                value.ProductStableId == consumption.ProductStableId
                && value.UnitCode == consumption.UnitCode);
            if (supply.Quantity < 0m)
                throw new SimulationConflictException("SimulationMarketSupplyConservationInvalid");
            consumption.StateCode = Simulation시장소비StateCodes.Consumed;
            consumption.ConsumedTick = appliedTick;
            consumption.MarketSupplyObservedAtConsumption = supply.Quantity;
            consumption.Revision++;
            order.StateCode = SimulationIndividualOrderStateCodes.Consumed;
            order.ConsumedTick = appliedTick;
            order.Revision++;
        }

        private Simulation시장소비Snapshot[] CreateMarketConsumptionSnapshots()
            => marketConsumptions.Values
                .OrderBy(value => value.ConsumptionStableId, StringComparer.Ordinal)
                .Select(CloneMarketConsumption).ToArray();

        private SimulationResidentConsumptionSummarySnapshot[] CreateResidentConsumptionSummaries()
            => marketConsumptions.Values
                .Where(value => value.StateCode == Simulation시장소비StateCodes.Consumed)
                .GroupBy(value => new { value.ProductStableId, value.UnitCode })
                .OrderBy(group => group.Key.ProductStableId, StringComparer.Ordinal)
                .ThenBy(group => group.Key.UnitCode, StringComparer.Ordinal)
                .Select(group => new SimulationResidentConsumptionSummarySnapshot
                {
                    ProductStableId = group.Key.ProductStableId,
                    Quantity = group.Sum(value => value.Quantity),
                    UnitCode = group.Key.UnitCode,
                    ConsumptionCount = group.Count(),
                    SourceStableIds = NormalizeIds(group.SelectMany(value => value.SourceStableIds).ToArray()),
                }).ToArray();

        internal static Simulation시장소비Snapshot CloneMarketConsumption(
            Simulation시장소비Snapshot source)
            => new Simulation시장소비Snapshot
            {
                ConsumptionStableId = source.ConsumptionStableId,
                OrderStableId = source.OrderStableId,
                ReservationStableId = source.ReservationStableId,
                ActorStableId = source.ActorStableId,
                ProductStableId = source.ProductStableId,
                MarketFacilityStableId = source.MarketFacilityStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                StateCode = source.StateCode,
                Revision = source.Revision,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                ScheduledTick = source.ScheduledTick,
                ConsumedTick = source.ConsumedTick,
                MarketSupplyAfterOrderFulfillment = source.MarketSupplyAfterOrderFulfillment,
                MarketSupplyObservedAtConsumption = source.MarketSupplyObservedAtConsumption,
                AdditionalMarketSupplyDeductionApplied = source.AdditionalMarketSupplyDeductionApplied,
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static void ValidateMarketConsumptionRequest(Simulation시장소비PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.ConsumptionStableId, "SimulationMarketConsumptionStableIdInvalid");
            if (!request.ConsumptionStableId.Trim().StartsWith("market-consumption:", StringComparison.Ordinal))
                throw new SimulationContractException("SimulationMarketConsumptionStableIdInvalid");
            RequireStableId(request.OrderStableId, "SimulationIndividualOrderStableIdInvalid");
            if (!request.OrderStableId.Trim().StartsWith("order:", StringComparison.Ordinal))
                throw new SimulationContractException("SimulationIndividualOrderStableIdInvalid");
            if (request.OrderRevision <= 0)
                throw new SimulationContractException("SimulationIndividualOrderRevisionInvalid");
            RequireStableId(request.ActorStableId, "SimulationActorStableIdInvalid");
            if (request.ConsumptionDurationTicks <= 0 || request.ConsumptionDurationTicks > 7)
                throw new SimulationContractException("SimulationMarketConsumptionDurationInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationMarketConsumptionSourceStableIdsInvalid");
        }

        private static string BuildMarketConsumptionPayloadKey(Simulation시장소비PreviewRequest value)
            => string.Join("\u001e", value.ConsumptionStableId.Trim(), value.OrderStableId.Trim(),
                value.OrderRevision.ToString(CultureInfo.InvariantCulture), value.ActorStableId.Trim(),
                value.ConsumptionDurationTicks.ToString(CultureInfo.InvariantCulture),
                string.Join("\u001f", value.SourceStableIds.OrderBy(source => source, StringComparer.Ordinal)));

        private sealed class 적용된시장소비Command
        {
            public 적용된시장소비Command(string payloadKey, 경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }
            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
