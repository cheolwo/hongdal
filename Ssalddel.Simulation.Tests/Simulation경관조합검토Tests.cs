using System.Security.Cryptography;
using System.Text.Json;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "D442/D443 순수 후보·관계·형상 검토 회귀.", Boundary = "합성 Fixture이며 실제 문·길·Player·격리 이미지 증거가 아니다.")]
public sealed class Simulation경관조합검토Tests
{
    const string Field = "farm:혼합 작물밭:A";
    static readonly string H = new('a', 64);
    static readonly string M = new('b', 64);

    [Fact]
    public void 실제9키_경사경계전달_출입은미확보()
    {
        var c = Catalog();
        var keys = new[] { "farm:헛간 작업마당:", "farm:혼합 작물밭:", "nature:숲 가장자리:" }
            .SelectMany(p => new[] { p + "A", p + "B", p + "C" }).ToArray();
        Assert.Equal(9, keys.Length);
        foreach (var key in keys)
        {
            var g = c.Entries.Single(e => e.CompositionKey == key);
            Assert.NotNull(g.MinimumSlopeDegrees); Assert.NotNull(g.MaximumSlopeDegrees);
            Assert.Equal(4, g.EdgeProfiles!.Count); Assert.Empty(g.Connectors);
            Assert.NotNull(g.AllowedNeighborTopologyCodes); Assert.NotNull(g.ForbiddenNeighborTopologyCodes);
        }
        Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(ManifestPath()))), c.SourceDocumentHashSha256);
    }

    [Fact]
    public void 필수왕복_같은패턴다른노드_통행검사만통과()
    {
        var (c, i) = Fixture(); var r = Review(c, i);
        Assert.Equal("ReadyForIsolatedReview", r.State);
        Has(r, "Reachability", "Passed"); Has(r, "ReturnReachability", "Passed");
        Assert.False(r.WorldApplied); Assert.False(r.ActualTraversalVerified);
        Assert.Equal(i.Placements[0].CompositionKey, i.Placements[1].CompositionKey);
        Assert.NotEqual(i.Placements[0].Id, i.Placements[1].Id);
    }

    [Theory]
    [InlineData("Optional", "NotApplicable")]
    [InlineData("Separated", "Passed")]
    [InlineData("Unknown", "NotInspected")]
    public void 길없는이웃_요구를따로판정(string requirement, string expected)
    {
        var (c, i) = Fixture(); i.Connections = [];
        i.Relations![0].Requirement = requirement; i.Relations[0].RequireReturn = false;
        i.Relations[0].Reason = "fixture:선택형 또는 경작구획 분리 근거";
        Has(Review(c, i), "TraversalPolicy", expected);
    }

    [Theory]
    [InlineData("Blocked", "Failed")]
    [InlineData("Disconnected", "Failed")]
    [InlineData("Unknown", "NotInspected")]
    [InlineData("Confirmed", "NotInspected")]
    public void 보고된관측만으로통과하지않음(string observed, string expected)
    {
        var (c, i) = Fixture(); i.Connections = []; i.TraversalSurveyComplete = false;
        i.Relations![0].Observation = observed;
        Has(Review(c, i), "Reachability", expected);
    }

    [Fact]
    public void 실제9키출입누락_필수관계는미검사()
    {
        var (c, i) = Fixture(); c.Entries.Single(g => g.CompositionKey == Field).Connectors = [];
        var r = Review(c, i); Has(r, "Connection", "NotInspected"); Has(r, "Reachability", "NotInspected");
        Assert.Equal("Incomplete", r.State);
    }

    [Fact]
    public void 배열순서독립_입력불변_세번째독립노드는전역연결불요()
    {
        var (c, i) = Fixture(); i.Placements = [.. i.Placements, Placement("unrelated", 35, 35)];
        var before = JsonSerializer.Serialize(i); var first = Review(c, i);
        Assert.Equal(before, JsonSerializer.Serialize(i));
        i.Placements = i.Placements.Reverse().ToArray();
        c.Entries = c.Entries.Reverse().ToArray();
        var second = Review(c, i);
        Assert.Equal(first.InputHash, second.InputHash); Assert.Equal(first.ResultHash, second.ResultHash);
        Assert.Equal("ReadyForIsolatedReview", second.State);
    }

    [Theory]
    [InlineData("node")][InlineData("edge")][InlineData("self")][InlineData("missing")][InlineData("kind")]
    public void 식별참조중복을거부(string defect)
    {
        var (c, i) = Fixture();
        switch (defect)
        {
            case "node": i.Placements[1].Id = "a"; break;
            case "edge": i.Relations = [i.Relations![0], i.Relations[0]]; break;
            case "self": i.Relations![0].Edge.ToNodeStableId = "a"; break;
            case "missing": i.Relations![0].Edge.ToNodeStableId = "absent"; break;
            case "kind": i.Relations![0].Edge.RelationCode = "decoration-road"; break;
        }
        Assert.Equal("Rejected", Review(c, i).State);
    }

    [Fact]
    public void 포함과통행분리_부모합계중복거부_포함순환거부()
    {
        var (c, i) = Fixture(); i.Connections = [];
        var parent = Placement("parent", 0, 0); parent.IsContainer = true; parent.Objects = [];
        parent.CompositionKey = "farm:헛간 작업마당:A";
        var child = Placement("child", 0, 0);
        i.Placements = [parent, child];
        i.Relations = [Relation("inside", "parent", "child", SimulationWorldLandscapeCompositionCodes.Contains)];
        var r = Review(c, i); Has(r, "Containment", "Passed");
        Assert.DoesNotContain(r.Findings, f => f.Rule == "Reachability" || f.Rule == "Spacing");
        parent.Objects = [Object("aggregate")]; Assert.Equal("Rejected", Review(c, i).State);
        parent.Objects = []; child.IsContainer = true; child.Objects = [];
        i.Relations = [.. i.Relations, Relation("cycle", "child", "parent", SimulationWorldLandscapeCompositionCodes.Contains)];
        Has(Review(c, i), "Containment", "Failed");
    }

    [Theory]
    [InlineData("adjacent")][InlineData("transitions-to")][InlineData("contains")]
    public void 비통행엣지는도달계산에사용불가(string kind)
    {
        var (c, i) = Fixture(); i.Relations![0].Edge.RelationCode = kind;
        i.Relations[0].RequireReturn = false;
        Assert.Equal("Rejected", Review(c, i).State); // Connection.RelationId가 통행 관계를 가리켜야 한다.
    }

    [Fact]
    public void 방향성복귀는선언된왕복만_통행순환정상()
    {
        var (c, i) = Fixture(); i.Connections[0].Bidirectional = false;
        Has(Review(c, i), "Reachability", "Passed"); Has(Review(c, i), "ReturnReachability", "Failed");
        i.Relations = [.. i.Relations!, Relation("back", "b", "a", "connects", "Optional")];
        i.Connections = [.. i.Connections, new() { Id = "return-path", RelationId = "back", From = "b", To = "a", Type = "farm-road", RouteSignature = "fixture-road", FromLocalDirection = "west", ToLocalDirection = "east" }];
        Has(Review(c, i), "ReturnReachability", "Passed");
        Assert.DoesNotContain(Review(c, i).Findings, f => f.Rule == "Containment" && f.State == "Failed");
    }

    [Fact]
    public void 필수요구는검사된대체통행경로사용_전역연결강제없음()
    {
        var (c, i) = Fixture();
        i.Relations = [.. i.Relations!, Relation("require-back", "b", "a", "connects")];
        var r = Review(c, i);
        Assert.Contains(r.Findings, f => f.Rule == "Reachability" && f.Target == "require-back" && f.State == "Passed");
    }

    [Fact]
    public void 명시통행금지와선언길충돌거부()
    {
        var (c, i) = Fixture(); i.Relations![0].TraversalForbidden = true; i.Relations[0].Reason = "fixture:접근금지";
        Has(Review(c, i), "TraversalPolicy", "Failed");
    }

    [Fact]
    public void 세번째물체가통로를막으면재검사실패_해당통행은도달불가()
    {
        var (c, i) = Fixture(); var before = Review(c, i);
        var blocker = Placement("third", 0, 0); blocker.Objects[0].Measurement.CenterZ = 0;
        i.Placements = [.. i.Placements, blocker];
        var r = Review(c, i); Has(r, "Route", "Failed"); Has(r, "Reachability", "Failed");
        Assert.NotEqual(before.InputHash, r.InputHash);
    }

    [Fact]
    public void 공간이웃은ID순서아님_농장정렬은시각거부아님()
    {
        var (c, i) = Fixture(); i.Placements = [i.Placements[0], Placement("aa-far", 35, 35), i.Placements[1]];
        var r = Review(c, i);
        Assert.Contains(r.Findings, f => f.Rule == "VariantRepeat" && f.Target == "a|b" && f.State == "Measurement");
        Assert.DoesNotContain(r.Findings, f => f.Rule == "VariantRepeat" && f.Target.Contains("aa-far"));
    }

    [Theory]
    [InlineData("height", "ConnectorHeight")][InlineData("width", "Connection")]
    [InlineData("direction", "Connection")][InlineData("buried", "Support")]
    [InlineData("floating", "Support")][InlineData("overlap", "Spacing")]
    public void 실제공통기하검사부정사례(string defect, string rule)
    {
        var (c, i) = Fixture();
        switch (defect)
        {
            case "height": i.Placements[1].ConnectorLocalHeight = 2; break;
            case "width": c.Entries.Single(g => g.CompositionKey == Field).Connectors[1].Width = 10; break;
            case "direction": i.Connections[0].ToLocalDirection = "east"; break;
            case "buried": i.Placements[0].Y = -1; break;
            case "floating": i.Placements[0].Y = 1; break;
            case "overlap": i.Placements[1].X = i.Placements[0].X; break;
        }
        Has(Review(c, i), rule, "Failed");
    }

    [Theory]
    [InlineData("diagonal")][InlineData("height")][InlineData("geometry")][InlineData("relations")]
    public void 미지원미관측을성공으로승격하지않음(string missing)
    {
        var (c, i) = Fixture();
        switch (missing)
        {
            case "diagonal": i.Placements[1].Z = .1; i.Placements[1].X = 8.8; break;
            case "height": i.Placements[1].ConnectorLocalHeight = null; break;
            case "geometry": i.Placements[1].GeometryComplete = false; break;
            case "relations": i.Relations = null; i.Connections = []; break;
        }
        Assert.NotEqual("ReadyForIsolatedReview", Review(c, i).State);
    }

    [Fact]
    public void 빈관계는선택승인아님_전역AccessRequired로선택형을막지않음()
    {
        var (c, i) = Fixture(); i.Connections = []; i.Relations = [];
        Has(Review(c, i), "ConnectionIntent", "NotInspected");
        i.Relations = [Relation("optional", "a", "b", "connects", "Optional")];
        c.Entries.Single(g => g.CompositionKey == Field).Connectors = [];
        Assert.True(i.Rules.AccessRequired); Assert.Equal("ReadyForIsolatedReview", Review(c, i).State);
    }

    [Fact]
    public void 구형검토는기존AccessRequired를유지()
    {
        var (c, i) = Fixture(); i.Revision = "pattern-composition-review.d442.r1";
        i.Relations = null; i.Connections = []; c.Entries.Single(g => g.CompositionKey == Field).Connectors = [];
        Has(Review(c, i), "Access", "NotInspected");
    }

    [Theory]
    [InlineData(90, false)][InlineData(270, true)][InlineData(180, false)]
    public void 회전반전의좌표와연결방향을같이변환(double yaw, bool mirror)
    {
        var (c, i) = Fixture();
        foreach (var p in i.Placements)
        {
            var x = mirror ? -p.X : p.X;
            (p.X, p.Z) = yaw == 90 ? (0d, -x) : yaw == 270 ? (0d, x) : (-x, 0d);
            p.Yaw = yaw; p.Mirrored = mirror;
        }
        Assert.Equal("ReadyForIsolatedReview", Review(c, i).State);
    }

    [Fact]
    public void 원문규칙변경감지와예산거부()
    {
        var (c, i) = Fixture(); i.GrammarSourceHash = new string('f', 64);
        Assert.Equal("Rejected", Review(c, i).State);
        i.GrammarSourceHash = c.SourceDocumentHashSha256;
        i.RulesHash = new string('f', 64);
        Assert.Equal("Rejected", new Simulation경관조합검토Service().Review(c, i, new Flat()).State);
        Assert.Throws<ArgumentException>(() => new Simulation경관조합검토Service().ReviewCandidates(c, Enumerable.Repeat(i, 65).ToArray(), new Flat()));
    }

    [Fact]
    public void 순수검토후구형조합해시불변()
    {
        var c = Catalog(); var (f, i) = Fixture(); _ = Review(f, i);
        var source = new PyeongchangFirstLandscapeSkeletonSource(new Layers());
        foreach (var (tile, expected) in new[] {
            ("kr5186:l2:700:1145", "6c8ecc50c0f1718560b06f6f4813513d7f91c8db7a54311b6e1e1b304a6f1734"),
            ("kr5186:l2:701:1144", "25dc9304d33d019324985664acc998353f87ae4603169f42b3c447bbd075528a") })
        {
            Assert.True(source.TryCreate(tile, out var skeleton, out _));
            Assert.Equal(expected, new SimulationWorldLandscapeGraphAssembler().Assemble(skeleton, c).GraphHashSha256);
        }
    }

    [Fact]
    public void 경사누락을0으로위장하지않고구형해시는보존()
    {
        var source = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(ManifestPath()))!;
        var entries = source["entries"]!.AsArray();
        var entry = entries.First(e => e!["compositionKey"]!.GetValue<string>() == Field)!.AsObject();
        Assert.Equal(0, entry["minimumSlopeDegrees"]!.GetValue<double>());
        entry.Remove("minimumSlopeDegrees");
        var temp=Path.Combine(Path.GetTempPath(),"d443-manifest-"+Guid.NewGuid().ToString("N")+".json");
        try
        {
            File.WriteAllText(temp,source.ToJsonString());
            Assert.True(new SimulationWorldLandscapeGrammarManifestReader(temp).TryRead(out var c,out var e),e);
            Assert.Null(c.Entries.Single(g=>g.CompositionKey==Field).MinimumSlopeDegrees);
            Assert.Equal(Catalog().CatalogHashSha256,c.CatalogHashSha256);
            Assert.NotEqual(Catalog().SourceDocumentHashSha256,c.SourceDocumentHashSha256);
        }
        finally { File.Delete(temp); }
    }

    [Theory]
    [InlineData("missing", "NotInspected")][InlineData("forbidden", "Failed")][InlineData("transition", "Failed")]
    public void 경계호환과완충조건은통행과별도(string mode,string expected)
    {
        var (c,i)=Fixture();
        var g=c.Entries.Single(g=>g.CompositionKey==Field);
        g.EdgeProfiles!.Single(e=>e.DirectionCode=="east").ProfileCode="forest-edge";
        if(mode!="missing")i.Rules.Edges=[new(){FromProfile="field",ToProfile="forest-edge",State=mode=="forbidden"?"Forbidden":"TransitionRequired",MinimumGapMeters=3,EvidenceRef="fixture:boundary"}];
        Has(Review(c,i),"Edge",expected);
    }

    [Fact]
    public void 명시수관접촉은허용하되통로차단을상쇄하지않음()
    {
        var (c,i)=Fixture(); var third=Placement("canopies",0,0);
        var left=Object("canopy-1"); var right=Object("canopy-2"); left.Role=right.Role="Canopy";
        left.Measurement.CenterZ=right.Measurement.CenterZ=0;
        third.Objects=[left,right];i.Placements=[..i.Placements,third];
        Has(Review(c,i),"CanopyContact","NotInspected");
        i.Rules.CanopyPermits=[new(){FromObjectId=left.Id,ToObjectId=right.Id,EvidenceRef="fixture:canopy-overlap"}];
        var r=Review(c,i);Has(r,"CanopyContact","Passed");Has(r,"Route","Failed");Assert.Equal("Rejected",r.State);
    }

    [Fact]
    public void 같은소유자의문앞작업공간침범도거부()
    {
        var (c,i)=Fixture();i.Placements[0].WorkAreas=[new(){SourceStableId="door-clearance",MinX=-1,MinZ=2,MaxX=1,MaxZ=4}];
        Has(Review(c,i),"WorkClearance","Failed");
    }

    [Fact]
    public void 선분주변정보가없으면도달성에쓰지않음()
    {
        var (c,i)=Fixture();i.NeighborhoodComplete=false;
        Has(Review(c,i),"Route","NotInspected");Has(Review(c,i),"Reachability","NotInspected");
    }

    [Theory]
    [InlineData("slope")][InlineData("revision")]
    public void 지면경사와후속판본변경거부(string mode)
    {
        var (c,i)=Fixture();var surface=new ChangedSurface(mode);
        var r=new Simulation경관조합검토Service().Review(c,i,surface);
        Has(r,mode=="slope"?"GrammarSlope":"SurfaceRevisionAfter","Failed");
        Assert.False(string.IsNullOrEmpty(r.SurfaceSamplesHash));
    }

    [Theory]
    [InlineData("NaN")][InlineData("edge-duplicate")][InlineData("connector-duplicate")][InlineData("mirror")]
    public void 잘못된입력은예외유출이나자동보정없이거부(string mode)
    {
        var (c,i)=Fixture();var g=c.Entries.Single(g=>g.CompositionKey==Field);
        switch(mode)
        {
            case "NaN":i.Placements[0].X=double.NaN;break;
            case "edge-duplicate":g.EdgeProfiles=[..g.EdgeProfiles!,g.EdgeProfiles![0]];break;
            case "connector-duplicate":g.Connectors=[..g.Connectors,g.Connectors[0]];break;
            case "mirror":g.MirrorAllowed=false;i.Placements[0].Mirrored=true;break;
        }
        Assert.Equal("Rejected",Review(c,i).State);
    }

    [Fact]
    public void 거부후보만있어도자동완화하지않음()
    {
        var (c,i)=Fixture();i.Placements[0].Y=20;
        var r=new Simulation경관조합검토Service().ReviewCandidates(c,[i,i],new Flat());
        Assert.All(r,x=>Assert.Equal("Rejected",x.State));Assert.Equal(20,i.Placements[0].Y);
    }

    static void Has(Simulation경관조합검토Result r, string rule, string state) =>
        Assert.True(r.Findings.Any(f => f.Rule == rule && f.State == state), JsonSerializer.Serialize(r));
    static Simulation경관조합검토Result Review(SimulationWorldLandscapeGrammarCatalog c, Simulation경관조합검토Input i)
    { i.RulesHash = Simulation경관조합검토Service.Hash(i.Rules); return new Simulation경관조합검토Service().Review(c, i, new Flat()); }
    static (SimulationWorldLandscapeGrammarCatalog, Simulation경관조합검토Input) Fixture()
    {
        var c = Catalog();
        // 명확한 합성 출입 입력: 실제 9키 출입 증거를 만들거나 원 manifest를 수정하지 않는다.
        c.Entries.Single(g => g.CompositionKey == Field).Connectors = [
            new() { ConnectorTypeCode="farm-road", DirectionCode="east", LocalX=8, Width=1, RouteSignature="fixture-road" },
            new() { ConnectorTypeCode="farm-road", DirectionCode="west", LocalX=-8, Width=1, RouteSignature="fixture-road" }];
        c.SourceDocumentHashSha256 = Simulation경관조합검토Service.Hash(c);
        var rules = new Simulation경관조합검토Rules { Revision="fixture-r1", EvidenceRef="SyntheticFixture:not-an-asset", NeighborDistanceMeters=3,
            MaximumConnectorHeightDifferenceMeters=.1, Geometry = new() { Revision="fixture-r1", EvidenceRef="SyntheticFixture", MaximumSlopeDegrees=12,
                MaximumHeightSpreadMeters=.1, BottomToleranceMeters=.01, MinimumSpacingMeters=.1, MinimumRouteWidthMeters=1, RouteSampleStepMeters=.5, MaximumRouteSlopeDegrees=12, MaximumRouteStepMeters=.1 } };
        var input = new Simulation경관조합검토Input { Revision=Simulation경관조합검토Service.RelationReviewRevision, Seed="fixture-1",
            GrammarHash=c.CatalogHashSha256, GrammarSourceHash=c.SourceDocumentHashSha256, Rules=rules, RulesHash=Simulation경관조합검토Service.Hash(rules), ReviewSizeMeters=100,
            SurfaceRevision="flat-r1", SurfaceHash=H, SurfaceEvidenceKind="SyntheticFixture", NeighborhoodComplete=true, TraversalSurveyComplete=true, ProtectedAreas=[],
            Placements=[Placement("a",-9,0), Placement("b",9,0)],
            Relations=[Relation("required", "a", "b", "connects")],
            Connections=[new() { Id="path", RelationId="required", From="a", To="b", Bidirectional=true, FromLocalDirection="east", ToLocalDirection="west", Type="farm-road", RouteSignature="fixture-road" }] };
        input.Relations[0].RequireReturn = true;
        return (c, input);
    }
    static Simulation경관검토Relation Relation(string id,string from,string to,string kind,string requirement="Required") => new()
    { Edge=new() { EdgeStableId=id, FromNodeStableId=from, ToNodeStableId=to, RelationCode=kind }, Revision="relation-fixture-r1", EvidenceRef="SyntheticFixture:relation", Requirement=requirement };
    static Simulation경관검토Placement Placement(string id,double x,double z) => new()
    { Id=id, CompositionKey=Field, X=x, Z=z, ConnectorLocalHeight=0, AccessEvidenceRef="SyntheticFixture:door", GeometryComplete=true, WorkAreas=[], Objects=[Object(id+"-object")] };
    static Simulation경관검토Object Object(string id) => new() { Id=id, Role="Occupancy", ExpectedMeasurementHash=M, ExpectedAssetFingerprint=H,
        Measurement=new() { Revision="fixture-m1", EvidenceKindCode="SyntheticFixture", EvidenceRef="fixture:box", AssetFingerprintSha256=H, MeasurementHashSha256=M,
            CenterY=1, CenterZ=3, SizeX=2, SizeY=2, SizeZ=2, ActiveRenderer=true } };
    static SimulationWorldLandscapeGrammarCatalog Catalog()
    { Assert.True(new SimulationWorldLandscapeGrammarManifestReader(ManifestPath()).TryRead(out var c,out var e),e); return c; }
    static string ManifestPath()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
        { var p=Path.Combine(d.FullName,"eng/world-seedbeds/manifests/pyeongchang-landscape-grammar.v1.json"); if(File.Exists(p)) return p; }
        throw new FileNotFoundException();
    }
    sealed class Flat : ISimulationFarmH2SurfaceReader
    { public string Revision=>"flat-r1"; public string HashSha256=>H; public SimulationFarmH2SurfaceSample Read(double x,double z)=>new() { Supported=true, PlacementAllowed=true }; }
    sealed class ChangedSurface(string mode):ISimulationFarmH2SurfaceReader
    { bool read; public string Revision=>read&&mode=="revision"?"changed":"flat-r1"; public string HashSha256=>H;
      public SimulationFarmH2SurfaceSample Read(double x,double z){read=true;return new(){Supported=true,PlacementAllowed=true,SlopeDegrees=mode=="slope"?30:0};} }
    sealed class Layers : ISimulationWorldTileArtifactReader
    { public bool TryRead(string tile,string layer,out SimulationWorldTileArtifactSnapshot value) { value=new() {TileKey=tile,LayerCode=layer,ArtifactHashSha256=H};return true;} }
}
