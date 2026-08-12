using Ssalddel.Unity.Learning;

namespace Ssalddel.Unity.Tests;

public sealed class 저녁학당업무Preview보강Tests
{
    private readonly 저녁학당업무Preview보강Projector projector = new();

    [Fact]
    public void CARD_BIZ_1_집중규칙이없으면CanonicalPreview만그대로보존한다()
    {
        var input = PortReceivingInput(string.Empty);

        var result = projector.Project(input);

        AssertCanonicalPreserved(input, result);
        Assert.Empty(result.AppliedRuleCode);
        Assert.Empty(result.RevealedUnknowns);
        Assert.Empty(result.MilestoneEvidence);
        Assert.False(result.MayMutateCanonicalState);
        Assert.False(result.MayChangeAllowedIntents);
    }

    [Fact]
    public void CARD_BIZ_1_바보는모르는것만드러내고300kg과계보를바꾸지않는다()
    {
        var input = PortReceivingInput(내면규칙Codes.BeginnerMind);

        var result = projector.Project(input);

        AssertCanonicalPreserved(input, result);
        Assert.Equal(내면규칙Codes.BeginnerMind, result.AppliedRuleCode);
        Assert.Equal(2, result.RevealedUnknowns.Length);
        Assert.Contains(result.RevealedUnknowns, value =>
            value.QuestionText.Contains("통관 준비", StringComparison.Ordinal));
        Assert.Empty(result.MilestoneEvidence);
    }

    [Fact]
    public void CARD_BIZ_1_전차는이동Milestone만통합해보이고300kg을바꾸지않는다()
    {
        var input = PortReceivingInput(내면규칙Codes.IntegratedProgress);

        var result = projector.Project(input);

        AssertCanonicalPreserved(input, result);
        Assert.Equal(내면규칙Codes.IntegratedProgress, result.AppliedRuleCode);
        Assert.Empty(result.RevealedUnknowns);
        Assert.Equal(new[] { 1, 2, 3, 4 },
            result.MilestoneEvidence.Select(value => value.Sequence));
        Assert.Equal("ArrivedAtDestination", result.MilestoneEvidence[^1].StateCode);
    }

    [Fact]
    public void CARD_BIZ_1_여러보유규칙중FocusedRule한장만적용한다()
    {
        var input = PortReceivingInput(내면규칙Codes.IntegratedProgress);
        input.InnerState.ActiveRuleCodes = new[]
        {
            내면규칙Codes.BeginnerMind,
            내면규칙Codes.IntegratedProgress,
        };

        var result = projector.Project(input);

        Assert.Equal(내면규칙Codes.IntegratedProgress, result.AppliedRuleCode);
        Assert.Empty(result.RevealedUnknowns);
        Assert.NotEmpty(result.MilestoneEvidence);
    }

    [Fact]
    public void CARD_BIZ_1_보유하지않은FocusedRule은거부한다()
    {
        var input = PortReceivingInput(내면규칙Codes.BeginnerMind);
        input.InnerState.ActiveRuleCodes = new[] { 내면규칙Codes.IntegratedProgress };

        var error = Assert.Throws<InvalidOperationException>(() => projector.Project(input));

        Assert.Equal("EveningBusinessPreviewFocusedRuleNotActive:BeginnerMind", error.Message);
    }

    private static void AssertCanonicalPreserved(
        저녁학당업무Preview보강Input input,
        저녁학당업무Preview보강 result)
    {
        Assert.Equal(input.PreviewStableId, result.PreviewStableId);
        Assert.Equal(input.ExpectedDataRevision, result.ExpectedDataRevision);
        Assert.Equal(input.BusinessStageCode, result.BusinessStageCode);
        Assert.Equal(input.ProductStableId, result.ProductStableId);
        Assert.Equal(300m, result.Quantity);
        Assert.Equal(input.UnitCode, result.UnitCode);
        Assert.Equal(input.CanonicalSourceStableIds, result.CanonicalSourceStableIds);
        Assert.NotSame(input.CanonicalSourceStableIds, result.CanonicalSourceStableIds);
    }

    private static 저녁학당업무Preview보강Input PortReceivingInput(string focusedRuleCode)
        => new()
        {
            PreviewStableId = "export-port-receiving-preview:cargo.sim.potato-1.r42",
            ExpectedDataRevision = 42,
            BusinessStageCode = "EXPORT-PORT-RECEIVING-1",
            ProductStableId = "product:potato",
            Quantity = 300m,
            UnitCode = "kg",
            CanonicalSourceStableIds = new[]
            {
                "harvest-lot:potato.20260407",
                "cargo:sim.potato-1",
                "logistics-movement:sim.potato-1",
            },
            InnerState = new 플레이어내면상태Snapshot
            {
                알아차림 = 1,
                의지 = 1,
                ActiveRuleCodes = focusedRuleCode.Length == 0
                    ? Array.Empty<string>()
                    : new[] { focusedRuleCode },
            },
            FocusedRuleCode = focusedRuleCode,
            Unknowns = new[]
            {
                new 업무Preview미확인사항
                {
                    StableId = "preview-unknown:port.customs-readiness",
                    QuestionText = "통관 준비는 별도 확인되었는가?",
                    ReasonText = "항만 준비시설 인수는 실제 통관 완료를 뜻하지 않는다.",
                    SourceStableIds = new[] { "cargo:sim.potato-1" },
                },
                new 업무Preview미확인사항
                {
                    StableId = "preview-unknown:port.storage-condition",
                    QuestionText = "도착 뒤 보관 조건은 확인되었는가?",
                    ReasonText = "현재 Preview에는 운영 보관 계약이 없다.",
                    SourceStableIds = new[] { "logistics-movement:sim.potato-1" },
                },
            },
            Milestones = new[]
            {
                Milestone(3, "movement", "InTransit", "logistics-movement:sim.potato-1"),
                Milestone(1, "harvest", "Harvested", "harvest-lot:potato.20260407"),
                Milestone(4, "arrival", "ArrivedAtDestination", "logistics-movement:sim.potato-1"),
                Milestone(2, "handoff", "HandedOffInSimulation", "cargo:sim.potato-1"),
            },
        };

    private static 업무PreviewMilestone Milestone(
        int sequence,
        string suffix,
        string stateCode,
        string sourceStableId)
        => new()
        {
            StableId = "preview-milestone:export." + suffix,
            Sequence = sequence,
            TitleText = suffix,
            StateCode = stateCode,
            SourceStableIds = new[] { sourceStableId },
        };
}
