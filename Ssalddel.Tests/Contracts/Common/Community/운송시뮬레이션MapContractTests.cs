using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class 운송시뮬레이션MapContractTests
{
    [Fact]
    public void 초기경로는_화물항공해상을_실제위치가아닌고정교육Fixture로만제공한다()
    {
        var routes = 운송시뮬레이션MapFixtureCatalog.Routes;

        Assert.Equal(3, routes.Count);
        Assert.Equal(
            [
                운송시뮬레이션ModeCodes.GroundCargo,
                운송시뮬레이션ModeCodes.Aviation,
                운송시뮬레이션ModeCodes.Maritime
            ],
            routes.Select(route => route.ModeCode));
        Assert.All(routes, route =>
        {
            Assert.True(route.IsSimulation);
            Assert.Equal(운송시뮬레이션SourceKindCodes.SimulatedFixture, route.SourceKindCode);
            Assert.Equal(운송시뮬레이션MapFixtureCatalog.FixtureSourceCode, route.SourceCode);
            Assert.Contains("SIMULATED", route.SimulationMark, StringComparison.Ordinal);
            Assert.Contains("실제 위치가 아닙니다", route.PositionMeaning, StringComparison.Ordinal);
            Assert.Contains("자동 갱신 없음", route.FreshnessLabel, StringComparison.Ordinal);
            Assert.True(route.Route.Count >= 3);
            Assert.All(route.Route, point =>
            {
                Assert.InRange(point.Latitude, -90d, 90d);
                Assert.InRange(point.Longitude, -180d, 180d);
            });
        });
    }

    [Fact]
    public void 공식외부Source는_초기경로에연결하지않고_향후AdapterCatalog로만보존한다()
    {
        var candidates = 운송공개데이터AdapterCatalog.All;
        var fixtureSourceCodes = 운송시뮬레이션MapFixtureCatalog.Routes
            .Select(route => route.SourceCode)
            .ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(candidates);
        Assert.All(candidates, candidate =>
        {
            Assert.Equal(
                운송시뮬레이션AdapterDecisionCodes.CatalogOnly,
                candidate.AdapterDecisionCode);
            Assert.StartsWith("https://", candidate.CatalogHref, StringComparison.Ordinal);
            Assert.DoesNotContain(candidate.SourceCode, fixtureSourceCodes);
            Assert.False(string.IsNullOrWhiteSpace(candidate.PositionDataBoundary));
        });
    }
}
