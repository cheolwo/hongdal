using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>Farm에서 추출한 동일 연산. 지면/객체를 수정하거나 누락 입력을 생성하지 않는다.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2, "지지·간격·통로 검사를 단일 구현으로 공유한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행, Boundary = "표본 기반 적합성은 실제 조립/Player 통행 증거가 아니다.")]
    internal static class Simulation배치적합성검사
    {
        internal sealed class 표면관찰Session
        {
            private readonly SimulationFarmH2PlacementRequest request;
            private readonly ISimulationFarmH2SurfaceReader surface;
            internal readonly SortedDictionary<string,string> Observations = new SortedDictionary<string,string>(StringComparer.Ordinal);
            internal 표면관찰Session(SimulationFarmH2PlacementRequest request, ISimulationFarmH2SurfaceReader surface)
            { this.request=request; this.surface=surface; }
            internal SimulationFarmH2SurfaceSample Read(double x,double z)
            {
                var wx = request.CellWorldOriginXMeters + x;
                var wz = request.CellWorldOriginZMeters + z;
                var s = surface.Read(wx, wz);
                if (s == null || !s.Supported || !s.PlacementAllowed) throw Error("SurfaceSupportMissingOrDenied");
                if (!Finite(s.HeightMeters) || !Finite(s.SlopeDegrees) || s.SlopeDegrees < 0 || s.SlopeDegrees > 90) throw Error("SurfaceSampleInvalid");
                var key = F(wx) + "," + F(wz);
                var value = F(s.HeightMeters) + "," + F(s.SlopeDegrees);
                if (Observations.TryGetValue(key, out var old) && old != value) throw Error("SurfaceChangedDuringConversion");
                Observations[key] = value;
                return s;
            }
            internal void ValidateRevision()
            { if(surface.Revision != request.SurfaceRevision || surface.HashSha256 != request.SurfaceHashSha256) throw Error("SurfaceChangedDuringConversion"); }
        }
        internal static void ValidateReserved(Box reserved,Box actual,double tolerance,string id)
        { if(!reserved.Contains(actual,tolerance)) throw Error("MeasuredEnvelopeExceedsCandidate:"+id); }
        internal static void ValidateSupport(Box actual,double bottom,string id,SimulationFarmH2PlacementRequest request,
            Func<double,double,SimulationFarmH2SurfaceSample> Read)
        {
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

        }
        internal static void ValidateSpacing(Box[] boxes,double spacing)
        {
            for (var a = 0; a < boxes.Length; a++) for (var b = a + 1; b < boxes.Length; b++)
                if (boxes[a].Touches(boxes[b]) || boxes[a].Distance(boxes[b]) < spacing) throw Error("ObjectOverlapOrSpacing");
        }
        internal static void ValidatePreserved(Box area,Box[] boxes)
        { if(boxes.Any(area.Touches)) throw Error("PreservedAreaIntrusion"); }
        internal static void ValidateObstacle(Box obstacle,Box[] boxes,double spacing)
        { if(boxes.Any(b=>b.Touches(obstacle)||b.Distance(obstacle)<spacing)) throw Error("ExistingObjectConflict"); }
        internal static void ValidateRouteSegment(SimulationFarmH2AnchorSnapshot a,SimulationFarmH2AnchorSnapshot b,
            SimulationFarmH2RouteSnapshot route,IEnumerable<(string OwnerId,Box Bounds)> objects,
            SimulationFarmH2ReservedAreaSnapshot[] areas,IEnumerable<Box> obstacles,SimulationFarmH2PlacementRequest r,
            Func<double,double,SimulationFarmH2SurfaceSample> read)
        {
                var dx = b.LocalXMeters - a.LocalXMeters; var dz = b.LocalZMeters - a.LocalZMeters;
                var length = Math.Sqrt(dx * dx + dz * dz);
                if (!Finite(route.WidthMeters) || route.WidthMeters < r.Policy.MinimumRouteWidthMeters || length <= 0
                    || (Math.Abs(dx) > 1e-8 && Math.Abs(dz) > 1e-8)) throw Error("RouteGeometryInvalid");
                var corridor = Math.Abs(dx) < 1e-8 ? new Box(a.LocalXMeters - route.WidthMeters / 2, Math.Min(a.LocalZMeters, b.LocalZMeters), a.LocalXMeters + route.WidthMeters / 2, Math.Max(a.LocalZMeters, b.LocalZMeters))
                    : new Box(Math.Min(a.LocalXMeters, b.LocalXMeters), a.LocalZMeters - route.WidthMeters / 2, Math.Max(a.LocalXMeters, b.LocalXMeters), a.LocalZMeters + route.WidthMeters / 2);
                CellContains(corridor, r);
                foreach (var binding in objects)
                    if (binding.OwnerId != a.OwnerPlacementStableId && binding.OwnerId != b.OwnerPlacementStableId
                        && corridor.Touches(binding.Bounds)) throw Error("ProtectedRouteIntrusion");
                if (areas.Any(x => x.RoleCode != "WorkYard" && corridor.Touches(new Box(x.MinX, x.MinZ, x.MaxX, x.MaxZ)))) throw Error("RoutePreservedAreaIntrusion");
                if (obstacles.Any(corridor.Touches)) throw Error("RouteObstacle");
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

        }
        private static string F(double d)=>d.ToString("R",CultureInfo.InvariantCulture);
        internal static bool Finite(double d)=>!double.IsNaN(d)&&!double.IsInfinity(d);
        private static double D(JsonElement e,string key) { var d=e.GetProperty(key).GetDouble();if(!Finite(d))throw Error("NumberInvalid");return d; }
        private static ArgumentException Error(string code)=>new ArgumentException("FarmH2:"+code);
        internal static double Normalize(double d) => (d % 360 + 360) % 360;
        internal static (double X, double Z) Rotate(double x, double z, double degrees)
        {
            var angle = Normalize(degrees);
            if (angle == 0) return (x, z); if (angle == 90) return (z, -x); if (angle == 180) return (-x, -z); if (angle == 270) return (-z, x);
            var rad = angle * Math.PI / 180; return (x * Math.Cos(rad) + z * Math.Sin(rad), -x * Math.Sin(rad) + z * Math.Cos(rad));
        }
        internal static (double X, double Z) RotatedSize(double x, double z, double yaw)
        { var a = Rotate(x, z, yaw); var b = Rotate(x, -z, yaw); return (Math.Max(Math.Abs(a.X), Math.Abs(b.X)), Math.Max(Math.Abs(a.Z), Math.Abs(b.Z))); }
        internal static (double X, double Z) Transform(double x, double z, SimulationFarmH2PlacementRequest r)
        { var p = Rotate(x * r.UniformScale, z * r.UniformScale, r.RotationDegrees); return (p.X + r.LocalOriginXMeters, p.Z + r.LocalOriginZMeters); }
        internal static Box Transform(Box b, SimulationFarmH2PlacementRequest r)
        { var pts = b.Samples().Select(p => Transform(p.X, p.Z, r)).ToArray(); return new Box(pts.Min(p => p.X), pts.Min(p => p.Z), pts.Max(p => p.X), pts.Max(p => p.Z)); }

        internal static void CellContains(Box b, SimulationFarmH2PlacementRequest r)
        { if (!new Box(-r.CellSizeMeters / 2, -r.CellSizeMeters / 2, r.CellSizeMeters / 2, r.CellSizeMeters / 2).Contains(b, 0)) throw Error("OutsideOwnerCell"); }
        internal sealed class Box
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

