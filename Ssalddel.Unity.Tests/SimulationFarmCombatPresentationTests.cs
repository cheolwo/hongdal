using Ssalddel.Unity.Survival;

namespace Ssalddel.Unity.Tests;

public sealed class SimulationFarmCombatPresentationTests
{
    [Theory]
    [InlineData(FarmCombatPresentationCodes.FirstPersonPrecision, true, false)]
    [InlineData(FarmCombatPresentationCodes.ThirdPersonAwareness, false, true)]
    public void 서버전투박자를_시점별표현으로만투영한다(
        string perspectiveCode,
        bool focused,
        bool awareness)
    {
        var frame = new FarmCombatPresentationMapper().Map(
            State(perspectiveCode), "actor:sim:player");

        Assert.Equal(1000, frame.ImpactOffsetMs);
        Assert.True(frame.OwnsCombatInput);
        Assert.Equal(focused, frame.ShowFocusedThreatTelegraph);
        Assert.Equal(awareness, frame.ShowAllThreats);
        Assert.Equal(awareness, frame.ShowAllies);
        Assert.Equal(awareness, frame.ShowFacilities);
        Assert.True(frame.PresentationOnly);
        Assert.False(frame.ChangesWorldState);
    }

    [Fact]
    public void 반응명령초안에는_행동과시각만있고_판정결과는없다()
    {
        var frame = new FarmCombatPresentationMapper().Map(
            State(FarmCombatPresentationCodes.FirstPersonPrecision),
            "actor:sim:player");

        var command = FarmCombatReactionCommandFactory.Create(frame, 3,
            "command:combat:unity:1", FarmCombatPresentationCodes.Counter, 980);

        Assert.Equal(frame.BeatStableId, command.BeatStableId);
        Assert.Equal(980, command.ReactionOffsetMs);
        Assert.Null(typeof(FarmCombatReactionCommandDraft).GetProperty("GradeCode"));
        Assert.Null(typeof(FarmCombatReactionCommandDraft).GetProperty("DamageUnits"));
        Assert.Null(typeof(FarmCombatReactionCommandDraft).GetProperty("Score"));
    }

    [Fact]
    public void 평상시좌클릭은_피해가아닌전투박자시작초안을만든다()
    {
        var state = ReadyState();
        var perspective = FarmCombatInputCommandFactory.CreatePerspective(
            state, "actor:sim:player",
            FarmCombatPresentationCodes.FirstPersonPrecision,
            "command:unity:perspective:1");
        state.Perspectives =
        [
            new FarmCombatPerspectiveApiModel
            {
                ActorStableId = perspective.ActorStableId,
                PerspectiveCode = perspective.PerspectiveCode,
            },
        ];

        var start = FarmCombatInputCommandFactory.CreateBeatStart(
            state, "actor:sim:player", "encounter:zombie:1",
            "command:unity:beat:1");

        Assert.Equal("encounter:zombie:1", start.EncounterStableId);
        Assert.Equal(3, start.ExpectedRevision);
        Assert.Null(typeof(FarmCombatBeatStartCommandDraft)
            .GetProperty("DamageUnits"));
        Assert.Null(typeof(FarmCombatBeatStartCommandDraft)
            .GetProperty("GradeCode"));
    }

    [Fact]
    public void 같은전투박자상태를반복관측해도_반응시계는초기화되지않는다()
    {
        var clock = new FarmCombatBeatClock();

        Assert.True(clock.Observe("combat-beat:1", 1000d));
        Assert.False(clock.Observe("combat-beat:1", 1300d));
        Assert.Equal(500, clock.ElapsedMilliseconds(1500d, 1600));
        Assert.True(clock.Observe("combat-beat:2", 1600d));
        Assert.Equal(100, clock.ElapsedMilliseconds(1700d, 1600));
    }

    [Fact]
    public void 전투Api경로는_기존FarmSurvival계약을그대로사용한다()
    {
        const string session = "session:one";
        Assert.Equal("api/simulation/v1/sessions/session%3Aone/farm-survival",
            SimulationFarmCombatApiRoutes.State(session));
        Assert.EndsWith("/combat/perspective/confirm",
            SimulationFarmCombatApiRoutes.Perspective(session));
        Assert.EndsWith("/combat/beats/start",
            SimulationFarmCombatApiRoutes.StartBeat(session));
        Assert.EndsWith("/combat/beats/beat%3Aone/react",
            SimulationFarmCombatApiRoutes.Reaction(session, "beat:one"));
    }

    [Fact]
    public void 전술명령창은_강제전환없이_주변전선과삼인칭전환만제안한다()
    {
        var source = State(FarmCombatPresentationCodes.FirstPersonPrecision);
        source.Tactical = TacticalState();

        var frame = new FarmTacticalOrderPresentationMapper().Map(
            source, "actor:sim:player");

        Assert.True(frame.SuggestThirdPersonTransition);
        Assert.False(frame.ForceThirdPersonTransition);
        Assert.Equal(FarmCombatPresentationCodes.ThirdPersonAwareness,
            frame.SuggestedPerspectiveCode);
        Assert.Single(frame.AvailableOpportunityStableIds);
        Assert.Equal(2, frame.HighlightSquadStableIds.Length);
        Assert.True(frame.PresentationOnly);
        Assert.False(frame.ChangesWorldState);
    }

    [Fact]
    public void 전술명령초안은_안정식별자만가지고_점수와피해를계산하지않는다()
    {
        var source = State(FarmCombatPresentationCodes.FirstPersonPrecision);
        source.Tactical = TacticalState();
        var frame = new FarmTacticalOrderPresentationMapper().Map(
            source, "actor:sim:player");

        var preview = FarmTacticalOrderCommandFactory.CreatePreview(frame, 4,
            FarmCombatPresentationCodes.AdvanceAndAttack,
            "tactical-opportunity:one");
        var confirm = FarmTacticalOrderCommandFactory.CreateConfirm(frame, 4,
            "command:tactical:unity:1",
            FarmCombatPresentationCodes.AdvanceAndAttack,
            "tactical-opportunity:one");

        Assert.Equal(frame.OrderWindowStableId, preview.OrderWindowStableId);
        Assert.Equal(preview.FrontStableId, confirm.FrontStableId);
        Assert.Null(typeof(FarmTacticalOrderConfirmDraft)
            .GetProperty("ProjectedResponseScore"));
        Assert.Null(typeof(FarmTacticalOrderConfirmDraft)
            .GetProperty("DamageUnits"));
        Assert.Null(typeof(FarmTacticalOrderConfirmDraft)
            .GetProperty("DefenseSucceeded"));
    }

    [Theory]
    [InlineData(FarmCombatPresentationCodes.AdvanceAndAttack,
        FarmCombatPresentationCodes.Forward,
        FarmCombatPresentationCodes.WedgeFormation,
        FarmCombatPresentationCodes.RunMovement)]
    [InlineData(FarmCombatPresentationCodes.HoldFormation,
        FarmCombatPresentationCodes.Perimeter,
        FarmCombatPresentationCodes.LineFormation,
        FarmCombatPresentationCodes.GuardMovement)]
    [InlineData(FarmCombatPresentationCodes.TacticalRetreat,
        FarmCombatPresentationCodes.InnerFarm,
        FarmCombatPresentationCodes.ColumnFormation,
        FarmCombatPresentationCodes.RunMovement)]
    public void 서버전술결과를_결정적분대이동표현으로만투영한다(
        string orderCode,
        string targetPositionCode,
        string formationCode,
        string movementCode)
    {
        var source = State(FarmCombatPresentationCodes.ThirdPersonAwareness);
        source.WorldRevision = 8;
        source.Tactical = ResolvedTacticalState(orderCode,
            targetPositionCode, 6);

        var frame = new FarmTacticalMovementPresentationMapper()
            .MapLatest(source);

        var allied = Assert.Single(frame.Squads,
            value => value.SideCode == FarmCombatPresentationCodes.Allied);
        Assert.Equal(formationCode, allied.FormationCode);
        Assert.Equal(movementCode, allied.MovementIntentCode);
        Assert.Equal(targetPositionCode, allied.TargetPositionCode);
        Assert.Equal(6, allied.DisplayedMemberCount);
        Assert.Equal("actor:npc:01", allied.DisplayMemberStableIds[0]);
        Assert.Equal("tactical-squad:allied:one:visual-member:06",
            allied.DisplayMemberStableIds[5]);
        Assert.True(frame.PresentationOnly);
        Assert.False(frame.ChangesWorldState);
        Assert.Null(typeof(FarmTacticalMovementPresentationFrame)
            .GetProperty("TacticalResponseScore"));
        Assert.Null(typeof(FarmTacticalMovementPresentationFrame)
            .GetProperty("DamageUnits"));
        Assert.Null(typeof(FarmTacticalMovementPresentationFrame)
            .GetProperty("DefenseSucceeded"));
    }

    [Fact]
    public void 적군전투력이0이면_사망이아닌경직표현만선택한다()
    {
        var source = State(FarmCombatPresentationCodes.ThirdPersonAwareness);
        source.Tactical = ResolvedTacticalState(
            FarmCombatPresentationCodes.AdvanceAndAttack,
            FarmCombatPresentationCodes.Forward, 9);

        var frame = new FarmTacticalMovementPresentationMapper()
            .MapLatest(source);
        var hostile = Assert.Single(frame.Squads,
            value => value.SideCode == FarmCombatPresentationCodes.Hostile);

        Assert.Equal(6, hostile.DisplayedMemberCount);
        Assert.Equal(9, hostile.CanonicalMemberCount);
        Assert.Equal(FarmCombatPresentationCodes.LineFormation,
            hostile.FormationCode);
        Assert.Equal(FarmCombatPresentationCodes.StaggerMovement,
            hostile.MovementIntentCode);
    }

    [Fact]
    public void 운영상태와_주문결과불일치는_이동표현으로받지않는다()
    {
        var operational = State(
            FarmCombatPresentationCodes.ThirdPersonAwareness);
        operational.Tactical = ResolvedTacticalState(
            FarmCombatPresentationCodes.HoldFormation,
            FarmCombatPresentationCodes.Perimeter, 3);
        operational.Tactical.IsOperationalState = true;
        Assert.Throws<InvalidOperationException>(() =>
            new FarmTacticalMovementPresentationMapper()
                .MapLatest(operational));

        var drift = State(FarmCombatPresentationCodes.ThirdPersonAwareness);
        drift.Tactical = ResolvedTacticalState(
            FarmCombatPresentationCodes.HoldFormation,
            FarmCombatPresentationCodes.Perimeter, 3);
        drift.Tactical.Orders[0].OrderCode =
            FarmCombatPresentationCodes.AdvanceAndAttack;
        Assert.Throws<InvalidOperationException>(() =>
            new FarmTacticalMovementPresentationMapper().MapLatest(drift));
    }

    private static FarmCombatStateApiModel State(string perspectiveCode)
        => new()
        {
            WorldRevision = 3,
            Perspectives =
            [
                new FarmCombatPerspectiveApiModel
                {
                    ActorStableId = "actor:sim:player",
                    PerspectiveCode = perspectiveCode,
                },
            ],
            Beats =
            [
                new FarmCombatBeatApiModel
                {
                    BeatStableId = "combat-beat:encounter:zombie:1",
                    EncounterStableId = "encounter:zombie:1",
                    ActorStableId = "actor:sim:player",
                    AppliedPerspectiveCode = perspectiveCode,
                    AttackPatternCode = "ZombieSwipe",
                    ImpactOffsetMs = 1000,
                    GuardWindowMs = perspectiveCode ==
                        FarmCombatPresentationCodes.FirstPersonPrecision ? 320 : 220,
                    CounterWindowMs = perspectiveCode ==
                        FarmCombatPresentationCodes.FirstPersonPrecision ? 200 : 130,
                    PerfectGuardWindowMs = 70,
                    PerfectCounterWindowMs = 45,
                    StateCode = FarmCombatPresentationCodes.Active,
                },
            ],
            SimulationOnly = true,
            IsOperationalState = false,
        };

    private static FarmCombatStateApiModel ReadyState()
        => new()
        {
            WorldRevision = 3,
            Engagements =
            [
                new FarmCombatEngagementApiModel
                {
                    EncounterStableId = "encounter:zombie:1",
                    StateCode = FarmCombatPresentationCodes.AwaitingCombat,
                },
            ],
            SimulationOnly = true,
            IsOperationalState = false,
        };

    private static FarmTacticalCombatStateApiModel TacticalState()
        => new()
        {
            Fronts =
            [
                new FarmTacticalFrontApiModel
                {
                    FrontStableId = "tactical-front:one",
                    EncounterStableId = "encounter:zombie:1",
                    PositionCode = "Perimeter",
                    StateCode = FarmCombatPresentationCodes.Open,
                },
            ],
            Squads =
            [
                new FarmTacticalSquadApiModel
                {
                    SquadStableId = "tactical-squad:allied:one",
                    FrontStableId = "tactical-front:one",
                    SideCode = "Allied",
                },
                new FarmTacticalSquadApiModel
                {
                    SquadStableId = "tactical-squad:hostile:one",
                    FrontStableId = "tactical-front:one",
                    SideCode = "Hostile",
                },
            ],
            Opportunities =
            [
                new FarmTacticalOpportunityApiModel
                {
                    OpportunityStableId = "tactical-opportunity:one",
                    FrontStableId = "tactical-front:one",
                    EarningActorStableId = "actor:sim:player",
                    OpportunityKindCode = "Breakthrough",
                    Quality = 2,
                    StateCode = FarmCombatPresentationCodes.Available,
                },
            ],
            OrderWindows =
            [
                new FarmTacticalOrderWindowApiModel
                {
                    OrderWindowStableId = "tactical-window:one",
                    EncounterStableId = "encounter:zombie:1",
                    FrontStableId = "tactical-front:one",
                    AuthorizedActorStableId = "actor:sim:player",
                    ClosesWorldTick = 6,
                    StateCode = FarmCombatPresentationCodes.Open,
                    AllowedOrderCodes =
                    [
                        FarmCombatPresentationCodes.AdvanceAndAttack,
                        FarmCombatPresentationCodes.HoldFormation,
                        FarmCombatPresentationCodes.TacticalRetreat,
                    ],
                },
            ],
            SimulationOnly = true,
            IsOperationalState = false,
        };

    private static FarmTacticalCombatStateApiModel ResolvedTacticalState(
        string orderCode,
        string alliedPositionCode,
        int memberCount)
        => new()
        {
            Fronts =
            [
                new FarmTacticalFrontApiModel
                {
                    FrontStableId = "tactical-front:one",
                    EncounterStableId = "encounter:zombie:1",
                    PositionCode = alliedPositionCode,
                    StateCode = FarmCombatPresentationCodes.Resolved,
                },
            ],
            Squads =
            [
                new FarmTacticalSquadApiModel
                {
                    SquadStableId = "tactical-squad:allied:one",
                    FrontStableId = "tactical-front:one",
                    SideCode = FarmCombatPresentationCodes.Allied,
                    PositionCode = alliedPositionCode,
                    MemberCount = memberCount,
                    CombatStrength = memberCount,
                    MemberActorStableIds = ["actor:npc:01"],
                },
                new FarmTacticalSquadApiModel
                {
                    SquadStableId = "tactical-squad:hostile:one",
                    FrontStableId = "tactical-front:one",
                    SideCode = FarmCombatPresentationCodes.Hostile,
                    PositionCode = FarmCombatPresentationCodes.Perimeter,
                    MemberCount = memberCount,
                    CombatStrength = 0,
                },
            ],
            Orders =
            [
                new FarmTacticalOrderApiModel
                {
                    OrderStableId = "tactical-order:one",
                    OrderWindowStableId = "tactical-window:one",
                    FrontStableId = "tactical-front:one",
                    ActorStableId = "actor:sim:player",
                    OrderCode = orderCode,
                    ResolvesWorldTick = 8,
                    StateCode = FarmCombatPresentationCodes.Resolved,
                },
            ],
            Resolutions =
            [
                new FarmTacticalResolutionApiModel
                {
                    ResolutionStableId = "tactical-resolution:one",
                    OrderStableId = "tactical-order:one",
                    EncounterStableId = "encounter:zombie:1",
                    FrontStableId = "tactical-front:one",
                    OrderCode = orderCode,
                    ResolvedWorldTick = 8,
                    FrontPositionCode = alliedPositionCode,
                },
            ],
            SimulationOnly = true,
            IsOperationalState = false,
        };
}
