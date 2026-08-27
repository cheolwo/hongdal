using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public SimulationNatureThreatObservationPreviewSnapshot
            PreviewNatureThreatObservation(
                SimulationNatureThreatObservationPreviewRequest request)
        {
            ValidateNatureThreatObservationPreview(request);
            lock (gate)
            {
                var decisionRequest = BuildNatureThreatObservationDecision(
                    request, includeExpectedRevisionBlock: true);
                var preview = CreateDecisionPreview(decisionRequest);
                var source = ResolveNatureThreatObservationSource(
                    request.NatureRouteCode.Trim());
                return new SimulationNatureThreatObservationPreviewSnapshot
                {
                    SessionStableId = SessionStableId,
                    NatureRouteCode = request.NatureRouteCode.Trim(),
                    EffectivePressure = source.EffectivePressure,
                    PressureLevelCode = source.PressureLevelCode,
                    SourceIncidentStableIds = source.SourceStableIds,
                    NextWorldInteractionIds = source.IsTwilightEncounter
                        ? new[] { "WI-NATURE-11", "WI-NATURE-02" }
                        : new[] { "WI-NATURE-02", "WI-NATURE-03" },
                    DecisionPreview = preview,
                    CanConfirm = preview.Decision.BlockReasonCodes.Length == 0,
                    BlockingReasonCodes = preview.Decision.BlockReasonCodes.ToArray(),
                    SimulationOnly = true,
                    IsOperationalState = false,
                };
            }
        }

        public 경영SimulationSessionSnapshot ConfirmNatureThreatObservation(
            SimulationNatureThreatObservationConfirmRequest request)
        {
            ValidateNatureThreatObservationConfirm(request);
            lock (gate)
            {
                if (appliedRegionalIncidentResponseCommands.ContainsKey(
                    request.CommandId.Trim()))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                var decisionRequest = BuildNatureThreatObservationDecision(
                    request.Preview, includeExpectedRevisionBlock: false);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId.Trim(),
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = decisionRequest,
                });
            }
        }

        private SimulationDecisionPreviewRequest BuildNatureThreatObservationDecision(
            SimulationNatureThreatObservationPreviewRequest request,
            bool includeExpectedRevisionBlock)
        {
            var routeCode = request.NatureRouteCode.Trim();
            var source = ResolveNatureThreatObservationSource(routeCode);
            var preferredSpatialStableId = request.PreferredSpatialStableId.Trim();
            var facilityStableId = spatialDefinitions.TryGetValue(
                preferredSpatialStableId, out var preferredSpatial)
                ? preferredSpatial.FacilityStableId
                : SimulationNatureInteractionCodes.NatureHomeFacility;
            var routeStableId = "nature-route:" + routeCode;
            var sourceIds = source.SourceStableIds.Length > 0
                ? source.SourceStableIds : new[] { routeStableId };
            var blockReasonCodes = !source.IsAvailable
                ? new[] { "NatureThreatRouteUnavailable" }
                : Array.Empty<string>();
            if (includeExpectedRevisionBlock && request.ExpectedRevision != Revision)
                blockReasonCodes = blockReasonCodes.Concat(
                    new[] { "SimulationExpectedRevisionMismatch" }).ToArray();

            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = request.DecisionStableId.Trim(),
                DecisionTypeCode = SimulationNatureInteractionCodes.RegionalThreatObservation,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { routeStableId },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = SimulationNatureInteractionCodes.NatureThreatObserved,
                        TargetLedgerStableId = routeStableId,
                        BeforeValue = source.EffectivePressure,
                        Delta = 0,
                        AfterValue = source.EffectivePressure,
                        UnitCode = SimulationNatureInteractionCodes.PressurePointUnit,
                        SourceStableIds = sourceIds,
                    },
                },
                Uncertainties = Array.Empty<string>(),
                BlockReasonCodes = blockReasonCodes,
                SourceStableIds = sourceIds,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = request.TaskStableId.Trim(),
                    TaskTypeCode = SimulationNatureInteractionCodes.ThreatObservationTask,
                    FacilityStableId = facilityStableId,
                    ActionCode = SimulationNatureInteractionCodes.RegionalThreatObservation,
                    AssignedActorStableId = request.ActorStableId.Trim(),
                    PreferredSpatialStableId = preferredSpatialStableId,
                    AssignedCapacity = 1m,
                    AssignedCapacityUnitCode = "slot",
                    DurationTicks = 1,
                    InputLotStableIds = new[] { routeStableId },
                    OutputCandidateCodes = new[]
                    {
                        SimulationNatureInteractionCodes.ThreatObserved,
                    },
                    SourceStableIds = sourceIds,
                },
            };
        }

        private NatureThreatObservationSource ResolveNatureThreatObservationSource(
            string routeCode)
        {
            var route = CreateNatureThreatStateSnapshot().Routes.FirstOrDefault(value =>
                value.NatureRouteCode == routeCode);
            if (route != null)
                return new NatureThreatObservationSource(true, false,
                    route.EffectivePressure, route.PressureLevelCode,
                    route.SourceIncidentStableIds.ToArray());
            var twilight = routeCode ==
                SimulationNatureInteractionCodes.NatureHomeTwilightRoute
                && natureSurvivalCreationState != null
                && natureEncounter != null
                && (natureEncounter.StateCode == SimulationNatureSurvivalCodes.Pending
                    || natureEncounter.StateCode ==
                    SimulationNatureSurvivalCodes.CombatActive);
            if (!twilight)
                return new NatureThreatObservationSource(false, false, 0,
                    string.Empty, Array.Empty<string>());
            var pressure = Math.Max(0, natureEncounter!.EffectiveThreatTier);
            var level = pressure switch
            {
                0 => SimulationNatureThreatCodes.Stable,
                1 => SimulationNatureThreatCodes.Warning,
                2 => SimulationNatureThreatCodes.Threatened,
                _ => SimulationNatureThreatCodes.Infested,
            };
            return new NatureThreatObservationSource(true, true, pressure, level,
                new[] { natureEncounter.EncounterStableId });
        }

        private sealed class NatureThreatObservationSource
        {
            public NatureThreatObservationSource(bool isAvailable,
                bool isTwilightEncounter, int effectivePressure,
                string pressureLevelCode, string[] sourceStableIds)
            {
                IsAvailable = isAvailable;
                IsTwilightEncounter = isTwilightEncounter;
                EffectivePressure = effectivePressure;
                PressureLevelCode = pressureLevelCode;
                SourceStableIds = sourceStableIds;
            }

            public bool IsAvailable { get; }
            public bool IsTwilightEncounter { get; }
            public int EffectivePressure { get; }
            public string PressureLevelCode { get; }
            public string[] SourceStableIds { get; }
        }

        private static void ValidateNatureThreatObservationPreview(
            SimulationNatureThreatObservationPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.DecisionStableId,
                "SimulationNatureObservationDecisionIdInvalid");
            RequireStableId(request.TaskStableId,
                "SimulationNatureObservationTaskIdInvalid");
            RequireStableId(request.ActorStableId,
                "SimulationNatureObservationActorIdInvalid");
            RequireStableId(request.NatureRouteCode,
                "SimulationNatureObservationRouteInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationNatureObservationSpatialIdInvalid");
        }

        private static void ValidateNatureThreatObservationConfirm(
            SimulationNatureThreatObservationConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.Preview == null)
                throw new SimulationContractException(
                    "SimulationNatureObservationPreviewMissing");
            ValidateNatureThreatObservationPreview(request.Preview);
            if (request.Preview.ExpectedRevision != request.ExpectedRevision)
                throw new SimulationContractException(
                    "SimulationNatureObservationRevisionMismatch");
        }
    }
}
