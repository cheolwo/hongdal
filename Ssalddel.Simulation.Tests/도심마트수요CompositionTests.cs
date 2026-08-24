using System.Reflection;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 도심마트수요CompositionTests
{
    [Fact]
    public void 기본수요와집단수요를_서로다른합계로보존한다()
    {
        var state = 도심마트감자수요CompositionSimulationFixture.Create();

        Assert.Equal(1720m, state.BaseScenarioDemand);
        Assert.Equal(410m, state.GroupIntentDemand);
        Assert.Equal(385m, state.GroupConfirmedDemand);
        Assert.Equal(2105m, state.HardDemand);
        Assert.Equal(
            도심마트수요CompositionPolicyCodes.BasePlusGroupConfirmed,
            state.HardDemandPolicyCode);
    }

    [Fact]
    public void 집단의향은참고신호이고_집단확정만HardDemand다()
    {
        var state = 도심마트감자수요CompositionSimulationFixture.Create();
        var intent = Assert.Single(state.Components, component =>
            component.SourceTypeCode == SimulationDemandSourceTypeCodes.GroupIntentDemand);
        var confirmed = Assert.Single(state.Components, component =>
            component.SourceTypeCode == SimulationDemandSourceTypeCodes.GroupConfirmedDemand);

        Assert.False(intent.IsHardDemand);
        Assert.Equal(67, intent.ParticipantCount);
        Assert.Equal(410m, intent.Quantity);
        Assert.True(confirmed.IsHardDemand);
        Assert.Equal(61, confirmed.ParticipantCount);
        Assert.Equal(385m, confirmed.Quantity);
    }

    [Fact]
    public void SourceComponent는_기간과출처를잃지않는다()
    {
        var state = 도심마트감자수요CompositionSimulationFixture.Create();

        Assert.Equal(6, state.Components.Length);
        Assert.Equal(4, state.Components.Count(component =>
            component.SourceTypeCode == SimulationDemandSourceTypeCodes.BaseScenarioDemand));
        Assert.All(state.Components.Where(component =>
            component.SourceTypeCode != SimulationDemandSourceTypeCodes.BaseScenarioDemand),
            component =>
            {
                Assert.Equal(7, component.StartsAtTick);
                Assert.Equal(27, component.EndsAtTick);
                Assert.Equal(
                    도심마트공동주택주문자집단SimulationFixture.OrdererGroupStableId,
                    component.SourceStableId);
            });
        Assert.Equal(2, state.SourceLineage.Length);
    }

    [Fact]
    public void 같은입력은_같은Composition을만든다()
    {
        var first = 도심마트감자수요CompositionSimulationFixture.Create();
        var second = 도심마트감자수요CompositionSimulationFixture.Create();

        Assert.Equal(first.InterpretationRevision, second.InterpretationRevision);
        Assert.Equal(
            first.Components.Select(ComponentKey),
            second.Components.Select(ComponentKey));
    }

    [Fact]
    public void 의향수량변경은_HardDemand를변경하지않는다()
    {
        var baseDemand = 도심마트감자4주DemandSimulationFixture.CreateScenario();
        var groupDemand = 도심마트공동주택주문자집단SimulationFixture.Create();
        groupDemand.Groups[0].IntentQuantity = 400m;

        var state = Interpret(baseDemand, groupDemand);

        Assert.Equal(400m, state.GroupIntentDemand);
        Assert.Equal(385m, state.GroupConfirmedDemand);
        Assert.Equal(2105m, state.HardDemand);
    }

    [Fact]
    public void 기본수요와집단수요의단위가다르면거부한다()
    {
        var groupDemand = 도심마트공동주택주문자집단SimulationFixture.Create();
        groupDemand.Groups[0].QuantityUnitCode = "box";

        AssertError(
            "DemandCompositionQuantityUnitMismatch",
            () => Interpret(도심마트감자4주DemandSimulationFixture.CreateScenario(), groupDemand));
    }

    [Fact]
    public void Session과Scenario가다르면거부한다()
    {
        var groupDemand = 도심마트공동주택주문자집단SimulationFixture.Create();
        groupDemand.SessionStableId = "simulation-session:other";
        AssertError(
            "DemandCompositionSessionMismatch",
            () => Interpret(도심마트감자4주DemandSimulationFixture.CreateScenario(), groupDemand));

        groupDemand = 도심마트공동주택주문자집단SimulationFixture.Create();
        groupDemand.ScenarioStableId = "scenario:other";
        AssertError(
            "DemandCompositionScenarioMismatch",
            () => Interpret(도심마트감자4주DemandSimulationFixture.CreateScenario(), groupDemand));
    }

    [Fact]
    public void 임의ConversionRate계약이없고_지원하지않는정책은거부한다()
    {
        var properties = typeof(도심마트수요CompositionRule)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();
        Assert.DoesNotContain(properties, name =>
            name.Contains("Conversion", StringComparison.OrdinalIgnoreCase));

        var rule = 도심마트감자수요CompositionSimulationFixture.Rule();
        rule.HardDemandPolicyCode = "IntentConversion";
        AssertError(
            "DemandCompositionPolicyInvalid",
            () => new 도심마트수요CompositionInterpreter().Interpret(
                도심마트감자4주DemandSimulationFixture.CreateScenario(),
                도심마트공동주택주문자집단SimulationFixture.Create(),
                rule));
    }

    private static 도심마트수요CompositionWorldState Interpret(
        도심마트수요시나리오DataSnapshot baseDemand,
        도심마트주문자집단수요SimulationDataSnapshot groupDemand)
        => new 도심마트수요CompositionInterpreter().Interpret(
            baseDemand,
            groupDemand,
            도심마트감자수요CompositionSimulationFixture.Rule());

    private static string ComponentKey(도심마트수요CompositionComponentWorldState component)
        => component.StableId + "|" + component.SourceTypeCode + "|"
            + component.SourceStableId + "|" + component.StartsAtTick + "|"
            + component.EndsAtTick + "|" + component.ParticipantCount + "|"
            + component.Quantity + "|" + component.IsHardDemand;

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
