using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using static Ssalddel.Simulation.Application.Simulation배치적합성검사;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2, "동결 LS01 B를 원A와 같은 지면 문맥에서 검사하여 Environment 계획으로 변환한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행, Boundary = "단일자산 결속 후보·제공 표면 검사는 실제 Resolver/Scene/통행 성공이 아니다.")]
    public sealed class SimulationFarm경관고정배치Service
    {
        public const string Revision = "farm-landscape-fixed-conversion.r1";
        public const string DeltaFileHash = "3e2d3c7b395cb4b1622d5b7b38971cb84a66b69b1d3ca50acc4f8062c79c97de";
        public const string MeasurementFileHash = "63150068bb9b76e84565f4427ea442ad208a72f051e8e73c8d2deece52323c90";
        public const string SourceAFileHash = "027caabcf45a1ba013833476b741093857325004be519764fb84fecdeb5e1578";
        public const string SourceAResultHash = "2d9a5c43075d6f96075fca0d67ac60ef5d8d74595d0a55e1506d1023bde7a720";
        public const string DeltaInputHash = "e8c00fb8e51b70ad9a178889c7b09733bbb42483bb8c29663c77424b10afa0de";
        public const string DeltaOutputHash = "63a08b079c947b9370a32f8d45af0044e4fab16abbd80df683aa723ffe2eb704";

        public SimulationFarm경관고정배치Result Convert(SimulationFarm경관고정배치Request request, ISimulationFarmH2SurfaceReader surface)
        {
            if (request == null || request.BaseRequest == null || surface == null) throw Error("InputMissing");
            // 호출자 자료를 변경하지 않으며 변환 도중 외부 DTO 변경도 입력 사본에 전파하지 않는다.
            var r = Copy(request); var a = r.BaseRequest;
            if (Hash(r.DeltaJson) != DeltaFileHash) throw Error("DeltaFileHashMismatch");
            if (Hash(r.MeasurementsJson) != MeasurementFileHash) throw Error("MeasurementFileHashMismatch");
            if (Hash(a.CandidateJson) != SourceAFileHash || a.ExpectedCandidateHashSha256 != SourceAResultHash) throw Error("SourceAHashMismatch");
            using var deltaDoc = JsonDocument.Parse(r.DeltaJson);
            using var measurementDoc = JsonDocument.Parse(r.MeasurementsJson);
            using var aDoc = JsonDocument.Parse(a.CandidateJson);
            var delta = deltaDoc.RootElement; var candidate = delta.GetProperty("candidate");
            if (S(delta,"schema") != "ls01-decorative-delta-envelope.r1"
                || S(candidate,"revision") != "farm-landscape-ls01.flat-ab.r1"
                || S(candidate,"status") != "UnapprovedCandidate" || candidate.GetProperty("coreConsumable").GetBoolean())
                throw Error("UnsupportedCandidate");
            if (S(delta,"inputHash") != DeltaInputHash || CanonHash(delta.GetProperty("canonicalInput")) != DeltaInputHash
                || S(delta,"outputHash") != DeltaOutputHash || CanonHash(candidate) != DeltaOutputHash) throw Error("DeltaCanonicalHashMismatch");
            if (a.Bindings == null || a.Bindings.Any(x => x == null || x.Measurement == null || x.Measurement.UniformScale != 1)
                || a.UniformScale != 1) throw Error("NativeScaleRequired");
            ValidateFrozenPolicy(a, aDoc.RootElement.GetProperty("Input").GetProperty("Policy"));
            if ((r.SurfaceEvidenceKindCode != "SyntheticFixture" && r.SurfaceEvidenceKindCode != "ProvidedSurfaceSnapshot")
                || string.IsNullOrWhiteSpace(r.SurfaceEvidenceRef)) throw Error("SurfaceEvidenceMissing");
            if (r.Bindings == null || r.Bindings.Any(x=>x==null) || r.Bindings.Length != 6
                || r.Bindings.Select(x=>x.VisualKey).Distinct(StringComparer.Ordinal).Count()!=6
                || string.IsNullOrWhiteSpace(r.BindingRevision) || ComputeBindingHash(r)!=r.BindingHashSha256) throw Error("BindingSealInvalid");
            ValidateContext(r);
            var items = candidate.GetProperty("items").EnumerateArray().OrderBy(x=>S(x,"stableId"),StringComparer.Ordinal).ToArray();
            var records = measurementDoc.RootElement.GetProperty("records").EnumerateArray().ToArray();
            var bindingMap = r.Bindings.ToDictionary(x=>x.VisualKey,StringComparer.Ordinal);
            if (!bindingMap.Keys.OrderBy(x=>x,StringComparer.Ordinal).SequenceEqual(items.Select(x=>S(x,"visualKey")).Distinct().OrderBy(x=>x,StringComparer.Ordinal)))
                throw Error("UnknownVisualKey");
            // 원A 측정 계보 역시 후보가 봉인한 실측을 유지한다.
            var originalMeasurements = candidate.GetProperty("lineage").GetProperty("playerEvidence").GetProperty("Measurements").EnumerateArray();
            foreach (var original in originalMeasurements)
            {
                var b = a.Bindings.SingleOrDefault(x=>x.SourcePlacementStableId == S(original,"SourcePlacementStableId"));
                if (b == null || b.Measurement.MeasurementHashSha256 != S(original,"MeasurementHashSha256")
                    || b.Measurement.AssetFingerprintSha256 != S(original,"AssetFingerprintSha256")) throw Error("SourceAMeasurementMismatch");
            }
            var session = new 표면관찰Session(a,surface);
            var baseResult = new SimulationFarmH2PlacementAdapter().ConvertWithObservations(a,surface,session,out var aBoxes);
            var placements = new List<Simulation세계자산PlacementSnapshot>();
            var snapshots = new List<SimulationFarm경관외곽Snapshot>();
            var bBoxes = new Dictionary<string,Box>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                var id=S(item,"stableId"); var visual=S(item,"visualKey"); var binding=bindingMap[visual];
                if (binding.SourceObjectCount!=1 || binding.CompositionKey != "candidate:ls01:single-prefab:"+S(item,"prefabGuid")
                    || binding.PrefabGuid!=S(item,"prefabGuid") || binding.PrefabHashSha256!=S(item,"prefabSha256")
                    || binding.MetaHashSha256!=S(item,"metaSha256")) throw Error("SinglePrefabBindingMismatch");
                var record=records.Single(x=>S(x,"guid")==binding.PrefabGuid);
                if (S(record,"path")!=S(item,"prefabPath") || S(record,"prefabHash")!=binding.PrefabHashSha256
                    || S(record,"metaHash")!=binding.MetaHashSha256) throw Error("MeasurementBindingMismatch");
                var pivot=Vector(item,"pivotLocal"); var center=Vector(record,"boundsCenter"); var size=Vector(record,"boundsSize");
                var position=Transform(pivot[0],pivot[2],a); var y=a.LocalOriginYMeters+pivot[1];
                var yaw=Normalize(item.GetProperty("yawDegrees").GetDouble()+a.RotationDegrees);
                var renderer=Envelope(center,size,position.X,position.Z,yaw);
                var bottom=y+center[1]-size[1]/2;
                var solid=record.GetProperty("colliders").EnumerateArray()
                    .Where(c=>c.GetProperty("active").GetBoolean() && c.GetProperty("enabled").GetBoolean() && !c.GetProperty("isTrigger").GetBoolean()).ToArray();
                var all=new List<Box>{renderer};
                foreach(var c in solid)
                {
                    if(c.TryGetProperty("meshNull",out var missing) && missing.ValueKind == JsonValueKind.True) throw Error("ColliderMeshMissing");
                    all.Add(Envelope(Vector(c,"center"),Vector(c,"size"),position.X,position.Z,yaw));
                }
                var conservative=new Box(all.Min(x=>x.MinX),all.Min(x=>x.MinZ),all.Max(x=>x.MaxX),all.Max(x=>x.MaxZ));
                CellContains(conservative,a);
                // 바닥은 Renderer의 가시 접지, XZ는 전체 LOD와 유효 Collider의 보수적 점유다.
                ValidateSupport(conservative,bottom,id,a,session.Read);
                bBoxes.Add(id,conservative);
                placements.Add(new Simulation세계자산PlacementSnapshot {
                    PlacementStableId=id, OwnerCellStableId=a.OwnerCellStableId,
                    PlacementKindCode=Simulation세계자산배치Codes.Environment, LayerCode="FarmLandscapeLS01",
                    CategoryCode=S(item,"assetFamily"), CompositionKey=binding.CompositionKey,
                    AuthorityKindCode=Simulation세계자산배치Codes.AmbientPresentation,
                    PersistenceKindCode=Simulation세계자산배치Codes.DerivedPersistent,
                    StateCode="UnapprovedCandidate", LocalXMeters=position.X,LocalYMeters=y,LocalZMeters=position.Z,
                    RotationDegrees=yaw,UniformScale=1,FixedAnchor=true,CollisionEligible=solid.Length>0,PresentationOnly=true
                });
                snapshots.Add(new SimulationFarm경관외곽Snapshot {
                    PlacementStableId=id,VisualKey=visual,PrefabGuid=binding.PrefabGuid,RendererBottomMeters=bottom,
                    AllLodRendererBounds=Snapshot(id,"AllLodRenderer",renderer),
                    ConservativeBounds=Snapshot(id,"RendererAndSolidCollider",conservative),ActiveSolidColliderCount=solid.Length
                });
            }
            var boxes=aBoxes.Values.Concat(bBoxes.Values).ToArray();
            ValidateSpacing(boxes,a.Policy.MinimumSpacingMeters);
            // 생산100㎡ 표시와 별개로 원A가 예약한 작업 외곽 전체를 장식으로 침범하지 않는다.
            foreach(var p in aDoc.RootElement.GetProperty("Placements").EnumerateArray())
                ValidateObstacle(Transform(Box.From(p.GetProperty("Bounds")),a),bBoxes.Values.ToArray(),a.Policy.MinimumSpacingMeters);
            foreach(var area in baseResult.ReservedAreas)
                ValidatePreserved(AreaBox(area),bBoxes.Values.ToArray());
            foreach(var area in r.AdditionalProtectedAreas) ValidatePreserved(AreaBox(area),boxes);
            var obstacles=aDoc.RootElement.GetProperty("Input").GetProperty("Obstacles").EnumerateArray().Select(x=>Transform(Box.From(x),a))
                .Concat(r.ExistingObstacles.Select(AreaBox)).ToArray();
            foreach(var obstacle in obstacles) ValidateObstacle(obstacle,boxes,a.Policy.MinimumSpacingMeters);
            var owners=a.Bindings.Select(b=>(OwnerId:b.PlacementStableId,Bounds:aBoxes[b.SourcePlacementStableId]))
                .Concat(bBoxes.Select(b=>(OwnerId:b.Key,Bounds:b.Value))).ToArray();
            var anchors=baseResult.Anchors.ToDictionary(x=>x.SourceAnchorStableId,StringComparer.Ordinal);
            foreach(var route in baseResult.Routes)
                ValidateRouteSegment(anchors[route.FromSourceAnchorStableId],anchors[route.ToSourceAnchorStableId],route,
                    owners,baseResult.ReservedAreas.Concat(r.AdditionalProtectedAreas).ToArray(),obstacles,a,session.Read);
            session.ValidateRevision();
            var plan=Copy(baseResult.Plan);
            plan.RuleRevision=Revision;
            plan.Placements=plan.Placements.Concat(placements).OrderBy(x=>x.PlacementStableId,StringComparer.Ordinal).ToArray();
            if(plan.Placements.Select(x=>x.PlacementStableId).Distinct().Count()!=13) throw Error("CombinedIdentityCollision");
            plan.AssetPlacementPlanHashSha256=Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(plan);
            var result=new SimulationFarm경관고정배치Result {
                ServiceRevision=Revision,SurfaceEvidenceKindCode=r.SurfaceEvidenceKindCode,SurfaceEvidenceRef=r.SurfaceEvidenceRef,
                DeltaFileHashSha256=DeltaFileHash,DeltaInputHashSha256=DeltaInputHash,DeltaOutputHashSha256=DeltaOutputHash,
                MeasurementFileHashSha256=MeasurementFileHash,BindingHashSha256=r.BindingHashSha256,
                ConversionInputHashSha256=HashObject(new { BaseInputHash=baseResult.ConversionInputHashSha256,
                    DeltaFileHash,MeasurementFileHash,r.BindingRevision,r.BindingHashSha256,r.ContextRevision,r.ContextHashSha256,
                    r.SurfaceEvidenceKindCode,r.SurfaceEvidenceRef }),
                SurfaceSamplesHashSha256=HashObject(session.Observations),BaseResult=baseResult,Plan=plan,
                Envelopes=snapshots.ToArray(),SurfaceSamples=new SortedDictionary<string,string>(session.Observations,StringComparer.Ordinal)
            };
            result.ResultHashSha256=ResultHash(result);
            return result;
        }

        public Simulation분리세계자산배치Result PartitionFrozen(SimulationFarm경관고정배치Result result)
        {
            if(result==null || result.ServiceRevision!=Revision || result.StatusCode!="UnapprovedCandidate"
                || result.ValidationCode!="ValidatedAgainstProvidedSurface" || result.ActualTraversalVerified || result.ActualResolverVerified
                || ResultHash(result)!=result.ResultHashSha256
                || Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(result.Plan)!=result.Plan.AssetPlacementPlanHashSha256)
                throw Error("FrozenOutputHashMismatch");
            return new Simulation결정적세계자산배치Plan분리Service().Partition(Copy(result.Plan));
        }
        public static string ComputeBindingHash(SimulationFarm경관고정배치Request r)
            => HashObject(new {r.BindingRevision,Bindings=r.Bindings.OrderBy(x=>x.VisualKey,StringComparer.Ordinal).ToArray()});
        public static string ComputeContextHash(SimulationFarm경관고정배치Request r)
            => HashObject(new {r.ContextRevision,ExistingObstacles=r.ExistingObstacles.OrderBy(x=>x.SourceStableId,StringComparer.Ordinal).ToArray(),
                AdditionalProtectedAreas=r.AdditionalProtectedAreas.OrderBy(x=>x.SourceStableId,StringComparer.Ordinal).ToArray()});
        private static void ValidateContext(SimulationFarm경관고정배치Request r)
        {
            if(r.ExistingObstacles==null || r.AdditionalProtectedAreas==null || string.IsNullOrWhiteSpace(r.ContextRevision)
                || r.ExistingObstacles.Concat(r.AdditionalProtectedAreas).Any(x=>x==null || string.IsNullOrWhiteSpace(x.SourceStableId)
                    || (x.RoleCode!="ExistingObstacle" && x.RoleCode!="AdditionalProtection"))
                || r.ExistingObstacles.Concat(r.AdditionalProtectedAreas).Select(x=>x.SourceStableId).Distinct().Count()!=r.ExistingObstacles.Length+r.AdditionalProtectedAreas.Length)
                throw Error("SpatialContextInvalid");
            if(ComputeContextHash(r)!=r.ContextHashSha256) throw Error("SpatialContextHashMismatch");
            foreach(var area in r.ExistingObstacles.Concat(r.AdditionalProtectedAreas)) CellContains(AreaBox(area),r.BaseRequest);
        }
        private static void ValidateFrozenPolicy(SimulationFarmH2PlacementRequest a, JsonElement source)
        {
            var p = a.Policy;
            // 승인된 고정 B 비교는 원A 시험 한계를 그대로 소비한다. 새 게임 규칙을 만들지 않는다.
            if (p == null || !p.TrialOnly
                || p.MaximumSlopeDegrees != source.GetProperty("MaximumSlope").GetDouble()
                || p.MaximumHeightSpreadMeters != source.GetProperty("MaximumHeightSpread").GetDouble()
                || p.GroundClearanceMeters != source.GetProperty("GroundClearance").GetDouble()
                || p.BottomToleranceMeters != source.GetProperty("BottomTolerance").GetDouble()
                || p.MinimumSpacingMeters != source.GetProperty("MinimumSpacing").GetDouble()
                || p.MinimumRouteWidthMeters != source.GetProperty("RouteWidth").GetDouble()
                || p.RouteSampleStepMeters != source.GetProperty("RouteSampleStep").GetDouble()
                || p.MaximumRouteSlopeDegrees != source.GetProperty("MaximumRouteSlope").GetDouble()
                || p.MaximumRouteStepMeters != source.GetProperty("MaximumRouteStep").GetDouble())
                throw Error("FrozenTrialPolicyMismatch");
        }
        private static SimulationFarmH2ReservedAreaSnapshot Snapshot(string id,string role,Box b)
            => new SimulationFarmH2ReservedAreaSnapshot {SourceStableId=id,RoleCode=role,MinX=b.MinX,MinZ=b.MinZ,MaxX=b.MaxX,MaxZ=b.MaxZ};
        private static Box AreaBox(SimulationFarmH2ReservedAreaSnapshot a)=>new Box(a.MinX,a.MinZ,a.MaxX,a.MaxZ);
        private static Box Envelope(double[] center,double[] size,double x,double z,double yaw)
        {
            var offset=Rotate(center[0],center[2],yaw); var s=RotatedSize(size[0],size[2],yaw);
            return new Box(x+offset.X-s.X/2,z+offset.Z-s.Z/2,x+offset.X+s.X/2,z+offset.Z+s.Z/2);
        }
        private static double[] Vector(JsonElement e,string key)
        {
            var v=e.GetProperty(key).EnumerateArray().Select(x=>x.GetDouble()).ToArray();
            if(v.Length!=3 || !v.All(Finite)) throw Error("VectorInvalid"); return v;
        }
        private static string ResultHash(SimulationFarm경관고정배치Result r)
        { var copy=Copy(r);copy.ResultHashSha256="";return HashObject(copy); }
        private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
        private static string S(JsonElement e,string key)=>e.GetProperty(key).GetString() ?? "";
        private static string Hash(string value)=>Simulation세계자산CanonicalHash.Hash(value);
        private static string HashObject(object value) { using var d=JsonDocument.Parse(JsonSerializer.Serialize(value));return CanonHash(d.RootElement); }
        private static string CanonHash(JsonElement e)=>Hash(Canonical(e));
        // XYZ/재질 슬롯 배열은 의미 순서를 보존한다. 원A의 배열정렬 정규형을 변경하거나 재사용하지 않는다.
        private static string Canonical(JsonElement e)
        {
            if(e.ValueKind==JsonValueKind.Object) {
                var p=e.EnumerateObject().ToArray();
                if(p.Select(x=>x.Name).Distinct(StringComparer.Ordinal).Count()!=p.Length) throw Error("DuplicateProperty");
                return "{"+string.Join(",",p.OrderBy(x=>x.Name,StringComparer.Ordinal).Select(x=>JsonSerializer.Serialize(x.Name)+":"+Canonical(x.Value)))+"}";
            }
            if(e.ValueKind==JsonValueKind.Array) return "["+string.Join(",",e.EnumerateArray().Select(Canonical))+"]";
            return e.ValueKind==JsonValueKind.String ? JsonSerializer.Serialize(e.GetString()) : e.GetRawText();
        }
        private static ArgumentException Error(string code)=>new ArgumentException("FarmLandscape:"+code);
    }
}
