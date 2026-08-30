using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q013 다중 공명의 최강 전체·후속 감쇠 정책과 입력 순서 독립적 정렬을 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 계획 시험이며 실제 감쇠량·상한·NatureMind 적용·Play Mode 증거가 아니다.")]
public sealed class SimulationPartyResonanceStackingCandidateTests
{
    [Fact]
    public void 최강공명은온전히_후속공명은순위감쇠후보가된다()
    {
        var result = new SimulationPartyResonanceStackingCandidatePlanner()
            .Plan(new[]
            {
                Input("player:b", 8m),
                Input("player:a", 10m),
                Input("player:c", 6m),
            }, "resonance-stacking.r1");

        Assert.Equal(Simulation파티공명중첩CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Equal(new[] { "player:a", "player:b", "player:c" },
            result.RankedContributions.Select(value =>
                value.ProviderPlayerStableId));
        Assert.True(result.RankedContributions[0].UsesFullContribution);
        Assert.All(result.RankedContributions.Skip(1), value =>
            Assert.True(value.RequiresAttenuation));
        Assert.False(result.AllowsUnlimitedLinearGrowth);
        Assert.False(result.AppliesStackedRecovery);
        Assert.False(result.ChangesWorldState);
    }

    [Fact]
    public void 입력순서와무관하고_동률은StableId로정렬한다()
    {
        var planner = new SimulationPartyResonanceStackingCandidatePlanner();
        var first = planner.Plan(new[]
        {
            Input("player:z", 7m), Input("player:a", 7m),
        }, "resonance-stacking.r1");
        var second = planner.Plan(new[]
        {
            Input("player:a", 7m), Input("player:z", 7m),
        }, "resonance-stacking.r1");

        Assert.Equal(first.RankedContributions.Select(value =>
                value.ProviderPlayerStableId),
            second.RankedContributions.Select(value =>
                value.ProviderPlayerStableId));
        Assert.Equal("player:a",
            first.RankedContributions[0].ProviderPlayerStableId);
    }

    private static Simulation파티공명기여CandidateInput Input(
        string id, decimal magnitude) => new()
        {
            ProviderPlayerStableId = id,
            BaseMagnitude = magnitude,
        };
}
