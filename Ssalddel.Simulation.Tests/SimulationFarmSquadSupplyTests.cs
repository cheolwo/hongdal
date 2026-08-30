using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "WI-SQUAD-SUPPLY의 Preview·Confirm·멱등·거부·결정적 보급 카드를 검증한다.",
    Boundary = "Logic·Presentation E3 자동시험이며 Save·RemoteHost·실제 보급 UI·Game View 증거가 아니다.",
    WorldInteractionIds = new[] { SimulationFarm분대보급Codes.WorldInteractionId })]
public sealed class SimulationFarmSquadSupplyTests
{
    [Fact]
    public void Preview는_필요량을_읽지만_권위상태를_바꾸지_않는다()
    {
        var aggregate = Create(); var before = aggregate.Snapshot();
        var preview = aggregate.Preview(Preview()); var after = aggregate.Snapshot();
        Assert.True(preview.CanConfirm); Assert.Equal(3, preview.RequiredFoodUnits); Assert.Equal(2, preview.RequiredDurabilityRestoreUnits);
        Assert.Equal(before.WorldRevision, after.WorldRevision); Assert.Equal(before.StateHashSha256, after.StateHashSha256); Assert.Empty(after.ActionLedger.TailRecords);
    }

    [Fact]
    public void Confirm은_식량과_내구도복구능력을_동시에_소비하고_분대를_준비시킨다()
    {
        var result = Create().Confirm(Confirm("command:supply:alpha"));
        Assert.False(result.Reused); Assert.Equal(42, result.Ledger.WorldRevision);
        Assert.Equal(7, result.Ledger.FoodStockUnits); Assert.Equal(3, result.Ledger.DurabilityRestoreCapacityUnits);
        Assert.True(result.Ledger.Squads.Single(x => x.SquadStableId == "squad:alpha").IsSupplied);
        Assert.Equal("FarmDefenseSquadSupplied", result.ActionRecord.PrimaryOutcomeCode);
        Assert.Equal("PlayerDriven", result.ActionRecord.TriggerSourceCode);
        Assert.Equal(new[] { Simulation행위변화의미Codes.Actor상태변경, Simulation행위변화의미Codes.재고변경 }, result.ActionRecord.변화의미Codes);
    }

    [Fact]
    public void 같은_Command는_한번만_소비하고_다른_분대_payload는_거부한다()
    {
        var aggregate = Create(); var request = Confirm("command:supply:idem");
        var first = aggregate.Confirm(request); var second = aggregate.Confirm(request);
        Assert.True(second.Reused); Assert.Equal(first.Ledger.StateHashSha256, second.Ledger.StateHashSha256);
        Assert.Equal(7, second.Ledger.FoodStockUnits); Assert.Single(second.Ledger.ActionLedger.TailRecords);
        request.SquadStableId = "squad:bravo";
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request));
        Assert.Equal(SimulationFarm분대보급Codes.CommandPayloadConflict, error.ErrorCode);
    }

    [Theory]
    [InlineData(40, "squad:alpha", SimulationFarm분대보급Codes.ExpectedRevisionMismatch)]
    [InlineData(41, "squad:unknown", SimulationFarm분대보급Codes.SquadUnknown)]
    public void revision과_분대_거부는_상태를_바꾸지_않는다(long revision, string squad, string errorCode)
    {
        var aggregate = Create(); var before = aggregate.Snapshot();
        var request = Confirm("command:supply:blocked"); request.ExpectedWorldRevision = revision; request.SquadStableId = squad;
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request));
        Assert.Equal(errorCode, error.ErrorCode); Assert.Equal(before.StateHashSha256, aggregate.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 식량과_내구도복구능력_부족은_서로_구분된다()
    {
        var food = Initial(); food.FoodStockUnits = 2;
        Assert.Contains(SimulationFarm분대보급Codes.FoodInsufficient,
            new SimulationFarm분대보급Aggregate(food).Preview(Preview()).BlockReasonCodes);
        var durability = Initial(); durability.DurabilityRestoreCapacityUnits = 1;
        Assert.Contains(SimulationFarm분대보급Codes.DurabilityRestoreInsufficient,
            new SimulationFarm분대보급Aggregate(durability).Preview(Preview()).BlockReasonCodes);
    }

    [Fact]
    public void 이미_보급된_분대는_다시_소비하지_않는다()
    {
        var aggregate = Create(); aggregate.Confirm(Confirm("command:supply:first"));
        var preview = Preview(); preview.ObservedWorldRevision = 42;
        Assert.Contains(SimulationFarm분대보급Codes.SquadAlreadySupplied, aggregate.Preview(preview).BlockReasonCodes);
        Assert.Equal(7, aggregate.Snapshot().FoodStockUnits);
    }

    [Fact]
    public void 카드투영은_분대_StableId순이며_읽기전용이다()
    {
        var service = new SimulationFarm분대보급Service(new InMemorySimulationFarm분대보급Store());
        service.Create("ledger:supply", Initial()); var before = service.Get("ledger:supply");
        var cards = service.ProjectCards("ledger:supply"); var after = service.Get("ledger:supply");
        Assert.Equal(new[] { "squad:alpha", "squad:bravo" }, cards.Select(x => x.SquadStableId));
        Assert.All(cards, x => Assert.Equal(41, x.SourceWorldRevision)); Assert.All(cards, x => Assert.False(x.IsSupplied));
        Assert.Equal(before.StateHashSha256, after.StateHashSha256);
    }

    [Fact]
    public void 초기_분대_중복과_음수_자원은_거부한다()
    {
        var duplicate = Initial(); duplicate.SquadRequirements[0].SquadStableId = "squad:alpha";
        Assert.Throws<SimulationContractException>(() => new SimulationFarm분대보급Aggregate(duplicate));
        var negative = Initial(); negative.FoodStockUnits = -1;
        Assert.Throws<SimulationContractException>(() => new SimulationFarm분대보급Aggregate(negative));
    }

    private static SimulationFarm분대보급Aggregate Create() => new(Initial());
    private static SimulationFarm분대보급InitialStateRequest Initial() => new()
    {
        WorldStableId = "world:farm", SessionStableId = "session:farm", InitialWorldRevision = 41,
        FoodStockUnits = 10, DurabilityRestoreCapacityUnits = 5,
        SquadRequirements = new[]
        {
            new SimulationFarm분대보급Requirement { SquadStableId = "squad:bravo", RequiredFoodUnits = 2, RequiredDurabilityRestoreUnits = 1 },
            new SimulationFarm분대보급Requirement { SquadStableId = "squad:alpha", RequiredFoodUnits = 3, RequiredDurabilityRestoreUnits = 2 }
        }
    };
    private static SimulationFarm분대보급PreviewRequest Preview() => new() { ObservedWorldRevision = 41, SquadStableId = "squad:alpha" };
    private static SimulationFarm분대보급ConfirmRequest Confirm(string command) => new() { CommandId = command, ExpectedWorldRevision = 41, SquadStableId = "squad:alpha" };
}
