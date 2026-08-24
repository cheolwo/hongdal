using Ssalddel.Unity.Data;
using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class CanonicalProductCultivationLifecycleTests
{
    private readonly 재배달력ProfileValidator calendarValidator = new();
    private readonly 감자재배LifecycleSimulationValidator lifecycleValidator;
    private readonly 감자재배LifecycleSimulationEngine engine;
    private readonly 감자재배LifecycleProjector projector;

    public CanonicalProductCultivationLifecycleTests()
    {
        lifecycleValidator = new 감자재배LifecycleSimulationValidator(
            new FarmSoilTileSimulationValidator(), calendarValidator);
        engine = new 감자재배LifecycleSimulationEngine(lifecycleValidator);
        projector = new 감자재배LifecycleProjector(lifecycleValidator);
    }

    [Fact]
    public void CALENDAR0_Profile은_상품_지역_작형_Source_Revision과Window를보존한다()
    {
        var profile = 감자재배LifecycleSimulationFixture.CreateCalendarProfile();

        calendarValidator.Validate(profile);

        Assert.Equal("product:potato", profile.CanonicalProductStableId);
        Assert.Equal("FixtureCentralKr", profile.RegionCode);
        Assert.Equal("OpenFieldFixture", profile.CultivationMethodCode);
        Assert.Equal(재배달력SourceTypeCodes.Fixture, profile.SourceTypeCode);
        Assert.Equal(데이터품질Codes.Fixture, profile.QualityCode);
        Assert.Single(profile.Limitations);
        Assert.Contains(profile.ActivityWindows, value => value.ActivityCode == 재배활동Codes.Sowing);
        Assert.Contains(profile.ActivityWindows, value => value.ActivityCode == 재배활동Codes.Harvest);
    }

    [Fact]
    public void CALENDAR0_Fixture와실제Source품질을혼합하지않는다()
    {
        var profile = 감자재배LifecycleSimulationFixture.CreateCalendarProfile();
        profile.QualityCode = 데이터품질Codes.Valid;

        Assert.Equal("CultivationCalendarProfileSourceQualityMismatch",
            Assert.Throws<InvalidOperationException>(() => calendarValidator.Validate(profile)).Message);
    }

    [Fact]
    public void CALENDAR0_날짜Window와생육단계가잘못되면거부한다()
    {
        var profile = 감자재배LifecycleSimulationFixture.CreateCalendarProfile();
        profile.ActivityWindows[0].StartMonth = 13;

        Assert.Equal("CultivationCalendarActivityWindowInvalid",
            Assert.Throws<InvalidOperationException>(() => calendarValidator.Validate(profile)).Message);

        var snapshot = 감자재배LifecycleSimulationFixture.Create();
        snapshot.SimulationRule.GrowthStages[^1].MinimumDaysAfterSowing = 0;
        Assert.Equal("CultivationCalendarGrowthStagesInvalid",
            Assert.Throws<InvalidOperationException>(() => lifecycleValidator.Validate(snapshot)).Message);
    }

    [Fact]
    public void FARM3_파종Preview와Confirm은Snapshot을변경하지않는다()
    {
        var snapshot = 감자재배LifecycleSimulationFixture.Create();
        var tile = snapshot.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);

        var preview = engine.PreviewSowing(snapshot, tile.StableId);
        var command = engine.Confirm(snapshot, preview);

        Assert.True(preview.RequiresExplicitConfirmation);
        Assert.Equal(재배LifecycleCommandCodes.Sow, command.CommandCode);
        Assert.Null(snapshot.Cultivation);
        Assert.Equal(FarmSoilTileCultivationStateCodes.Tilled, tile.CultivationStateCode);
    }

    [Fact]
    public void FARM3_확인된파종Tick이재배작기와Sown타일을만든다()
    {
        var snapshot = 감자재배LifecycleSimulationFixture.Create();
        var tile = snapshot.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        var command = engine.Confirm(snapshot, engine.PreviewSowing(snapshot, tile.StableId));

        var next = engine.Tick(snapshot, command);

        Assert.Null(snapshot.Cultivation);
        Assert.NotNull(next.Cultivation);
        Assert.Equal("product:potato", next.Cultivation!.CanonicalProductStableId);
        Assert.Equal(next.CalendarProfile.StableId, next.Cultivation.CalendarProfileStableId);
        Assert.Equal(next.CalendarProfile.Revision, next.Cultivation.CalendarProfileRevision);
        var changed = next.Soil.Tiles.Single(value => value.StableId == tile.StableId);
        Assert.Equal(FarmSoilTileCultivationStateCodes.Sown, changed.CultivationStateCode);
        Assert.Equal(next.Cultivation.StableId, changed.ActiveCultivationStableId);
    }

    [Fact]
    public void FARM3_날짜진행은생육단계를결정적으로HarvestReady까지전이한다()
    {
        var sown = Sow();

        var dayTwo = engine.Tick(sown, engine.CreateAdvanceDaysCommand(sown, 2));
        var daySix = engine.Tick(dayTwo, engine.CreateAdvanceDaysCommand(dayTwo, 4));

        Assert.Equal(new DateTimeOffset(2026, 4, 7, 0, 0, 0, TimeSpan.Zero), daySix.SimulationDate);
        Assert.Equal(재배생육단계Codes.Vegetative, dayTwo.Cultivation!.GrowthStageCode);
        Assert.Equal(재배생육단계Codes.HarvestReady, daySix.Cultivation!.GrowthStageCode);
        Assert.Equal(6, daySix.Cultivation.DaysAfterSowing);
    }

    [Fact]
    public void FARM3_HarvestReady전에는수확Preview를거부한다()
    {
        var sown = Sow();

        Assert.Equal("PotatoHarvestNotReady",
            Assert.Throws<InvalidOperationException>(() => engine.PreviewHarvest(sown)).Message);
    }

    [Fact]
    public void FARM3_수확ConfirmTick이Lineage와수량을가진HarvestLot을만든다()
    {
        var ready = AdvanceToHarvestReady();
        var preview = engine.PreviewHarvest(ready);
        var command = engine.Confirm(ready, preview);

        var harvested = engine.Tick(ready, command);

        Assert.Null(ready.HarvestLot);
        Assert.Equal(재배생육단계Codes.Harvested, harvested.Cultivation!.GrowthStageCode);
        Assert.Equal(harvested.SimulationDate, harvested.Cultivation.HarvestedOn);
        Assert.NotNull(harvested.HarvestLot);
        Assert.Equal("product:potato", harvested.HarvestLot!.CanonicalProductStableId);
        Assert.Equal(harvested.Cultivation.StableId, harvested.HarvestLot.CultivationStableId);
        Assert.Equal(300m, harvested.HarvestLot.Quantity);
        Assert.Equal("kg", harvested.HarvestLot.UnitCode);
        Assert.Contains(harvested.Cultivation.StableId, harvested.HarvestLot.SourceStableIds);
        var tile = harvested.Soil.Tiles.Single(value =>
            value.StableId == harvested.Cultivation.TileStableId);
        Assert.Equal(FarmSoilTileCultivationStateCodes.Harvested, tile.CultivationStateCode);
        Assert.Null(tile.ActiveCultivationStableId);
    }

    [Fact]
    public void FARM3_StalePreview와Command는재적용하지않는다()
    {
        var snapshot = 감자재배LifecycleSimulationFixture.Create();
        var tile = snapshot.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        var preview = engine.PreviewSowing(snapshot, tile.StableId);
        snapshot.DataRevision++;
        snapshot.Soil.DataRevision++;

        Assert.Equal("PotatoCultivationPreviewStaleOrInvalid",
            Assert.Throws<InvalidOperationException>(() => engine.Confirm(snapshot, preview)).Message);
    }

    [Fact]
    public void FARM3_날짜진행Command는다른타일로위조할수없다()
    {
        var sown = Sow();
        var command = engine.CreateAdvanceDaysCommand(sown, 1);
        var other = sown.Soil.Tiles.First(value => value.StableId != command.TileStableId);
        command.TileStableId = other.StableId;
        command.StableId = "farm-advancedays-command:sim.potato.r"
            + sown.DataRevision + ".tile." + other.GridX + "." + other.GridZ + ".d1";

        Assert.Equal("PotatoAdvanceDaysCommandInvalid",
            Assert.Throws<InvalidOperationException>(() => engine.Tick(sown, command)).Message);
    }

    [Fact]
    public void FARM3_Projector는게임날짜_Source제한_행동가능성과HarvestLot을보존한다()
    {
        var initial = 감자재배LifecycleSimulationFixture.Create();
        var initialView = projector.Project(initial);
        var harvestedView = projector.Project(Harvest());

        Assert.Equal("2026-04-01", initialView.SimulationDateText);
        Assert.Equal("Simulation/Fixture", initialView.SourceModeCode);
        Assert.True(initialView.CanPreviewSowing);
        Assert.Contains("실제 파종·수확 권고", initialView.LimitationText);
        Assert.Equal(재배생육단계Codes.Harvested, harvestedView.GrowthStageCode);
        Assert.Contains("300kg", harvestedView.HarvestLotText);
    }

    private 감자재배LifecycleSimulationSnapshot Sow()
    {
        var snapshot = 감자재배LifecycleSimulationFixture.Create();
        var tile = snapshot.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        return engine.Tick(snapshot, engine.Confirm(snapshot, engine.PreviewSowing(snapshot, tile.StableId)));
    }

    private 감자재배LifecycleSimulationSnapshot AdvanceToHarvestReady()
    {
        var sown = Sow();
        return engine.Tick(sown, engine.CreateAdvanceDaysCommand(sown, 6));
    }

    private 감자재배LifecycleSimulationSnapshot Harvest()
    {
        var ready = AdvanceToHarvestReady();
        return engine.Tick(ready, engine.Confirm(ready, engine.PreviewHarvest(ready)));
    }
}
