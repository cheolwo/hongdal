using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class SimulationRegionalIncidentService
    {
        private readonly 경영SimulationSessionAccessor sessions;
        private readonly I세계상호작용실행Pipeline worldInteractions;

        public SimulationRegionalIncidentService(경영SimulationSessionAccessor sessions,
            I세계상호작용실행Pipeline? worldInteractionPipeline = null)
        {
            this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            worldInteractions = worldInteractionPipeline
                ?? new 세계상호작용실행Pipeline();
        }

        public SimulationRegionalIncidentResponsePreviewSnapshot Preview(
            string sessionStableId, string eventStableId,
            SimulationRegionalIncidentResponsePreviewRequest request)
            => sessions.Require(sessionStableId)
                .PreviewRegionalIncidentResponse(eventStableId, request);

        public 경영SimulationSessionSnapshot Confirm(
            string sessionStableId, string eventStableId,
            SimulationRegionalIncidentResponseConfirmRequest request)
            => sessions.Require(sessionStableId)
                .ConfirmRegionalIncidentResponse(eventStableId, request);

        public SimulationNatureThreatObservationPreviewSnapshot PreviewThreatObservation(
            string sessionStableId,
            SimulationNatureThreatObservationPreviewRequest request)
            => sessions.Require(sessionStableId)
                .PreviewNatureThreatObservation(request);

        public 경영SimulationSessionSnapshot ConfirmThreatObservation(
            string sessionStableId,
            SimulationNatureThreatObservationConfirmRequest request)
        {
            var aggregate = sessions.Require(sessionStableId);
            return ExecuteNature(aggregate, "WI-NATURE-01", request.CommandId,
                request.Preview.ActorStableId, request.Preview.NatureRouteCode,
                request.Preview.TaskStableId,
                new[] { "WI-NATURE-02", "WI-NATURE-03" },
                () => aggregate.ConfirmNatureThreatObservation(request));
        }

        public SimulationNatureEmergencyRetreatPreviewSnapshot PreviewEmergencyRetreat(
            string sessionStableId,
            SimulationNatureEmergencyRetreatPreviewRequest request)
            => sessions.Require(sessionStableId)
                .PreviewNatureEmergencyRetreat(request);

        public 경영SimulationSessionSnapshot ConfirmEmergencyRetreat(
            string sessionStableId,
            SimulationNatureEmergencyRetreatConfirmRequest request)
        {
            var aggregate = sessions.Require(sessionStableId);
            return ExecuteNature(aggregate, "WI-NATURE-02", request.CommandId,
                request.Preview.ActorStableId, request.Preview.NatureRouteCode,
                request.Preview.TaskStableId, new[] { "WI-NATURE-04" },
                () => aggregate.ConfirmNatureEmergencyRetreat(request));
        }

        public SimulationNatureRestorationPreviewSnapshot PreviewRestoration(
            string sessionStableId,
            SimulationNatureRestorationPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewNatureRestoration(request);

        public 경영SimulationSessionSnapshot ConfirmRestoration(
            string sessionStableId,
            SimulationNatureRestorationConfirmRequest request)
        {
            var aggregate = sessions.Require(sessionStableId);
            return ExecuteNature(aggregate, "WI-NATURE-03", request.CommandId,
                request.Preview.ActorStableId, request.Preview.NatureRouteCode,
                request.Preview.TaskStableId, new[] { "WI-NATURE-04" },
                () => aggregate.ConfirmNatureRestoration(request));
        }

        public SimulationNaturePartyRecoveryPreviewSnapshot PreviewPartyRecovery(
            string sessionStableId,
            SimulationNaturePartyRecoveryPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewNaturePartyRecovery(request);

        public 경영SimulationSessionSnapshot ConfirmPartyRecovery(
            string sessionStableId,
            SimulationNaturePartyRecoveryConfirmRequest request)
        {
            var aggregate = sessions.Require(sessionStableId);
            return ExecuteNature(aggregate, "WI-NATURE-04", request.CommandId,
                request.Preview.ActorStableId, request.Preview.NatureRouteCode,
                request.Preview.TaskStableId,
                new[] { "NatureExplorationRestart" },
                () => aggregate.ConfirmNaturePartyRecovery(request));
        }

        private 경영SimulationSessionSnapshot ExecuteNature(
            경영SimulationSessionAggregate aggregate,
            string worldInteractionId,
            string commandId,
            string actorStableId,
            string natureRouteCode,
            string taskStableId,
            string[] successors,
            Func<경영SimulationSessionSnapshot> authorityConfirm)
            => worldInteractions.ExecutePlayerDriven(aggregate,
                new 세계상호작용실행Context
                {
                    WorldInteractionId = worldInteractionId,
                    CommandId = commandId,
                    InitiatorStableId = actorStableId,
                    ActorStableId = actorStableId,
                    TargetStableId = "nature-route:" + natureRouteCode,
                    SourceReferenceIds = new[] { natureRouteCode },
                    TimeReferenceId = "simulation-time:world-tick",
                    PlayableLoopStableId =
                        "playable-loop:nature-twilight-return.v1",
                    SpatialEvidenceStateCode =
                        SimulationWorldInteractionSpatialEvidenceCodes.Bound,
                    SpatialEvidenceReferenceIds = new[]
                    {
                        "e9-wi-h:" + worldInteractionId.ToLowerInvariant(),
                    },
                    TaskOrEffectReferenceIds = new[] { taskStableId },
                    SuccessorOrReturnCodes = successors,
                }, authorityConfirm);
    }
}
