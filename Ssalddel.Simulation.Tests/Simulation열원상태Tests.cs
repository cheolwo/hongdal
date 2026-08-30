using System.Globalization;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "열원 상태 변경의 단일 책임·멱등·거부·행위 기록을 검증한다.",
    Boundary = "독립 Core 집중시험. Runtime·Save·원격·화면 증거가 아니다.",
    WorldInteractionIds = new[] { Simulation열원상태Codes.WorldInteractionId })]
public sealed class Simulation열원상태Tests
{
    private static Simulation열원InitialState 초기값() => new()
    {
        WorldStableId = "world:test", SessionStableId = "session:test", PlayerStableId = "player:one",
        HeatSourceStableId = "heat:camp", PolicyRevision = "fixture:heat.r1", Capacity = 100,
        Accessible = true, HasBasicSurvivalAbility = true,
        Fuels = new[] { new Simulation연료Definition { FuelStableId = "fuel:dry", UnitEnergy = 10, Quantity = 10 },
            new Simulation연료Definition { FuelStableId = "fuel:timber", UnitEnergy = 20, Quantity = 5 } },
    };
    private static Simulation열원ConfirmRequest 요청(string 작업 = "Ignite", long 개정 = 0, string 명령 = "command:1") => new()
    {
        ActorStableId = "player:one", HeatSourceStableId = "heat:camp", OperationCode = 작업,
        ExpectedRevision = 개정, CommandId = 명령, FuelStableId = 작업 == "Extinguish" ? "" : "fuel:dry",
        Quantity = 작업 == "Extinguish" ? 0 : 1,
    };

    [Fact]
    public void 조회와Preview는_연료_열원_행위원장을_변경하지않는다()
    {
        var 원장 = new Simulation열원상태Aggregate(초기값());
        var 이전 = 원장.Snapshot();
        var 결과 = 원장.Preview(요청());
        Assert.True(결과.CanConfirm);
        Assert.Equal("Burning", 결과.ProjectedStatusCode);
        Assert.Equal(10, 결과.ProjectedEnergy);
        Assert.Equal(이전.StateHashSha256, 원장.Snapshot().StateHashSha256);
        Assert.Empty(원장.Snapshot().ActionLedger.TailRecords);
    }

    [Theory]
    [InlineData("Off", 0)]
    [InlineData("Smoldering", 3)]
    public void 점화_연료추가_소화는_각각한번만_발현되고_소화는환불하지않는다(string 상태, long 에너지)
    {
        var 초기 = 초기값(); 초기.StatusCode = 상태; 초기.Energy = 에너지;
        var 원장 = new Simulation열원상태Aggregate(초기);
        var 점화 = 원장.Confirm(요청()).Ledger;
        Assert.Equal(에너지 + 10, 점화.State.Energy);
        var 추가 = 요청("AddFuel", 1, "command:2"); 추가.FuelStableId = "fuel:timber";
        var 추가결과 = 원장.Confirm(추가).Ledger;
        Assert.Equal(에너지 + 30, 추가결과.State.Energy);
        var 종료 = 원장.Confirm(요청("Extinguish", 2, "command:3")).Ledger;
        Assert.Equal("Off", 종료.State.StatusCode);
        Assert.Equal(0, 종료.State.Energy);
        Assert.Equal(9, 종료.State.Fuels.Single(x => x.FuelStableId == "fuel:dry").Quantity);
        Assert.Equal(4, 종료.State.Fuels.Single(x => x.FuelStableId == "fuel:timber").Quantity);
        Assert.Equal(3, 종료.State.WorldRevision);
        Assert.Equal(3, 종료.ActionLedger.TailRecords.Length);
        for (var i = 0; i < 3; i++)
        {
            var 행위 = 종료.ActionLedger.TailRecords[i];
            Assert.Equal(i, 행위.BeforeWorldRevision); Assert.Equal(i + 1, 행위.AfterWorldRevision);
            Assert.Equal(Simulation열원상태Codes.WorldInteractionId, 행위.WorldInteractionId);
            Assert.Equal("HeatSourceStateChanged", 행위.PrimaryOutcomeCode);
            Assert.Equal("player:one", 행위.InitiatorStableId);
            Assert.Contains("fixture:heat.r1", 행위.RuleRevision);
        }
    }

    [Theory]
    [InlineData("actor", "HeatActorNotAuthorized")]
    [InlineData("target", "HeatSourceUnknown")]
    [InlineData("revision", "HeatExpectedRevisionMismatch")]
    [InlineData("operation", "HeatOperationInvalid")]
    [InlineData("fuel", "HeatFuelNotApproved")]
    [InlineData("zero", "HeatFuelQuantityInvalid")]
    [InlineData("negative", "HeatFuelQuantityInvalid")]
    [InlineData("stock", "HeatFuelInsufficient")]
    [InlineData("capacity", "HeatCapacityExceeded")]
    [InlineData("refuel-off", "HeatIgnitionRequired")]
    [InlineData("extinguish-off", "HeatAlreadyOff")]
    public void 거부는_아무것도_소비하거나_기록하지않는다(string 경우, string 오류)
    {
        var 원장 = new Simulation열원상태Aggregate(초기값()); var 명령 = 요청();
        switch (경우)
        {
            case "actor": 명령.ActorStableId = "npc:spoof"; break;
            case "target": 명령.HeatSourceStableId = "heat:other"; break;
            case "revision": 명령.ExpectedRevision = 1; break;
            case "operation": 명령.OperationCode = "AutomaticSleepIgnition"; break;
            case "fuel": 명령.FuelStableId = "fuel:wet"; break;
            case "zero": 명령.Quantity = 0; break;
            case "negative": 명령.Quantity = -1; break;
            case "stock": 명령.Quantity = 11; break;
            case "capacity": 명령.FuelStableId = "fuel:timber"; 명령.Quantity = 5; 원장.Confirm(요청(명령: "before")); 명령.ExpectedRevision = 1; 명령.OperationCode = "AddFuel"; break;
            case "refuel-off": 명령.OperationCode = "AddFuel"; break;
            case "extinguish-off": 명령 = 요청("Extinguish"); break;
        }
        var 이전 = 원장.Snapshot().StateHashSha256;
        Assert.Contains(오류, 원장.Preview(명령).BlockReasonCodes);
        Assert.Throws<SimulationConflictException>(() => 원장.Confirm(명령));
        Assert.Equal(이전, 원장.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 접근권한과기초생존능력은_신뢰된초기화에서만_받는다()
    {
        var 초기 = 초기값(); 초기.Accessible = false; 초기.HasBasicSurvivalAbility = false;
        var 원장 = new Simulation열원상태Aggregate(초기);
        Assert.Contains("HeatSourceInaccessible", 원장.Preview(요청()).BlockReasonCodes);
        Assert.Contains("HeatBasicSurvivalRequired", 원장.Preview(요청()).BlockReasonCodes);
        초기.Accessible = true; 초기.HasBasicSurvivalAbility = true;
        Assert.False(원장.Preview(요청()).CanConfirm);
    }

    [Fact]
    public void 멱등재요청은_후속상태가아닌_최초결과를_독립사본으로_반환한다()
    {
        var 원장 = new Simulation열원상태Aggregate(초기값());
        var 최초 = 원장.Confirm(요청()); var 해시 = 최초.Ledger.StateHashSha256;
        최초.Ledger.State.Energy = 999;
        최초.Ledger.ActionLedger.TailRecords[0].ActorStableId = "tampered";
        원장.Confirm(요청("AddFuel", 1, "command:2"));
        var 재요청 = 원장.Confirm(요청());
        Assert.True(재요청.Reused); Assert.Equal(해시, 재요청.Ledger.StateHashSha256);
        Assert.Equal(10, 재요청.Ledger.State.Energy); Assert.Single(재요청.Ledger.ActionLedger.TailRecords);
        Assert.Equal("player:one", 재요청.Ledger.ActionLedger.TailRecords[0].ActorStableId);
        재요청.Ledger.State.Fuels[0].Quantity = 999;
        Assert.NotEqual(999, 원장.Confirm(요청()).Ledger.State.Fuels[0].Quantity);
        Assert.Equal(2, 원장.Snapshot().State.WorldRevision);
        var 충돌 = 요청(); 충돌.Quantity = 2;
        Assert.Throws<SimulationConflictException>(() => 원장.Confirm(충돌));
    }

    [Fact]
    public async Task 동시중복Confirm은_단한번_소비한다()
    {
        var 원장 = new Simulation열원상태Aggregate(초기값());
        var 결과 = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Task.Run(() => 원장.Confirm(요청()))));
        Assert.Single(결과, x => !x.Reused); Assert.Single(원장.Snapshot().ActionLedger.TailRecords);
        Assert.Equal(1, 원장.Snapshot().State.WorldRevision);
    }

    [Fact]
    public async Task 동일개정의_서로다른Confirm은_하나만성공한다()
    {
        var 원장 = new Simulation열원상태Aggregate(초기값());
        var 결과 = await Task.WhenAll(Enumerable.Range(0, 8).Select(i => Task.Run(() =>
        {
            try { 원장.Confirm(요청(명령: "command:" + i)); return true; }
            catch (SimulationConflictException) { return false; }
        })));
        Assert.Single(결과, x => x); Assert.Single(원장.Snapshot().ActionLedger.TailRecords);
    }

    [Fact]
    public void 연료용량과개정의_정수범위초과를_변경전에_거부한다()
    {
        var 초기 = 초기값(); 초기.Capacity = long.MaxValue;
        초기.Fuels[0].Quantity = long.MaxValue; 초기.Fuels[0].UnitEnergy = long.MaxValue;
        var 원장 = new Simulation열원상태Aggregate(초기); var 명령 = 요청(); 명령.Quantity = 2;
        Assert.Contains("HeatCapacityExceeded", 원장.Preview(명령).BlockReasonCodes);
        Assert.Throws<SimulationConflictException>(() => 원장.Confirm(명령));
        Assert.Equal(0, 원장.Snapshot().State.WorldRevision);
        초기.WorldRevision = long.MaxValue; 원장 = new Simulation열원상태Aggregate(초기);
        명령.Quantity = 1; 명령.ExpectedRevision = long.MaxValue;
        Assert.Contains("HeatRevisionExhausted", 원장.Preview(명령).BlockReasonCodes);
    }

    [Fact]
    public void 소화에숨긴연료소비와_이중점화를_거부한다()
    {
        var 원장 = new Simulation열원상태Aggregate(초기값()); 원장.Confirm(요청());
        Assert.Contains("HeatAlreadyBurning", 원장.Preview(요청(개정: 1)).BlockReasonCodes);
        var 명령 = 요청("Extinguish", 1, "command:2"); 명령.Quantity = 1;
        Assert.Contains("HeatExtinguishPayloadInvalid", 원장.Preview(명령).BlockReasonCodes);
        Assert.Throws<SimulationConflictException>(() => 원장.Confirm(명령));
        Assert.Equal(1, 원장.Snapshot().State.WorldRevision);
    }

    [Fact]
    public void 정책과조회사본의수정은_권위에_역류하지않고_정렬과문화권에독립적이다()
    {
        var 초기 = 초기값(); var 원장 = new Simulation열원상태Aggregate(초기);
        var 다른초기 = 초기값(); Array.Reverse(다른초기.Fuels);
        var 다른원장 = new Simulation열원상태Aggregate(다른초기);
        초기.Fuels[0].UnitEnergy = 999; 원장.Snapshot().State.Capacity = 999;
        var 문화권 = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var 결과 = 원장.Confirm(요청()).Ledger;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal(결과.StateHashSha256, 다른원장.Confirm(요청()).Ledger.StateHashSha256);
        }
        finally { CultureInfo.CurrentCulture = 문화권; }
    }

    [Fact]
    public void 잘못된초기정책과중복연료를_거부한다()
    {
        var 초기 = 초기값(); 초기.Fuels[1].FuelStableId = 초기.Fuels[0].FuelStableId;
        Assert.Throws<SimulationContractException>(() => new Simulation열원상태Aggregate(초기));
        초기 = 초기값(); 초기.Fuels[0].UnitEnergy = 0;
        Assert.Throws<SimulationContractException>(() => new Simulation열원상태Aggregate(초기));
        초기 = 초기값(); 초기.Energy = 1;
        Assert.Throws<SimulationContractException>(() => new Simulation열원상태Aggregate(초기));
    }

    [Fact]
    public void Application은_동일규칙을호출하고_원장을분리한다()
    {
        var 서비스 = new Simulation열원상태Service(new InMemorySimulation열원상태Store());
        서비스.Create("one", 초기값()); 서비스.Create("two", 초기값());
        Assert.Throws<SimulationConflictException>(() => 서비스.Create("one", 초기값()));
        Assert.Throws<SimulationNotFoundException>(() => 서비스.Get("missing"));
        Assert.True(서비스.Preview("one", 요청()).CanConfirm);
        서비스.Confirm("one", 요청());
        Assert.Equal(1, 서비스.Get("one").State.WorldRevision);
        Assert.Equal(0, 서비스.Get("two").State.WorldRevision);
    }
}
