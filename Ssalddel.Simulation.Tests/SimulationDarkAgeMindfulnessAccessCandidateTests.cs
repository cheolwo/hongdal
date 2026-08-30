using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q020 암흑기 우세 유지와 극한 명상 숙련자의 제한적 효과 접근 후보 분리를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 판정 시험이며 실제 기간 전이·Effect 적용·Save/Replay·Play Mode 증거가 아니다.")]
public sealed class SimulationDarkAgeMindfulnessAccessCandidateTests
{
    [Fact]
    public void 극한위협에서는_암흑기를유지하고_정신차림효과접근만별도후보로둔다()
    {
        var result = new SimulationDarkAgeMindfulnessAccessCandidateEvaluator()
            .Evaluate(Request(SimulationNaturePeriodCodes.DarkAgePeriod,
                recoveryShare: .01m, threatShare: .99m,
                extremeProficiency: true));

        Assert.Equal(Simulation암흑기정신차림접근CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Equal(SimulationNaturePeriodCodes.DarkAgePeriod,
            result.DominantPeriodStateCode);
        Assert.True(result.PreservesSingleDominantPeriodState);
        Assert.True(result.DarkAgeRemainsDominant);
        Assert.True(result.LimitedGwangbokEffectAccessCandidate);
        Assert.False(result.ReplacesPeriodStateCode);
        Assert.False(result.AppliesEffectAccess);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation암흑기정신차림접근CandidateCodes
            .AllowedEffectScopeOwnedByQ021, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 암흑기가아니거나_회복이나극한숙련이없으면_접근후보를열지않는다()
    {
        var result = new SimulationDarkAgeMindfulnessAccessCandidateEvaluator()
            .Evaluate(Request(SimulationNaturePeriodCodes.OrdinaryPeriod,
                recoveryShare: 0m, threatShare: 1m,
                extremeProficiency: false));

        Assert.Equal(Simulation암흑기정신차림접근CandidateCodes.Gap,
            result.ReadinessCode);
        Assert.False(result.DarkAgeRemainsDominant);
        Assert.False(result.LimitedGwangbokEffectAccessCandidate);
        Assert.Contains(Simulation암흑기정신차림접근CandidateCodes
            .DarkAgePeriodRequired, result.MissingRequirementCodes);
        Assert.Contains(Simulation암흑기정신차림접근CandidateCodes
            .PositiveRecoveryShareRequired, result.MissingRequirementCodes);
    }

    private static Simulation암흑기정신차림접근CandidateRequest Request(
        string periodCode, decimal recoveryShare, decimal threatShare,
        bool extremeProficiency)
    {
        return new Simulation암흑기정신차림접근CandidateRequest
        {
            RecoveryThreatOffsetCandidate =
                new Simulation개인회복위협상쇄CandidateSnapshot
                {
                    ReadinessCode =
                        Simulation개인회복위협상쇄CandidateCodes.Ready,
                },
            Period = new SimulationNaturePeriodStateSnapshot
            {
                PlayerStableId = "player:test",
                PeriodStateCode = periodCode,
            },
            RecoveryShare = recoveryShare,
            ThreatShare = threatShare,
            ExtremeMeditationProficiencyReached = extremeProficiency,
            ConflictPolicyRevision = "period-conflict.r1",
        };
    }
}
