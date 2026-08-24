using System.Reflection;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 도심마트기본방문주문SimulationTests
{
    [Fact]
    public void 전체4주기대수요를_명시적일별건수의_기본방문주문으로보존한다()
    {
        var demand = 도심마트감자4주DemandSimulationFixture.CreateScenario();
        var orders = new 도심마트기본방문주문SimulationBuilder().Build(
            demand,
            도심마트감자기본방문주문SimulationFixture.Rule());

        Assert.Equal(56, orders.Orders.Length);
        Assert.Empty(orders.Allocations);
        Assert.All(orders.Orders, order =>
        {
            Assert.Equal(SimulationDemandSourceTypeCodes.BaseScenarioDemand, order.DemandSourceTypeCode);
            Assert.Equal(SimulationOrderStateCodes.Pending, order.StateCode);
            Assert.Equal(0m, order.AllocatedQuantity);
            Assert.Equal(0m, order.FulfilledQuantity);
            Assert.Equal(0m, order.UnfulfilledQuantity);
        });

        foreach (var segment in demand.DemandSegments)
        {
            var generated = orders.Orders
                .Where(order => order.SourceDemandSegmentStableId == segment.DemandSegmentStableId)
                .ToArray();
            Assert.Equal(14, generated.Length);
            Assert.Equal(segment.ExpectedQuantity, generated.Sum(order => order.RequestedQuantity));
        }
    }

    [Fact]
    public void 주문생성Tick과기한은_원천수요구간을벗어나지않는다()
    {
        var demand = 도심마트감자4주DemandSimulationFixture.CreateScenario();
        var snapshot = new 도심마트기본방문주문SimulationBuilder().Build(
            demand,
            도심마트감자기본방문주문SimulationFixture.Rule());

        foreach (var segment in demand.DemandSegments)
        {
            var generated = snapshot.Orders
                .Where(order => order.SourceDemandSegmentStableId == segment.DemandSegmentStableId)
                .ToArray();
            Assert.Equal(
                Enumerable.Repeat(2, 7),
                Enumerable.Range(segment.StartsAtTick, 7)
                    .Select(tick => generated.Count(order => order.CreatedTick == tick)));
            Assert.All(generated, order =>
            {
                Assert.InRange(order.CreatedTick, segment.StartsAtTick, segment.EndsAtTick);
                Assert.InRange(order.FulfillmentDueTick, order.CreatedTick, segment.EndsAtTick);
            });
        }
    }

    [Fact]
    public void 같은Scenario와Rule은_같은주문Stream을만든다()
    {
        var builder = new 도심마트기본방문주문SimulationBuilder();
        var first = builder.Build(
            도심마트감자4주DemandSimulationFixture.CreateScenario(),
            도심마트감자기본방문주문SimulationFixture.Rule());
        var second = builder.Build(
            도심마트감자4주DemandSimulationFixture.CreateScenario(),
            도심마트감자기본방문주문SimulationFixture.Rule());

        Assert.Equal(first.SnapshotStableId, second.SnapshotStableId);
        Assert.Equal(first.DataRevision, second.DataRevision);
        Assert.Equal(first.Orders.Select(OrderKey), second.Orders.Select(OrderKey));
    }

    [Fact]
    public void Seed는_총수요를바꾸지않고_나머지분할위치와Revision만바꾼다()
    {
        var firstDemand = 도심마트감자4주DemandSimulationFixture.CreateScenario();
        var secondDemand = 도심마트감자4주DemandSimulationFixture.CreateScenario();
        secondDemand.ScenarioSeed++;
        var builder = new 도심마트기본방문주문SimulationBuilder();
        var first = builder.Build(firstDemand, 도심마트감자기본방문주문SimulationFixture.Rule());
        var second = builder.Build(secondDemand, 도심마트감자기본방문주문SimulationFixture.Rule());

        Assert.NotEqual(first.DataRevision, second.DataRevision);
        Assert.Equal(
            first.Orders.Sum(order => order.RequestedQuantity),
            second.Orders.Sum(order => order.RequestedQuantity));
        Assert.NotEqual(
            first.Orders.Select(order => order.RequestedQuantity),
            second.Orders.Select(order => order.RequestedQuantity));
    }

    [Fact]
    public void 주문Snapshot은_DemandRevision과생성Rule계보를보존한다()
    {
        var demand = 도심마트감자4주DemandSimulationFixture.CreateScenario();
        var rule = 도심마트감자기본방문주문SimulationFixture.Rule();
        var snapshot = new 도심마트기본방문주문SimulationBuilder().Build(demand, rule);

        Assert.Equal(demand.DataRevision, snapshot.DemandScenarioDataRevision);
        Assert.Equal(demand.ScenarioSeed, snapshot.ScenarioSeed);
        Assert.Equal(rule.RuleRevision, snapshot.GenerationRuleRevision);
        Assert.Equal(rule.QuantityBasisCode, snapshot.QuantityBasisCode);
        Assert.Equal(rule.SplitStrategyCode, snapshot.SplitStrategyCode);
        var lineage = Assert.Single(snapshot.SourceLineage);
        Assert.Equal(demand.SnapshotStableId, lineage.SourceStableId);
        Assert.Equal(demand.DataRevision, lineage.SourceDataRevision);
        Assert.Equal(rule.RuleRevision, lineage.RuleRevision);
    }

    [Fact]
    public void 생성Rule은_수요구간과같은TickPattern과_표현가능한정밀도를요구한다()
    {
        var rule = 도심마트감자기본방문주문SimulationFixture.Rule();
        rule.OrdersPerTickPattern = new[] { 2, 2, 2 };
        AssertError("OrderGenerationTickPatternLengthMismatch", () => Build(rule));

        rule = 도심마트감자기본방문주문SimulationFixture.Rule();
        rule.QuantityDecimalPlaces = 7;
        AssertError("OrderGenerationDecimalPlacesInvalid", () => Build(rule));

        rule = 도심마트감자기본방문주문SimulationFixture.Rule();
        rule.OrdersPerTickPattern = new[] { 0, 0, 0, 0, 0, 0, 0 };
        AssertError("OrderGenerationTickPatternInvalid", () => Build(rule));
    }

    [Fact]
    public void 기본방문주문계약에는_개인주소와운영주문식별자가없다()
    {
        var properties = typeof(도심마트주문SimulationData)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain(properties, name => name.Contains("User", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Name", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Address", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Contact", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Operational", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void 주문Validator는_수요SourceType이없는주문을거부한다()
    {
        var snapshot = 도심마트감자기본방문주문SimulationFixture.Create();
        snapshot.Orders[0].DemandSourceTypeCode = string.Empty;

        AssertError(
            "SimulationOrderDemandSourceTypeInvalid",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    private static 도심마트주문SimulationDataSnapshot Build(
        도심마트기본방문주문생성SimulationRule rule)
        => new 도심마트기본방문주문SimulationBuilder().Build(
            도심마트감자4주DemandSimulationFixture.CreateScenario(),
            rule);

    private static string OrderKey(도심마트주문SimulationData order)
        => order.OrderStableId + "|" + order.SourceDemandSegmentStableId
            + "|" + order.CreatedTick + "|" + order.FulfillmentDueTick
            + "|" + order.RequestedQuantity + "|" + order.QuantityUnitCode;

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
