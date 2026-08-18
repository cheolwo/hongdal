using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public SimulationNatureEmergencyRetreatPreviewSnapshot
            PreviewNatureEmergencyRetreat(
                SimulationNatureEmergencyRetreatPreviewRequest request)
        {
            ValidateNatureEmergencyRetreatPreview(request);
            lock (gate)
            {
                var routeCode = request.NatureRouteCode.Trim();
                var routeStableId = "nature-route:" + routeCode;
                var observedEffects = effects.Values.Where(value =>
                    value.EffectTypeCode == SimulationNatureInteractionCodes.NatureThreatObserved
                    && value.TargetLedgerStableId == routeStableId
                    && value.StateCode == SimulationEffectStateCodes.Applied).ToArray();
                var activeEncounters = natureThreatEncounters.Values.Where(value =>
                    value.NatureRouteCode == routeCode
                    && value.StateCode == SimulationRegionalIncidentCodes.Active).ToArray();
                var decisionRequest = BuildNatureEmergencyRetreatDecision(request,
                    observedEffects, activeEncounters, includeExpectedRevisionBlock: true);
                var preview = CreateDecisionPreview(decisionRequest);
                return new SimulationNatureEmergencyRetreatPreviewSnapshot
                {
                    SessionStableId = SessionStableId,
                    NatureRouteCode = routeCode,
                    HasObservedThreat = observedEffects.Length > 0,
                    HasActiveEncounter = activeEncounters.Length > 0,
                    NextWorldInteractionIds = new[] { "WI-NATURE-04" },
                    DecisionPreview = preview,
                    CanConfirm = preview.Decision.BlockReasonCodes.Length == 0,
                    BlockingReasonCodes = preview.Decision.BlockReasonCodes.ToArray(),
                    SimulationOnly = true,
                    IsOperationalState = false,
                };
            }
        }

        public 경영SimulationSessionSnapshot ConfirmNatureEmergencyRetreat(
            SimulationNatureEmergencyRetreatConfirmRequest request)
        {
            ValidateNatureEmergencyRetreatConfirm(request);
            lock (gate)
            {
                if (appliedRegionalIncidentResponseCommands.ContainsKey(
                    request.CommandId.Trim()))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                var routeCode = request.Preview.NatureRouteCode.Trim();
                var routeStableId = "nature-route:" + routeCode;
                var observedEffects = effects.Values.Where(value =>
                    value.EffectTypeCode == SimulationNatureInteractionCodes.NatureThreatObserved
                    && value.TargetLedgerStableId == routeStableId
                    && value.StateCode == SimulationEffectStateCodes.Applied).ToArray();
                var activeEncounters = natureThreatEncounters.Values.Where(value =>
                    value.NatureRouteCode == routeCode
                    && value.StateCode == SimulationRegionalIncidentCodes.Active).ToArray();
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId.Trim(),
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = BuildNatureEmergencyRetreatDecision(request.Preview,
                        observedEffects, activeEncounters,
                        includeExpectedRevisionBlock: false),
                });
            }
        }

        private SimulationDecisionPreviewRequest BuildNatureEmergencyRetreatDecision(
            SimulationNatureEmergencyRetreatPreviewRequest request,
            SimulationEffectRecord[] observedEffects,
            SimulationNatureThreatEncounterSnapshot[] activeEncounters,
            bool includeExpectedRevisionBlock)
        {
            var routeCode = request.NatureRouteCode.Trim();
            var routeExists = CreateNatureThreatStateSnapshot().Routes.Any(value =>
                value.NatureRouteCode == routeCode);
            var routeStableId = "nature-route:" + routeCode;
            var sourceIds = observedEffects.Select(value => value.EffectStableId)
                .Concat(activeEncounters.Select(value => value.EncounterStableId))
                .DefaultIfEmpty(routeStableId).ToArray();
            var blockReasonCodes = routeExists
                ? Array.Empty<string>()
                : new[] { "NatureThreatRouteUnavailable" };
            if (routeExists && observedEffects.Length == 0 && activeEncounters.Length == 0)
                blockReasonCodes = blockReasonCodes.Concat(
                    new[] { "NatureThreatObservationRequired" }).ToArray();
            if (includeExpectedRevisionBlock && request.ExpectedRevision != Revision)
                blockReasonCodes = blockReasonCodes.Concat(
                    new[] { "SimulationExpectedRevisionMismatch" }).ToArray();

            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = request.DecisionStableId.Trim(),
                DecisionTypeCode = SimulationNatureInteractionCodes.EmergencyRetreat,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { routeStableId },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = SimulationNatureInteractionCodes.PartyRetreatedToSafeCore,
                        TargetLedgerStableId = routeStableId + ":party",
                        BeforeValue = 0m,
                        Delta = 1m,
                        AfterValue = 1m,
                        UnitCode = SimulationNatureInteractionCodes.PartyStateUnit,
                        SourceStableIds = sourceIds,
                    },
                },
                Uncertainties = Array.Empty<string>(),
                BlockReasonCodes = blockReasonCodes,
                SourceStableIds = sourceIds,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = request.TaskStableId.Trim(),
                    TaskTypeCode = SimulationNatureInteractionCodes.EmergencyRetreatTask,
                    FacilityStableId = SimulationNatureInteractionCodes.NatureHomeFacility,
                    ActionCode = SimulationNatureInteractionCodes.EmergencyRetreat,
                    AssignedActorStableId = request.ActorStableId.Trim(),
                    PreferredSpatialStableId = request.PreferredSpatialStableId.Trim(),
                    AssignedCapacity = 1m,
                    AssignedCapacityUnitCode = "party",
                    DurationTicks = 1,
                    InputLotStableIds = new[] { routeStableId },
                    OutputCandidateCodes = new[]
                    {
                        SimulationNatureInteractionCodes.RetreatedToSafeCore,
                    },
                    SourceStableIds = sourceIds,
                },
            };
        }

        private static void ValidateNatureEmergencyRetreatPreview(
            SimulationNatureEmergencyRetreatPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.DecisionStableId,
                "SimulationNatureRetreatDecisionIdInvalid");
            RequireStableId(request.TaskStableId,
                "SimulationNatureRetreatTaskIdInvalid");
            RequireStableId(request.ActorStableId,
                "SimulationNatureRetreatActorIdInvalid");
            RequireStableId(request.NatureRouteCode,
                "SimulationNatureRetreatRouteInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationNatureRetreatSpatialIdInvalid");
        }

        private static void ValidateNatureEmergencyRetreatConfirm(
            SimulationNatureEmergencyRetreatConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.Preview == null)
                throw new SimulationContractException("SimulationNatureRetreatPreviewMissing");
            ValidateNatureEmergencyRetreatPreview(request.Preview);
            if (request.Preview.ExpectedRevision != request.ExpectedRevision)
                throw new SimulationContractException(
                    "SimulationNatureRetreatRevisionMismatch");
        }
    }
}
