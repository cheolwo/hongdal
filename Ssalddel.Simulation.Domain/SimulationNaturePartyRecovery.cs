using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public SimulationNaturePartyRecoveryPreviewSnapshot PreviewNaturePartyRecovery(
            SimulationNaturePartyRecoveryPreviewRequest request)
        {
            ValidateNaturePartyRecoveryPreview(request);
            lock (gate)
            {
                var context = ResolveNaturePartyRecoveryContext(request);
                var decisionRequest = BuildNaturePartyRecoveryDecision(request, context,
                    includeExpectedRevisionBlock: true);
                var preview = CreateDecisionPreview(decisionRequest);
                return new SimulationNaturePartyRecoveryPreviewSnapshot
                {
                    SessionStableId = SessionStableId,
                    NatureRouteCode = request.NatureRouteCode.Trim(),
                    HasRetreatPredecessor = context.RetreatEffects.Length > 0,
                    HasRestorationPredecessor = context.RestorationEffects.Length > 0,
                    NextPlayerActionCode = "Explore",
                    DecisionPreview = preview,
                    CanConfirm = preview.Decision.BlockReasonCodes.Length == 0,
                    BlockingReasonCodes = preview.Decision.BlockReasonCodes.ToArray(),
                    SimulationOnly = true,
                    IsOperationalState = false,
                };
            }
        }

        public 경영SimulationSessionSnapshot ConfirmNaturePartyRecovery(
            SimulationNaturePartyRecoveryConfirmRequest request)
        {
            ValidateNaturePartyRecoveryConfirm(request);
            lock (gate)
            {
                if (appliedRegionalIncidentResponseCommands.ContainsKey(
                    request.CommandId.Trim()))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                var context = ResolveNaturePartyRecoveryContext(request.Preview);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId.Trim(),
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = BuildNaturePartyRecoveryDecision(request.Preview, context,
                        includeExpectedRevisionBlock: false),
                });
            }
        }

        private NaturePartyRecoveryContext ResolveNaturePartyRecoveryContext(
            SimulationNaturePartyRecoveryPreviewRequest request)
        {
            var routeCode = request.NatureRouteCode.Trim();
            var routeStableId = "nature-route:" + routeCode;
            var retreatEffects = effects.Values.Where(value =>
                value.EffectTypeCode == SimulationNatureInteractionCodes.PartyRetreatedToSafeCore
                && value.TargetLedgerStableId == routeStableId + ":party"
                && value.StateCode == SimulationEffectStateCodes.Applied).ToArray();
            var restorationEffects = effects.Values.Where(value =>
                value.EffectTypeCode == SimulationNatureInteractionCodes.NatureRouteRestored
                && value.TargetLedgerStableId == routeStableId + ":restoration"
                && value.StateCode == SimulationEffectStateCodes.Applied).ToArray();
            var recoveryTarget = routeStableId + ":party:" + request.ActorStableId.Trim();
            var alreadyRecovered = effects.Values.Any(value =>
                value.EffectTypeCode == SimulationNatureInteractionCodes.PartyRecovered
                && value.TargetLedgerStableId == recoveryTarget
                && value.StateCode == SimulationEffectStateCodes.Applied);
            return new NaturePartyRecoveryContext(
                CreateNatureThreatStateSnapshot().Routes.Any(value =>
                    value.NatureRouteCode == routeCode),
                retreatEffects, restorationEffects, alreadyRecovered);
        }

        private SimulationDecisionPreviewRequest BuildNaturePartyRecoveryDecision(
            SimulationNaturePartyRecoveryPreviewRequest request,
            NaturePartyRecoveryContext context,
            bool includeExpectedRevisionBlock)
        {
            var routeCode = request.NatureRouteCode.Trim();
            var routeStableId = "nature-route:" + routeCode;
            var recoveryTarget = routeStableId + ":party:" + request.ActorStableId.Trim();
            var blocks = context.RouteExists
                ? Array.Empty<string>()
                : new[] { "NatureThreatRouteUnavailable" };
            if (context.RouteExists && context.RetreatEffects.Length == 0
                && context.RestorationEffects.Length == 0)
                blocks = blocks.Concat(new[] { "NatureRecoveryPrerequisiteMissing" }).ToArray();
            if (context.AlreadyRecovered)
                blocks = blocks.Concat(new[] { "PartyAlreadyRecovered" }).ToArray();
            if (includeExpectedRevisionBlock && request.ExpectedRevision != Revision)
                blocks = blocks.Concat(new[] { "SimulationExpectedRevisionMismatch" }).ToArray();
            var sourceIds = context.RetreatEffects.Select(value => value.EffectStableId)
                .Concat(context.RestorationEffects.Select(value => value.EffectStableId))
                .DefaultIfEmpty(routeStableId).ToArray();

            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = request.DecisionStableId.Trim(),
                DecisionTypeCode = SimulationNatureInteractionCodes.PartyRecovery,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { recoveryTarget },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = SimulationNatureInteractionCodes.PartyRecovered,
                        TargetLedgerStableId = recoveryTarget,
                        BeforeValue = 0m,
                        Delta = 1m,
                        AfterValue = 1m,
                        UnitCode = SimulationNatureInteractionCodes.PartyStateUnit,
                        SourceStableIds = sourceIds,
                    },
                },
                Uncertainties = Array.Empty<string>(),
                BlockReasonCodes = blocks,
                SourceStableIds = sourceIds,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = request.TaskStableId.Trim(),
                    TaskTypeCode = SimulationNatureInteractionCodes.PartyRecoveryTask,
                    FacilityStableId = SimulationNatureInteractionCodes.NatureHomeFacility,
                    ActionCode = SimulationNatureInteractionCodes.PartyRecovery,
                    AssignedActorStableId = request.ActorStableId.Trim(),
                    PreferredSpatialStableId = request.PreferredSpatialStableId.Trim(),
                    AssignedCapacity = 1m,
                    AssignedCapacityUnitCode = "party",
                    DurationTicks = 1,
                    InputLotStableIds = new[] { routeStableId },
                    OutputCandidateCodes = new[]
                    {
                        SimulationNatureInteractionCodes.PartyRecovered,
                    },
                    SourceStableIds = sourceIds,
                },
            };
        }

        private static void ValidateNaturePartyRecoveryPreview(
            SimulationNaturePartyRecoveryPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.DecisionStableId,
                "SimulationNatureRecoveryDecisionIdInvalid");
            RequireStableId(request.TaskStableId,
                "SimulationNatureRecoveryTaskIdInvalid");
            RequireStableId(request.ActorStableId,
                "SimulationNatureRecoveryActorIdInvalid");
            RequireStableId(request.NatureRouteCode,
                "SimulationNatureRecoveryRouteInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationNatureRecoverySpatialIdInvalid");
        }

        private static void ValidateNaturePartyRecoveryConfirm(
            SimulationNaturePartyRecoveryConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.Preview == null)
                throw new SimulationContractException("SimulationNatureRecoveryPreviewMissing");
            ValidateNaturePartyRecoveryPreview(request.Preview);
            if (request.Preview.ExpectedRevision != request.ExpectedRevision)
                throw new SimulationContractException("SimulationNatureRecoveryRevisionMismatch");
        }

        private sealed class NaturePartyRecoveryContext
        {
            public NaturePartyRecoveryContext(bool routeExists,
                SimulationEffectRecord[] retreatEffects,
                SimulationEffectRecord[] restorationEffects,
                bool alreadyRecovered)
            {
                RouteExists = routeExists;
                RetreatEffects = retreatEffects;
                RestorationEffects = restorationEffects;
                AlreadyRecovered = alreadyRecovered;
            }

            public bool RouteExists { get; }
            public SimulationEffectRecord[] RetreatEffects { get; }
            public SimulationEffectRecord[] RestorationEffects { get; }
            public bool AlreadyRecovered { get; }
        }
    }
}
