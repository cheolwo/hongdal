using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed record Mof어획구역Catalog적재Result(
    string StatusCode,
    string RunKey,
    long? SnapshotId,
    bool SnapshotCreated,
    int SourceRowCount,
    string ContentSha256,
    string ErrorMessage);

public interface IMof어획구역CatalogArchiveService
{
    Task<Mof어획구역Catalog적재Result> CollectAsync(
        string runKey,
        CancellationToken cancellationToken = default);
}

public sealed class Mof어획구역CatalogArchiveService(
    AgriculturalFisheriesDbContext db,
    IMof어획구역CatalogSource source,
    IOptions<PublicDataOptions> options,
    TimeProvider timeProvider) : IMof어획구역CatalogArchiveService
{
    private const string SourceKey = "mof-fishing-area-catalog";

    public async Task<Mof어획구역Catalog적재Result> CollectAsync(
        string runKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runKey);
        var configured = options.Value.MofFishingAreas;
        if (!configured.PersistenceEnabled)
        {
            return new Mof어획구역Catalog적재Result(
                Mof어획구역Catalog수집상태Codes.비활성,
                runKey.Trim(),
                null,
                false,
                0,
                string.Empty,
                "PublicData:MofFishingAreas:PersistenceEnabled가 false여서 외부 호출과 DB 쓰기를 수행하지 않았습니다.");
        }

        var normalizedRunKey = runKey.Trim();
        var existingRun = await db.MofFishingAreaCollectionRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.RunKey == normalizedRunKey, cancellationToken);
        if (existingRun is not null)
        {
            return Result(existingRun, snapshotCreated: false);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var run = new Mof어획구역Catalog수집Run
        {
            RunKey = normalizedRunKey,
            DatasetVersion = configured.DatasetVersion,
            StartedAtUtc = now
        };
        db.MofFishingAreaCollectionRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var collected = await source.수집Async(cancellationToken);
            var normalizedRecords = collected.Records
                .Where(item => !string.IsNullOrWhiteSpace(item.Abbreviation)
                    || !string.IsNullOrWhiteSpace(item.KoreanName))
                .OrderBy(item => item.Abbreviation, StringComparer.Ordinal)
                .ThenBy(item => item.KoreanName, StringComparer.Ordinal)
                .Select(item => new NormalizedRecord(
                    item.Abbreviation.Trim(),
                    item.EnglishName.Trim(),
                    item.KoreanName.Trim(),
                    item.SeaName.Trim()))
                .ToArray();
            var snapshot = await db.MofFishingAreaSnapshots.FirstOrDefaultAsync(
                item => item.SourceKey == SourceKey
                    && item.DatasetVersion == configured.DatasetVersion
                    && item.ContentSha256 == collected.ContentSha256,
                cancellationToken);
            var snapshotCreated = snapshot is null;
            if (snapshot is null)
            {
                snapshot = new Mof어획구역Catalog영속Snapshot
                {
                    SourceUrl = configured.SourceUrl,
                    DatasetVersion = configured.DatasetVersion,
                    ContentSha256 = collected.ContentSha256,
                    CollectedAtUtc = collected.CollectedAtUtc,
                    StoredAtUtc = now,
                    LastSeenAtUtc = now,
                    SourceRowCount = collected.Records.Count,
                    NormalizedRecordCount = normalizedRecords.Length,
                    FreshnessCode = 커뮤니티세계지도FreshnessCodes.Fresh,
                    NormalizedRecordsJson = JsonSerializer.Serialize(normalizedRecords)
                };
                db.MofFishingAreaSnapshots.Add(snapshot);
            }
            else
            {
                snapshot.LastSeenAtUtc = now;
                snapshot.FreshnessCode = 커뮤니티세계지도FreshnessCodes.Fresh;
            }

            run.StatusCode = Mof어획구역Catalog수집상태Codes.완료;
            run.ContentSha256 = collected.ContentSha256;
            run.SourceRowCount = collected.Records.Count;
            run.CompletedAtUtc = now;
            run.Snapshot = snapshot;
            await db.SaveChangesAsync(cancellationToken);
            return Result(run, snapshotCreated);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run.StatusCode = Mof어획구역Catalog수집상태Codes.실패;
            run.ErrorMessage = exception.Message.Length <= 2000
                ? exception.Message
                : exception.Message[..2000];
            run.CompletedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
            await db.SaveChangesAsync(cancellationToken);
            return Result(run, snapshotCreated: false);
        }
    }

    private static Mof어획구역Catalog적재Result Result(
        Mof어획구역Catalog수집Run run,
        bool snapshotCreated)
        => new(
            run.StatusCode,
            run.RunKey,
            run.SnapshotId,
            snapshotCreated,
            run.SourceRowCount,
            run.ContentSha256,
            run.ErrorMessage);

    private sealed record NormalizedRecord(
        string Abbreviation,
        string EnglishName,
        string KoreanName,
        string SeaName);
}
