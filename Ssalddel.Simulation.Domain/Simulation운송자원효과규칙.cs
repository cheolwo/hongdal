using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation운송자원효과규칙
    {
        private const string RuleStableId = "rule:freight-transport.resource.v1";
        private readonly Simulation자원효과묶음Validator validator;

        public Simulation운송자원효과규칙()
            : this(new Simulation자원효과묶음Validator())
        {
        }

        public Simulation운송자원효과규칙(Simulation자원효과묶음Validator value)
        {
            validator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Simulation운송자원효과Result CreateLoading(Simulation운송자원효과Request request)
        {
            ValidateCommon(request);
            if (request.Movement.StateCode != SimulationLogisticsMovementStateCodes.InTransit
                || request.Freight.StateCode != 화물운송상태코드.운송중
                || !request.Freight.PickedUpTick.HasValue
                || !request.Freight.StateHistory.Any(value => value.ToStateCode == 화물운송상태코드.상차완료))
            {
                throw new SimulationContractException("SimulationTransportLoadingStateInvalid");
            }
            if (request.OriginCargoBefore < request.Freight.Quantity
                || request.VehicleCargoBefore != 0m)
            {
                throw new SimulationContractException("SimulationTransportLoadingLedgerInvalid");
            }
            RequireIds(request.OriginCargoLedgerStableId, request.VehicleCargoLedgerStableId);
            var quantity = request.Freight.Quantity;
            var sources = Sources(request);
            var conservation = "conservation:transport-loading:" + request.Freight.CargoStableId;
            var bundle = Bundle(request, request.Freight.LogisticsTaskStableId, sources,
                TransferLine(request, "loading-source", Simulation자원효과역할Codes.Source,
                    "OriginCargoStock", request.OriginCargoLedgerStableId,
                    request.OriginCargoBefore, -quantity, conservation, -quantity, sources),
                TransferLine(request, "loading-target", Simulation자원효과역할Codes.Target,
                    "VehicleCargoStock", request.VehicleCargoLedgerStableId,
                    request.VehicleCargoBefore, quantity, conservation, quantity, sources));
            return Result("Loading", quantity, 0m, quantity, request.Freight.UnitCode, bundle);
        }

        public Simulation운송자원효과Result CreateTravel(Simulation운송자원효과Request request)
        {
            ValidateCommon(request);
            if (request.Movement.StateCode != SimulationLogisticsMovementStateCodes.ArrivedAtDestination
                || request.Freight.StateCode != 화물운송상태코드.하차지도착
                || !request.Movement.ArrivedTick.HasValue
                || request.Movement.CompletedRouteTicks != request.Movement.RequiredRouteTicks
                || !request.Freight.ArrivedAtDropoffTick.HasValue)
            {
                throw new SimulationContractException("SimulationTransportTravelStateInvalid");
            }
            if (request.CargoLossQuantity < 0m
                || request.CargoLossQuantity >= request.Freight.Quantity
                || request.VehicleCargoBefore != request.Freight.Quantity
                || request.FuelConsumption <= 0m || request.FuelBefore < request.FuelConsumption
                || request.LaborConsumption <= 0m || request.LaborBefore < request.LaborConsumption)
            {
                throw new SimulationContractException("SimulationTransportTravelResourceInvalid");
            }
            RequireIds(request.VehicleCargoLedgerStableId, request.FuelLedgerStableId,
                request.LaborLedgerStableId);
            RequireText(request.FuelUnitCode, "SimulationTransportFuelUnitInvalid");
            RequireText(request.LaborUnitCode, "SimulationTransportLaborUnitInvalid");
            var sources = Sources(request);
            var lines = new List<Simulation자원효과선Snapshot>
            {
                ConsumptionLine(request, "fuel", "TransportFuel", request.FuelLedgerStableId,
                    request.FuelBefore, request.FuelConsumption, request.FuelUnitCode, sources),
                ConsumptionLine(request, "labor", "TransportLabor", request.LaborLedgerStableId,
                    request.LaborBefore, request.LaborConsumption, request.LaborUnitCode, sources),
            };
            if (request.CargoLossQuantity > 0m)
            {
                RequireIds(request.TransportLossLedgerStableId);
                var conservation = "conservation:transport-loss:" + request.Freight.CargoStableId;
                lines.Add(LossLine(request, "loss-source", Simulation자원효과역할Codes.Source,
                    "VehicleCargoStock", request.VehicleCargoLedgerStableId,
                    request.VehicleCargoBefore, -request.CargoLossQuantity,
                    conservation, -request.CargoLossQuantity, sources));
                lines.Add(LossLine(request, "loss-record", Simulation자원효과역할Codes.Loss,
                    "TransportCargoLoss", request.TransportLossLedgerStableId,
                    request.TransportLossBefore, request.CargoLossQuantity,
                    conservation, request.CargoLossQuantity, sources));
            }
            var bundle = Bundle(request, request.Freight.LogisticsTaskStableId, sources, lines.ToArray());
            return Result("Travel", request.Freight.Quantity, request.CargoLossQuantity,
                request.Freight.Quantity - request.CargoLossQuantity, request.Freight.UnitCode, bundle);
        }

        public Simulation운송자원효과Result CreateUnloading(Simulation운송자원효과Request request)
        {
            ValidateCommon(request);
            if (request.Movement.StateCode != SimulationLogisticsMovementStateCodes.ArrivedAtDestination
                || request.Freight.StateCode != 화물운송상태코드.하차지도착
                || !request.Freight.ArrivedAtDropoffTick.HasValue)
            {
                throw new SimulationContractException("SimulationTransportUnloadingStateInvalid");
            }
            var delivered = request.Freight.Quantity - request.CargoLossQuantity;
            if (request.CargoLossQuantity < 0m || delivered <= 0m
                || request.VehicleCargoBefore != delivered
                || request.DestinationStagingBefore < 0m)
            {
                throw new SimulationContractException("SimulationTransportUnloadingLedgerInvalid");
            }
            RequireIds(request.VehicleCargoLedgerStableId, request.DestinationStagingLedgerStableId);
            var sources = Sources(request);
            var conservation = "conservation:transport-unloading:" + request.Freight.CargoStableId;
            var bundle = Bundle(request, request.Freight.LogisticsTaskStableId, sources,
                TransferLine(request, "unloading-source", Simulation자원효과역할Codes.Source,
                    "VehicleCargoStock", request.VehicleCargoLedgerStableId,
                    request.VehicleCargoBefore, -delivered, conservation, -delivered, sources),
                TransferLine(request, "unloading-target", Simulation자원효과역할Codes.Target,
                    "DestinationCargoStaging", request.DestinationStagingLedgerStableId,
                    request.DestinationStagingBefore, delivered, conservation, delivered, sources));
            return Result("Unloading", request.Freight.Quantity, request.CargoLossQuantity,
                delivered, request.Freight.UnitCode, bundle);
        }

        public Simulation운송자원효과Result CreateReceipt(Simulation운송자원효과Request request)
        {
            ValidateCommon(request);
            if (request.Freight.StateCode != 화물운송상태코드.인수완료
                || !request.Freight.ReceivedTick.HasValue
                || string.IsNullOrWhiteSpace(request.Freight.ReceiptDecisionStableId)
                || string.IsNullOrWhiteSpace(request.Freight.ReceiptTaskStableId))
            {
                throw new SimulationContractException("SimulationTransportReceiptStateInvalid");
            }
            var delivered = request.Freight.Quantity - request.CargoLossQuantity;
            if (request.CargoLossQuantity < 0m || delivered <= 0m
                || request.DestinationStagingBefore < delivered
                || request.ReceivedCargoBefore < 0m)
            {
                throw new SimulationContractException("SimulationTransportReceiptLedgerInvalid");
            }
            RequireIds(request.DestinationStagingLedgerStableId, request.ReceivedCargoLedgerStableId);
            var sources = Sources(request);
            var conservation = "conservation:transport-receipt:" + request.Freight.CargoStableId;
            var bundle = Bundle(request, request.Freight.ReceiptTaskStableId!, sources,
                TransferLine(request, "receipt-source", Simulation자원효과역할Codes.Source,
                    "DestinationCargoStaging", request.DestinationStagingLedgerStableId,
                    request.DestinationStagingBefore, -delivered, conservation, -delivered, sources),
                TransferLine(request, "receipt-target", Simulation자원효과역할Codes.Target,
                    "ReceivedCargoHandoff", request.ReceivedCargoLedgerStableId,
                    request.ReceivedCargoBefore, delivered, conservation, delivered, sources));
            bundle.CausedByDecisionStableId = request.Freight.ReceiptDecisionStableId!;
            validator.Validate(bundle);
            return Result("Receipt", request.Freight.Quantity, request.CargoLossQuantity,
                delivered, request.Freight.UnitCode, bundle);
        }

        private Simulation자원효과묶음Snapshot Bundle(
            Simulation운송자원효과Request request,
            string taskId,
            string[] sources,
            params Simulation자원효과선Snapshot[] lines)
        {
            var bundle = new Simulation자원효과묶음Snapshot
            {
                EffectBundleStableId = request.EffectBundleStableId.Trim(),
                RuleStableId = RuleStableId,
                RuleRevision = 1,
                RuleDomainCode = Simulation업무규칙영역Codes.Transport,
                ModeCode = "Simulation",
                StateCode = SimulationEffectStateCodes.Pending,
                CausedByDecisionStableId = request.Movement.DecisionStableId.Trim(),
                CausedByTaskStableId = taskId.Trim(),
                SourceStableIds = sources,
                Lines = lines,
            };
            validator.Validate(bundle);
            return bundle;
        }

        private static Simulation자원효과선Snapshot TransferLine(
            Simulation운송자원효과Request request, string suffix, string role,
            string resourceType, string ledger, decimal before, decimal delta,
            string conservation, decimal conserved, string[] sources)
            => CargoLine(request, suffix, Simulation자원변동유형Codes.Transfer, role,
                resourceType, ledger, before, delta, conservation, conserved, sources);

        private static Simulation자원효과선Snapshot LossLine(
            Simulation운송자원효과Request request, string suffix, string role,
            string resourceType, string ledger, decimal before, decimal delta,
            string conservation, decimal conserved, string[] sources)
            => CargoLine(request, suffix, Simulation자원변동유형Codes.Loss, role,
                resourceType, ledger, before, delta, conservation, conserved, sources);

        private static Simulation자원효과선Snapshot CargoLine(
            Simulation운송자원효과Request request, string suffix, string kind, string role,
            string resourceType, string ledger, decimal before, decimal delta,
            string conservation, decimal conserved, string[] sources)
            => new Simulation자원효과선Snapshot
            {
                EffectLineStableId = request.EffectLineStableIdPrefix.Trim() + "." + suffix,
                MutationKindCode = kind,
                RoleCode = role,
                ResourceTypeCode = resourceType,
                TargetLedgerStableId = ledger.Trim(),
                ProductStableId = request.Movement.ProductStableId.Trim(),
                LotStableId = request.Freight.CargoStableId.Trim(),
                BeforeValue = before,
                Delta = delta,
                AfterValue = before + delta,
                UnitCode = request.Freight.UnitCode.Trim(),
                ConservationGroupStableId = conservation,
                ConservationQuantity = conserved,
                ConservationUnitCode = request.Freight.UnitCode.Trim(),
                SourceStableIds = sources,
            };

        private static Simulation자원효과선Snapshot ConsumptionLine(
            Simulation운송자원효과Request request, string suffix, string resourceType,
            string ledger, decimal before, decimal amount, string unit, string[] sources)
            => new Simulation자원효과선Snapshot
            {
                EffectLineStableId = request.EffectLineStableIdPrefix.Trim() + "." + suffix,
                MutationKindCode = Simulation자원변동유형Codes.Consumption,
                RoleCode = Simulation자원효과역할Codes.Input,
                ResourceTypeCode = resourceType,
                TargetLedgerStableId = ledger.Trim(),
                BeforeValue = before,
                Delta = -amount,
                AfterValue = before - amount,
                UnitCode = unit.Trim(),
                SourceStableIds = sources,
            };

        private static void ValidateCommon(Simulation운송자원효과Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireIds(request.EffectBundleStableId, request.EffectLineStableIdPrefix);
            var movement = request.Movement;
            var freight = request.Freight;
            if (movement == null || freight == null
                || movement.Revision <= 0 || freight.Revision <= 0
                || movement.CargoStableId != freight.CargoStableId
                || movement.Quantity != freight.Quantity
                || movement.UnitCode != freight.UnitCode
                || freight.VehicleCapacity < freight.Quantity
                || freight.VehicleCapacityUnitCode != freight.UnitCode
                || movement.RequiredRouteTicks <= 0
                || movement.CompletedRouteTicks < 0
                || movement.CompletedRouteTicks > movement.RequiredRouteTicks)
            {
                throw new SimulationContractException("SimulationTransportBindingInvalid");
            }
            RequireIds(movement.CargoStableId, movement.ProductStableId,
                movement.OriginFacilityStableId, movement.DestinationFacilityStableId,
                movement.DecisionStableId, movement.TaskStableId, freight.TransportRequestStableId,
                freight.VehicleStableId, freight.LogisticsTaskStableId);
            ValidateSources(request.SourceStableIds, "SimulationTransportResourceSourcesInvalid");
            ValidateSources(movement.SourceStableIds, "SimulationTransportMovementSourcesInvalid");
            ValidateSources(freight.SourceStableIds, "SimulationTransportFreightSourcesInvalid");
        }

        private static string[] Sources(Simulation운송자원효과Request request)
            => request.SourceStableIds.Concat(request.Movement.SourceStableIds)
                .Concat(request.Freight.SourceStableIds)
                .Concat(new[]
                {
                    request.Movement.CargoStableId,
                    request.Movement.RouteStableId,
                    request.Movement.OriginFacilityStableId,
                    request.Movement.DestinationFacilityStableId,
                    request.Freight.TransportRequestStableId,
                    request.Freight.VehicleStableId,
                })
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private static Simulation운송자원효과Result Result(
            string stage, decimal loaded, decimal lost, decimal delivered, string unit,
            Simulation자원효과묶음Snapshot bundle)
            => new Simulation운송자원효과Result
            {
                StageCode = stage,
                LoadedQuantity = loaded,
                LostQuantity = lost,
                DeliveredQuantity = delivered,
                UnitCode = unit,
                PendingEffectBundle = bundle,
            };

        private static void ValidateSources(string[] values, string errorCode)
        {
            if (values == null || values.Length == 0
                || values.Any(string.IsNullOrWhiteSpace)
                || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
                throw new SimulationContractException(errorCode);
            foreach (var value in values) RequireIds(value);
        }

        private static void RequireIds(params string[] values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length > 160
                    || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                    throw new SimulationContractException("SimulationTransportResourceStableIdInvalid");
            }
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException(errorCode);
        }
    }
}
