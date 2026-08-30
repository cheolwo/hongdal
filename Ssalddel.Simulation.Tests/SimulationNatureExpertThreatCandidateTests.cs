using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q005 숙련자 위협 강화 세 축과 기존 집중 체계 결속 후보를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-11", "WI-NATURE-14" },
    Boundary = "후보 계약 시험이며 실제 Spawn·전투·보상·집중 소비·Play Mode 증거가 아니다.")]
public sealed class SimulationNatureExpertThreatCandidateTests
{
    [Fact]
    public void 세위협축과_집중요구Revision이_모두있어야_준비된다()
    {
        var result = new SimulationNatureExpertThreatCandidateEvaluator()
            .Evaluate(new[]
            {
                Dimension(SimulationNatureExpertThreatCandidateCodes
                    .SpawnFrequency, "spawn-frequency.r1"),
                Dimension(SimulationNatureExpertThreatCandidateCodes
                    .GroupSize, "group-size.r1"),
                Dimension(SimulationNatureExpertThreatCandidateCodes
                    .IndividualAbility, "individual-ability.r1"),
            }, "expert-focus-requirement.r1");

        Assert.Equal(SimulationNatureExpertThreatCandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Empty(result.MissingRequirementCodes);
        Assert.True(result.ReusesExistingMeditationSystem);
        Assert.Equal(Simulation집중판정Codes.FocusProfileCatalogRevision,
            result.FocusProfileCatalogRevision);
        Assert.False(result.ChangesBaseWorldInteractionOutcome);
    }

    [Fact]
    public void 집중요구와_개별능력축이_없으면_공백을_명시한다()
    {
        var result = new SimulationNatureExpertThreatCandidateEvaluator()
            .Evaluate(new[]
            {
                Dimension(SimulationNatureExpertThreatCandidateCodes
                    .SpawnFrequency, "spawn-frequency.r1"),
                Dimension(SimulationNatureExpertThreatCandidateCodes
                    .GroupSize, "group-size.r1"),
            }, string.Empty);

        Assert.Equal(SimulationNatureExpertThreatCandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Contains(SimulationNatureExpertThreatCandidateCodes
            .IndividualAbility, result.MissingRequirementCodes);
        Assert.Contains(SimulationNatureExpertThreatCandidateCodes
            .FocusRequirement, result.MissingRequirementCodes);
        Assert.Contains(SimulationNatureExpertThreatCandidateCodes
            .ThreatRewardScalingUnresolved,
            result.UnresolvedDecisionCodes);
    }

    private static SimulationNatureThreatIntensityDimensionRevision Dimension(
        string code, string revision)
        => new SimulationNatureThreatIntensityDimensionRevision
        {
            DimensionCode = code,
            RuleRevision = revision,
        };
}
