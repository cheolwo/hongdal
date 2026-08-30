using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q021 암흑기 제한 접근에서 개인 정신 차림 효과 허용과 세계·공동체 효과 거부를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 Profile 시험이며 실제 Effect 적용·전투·관찰·제작·Play Mode 증거가 아니다.")]
public sealed class SimulationDarkAgeMindfulnessEffectScopeCandidateTests
{
    [Theory]
    [InlineData(Simulation암흑기정신차림EffectScopeCandidateCodes.PersonalCombatFocus)]
    [InlineData(Simulation암흑기정신차림EffectScopeCandidateCodes.PersonalDeepObservation)]
    [InlineData(Simulation암흑기정신차림EffectScopeCandidateCodes.PersonalPrecisionCrafting)]
    public void 개인정신차림효과만_암흑기제한접근후보로허용한다(string effectCode)
    {
        var result = new SimulationDarkAgeMindfulnessEffectScopeCandidateEvaluator()
            .Evaluate(Request(effectCode));

        Assert.Equal(Simulation암흑기정신차림EffectScopeCandidateCodes.Allowed,
            result.AccessDecisionCode);
        Assert.True(result.PersonalMindfulnessEffect);
        Assert.False(result.WorldOrCommunityEffect);
        Assert.False(result.AppliesEffect);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation암흑기정신차림EffectScopeCandidateCodes
            .PersonalEffectStrengthOwnedByQ022,
            result.UnresolvedDecisionCodes);
    }

    [Theory]
    [InlineData(Simulation암흑기정신차림EffectScopeCandidateCodes.RegionalRestoration)]
    [InlineData(Simulation암흑기정신차림EffectScopeCandidateCodes.SpatialExpansion)]
    [InlineData(Simulation암흑기정신차림EffectScopeCandidateCodes.CommunityProduction)]
    public void 세계와공동체효과는_암흑기제한접근으로열지않는다(string effectCode)
    {
        var result = new SimulationDarkAgeMindfulnessEffectScopeCandidateEvaluator()
            .Evaluate(Request(effectCode));

        Assert.Equal(Simulation암흑기정신차림EffectScopeCandidateCodes.Denied,
            result.AccessDecisionCode);
        Assert.False(result.PersonalMindfulnessEffect);
        Assert.True(result.WorldOrCommunityEffect);
        Assert.False(result.AppliesEffect);
        Assert.False(result.ChangesWorldState);
    }

    private static Simulation암흑기정신차림EffectScopeCandidateRequest Request(
        string effectCode)
    {
        return new Simulation암흑기정신차림EffectScopeCandidateRequest
        {
            AccessCandidate = new Simulation암흑기정신차림접근CandidateSnapshot
            {
                ReadinessCode =
                    Simulation암흑기정신차림접근CandidateCodes.Ready,
                LimitedGwangbokEffectAccessCandidate = true,
                PlayerStableId = "player:test",
                DominantPeriodStateCode =
                    SimulationNaturePeriodCodes.DarkAgePeriod,
            },
            EffectCode = effectCode,
            ProfileRevision =
                Simulation암흑기정신차림EffectScopeCandidateCodes
                    .ProfileRevision,
            Profiles = SimulationDarkAgeMindfulnessEffectScopeCandidateEvaluator
                .DefaultProfiles(),
        };
    }
}
