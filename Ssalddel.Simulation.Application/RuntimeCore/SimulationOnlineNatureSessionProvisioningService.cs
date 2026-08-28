using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Application,
        "온라인 AreaSet에 결속된 Nature RemoteHost 세션을 결정적으로 준비한다.",
        StepKey = "application.online-nature-session-provision",
        DependsOnStepKeys = new[] { "application.online-world" },
        ExecutionStage = SsalddelCodeExecutionStage.Confirm,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 31,
        Boundary = "현재 Nature Core는 단일 Actor만 지원한다. 세션 준비를 다중 Actor 협동 완료로 승격하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "온라인 AreaSet 식별자를 실제 프로세스 내부 RemoteHost 세션에 결속한다.",
        Boundary = "프로세스 내부 준비는 durable 세션 복원·다중 Actor·Unity 실제 입력 증거가 아니다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2원격HostAdapter)]
    public sealed class SimulationOnlineNatureSessionProvisioningService
    {
        private readonly object gate = new object();
        private readonly I경영SimulationSessionStore sessionStore;
        private readonly SimulationOnlineWorldService onlineWorldService;

        public SimulationOnlineNatureSessionProvisioningService(
            I경영SimulationSessionStore sessionStore,
            SimulationOnlineWorldService onlineWorldService)
        {
            this.sessionStore = sessionStore
                ?? throw new ArgumentNullException(nameof(sessionStore));
            this.onlineWorldService = onlineWorldService
                ?? throw new ArgumentNullException(nameof(onlineWorldService));
        }

        public SimulationOnlineAuthoritySessionRuntimeSnapshot Ensure(
            string authenticatedPlayerStableId,
            SimulationOnlineAuthoritySessionProvisionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            Require(request.WorldStableId, nameof(request.WorldStableId),
                "SimulationOnlineWorldStableIdRequired");
            var player = Require(authenticatedPlayerStableId,
                nameof(authenticatedPlayerStableId),
                "SimulationAuthenticatedPlayerRequired");
            lock (gate)
            {
                var world = onlineWorldService.GetWorld(request.WorldStableId);
                if (world.WorldRevision != request.ExpectedOnlineWorldRevision)
                    throw new SimulationConflictException(
                        "SimulationOnlineWorldRevisionConflict");

                var participant = world.Participants.SingleOrDefault(value =>
                    string.Equals(value.PlayerStableId, player,
                        StringComparison.Ordinal)
                    && string.Equals(value.ParticipantStateCode,
                        SimulationOnlineWorldCodes.Connected,
                        StringComparison.Ordinal));
                if (participant == null)
                    throw new SimulationContractException(
                        "SimulationOnlineConnectedParticipantRequired");

                var areaSet = world.AreaSets.Single(value => string.Equals(
                    value.AreaSetStableId, participant.AreaSetStableId,
                    StringComparison.Ordinal));
                if (!string.Equals(areaSet.AuthorityLocationCode,
                        SimulationOnlineWorldCodes.RemoteHost,
                        StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(
                        areaSet.AuthoritySessionStableId))
                {
                    throw new SimulationContractException(
                        "SimulationOnlineAuthoritySessionUnavailable");
                }

                var aggregate = sessionStore.Find(
                    areaSet.AuthoritySessionStableId);
                if (aggregate == null)
                {
                    var primaryActor = CalculatePrimaryActorStableId(
                        world.WorldStableId, areaSet.AreaSetStableId);
                    aggregate = sessionStore.CreateOrGet(CreateNatureRequest(
                        areaSet.AuthoritySessionStableId,
                        areaSet.AreaSetStableId, primaryActor));
                }

                var participantBindings = world.Participants
                    .Where(value => string.Equals(value.AreaSetStableId,
                            areaSet.AreaSetStableId, StringComparison.Ordinal)
                        && string.Equals(value.ParticipantStateCode,
                            SimulationOnlineWorldCodes.Connected,
                            StringComparison.Ordinal))
                    .OrderBy(value => value.PlayerStableId,
                        StringComparer.Ordinal)
                    .Select(value => new
                    {
                        Participant = value,
                        ActorStableId = CalculateParticipantActorStableId(
                            world.WorldStableId, areaSet.AreaSetStableId,
                            value.PlayerStableId),
                    }).ToArray();
                foreach (var binding in participantBindings)
                    aggregate.RegisterNatureCooperativeActor(
                        binding.ActorStableId);
                var snapshot = aggregate.Snapshot();
                if (!string.Equals(aggregate.SessionStableId,
                        areaSet.AuthoritySessionStableId,
                        StringComparison.Ordinal)
                    || !string.Equals(snapshot.NatureSurvival.AreaSetStableId,
                        areaSet.AreaSetStableId, StringComparison.Ordinal))
                {
                    throw new SimulationConflictException(
                        "SimulationOnlineAuthoritySessionPayloadConflict");
                }
                return new SimulationOnlineAuthoritySessionRuntimeSnapshot
                {
                    WorldStableId = world.WorldStableId,
                    AreaSetStableId = areaSet.AreaSetStableId,
                    AuthoritySessionStableId = aggregate.SessionStableId,
                    AuthorityLocationCode = areaSet.AuthorityLocationCode,
                    RuntimeStateCode = SimulationOnlineWorldCodes
                        .AuthoritySessionRuntimeReadyCooperativeLogging,
                    PrimaryActorStableId = snapshot.NatureSurvival.PlayerStableId,
                    ParticipantActors = participantBindings
                        .Select(value => new
                            SimulationOnlineParticipantActorBindingSnapshot
                            {
                                PlayerStableId =
                                    value.Participant.PlayerStableId,
                                ActorStableId = value.ActorStableId,
                                AreaSetStableId = areaSet.AreaSetStableId,
                                AuthoritySessionStableId =
                                    areaSet.AuthoritySessionStableId,
                                RegistrationStateCode =
                                    SimulationOnlineWorldCodes
                                        .ParticipantActorRegistered,
                                HasAuthorityInventory = true,
                                CanExecuteNatureWorldInteraction = true,
                                SourceParticipantRevision =
                                    value.Participant.LastChangedAtWorldRevision,
                            }).ToArray(),
                    SupportsMultipleActors = true,
                    SourceOnlineWorldRevision = world.WorldRevision,
                    SourceAreaSetRevision = areaSet.PartitionRevision,
                    SessionWorldRevision = snapshot.Revision,
                    SimulationOnly = true,
                    IsOperationalState = false,
                };
            }
        }

        public static string CalculatePrimaryActorStableId(
            string worldStableId, string areaSetStableId)
        {
            var canonical = Require(worldStableId, nameof(worldStableId),
                "SimulationOnlineWorldStableIdRequired") + "\n"
                + Require(areaSetStableId, nameof(areaSetStableId),
                    "SimulationOnlineAreaSetStableIdRequired") + "\n"
                + SimulationOnlineWorldCodes.RuleRevision;
            using var sha = SHA256.Create();
            var hash = BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
            return "actor:online-area-set:" + hash.Substring(0, 24);
        }

        public static string CalculateParticipantActorStableId(
            string worldStableId, string areaSetStableId,
            string playerStableId)
        {
            var canonical = Require(worldStableId, nameof(worldStableId),
                    "SimulationOnlineWorldStableIdRequired") + "\n"
                + Require(areaSetStableId, nameof(areaSetStableId),
                    "SimulationOnlineAreaSetStableIdRequired") + "\n"
                + Require(playerStableId, nameof(playerStableId),
                    "SimulationOnlinePlayerStableIdInvalid") + "\n"
                + SimulationOnlineWorldCodes.RuleRevision;
            using var sha = SHA256.Create();
            var hash = BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
            return "actor:online-player:" + hash.Substring(0, 24);
        }

        private static 경영SimulationSession생성Request CreateNatureRequest(
            string sessionStableId, string areaSetStableId,
            string primaryActorStableId)
        {
            var acquireAxe = new Simulation공간정의InitialRequest
            {
                SpatialStableId = SimulationNatureSurvivalCodes
                    .ActualE5SpatialStableId(
                        SimulationNatureSurvivalCodes
                            .AcquireAxeWorldInteractionId),
                FacilityStableId = "facility:nature-tool-pickup",
                AreaStableId = "area:nature-home",
                AreaSetStableId = areaSetStableId,
                LandscapeGraphStableId =
                    "landscape-graph:nature-survival-home.v1",
                LandscapeNodeStableId = "nature-tool-pickup",
                EvidenceKindCode = Simulation공간근거종류Codes.LandscapeGraph,
                AccessStateCode = Simulation공간접근상태Codes.Available,
                CapabilityCodes = new[] { Simulation공간능력Codes.Traversable },
                DefinitionRevision = "wi-nature-05.actual-e5.r1",
                DefinitionHashSha256 =
                    "8f08298c84a82e52b8f977d6652b43472b79b3e755ee66c9698c65973ec95eef",
                SourceStableIds = new[]
                {
                    "wi-spatial-seedbed:nature-survival-home.v1",
                    "world-interaction:wi-nature-05",
                },
            };
            var timber = PyeongchangSimulation공간상호작용Fixture
                .CreateNatureDroppedTimberActualE5().Definitions;
            return new 경영SimulationSession생성Request
            {
                ClientRequestId = Guid.ParseExact(sessionStableId.Substring(
                    "simulation-session:".Length), "N"),
                ScenarioStableId = "scenario:online-nature-cooperation.v1",
                ScenarioDataRevision = SimulationOnlineWorldCodes.RuleRevision,
                ScenarioSeed = StableSeed(areaSetStableId),
                RuleRevision = SimulationNatureSurvivalCodes.ProfileRevisionR5,
                DurationTicks = 28,
                WorldContext = new SimulationWorldContext생성Request
                {
                    FactionStableId = "faction:online-nature-cooperation",
                    TerritoryStableId = areaSetStableId,
                    SettlementStableId = "settlement:nature-home",
                    GameDateStartsOn = new DateTimeOffset(
                        2026, 8, 28, 0, 0, 0, TimeSpan.Zero),
                },
                SpatialWorld = new Simulation공간세계InitialStateRequest
                {
                    Definitions = new[] { acquireAxe }.Concat(timber).ToArray(),
                },
                NatureSurvival = new SimulationNatureSurvivalInitialStateRequest
                {
                    PlayerStableId = primaryActorStableId,
                    AreaSetStableId = areaSetStableId,
                    ProfileRevision =
                        SimulationNatureSurvivalCodes.ProfileRevisionR5,
                    BuildingProgressionCatalog =
                        Simulation영역건물발전Catalog.CreateDefault(),
                    ResourceNodes = Enumerable.Range(1, 6).Select(index =>
                        new SimulationNatureResourceNodeInitialStateRequest
                        {
                            ResourceNodeStableId =
                                $"resource:nature-tree:{index:00}",
                            H2StableId =
                                SimulationNatureSurvivalCodes.HarvestH2StableId,
                            H1StableId =
                                "h1-stock:nature-exploration-buffer",
                            LocalX = -8 + index * 2,
                            LocalZ = 8,
                        }).ToArray(),
                },
                NatureMind = new SimulationNatureMindInitialStateRequest
                {
                    Players = new[]
                    {
                        new SimulationNatureMindPlayerInitialStateRequest
                        {
                            PlayerStableId = primaryActorStableId,
                        },
                    },
                },
            };
        }

        private static int StableSeed(string value)
        {
            unchecked
            {
                var seed = 17;
                foreach (var character in value)
                    seed = seed * 31 + character;
                return seed == int.MinValue ? 0 : Math.Abs(seed);
            }
        }

        private static string Require(string value, string parameterName,
            string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(errorCode);
            return value.Trim();
        }
    }
}
