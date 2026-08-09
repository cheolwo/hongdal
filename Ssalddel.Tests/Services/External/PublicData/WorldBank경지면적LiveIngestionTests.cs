using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using Ssalddel.Infrastructure.Persistence.PublicData;
using Ssalddel.Services.Storage;
using Xunit.Abstractions;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.WorldBank;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class WorldBank경지면적LiveIngestionTests(ITestOutputHelper output)
{
    public const string LiveOptInEnvironmentVariable = "SSALDDEL_RUN_WORLD_BANK_LIVE";

    [Fact]
    [Trait("Category", "ExternalLive")]
    public async Task P6B_명시허용시_실응답을_privateRaw와_normalizedDb에저장한다()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(LiveOptInEnvironmentVariable),
                "1",
                StringComparison.Ordinal))
        {
            output.WriteLine($"Live provider call disabled. Set {LiveOptInEnvironmentVariable}=1 to run.");
            return;
        }

        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            "ssalddel-world-bank-p6b-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryRoot);
        try
        {
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var dbOptions = new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
                .UseSqlite(connection)
                .Options;
            await using var db = new PublicDataIngestionDbContext(dbOptions);
            await db.Database.EnsureCreatedAsync();

            var store = new EfExternalDataIngestionStore(db);
            var source = Assert.Single(new WorldBank경지면적SourceRegistration().GetDefinitions());
            using var httpClient = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-P6B-LiveVerification/1.0");
            var collector = new WorldBank경지면적Collector(
                httpClient,
                Options.Create(new WorldBank경지면적Options
                {
                    CountryCodes = ["KOR"],
                    MostRecentValues = 1,
                    MaxResponseBytes = 64 * 1024,
                }));
            var localStorage = new DevelopmentLocalStorageService(
                new TestHostEnvironment(temporaryRoot),
                new HttpContextAccessor());
            var rawStorage = new ExternalDataRawObjectStorage(localStorage);
            var runtime = new ExternalDataIngestionRuntime(
                new SingleSourceCatalog(source),
                new NoCredentialProvider(),
                new EnabledPolicy(),
                [collector],
                [new WorldBank경지면적Normalizer(store)],
                rawStorage,
                store,
                new SystemExternalDataRetryDelay(),
                TimeProvider.System);

            var result = await runtime.IngestAsync(new ExternalDataIngestionRequest
            {
                SourceId = source.SourceId,
                DatasetId = source.DatasetId,
                RunKey = "world-bank-p6b-" + Guid.NewGuid().ToString("N"),
                Timeout = TimeSpan.FromSeconds(20),
                MaxAttempts = 2,
            });

            Assert.Equal(외부데이터수집StatusCodes.Success, result.StatusCode);
            Assert.Equal(1, result.FetchedCount);
            Assert.Equal(1, result.NormalizedCount);
            Assert.Equal(1, result.InsertedCount);
            Assert.Empty(result.ErrorCode);

            var run = Assert.Single(await db.IngestionRuns.AsNoTracking().ToArrayAsync());
            var raw = Assert.Single(await db.RawSnapshots.AsNoTracking().ToArrayAsync());
            var normalized = Assert.Single(await db.NormalizedRecords.AsNoTracking().ToArrayAsync());
            Assert.Equal(run.Id, raw.FirstCollectionRunId);
            Assert.Equal(raw.Id, normalized.RawSnapshotId);
            Assert.Equal("country:kr", normalized.RegionStableId);
            Assert.Equal(WorldBank경지면적Dataset.MetricCode, normalized.MetricCode);
            Assert.Equal(WorldBank경지면적Dataset.UnitCode, normalized.UnitCode);
            Assert.True(normalized.NumericValue > 0);
            Assert.Equal(raw.EvidenceAsOfUtc, normalized.EvidenceAsOfUtc);
            Assert.StartsWith("local-storage-private://", raw.StorageLocation, StringComparison.Ordinal);

            var physicalRawPath = Path.Combine(
                temporaryRoot,
                DevelopmentLocalStorageService.PrivateStorageDirectoryName,
                raw.StorageObjectName.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(physicalRawPath));
            Assert.Equal(raw.ContentLength, new FileInfo(physicalRawPath).Length);
            Assert.Equal(64, raw.ContentHashSha256.Length);

            output.WriteLine(
                $"P6-B live success: sourceVersion={result.SourceVersion}, "
                + $"dataRevision={result.DataRevision}, evidenceAsOf={normalized.EvidenceAsOfUtc:O}, "
                + $"value={normalized.NumericValue} {normalized.UnitCode}");
        }
        finally
        {
            var resolved = Path.GetFullPath(temporaryRoot);
            var tempPrefix = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)
                             + Path.DirectorySeparatorChar;
            if (resolved.StartsWith(tempPrefix, StringComparison.OrdinalIgnoreCase)
                && Path.GetFileName(resolved).StartsWith("ssalddel-world-bank-p6b-", StringComparison.Ordinal)
                && Directory.Exists(resolved))
            {
                Directory.Delete(resolved, recursive: true);
            }
        }
    }

    private sealed class SingleSourceCatalog(ExternalDataSourceDefinition source)
        : IExternalDataSourceCatalog
    {
        public ExternalDataSourceCatalogResponse GetCatalog() => new() { Items = [source] };

        public ExternalDataSourceDefinition GetRequired(string sourceId, string datasetId)
            => sourceId == source.SourceId && datasetId == source.DatasetId
                ? source
                : throw new KeyNotFoundException("ExternalDataSourceNotRegistered");
    }

    private sealed class NoCredentialProvider : IExternalDataCredentialProvider
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

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Ssalddel.P6B.LiveTest";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
