using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed class 대한민국법정동CodeNormalizer : IExternalDataNormalizer
{
    private readonly 대한민국법정동CodeOptions options;

    public 대한민국법정동CodeNormalizer(
        Microsoft.Extensions.Options.IOptions<대한민국법정동CodeOptions> options)
        => this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public bool CanNormalize(ExternalDataSourceDefinition source)
        => source.SourceId == 대한민국법정동CodeDataset.SourceId
           && source.DatasetId == 대한민국법정동CodeDataset.DatasetId;

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
            var rows = 대한민국법정동CodeArchiveReader.Read(
                bytes,
                options.MaxExpandedBytes,
                options.MaxRecordCount);
            var evidenceAt = rawSnapshot.EvidenceAsOfUtc ?? rawSnapshot.CollectedAtUtc;
            var dataRevision = $"mois-bjd-{rawSnapshot.ContentHashSha256[..20]}";
            var records = rows.Select(row => Map(row, rawSnapshot, evidenceAt, dataRevision)).ToArray();
            return new ExternalDataNormalizationBatch(records, 0, dataRevision);
        }
        catch (Exception error) when (error is InvalidDataException or ArgumentOutOfRangeException)
        {
            throw new ExternalDataCollectionException(
                ExternalDataCollectionErrorCode.InvalidPayload,
                innerException: error);
        }
    }

    private static 외부데이터정규화Record Map(
        대한민국법정동CodeRow row,
        외부데이터RawSnapshot raw,
        DateTimeOffset evidenceAt,
        string dataRevision)
    {
        var regionStableId = $"region:kr:bjd:{row.법정동Code}";
        var parentStableId = row.상위법정동Code is null
            ? string.Empty
            : $"region:kr:bjd:{row.상위법정동Code}";
        var dimensionKey = string.Join('|',
            $"code={row.법정동Code}",
            $"status={row.상태Code}",
            $"level={row.행정계층Code}",
            $"parent={parentStableId}");
        return new 외부데이터정규화Record
        {
            RawSnapshotId = raw.Id,
            RecordKey = CreateStableRecordKey(row.법정동Code),
            StableId = regionStableId,
            SourceId = raw.SourceId,
            DatasetId = raw.DatasetId,
            RegionStableId = regionStableId,
            MetricCode = 대한민국법정동CodeDataset.MetricCode,
            TextValue = row.전체명,
            UnitCode = "text",
            EvidenceAsOfUtc = evidenceAt,
            CollectedAtUtc = raw.CollectedAtUtc,
            SpatialPrecisionCode = row.행정계층Code,
            TemporalPrecisionCode = "collection-time",
            QualityCode = "official-reference",
            LimitationCode = "source-effective-date-and-boundary-not-included",
            DimensionKey = dimensionKey,
            SourceVersion = raw.SourceVersion,
            DataRevision = dataRevision,
            FirstSeenAtUtc = raw.CollectedAtUtc,
            LastSeenAtUtc = raw.CollectedAtUtc,
        };
    }

    private static string CreateStableRecordKey(string code)
    {
        var canonical = string.Join('|',
            대한민국법정동CodeDataset.SourceId,
            대한민국법정동CodeDataset.DatasetId,
            대한민국법정동CodeDataset.MetricCode,
            code);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }
}
