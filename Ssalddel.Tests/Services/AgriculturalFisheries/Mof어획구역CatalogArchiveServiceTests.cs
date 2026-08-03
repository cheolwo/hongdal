using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Mof어획구역CatalogArchiveServiceTests
{
    [Fact]
    public async Task 영속옵션이꺼져있으면_외부호출과Db쓰기를하지않는다()
    {
        await using var db = CreateDb();
        var source = new FakeSource(Snapshot("hash-disabled"));
        var service = CreateService(db, source, persistenceEnabled: false);

        var result = await service.CollectAsync("disabled-run");

        Assert.Equal(Mof어획구역Catalog수집상태Codes.비활성, result.StatusCode);
        Assert.Equal(0, source.CallCount);
        Assert.Empty(await db.MofFishingAreaCollectionRuns.ToArrayAsync());
        Assert.Empty(await db.MofFishingAreaSnapshots.ToArrayAsync());
    }

    [Fact]
    public async Task 같은RunKey재시도는_외부호출과Snapshot을중복생성하지않는다()
    {
        await using var db = CreateDb();
        var source = new FakeSource(Snapshot("hash-same-run"));
        var service = CreateService(db, source, persistenceEnabled: true);

        var first = await service.CollectAsync("same-run");
        var second = await service.CollectAsync("same-run");

        Assert.True(first.SnapshotCreated);
        Assert.False(second.SnapshotCreated);
        Assert.Equal(first.SnapshotId, second.SnapshotId);
        Assert.Equal(1, source.CallCount);
        Assert.Single(await db.MofFishingAreaCollectionRuns.ToArrayAsync());
        Assert.Single(await db.MofFishingAreaSnapshots.ToArrayAsync());
    }

    [Fact]
    public async Task 같은원문Hash의새Run은_Run만남기고Snapshot을재사용한다()
    {
        await using var db = CreateDb();
        var source = new FakeSource(Snapshot("hash-same-content"));
        var service = CreateService(db, source, persistenceEnabled: true);

        var first = await service.CollectAsync("run-1");
        var second = await service.CollectAsync("run-2");

        Assert.True(first.SnapshotCreated);
        Assert.False(second.SnapshotCreated);
        Assert.Equal(first.SnapshotId, second.SnapshotId);
        Assert.Equal(2, source.CallCount);
        Assert.Equal(2, await db.MofFishingAreaCollectionRuns.CountAsync());
        var snapshot = Assert.Single(await db.MofFishingAreaSnapshots.ToArrayAsync());
        Assert.Equal(2, snapshot.SourceRowCount);
        Assert.Equal(2, snapshot.NormalizedRecordCount);
        using var normalized = JsonDocument.Parse(snapshot.NormalizedRecordsJson);
        Assert.Contains(normalized.RootElement.EnumerateArray(), item =>
            item.GetProperty("SeaName").GetString() == "태평양");
        Assert.Equal("fresh", snapshot.FreshnessCode);
    }

    [Fact]
    public async Task 수집오류는_실패Run으로저장하고Snapshot을만들지않는다()
    {
        await using var db = CreateDb();
        var source = new FakeSource(new InvalidOperationException("fixture failure"));
        var service = CreateService(db, source, persistenceEnabled: true);

        var result = await service.CollectAsync("failed-run");

        Assert.Equal(Mof어획구역Catalog수집상태Codes.실패, result.StatusCode);
        Assert.Contains("fixture failure", result.ErrorMessage);
        var run = Assert.Single(await db.MofFishingAreaCollectionRuns.ToArrayAsync());
        Assert.Equal(Mof어획구역Catalog수집상태Codes.실패, run.StatusCode);
        Assert.Empty(await db.MofFishingAreaSnapshots.ToArrayAsync());
    }

    private static AgriculturalFisheriesDbContext CreateDb()
        => new(new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase($"mof-fishing-area-{Guid.NewGuid():N}")
            .Options);

    private static Mof어획구역CatalogArchiveService CreateService(
        AgriculturalFisheriesDbContext db,
        IMof어획구역CatalogSource source,
        bool persistenceEnabled)
        => new(
            db,
            source,
            Options.Create(new PublicDataOptions
            {
                MofFishingAreas = new MofFishingAreaCatalogOptions
                {
                    PersistenceEnabled = persistenceEnabled,
                    DatasetVersion = "20211230",
                    SourceUrl = "https://www.data.go.kr/data/15147444/fileData.do"
                }
            }),
            TimeProvider.System);

    private static Mof어획구역CatalogSnapshot Snapshot(string hash)
        => new(
            new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc),
            hash,
            [
                new("A", "Pacific A", "태평양 A", "태평양"),
                new("B", "Atlantic B", "대서양 B", "대서양")
            ]);

    private sealed class FakeSource : IMof어획구역CatalogSource
    {
        private readonly Mof어획구역CatalogSnapshot? _snapshot;
        private readonly Exception? _exception;

        public FakeSource(Mof어획구역CatalogSnapshot snapshot) => _snapshot = snapshot;

        public FakeSource(Exception exception) => _exception = exception;

        public int CallCount { get; private set; }

        public Task<Mof어획구역CatalogSnapshot> 수집Async(
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return _exception is null
                ? Task.FromResult(_snapshot!)
                : Task.FromException<Mof어획구역CatalogSnapshot>(_exception);
        }
    }
}
