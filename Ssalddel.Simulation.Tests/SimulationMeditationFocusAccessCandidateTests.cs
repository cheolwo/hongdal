using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q006 명상 숙련의 집중 접근 확대와 순간 집중·전투 효과의 미결정 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-11" },
    Boundary = "후보 계약 시험이며 실제 숙련 성장·집중 Challenge·전투 피해·Play Mode 증거가 아니다.")]
public sealed class SimulationMeditationFocusAccessCandidateTests
{
    [Fact]
    public void 네책임Revision이_모두있어야_집중접근후보가_준비된다()
    {
        var result = new SimulationMeditationFocusAccessCandidateEvaluator()
            .Evaluate(new[]
            {
                Binding(Simulation명상집중접근CandidateCodes
                    .EverydayActionAccess, "everyday-focus.r1"),
                Binding(Simulation명상집중접근CandidateCodes
                    .FocusThresholdCurve, "focus-threshold.r1"),
                Binding(Simulation명상집중접근CandidateCodes
                    .BasicAttackEligibility, "basic-attack-focus.r1"),
                Binding(Simulation명상집중접근CandidateCodes
                    .CurrentFocusRole, "current-focus-role.r1"),
            });

        Assert.Equal(Simulation명상집중접근CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Empty(result.MissingResponsibilityCodes);
        Assert.True(result.ReusesExistingFocusPolicy);
        Assert.Equal(Simulation집중판정Codes.FocusProfileCatalogRevision,
            result.FocusProfileCatalogRevision);
        Assert.False(result.GuaranteesCriticalOutcome);
    }

    [Fact]
    public void 순간집중역할과_기본공격결속이_없으면_공백으로남는다()
    {
        var result = new SimulationMeditationFocusAccessCandidateEvaluator()
            .Evaluate(new[]
            {
                Binding(Simulation명상집중접근CandidateCodes
                    .EverydayActionAccess, "everyday-focus.r1"),
                Binding(Simulation명상집중접근CandidateCodes
                    .FocusThresholdCurve, "focus-threshold.r1"),
            });

        Assert.Equal(Simulation명상집중접근CandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Contains(Simulation명상집중접근CandidateCodes
            .BasicAttackEligibility, result.MissingResponsibilityCodes);
        Assert.Contains(Simulation명상집중접근CandidateCodes
            .CurrentFocusRole, result.MissingResponsibilityCodes);
        Assert.Contains(Simulation명상집중접근CandidateCodes
            .CriticalOutcomeUnresolved, result.UnresolvedDecisionCodes);
    }

    private static Simulation명상집중접근RevisionBinding Binding(
        string code, string revision)
        => new Simulation명상집중접근RevisionBinding
        {
            ResponsibilityCode = code,
            RuleRevision = revision,
        };
}
