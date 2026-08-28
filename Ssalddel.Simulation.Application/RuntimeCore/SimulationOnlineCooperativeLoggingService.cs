using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Application,
        "JWT 참가자를 AreaSet Actor로 결속해 협동 벌목·집중·재접속을 조율한다.",
        StepKey = "application.online-cooperative-logging",
        DependsOnStepKeys = new[]
        {
            "application.online-nature-session-provision",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Confirm,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 32,
        Boundary = "Simulation 행위와 계정 명상만 변경하며 운영 상태와 Unity 표현을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "온라인 협동 벌목의 RemoteHost 권위 Adapter와 재접속 cursor를 제공한다.",
        Boundary = "서버 자동시험은 Unity 실제 다중 입력·Game View 증거를 대신하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2원격HostAdapter)]
    public sealed class SimulationOnlineCooperativeLoggingService
    {
        private readonly I경영SimulationSessionStore sessionStore;
        private readonly SimulationOnlineNatureSessionProvisioningService provisioning;
        private readonly SimulationOnlineMeditationBridgeService meditationBridge;
        private readonly SimulationNatureSurvivalService nature;

        public SimulationOnlineCooperativeLoggingService(
            I경영SimulationSessionStore sessionStore,
            SimulationOnlineWorldService onlineWorlds,
            SimulationOnlineNatureSessionProvisioningService provisioning,
            SimulationOnlineMeditationBridgeService meditationBridge)
        {
            this.sessionStore = sessionStore;
            _ = onlineWorlds ?? throw new ArgumentNullException(nameof(onlineWorlds));
            this.provisioning = provisioning;
            this.meditationBridge = meditationBridge;
            nature = new SimulationNatureSurvivalService(sessionStore,
                authorityLocationCode: SimulationOnlineWorldCodes.RemoteHost);
        }

        public SimulationOnlineLoggingResultSnapshot Begin(string player,
            SimulationOnlineLoggingBeginRequest request)
        {
            var context = Context(player, request.WorldStableId,
                request.ExpectedOnlineWorldRevision);
            nature.Confirm(context.SessionId, new()
            {
                CommandId = request.CommandId,
                ExpectedRevision = request.ExpectedSessionWorldRevision,
                PlayerStableId = context.ActorId,
                ActionCode = SimulationNatureSurvivalCodes.BeginHarvest,
                TargetStableId = request.TargetResourceStableId,
            });
            return Result(context, null, null);
        }

        public SimulationOnlineLoggingResultSnapshot Focus(string player,
            SimulationOnlineLoggingFocusRequest request)
        {
            var context = Context(player, request.WorldStableId,
                request.ExpectedOnlineWorldRevision);
            var active = nature.Get(context.SessionId).ActiveFocusChallenge;
            if (active == null || !string.Equals(active.PlayerStableId,
                    context.ActorId, StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationOnlineLoggingActorDoesNotOwnWork");
            nature.SubmitFocusTiming(context.SessionId, new()
            {
                CommandId = request.CommandId,
                ChallengeStableId = request.ChallengeStableId,
                ExpectedWorldRevision = request.ExpectedSessionWorldRevision,
                ExpectedChallengeRevision = request.ExpectedChallengeRevision,
                InputOffsetMillis = request.InputOffsetMillis,
            });
            return Result(context, null, null);
        }

        public SimulationOnlineLoggingResultSnapshot Complete(string player,
            SimulationOnlineLoggingCompleteRequest request)
        {
            var context = Context(player, request.WorldStableId,
                request.ExpectedOnlineWorldRevision);
            var active = nature.Get(context.SessionId).ActiveWork;
            if (active == null || !string.Equals(active.ActorStableId,
                    context.ActorId, StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationOnlineLoggingActorDoesNotOwnWork");
            nature.AdvanceClock(context.SessionId, new()
            {
                CommandId = request.CommandId,
                ExpectedRevision = request.ExpectedSessionWorldRevision,
                ElapsedRealtimeSeconds = active.RequiredWorkSeconds,
                WorkInputHeld = true,
            });
            var aggregate = sessionStore.Find(context.SessionId)!;
            var completed = aggregate.GetActionManifestationLedger()!
                .TailRecords.Single(value => value.CommandId ==
                    active.OriginCommandId + ":completed");
            var synced = meditationBridge.Sync(player,
                new SimulationOnlineMeditationSyncRequest
                {
                    WorldStableId = request.WorldStableId,
                    SessionStableId = context.SessionId,
                    SourceActionRecordStableId = completed.행위기록StableId,
                    ExpectedOnlineWorldRevision =
                        request.ExpectedOnlineWorldRevision,
                });
            return Result(context, completed, synced.AccountMeditation);
        }

        public SimulationOnlineLoggingReconnectSnapshot Reconnect(string player,
            SimulationOnlineLoggingReconnectRequest request)
        {
            var context = Context(player, request.WorldStableId,
                request.ExpectedOnlineWorldRevision);
            var aggregate = sessionStore.Find(context.SessionId)!;
            var ledger = aggregate.GetActionManifestationLedger();
            var page = aggregate.QueryActionManifestations(
                new Simulation행위기록Query
                {
                    WorldStableId = ledger?.WorldStableId
                        ?? aggregate.SessionStableId,
                    Cursor = request.Cursor,
                    WorldInteractionIds = new[]
                    {
                        SimulationNatureSurvivalCodes
                            .BeginHarvestWorldInteractionId,
                    },
                    MaxCount = request.MaxCount,
                });
            return new SimulationOnlineLoggingReconnectSnapshot
            {
                WorldStableId = context.WorldId,
                AreaSetStableId = context.AreaSetId,
                AuthoritySessionStableId = context.SessionId,
                ActorStableId = context.ActorId,
                SessionWorldRevision = aggregate.Revision,
                Nature = nature.Get(context.SessionId),
                ActionRecords = page,
            };
        }

        private OnlineContext Context(string player, string worldId,
            long expectedOnlineRevision)
        {
            var runtime = provisioning.Ensure(player,
                new SimulationOnlineAuthoritySessionProvisionRequest
                {
                    WorldStableId = worldId,
                    ExpectedOnlineWorldRevision = expectedOnlineRevision,
                });
            var binding = runtime.ParticipantActors.Single(value =>
                string.Equals(value.PlayerStableId, player,
                    StringComparison.Ordinal));
            return new OnlineContext(worldId, runtime.AreaSetStableId,
                runtime.AuthoritySessionStableId, player,
                binding.ActorStableId);
        }

        private SimulationOnlineLoggingResultSnapshot Result(
            OnlineContext context, Simulation행위발현Record? record,
            SimulationAccountMeditationSnapshot? account)
            => new SimulationOnlineLoggingResultSnapshot
            {
                WorldStableId = context.WorldId,
                AreaSetStableId = context.AreaSetId,
                AuthoritySessionStableId = context.SessionId,
                PlayerStableId = context.PlayerId,
                ActorStableId = context.ActorId,
                SessionWorldRevision = sessionStore.Find(context.SessionId)!
                    .Revision,
                Nature = nature.Get(context.SessionId),
                CompletedActionRecord = record,
                AccountMeditation = account,
                SimulationOnly = true,
                IsOperationalState = false,
            };

        private sealed class OnlineContext
        {
            public OnlineContext(string worldId, string areaSetId,
                string sessionId, string playerId, string actorId)
            {
                WorldId = worldId;
                AreaSetId = areaSetId;
                SessionId = sessionId;
                PlayerId = playerId;
                ActorId = actorId;
            }

            public string WorldId { get; }
            public string AreaSetId { get; }
            public string SessionId { get; }
            public string PlayerId { get; }
            public string ActorId { get; }
        }
    }
}
