using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class 시각자동할당Tests
{
    [Fact]
    public async Task 초안선정은독립조회멱등입력보존과과거사본을유지한다()
    {
        await using var f = new Fixture(); await f.Seed(); var request = f.Request();
        var original = JsonSerializer.Serialize(request);
        var first = await f.Composition.SaveAsync(f.User, request, default);
        Assert.Equal("Persisted", first.Diagnostic); Assert.Equal("Draft", first.Composition!.ReviewState);
        Assert.Equal("NotApplied", first.Composition.ApplicationState);
        Assert.Equal("AutomaticDraft_NotApplied", first.Composition.Items.Single().SelectionState);
        Assert.True((await f.Composition.SaveAsync(f.User, request, default)).Duplicate);
        Assert.Equal("IdempotencyConflict", (await f.Composition.SaveAsync(f.User, request with
            { Definition = request.Definition with { DisplayName = "충돌" } }, default)).Diagnostic);
        Assert.Equal(original, JsonSerializer.Serialize(request));
        await using var fresh = f.Context();
        Assert.Equal(request.Definition.Items, (await f.MakeComposition(fresh).GetAsync(f.User, "test:object", null, default)).Composition!.Definition.Items);
        Assert.Null((await f.MakeComposition(fresh).GetAsync(f.User, "test:object", 1, default)).Composition!.Items.Single().Item.InventorySnapshotId);
        Assert.Equal(2, await fresh.Compositions.CountAsync()); Assert.Empty(await fresh.Bindings.ToArrayAsync());
    }

    [Theory]
    [InlineData("shape", "SelectionEvidenceIncomplete")]
    [InlineData("purpose", "SelectionEvidenceIncomplete")]
    [InlineData("kind", "SelectionKindUnsupported")]
    [InlineData("unresolved", "SelectionKindUnsupported")]
    [InlineData("information", "SelectionKindUnsupported")]
    [InlineData("rig", "SelectionKindUnsupported")]
    [InlineData("guid", "SelectionIdentityMismatch")]
    [InlineData("source", "SelectionEvidenceReadOrDrift")]
    [InlineData("image", "SelectionEvidenceReadOrDrift")]
    [InlineData("review", "SelectionReviewMismatch")]
    [InlineData("revision", "AutomaticDefinitionChanged")]
    [InlineData("both", "InvalidComposition")]
    [InlineData("lowerhash", "SelectionEvidenceReadOrDrift")]
    [InlineData("nonhexhash", "SelectionEvidenceReadOrDrift")]
    public async Task 이름유사성으로결손과틀린종류를덮지않는다(string mode, string diagnostic)
    {
        await using var f = new Fixture(); await f.Seed(); var proof = f.Proof();
        proof = mode switch
        {
            "shape" => proof with { Conditions = proof.Conditions.Select(x => x.Condition == "Shape" ? x with { State = "Unknown" } : x).ToArray() },
            "purpose" => proof with { Conditions = proof.Conditions.Where(x => x.Condition != "Purpose").ToArray() },
            "kind" => proof with { AssetKind = "AnimationClipFile" }, "unresolved" => proof with { ObjectKind = "Unresolved" },
            "information" => proof with { ObjectKind = "Information" }, "rig" => proof with { ObjectKind = "Actor" },
            "guid" => proof with { Guid = new('b', 32) },
            "lowerhash" => proof with { Image = proof.Image with { Sha256 = new('a', 64) } },
            "nonhexhash" => proof with { Image = proof.Image with { Sha256 = new('G', 64) } }, _ => proof
        };
        var request = f.Request(proof);
        if (mode == "source") File.AppendAllText(Path.Combine(f.Root, f.Asset.RelativePath), "drift");
        if (mode == "image") File.AppendAllText(Path.Combine(f.Root, proof.Image.Path), "different asset image");
        if (mode == "review") request = request with { Definition = request.Definition with { Items = [request.Definition.Items[0] with
            { SelectionEvidenceJson = JsonSerializer.Serialize(JsonSerializer.Deserialize<시각자동선정근거>(request.Definition.Items[0].SelectionEvidenceJson!)! with { Purpose = "다른 용도" }) }] } };
        if (mode == "revision") request = request with { Definition = request.Definition with { DefinitionRevision = "r2" } };
        if (mode == "both") request = request with { Definition = request.Definition with { Items = [request.Definition.Items[0] with { AssetVersionId = new('A',64) }] } };
        Assert.Equal(diagnostic, (await f.Composition.SaveAsync(f.User, request, default)).Diagnostic);
        Assert.Single(await f.Db.Compositions.ToArrayAsync()); Assert.Single(await f.Db.CompositionHistory.ToArrayAsync());
    }

    [Fact]
    public async Task 기존선택은출처유무와무관하게보호하고경합은거부한다()
    {
        await using var f = new Fixture(); await f.Seed(); var request = f.Request();
        await f.Composition.SaveAsync(f.User, request, default);
        var changed = request with { RequestId = "next", ExpectedRevision = 2, Definition = request.Definition with
            { Items = [request.Definition.Items[0] with { SelectionEvidenceJson = request.Definition.Items[0].SelectionEvidenceJson + " " }] } };
        Assert.Equal("ExistingSelectionProtected", (await f.Composition.SaveAsync(f.User, changed, default)).Diagnostic);
        await using var fresh = f.Context();
        Assert.Equal("RevisionConflict", (await f.MakeComposition(fresh).SaveAsync(f.User, request with { RequestId = "race" }, default)).Diagnostic);
        Assert.Equal(2, await f.Db.Compositions.CountAsync());
    }

    [Fact]
    public async Task 복수역할과같은자산다른슬롯은가능하지만대안중복은거부한다()
    {
        await using var f = new Fixture(); await f.Seed(); var request = f.Request();
        var other = f.Request(f.Proof() with { Role = "Decoration", SlotKey = "other" }, "second").Definition.Items[0] with { ItemId = "second", Role = "Decoration", SlotKey = "other" };
        var bad = request with { Definition = request.Definition with { Items = [request.Definition.Items[0], request.Definition.Items[0] with { ItemId = "alternative" }] } };
        Assert.Equal("InvalidComposition", (await f.Composition.SaveAsync(f.User, bad, default)).Diagnostic);
        var result = await f.Composition.SaveAsync(f.User, request with { Definition = request.Definition with { Items = [request.Definition.Items[0], other] } }, default);
        Assert.Equal("Persisted", result.Diagnostic); Assert.Equal(2, result.Composition!.Items.Count);
    }

    [Fact]
    public async Task 분류는다대다전체경로와추정상태를보존하고원사본을바꾸지않는다()
    {
        await using var f = new Fixture(); await f.Seed();
        var old = await f.Db.InventorySnapshots.AsNoTracking().SingleAsync(); var input = f.Classification();
        Assert.Equal(1, (await f.Inventory.ImportClassificationsAsync(f.User, new([input]), default)).Inserted);
        Assert.Equal(1, (await f.Inventory.ImportClassificationsAsync(f.User, new([input]), default)).Existing);
        Assert.Equal("ClassificationRevisionConflict", (await f.Inventory.ImportClassificationsAsync(f.User, new([input with { Rationale = "different" }]), default)).Diagnostic);
        var inferred = f.Classification("Outdoor/group/kind/box", "Inferred");
        Assert.Equal("Persisted", (await f.Inventory.ImportClassificationsAsync(f.User, new([inferred]), default)).Diagnostic);
        Assert.Single((await f.Inventory.ListAsync(f.User, null, "Prefab", null, null, null, 0, default, "Outdoor/group")).Items);
        Assert.Equal(2, (await f.Inventory.ListAsync(f.User, null, null, null, null, null, 0, default)).Classifications!.Count);
        Assert.Empty((await f.Inventory.ListAsync(f.User, null, null, null, null, null, 0, default, classificationState: "Unclassified")).Items);
        Assert.Single((await f.Inventory.ListAsync(f.User, null, null, null, null, null, 0, default, taxonomyHash: new('F',64), classificationState: "Unclassified")).Items);
        Assert.Equal(old.MetadataHash, (await f.Db.InventorySnapshots.AsNoTracking().SingleAsync()).MetadataHash);
        Assert.Equal(old.MetadataJson, (await f.Db.InventorySnapshots.AsNoTracking().SingleAsync()).MetadataJson);
        Assert.Equal("TaxonomyPathUnknown", (await f.Inventory.ImportClassificationsAsync(f.User, new([input with { TaxonomyPath = "Indoor/group/kind/box" }]), default)).Diagnostic);
        Assert.Equal("ClassificationSourceMismatch", (await f.Inventory.ImportClassificationsAsync(f.User, new([input with { FamilyId = "unknown" }]), default)).Diagnostic);
    }

    private sealed class Current : ICurrentUserAccessor { public string? UserId => "admin"; public string? Role => 살뜰.Data.역할명.서버관리자; }
    private sealed class Fixture : IAsyncDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "visual-auto-test-" + Guid.NewGuid().ToString("N"));
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly ServiceProvider services;
        private readonly 개체시각대응Tests.Monitor options = new();
        public 개체시각대응DbContext Db { get; }
        public 게임객체시각구성UseCase Composition { get; }
        public 보유시각자산목록UseCase Inventory { get; }
        public 보유시각자산Input Asset { get; }
        public ClaimsPrincipal User { get; } = new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"admin"), new(ClaimTypes.Role,살뜰.Data.역할명.서버관리자)],"Fixture"));
        public Fixture()
        {
            connection.Open(); Db = Context(); Db.Database.EnsureCreated(); Directory.CreateDirectory(Root);
            options.Value = new() { ReviewEnabled = true, EvidenceRoot = Root, UnitySourceRoot = Root };
            var s = new ServiceCollection(); s.AddLogging(); s.AddAuthorizationCore(o => o.AddPolicy(개체시각대응Codes.Policy, p => p.RequireRole(살뜰.Data.역할명.서버관리자)));
            services = s.BuildServiceProvider(); Composition = MakeComposition(Db);
            Inventory = new(Db, services.GetRequiredService<IAuthorizationService>(), new Current(), options, TimeProvider.System);
            Write("Assets/Synty/Test/pot.prefab", "synthetic fixture, not a verified actual pot");
            Write("Assets/Synty/Test/pot.prefab.meta", "guid: " + new string('a',32)); Write("docs/evidence.md", "fixture only");
            var path = "Assets/Synty/Test/pot.prefab";
            Asset = new("r1", new('A',64), "Test", null, path, "Pot", "Prefab", new('a',32), H(path), H(path+".meta"), null,
                "{\"existingModuleEntries\":[{\"assetFamilyId\":\"test:family\",\"moduleCodes\":[\"group\"]}]}", []);
            Write("eng/execution-ledgers/synty-asset-human-taxonomy.json", "{\"revision\":\"r1\",\"표현범위\":[{\"범위Code\":\"Outdoor\",\"기능군\":[{\"기능군Code\":\"group\",\"세부기능군\":[{\"세부기능군Code\":\"kind\",\"자산종류\":[{\"자산종류Code\":\"box\"}]}]}]}]}");
            var png = new byte[33]; new byte[] {137,80,78,71,13,10,26,10}.CopyTo(png,0); "IHDR"u8.CopyTo(png.AsSpan(12));
            Write("artifacts/local/image.png", ""); File.WriteAllBytes(Path.Combine(Root,"artifacts/local/image.png"),png);
        }
        public 개체시각대응DbContext Context() => new(new DbContextOptionsBuilder<개체시각대응DbContext>().UseSqlite(connection).Options);
        public 게임객체시각구성UseCase MakeComposition(개체시각대응DbContext db) => new(db, services.GetRequiredService<IAuthorizationService>(), new Current(), options, TimeProvider.System);
        public async Task Seed()
        {
            Db.InventorySnapshots.Add(new() { SnapshotId = 보유시각자산목록UseCase.Id(Asset), ContentVersionId = 보유시각자산목록UseCase.ContentId(Asset),
                Guid=Asset.Guid,SurveyRevision=Asset.SurveyRevision,SourceGroup=Asset.SourceGroup,AssetKind=Asset.AssetKind,Name=Asset.Name,RelativePath=Asset.RelativePath,
                MetadataJson=JsonSerializer.Serialize(Asset),MetadataHash=개체시각선택Policy.Hash(Asset),EvidenceRef="docs/evidence.md",EvidenceHash=H("docs/evidence.md"),RegisteredBy="fixture" });
            await Db.SaveChangesAsync();
            await Composition.SaveAsync(User, new("seed",0,Definition([new("vessel","Vessel","main")])),default);
        }
        private 게임객체시각구성Input Definition(게임객체시각항목Input[] items) => new("test:object","시험 대상","r1","docs/evidence.md",H("docs/evidence.md"),items);
        public 시각자동선정근거 Proof() => new("visual-auto-selection.r1","CodexAutomatic","test:object","r1","Vessel","main","Physical","Prefab",Asset.Guid,Asset.AssetHash,Asset.MetaHash,
            보유시각자산목록UseCase.ContentId(Asset),"시험 용도","실제 자산 검증 아님","ExactPrefabPreview",new("Repository","artifacts/local/image.png",H("artifacts/local/image.png")),
            new("Repository","artifacts/local/review.json",new('A',64)),new[]{"Purpose","Shape","Technical"}.Select(k=>new 시각선정조건근거(k,"Verified","synthetic fixture")).ToArray(),[new("Repository","docs/evidence.md",H("docs/evidence.md"))]);
        public 게임객체시각구성Request Request(시각자동선정근거? proof=null,string reviewName="review")
        {
            proof ??= Proof(); var node=JsonSerializer.SerializeToNode(proof)!; node.AsObject().Remove("Review");
            var path="artifacts/local/"+reviewName+".json"; Write(path,node.ToJsonString()); proof=proof with { Review=new("Repository",path,H(path)) };
            return new("assign",1,Definition([new("vessel",proof.Role,proof.SlotKey,InventorySnapshotId:보유시각자산목록UseCase.Id(Asset),SelectionEvidenceJson:JsonSerializer.Serialize(proof))]));
        }
        public 보유시각분류Input Classification(string path="Outdoor/group",string state="CatalogMapped")
        {
            var input=new 보유시각분류Input(보유시각자산목록UseCase.Id(Asset),보유시각자산목록UseCase.ContentId(Asset),"r1",H("eng/execution-ledgers/synty-asset-human-taxonomy.json"),path,state,"test:family","","docs/evidence.md",H("docs/evidence.md"),"기존 추정규칙 대장 연결; 적합성 승인 아님");
            if(state=="CatalogMapped")return input;
            var n=JsonSerializer.SerializeToNode(input)!; n.AsObject().Remove("EvidenceRef");n.AsObject().Remove("EvidenceHash");Write("artifacts/local/classification.json",n.ToJsonString());
            return input with { EvidenceRef="artifacts/local/classification.json", EvidenceHash=H("artifacts/local/classification.json") };
        }
        private void Write(string path,string text) { var p=Path.Combine(Root,path);Directory.CreateDirectory(Path.GetDirectoryName(p)!);File.WriteAllText(p,text); }
        private string H(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root,path))));
        public async ValueTask DisposeAsync() { await Db.DisposeAsync();await connection.DisposeAsync();await services.DisposeAsync();Directory.Delete(Root,true); }
    }
}
