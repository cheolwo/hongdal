using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "D319 자연 회복 순수 계산의 분할·활동 전이·거부·무변경 경계를 시험한다.",
    Boundary = "합성 활동/시간 입력 시험이며 실제 Host·Session·Save·Farm 두 주기 실행 증거가 아니다.",
    WorldInteractionIds = new[] { "WI-FARM-01", "WI-FARM-02", "WI-FARM-03", "WI-FARM-04" })]
public sealed class Simulation행동체력자연회복Tests
{
    private static Simulation행동체력회복Cursor Cursor(Simulation행동체력활동 활동 = Simulation행동체력활동.대기) => new()
    { SessionStableId = "session:farm", ActorStableId = "actor:player", 활동 = 활동 };

    private static Simulation행동체력회복구간 구간(Simulation행동체력회복Cursor 현재, long 경과 = 1000) => new()
    {
        SessionStableId = 현재.SessionStableId, ActorStableId = 현재.ActorStableId,
        시작Millis = 현재.정산시각Millis, 종료Millis = 현재.정산시각Millis + 경과,
        Expected활동Revision = 현재.활동Revision
    };

    [Theory]
    [InlineData(Simulation행동체력활동.대기)]
    [InlineData(Simulation행동체력활동.걷기)]
    public void 허용활동_60초는_작업1회분15_회복(Simulation행동체력활동 활동)
    {
        var 현재 = Cursor(활동);
        var 결과 = Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 구간(현재, 60000));
        Assert.Equal(55m, 결과.다음체력);
        Assert.Equal(15m, 결과.회복량);
        Assert.Equal("Recovering", 결과.회복상태Code);
    }

    [Theory]
    [InlineData(Simulation행동체력활동.노동)]
    [InlineData(Simulation행동체력활동.질주)]
    [InlineData(Simulation행동체력활동.전투)]
    [InlineData(Simulation행동체력활동.대기 | Simulation행동체력활동.전투)]
    [InlineData(Simulation행동체력활동.걷기 | Simulation행동체력활동.전투)]
    [InlineData(Simulation행동체력활동.대기 | Simulation행동체력활동.노동)]
    [InlineData(Simulation행동체력활동.걷기 | Simulation행동체력활동.질주)]
    [InlineData(Simulation행동체력활동.노동 | Simulation행동체력활동.질주 | Simulation행동체력활동.전투)]
    public void 하나라도_금지활동이면_회복0_시간만소비(Simulation행동체력활동 활동)
    {
        var 현재 = Cursor(활동); 현재.회복잔여분자 = 713;
        var 결과 = Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 구간(현재, 60000));
        Assert.Equal(40m, 결과.다음체력);
        Assert.Equal("BlockedByActivity", 결과.회복상태Code);
        Assert.Equal(60000, 결과.다음Cursor.정산시각Millis);
        Assert.Equal(713, 결과.다음Cursor.회복잔여분자);
    }

    [Fact]
    public void 모르는_활동을_대기로_간주하지않음()
    {
        var 현재 = Cursor(Simulation행동체력활동.미확인);
        var 결과 = Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 구간(현재, 60000));
        Assert.Equal(0m, 결과.회복량);
        Assert.Equal("ActivityUnverified", 결과.회복상태Code);
    }

    [Fact]
    public void 입력객체와_원본체력은_변경하지않고_독립사본을_반환()
    {
        var 현재 = Cursor(); var 요청 = 구간(현재);
        var 첫째 = Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 요청);
        첫째.다음Cursor.ActorStableId = "tampered";
        var 둘째 = Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 요청);
        Assert.Equal(40.25m, 둘째.다음체력);
        Assert.Equal(0, 현재.정산시각Millis);
        Assert.Equal(0, 현재.활동Revision);
        Assert.Equal("actor:player", 둘째.다음Cursor.ActorStableId);
        Assert.Equal(1000, 요청.종료Millis);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(333)]
    [InlineData(1000)]
    [InlineData(5000)]
    [InlineData(60000)]
    public void 같은_60초는_분할에_관계없이_소수와잔여가_일치(long 단위)
    {
        var 현재 = Cursor(); 현재.회복잔여분자 = 321;
        var 한번 = Simulation행동체력자연회복Calculator.Prepare(40.000001m, 현재, 구간(현재, 60000));
        decimal 체력 = 40.000001m;
        while (현재.정산시각Millis < 60000)
        {
            var 결과 = Simulation행동체력자연회복Calculator.Prepare(체력, 현재,
                구간(현재, Math.Min(단위, 60000 - 현재.정산시각Millis)));
            체력 = 결과.다음체력; 현재 = 결과.다음Cursor;
        }
        Assert.Equal(한번.다음체력, 체력);
        Assert.Equal(한번.다음Cursor.회복잔여분자, 현재.회복잔여분자);
        Assert.Equal(한번.다음Cursor.정산시각Millis, 현재.정산시각Millis);
    }

    [Fact]
    public void 활동전이는_이전구간_정산후_적용하고_금지시간을_소급하지않음()
    {
        var 현재 = Cursor();
        var 시작 = 구간(현재, 20000);
        시작.끝에서활동변경 = true; 시작.다음활동Revision = 1;
        시작.다음활동 = Simulation행동체력활동.노동 | Simulation행동체력활동.전투;
        var 결과 = Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 시작);
        Assert.Equal(45m, 결과.다음체력); 현재 = 결과.다음Cursor;
        var 일부해제 = 구간(현재, 30000);
        일부해제.끝에서활동변경 = true; 일부해제.다음활동Revision = 2;
        일부해제.다음활동 = Simulation행동체력활동.대기 | Simulation행동체력활동.전투;
        결과 = Simulation행동체력자연회복Calculator.Prepare(결과.다음체력, 현재, 일부해제);
        Assert.Equal(45m, 결과.다음체력); 현재 = 결과.다음Cursor;
        var 모두해제 = 구간(현재, 10000);
        모두해제.끝에서활동변경 = true; 모두해제.다음활동Revision = 3; 모두해제.다음활동 = Simulation행동체력활동.걷기;
        결과 = Simulation행동체력자연회복Calculator.Prepare(결과.다음체력, 현재, 모두해제);
        Assert.Equal(45m, 결과.다음체력); 현재 = 결과.다음Cursor;
        결과 = Simulation행동체력자연회복Calculator.Prepare(결과.다음체력, 현재, 구간(현재, 1));
        Assert.Equal(45.00025m, 결과.다음체력); // 재개 지연0, 전환 자체 보상0
    }

    [Fact]
    public void 경과0_활동전환반복은_회복0()
    {
        var 현재 = Cursor();
        for (int i = 0; i < 100; i++)
        {
            var 요청 = 구간(현재, 0); 요청.끝에서활동변경 = true;
            요청.다음활동Revision = 현재.활동Revision + 1;
            요청.다음활동 = i % 2 == 0 ? Simulation행동체력활동.노동 : Simulation행동체력활동.대기;
            var 결과 = Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 요청);
            Assert.Equal(0m, 결과.회복량); 현재 = 결과.다음Cursor;
        }
        Assert.Equal(0, 현재.정산시각Millis);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(99)]
    [InlineData(99.9999)]
    public void 최대치_초과분과잔여를_이월하지않음(decimal 체력)
    {
        var 현재 = Cursor(); 현재.회복잔여분자 = 999;
        var 결과 = Simulation행동체력자연회복Calculator.Prepare(체력, 현재, 구간(현재, 60000));
        Assert.Equal(100m, 결과.다음체력);
        Assert.Equal(0, 결과.다음Cursor.회복잔여분자);
        Assert.Equal("Full", 결과.회복상태Code);
        var 비용후 = Simulation행동체력자연회복Calculator.Prepare(85m, 결과.다음Cursor, 구간(결과.다음Cursor, 0));
        Assert.Equal(85m, 비용후.다음체력);
    }

    [Fact]
    public void 이미_정산한_구간의_중복과_누락시간은_거부()
    {
        var 현재 = Cursor(); var 요청 = 구간(현재);
        var 결과 = Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 요청);
        Assert.Throws<SimulationConflictException>(() => Simulation행동체력자연회복Calculator.Prepare(결과.다음체력, 결과.다음Cursor, 요청));
        요청.시작Millis = 2000; 요청.종료Millis = 3000;
        Assert.Throws<SimulationConflictException>(() => Simulation행동체력자연회복Calculator.Prepare(결과.다음체력, 결과.다음Cursor, 요청));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(60001)]
    [InlineData(long.MaxValue)]
    public void 시간_범위초과를_거부(long 경과)
    {
        var 현재 = Cursor();
        Assert.Throws<SimulationContractException>(() => Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 구간(현재, 경과)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void 기존_체력상한을_임의_변경하지않음(decimal 체력)
    {
        var 현재 = Cursor();
        Assert.Throws<SimulationContractException>(() => Simulation행동체력자연회복Calculator.Prepare(체력, 현재, 구간(현재)));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void 신원_판본_손상을_거부(int 종류)
    {
        var 현재 = Cursor(); var 요청 = 구간(현재);
        if (종류 == 0) 요청.SessionStableId = "other";
        if (종류 == 1) 요청.ActorStableId = "other";
        if (종류 == 2) 요청.Expected활동Revision = 1;
        if (종류 == 3) { 요청.끝에서활동변경 = true; 요청.다음활동Revision = 2; }
        if (종류 == 4) { 현재.활동Revision = long.MaxValue; 요청.Expected활동Revision = long.MaxValue; 요청.끝에서활동변경 = true; 요청.다음활동Revision = long.MinValue; }
        Assert.Throws<SimulationConflictException>(() => Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 요청));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void 잘못된_Cursor나_모호한활동은_거부(int 종류)
    {
        var 현재 = Cursor(); var 요청 = 구간(현재);
        if (종류 == 0) 현재.RuleRevision = "future";
        if (종류 == 1) 현재.회복잔여분자 = 1000;
        if (종류 == 2) 현재.회복잔여분자 = -1;
        if (종류 == 3) 현재.활동 = (Simulation행동체력활동)32;
        if (종류 == 4) 현재.활동 = Simulation행동체력활동.대기 | Simulation행동체력활동.걷기;
        if (종류 == 5) 현재.활동Revision = -1;
        if (종류 == 6) 요청.다음활동 = Simulation행동체력활동.노동;
        if (종류 == 7) 현재.SessionStableId = " ";
        Assert.Throws<SimulationContractException>(() => Simulation행동체력자연회복Calculator.Prepare(40m, 현재, 요청));
    }
}
