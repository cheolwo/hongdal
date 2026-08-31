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
public sealed class 게임객체WI참여Tests
{
    private static ClaimsPrincipal User(string id = "admin", bool admin = true) => new(new ClaimsIdentity(
        [new(ClaimTypes.NameIdentifier, id), new(ClaimTypes.Role, admin ? 살뜰.Data.역할명.서버관리자 : "User")], "Fixture"));

    [Fact]
    public async Task 같은객체의여러WI와역할을저장하고새문맥에서읽는다()
    {
        await using var f = new Fixture(); var input = f.Request(); var before = JsonSerializer.Serialize(input);
        var first = await f.Service.ImportAsync(User(), input, default);
        Assert.Equal("Persisted", first.Diagnostic); Assert.Equal(1, first.DefinitionsInserted); Assert.Equal(2, first.RelationsInserted);
        Assert.True((await f.Service.ImportAsync(User(), input, default)).Duplicate);
        var other = input with { RequestId = "another" };
        var reused = await f.Service.ImportAsync(User(), other, default);
        Assert.Equal(0, reused.DefinitionsInserted); Assert.Equal(0, reused.RelationsInserted);
        await using var read = f.Context(); var service = f.Make(read);
        Assert.Equal(2, (await service.ListAsync(User(), null, "actor:player", 0, default)).Items.Count);
        Assert.Single((await service.ListAsync(User(), "WI-TEST-01", null, 0, default)).Items);
        Assert.Equal(before, JsonSerializer.Serialize(input));
        Assert.Equal(1, await read.Definitions.CountAsync()); Assert.Empty(await read.CompositionItems.ToArrayAsync());
        Assert.All((await service.ListAsync(User(), null, null, 0, default)).Items, x =>
        { Assert.Equal("CurrentFileSnapshot", x.Freshness); Assert.Equal("CatalogStatement_NotDesignApproval", x.DecisionState); Assert.Equal("NotInstantiated", x.ApplicationState); });
    }

    [Theory]
    [InlineData("path", "InvalidExtraction")]
    [InlineData("wi", "UnknownWorldInteraction")]
    [InlineData("rule", "RuleRevisionMismatch")]
    [InlineData("quote", "SourceQuoteMismatch")]
    [InlineData("field", "SourceQuoteMismatch")]
    [InlineData("hash", "SourceDrift")]
    [InlineData("duplicate", "DuplicateExtractionInput")]
    [InlineData("missing", "DefinitionNotFound")]
    public async Task 잘못된근거와참조는부분정의를남기지않는다(string mode, string diagnostic)
    {
        await using var f = new Fixture(); var r = f.Request(); var first = r.Relations[0];
        r = mode switch
        {
            "path" => r with { SourceRef = "../../secret.json" },
            "wi" => r with { Relations = [first with { WorldInteractionId = "WI-NO-01" }] },
            "rule" => r with { Relations = [first with { RuleRevision = "not-current" }] },
            "quote" => r with { Relations = [first with { ExactQuote = "invented" }] },
            "field" => r with { Relations = [first with { SourceField = "notes" }] },
            "hash" => r with { SourceHash = new('F', 64) },
            "duplicate" => r with { Relations = [first, first] },
            _ => r with { Definitions = [] }
        };
        Assert.Equal(diagnostic, (await f.Service.ImportAsync(User(), r, default)).Diagnostic);
        Assert.Empty(await f.Db.Definitions.ToArrayAsync()); Assert.Empty(await f.Db.WiUses.ToArrayAsync()); Assert.Empty(await f.Db.WiBatches.ToArrayAsync());
    }

    [Fact]
    public async Task 원문변경은과거관계를삭제하지않고검토필요로반환한다()
    {
        await using var f = new Fixture(); var r = f.Request(); await f.Service.ImportAsync(User(), r, default);
        File.AppendAllText(f.Source, "\n"); // owned fixture only
        Assert.Equal("SourceDrift", (await f.Service.ImportAsync(User(), r, default)).Diagnostic);
        Assert.All((await f.Service.ListAsync(User(), null, null, 0, default)).Items, x => Assert.Equal("ReviewRequired_SourceDrift", x.Freshness));
        Assert.All((await f.Service.InventoryAsync(User(), default)).Items, x => Assert.Equal("Unreviewed", x.ReviewState));
        var revised = r with { RequestId = "r2", SourceHash = f.Hash };
        Assert.Equal("Persisted", (await f.Service.ImportAsync(User(), revised, default)).Diagnostic);
        Assert.Equal(4, await f.Db.WiUses.CountAsync()); Assert.Equal(1, await f.Db.Definitions.CountAsync());
    }

    [Fact]
    public async Task 상태조건과비물리해석을실물이나승인으로바꾸지않는다()
    {
        await using var f = new Fixture(); var r = f.Request();
        var relations = new 게임객체WI참여Input[] {
            r.Relations[0] with { ObjectKind = "Information", ExtractionState = "InterpretationCandidate", ContextNote = "책인지 교사인지 미정; 정의는 정보형 후보" },
            new("WI-TEST-02", null, "Condition", "access", "NotObject", "NonObject", "rule.r1", "startStateCodes", "Accessible", "접근성은 상태이지 새 객체가 아니다") };
        Assert.Equal("Persisted", (await f.Service.ImportAsync(User(), r with { Relations = relations }, default)).Diagnostic);
        var read = (await f.Service.ListAsync(User(), null, null, 0, default)).Items;
        Assert.Contains(read, x => x.Relation.ExtractionState == "InterpretationCandidate");
        Assert.Contains(read, x => x.Relation.DefinitionId is null && x.DefinitionCompositionId is null && x.Relation.ExtractionState == "NonObject");
        Assert.Equal(1, await f.Db.Definitions.CountAsync());
    }

    [Fact]
    public async Task 참여저장실패는재사용한구성서비스의신규정의와이력도롤백한다()
    {
        await using var f = new Fixture();
        await f.Db.Database.ExecuteSqlRawAsync("CREATE TRIGGER fail_use BEFORE INSERT ON world_visual_object_wi_uses BEGIN SELECT RAISE(ABORT, 'fixture'); END;");
        Assert.Equal("ExtractionStorageConflictOrFailure", (await f.Service.ImportAsync(User(), f.Request(), default)).Diagnostic);
        Assert.Empty(await f.Db.Definitions.ToArrayAsync()); Assert.Empty(await f.Db.Compositions.ToArrayAsync());
        Assert.Empty(await f.Db.CompositionHistory.ToArrayAsync()); Assert.Empty(await f.Db.WiBatches.ToArrayAsync());
    }

    [Fact]
    public async Task 요청재사용과같은원문의상이한관계변경을거부한다()
    {
        await using var f = new Fixture(); var r = f.Request(); await f.Service.ImportAsync(User(), r, default);
        var changed = r with { Relations = [r.Relations[0] with { ContextNote = "changed" }, r.Relations[1]] };
        Assert.Equal("IdempotencyConflict", (await f.Service.ImportAsync(User(), changed, default)).Diagnostic);
        Assert.Equal("RelationConflict", (await f.Service.ImportAsync(User(), changed with { RequestId = "new" }, default)).Diagnostic);
        Assert.Equal(1, await f.Db.WiBatches.CountAsync());
    }

    [Theory]
    [InlineData("anonymous", "Unauthorized")]
    [InlineData("member", "Forbidden")]
    [InlineData("other", "PrincipalMismatch")]
    [InlineData("disabled", "FeatureDisabled")]
    public async Task 권한검사전에원문이나DB를읽지않는다(string mode, string diagnostic)
    {
        await using var f = new Fixture(); var r = f.Request(); f.Options.Value.EvidenceRoot = null;
        if (mode == "disabled") f.Options.Value.ReviewEnabled = false;
        var user = mode switch { "anonymous" => new ClaimsPrincipal(), "member" => User(admin: false), "other" => User("other"), _ => User() };
        Assert.Equal(diagnostic, (await f.Service.ImportAsync(user, r, default)).Diagnostic);
        Assert.Equal(diagnostic, (await f.Service.ListAsync(user, null, null, 0, default)).Diagnostic);
        Assert.Equal(diagnostic, (await f.Service.InventoryAsync(user, default)).Diagnostic);
    }

    [Fact]
    public async Task 목록미검토와관계열변조를구분한다()
    {
        await using var f = new Fixture();
        Assert.Equal(3, (await f.Service.InventoryAsync(User(), default)).Items.Count);
        await f.Service.ImportAsync(User(), f.Request(), default);
        var inventory = await f.Service.InventoryAsync(User(), default);
        Assert.Equal(1, inventory.Items.Count(x => x.ReviewState == "Unreviewed"));
        await f.Db.Database.ExecuteSqlRawAsync("UPDATE world_visual_object_wi_uses SET Role='Tool'");
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.ListAsync(User(), null, null, 0, default));
    }

    private sealed class Current : ICurrentUserAccessor { public string? UserId => "admin"; public string? Role => 살뜰.Data.역할명.서버관리자; }
    private sealed class Fixture : IAsyncDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "d435-" + Guid.NewGuid().ToString("N"));
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly ServiceProvider services;
        public string Source => Path.Combine(root, 게임객체WI참여UseCase.SourceRef);
        public string Hash => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Source)));
        public 개체시각대응Tests.Monitor Options { get; } = new() { Value = new() { ReviewEnabled = true, Enabled = false } };
        public 개체시각대응DbContext Db { get; }
        public 게임객체WI참여UseCase Service { get; }
        public Fixture()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Source)!);
            File.WriteAllText(Source, JsonSerializer.Serialize(new { catalogKey = "simulation-world-interactions", revision = "fixture.r1", items = Enumerable.Range(1, 3).Select(i =>
                new { id = "WI-TEST-0" + i, title = "시험" + i, groupCode = "TEST", ruleRevision = "rule.r1", actorRequirements = new[] { "Player" }, worldAction = "Player chooses", startStateCodes = new[] { "Accessible" } }) }));
            Options.Value.EvidenceRoot = root; connection.Open(); Db = Context(); Db.Database.EnsureCreated();
            var s = new ServiceCollection(); s.AddLogging(); s.AddAuthorizationCore(o => o.AddPolicy(개체시각대응Codes.Policy, p => p.RequireRole(살뜰.Data.역할명.서버관리자)));
            services = s.BuildServiceProvider(); Service = Make(Db);
        }
        public 게임객체WI추출Request Request() => new("fixture:first", 게임객체WI참여UseCase.SourceRef, "fixture.r1", Hash,
            [new("actor:player", "플레이어", "fixture.r1", "docs/fixture.md", new('A',64), [])],
            [new("WI-TEST-01", "actor:player", "Actor", "main", "Actor", "DirectMention", "rule.r1", "actorRequirements", "Player", "직접 행위자"),
             new("WI-TEST-02", "actor:player", "Target", "self", "Actor", "ExistingDefinitionReuse", "rule.r1", "actorRequirements", "Player", "자기 대상 해석 시험 입력")]);
        public 개체시각대응DbContext Context() => new(new DbContextOptionsBuilder<개체시각대응DbContext>().UseSqlite(connection).Options);
        public 게임객체WI참여UseCase Make(개체시각대응DbContext db)
        {
            var auth = services.GetRequiredService<IAuthorizationService>(); var current = new Current();
            return new(db, new(db, auth, current, Options, TimeProvider.System), auth, current, Options, TimeProvider.System);
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await connection.DisposeAsync(); await services.DisposeAsync(); Directory.Delete(root, true); }
    }
}
