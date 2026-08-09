using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData;

namespace Ssalddel.Infrastructure.Persistence.PublicData;

public sealed class EfExternalDataIngestionStore : I외부데이터수집Store, I외부지역MappingStore
{
    private readonly PublicDataIngestionDbContext db;

    public EfExternalDataIngestionStore(PublicDataIngestionDbContext db)
        => this.db = db ?? throw new ArgumentNullException(nameof(db));

    public async Task<외부데이터수집Run> StartRunAsync(
        외부데이터수집Run run,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);
        db.IngestionRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        return run;
    }

    public Task<외부데이터RawSnapshot?> FindRawSnapshotAsync(
        string sourceId,
        string datasetId,
        string contentHashSha256,
        CancellationToken cancellationToken = default)
        => db.RawSnapshots.SingleOrDefaultAsync(item =>
            item.SourceId == sourceId
            && item.DatasetId == datasetId
            && item.ContentHashSha256 == contentHashSha256,
            cancellationToken);

    public async Task<외부데이터RawSnapshot> SaveRawSnapshotAsync(
        외부데이터RawSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        db.RawSnapshots.Add(snapshot);
        await db.SaveChangesAsync(cancellationToken);
        return snapshot;
    }

    public async Task<외부데이터정규화저장Result> UpsertNormalizedAsync(
        IReadOnlyCollection<외부데이터정규화Record> records,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        if (records.Count == 0) return new 외부데이터정규화저장Result(0, 0, 0);
        // List.Contains keeps the expression translatable across the current EF providers;
        // array Contains can bind to the .NET 10 span overload during parameter evaluation.
        var keys = records.Select(item => item.RecordKey).ToList();
        var existingItems = await db.NormalizedRecords
            .Where(item => keys.Contains(item.RecordKey))
            .ToListAsync(cancellationToken);
        var existing = existingItems.ToDictionary(item => item.RecordKey, StringComparer.Ordinal);
        var inserted = 0;
        var updated = 0;
        var unchanged = 0;
        foreach (var incoming in records)
        {
            if (!existing.TryGetValue(incoming.RecordKey, out var stored))
            {
                db.NormalizedRecords.Add(incoming);
                inserted++;
                continue;
            }

            if (Equivalent(stored, incoming))
            {
                stored.LastSeenAtUtc = incoming.LastSeenAtUtc;
                unchanged++;
                continue;
            }

            stored.RawSnapshotId = incoming.RawSnapshotId;
            stored.StableId = incoming.StableId;
            stored.RegionStableId = incoming.RegionStableId;
            stored.MetricCode = incoming.MetricCode;
            stored.NumericValue = incoming.NumericValue;
            stored.TextValue = incoming.TextValue;
            stored.UnitCode = incoming.UnitCode;
            stored.EvidenceAsOfUtc = incoming.EvidenceAsOfUtc;
            stored.CollectedAtUtc = incoming.CollectedAtUtc;
            stored.SpatialPrecisionCode = incoming.SpatialPrecisionCode;
            stored.TemporalPrecisionCode = incoming.TemporalPrecisionCode;
            stored.QualityCode = incoming.QualityCode;
            stored.LimitationCode = incoming.LimitationCode;
            stored.DimensionKey = incoming.DimensionKey;
            stored.SourceVersion = incoming.SourceVersion;
            stored.DataRevision = incoming.DataRevision;
            stored.LastSeenAtUtc = incoming.LastSeenAtUtc;
            updated++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new 외부데이터정규화저장Result(inserted, updated, unchanged);
    }

    public async Task CompleteRunAsync(
        외부데이터수집Run run,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);
        db.IngestionRuns.Update(run);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<외부지역CodeMapping?> FindAsync(
        string sourceId,
        string externalRegionCode,
        CancellationToken cancellationToken = default)
        => db.RegionMappings.AsNoTracking().SingleOrDefaultAsync(item =>
            item.SourceId == sourceId && item.ExternalRegionCode == externalRegionCode,
            cancellationToken);

    private static bool Equivalent(
        외부데이터정규화Record stored,
        외부데이터정규화Record incoming)
        => stored.NumericValue == incoming.NumericValue
           && string.Equals(stored.TextValue, incoming.TextValue, StringComparison.Ordinal)
           && string.Equals(stored.UnitCode, incoming.UnitCode, StringComparison.Ordinal)
           && stored.EvidenceAsOfUtc == incoming.EvidenceAsOfUtc
           && string.Equals(stored.SpatialPrecisionCode, incoming.SpatialPrecisionCode, StringComparison.Ordinal)
           && string.Equals(stored.TemporalPrecisionCode, incoming.TemporalPrecisionCode, StringComparison.Ordinal)
           && string.Equals(stored.QualityCode, incoming.QualityCode, StringComparison.Ordinal)
           && string.Equals(stored.LimitationCode, incoming.LimitationCode, StringComparison.Ordinal)
           && string.Equals(stored.DimensionKey, incoming.DimensionKey, StringComparison.Ordinal)
           && string.Equals(stored.SourceVersion, incoming.SourceVersion, StringComparison.Ordinal)
           && string.Equals(stored.DataRevision, incoming.DataRevision, StringComparison.Ordinal);
}
