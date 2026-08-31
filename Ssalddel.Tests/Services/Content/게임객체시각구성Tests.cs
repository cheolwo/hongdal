using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Domain.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class 게임객체시각구성Tests
{
    [Fact]
    public void 관리자도입메타데이터는실행기능승인과별개로기록한다()
    {
        var attributes = typeof(Ssalddel.Controllers.Admin.Content.개체시각대응Controller)
            .GetCustomAttributes(typeof(Ssalddel.ApiMetadata.SsalddelApiVersionAttribute), true);
        Assert.Single(attributes);
    }
    public static 게임객체시각구성Request Request(string id = "fixture:workshop", string? asset = null) => new("request:1", 0,
        new(id, "공방 시험 정의", "r1", "docs/fixture.md", new('A', 64),
            [new("shell", "Exterior", "main", asset), new("workbench", "WorkEquipment", "main", asset)]));
    private static ClaimsPrincipal User(string id = "admin", bool admin = true) => new(new ClaimsIdentity(
        [new(ClaimTypes.NameIdentifier, id), new(ClaimTypes.Role, admin ? 살뜰.Data.역할명.서버관리자 : "User")], "Fixture"));

    [Fact]
    public async Task 미선정역할초안저장과목록조회는실제객체나할당성공이아니다()
    {
        await using var f = new Fixture();
        var request = Request(); var before = JsonSerializer.Serialize(request);
        var result = await f.Service.SaveAsync(User(), request, default);
        Assert.Equal("Persisted", result.Diagnostic);
        Assert.Equal("Draft", result.Composition!.ReviewState);
        Assert.Equal("NotApplied", result.Composition.ApplicationState);
        Assert.Equal("EditableDefinition_NotWorldInstance", result.Composition.Kind);
        Assert.All(result.Composition.Items, x => { Assert.Null(x.Asset); Assert.Equal("Unselected", x.SelectionState); Assert.Equal("NotObserved", x.ImageEvidenceState); });
        Assert.Single((await f.Service.ListAsync(User(), 0, default)).Items);
        Assert.Equal(before, JsonSerializer.Serialize(request));
        await using var read = f.Context();
        Assert.Equal(2, await read.CompositionItems.CountAsync());
        Assert.Empty(await read.Bindings.ToArrayAsync());
    }

    [Fact]
    public async Task 한자산을여러역할과여러객체에재사용하고같은역할도다른슬롯으로식별한다()
    {
        await using var f = new Fixture(); var id = await f.Asset();
        var request = Request(asset: id);
        request = request with { Definition = request.Definition with { Items = [.. request.Definition.Items, new("second", "WorkEquipment", "second", id)] } };
        Assert.Equal("Persisted", (await f.Service.SaveAsync(User(), request, default)).Diagnostic);
        Assert.Equal("Persisted", (await f.Service.SaveAsync(User(), Request("fixture:other", id), default)).Diagnostic);
        Assert.Equal(5, await f.Db.CompositionItems.CountAsync(x => x.AssetVersionId == id));
        Assert.Equal(1, await f.Db.Assets.CountAsync());
        Assert.All((await f.Service.GetAsync(User(), "fixture:workshop", null, default)).Composition!.Items, x => Assert.Equal(id, x.Asset!.AssetVersionId));
    }

    [Theory]
    [InlineData("item")]
    [InlineData("role-slot")]
    [InlineData("revision")]
    [InlineData("empty-role")]
    [InlineData("too-many")]
    public async Task 중복이나잘못된판본은원자적으로거부한다(string mode)
    {
        await using var f = new Fixture(); var r = Request();
        r = mode switch
        {
            "revision" => r with { ExpectedRevision = -1 },
            "item" => r with { Definition = r.Definition with { Items = [new("a", "Role", "one"), new("a", "Role", "two")] } },
            "role-slot" => r with { Definition = r.Definition with { Items = [new("a", "Role", "one"), new("b", "Role", "one")] } },
            "empty-role" => r with { Definition = r.Definition with { Items = [new("a", "", "one")] } },
            _ => r with { Definition = r.Definition with { Items = Enumerable.Range(0, 65).Select(i => new 게임객체시각항목Input("a" + i, "Role", "s" + i)).ToArray() } }
        };
        Assert.Equal("InvalidComposition", (await f.Service.SaveAsync(User(), r, default)).Diagnostic);
        Assert.Empty(await f.Db.Definitions.ToArrayAsync());
        Assert.Empty(await f.Db.CompositionHistory.ToArrayAsync());
    }

    [Fact]
    public async Task 자산판본고아참조와연결자산삭제를거부한다()
    {
        await using var f = new Fixture();
        Assert.Equal("AssetVersionNotFound", (await f.Service.SaveAsync(User(), Request(asset: new('F', 64)), default)).Diagnostic);
        Assert.Empty(await f.Db.Definitions.ToArrayAsync());
        var id = await f.Asset();
        await f.Service.SaveAsync(User(), Request(asset: id), default);
        f.Db.Assets.Remove(await f.Db.Assets.SingleAsync());
        await Assert.ThrowsAsync<DbUpdateException>(() => f.Db.SaveChangesAsync());
        f.Db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<SqliteException>(() => f.Db.Database.ExecuteSqlRawAsync("DELETE FROM world_visual_object_compositions"));
    }

    [Fact]
    public async Task 재전송멱등과개정이력및이전판본을보존한다()
    {
        await using var f = new Fixture(); var r = Request();
        var first = await f.Service.SaveAsync(User(), r, default);
        Assert.True((await f.Service.SaveAsync(User(), r, default)).Duplicate);
        Assert.Equal("IdempotencyConflict", (await f.Service.SaveAsync(User(), r with { Definition = r.Definition with { DisplayName = "다름" } }, default)).Diagnostic);
        var next = r with { RequestId = "request:2", ExpectedRevision = 1, Definition = r.Definition with { DefinitionRevision = "r2", Items = [] } };
        Assert.Equal(2, (await f.Service.SaveAsync(User(), next, default)).Composition!.Revision);
        Assert.Equal(first.Composition!.CompositionId, (await f.Service.GetAsync(User(), r.Definition.DefinitionId, 1, default)).Composition!.CompositionId);
        Assert.Equal(2, (await f.Service.GetAsync(User(), r.Definition.DefinitionId, null, default)).Composition!.Revision);
        Assert.True((await f.Service.SaveAsync(User(), r, default)).Duplicate);
        Assert.Equal("RevisionConflict", (await f.Service.SaveAsync(User(), next with { RequestId = "stale" }, default)).Diagnostic);
        Assert.Equal(2, await f.Db.CompositionHistory.CountAsync());
    }

    [Fact]
    public async Task 저장중이력실패는정의와구성항목도롤백한다()
    {
        await using var f = new Fixture();
        await f.Db.Database.ExecuteSqlRawAsync("CREATE TRIGGER fail_history BEFORE INSERT ON world_visual_object_composition_history BEGIN SELECT RAISE(ABORT, 'fixture'); END;");
        Assert.Equal("CompositionStorageConflictOrFailure", (await f.Service.SaveAsync(User(), Request(), default)).Diagnostic);
        Assert.Empty(await f.Db.Definitions.ToArrayAsync()); Assert.Empty(await f.Db.Compositions.ToArrayAsync());
        Assert.Empty(await f.Db.CompositionItems.ToArrayAsync());
    }

    [Fact]
    public async Task 낡은추적문맥의경합은새판본을부분기록하지않는다()
    {
        await using var f = new Fixture(); var r = Request(); await f.Service.SaveAsync(User(), r, default);
        await using var other = f.Context();
        var second = f.ServiceFor(other);
        await second.SaveAsync(User(), r with { RequestId = "other", ExpectedRevision = 1 }, default);
        Assert.Equal("RevisionConflict", (await f.Service.SaveAsync(User(), r with { RequestId = "stale", ExpectedRevision = 1 }, default)).Diagnostic);
        Assert.Equal(2, await other.Compositions.CountAsync()); Assert.Equal(2, await other.CompositionHistory.CountAsync());
    }

    [Theory]
    [InlineData("anonymous", "Unauthorized")]
    [InlineData("member", "Forbidden")]
    [InlineData("other", "PrincipalMismatch")]
    [InlineData("disabled", "FeatureDisabled")]
    public async Task 권한과기능경계를조회저장에함께적용한다(string mode, string diagnostic)
    {
        await using var f = new Fixture(); if (mode == "disabled") f.Options.Value.ReviewEnabled = false;
        var user = mode switch { "anonymous" => new ClaimsPrincipal(), "member" => User(admin: false), "other" => User("other"), _ => User() };
        Assert.Equal(diagnostic, (await f.Service.SaveAsync(user, Request(), default)).Diagnostic);
        Assert.Equal(diagnostic, (await f.Service.ListAsync(user, 0, default)).Diagnostic);
        Assert.Equal(diagnostic, (await f.Service.GetAsync(user, "fixture:workshop", null, default)).Diagnostic);
        Assert.Empty(await f.Db.Definitions.ToArrayAsync());
    }

    [Theory]
    [InlineData("item")]
    [InlineData("snapshot")]
    public async Task 저장사본과FK행변조는조회에서숨기지않는다(string mode)
    {
        await using var f = new Fixture(); await f.Service.SaveAsync(User(), Request(), default);
        await f.Db.Database.ExecuteSqlRawAsync(mode == "item" ? "UPDATE world_visual_object_composition_items SET AnchorIntent='tampered'" : "UPDATE world_visual_object_compositions SET SnapshotHash='tampered'");
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.GetAsync(User(), "fixture:workshop", null, default));
    }

    private sealed class Current : ICurrentUserAccessor { public string? UserId => "admin"; public string? Role => 살뜰.Data.역할명.서버관리자; }
    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly ServiceProvider services;
        public 개체시각대응Tests.Monitor Options { get; } = new() { Value = new() { ReviewEnabled = true, Enabled = false } };
        public 개체시각대응DbContext Db { get; }
        public 게임객체시각구성UseCase Service { get; }
        public Fixture()
        {
            connection.Open(); Db = Context(); Db.Database.EnsureCreated();
            var s = new ServiceCollection(); s.AddLogging();
            s.AddAuthorizationCore(o => o.AddPolicy(개체시각대응Codes.Policy, p => p.RequireRole(살뜰.Data.역할명.서버관리자)));
            services = s.BuildServiceProvider(); Service = ServiceFor(Db);
        }
        public 개체시각대응DbContext Context() => new(new DbContextOptionsBuilder<개체시각대응DbContext>().UseSqlite(connection).Options);
        public 게임객체시각구성UseCase ServiceFor(개체시각대응DbContext db) => new(db, services.GetRequiredService<IAuthorizationService>(), new Current(), Options, TimeProvider.System);
        public async Task<string> Asset()
        {
            var m = new 개체시각자산입력("fixture.asset", "r1", "Assets/Ssalddel/Fixture.asset", new('A', 64), "Synty", "Test",
                "Assets/Synty/Test/fixture.prefab", new('a', 32), new('B', 64), new('C', 64), "Fixture", "Fixture", "docs/fixture.md", new('D', 64));
            var id = 개체시각목록UseCase.Id(m);
            Db.Assets.Add(new() { AssetVersionId = id, VisualKey = m.VisualKey, CatalogRevision = m.CatalogRevision,
                PrefabGuid = m.PrefabGuid, MetadataJson = JsonSerializer.Serialize(m), MetadataHash = 개체시각선택Policy.Hash(m), RegisteredBy = "fixture" });
            await Db.SaveChangesAsync(); return id; // SQLite synthetic input only, not a verified Synty source file.
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await connection.DisposeAsync(); await services.DisposeAsync(); }
    }
}
