using System.Globalization;
using System.Text.Json;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.Farm;
using Ssalddel.Unity.PresentationContracts;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "한 재배의 동결 시각 후보·상태·판본과 기존 연결 사전검사 소비를 시험한다.",
    WorldInteractionIds = new[] { "WI-FARM-04" },
    Boundary = "순수 Fixture는 실제 자산조회·Session·Scene·E5 또는 게임 실행 증거가 아니다.")]
public sealed class Farm수확시각후보PreparationTests
{
    [Theory]
    [InlineData("Growing", "potato-s", "CropGrowing", "farm.crop.grow")]
    [InlineData("HarvestReady", "potato-l", "CropGrowing", "farm.crop.grow")]
    [InlineData("Harvested", "box-potato", "CropHarvested", "farm.crop.harvest")]
    public void 세권위상태를_정확동결후보와_기존표현키로_결속한다(string code, string family, string presentation, string visualKey)
    {
        var state = State(code);
        var input = Candidate(code);
        Assert.True(Farm수확시각후보Preparation.TryPrepare(state, new[] { input }, out var result, out var reason));
        Assert.Equal("Prepared_NotAssetLookup", reason);
        Assert.Same(state, result!.Source);
        Assert.Same(input, result.Candidate);
        Assert.Equal("synty-family:farm:plants:" + family, result.Candidate.FamilyId);
        Assert.Equal(presentation, result.PresentationStateCode);
        Assert.Equal(visualKey, result.VisualKey);
        Assert.Equal(state.PresentationRevision, result.SourcePresentationRevision);
        Assert.Equal(4, result.SourceWorldRevision);
        Assert.Equal(Farm수확시각후보Preparation.Revision, result.CandidateRevision);
        Assert.Equal("E5Unlinked", result.SceneBindingStatus);
        Assert.False(result.CatalogLookupVerified);
        Assert.False(result.AssetLookupVerified);
        Assert.False(result.CanConfirmAuthority);
        Assert.Equal(64, result.CandidateFingerprint.Length);
        Assert.Equal(64, result.BindingFingerprint.Length);
    }

    [Theory]
    [InlineData("path")]
    [InlineData("guid")]
    [InlineData("file")]
    [InlineData("meta")]
    [InlineData("family")]
    [InlineData("revision")]
    public void 명시후보필드_drift는_자동대체없이_거부한다(string fault)
    {
        var valid = Candidate("HarvestReady");
        var invalid = Copy(valid, fault);
        var state = State();
        var before = JsonSerializer.Serialize(new { state, invalid });
        Assert.False(Farm수확시각후보Preparation.TryPrepare(state, new[] { invalid }, out var result, out var reason));
        Assert.Null(result);
        Assert.Equal("FarmVisualCandidateDrift", reason);
        Assert.Equal(before, JsonSerializer.Serialize(new { state, invalid }));
    }

    [Theory]
    [InlineData("none", "FarmVisualCandidatesMissing")]
    [InlineData("empty", "FarmVisualCandidateMissingForState")]
    [InlineData("null-item", "FarmVisualCandidateNull")]
    [InlineData("duplicate", "FarmVisualCandidateDuplicate")]
    [InlineData("other-state", "FarmVisualCandidateMissingForState")]
    [InlineData("unknown-state", "FarmVisualCandidateStateUnsupported")]
    public void 미확보와_중복과_미지원후보를_명시거부한다(string fault, string expected)
    {
        var candidate = Candidate("HarvestReady");
        Farm수확시각후보Reference[]? inputs = fault switch
        {
            "none" => null,
            "empty" => Array.Empty<Farm수확시각후보Reference>(),
            "null-item" => new Farm수확시각후보Reference[] { null! },
            "duplicate" => new[] { candidate, candidate },
            "other-state" => new[] { Candidate("Growing") },
            _ => new[] { Copy(candidate, "state") }
        };
        Assert.False(Farm수확시각후보Preparation.TryPrepare(State(), inputs, out var result, out var reason));
        Assert.Null(result);
        Assert.Equal(expected, reason);
    }

    [Fact]
    public void 상태미확보와_다른제품을_감자로_간주하지않는다()
    {
        Assert.False(Farm수확시각후보Preparation.TryPrepare(null, new[] { Candidate("HarvestReady") }, out _, out var missing));
        Assert.Equal("FarmSnapshotMissing_E5Unlinked", missing);
        Assert.False(Farm수확시각후보Preparation.TryPrepare(State(product: "product:other"),
            new[] { Candidate("HarvestReady") }, out _, out var unsupported));
        Assert.Equal("FarmVisualProductUnsupported", unsupported);
    }

    [Fact]
    public void 미지원권위상태는_기존준비소비자에서_거부된다()
    {
        var source = Snapshot("invented");
        Assert.False(new Farm수확상태PresentationPreparation("session:farm", "farm-rule.r1", "soil:one", "crop:one")
            .TryPrepare(source, out var state, out var reason));
        Assert.Null(state);
        Assert.Equal("FarmCultivationStateUnsupported", reason);
    }

    [Fact]
    public void 입력순서와_문화권에_결정적이며_실패가_직전불변결과를_바꾸지않는다()
    {
        var state = State();
        var values = new[] { Candidate("Growing"), Candidate("HarvestReady"), Candidate("Harvested") };
        var before = JsonSerializer.Serialize(new { state, values });
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.True(Farm수확시각후보Preparation.TryPrepare(state, values, out var first, out _));
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.True(Farm수확시각후보Preparation.TryPrepare(state, values.Reverse(), out var second, out _));
            Assert.Equal(first!.BindingFingerprint, second!.BindingFingerprint);
            var resultBefore = JsonSerializer.Serialize(first);
            Assert.False(Farm수확시각후보Preparation.TryPrepare(state, Array.Empty<Farm수확시각후보Reference>(), out _, out _));
            Assert.Equal(resultBefore, JsonSerializer.Serialize(first));
            Assert.Equal(before, JsonSerializer.Serialize(new { state, values }));
        }
        finally { CultureInfo.CurrentCulture = culture; }
    }

    [Theory]
    [InlineData("session")]
    [InlineData("target")]
    [InlineData("revision")]
    [InlineData("soil")]
    [InlineData("rule")]
    [InlineData("state")]
    public void 다른상태에_이전후보결과를_재사용하면_차단한다(string change)
    {
        var original = State();
        var candidate = Prepare(original);
        var changed = State(code: change == "state" ? "Growing" : "HarvestReady",
            session: change == "session" ? "session:other" : "session:farm",
            crop: change == "target" ? "crop:other" : "crop:one",
            revision: change == "revision" ? 5 : 4,
            soil: change == "soil" ? "soil:other" : "soil:one",
            rule: change == "rule" ? "different.r1" : "farm-rule.r1");
        var plan = Plan(changed, candidate);
        var result = Farm수확표현연결Preflight.ReviewVisualCandidate(changed, candidate, plan, Observations(plan));
        Assert.Equal(표현연결Readiness.Blocked, result.Readiness);
        Assert.Contains(result.Checks, x => x.Code == "FarmVisualCandidateBindingMismatch");
    }

    [Theory]
    [InlineData(표현연결항목.CandidatePath)]
    [InlineData(표현연결항목.CandidateFingerprint)]
    [InlineData(표현연결항목.VisualKey)]
    public void 기대계획과_후보가_다르면_관측문자열일치만으로_통과하지않는다(표현연결항목 mismatch)
    {
        var state = State(); var candidate = Prepare(state);
        var valid = Plan(state, candidate);
        var plan = new 표현연결Plan(valid.PreparationRevision, valid.Requirements.Select(x =>
            new 표현연결Requirement(x.Item, x.Key, x.Item == mismatch ? "other" : x.ExpectedValue)));
        var result = Farm수확표현연결Preflight.ReviewVisualCandidate(state, candidate, plan, Observations(plan));
        Assert.Equal(표현연결Readiness.Blocked, result.Readiness);
        Assert.Contains(result.Checks, x => x.Item == mismatch && x.Code == "FarmVisualCandidateBindingMismatch");
    }

    [Fact]
    public void E5미관측과_필수컴포넌트결손은_후보준비로_우회하지않는다()
    {
        var state = State(); var candidate = Prepare(state); var plan = Plan(state, candidate);
        var unobserved = Farm수확표현연결Preflight.ReviewVisualCandidate(state, candidate, plan, Observations(plan));
        Assert.Equal(표현연결Readiness.Conditional, unobserved.Readiness);
        Assert.Contains(unobserved.Checks, x => x.Code == "FarmLogicE5EvidenceMissing");
        Assert.False(unobserved.IsE5Completion);
        var missing = new 표현연결관측Snapshot(plan.ContextFingerprint, Observations(plan).Observations.Select(x =>
            x.Item == 표현연결항목.Component ? new 표현연결Observation(x.Item, x.Key, 표현연결ObservationStatus.Missing,
                "", "fixture:missing-component", new string('b', 64)) : x));
        var blocked = Farm수확표현연결Preflight.ReviewVisualCandidate(state, candidate, plan, missing);
        Assert.Equal(표현연결Readiness.Blocked, blocked.Readiness);
        Assert.Contains(blocked.Checks, x => x.Item == 표현연결항목.Component && x.Readiness == 표현연결Readiness.Blocked);
    }

    [Fact]
    public void 후보없음은_조건부이고_기존Review_null호출계약은_유지된다()
    {
        var legacy = Farm수확표현연결Preflight.Review(null, null, null);
        var current = Farm수확표현연결Preflight.ReviewVisualCandidate(null, null, null, null);
        Assert.Equal(표현연결Readiness.Conditional, current.Readiness);
        Assert.Contains(current.Checks, x => x.Code == "FarmVisualCandidateNotPrepared");
        foreach (var check in legacy.Checks) Assert.Contains(current.Checks, x => x.Code == check.Code && x.Item == check.Item);
    }

    private static Farm수확시각후보State Prepare(Farm수확상태PresentationState state)
    {
        Assert.True(Farm수확시각후보Preparation.TryPrepare(state, new[] { Candidate(state.StateCode) }, out var candidate, out _));
        return candidate!;
    }
    private static 표현연결Plan Plan(Farm수확상태PresentationState state, Farm수확시각후보State candidate)
        => new(Farm수확시각후보Preparation.Revision, Enum.GetValues<표현연결항목>().Select(item =>
            new 표현연결Requirement(item, "only", item switch
            {
                표현연결항목.Target => state.CultivationUnitStableId,
                표현연결항목.Session => state.SessionStableId,
                표현연결항목.StateRevision => state.SourceWorldRevision.ToString(CultureInfo.InvariantCulture),
                표현연결항목.PresentationRevision => state.PresentationRevision,
                표현연결항목.PresentationSlot => state.PresentationSlot,
                표현연결항목.StateCode => state.StateCode,
                표현연결항목.CandidatePath => candidate.Candidate.AssetPath,
                표현연결항목.CandidateFingerprint => candidate.CandidateFingerprint,
                표현연결항목.VisualKey => candidate.VisualKey,
                표현연결항목.LogicE5 => "E5",
                _ => item.ToString()
            })));
    private static 표현연결관측Snapshot Observations(표현연결Plan plan)
        => new(plan.ContextFingerprint, plan.Requirements.Select(x => new 표현연결Observation(x.Item, x.Key,
            x.Item == 표현연결항목.LogicE5 ? 표현연결ObservationStatus.Unobserved : 표현연결ObservationStatus.Confirmed,
            x.ExpectedValue, "fixture:read-only", new string('a', 64), true)));

    private static Farm수확상태PresentationState State(string code = "HarvestReady", string product = "product:potato",
        string session = "session:farm", string crop = "crop:one", long revision = 4, string soil = "soil:one", string rule = "farm-rule.r1")
    {
        var source = Snapshot(code); source.SessionStableId = session; source.RuleRevision = rule; source.WorldRevision = revision;
        source.SoilTiles[0].SoilTileStableId = soil; source.CultivationUnits[0].TileStableId = soil;
        source.CultivationUnits[0].CultivationUnitStableId = crop; source.CultivationUnits[0].ProductStableId = product;
        foreach (var lot in source.HarvestLots) { lot.CultivationUnitStableId = crop; lot.ProductStableId = product; }
        Assert.True(new Farm수확상태PresentationPreparation(session, rule, soil, crop).TryPrepare(source, out var state, out _));
        return state!;
    }
    private static SimulationFarmSurvivalStateSnapshot Snapshot(string code) => new()
    {
        SessionStableId = "session:farm", RuleRevision = "farm-rule.r1", WorldRevision = 4, WorldTick = 2,
        SoilTiles = new[] { new SimulationFarmSoilTileSnapshot { SoilTileStableId = "soil:one", StateCode = "Tilled" } },
        CultivationUnits = new[] { new Simulation재배단위Snapshot { CultivationUnitStableId = "crop:one", TileStableId = "soil:one",
            Revision = 2, ProductStableId = "product:potato", StateCode = code } },
        HarvestLots = code == "Harvested" ? new[] { new Simulation수확LotSnapshot { HarvestLotStableId = "lot:one", Revision = 1,
            CultivationUnitStableId = "crop:one", ProductStableId = "product:potato", Quantity = 12.375m, UnitCode = "kg",
            StateCode = "HarvestedAtField", CausedByTaskStableId = "task:harvest" } } : Array.Empty<Simulation수확LotSnapshot>()
    };

    private static Farm수확시각후보Reference Candidate(string code) => code switch
    {
        "Growing" => new(code, "Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_S.prefab",
            "53e5ab917382c9749a58810d6e170537", "2D5093E764F6F66C08EC2C862ECDF250745B66694CE5278622083E3C9FD12912",
            "EE7CDCB2404C218F97697C60B6E1299A307DDE102B9F52C1E54C1EBFE917BD70", "synty-family:farm:plants:potato-s", Farm수확시각후보Preparation.Revision),
        "HarvestReady" => new(code, "Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_L.prefab",
            "e48b8d820d122d64484926ce5e8f6e8c", "FC01F89A96545D8FBA023FCAE7BE54F4EAE5330306A46519D52D6F3C945FF627",
            "D960A73F1FB4EB2A5A55A3F8045C65B7FF0A889771AABECA8CA1085CDBB98703", "synty-family:farm:plants:potato-l", Farm수확시각후보Preparation.Revision),
        "Harvested" => new(code, "Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Box_Potato_01.prefab",
            "2131bc3845099584ebe0cb30614e96f4", "A128993CF0644A5988A537A0196DC69DCD36619538FC499E01ED7F5A3377C583",
            "374ED7092770D129BFC362BAA94068F24CE76EADC5EE3B6AEE29A0498306011B", "synty-family:farm:plants:box-potato", Farm수확시각후보Preparation.Revision),
        _ => throw new ArgumentException("fixture-state")
    };
    private static Farm수확시각후보Reference Copy(Farm수확시각후보Reference value, string change)
        => new(change == "state" ? "invented" : value.StateCode, change == "path" ? "other.prefab" : value.AssetPath,
            change == "guid" ? new string('0', 32) : value.Guid, change == "file" ? new string('0', 64) : value.FileSha256,
            change == "meta" ? new string('0', 64) : value.MetaSha256, change == "family" ? "synty-family:farm:props:crate" : value.FamilyId,
            change == "revision" ? "other.r1" : value.CandidateRevision);
}
