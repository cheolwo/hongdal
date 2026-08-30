using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q002 단계적 수면 안전 후보와 미결정 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-14" },
    Boundary = "후보 계약 시험이며 실제 수면 허용·연료 소비·질병 결과나 Play Mode 증거가 아니다.")]
public sealed class SimulationNatureSleepSafetyCandidateTests
{
    [Fact]
    public void 동물위협은_오두막과불을_후보요구로_드러낸다()
    {
        var result = new SimulationNatureSleepSafetyCandidateEvaluator()
            .Evaluate(SimulationNatureSleepSafetyCandidateCodes.AnimalThreat,
                new[] { SimulationNatureSleepSafetyCandidateCodes.Cabin },
                1, 3);

        Assert.Equal(SimulationNatureSleepSafetyCandidateCodes
            .PlanningCandidate, result.DecisionStatusCode);
        Assert.Equal(SimulationNatureSleepSafetyCandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Equal(new[] { "animal-deterrence" },
            result.MissingRequirementStableIds);
        Assert.Contains(SimulationNatureSleepSafetyCandidateCodes
            .SleepPermissionPolicyUnresolved,
            result.UnresolvedDecisionCodes);
        Assert.Contains(SimulationNatureSleepSafetyCandidateCodes
            .FireFuelCostUnresolved, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 몬스터위협은_울타리와마법진_중_하나를_후보요구로_받는다()
    {
        var result = new SimulationNatureSleepSafetyCandidateEvaluator()
            .Evaluate(SimulationNatureSleepSafetyCandidateCodes.MonsterThreat,
                new[]
                {
                    SimulationNatureSleepSafetyCandidateCodes.Cabin,
                    SimulationNatureSleepSafetyCandidateCodes.MagicCircle,
                }, 0, 2);

        Assert.Equal(SimulationNatureSleepSafetyCandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Empty(result.MissingRequirementStableIds);
        Assert.True(result.DiseaseIncrementBoundsDefined);
    }

    [Fact]
    public void 질병증분의_최소최대가_없거나_뒤집히면_준비공백이다()
    {
        var result = new SimulationNatureSleepSafetyCandidateEvaluator()
            .Evaluate(SimulationNatureSleepSafetyCandidateCodes.Temperate,
                new[] { SimulationNatureSleepSafetyCandidateCodes.Cabin },
                3, 1);

        Assert.Equal(SimulationNatureSleepSafetyCandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Contains(SimulationNatureSleepSafetyCandidateCodes
            .DiseaseIncrementBoundsRequired,
            result.MissingRequirementStableIds);
        Assert.False(result.DiseaseIncrementBoundsDefined);
    }
}
