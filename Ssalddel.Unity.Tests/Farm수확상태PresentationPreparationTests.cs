using System.Globalization;
using System.Text.Json;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "독립 상태 사본의 누락·판본·중복·실패 보존과 Farm 표현 소비를 검증한다.",
    Boundary = "시험 자료는 실제 Session에 주입하지 않으며 Editor·Game View·수확 성공 증거가 아니다.")]
public sealed class Farm수확상태PresentationPreparationTests
{
    [Theory]
    [InlineData("Growing", "farm.crop.grow")]
    [InlineData("HarvestReady", "farm.crop.grow")]
    [InlineData("Harvested", "farm.crop.harvest")]
    public void 기존상태사본은_생산계산없이_정적표현으로_분리한다(string code, string slot)
    {
        var source = Snapshot(code);
        var before = JsonSerializer.Serialize(source);
        Assert.True(Create().TryPrepare(source, out var state, out var reason));
        Assert.Equal("Prepared", reason);
        Assert.Equal(code, state!.StateCode);
        Assert.Equal(slot, state.PresentationSlot);
        Assert.True(state.PresentationOnly);
        Assert.False(state.CanConfirmAuthority);
        Assert.Equal("E5Unlinked", state.SceneBindingStatus);
        Assert.Equal(code == "Harvested" ? 12.375m : (decimal?)null, state.Quantity);
        Assert.Equal(before, JsonSerializer.Serialize(source));
    }

    [Fact]
    public void 수확량과_단위와_계보는_기존Lot의_값을읽는다()
    {
        var source = Snapshot("Harvested");
        source.HarvestLots[0].Quantity = 0;
        source.HarvestLots[0].UnitCode = "fixture-unit";
        Assert.True(Create().TryPrepare(source, out var state, out _));
        Assert.Equal(0, state!.Quantity);
        Assert.Equal("fixture-unit", state.UnitCode);
        Assert.Equal("task:harvest", state.CausedByTaskStableId);
        Assert.Equal("lot:one", state.HarvestLotStableId);
    }

    [Fact]
    public void 같은사본재반영은_기존준비인스턴스를_유지한다()
    {
        var consumer = Create();
        Assert.True(consumer.TryPrepare(Snapshot(), out var first, out _));
        Assert.True(consumer.TryPrepare(Snapshot(), out var second, out var reason));
        Assert.Equal("Unchanged", reason);
        Assert.Same(first, second);
    }

    [Fact]
    public void 동일세계판본의_다른표시내용은_거부하고_직전값을보존한다()
    {
        var consumer = Create();
        consumer.TryPrepare(Snapshot(), out var first, out _);
        Assert.False(consumer.TryPrepare(Snapshot("Harvested"), out var rejected, out var reason));
        Assert.Null(rejected);
        Assert.Equal("FarmSameRevisionConflict", reason);
        Assert.Same(first, consumer.Current);
    }

    [Fact]
    public void 더높은판본의_수확결과를반영하고_공통비교기로_오래된사본을거부한다()
    {
        var consumer = Create();
        consumer.TryPrepare(Snapshot(), out _, out _);
        var next = Snapshot("Harvested");
        next.WorldRevision = 5;
        Assert.True(consumer.TryPrepare(next, out var harvested, out _));
        Assert.False(consumer.TryPrepare(Snapshot(), out var stale, out var reason));
        Assert.Equal("LowerDataRevision", reason);
        Assert.Null(stale);
        Assert.Same(harvested, consumer.Current);
    }

    [Fact]
    public void 내용이같아도_높아진조회판본을_기억해_중간판본을거부한다()
    {
        var consumer = Create();
        consumer.TryPrepare(Snapshot(), out _, out _);
        var newer = Snapshot(); newer.WorldRevision = 7;
        Assert.True(consumer.TryPrepare(newer, out var state, out _));
        var middle = Snapshot(); middle.WorldRevision = 6;
        Assert.False(consumer.TryPrepare(middle, out _, out var reason));
        Assert.Equal("LowerDataRevision", reason);
        Assert.Equal(7, state!.SourceWorldRevision);
    }

    [Fact]
    public void 사본이없으면_E5미연결이며_초기상태를_만들지않는다()
    {
        var consumer = Create();
        Assert.False(consumer.TryPrepare(null, out var state, out var reason));
        Assert.Equal("FarmSnapshotMissing_E5Unlinked", reason);
        Assert.Null(state);
        Assert.Null(consumer.Current);
    }

    [Theory]
    [InlineData("session", "FarmSessionMissing")]
    [InlineData("rule", "FarmRuleRevisionMissing")]
    [InlineData("other-session", "FarmSourceBindingMismatch")]
    [InlineData("other-rule", "FarmSourceBindingMismatch")]
    [InlineData("revision", "FarmSourceRevisionInvalid")]
    [InlineData("tick", "FarmSourceRevisionInvalid")]
    [InlineData("operational", "FarmSimulationBoundaryInvalid")]
    [InlineData("simulation", "FarmSimulationBoundaryInvalid")]
    [InlineData("soils-null", "FarmCollectionsMissing")]
    [InlineData("crops-null", "FarmCollectionsMissing")]
    [InlineData("lots-null", "FarmCollectionsMissing")]
    [InlineData("item-null", "FarmCollectionItemMissing")]
    [InlineData("soil-missing", "FarmSelectedTargetMissingOrDuplicate")]
    [InlineData("crop-duplicate", "FarmSelectedTargetMissingOrDuplicate")]
    [InlineData("soil-link", "FarmCultivationSoilMismatch")]
    [InlineData("state", "FarmCultivationStateUnsupported")]
    [InlineData("crop-revision", "FarmSelectedStateInvalid")]
    [InlineData("lot-missing", "FarmHarvestLotMissingOrDuplicate")]
    [InlineData("lot-duplicate", "FarmHarvestLotMissingOrDuplicate")]
    [InlineData("lot-product", "FarmHarvestLotInvalid")]
    [InlineData("lot-task", "FarmHarvestLotInvalid")]
    [InlineData("lot-unit", "FarmHarvestLotInvalid")]
    [InlineData("lot-quantity", "FarmHarvestLotInvalid")]
    [InlineData("lot-conflict", "FarmHarvestStateConflict")]
    public void 실패는_명시사유와함께_중단하고_입력과_기존준비상태를보존한다(string fault, string expected)
    {
        var consumer = Create();
        consumer.TryPrepare(Snapshot(), out var current, out _);
        var source = Snapshot("Harvested"); source.WorldRevision = 5;
        switch (fault)
        {
            case "session": source.SessionStableId = ""; break;
            case "rule": source.RuleRevision = ""; break;
            case "other-session": source.SessionStableId = "session:other"; break;
            case "other-rule": source.RuleRevision = "other.r1"; break;
            case "revision": source.WorldRevision = -1; break;
            case "tick": source.WorldTick = -1; break;
            case "operational": source.IsOperationalState = true; break;
            case "simulation": source.SimulationOnly = false; break;
            case "soils-null": source.SoilTiles = null!; break;
            case "crops-null": source.CultivationUnits = null!; break;
            case "lots-null": source.HarvestLots = null!; break;
            case "item-null": source.CultivationUnits[0] = null!; break;
            case "soil-missing": source.SoilTiles = Array.Empty<SimulationFarmSoilTileSnapshot>(); break;
            case "crop-duplicate": source.CultivationUnits = new[] { source.CultivationUnits[0], source.CultivationUnits[0] }; break;
            case "soil-link": source.CultivationUnits[0].TileStableId = "soil:other"; break;
            case "state": source.CultivationUnits[0].StateCode = "invented"; break;
            case "crop-revision": source.CultivationUnits[0].Revision = -1; break;
            case "lot-missing": source.HarvestLots = Array.Empty<Simulation수확LotSnapshot>(); break;
            case "lot-duplicate": source.HarvestLots = new[] { source.HarvestLots[0], source.HarvestLots[0] }; break;
            case "lot-product": source.HarvestLots[0].ProductStableId = "product:other"; break;
            case "lot-task": source.HarvestLots[0].CausedByTaskStableId = ""; break;
            case "lot-unit": source.HarvestLots[0].UnitCode = ""; break;
            case "lot-quantity": source.HarvestLots[0].Quantity = -1; break;
            case "lot-conflict": source.CultivationUnits[0].StateCode = "HarvestReady"; break;
        }
        var before = JsonSerializer.Serialize(source);
        Assert.False(consumer.TryPrepare(source, out var rejected, out var reason));
        Assert.Equal(expected, reason);
        Assert.Null(rejected);
        Assert.Same(current, consumer.Current);
        Assert.Equal(before, JsonSerializer.Serialize(source));
        // 실패가 판본을 소비하지 않아 올바른 같은 판본으로 회복할 수 있다.
        var repaired = Snapshot("Harvested"); repaired.WorldRevision = 5;
        Assert.True(consumer.TryPrepare(repaired, out _, out _));
    }

    [Fact]
    public void 외부사본수정이_이미준비된_불변자료를_바꾸지않는다()
    {
        var source = Snapshot("Harvested");
        var consumer = Create();
        consumer.TryPrepare(source, out var prepared, out _);
        source.CultivationUnits[0].StateCode = "Growing";
        source.HarvestLots[0].Quantity = 999;
        Assert.Equal("Harvested", prepared!.StateCode);
        Assert.Equal(12.375m, prepared.Quantity);
    }

    [Fact]
    public void 다른재배주기의Lot을_선택한재배의_결과로_사용하지않는다()
    {
        var source = Snapshot("Harvested");
        source.HarvestLots[0].CultivationUnitStableId = "crop:previous";
        Assert.False(Create().TryPrepare(source, out _, out var reason));
        Assert.Equal("FarmHarvestLotMissingOrDuplicate", reason);
    }

    [Fact]
    public void 영판본도_유효하며_문화권과_배열순서가_표현판본을_바꾸지않는다()
    {
        var source = Snapshot("Harvested"); source.WorldRevision = 0;
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.True(Create().TryPrepare(source, out var first, out _));
            source.CultivationUnits = new[] { new Simulation재배단위Snapshot { CultivationUnitStableId = "crop:other" }, source.CultivationUnits[0] };
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.True(Create().TryPrepare(source, out var second, out _));
            Assert.Equal(first!.PresentationRevision, second!.PresentationRevision);
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    private static Farm수확상태PresentationPreparation Create() =>
        new("session:farm", "farm-rule.r1", "soil:one", "crop:one");

    // 독립 계약 시험값이며 실제 Session에 주입하거나 Core/Save를 실행하지 않는다.
    private static SimulationFarmSurvivalStateSnapshot Snapshot(string state = "HarvestReady") => new()
    {
        SessionStableId = "session:farm", RuleRevision = "farm-rule.r1", WorldRevision = 4, WorldTick = 2,
        SoilTiles = new[] { new SimulationFarmSoilTileSnapshot { SoilTileStableId = "soil:one", StateCode = "Tilled" } },
        CultivationUnits = new[] { new Simulation재배단위Snapshot { CultivationUnitStableId = "crop:one", TileStableId = "soil:one",
            Revision = 2, ProductStableId = "product:potato", StateCode = state } },
        HarvestLots = state == "Harvested" ? new[] { new Simulation수확LotSnapshot { HarvestLotStableId = "lot:one",
            Revision = 1, CultivationUnitStableId = "crop:one", ProductStableId = "product:potato", Quantity = 12.375m,
            UnitCode = "kg", StateCode = "HarvestedAtField", CausedByTaskStableId = "task:harvest" } }
            : Array.Empty<Simulation수확LotSnapshot>()
    };
}
