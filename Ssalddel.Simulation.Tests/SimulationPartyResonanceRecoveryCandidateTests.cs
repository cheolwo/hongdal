using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q011 파티 공명이 개인 Recovery 축만 겨냥하고 분야별 직접 버프와 권위 변경을 만들지 않음을 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 계약 시험이며 실제 MindImpact 적용·수치 균형·Unity 표현·Play Mode 증거가 아니다.")]
public sealed class SimulationPartyResonanceRecoveryCandidateTests
{
    [Fact]
    public void 승인된근접공명은_개인회복축후보로만_해석된다()
    {
        var result = new SimulationPartyResonanceRecoveryCandidateResolver()
            .Resolve(new Simulation파티공명회복CandidateRequest
            {
                ProximityCandidate = EligibleProximity(),
                EffectPolicyRevision = "party-resonance-effect.r1",
            });

        Assert.Equal(Simulation파티공명회복CandidateCodes.Eligible,
            result.EligibilityCode);
        Assert.Equal(SimulationNatureMindCodes.RecoveryAxis,
            result.TargetAxisCode);
        Assert.Equal(Simulation파티공명회복CandidateCodes.PartyResonance,
            result.SourceCode);
        Assert.False(result.CreatesDirectCombatModifier);
        Assert.False(result.CreatesDirectCraftModifier);
        Assert.False(result.CreatesDirectGatheringModifier);
        Assert.False(result.ChangesRegionalThreat);
        Assert.False(result.AppliesMindImpactEffect);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation파티공명회복CandidateCodes
            .PersistenceOwnedByQ012, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 근접공명후보가없으면_회복축후보를_승인하지않는다()
    {
        var proximity = EligibleProximity();
        proximity.EligibilityCode =
            Simulation파티근접공명CandidateCodes.Ineligible;
        proximity.PassiveEffectCandidateCreated = false;

        var result = new SimulationPartyResonanceRecoveryCandidateResolver()
            .Resolve(new Simulation파티공명회복CandidateRequest
            {
                ProximityCandidate = proximity,
                EffectPolicyRevision = "party-resonance-effect.r1",
            });

        Assert.Equal(Simulation파티공명회복CandidateCodes.Ineligible,
            result.EligibilityCode);
        Assert.Contains(Simulation파티공명회복CandidateCodes
            .PartyProximityCandidateRequired, result.ReasonCodes);
    }

    private static Simulation파티근접공명CandidateSnapshot EligibleProximity()
        => new()
        {
            EligibilityCode =
                Simulation파티근접공명CandidateCodes.Eligible,
            ProviderPlayerStableId = "player:meditator",
            TargetPlayerStableId = "player:companion",
            PartyStableId = "party:nature:a",
            PassiveEffectCandidateCreated = true,
        };
}
