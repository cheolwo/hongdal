using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>동결 후보를 현행 배치 정규형으로 변환한다. 재생성·재추첨·지형 수정·권위 전이는 없다.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "동결 Farm H2 후보를 현행 공간 배치 계획과 분리·LH 소비 경계로 변환한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행,
        Boundary = "순수 변환과 측정 입력 검사는 실제 Unity 배치·이동·권위 WI 완료 또는 E5 승격이 아니다.")]
    public sealed class SimulationFarmH2PlacementAdapter
    {
        public const string Revision = "farm-h2-placement-adapter.r1";
        public const string FarmAreaSet = "area-set:sim:pyeongchang:farm-production.v1";

        public SimulationFarmH2PlacementResult Convert(SimulationFarmH2PlacementRequest request,
            ISimulationFarmH2SurfaceReader surface)
        {
            if (request == null || surface == null) throw Error("InputMissing");
            ValidateContext(request, surface);
            using var document = Parse(request.CandidateJson);
            var root = document.RootElement;
            RequireUniqueProperties(root);
            if (S(root, "SchemaVersion") != "farm-h2-candidate.v1"
                || S(root, "PatternStableId") != "candidate-pattern:farm-riverside-practical-h2"
                || S(root, "Status") != "UnapprovedCandidate" || !root.GetProperty("PresentationOnly").GetBoolean())
                throw Error("CandidateSchemaInvalid");
            var candidateHash = OriginalHash(root, "ResultHash");
            if (candidateHash != request.ExpectedCandidateHashSha256 || candidateHash != S(root, "ResultHash"))
                throw Error("CandidateHashMismatch");
            var input = root.GetProperty("Input");
            if (S(input, "PatternRevision") != "farm-riverside-h2.trial.r1"
                || string.IsNullOrWhiteSpace(S(input, "Seed")) || string.IsNullOrWhiteSpace(S(input, "InstanceStableId"))
                || OriginalHash(input) != S(root, "InputHash")) throw Error("CandidateInputHashMismatch");
            var sourceSurface = JsonSerializer.SerializeToElement(new { Terrain = input.GetProperty("Terrain"), Seed = S(input, "Seed") });
            if (OriginalHash(sourceSurface) != S(root, "SurfaceHash")) throw Error("CandidateSurfaceHashMismatch");
            if (root.GetProperty("Issues").GetArrayLength() != 0)
                throw Error("CandidateRejected:" + string.Join(",", root.GetProperty("Issues").EnumerateArray().Select(x => S(x, "Code")).Distinct().OrderBy(x => x, StringComparer.Ordinal)));

            var sourceObjects = root.GetProperty("Placements").EnumerateArray().ToArray();
            var bindings = request.Bindings.ToDictionary(x => x.SourcePlacementStableId, StringComparer.Ordinal);
            if (sourceObjects.Length != bindings.Count || sourceObjects.Select(x => S(x, "StableId")).Distinct().Count() != sourceObjects.Length)
                throw Error("BindingCoverageInvalid");
            var sourceIds = new HashSet<string>(sourceObjects.Select(x => S(x, "StableId")), StringComparer.Ordinal);
            if (bindings.Keys.Any(x => !sourceIds.Contains(x))) throw Error("UnknownPlacementBinding");
            var observations = new SortedDictionary<string, string>(StringComparer.Ordinal);
            SimulationFarmH2SurfaceSample Read(double x, double z)
            {
                var wx = request.CellWorldOriginXMeters + x;
                var wz = request.CellWorldOriginZMeters + z;
                var s = surface.Read(wx, wz);
                if (s == null || !s.Supported || !s.PlacementAllowed) throw Error("SurfaceSupportMissingOrDenied");
                if (!Finite(s.HeightMeters) || !Finite(s.SlopeDegrees) || s.SlopeDegrees < 0 || s.SlopeDegrees > 90) throw Error("SurfaceSampleInvalid");
                var key = F(wx) + "," + F(wz);
                var value = F(s.HeightMeters) + "," + F(s.SlopeDegrees);
                if (observations.TryGetValue(key, out var old) && old != value) throw Error("SurfaceChangedDuringConversion");
                observations[key] = value;
                return s;
            }
            var placements = new List<Simulation세계자산PlacementSnapshot>();
            var envelopes = new Dictionary<string, Box>(StringComparer.Ordinal);
            foreach (var source in sourceObjects.OrderBy(x => S(x, "StableId"), StringComparer.Ordinal))
            {
                var id = S(source, "StableId");
                if (!bindings.TryGetValue(id, out var b)) throw Error("BindingMissing");
                ValidateBinding(source, b, request);
                var m = b.Measurement;
                var sourceBox = Box.From(source.GetProperty("Bounds"));
                var center = Transform((sourceBox.MinX + sourceBox.MaxX) / 2, (sourceBox.MinZ + sourceBox.MaxZ) / 2, request);
                var yaw = Normalize(D(source, "Yaw") + request.RotationDegrees);
                var scale = m.UniformScale * request.UniformScale;
                var offset = Rotate(m.CenterX * scale, m.CenterZ * scale, yaw);
                var size = RotatedSize(m.SizeX * scale, m.SizeZ * scale, yaw);
                var actual = new Box(center.X - size.X / 2, center.Z - size.Z / 2, center.X + size.X / 2, center.Z + size.Z / 2);
                var reserved = Transform(sourceBox, request);
                if (!reserved.Contains(actual, request.Policy.BottomToleranceMeters)) throw Error("MeasuredEnvelopeExceedsCandidate:" + id);
                CellContains(actual, request);
                var bottom = request.LocalOriginYMeters + D(source, "Bottom") * request.UniformScale;
                var heightSamples = new List<double>();
                foreach (var p in actual.Samples())
                {
                    var s = Read(p.X, p.Z);
                    if (s.SlopeDegrees > request.Policy.MaximumSlopeDegrees) throw Error("SlopeTooSteep:" + id);
                    heightSamples.Add(s.HeightMeters - request.CellWorldOriginYMeters);
                }
                if (heightSamples.Max() - heightSamples.Min() > request.Policy.MaximumHeightSpreadMeters) throw Error("HeightSpreadExceeded:" + id);
                var clearanceError = bottom - heightSamples.Max() - request.Policy.GroundClearanceMeters;
                if (clearanceError < -request.Policy.BottomToleranceMeters) throw Error("BuriedBottom:" + id);
                if (clearanceError > request.Policy.BottomToleranceMeters) throw Error("FloatingBottom:" + id);
                envelopes.Add(id, actual);
                placements.Add(new Simulation세계자산PlacementSnapshot
                {
                    PlacementStableId = b.PlacementStableId, OwnerCellStableId = request.OwnerCellStableId,
                    H1StableId = b.H1StableId, CompositionKey = b.CompositionKey,
                    PlacementKindCode = S(source, "Role") == "BarnWorkYard" ? Simulation세계자산배치Codes.Building : Simulation세계자산배치Codes.MapAnchor,
                    LayerCode = "FarmH2Review", CategoryCode = b.AssetFamilyId,
                    AuthorityKindCode = Simulation세계자산배치Codes.AmbientPresentation,
                    PersistenceKindCode = Simulation세계자산배치Codes.DerivedPersistent,
                    StateCode = "UnapprovedCandidate", LocalXMeters = center.X - offset.X,
                    LocalYMeters = bottom - (m.CenterY - m.SizeY / 2) * scale, LocalZMeters = center.Z - offset.Z,
                    RotationDegrees = yaw, UniformScale = scale, FixedAnchor = true,
                    CollisionEligible = m.ActiveCollider, PresentationOnly = true
                });
            }
            var boxes = envelopes.Values.ToArray();
            for (var a = 0; a < boxes.Length; a++) for (var b = a + 1; b < boxes.Length; b++)
                if (boxes[a].Touches(boxes[b]) || boxes[a].Distance(boxes[b]) < request.Policy.MinimumSpacingMeters) throw Error("ObjectOverlapOrSpacing");
            var areas = root.GetProperty("PreservedAreas").EnumerateArray().Select(a =>
            {
                var box = Transform(Box.From(a.GetProperty("Bounds")), request);
                CellContains(box, request);
                if (boxes.Any(box.Touches)) throw Error("PreservedAreaIntrusion");
                return new SimulationFarmH2ReservedAreaSnapshot { SourceStableId = S(a, "StableId"), RoleCode = S(a, "Role"), MinX = box.MinX, MinZ = box.MinZ, MaxX = box.MaxX, MaxZ = box.MaxZ };
            }).OrderBy(x => x.SourceStableId, StringComparer.Ordinal).ToArray();
            foreach (var obstacle in input.GetProperty("Obstacles").EnumerateArray())
            {
                var box = Transform(Box.From(obstacle), request);
                if (boxes.Any(b => b.Touches(box) || b.Distance(box) < request.Policy.MinimumSpacingMeters)) throw Error("ExistingObjectConflict");
            }
            var anchors = root.GetProperty("Anchors").EnumerateArray().Select(a =>
            {
                var p = a.GetProperty("Position"); var xy = Transform(D(p, "X"), D(p, "Z"), request);
                var owner = S(a, "OwnerStableId");
                if (owner.Length > 0 && !bindings.ContainsKey(owner)) throw Error("AnchorOwnerMissing");
                return new SimulationFarmH2AnchorSnapshot { SourceAnchorStableId = S(a, "StableId"), RoleCode = S(a, "Role"),
                    OwnerPlacementStableId = owner.Length == 0 ? "" : bindings[owner].PlacementStableId,
                    H2StableId = request.H2StableId, LocalXMeters = xy.X, LocalZMeters = xy.Z, FacingCode = RotateFacing(S(a, "Facing"), request.RotationDegrees) };
            }).OrderBy(x => x.SourceAnchorStableId, StringComparer.Ordinal).ToArray();
            var routes = root.GetProperty("Routes").EnumerateArray().Select(r => new SimulationFarmH2RouteSnapshot
            { SourceRouteStableId = S(r, "StableId"), FromSourceAnchorStableId = S(r, "From"), ToSourceAnchorStableId = S(r, "To"), WidthMeters = D(r, "Width") * request.UniformScale }).OrderBy(x => x.SourceRouteStableId, StringComparer.Ordinal).ToArray();
            ValidateRoutes(anchors, routes, areas, placements, envelopes, bindings, input, request, Read);
            if (surface.Revision != request.SurfaceRevision || surface.HashSha256 != request.SurfaceHashSha256) throw Error("SurfaceChangedDuringConversion");
            var inputHash = RequestHash(request);
            var plan = new Simulation세계자산배치Plan
            {
                RuleRevision = Revision, CellStableId = request.OwnerCellStableId, SourceWorldRevision = request.MapPlan.SourceWorldRevision,
                MapPlanHashSha256 = request.MapPlan.MapPlanHashSha256,
                Placements = placements.OrderBy(x => x.PlacementStableId, StringComparer.Ordinal).ToArray()
            };
            plan.AssetPlacementPlanHashSha256 = Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(plan);
            var result = new SimulationFarmH2PlacementResult
            {
                AdapterRevision = Revision, CandidateHashSha256 = candidateHash, CandidateInputHashSha256 = S(root, "InputHash"),
                CandidateSurfaceHashSha256 = S(root, "SurfaceHash"), CandidateSeed = S(input, "Seed"), CandidatePatternRevision = S(input, "PatternRevision"),
                ConversionInputHashSha256 = inputHash, SurfaceSamplesHashSha256 = CanonicalHash(observations),
                AreaSetStableId = request.AreaSetStableId, H2StableId = request.H2StableId,
                ContainsSyntheticMeasurements = request.Bindings.Any(x => x.Measurement.EvidenceKindCode == "SyntheticFixture"),
                PolicyIsTrial = request.Policy.TrialOnly, Plan = plan, Anchors = anchors, Routes = routes, ReservedAreas = areas
            };
            result.ConversionOutputHashSha256 = ResultHash(result);
            return result;
        }

        public Simulation분리세계자산배치Result PartitionFrozen(SimulationFarmH2PlacementResult result)
        {
            if (result == null || result.AdapterRevision != Revision || result.PatternStatusCode != "UnapprovedCandidate"
                || result.ActualTraversalVerified || ResultHash(result) != result.ConversionOutputHashSha256
                || Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(result.Plan) != result.Plan.AssetPlacementPlanHashSha256)
                throw Error("FrozenOutputHashMismatch");
            // 기존 Partition이 배열을 공유하므로 호출자 결과와의 alias를 끊는다.
            var copy = JsonSerializer.Deserialize<Simulation세계자산배치Plan>(JsonSerializer.Serialize(result.Plan))!;
            return new Simulation결정적세계자산배치Plan분리Service().Partition(copy);
        }

        public static string ComputeMeasurementHash(SimulationFarmH2AssetMeasurement m)
            => CanonicalHash(m, "MeasurementHashSha256");
        private static string ResultHash(SimulationFarmH2PlacementResult r) => CanonicalHash(r, "ConversionOutputHashSha256");
        private static string RequestHash(SimulationFarmH2PlacementRequest r) => CanonicalHash(r, "CandidateJson");

        private static void ValidateContext(SimulationFarmH2PlacementRequest r, ISimulationFarmH2SurfaceReader surface)
        {
            if (r.MapPlan == null || r.Policy == null || r.Bindings == null || r.ResolvedCompositionKeys == null
                || r.MapPlan.HBindings == null || r.MapPlan.Anchors == null || r.MapPlan.Connectors == null) throw Error("ContextMissing");
            var map = r.MapPlan;
            if (!Sha(r.ExpectedCandidateHashSha256) || !Sha(r.ResolverHashSha256) || !Sha(r.SurfaceHashSha256)
                || string.IsNullOrWhiteSpace(r.ResolverRevision) || string.IsNullOrWhiteSpace(r.SurfaceRevision)
                || r.SurfaceRevision != surface.Revision || r.SurfaceHashSha256 != surface.HashSha256) throw Error("SourceFingerprintInvalid");
            if (string.IsNullOrWhiteSpace(r.OwnerCellStableId) || r.OwnerCellStableId != map.CellStableId
                || r.AreaSetStableId != FarmAreaSet || string.IsNullOrWhiteSpace(r.H2StableId)
                || !map.HBindings.Any(x => x.HLevelCode == "H4" && x.SpatialStableId == r.AreaSetStableId)
                || !map.HBindings.Any(x => x.HLevelCode == "H2" && x.SpatialStableId == r.H2StableId)) throw Error("CellOrHOwnershipMissing");
            if (map.SourceWorldRevision < 0 || !Sha(map.MapPlanHashSha256)
                || map.MapPlanHashSha256 != Simulation세계자산CanonicalHash.ComputeMapPlanHash(map)) throw Error("MapHashMismatch");
            if (r.UnitCode != "Meters" || r.AxisCode != "XRightYUpZForward"
                || !new[] { r.CellSizeMeters, r.CellWorldOriginXMeters, r.CellWorldOriginYMeters, r.CellWorldOriginZMeters,
                    r.LocalOriginXMeters, r.LocalOriginYMeters, r.LocalOriginZMeters, r.UniformScale, r.RotationDegrees }.All(Finite)
                || r.CellSizeMeters <= 0 || r.UniformScale <= 0 || !new[] { 0d, 90d, 180d, 270d }.Contains(r.RotationDegrees)) throw Error("CoordinateFrameInvalid");
            if (r.Bindings.Any(x => x == null || string.IsNullOrWhiteSpace(x.SourcePlacementStableId) || string.IsNullOrWhiteSpace(x.PlacementStableId))
                || r.Bindings.Select(x => x.SourcePlacementStableId).Distinct().Count() != r.Bindings.Length
                || r.Bindings.Select(x => x.PlacementStableId).Distinct().Count() != r.Bindings.Length) throw Error("BindingIdentityInvalid");
            var p = r.Policy;
            if (string.IsNullOrWhiteSpace(p.Revision) || string.IsNullOrWhiteSpace(p.EvidenceRef)
                || !new[] { p.MaximumSlopeDegrees, p.MaximumHeightSpreadMeters, p.GroundClearanceMeters, p.BottomToleranceMeters,
                    p.MinimumSpacingMeters, p.MinimumRouteWidthMeters, p.RouteSampleStepMeters, p.MaximumRouteSlopeDegrees, p.MaximumRouteStepMeters }.All(x => Finite(x) && x >= 0)
                || p.MaximumSlopeDegrees >= 90 || p.MaximumRouteSlopeDegrees >= 90 || p.MinimumRouteWidthMeters <= 0
                || p.RouteSampleStepMeters < .05 || p.RouteSampleStepMeters > 2) throw Error("MeasurementPolicyInvalid");
        }

        private static void ValidateBinding(JsonElement source, SimulationFarmH2PlacementBinding b, SimulationFarmH2PlacementRequest r)
        {
            if (string.IsNullOrWhiteSpace(S(source, "H1StableId"))) throw Error("SourceH1OwnershipMissing");
            var role = S(source, "Role");
            var expected = role switch { "BarnWorkYard" => "candidate:farm.red-barn", "ProductionPlot" => "candidate:farm.crop-plot",
                "WaterAccess" => "candidate:farm.water-access", "NaturalAccent" => "candidate:farm.grass-accent", _ => throw Error("UnknownRole") };
            if (b.VisualKey != expected || b.VisualKey != S(source, "VisualKey") || b.AssetFamilyId != S(source, "AssetFamilyId")
                || string.IsNullOrWhiteSpace(b.CompositionKey) || !r.ResolvedCompositionKeys.Contains(b.CompositionKey, StringComparer.Ordinal)) throw Error("UnknownVisualOrCompositionKey");
            if (role == "ProductionPlot" && !new[] { "farm:감자밭 두렁:A", "farm:감자밭 두렁:B", "farm:감자밭 두렁:C" }.Contains(b.CompositionKey)) throw Error("ProductionCompositionNotAllowed");
            if (string.IsNullOrWhiteSpace(b.H1StableId) || !r.MapPlan.HBindings.Any(x => x.HLevelCode == "H1" && x.SpatialStableId == b.H1StableId)
                || !r.MapPlan.Anchors.Any(x => x.H1StableId == b.H1StableId && x.PreferredCompositionKey == b.CompositionKey)) throw Error("H1OwnershipMissing");
            var m = b.Measurement;
            if (m == null || string.IsNullOrWhiteSpace(m.Revision) || string.IsNullOrWhiteSpace(m.EvidenceRef)
                || (m.EvidenceKindCode != "SyntheticFixture" && m.EvidenceKindCode != "MeasuredWrapper") || !Sha(m.AssetFingerprintSha256)
                || !m.ActiveRenderer || (role != "NaturalAccent" && !m.ActiveCollider)
                || !new[] { m.CenterX, m.CenterY, m.CenterZ, m.SizeX, m.SizeY, m.SizeZ, m.UniformScale }.All(Finite)
                || m.SizeX <= 0 || m.SizeY < 0 || m.SizeZ <= 0 || m.UniformScale <= 0) throw Error("AssetMeasurementMissingOrInvalid");
            if (m.MeasurementHashSha256 != ComputeMeasurementHash(m)) throw Error("MeasurementHashMismatch");
            if (role == "ProductionPlot" && (string.IsNullOrWhiteSpace(b.WorkAreaEvidenceRef)
                || !Finite(b.WorkAreaWidthMeters) || !Finite(b.WorkAreaDepthMeters)
                || b.WorkAreaWidthMeters < 14 || b.WorkAreaWidthMeters > 56 || b.WorkAreaDepthMeters < 12 || b.WorkAreaDepthMeters > 48
                || b.WorkAreaWidthMeters < m.SizeX * m.UniformScale * r.UniformScale
                || b.WorkAreaDepthMeters < m.SizeZ * m.UniformScale * r.UniformScale)) throw Error("ProductionSeedbedBoundsInvalid");
        }

        private static void ValidateRoutes(SimulationFarmH2AnchorSnapshot[] anchors, SimulationFarmH2RouteSnapshot[] routes,
            SimulationFarmH2ReservedAreaSnapshot[] areas, List<Simulation세계자산PlacementSnapshot> placements, Dictionary<string, Box> envelopes,
            Dictionary<string, SimulationFarmH2PlacementBinding> bindings, JsonElement input, SimulationFarmH2PlacementRequest r,
            Func<double, double, SimulationFarmH2SurfaceSample> read)
        {
            if (anchors.Select(x => x.SourceAnchorStableId).Distinct().Count() != anchors.Length
                || routes.Select(x => x.SourceRouteStableId).Distinct().Count() != routes.Length) throw Error("RouteIdentityInvalid");
            var nodes = anchors.ToDictionary(x => x.SourceAnchorStableId, StringComparer.Ordinal);
            var graph = nodes.Keys.ToDictionary(x => x, _ => new List<string>(), StringComparer.Ordinal);
            foreach (var route in routes)
            {
                if (!nodes.TryGetValue(route.FromSourceAnchorStableId, out var a) || !nodes.TryGetValue(route.ToSourceAnchorStableId, out var b)) throw Error("RouteEndpointMissing");
                var dx = b.LocalXMeters - a.LocalXMeters; var dz = b.LocalZMeters - a.LocalZMeters;
                var length = Math.Sqrt(dx * dx + dz * dz);
                if (!Finite(route.WidthMeters) || route.WidthMeters < r.Policy.MinimumRouteWidthMeters || length <= 0
                    || (Math.Abs(dx) > 1e-8 && Math.Abs(dz) > 1e-8)) throw Error("RouteGeometryInvalid");
                var corridor = Math.Abs(dx) < 1e-8 ? new Box(a.LocalXMeters - route.WidthMeters / 2, Math.Min(a.LocalZMeters, b.LocalZMeters), a.LocalXMeters + route.WidthMeters / 2, Math.Max(a.LocalZMeters, b.LocalZMeters))
                    : new Box(Math.Min(a.LocalXMeters, b.LocalXMeters), a.LocalZMeters - route.WidthMeters / 2, Math.Max(a.LocalXMeters, b.LocalXMeters), a.LocalZMeters + route.WidthMeters / 2);
                CellContains(corridor, r);
                foreach (var binding in bindings.Values)
                    if (binding.PlacementStableId != a.OwnerPlacementStableId && binding.PlacementStableId != b.OwnerPlacementStableId
                        && corridor.Touches(envelopes[binding.SourcePlacementStableId])) throw Error("ProtectedRouteIntrusion");
                if (areas.Any(x => x.RoleCode != "WorkYard" && corridor.Touches(new Box(x.MinX, x.MinZ, x.MaxX, x.MaxZ)))) throw Error("RoutePreservedAreaIntrusion");
                if (input.GetProperty("Obstacles").EnumerateArray().Any(x => corridor.Touches(Transform(Box.From(x), r)))) throw Error("RouteObstacle");
                var count = (int)Math.Ceiling(length / r.Policy.RouteSampleStepMeters);
                if (count > 100000) throw Error("RouteSampleBudgetExceeded");
                var previous = new double?[3];
                for (var i = 0; i <= count; i++) for (var side = 0; side < 3; side++)
                {
                    var offset = (side - 1) * route.WidthMeters / 2;
                    var sample = read(a.LocalXMeters + dx * i / count - dz / length * offset, a.LocalZMeters + dz * i / count + dx / length * offset);
                    if (sample.SlopeDegrees > r.Policy.MaximumRouteSlopeDegrees) throw Error("RouteSlopeTooSteep");
                    if (previous[side].HasValue && Math.Abs(sample.HeightMeters - previous[side]!.Value) > r.Policy.MaximumRouteStepMeters) throw Error("RouteStepExceeded");
                    previous[side] = sample.HeightMeters;
                }
                graph[a.SourceAnchorStableId].Add(b.SourceAnchorStableId); graph[b.SourceAnchorStableId].Add(a.SourceAnchorStableId);
            }
            var skeleton = new[] { "ExternalEntry", "BarnWorkYard", "InternalWorkPath", "ExternalExit" };
            var core = skeleton.Select(role => anchors.Where(a => a.RoleCode == role).ToArray()).ToArray();
            if (core.Any(x => x.Length != 1)) throw Error("SkeletonMissing");
            for (var i = 1; i < core.Length; i++) if (!graph[core[i - 1][0].SourceAnchorStableId].Contains(core[i][0].SourceAnchorStableId)) throw Error("SkeletonContinuityBroken");
            var visited = new HashSet<string>(); var queue = new Queue<string>(); queue.Enqueue(core[0][0].SourceAnchorStableId);
            while (queue.Count > 0) { var id = queue.Dequeue(); if (visited.Add(id)) foreach (var next in graph[id]) queue.Enqueue(next); }
            if (visited.Count != anchors.Length) throw Error("RouteDisconnected");
            foreach (var p in placements.Where(x => x.CompositionKey.Contains("감자밭") || x.PlacementKindCode == Simulation세계자산배치Codes.Building))
                if (!anchors.Any(x => x.RoleCode == "H1Entrance" && x.OwnerPlacementStableId == p.PlacementStableId)) throw Error("H1EntranceMissing");
        }

        private static JsonDocument Parse(string json)
        { try { return JsonDocument.Parse(json); } catch (JsonException e) { throw new ArgumentException("FarmH2:CandidateJsonInvalid", e); } }
        private static void RequireUniqueProperties(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Object)
            {
                var properties = value.EnumerateObject().ToArray();
                if (properties.Select(x => x.Name).Distinct(StringComparer.Ordinal).Count() != properties.Length) throw Error("DuplicateJsonProperty");
                foreach (var p in properties) RequireUniqueProperties(p.Value);
            }
            else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) RequireUniqueProperties(item);
        }
        private static string OriginalHash(JsonElement e, string blank = "")
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                if (blank.Length == 0) e.WriteTo(writer);
                else { writer.WriteStartObject(); foreach (var p in e.EnumerateObject()) { writer.WritePropertyName(p.Name); if (p.Name == blank) writer.WriteStringValue(""); else p.Value.WriteTo(writer); } writer.WriteEndObject(); }
            }
            return Simulation세계자산CanonicalHash.Hash(Encoding.UTF8.GetString(stream.ToArray()));
        }
        private static string CanonicalHash<T>(T value, string blank = "")
        {
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(value));
            return Simulation세계자산CanonicalHash.Hash(Canonical(doc.RootElement, blank));
        }
        private static string Canonical(JsonElement e, string blank = "")
        {
            if (e.ValueKind == JsonValueKind.Object) return "{" + string.Join(",", e.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p => JsonSerializer.Serialize(p.Name) + ":" + (p.Name == blank ? "\"\"" : Canonical(p.Value)))) + "}";
            if (e.ValueKind == JsonValueKind.Array) return "[" + string.Join(",", e.EnumerateArray().Select(x => Canonical(x)).OrderBy(x => x, StringComparer.Ordinal)) + "]";
            return e.GetRawText();
        }
        private static string S(JsonElement e, string key) => e.GetProperty(key).GetString() ?? throw Error("StringMissing:" + key);
        private static double D(JsonElement e, string key) { var d = e.GetProperty(key).GetDouble(); if (!Finite(d)) throw Error("NumberInvalid"); return d; }
        private static bool Sha(string value) => value != null && value.Length == 64 && value.All(Uri.IsHexDigit);
        private static bool Finite(double d) => !double.IsNaN(d) && !double.IsInfinity(d);
        private static string F(double d) => d.ToString("R", CultureInfo.InvariantCulture);
        private static ArgumentException Error(string code) => new ArgumentException("FarmH2:" + code);
        private static double Normalize(double d) => (d % 360 + 360) % 360;
        private static (double X, double Z) Rotate(double x, double z, double degrees)
        {
            var angle = Normalize(degrees);
            if (angle == 0) return (x, z); if (angle == 90) return (z, -x); if (angle == 180) return (-x, -z); if (angle == 270) return (-z, x);
            var rad = angle * Math.PI / 180; return (x * Math.Cos(rad) + z * Math.Sin(rad), -x * Math.Sin(rad) + z * Math.Cos(rad));
        }
        private static (double X, double Z) RotatedSize(double x, double z, double yaw)
        { var a = Rotate(x, z, yaw); var b = Rotate(x, -z, yaw); return (Math.Max(Math.Abs(a.X), Math.Abs(b.X)), Math.Max(Math.Abs(a.Z), Math.Abs(b.Z))); }
        private static (double X, double Z) Transform(double x, double z, SimulationFarmH2PlacementRequest r)
        { var p = Rotate(x * r.UniformScale, z * r.UniformScale, r.RotationDegrees); return (p.X + r.LocalOriginXMeters, p.Z + r.LocalOriginZMeters); }
        private static Box Transform(Box b, SimulationFarmH2PlacementRequest r)
        { var pts = b.Samples().Select(p => Transform(p.X, p.Z, r)).ToArray(); return new Box(pts.Min(p => p.X), pts.Min(p => p.Z), pts.Max(p => p.X), pts.Max(p => p.Z)); }
        private static string RotateFacing(string facing, double degrees)
        {
            if (facing.Length == 0) return "";
            var values = new[] { "North", "East", "South", "West" }; var index = Array.IndexOf(values, facing);
            if (index < 0) throw Error("FacingInvalid"); return values[(index + (int)(degrees / 90)) % 4];
        }
        private static void CellContains(Box b, SimulationFarmH2PlacementRequest r)
        { if (!new Box(-r.CellSizeMeters / 2, -r.CellSizeMeters / 2, r.CellSizeMeters / 2, r.CellSizeMeters / 2).Contains(b, 0)) throw Error("OutsideOwnerCell"); }
        private sealed class Box
        {
            public double MinX, MinZ, MaxX, MaxZ;
            public Box(double minX, double minZ, double maxX, double maxZ)
            { if (!new[] { minX, minZ, maxX, maxZ }.All(Finite) || minX >= maxX || minZ >= maxZ) throw Error("BoundsInvalid"); MinX = minX; MinZ = minZ; MaxX = maxX; MaxZ = maxZ; }
            public static Box From(JsonElement e) => new Box(D(e, "MinX"), D(e, "MinZ"), D(e, "MaxX"), D(e, "MaxZ"));
            public bool Contains(Box b, double tolerance) => b.MinX >= MinX - tolerance && b.MaxX <= MaxX + tolerance && b.MinZ >= MinZ - tolerance && b.MaxZ <= MaxZ + tolerance;
            public bool Touches(Box b) => MinX <= b.MaxX && MaxX >= b.MinX && MinZ <= b.MaxZ && MaxZ >= b.MinZ;
            public double Distance(Box b) { var x = Math.Max(0, Math.Max(MinX - b.MaxX, b.MinX - MaxX)); var z = Math.Max(0, Math.Max(MinZ - b.MaxZ, b.MinZ - MaxZ)); return Math.Sqrt(x * x + z * z); }
            public IEnumerable<(double X, double Z)> Samples() { for (var x = 0; x < 3; x++) for (var z = 0; z < 3; z++) yield return (MinX + (MaxX - MinX) * x / 2, MinZ + (MaxZ - MinZ) * z / 2); }
        }
    }
}
