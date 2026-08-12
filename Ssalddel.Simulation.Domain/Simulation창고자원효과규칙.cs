using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation창고자원효과규칙
    {
        private const string RuleStableId = "rule:warehouse-resource-flow.v1";
        private readonly Simulation자원효과묶음Validator validator;

        public Simulation창고자원효과규칙()
            : this(new Simulation자원효과묶음Validator())
        {
        }

        public Simulation창고자원효과규칙(Simulation자원효과묶음Validator value)
        {
            validator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Simulation창고자원효과Result CreateIntake(Simulation창고자원효과Request request)
        {
            Validate(request, Simulation창고작업상태Codes.ReceivedAtDock);
            var quantity = request.Work.ReceivedQuantity;
            if (request.ReceivedCargoBefore < quantity || request.InspectionPendingBefore < 0m)
                throw Error("SimulationWarehouseIntakeLedgerInvalid");
            var sources = Sources(request);
            var lines = Transfer(request, "intake", "ReceivedCargoHandoff",
                request.ReceivedCargoLedgerStableId, request.ReceivedCargoBefore,
                "WarehouseInspectionPending", request.InspectionPendingLedgerStableId,
                request.InspectionPendingBefore, quantity, sources);
            return Result("Intake", quantity, quantity, 0m, request, Bundle(request, sources, lines));
        }

        public Simulation창고자원효과Result CreateInspection(Simulation창고자원효과Request request)
        {
            Validate(request, Simulation창고작업상태Codes.InspectionCompleted);
            var total = request.Work.AcceptedQuantity + request.Work.RejectedQuantity;
            if (request.Work.AcceptedQuantity < 0m || request.Work.RejectedQuantity < 0m
                || total != request.Work.ReceivedQuantity
                || request.InspectionPendingBefore < total)
                throw Error("SimulationWarehouseInspectionQuantityInvalid");
            RequireIds(request.InspectionPendingLedgerStableId,
                request.InspectionAcceptedLedgerStableId, request.InspectionRejectedLedgerStableId);
            var sources = Sources(request);
            var group = Group("inspection", request);
            var lines = new List<Simulation자원효과선Snapshot>
            {
                CargoLine(request, "inspection-input", Simulation자원변동유형Codes.Transformation,
                    Simulation자원효과역할Codes.Input, "WarehouseInspectionPending",
                    request.InspectionPendingLedgerStableId, request.InspectionPendingBefore,
                    -total, group, -total, sources),
            };
            if (request.Work.AcceptedQuantity > 0m)
                lines.Add(CargoLine(request, "inspection-accepted",
                    Simulation자원변동유형Codes.Transformation,
                    Simulation자원효과역할Codes.Output, "WarehouseInspectionAccepted",
                    request.InspectionAcceptedLedgerStableId, request.InspectionAcceptedBefore,
                    request.Work.AcceptedQuantity, group, request.Work.AcceptedQuantity, sources));
            if (request.Work.RejectedQuantity > 0m)
                lines.Add(CargoLine(request, "inspection-rejected",
                    Simulation자원변동유형Codes.Transformation,
                    Simulation자원효과역할Codes.Loss, "WarehouseInspectionRejected",
                    request.InspectionRejectedLedgerStableId, request.InspectionRejectedBefore,
                    request.Work.RejectedQuantity, group, request.Work.RejectedQuantity, sources));
            return Result("Inspection", total, request.Work.AcceptedQuantity,
                request.Work.RejectedQuantity, request, Bundle(request, sources, lines.ToArray()));
        }

        public Simulation창고자원효과Result CreatePutaway(Simulation창고자원효과Request request)
        {
            Validate(request, Simulation창고작업상태Codes.Stored);
            var quantity = request.Work.AcceptedQuantity;
            ValidateCapacity(request, quantity);
            if (quantity <= 0m || request.InspectionAcceptedBefore < quantity
                || request.WarehouseStockBefore < 0m)
                throw Error("SimulationWarehousePutawayLedgerInvalid");
            var sources = Sources(request);
            var lines = Transfer(request, "putaway", "WarehouseInspectionAccepted",
                request.InspectionAcceptedLedgerStableId, request.InspectionAcceptedBefore,
                "WarehouseStock", request.WarehouseStockLedgerStableId,
                request.WarehouseStockBefore, quantity, sources)
                .Concat(CapacityReservation(request, "putaway-capacity", quantity, sources))
                .ToArray();
            return Result("Putaway", quantity, quantity, 0m, request, Bundle(request, sources, lines));
        }

        public Simulation창고자원효과Result CreateStorageLoss(Simulation창고자원효과Request request)
        {
            Validate(request, Simulation창고작업상태Codes.Stored);
            var loss = request.StorageLossQuantity;
            if (loss <= 0m || request.WarehouseStockBefore < loss
                || request.StorageOccupiedBefore < loss
                || request.StorageAvailableBefore + request.StorageOccupiedBefore
                    != request.StorageCapacity)
                throw Error("SimulationWarehouseStorageLossInvalid");
            RequireIds(request.WarehouseStockLedgerStableId, request.WarehouseLossLedgerStableId);
            var sources = Sources(request);
            var lossGroup = Group("storage-loss", request);
            var lines = new[]
            {
                CargoLine(request, "storage-loss-source", Simulation자원변동유형Codes.Loss,
                    Simulation자원효과역할Codes.Source, "WarehouseStock",
                    request.WarehouseStockLedgerStableId, request.WarehouseStockBefore,
                    -loss, lossGroup, -loss, sources),
                CargoLine(request, "storage-loss-record", Simulation자원변동유형Codes.Loss,
                    Simulation자원효과역할Codes.Loss, "WarehouseStorageLoss",
                    request.WarehouseLossLedgerStableId, request.WarehouseLossBefore,
                    loss, lossGroup, loss, sources),
            }.Concat(CapacityRelease(request, "storage-loss-capacity", loss, sources)).ToArray();
            return Result("StorageLoss", loss, 0m, loss, request, Bundle(request, sources, lines));
        }

        public Simulation창고자원효과Result CreatePicking(Simulation창고자원효과Request request)
        {
            Validate(request, Simulation창고작업상태Codes.Picked);
            var quantity = request.Work.PickedQuantity;
            if (quantity <= 0m || request.WarehouseStockBefore < quantity
                || request.OutboundReservedBefore < 0m)
                throw Error("SimulationWarehousePickingQuantityInvalid");
            RequireIds(request.WarehouseStockLedgerStableId, request.OutboundReservedLedgerStableId);
            var sources = Sources(request);
            var group = Group("picking", request);
            var lines = new[]
            {
                CargoLine(request, "picking-stock", Simulation자원변동유형Codes.Reservation,
                    Simulation자원효과역할Codes.Available, "WarehouseStock",
                    request.WarehouseStockLedgerStableId, request.WarehouseStockBefore,
                    -quantity, group, -quantity, sources),
                CargoLine(request, "picking-reserved", Simulation자원변동유형Codes.Reservation,
                    Simulation자원효과역할Codes.Reserved, "WarehouseOutboundReserved",
                    request.OutboundReservedLedgerStableId, request.OutboundReservedBefore,
                    quantity, group, quantity, sources),
            };
            return Result("Picking", quantity, quantity, 0m, request, Bundle(request, sources, lines));
        }

        public Simulation창고자원효과Result CreateOutbound(Simulation창고자원효과Request request)
        {
            Validate(request, Simulation창고작업상태Codes.OutboundCompleted);
            var quantity = request.Work.PickedQuantity;
            if (quantity <= 0m || request.OutboundReservedBefore < quantity
                || request.OutboundHandoffBefore < 0m || request.StorageOccupiedBefore < quantity)
                throw Error("SimulationWarehouseOutboundQuantityInvalid");
            RequireIds(request.OutboundReservedLedgerStableId,
                request.OutboundHandoffLedgerStableId);
            var sources = Sources(request);
            var lines = Transfer(request, "outbound", "WarehouseOutboundReserved",
                request.OutboundReservedLedgerStableId, request.OutboundReservedBefore,
                "WarehouseOutboundHandoff", request.OutboundHandoffLedgerStableId,
                request.OutboundHandoffBefore, quantity, sources)
                .Concat(CapacityRelease(request, "outbound-capacity", quantity, sources))
                .ToArray();
            return Result("Outbound", quantity, quantity, 0m, request, Bundle(request, sources, lines));
        }

        private Simulation자원효과묶음Snapshot Bundle(
            Simulation창고자원효과Request request, string[] sources,
            params Simulation자원효과선Snapshot[] lines)
        {
            var bundle = new Simulation자원효과묶음Snapshot
            {
                EffectBundleStableId = request.EffectBundleStableId.Trim(),
                RuleStableId = RuleStableId,
                RuleRevision = 1,
                RuleDomainCode = Simulation업무규칙영역Codes.Warehouse,
                ModeCode = "Simulation",
                StateCode = SimulationEffectStateCodes.Pending,
                CausedByDecisionStableId = request.Work.DecisionStableId.Trim(),
                CausedByTaskStableId = request.Work.TaskStableId.Trim(),
                SourceStableIds = sources,
                Lines = lines,
            };
            validator.Validate(bundle);
            return bundle;
        }

        private static Simulation자원효과선Snapshot[] Transfer(
            Simulation창고자원효과Request request, string stage,
            string sourceType, string sourceLedger, decimal sourceBefore,
            string targetType, string targetLedger, decimal targetBefore,
            decimal quantity, string[] sources)
        {
            RequireIds(sourceLedger, targetLedger);
            var group = Group(stage, request);
            return new[]
            {
                CargoLine(request, stage + "-source", Simulation자원변동유형Codes.Transfer,
                    Simulation자원효과역할Codes.Source, sourceType, sourceLedger,
                    sourceBefore, -quantity, group, -quantity, sources),
                CargoLine(request, stage + "-target", Simulation자원변동유형Codes.Transfer,
                    Simulation자원효과역할Codes.Target, targetType, targetLedger,
                    targetBefore, quantity, group, quantity, sources),
            };
        }

        private static Simulation자원효과선Snapshot[] CapacityReservation(
            Simulation창고자원효과Request request, string stage, decimal quantity, string[] sources)
        {
            RequireIds(request.StorageAvailableLedgerStableId, request.StorageOccupiedLedgerStableId);
            var group = Group(stage, request);
            return new[]
            {
                CapacityLine(request, stage + "-available", Simulation자원변동유형Codes.Reservation,
                    Simulation자원효과역할Codes.Available, request.StorageAvailableLedgerStableId,
                    request.StorageAvailableBefore, -quantity, group, -quantity, sources),
                CapacityLine(request, stage + "-occupied", Simulation자원변동유형Codes.Reservation,
                    Simulation자원효과역할Codes.Reserved, request.StorageOccupiedLedgerStableId,
                    request.StorageOccupiedBefore, quantity, group, quantity, sources),
            };
        }

        private static Simulation자원효과선Snapshot[] CapacityRelease(
            Simulation창고자원효과Request request, string stage, decimal quantity, string[] sources)
        {
            RequireIds(request.StorageAvailableLedgerStableId, request.StorageOccupiedLedgerStableId);
            var group = Group(stage, request);
            return new[]
            {
                CapacityLine(request, stage + "-available",
                    Simulation자원변동유형Codes.ReservationRelease,
                    Simulation자원효과역할Codes.Available, request.StorageAvailableLedgerStableId,
                    request.StorageAvailableBefore, quantity, group, quantity, sources),
                CapacityLine(request, stage + "-occupied",
                    Simulation자원변동유형Codes.ReservationRelease,
                    Simulation자원효과역할Codes.Reserved, request.StorageOccupiedLedgerStableId,
                    request.StorageOccupiedBefore, -quantity, group, -quantity, sources),
            };
        }

        private static Simulation자원효과선Snapshot CargoLine(
            Simulation창고자원효과Request request, string suffix, string kind, string role,
            string resourceType, string ledger, decimal before, decimal delta,
            string group, decimal conserved, string[] sources)
            => new Simulation자원효과선Snapshot
            {
                EffectLineStableId = request.EffectLineStableIdPrefix.Trim() + "." + suffix,
                MutationKindCode = kind,
                RoleCode = role,
                ResourceTypeCode = resourceType,
                TargetLedgerStableId = ledger.Trim(),
                ProductStableId = request.Work.ProductStableId.Trim(),
                LotStableId = request.Work.CargoStableId.Trim(),
                BeforeValue = before,
                Delta = delta,
                AfterValue = before + delta,
                UnitCode = request.Work.UnitCode.Trim(),
                ConservationGroupStableId = group,
                ConservationQuantity = conserved,
                ConservationUnitCode = request.Work.UnitCode.Trim(),
                SourceStableIds = sources,
            };

        private static Simulation자원효과선Snapshot CapacityLine(
            Simulation창고자원효과Request request, string suffix, string kind, string role,
            string ledger, decimal before, decimal delta, string group, decimal conserved,
            string[] sources)
            => new Simulation자원효과선Snapshot
            {
                EffectLineStableId = request.EffectLineStableIdPrefix.Trim() + "." + suffix,
                MutationKindCode = kind,
                RoleCode = role,
                ResourceTypeCode = "WarehouseStorageCapacity",
                TargetLedgerStableId = ledger.Trim(),
                BeforeValue = before,
                Delta = delta,
                AfterValue = before + delta,
                UnitCode = request.Work.UnitCode.Trim(),
                ConservationGroupStableId = group,
                ConservationQuantity = conserved,
                ConservationUnitCode = request.Work.UnitCode.Trim(),
                SourceStableIds = sources,
            };

        private static void Validate(Simulation창고자원효과Request request, string state)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireIds(request.EffectBundleStableId, request.EffectLineStableIdPrefix);
            var work = request.Work;
            if (work == null || work.Revision <= 0 || work.StateCode != state
                || work.ReceivedQuantity <= 0m || string.IsNullOrWhiteSpace(work.UnitCode)
                || work.DecisionStateCode != SimulationDecisionStateCodes.Confirmed
                || work.TaskStateCode != SimulationTaskStateCodes.Completed
                || work.CompletedTick < 0)
                throw Error("SimulationWarehouseWorkStateInvalid");
            RequireIds(work.WarehouseWorkStableId, work.CargoStableId, work.ProductStableId,
                work.WarehouseFacilityStableId, work.DecisionStableId, work.TaskStableId);
            ValidateSources(work.SourceStableIds, "SimulationWarehouseWorkSourcesInvalid");
            ValidateSources(request.SourceStableIds, "SimulationWarehouseResourceSourcesInvalid");
        }

        private static void ValidateCapacity(Simulation창고자원효과Request request, decimal quantity)
        {
            if (request.StorageCapacity <= 0m
                || request.StorageAvailableBefore < quantity
                || request.StorageOccupiedBefore < 0m
                || request.StorageAvailableBefore + request.StorageOccupiedBefore
                    != request.StorageCapacity)
                throw Error("SimulationWarehouseStorageCapacityExceeded");
        }

        private static string[] Sources(Simulation창고자원효과Request request)
            => request.SourceStableIds.Concat(request.Work.SourceStableIds)
                .Concat(new[] { request.Work.WarehouseWorkStableId, request.Work.CargoStableId,
                    request.Work.WarehouseFacilityStableId, request.Work.DecisionStableId,
                    request.Work.TaskStableId })
                .Select(value => value.Trim()).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

        private static string Group(string stage, Simulation창고자원효과Request request)
            => "conservation:warehouse-" + stage + ":" + request.Work.CargoStableId;

        private static Simulation창고자원효과Result Result(
            string stage, decimal input, decimal output, decimal loss,
            Simulation창고자원효과Request request, Simulation자원효과묶음Snapshot bundle)
            => new Simulation창고자원효과Result
            {
                StageCode = stage,
                InputQuantity = input,
                OutputQuantity = output,
                LossQuantity = loss,
                UnitCode = request.Work.UnitCode,
                PendingEffectBundle = bundle,
            };

        private static void ValidateSources(string[] values, string errorCode)
        {
            if (values == null || values.Length == 0 || values.Any(string.IsNullOrWhiteSpace)
                || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
                throw Error(errorCode);
            foreach (var value in values) RequireIds(value);
        }

        private static void RequireIds(params string[] values)
        {
            foreach (var value in values)
                if (string.IsNullOrWhiteSpace(value) || value.Length > 160
                    || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                    throw Error("SimulationWarehouseResourceStableIdInvalid");
        }

        private static SimulationContractException Error(string code)
            => new SimulationContractException(code);
    }
}
