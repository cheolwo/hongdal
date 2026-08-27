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
            var preview = aggregate.PreviewNatureThreatObservation(
                request.Preview);
            return ExecuteNature(aggregate, "WI-NATURE-01", request.CommandId,
                request.Preview.ActorStableId, request.Preview.NatureRouteCode,
                request.Preview.TaskStableId,
                new[] { "WI-NATURE-02", "WI-NATURE-03" },
                preview.CanConfirm, preview.BlockingReasonCodes,
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
            var preview = aggregate.PreviewNatureEmergencyRetreat(
                request.Preview);
            return ExecuteNature(aggregate, "WI-NATURE-02", request.CommandId,
                request.Preview.ActorStableId, request.Preview.NatureRouteCode,
                request.Preview.TaskStableId, new[] { "WI-NATURE-04" },
                preview.CanConfirm, preview.BlockingReasonCodes,
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
            var preview = aggregate.PreviewNatureRestoration(request.Preview);
            return ExecuteNature(aggregate, "WI-NATURE-03", request.CommandId,
                request.Preview.ActorStableId, request.Preview.NatureRouteCode,
                request.Preview.TaskStableId, new[] { "WI-NATURE-04" },
                preview.CanConfirm, preview.BlockingReasonCodes,
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
            var preview = aggregate.PreviewNaturePartyRecovery(request.Preview);
            return ExecuteNature(aggregate, "WI-NATURE-04", request.CommandId,
                request.Preview.ActorStableId, request.Preview.NatureRouteCode,
                request.Preview.TaskStableId,
                new[] { "NatureExplorationRestart" },
                preview.CanConfirm, preview.BlockingReasonCodes,
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
            bool canConfirm,
            string[] blockingReasonCodes,
            Func<경영SimulationSessionSnapshot> authorityConfirm)
        {
            var context = new 세계상호작용실행Context
                {
                    WorldInteractionId = worldInteractionId,
                    CommandId = commandId,
                    // 지역 사건의 Actor는 정찰대·파티처럼 매 단계 달라질 수 있다.
                    // 분야 Profile의 주체는 세션 소유 플레이어로 고정하고,
                    // 실제 수행자는 ActorStableId로 별도 보존한다.
                    InitiatorStableId = aggregate.GetPlayerDomainProfile()
                        ?.PlayerStableId
                        ?? SimulationNatureMindCodes.DefaultPlayerStableId,
                    ActorStableId = actorStableId,
                    TargetStableId = "nature-route:" + natureRouteCode,
                    SourceReferenceIds = new[] { natureRouteCode },
                    TimeReferenceId = "simulation-time:world-tick",
                    PlayableLoopStableId =
                        "playable-loop:nature-regional-threat-recovery.v1",
                    AuthorityLocationCode = "RemoteHost",
                    SpatialEvidenceStateCode =
                        SimulationWorldInteractionSpatialEvidenceCodes.Bound,
                    SpatialEvidenceReferenceIds = new[]
                    {
                        "e9-wi-h:" + worldInteractionId.ToLowerInvariant(),
                    },
                    TaskOrEffectReferenceIds = new[] { taskStableId },
                    // Confirm은 Task 시작만 확정한다. 결과 상태는 Tick 완료 시
                    // 기존 E5 기록에 추가되어야 한다.
                    ResultStateCodes = Array.Empty<string>(),
                    SuccessorOrReturnCodes = successors,
                    PrimaryOutcomeCode = worldInteractionId + ":TaskStarted",
                    결과분류Code = worldInteractionId == "WI-NATURE-02"
                        ? Simulation행위결과분류Codes.후퇴복구
                        : Simulation행위결과분류Codes.성공,
                    변화의미Codes = worldInteractionId switch
                    {
                        "WI-NATURE-03" => new[]
                        {
                            Simulation행위변화의미Codes.지표변경,
                            Simulation행위변화의미Codes.통행변경,
                            Simulation행위변화의미Codes.실외배치변경,
                        },
                        _ => new[]
                        {
                            Simulation행위변화의미Codes.Actor상태변경,
                            Simulation행위변화의미Codes.통행변경,
                        },
                    },
                    SpatialRevision = aggregate.SpatialCompositionRuleRevision,
                };
            worldInteractions.RecordPreview(context, aggregate.Revision,
                canConfirm, blockingReasonCodes);
            return worldInteractions.ExecutePlayerDriven(aggregate, context,
                authorityConfirm);
        }
    }
}
