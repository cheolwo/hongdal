using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q014 공명 단독 광복기 진입 금지와 대상 플레이어 자기 회복 기여 관문을 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 계약 시험이며 실제 Recovery cap·기간 전이·ActionRecord·Play Mode 증거가 아니다.")]
public sealed class SimulationGwangbokResonanceEntryCapCandidateTests
{
    [Fact]
    public void 공명만으로는_광복기마지막문턱을_넘지못한다()
    {
        var result = new SimulationGwangbokResonanceEntryCapCandidateEvaluator()
            .Evaluate(Request(false));

        Assert.Equal(Simulation광복기공명상한CandidateCodes
                .CappedBeforeEntry,
            result.EntryDecisionCode);
        Assert.False(result.ResonanceOnlyEntryAllowed);
        Assert.False(result.EntryThresholdCrossingCandidate);
        Assert.Contains(Simulation광복기공명상한CandidateCodes
            .TargetOwnRecoveryContributionRequired, result.ReasonCodes);
        Assert.False(result.AppliesPeriodTransition);
        Assert.False(result.ChangesWorldState);
    }

    [Fact]
    public void 자기회복기여가있어야_진입판정후보가된다()
    {
        var result = new SimulationGwangbokResonanceEntryCapCandidateEvaluator()
            .Evaluate(Request(true));

        Assert.Equal(Simulation광복기공명상한CandidateCodes.EntryCandidate,
            result.EntryDecisionCode);
        Assert.True(result.EntryThresholdCrossingCandidate);
        Assert.False(result.ResonanceOnlyEntryAllowed);
        Assert.Contains(Simulation광복기공명상한CandidateCodes
            .EligibleSelfActionOwnedByQ015, result.UnresolvedDecisionCodes);
    }

    private static Simulation광복기공명상한CandidateRequest Request(
        bool ownContribution) => new()
        {
            TargetPlayerStableId = "player:companion",
            PeriodEntryPolicyRevision = "nature-period-entry.r1",
            TargetOwnRecoveryContributionPresent = ownContribution,
            StackingCandidate = new Simulation파티공명중첩CandidateSnapshot
            {
                ReadinessCode =
                    Simulation파티공명중첩CandidateCodes.Ready,
            },
        };
}
