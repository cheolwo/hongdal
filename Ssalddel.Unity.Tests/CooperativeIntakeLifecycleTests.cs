using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

public sealed class CooperativeIntakeLifecycleTests
{
    private readonly CooperativeIntakeSimulationValidator validator = new();

    [Fact]
    public void COOP1은조합출하결정과300kg수확Lot에서시작한다()
    {
        var source = Snapshot();
        Assert.Equal(CooperativeIntakeStateCodes.AwaitingReview, source.StateCode);
        Assert.Equal(HarvestDispositionChoiceCodes.CooperativeShipment,
            source.DispositionDecision.ChoiceCode);
        Assert.Equal(300m, source.HarvestLot.Quantity);
        Assert.Null(source.IntakeLot);
        Assert.Null(source.CargoPreparationCandidate);
    }

    [Fact]
    public void COOP1_PreviewConfirm은조합인수를미리만들지않는다()
    {
        var source = Snapshot();
        var engine = Engine();
        var command = engine.Confirm(source, engine.Preview(source));
        Assert.Equal(source.DataRevision, command.ExpectedDataRevision);
        Assert.Null(source.IntakeLot);
        Assert.Null(source.CargoPreparationCandidate);
    }

    [Fact]
    public void COOP1_Tick은수량을보존한인수Lot과Cargo준비후보를만든다()
    {
        var result = Accept();
        Assert.Equal(CooperativeIntakeStateCodes.AcceptedForPreparation, result.StateCode);
        Assert.Equal(300m, result.IntakeLot!.Quantity);
        Assert.Equal(result.HarvestLot.StableId, result.IntakeLot.HarvestLotStableId);
        Assert.Equal(result.IntakeLot.StableId, result.CargoPreparationCandidate!.IntakeLotStableId);
        Assert.Equal("PotatoHarvestCargoLifecycle", result.CargoPreparationCandidate.NextWorkflowCode);
        Assert.Contains(result.DispositionDecision.StableId, result.IntakeLot.SourceStableIds);
    }

    [Fact]
    public void COOP1_직판결정은조합인수로열리지않는다()
    {
        var disposition = Disposition(HarvestDispositionChoiceCodes.DirectOnlineSale);
        Assert.Equal("CooperativeIntakeDispositionRequired",
            Assert.Throws<InvalidOperationException>(() => CooperativeIntakeSimulationFixture.Create(disposition)).Message);
    }

    [Fact]
    public void COOP1_StalePreview를거부한다()
    {
        var source = Snapshot();
        var preview = Engine().Preview(source);
        source.DataRevision++;
        Assert.Equal("CooperativeIntakePreviewStaleOrInvalid",
            Assert.Throws<InvalidOperationException>(() => Engine().Confirm(source, preview)).Message);
    }

    [Fact]
    public void COOP1_CargoAdapter는승인전연결을거부한다()
    {
        Assert.Equal("CooperativeCargoPreparationCandidateRequired",
            Assert.Throws<InvalidOperationException>(() =>
                new CooperativeHarvestCargoAdapter(validator).Create(Snapshot())).Message);
    }

    [Fact]
    public void COOP1_CargoAdapter는같은HarvestLot으로포장검토만연다()
    {
        var accepted = Accept();
        var cargo = new CooperativeHarvestCargoAdapter(validator).Create(accepted);
        Assert.Equal(accepted.HarvestLot.StableId, cargo.HarvestLot.StableId);
        Assert.Contains(accepted.IntakeLot!.StableId, cargo.SourceStableIds);
        Assert.Contains(accepted.CargoPreparationCandidate!.StableId, cargo.SourceStableIds);
        Assert.Null(cargo.PackageLot);
        Assert.Null(cargo.Cargo);
        Assert.True(new 감자수확CargoProjector(new 감자수확CargoSimulationValidator())
            .Project(cargo).CanPreviewPacking);
    }

    [Fact]
    public void COOP1_Card는후속후보와실행제한을표시한다()
    {
        var card = new CooperativeIntakeProjector(validator).Project(Accept());
        Assert.Contains("300kg", card.IntakeText);
        Assert.Contains("CANDIDATE ONLY", card.CandidateText);
        Assert.Contains("정산", card.LimitationText);
    }

    private CooperativeIntakeSimulationEngine Engine() => new(validator);

    private CooperativeIntakeSimulationSnapshot Accept()
    {
        var source = Snapshot();
        var engine = Engine();
        return engine.Tick(source, engine.Confirm(source, engine.Preview(source)));
    }

    private static CooperativeIntakeSimulationSnapshot Snapshot()
        => CooperativeIntakeSimulationFixture.Create(
            Disposition(HarvestDispositionChoiceCodes.CooperativeShipment));

    private static HarvestDispositionSimulationSnapshot Disposition(string choice)
    {
        var cultivationValidator = new 감자재배LifecycleSimulationValidator(
            new FarmSoilTileSimulationValidator(), new 재배달력ProfileValidator());
        var cultivation = new 감자재배LifecycleSimulationEngine(cultivationValidator);
        var farm = 감자재배LifecycleSimulationFixture.Create();
        var tile = farm.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        farm = cultivation.Tick(farm, cultivation.Confirm(farm, cultivation.PreviewSowing(farm, tile.StableId)));
        farm = cultivation.Tick(farm, cultivation.CreateAdvanceDaysCommand(farm, 6));
        farm = cultivation.Tick(farm, cultivation.Confirm(farm, cultivation.PreviewHarvest(farm)));
        var disposition = HarvestDispositionSimulationFixture.Create(farm);
        var engine = new HarvestDispositionSimulationEngine(new HarvestDispositionSimulationValidator());
        return engine.Tick(disposition, engine.Confirm(disposition, engine.Preview(disposition, choice)));
    }
}
