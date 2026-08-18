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
                var route = CreateNatureThreatStateSnapshot().Routes.FirstOrDefault(value =>
                    value.NatureRouteCode == request.NatureRouteCode.Trim());
                return new SimulationNatureThreatObservationPreviewSnapshot
                {
                    SessionStableId = SessionStableId,
                    NatureRouteCode = request.NatureRouteCode.Trim(),
                    EffectivePressure = route?.EffectivePressure ?? 0,
                    PressureLevelCode = route?.PressureLevelCode ?? string.Empty,
                    SourceIncidentStableIds = route?.SourceIncidentStableIds.ToArray()
                        ?? Array.Empty<string>(),
                    NextWorldInteractionIds = new[] { "WI-NATURE-02", "WI-NATURE-03" },
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
            var route = CreateNatureThreatStateSnapshot().Routes.FirstOrDefault(value =>
                value.NatureRouteCode == routeCode);
            var routeStableId = "nature-route:" + routeCode;
            var sourceIds = route?.SourceIncidentStableIds.Length > 0
                ? route.SourceIncidentStableIds.ToArray()
                : new[] { routeStableId };
            var blockReasonCodes = route == null
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
                        BeforeValue = route?.EffectivePressure ?? 0,
                        Delta = 0,
                        AfterValue = route?.EffectivePressure ?? 0,
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
                    FacilityStableId = SimulationNatureInteractionCodes.NatureHomeFacility,
                    ActionCode = SimulationNatureInteractionCodes.RegionalThreatObservation,
                    AssignedActorStableId = request.ActorStableId.Trim(),
                    PreferredSpatialStableId = request.PreferredSpatialStableId.Trim(),
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
