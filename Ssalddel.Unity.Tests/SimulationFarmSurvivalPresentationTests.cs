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
