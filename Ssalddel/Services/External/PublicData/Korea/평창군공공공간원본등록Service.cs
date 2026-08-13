using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData;
using Ssalddel.Infrastructure.Persistence.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed record 공공공간원본등록Request(
    string SourceId,
    string DatasetId,
    string SourceVersion,
    string DataRevision,
    DateTimeOffset? EvidenceAsOfUtc,
    string ContentType,
    string PrivateStorageLocation);

public sealed record 공공공간원본등록Result(
    long RawSnapshotId,
    bool Inserted,
    string SourceHashSha256,
    long ContentLength);

public sealed record 평창군토지피복통계정규화Result(
    int ParsedRowCount,
    int InsertedRecordCount,
    int ExistingRecordCount);

/// <summary>
/// 로그인 다운로드 원본을 공유 공공데이터 DB의 계보와 정규화 수치로 축적합니다.
/// 원본 파일은 비공개 보관소에 유지하고 DB에는 hash·경로·기준일만 저장합니다.
/// </summary>
public sealed class 평창군공공공간원본등록Service
{
    private readonly PublicDataIngestionDbContext _db;

    public 평창군공공공간원본등록Service(PublicDataIngestionDbContext db)
        => _db = db;

    public async Task<공공공간원본등록Result> RegisterFileAsync(
        string filePath,
        공공공간원본등록Request request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("PublicSpatialSourceMissing", filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatasetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DataRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PrivateStorageLocation);

        await using var stream = File.OpenRead(filePath);
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken))
            .ToLowerInvariant();
        var length = stream.Length;
        var existing = await _db.RawSnapshots.SingleOrDefaultAsync(item =>
            item.SourceId == request.SourceId
            && item.DatasetId == request.DatasetId
            && item.ContentHashSha256 == hash,
            cancellationToken);
        if (existing != null)
        {
            existing.LastSeenAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return new 공공공간원본등록Result(existing.Id, false, hash, length);
        }

        var now = DateTimeOffset.UtcNow;
        var run = new 외부데이터수집Run
        {
            RunKey = $"public-spatial-register:{now:yyyyMMddHHmmssfff}:{hash[..12]}",
            SourceId = request.SourceId,
            DatasetId = request.DatasetId,
            StatusCode = 외부데이터수집StatusCodes.Success,
            StartedAtUtc = now,
            CompletedAtUtc = now,
            AttemptCount = 1,
            FetchedCount = 1,
            InsertedCount = 1,
            SourceVersion = request.SourceVersion,
            DataRevision = request.DataRevision,
        };
        _db.IngestionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        var snapshot = new 외부데이터RawSnapshot
        {
            FirstCollectionRunId = run.Id,
            SourceId = request.SourceId,
            DatasetId = request.DatasetId,
            SourceVersion = request.SourceVersion,
            CollectedAtUtc = now,
            EvidenceAsOfUtc = request.EvidenceAsOfUtc,
            ContentHashSha256 = hash,
            ContentLength = length,
            ContentType = request.ContentType,
            OriginalFileName = Path.GetFileName(filePath),
            StorageContainer = "local-private-public-spatial",
            StorageObjectName = request.PrivateStorageLocation.Replace('\\', '/'),
            StorageLocation = "private-file://" + request.PrivateStorageLocation.Replace('\\', '/'),
            FirstSeenAtUtc = now,
            LastSeenAtUtc = now,
        };
        _db.RawSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(cancellationToken);
        return new 공공공간원본등록Result(snapshot.Id, true, hash, length);
    }

    public async Task<평창군토지피복통계정규화Result> NormalizeLandCoverStatisticsAsync(
        string csvPath,
        long rawSnapshotId,
        string dataRevision,
        CancellationToken cancellationToken = default)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = new StreamReader(csvPath, Encoding.UTF8, true);
        using var rows = CsvRows(reader).GetEnumerator();
        if (!rows.MoveNext())
            return new 평창군토지피복통계정규화Result(0, 0, 0);
        var headers = rows.Current;
        var candidates = new List<외부데이터정규화Record>();
        var parsedRows = 0;
        while (rows.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var values = rows.Current;
            if (values.Length < 4 || values[2].Trim() != "평창군")
                continue;
            if (!int.TryParse(values[0], NumberStyles.None, CultureInfo.InvariantCulture, out var year))
                continue;
            parsedRows++;
            var evidenceAt = new DateTimeOffset(year, 12, 31, 0, 0, 0, TimeSpan.Zero);
            for (var index = 3; index < Math.Min(headers.Length, values.Length); index++)
            {
                if (!decimal.TryParse(values[index], NumberStyles.Number, CultureInfo.InvariantCulture, out var area))
                    continue;
                var metricCode = "land-cover-area-square-km:" + index.ToString(CultureInfo.InvariantCulture);
                var dimension = "category-ko=" + headers[index].Trim();
                var recordKey = 외부데이터RecordKey.Create(
                    "mcee-environmental-spatial-information",
                    "detailed-land-cover-area-by-region",
                    "region:kr:sig:51760",
                    metricCode,
                    evidenceAt,
                    dimension);
                candidates.Add(new 외부데이터정규화Record
                {
                    RawSnapshotId = rawSnapshotId,
                    RecordKey = recordKey,
                    StableId = $"metric:kr:51760:land-cover:{year}:{index}",
                    SourceId = "mcee-environmental-spatial-information",
                    DatasetId = "detailed-land-cover-area-by-region",
                    RegionStableId = "region:kr:sig:51760",
                    MetricCode = metricCode,
                    NumericValue = area,
                    TextValue = headers[index].Trim(),
                    UnitCode = "km2",
                    EvidenceAsOfUtc = evidenceAt,
                    CollectedAtUtc = DateTimeOffset.UtcNow,
                    SpatialPrecisionCode = "sigungu-statistic",
                    TemporalPrecisionCode = "year",
                    QualityCode = "Observed",
                    LimitationCode = "AreaStatisticWithoutGeometry",
                    DimensionKey = dimension,
                    SourceVersion = "2013-2024",
                    DataRevision = dataRevision,
                    FirstSeenAtUtc = DateTimeOffset.UtcNow,
                    LastSeenAtUtc = DateTimeOffset.UtcNow,
                });
            }
        }

        var keys = candidates.Select(item => item.RecordKey).ToList();
        var existingKeys = new HashSet<string>(StringComparer.Ordinal);
        for (var start = 0; start < keys.Count; start += 500)
        {
            var batch = keys.Skip(start).Take(500).ToList();
            existingKeys.UnionWith(await _db.NormalizedRecords
                .Where(item => batch.Contains(item.RecordKey))
                .Select(item => item.RecordKey)
                .ToListAsync(cancellationToken));
        }
        var additions = candidates.Where(item => !existingKeys.Contains(item.RecordKey)).ToArray();
        _db.NormalizedRecords.AddRange(additions);
        await _db.SaveChangesAsync(cancellationToken);
        return new 평창군토지피복통계정규화Result(
            parsedRows,
            additions.Length,
            existingKeys.Count);
    }

    private static IEnumerable<string[]> CsvRows(TextReader reader)
    {
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        while (reader.Read() is var value && value >= 0)
        {
            var character = (char)value;
            if (quoted)
            {
                if (character == '"' && reader.Peek() == '"')
                {
                    reader.Read();
                    field.Append('"');
                }
                else if (character == '"')
                    quoted = false;
                else
                    field.Append(character);
                continue;
            }
            if (character == '"' && field.Length == 0)
                quoted = true;
            else if (character == ',')
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (character == '\n')
            {
                row.Add(field.ToString().TrimEnd('\r'));
                field.Clear();
                yield return row.ToArray();
                row.Clear();
            }
            else if (character != '\r')
                field.Append(character);
        }
        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            yield return row.ToArray();
        }
    }
}
