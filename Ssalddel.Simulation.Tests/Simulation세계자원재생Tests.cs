using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3, "자원 재생 정책·자동 Tick·멱등·원자성·거부·순서 독립성을 검증한다.",
    Boundary = "신뢰 Fixture Core 시험. 실제 지형·생산 수치·Save·Runtime·화면은 미검증.",
    WorldInteractionIds = new[] { Simulation세계자원재생Codes.WorldInteractionId })]
public sealed class Simulation세계자원재생Tests
{
    private static Simulation세계자원재생InitialState 초기값() => new()
    {
        WorldStableId = "world:test", SessionStableId = "session:test", 권위주체StableId = "rule:world", WorldSeed = 17,
        Profiles = new[] { new Simulation자원재생Profile { ProfileStableId = "profile:tree", Revision = "fixture.r1", 자원StableId = "resource:wood",
            성숙단계 = 2, 단계간Tick = 3, 생성확률Micro = 1000000, Tick당생성상한 = 1 } },
        Cells = new[] { 셀("cell:stump", 1234), 셀("cell:empty-a", 5000), 셀("cell:empty-b", -5000) },
        Nodes = new[] { new Simulation자원재생Node { NodeStableId = "node:original", CellStableId = "cell:stump", 성장단계 = 0, 다음성장Tick = 3 } },
    };
    private static Simulation자원재생Cell 셀(string id, long x = 0) => new()
    { CellStableId = id, ProfileStableId = "profile:tree", 중심Xmm = x, 중심Zmm = -987 };
    private static Simulation자원재생TickRequest 요청(long tick = 1, long? revision = null, string? id = null) => new()
    { WorldTick = tick, ExpectedRevision = revision ?? tick - 1, TransitionId = id ?? "transition:" + tick, 권위주체StableId = "rule:world" };

    [Fact]
    public void 진단은_새노드와기록을_만들지않는다()
    {
        var 원장 = new Simulation세계자원재생Aggregate(초기값()); var 이전 = 원장.Snapshot();
        Assert.Single(원장.PreviewTick(요청()).변경노드StableIds);
        Assert.Equal(이전.StateHashSha256, 원장.Snapshot().StateHashSha256);
        Assert.Single(원장.Snapshot().Nodes); Assert.Empty(원장.Snapshot().ActionLedger.TailRecords);
    }

    [Fact]
    public void 기존나무는_Tick경계에서_동일위치와ID로_단계별재성장한다()
    {
        var 초기 = 초기값(); 초기.Profiles[0].생성확률Micro = 0;
        var 원장 = new Simulation세계자원재생Aggregate(초기);
        for (var tick = 1; tick <= 6; tick++)
        {
            var 사본 = 원장.ApplyTick(요청(tick)).State; var 나무 = Assert.Single(사본.Nodes);
            Assert.Equal("node:original", 나무.NodeStableId); Assert.Equal(1234, 나무.Xmm); Assert.Equal(-987, 나무.Zmm);
            Assert.Equal(tick / 3, 나무.성장단계); Assert.Equal(tick == 6, 나무.채집가능);
            Assert.Equal(tick, 사본.WorldRevision); Assert.Equal(tick, 사본.WorldTick);
        }
        var 기록 = 원장.Snapshot().ActionLedger.TailRecords;
        Assert.Equal(6, 기록.Length);
        Assert.Equal(2, 기록.Count(r => r.PrimaryOutcomeCode == "ResourceAvailabilityRestored"));
        Assert.All(기록, r => { Assert.Equal("WorldDerived", r.TriggerSourceCode); Assert.Equal("WorldSystem", r.ActorKindCode);
            Assert.Equal(Simulation세계자원재생Codes.WorldInteractionId, r.WorldInteractionId); Assert.Equal(r.AfterWorldRevision, r.AppliedWorldTick); });
    }

    [Fact]
    public void 새식물은_빈셀에만_상한만큼생기며_즉시채집할수없다()
    {
        var 원장 = new Simulation세계자원재생Aggregate(초기값());
        var 첫째 = 원장.ApplyTick(요청()).State;
        Assert.Equal(2, 첫째.Nodes.Length); Assert.All(첫째.Nodes, n => Assert.False(n.채집가능));
        var 둘째 = 원장.ApplyTick(요청(2)).State; Assert.Equal(3, 둘째.Nodes.Length);
        Assert.Equal(3, 둘째.Nodes.Select(n => n.CellStableId).Distinct().Count());
        for (var t = 3; t <= 9; t++) 원장.ApplyTick(요청(t));
        Assert.Equal(3, 원장.Snapshot().Nodes.Length); Assert.All(원장.Snapshot().Nodes, n => Assert.True(n.채집가능));
    }

    [Fact]
    public void 환경묶음은_해당Profile로_생성될때_가용하다()
    {
        var 초기 = 초기값(); 초기.Nodes = Array.Empty<Simulation자원재생Node>();
        초기.Profiles[0].종류Code = Simulation세계자원재생Codes.환경묶음; 초기.Profiles[0].자원StableId = "resource:dry-grass";
        var 사본 = new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State;
        var 묶음 = Assert.Single(사본.Nodes); Assert.True(묶음.채집가능); Assert.Equal(0, 묶음.다음성장Tick);
        Assert.Equal("resource:dry-grass", 묶음.자원StableId); Assert.Equal("Loose", 묶음.종류Code);
    }

    [Fact]
    public void 세계파생재생은_행위원장과_사유있는분야성장미적용을_같이반환한다()
    {
        var 원장 = new Simulation세계자원재생Aggregate(초기값());
        var 요청값 = 요청();

        var 최초 = 원장.ApplyTick(요청값);
        var 기록 = Assert.Single(최초.State.ActionLedger.TailRecords);

        Assert.Equal(요청값.TransitionId, 기록.CommandId);
        Assert.Equal(최초.State.WorldRevision, 기록.AfterWorldRevision);
        Assert.Equal(Simulation분야성장적용상태Codes.NotApplicable,
            최초.분야성장적용.상태Code);
        Assert.Equal(Simulation세계자원재생Codes.PlayerProgressionNotApplicableReason,
            최초.분야성장적용.사유Code);
        Assert.Equal(string.Empty, 최초.분야성장적용.PlayerStableId);
        Assert.Equal(0, 최초.분야성장적용.BeforeProfileRevision);
        Assert.Equal(0, 최초.분야성장적용.AfterProfileRevision);

        var 재전송 = 원장.ApplyTick(요청값);

        Assert.True(재전송.Reused);
        Assert.Equal(기록.기록HashSha256,
            Assert.Single(재전송.State.ActionLedger.TailRecords).기록HashSha256);
        Assert.Equal(Simulation분야성장적용상태Codes.NotApplicable,
            재전송.분야성장적용.상태Code);
        Assert.Equal(Simulation세계자원재생Codes.PlayerProgressionNotApplicableReason,
            재전송.분야성장적용.사유Code);
    }

    [Theory]
    [InlineData("Natural", true)]
    [InlineData("Flattened", false)]
    [InlineData("Construction", false)]
    [InlineData("Road", false)]
    public void 토지용도는_재성장과신규생성을_함께제한한다(string 용도, bool 허용)
    {
        var 초기 = 초기값(); 초기.Profiles[0].성숙단계 = 1; 초기.Nodes[0].다음성장Tick = 1;
        foreach (var c in 초기.Cells) c.토지용도Code = 용도;
        var 결과 = new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State;
        Assert.Equal(허용 ? 2 : 1, 결과.Nodes.Length);
        Assert.Equal(허용 ? 1 : 0, 결과.Nodes.Single(n => n.NodeStableId == "node:original").성장단계);
    }

    [Theory]
    [InlineData("authority", "ResourceRegenerationAuthorityRequired")]
    [InlineData("revision", "ResourceRegenerationExpectedRevisionMismatch")]
    [InlineData("past", "ResourceRegenerationNextTickRequired")]
    [InlineData("skip", "ResourceRegenerationNextTickRequired")]
    [InlineData("blank", "ResourceRegenerationTransitionIdInvalid")]
    public void 잘못된권위호출은_전체상태를_보존한다(string 경우, string 차단)
    {
        var 원장 = new Simulation세계자원재생Aggregate(초기값()); var 명령 = 요청();
        switch (경우) { case "authority": 명령.권위주체StableId = "player:fake"; break; case "revision": 명령.ExpectedRevision = 3; break;
            case "past": 명령.WorldTick = 0; break; case "skip": 명령.WorldTick = 2; break; case "blank": 명령.TransitionId = " "; break; }
        var 해시 = 원장.Snapshot().StateHashSha256;
        Assert.Contains(차단, 원장.PreviewTick(명령).BlockReasonCodes);
        if (경우 == "blank") Assert.Throws<SimulationContractException>(() => 원장.ApplyTick(명령));
        else Assert.Throws<SimulationConflictException>(() => 원장.ApplyTick(명령));
        Assert.Equal(해시, 원장.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 재전송은_최초결과를_반환하고_내용충돌과_다른ID중복Tick은_거부한다()
    {
        var 원장 = new Simulation세계자원재생Aggregate(초기값()); var 최초 = 원장.ApplyTick(요청()).State;
        var 해시 = 최초.StateHashSha256; 최초.Nodes[0].NodeStableId = "오염"; 최초.ActionLedger.TailRecords[0].ActorStableId = "오염";
        원장.ApplyTick(요청(2)); var 재전송 = 원장.ApplyTick(요청());
        Assert.True(재전송.Reused); Assert.Equal(해시, 재전송.State.StateHashSha256);
        Assert.DoesNotContain(재전송.State.Nodes, n => n.NodeStableId == "오염");
        Assert.Equal("rule:world", Assert.Single(재전송.State.ActionLedger.TailRecords).ActorStableId);
        Assert.Throws<SimulationConflictException>(() => 원장.ApplyTick(요청(3, 2, "transition:1")));
        Assert.Throws<SimulationConflictException>(() => 원장.ApplyTick(요청(1, 2, "another")));
        Assert.Equal(2, 원장.Snapshot().WorldRevision);
    }

    [Fact]
    public async Task 동시중복전이는_한번만확정된다()
    {
        var 원장 = new Simulation세계자원재생Aggregate(초기값());
        var 결과 = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Task.Run(() => 원장.ApplyTick(요청()))));
        Assert.Single(결과, r => !r.Reused); Assert.Single(원장.Snapshot().ActionLedger.TailRecords);
        Assert.Equal(2, 원장.Snapshot().Nodes.Length);
    }

    [Fact]
    public async Task 다른전이ID의_동시같은Tick도_한번만확정된다()
    {
        var 원장 = new Simulation세계자원재생Aggregate(초기값());
        var 결과 = await Task.WhenAll(new[] { "one", "two" }.Select(id => Task.Run(() =>
        {
            try { 원장.ApplyTick(요청(id: id)); return true; }
            catch (SimulationConflictException) { return false; }
        })));
        Assert.Single(결과, x => x); Assert.Single(원장.Snapshot().ActionLedger.TailRecords);
        Assert.Equal(1, 원장.Snapshot().WorldRevision);
    }

    [Fact]
    public void 순서와호출자사본변경은_결과에영향을주지않는다()
    {
        var 초기 = 초기값(); var 원장 = new Simulation세계자원재생Aggregate(초기);
        초기.Profiles[0].생성확률Micro = 0; 초기.Cells[0].중심Xmm = 100; 초기.Nodes[0].성장단계 = 2;
        var 역순 = 초기값(); Array.Reverse(역순.Cells); Array.Reverse(역순.Nodes);
        var 다른원장 = new Simulation세계자원재생Aggregate(역순);
        for (var t = 1; t <= 7; t++) Assert.Equal(원장.ApplyTick(요청(t)).State.StateHashSha256, 다른원장.ApplyTick(요청(t)).State.StateHashSha256);
        var 사본 = 원장.Snapshot(); 사본.Nodes[0].성장단계 = -10;
        Assert.All(원장.Snapshot().Nodes, n => Assert.True(n.성장단계 >= 0));
    }

    [Fact]
    public void Seed와생성확률과상한은_신뢰정책에따라_결정된다()
    {
        var 초기 = 초기값(); 초기.Nodes = Array.Empty<Simulation자원재생Node>();
        초기.Cells = Enumerable.Range(0, 100).Select(i => 셀("cell:" + i)).ToArray();
        초기.Profiles[0].Tick당생성상한 = 100; 초기.Profiles[0].생성확률Micro = 250000;
        var 첫째 = new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State;
        var 재실행 = new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State;
        Assert.Equal(첫째.StateHashSha256, 재실행.StateHashSha256); Assert.InRange(첫째.Nodes.Length, 1, 99);
        초기.WorldSeed++;
        var 다른Seed = new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State;
        Assert.NotEqual(첫째.StateHashSha256, 다른Seed.StateHashSha256);
        Assert.False(첫째.Nodes.Select(n => n.CellStableId).SequenceEqual(다른Seed.Nodes.Select(n => n.CellStableId)));
        초기.Profiles[0].생성확률Micro = 0;
        var 생성없음 = new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State;
        Assert.Empty(생성없음.Nodes); Assert.Equal("ResourceAvailabilityUnchanged", Assert.Single(생성없음.ActionLedger.TailRecords).PrimaryOutcomeCode);
        초기.Profiles[0].생성확률Micro = 1000000; 초기.Profiles[0].Tick당생성상한 = 0;
        Assert.Empty(new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State.Nodes);
    }

    [Fact]
    public void 뒤쪽후보의_성장시각넘침도_앞쪽변경을_확정하지않는다()
    {
        var 초기 = 초기값(); 초기.WorldTick = int.MaxValue - 1; 초기.Profiles[0].단계간Tick = 2; 초기.Nodes[0].다음성장Tick = 1;
        초기.Nodes[0].성장단계 = 1; // 이 노드는 먼저 정상 성숙하지만 신규 식물의 다음 성장 Tick은 넘친다.
        var 원장 = new Simulation세계자원재생Aggregate(초기); var 해시 = 원장.Snapshot().StateHashSha256;
        var 명령 = 요청(int.MaxValue, 0);
        Assert.Contains("ResourceRegenerationTickOverflow", 원장.PreviewTick(명령).BlockReasonCodes);
        Assert.Throws<SimulationConflictException>(() => 원장.ApplyTick(명령));
        Assert.Equal(해시, 원장.Snapshot().StateHashSha256); Assert.Empty(원장.Snapshot().ActionLedger.TailRecords);
    }

    [Theory]
    [InlineData("revision")]
    [InlineData("tick")]
    public void 권위정수상한에서_부분변경이없다(string 경우)
    {
        var 초기 = 초기값(); if (경우 == "revision") 초기.WorldRevision = long.MaxValue; else 초기.WorldTick = int.MaxValue;
        var 원장 = new Simulation세계자원재생Aggregate(초기); var 해시 = 원장.Snapshot().StateHashSha256;
        Assert.False(원장.PreviewTick(요청(1, 초기.WorldRevision)).CanApply);
        Assert.Throws<SimulationConflictException>(() => 원장.ApplyTick(요청(1, 초기.WorldRevision)));
        Assert.Equal(해시, 원장.Snapshot().StateHashSha256);
    }

    [Theory]
    [InlineData("period")]
    [InlineData("period-range")]
    [InlineData("deadline-range")]
    [InlineData("chance")]
    [InlineData("stage")]
    [InlineData("duplicate-profile")]
    [InlineData("duplicate-cell")]
    [InlineData("duplicate-node")]
    [InlineData("occupied")]
    [InlineData("unknown-profile")]
    [InlineData("unknown-landuse")]
    public void 모호하거나잘못된초기정책을_거부한다(string 경우)
    {
        var 초기 = 초기값();
        switch (경우)
        {
            case "period": 초기.Profiles[0].단계간Tick = 0; break;
            case "period-range": 초기.Profiles[0].단계간Tick = (long)int.MaxValue + 1; break;
            case "deadline-range": 초기.Nodes[0].다음성장Tick = (long)int.MaxValue + 1; break;
            case "chance": 초기.Profiles[0].생성확률Micro = 1000001; break;
            case "stage": 초기.Nodes[0].성장단계 = 3; break;
            case "duplicate-profile": 초기.Profiles = new[] { 초기.Profiles[0], 초기.Profiles[0] }; break;
            case "duplicate-cell": 초기.Cells[1] = 초기.Cells[0]; break;
            case "duplicate-node": 초기.Nodes = new[] { 초기.Nodes[0], 초기.Nodes[0] }; break;
            case "occupied": 초기.Nodes = new[] { 초기.Nodes[0], new Simulation자원재생Node { NodeStableId = "other", CellStableId = "cell:stump" } }; break;
            case "unknown-profile": 초기.Cells[0].ProfileStableId = "unknown"; break;
            case "unknown-landuse": 초기.Cells[0].토지용도Code = "unknown"; break;
        }
        Assert.Throws<SimulationContractException>(() => new Simulation세계자원재생Aggregate(초기));
    }

    [Fact]
    public void Application은_원장을격리하고_플레이어Confirm을_노출하지않는다()
    {
        var 서비스 = new Simulation세계자원재생Service(); 서비스.Create("one", 초기값()); 서비스.Create("two", 초기값());
        Assert.Throws<SimulationConflictException>(() => 서비스.Create("one", 초기값()));
        Assert.Throws<SimulationNotFoundException>(() => 서비스.Get("missing"));
        Assert.True(서비스.PreviewTick("one", 요청()).CanApply); 서비스.ApplyTick("one", 요청());
        Assert.Equal(1, 서비스.Get("one").WorldRevision); Assert.Equal(0, 서비스.Get("two").WorldRevision);
        Assert.Null(typeof(Simulation세계자원재생Service).GetMethod("Confirm"));
    }

    [Fact]
    public void 생성ID충돌은_기존노드나_시계를덮어쓰지않는다()
    {
        var 기준 = 초기값();
        var 예정 = new Simulation세계자원재생Aggregate(기준).PreviewTick(요청()).변경노드StableIds.Single();
        기준.Nodes[0].NodeStableId = 예정;
        var 원장 = new Simulation세계자원재생Aggregate(기준); var 이전 = 원장.Snapshot().StateHashSha256;
        Assert.Contains("ResourceRegenerationNodeCollision", 원장.PreviewTick(요청()).BlockReasonCodes);
        Assert.Throws<SimulationConflictException>(() => 원장.ApplyTick(요청()));
        Assert.Equal(이전, 원장.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 자원별상한과종류는_서로침범하지않고_Profile정렬에도_독립적이다()
    {
        var 초기 = 초기값();
        초기.Profiles = new[] { 초기.Profiles[0], new Simulation자원재생Profile { ProfileStableId = "profile:loose", Revision = "fixture.r1",
            자원StableId = "resource:grass", 종류Code = "Loose", 성숙단계 = 1, 단계간Tick = 5, 생성확률Micro = 1000000, Tick당생성상한 = 1 } };
        초기.Cells = 초기.Cells.Concat(new[] { new Simulation자원재생Cell { CellStableId = "cell:loose", ProfileStableId = "profile:loose" } }).ToArray();
        var 첫째 = new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State;
        Array.Reverse(초기.Profiles); Array.Reverse(초기.Cells);
        var 둘째 = new Simulation세계자원재생Aggregate(초기).ApplyTick(요청()).State;
        Assert.Equal(첫째.StateHashSha256, 둘째.StateHashSha256);
        Assert.Equal(3, 첫째.Nodes.Length); Assert.Single(첫째.Nodes, n => n.종류Code == "Loose" && n.채집가능);
        Assert.Equal(2, 첫째.Nodes.Count(n => n.종류Code == "Plant" && !n.채집가능));
    }
}
