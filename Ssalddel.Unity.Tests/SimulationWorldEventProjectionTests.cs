using Ssalddel.Unity.WorldEvents;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationWorldEventProjectionTests
{
    [Fact]
    public void Unity는_서버사건의의미키와선택지만_표현자료로변환한다()
    {
        var projection = new SimulationWorldEventProjectionMapper().Map(
            Projection(afterWorldRevision: -1));

        var worldEvent = Assert.Single(projection.Events);
        Assert.Equal("survival.external-expedition", worldEvent.PresentationKey);
        Assert.Equal("choice:tarot:1", Assert.Single(worldEvent.Choices).ChoiceStableId);
        Assert.Equal("kr5186:l2:438:419", Assert.Single(worldEvent.TileKeys));
        Assert.Equal("building:sim:farm:safe-barn",
            worldEvent.ActiveBuildingStableId);
        Assert.True(worldEvent.CanRespond);
        Assert.True(worldEvent.RequiresExpectedRevision);
    }

    [Fact]
    public void Unity는_운영상태나표현전용이아닌사건을_거부한다()
    {
        var source = Projection(afterWorldRevision: -1);
        source.Events[0].IsOperationalState = true;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new SimulationWorldEventProjectionMapper().Map(source));

        Assert.Equal("WorldEventBoundaryInvalid", error.Message);
    }

    [Fact]
    public async Task Repository는_마지막세계개정을서버에넘기고_다음개정을반환한다()
    {
        var client = new FakeWorldEventApiClient(Projection(afterWorldRevision: 7));
        var repository = new SimulationWorldEventApiRepository(client,
            new SimulationWorldEventProjectionMapper());

        var result = await repository.변경조회Async("session:sim:survival-1", 7);

        Assert.Equal("session:sim:survival-1", client.SessionStableId);
        Assert.Equal(7, client.AfterWorldRevision);
        Assert.Equal(8, result.NextAfterWorldRevision);
        Assert.Equal(8, Assert.Single(result.Events).LastChangedWorldRevision);
    }

    [Fact]
    public void Api경로는_세션식별자와세계개정을명시한다()
    {
        var route = SimulationWorldEventApiRoutes.Changes(
            "session:sim:survival 1", 8);

        Assert.Equal(
            "/api/simulation/v1/sessions/session%3Asim%3Asurvival%201/world-events?afterWorldRevision=8",
            route);
    }

    private static SimulationWorldEventProjectionApiModel Projection(
        long afterWorldRevision)
    {
        var worldRevision = afterWorldRevision < 0 ? 0 : afterWorldRevision + 1;
        return new SimulationWorldEventProjectionApiModel
        {
            SessionStableId = "session:sim:survival-1",
            WorldTick = 2,
            WorldRevision = worldRevision,
            AfterWorldRevision = afterWorldRevision,
            NextAfterWorldRevision = worldRevision,
            SimulationOnly = true,
            IsOperationalState = false,
            PresentationOnly = true,
            Events =
            [
                new SimulationWorldEventApiModel
                {
                    EventStableId = "world-event:survival-tarot:1",
                    EventRevision = 2,
                    LastChangedWorldRevision = worldRevision,
                    EventTypeCode = "SurvivalTarotOpportunity",
                    TriggerCode = "ExternalExpeditionRequired",
                    StateCode = "AwaitingResponse",
                    OccurredWorldTick = 0,
                    VisibleFromWorldTick = 0,
                    AudienceScopeCode = "SessionParticipants",
                    PresentationKey = "survival.external-expedition",
                    ResponseKindCode = "SurvivalTarotConsensus",
                    SourceOpportunityStableId = "survival-tarot:1",
                    ChoiceSetStableId = "tarot-draw:1",
                    ActiveBuildingStableId = "building:sim:farm:safe-barn",
                    AnchorBuildingStableIds = ["building:sim:farm:safe-barn"],
                    TileKeys = ["kr5186:l2:438:419"],
                    RegionStableIds = ["region:legal-dong:5176031000"],
                    ParticipantPlayerStableIds = ["player:a", "player:b"],
                    RespondedParticipantCount = 1,
                    RequiredParticipantCount = 2,
                    CanRespond = true,
                    RequiresUnanimousResponse = true,
                    RequiresExpectedRevision = true,
                    RuleRevision = "survival-tarot.consensus.r2",
                    SourceStableIds = ["survival-tarot:1"],
                    SimulationOnly = true,
                    IsOperationalState = false,
                    PresentationOnly = true,
                    Choices =
                    [
                        new SimulationWorldEventChoiceApiModel
                        {
                            ChoiceStableId = "choice:tarot:1",
                            DisplayOrder = 1,
                            CardStableId = "tarot-card:trail-rations",
                            CardRevision = "tarot-card.r1",
                            OrientationCode = "Upright",
                            KoreanTitle = "길위의 식량",
                            KoreanSummary = "외부 탐색을 준비한다.",
                        },
                    ],
                },
            ],
        };
    }

    private sealed class FakeWorldEventApiClient : ISimulationWorldEventApiClient
    {
        private readonly SimulationWorldEventProjectionApiModel response;

        public FakeWorldEventApiClient(SimulationWorldEventProjectionApiModel response)
        {
            this.response = response;
        }

        public string SessionStableId { get; private set; } = string.Empty;
        public long AfterWorldRevision { get; private set; } = long.MinValue;

        public Task<SimulationWorldEventProjectionApiModel> GetChangesAsync(
            string sessionStableId,
            long afterWorldRevision,
            CancellationToken cancellationToken = default)
        {
            SessionStableId = sessionStableId;
            AfterWorldRevision = afterWorldRevision;
            return Task.FromResult(response);
        }
    }
}
