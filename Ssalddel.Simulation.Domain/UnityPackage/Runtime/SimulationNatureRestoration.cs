using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public SimulationNatureRestorationPreviewSnapshot PreviewNatureRestoration(
            SimulationNatureRestorationPreviewRequest request)
        {
            ValidateNatureRestorationPreview(request);
            lock (gate)
            {
                var context = ResolveNatureRestorationContext(request.NatureRouteCode);
                var decisionRequest = BuildNatureRestorationDecision(request, context,
                    includeExpectedRevisionBlock: true);
                var preview = CreateDecisionPreview(decisionRequest);
                return new SimulationNatureRestorationPreviewSnapshot
                {
                    SessionStableId = SessionStableId,
                    NatureRouteCode = request.NatureRouteCode.Trim(),
                    ResolvedCauseIncidentStableIds = context.CauseIncidents
                        .Where(value => value.StateCode == SimulationRegionalIncidentCodes.Resolved
                            && value.RemainingSeverity == 0)
                        .Select(value => value.IncidentStableId)
                        .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                    NextWorldInteractionIds = new[] { "WI-NATURE-04" },
                    DecisionPreview = preview,
                    CanConfirm = preview.Decision.BlockReasonCodes.Length == 0,
                    BlockingReasonCodes = preview.Decision.BlockReasonCodes.ToArray(),
                    SimulationOnly = true,
                    IsOperationalState = false,
                };
            }
        }

        public 경영SimulationSessionSnapshot ConfirmNatureRestoration(
            SimulationNatureRestorationConfirmRequest request)
        {
            ValidateNatureRestorationConfirm(request);
            lock (gate)
            {
                if (appliedRegionalIncidentResponseCommands.ContainsKey(
                    request.CommandId.Trim()))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                var context = ResolveNatureRestorationContext(
                    request.Preview.NatureRouteCode);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId.Trim(),
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = BuildNatureRestorationDecision(request.Preview, context,
                        includeExpectedRevisionBlock: false),
                });
            }
        }

        private NatureRestorationContext ResolveNatureRestorationContext(string routeCodeValue)
        {
            var routeCode = routeCodeValue.Trim();
            var routeStableId = "nature-route:" + routeCode;
            var route = CreateNatureThreatStateSnapshot().Routes.FirstOrDefault(value =>
                value.NatureRouteCode == routeCode);
            var observations = effects.Values.Where(value =>
                value.EffectTypeCode == SimulationNatureInteractionCodes.NatureThreatObserved
                && value.TargetLedgerStableId == routeStableId
                && value.StateCode == SimulationEffectStateCodes.Applied).ToArray();
            var observedCauseIds = observations.SelectMany(value => value.SourceStableIds)
                .Distinct(StringComparer.Ordinal).ToArray();
            var causes = regionalIncidents.Values.Where(value =>
                    value.NatureRouteCode == routeCode
                    && observedCauseIds.Contains(value.IncidentStableId, StringComparer.Ordinal))
                .ToArray();
            var alreadyRestored = effects.Values.Any(value =>
                value.EffectTypeCode == SimulationNatureInteractionCodes.NatureRouteRestored
                && value.TargetLedgerStableId == routeStableId + ":restoration"
                && value.StateCode == SimulationEffectStateCodes.Applied);
            return new NatureRestorationContext(route, observations, causes,
                alreadyRestored);
        }

        private SimulationDecisionPreviewRequest BuildNatureRestorationDecision(
            SimulationNatureRestorationPreviewRequest request,
            NatureRestorationContext context,
            bool includeExpectedRevisionBlock)
        {
            var routeCode = request.NatureRouteCode.Trim();
            var routeStableId = "nature-route:" + routeCode;
            var blocks = context.Route == null
                ? new[] { "NatureThreatRouteUnavailable" }
                : Array.Empty<string>();
            if (context.Route != null && context.Observations.Length == 0)
                blocks = blocks.Concat(new[] { "NatureThreatObservationRequired" }).ToArray();
            var causeResolved = context.CauseIncidents.Length > 0
                && context.CauseIncidents.All(value =>
                    value.StateCode == SimulationRegionalIncidentCodes.Resolved
                    && value.RemainingSeverity == 0)
                && context.Route?.RootRemainingSeverity == 0;
            if (context.Observations.Length > 0 && !causeResolved)
                blocks = blocks.Concat(new[] { "NatureIncidentCauseUnresolved" }).ToArray();
            if (context.AlreadyRestored)
                blocks = blocks.Concat(new[] { "NatureRouteAlreadyRestored" }).ToArray();
            if (includeExpectedRevisionBlock && request.ExpectedRevision != Revision)
                blocks = blocks.Concat(new[] { "SimulationExpectedRevisionMismatch" }).ToArray();
            var sourceIds = context.Observations.Select(value => value.EffectStableId)
                .Concat(context.CauseIncidents.Select(value => value.IncidentStableId))
                .DefaultIfEmpty(routeStableId).ToArray();

            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = request.DecisionStableId.Trim(),
                DecisionTypeCode = SimulationNatureInteractionCodes.NatureRestoration,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { routeStableId },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = SimulationNatureInteractionCodes.NatureRouteRestored,
                        TargetLedgerStableId = routeStableId + ":restoration",
                        BeforeValue = 0m,
                        Delta = 1m,
                        AfterValue = 1m,
                        UnitCode = SimulationNatureInteractionCodes.RestorationStateUnit,
                        SourceStableIds = sourceIds,
                    },
                },
                Uncertainties = Array.Empty<string>(),
                BlockReasonCodes = blocks,
                SourceStableIds = sourceIds,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = request.TaskStableId.Trim(),
                    TaskTypeCode = SimulationNatureInteractionCodes.NatureRestorationTask,
                    FacilityStableId = SimulationNatureInteractionCodes.NatureHomeFacility,
                    ActionCode = SimulationNatureInteractionCodes.NatureRestoration,
                    AssignedActorStableId = request.ActorStableId.Trim(),
                    PreferredSpatialStableId = request.PreferredSpatialStableId.Trim(),
                    AssignedCapacity = 1m,
                    AssignedCapacityUnitCode = "material-lot",
                    DurationTicks = 1,
                    InputLotStableIds = context.CauseIncidents
                        .Select(value => value.IncidentStableId).ToArray(),
                    OutputCandidateCodes = new[]
                    {
                        SimulationNatureInteractionCodes.NatureRouteRestored,
                    },
                    SourceStableIds = sourceIds,
                },
            };
        }

        private static void ValidateNatureRestorationPreview(
            SimulationNatureRestorationPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.DecisionStableId,
                "SimulationNatureRestorationDecisionIdInvalid");
            RequireStableId(request.TaskStableId,
                "SimulationNatureRestorationTaskIdInvalid");
            RequireStableId(request.ActorStableId,
                "SimulationNatureRestorationActorIdInvalid");
            RequireStableId(request.NatureRouteCode,
                "SimulationNatureRestorationRouteInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationNatureRestorationSpatialIdInvalid");
        }

        private static void ValidateNatureRestorationConfirm(
            SimulationNatureRestorationConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.Preview == null)
                throw new SimulationContractException("SimulationNatureRestorationPreviewMissing");
            ValidateNatureRestorationPreview(request.Preview);
            if (request.Preview.ExpectedRevision != request.ExpectedRevision)
                throw new SimulationContractException(
                    "SimulationNatureRestorationRevisionMismatch");
        }

        private sealed class NatureRestorationContext
        {
            public NatureRestorationContext(
                SimulationNatureThreatRouteSnapshot? route,
                SimulationEffectRecord[] observations,
                SimulationRegionalIncidentSnapshot[] causeIncidents,
                bool alreadyRestored)
            {
                Route = route;
                Observations = observations;
                CauseIncidents = causeIncidents;
                AlreadyRestored = alreadyRestored;
            }

            public SimulationNatureThreatRouteSnapshot? Route { get; }
            public SimulationEffectRecord[] Observations { get; }
            public SimulationRegionalIncidentSnapshot[] CauseIncidents { get; }
            public bool AlreadyRestored { get; }
        }
    }
}
