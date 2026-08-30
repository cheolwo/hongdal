using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "WI-FARM-DEFENSE-RESOLVE의 확정 결과 발현·Preview 무변경·멱등·거부·읽기 전용 투영을 검증한다.",
    Boundary = "Logic E3 자동시험이며 전투 계산·Save·RemoteHost·Unity·Game View 증거가 아니다.",
    WorldInteractionIds = new[] { SimulationFarm방위결과Codes.WorldInteractionId })]
public sealed class SimulationFarmDefenseResolutionTests
{
    [Fact]
    public void Preview는_확정결과를_읽지만_권위상태를_바꾸지_않는다()
    {
        var aggregate = Create(); var before = aggregate.Snapshot();
        var preview = aggregate.Preview(Preview()); var after = aggregate.Snapshot();
        Assert.True(preview.CanConfirm); Assert.Equal(4, preview.ThreatReductionUnits);
        Assert.Equal(180, preview.SafeUntilWorldTick); Assert.Equal(2, preview.LootLineCount);
        Assert.Equal(before.StateHashSha256, after.StateHashSha256); Assert.Empty(after.ActionLedger.TailRecords);
    }

    [Fact]
    public void Confirm은_위협_안전기간_보정_전리품을_원자적으로_발현한다()
    {
        var result = Create().Confirm(Confirm("command:defense-result:alpha"));
        Assert.False(result.Reused); Assert.Equal(52, result.Ledger.WorldRevision);
        Assert.Equal(5, result.Ledger.ThreatUnits); Assert.Equal(180, result.Ledger.SafeUntilWorldTick);
        Assert.Equal(1100, result.Ledger.ProductionModifierMilli); Assert.Equal(1150, result.Ledger.RecoveryModifierMilli);
        Assert.Equal(new[] { "item:bone", "item:hide" }, result.Ledger.LootInventory.Select(x => x.ItemStableId));
        Assert.Equal(new[] { 3, 2 }, result.Ledger.LootInventory.Select(x => x.Quantity));
        Assert.True(result.Ledger.Results.Single(x => x.EncounterStableId == "encounter:alpha").IsResolved);
        Assert.Equal("WorldDerived", result.ActionRecord.TriggerSourceCode);
        Assert.Equal("FarmDefenseResolved", result.ActionRecord.PrimaryOutcomeCode);
        Assert.Equal("encounter:alpha", result.ActionRecord.BattleOutcomeStableId);
    }

    [Fact]
    public void 같은_Command는_한번만_발현하고_다른_조우_payload는_거부한다()
    {
        var aggregate = Create(); var request = Confirm("command:defense-result:idem");
        var first = aggregate.Confirm(request); var second = aggregate.Confirm(request);
        Assert.True(second.Reused); Assert.Equal(first.Ledger.StateHashSha256, second.Ledger.StateHashSha256);
        Assert.Single(second.Ledger.ActionLedger.TailRecords); Assert.Equal(3, second.Ledger.LootInventory.Single(x => x.ItemStableId == "item:bone").Quantity);
        request.EncounterStableId = "encounter:bravo";
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request));
        Assert.Equal(SimulationFarm방위결과Codes.CommandPayloadConflict, error.ErrorCode);
    }

    [Theory]
    [InlineData(50, "encounter:alpha", SimulationFarm방위결과Codes.ExpectedRevisionMismatch)]
    [InlineData(51, "encounter:unknown", SimulationFarm방위결과Codes.EncounterUnknown)]
    public void revision과_알수없는_조우는_상태변경없이_거부한다(long revision, string encounter, string errorCode)
    {
        var aggregate = Create(); var before = aggregate.Snapshot();
        var request = Confirm("command:defense-result:blocked"); request.ExpectedWorldRevision = revision; request.EncounterStableId = encounter;
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request));
        Assert.Equal(errorCode, error.ErrorCode); Assert.Equal(before.StateHashSha256, aggregate.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 실패로_확정된_결과는_성공효과로_발현하지_않는다()
    {
        var aggregate = Create(); var request = Preview(); request.EncounterStableId = "encounter:failed";
        var preview = aggregate.Preview(request);
        Assert.False(preview.CanConfirm); Assert.Contains(SimulationFarm방위결과Codes.ResultNotSuccessful, preview.BlockReasonCodes);
        var confirm = Confirm("command:defense-result:failed"); confirm.EncounterStableId = "encounter:failed";
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(confirm));
        Assert.Equal(SimulationFarm방위결과Codes.ResultNotSuccessful, error.ErrorCode);
    }

    [Fact]
    public void 이미_발현한_조우는_새_Command로_다시_발현하지_않는다()
    {
        var aggregate = Create(); aggregate.Confirm(Confirm("command:defense-result:first"));
        var request = Confirm("command:defense-result:second"); request.ExpectedWorldRevision = 52;
        var error = Assert.Throws<SimulationConflictException>(() => aggregate.Confirm(request));
        Assert.Equal(SimulationFarm방위결과Codes.EncounterAlreadyResolved, error.ErrorCode);
        Assert.Equal(3, aggregate.Snapshot().LootInventory.Single(x => x.ItemStableId == "item:bone").Quantity);
    }

    [Fact]
    public void 여러_성공결과는_위협을_0에서_멈추고_안전기간과_전리품을_결정적으로_합친다()
    {
        var aggregate = Create(); aggregate.Confirm(Confirm("command:defense-result:alpha"));
        var second = Confirm("command:defense-result:bravo"); second.ExpectedWorldRevision = 52; second.EncounterStableId = "encounter:bravo";
        var result = aggregate.Confirm(second);
        Assert.Equal(0, result.Ledger.ThreatUnits); Assert.Equal(220, result.Ledger.SafeUntilWorldTick);
        Assert.Equal(4, result.Ledger.LootInventory.Single(x => x.ItemStableId == "item:bone").Quantity);
        Assert.Equal(2, result.Ledger.ActionLedger.TailRecords.Length);
    }

    [Fact]
    public void 카드투영은_조우순이며_읽기전용이다()
    {
        var service = new SimulationFarm방위결과Service(new InMemorySimulationFarm방위결과Store());
        service.Create("ledger:defense-result", Initial()); var before = service.Get("ledger:defense-result");
        var cards = service.ProjectCards("ledger:defense-result"); var after = service.Get("ledger:defense-result");
        Assert.Equal(new[] { "encounter:alpha", "encounter:bravo", "encounter:failed" }, cards.Select(x => x.EncounterStableId));
        Assert.All(cards, x => Assert.Equal(51, x.SourceWorldRevision));
        Assert.Equal(before.StateHashSha256, after.StateHashSha256);
    }

    [Fact]
    public void 중복_조우와_잘못된_전리품_수량은_초기계약에서_거부한다()
    {
        var duplicate = Initial(); duplicate.PendingResults[0].EncounterStableId = "encounter:alpha";
        Assert.Throws<SimulationContractException>(() => new SimulationFarm방위결과Aggregate(duplicate));
        var invalid = Initial(); invalid.PendingResults[0].Loot[0].Quantity = 0;
        Assert.Throws<SimulationContractException>(() => new SimulationFarm방위결과Aggregate(invalid));
    }

    private static SimulationFarm방위결과Aggregate Create() => new(Initial());
    private static SimulationFarm방위결과PreviewRequest Preview() => new() { ObservedWorldRevision = 51, EncounterStableId = "encounter:alpha" };
    private static SimulationFarm방위결과ConfirmRequest Confirm(string command) => new() { CommandId = command, ExpectedWorldRevision = 51, EncounterStableId = "encounter:alpha" };
    private static SimulationFarm방위결과InitialStateRequest Initial() => new()
    {
        WorldStableId = "world:farm", SessionStableId = "session:farm", InitialWorldRevision = 51,
        CurrentWorldTick = 100, ThreatUnits = 9, ProductionModifierMilli = 900, RecoveryModifierMilli = 950,
        PendingResults = new[]
        {
            new SimulationFarm방위확정결과Definition
            {
                EncounterStableId = "encounter:bravo", SquadStableId = "squad:bravo", DefenseSucceeded = true,
                ThreatReductionUnits = 8, SafeUntilWorldTick = 220, ProductionModifierMilli = 1200, RecoveryModifierMilli = 1250,
                Loot = new[] { new SimulationFarm방위전리품Definition { ItemStableId = "item:bone", Quantity = 1 } }
            },
            new SimulationFarm방위확정결과Definition
            {
                EncounterStableId = "encounter:alpha", SquadStableId = "squad:alpha", DefenseSucceeded = true,
                ThreatReductionUnits = 4, SafeUntilWorldTick = 180, ProductionModifierMilli = 1100, RecoveryModifierMilli = 1150,
                Loot = new[]
                {
                    new SimulationFarm방위전리품Definition { ItemStableId = "item:hide", Quantity = 2 },
                    new SimulationFarm방위전리품Definition { ItemStableId = "item:bone", Quantity = 3 }
                }
            },
            new SimulationFarm방위확정결과Definition
            {
                EncounterStableId = "encounter:failed", SquadStableId = "squad:failed", DefenseSucceeded = false,
                ThreatReductionUnits = 2, SafeUntilWorldTick = 140, ProductionModifierMilli = 1000, RecoveryModifierMilli = 1000
            }
        }
    };
}
