using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "WI-SQUAD-ASSIGN의 Preview·Confirm·멱등·거부·결정적 초소 슬롯 카드를 검증한다.",
    Boundary = "Logic·Presentation E3 자동시험이며 Save·RemoteHost·실제 초소 UI·Game View 증거가 아니다.",
    WorldInteractionIds = new[] { SimulationFarm분대배정Codes.WorldInteractionId })]
public sealed class SimulationFarmSquadAssignmentTests
{
    [Fact]
    public void Preview는_권위상태와_revision을_바꾸지_않는다()
    {
        var aggregate = Create(); var before = aggregate.Snapshot();
        var preview = aggregate.Preview(Preview()); var after = aggregate.Snapshot();
        Assert.True(preview.CanConfirm); Assert.Equal(before.WorldRevision, after.WorldRevision);
        Assert.Equal(before.StateHashSha256, after.StateHashSha256); Assert.Empty(after.Assignments); Assert.Empty(after.ActionLedger.TailRecords);
    }

    [Fact]
    public void Confirm은_초소_슬롯에_분대를_한번_배정한다()
    {
        var result = Create().Confirm(Confirm("command:squad:assign"));
        Assert.False(result.Reused); Assert.Equal(42, result.Ledger.WorldRevision);
        var assignment = Assert.Single(result.Ledger.Assignments);
        Assert.Equal("outpost:ridge", assignment.OutpostStableId); Assert.Equal("slot:a", assignment.SlotStableId); Assert.Equal("squad:alpha", assignment.SquadStableId);
        Assert.Equal(SimulationFarm분대배정Codes.WorldInteractionId, result.ActionRecord.WorldInteractionId);
        Assert.Equal("PlayerDriven", result.ActionRecord.TriggerSourceCode); Assert.Equal(41, result.ActionRecord.BeforeWorldRevision); Assert.Equal(42, result.ActionRecord.AfterWorldRevision);
    }

    [Fact]
    public void 같은_Command는_결과를_재사용하고_payload_변경은_거부한다()
    {
        var aggregate = Create(); var request = Confirm("command:squad:idem"); var first = aggregate.Confirm(request); var second = aggregate.Confirm(request);
        Assert.True(second.Reused); Assert.Equal(first.Ledger.StateHashSha256, second.Ledger.StateHashSha256); Assert.Single(second.Ledger.ActionLedger.TailRecords);
        request.SlotStableId = "slot:b"; var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request));
        Assert.Equal(SimulationFarm분대배정Codes.CommandPayloadConflict, error.ErrorCode);
    }

    [Theory]
    [InlineData(40, "outpost:ridge", "slot:a", "squad:alpha", SimulationFarm분대배정Codes.ExpectedRevisionMismatch)]
    [InlineData(41, "outpost:unknown", "slot:a", "squad:alpha", SimulationFarm분대배정Codes.OutpostUnknown)]
    [InlineData(41, "outpost:ridge", "slot:unknown", "squad:alpha", SimulationFarm분대배정Codes.SlotUnknown)]
    [InlineData(41, "outpost:ridge", "slot:a", "squad:unknown", SimulationFarm분대배정Codes.SquadUnknown)]
    public void 거부경계는_상태를_바꾸지_않는다(long revision, string outpost, string slot, string squad, string errorCode)
    {
        var aggregate = Create(); var before = aggregate.Snapshot(); var request = Confirm("command:squad:blocked");
        request.ExpectedWorldRevision = revision; request.OutpostStableId = outpost; request.SlotStableId = slot; request.SquadStableId = squad;
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request));
        Assert.Equal(errorCode, error.ErrorCode); Assert.Equal(before.StateHashSha256, aggregate.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 이미_배정된_분대와_점유된_슬롯은_각각_거부한다()
    {
        var aggregate = Create(); aggregate.Confirm(Confirm("command:squad:first"));
        var sameSquad = Preview(); sameSquad.ObservedWorldRevision = 42; sameSquad.SlotStableId = "slot:b";
        Assert.Contains(SimulationFarm분대배정Codes.SquadAlreadyAssigned, aggregate.Preview(sameSquad).BlockReasonCodes);
        var occupied = Preview(); occupied.ObservedWorldRevision = 42; occupied.SquadStableId = "squad:bravo";
        Assert.Contains(SimulationFarm분대배정Codes.SlotOccupied, aggregate.Preview(occupied).BlockReasonCodes);
    }

    [Fact]
    public void 카드투영은_빈_슬롯과_배정_슬롯을_StableId순으로_읽는다()
    {
        var service = new SimulationFarm분대배정Service(new InMemorySimulationFarm분대배정Store()); service.Create("ledger:squad", Initial());
        var before = service.Get("ledger:squad"); var empty = service.ProjectCards("ledger:squad"); var afterEmpty = service.Get("ledger:squad");
        Assert.Equal(new[] { "slot:a", "slot:b" }, empty.Select(x => x.SlotStableId)); Assert.All(empty, x => Assert.False(x.IsOccupied));
        service.Confirm("ledger:squad", Confirm("command:squad:card")); var cards = service.ProjectCards("ledger:squad");
        Assert.True(cards[0].IsOccupied); Assert.Equal("squad:alpha", cards[0].SquadStableId); Assert.False(cards[1].IsOccupied);
        Assert.Equal(before.StateHashSha256, afterEmpty.StateHashSha256); Assert.All(cards, x => Assert.Equal(42, x.SourceWorldRevision));
    }

    [Fact]
    public void 초기_분대와_슬롯_중복은_거부한다()
    {
        var request = Initial(); request.SquadStableIds = new[] { "squad:alpha", "squad:alpha" };
        Assert.Throws<SimulationContractException>(() => new SimulationFarm분대배정Aggregate(request));
        request = Initial(); request.Outposts[0].SlotStableIds = new[] { "slot:a", "slot:a" };
        Assert.Throws<SimulationContractException>(() => new SimulationFarm분대배정Aggregate(request));
    }

    private static SimulationFarm분대배정Aggregate Create() => new(Initial());
    private static SimulationFarm분대배정InitialStateRequest Initial() => new() {
        WorldStableId = "world:farm", SessionStableId = "session:farm", InitialWorldRevision = 41,
        SquadStableIds = new[] { "squad:bravo", "squad:alpha" },
        Outposts = new[] { new SimulationFarm경비초소Definition { OutpostStableId = "outpost:ridge", SlotStableIds = new[] { "slot:b", "slot:a" } } } };
    private static SimulationFarm분대배정PreviewRequest Preview() => new() { ObservedWorldRevision = 41, OutpostStableId = "outpost:ridge", SlotStableId = "slot:a", SquadStableId = "squad:alpha" };
    private static SimulationFarm분대배정ConfirmRequest Confirm(string command) => new() { CommandId = command, ExpectedWorldRevision = 41, OutpostStableId = "outpost:ridge", SlotStableId = "slot:a", SquadStableId = "squad:alpha" };
}
