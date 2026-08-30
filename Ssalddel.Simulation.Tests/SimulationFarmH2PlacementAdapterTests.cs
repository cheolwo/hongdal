using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Farm H2 호환 변환의 결정성·입력 거부·동결 분리/LH 수명주기 불변을 검증한다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E3결정성검증,
    Boundary = "합성 측정 시험은 실제 Prefab·Player 통행·Game View 증거가 아니다.")]
public sealed class SimulationFarmH2PlacementAdapterTests
{
    private static readonly string[] Hashes =
    {
        "7c3e72b8e937b7a2987b34e4aa8456e889b060340f4e5bc19cf3f1b9aff43bf3",
        "8ff572c8b12b50484e7df60de7075ef6a50c85db3ed28f6d6e83b9bfa857421b",
        "370d3df9e4d800a460111a51a4e828ff88fa60e3d4f9f6386855deba7dd0cd9c",
        "c161aa86c33e7ade19229f206cf6143a4a72f96c8bba1640140bb30d3ded0e70"
    };
    private static string Hash(string text) => Simulation세계자산CanonicalHash.Hash(text);
    private static SimulationFarmH2PlacementResult Convert(SimulationFarmH2PlacementRequest r) => new SimulationFarmH2PlacementAdapter().Convert(r, new SyntheticSurface(r));

    [Fact]
    public void 공간변환기의계약과실행은_E책임누락이없다()
    {
        var errors = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceCoverageValidator.Validate(true,
            typeof(SimulationFarmH2PlacementAdapter).Assembly, typeof(SimulationFarmH2PlacementRequest).Assembly);
        Assert.DoesNotContain(errors, e => e.ComponentId.Contains("SimulationFarmH2", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void 원본평지_노이즈_단차후보를_재생성없이현행형식으로변환한다(int fixture)
    {
        var r = Request(fixture); var before = JsonSerializer.Serialize(r);
        var result = Convert(r); var again = Convert(r);
        Assert.Equal(Hashes[fixture], result.CandidateHashSha256);
        Assert.Equal(before, JsonSerializer.Serialize(r));
        Assert.Equal(result.ConversionOutputHashSha256, again.ConversionOutputHashSha256);
        Assert.NotEqual(result.CandidateHashSha256, result.Plan.AssetPlacementPlanHashSha256);
        Assert.Equal(Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(result.Plan), result.Plan.AssetPlacementPlanHashSha256);
        Assert.Equal(37, result.Plan.SourceWorldRevision);
        Assert.Equal(5, result.Plan.Placements.Length);
        Assert.Equal(7, result.Anchors.Length); Assert.Equal(6, result.Routes.Length); Assert.Equal(4, result.ReservedAreas.Length);
        Assert.True(result.ContainsSyntheticMeasurements); Assert.True(result.PolicyIsTrial);
        Assert.False(result.ActualTraversalVerified); Assert.Equal("UnapprovedCandidate", result.PatternStatusCode);
        Assert.Empty(result.Plan.InteriorPlanBodies); Assert.Empty(result.Plan.InteriorPlanHandles);
        Assert.Empty(result.Plan.ChangeProjectionHashSha256); Assert.Empty(result.Plan.SpawnDecisionPlanHashSha256);
        Assert.All(result.Plan.Placements, p => { Assert.True(p.PresentationOnly); Assert.Empty(p.SourceChangeStableIds); });
    }

    [Fact]
    public void 원본급경사거부후보는_계획을만들지않는다()
    {
        var r = Request(3);
        var ex = Assert.Throws<ArgumentException>(() => Convert(r));
        Assert.Contains("CandidateRejected:", ex.Message); Assert.Contains("SlopeTooSteep", ex.Message);
    }

    [Fact]
    public void 배열순서와문화권이달라도_정규형인계는같다()
    {
        var r = Request(0); var expected = Convert(r); var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Array.Reverse(r.Bindings); Array.Reverse(r.ResolvedCompositionKeys); Array.Reverse(r.MapPlan.HBindings); Array.Reverse(r.MapPlan.Anchors);
            var actual = Convert(r);
            Assert.Equal(expected.ConversionInputHashSha256, actual.ConversionInputHashSha256);
            Assert.Equal(expected.Plan.AssetPlacementPlanHashSha256, actual.Plan.AssetPlacementPlanHashSha256);
            Assert.Equal(expected.ConversionOutputHashSha256, actual.ConversionOutputHashSha256);
        }
        finally { CultureInfo.CurrentCulture = culture; }
    }

    [Theory]
    [InlineData(0)] [InlineData(90)] [InlineData(180)] [InlineData(270)]
    public void 측정pivot과셀원점을_명시적으로변환한다(double rotation)
    {
        var r = Request(0); r.RotationDegrees = rotation; r.LocalOriginXMeters = 3; r.LocalOriginZMeters = 4;
        var result = Convert(r);
        var barn = result.Plan.Placements.Single(x => x.PlacementStableId.EndsWith("/barn", StringComparison.Ordinal));
        Assert.Equal(rotation, barn.RotationDegrees);
        Assert.Equal(-.22, barn.LocalYMeters, 10); // bottom .03 - (centerY 1.25 - height2/2)
        if (rotation == 0) { Assert.Equal(-8.25, barn.LocalXMeters, 10); Assert.Equal(-7.25, barn.LocalZMeters, 10); }
        Assert.Equal(r.OwnerCellStableId, barn.OwnerCellStableId);
        Assert.Equal("h1:farm:BarnWorkYard", barn.H1StableId);
        Assert.NotEqual(-10, barn.LocalXMeters); // AABB 중심을 pivot으로 쓰지 않는다.
    }

    [Fact]
    public void Partition과LH수명주기는_Seed와배치와권위Revision을재추첨하지않는다()
    {
        var result = Convert(Request(0)); var before = JsonSerializer.Serialize(result);
        var adapter = new SimulationFarmH2PlacementAdapter();
        var parts = adapter.PartitionFrozen(result); var frozen = JsonSerializer.Serialize(parts);
        var lh = new SimulationLhAssetPlanLifecycleService(); var state = lh.Prepare(parts);
        foreach (var target in new[] { "Active", "Cached", "Active", "Released", "Prepared", "Active" })
        {
            state = lh.Transition(state, new() { ExpectedLifecycleRevision = state.LifecycleRevision, TargetStateCode = target });
            Assert.Equal(result.Plan.AssetPlacementPlanHashSha256, state.SourceCombinedPlanHashSha256);
            Assert.Equal(result.Plan.SourceWorldRevision, state.SourceWorldRevision);
            Assert.Equal(frozen, JsonSerializer.Serialize(parts));
        }
        Assert.Equal(before, JsonSerializer.Serialize(result));
        var again = adapter.PartitionFrozen(result);
        Assert.Equal(parts.ExteriorPlan.ExteriorPlacementPlanHashSha256, again.ExteriorPlan.ExteriorPlacementPlanHashSha256);
        Assert.Equal(5, again.ExteriorPlan.Placements.Select(x => x.PlacementStableId).Distinct().Count());
        result.Plan.Placements[0].LocalXMeters += 1;
        Assert.Equal(frozen, JsonSerializer.Serialize(parts)); // 기존 Partition alias가 원본으로 새지 않는다.
        Assert.Throws<ArgumentException>(() => adapter.PartitionFrozen(result));
    }

    [Theory]
    [InlineData("unknown-key", "UnknownVisualOrCompositionKey")]
    [InlineData("unknown-visual", "UnknownVisualOrCompositionKey")]
    [InlineData("bad-production-key", "ProductionCompositionNotAllowed")]
    [InlineData("h1", "H1OwnershipMissing")]
    [InlineData("h2", "CellOrHOwnershipMissing")]
    [InlineData("cell", "CellOrHOwnershipMissing")]
    [InlineData("map-hash", "MapHashMismatch")]
    [InlineData("candidate-hash", "CandidateHashMismatch")]
    [InlineData("candidate-body", "CandidateHashMismatch")]
    [InlineData("measurement-hash", "MeasurementHashMismatch")]
    [InlineData("missing-measurement", "AssetMeasurementMissingOrInvalid")]
    [InlineData("unit", "CoordinateFrameInvalid")]
    [InlineData("axis", "CoordinateFrameInvalid")]
    [InlineData("rotation", "CoordinateFrameInvalid")]
    [InlineData("nan", "CoordinateFrameInvalid")]
    [InlineData("cell-bounds", "OutsideOwnerCell")]
    [InlineData("binding-duplicate", "BindingIdentityInvalid")]
    [InlineData("binding-missing", "BindingCoverageInvalid")]
    [InlineData("bounds", "MeasuredEnvelopeExceedsCandidate")]
    [InlineData("work-area", "ProductionSeedbedBoundsInvalid")]
    public void 손상입력과소유누락은_명시적으로거부한다(string mutation, string code)
    {
        var r = Request(0); var field = r.Bindings.Single(x => x.VisualKey == "candidate:farm.crop-plot");
        switch (mutation)
        {
            case "unknown-key": r.Bindings[0].CompositionKey = "unknown:key"; break;
            case "unknown-visual": r.Bindings[0].VisualKey = "candidate:unknown"; break;
            case "bad-production-key": field.CompositionKey = r.Bindings[0].CompositionKey; break;
            case "h1": r.Bindings[0].H1StableId = ""; break;
            case "h2": r.H2StableId = ""; break;
            case "cell": r.OwnerCellStableId = "other"; break;
            case "map-hash": r.MapPlan.MapPlanHashSha256 = Hash("corrupt"); break;
            case "candidate-hash": r.ExpectedCandidateHashSha256 = Hash("corrupt"); break;
            case "candidate-body": r.CandidateJson = r.CandidateJson.Replace("farm-flat-297", "changed-seed"); break;
            case "measurement-hash": r.Bindings[0].Measurement.CenterX += 1; break;
            case "missing-measurement": r.Bindings[0].Measurement.EvidenceRef = ""; break;
            case "unit": r.UnitCode = "Centimeters"; break;
            case "axis": r.AxisCode = "ZUp"; break;
            case "rotation": r.RotationDegrees = 45; break;
            case "nan": r.LocalOriginXMeters = double.NaN; break;
            case "cell-bounds": r.LocalOriginXMeters = 1000; break;
            case "binding-duplicate": r.Bindings[1].PlacementStableId = r.Bindings[0].PlacementStableId; break;
            case "binding-missing": r.Bindings = r.Bindings.Take(4).ToArray(); break;
            case "bounds": r.Bindings[0].Measurement.SizeX = 500; SealMeasurements(r); break;
            case "work-area": field.WorkAreaWidthMeters = 13; break;
        }
        var ex = Assert.Throws<ArgumentException>(() => Convert(r)); Assert.Contains(code, ex.Message);
    }

    [Theory]
    [InlineData("unsupported", "SurfaceSupportMissingOrDenied")]
    [InlineData("steep", "SlopeTooSteep")]
    [InlineData("spread", "HeightSpreadExceeded")]
    [InlineData("higher", "BuriedBottom")]
    [InlineData("lower", "FloatingBottom")]
    [InlineData("route-hole", "SurfaceSupportMissingOrDenied")]
    [InlineData("route-step", "RouteStepExceeded")]
    public void 측정지형의부적합은_평탄화하지않고거부한다(string mode, string code)
    {
        var r = Request(0); var reader = new SyntheticSurface(r, mode);
        var ex = Assert.Throws<ArgumentException>(() => new SimulationFarmH2PlacementAdapter().Convert(r, reader));
        Assert.Contains(code, ex.Message);
    }

    [Fact]
    public void 후보JSON의공백과줄바꿈은_원후보Hash를바꾸지않는다()
    {
        var r = Request(0); using var doc = JsonDocument.Parse(r.CandidateJson);
        r.CandidateJson = JsonSerializer.Serialize(doc.RootElement);
        Assert.Equal(Hashes[0], Convert(r).CandidateHashSha256);
    }

    [Fact]
    public void 측정fingerprint변경은_별도정규형계보hash를바꾼다()
    {
        var r = Request(0); var before = Convert(r);
        r.Bindings[0].Measurement.AssetFingerprintSha256 = Hash("different-fingerprint"); SealMeasurements(r);
        var after = Convert(r);
        Assert.Equal(before.CandidateHashSha256, after.CandidateHashSha256);
        Assert.NotEqual(before.ConversionInputHashSha256, after.ConversionInputHashSha256);
        Assert.NotEqual(before.ConversionOutputHashSha256, after.ConversionOutputHashSha256);
    }

    [Theory]
    [InlineData("route-missing", "RouteDisconnected")]
    [InlineData("route-blocked", "ProtectedRouteIntrusion")]
    [InlineData("reserved-area", "PreservedAreaIntrusion")]
    [InlineData("source-owner", "SourceH1OwnershipMissing")]
    [InlineData("anchor-owner", "AnchorOwnerMissing")]
    public void 별도동결된수정후보도_통행과소유검사를우회하지못한다(string change, string code)
    {
        var r = Request(0); var root = JsonNode.Parse(r.CandidateJson)!;
        switch (change)
        {
            case "route-missing": root["Routes"]!.AsArray().RemoveAt(5); break;
            case "route-blocked":
                var b = root["Placements"]![3]!["Bounds"]!;
                b["MinX"] = -.5; b["MaxX"] = .5; b["MinZ"] = -.5; b["MaxZ"] = .5;
                break;
            case "reserved-area": root["PreservedAreas"]![0]!["Bounds"] = root["Placements"]![0]!["Bounds"]!.DeepClone(); break;
            case "source-owner": root["Placements"]![0]!["H1StableId"] = ""; break;
            case "anchor-owner": root["Anchors"]![4]!["OwnerStableId"] = "absent-owner"; break;
        }
        root["ResultHash"] = ""; var hash = Hash(JsonSerializer.Serialize(root)); root["ResultHash"] = hash;
        r.CandidateJson = JsonSerializer.Serialize(root); r.ExpectedCandidateHashSha256 = hash;
        Assert.Contains(code, Assert.Throws<ArgumentException>(() => Convert(r)).Message);
    }

    private static SimulationFarmH2PlacementRequest Request(int fixture)
    {
        // 인계 후보의 정규형 hash를 보존한 LF Fixture다. 개인 worktree나 생성기에 의존하지 않는다.
        var root = Environment.GetEnvironmentVariable("SSALDDEL_FARM_H2_FIXTURES")
            ?? Path.Combine(AppContext.BaseDirectory, "Fixtures", "FarmH2");
        var filename = new[] { "01-flat", "02-noise", "03-terrace", "04-rejected-slope" }[fixture] + ".plan.json";
        var json = File.ReadAllText(Path.Combine(root, filename));
        using var doc = JsonDocument.Parse(json);
        var r = new SimulationFarmH2PlacementRequest
        {
            CandidateJson = json, ExpectedCandidateHashSha256 = Hashes[fixture], OwnerCellStableId = "fixture-cell:farm-review:8:5",
            AreaSetStableId = SimulationFarmH2PlacementAdapter.FarmAreaSet, H2StableId = "h2:farm:review-instance",
            CellSizeMeters = 125, CellWorldOriginXMeters = 1000, CellWorldOriginZMeters = 625,
            ResolverRevision = "synthetic-resolver.r1", ResolverHashSha256 = Hash("synthetic-resolver.r1"),
            SurfaceRevision = "synthetic-measured-support.r1", SurfaceHashSha256 = Hash("synthetic-support:" + Hashes[fixture]),
            Policy = new() { Revision = "original-trial-policy.r1", EvidenceRef = "Fixture:explicit-test-values-not-player-rules", TrialOnly = true,
                MaximumSlopeDegrees = 5, MaximumHeightSpreadMeters = .15, GroundClearanceMeters = .03, BottomToleranceMeters = .015,
                MinimumSpacingMeters = .55, MinimumRouteWidthMeters = 3, RouteSampleStepMeters = .5, MaximumRouteSlopeDegrees = 12, MaximumRouteStepMeters = .12 }
        };
        r.Bindings = doc.RootElement.GetProperty("Placements").EnumerateArray().Select(o =>
        {
            var role = o.GetProperty("Role").GetString()!; var bounds = o.GetProperty("Bounds");
            return new SimulationFarmH2PlacementBinding
            {
                SourcePlacementStableId = o.GetProperty("StableId").GetString()!, PlacementStableId = "review:" + o.GetProperty("StableId").GetString(),
                H1StableId = "h1:farm:" + role, VisualKey = o.GetProperty("VisualKey").GetString()!, AssetFamilyId = o.GetProperty("AssetFamilyId").GetString()!,
                CompositionKey = role == "ProductionPlot" ? "farm:감자밭 두렁:A" : role == "BarnWorkYard" ? "farm:헛간 작업마당:A" : "fixture-resolved:" + role,
                WorkAreaWidthMeters = role == "ProductionPlot" ? Math.Max(14, bounds.GetProperty("MaxX").GetDouble() - bounds.GetProperty("MinX").GetDouble()) : 0,
                WorkAreaDepthMeters = role == "ProductionPlot" ? Math.Max(12, bounds.GetProperty("MaxZ").GetDouble() - bounds.GetProperty("MinZ").GetDouble()) : 0,
                WorkAreaEvidenceRef = role == "ProductionPlot" ? "SyntheticFixture:work-area-separate-from-visual-envelope" : "",
                Measurement = new() { Revision = "synthetic-envelope.r1", EvidenceKindCode = "SyntheticFixture", EvidenceRef = "test:non-centered-pivot",
                    AssetFingerprintSha256 = Hash("synthetic:" + role), CenterX = 1.25, CenterY = 1.25, CenterZ = -.75,
                    SizeX = role == "NaturalAccent" ? .5 : bounds.GetProperty("MaxX").GetDouble() - bounds.GetProperty("MinX").GetDouble(),
                    SizeY = 2, SizeZ = role == "NaturalAccent" ? .5 : bounds.GetProperty("MaxZ").GetDouble() - bounds.GetProperty("MinZ").GetDouble(),
                    UniformScale = 1, ActiveRenderer = true, ActiveCollider = role != "NaturalAccent" }
            };
        }).ToArray();
        SealMeasurements(r);
        r.ResolvedCompositionKeys = r.Bindings.Select(x => x.CompositionKey).Distinct().ToArray();
        r.MapPlan = new()
        {
            CellStableId = r.OwnerCellStableId, CellX = 8, CellY = 5, WorldSeed = doc.RootElement.GetProperty("Input").GetProperty("Seed").GetString()!, SourceWorldRevision = 37,
            HBindings = new[] { new Simulation지도H결속Snapshot { HLevelCode = "H4", SpatialStableId = r.AreaSetStableId, StateCode = "ReviewInstance" },
                new Simulation지도H결속Snapshot { HLevelCode = "H2", SpatialStableId = r.H2StableId, StateCode = "ReviewInstance" } }
                .Concat(r.Bindings.Select(x => x.H1StableId).Distinct().Select(id => new Simulation지도H결속Snapshot { HLevelCode = "H1", SpatialStableId = id, StateCode = "ReviewInstance" })).ToArray(),
            Anchors = r.Bindings.Select(b => new Simulation지도배치AnchorSnapshot { AnchorStableId = "map:" + b.PlacementStableId,
                H1StableId = b.H1StableId, PreferredCompositionKey = b.CompositionKey }).ToArray()
        };
        r.MapPlan.MapPlanHashSha256 = Simulation세계자산CanonicalHash.ComputeMapPlanHash(r.MapPlan);
        return r;
    }
    private static void SealMeasurements(SimulationFarmH2PlacementRequest r)
    { foreach (var b in r.Bindings) b.Measurement.MeasurementHashSha256 = SimulationFarmH2PlacementAdapter.ComputeMeasurementHash(b.Measurement); }

    // 합성 측정 사본: 후보별 고정 높이 지지 patch. 원래 noise 생성기나 실제 Unity 측정이라는 주장이 아니다.
    private sealed class SyntheticSurface : ISimulationFarmH2SurfaceReader
    {
        private readonly SimulationFarmH2PlacementRequest r;
        private readonly string mode;
        private readonly JsonDocument doc;
        public SyntheticSurface(SimulationFarmH2PlacementRequest request, string mode = "normal") { r = request; this.mode = mode; doc = JsonDocument.Parse(r.CandidateJson); }
        public string Revision => r.SurfaceRevision;
        public string HashSha256 => r.SurfaceHashSha256;
        public SimulationFarmH2SurfaceSample Read(double x, double z)
        {
            x -= r.CellWorldOriginXMeters + r.LocalOriginXMeters; z -= r.CellWorldOriginZMeters + r.LocalOriginZMeters;
            var a = -r.RotationDegrees * Math.PI / 180; var tx = x * Math.Cos(a) + z * Math.Sin(a); z = -x * Math.Sin(a) + z * Math.Cos(a); x = tx;
            var terrain = doc.RootElement.GetProperty("Input").GetProperty("Terrain");
            var height = terrain.GetProperty("BaseHeight").GetDouble();
            if (terrain.GetProperty("Mode").GetString() == "Terrace" && z >= 0) height += terrain.GetProperty("StepHeight").GetDouble();
            foreach (var o in doc.RootElement.GetProperty("Placements").EnumerateArray())
            {
                var b = o.GetProperty("Bounds");
                if (x >= b.GetProperty("MinX").GetDouble() - .001 && x <= b.GetProperty("MaxX").GetDouble() + .001
                    && z >= b.GetProperty("MinZ").GetDouble() - .001 && z <= b.GetProperty("MaxZ").GetDouble() + .001)
                    height = o.GetProperty("Bottom").GetDouble() - .03;
            }
            if (mode == "higher") height += 1; if (mode == "lower") height -= 1;
            if (mode == "spread" && x < -10) height += .3;
            if (mode == "route-step" && Math.Abs(x) <= 2 && z >= 0 && z <= 1) height += 1;
            return new() { Supported = mode != "unsupported" && !(mode == "route-hole" && Math.Abs(x) <= 2 && Math.Abs(z) <= 1),
                PlacementAllowed = true, HeightMeters = r.CellWorldOriginYMeters + r.LocalOriginYMeters + height,
                SlopeDegrees = mode == "steep" ? 30 : 0 };
        }
    }
}
