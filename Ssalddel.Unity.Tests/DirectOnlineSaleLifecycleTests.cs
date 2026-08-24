using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class DirectOnlineSaleLifecycleTests
{
    private readonly DirectOnlineSaleSimulationValidator validator = new();

    [Fact]
    public void DIRECT1은온라인직판결정과300kg수확Lot에서시작한다()
    {
        var source = Snapshot();
        Assert.Equal(DirectOnlineSaleStateCodes.AwaitingPackingReview, source.StateCode);
        Assert.Equal(HarvestDispositionChoiceCodes.DirectOnlineSale, source.DispositionDecision.ChoiceCode);
        Assert.Equal(300m, source.HarvestLot.Quantity);
        Assert.Null(source.PackingLot);
    }

    [Fact]
    public void DIRECT1_PreviewConfirm은포장Lot을미리만들지않는다()
    {
        var source = Snapshot();
        var engine = Engine();
        var command = engine.Confirm(source, engine.PreviewPacking(source));
        Assert.Equal(source.DataRevision, command.ExpectedDataRevision);
        Assert.Null(source.PackingLot);
        Assert.Null(source.ListingCandidate);
    }

    [Fact]
    public void DIRECT1_Tick은5kg60개와등록후보를만든다()
    {
        var result = Pack();
        Assert.Equal(60, result.PackingLot!.ParcelCount);
        Assert.Equal(5m, result.PackingLot.NetQuantityPerParcelKg);
        Assert.Equal(300m, result.PackingLot.NetQuantity);
        Assert.Equal("CandidateOnly", result.ListingCandidate!.PublicationStateCode);
        Assert.Equal(result.PackingLot.StableId, result.ListingCandidate.PackingLotStableId);
    }

    [Fact]
    public void DIRECT1_조합결정은직판포장으로열리지않는다()
    {
        var disposition = Disposition(HarvestDispositionChoiceCodes.CooperativeShipment);
        Assert.Equal("DirectOnlineSaleDispositionRequired",
            Assert.Throws<InvalidOperationException>(() => DirectOnlineSaleSimulationFixture.Create(disposition)).Message);
    }

    [Fact]
    public void DIRECT1_StalePreview를거부한다()
    {
        var source = Snapshot();
        var preview = Engine().PreviewPacking(source);
        source.DataRevision++;
        Assert.Equal("DirectOnlineSalePreviewStaleOrInvalid",
            Assert.Throws<InvalidOperationException>(() => Engine().Confirm(source, preview)).Message);
    }

    [Fact]
    public void DIRECT1_포장전상품초안을열수없다()
    {
        Assert.Equal("OnlineMarketListingCandidateRequired",
            Assert.Throws<InvalidOperationException>(() =>
                new DirectOnlineListingDraftAdapter(validator).Create(Snapshot())).Message);
    }

    [Fact]
    public void DIRECT1_상품초안은비공개이고가격과주문이없다()
    {
        var draft = new DirectOnlineListingDraftAdapter(validator).Create(Pack());
        Assert.False(draft.IsPublished);
        Assert.Null(draft.UnitPrice);
        Assert.Equal(0, draft.OrderCount);
        Assert.Equal(60, draft.AvailableParcelCount);
    }

    [Fact]
    public void DIRECT1_Card는수량후보와판매제한을표시한다()
    {
        var card = new DirectOnlineSaleProjector(validator).Project(Pack());
        Assert.Contains("5kg × 60", card.PackingText);
        Assert.Contains("CANDIDATE ONLY", card.CandidateText);
        Assert.Contains("결제", card.LimitationText);
    }

    private DirectOnlineSaleSimulationEngine Engine() => new(validator);

    private DirectOnlineSaleSimulationSnapshot Pack()
    {
        var source = Snapshot();
        var engine = Engine();
        return engine.Tick(source, engine.Confirm(source, engine.PreviewPacking(source)));
    }

    private static DirectOnlineSaleSimulationSnapshot Snapshot()
        => DirectOnlineSaleSimulationFixture.Create(Disposition(HarvestDispositionChoiceCodes.DirectOnlineSale));

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
