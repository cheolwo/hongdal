using System.Reflection;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class 도심마트지역수요SimulationTests
{
    [Fact]
    public void 공공인구Basis는_지역집계사실과출처만보존한다()
    {
        var data = 도심마트감자4주DemandSimulationFixture.PopulationBasis();

        Assert.True(data.IsPublicAggregate);
        Assert.False(data.IsSuppressed);
        Assert.Equal(42000, data.RegisteredPopulation);
        Assert.Equal(18000, data.RegisteredHouseholdCount);
        Assert.Contains("fixture", data.SourceKey, StringComparison.OrdinalIgnoreCase);

        var properties = typeof(지역인구SimulationBasisDataSnapshot)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(value => value.Name)
            .ToArray();
        Assert.DoesNotContain(properties, name => name.Contains("Product", StringComparison.Ordinal));
        Assert.DoesNotContain(properties, name => name.Contains("Order", StringComparison.Ordinal));
        Assert.DoesNotContain(properties, name => name.Contains("Address", StringComparison.Ordinal));
    }

    [Fact]
    public void 잠재수요는_세대수Band와근거를보존하지만_상품수량을만들지않는다()
    {
        var world = new 지역잠재수요Interpreter().Interpret(
            도심마트감자4주DemandSimulationFixture.PopulationBasis(),
            도심마트감자4주DemandSimulationFixture.PotentialDemandRule());

        Assert.Equal("MediumPotentialBasis", world.PotentialDemandBandCode);
        Assert.Equal("RegisteredHouseholdCount", world.BasisMetricCode);
        Assert.Equal(18000, world.BasisMetricValue);
        Assert.Equal("population-basis-fixture:2026-07:households-18000:1", world.PopulationBasisRevision);
        Assert.DoesNotContain(typeof(지역잠재수요WorldState).GetProperties(),
            property => property.Name.Contains("Order", StringComparison.Ordinal)
                || property.Name.Contains("Product", StringComparison.Ordinal)
                || property.Name.Contains("Quantity", StringComparison.Ordinal));
    }

    [Fact]
    public void 억제되거나결측인_인구Basis를_0으로대체하지않는다()
    {
        var suppressed = 도심마트감자4주DemandSimulationFixture.PopulationBasis();
        suppressed.IsSuppressed = true;
        AssertError("PopulationBasisUnavailable", () => Interpret(suppressed));

        var missing = 도심마트감자4주DemandSimulationFixture.PopulationBasis();
        missing.RegisteredHouseholdCount = null;
        AssertError("PopulationMetricMissingOrInvalid", () => Interpret(missing));
    }

    [Fact]
    public void 명시적가정으로_4주DemandScenario를만든다()
    {
        var snapshot = 도심마트감자4주DemandSimulationFixture.CreateScenario();

        Assert.Equal(4, snapshot.DemandSegments.Length);
        Assert.Equal(new[] { 0, 7, 14, 21 }, snapshot.DemandSegments.Select(value => value.StartsAtTick));
        Assert.Equal(new[] { 6, 13, 20, 27 }, snapshot.DemandSegments.Select(value => value.EndsAtTick));
        Assert.Equal(new[] { 420m, 440m, 410m, 450m },
            snapshot.DemandSegments.Select(value => value.ExpectedQuantity));
        Assert.Equal(0.20m, snapshot.ProductSelectionRate);
        Assert.Equal(0.05m, snapshot.SimulationMarketShareRate);
        Assert.Equal(2, snapshot.SourceLineage.Length);
        Assert.False(snapshot.IsOperationalState);
    }

    [Fact]
    public void 인구가달라도_명시적주별가정이같으면_수요량은바뀌지않는다()
    {
        var lowerPopulation = 도심마트감자4주DemandSimulationFixture.CreateScenario(12000);
        var higherPopulation = 도심마트감자4주DemandSimulationFixture.CreateScenario(40000);

        Assert.Equal(
            lowerPopulation.DemandSegments.Select(value => value.ExpectedQuantity),
            higherPopulation.DemandSegments.Select(value => value.ExpectedQuantity));
        Assert.NotEqual(lowerPopulation.DataRevision, higherPopulation.DataRevision);
    }

    [Fact]
    public void 같은Basis와가정은_같은DemandScenario를만든다()
    {
        var first = 도심마트감자4주DemandSimulationFixture.CreateScenario();
        var second = 도심마트감자4주DemandSimulationFixture.CreateScenario();

        Assert.Equal(first.SnapshotStableId, second.SnapshotStableId);
        Assert.Equal(first.DataRevision, second.DataRevision);
        Assert.Equal(
            first.DemandSegments.Select(SegmentKey),
            second.DemandSegments.Select(SegmentKey));
    }

    [Fact]
    public void DemandScenario는_정확히4주의_고유가정을요구한다()
    {
        var assumptions = 도심마트감자4주DemandSimulationFixture.Assumptions();
        assumptions.WeeklyDemand = assumptions.WeeklyDemand.Take(3).ToArray();
        AssertError("DemandAssumptionFourWeeksRequired", () => Build(assumptions));

        assumptions = 도심마트감자4주DemandSimulationFixture.Assumptions();
        assumptions.WeeklyDemand[3].WeekIndex = 2;
        AssertError("DemandAssumptionFourWeeksRequired", () => Build(assumptions));
    }

    [Fact]
    public void DemandScenario는_숨겨진비율과역전된수량범위를거부한다()
    {
        var assumptions = 도심마트감자4주DemandSimulationFixture.Assumptions();
        assumptions.ProductSelectionRate = 1.1m;
        AssertError("DemandAssumptionProductSelectionRateInvalid", () => Build(assumptions));

        assumptions = 도심마트감자4주DemandSimulationFixture.Assumptions();
        assumptions.WeeklyDemand[0].MaximumQuantity = 300m;
        AssertError("DemandAssumptionQuantityRangeInvalid", () => Build(assumptions));
    }

    private static 지역잠재수요WorldState Interpret(지역인구SimulationBasisDataSnapshot data)
        => new 지역잠재수요Interpreter().Interpret(
            data,
            도심마트감자4주DemandSimulationFixture.PotentialDemandRule());

    private static 도심마트수요시나리오DataSnapshot Build(도심마트4주DemandScenarioAssumptions assumptions)
    {
        var potential = Interpret(도심마트감자4주DemandSimulationFixture.PopulationBasis());
        return new 도심마트4주DemandScenarioBuilder().Build(
            "simulation-session:potato-fixture",
            도심마트감자공급SimulationFixture.ScenarioStableId,
            240809,
            potential,
            assumptions);
    }

    private static string SegmentKey(도심마트기간별수요SimulationData value)
        => value.DemandSegmentStableId + "|" + value.StartsAtTick + "|" + value.EndsAtTick
            + "|" + value.MinimumQuantity + "|" + value.ExpectedQuantity + "|" + value.MaximumQuantity;

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
