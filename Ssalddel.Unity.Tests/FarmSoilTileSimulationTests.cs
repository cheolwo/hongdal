using Ssalddel.Unity.Farm;

namespace Ssalddel.Tests.UnityData;

public sealed class FarmSoilTileSimulationTests
{
    private readonly FarmSoilTileSimulationValidator validator = new();
    private readonly FarmSoilTileMapProjector projector =
        new(new FarmSoilTileSimulationValidator());
    private readonly FarmSoilTileTillingSimulationEngine tilling =
        new(new FarmSoilTileSimulationValidator());

    [Fact]
    public void 감자Fixture는_6x6토양타일과Simulation경계를보존한다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();

        validator.Validate(snapshot);
        Assert.Equal("Simulation", snapshot.ModeCode);
        Assert.Equal(36, snapshot.Tiles.Length);
        Assert.Equal(36, snapshot.Tiles.Select(value => (value.GridX, value.GridZ)).Distinct().Count());
        Assert.Single(snapshot.Tiles, value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Sown);
    }

    [Fact]
    public void Projector는_StableId선택과상태색을View판단없이제공한다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();
        var selected = snapshot.Tiles.Single(value => value.GridX == 0 && value.GridZ == 0);

        var result = projector.Project(snapshot, selected.StableId);

        Assert.Equal(selected.StableId, result.SelectedTileStableId);
        Assert.Equal(FarmSoilTileColorTokens.Selected,
            result.Tiles.Single(value => value.StableId == selected.StableId).ColorToken);
        Assert.Contains("밭갈이 Preview 가능", result.SelectedTileDetailText);
    }

    [Fact]
    public void 중복좌표를거부한다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();
        snapshot.Tiles[1].GridX = snapshot.Tiles[0].GridX;
        snapshot.Tiles[1].GridZ = snapshot.Tiles[0].GridZ;

        Assert.Equal("FarmSoilTileCoordinateDuplicate",
            Assert.Throws<InvalidOperationException>(() => validator.Validate(snapshot)).Message);
    }

    [Fact]
    public void 빠진타일을0이나가상타일로보완하지않는다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();
        snapshot.Tiles = snapshot.Tiles.Skip(1).ToArray();

        Assert.Equal("FarmSoilTileGridIncomplete",
            Assert.Throws<InvalidOperationException>(() => validator.Validate(snapshot)).Message);
    }

    [Fact]
    public void 파종상태와재배참조불일치를거부한다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();
        snapshot.Tiles.Single(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Sown)
            .ActiveCultivationStableId = null;

        Assert.Equal("FarmSoilTileStateReferenceMismatch",
            Assert.Throws<InvalidOperationException>(() => validator.Validate(snapshot)).Message);
    }

    [Fact]
    public void 존재하지않는선택을거부한다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();

        Assert.Equal("FarmSoilTileSelectionMissing:farm-soil-tile:sim.missing.0.0",
            Assert.Throws<InvalidOperationException>(() =>
                projector.Project(snapshot, "farm-soil-tile:sim.missing.0.0")).Message);
    }

    [Fact]
    public void Preview와Confirm은_Snapshot을변경하지않는다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();
        var tile = snapshot.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Untilled);

        var preview = tilling.Preview(snapshot, tile.StableId);
        var command = tilling.Confirm(snapshot, preview);

        Assert.True(preview.RequiresExplicitConfirmation);
        Assert.Equal(snapshot.DataRevision, command.ExpectedDataRevision);
        Assert.Equal(FarmSoilTileCultivationStateCodes.Untilled, tile.CultivationStateCode);
        Assert.Equal(1, snapshot.DataRevision);
    }

    [Fact]
    public void 명시적으로확인된Command의Tick만_새Snapshot을Tilled로전이한다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();
        var tile = snapshot.Tiles.Single(value => value.GridX == 0 && value.GridZ == 0);
        var command = tilling.Confirm(snapshot, tilling.Preview(snapshot, tile.StableId));

        var next = tilling.Tick(snapshot, command);

        Assert.NotSame(snapshot, next);
        Assert.Equal(1, snapshot.DataRevision);
        Assert.Equal(2, next.DataRevision);
        Assert.Equal(FarmSoilTileCultivationStateCodes.Untilled, tile.CultivationStateCode);
        var changed = next.Tiles.Single(value => value.StableId == tile.StableId);
        Assert.Equal(FarmSoilTileCultivationStateCodes.Tilled, changed.CultivationStateCode);
        Assert.Equal(tile.Revision + 1, changed.Revision);
        Assert.Contains(command.StableId, next.SourceStableIds);
    }

    [Fact]
    public void Confirm없이만든Command와StaleCommand를_Tick하지않는다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();
        var tile = snapshot.Tiles.Single(value => value.GridX == 0 && value.GridZ == 0);
        var command = tilling.Confirm(snapshot, tilling.Preview(snapshot, tile.StableId));
        command.PreviewStableId = "farm-tilling-preview:forged";

        Assert.Equal("FarmSoilTileTillingCommandInvalid",
            Assert.Throws<InvalidOperationException>(() => tilling.Tick(snapshot, command)).Message);

        command = tilling.Confirm(snapshot, tilling.Preview(snapshot, tile.StableId));
        snapshot.DataRevision++;
        Assert.Equal("FarmSoilTileTillingCommandStale",
            Assert.Throws<InvalidOperationException>(() => tilling.Tick(snapshot, command)).Message);
    }

    [Fact]
    public void 이미경작된타일은_밭갈이Preview를거부한다()
    {
        var snapshot = FarmPotatoSoilTileSimulationFixture.Create();
        var tile = snapshot.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);

        Assert.Equal("FarmSoilTileTillingNotAllowed:" + tile.StableId,
            Assert.Throws<InvalidOperationException>(() =>
                tilling.Preview(snapshot, tile.StableId)).Message);
    }
}
