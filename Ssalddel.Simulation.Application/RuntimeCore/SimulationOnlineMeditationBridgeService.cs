using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Nature 행위 기록과 플레이어 명상 기여를 검증해 온라인 계정 원장으로 인계한다.",
        Boundary = "클라이언트가 보상량을 정하지 않으며 자동 동기화·Unity 화면·운영 배포 증거를 대신하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2원격HostAdapter)]
    public sealed class SimulationOnlineMeditationBridgeService
    {
        private readonly I경영SimulationSessionStore sessionStore;
        private readonly SimulationOnlineWorldService onlineWorldService;

        public SimulationOnlineMeditationBridgeService(
            I경영SimulationSessionStore sessionStore,
            SimulationOnlineWorldService onlineWorldService)
        {
            this.sessionStore = sessionStore
                ?? throw new ArgumentNullException(nameof(sessionStore));
            this.onlineWorldService = onlineWorldService
                ?? throw new ArgumentNullException(nameof(onlineWorldService));
        }

        public SimulationOnlineWorldMutationResult Sync(
            string authenticatedPlayerStableId,
            SimulationOnlineMeditationSyncRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var player = Require(authenticatedPlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            var worldId = Require(request.WorldStableId,
                "SimulationOnlineWorldStableIdInvalid");
            var sessionId = Require(request.SessionStableId,
                "SimulationSessionStableIdInvalid");
            var actionRecordId = Require(request.SourceActionRecordStableId,
                "SimulationActionRecordStableIdInvalid");

            var world = onlineWorldService.GetWorld(worldId);
            var participant = world.Participants.SingleOrDefault(value =>
                string.Equals(value.PlayerStableId, player,
                    StringComparison.Ordinal)
                && value.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Connected)
                ?? throw new SimulationConflictException(
                    "SimulationOnlineVerifiedParticipantRequired");
            var area = world.AreaSets.Single(value =>
                string.Equals(value.AreaSetStableId,
                    participant.AreaSetStableId, StringComparison.Ordinal));
            if (!string.Equals(area.AuthorityLocationCode,
                    SimulationOnlineWorldCodes.RemoteHost,
                    StringComparison.Ordinal)
                || !string.Equals(area.AuthoritySessionStableId, sessionId,
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationOnlineAuthoritySessionMismatch");

            var session = sessionStore.Find(sessionId)
                ?? throw new SimulationConflictException(
                    "SimulationSessionNotActive");
            var ledger = session.GetActionManifestationLedger()
                ?? throw new SimulationConflictException(
                    "SimulationActionManifestationLedgerUnavailable");
            var onlineActor = SimulationOnlineNatureSessionProvisioningService
                .CalculateParticipantActorStableId(world.WorldStableId,
                    area.AreaSetStableId, player);
            var profile = session.GetPlayerDomainProfile(onlineActor)
                ?? session.GetPlayerDomainProfile(player)
                ?? throw new SimulationConflictException(
                    "SimulationPlayerDomainProfileUnavailable");

            var actionActor = profile.PlayerStableId;
            if (!string.Equals(actionActor, player, StringComparison.Ordinal)
                && !string.Equals(actionActor, onlineActor,
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationOnlineMeditationPlayerMismatch");

            var record = ledger.TailRecords.SingleOrDefault(value =>
                string.Equals(value.행위기록StableId, actionRecordId,
                    StringComparison.Ordinal))
                ?? throw new SimulationConflictException(
                    "SimulationOnlineMeditationActionRecordUnavailable");
            if (!string.Equals(record.SessionStableId, sessionId,
                    StringComparison.Ordinal)
                || !string.Equals(record.ActorStableId, actionActor,
                    StringComparison.Ordinal)
                || string.Equals(record.결과분류Code,
                    Simulation행위결과분류Codes.취소,
                    StringComparison.Ordinal)
                || !record.변화의미Codes.Contains(
                    Simulation행위변화의미Codes.플레이어명상변경,
                    StringComparer.Ordinal)
                || !string.Equals(record.기록HashSha256,
                    Simulation행위발현Ledger.CalculateRecordHash(record),
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationOnlineMeditationActionLineageInvalid");

            var contribution = profile.명상기여기록들.SingleOrDefault(value =>
                string.Equals(value.SourceActionRecordStableId,
                    actionRecordId, StringComparison.Ordinal))
                ?? throw new SimulationConflictException(
                    "SimulationOnlineMeditationContributionUnavailable");
            if (!string.Equals(contribution.PlayerStableId, actionActor,
                    StringComparison.Ordinal)
                || !string.Equals(contribution.WorldInteractionId,
                    record.WorldInteractionId, StringComparison.Ordinal)
                || contribution.AppliedWorldRevision !=
                    record.AfterWorldRevision
                || contribution.명상경험증가Milli <= 0
                || string.IsNullOrWhiteSpace(contribution.RuleRevision))
                throw new SimulationConflictException(
                    "SimulationOnlineMeditationContributionLineageInvalid");

            return onlineWorldService.ApplyVerifiedMeditation(
                new SimulationVerifiedMeditationContributionRequest
                {
                    WorldStableId = worldId,
                    PlayerStableId = player,
                    SourceActionRecordStableId = actionRecordId,
                    MeditationExperienceMilli =
                        contribution.명상경험증가Milli,
                    SourceActionWorldRevision = record.AfterWorldRevision,
                    ExpectedOnlineWorldRevision =
                        request.ExpectedOnlineWorldRevision,
                    RuleRevision = contribution.RuleRevision,
                });
        }

        private static string Require(string value, string code)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(code);
            return value.Trim();
        }
    }
}
