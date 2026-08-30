using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q012 공명 종료 뒤 잔향과 권위 Tick 기반 감쇠·저장 요구 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 계약 시험이며 실제 감쇠·Save/Replay·Unity 표현·Play Mode 증거가 아니다.")]
public sealed class SimulationPartyResonanceAfterglowCandidateTests
{
    [Fact]
    public void 승인된회복공명과_시간정책이있으면_잔향준비가성립한다()
    {
        var result = new SimulationPartyResonanceAfterglowCandidateEvaluator()
            .Evaluate(new Simulation파티공명잔향CandidateRequest
            {
                RecoveryCandidate = EligibleRecovery(),
                DurationPolicyRevision = "resonance-duration.r1",
                DecayCurveRevision = "resonance-decay.r1",
                AuthorityTimeRevision = "world-tick.r1",
            });

        Assert.Equal(Simulation파티공명잔향CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.True(result.LeavesAfterglowOnProximityExit);
        Assert.False(result.RemovesEffectImmediatelyOnExit);
        Assert.True(result.UsesAuthorityWorldTick);
        Assert.False(result.UsesUnityDeltaTime);
        Assert.True(result.RequiresRemainingMagnitudeInSave);
        Assert.True(result.RequiresReferenceTickInSave);
        Assert.False(result.AppliesAfterglowState);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation파티공명잔향CandidateCodes
            .StackingOwnedByQ013, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 권위시간판본이없으면_잔향준비를_승인하지않는다()
    {
        var result = new SimulationPartyResonanceAfterglowCandidateEvaluator()
            .Evaluate(new Simulation파티공명잔향CandidateRequest
            {
                RecoveryCandidate = EligibleRecovery(),
                DurationPolicyRevision = "resonance-duration.r1",
                DecayCurveRevision = "resonance-decay.r1",
            });

        Assert.Equal(Simulation파티공명잔향CandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Contains(Simulation파티공명잔향CandidateCodes
            .AuthorityTimeRevisionRequired,
            result.MissingRequirementCodes);
    }

    private static Simulation파티공명회복CandidateSnapshot EligibleRecovery()
        => new()
        {
            EligibilityCode =
                Simulation파티공명회복CandidateCodes.Eligible,
            ProviderPlayerStableId = "player:meditator",
            TargetPlayerStableId = "player:companion",
            PartyStableId = "party:nature:a",
        };
}
