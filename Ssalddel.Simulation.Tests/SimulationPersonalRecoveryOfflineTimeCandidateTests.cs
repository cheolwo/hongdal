using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q018 오프라인 현실 시간 감쇠 금지와 권위 게임 시간 재개 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 판정 시험이며 실제 Save/Load·WorldTick 감쇠·Replay·Play Mode 증거가 아니다.")]
public sealed class SimulationPersonalRecoveryOfflineTimeCandidateTests
{
    [Fact]
    public void 오프라인벽시계는_감쇠입력에서제외하고_권위게임시간재개만요구한다()
    {
        var result = new SimulationPersonalRecoveryOfflineTimeCandidateEvaluator()
            .Evaluate(ReadyRequest());

        Assert.Equal(Simulation개인회복오프라인시간CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.True(result.PausesDuringOfflineRealTime);
        Assert.False(result.AppliesOfflineRealTimeDecay);
        Assert.False(result.UsesWallClockElapsedTime);
        Assert.True(result.ResumesOnAuthorityGameTime);
        Assert.True(result.RequiresSaveReferenceTick);
        Assert.True(result.SaveReferenceTickAvailable);
        Assert.False(result.AppliesRecoveryDecay);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation개인회복오프라인시간CandidateCodes
            .ThreatOffsetOwnedByQ019, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 저장기준Tick이없으면_오프라인정책후보를준비완료로보지않는다()
    {
        var request = ReadyRequest();
        request.SaveReferenceTickAvailable = false;

        var result = new SimulationPersonalRecoveryOfflineTimeCandidateEvaluator()
            .Evaluate(request);

        Assert.Equal(Simulation개인회복오프라인시간CandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Contains(Simulation개인회복오프라인시간CandidateCodes
            .SaveReferenceTickRequired, result.MissingRequirementCodes);
    }

    private static Simulation개인회복오프라인시간CandidateRequest ReadyRequest()
    {
        return new Simulation개인회복오프라인시간CandidateRequest
        {
            RecoveryDecayCandidate =
                new SimulationPersonalRecoveryDecayCandidatePlanner().Plan(
                    "authority-game-time.r1", "recovery-decay.r1",
                    threatExposure: false, fatigueAccumulation: false,
                    focusFailure: false),
            OfflinePolicyRevision = "recovery-offline-time.r1",
            SaveStateRevision = "simulation-save-reference.r1",
            SaveReferenceTickAvailable = true,
        };
    }
}
