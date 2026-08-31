using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Nongsaro감자ProfileArchiveServiceTests
{
    [Fact]
    public async Task 같은내용의다른입수시각은_새개정이나원기록덮어쓰기가아니다()
    {
        await using var db = CreateDb();
        var query = new FakeProfileQuery(Profile(Hash('a')));
        var disaster = new FakeDisasterModule(Hash('d'));
        var service = new Nongsaro감자ProfileArchiveService(query, disaster, db, TimeProvider.System);
        var first = await service.CollectAndArchiveAsync(false);
        var originalJson = first.ProfileJson;
        var originalHash = first.SourceSetHashSha256;
        var originalAt = first.RetrievedAtUtc;
        query.Value = query.Value with
        {
            RetrievedAtUtc = query.Value.RetrievedAtUtc.AddDays(1),
            Sources = query.Value.Sources.Select(item => item with
            { RetrievedAtUtc = item.RetrievedAtUtc.AddDays(1) }).ToArray()
        };
        var next = await service.CollectAndArchiveAsync(false);
        Assert.Equal(first.Id, next.Id);
        Assert.Equal(originalJson, next.ProfileJson);
        Assert.Equal(originalHash, next.SourceSetHashSha256);
        Assert.Equal(originalAt, next.RetrievedAtUtc);
        Assert.Single(await db.NongsaroPotatoProfiles.ToArrayAsync());
    }

    [Fact]
    public async Task 원문A_B_A는_과거행재선택없이_세번째개정이다()
    {
        await using var db = CreateDb();
        var query = new FakeProfileQuery(Profile(Hash('a')));
        var original = query.Value;
        var service = new Nongsaro감자ProfileArchiveService(query, new FakeDisasterModule(Hash('d')), db, TimeProvider.System);
        var first = await service.CollectAndArchiveAsync(false);
        query.Value = Profile(Hash('b'));
        Assert.Equal(2, (await service.CollectAndArchiveAsync(false)).Revision);
        query.Value = original;
        var reverted = await service.CollectAndArchiveAsync(false);
        Assert.Equal(3, reverted.Revision);
        Assert.NotEqual(first.Id, reverted.Id);
        Assert.Equal(first.ProfileJson, reverted.ProfileJson);
    }

    [Fact]
    public async Task 최신보류자료는_과거승인자료로fallback하지않고_조회는추적하지않는다()
    {
        await using var db = CreateDb();
        var query = new FakeProfileQuery(Profile(Hash('a')));
        var service = new Nongsaro감자ProfileArchiveService(query, new FakeDisasterModule(Hash('d')), db, TimeProvider.System);
        Assert.Null(await service.최신자료승인조회Async());
        var first = await service.CollectAndArchiveAsync(true);
        db.ChangeTracker.Clear();
        var read = await service.최신자료승인조회Async();
        Assert.Equal(first.Id, read!.Id);
        Assert.Empty(db.ChangeTracker.Entries());
        query.Value = Profile(Hash('b'));
        await service.CollectAndArchiveAsync(false);
        Assert.Null(await service.최신자료승인조회Async());
        Assert.Equal(2, await db.NongsaroPotatoProfiles.CountAsync());
    }

    [Theory]
    [InlineData("StableId")]
    [InlineData("Product")]
    [InlineData("Group")]
    [InlineData("ContentNo")]
    [InlineData("Relation")]
    [InlineData("SourceHash")]
    [InlineData("SourcesNull")]
    [InlineData("DuplicateSource")]
    [InlineData("PublishRule")]
    [InlineData("Revision")]
    public async Task 손상된감자사본은_재해호출과DB변경전에거부한다(string defect)
    {
        await using var db = CreateDb();
        var profile = Profile(Hash('a'));
        profile = defect switch
        {
            "StableId" => profile with { StableId = "arbitrary" },
            "Product" => profile with { CanonicalProductStableId = "product:other" },
            "Group" => profile with { WorkScheduleGroupCode = "30699" },
            "ContentNo" => profile with { WorkScheduleContentNo = "210005" },
            "Relation" => profile with { NongsaroProductRelationStatusCode = 공통식품품목관계StatusCodes.Confirmed },
            "SourceHash" => profile with { Sources = [profile.Sources[0] with { RawContentHashSha256 = Hash('z') }] },
            "SourcesNull" => profile with { Sources = null! },
            "DuplicateSource" => profile with { Sources = [profile.Sources[0], profile.Sources[0]] },
            "PublishRule" => profile with { CanPublishSimulationRule = true },
            "Revision" => profile with { Revision = 0 },
            _ => throw new InvalidOperationException()
        };
        var disaster = new FakeDisasterModule(Hash('d'));
        var service = new Nongsaro감자ProfileArchiveService(new FakeProfileQuery(profile), disaster, db, TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CollectAndArchiveAsync(false));
        Assert.Equal(0, disaster.Calls);
        Assert.Empty(await db.NongsaroPotatoProfiles.ToArrayAsync());
    }

    [Theory]
    [InlineData("ErrorResult")]
    [InlineData("InvalidHash")]
    [InlineData("FetchFailure")]
    public async Task 재해자료오류는_마지막성공자료를변경하지않는다(string defect)
    {
        await using var db = CreateDb();
        var query = new FakeProfileQuery(Profile(Hash('a')));
        var disaster = new FakeDisasterModule(Hash('d'));
        var service = new Nongsaro감자ProfileArchiveService(query, disaster, db, TimeProvider.System);
        var first = await service.CollectAndArchiveAsync(true);
        query.Value = Profile(Hash('b'));
        disaster.ResultCode = defect == "ErrorResult" ? "99" : "00";
        disaster.ContentHash = defect == "InvalidHash" ? Hash('z') : Hash('d');
        disaster.Fail = defect == "FetchFailure";
        await Assert.ThrowsAnyAsync<Exception>(() => service.CollectAndArchiveAsync(false));
        Assert.Single(await db.NongsaroPotatoProfiles.ToArrayAsync());
        Assert.Equal(first.Id, (await service.최신자료승인조회Async())!.Id);
    }

    [Fact]
    public async Task 손상된저장사본과거부자료는_승인복구하지않는다()
    {
        await using var db = CreateDb();
        var query = new FakeProfileQuery(Profile(Hash('a')));
        var service = new Nongsaro감자ProfileArchiveService(query, new FakeDisasterModule(Hash('d')), db, TimeProvider.System);
        query.Value = query.Value with { ReviewStatusCode = 작물생육요구검토StatusCodes.Rejected };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CollectAndArchiveAsync(true));
        var rejected = await service.CollectAndArchiveAsync(false);
        Assert.Null(await service.최신자료승인조회Async());
        rejected.ProfileJson = "{bad-json";
        await db.SaveChangesAsync();
        query.Value = Profile(Hash('b'));
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.CollectAndArchiveAsync(false));
        Assert.Single(await db.NongsaroPotatoProfiles.ToArrayAsync());
    }

    [Fact]
    public async Task 같은원문은멱등이고_바뀐원문은새ArchiveRevision으로보관한다()
    {
        await using var db = new AgriculturalFisheriesDbContext(
            new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
                .UseInMemoryDatabase("nongsaro-profile-" + Guid.NewGuid()).Options);
        var query = new FakeProfileQuery(Profile(Hash('a')));
        var disaster = new FakeDisasterModule(Hash('d'));
        var service = new Nongsaro감자ProfileArchiveService(
            query, disaster, db, TimeProvider.System);

        var first = await service.CollectAndArchiveAsync(false);
        var repeated = await service.CollectAndArchiveAsync(true);
        query.Value = Profile(Hash('b'));
        var changed = await service.CollectAndArchiveAsync(false);

        Assert.Equal(first.Id, repeated.Id);
        Assert.True(repeated.ApprovedForSimulationContext);
        Assert.Equal(1, first.Revision);
        Assert.Equal(2, changed.Revision);
        Assert.Equal(2, await db.NongsaroPotatoProfiles.CountAsync());
    }

    private static 농사로작물생육요구ProfileResponse Profile(string hash) => new(
        "crop-requirement-profile:nongsaro.potato.1", 1, "product:potato", "감자",
        "210005", "밭농사", "30699", 공통식품품목관계StatusCodes.Unlinked,
        작물생육요구검토StatusCodes.PendingHumanReview, false,
        DateTimeOffset.UtcNow,
        [new 농사로작물생육SourceSnapshot("source:test", "farmWorkingPlanNew",
            "workScheduleDtl", "30699", DateTimeOffset.UtcNow,
            "https://www.nongsaro.go.kr", hash)],
        [], ["정보 문맥 전용"]);

    private static string Hash(char value) => new(value, 64);

    private static AgriculturalFisheriesDbContext CreateDb() => new(
        new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase("d424-synthetic-only-" + Guid.NewGuid()).Options);

    private sealed class FakeProfileQuery(농사로작물생육요구ProfileResponse value)
        : I농사로감자생육요구Profile조회UseCase
    {
        public 농사로작물생육요구ProfileResponse Value { get; set; } = value;
        public Task<농사로작물생육요구ProfileResponse> 조회Async(
            CancellationToken cancellationToken = default) => Task.FromResult(Value);
    }

    private sealed class FakeDisasterModule(string hash) : I농사로농작물재해예방Module
    {
        public int Calls { get; private set; }
        public string ContentHash { get; set; } = hash;
        public string ResultCode { get; set; } = "00";
        public bool Fail { get; set; }
        public Task<Nongsaro공공데이터Response> 연도조회Async(
            CancellationToken cancellationToken = default)
        {
            Calls++;
            if (Fail) throw new HttpRequestException("Synthetic source unavailable");
            return Task.FromResult(new Nongsaro공공데이터Response("frcDsstrPrevnt", "frcDsstrPrevntYear",
                ResultCode, "정상", DateTimeOffset.UtcNow,
                "https://www.nongsaro.go.kr", [], ContentHash));
        }
    }
}
