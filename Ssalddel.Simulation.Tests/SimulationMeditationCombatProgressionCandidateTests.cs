using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q007 명상 전투 성장 순서와 기존 LocalCombat 불변 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-11" },
    Boundary = "후보 계약 시험이며 실제 피해·Critical Event·관찰 Projection·Play Mode 증거가 아니다.")]
public sealed class SimulationMeditationCombatProgressionCandidateTests
{
    [Fact]
    public void 성장후보는_확률상승_피해안정화_관찰인계순서를_유지한다()
    {
        var result = new
            SimulationMeditationCombatProgressionCandidateEvaluator()
            .Evaluate(new[]
            {
                Stage(Simulation명상전투성장CandidateCodes
                    .DeepObservationHandover, "observation-handover.r1"),
                Stage(Simulation명상전투성장CandidateCodes
                    .CriticalChanceIncrease, "critical-chance.r1"),
                Stage(Simulation명상전투성장CandidateCodes
                    .BasicDamageStabilization, "damage-stabilization.r1"),
            });

        Assert.Equal(Simulation명상전투성장CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Equal(new[]
        {
            Simulation명상전투성장CandidateCodes.CriticalChanceIncrease,
            Simulation명상전투성장CandidateCodes.BasicDamageStabilization,
            Simulation명상전투성장CandidateCodes.DeepObservationHandover,
        }, result.StageRevisions.Select(value => value.StageCode));
        Assert.Equal(SimulationLocalCombatCodes.RuleRevision,
            result.CurrentCombatRuleRevision);
        Assert.False(result.MutatesCurrentCombatRule);
        Assert.False(result.GuaranteesCriticalAtAnyStage);
    }

    [Fact]
    public void 전투EffectRevision이_없으면_현재전투를바꾸지않고_공백으로남는다()
    {
        var result = new
            SimulationMeditationCombatProgressionCandidateEvaluator()
            .Evaluate(new[]
            {
                Stage(Simulation명상전투성장CandidateCodes
                    .CriticalChanceIncrease, "critical-chance.r1"),
            });

        Assert.Equal(Simulation명상전투성장CandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Contains(Simulation명상전투성장CandidateCodes
            .BasicDamageStabilization, result.MissingStageCodes);
        Assert.Contains(Simulation명상전투성장CandidateCodes
            .CombatEffectUnapproved, result.UnresolvedDecisionCodes);
        Assert.False(result.MutatesCurrentCombatRule);
    }

    private static Simulation명상전투성장StageRevision Stage(
        string code, string revision)
        => new Simulation명상전투성장StageRevision
        {
            StageCode = code,
            RuleRevision = revision,
        };
}
