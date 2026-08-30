using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "실측 Farm 부지 확장의 결정성·거부·동결계보와 LH 보존을 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3결정성검증,
    Boundary = "실제 자산/Player 측정 입력과 합성 지지면의 조합이다. 실제 이동·Game View·E 승인 증거가 아니다.")]
public sealed class SimulationFarmH2부지확장Tests
{
    private static string Root => Environment.GetEnvironmentVariable("SSALDDEL_FARM_H2_EXPANDED_FIXTURES")
        ?? Path.Combine(AppContext.BaseDirectory, "Fixtures", "FarmH2ExpandedR2");
    private static string Hash(string value) => Simulation세계자산CanonicalHash.Hash(value);

    [Theory]
    [InlineData(0, "Barn01")] [InlineData(1, "Barn01")] [InlineData(2, "Barn01")]
    [InlineData(0, "Barn02")] [InlineData(1, "Barn02")] [InlineData(2, "Barn02")]
    public void 실측native모델과외부접근띠는_평지노이즈단차에들어간다(int fixture, string barn)
    {
        var r = Input(fixture, barn); var before = JsonSerializer.Serialize(r);
        var result = new SimulationFarmH2부지확장Service().ExpandAndValidate(r, new Surface(r.ParentRequest));
        var actual = result.ValidatedPlacement!;
        Assert.False(actual.ContainsSyntheticMeasurements); Assert.True(actual.PolicyIsTrial);
        Assert.False(actual.ActualTraversalVerified); Assert.False(result.ActualTraversalVerified);
        Assert.Equal("UnapprovedCandidate", actual.PatternStatusCode);
        Assert.Equal(SimulationFarmH2부지확장Service.Revision, actual.CandidatePatternRevision);
        Assert.Equal(before, JsonSerializer.Serialize(r));
        Assert.All(actual.Plan.Placements, p => Assert.Equal(1, p.UniformScale));
        Assert.Equal(12, actual.Anchors.Length); Assert.Equal(12, actual.Routes.Length);
        Assert.Equal(3, result.AccessLaneWidthMeters);
        Assert.InRange(result.PlayerCenterClearanceMeters, .5999, .6001);
        Assert.True(result.ReservationWidthMeters > 20.59);
        Assert.True(result.ReservationDepthMeters > (barn == "Barn01" ? 21.25 : 14.54));
        Assert.Empty(actual.Plan.InteriorPlanBodies); Assert.Empty(actual.Plan.InteriorPlanHandles);
        Assert.Equal(37, actual.Plan.SourceWorldRevision);
        Assert.All(actual.Plan.Placements, p => Assert.Empty(p.SourceChangeStableIds));
        using var parent = JsonDocument.Parse(r.ParentRequest.CandidateJson);
        using var next = JsonDocument.Parse(result.CandidateRequest.CandidateJson);
        Assert.Equal(parent.RootElement.GetProperty("SurfaceHash").GetString(), next.RootElement.GetProperty("SurfaceHash").GetString());
        Assert.Equal(JsonSerializer.Serialize(parent.RootElement.GetProperty("Input").GetProperty("Terrain")),
            JsonSerializer.Serialize(next.RootElement.GetProperty("Input").GetProperty("Terrain")));
        Assert.Equal(100, next.RootElement.GetProperty("Input").GetProperty("Expansion").GetProperty("ProductionAreaSquareMeters").GetInt32());
        var protectedBefore = parent.RootElement.GetProperty("PreservedAreas").EnumerateArray().Where(x => x.GetProperty("Role").GetString() != "WorkYard").Select(x => JsonSerializer.Serialize(x));
        var protectedAfter = next.RootElement.GetProperty("PreservedAreas").EnumerateArray().Where(x => x.GetProperty("Role").GetString() != "WorkYard").Select(x => JsonSerializer.Serialize(x));
        Assert.Equal(protectedBefore, protectedAfter);
        Assert.Equal(r.ParentRequest.MapPlan.MapPlanHashSha256, actual.Plan.MapPlanHashSha256);
        Assert.Equal(r.ParentRequest.Bindings.Select(x => x.H1StableId).Order(), actual.Plan.Placements.Select(x => x.H1StableId).Order());
    }

    [Theory]
    [InlineData(0, "Barn01")] [InlineData(1, "Barn01")] [InlineData(2, "Barn01")]
    [InlineData(0, "Barn02")] [InlineData(1, "Barn02")] [InlineData(2, "Barn02")]
    public void 동결후보파일은_재생성결과와원후보계보가같다(int fixture, string barn)
    {
        var json = Preview(fixture, barn);
        var file = File.ReadAllText(Path.Combine(Root, $"0{fixture + 1}-{barn}.plan.json"));
        Assert.Equal(JsonNode.Parse(json)!.ToJsonString(), JsonNode.Parse(file)!.ToJsonString());
        var parsed = JsonNode.Parse(file)!;
        Assert.Equal(parsed["ResultHash"]!.GetValue<string>(), SimulationFarmH2부지확장Service.HashCandidate(file));
        Assert.Equal(Input(fixture, barn).ParentRequest.ExpectedCandidateHashSha256,
            parsed["Input"]!["Expansion"]!["ParentCandidateHashSha256"]!.GetValue<string>());
    }

    [Fact]
    public void 재호출_입력순서_문화권을바꿔도_hash는같다()
    {
        var r = Input(1); var service = new SimulationFarmH2부지확장Service();
        var first = service.ExpandAndValidate(r, new Surface(r.ParentRequest));
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Array.Reverse(r.ParentRequest.Bindings); Array.Reverse(r.ParentRequest.MapPlan.HBindings); Array.Reverse(r.ParentRequest.MapPlan.Anchors);
            var again = service.ExpandAndValidate(r, new Surface(r.ParentRequest));
            Assert.Equal(first.CandidateRequest.ExpectedCandidateHashSha256, again.CandidateRequest.ExpectedCandidateHashSha256);
            Assert.Equal(first.ValidatedPlacement!.ConversionOutputHashSha256, again.ValidatedPlacement!.ConversionOutputHashSha256);
        }
        finally { CultureInfo.CurrentCulture = old; }
    }

    [Theory]
    [InlineData(0)] [InlineData(90)] [InlineData(180)] [InlineData(270)]
    public void 셀회전과pivot변환_뒤에도_LH는재추첨하지않는다(double yaw)
    {
        var r = Input(2); r.ParentRequest.RotationDegrees = yaw;
        var result = new SimulationFarmH2부지확장Service().ExpandAndValidate(r, new Surface(r.ParentRequest));
        var plan = result.ValidatedPlacement!;
        var before = JsonSerializer.Serialize(result);
        var parts = new SimulationFarmH2PlacementAdapter().PartitionFrozen(plan);
        var frozen = JsonSerializer.Serialize(parts); var lh = new SimulationLhAssetPlanLifecycleService(); var state = lh.Prepare(parts);
        foreach (var next in new[] { "Active", "Cached", "Active", "Released", "Prepared", "Active" })
        {
            state = lh.Transition(state, new() { ExpectedLifecycleRevision = state.LifecycleRevision, TargetStateCode = next });
            Assert.Equal(plan.Plan.AssetPlacementPlanHashSha256, state.SourceCombinedPlanHashSha256);
            Assert.Equal(37, state.SourceWorldRevision); Assert.Equal(frozen, JsonSerializer.Serialize(parts));
        }
        Assert.Equal(before, JsonSerializer.Serialize(result));
        Assert.Equal(5, plan.Plan.Placements.Length);
        var barn = plan.Plan.Placements.Single(p => p.PlacementStableId.EndsWith("/barn"));
        Assert.Equal(yaw, barn.RotationDegrees);
        Assert.NotEqual(-10, barn.LocalXMeters);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void 구후보는_실측nativeBarn외곽을계속거부한다(int fixture)
    {
        var r = Input(fixture);
        Assert.Contains("MeasuredEnvelopeExceedsCandidate", Assert.Throws<ArgumentException>(() =>
            new SimulationFarmH2PlacementAdapter().Convert(r.ParentRequest, new Surface(r.ParentRequest))).Message);
    }

    [Theory]
    [InlineData("study", "MeasuredProvenanceMissing")]
    [InlineData("player", "MeasuredProvenanceMissing")]
    [InlineData("scale", "NativeScaleRequired")]
    [InlineData("player-scale", "NativeScaleRequired")]
    [InlineData("hash", "ParentCandidateHashMismatch")]
    [InlineData("measurement", "MeasuredAssetRequired")]
    [InlineData("solid", "SolidMeasurementHashMismatch")]
    [InlineData("barn", "BarnMeasurementBindingMismatch")]
    public void 근거누락과크기변조를거부한다(string mode, string reason)
    {
        var r = Input(0);
        switch (mode)
        {
            case "study": r.StudyHashSha256 = Hash("other"); break;
            case "player": r.Player.EvidenceKindCode = "SyntheticFixture"; break;
            case "scale": r.ParentRequest.UniformScale = .7; break;
            case "player-scale": r.Player.UniformScale = .7; break;
            case "hash": r.ParentRequest.ExpectedCandidateHashSha256 = Hash("bad"); break;
            case "measurement": r.ParentRequest.Bindings[0].Measurement.SizeX += 1; break;
            case "solid": r.BarnSolidBounds.MaxX += 1; break;
            case "barn": r.BarnCode = "Barn02"; break;
        }
        Assert.Contains(reason, Assert.Throws<ArgumentException>(() => new SimulationFarmH2부지확장Service().CreateCandidate(r, new Surface(r.ParentRequest))).Message);
    }

    [Theory]
    [InlineData("steep", "SlopeTooSteep")]
    [InlineData("missing", "SurfaceSupportMissingOrDenied")]
    [InlineData("spread", "HeightSpreadExceeded")]
    [InlineData("route-hole", "SurfaceSupportMissingOrDenied")]
    public void 지지면과통로의실패를_평탄화하지않고거부한다(string mode, string reason)
    {
        var r = Input(0);
        Assert.Contains(reason, Assert.Throws<ArgumentException>(() => new SimulationFarmH2부지확장Service().ExpandAndValidate(r, new Surface(r.ParentRequest, mode))).Message);
    }

    [Theory]
    [InlineData("preserved", "AccessReservationPreservedAreaIntrusion")]
    [InlineData("obstacle", "AccessReservationObstacle")]
    [InlineData("route-block", "RouteObstacle")]
    public void 넓어진부지가보호영역이나통로를침범하면거부한다(string mode, string reason)
    {
        var r = Input(0); var node = JsonNode.Parse(r.ParentRequest.CandidateJson)!;
        if (mode == "preserved") node["PreservedAreas"]![1]!["Bounds"]!["MaxX"] = -22;
        else node["Input"]!["Obstacles"]!.AsArray().Add(JsonSerializer.SerializeToNode(new { MinX = mode == "route-block" ? -.2 : -22, MinZ = -13, MaxX = mode == "route-block" ? .2 : -21, MaxZ = -11 }));
        node["InputHash"] = Hash(JsonSerializer.Serialize(node["Input"]));
        node["ResultHash"] = SimulationFarmH2부지확장Service.HashCandidate(node.ToJsonString());
        r.ParentRequest.CandidateJson = node.ToJsonString(); r.ParentRequest.ExpectedCandidateHashSha256 = node["ResultHash"]!.GetValue<string>();
        Assert.Contains(reason, Assert.Throws<ArgumentException>(() => new SimulationFarmH2부지확장Service().ExpandAndValidate(r, new Surface(r.ParentRequest))).Message);
    }

    [Fact]
    public void 급경사원후보의거부기록을지우지않는다()
    {
        var r = Input(3);
        Assert.Contains("ParentCandidateRejected", Assert.Throws<ArgumentException>(() => new SimulationFarmH2부지확장Service().CreateCandidate(r, new Surface(r.ParentRequest))).Message);
    }

    [Fact]
    public void 새공간계약과실행은_E책임을명시한다()
    {
        var errors = SsalddelEvidenceCoverageValidator.Validate(true, typeof(SimulationFarmH2부지확장Service).Assembly, typeof(SimulationFarmH2부지확장Request).Assembly);
        Assert.DoesNotContain(errors, x => x.ComponentId.Contains("SimulationFarmH2부지확장"));
    }

    // 동결 파일 생성 시 호출하되 시험 자체는 파일을 쓰지 않는다.
    public static string Preview(int fixture, string barn) { var r = Input(fixture, barn); return new SimulationFarmH2부지확장Service().CreateCandidate(r, new Surface(r.ParentRequest)).CandidateRequest.CandidateJson; }

    public static SimulationFarmH2부지확장Request Input(int fixture, string barn = "Barn01")
    {
        var factory = typeof(SimulationFarmH2PlacementAdapterTests).GetMethod("Request", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ExistingFixtureFactoryChanged");
        var parent = (SimulationFarmH2PlacementRequest)factory.Invoke(null, new object[] { fixture })!;
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "measured-assets.json")));
        JsonElement Record(string file) => assets.RootElement.GetProperty("records").EnumerateArray().Single(x => x.GetProperty("path").GetString()!.EndsWith(file, StringComparison.Ordinal));
        foreach (var b in parent.Bindings)
        {
            var file = b.VisualKey switch { "candidate:farm.red-barn" => barn == "Barn01" ? "SM_Bld_Barn_01.prefab" : "SM_Bld_Barn_02.prefab",
                "candidate:farm.crop-plot" => "SM_Env_Dirt_Rows_01.prefab", "candidate:farm.water-access" => "SM_Prop_Well_01.prefab",
                "candidate:farm.grass-accent" => "SM_Generic_Grass_Patch_01.prefab", _ => throw new InvalidOperationException("UnknownVisual") };
            b.Measurement = JsonSerializer.Deserialize<SimulationFarmH2AssetMeasurement>(Record(file).GetProperty("adapterMeasurementUnsealed"))!;
            b.Measurement.MeasurementHashSha256 = SimulationFarmH2PlacementAdapter.ComputeMeasurementHash(b.Measurement);
        }
        var playerJson = File.ReadAllText(Path.Combine(Root, "player-readonly-evidence.json"));
        using var evidence = JsonDocument.Parse(playerJson);
        using var player = JsonDocument.Parse(evidence.RootElement.GetProperty("colliderJson").GetString()!);
        using var movement = JsonDocument.Parse(evidence.RootElement.GetProperty("movementJson").GetString()!);
        var cc = player.RootElement.GetProperty("CharacterController");
        var selected = Record(barn == "Barn01" ? "SM_Bld_Barn_01.prefab" : "SM_Bld_Barn_02.prefab").GetProperty("localMeasurement");
        var boxes = selected.GetProperty("colliders").EnumerateArray().Where(x => x.GetProperty("enabled").GetBoolean() && !x.GetProperty("isTrigger").GetBoolean())
            .Select(x => x.GetProperty("wrapperAabb")).Append(selected.GetProperty("visibleBounds")).ToArray();
        var solid = new SimulationFarmH2외곽측정 { MinX = boxes.Min(x => x.GetProperty("min")[0].GetDouble()), MinZ = boxes.Min(x => x.GetProperty("min")[2].GetDouble()),
            MaxX = boxes.Max(x => x.GetProperty("max")[0].GetDouble()), MaxZ = boxes.Max(x => x.GetProperty("max")[2].GetDouble()) };
        parent.SurfaceRevision = "explicit-synthetic-analytic-ground.r2";
        parent.SurfaceHashSha256 = Hash(parent.SurfaceRevision + ":" + fixture);
        return new() {
            ParentRequest = parent, StudyHashSha256 = SimulationFarmH2부지확장Service.StudyHash,
            MeasurementSourceHashSha256 = assets.RootElement.GetProperty("sourceSha256").GetString()!, BarnCode = barn, BarnSolidBounds = solid,
            BarnSolidMeasurementHashSha256 = SimulationFarmH2부지확장Service.HashSolidMeasurement(solid, parent.Bindings.Single(x => x.VisualKey == "candidate:farm.red-barn").Measurement.AssetFingerprintSha256),
            Player = new() { EvidenceKindCode = "ReadOnlyOpenDirtyEditor", EvidenceRef = "Fixtures/FarmH2ExpandedR2/player-readonly-evidence.json",
                EvidenceHashSha256 = Hash(playerJson), RadiusMeters = cc.GetProperty("m_Radius").GetDouble(), SkinWidthMeters = cc.GetProperty("m_SkinWidth").GetDouble(),
                HeightMeters = cc.GetProperty("m_Height").GetDouble(), StepOffsetMeters = cc.GetProperty("m_StepOffset").GetDouble(), SlopeLimitDegrees = cc.GetProperty("m_SlopeLimit").GetDouble(),
                ClickStopDistanceMeters = movement.RootElement.GetProperty("MonoBehaviour").GetProperty("profile").GetProperty("ClickMoveStopDistance").GetDouble(), UniformScale = 1 }
        };
    }

    private sealed class Surface : ISimulationFarmH2SurfaceReader
    {
        private readonly SimulationFarmH2PlacementRequest r; private readonly string mode; private readonly JsonElement terrain;
        public Surface(SimulationFarmH2PlacementRequest r, string mode = "normal") { this.r = r; this.mode = mode; using var doc = JsonDocument.Parse(r.CandidateJson); terrain = doc.RootElement.GetProperty("Input").GetProperty("Terrain").Clone(); }
        public string Revision => r.SurfaceRevision; public string HashSha256 => r.SurfaceHashSha256;
        public SimulationFarmH2SurfaceSample Read(double wx, double wz)
        {
            var x = wx - r.CellWorldOriginXMeters - r.LocalOriginXMeters; var z = wz - r.CellWorldOriginZMeters - r.LocalOriginZMeters;
            var a = -r.RotationDegrees * Math.PI / 180; var tx = x * Math.Cos(a) + z * Math.Sin(a); z = -x * Math.Sin(a) + z * Math.Cos(a); x = tx;
            var h = terrain.GetProperty("BaseHeight").GetDouble(); var kind = terrain.GetProperty("Mode").GetString();
            if (kind == "Noise") h += terrain.GetProperty("Amplitude").GetDouble() * (Math.Sin(x / 32) + Math.Sin(z / 32)) / 2;
            if (kind == "Terrace" && z >= 0) h += terrain.GetProperty("StepHeight").GetDouble();
            if (mode == "spread" && x < -10) h += 1;
            return new() { Supported = mode != "missing" && !(mode == "route-hole" && Math.Abs(x) < 1 && Math.Abs(z) < 1), PlacementAllowed = true,
                HeightMeters = r.CellWorldOriginYMeters + r.LocalOriginYMeters + h, SlopeDegrees = mode == "steep" ? 30 : kind == "Noise" ? .2 : 0 };
        }
    }
}
