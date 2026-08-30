using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "D320 실측 크기와 Player 여유로 별도 후보를 봉인하고 현행 배치 검사를 재사용한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행,
        Boundary = "원후보·지면·생산 상태를 변경하지 않으며 후보 자동 승인이나 Scene 배치를 하지 않는다.")]
    public sealed class SimulationFarmH2부지확장Service
    {
        public const string Revision = "farm-riverside-h2.measured-expansion.r2";
        public const string StudyHash = "EC9AD2D5333102B22477A8063A6938C5CE15817A09D53900CC98C5EC8A78ACE5";

        public SimulationFarmH2부지확장Result ExpandAndValidate(SimulationFarmH2부지확장Request request,
            ISimulationFarmH2SurfaceReader surface)
        {
            var result = CreateCandidate(request, surface);
            result.ValidatedPlacement = new SimulationFarmH2PlacementAdapter().Convert(result.CandidateRequest, surface);
            return result;
        }

        public SimulationFarmH2부지확장Result CreateCandidate(SimulationFarmH2부지확장Request input,
            ISimulationFarmH2SurfaceReader surface)
        {
            if (input == null || input.ParentRequest == null || input.Player == null || input.BarnSolidBounds == null || surface == null)
                throw Error("InputMissing");
            var r = JsonSerializer.Deserialize<SimulationFarmH2PlacementRequest>(JsonSerializer.Serialize(input.ParentRequest))!;
            var root = JsonNode.Parse(r.CandidateJson) ?? throw Error("CandidateMissing");
            if (S(root, "ResultHash") != r.ExpectedCandidateHashSha256 || HashCandidate(r.CandidateJson) != r.ExpectedCandidateHashSha256)
                throw Error("ParentCandidateHashMismatch");
            if (S(root, "SchemaVersion") != "farm-h2-candidate.v1" || S(root, "Status") != "UnapprovedCandidate"
                || S(root, "PatternStableId") != "candidate-pattern:farm-riverside-practical-h2"
                || root["PresentationOnly"]!.GetValue<bool>() != true
                || S(root["Input"]!, "PatternRevision") != "farm-riverside-h2.trial.r1") throw Error("ParentSchemaInvalid");
            if (HashNode(root["Input"]!) != S(root, "InputHash")
                || HashNode(JsonSerializer.SerializeToNode(new { Terrain = root["Input"]!["Terrain"], Seed = S(root["Input"]!, "Seed") })!) != S(root, "SurfaceHash"))
                throw Error("ParentInputHashMismatch");
            if (root["Issues"]!.AsArray().Count != 0) throw Error("ParentCandidateRejected:" +
                string.Join(",", root["Issues"]!.AsArray().Select(x => S(x!, "Code")).Distinct().OrderBy(x => x, StringComparer.Ordinal)));
            ValidateInput(input, r, surface);
            var placements = root["Placements"]!.AsArray();
            var barn = placements.Single(x => S(x!, "Role") == "BarnWorkYard")!;
            if (D(barn, "Yaw") != 0) throw Error("SourceBarnYawUnsupported:use explicit cell cardinal rotation");
            var binding = r.Bindings.Single(x => x.SourcePlacementStableId == S(barn, "StableId"));
            var m = binding.Measurement;
            var expectedBarnFingerprint = input.BarnCode == "Barn01"
                ? "43e124930635ba7aa213268e278a243f1800c240af554b3b500f9174d0ae5ed6"
                : "b75924cf67357599af7e44d41f36a9a59445da972e9b41a4b16df94e9bdb32f5";
            if (m.AssetFingerprintSha256 != expectedBarnFingerprint) throw Error("BarnMeasurementBindingMismatch");
            var solid = input.BarnSolidBounds;
            ValidateBounds(solid);
            if (input.BarnSolidMeasurementHashSha256 != HashSolidMeasurement(solid, m.AssetFingerprintSha256)) throw Error("SolidMeasurementHashMismatch");
            if (solid.MinX > m.CenterX - m.SizeX / 2 + 1e-5 || solid.MaxX < m.CenterX + m.SizeX / 2 - 1e-5
                || solid.MinZ > m.CenterZ - m.SizeZ / 2 + 1e-5 || solid.MaxZ < m.CenterZ + m.SizeZ / 2 - 1e-5)
                throw Error("SolidBoundsMustContainRenderer");
            // Adapter가 Renderer 중심을 배치하므로 그 중심에 대해 Collider도 포함하는 대칭 외곽을 예약한다.
            var halfX = Math.Max(m.CenterX - solid.MinX, solid.MaxX - m.CenterX);
            var halfZ = Math.Max(m.CenterZ - solid.MinZ, solid.MaxZ - m.CenterZ);
            var clearance = input.Player.RadiusMeters + input.Player.SkinWidthMeters + input.Player.ClickStopDistanceMeters;
            var lane = Math.Max(2 * clearance, Math.Max(r.Policy.MinimumRouteWidthMeters,
                root["Routes"]!.AsArray().Max(x => D(x!, "Width"))));
            var gap = r.Policy.MinimumSpacingMeters;
            if (!Finite(gap) || gap < 0 || !Finite(lane) || lane <= 0) throw Error("AccessPolicyInvalid");
            var margin = lane + gap;
            var width = 2 * halfX + 2 * margin;
            var depth = 2 * halfZ + 2 * margin;
            var yardArea = root["PreservedAreas"]!.AsArray().Single(x => S(x!, "Role") == "WorkYard")!;
            var oldBounds = barn["Bounds"]!;
            var centerZ = (D(oldBounds, "MinZ") + D(oldBounds, "MaxZ")) / 2;
            // 마당 서쪽 경계에 접하는 예약 부지. 모델은 native scale1이며 서쪽으로 명시 이동한다.
            var right = D(yardArea["Bounds"]!, "MinX");
            var bounds = new SimulationFarmH2외곽측정 { MinX = right - width, MaxX = right, MinZ = centerZ - depth / 2, MaxZ = centerZ + depth / 2 };
            var cx = (bounds.MinX + bounds.MaxX) / 2;
            barn["Bounds"] = BoxNode(bounds);
            barn["DimensionEvidence"] = "MeasuredRendererColliderNativeScale1+PlayerAccessBand;D320;r2";
            yardArea["Bounds"]!["MinZ"] = Math.Min(D(yardArea["Bounds"]!, "MinZ"), bounds.MinZ);
            yardArea["Bounds"]!["MaxZ"] = Math.Max(D(yardArea["Bounds"]!, "MaxZ"), bounds.MaxZ);

            var anchors = root["Anchors"]!.AsArray(); var routes = root["Routes"]!.AsArray();
            var door = anchors.Single(x => S(x!, "Role") == "H1Entrance" && S(x!, "OwnerStableId") == binding.SourcePlacementStableId)!;
            var east = bounds.MaxX - lane / 2; var west = bounds.MinX + lane / 2;
            var north = bounds.MaxZ - lane / 2; var south = bounds.MinZ + lane / 2;
            door["Position"]!["X"] = east; door["Position"]!["Z"] = centerZ; door["Facing"] = "East";
            var nodes = new[] { ("access-ne", east, north), ("access-nw", west, north),
                ("access-west", west, centerZ), ("access-sw", west, south), ("access-se", east, south) };
            var previous = S(door, "StableId");
            foreach (var node in nodes)
            {
                var id = binding.SourcePlacementStableId + "/" + node.Item1;
                anchors.Add(JsonSerializer.SerializeToNode(new { StableId = id, Role = "BarnExteriorAccess", Position = new { X = node.Item2, Z = node.Item3 }, OwnerStableId = binding.SourcePlacementStableId, Facing = "" }));
                routes.Add(Route(previous, id, lane)); previous = id;
            }
            routes.Add(Route(previous, S(door, "StableId"), lane));
            // 공통 Adapter의 목적지 소유자 예외와 별개로 Barn은 실내 진입하지 않는다.
            // 문 앞 연결을 포함해 모든 경로 띠가 실제 Collider 외곽을 가로지르는지 확인한다.
            var barnPhysical = new SimulationFarmH2외곽측정 { MinX = cx + solid.MinX - m.CenterX,
                MaxX = cx + solid.MaxX - m.CenterX, MinZ = centerZ + solid.MinZ - m.CenterZ, MaxZ = centerZ + solid.MaxZ - m.CenterZ };
            foreach (var route in routes)
            {
                var a = anchors.Single(x => S(x!, "StableId") == S(route!, "From"))!["Position"]!;
                var b = anchors.Single(x => S(x!, "StableId") == S(route!, "To"))!["Position"]!;
                var ax = D(a, "X"); var az = D(a, "Z"); var bx = D(b, "X"); var bz = D(b, "Z");
                var halfWidth = D(route!, "Width") / 2;
                if (ax != bx && az != bz) throw Error("RouteGeometryInvalid");
                var corridor = new SimulationFarmH2외곽측정 { MinX = Math.Min(ax, bx) - (ax == bx ? halfWidth : 0),
                    MaxX = Math.Max(ax, bx) + (ax == bx ? halfWidth : 0), MinZ = Math.Min(az, bz) - (az == bz ? halfWidth : 0),
                    MaxZ = Math.Max(az, bz) + (az == bz ? halfWidth : 0) };
                if (Touches(corridor, barnPhysical)) throw Error("BarnAccessColliderIntrusion");
            }
            // 예약 부지는 렌더링 물체가 아니라 보호해야 할 접근 공간이다.
            foreach (var area in root["PreservedAreas"]!.AsArray().Where(x => S(x!, "Role") != "WorkYard"))
                if (Touches(bounds, ReadBox(area!["Bounds"]!))) throw Error("AccessReservationPreservedAreaIntrusion");
            foreach (var obstacle in root["Input"]!["Obstacles"]!.AsArray())
                if (Touches(bounds, ReadBox(obstacle!))) throw Error("AccessReservationObstacle");
            foreach (var other in placements.Where(x => S(x!, "StableId") != binding.SourcePlacementStableId))
                if (Touches(bounds, ReadBox(other!["Bounds"]!))) throw Error("AccessReservationOtherPlacement");
            foreach (var point in Samples(bounds)) EnsureCell(point.X, point.Z, r);

            // 새 위치의 현재 읽기 전용 지지면을 표본화해 Y만 정한다. 지형 수정 API는 없다.
            foreach (var p in placements)
            {
                var b = r.Bindings.Single(x => x.SourcePlacementStableId == S(p!, "StableId"));
                var box = ReadBox(p!["Bounds"]!); var px = (box.MinX + box.MaxX) / 2; var pz = (box.MinZ + box.MaxZ) / 2;
                var size = RotateSize(b.Measurement.SizeX, b.Measurement.SizeZ, D(p, "Yaw"));
                var actual = new SimulationFarmH2외곽측정 { MinX = px - size.X / 2, MaxX = px + size.X / 2, MinZ = pz - size.Z / 2, MaxZ = pz + size.Z / 2 };
                var heights = Samples(actual).Select(pt => Sample(pt.X, pt.Z, r, surface)).ToArray();
                if (heights.Any(s => s.SlopeDegrees > r.Policy.MaximumSlopeDegrees)) throw Error("SlopeTooSteep");
                if (heights.Max(s => s.HeightMeters) - heights.Min(s => s.HeightMeters) > r.Policy.MaximumHeightSpreadMeters) throw Error("HeightSpreadExceeded");
                p["Bottom"] = heights.Max(s => s.HeightMeters) - r.CellWorldOriginYMeters - r.LocalOriginYMeters + r.Policy.GroundClearanceMeters;
            }
            var sourceInput = root["Input"]!;
            sourceInput["PatternRevision"] = Revision;
            sourceInput["Expansion"] = JsonSerializer.SerializeToNode(new {
                Revision, ParentCandidateHashSha256 = input.ParentRequest.ExpectedCandidateHashSha256,
                ParentInputHashSha256 = S(root, "InputHash"), StudyHashSha256 = input.StudyHashSha256,
                input.MeasurementSourceHashSha256, Player = input.Player, input.BarnCode, input.BarnSolidBounds,
                input.BarnSolidMeasurementHashSha256, PlayerCenterClearanceMeters = clearance, AccessLaneWidthMeters = lane,
                MinimumSpacingMeters = gap, ProductionAreaSquareMeters = 100, InteriorDisposition = "NotApplicable",
                SourceSurfaceRevision = surface.Revision, SurfaceHashSha256 = surface.HashSha256,
                Measurements = r.Bindings.OrderBy(x => x.SourcePlacementStableId, StringComparer.Ordinal)
                    .Select(x => new { x.SourcePlacementStableId, x.Measurement.AssetFingerprintSha256, x.Measurement.MeasurementHashSha256 }).ToArray() });
            root["InputHash"] = HashNode(sourceInput);
            root["ResultHash"] = "";
            root["ResultHash"] = HashCandidate(root.ToJsonString());
            r.CandidateJson = root.ToJsonString(); r.ExpectedCandidateHashSha256 = S(root, "ResultHash");
            if (surface.Revision != r.SurfaceRevision || surface.HashSha256 != r.SurfaceHashSha256) throw Error("SurfaceChangedDuringExpansion");
            return new SimulationFarmH2부지확장Result { Revision = Revision,
                ParentCandidateHashSha256 = input.ParentRequest.ExpectedCandidateHashSha256, StudyHashSha256 = input.StudyHashSha256,
                MeasurementSourceHashSha256 = input.MeasurementSourceHashSha256, PlayerCenterClearanceMeters = clearance,
                AccessLaneWidthMeters = lane, ReservationWidthMeters = width, ReservationDepthMeters = depth, CandidateRequest = r };
        }

        public static string HashCandidate(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            using var stream = new System.IO.MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                foreach (var p in root.EnumerateObject()) { writer.WritePropertyName(p.Name); if (p.Name == "ResultHash") writer.WriteStringValue(""); else p.Value.WriteTo(writer); }
                writer.WriteEndObject();
            }
            return Simulation세계자산CanonicalHash.Hash(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
        }
        public static string HashSolidMeasurement(SimulationFarmH2외곽측정 b, string fingerprint)
            => Simulation세계자산CanonicalHash.Hash(JsonSerializer.Serialize(new { b.MinX, b.MinZ, b.MaxX, b.MaxZ, AssetFingerprintSha256 = fingerprint }));
        private static void ValidateInput(SimulationFarmH2부지확장Request i, SimulationFarmH2PlacementRequest r, ISimulationFarmH2SurfaceReader surface)
        {
            if (i.StudyHashSha256 != StudyHash || !Sha(i.MeasurementSourceHashSha256) || !Sha(i.Player.EvidenceHashSha256)
                || string.IsNullOrWhiteSpace(i.Player.EvidenceRef) || i.Player.EvidenceKindCode != "ReadOnlyOpenDirtyEditor") throw Error("MeasuredProvenanceMissing");
            if (i.BarnCode != "Barn01" && i.BarnCode != "Barn02") throw Error("UnknownBarn");
            if (i.Player.UniformScale != 1 || r.UniformScale != 1 || r.Bindings.Any(b => b.Measurement.UniformScale != 1)) throw Error("NativeScaleRequired");
            if (!new[] { i.Player.RadiusMeters, i.Player.SkinWidthMeters, i.Player.ClickStopDistanceMeters, i.Player.HeightMeters,
                    i.Player.StepOffsetMeters, i.Player.SlopeLimitDegrees }.All(Finite)
                || i.Player.RadiusMeters <= 0 || i.Player.HeightMeters < 2 * i.Player.RadiusMeters || i.Player.SkinWidthMeters < 0
                || i.Player.ClickStopDistanceMeters < 0 || i.Player.StepOffsetMeters < 0 || i.Player.SlopeLimitDegrees <= 0 || i.Player.SlopeLimitDegrees >= 90)
                throw Error("PlayerMeasurementInvalid");
            if (r.UnitCode != "Meters" || r.AxisCode != "XRightYUpZForward" || !new[] { 0d, 90d, 180d, 270d }.Contains(r.RotationDegrees)) throw Error("CoordinateFrameInvalid");
            if (surface.Revision != r.SurfaceRevision || surface.HashSha256 != r.SurfaceHashSha256) throw Error("SurfaceFingerprintInvalid");
            foreach (var b in r.Bindings)
            {
                var m = b.Measurement;
                if (m.EvidenceKindCode != "MeasuredWrapper" || !m.ActiveRenderer || !Sha(m.AssetFingerprintSha256)
                    || m.MeasurementHashSha256 != SimulationFarmH2PlacementAdapter.ComputeMeasurementHash(m)) throw Error("MeasuredAssetRequired");
            }
        }
        private static SimulationFarmH2SurfaceSample Sample(double x, double z, SimulationFarmH2PlacementRequest r, ISimulationFarmH2SurfaceReader surface)
        {
            var pt = Rotate(x, z, r.RotationDegrees);
            var s = surface.Read(r.CellWorldOriginXMeters + r.LocalOriginXMeters + pt.X, r.CellWorldOriginZMeters + r.LocalOriginZMeters + pt.Z);
            if (s == null || !s.Supported || !s.PlacementAllowed || !Finite(s.HeightMeters) || !Finite(s.SlopeDegrees) || s.SlopeDegrees < 0) throw Error("SurfaceSupportMissingOrDenied");
            return s;
        }
        private static void EnsureCell(double x, double z, SimulationFarmH2PlacementRequest r)
        { var p = Rotate(x, z, r.RotationDegrees); if (Math.Abs(p.X + r.LocalOriginXMeters) > r.CellSizeMeters / 2 || Math.Abs(p.Z + r.LocalOriginZMeters) > r.CellSizeMeters / 2) throw Error("AccessReservationOutsideCell"); }
        private static System.Collections.Generic.IEnumerable<(double X, double Z)> Samples(SimulationFarmH2외곽측정 b)
        { for (var x = 0; x < 3; x++) for (var z = 0; z < 3; z++) yield return (b.MinX + (b.MaxX - b.MinX) * x / 2, b.MinZ + (b.MaxZ - b.MinZ) * z / 2); }
        private static (double X, double Z) Rotate(double x, double z, double yaw)
        { if (yaw == 0) return (x, z); if (yaw == 90) return (z, -x); if (yaw == 180) return (-x, -z); if (yaw == 270) return (-z, x); var a = yaw * Math.PI / 180; return (x * Math.Cos(a) + z * Math.Sin(a), -x * Math.Sin(a) + z * Math.Cos(a)); }
        private static (double X, double Z) RotateSize(double x, double z, double yaw)
        { var a = Rotate(x, z, yaw); var b = Rotate(x, -z, yaw); return (Math.Max(Math.Abs(a.X), Math.Abs(b.X)), Math.Max(Math.Abs(a.Z), Math.Abs(b.Z))); }
        private static bool Touches(SimulationFarmH2외곽측정 a, SimulationFarmH2외곽측정 b) => a.MinX <= b.MaxX && a.MaxX >= b.MinX && a.MinZ <= b.MaxZ && a.MaxZ >= b.MinZ;
        private static void ValidateBounds(SimulationFarmH2외곽측정 b)
        { if (!new[] { b.MinX, b.MinZ, b.MaxX, b.MaxZ }.All(Finite) || b.MinX >= b.MaxX || b.MinZ >= b.MaxZ) throw Error("SolidBoundsInvalid"); }
        private static SimulationFarmH2외곽측정 ReadBox(JsonNode n) => new() { MinX = D(n, "MinX"), MinZ = D(n, "MinZ"), MaxX = D(n, "MaxX"), MaxZ = D(n, "MaxZ") };
        private static JsonNode BoxNode(SimulationFarmH2외곽측정 b) => JsonSerializer.SerializeToNode(new { b.MinX, b.MinZ, b.MaxX, b.MaxZ, Valid = true })!;
        private static JsonNode Route(string from, string to, double width) => JsonSerializer.SerializeToNode(new { StableId = from + "->" + to, From = from, To = to, Width = width })!;
        private static string HashNode(JsonNode n) { using var d = JsonDocument.Parse(n.ToJsonString()); return Simulation세계자산CanonicalHash.Hash(JsonSerializer.Serialize(d.RootElement)); }
        private static string S(JsonNode n, string name) => n[name]!.GetValue<string>();
        private static double D(JsonNode n, string name) { var v = n[name]!.GetValue<double>(); if (!Finite(v)) throw Error("NumberInvalid"); return v; }
        private static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        private static bool Sha(string s) => s != null && s.Length == 64 && s.All(Uri.IsHexDigit);
        private static ArgumentException Error(string code) => new("FarmH2Expansion:" + code);
    }
}
