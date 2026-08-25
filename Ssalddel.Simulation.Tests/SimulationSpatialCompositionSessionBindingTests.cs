using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationSpatialCompositionSessionBindingTests
{
    [Fact]
    public void OptInSession_QualifiesThenFormsHubWarehouseOnWorldTick()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());

        Assert.Equal(SimulationSpatialCompositionCodes.Qualified,
            InternalWarehouse(session).StateCode);

        session.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:spatial-composition:form",
            ExpectedRevision = session.Revision,
            TickCount = 1,
        });

        Assert.Equal(SimulationSpatialCompositionCodes.Formed,
            InternalWarehouse(session).StateCode);
    }

    [Fact]
    public void Query_ReturnsDefensiveCompositionSnapshot()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var first = session.GetSpatialComposition("Hub");
        first.Assessments[0].StateCode = "mutated";

        var second = session.GetSpatialComposition("Hub");

        Assert.DoesNotContain(second.Assessments,
            value => value.StateCode == "mutated");
        Assert.Equal(64, second.GraphHashSha256.Length);
    }

    private static SpatialCompositionAssessment InternalWarehouse(
        경영SimulationSessionAggregate session)
        => session.GetSpatialComposition("Hub").Assessments.Single(value =>
            value.TargetDefinitionStableId ==
            PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2);

    private static 경영SimulationSession생성Request CreateRequest() => new()
    {
        ClientRequestId = Guid.Parse(
            "73a109f3-6449-4e1d-a873-9290aee92b25"),
        ScenarioStableId = "scenario:hub-spatial-composition-binding",
        ScenarioDataRevision = "fixture.hub-spatial-composition.r1",
        ScenarioSeed = 300,
        RuleRevision = "world-interaction.hub-spatial-composition.r1",
        SpatialCompositionRuleRevision =
            SimulationSpatialCompositionCodes.RuleRevision,
        DurationTicks = 30,
        WorldContext = new SimulationWorldContext생성Request
        {
            FactionStableId = "faction:hub-independent",
            TerritoryStableId = "territory:pyeongchang",
            SettlementStableId = "settlement:pyeongchang",
            GameDateStartsOn = new DateTimeOffset(
                2026, 8, 25, 0, 0, 0, TimeSpan.Zero),
        },
    };
}
