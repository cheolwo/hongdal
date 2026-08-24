using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class CanonicalProductHarvestCargoLifecycleTests
{
    private readonly 감자수확CargoSimulationValidator validator = new();

    [Fact]
    public void CARGO1_포장Preview와Confirm은HarvestLot과Snapshot을변경하지않는다()
    {
        var source = Snapshot();
        var engine = Engine();
        var preview = engine.PreviewPacking(source);
        var command = engine.Confirm(source, preview);

        Assert.True(preview.RequiresExplicitConfirmation);
        Assert.Equal(수확CargoCommandCodes.Pack, command.CommandCode);
        Assert.Null(source.PackageLot);
        Assert.Null(source.Cargo);
        Assert.Equal(1, source.DataRevision);
    }

    [Fact]
    public void CARGO1_포장Tick은300kg을20kg상자15개로수량보존한다()
    {
        var source = Snapshot();
        var engine = Engine();
        var packed = engine.Tick(source, engine.Confirm(source, engine.PreviewPacking(source)));

        Assert.Null(source.PackageLot);
        Assert.NotNull(packed.PackageLot);
        Assert.Equal(15, packed.PackageLot!.PackageCount);
        Assert.Equal(300m, packed.PackageLot.NetQuantity);
        Assert.Equal(source.HarvestLot.StableId, packed.PackageLot.HarvestLotStableId);
        Assert.Contains(source.HarvestLot.StableId, packed.PackageLot.SourceStableIds);
    }

    [Fact]
    public void CARGO1_상차Tick은Harvest_Package_CargoLineage와차량용량을보존한다()
    {
        var packed = Pack();
        var engine = Engine();
        var loaded = engine.Tick(packed, engine.Confirm(packed, engine.PreviewLoading(packed)));

        Assert.Null(packed.Cargo);
        Assert.NotNull(loaded.Cargo);
        Assert.Equal("product:potato", loaded.Cargo!.CanonicalProductStableId);
        Assert.Equal(loaded.HarvestLot.StableId, loaded.Cargo.HarvestLotStableId);
        Assert.Equal(loaded.PackageLot!.StableId, loaded.Cargo.PackageLotStableId);
        Assert.Equal(15, loaded.Cargo.PackageCount);
        Assert.Equal(300m, loaded.Cargo.Quantity);
        Assert.Equal(400m, loaded.Cargo.VehicleCapacityKg);
        Assert.Equal(수확CargoStateCodes.Loaded, loaded.Cargo.StateCode);
    }

    [Fact]
    public void CARGO1_포장없는상차와차량용량초과를거부한다()
    {
        var source = Snapshot();
        Assert.Equal("PotatoHarvestCargoPackageRequired",
            Assert.Throws<InvalidOperationException>(() => Engine().PreviewLoading(source)).Message);

        var packed = Pack();
        packed.PackagingRule.VehicleCapacityKg = 200m;
        Assert.Equal("PotatoHarvestCargoVehicleCapacityExceeded",
            Assert.Throws<InvalidOperationException>(() => Engine().PreviewLoading(packed)).Message);
    }

    [Fact]
    public void CARGO1_수량변조와StalePreview를거부한다()
    {
        var packed = Pack();
        packed.PackageLot!.NetQuantity = 280m;
        Assert.Equal("PotatoHarvestCargoPackageConservationInvalid",
            Assert.Throws<InvalidOperationException>(() => validator.Validate(packed)).Message);

        var source = Snapshot();
        var preview = Engine().PreviewPacking(source);
        source.DataRevision++;
        Assert.Equal("PotatoHarvestCargoPreviewStaleOrInvalid",
            Assert.Throws<InvalidOperationException>(() => Engine().Confirm(source, preview)).Message);
    }

    [Fact]
    public void CARGO1_Projector는Simulation경계와전체Lineage를표시한다()
    {
        var loaded = Load();
        var view = new 감자수확CargoProjector(validator).Project(loaded);

        Assert.Equal(수확CargoStateCodes.Loaded, view.StateCode);
        Assert.Equal("Simulation/Fixture", view.SourceModeCode);
        Assert.Contains("15 Box", view.PackageLotText);
        Assert.Contains("300kg / 400kg", view.CargoText);
        Assert.Contains(loaded.HarvestLot.StableId, view.LineageText);
        Assert.Contains(loaded.PackageLot!.StableId, view.LineageText);
        Assert.Contains(loaded.Cargo!.StableId, view.LineageText);
        Assert.Contains("운영 포장·운송 기준이 아닙니다", view.LimitationText);
    }

    private 감자수확CargoSimulationEngine Engine() => new(validator);

    private 감자수확CargoSimulationSnapshot Pack()
    {
        var source = Snapshot();
        var engine = Engine();
        return engine.Tick(source, engine.Confirm(source, engine.PreviewPacking(source)));
    }

    private 감자수확CargoSimulationSnapshot Load()
    {
        var packed = Pack();
        var engine = Engine();
        return engine.Tick(packed, engine.Confirm(packed, engine.PreviewLoading(packed)));
    }

    private static 감자수확CargoSimulationSnapshot Snapshot()
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
        return 감자수확CargoSimulationFixture.Create(harvested.HarvestLot!);
    }
}
