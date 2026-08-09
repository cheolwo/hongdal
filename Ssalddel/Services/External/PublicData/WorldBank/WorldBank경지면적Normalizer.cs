using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;

namespace 살뜰.Services.External.PublicData.WorldBank;

public sealed class WorldBank경지면적Normalizer : IExternalDataNormalizer
{
    private readonly I외부지역MappingStore regionMappingStore;

    public WorldBank경지면적Normalizer(I외부지역MappingStore regionMappingStore)
        => this.regionMappingStore = regionMappingStore
            ?? throw new ArgumentNullException(nameof(regionMappingStore));

    public bool CanNormalize(ExternalDataSourceDefinition source)
        => source.SourceId == WorldBank경지면적Dataset.SourceId
           && source.DatasetId == WorldBank경지면적Dataset.DatasetId;

    public async Task<ExternalDataNormalizationBatch> NormalizeAsync(
        ExternalDataSourceDefinition source,
        외부데이터RawSnapshot rawSnapshot,
        IExternalDataRawStorage rawStorage,
        CancellationToken cancellationToken = default)
    {
        if (!CanNormalize(source))
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.NormalizerMissing);

        await using var stream = await rawStorage.OpenReadAsync(rawSnapshot, cancellationToken);
        JsonDocument document;
        try
        {
            document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException error)
        {
            throw new ExternalDataCollectionException(
                ExternalDataCollectionErrorCode.InvalidPayload,
                innerException: error);
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array
                || root.GetArrayLength() < 2
                || root[1].ValueKind != JsonValueKind.Array)
                throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);

            var records = new List<외부데이터정규화Record>();
            var rejected = 0;
            var dataRevision = CreateDataRevision(rawSnapshot);
            var resolvedRegions = new Dictionary<string, 외부지역CodeMapping>(StringComparer.OrdinalIgnoreCase);
            foreach (var observation in root[1].EnumerateArray())
            {
                if (!TryReadObservation(observation, out var countryCode, out var year, out var value))
                {
                    rejected++;
                    continue;
                }
                if (!resolvedRegions.TryGetValue(countryCode, out var mapping))
                {
                    mapping = await regionMappingStore.FindAsync(
                                  source.SourceId,
                                  countryCode,
                                  cancellationToken)
                              ?? throw new ExternalDataCollectionException(
                                  ExternalDataCollectionErrorCode.InvalidPayload);
                    if (!RegionStableIdRules.IsValid(mapping.RegionStableId)
                        || !string.Equals(mapping.SpatialPrecisionCode, "country", StringComparison.Ordinal))
                        throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);
                    resolvedRegions.Add(countryCode, mapping);
                }

                var regionStableId = mapping.RegionStableId;

                var evidenceAsOf = new DateTimeOffset(year, 12, 31, 0, 0, 0, TimeSpan.Zero);
                const string dimensionKey = "indicator:AG.LND.ARBL.HA;period:annual";
                records.Add(new 외부데이터정규화Record
                {
                    RawSnapshotId = rawSnapshot.Id,
                    RecordKey = 외부데이터RecordKey.Create(
                        source.SourceId,
                        source.DatasetId,
                        regionStableId,
                        WorldBank경지면적Dataset.MetricCode,
                        evidenceAsOf,
                        dimensionKey),
                    StableId = $"agricultural-land:{regionStableId}:arable-area:{year}",
                    SourceId = source.SourceId,
                    DatasetId = source.DatasetId,
                    RegionStableId = regionStableId,
                    MetricCode = WorldBank경지면적Dataset.MetricCode,
                    NumericValue = value,
                    UnitCode = WorldBank경지면적Dataset.UnitCode,
                    EvidenceAsOfUtc = evidenceAsOf,
                    CollectedAtUtc = rawSnapshot.CollectedAtUtc,
                    SpatialPrecisionCode = "country",
                    TemporalPrecisionCode = "annual",
                    QualityCode = "provider-reported",
                    LimitationCode = "national-aggregate-fao-derived",
                    DimensionKey = dimensionKey,
                    SourceVersion = rawSnapshot.SourceVersion,
                    DataRevision = dataRevision,
                    FirstSeenAtUtc = rawSnapshot.CollectedAtUtc,
                    LastSeenAtUtc = rawSnapshot.CollectedAtUtc,
                });
            }

            if (records.Count == 0)
                throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);
            return new ExternalDataNormalizationBatch(records, rejected, dataRevision);
        }
    }

    private static bool TryReadObservation(
        JsonElement observation,
        out string countryCode,
        out int year,
        out decimal value)
    {
        countryCode = string.Empty;
        year = 0;
        value = 0;
        if (observation.ValueKind != JsonValueKind.Object
            || !observation.TryGetProperty("indicator", out var indicator)
            || !indicator.TryGetProperty("id", out var indicatorId)
            || indicatorId.GetString() != WorldBank경지면적Dataset.IndicatorCode
            || !observation.TryGetProperty("countryiso3code", out var country)
            || string.IsNullOrWhiteSpace(country.GetString())
            || !observation.TryGetProperty("date", out var date)
            || !int.TryParse(date.GetString(), out year)
            || year is < 1900 or > 2200
            || !observation.TryGetProperty("value", out var valueElement)
            || valueElement.ValueKind == JsonValueKind.Null
            || !valueElement.TryGetDecimal(out value))
            return false;

        countryCode = country.GetString()!.Trim().ToUpperInvariant();
        return value >= 0;
    }

    private static string CreateDataRevision(외부데이터RawSnapshot rawSnapshot)
    {
        var input = $"{rawSnapshot.SourceVersion}|{rawSnapshot.ContentHashSha256}";
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)))
            .ToLowerInvariant();
        return $"world-bank-wdi-{digest[..20]}";
    }
}
