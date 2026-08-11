using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 수출Cargo준비DecisionTypeCode = "ExportCargoPreparation";
        private const string 수출Cargo준비DecisionPrefix = "decision:export-cargo-preparation:";
        private readonly Dictionary<string, Simulation수출Cargo준비Snapshot> 수출Cargo준비원장 =
            new Dictionary<string, Simulation수출Cargo준비Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> 수출Cargo준비출처연결 =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public Simulation수출Cargo준비PreviewSnapshot Preview수출Cargo준비(
            Simulation수출Cargo준비PreviewRequest request)
        {
            Validate수출Cargo준비Request(request);
            lock (gate)
            {
                return Create수출Cargo준비Preview(request);
            }
        }

        public 경영SimulationSessionSnapshot Confirm수출Cargo준비(
            Simulation수출Cargo준비ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            Validate수출Cargo준비Request(request.CargoPreparation);
            return ConfirmDecision(new SimulationDecisionConfirmRequest
            {
                CommandId = request.CommandId.Trim(),
                ExpectedRevision = request.ExpectedRevision,
                Preview = Create수출Cargo준비DecisionRequest(request.CargoPreparation),
            });
        }

        private Simulation수출Cargo준비PreviewSnapshot Create수출Cargo준비Preview(
            Simulation수출Cargo준비PreviewRequest request)
        {
            var common = Create수출Cargo준비DecisionRequest(request);
            수출준비원장.TryGetValue(
                request.SourceExportPreparationStableId.Trim(), out var preparation);
            return new Simulation수출Cargo준비PreviewSnapshot
            {
                CargoPreparationStableId = request.CargoPreparationStableId.Trim(),
                SourceExportPreparationStableId = request.SourceExportPreparationStableId.Trim(),
                RootExportPreparationStableId = preparation?.RootPreparationStableId ?? string.Empty,
                ExportPreparationAttemptNumber = preparation?.AttemptNumber ?? 0,
                SourceAllocationStableId = preparation?.SourceAllocationStableId ?? string.Empty,
                HarvestLotStableId = preparation?.HarvestLotStableId ?? string.Empty,
                PackageLotStableId = preparation?.PackageLotCandidateStableId ?? string.Empty,
                ProductStableId = preparation?.ProductStableId ?? string.Empty,
                Quantity = preparation?.Quantity ?? 0m,
                UnitCode = preparation?.UnitCode ?? string.Empty,
                CargoStableId = request.CargoStableId.Trim(),
                CargoRevision = request.CargoRevision,
                RouteStableId = request.RouteStableId.Trim(),
                OriginFacilityStableId = preparation?.HandoffFacilityStableId ?? string.Empty,
                DestinationFacilityStableId = request.DestinationFacilityStableId.Trim(),
                IsCandidateOnly = true,
                DoesNotCreateOperationalHandoff = true,
                BoundaryCodes = 수출Cargo준비BoundaryCodes(),
                CommonDecisionPreview = CreateDecisionPreview(common),
            };
        }

        private SimulationDecisionPreviewRequest Create수출Cargo준비DecisionRequest(
            Simulation수출Cargo준비PreviewRequest request)
        {
            var preparationId = request.SourceExportPreparationStableId.Trim();
            var blocks = new List<string>();
            Simulation수출준비Snapshot? preparation = null;
            if (!수출준비원장.TryGetValue(preparationId, out preparation))
            {
                blocks.Add("SourceExportPreparationNotFound");
            }
            else
            {
                if (preparation.StateCode != Simulation수출준비상태Codes.HandoffCandidateReady)
                    blocks.Add("SourceExportPreparationNotPassed");
                if (수출준비원장.Values.Any(value =>
                    value.RootPreparationStableId == preparation.RootPreparationStableId
                    && value.AttemptNumber > preparation.AttemptNumber))
                    blocks.Add("SourceExportPreparationNotLatestAttempt");
                if (!string.IsNullOrWhiteSpace(preparation.CargoPreparationStableId))
                    blocks.Add("SourceExportPreparationCargoAlreadyPrepared");
                if (!harvestLotAllocations.TryGetValue(
                        preparation.SourceAllocationStableId, out var allocation)
                    || allocation.OutboundReservedQuantity < preparation.Quantity)
                    blocks.Add("SourceExportPreparationQuantityNotReserved");
            }
            if (수출Cargo준비원장.ContainsKey(request.CargoPreparationStableId.Trim()))
                blocks.Add("ExportCargoPreparationStableIdConflict");
            if (수출Cargo준비출처연결.ContainsKey(preparationId))
                blocks.Add("ExportCargoPreparationSourceAlreadyUsed");
            if (수출Cargo준비원장.Values.Any(value => value.CargoStableId == request.CargoStableId.Trim())
                || logisticsMovements.ContainsKey(request.CargoStableId.Trim()))
                blocks.Add("ExportCargoStableIdConflict");
            if (settlementInitialState == null
                || !settlementInitialState.Facilities.Any(value =>
                    value.FacilityStableId == request.DestinationFacilityStableId.Trim()))
                blocks.Add("ExportCargoDestinationFacilityNotFound");
            if (preparation != null
                && preparation.HandoffFacilityStableId == request.DestinationFacilityStableId.Trim())
                blocks.Add("ExportCargoRouteEndpointsEqual");

            var cargoPreparationId = request.CargoPreparationStableId.Trim();
            var sources = MergeSources(request.SourceStableIds, new[] { preparationId });
            var quantity = preparation?.Quantity ?? 1m;
            var unitCode = preparation?.UnitCode ?? "KGM";
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = 수출Cargo준비DecisionPrefix + cargoPreparationId,
                DecisionTypeCode = 수출Cargo준비DecisionTypeCode,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    cargoPreparationId,
                    preparationId,
                    request.CargoStableId.Trim(),
                    request.DestinationFacilityStableId.Trim(),
                },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = "ExportCargoPreparedQuantity",
                        TargetLedgerStableId = cargoPreparationId,
                        BeforeValue = 0m,
                        Delta = quantity,
                        AfterValue = quantity,
                        UnitCode = unitCode,
                        SourceStableIds = sources,
                    },
                },
                Uncertainties = new[]
                {
                    "Cargo preparation does not confirm carrier handoff, dispatch, departure, or export.",
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:export-cargo-preparation:" + cargoPreparationId,
                    TaskTypeCode = 수출Cargo준비DecisionTypeCode,
                    FacilityStableId = preparation?.HandoffFacilityStableId
                        ?? request.DestinationFacilityStableId.Trim(),
                    AssignedCapacity = quantity,
                    AssignedCapacityUnitCode = unitCode,
                    DurationTicks = request.RequiredPreparationTicks,
                    InputLotStableIds = new[]
                    {
                        preparation?.PackageLotCandidateStableId ?? preparationId,
                    },
                    OutputCandidateCodes = new[]
                    {
                        "route-ref:" + request.RouteStableId.Trim(),
                        "cargo-revision:" + request.CargoRevision.ToString(CultureInfo.InvariantCulture),
                    },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation수출Cargo준비Snapshot? Prepare수출Cargo준비(
            SimulationDecisionPreviewRequest request,
            SimulationDecisionPreviewSnapshot preview)
        {
            if (request.DecisionTypeCode != 수출Cargo준비DecisionTypeCode) return null;
            var cargoPreparationId = request.DecisionStableId.Substring(
                수출Cargo준비DecisionPrefix.Length);
            var sourcePreparation = request.TargetStableIds
                .Select(value => 수출준비원장.TryGetValue(value, out var found) ? found : null)
                .First(value => value != null)!;
            var destination = request.TargetStableIds.Single(value =>
                value != cargoPreparationId
                && value != sourcePreparation.PreparationStableId
                && settlementInitialState!.Facilities.Any(facility => facility.FacilityStableId == value));
            var cargoId = request.TargetStableIds.Single(value =>
                value != cargoPreparationId
                && value != sourcePreparation.PreparationStableId
                && value != destination);
            var routeCode = request.Task.OutputCandidateCodes.Single(value =>
                value.StartsWith("route-ref:", StringComparison.Ordinal));
            var revisionCode = request.Task.OutputCandidateCodes.Single(value =>
                value.StartsWith("cargo-revision:", StringComparison.Ordinal));
            return new Simulation수출Cargo준비Snapshot
            {
                CargoPreparationStableId = cargoPreparationId,
                StateCode = Simulation수출Cargo준비상태Codes.Scheduled,
                Revision = 1,
                SourceExportPreparationStableId = sourcePreparation.PreparationStableId,
                RootExportPreparationStableId = sourcePreparation.RootPreparationStableId,
                ExportPreparationAttemptNumber = sourcePreparation.AttemptNumber,
                SourceAllocationStableId = sourcePreparation.SourceAllocationStableId,
                HarvestLotStableId = sourcePreparation.HarvestLotStableId,
                PackageLotStableId = sourcePreparation.PackageLotCandidateStableId,
                ProductStableId = sourcePreparation.ProductStableId,
                Quantity = sourcePreparation.Quantity,
                UnitCode = sourcePreparation.UnitCode,
                CargoStableId = cargoId,
                CargoRevision = long.Parse(
                    revisionCode.Substring("cargo-revision:".Length), CultureInfo.InvariantCulture),
                RouteStableId = routeCode.Substring("route-ref:".Length),
                OriginFacilityStableId = sourcePreparation.HandoffFacilityStableId,
                DestinationFacilityStableId = destination,
                DecisionStableId = preview.Decision.DecisionStableId,
                TaskStableId = preview.TaskPlan.TaskStableId,
                RequiredPreparationTicks = request.Task.DurationTicks,
                ScheduledTick = CurrentTick,
                BoundaryCodes = 수출Cargo준비BoundaryCodes(),
                SourceStableIds = Copy(preview.Decision.SourceStableIds),
            };
        }

        private void Schedule수출Cargo준비(Simulation수출Cargo준비Snapshot? cargoPreparation)
        {
            if (cargoPreparation == null) return;
            var source = 수출준비원장[cargoPreparation.SourceExportPreparationStableId];
            source.CargoPreparationStableId = cargoPreparation.CargoPreparationStableId;
            source.CargoStableId = cargoPreparation.CargoStableId;
            source.Revision++;
            수출Cargo준비원장.Add(cargoPreparation.CargoPreparationStableId, cargoPreparation);
            수출Cargo준비출처연결.Add(
                cargoPreparation.SourceExportPreparationStableId,
                cargoPreparation.CargoPreparationStableId);
        }

        private void Advance수출Cargo준비ForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var cargoPreparation = 수출Cargo준비원장.Values.FirstOrDefault(
                value => value.TaskStableId == task.TaskStableId
                    && value.StateCode == Simulation수출Cargo준비상태Codes.Scheduled);
            if (cargoPreparation == null || currentTick < task.ExpectedEndTick) return;
            cargoPreparation.StateCode = Simulation수출Cargo준비상태Codes.ReadyForHandoff;
            cargoPreparation.Revision++;
            cargoPreparation.ReadyForHandoffTick = task.ExpectedEndTick;
        }

        private Simulation수출Cargo준비Snapshot[] Create수출Cargo준비Snapshots()
            => 수출Cargo준비원장.Values
                .OrderBy(value => value.CargoPreparationStableId, StringComparer.Ordinal)
                .Select(Clone수출Cargo준비).ToArray();

        internal static Simulation수출Cargo준비Snapshot Clone수출Cargo준비(
            Simulation수출Cargo준비Snapshot source)
            => new Simulation수출Cargo준비Snapshot
            {
                CargoPreparationStableId = source.CargoPreparationStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                SourceExportPreparationStableId = source.SourceExportPreparationStableId,
                RootExportPreparationStableId = source.RootExportPreparationStableId,
                ExportPreparationAttemptNumber = source.ExportPreparationAttemptNumber,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                CargoStableId = source.CargoStableId,
                CargoRevision = source.CargoRevision,
                RouteStableId = source.RouteStableId,
                OriginFacilityStableId = source.OriginFacilityStableId,
                DestinationFacilityStableId = source.DestinationFacilityStableId,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                RequiredPreparationTicks = source.RequiredPreparationTicks,
                ScheduledTick = source.ScheduledTick,
                ReadyForHandoffTick = source.ReadyForHandoffTick,
                BoundaryCodes = Copy(source.BoundaryCodes),
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static string[] 수출Cargo준비BoundaryCodes()
            => new[]
            {
                "SimulationOnly",
                "NoOperationalCarrierHandoff",
                "NoOperationalDispatch",
                "NoVehicleDeparture",
                "NoExportDeclaration",
                "NoCustomsClearance",
            };

        private static void Validate수출Cargo준비Request(
            Simulation수출Cargo준비PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CargoPreparationStableId,
                "SimulationExportCargoPreparationStableIdInvalid");
            RequireStableId(request.SourceExportPreparationStableId,
                "SimulationExportCargoSourcePreparationStableIdInvalid");
            RequireStableId(request.CargoStableId, "SimulationExportCargoStableIdInvalid");
            if (request.CargoRevision <= 0)
                throw new SimulationContractException("SimulationExportCargoRevisionInvalid");
            RequireStableId(request.RouteStableId, "SimulationExportCargoRouteStableIdInvalid");
            RequireStableId(request.DestinationFacilityStableId,
                "SimulationExportCargoDestinationFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationExportCargoActorStableIdInvalid");
            var distinctTargets = new HashSet<string>(StringComparer.Ordinal)
            {
                request.CargoPreparationStableId.Trim(),
                request.SourceExportPreparationStableId.Trim(),
                request.CargoStableId.Trim(),
                request.DestinationFacilityStableId.Trim(),
            };
            if (distinctTargets.Count != 4)
                throw new SimulationContractException("SimulationExportCargoTargetStableIdsMustDiffer");
            if (request.RequiredPreparationTicks <= 0 || request.RequiredPreparationTicks > 28)
                throw new SimulationContractException("SimulationExportCargoDurationInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationExportCargoSourceStableIdsInvalid");
        }
    }
}
