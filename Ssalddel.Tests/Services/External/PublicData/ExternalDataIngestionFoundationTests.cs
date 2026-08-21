using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using Ssalddel.Infrastructure.Persistence.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.Agriculture;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class ExternalDataIngestionFoundationTests
{
    [Fact]
    public void SourceCatalog_ExtendsExistingApiCatalog_AndKeepsCollectionDisabled()
    {
        var apiCatalog = new StubApiCatalog(new PublicDataApiMetadataItem
        {
            Key = "soil-api",
            Provider = "official-provider",
            DisplayName = "soil",
            Domain = "Agriculture",
            ApiType = "REST",
            DataFormat = "JSON",
            RequiresServiceKey = true,
            ConfigurationPaths = ["PublicData:Soil:ApiKey"],
        });
        var fileRegistration = new StubRegistration(Source(
            sourceId: "land-file",
            datasetId: "land-2026",
            accessMethod: ExternalDataAccessMethod.DownloadFile));

        var catalog = new ExternalDataSourceCatalog(apiCatalog, [fileRegistration]);

        var api = catalog.GetRequired("soil-api", "soil-api");
        Assert.Equal(ExternalDataCredentialType.ApiKeyQuery, api.CredentialType);
        Assert.False(api.DefaultCollectionEnabled);
        Assert.Equal("PublicData:Soil:ApiKey", Assert.Single(api.CredentialReferences));
        Assert.Equal(ExternalDataAccessMethod.DownloadFile,
            catalog.GetRequired("land-file", "land-2026").AccessMethod);
    }

    [Fact]
    public void SourceCatalog_CanLoadCurrentReferenceOnlyMetadata()
    {
        var catalog = new ExternalDataSourceCatalog(
            new PublicDataApiMetadataCatalog(),
            []);

        Assert.NotEmpty(catalog.GetCatalog().Items);
        Assert.All(catalog.GetCatalog().Items, item => Assert.False(item.DefaultCollectionEnabled));
    }

    [Fact]
    public void SourceCatalog_FarmRealityThreeProvidersKeepStableDatasetBoundaries()
    {
        var catalog = new ExternalDataSourceCatalog(
            new StubApiCatalog(), [new FarmRealityDataSourceRegistration()]);

        Assert.Equal("CropWorkReference", catalog.GetRequired(
            FarmRealityDataSourceIds.Nongsaro,
            FarmRealityDataSourceIds.NongsaroWorkSchedule).DataDomain);
        Assert.Equal("MarketPriceObservation", catalog.GetRequired(
            FarmRealityDataSourceIds.Kamis,
            FarmRealityDataSourceIds.KamisPriceObservations).DataDomain);
        var ams = catalog.GetRequired(FarmRealityDataSourceIds.UsdaAms,
            FarmRealityDataSourceIds.UsdaAmsPriceObservations);
        Assert.Contains("직접 비교하지 않습니다", ams.UsageLimitations,
            StringComparison.Ordinal);
        Assert.All(catalog.GetCatalog().Items,
            item => Assert.False(item.DefaultCollectionEnabled));
    }

    [Fact]
    public async Task CredentialProvider_ReadsServerConfiguration_WithoutExposingSecret()
    {
        var source = Source(requiresCredential: true);
        var catalog = new StubSourceCatalog(source);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExternalData:Test:Secret"] = "server-secret",
            })
            .Build();
        var provider = new ConfigurationExternalDataCredentialProvider(configuration, catalog);

        var credential = await provider.GetAsync(source.SourceId, source.DatasetId);

        Assert.NotNull(credential);
        Assert.Equal("server-secret", credential.SecretValue);
        Assert.DoesNotContain("server-secret", credential.ToString(), StringComparison.Ordinal);
        Assert.Contains("REDACTED", credential.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void CollectionPolicy_IsOptInByDefault()
    {
        var source = Source();
        var disabled = new ConfigurationExternalDataCollectionPolicy(
            new ConfigurationBuilder().Build());
        var enabled = new ConfigurationExternalDataCollectionPolicy(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"ExternalData:Sources:{source.SourceId}:Enabled"] = "true",
                })
                .Build());

        Assert.False(disabled.IsEnabled(source));
        Assert.True(enabled.IsEnabled(source));
    }

    [Fact]
    public async Task Runtime_PreservesLineage_AndReturnsPartialCounts()
    {
        var source = Source();
        var store = new MemoryStore();
        var runtime = Runtime(
            source,
            store,
            new SuccessfulCollector(),
            new SuccessfulNormalizer(rejectedCount: 1));

        var result = await runtime.IngestAsync(Request("run-success"));

        Assert.Equal(외부데이터수집StatusCodes.Partial, result.StatusCode);
        Assert.Equal(1, result.NormalizedCount);
        Assert.Equal(1, result.RejectedCount);
        Assert.Equal("data-r1", result.DataRevision);
        var record = Assert.Single(store.Records);
        Assert.Equal(source.SourceId, record.SourceId);
        Assert.Equal(source.DatasetId, record.DatasetId);
        Assert.Equal("region:kr:seoul", record.RegionStableId);
        Assert.Equal("ph", record.UnitCode);
        Assert.Equal("source-v1", record.SourceVersion);
        Assert.Equal(store.RawSnapshots.Single().Id, record.RawSnapshotId);
    }

    [Fact]
    public async Task Runtime_MissingCredentialFailsWithoutCallingProviderCollector()
    {
        var source = Source(requiresCredential: true);
        var collector = new SuccessfulCollector();
        var store = new MemoryStore();
        var runtime = Runtime(
            source,
            store,
            collector,
            new SuccessfulNormalizer(),
            credentialProvider: new NullCredentialProvider());

        var result = await runtime.IngestAsync(Request("run-no-secret"));

        Assert.Equal(외부데이터수집StatusCodes.Failed, result.StatusCode);
        Assert.Equal(ExternalDataCollectionErrorCode.MissingCredential.ToString(), result.ErrorCode);
        Assert.Equal(0, collector.CallCount);
        Assert.Empty(store.RawSnapshots);
        Assert.Empty(store.Records);
    }

    [Fact]
    public async Task Runtime_RetriesOnlyRetryableFailure_WithinRequestedBound()
    {
        var collector = new RetryOnceCollector();
        var delays = new RecordingRetryDelay();
        var runtime = Runtime(
            Source(),
            new MemoryStore(),
            collector,
            new SuccessfulNormalizer(),
            retryDelay: delays);

        var result = await runtime.IngestAsync(Request("run-retry") with { MaxAttempts = 2 });

        Assert.Equal(외부데이터수집StatusCodes.Success, result.StatusCode);
        Assert.Equal(2, collector.CallCount);
        Assert.Single(delays.Delays);
    }

    [Fact]
    public async Task Runtime_SameRawHashDoesNotNormalizeAgain()
    {
        var source = Source();
        var store = new MemoryStore();
        var normalizer = new SuccessfulNormalizer();
        var runtime = Runtime(source, store, new SuccessfulCollector(), normalizer);

        var first = await runtime.IngestAsync(Request("run-first"));
        var second = await runtime.IngestAsync(Request("run-second"));

        Assert.Equal(외부데이터수집StatusCodes.Success, first.StatusCode);
        Assert.Equal(외부데이터수집StatusCodes.Success, second.StatusCode);
        Assert.Equal(1, normalizer.CallCount);
        Assert.Single(store.RawSnapshots);
        Assert.Single(store.Records);
        Assert.Equal(1, second.ExistingCount);
    }

    [Fact]
    public void NormalizationValidator_RejectsDuplicateOrInvalidSpatialRecord()
    {
        var source = Source();
        var raw = RawSnapshot();
        var valid = NormalizedRecord(raw.Id);

        Assert.Throws<ExternalDataCollectionException>(() =>
            ExternalDataNormalizationValidator.Validate(
                source,
                raw,
                new ExternalDataNormalizationBatch([valid, valid], 0, "r1")));

        var invalid = NormalizedRecord(raw.Id);
        invalid.RegionStableId = "provider-region-code";
        Assert.Throws<ExternalDataCollectionException>(() =>
            ExternalDataNormalizationValidator.Validate(
                source,
                raw,
                new ExternalDataNormalizationBatch([invalid], 0, "r1")));
    }

    [Fact]
    public async Task EfStore_UpsertIsIdempotentByRecordKey()
    {
        var options = new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        await using var db = new PublicDataIngestionDbContext(options);
        var store = new EfExternalDataIngestionStore(db);
        var record = NormalizedRecord(rawId: 1);

        var inserted = await store.UpsertNormalizedAsync([record]);
        var existing = await store.UpsertNormalizedAsync([NormalizedRecord(rawId: 1)]);

        Assert.Equal(1, inserted.InsertedCount);
        Assert.Equal(1, existing.ExistingCount);
        Assert.Single(db.NormalizedRecords);
    }

    [Theory]
    [InlineData("country:kr", true)]
    [InlineData("region:kr:seoul:jungnang", true)]
    [InlineData("grid:kr:5179:1269", true)]
    [InlineData("provider-code", false)]
    [InlineData("", false)]
    public void RegionStableId_UsesCanonicalSpatialScope(string value, bool expected)
        => Assert.Equal(expected, RegionStableIdRules.IsValid(value));

    private static ExternalDataIngestionRuntime Runtime(
        ExternalDataSourceDefinition source,
        MemoryStore store,
        IExternalDataCollector collector,
        IExternalDataNormalizer normalizer,
        IExternalDataCredentialProvider? credentialProvider = null,
        IExternalDataRetryDelay? retryDelay = null)
        => new(
            new StubSourceCatalog(source),
            credentialProvider ?? new NullCredentialProvider(),
            new EnabledPolicy(),
            [collector],
            [normalizer],
            new MemoryRawStorage(),
            store,
            retryDelay ?? new RecordingRetryDelay(),
            TimeProvider.System);

    private static ExternalDataIngestionRequest Request(string runKey) => new()
    {
        SourceId = "test-source",
        DatasetId = "test-dataset",
        RunKey = runKey,
        Timeout = TimeSpan.FromSeconds(1),
    };

    private static ExternalDataSourceDefinition Source(
        string sourceId = "test-source",
        string datasetId = "test-dataset",
        bool requiresCredential = false,
        ExternalDataAccessMethod accessMethod = ExternalDataAccessMethod.HttpApi)
        => new()
        {
            SourceId = sourceId,
            DatasetId = datasetId,
            Name = "test data",
            Provider = "official test provider",
            AccessMethod = accessMethod,
            CredentialType = requiresCredential
                ? ExternalDataCredentialType.ApiKeyHeader
                : ExternalDataCredentialType.None,
            RequiresCredential = requiresCredential,
            DefaultCollectionEnabled = false,
            CredentialReferences = requiresCredential ? ["ExternalData:Test:Secret"] : [],
        };

    private static 외부데이터RawSnapshot RawSnapshot() => new()
    {
        Id = 10,
        SourceId = "test-source",
        DatasetId = "test-dataset",
        SourceVersion = "source-v1",
        ContentHashSha256 = "hash-1",
        CollectedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
        EvidenceAsOfUtc = DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
    };

    private static 외부데이터정규화Record NormalizedRecord(long rawId)
    {
        var asOf = DateTimeOffset.Parse("2026-08-01T00:00:00Z");
        return new 외부데이터정규화Record
        {
            RawSnapshotId = rawId,
            RecordKey = 외부데이터RecordKey.Create(
                "test-source", "test-dataset", "region:kr:seoul", "soil-ph", asOf, "depth:0-5cm"),
            StableId = "soil:region:kr:seoul:depth-0-5cm",
            SourceId = "test-source",
            DatasetId = "test-dataset",
            RegionStableId = "region:kr:seoul",
            MetricCode = "soil-ph",
            NumericValue = 5.4m,
            UnitCode = "ph",
            EvidenceAsOfUtc = asOf,
            CollectedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            SpatialPrecisionCode = "region",
            TemporalPrecisionCode = "annual",
            QualityCode = "source-reported",
            LimitationCode = "test-only",
            DimensionKey = "depth:0-5cm",
            SourceVersion = "source-v1",
            DataRevision = "data-r1",
            FirstSeenAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            LastSeenAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
        };
    }

    private sealed class StubApiCatalog(params PublicDataApiMetadataItem[] items)
        : IPublicDataApiMetadataCatalog
    {
        public PublicDataApiMetadataResponse GetCatalog(PublicDataApiMetadataQuery query)
            => new() { Items = items };
    }

    private sealed class StubRegistration(params ExternalDataSourceDefinition[] definitions)
        : IExternalDataSourceRegistration
    {
        public IReadOnlyCollection<ExternalDataSourceDefinition> GetDefinitions() => definitions;
    }

    private sealed class StubSourceCatalog(params ExternalDataSourceDefinition[] definitions)
        : IExternalDataSourceCatalog
    {
        public ExternalDataSourceCatalogResponse GetCatalog() => new() { Items = definitions };

        public ExternalDataSourceDefinition GetRequired(string sourceId, string datasetId)
            => definitions.Single(item => item.SourceId == sourceId && item.DatasetId == datasetId);
    }

    private sealed class NullCredentialProvider : IExternalDataCredentialProvider
    {
        public ValueTask<ExternalDataCredential?> GetAsync(
            string sourceId,
            string datasetId,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult<ExternalDataCredential?>(null);
    }

    private sealed class EnabledPolicy : IExternalDataCollectionPolicy
    {
        public bool IsEnabled(ExternalDataSourceDefinition source) => true;
    }

    private class SuccessfulCollector : IExternalDataCollector
    {
        public int CallCount { get; protected set; }
        public bool CanCollect(ExternalDataSourceDefinition source) => true;

        public virtual Task<ExternalDataCollectedPayload> CollectAsync(
            ExternalDataSourceDefinition source,
            ExternalDataIngestionRequest request,
            ExternalDataCredential? credential,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new ExternalDataCollectedPayload(
                new MemoryStream(Encoding.UTF8.GetBytes("raw-payload")),
                "soil.json",
                "application/json",
                DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
                "source-v1",
                1));
        }
    }

    private sealed class RetryOnceCollector : SuccessfulCollector
    {
        public override Task<ExternalDataCollectedPayload> CollectAsync(
            ExternalDataSourceDefinition source,
            ExternalDataIngestionRequest request,
            ExternalDataCredential? credential,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            if (CallCount == 1)
                throw new ExternalDataCollectionException(
                    ExternalDataCollectionErrorCode.RateLimited,
                    retryable: true,
                    retryAfter: TimeSpan.Zero);
            return Task.FromResult(new ExternalDataCollectedPayload(
                new MemoryStream(Encoding.UTF8.GetBytes("raw-payload")),
                "soil.json",
                "application/json",
                DateTimeOffset.Parse("2026-08-01T00:00:00Z"),
                "source-v1",
                1));
        }
    }

    private sealed class SuccessfulNormalizer(int rejectedCount = 0) : IExternalDataNormalizer
    {
        public int CallCount { get; private set; }
        public bool CanNormalize(ExternalDataSourceDefinition source) => true;

        public Task<ExternalDataNormalizationBatch> NormalizeAsync(
            ExternalDataSourceDefinition source,
            외부데이터RawSnapshot rawSnapshot,
            IExternalDataRawStorage rawStorage,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new ExternalDataNormalizationBatch(
                [NormalizedRecord(rawSnapshot.Id)],
                rejectedCount,
                "data-r1"));
        }
    }

    private sealed class MemoryRawStorage : IExternalDataRawStorage
    {
        public Task<ExternalDataRawStorageResult> StoreAsync(
            ExternalDataSourceDefinition source,
            ExternalDataCollectedPayload payload,
            DateTimeOffset collectedAtUtc,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new ExternalDataRawStorageResult(
                "fixed-hash", 11, "private", "raw/test", "private://raw/test"));

        public Task<Stream> OpenReadAsync(
            외부데이터RawSnapshot snapshot,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("raw-payload")));
    }

    private sealed class RecordingRetryDelay : IExternalDataRetryDelay
    {
        public List<TimeSpan> Delays { get; } = [];
        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            Delays.Add(delay);
            return Task.CompletedTask;
        }
    }

    private sealed class MemoryStore : I외부데이터수집Store
    {
        private long nextRunId = 1;
        private long nextRawId = 10;
        public List<외부데이터수집Run> Runs { get; } = [];
        public List<외부데이터RawSnapshot> RawSnapshots { get; } = [];
        public List<외부데이터정규화Record> Records { get; } = [];

        public Task<외부데이터수집Run> StartRunAsync(
            외부데이터수집Run run,
            CancellationToken cancellationToken = default)
        {
            run.Id = nextRunId++;
            Runs.Add(run);
            return Task.FromResult(run);
        }

        public Task<외부데이터RawSnapshot?> FindRawSnapshotAsync(
            string sourceId,
            string datasetId,
            string contentHashSha256,
            CancellationToken cancellationToken = default)
            => Task.FromResult(RawSnapshots.SingleOrDefault(item =>
                item.SourceId == sourceId
                && item.DatasetId == datasetId
                && item.ContentHashSha256 == contentHashSha256));

        public Task<외부데이터RawSnapshot> SaveRawSnapshotAsync(
            외부데이터RawSnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            snapshot.Id = nextRawId++;
            RawSnapshots.Add(snapshot);
            return Task.FromResult(snapshot);
        }

        public Task<외부데이터정규화저장Result> UpsertNormalizedAsync(
            IReadOnlyCollection<외부데이터정규화Record> records,
            CancellationToken cancellationToken = default)
        {
            Records.AddRange(records);
            return Task.FromResult(new 외부데이터정규화저장Result(records.Count, 0, 0));
        }

        public Task CompleteRunAsync(
            외부데이터수집Run run,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
