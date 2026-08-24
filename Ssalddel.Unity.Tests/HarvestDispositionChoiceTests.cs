using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class HarvestDispositionChoiceTests
{
    private readonly HarvestDispositionSimulationValidator validator = new();

    [Fact]
    public void HARVEST_CHOICE1_수확Lot에세판로가제공된다()
    {
        var snapshot = Snapshot();
        Assert.Equal(HarvestDispositionStateCodes.AwaitingChoice, snapshot.StateCode);
        Assert.Equal(4, snapshot.Options.Length);
        Assert.Contains(snapshot.Options, value => value.ChoiceCode == HarvestDispositionChoiceCodes.CooperativeShipment);
        Assert.Contains(snapshot.Options, value => value.ChoiceCode == HarvestDispositionChoiceCodes.DirectOnlineSale);
        Assert.Contains(snapshot.Options, value => value.ChoiceCode == HarvestDispositionChoiceCodes.ExportAgent);
        Assert.Contains(snapshot.Options, value => value.ChoiceCode == HarvestDispositionChoiceCodes.ReserveStorage);
    }

    [Theory]
    [InlineData(HarvestDispositionChoiceCodes.CooperativeShipment, "CooperativeIntakeCandidate")]
    [InlineData(HarvestDispositionChoiceCodes.DirectOnlineSale, "ProducerPackingCandidate")]
    [InlineData(HarvestDispositionChoiceCodes.ExportAgent, "ExportReadinessCandidate")]
    [InlineData(HarvestDispositionChoiceCodes.ReserveStorage, "ReserveStockLotCandidate")]
    public void HARVEST_CHOICE1_각선택은서로다른후속업무후보를만든다(string choice, string workflow)
    {
        var decided = Decide(choice);
        Assert.Equal(choice, decided.Decision!.ChoiceCode);
        Assert.Equal(workflow, decided.Decision.NextWorkflowCode);
        Assert.Equal(300m, decided.Decision.Quantity);
    }

    [Fact]
    public void HARVEST_CHOICE1_PreviewConfirm은결정을미리만들지않는다()
    {
        var source = Snapshot();
        var engine = Engine();
        var preview = engine.Preview(source, HarvestDispositionChoiceCodes.DirectOnlineSale);
        var command = engine.Confirm(source, preview);
        Assert.True(preview.RequiresExplicitConfirmation);
        Assert.Equal(source.DataRevision, command.ExpectedDataRevision);
        Assert.Null(source.Decision);
    }

    [Fact]
    public void HARVEST_CHOICE1_Tick뒤에는다른판로로조용히바꿀수없다()
    {
        var decided = Decide(HarvestDispositionChoiceCodes.CooperativeShipment);
        Assert.Equal("HarvestDispositionAlreadyDecided",
            Assert.Throws<InvalidOperationException>(() => Engine().Preview(
                decided, HarvestDispositionChoiceCodes.ExportAgent)).Message);
    }

    [Fact]
    public void HARVEST_CHOICE1_StalePreview와미지원선택을거부한다()
    {
        var source = Snapshot();
        var preview = Engine().Preview(source, HarvestDispositionChoiceCodes.DirectOnlineSale);
        source.DataRevision++;
        Assert.Equal("HarvestDispositionPreviewStaleOrInvalid",
            Assert.Throws<InvalidOperationException>(() => Engine().Confirm(source, preview)).Message);
        source = Snapshot();
        Assert.Equal("HarvestDispositionChoiceUnknown:WholesaleAuto",
            Assert.Throws<InvalidOperationException>(() => Engine().Preview(source, "WholesaleAuto")).Message);
    }

    [Fact]
    public void HARVEST_CHOICE1_카드는수확량선택과후속경계를표시한다()
    {
        var decided = Decide(HarvestDispositionChoiceCodes.ExportAgent);
        var card = new HarvestDispositionProjector(validator).Project(decided);
        Assert.Contains("300kg", card.HarvestText);
        Assert.Contains("ExportAgent", card.DecisionText);
        Assert.Contains("ExportReadinessCandidate", card.DecisionText);
        Assert.Contains(card.Options, value => value.Limitations[0].Contains("통관"));
    }

    private HarvestDispositionSimulationEngine Engine() => new(validator);

    private HarvestDispositionSimulationSnapshot Decide(string choice)
    {
        var source = Snapshot();
        var engine = Engine();
        return engine.Tick(source, engine.Confirm(source, engine.Preview(source, choice)));
    }

    private static HarvestDispositionSimulationSnapshot Snapshot()
        => HarvestDispositionSimulationFixture.Create(Harvested());

    private static 감자재배LifecycleSimulationSnapshot Harvested()
    {
        var validator = new 감자재배LifecycleSimulationValidator(
            new FarmSoilTileSimulationValidator(), new 재배달력ProfileValidator());
        var engine = new 감자재배LifecycleSimulationEngine(validator);
        var source = 감자재배LifecycleSimulationFixture.Create();
        var tile = source.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        source = engine.Tick(source, engine.Confirm(source, engine.PreviewSowing(source, tile.StableId)));
        source = engine.Tick(source, engine.CreateAdvanceDaysCommand(source, 6));
        return engine.Tick(source, engine.Confirm(source, engine.PreviewHarvest(source)));
    }
}
