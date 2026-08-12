using Ssalddel.Unity.Farm;
using Ssalddel.Unity.PotatoJourney;

namespace Ssalddel.Unity.Tests;

public sealed class PotatoHubDispositionLifecycleTests
{
    private readonly PotatoHubDispositionSimulationValidator validator = new();

    [Fact]
    public void HUB2_분리Preview와Confirm은Lot을만들지않는다()
    {
        var source = Snapshot();
        var engine = Engine();
        var preview = engine.PreviewSeparation(source);
        var command = engine.Confirm(source, preview);

        Assert.True(preview.RequiresExplicitConfirmation);
        Assert.Equal(PotatoHubDispositionCommandCodes.SeparateLots, command.CommandCode);
        Assert.Null(source.AcceptedLot);
        Assert.Null(source.RejectedLossLot);
    }

    [Fact]
    public void HUB2_분리Tick은288kg합격Lot과12kg손실Lot을함께만든다()
    {
        var separated = Separate();

        Assert.Equal(PotatoHubDispositionStateCodes.LotsSeparated, separated.StateCode);
        Assert.Equal(288m, separated.AcceptedLot!.Quantity);
        Assert.Equal("AcceptedForOutbound", separated.AcceptedLot.StateCode);
        Assert.Equal(12m, separated.RejectedLossLot!.Quantity);
        Assert.Equal("LossRecorded", separated.RejectedLossLot.StateCode);
        Assert.Equal("DamageFixture", separated.RejectedLossLot.ReasonCode);
        Assert.Equal(separated.ReceivedQuantityKg,
            separated.AcceptedLot.Quantity + separated.RejectedLossLot.Quantity);
    }

    [Fact]
    public void WORLD8_OutboundPreview와Confirm은Cargo나Candidate를미리만들지않는다()
    {
        var separated = Separate();
        var engine = Engine();
        var preview = engine.PreviewOutboundCandidate(separated);
        var command = engine.Confirm(separated, preview);

        Assert.Equal(separated.AcceptedLot!.StableId, preview.SourceLotStableId);
        Assert.Equal(PotatoHubDispositionCommandCodes.CreateOutboundCandidate, command.CommandCode);
        Assert.Null(separated.OutboundCandidate);
    }

    [Fact]
    public void WORLD8_OutboundCandidate는합격Lot288kg만참조한다()
    {
        var candidate = Candidate();

        Assert.Equal(PotatoHubDispositionStateCodes.OutboundCandidate, candidate.StateCode);
        Assert.Equal(candidate.AcceptedLot!.StableId, candidate.OutboundCandidate!.AcceptedLotStableId);
        Assert.Equal(288m, candidate.OutboundCandidate.Quantity);
        Assert.Equal("CandidateOnly", candidate.OutboundCandidate.StateCode);
        Assert.Contains(candidate.AcceptedLot.StableId, candidate.OutboundCandidate.SourceStableIds);
        Assert.DoesNotContain(candidate.RejectedLossLot!.StableId, candidate.OutboundCandidate.SourceStableIds);
    }

    [Fact]
    public void HUB2_손실Lot누락과수량변조를거부한다()
    {
        var separated = Separate();
        separated.RejectedLossLot = null;
        Assert.Equal("PotatoHubDispositionLotsStateMismatch",
            Assert.Throws<InvalidOperationException>(() => validator.Validate(separated)).Message);

        separated = Separate();
        separated.AcceptedLot!.Quantity = 289m;
        Assert.Equal("PotatoHubAcceptedLotInvalid",
            Assert.Throws<InvalidOperationException>(() => validator.Validate(separated)).Message);
    }

    [Fact]
    public void WORLD8_손실Lot을OutboundCandidateSource로사용하면거부한다()
    {
        var candidate = Candidate();
        candidate.OutboundCandidate!.SourceStableIds = candidate.OutboundCandidate.SourceStableIds
            .Append(candidate.RejectedLossLot!.StableId).ToArray();

        Assert.Equal("PotatoCityOutboundCandidateInvalid",
            Assert.Throws<InvalidOperationException>(() => validator.Validate(candidate)).Message);
    }

    [Fact]
    public void HUB2_Projector는분리Lot후보Lineage와Simulation제한을보존한다()
    {
        var candidate = Candidate();
        var model = new PotatoHubDispositionProjector(validator).Project(candidate);

        Assert.Contains("288kg ACCEPTED", model.LotsText);
        Assert.Contains("12kg LOSS", model.LotsText);
        Assert.Contains("DamageFixture", model.LotsText);
        Assert.Contains("CANDIDATE ONLY", model.CandidateText);
        Assert.Contains(candidate.AcceptedLot!.StableId, model.LineageText);
        Assert.DoesNotContain(candidate.RejectedLossLot!.StableId, model.LineageText);
        Assert.Contains("출발 Cargo", model.LimitationText);
    }

    private PotatoHubDispositionSimulationEngine Engine() => new(validator);

    private PotatoHubDispositionSimulationSnapshot Separate()
    {
        var source = Snapshot();
        var engine = Engine();
        return engine.Tick(source, engine.Confirm(source, engine.PreviewSeparation(source)));
    }

    private PotatoHubDispositionSimulationSnapshot Candidate()
    {
        var source = Separate();
        var engine = Engine();
        return engine.Tick(source, engine.Confirm(source, engine.PreviewOutboundCandidate(source)));
    }

    private static PotatoHubDispositionSimulationSnapshot Snapshot()
        => PotatoHubDispositionSimulationFixture.Create(Accepted());

    private static PotatoHubReceivingSimulationSnapshot Accepted()
    {
        var receiving = PotatoHubReceivingSimulationFixture.Create(Arrived());
        var engine = new PotatoHubReceivingSimulationEngine(new PotatoHubReceivingSimulationValidator());
        receiving = engine.Tick(receiving, engine.Confirm(receiving, engine.PreviewReceiving(receiving)));
        return engine.Tick(receiving, engine.Confirm(receiving, engine.PreviewInspection(receiving)));
    }

    private static PotatoCargoJourneySimulationSnapshot Arrived()
    {
        var journey = PotatoCargoJourneySimulationFixture.Create(Loaded());
        var engine = new PotatoCargoJourneySimulationEngine(new PotatoCargoJourneySimulationValidator());
        journey = engine.Tick(journey, engine.ConfirmDispatch(journey, engine.PreviewDispatch(journey)));
        return engine.Tick(journey, engine.CreateAdvanceRouteCommand(journey, 3));
    }

    private static 감자수확CargoSimulationSnapshot Loaded()
    {
        var lifecycleValidator = new 감자재배LifecycleSimulationValidator(
            new FarmSoilTileSimulationValidator(), new 재배달력ProfileValidator());
        var lifecycleEngine = new 감자재배LifecycleSimulationEngine(lifecycleValidator);
        var lifecycle = 감자재배LifecycleSimulationFixture.Create();
        var tile = lifecycle.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        lifecycle = lifecycleEngine.Tick(lifecycle,
            lifecycleEngine.Confirm(lifecycle, lifecycleEngine.PreviewSowing(lifecycle, tile.StableId)));
        lifecycle = lifecycleEngine.Tick(lifecycle, lifecycleEngine.CreateAdvanceDaysCommand(lifecycle, 6));
        lifecycle = lifecycleEngine.Tick(lifecycle,
            lifecycleEngine.Confirm(lifecycle, lifecycleEngine.PreviewHarvest(lifecycle)));
        var cargo = 감자수확CargoSimulationFixture.Create(lifecycle.HarvestLot!);
        var cargoEngine = new 감자수확CargoSimulationEngine(new 감자수확CargoSimulationValidator());
        cargo = cargoEngine.Tick(cargo, cargoEngine.Confirm(cargo, cargoEngine.PreviewPacking(cargo)));
        return cargoEngine.Tick(cargo, cargoEngine.Confirm(cargo, cargoEngine.PreviewLoading(cargo)));
    }
}
