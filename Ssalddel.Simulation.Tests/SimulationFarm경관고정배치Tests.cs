using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit.Abstractions;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3, "LS01 고정 변환·지지 거부·단일자산·LH 불변을 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3결정성검증, Boundary = "지면은 명시적 합성 Fixture이며 실제 Scene/통행을 검증하지 않는다.")]
public sealed class SimulationFarm경관고정배치Tests
{
    private readonly ITestOutputHelper output;
    public SimulationFarm경관고정배치Tests(ITestOutputHelper output)=>this.output=output;
    private static string Read(string name)=>File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Fixtures","FarmLandscapeLS01",name));
    private static string Hash(string s)=>Simulation세계자산CanonicalHash.Hash(s);
    private static SimulationFarm경관고정배치Request Input()
    {
        var a=SimulationFarmH2부지확장Tests.Input(0).ParentRequest;
        a.CandidateJson=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Fixtures","FarmH2ExpandedR2","01-Barn01.plan.json"));
        a.ExpectedCandidateHashSha256=SimulationFarm경관고정배치Service.SourceAResultHash;
        using var support=JsonDocument.Parse(Read("support.flat.fixture.r1.json"));
        a.SurfaceRevision=support.RootElement.GetProperty("revision").GetString()!;
        a.SurfaceHashSha256=Hash(Read("support.flat.fixture.r1.json"));
        var r=new SimulationFarm경관고정배치Request {
            BaseRequest=a,DeltaJson=Read("B.delta.r1.json"),MeasurementsJson=Read("measurements.r1.json"),
            Bindings=JsonSerializer.Deserialize<SimulationFarm경관단일자산Binding[]>(Read("single-prefab-bindings.r1.json"))!,
            BindingRevision="ls01-single-prefab-candidates.r1",ContextRevision="ls01-empty-external-context.r1",
            SurfaceEvidenceKindCode="SyntheticFixture",SurfaceEvidenceRef="Fixtures/FarmLandscapeLS01/support.flat.fixture.r1.json"
        };
        Reseal(r); return r;
    }
    private static void Reseal(SimulationFarm경관고정배치Request r) {
        r.BindingHashSha256=SimulationFarm경관고정배치Service.ComputeBindingHash(r);
        r.ContextHashSha256=SimulationFarm경관고정배치Service.ComputeContextHash(r);
    }
    private sealed class Surface : ISimulationFarmH2SurfaceReader
    {
        private readonly SimulationFarmH2PlacementRequest r;
        private readonly JsonElement fixture;
        private readonly string mode;
        private bool seenB;
        public Surface(SimulationFarmH2PlacementRequest r,string mode="normal") {
            this.r=r;this.mode=mode;using var doc=JsonDocument.Parse(SimulationFarm경관고정배치Tests.Read("support.flat.fixture.r1.json"));fixture=doc.RootElement.Clone();
        }
        public string Revision=>mode=="revision"&&seenB?"changed":r.SurfaceRevision;
        public string HashSha256=>r.SurfaceHashSha256;
        public SimulationFarmH2SurfaceSample Read(double wx,double wz)
        {
            var x=wx-r.CellWorldOriginXMeters;var z=wz-r.CellWorldOriginZMeters;
            // B-only 샘플에 실패 주입: 원A 검사를 먼저 통과했음을 분리한다.
            var b = mode == "grass-missing" ? x > 3 && x < 6 && z > 32 && z < 36 : x < -13 && z > 18;seenB|=b;
            var inRange=x>=fixture.GetProperty("minX").GetDouble()&&x<=fixture.GetProperty("maxX").GetDouble()
                &&z>=fixture.GetProperty("minZ").GetDouble()&&z<=fixture.GetProperty("maxZ").GetDouble();
            var height=fixture.GetProperty("height").GetDouble()+r.CellWorldOriginYMeters+r.LocalOriginYMeters;
            if(b) height+=mode switch {"buried"=>.3,"floating"=>-.3,"spread"=>x< -16?.3:0,"nan"=>double.NaN,"infinity"=>double.PositiveInfinity,_=>0};
            if(mode=="cross"&&seenB&&!b)height+=.001;
            return new() {Supported=inRange&&!(b&&(mode=="missing" || mode=="grass-missing")),PlacementAllowed=!(b&&mode=="denied"),
                HeightMeters=height,SlopeDegrees=b&&mode=="steep"?30:fixture.GetProperty("slope").GetDouble()};
        }
    }
    private static SimulationFarm경관고정배치Result Convert(SimulationFarm경관고정배치Request r,string mode="normal")
        =>new SimulationFarm경관고정배치Service().Convert(r,new Surface(r.BaseRequest,mode));

    [Fact]
    public void 평지_A5B8_고정계획과_원A_계보를보존한다()
    {
        var r=Input();var before=JsonSerializer.Serialize(r);var result=Convert(r);
        Assert.Equal(before,JsonSerializer.Serialize(r));
        Assert.Equal(13,result.Plan.Placements.Length);
        Assert.Equal(JsonSerializer.Serialize(new SimulationFarmH2PlacementAdapter().Convert(r.BaseRequest,new Surface(r.BaseRequest))),JsonSerializer.Serialize(result.BaseResult));
        foreach(var p in result.BaseResult.Plan.Placements)
            Assert.Equal(JsonSerializer.Serialize(p),JsonSerializer.Serialize(result.Plan.Placements.Single(x=>x.PlacementStableId==p.PlacementStableId)));
        Assert.Equal("SyntheticFixture",result.SurfaceEvidenceKindCode);
        Assert.False(result.ActualResolverVerified);Assert.False(result.ActualTraversalVerified);
        Assert.Equal("UnapprovedCandidate",result.StatusCode);
        Assert.NotEqual(result.Plan.AssetPlacementPlanHashSha256,result.BaseResult.Plan.AssetPlacementPlanHashSha256);
        Assert.Equal(SimulationFarm경관고정배치Service.DeltaFileHash,Hash(r.DeltaJson));
        Assert.Equal(SimulationFarm경관고정배치Service.MeasurementFileHash,Hash(r.MeasurementsJson));
        output.WriteLine("RESULT "+result.ResultHashSha256+" PLAN "+result.Plan.AssetPlacementPlanHashSha256+" INPUT "+result.ConversionInputHashSha256+" SAMPLES "+result.SurfaceSamples.Count);
    }

    [Theory]
    [InlineData(0)] [InlineData(90)] [InlineData(180)] [InlineData(270)]
    public void 고정pivot_yaw_nativeScale과_H1없는Environment(int angle)
    {
        var r=Input();r.BaseRequest.RotationDegrees=angle;
        var result=Convert(r);using var doc=JsonDocument.Parse(r.DeltaJson);
        foreach(var item in doc.RootElement.GetProperty("candidate").GetProperty("items").EnumerateArray())
        {
            var id=item.GetProperty("stableId").GetString();
            var p=result.Plan.Placements.Single(x=>x.PlacementStableId==id);
            var v=item.GetProperty("pivotLocal");var rad=angle*Math.PI/180;
            Assert.Equal(v[0].GetDouble()*Math.Cos(rad)+v[2].GetDouble()*Math.Sin(rad),p.LocalXMeters,9);
            Assert.Equal(-v[0].GetDouble()*Math.Sin(rad)+v[2].GetDouble()*Math.Cos(rad),p.LocalZMeters,9);
            Assert.Equal(v[1].GetDouble(),p.LocalYMeters);
            Assert.Equal((item.GetProperty("yawDegrees").GetDouble()+angle)%360,p.RotationDegrees);
            Assert.Equal(1,p.UniformScale);Assert.Equal("",p.H1StableId);Assert.Equal("",p.ParentPlacementStableId);
            Assert.Equal(r.BaseRequest.OwnerCellStableId,p.OwnerCellStableId);Assert.Equal("Environment",p.PlacementKindCode);
            Assert.Equal("AmbientPresentation",p.AuthorityKindCode);Assert.True(p.PresentationOnly);
            if(angle==0) {
                var measured=result.Envelopes.Single(x=>x.PlacementStableId==id).ConservativeBounds;
                var frozen=item.GetProperty("conservativeFootprint");
                Assert.Equal(frozen.GetProperty("MinX").GetDouble(),measured.MinX,7);
                Assert.Equal(frozen.GetProperty("MinZ").GetDouble(),measured.MinZ,7);
                Assert.Equal(frozen.GetProperty("MaxX").GetDouble(),measured.MaxX,7);
                Assert.Equal(frozen.GetProperty("MaxZ").GetDouble(),measured.MaxZ,7);
            }
        }
        Assert.Equal(5,result.Envelopes.Count(x=>x.ActiveSolidColliderCount==0));
        Assert.All(result.Envelopes,x=>Assert.Equal("NotEvaluatedAtRuntime",x.ActiveLodStatusCode));
    }

    [Fact]
    public void 동일입력_순서변형_문화권_결정성()
    {
        var r=Input();var first=Convert(r);var prior=CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("fr-FR");
            r.Bindings=r.Bindings.Reverse().ToArray();r.BaseRequest.Bindings=r.BaseRequest.Bindings.Reverse().ToArray();
            r.BaseRequest.ResolvedCompositionKeys=r.BaseRequest.ResolvedCompositionKeys.Reverse().ToArray();
            Reseal(r);Assert.Equal(first.ResultHashSha256,Convert(r).ResultHashSha256);
            Assert.Equal(first.ResultHashSha256,Convert(Input()).ResultHashSha256);
        } finally {CultureInfo.CurrentCulture=prior;}
    }

    [Theory]
    [InlineData("delta","DeltaFileHashMismatch")] [InlineData("inputhash","DeltaFileHashMismatch")]
    [InlineData("outputhash","DeltaFileHashMismatch")] [InlineData("duplicate","DeltaFileHashMismatch")]
    [InlineData("vector","DeltaFileHashMismatch")] [InlineData("measurement","MeasurementFileHashMismatch")]
    [InlineData("source","SourceAHashMismatch")] [InlineData("bindinghash","BindingSealInvalid")]
    [InlineData("missingbinding","BindingSealInvalid")] [InlineData("duplicatebinding","BindingSealInvalid")]
    [InlineData("unknown","UnknownVisualKey")] [InlineData("guid","SinglePrefabBindingMismatch")]
    [InlineData("prefab","SinglePrefabBindingMismatch")] [InlineData("meta","SinglePrefabBindingMismatch")]
    [InlineData("multi","SinglePrefabBindingMismatch")] [InlineData("legacy","SinglePrefabBindingMismatch")]
    [InlineData("scale","NativeScaleRequired")] [InlineData("h","CellOrHOwnershipMissing")]
    [InlineData("context","SpatialContextHashMismatch")] [InlineData("evidence","SurfaceEvidenceMissing")]
    [InlineData("spacing","FrozenTrialPolicyMismatch")] [InlineData("policy","FrozenTrialPolicyMismatch")]
    [InlineData("a-measurement","SourceAMeasurementMismatch")]
    public void 손상_누락_위장_거부(string mode,string expected)
    {
        var r=Input();
        switch(mode) {
            case "delta":r.DeltaJson+=" ";break;
            case "inputhash":r.DeltaJson=r.DeltaJson.Replace(SimulationFarm경관고정배치Service.DeltaInputHash,new string('0',64));break;
            case "outputhash":r.DeltaJson=r.DeltaJson.Replace(SimulationFarm경관고정배치Service.DeltaOutputHash,new string('0',64));break;
            case "duplicate":r.DeltaJson=r.DeltaJson.Replace("\"schema\":","\"schema\":\"duplicate\",\"schema\":");break;
            case "vector":var j=JsonNode.Parse(r.DeltaJson)!;j["candidate"]!["items"]![0]!["pivotLocal"]![0]=0;r.DeltaJson=j.ToJsonString();break;
            case "measurement":r.MeasurementsJson+=" ";break;
            case "source":r.BaseRequest.CandidateJson+=" ";break;
            case "bindinghash":r.BindingHashSha256=new string('0',64);break;
            case "missingbinding":r.Bindings=r.Bindings.Skip(1).ToArray();Reseal(r);break;
            case "duplicatebinding":r.Bindings[1]=r.Bindings[0];Reseal(r);break;
            case "unknown":r.Bindings[0].VisualKey="unknown";Reseal(r);break;
            case "guid":r.Bindings[0].PrefabGuid=new string('0',32);Reseal(r);break;
            case "prefab":r.Bindings[0].PrefabHashSha256=new string('0',64);Reseal(r);break;
            case "meta":r.Bindings[0].MetaHashSha256=new string('0',64);Reseal(r);break;
            case "multi":r.Bindings[0].SourceObjectCount=2;Reseal(r);break;
            case "legacy":r.Bindings[0].CompositionKey="farm:수목완충지:A";Reseal(r);break;
            case "scale":r.BaseRequest.UniformScale=.5;break;
            case "h":r.BaseRequest.H2StableId="";break;
            case "context":r.ContextHashSha256=new string('0',64);break;
            case "evidence":r.SurfaceEvidenceKindCode="ActualSceneVerified";break;
            case "spacing":r.BaseRequest.Policy.MinimumSpacingMeters=0;break;
            case "policy":r.BaseRequest.Policy.TrialOnly=false;break;
            case "a-measurement":r.BaseRequest.Bindings[0].Measurement.SizeX=.1;
                r.BaseRequest.Bindings[0].Measurement.MeasurementHashSha256=SimulationFarmH2PlacementAdapter.ComputeMeasurementHash(r.BaseRequest.Bindings[0].Measurement);break;
        }
        Assert.Contains(expected,Assert.Throws<ArgumentException>(()=>Convert(r)).Message);
    }

    [Theory]
    [InlineData("missing","SurfaceSupportMissingOrDenied")] [InlineData("denied","SurfaceSupportMissingOrDenied")]
    [InlineData("steep","SlopeTooSteep")] [InlineData("spread","HeightSpreadExceeded")]
    [InlineData("buried","BuriedBottom")] [InlineData("floating","FloatingBottom")]
    [InlineData("nan","SurfaceSampleInvalid")] [InlineData("infinity","SurfaceSampleInvalid")]
    [InlineData("revision","SurfaceChangedDuringConversion")] [InlineData("cross","SurfaceChangedDuringConversion")]
    [InlineData("grass-missing","SurfaceSupportMissingOrDenied")]
    public void B지지실패와_A_B교차표본변경_거부(string mode,string expected)
    {
        var r=Input();var before=JsonSerializer.Serialize(r);
        Assert.Contains(expected,Assert.Throws<ArgumentException>(()=>Convert(r,mode)).Message);
        Assert.Equal(before,JsonSerializer.Serialize(r));
    }

    [Fact]
    public void 지면공급자없음_실행계획없음()=>Assert.Contains("InputMissing",
        Assert.Throws<ArgumentException>(()=>new SimulationFarm경관고정배치Service().Convert(Input(),null!)).Message);

    [Theory]
    [InlineData("obstacle","ExistingObjectConflict")] [InlineData("protection","PreservedAreaIntrusion")]
    [InlineData("route","RouteObstacle")] [InlineData("cell","OutsideOwnerCell")]
    public void Collider없는풀도_시각점유_보호_통로_셀을검사한다(string mode,string expected)
    {
        var r=Input();
        var region=new SimulationFarmH2ReservedAreaSnapshot {SourceStableId="test:external",RoleCode="ExistingObstacle",MinX=4,MaxX=5,MinZ=33,MaxZ=35};
        if(mode=="protection"){region.RoleCode="AdditionalProtection";r.AdditionalProtectedAreas=new[]{region};}
        else if(mode=="route"){region.MinX=-.1;region.MaxX=.1;region.MinZ=29;region.MaxZ=30;r.ExistingObstacles=new[]{region};}
        else if(mode=="cell"){r.BaseRequest.LocalOriginXMeters=35;}
        else r.ExistingObstacles=new[]{region};
        Reseal(r);Assert.Contains(expected,Assert.Throws<ArgumentException>(()=>Convert(r)).Message);
    }

    [Fact]
    public void Partition_LH_전이_재추첨없음_입력alias분리()
    {
        var result=Convert(Input());var before=JsonSerializer.Serialize(result);
        var service=new SimulationFarm경관고정배치Service();var parts=service.PartitionFrozen(result);
        var lh=new SimulationLhAssetPlanLifecycleService();var state=lh.Prepare(parts);
        foreach(var target in new[]{"Active","Cached","Active","Released","Prepared"}) {
            state=lh.Transition(state,new(){ExpectedLifecycleRevision=state.LifecycleRevision,TargetStateCode=target});
            Assert.Equal(result.Plan.AssetPlacementPlanHashSha256,state.SourceCombinedPlanHashSha256);
        }
        Assert.Equal(before,JsonSerializer.Serialize(result));
        parts.ExteriorPlan.Placements[0].LocalXMeters=999;
        Assert.Equal(before,JsonSerializer.Serialize(result));
        result.Plan.Placements[0].LocalXMeters=999;
        Assert.Contains("FrozenOutputHashMismatch",Assert.Throws<ArgumentException>(()=>service.PartitionFrozen(result)).Message);
    }

    [Fact]
    public void 비영점_Cellworld와_local원점_입력그대로투영한다()
    {
        var original=Convert(Input());var r=Input();
        r.BaseRequest.CellWorldOriginXMeters=2400;r.BaseRequest.CellWorldOriginYMeters=12;r.BaseRequest.CellWorldOriginZMeters=-1700;
        r.BaseRequest.LocalOriginXMeters=7;r.BaseRequest.LocalOriginYMeters=2;r.BaseRequest.LocalOriginZMeters=-5;
        var moved=Convert(r);
        foreach(var p in original.Plan.Placements) {
            var next=moved.Plan.Placements.Single(x=>x.PlacementStableId==p.PlacementStableId);
            Assert.Equal(p.LocalXMeters+7,next.LocalXMeters,9);Assert.Equal(p.LocalYMeters+2,next.LocalYMeters,9);
            Assert.Equal(p.LocalZMeters-5,next.LocalZMeters,9);
        }
        Assert.NotEqual(original.ConversionInputHashSha256,moved.ConversionInputHashSha256);
    }

    [Theory]
    [InlineData("traversal")] [InlineData("resolver")] [InlineData("samples")] [InlineData("base")] [InlineData("bounds")]
    public void 출력근거변조와_성공표시위장_거부(string field)
    {
        var result=Convert(Input());
        switch(field) {
            case "traversal":result.ActualTraversalVerified=true;break;
            case "resolver":result.ActualResolverVerified=true;break;
            case "samples":result.SurfaceSamples[result.SurfaceSamples.Keys.First()]="999,0";break;
            case "base":result.BaseResult.Plan.Placements[0].LocalXMeters=999;break;
            case "bounds":result.Envelopes[0].ConservativeBounds.MinX=999;break;
        }
        Assert.Contains("FrozenOutputHashMismatch",Assert.Throws<ArgumentException>(()=>new SimulationFarm경관고정배치Service().PartitionFrozen(result)).Message);
    }
}
