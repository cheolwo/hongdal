using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q022 현재 회복의 접근 책임과 장기 명상 숙련도의 효과 강도 책임 분리를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 판정 시험이며 실제 강도 계산·Effect 적용·Save/Replay·Play Mode 증거가 아니다.")]
public sealed class SimulationDarkAgeMindfulnessEffectStrengthCandidateTests
{
    [Fact]
    public void 현재회복은접근만_장기명상숙련도는강도후보만결정한다()
    {
        var result = new SimulationDarkAgeMindfulnessEffectStrengthCandidateEvaluator()
            .Evaluate(Request(recoveryShare: .01m, proficiency: 90m));

        Assert.Equal(Simulation암흑기정신차림EffectStrengthCandidateCodes.Ready,
            result.ReadinessCode);
        Assert.True(result.AccessAvailableFromCurrentRecovery);
        Assert.True(result.StrengthCandidateFromLongTermProficiency);
        Assert.False(result.UsesCurrentRecoveryShareForStrength);
        Assert.False(result.AppliesEffectStrength);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation암흑기정신차림EffectStrengthCandidateCodes
            .StrengthCurveUnresolved, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 현재회복이나장기숙련이없으면_강도후보를준비완료로보지않는다()
    {
        var result = new SimulationDarkAgeMindfulnessEffectStrengthCandidateEvaluator()
            .Evaluate(Request(recoveryShare: 0m, proficiency: 0m));

        Assert.Equal(Simulation암흑기정신차림EffectStrengthCandidateCodes.Gap,
            result.ReadinessCode);
        Assert.False(result.AccessAvailableFromCurrentRecovery);
        Assert.False(result.StrengthCandidateFromLongTermProficiency);
        Assert.Contains(Simulation암흑기정신차림EffectStrengthCandidateCodes
            .PositiveCurrentRecoveryShareRequired,
            result.MissingRequirementCodes);
    }

    private static Simulation암흑기정신차림EffectStrengthCandidateRequest Request(
        decimal recoveryShare, decimal proficiency)
    {
        return new Simulation암흑기정신차림EffectStrengthCandidateRequest
        {
            EffectScopeCandidate =
                new Simulation암흑기정신차림EffectScopeCandidateSnapshot
                {
                    AccessDecisionCode =
                        Simulation암흑기정신차림EffectScopeCandidateCodes
                            .Allowed,
                    PlayerStableId = "player:test",
                    EffectCode =
                        Simulation암흑기정신차림EffectScopeCandidateCodes
                            .PersonalCombatFocus,
                    PersonalMindfulnessEffect = true,
                },
            CurrentRecoveryShare = recoveryShare,
            LongTermMeditationProficiency = proficiency,
            StrengthProfileRevision = "mindfulness-strength.r1",
        };
    }
}
