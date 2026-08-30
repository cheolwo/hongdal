using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "개인계획 설정·수정·일회성 근거·무변경 Preview·멱등·거부를 검증한다.",
    Boundary = "단일 슬롯 Core 시험. 회복량·진척·Save·화면은 미검증.",
    WorldInteractionIds = new[] { Simulation개인계획Codes.WorldInteractionId })]
public sealed class Simulation개인계획Tests
{
    private static Simulation개인계획InitialState 초기값() => new()
    {
        WorldStableId = "world:fixture", SessionStableId = "session:fixture", PlayerStableId = "player:one",
        PlanStableId = "plan:one", PolicyRevision = "fixture-personal-plan.r1", MaxDescriptionLength = 60,
        AllowedObjectiveStableIds = new[] { "objective:wood", "objective:shelter" },
    };
    private static Simulation개인계획ConfirmRequest 요청(long 개정 = 0, string 명령 = "command:1", string 문구 = "땔감을 모으자") => new()
    {
        ActorStableId = "player:one", PlanStableId = "plan:one", ObjectiveStableId = "objective:wood",
        Description = 문구, ExpectedRevision = 개정, CommandId = 명령,
    };

    [Fact]
    public void Preview는_최초안정근거까지_발행하지않는다()
    {
        var 원장 = new Simulation개인계획Aggregate(초기값()); var 이전 = 원장.Snapshot();
        var 결과 = 원장.Preview(요청());
        Assert.True(결과.CanConfirm); Assert.True(결과.InitialStabilityEligibilityWouldBeRecorded);
        Assert.Equal(이전.StateHashSha256, 원장.Snapshot().StateHashSha256);
        Assert.Empty(원장.Snapshot().InitialStabilityEligibilityStableId);
        Assert.Empty(원장.Snapshot().ActionLedger.TailRecords);
    }

    [Fact]
    public void 계획설정은_시간공간조건없이_하나의결과와_보상전근거만_남긴다()
    {
        var 원장 = new Simulation개인계획Aggregate(초기값()); var 결과 = 원장.Confirm(요청()).State;
        Assert.Equal("땔감을 모으자", 결과.Description); Assert.Equal(1, 결과.WorldRevision);
        Assert.NotEmpty(결과.InitialStabilityEligibilityStableId); Assert.False(결과.RecoveryApplied);
        Assert.Equal("NumericRecoveryAndProgressPolicyUndefined", 결과.RecoveryNotAppliedReasonCode);
        var 행위 = Assert.Single(결과.ActionLedger.TailRecords);
        Assert.Equal("PersonalPlanSet", 행위.PrimaryOutcomeCode);
        Assert.Equal(Simulation개인계획Codes.WorldInteractionId, 행위.WorldInteractionId);
        Assert.Equal(0, 행위.BeforeWorldRevision); Assert.Equal(1, 행위.AfterWorldRevision);
        Assert.Contains(결과.InitialStabilityEligibilityStableId, 행위.SourceReferenceIds);
        Assert.DoesNotContain("땔감을 모으자", string.Join(",", 행위.SourceReferenceIds));
    }

    [Fact]
    public void 문구수정과목표왕복은_최초안정근거를_재발급하지않는다()
    {
        var 원장 = new Simulation개인계획Aggregate(초기값()); var 최초 = 원장.Confirm(요청()).State;
        var 두번째 = 요청(1, "command:2", "조금씩 땔감을 모으자");
        Assert.False(원장.Preview(두번째).InitialStabilityEligibilityWouldBeRecorded);
        원장.Confirm(두번째);
        var 세번째 = 요청(2, "command:3", "쉼터를 고치자"); 세번째.ObjectiveStableId = "objective:shelter";
        원장.Confirm(세번째);
        var 마지막 = 원장.Confirm(요청(3, "command:4")).State;
        Assert.Equal(최초.InitialStabilityEligibilityStableId, 마지막.InitialStabilityEligibilityStableId);
        Assert.Single(마지막.ActionLedger.TailRecords, x => x.SourceReferenceIds.Contains(최초.InitialStabilityEligibilityStableId));
        Assert.False(마지막.RecoveryApplied); Assert.Equal(4, 마지막.WorldRevision);
    }

    [Fact]
    public void 재전송은_최초결과를_격리해서반환하고_다른입력은_거부한다()
    {
        var 원장 = new Simulation개인계획Aggregate(초기값()); var 최초 = 원장.Confirm(요청());
        var 해시 = 최초.State.StateHashSha256; 최초.State.Description = "오염";
        최초.State.ActionLedger.TailRecords[0].ActorStableId = "오염";
        원장.Confirm(요청(1, "command:2", "목표를 다듬자"));
        var 재전송 = 원장.Confirm(요청(문구: " 땔감을 모으자 "));
        Assert.True(재전송.Reused); Assert.Equal(해시, 재전송.State.StateHashSha256);
        Assert.Equal("땔감을 모으자", 재전송.State.Description);
        Assert.Equal("player:one", 재전송.State.ActionLedger.TailRecords[0].ActorStableId);
        Assert.Equal(2, 원장.Snapshot().WorldRevision);
        Assert.Throws<SimulationConflictException>(() => 원장.Confirm(요청(문구: "다른 목표")));
    }

    [Theory]
    [InlineData("actor", "PersonalPlanActorNotAuthorized")]
    [InlineData("slot", "PersonalPlanUnknown")]
    [InlineData("objective", "PersonalPlanObjectiveUnknown")]
    [InlineData("revision", "PersonalPlanExpectedRevisionMismatch")]
    [InlineData("blank", "PersonalPlanDescriptionInvalid")]
    [InlineData("long", "PersonalPlanDescriptionInvalid")]
    [InlineData("control", "PersonalPlanDescriptionInvalid")]
    public void 거부는_계획과기록을_바꾸지않는다(string 경우, string 오류)
    {
        var 원장 = new Simulation개인계획Aggregate(초기값()); var 명령 = 요청();
        switch (경우)
        {
            case "actor": 명령.ActorStableId = "player:other"; break;
            case "slot": 명령.PlanStableId = "plan:other"; break;
            case "objective": 명령.ObjectiveStableId = "objective:unknown"; break;
            case "revision": 명령.ExpectedRevision = 7; break;
            case "blank": 명령.Description = " "; break;
            case "long": 명령.Description = new string('x', 61); break;
            case "control": 명령.Description = "중간\0제어문자"; break;
        }
        var 해시 = 원장.Snapshot().StateHashSha256;
        Assert.Contains(오류, 원장.Preview(명령).BlockReasonCodes);
        Assert.Throws<SimulationConflictException>(() => 원장.Confirm(명령));
        Assert.Equal(해시, 원장.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 같은내용의새명령과_개정범위초과를_거부한다()
    {
        var 원장 = new Simulation개인계획Aggregate(초기값()); 원장.Confirm(요청());
        Assert.Contains("PersonalPlanUnchanged", 원장.Preview(요청(1, "command:2", " 땔감을 모으자 ")).BlockReasonCodes);
        Assert.Throws<SimulationConflictException>(() => 원장.Confirm(요청(1, "command:2")));
        var 초기 = 초기값(); 초기.WorldRevision = long.MaxValue; 원장 = new Simulation개인계획Aggregate(초기);
        Assert.Contains("PersonalPlanRevisionExhausted", 원장.Preview(요청(long.MaxValue)).BlockReasonCodes);
        Assert.Throws<SimulationConflictException>(() => 원장.Confirm(요청(long.MaxValue)));
        Assert.Empty(원장.Snapshot().ActionLedger.TailRecords);
    }

    [Fact]
    public async Task 동시중복은_계획과안정근거를_한번만기록한다()
    {
        var 원장 = new Simulation개인계획Aggregate(초기값());
        var 결과 = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Task.Run(() => 원장.Confirm(요청()))));
        Assert.Single(결과, x => !x.Reused); Assert.Single(원장.Snapshot().ActionLedger.TailRecords);
    }

    [Fact]
    public void 정책사본과_목표순서에_결과가영향받지않는다()
    {
        var 초기 = 초기값(); var 원장 = new Simulation개인계획Aggregate(초기);
        초기.AllowedObjectiveStableIds[0] = "objective:tampered"; 초기.MaxDescriptionLength = 1;
        var 다른초기 = 초기값(); Array.Reverse(다른초기.AllowedObjectiveStableIds);
        var 다른원장 = new Simulation개인계획Aggregate(다른초기);
        Assert.Equal(원장.Confirm(요청()).State.StateHashSha256, 다른원장.Confirm(요청()).State.StateHashSha256);
        var 사본 = 원장.Snapshot(); 사본.Description = "오염";
        Assert.Equal("땔감을 모으자", 원장.Snapshot().Description);
    }

    [Fact]
    public void 정책중복과_잘못된문구상한을_거부한다()
    {
        var 초기 = 초기값(); 초기.AllowedObjectiveStableIds[1] = 초기.AllowedObjectiveStableIds[0];
        Assert.Throws<SimulationContractException>(() => new Simulation개인계획Aggregate(초기));
        초기 = 초기값(); 초기.MaxDescriptionLength = 0;
        Assert.Throws<SimulationContractException>(() => new Simulation개인계획Aggregate(초기));
    }

    [Fact]
    public void Application은_별도원장을분리하고_같은Domain규칙을호출한다()
    {
        var 서비스 = new Simulation개인계획Service(); 서비스.Create("one", 초기값()); 서비스.Create("two", 초기값());
        Assert.Throws<SimulationConflictException>(() => 서비스.Create("one", 초기값()));
        Assert.Throws<SimulationNotFoundException>(() => 서비스.Get("unknown"));
        Assert.True(서비스.Preview("one", 요청()).CanConfirm); 서비스.Confirm("one", 요청());
        Assert.Equal(1, 서비스.Get("one").WorldRevision); Assert.Equal(0, 서비스.Get("two").WorldRevision);
    }
}
