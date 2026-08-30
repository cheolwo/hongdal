using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q010 파티 근접 공명 조건과 역할 배정·권위 변경 금지 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 계약 시험이며 실제 온라인 거리·NatureMind 회복·Unity 피드백·Play Mode 증거가 아니다.")]
public sealed class SimulationPartyProximityResonanceCandidateTests
{
    [Fact]
    public void 같은파티의_승인된명상숙련자가_가까우면_수동공명후보가생긴다()
    {
        var result = new SimulationPartyProximityResonanceCandidateEvaluator()
            .Evaluate(EligibleRequest());

        Assert.Equal(Simulation파티근접공명CandidateCodes.Eligible,
            result.EligibilityCode);
        Assert.True(result.PassiveEffectCandidateCreated);
        Assert.Empty(result.ReasonCodes);
        Assert.False(result.RequiresRoleProposal);
        Assert.False(result.RequiresRoleAcceptance);
        Assert.False(result.AssignsRole);
        Assert.False(result.ReadsPrivateGrowthProfile);
        Assert.False(result.ChangesNatureMindState);
        Assert.False(result.ChangesWorldState);
        Assert.Equal(Simulation파티근접공명CandidateCodes
                .EffectOutcomeOwnedByQ011,
            result.PendingEffectOutcomeCode);
    }

    [Fact]
    public void 다른파티이거나_근접하지않으면_공명후보가생기지않는다()
    {
        var request = EligibleRequest();
        request.TargetPartyStableId = "party:other";
        request.IsWithinApprovedProximity = false;

        var result = new SimulationPartyProximityResonanceCandidateEvaluator()
            .Evaluate(request);

        Assert.Equal(Simulation파티근접공명CandidateCodes.Ineligible,
            result.EligibilityCode);
        Assert.False(result.PassiveEffectCandidateCreated);
        Assert.Contains(Simulation파티근접공명CandidateCodes
            .SamePartyRequired, result.ReasonCodes);
        Assert.Contains(Simulation파티근접공명CandidateCodes
            .ApprovedProximityRequired, result.ReasonCodes);
    }

    [Fact]
    public void 자격정책판본없이_높은명상숙련을_임의판정하지않는다()
    {
        var request = EligibleRequest();
        request.MeditationEligibilityPolicyRevision = string.Empty;
        request.ProviderEligibleByMeditationPolicy = false;

        var result = new SimulationPartyProximityResonanceCandidateEvaluator()
            .Evaluate(request);

        Assert.False(result.PassiveEffectCandidateCreated);
        Assert.Contains(Simulation파티근접공명CandidateCodes
            .MeditationPolicyRevisionRequired, result.ReasonCodes);
        Assert.Contains(Simulation파티근접공명CandidateCodes
            .ProviderMeditationEligibilityRequired, result.ReasonCodes);
    }

    private static Simulation파티근접공명CandidateRequest EligibleRequest()
        => new()
        {
            ProviderPlayerStableId = "player:meditator",
            TargetPlayerStableId = "player:companion",
            ProviderPartyStableId = "party:nature:a",
            TargetPartyStableId = "party:nature:a",
            ProviderEligibleByMeditationPolicy = true,
            IsWithinApprovedProximity = true,
            MeditationEligibilityPolicyRevision =
                "meditation-resonance-eligibility.r1",
            ProximityPolicyRevision = "party-resonance-proximity.r1",
        };
}
