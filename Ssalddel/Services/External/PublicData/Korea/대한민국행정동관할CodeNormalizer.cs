using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed class 대한민국행정동관할CodeNormalizer : IExternalDataNormalizer
{
    private readonly 대한민국행정동관할CodeOptions options;

    public 대한민국행정동관할CodeNormalizer(IOptions<대한민국행정동관할CodeOptions> options)
        => this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public bool CanNormalize(ExternalDataSourceDefinition source)
        => source.SourceId == 대한민국행정동관할CodeDataset.SourceId
           && source.DatasetId == 대한민국행정동관할CodeDataset.DatasetId;

    public async Task<ExternalDataNormalizationBatch> NormalizeAsync(
        ExternalDataSourceDefinition source,
        외부데이터RawSnapshot rawSnapshot,
        IExternalDataRawStorage rawStorage,
        CancellationToken cancellationToken = default)
    {
        if (!CanNormalize(source))
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.NormalizerMissing);

        try
        {
            await using var stream = await rawStorage.OpenReadAsync(rawSnapshot, cancellationToken);
            var bytes = await 대한민국법정동CodeArchiveReader.ReadAllBytesAsync(
                stream,
                options.MaxArchiveBytes,
                cancellationToken);
            var archive = 대한민국행정동관할CodeArchiveReader.Read(
                bytes,
                options.MaxExpandedBytes,
                options.MaxRecordCount);
            var evidenceAt = rawSnapshot.EvidenceAsOfUtc ?? rawSnapshot.CollectedAtUtc;
            var dataRevision = $"mois-hjd-bjd-{archive.기준일}-{rawSnapshot.ContentHashSha256[..20]}";
            var records = archive.행정기관들
                .Select(row => MapAdministrative(row, rawSnapshot, evidenceAt, dataRevision))
                .Concat(archive.관할관계들.Select(row =>
                    MapJurisdiction(row, rawSnapshot, evidenceAt, dataRevision)))
                .ToArray();
            return new ExternalDataNormalizationBatch(records, 0, dataRevision);
        }
        catch (Exception error) when (error is InvalidDataException or ArgumentOutOfRangeException)
        {
            throw new ExternalDataCollectionException(
                ExternalDataCollectionErrorCode.InvalidPayload,
                innerException: error);
        }
    }

    private static 외부데이터정규화Record MapAdministrative(
        대한민국행정기관CodeRow row,
        외부데이터RawSnapshot raw,
        DateTimeOffset evidenceAt,
        string dataRevision)
    {
        var regionId = $"region:kr:hjd:{row.행정기관Code}";
        var parent = row.상위행정기관Code is null
            ? string.Empty
            : $"region:kr:hjd:{row.상위행정기관Code}";
        var dimension = string.Join('|',
            $"code={row.행정기관Code}",
            $"status={(row.말소일 is null ? "active" : "abolished")}",
            $"level={row.행정계층Code}",
            $"parent={parent}",
            $"valid-from={row.생성일:yyyy-MM-dd}",
            $"valid-to={(row.말소일 is null ? string.Empty : row.말소일.Value.ToString("yyyy-MM-dd"))}");
        return CreateRecord(
            raw,
            regionId,
            regionId,
            대한민국행정동관할CodeDataset.AdministrativeMetricCode,
            row.전체명,
            row.행정계층Code,
            dimension,
            dataRevision,
            evidenceAt,
            CreateKey("administrative", row.행정기관Code));
    }

    private static 외부데이터정규화Record MapJurisdiction(
        대한민국행정동법정동관할Row row,
        외부데이터RawSnapshot raw,
        DateTimeOffset evidenceAt,
        string dataRevision)
    {
        var administrativeId = $"region:kr:hjd:{row.행정기관Code}";
        var legalId = $"region:kr:bjd:{row.법정동Code}";
        var stableId = $"region:kr:hjd-bjd:{row.행정기관Code}-{row.법정동Code}";
        var dimension = string.Join('|',
            $"administrative={administrativeId}",
            $"legal={legalId}",
            $"status={(row.말소일 is null ? "active" : "abolished")}",
            $"valid-from={row.생성일:yyyy-MM-dd}",
            $"valid-to={(row.말소일 is null ? string.Empty : row.말소일.Value.ToString("yyyy-MM-dd"))}",
            "assignment=official-jurisdiction-crosswalk");
        return CreateRecord(
            raw,
            stableId,
            administrativeId,
            대한민국행정동관할CodeDataset.JurisdictionMetricCode,
            row.법정동명.Length > 0 ? row.법정동명 : row.행정기관명,
            "administrative-legal-crosswalk",
            dimension,
            dataRevision,
            evidenceAt,
            CreateKey(
                "jurisdiction",
                row.행정기관Code,
                row.법정동Code,
                row.생성일.ToString("yyyyMMdd"),
                row.말소일?.ToString("yyyyMMdd") ?? string.Empty));
    }

    private static 외부데이터정규화Record CreateRecord(
        외부데이터RawSnapshot raw,
        string stableId,
        string regionId,
        string metric,
        string text,
        string spatialPrecision,
        string dimension,
        string dataRevision,
        DateTimeOffset evidenceAt,
        string recordKey)
        => new()
        {
            RawSnapshotId = raw.Id,
            RecordKey = recordKey,
            StableId = stableId,
            SourceId = raw.SourceId,
            DatasetId = raw.DatasetId,
            RegionStableId = regionId,
            MetricCode = metric,
            TextValue = text,
            UnitCode = "text",
            EvidenceAsOfUtc = evidenceAt,
            CollectedAtUtc = raw.CollectedAtUtc,
            SpatialPrecisionCode = spatialPrecision,
            TemporalPrecisionCode = "effective-date",
            QualityCode = "official-reference",
            LimitationCode = "administrative-boundary-and-building-location-not-included",
            DimensionKey = dimension,
            SourceVersion = raw.SourceVersion,
            DataRevision = dataRevision,
            FirstSeenAtUtc = raw.CollectedAtUtc,
            LastSeenAtUtc = raw.CollectedAtUtc,
        };

    private static string CreateKey(params string[] values)
    {
        var canonical = string.Join('|',
            new[]
            {
                대한민국행정동관할CodeDataset.SourceId,
                대한민국행정동관할CodeDataset.DatasetId,
            }.Concat(values));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }
}
