using Ssalddel.Unity.Survival;

namespace Ssalddel.Unity.Tests;

public sealed class SimulationFarmSurvivalPresentationTests
{
    [Fact]
    public void 좀비경고는_서버수량만큼의미키로표현하고_현재보유팩Fallback을유지한다()
    {
        var intents = new FarmSurvivalVisualIntentMapper(
            FarmSurvivalVisualCatalog.CreateDefault()).Map(State());

        var zombies = intents.Where(value =>
            value.VisualKey == FarmSurvivalVisualKeys.StylizedZombie).ToArray();
        Assert.Equal(3, zombies.Length);
        Assert.All(zombies, value =>
        {
            Assert.Equal(FarmSurvivalVisualKeys.SkeletonThreatFallback,
                value.FallbackVisualKey);
            Assert.Equal("POLYGON Apocalypse", value.PreferredSourcePack);
            Assert.True(value.PresentationOnly);
            Assert.DoesNotContain("Assets/", value.VisualKey,
                StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Unity표현기는_운영상태를생존오버레이로받지않는다()
    {
        var state = State();
        state.IsOperationalState = true;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new FarmSurvivalVisualIntentMapper(
                FarmSurvivalVisualCatalog.CreateDefault()).Map(state));

        Assert.Equal("FarmSurvivalBoundaryInvalid", error.Message);
    }

    [Fact]
    public void 경관중심규칙은_방어예고동안적개체와전투Hud를숨긴다()
    {
        var state = State();
        state.RuleRevision =
            FarmSurvivalExperienceCodes.ScenicSeasonRuleRevision;
        state.Encounters[0].PresentationKey =
            "survival.seasonal-defense.warning";

        var visuals = new FarmSurvivalVisualIntentMapper(
            FarmSurvivalVisualCatalog.CreateDefault()).Map(state);
        var experience = FarmSurvivalExperienceIntentMapper.Map(state);

        Assert.DoesNotContain(visuals, value =>
            value.VisualKey == FarmSurvivalVisualKeys.StylizedZombie);
        Assert.Equal(FarmSurvivalExperienceCodes.SeasonalPreparation,
            experience.MoodCode);
        Assert.True(experience.ShowScenicHud);
        Assert.False(experience.ShowCombatHud);
        Assert.False(experience.ShowThreatVisuals);
        Assert.True(experience.DirectCombatOptional);
    }

    [Fact]
    public void 경관중심규칙도_직접전투선택뒤에는전투표현을연다()
    {
        var state = State();
        state.RuleRevision =
            FarmSurvivalExperienceCodes.ScenicSeasonRuleRevision;
        state.Encounters[0].StateCode =
            FarmSurvivalExperienceCodes.AwaitingCombat;
        state.Encounters[0].PresentationKey = "survival.combat.ready";

        var visuals = new FarmSurvivalVisualIntentMapper(
            FarmSurvivalVisualCatalog.CreateDefault()).Map(state);
        var experience = FarmSurvivalExperienceIntentMapper.Map(state);

        Assert.Equal(3, visuals.Count(value =>
            value.VisualKey == FarmSurvivalVisualKeys.StylizedZombie));
        Assert.Equal(FarmSurvivalExperienceCodes.Combat,
            experience.MoodCode);
        Assert.False(experience.ShowScenicHud);
        Assert.True(experience.ShowCombatHud);
        Assert.True(experience.ShowThreatVisuals);
    }

    private static FarmSurvivalStateApiModel State()
        => new()
        {
            SessionStableId = "simulation-session:survival",
            WorldRevision = 1,
            WorldTick = 4,
            TileKey = "kr5186:l2:438:419",
            FarmBuildingStableId = "building:sim.daegwallyeong-farmhouse",
            Actors =
            [
                new FarmSurvivalActorApiModel
                {
                    ActorStableId = "actor:sim:player",
                    ActorKindCode = "Player",
                },
            ],
            SoilTiles =
            [
                new FarmSurvivalSoilTileApiModel
                {
                    SoilTileStableId = "soil-tile:sim:0:0",
                    StateCode = "Tilled",
                },
            ],
            Defenses =
            [
                new FarmSurvivalDefenseApiModel
                {
                    DefenseStableId = "defense:sim:fence",
                    Durability = 60m,
                },
            ],
            Encounters =
            [
                new FarmSurvivalEncounterApiModel
                {
                    EncounterStableId = "encounter:sim:zombie:day-5",
                    ThreatTypeCode = "ZombiePressure",
                    ThreatUnitCount = 3,
                    StateCode = "Warning",
                    PresentationKey = "survival.zombie-warning",
                },
            ],
            SimulationOnly = true,
            IsOperationalState = false,
        };
}
