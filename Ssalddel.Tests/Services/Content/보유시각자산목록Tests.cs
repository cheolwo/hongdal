using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class 보유시각자산목록Tests
{
    private static ClaimsPrincipal User(string id = "admin", bool admin = true) => new(new ClaimsIdentity(
        [new(ClaimTypes.NameIdentifier,id),new(ClaimTypes.Role,admin ? 살뜰.Data.역할명.서버관리자 : "User")],"Fixture"));
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("lower")]
    [InlineData("nonhex")]
    public async Task 공통Hash검사는기존거부코드와입력을보존한다(string? kind)
    {
        await using var f = new Fixture();
        var hash = kind switch { "lower" => new string('a',64), "nonhex" => new string('G',64), _ => kind };
        var request = f.Request(f.Asset(1)) with { EvidenceHash = hash! };
        var original = JsonSerializer.Serialize(request);
        for (var attempt = 0; attempt < 2; attempt++)
            Assert.Equal("InvalidInventoryInput", (await f.Service.ImportAsync(User(), request, default)).Diagnostic);
        Assert.Equal(original, JsonSerializer.Serialize(request));
        Assert.Empty(await f.Db.InventorySnapshots.ToArrayAsync());
    }
    [Fact]
    public async Task 키없는파일과기존후보연결을분리하여독립조회하고재입력한다()
    {
        await using var f = new Fixture(); var a = f.Asset(1); var b = f.Asset(2,".mat");
        var candidate = f.Candidate(a); f.Db.Assets.Add(candidate); await f.Db.SaveChangesAsync();
        a = a with { ExistingCandidateIds = [candidate.AssetVersionId] };
        var request = f.Request(a,b); var original = JsonSerializer.Serialize(request);
        var result = await f.Service.ImportAsync(User(),request,default);
        Assert.Equal("Persisted",result.Diagnostic); Assert.Equal(2,result.Inserted); Assert.Equal(2,result.FirstSeenGuids);
        var replay = await f.Service.ImportAsync(User(),request,default); Assert.Equal(0,replay.Inserted); Assert.Equal(2,replay.Existing);
        await using var fresh = f.Context(); var reader = f.Make(fresh);
        Assert.Equal(2,(await reader.ListAsync(User(),null,null,null,null,null,0,default)).Total);
        Assert.Single((await reader.ListAsync(User(),null,null,null,"test.prefab",null,0,default)).Items);
        Assert.Single((await reader.ListAsync(User(),"TestPack","Material",null,null,"fixture.r1",0,default)).Items);
        Assert.Equal(original,JsonSerializer.Serialize(request));
        Assert.Single(await fresh.Assets.ToArrayAsync()); Assert.Empty(await fresh.Bindings.ToArrayAsync()); Assert.Empty(await fresh.Definitions.ToArrayAsync());
        Assert.All((await reader.ListAsync(User(),null,null,null,null,null,0,default)).Items,x =>
        { Assert.Equal("Unreviewed",x.ReviewState); Assert.Equal("NotSelected_NotInstantiated",x.ApplicationState); Assert.Equal("StoredSurveySnapshot_NotLiveFileCheck",x.Freshness); });
    }
    [Fact]
    public async Task 여러판본은과거를보존하고동일판본메타변경을거부한다()
    {
        await using var f=new Fixture(); var a=f.Asset(1); await f.Service.ImportAsync(User(),f.Request(a),default);
        var changed = a with { Name="different" };
        Assert.Equal("InventoryRevisionConflict",(await f.Service.ImportAsync(User(),f.Request(changed),default)).Diagnostic);
        File.AppendAllText(Path.Combine(f.Root,a.RelativePath),"changed");
        Assert.Equal("InventoryFileDrift",(await f.Service.ImportAsync(User(),f.Request(a),default)).Diagnostic);
        var b=f.Revidence(a with { SurveyRevision="fixture.r2",AssetHash=Fixture.H(Path.Combine(f.Root,a.RelativePath)) });
        var second=await f.Service.ImportAsync(User(),f.Request(b),default);
        Assert.Equal(1,second.AdditionalSnapshots); Assert.Equal(0,second.FirstSeenGuids);
        var rows=(await f.Service.ListAsync(User(),null,null,null,null,null,0,default)).Items;
        Assert.Equal(2,rows.Count); Assert.Equal(2,rows.Select(x=>x.ContentVersionId).Distinct().Count());
    }
    [Theory]
    [InlineData("path","InventoryFileReadOrPathRejected")]
    [InlineData("group","InventoryFileReadOrPathRejected")]
    [InlineData("hash","InventoryFileDrift")]
    [InlineData("guid","InventoryGuidMismatch")]
    [InlineData("kind","InvalidInventoryInput")]
    [InlineData("evidence","InvalidInventoryInput")]
    [InlineData("origin","InventoryOriginVersionMismatch")]
    [InlineData("duplicate","DuplicateInventoryInput")]
    [InlineData("candidate","InventoryCandidateMissing")]
    [InlineData("approval","InventoryEvidenceChanged")]
    public async Task 틀린입력은어느행도반입하지않는다(string mode,string diagnostic)
    {
        await using var f=new Fixture(); var a=f.Asset(1); var bad=f.Asset(2);
        bad=mode switch {
            "path"=>f.Revidence(bad with { RelativePath="Assets/Synty/TestPack/../outside.prefab" }),
            "group"=>f.Revidence(bad with { SourceGroup="Other" }),
            "hash"=>f.Revidence(bad with { AssetHash=new('F',64) }),
            "guid"=>f.Revidence(bad with { Guid=new('f',32) }),
            "kind"=>bad with { AssetKind="Material" },
            "evidence"=>bad with { EvidenceJson="{}" },
            "origin"=>bad with { OriginVersion="fake-version" },
            "candidate"=>bad with { ExistingCandidateIds=[new('F',64)] }, _=>bad };
        var request=mode=="duplicate"?f.Request(a,a):f.Request(a,bad);
        if(mode=="approval") request=request with { EvidenceHash=new('F',64) };
        Assert.Equal(diagnostic,(await f.Service.ImportAsync(User(),request,default)).Diagnostic);
        Assert.Empty(await f.Db.InventorySnapshots.ToArrayAsync()); Assert.Empty(await f.Db.InventoryLinks.ToArrayAsync());
    }
    [Theory]
    [InlineData("anonymous","Unauthorized")]
    [InlineData("member","Forbidden")]
    [InlineData("other","PrincipalMismatch")]
    [InlineData("disabled","FeatureDisabled")]
    public async Task 권한과활성화경계를지킨다(string mode,string diagnostic)
    {
        await using var f=new Fixture(); var request=f.Request(f.Asset(1)); f.Options.Value.UnitySourceRoot=null;
        if(mode=="disabled")f.Options.Value.ReviewEnabled=false;
        var u=mode switch {"anonymous"=>new ClaimsPrincipal(),"member"=>User(admin:false),"other"=>User("other"),_=>User()};
        Assert.Equal(diagnostic,(await f.Service.ImportAsync(u,request,default)).Diagnostic);
        Assert.Equal(diagnostic,(await f.Service.ListAsync(u,null,null,null,null,null,0,default)).Diagnostic);
    }
    [Fact]
    public async Task 연결실패는새사본도남기지않는다()
    {
        await using var f=new Fixture();var a=f.Asset(1);var candidate=f.Candidate(a);
        f.Db.Assets.Add(candidate);await f.Db.SaveChangesAsync();
        await f.Db.Database.ExecuteSqlRawAsync("CREATE TRIGGER fail_inventory_link BEFORE INSERT ON world_visual_inventory_candidate_links BEGIN SELECT RAISE(ABORT, 'fixture'); END;");
        Assert.Equal("InventoryStorageConflictOrFailure",(await f.Service.ImportAsync(User(),f.Request(a with {ExistingCandidateIds=[candidate.AssetVersionId]}),default)).Diagnostic);
        Assert.Empty(await f.Db.InventorySnapshots.ToArrayAsync());Assert.Empty(await f.Db.InventoryLinks.ToArrayAsync());Assert.Single(await f.Db.Assets.ToArrayAsync());
    }
    [Fact]
    public async Task 페이지제한을전수누락으로오인하지않고같은이름을합치지않는다()
    {
        await using var f=new Fixture();var items=Enumerable.Range(1,105).Select(i=>f.Asset(i) with {Name="same-name"}).ToArray();
        Assert.Equal(105,(await f.Service.ImportAsync(User(),f.Request(items),default)).Inserted);
        var first=await f.Service.ListAsync(User(),null,null,"same",null,null,0,default);
        var second=await f.Service.ListAsync(User(),null,null,null,null,null,100,default);
        Assert.Equal(105,first.Total);Assert.Equal(100,first.Items.Count);Assert.Equal(5,second.Items.Count);
        Assert.Equal(105,first.Items.Concat(second.Items).Select(x=>x.SnapshotId).Distinct().Count());
    }
    [Fact]
    public async Task 열변조를감지하고빈파일도실제파일로구분한다()
    {
        await using var f=new Fixture();var a=f.Asset(1,".txt");File.WriteAllText(Path.Combine(f.Root,a.RelativePath),"");
        a=f.Revidence(a with {AssetHash=Fixture.H(Path.Combine(f.Root,a.RelativePath))});
        Assert.Equal("Persisted",(await f.Service.ImportAsync(User(),f.Request(a),default)).Diagnostic);
        await f.Db.Database.ExecuteSqlRawAsync("UPDATE world_visual_inventory_snapshots SET Name='tampered'");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.ListAsync(User(),null,null,null,null,null,0,default));
    }
    private sealed class Current:ICurrentUserAccessor {public string? UserId=>"admin"; public string? Role=>살뜰.Data.역할명.서버관리자;}
    private sealed class Fixture:IAsyncDisposable
    {
        public string Root {get;}=Path.Combine(Path.GetTempPath(),"d437-"+Guid.NewGuid().ToString("N"));
        private readonly SqliteConnection connection=new("Data Source=:memory:");
        private readonly ServiceProvider services;
        public 개체시각대응Tests.Monitor Options {get;}=new(){Value=new(){ReviewEnabled=true}};
        public 개체시각대응DbContext Db {get;}
        public 보유시각자산목록UseCase Service {get;}
        public Fixture()
        {
            Directory.CreateDirectory(Path.Combine(Root,"docs"));File.WriteAllText(Path.Combine(Root,"docs/test.md"),"fixture metadata approval");
            Options.Value.UnitySourceRoot=Root; Options.Value.EvidenceRoot=Root;
            connection.Open();Db=Context();Db.Database.EnsureCreated();
            var s=new ServiceCollection();s.AddLogging();s.AddAuthorizationCore(o=>o.AddPolicy(개체시각대응Codes.Policy,p=>p.RequireRole(살뜰.Data.역할명.서버관리자)));
            services=s.BuildServiceProvider();Service=Make(Db);
        }
        public static string H(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
        public 보유시각자산Input Asset(int n,string extension=".prefab")
        {
            var relative=$"Assets/Synty/TestPack/file-{n}{extension}";var path=Path.Combine(Root,relative);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var guid=n.ToString("x32");File.WriteAllText(path,"source "+n);File.WriteAllText(path+".meta","fileFormatVersion: 2\nguid: "+guid+"\n");
            return Revidence(new("fixture.r1",new('A',64),"TestPack",null,relative,"file-"+n,보유시각자산목록UseCase.Kind(extension),guid,H(path),H(path+".meta"),null,"",[]));
        }
        public 보유시각자산Input Revidence(보유시각자산Input x)=>x with {EvidenceJson=JsonSerializer.Serialize(new{guid=x.Guid,relativePath=x.RelativePath,assetHash=x.AssetHash,metaHash=x.MetaHash,assetKind=x.AssetKind,sourceGroup=x.SourceGroup,surveyRevision=x.SurveyRevision})};
        public 보유시각자산반입Request Request(params 보유시각자산Input[] items)=>new("docs/test.md",H(Path.Combine(Root,"docs/test.md")),items);
        public Ssalddel.Domain.Content.개체시각자산판본 Candidate(보유시각자산Input a)
        {
            var m=new 개체시각자산입력("test.prefab","fixture.r1","Assets/Ssalddel/Catalog.asset",new('A',64),"Synty",a.SourceGroup,a.RelativePath,a.Guid,a.AssetHash,a.MetaHash,a.Name,"test","docs/test.md",new('A',64));
            return new(){AssetVersionId=개체시각목록UseCase.Id(m),VisualKey=m.VisualKey,CatalogRevision=m.CatalogRevision,PrefabGuid=m.PrefabGuid,MetadataJson=JsonSerializer.Serialize(m),MetadataHash=개체시각선택Policy.Hash(m),VerificationState=개체시각목록UseCase.Verification,RegisteredBy="admin",RegisteredAtUtc=DateTime.UtcNow};
        }
        public 개체시각대응DbContext Context()=>new(new DbContextOptionsBuilder<개체시각대응DbContext>().UseSqlite(connection).Options);
        public 보유시각자산목록UseCase Make(개체시각대응DbContext db)=>new(db,services.GetRequiredService<IAuthorizationService>(),new Current(),Options,TimeProvider.System);
        public async ValueTask DisposeAsync(){await Db.DisposeAsync();await connection.DisposeAsync();await services.DisposeAsync();Directory.Delete(Root,true);}
    }
}
