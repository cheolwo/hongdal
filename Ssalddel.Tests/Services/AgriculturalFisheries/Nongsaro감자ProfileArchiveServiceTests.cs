using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Nongsaro감자ProfileArchiveServiceTests
{
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

    private sealed class FakeProfileQuery(농사로작물생육요구ProfileResponse value)
        : I농사로감자생육요구Profile조회UseCase
    {
        public 농사로작물생육요구ProfileResponse Value { get; set; } = value;
        public Task<농사로작물생육요구ProfileResponse> 조회Async(
            CancellationToken cancellationToken = default) => Task.FromResult(Value);
    }

    private sealed class FakeDisasterModule(string hash) : I농사로농작물재해예방Module
    {
        public Task<Nongsaro공공데이터Response> 연도조회Async(
            CancellationToken cancellationToken = default) => Task.FromResult(new
                Nongsaro공공데이터Response("frcDsstrPrevnt", "frcDsstrPrevntYear",
                    "00", "정상", DateTimeOffset.UtcNow,
                    "https://www.nongsaro.go.kr", [], hash));
    }
}
