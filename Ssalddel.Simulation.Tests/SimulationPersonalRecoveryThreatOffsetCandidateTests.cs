using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q019 개인 Recovery의 Threat 상쇄와 자기 회복 행위 가속·숙련도 문턱 완화 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 판정 시험이며 실제 Threat 변경·기간 전이·Save/Replay·Play Mode 증거가 아니다.")]
public sealed class SimulationPersonalRecoveryThreatOffsetCandidateTests
{
    [Fact]
    public void 개인회복이있으면_같은플레이어위협상쇄후보가되고_자기행위가가속한다()
    {
        var result = new SimulationPersonalRecoveryThreatOffsetCandidateEvaluator()
            .Evaluate(ReadyRequest(recovery: 4m, threat: 7m,
                selfAction: true, proficiency: 3m));

        Assert.Equal(Simulation개인회복위협상쇄CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.True(result.ThreatOffsetCandidate);
        Assert.True(result.AcceleratedByEligibleSelfRecoveryAction);
        Assert.True(result.ProficiencyAdjustedGwangbokThresholdCandidate);
        Assert.False(result.AppliesThreatOffset);
        Assert.False(result.AppliesPeriodTransition);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation개인회복위협상쇄CandidateCodes
            .PeriodConflictOwnedByQ020, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 회복이나위협이없으면_상쇄와자기행위가속후보를만들지않는다()
    {
        var result = new SimulationPersonalRecoveryThreatOffsetCandidateEvaluator()
            .Evaluate(ReadyRequest(recovery: 0m, threat: 7m,
                selfAction: true, proficiency: 0m));

        Assert.False(result.ThreatOffsetCandidate);
        Assert.False(result.AcceleratedByEligibleSelfRecoveryAction);
        Assert.False(result.ProficiencyAdjustedGwangbokThresholdCandidate);
    }

    private static Simulation개인회복위협상쇄CandidateRequest ReadyRequest(
        decimal recovery, decimal threat, bool selfAction,
        decimal proficiency)
    {
        var decay = new SimulationPersonalRecoveryDecayCandidatePlanner().Plan(
            "authority-game-time.r1", "recovery-decay.r1",
            false, false, false);
        var offline = new SimulationPersonalRecoveryOfflineTimeCandidateEvaluator()
            .Evaluate(new Simulation개인회복오프라인시간CandidateRequest
            {
                RecoveryDecayCandidate = decay,
                OfflinePolicyRevision = "recovery-offline-time.r1",
                SaveStateRevision = "simulation-save-reference.r1",
                SaveReferenceTickAvailable = true,
            });
        return new Simulation개인회복위협상쇄CandidateRequest
        {
            OfflineTimeCandidate = offline,
            PlayerStableId = "player:test",
            RecoveryOutput = recovery,
            ThreatOutput = threat,
            EligibleSelfRecoveryActionPresent = selfAction,
            MeditationProficiency = proficiency,
            OffsetPolicyRevision = "recovery-threat-offset.r1",
            PeriodThresholdProfileRevision = "gwangbok-threshold-profile.r1",
        };
    }
}
