using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 시장재고수량EffectCode = "MarketSupplyQuantity";
        private const string 주문금액CostCode = "IndividualOrderTotalPrice";
        private const string 필요노동CostCode = "RequiredLabor";
        private const string 주문취소DecisionTypeCode = "IndividualOrderCancellation";
        private const string 재고예약해제EffectCode = "StockReservationRelease";
        private readonly Dictionary<string, SimulationIndividualOrderSnapshot> individualOrders =
            new Dictionary<string, SimulationIndividualOrderSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationStockReservationSnapshot> stockReservations =
            new Dictionary<string, SimulationStockReservationSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, 적용된IndividualOrderCommand> appliedIndividualOrderCommands =
            new Dictionary<string, 적용된IndividualOrderCommand>(StringComparer.Ordinal);

        public SimulationIndividualOrderPreviewSnapshot PreviewIndividualOrder(
            SimulationIndividualOrderPreviewRequest request)
        {
            ValidateIndividualOrderPreviewRequest(request);
            lock (gate)
            {
                return CreateIndividualOrderPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmIndividualOrder(
            SimulationIndividualOrderConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateIndividualOrderPreviewRequest(request.Order);
            lock (gate)
            {
                var payloadKey = BuildIndividualOrderPayloadKey(request.Order);
                if (appliedIndividualOrderCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }

                var preview = CreateIndividualOrderPreview(request.Order);
                var snapshot = ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = ToDecisionPreviewRequest(request.Order, preview),
                });
                appliedIndividualOrderCommands.Add(
                    request.CommandId,
                    new 적용된IndividualOrderCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        public SimulationDecisionPreviewSnapshot PreviewIndividualOrderCancellation(
            SimulationIndividualOrderCancelRequest request)
        {
            ValidateIndividualOrderCancelRequest(request);
            lock (gate)
            {
                return CreateDecisionPreview(CreateIndividualOrderCancellationPreview(request));
            }
        }

        public 경영SimulationSessionSnapshot ConfirmIndividualOrderCancellation(
            SimulationIndividualOrderCancelRequest request)
        {
            ValidateIndividualOrderCancelRequest(request);
            lock (gate)
            {
                var payloadKey = "cancel\u001f" + BuildIndividualOrderCancelPayloadKey(request);
                if (appliedIndividualOrderCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }

                var snapshot = ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateIndividualOrderCancellationPreview(request),
                });
                appliedIndividualOrderCommands.Add(
                    request.CommandId,
                    new 적용된IndividualOrderCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private SimulationIndividualOrderPreviewSnapshot CreateIndividualOrderPreview(
            SimulationIndividualOrderPreviewRequest request)
        {
            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException("SimulationSettlementRequiredForIndividualOrder");
            var source = settlement.MarketSupplyByProduct.FirstOrDefault(value =>
                string.Equals(value.ProductStableId, request.ProductStableId.Trim(), StringComparison.Ordinal)
                && string.Equals(value.UnitCode, request.UnitCode.Trim(), StringComparison.Ordinal));
            var alreadyReserved = stockReservations.Values
                .Where(value => value.StateCode == SimulationStockReservationStateCodes.Reserved)
                .Where(value => string.Equals(
                    value.ProductStableId,
                    request.ProductStableId.Trim(),
                    StringComparison.Ordinal))
                .Where(value => string.Equals(value.UnitCode, request.UnitCode.Trim(), StringComparison.Ordinal))
                .Sum(value => value.Quantity);
            var available = (source?.Quantity ?? 0m) - alreadyReserved;
            var blockReasons = new List<string>();
            if (!settlement.Facilities.Any(value =>
                string.Equals(value.FacilityStableId, request.MarketFacilityStableId.Trim(), StringComparison.Ordinal)
                && string.Equals(value.FacilityTypeCode, SimulationSettlementFacilityTypeCodes.Market, StringComparison.Ordinal)))
            {
                blockReasons.Add("SimulationMarketFacilityNotFound");
            }
            if (source == null)
                blockReasons.Add("SimulationMarketSupplyNotFound");
            else if (available < request.Quantity)
                blockReasons.Add("SimulationMarketSupplyInsufficient");
            if (settlement.LaborAvailable < request.RequiredLabor)
                blockReasons.Add("SimulationSettlementLaborCapacityExceeded");
            if (individualOrders.ContainsKey(request.OrderStableId.Trim()))
                blockReasons.Add("SimulationIndividualOrderStableIdConflict");

            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.개별주문);
            var sources = MergeSources(request.SourceStableIds, workflow.SourceStableIds);
            var totalPrice = request.Quantity * request.UnitPrice;
            var snapshot = new SimulationIndividualOrderPreviewSnapshot
            {
                OrderStableId = request.OrderStableId.Trim(),
                ProductStableId = request.ProductStableId.Trim(),
                RequestedQuantity = request.Quantity,
                AvailableBeforeReservation = available,
                AvailableAfterReservation = available - request.Quantity,
                UnitCode = request.UnitCode.Trim(),
                TotalPrice = totalPrice,
                CurrencyCode = request.CurrencyCode.Trim(),
                RequiredLabor = request.RequiredLabor,
                LaborAvailableBeforeReservation = settlement.LaborAvailable,
                LaborAvailableAfterReservation = settlement.LaborAvailable - request.RequiredLabor,
                FulfillmentDurationTicks = request.FulfillmentDurationTicks,
                RuleRevision = workflow.RuleRevision,
                BlockReasonCodes = blockReasons.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
            };
            snapshot.CommonDecisionPreview = CreateDecisionPreview(
                ToDecisionPreviewRequest(request, snapshot));
            return snapshot;
        }

        private static SimulationDecisionPreviewRequest ToDecisionPreviewRequest(
            SimulationIndividualOrderPreviewRequest request,
            SimulationIndividualOrderPreviewSnapshot preview)
        {
            var orderId = request.OrderStableId.Trim();
            var productId = request.ProductStableId.Trim();
            var sources = Copy(preview.SourceStableIds);
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:" + orderId,
                DecisionTypeCode = SimulationIndividualOrderDecisionTypeCodes.IndividualOrder,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { orderId, productId }
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                ExpectedCosts = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = 주문금액CostCode,
                        TargetLedgerStableId = "simulation-budget:" + request.ActorStableId.Trim(),
                        BeforeValue = preview.TotalPrice,
                        Delta = -preview.TotalPrice,
                        AfterValue = 0m,
                        UnitCode = request.CurrencyCode.Trim(),
                        SourceStableIds = sources,
                    },
                    new SimulationValueProjection
                    {
                        ValueTypeCode = 필요노동CostCode,
                        TargetLedgerStableId = "settlement-labor:" + request.MarketFacilityStableId.Trim(),
                        BeforeValue = preview.LaborAvailableBeforeReservation,
                        Delta = -preview.RequiredLabor,
                        AfterValue = preview.LaborAvailableAfterReservation,
                        UnitCode = "labor-unit",
                        SourceStableIds = sources,
                    },
                },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = 시장재고수량EffectCode,
                        TargetLedgerStableId = "market-supply:" + productId,
                        BeforeValue = preview.AvailableBeforeReservation,
                        Delta = -request.Quantity,
                        AfterValue = preview.AvailableAfterReservation,
                        UnitCode = request.UnitCode.Trim(),
                        SourceStableIds = sources,
                    },
                },
                BlockReasonCodes = Copy(preview.BlockReasonCodes),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:" + orderId + ":pick-pack",
                    TaskTypeCode = "IndividualOrderPickPack",
                    FacilityStableId = request.MarketFacilityStableId.Trim(),
                    AssignedCapacity = request.Quantity,
                    AssignedCapacityUnitCode = request.UnitCode.Trim(),
                    DurationTicks = request.FulfillmentDurationTicks,
                    InputLotStableIds = new[] { "market-supply:" + productId },
                    OutputCandidateCodes = new[] { SimulationIndividualOrderStateCodes.ReadyForPickup },
                    SourceStableIds = sources,
                },
            };
        }

        private SimulationIndividualOrderSnapshot? PrepareIndividualOrderReservation(
            SimulationDecisionPreviewRequest request,
            SimulationDecisionPreviewSnapshot preview)
        {
            if (!string.Equals(
                request.DecisionTypeCode,
                SimulationIndividualOrderDecisionTypeCodes.IndividualOrder,
                StringComparison.Ordinal))
            {
                return null;
            }

            var stockEffect = request.ExpectedEffects.SingleOrDefault(value =>
                string.Equals(value.ValueTypeCode, 시장재고수량EffectCode, StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationIndividualOrderMarketEffectMissing");
            var price = request.ExpectedCosts.SingleOrDefault(value =>
                string.Equals(value.ValueTypeCode, 주문금액CostCode, StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationIndividualOrderPriceCostMissing");
            var labor = request.ExpectedCosts.SingleOrDefault(value =>
                string.Equals(value.ValueTypeCode, 필요노동CostCode, StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationIndividualOrderLaborCostMissing");
            var orderId = request.TargetStableIds.SingleOrDefault(value => value.StartsWith("order:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationIndividualOrderStableIdInvalid");
            var productId = request.TargetStableIds.SingleOrDefault(value => value.StartsWith("product:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationIndividualOrderProductStableIdInvalid");
            if (individualOrders.ContainsKey(orderId))
                throw new SimulationConflictException("SimulationIndividualOrderStableIdConflict");

            var settlement = CreateSettlementSnapshot()
                ?? throw new SimulationContractException("SimulationSettlementRequiredForIndividualOrder");
            var supply = settlementInitialState!.MarketSupplyByProduct.SingleOrDefault(value =>
                string.Equals(value.ProductStableId, productId, StringComparison.Ordinal)
                && string.Equals(value.UnitCode, stockEffect.UnitCode, StringComparison.Ordinal))
                ?? throw new SimulationConflictException("SimulationMarketSupplyNotFound");
            var reserved = stockReservations.Values
                .Where(value => value.StateCode == SimulationStockReservationStateCodes.Reserved)
                .Where(value => string.Equals(value.ProductStableId, productId, StringComparison.Ordinal))
                .Where(value => string.Equals(value.UnitCode, stockEffect.UnitCode, StringComparison.Ordinal))
                .Sum(value => value.Quantity);
            if (supply.Quantity - reserved < -stockEffect.Delta)
                throw new SimulationConflictException("SimulationMarketSupplyInsufficient");
            if (settlement.LaborAvailable < -labor.Delta)
                throw new SimulationConflictException("SimulationSettlementLaborCapacityExceeded");

            return new SimulationIndividualOrderSnapshot
            {
                OrderStableId = orderId,
                StateCode = SimulationIndividualOrderStateCodes.StockReserved,
                Revision = 1,
                ActorStableId = request.ActorStableId,
                ProductStableId = productId,
                MarketFacilityStableId = preview.TaskPlan.FacilityStableId,
                OrderedQuantity = -stockEffect.Delta,
                UnitCode = stockEffect.UnitCode,
                TotalPrice = -price.Delta,
                CurrencyCode = price.UnitCode,
                RequiredLabor = -labor.Delta,
                DecisionStableId = preview.Decision.DecisionStableId,
                TaskStableId = preview.TaskPlan.TaskStableId,
                ReservedTick = CurrentTick,
                ConfirmedTick = CurrentTick,
                StockReservedTick = CurrentTick,
                SourceStableIds = MergeSources(request.SourceStableIds, stockEffect.SourceStableIds),
            };
        }

        private SimulationIndividualOrderSnapshot? PrepareIndividualOrderCancellation(
            SimulationDecisionPreviewRequest request)
        {
            if (!string.Equals(request.DecisionTypeCode, 주문취소DecisionTypeCode, StringComparison.Ordinal))
                return null;
            var orderId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("order:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationIndividualOrderStableIdInvalid");
            if (!individualOrders.TryGetValue(orderId, out var order))
                throw new SimulationConflictException("SimulationIndividualOrderNotFound");
            if (!string.Equals(order.ActorStableId, request.ActorStableId, StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationIndividualOrderActorMismatch");
            if (order.StateCode != SimulationIndividualOrderStateCodes.StockReserved
                && order.StateCode != SimulationIndividualOrderStateCodes.Picking
                && order.StateCode != SimulationIndividualOrderStateCodes.Packed)
                throw new SimulationConflictException("SimulationIndividualOrderCancellationNotAllowed");
            return order;
        }

        private void ReserveIndividualOrder(SimulationIndividualOrderSnapshot? order)
        {
            if (order == null) return;
            var reservation = new SimulationStockReservationSnapshot
            {
                ReservationStableId = "reservation:" + order.OrderStableId,
                OrderStableId = order.OrderStableId,
                MarketFacilityStableId = order.MarketFacilityStableId,
                ProductStableId = order.ProductStableId,
                Quantity = order.OrderedQuantity,
                UnitCode = order.UnitCode,
                StateCode = SimulationStockReservationStateCodes.Reserved,
                ReservedTick = CurrentTick,
                SourceStableIds = Copy(order.SourceStableIds),
            };
            individualOrders.Add(order.OrderStableId, order);
            stockReservations.Add(reservation.ReservationStableId, reservation);
            settlementInitialState!.LaborReserved += order.RequiredLabor;
        }

        private void ScheduleIndividualOrderCancellation(
            SimulationIndividualOrderSnapshot? order,
            SimulationTaskSnapshot task)
        {
            if (order == null) return;
            order.StateCode = SimulationIndividualOrderStateCodes.CancellationScheduled;
            order.CancellationTaskStableId = task.TaskStableId;
            order.Revision++;
        }

        private void ApplyIndividualOrderCancellationForTask(
            SimulationTaskSnapshot task,
            int appliedTick)
        {
            var order = individualOrders.Values.FirstOrDefault(value =>
                value.CancellationTaskStableId == task.TaskStableId
                && value.StateCode == SimulationIndividualOrderStateCodes.CancellationScheduled);
            if (order == null) return;
            var reservation = stockReservations.Values.Single(value =>
                value.OrderStableId == order.OrderStableId
                && value.StateCode == SimulationStockReservationStateCodes.Reserved);
            settlementInitialState!.LaborReserved -= order.RequiredLabor;
            reservation.StateCode = SimulationStockReservationStateCodes.Released;
            reservation.ReleasedTick = appliedTick;
            order.StateCode = SimulationIndividualOrderStateCodes.Cancelled;
            order.CancelledTick = appliedTick;
            order.Revision++;

            if (tasks.TryGetValue(order.TaskStableId, out var fulfillmentTask)
                && fulfillmentTask.StateCode != SimulationTaskStateCodes.Completed)
            {
                fulfillmentTask.StateCode = SimulationTaskStateCodes.Cancelled;
                fulfillmentTask.Revision++;
                fulfillmentTask.ActualEndTick = appliedTick;
                foreach (var effect in effects.Values.Where(value =>
                    value.CausedByTaskStableId == fulfillmentTask.TaskStableId
                    && value.StateCode == SimulationEffectStateCodes.Pending))
                {
                    effect.StateCode = SimulationEffectStateCodes.Cancelled;
                    effect.Revision++;
                }
            }
        }

        private void ApplyIndividualOrderForTask(SimulationTaskSnapshot task, int appliedTick)
        {
            var order = individualOrders.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId
                && (value.StateCode == SimulationIndividualOrderStateCodes.StockReserved
                    || value.StateCode == SimulationIndividualOrderStateCodes.Picking
                    || value.StateCode == SimulationIndividualOrderStateCodes.Packed));
            if (order == null) return;
            if (order.PickedTick == null) order.PickedTick = appliedTick;
            if (order.PackedTick == null) order.PackedTick = appliedTick;
            var reservation = stockReservations.Values.Single(value =>
                value.OrderStableId == order.OrderStableId
                && value.StateCode == SimulationStockReservationStateCodes.Reserved);
            var supply = settlementInitialState!.MarketSupplyByProduct.Single(value =>
                value.ProductStableId == order.ProductStableId
                && value.UnitCode == order.UnitCode);
            if (supply.Quantity < reservation.Quantity)
                throw new SimulationConflictException("SimulationMarketSupplyConservationInvalid");

            supply.Quantity -= reservation.Quantity;
            settlementInitialState.LaborReserved -= order.RequiredLabor;
            order.StateCode = SimulationIndividualOrderStateCodes.ReadyForPickup;
            order.FulfilledQuantity = order.OrderedQuantity;
            order.ReadyForPickupTick = appliedTick;
            order.Revision++;
            reservation.StateCode = SimulationStockReservationStateCodes.Consumed;
            reservation.ConsumedTick = appliedTick;
        }

        private void AdvanceIndividualOrderFulfillmentForTask(
            SimulationTaskSnapshot task,
            int currentTick)
        {
            var order = individualOrders.Values.FirstOrDefault(value =>
                value.TaskStableId == task.TaskStableId
                && (value.StateCode == SimulationIndividualOrderStateCodes.StockReserved
                    || value.StateCode == SimulationIndividualOrderStateCodes.Picking
                    || value.StateCode == SimulationIndividualOrderStateCodes.Packed));
            if (order == null || currentTick < task.ScheduledStartTick) return;
            if (order.PickedTick == null)
            {
                order.StateCode = SimulationIndividualOrderStateCodes.Picking;
                order.PickedTick = currentTick;
                order.Revision++;
            }
            if (currentTick >= Math.Max(task.ScheduledStartTick, task.ExpectedEndTick - 1)
                && order.PackedTick == null)
            {
                order.StateCode = SimulationIndividualOrderStateCodes.Packed;
                order.PackedTick = currentTick;
                order.Revision++;
            }
        }

        private SimulationIndividualOrderSnapshot[] CreateIndividualOrderSnapshots()
            => individualOrders.Values
                .OrderBy(value => value.OrderStableId, StringComparer.Ordinal)
                .Select(CloneIndividualOrder)
                .ToArray();

        private SimulationStockReservationSnapshot[] CreateStockReservationSnapshots()
            => stockReservations.Values
                .OrderBy(value => value.ReservationStableId, StringComparer.Ordinal)
                .Select(CloneStockReservation)
                .ToArray();

        internal static SimulationIndividualOrderSnapshot CloneIndividualOrder(
            SimulationIndividualOrderSnapshot source)
            => new SimulationIndividualOrderSnapshot
            {
                OrderStableId = source.OrderStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                ActorStableId = source.ActorStableId,
                ProductStableId = source.ProductStableId,
                MarketFacilityStableId = source.MarketFacilityStableId,
                OrderedQuantity = source.OrderedQuantity,
                FulfilledQuantity = source.FulfilledQuantity,
                UnitCode = source.UnitCode,
                TotalPrice = source.TotalPrice,
                CurrencyCode = source.CurrencyCode,
                RequiredLabor = source.RequiredLabor,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                CancellationTaskStableId = source.CancellationTaskStableId,
                ReservedTick = source.ReservedTick,
                ConfirmedTick = source.ConfirmedTick,
                StockReservedTick = source.StockReservedTick,
                PickedTick = source.PickedTick,
                PackedTick = source.PackedTick,
                ReadyForPickupTick = source.ReadyForPickupTick,
                PickupDecisionStableId = source.PickupDecisionStableId,
                PickupTaskStableId = source.PickupTaskStableId,
                FulfilledTick = source.FulfilledTick,
                ConsumptionDecisionStableId = source.ConsumptionDecisionStableId,
                ConsumptionTaskStableId = source.ConsumptionTaskStableId,
                ConsumedTick = source.ConsumedTick,
                CancelledTick = source.CancelledTick,
                SourceStableIds = Copy(source.SourceStableIds),
            };

        internal static SimulationStockReservationSnapshot CloneStockReservation(
            SimulationStockReservationSnapshot source)
            => new SimulationStockReservationSnapshot
            {
                ReservationStableId = source.ReservationStableId,
                OrderStableId = source.OrderStableId,
                MarketFacilityStableId = source.MarketFacilityStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                StateCode = source.StateCode,
                ReservedTick = source.ReservedTick,
                ConsumedTick = source.ConsumedTick,
                ReleasedTick = source.ReleasedTick,
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static void ValidateIndividualOrderPreviewRequest(
            SimulationIndividualOrderPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.OrderStableId, "SimulationIndividualOrderStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationIndividualOrderActorStableIdInvalid");
            RequireStableId(request.ProductStableId, "SimulationIndividualOrderProductStableIdInvalid");
            RequireStableId(request.MarketFacilityStableId, "SimulationIndividualOrderMarketFacilityStableIdInvalid");
            if (request.Quantity <= 0m)
                throw new SimulationContractException("SimulationIndividualOrderQuantityInvalid");
            RequireStableId(request.UnitCode, "SimulationIndividualOrderUnitCodeInvalid");
            if (request.UnitPrice < 0m)
                throw new SimulationContractException("SimulationIndividualOrderUnitPriceInvalid");
            RequireStableId(request.CurrencyCode, "SimulationIndividualOrderCurrencyCodeInvalid");
            if (request.RequiredLabor < 0m)
                throw new SimulationContractException("SimulationIndividualOrderRequiredLaborInvalid");
            if (request.FulfillmentDurationTicks <= 0 || request.FulfillmentDurationTicks > 28)
                throw new SimulationContractException("SimulationIndividualOrderDurationTicksInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationIndividualOrderSourceStableIdsInvalid");
        }

        private SimulationDecisionPreviewRequest CreateIndividualOrderCancellationPreview(
            SimulationIndividualOrderCancelRequest request)
        {
            var orderId = request.OrderStableId.Trim();
            var blockReasons = new List<string>();
            individualOrders.TryGetValue(orderId, out var order);
            if (order == null)
                blockReasons.Add("SimulationIndividualOrderNotFound");
            else
            {
                if (!string.Equals(order.ActorStableId, request.ActorStableId.Trim(), StringComparison.Ordinal))
                    blockReasons.Add("SimulationIndividualOrderActorMismatch");
                if (order.StateCode != SimulationIndividualOrderStateCodes.StockReserved)
                    blockReasons.Add("SimulationIndividualOrderCancellationNotAllowed");
            }

            var quantity = order?.OrderedQuantity ?? 1m;
            var unit = order?.UnitCode ?? "unit";
            var sources = MergeSources(
                MergeSources(request.SourceStableIds, new[] { "reason:" + request.ReasonCode.Trim() }),
                order?.SourceStableIds ?? Array.Empty<string>());
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:" + orderId + ":cancel",
                DecisionTypeCode = 주문취소DecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { orderId },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = 재고예약해제EffectCode,
                        TargetLedgerStableId = "reservation:" + orderId,
                        BeforeValue = quantity,
                        Delta = -quantity,
                        AfterValue = 0m,
                        UnitCode = unit,
                        SourceStableIds = sources,
                    },
                },
                BlockReasonCodes = blockReasons.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:" + orderId + ":00-cancel",
                    TaskTypeCode = "IndividualOrderCancellation",
                    FacilityStableId = order?.MarketFacilityStableId ?? "facility:unknown",
                    AssignedCapacity = quantity,
                    AssignedCapacityUnitCode = unit,
                    DurationTicks = 1,
                    InputLotStableIds = new[] { "reservation:" + orderId },
                    OutputCandidateCodes = new[] { SimulationIndividualOrderStateCodes.Cancelled },
                    SourceStableIds = sources,
                },
            };
        }

        private static void ValidateIndividualOrderCancelRequest(
            SimulationIndividualOrderCancelRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.OrderStableId, "SimulationIndividualOrderStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationIndividualOrderActorStableIdInvalid");
            RequireStableId(request.ReasonCode, "SimulationIndividualOrderCancellationReasonCodeInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationIndividualOrderSourceStableIdsInvalid");
        }

        private static string BuildIndividualOrderPayloadKey(
            SimulationIndividualOrderPreviewRequest request)
        {
            return string.Join("\u001f", new[]
            {
                request.OrderStableId.Trim(),
                request.ActorStableId.Trim(),
                request.ProductStableId.Trim(),
                request.MarketFacilityStableId.Trim(),
                request.Quantity.ToString(CultureInfo.InvariantCulture),
                request.UnitCode.Trim(),
                request.UnitPrice.ToString(CultureInfo.InvariantCulture),
                request.CurrencyCode.Trim(),
                request.RequiredLabor.ToString(CultureInfo.InvariantCulture),
                request.FulfillmentDurationTicks.ToString(CultureInfo.InvariantCulture),
                string.Join("\u001e", NormalizeIds(request.SourceStableIds)),
            });
        }

        private static string BuildIndividualOrderCancelPayloadKey(
            SimulationIndividualOrderCancelRequest request)
            => string.Join("\u001f", new[]
            {
                request.OrderStableId.Trim(),
                request.ActorStableId.Trim(),
                request.ReasonCode.Trim(),
                string.Join("\u001e", NormalizeIds(request.SourceStableIds)),
            });

        private sealed class 적용된IndividualOrderCommand
        {
            public 적용된IndividualOrderCommand(
                string payloadKey,
                경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
