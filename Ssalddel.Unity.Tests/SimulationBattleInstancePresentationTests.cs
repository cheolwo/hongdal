using Ssalddel.Unity.Battles;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationBattleInstancePresentationTests
{
    [Fact]
    public void 지휘자는배치에서삼인칭_활성전투에서일인칭을기본으로쓴다()
    {
        var deploying = State(BattlePresentationCodes.Deploying);
        var deployment = new BattlePresentationMapper().Map(deploying, "actor:commander");
        deploying.PhaseCode = BattlePresentationCodes.Active;
        var active = new BattlePresentationMapper().Map(deploying, "actor:commander");

        Assert.Equal(BattlePresentationCodes.TacticalThirdPerson,
            deployment.DefaultViewModeCode);
        Assert.Equal(BattlePresentationCodes.FirstPerson, active.DefaultViewModeCode);
        Assert.True(active.ShowBattleRoot);
        Assert.False(active.ShowManagementRoot);
        Assert.True(active.CanControlBattle);
        Assert.False(active.ChangesWorldState);
    }

    [Fact]
    public void 비참가팀원은경영을계속하고_제한된지원명령초안만만든다()
    {
        var source = State(BattlePresentationCodes.Active);
        var frame = new BattlePresentationMapper().Map(source, "actor:manager");
        var command = BattleSupportCommandFactory.Create(frame,
            "command:support:one", 12, source.BattleRevision,
            "actor:manager", "SupplyCrate", "item-stack:supply");

        Assert.Equal(BattlePresentationCodes.Management, frame.LocalModeCode);
        Assert.True(frame.ShowManagementRoot);
        Assert.True(frame.ShowBattleStatusCard);
        Assert.True(frame.CanSendManagementSupport);
        Assert.Equal("item-stack:supply", command.SourceResourceStableId);
        Assert.Null(typeof(BattleSupportCommandDraft).GetProperty("StrengthBonus"));
        Assert.Null(typeof(BattleSupportCommandDraft).GetProperty("OutcomeCode"));
    }

    [Fact]
    public void 관전자는전투Root를보지만전투와경영상태를조작하지않는다()
    {
        var source = State(BattlePresentationCodes.Active);
        source.Participants = source.Participants.Concat(
        [new BattleParticipantApiModel
        {
            ActorStableId = "actor:spectator",
            ParticipationRoleCode = BattlePresentationCodes.Spectator,
            PresentationOnly = true,
        }]).ToArray();

        var frame = new BattlePresentationMapper().Map(source, "actor:spectator");

        Assert.Equal(BattlePresentationCodes.BattleObservation, frame.LocalModeCode);
        Assert.Equal(BattlePresentationCodes.Follow, frame.DefaultViewModeCode);
        Assert.True(frame.ShowBattleRoot);
        Assert.False(frame.CanControlBattle);
        Assert.False(frame.CanSendManagementSupport);
    }

    [Fact]
    public void 합류된결과는경영표현으로돌아가고_서버결과를그대로보여준다()
    {
        var source = State(BattlePresentationCodes.Reconciled);
        source.Outcome = new BattleOutcomeApiModel
        {
            ResultCode = "Victory",
            RecoverableInjuryCount = 2,
            FacilityDamageUnits = 1,
            SupplyLossUnits = 3,
            ReconciliationStateCode = "Applied",
            AppliedWorldTick = 8,
        };

        var frame = new BattlePresentationMapper().Map(source, "actor:commander");

        Assert.Equal(BattlePresentationCodes.Management, frame.LocalModeCode);
        Assert.True(frame.ShowManagementRoot);
        Assert.True(frame.Outcome!.AppliedToManagementWorld);
        Assert.Equal(2, frame.Outcome.RecoverableInjuryCount);
        Assert.False(frame.ChangesWorldState);
    }

    private static BattleInstanceApiModel State(string phase) => new()
    {
        BattleStableId = "battle:one",
        SessionStableId = "simulation-session:one",
        EncounterStableId = "encounter:one",
        AreaStableId = "area:daegwallyeong-farm",
        PhaseCode = phase,
        BattleRevision = 3,
        CombatTick = 120,
        Participants = [new BattleParticipantApiModel
        {
            ActorStableId = "actor:commander",
            ParticipationRoleCode = BattlePresentationCodes.Commander,
            CanControlWorldState = true,
        }],
        ReplayHashSha256 = new string('a', 64),
        SimulationOnly = true,
    };
}
