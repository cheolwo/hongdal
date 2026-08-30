using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3, "WI-FARM-DEFENSE-RETURN의 Preview·Confirm·멱등·거부·후속 대기열 인계를 검증한다.", Boundary = "Logic·Presentation E3 자동시험이며 치료·생산 재합류·Save·RemoteHost·Game View 증거가 아니다.", WorldInteractionIds = new[] { SimulationFarm방위귀환Codes.WorldInteractionId })]
public sealed class SimulationFarmDefenseReturnTests
{
    [Fact]
    public void Preview는_귀환인계를_읽지만_상태를_바꾸지_않는다()
    {
        var aggregate = Create(); var before = aggregate.Snapshot(); var preview = aggregate.Preview(Preview()); var after = aggregate.Snapshot();
        Assert.True(preview.CanConfirm); Assert.Equal(1, preview.TreatmentRequiredCount); Assert.Equal(2, preview.ProductionRejoinCandidateCount);
        Assert.Equal(before.StateHashSha256, after.StateHashSha256); Assert.Empty(after.ActionLedger.TailRecords);
    }
    [Fact]
    public void Confirm은_분대를_초소로_귀환시키고_후속대기열만_만든다()
    {
        var result = Create().Confirm(Confirm("command:return:alpha"));
        Assert.Equal(62, result.Ledger.WorldRevision); Assert.True(result.Ledger.Returns.Single(x => x.ReturnStableId == "return:alpha").IsReturned);
        Assert.Equal(new[] { "actor:injured" }, result.Ledger.PendingTreatmentActorStableIds);
        Assert.Equal(new[] { "actor:worker-a", "actor:worker-b" }, result.Ledger.PendingProductionRejoinActorStableIds);
        Assert.Equal("FarmDefenseSquadReturned", result.ActionRecord.PrimaryOutcomeCode); Assert.Equal(Simulation행위결과분류Codes.후퇴복구, result.ActionRecord.결과분류Code);
    }
    [Fact]
    public void 같은_Command는_재사용하고_다른_귀환_payload는_거부한다()
    {
        var aggregate = Create(); var request = Confirm("command:return:idem"); var first = aggregate.Confirm(request); var second = aggregate.Confirm(request);
        Assert.True(second.Reused); Assert.Equal(first.Ledger.StateHashSha256, second.Ledger.StateHashSha256); Assert.Single(second.Ledger.ActionLedger.TailRecords);
        request.ReturnStableId = "return:bravo"; var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request));
        Assert.Equal(SimulationFarm방위귀환Codes.CommandPayloadConflict, error.ErrorCode);
    }
    [Theory]
    [InlineData(60, "return:alpha", SimulationFarm방위귀환Codes.ExpectedRevisionMismatch)]
    [InlineData(61, "return:unknown", SimulationFarm방위귀환Codes.ReturnUnknown)]
    public void revision과_알수없는_귀환은_무변경_거부한다(long revision, string id, string code)
    {
        var aggregate = Create(); var before = aggregate.Snapshot(); var request = Confirm("command:return:blocked"); request.ExpectedWorldRevision = revision; request.ReturnStableId = id;
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request)); Assert.Equal(code, error.ErrorCode); Assert.Equal(before.StateHashSha256, aggregate.Snapshot().StateHashSha256);
    }
    [Fact]
    public void 결과가_확정되지_않은_분대는_귀환완료로_만들지_않는다()
    {
        var aggregate = Create(); var request = Preview(); request.ReturnStableId = "return:unresolved";
        Assert.Contains(SimulationFarm방위귀환Codes.ResultNotResolved, aggregate.Preview(request).BlockReasonCodes);
    }
    [Fact]
    public void 이미_귀환한_분대는_새_Command로_다시_인계하지_않는다()
    {
        var aggregate = Create(); aggregate.Confirm(Confirm("command:return:first")); var request = Confirm("command:return:second"); request.ExpectedWorldRevision = 62;
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request)); Assert.Equal(SimulationFarm방위귀환Codes.AlreadyReturned, error.ErrorCode);
    }
    [Fact]
    public void 카드투영은_귀환StableId순이며_읽기전용이다()
    {
        var service = new SimulationFarm방위귀환Service(new InMemorySimulationFarm방위귀환Store()); service.Create("ledger:return", Initial()); var before = service.Get("ledger:return");
        var cards = service.ProjectCards("ledger:return"); var after = service.Get("ledger:return");
        Assert.Equal(new[] { "return:alpha", "return:bravo", "return:unresolved" }, cards.Select(x => x.ReturnStableId)); Assert.All(cards, x => Assert.Equal(61, x.SourceWorldRevision)); Assert.Equal(before.StateHashSha256, after.StateHashSha256);
    }
    [Fact]
    public void 중복_귀환과_치료_재합류_역할중첩은_초기계약에서_거부한다()
    {
        var duplicate = Initial(); duplicate.Returns[0].ReturnStableId = "return:alpha"; Assert.Throws<SimulationContractException>(() => new SimulationFarm방위귀환Aggregate(duplicate));
        var overlap = Initial(); overlap.Returns[1].ProductionRejoinCandidateActorStableIds = new[] { "actor:injured" }; Assert.Throws<SimulationContractException>(() => new SimulationFarm방위귀환Aggregate(overlap));
    }
    private static SimulationFarm방위귀환Aggregate Create() => new(Initial());
    private static SimulationFarm방위귀환PreviewRequest Preview() => new() { ObservedWorldRevision = 61, ReturnStableId = "return:alpha" };
    private static SimulationFarm방위귀환ConfirmRequest Confirm(string command) => new() { CommandId = command, ExpectedWorldRevision = 61, ReturnStableId = "return:alpha" };
    private static SimulationFarm방위귀환InitialStateRequest Initial() => new()
    {
        WorldStableId = "world:farm", SessionStableId = "session:farm", InitialWorldRevision = 61,
        Returns = new[]
        {
            new SimulationFarm방위귀환Definition { ReturnStableId = "return:bravo", EncounterStableId = "encounter:bravo", SquadStableId = "squad:bravo", OutpostStableId = "outpost:east", ResultResolved = true },
            new SimulationFarm방위귀환Definition { ReturnStableId = "return:alpha", EncounterStableId = "encounter:alpha", SquadStableId = "squad:alpha", OutpostStableId = "outpost:north", ResultResolved = true, TreatmentRequiredActorStableIds = new[] { "actor:injured" }, ProductionRejoinCandidateActorStableIds = new[] { "actor:worker-b", "actor:worker-a" } },
            new SimulationFarm방위귀환Definition { ReturnStableId = "return:unresolved", EncounterStableId = "encounter:failed", SquadStableId = "squad:failed", OutpostStableId = "outpost:north", ResultResolved = false }
        }
    };
}
