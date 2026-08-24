using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation소비자원효과규칙
    {
        private const string RuleStableId = "rule:market-resident-consumption.resource.v1";
        private readonly Simulation자원효과묶음Validator validator;

        public Simulation소비자원효과규칙()
            : this(new Simulation자원효과묶음Validator())
        {
        }

        public Simulation소비자원효과규칙(Simulation자원효과묶음Validator value)
        {
            validator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Simulation소비흐름자원효과Result CreateReservation(
            Simulation주문예약자원효과Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateOrderAndReservation(request.Order, request.Reservation);
            if (request.Order.StateCode != SimulationIndividualOrderStateCodes.StockReserved
                || request.Reservation.StateCode != SimulationStockReservationStateCodes.Reserved)
            {
                throw new SimulationContractException("SimulationConsumptionReservationStateInvalid");
            }
            if (request.AvailableBeforeReservation < request.Reservation.Quantity)
                throw new SimulationContractException("SimulationConsumptionAvailableSupplyInsufficient");
            ValidateCommonIds(request.EffectBundleStableId, request.AvailableEffectLineStableId,
                request.ReservedEffectLineStableId, request.AvailableLedgerStableId,
                request.ReservedLedgerStableId);

            var quantity = request.Reservation.Quantity;
            var sources = Sources(request.SourceStableIds, request.Order.SourceStableIds,
                request.Reservation.SourceStableIds, request.Order.OrderStableId,
                request.Reservation.ReservationStableId);
            var conservation = "conservation:order-reservation:" + request.Order.OrderStableId;
            var bundle = Bundle(request.EffectBundleStableId, request.Order.DecisionStableId,
                request.Order.TaskStableId, Simulation업무규칙영역Codes.Market, sources,
                Line(request.AvailableEffectLineStableId, Simulation자원변동유형Codes.Reservation,
                    Simulation자원효과역할Codes.Available, "MarketAvailableStock",
                    request.AvailableLedgerStableId, request.Order, null,
                    request.AvailableBeforeReservation, -quantity,
                    conservation, -quantity, sources),
                Line(request.ReservedEffectLineStableId, Simulation자원변동유형Codes.Reservation,
                    Simulation자원효과역할Codes.Reserved, "MarketReservedStock",
                    request.ReservedLedgerStableId, request.Order, null,
                    0m, quantity, conservation, quantity, sources));
            return Result("Reservation", quantity, request.Order.UnitCode, bundle);
        }

        public Simulation소비흐름자원효과Result CreateFulfillment(
            Simulation주문이행자원효과Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateOrderAndReservation(request.Order, request.Reservation);
            if (request.Order.StateCode != SimulationIndividualOrderStateCodes.ReadyForPickup
                || request.Reservation.StateCode != SimulationStockReservationStateCodes.Consumed
                || request.Order.FulfilledQuantity != request.Order.OrderedQuantity
                || !request.Order.ReadyForPickupTick.HasValue)
            {
                throw new SimulationContractException("SimulationConsumptionFulfillmentStateInvalid");
            }
            ValidateCommonIds(request.EffectBundleStableId, request.ReservedEffectLineStableId,
                request.ResidentReceivedEffectLineStableId, request.ReservedLedgerStableId,
                request.ResidentReceivedLedgerStableId);

            var quantity = request.Reservation.Quantity;
            var sources = Sources(request.SourceStableIds, request.Order.SourceStableIds,
                request.Reservation.SourceStableIds, request.Order.OrderStableId,
                request.Reservation.ReservationStableId);
            var conservation = "conservation:order-fulfillment:" + request.Order.OrderStableId;
            var bundle = Bundle(request.EffectBundleStableId, request.Order.DecisionStableId,
                request.Order.TaskStableId, Simulation업무규칙영역Codes.Market, sources,
                Line(request.ReservedEffectLineStableId, Simulation자원변동유형Codes.Transformation,
                    Simulation자원효과역할Codes.Input, "MarketReservedStock",
                    request.ReservedLedgerStableId, request.Order, null,
                    quantity, -quantity, conservation, -quantity, sources),
                Line(request.ResidentReceivedEffectLineStableId,
                    Simulation자원변동유형Codes.Transformation,
                    Simulation자원효과역할Codes.Output, "ResidentReceivedStock",
                    request.ResidentReceivedLedgerStableId, request.Order,
                    request.Reservation.ReservationStableId,
                    request.ResidentReceivedBeforeFulfillment, quantity,
                    conservation, quantity, sources));
            return Result("Fulfillment", quantity, request.Order.UnitCode, bundle);
        }

        public Simulation소비흐름자원효과Result CreateConsumption(
            Simulation주민소비자원효과Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateOrderAndConsumption(request.Order, request.Consumption);
            if (request.Order.StateCode != SimulationIndividualOrderStateCodes.Consumed
                || request.Consumption.StateCode != Simulation시장소비StateCodes.Consumed
                || !request.Order.ConsumedTick.HasValue
                || request.Order.ConsumedTick != request.Consumption.ConsumedTick
                || request.Consumption.AdditionalMarketSupplyDeductionApplied
                || request.Consumption.MarketSupplyObservedAtConsumption
                    != request.Consumption.MarketSupplyAfterOrderFulfillment)
            {
                throw new SimulationContractException("SimulationConsumptionCompletionStateInvalid");
            }
            if (request.ResidentReceivedBeforeConsumption < request.Consumption.Quantity
                || request.ConsumptionRecordBefore < 0m)
            {
                throw new SimulationContractException("SimulationConsumptionLedgerValueInvalid");
            }
            ValidateCommonIds(request.EffectBundleStableId,
                request.ResidentReceivedEffectLineStableId,
                request.ConsumptionRecordEffectLineStableId,
                request.ResidentReceivedLedgerStableId,
                request.ConsumptionRecordLedgerStableId);

            var quantity = request.Consumption.Quantity;
            var sources = Sources(request.SourceStableIds, request.Order.SourceStableIds,
                request.Consumption.SourceStableIds, request.Order.OrderStableId,
                request.Consumption.ConsumptionStableId);
            var bundle = Bundle(request.EffectBundleStableId,
                request.Consumption.DecisionStableId, request.Consumption.TaskStableId,
                Simulation업무규칙영역Codes.Consumption, sources,
                Line(request.ResidentReceivedEffectLineStableId,
                    Simulation자원변동유형Codes.Consumption,
                    Simulation자원효과역할Codes.Input, "ResidentReceivedStock",
                    request.ResidentReceivedLedgerStableId, request.Order,
                    request.Consumption.ReservationStableId,
                    request.ResidentReceivedBeforeConsumption, -quantity, null, 0m, sources),
                Line(request.ConsumptionRecordEffectLineStableId,
                    Simulation자원변동유형Codes.Consumption,
                    Simulation자원효과역할Codes.Record, "ResidentConsumptionCumulative",
                    request.ConsumptionRecordLedgerStableId, request.Order,
                    null,
                    request.ConsumptionRecordBefore, quantity, null, 0m, sources));
            return Result("Consumption", quantity, request.Order.UnitCode, bundle);
        }

        private Simulation자원효과묶음Snapshot Bundle(
            string bundleId,
            string decisionId,
            string taskId,
            string domainCode,
            string[] sources,
            params Simulation자원효과선Snapshot[] lines)
        {
            var bundle = new Simulation자원효과묶음Snapshot
            {
                EffectBundleStableId = bundleId.Trim(),
                RuleStableId = RuleStableId,
                RuleRevision = 1,
                RuleDomainCode = domainCode,
                ModeCode = "Simulation",
                StateCode = SimulationEffectStateCodes.Pending,
                CausedByDecisionStableId = decisionId.Trim(),
                CausedByTaskStableId = taskId.Trim(),
                SourceStableIds = sources,
                Lines = lines,
            };
            validator.Validate(bundle);
            return bundle;
        }

        private static Simulation자원효과선Snapshot Line(
            string lineId,
            string kind,
            string role,
            string resourceType,
            string ledgerId,
            SimulationIndividualOrderSnapshot order,
            string? consumptionId,
            decimal before,
            decimal delta,
            string? conservationId,
            decimal conservationQuantity,
            string[] sources)
            => new Simulation자원효과선Snapshot
            {
                EffectLineStableId = lineId.Trim(),
                MutationKindCode = kind,
                RoleCode = role,
                ResourceTypeCode = resourceType,
                TargetLedgerStableId = ledgerId.Trim(),
                ProductStableId = order.ProductStableId.Trim(),
                LotStableId = consumptionId,
                BeforeValue = before,
                Delta = delta,
                AfterValue = before + delta,
                UnitCode = order.UnitCode.Trim(),
                ConservationGroupStableId = conservationId,
                ConservationQuantity = conservationQuantity,
                ConservationUnitCode = conservationId == null ? null : order.UnitCode.Trim(),
                SourceStableIds = sources,
            };

        private static void ValidateOrderAndReservation(
            SimulationIndividualOrderSnapshot order,
            SimulationStockReservationSnapshot reservation)
        {
            if (order == null || reservation == null
                || order.Revision <= 0 || reservation.Quantity <= 0m
                || order.OrderedQuantity <= 0m
                || order.OrderedQuantity != reservation.Quantity
                || order.OrderStableId != reservation.OrderStableId
                || order.ProductStableId != reservation.ProductStableId
                || order.MarketFacilityStableId != reservation.MarketFacilityStableId
                || order.UnitCode != reservation.UnitCode)
            {
                throw new SimulationContractException("SimulationConsumptionOrderReservationMismatch");
            }
            RequireStableId(order.DecisionStableId, "SimulationConsumptionDecisionInvalid");
            RequireStableId(order.TaskStableId, "SimulationConsumptionTaskInvalid");
            ValidateSources(order.SourceStableIds, "SimulationConsumptionOrderSourcesInvalid");
            ValidateSources(reservation.SourceStableIds, "SimulationConsumptionReservationSourcesInvalid");
        }

        private static void ValidateOrderAndConsumption(
            SimulationIndividualOrderSnapshot order,
            Simulation시장소비Snapshot consumption)
        {
            if (order == null || consumption == null
                || order.Revision <= 0 || consumption.Revision <= 0
                || order.OrderStableId != consumption.OrderStableId
                || order.ActorStableId != consumption.ActorStableId
                || order.ProductStableId != consumption.ProductStableId
                || order.MarketFacilityStableId != consumption.MarketFacilityStableId
                || order.FulfilledQuantity != consumption.Quantity
                || order.UnitCode != consumption.UnitCode)
            {
                throw new SimulationContractException("SimulationConsumptionOrderMismatch");
            }
            RequireStableId(consumption.DecisionStableId, "SimulationConsumptionDecisionInvalid");
            RequireStableId(consumption.TaskStableId, "SimulationConsumptionTaskInvalid");
            ValidateSources(order.SourceStableIds, "SimulationConsumptionOrderSourcesInvalid");
            ValidateSources(consumption.SourceStableIds, "SimulationConsumptionSourcesInvalid");
        }

        private static void ValidateCommonIds(params string[] values)
        {
            foreach (var value in values)
                RequireStableId(value, "SimulationConsumptionResourceStableIdInvalid");
        }

        private static void ValidateSources(string[] values, string errorCode)
        {
            if (values == null || values.Length == 0
                || values.Any(string.IsNullOrWhiteSpace)
                || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
            {
                throw new SimulationContractException(errorCode);
            }
            foreach (var value in values) RequireStableId(value, errorCode);
        }

        private static string[] Sources(
            string[] direct,
            string[] first,
            string[] second,
            params string[] ids)
        {
            ValidateSources(direct, "SimulationConsumptionResourceSourcesInvalid");
            return direct.Concat(first).Concat(second).Concat(ids)
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
        }

        private static Simulation소비흐름자원효과Result Result(
            string stage,
            decimal quantity,
            string unit,
            Simulation자원효과묶음Snapshot bundle)
            => new Simulation소비흐름자원효과Result
            {
                StageCode = stage,
                Quantity = quantity,
                UnitCode = unit,
                PendingEffectBundle = bundle,
            };

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                throw new SimulationContractException(errorCode);
            }
        }
    }
}
