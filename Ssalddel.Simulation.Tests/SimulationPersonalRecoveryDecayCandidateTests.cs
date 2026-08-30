using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q017 개인 Recovery 기본 감쇠와 위협·피로·집중 실패 추가 원인 순서를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 계획 시험이며 실제 감쇠 수치·WorldTick·Save/Replay·Play Mode 증거가 아니다.")]
public sealed class SimulationPersonalRecoveryDecayCandidateTests
{
    [Fact]
    public void 권위시간기본감쇠뒤에_상황별추가감쇠를_고정순서로계획한다()
    {
        var result = new SimulationPersonalRecoveryDecayCandidatePlanner()
            .Plan("authority-game-time.r1", "recovery-decay.r1",
                threatExposure: true, fatigueAccumulation: true,
                focusFailure: true);

        Assert.Equal(Simulation개인회복감쇠CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.Equal(Simulation개인회복감쇠CandidateCodes
                .OrderedCauseCodes(),
            result.OrderedCauses.Select(value => value.CauseCode));
        Assert.All(result.OrderedCauses, value => Assert.True(value.Active));
        Assert.True(result.UsesAuthorityGameTime);
        Assert.False(result.UsesUnityDeltaTime);
        Assert.False(result.AppliesRecoveryDecay);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation개인회복감쇠CandidateCodes
            .OfflineTimeOwnedByQ018, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 상황원인이없어도_권위시간기본감쇠는활성이다()
    {
        var result = new SimulationPersonalRecoveryDecayCandidatePlanner()
            .Plan("authority-game-time.r1", "recovery-decay.r1",
                false, false, false);

        Assert.True(result.OrderedCauses[0].Active);
        Assert.All(result.OrderedCauses.Skip(1), value =>
            Assert.False(value.Active));
    }
}
