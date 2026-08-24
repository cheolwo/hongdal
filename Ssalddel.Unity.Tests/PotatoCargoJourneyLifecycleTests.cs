using Ssalddel.Unity.Farm;
using Ssalddel.Unity.PotatoJourney;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class PotatoCargoJourneyLifecycleTests
{
    private readonly PotatoCargoJourneySimulationValidator validator = new();

    [Fact]
    public void JOURNEY1_DispatchPreview와Confirm은Snapshot과CargoRevision을변경하지않는다()
    {
        var source = Snapshot();
        var engine = Engine();
        var preview = engine.PreviewDispatch(source);
        var command = engine.ConfirmDispatch(source, preview);

        Assert.True(preview.RequiresExplicitConfirmation);
        Assert.Equal(PotatoCargoJourneyCommandCodes.Dispatch, command.CommandCode);
        Assert.Equal(PotatoCargoJourneyStateCodes.Loaded, source.StateCode);
        Assert.Equal(1, source.CargoRevision);
        Assert.Equal(1, source.DataRevision);
    }

    [Fact]
    public void JOURNEY1_DispatchTick만Loaded를InTransit으로바꾼다()
    {
        var source = Snapshot();
        var engine = Engine();
        var transit = engine.Tick(source,
            engine.ConfirmDispatch(source, engine.PreviewDispatch(source)));

        Assert.Equal(PotatoCargoJourneyStateCodes.Loaded, source.StateCode);
        Assert.Equal(PotatoCargoJourneyStateCodes.InTransit, transit.StateCode);
        Assert.Equal(source.Cargo.StableId, transit.Cargo.StableId);
        Assert.Equal(2, transit.CargoRevision);
        Assert.Equal(300m, transit.Cargo.Quantity);
    }

    [Fact]
    public void JOURNEY1_RouteTick은날짜와진행도를올리고세번째에Hub도착한다()
    {
        var engine = Engine();
        var transit = Dispatch();
        var dayOne = engine.Tick(transit, engine.CreateAdvanceRouteCommand(transit, 1));
        var arrived = engine.Tick(dayOne, engine.CreateAdvanceRouteCommand(dayOne, 2));

        Assert.Equal(PotatoCargoJourneyStateCodes.InTransit, dayOne.StateCode);
        Assert.Equal(1, dayOne.CompletedRouteTicks);
        Assert.Equal(new DateTimeOffset(2026, 4, 8, 0, 0, 0, TimeSpan.Zero), dayOne.SimulationDate);
        Assert.Equal(PotatoCargoJourneyStateCodes.ArrivedAtHub, arrived.StateCode);
        Assert.Equal(3, arrived.CompletedRouteTicks);
        Assert.Equal(new DateTimeOffset(2026, 4, 10, 0, 0, 0, TimeSpan.Zero), arrived.SimulationDate);
    }

    [Fact]
    public void JOURNEY1_상태전이후에도CargoIdentity수량과Lineage가보존된다()
    {
        var source = Snapshot();
        var arrived = Arrive();

        Assert.Equal(source.Cargo.StableId, arrived.Cargo.StableId);
        Assert.Equal(source.HarvestLotStableId, arrived.HarvestLotStableId);
        Assert.Equal(source.PackageLotStableId, arrived.PackageLotStableId);
        Assert.Equal(15, arrived.Cargo.PackageCount);
        Assert.Equal(300m, arrived.Cargo.Quantity);
        Assert.Contains(source.HarvestLotStableId, arrived.Cargo.SourceStableIds);
        Assert.Contains(source.PackageLotStableId, arrived.Cargo.SourceStableIds);
    }

    [Fact]
    public void JOURNEY1_StaleDispatch와범위를넘는RouteTick을거부한다()
    {
        var source = Snapshot();
        var preview = Engine().PreviewDispatch(source);
        source.DataRevision++;
        Assert.Equal("PotatoCargoJourneyPreviewStaleOrInvalid",
            Assert.Throws<InvalidOperationException>(() => Engine().ConfirmDispatch(source, preview)).Message);

        var transit = Dispatch();
        Assert.Equal("PotatoCargoJourneyAdvanceInvalid",
            Assert.Throws<InvalidOperationException>(() =>
                Engine().CreateAdvanceRouteCommand(transit, 4)).Message);
    }

    [Fact]
    public void JOURNEY1_Projector는상태날짜진행도와Simulation제한을보존한다()
    {
        var arrived = Arrive();
        var view = new PotatoCargoJourneyProjector(validator).Project(arrived);

        Assert.Equal(PotatoCargoJourneyStateCodes.ArrivedAtHub, view.StateCode);
        Assert.Equal("2026-04-10", view.DateText);
        Assert.Equal("3 / 3 route ticks", view.ProgressText);
        Assert.Equal(1f, view.NormalizedProgress);
        Assert.Contains(arrived.Cargo.StableId, view.CargoText);
        Assert.Contains(arrived.HarvestLotStableId, view.LineageText);
        Assert.Contains("실제 운송 시간이나 인수를 뜻하지 않습니다", view.LimitationText);
    }

    private PotatoCargoJourneySimulationEngine Engine() => new(validator);

    private PotatoCargoJourneySimulationSnapshot Dispatch()
    {
        var source = Snapshot();
        var engine = Engine();
        return engine.Tick(source, engine.ConfirmDispatch(source, engine.PreviewDispatch(source)));
    }

    private PotatoCargoJourneySimulationSnapshot Arrive()
    {
        var transit = Dispatch();
        return Engine().Tick(transit, Engine().CreateAdvanceRouteCommand(transit, 3));
    }

    private static PotatoCargoJourneySimulationSnapshot Snapshot()
        => PotatoCargoJourneySimulationFixture.Create(LoadedCargo());

    private static 감자수확CargoSimulationSnapshot LoadedCargo()
    {
        var lifecycleValidator = new 감자재배LifecycleSimulationValidator(
            new FarmSoilTileSimulationValidator(), new 재배달력ProfileValidator());
        var lifecycle = new 감자재배LifecycleSimulationEngine(lifecycleValidator);
        var source = 감자재배LifecycleSimulationFixture.Create();
        var tile = source.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        var sown = lifecycle.Tick(source,
            lifecycle.Confirm(source, lifecycle.PreviewSowing(source, tile.StableId)));
        var ready = lifecycle.Tick(sown, lifecycle.CreateAdvanceDaysCommand(sown, 6));
        var harvested = lifecycle.Tick(ready,
            lifecycle.Confirm(ready, lifecycle.PreviewHarvest(ready)));
        var cargo = 감자수확CargoSimulationFixture.Create(harvested.HarvestLot!);
        var engine = new 감자수확CargoSimulationEngine(new 감자수확CargoSimulationValidator());
        cargo = engine.Tick(cargo, engine.Confirm(cargo, engine.PreviewPacking(cargo)));
        return engine.Tick(cargo, engine.Confirm(cargo, engine.PreviewLoading(cargo)));
    }
}
