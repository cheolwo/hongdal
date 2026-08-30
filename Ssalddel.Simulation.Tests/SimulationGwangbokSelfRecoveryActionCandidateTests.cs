using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q015 명상·집중 성공의 ActionRecord·회복 기여 자격과 완전한 수면 제외를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[]
    {
        "WI-REFLECT-01", "WI-NATURE-06", "WI-NATURE-14",
    },
    Boundary = "후보 계약 시험이며 실제 광복기 기간 전이·Unity 피드백·Play Mode 증거가 아니다.")]
public sealed class SimulationGwangbokSelfRecoveryActionCandidateTests
{
    [Fact]
    public void 집중성공과_회복기여를남긴벌목행위는_자기행위후보가된다()
    {
        var request = Request("WI-NATURE-06");
        request.FocusResult = new Simulation집중판정ResultSnapshot
        {
            ResultCode = Simulation집중판정Codes.Good,
            회복증가Milli = 100,
            SourceActionRecordStableId = request.ActionRecord.행위기록StableId,
        };

        var result = new SimulationGwangbokSelfRecoveryActionCandidateEvaluator()
            .Evaluate(request);

        Assert.Equal(Simulation광복기자기회복행위CandidateCodes.Eligible,
            result.EligibilityCode);
        Assert.True(result.HasSuccessfulActionRecord);
        Assert.True(result.HasRecoveryContribution);
        Assert.True(result.HasRequiredFocusSuccess);
        Assert.True(result.EligibleForGwangbokEntryTrigger);
        Assert.False(result.AppliesPeriodTransition);
        Assert.False(result.ChangesWorldState);
    }

    [Fact]
    public void 명상정신차림은_회복변화ActionRecord가있어야_후보가된다()
    {
        var result = new SimulationGwangbokSelfRecoveryActionCandidateEvaluator()
            .Evaluate(Request("WI-REFLECT-01"));

        Assert.Equal(Simulation광복기자기회복행위CandidateCodes.Eligible,
            result.EligibilityCode);
        Assert.Equal(Simulation광복기자기회복행위CandidateCodes
            .MindfulnessAction, result.ActionKindCode);
    }

    [Fact]
    public void 완전한수면은_회복변화가있어도_광복기진입계기에서제외한다()
    {
        var result = new SimulationGwangbokSelfRecoveryActionCandidateEvaluator()
            .Evaluate(Request("WI-NATURE-14"));

        Assert.Equal(Simulation광복기자기회복행위CandidateCodes.Ineligible,
            result.EligibilityCode);
        Assert.Contains(Simulation광복기자기회복행위CandidateCodes
            .CompleteSleepExcluded, result.ReasonCodes);
        Assert.False(result.EligibleForGwangbokEntryTrigger);
    }

    private static Simulation광복기자기회복행위CandidateRequest Request(
        string wi) => new()
        {
            TargetPlayerStableId = "player:owner",
            ProfileRevision =
                Simulation광복기자기회복행위CandidateCodes.ProfileRevision,
            Profiles = SimulationGwangbokSelfRecoveryActionCandidateEvaluator
                .CreateDefaultCandidateProfiles(),
            ActionRecord = new Simulation행위발현Record
            {
                행위기록StableId = "action:q015:1",
                WorldInteractionId = wi,
                결과분류Code = Simulation행위결과분류Codes.성공,
                변화의미Codes = new[]
                {
                    Simulation행위변화의미Codes.플레이어회복변경,
                },
            },
        };
}
