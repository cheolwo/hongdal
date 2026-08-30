using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q023 위험 수면의 위협 접근 중단과 환경 노출 기상 결과 분리를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-14", "WI-NATURE-11" },
    Boundary = "후보 판정 시험이며 실제 수면 Task·전투/후퇴·신체 상태·Play Mode 증거가 아니다.")]
public sealed class SimulationNatureRiskySleepOutcomeCandidateTests
{
    [Fact]
    public void 동물과몬스터접근은_수면을중단하고_전투후퇴선택으로인계한다()
    {
        var request = ReadyRequest();
        request.AnimalApproachDetected = true;
        request.MonsterApproachDetected = true;

        var result = new SimulationNatureRiskySleepOutcomeCandidateEvaluator()
            .Evaluate(request);

        Assert.Equal(SimulationNatureRiskySleepOutcomeCandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Equal(SimulationNatureRiskySleepOutcomeCandidateCodes
            .OrderedThreatApproachCodes(), result.ThreatApproachCodes);
        Assert.True(result.InterruptsSleepForThreatApproach);
        Assert.True(result.ReturnsCombatOrRetreatChoice);
        Assert.False(result.AppliesSleepInterruption);
        Assert.False(result.ChangesWorldState);
    }

    [Fact]
    public void 추위강수질병위험은_수면을즉시중단하지않고_기상결과로누적한다()
    {
        var request = ReadyRequest();
        request.ColdExposureDetected = true;
        request.PrecipitationExposureDetected = true;
        request.DiseaseRiskAccumulated = true;

        var result = new SimulationNatureRiskySleepOutcomeCandidateEvaluator()
            .Evaluate(request);

        Assert.False(result.InterruptsSleepForThreatApproach);
        Assert.False(result.ReturnsCombatOrRetreatChoice);
        Assert.True(result.DefersEnvironmentalOutcomeUntilWake);
        Assert.Contains(SimulationNatureRiskySleepOutcomeCandidateCodes
            .FatigueWakeOutcome, result.AccumulatedWakeOutcomeCodes);
        Assert.Contains(SimulationNatureRiskySleepOutcomeCandidateCodes
            .TemperatureWakeOutcome, result.AccumulatedWakeOutcomeCodes);
        Assert.Contains(SimulationNatureRiskySleepOutcomeCandidateCodes
            .DiseaseRiskWakeOutcome, result.AccumulatedWakeOutcomeCodes);
        Assert.False(result.AppliesWakeOutcome);
        Assert.Contains(SimulationNatureRiskySleepOutcomeCandidateCodes
            .WeatherProfileBindingOwnedByQ024,
            result.UnresolvedDecisionCodes);
    }

    private static SimulationNatureRiskySleepOutcomeCandidateRequest
        ReadyRequest()
    {
        return new SimulationNatureRiskySleepOutcomeCandidateRequest
        {
            SleepSafetyCandidate = new SimulationNatureSleepSafetyCandidateEvaluator()
                .Evaluate(SimulationNatureSleepSafetyCandidateCodes.Temperate,
                    new[] { SimulationNatureSleepSafetyCandidateCodes.Cabin },
                    0, 2),
            WeatherInputRevision = "weather-input-candidate.r1",
        };
    }
}
