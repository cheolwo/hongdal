using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class 개체시각목록Tests
{
    [Fact]
    public void 기존후보의요청hash호환을위해없는자산참조는직렬화하지않는다()
    {
        var candidate = 개체시각대응Tests.Candidate;
        var legacy = new { candidate.VisualKey, candidate.CatalogRevision, candidate.CatalogFingerprint,
            candidate.AssetFingerprint, candidate.Fitness, candidate.EvidenceRef, candidate.EvidenceFingerprint };
        Assert.Equal(개체시각선택Policy.Hash(legacy), 개체시각선택Policy.Hash(candidate));
    }
    private static ClaimsPrincipal User(string id = "admin", bool admin = true) => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Role, admin ? 살뜰.Data.역할명.서버관리자 : "User")], "Fixture"));

    [Fact]
    public async Task 파일검증등록_새문맥재조회_같은입력멱등_제품선택비활성을분리한다()
    {
        await using var f = new Fixture();
        var first = await f.Assets.ImportAsync(User(), [f.Input], default);
        Assert.Equal("Persisted", first.Diagnostic);
        Assert.Equal(1, first.Inserted);
        Assert.Equal(개체시각목록UseCase.Verification, Assert.Single(first.Items).VerificationState);
        var second = await f.Assets.ImportAsync(User(), [f.Input], default);
        Assert.Equal(0, second.Inserted);
        Assert.Equal(1, second.Existing);
        await using var read = f.NewContext();
        Assert.Equal(f.Input, 개체시각목록UseCase.Parse(await read.Assets.SingleAsync()).Metadata);
        Assert.Single((await f.Assets.ListAsync(User(), null, 0, default)).Items);
        Assert.Equal("FeatureDisabled", (await f.Bindings.ResolveAsync(User(), f.Query, default)).Diagnostic);
    }

    [Theory]
    [InlineData("anonymous", "Unauthorized")]
    [InlineData("regular", "Forbidden")]
    [InlineData("other", "PrincipalMismatch")]
    [InlineData("disabled", "FeatureDisabled")]
    public async Task 관리자권한과검토스위치가없으면파일조회나저장하지않는다(string mode, string expected)
    {
        await using var f = new Fixture();
        f.Options.Value.UnitySourceRoot = null;
        if (mode == "disabled") f.Options.Value.ReviewEnabled = false;
        var user = mode switch { "anonymous" => new ClaimsPrincipal(), "regular" => User(admin: false), "other" => User("other"), _ => User() };
        Assert.Equal(expected, (await f.Assets.ImportAsync(user, [f.Input], default)).Diagnostic);
        Assert.Equal(expected, (await f.Assets.ListAsync(user, null, 0, default)).Diagnostic);
        Assert.Empty(await f.Db.Assets.ToArrayAsync());
    }

    [Theory]
    [InlineData("file", "AssetFileHashMismatch")]
    [InlineData("guid", "AssetGuidMismatch")]
    [InlineData("catalog", "AssetCatalogReferenceMismatch")]
    [InlineData("path", "AssetFileReadOrPathRejected")]
    [InlineData("missing", "AssetFileReadOrPathRejected")]
    [InlineData("pack", "AssetPackMismatch")]
    public async Task 잘못된파일이나참조는등록하지않는다(string mode, string expected)
    {
        await using var f = new Fixture();
        var input = mode switch
        {
            "file" => f.Input with { AssetFingerprint = new('F', 64) },
            "guid" => f.Input with { PrefabGuid = new('b', 32) },
            "catalog" => f.Input with { VisualKey = "missing.key" },
            "path" => f.Input with { PrefabPath = "Assets/Synty/Test/../fixture.prefab" },
            "missing" => f.Input with { PrefabPath = "Assets/Synty/Test/missing.prefab" },
            "pack" => f.Input with { Pack = "Other" },
            _ => f.Input
        };
        Assert.Equal(expected, (await f.Assets.ImportAsync(User(), [input], default)).Diagnostic);
        Assert.Empty(await f.Db.Assets.ToArrayAsync());
    }

    [Fact]
    public async Task 묶음중복과중간오류는부분등록하지않으며같은판본덮어쓰기를거부한다()
    {
        await using var f = new Fixture();
        Assert.Equal("DuplicateAssetInput", (await f.Assets.ImportAsync(User(), [f.Input, f.Input], default)).Diagnostic);
        Assert.Equal("AssetFileHashMismatch", (await f.Assets.ImportAsync(User(), [f.Input, f.Input with
            { CatalogRevision = "r2", MetaFingerprint = new('F', 64) }], default)).Diagnostic);
        Assert.Empty(await f.Db.Assets.ToArrayAsync());
        await f.Assets.ImportAsync(User(), [f.Input], default);
        Assert.Equal("AssetRevisionConflict", (await f.Assets.ImportAsync(User(), [f.Input with { DisplayName = "Changed" }], default)).Diagnostic);
        Assert.Equal(1, (await f.Assets.ImportAsync(User(), [f.Input with { CatalogRevision = "r2" }], default)).Inserted);
        Assert.Equal(2, await f.Db.Assets.CountAsync());
    }

    [Fact]
    public async Task DB후보관계와검색열은JSON과일치하고초안은선택되지않는다()
    {
        await using var f = new Fixture();
        await f.Assets.ImportAsync(User(), [f.Input], default);
        var result = await f.Bindings.ExecuteAsync(User(), f.Request(), default);
        Assert.True(result.Success, result.Diagnostic);
        Assert.Equal("Draft", result.Binding!.ReviewState);
        await using var read = f.NewContext();
        var row = await read.Bindings.SingleAsync();
        Assert.Equal(개체시각목록UseCase.Id(f.Input), row.AssetVersionId);
        Assert.Equal(f.Query.StableId, row.SourceStableId);
        Assert.Single((await f.Bindings.ListAsync(User(), f.Query, default)).Items);
        f.Options.Value.Enabled = true;
        Assert.Equal("Unmapped", (await f.Bindings.ResolveAsync(User(), f.Query, default)).Diagnostic);
        var submit = f.Request() with { Candidate = null, ExpectedRevision = 1, IdempotencyKey = "submit", Action = 개체시각대응Action.SubmitReview };
        Assert.True((await f.Bindings.ExecuteAsync(User(), submit, default)).Success);
        Assert.Equal("UnregisteredCandidate", (await f.Bindings.ExecuteAsync(User(), submit with
            { ExpectedRevision = 2, IdempotencyKey = "approve", Action = 개체시각대응Action.Approve }, default)).Diagnostic);
    }

    [Fact]
    public async Task 고아관계와다른판본참조및참조중삭제를거부한다()
    {
        await using var f = new Fixture();
        Assert.Equal("AssetVersionNotFound", (await f.Bindings.ExecuteAsync(User(), f.Request(), default)).Diagnostic);
        await f.Assets.ImportAsync(User(), [f.Input], default);
        Assert.Equal("AssetReferenceMismatch", (await f.Bindings.ExecuteAsync(User(), f.Request() with
            { Candidate = f.Candidate with { AssetFingerprint = new('F', 64) } }, default)).Diagnostic);
        Assert.True((await f.Bindings.ExecuteAsync(User(), f.Request(), default)).Success);
        await using var other = f.NewContext();
        other.Assets.Remove(await other.Assets.SingleAsync());
        await Assert.ThrowsAsync<DbUpdateException>(() => other.SaveChangesAsync());
        Assert.Equal(1, await f.Db.Assets.CountAsync());
    }

    [Fact]
    public async Task 저장열과JSON의문맥불일치를숨기지않는다()
    {
        await using var f = new Fixture();
        await f.Assets.ImportAsync(User(), [f.Input], default);
        await f.Bindings.ExecuteAsync(User(), f.Request(), default);
        var row = await f.Db.Bindings.SingleAsync();
        row.SourceStableId = "other";
        await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Bindings.ListAsync(User(), f.Query, default));
    }

    private sealed class Current : ICurrentUserAccessor { public string? UserId => "admin"; public string? Role => 살뜰.Data.역할명.서버관리자; }
    private sealed class Source : I개체시각대상Reader
    {
        public Task<개체시각대상ReadResult> ReadAsync(개체시각대상Query q, CancellationToken ct)
            => Task.FromResult(new 개체시각대상ReadResult("Found", 개체시각대응Tests.Target));
    }
    private sealed class Fixture : IAsyncDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "d431-fixture-" + Guid.NewGuid().ToString("N"));
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly ServiceProvider provider;
        public 개체시각대응Tests.Monitor Options { get; } = new();
        public 개체시각대응DbContext Db { get; }
        public 개체시각목록UseCase Assets { get; }
        public 개체시각대응UseCase Bindings { get; }
        public 개체시각자산입력 Input { get; }
        public 개체시각대상Query Query => 개체시각대응Tests.Query;
        public 개체시각후보Dto Candidate => new(Input.VisualKey, Input.CatalogRevision, Input.CatalogFingerprint,
            Input.AssetFingerprint, "Unreviewed", Input.EvidenceRef, Input.EvidenceFingerprint, 개체시각목록UseCase.Id(Input));
        public 개체시각대응Request Request() => new("fixture:binding", 0, "draft", 개체시각대응Action.SaveDraft, "Fixture only", Query, false, Candidate);
        public Fixture()
        {
            var prefab = Write("Assets/Synty/Test/fixture.prefab", "%YAML 1.1\nfixture: true\n");
            var meta = Write("Assets/Synty/Test/fixture.prefab.meta", "guid: " + new string('a', 32) + "\n");
            var catalog = Write("Assets/Ssalddel/Fixture.asset", "  - visualKey: fixture.box\n    prefab: {fileID: 1, guid: " + new string('a', 32) + ", type: 3}\n");
            var evidence = Write("docs/fixture.md", "Fixture only\n");
            Input = new("fixture.box", "r1", "Assets/Ssalddel/Fixture.asset", H(catalog), "Synty", "Test",
                "Assets/Synty/Test/fixture.prefab", new('a', 32), H(prefab), H(meta), "Fixture", "Cargo", "docs/fixture.md", H(evidence));
            Options.Value = new() { ReviewEnabled = true, Enabled = false, UnitySourceRoot = root, EvidenceRoot = root };
            connection.Open();
            Db = NewContext();
            Db.Database.EnsureCreated(); // SQLite fixture only; never used by local MySQL runner.
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthorizationCore(o => o.AddPolicy(개체시각대응Codes.Policy, p => p.RequireRole(살뜰.Data.역할명.서버관리자)));
            provider = services.BuildServiceProvider();
            var authorization = provider.GetRequiredService<IAuthorizationService>();
            Assets = new(Db, authorization, new Current(), Options, TimeProvider.System);
            Bindings = new(Db, new Source(), new 개체시각자산Catalog(Options), authorization, new Current(), Options, TimeProvider.System);
        }
        public 개체시각대응DbContext NewContext() => new(new DbContextOptionsBuilder<개체시각대응DbContext>().UseSqlite(connection).Options);
        private string Write(string relative, string value) { var path = Path.Combine(root, relative); Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, value); return path; }
        private static string H(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync(); await connection.DisposeAsync(); await provider.DisposeAsync();
            Directory.Delete(root, recursive: true); // only this fixture's random, explicitly owned directory
        }
    }
}
