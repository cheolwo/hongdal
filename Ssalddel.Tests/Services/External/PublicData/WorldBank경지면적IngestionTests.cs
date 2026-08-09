using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.WorldBank;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class WorldBank경지면적IngestionTests
{
    [Fact]
    public void SourceRegistration_RecordsOfficialAccessAndUsageBoundary()
    {
        var source = Assert.Single(new WorldBank경지면적SourceRegistration().GetDefinitions());

        Assert.Equal("world-bank-indicators", source.SourceId);
        Assert.Equal("wdi-ag-lnd-arbl-ha", source.DatasetId);
        Assert.Equal(ExternalDataCredentialType.None, source.CredentialType);
        Assert.False(source.RequiresCredential);
        Assert.False(source.DefaultCollectionEnabled);
        Assert.Equal("CC BY 4.0", source.License);
        Assert.Equal("Country", source.SpatialResolution);
        Assert.Equal("Annual", source.TemporalResolution);
        Assert.Contains("FAO", source.AttributionRequirement, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Collector_UsesHttpsCountryQuery_AndPreservesProviderVersion()
    {
        HttpRequestMessage? captured = null;
        var client = new HttpClient(new StubHttpMessageHandler(request =>
        {
            captured = request;
            return JsonResponse(SampleJson);
        }));
        var collector = new WorldBank경지면적Collector(
            client,
            Options.Create(new WorldBank경지면적Options
            {
                BaseUrl = "https://api.worldbank.org/v2",
                CountryCodes = ["KOR"],
            }));

        await using var payload = await collector.CollectAsync(
            Source(),
            Request(),
            credential: null);

        Assert.NotNull(captured);
        Assert.Equal(Uri.UriSchemeHttps, captured.RequestUri!.Scheme);
        Assert.Equal(
            "/v2/country/KOR/indicator/AG.LND.ARBL.HA",
            captured.RequestUri.AbsolutePath);
        Assert.Contains("format=json", captured.RequestUri.Query, StringComparison.Ordinal);
        Assert.Contains("mrv=1", captured.RequestUri.Query, StringComparison.Ordinal);
        Assert.Equal("wdi:2:lastupdated:2026-07-13", payload.SourceVersion);
        Assert.Equal(2, payload.FetchedCount);
        Assert.Equal("application/json", payload.ContentType);
        Assert.Equal(
            new DateTimeOffset(2023, 12, 31, 0, 0, 0, TimeSpan.Zero),
            payload.EvidenceAsOfUtc);
    }

    [Fact]
    public async Task Collector_MapsRateLimitWithoutLeakingProviderBody()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7));
        response.Content = new StringContent("secret provider diagnostic");
        var collector = new WorldBank경지면적Collector(
            new HttpClient(new StubHttpMessageHandler(_ => response)),
            Options.Create(new WorldBank경지면적Options()));

        var error = await Assert.ThrowsAsync<ExternalDataCollectionException>(() =>
            collector.CollectAsync(Source(), Request(), credential: null));

        Assert.Equal(ExternalDataCollectionErrorCode.RateLimited, error.ErrorCode);
        Assert.True(error.Retryable);
        Assert.Equal(TimeSpan.FromSeconds(7), error.RetryAfter);
        Assert.DoesNotContain("secret", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Collector_RejectsUnboundedMostRecentValueRequestBeforeProviderCall()
    {
        var callCount = 0;
        var collector = new WorldBank경지면적Collector(
            new HttpClient(new StubHttpMessageHandler(_ =>
            {
                callCount++;
                return JsonResponse(SampleJson);
            })),
            Options.Create(new WorldBank경지면적Options { MostRecentValues = 0 }));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            collector.CollectAsync(Source(), Request(), credential: null));

        Assert.Equal("WorldBankMostRecentValuesInvalid", error.Message);
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task Normalizer_ProducesCountryAnnualFact_AndRejectsNullObservation()
    {
        var raw = new 외부데이터RawSnapshot
        {
            Id = 17,
            SourceId = WorldBank경지면적Dataset.SourceId,
            DatasetId = WorldBank경지면적Dataset.DatasetId,
            SourceVersion = "wdi:2:lastupdated:2026-07-13",
            ContentHashSha256 = new string('a', 64),
            CollectedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
        };
        var normalizer = new WorldBank경지면적Normalizer(new StubRegionMappingStore());

        var result = await normalizer.NormalizeAsync(
            Source(),
            raw,
            new InMemoryRawStorage(SampleJson));

        var record = Assert.Single(result.Records);
        Assert.Equal(1, result.RejectedCount);
        Assert.Equal("country:kr", record.RegionStableId);
        Assert.Equal("agricultural-land.arable-area", record.MetricCode);
        Assert.Equal(1571000m, record.NumericValue);
        Assert.Equal("ha", record.UnitCode);
        Assert.Equal("country", record.SpatialPrecisionCode);
        Assert.Equal("annual", record.TemporalPrecisionCode);
        Assert.Equal(new DateTimeOffset(2023, 12, 31, 0, 0, 0, TimeSpan.Zero), record.EvidenceAsOfUtc);
        Assert.Equal(raw.Id, record.RawSnapshotId);
        Assert.Equal(raw.SourceVersion, record.SourceVersion);
        Assert.StartsWith("world-bank-wdi-", record.DataRevision, StringComparison.Ordinal);
        ExternalDataNormalizationValidator.Validate(Source(), raw, result);
    }

    [Fact]
    public async Task Normalizer_RejectsCountryOutsideRegisteredRegionMapping()
    {
        var unsupported = SampleJson.Replace("\"KOR\"", "\"FRA\"", StringComparison.Ordinal);
        var raw = new 외부데이터RawSnapshot
        {
            Id = 18,
            SourceId = WorldBank경지면적Dataset.SourceId,
            DatasetId = WorldBank경지면적Dataset.DatasetId,
            SourceVersion = "wdi:2:lastupdated:2026-07-13",
            ContentHashSha256 = new string('b', 64),
            CollectedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
        };

        var error = await Assert.ThrowsAsync<ExternalDataCollectionException>(() =>
            new WorldBank경지면적Normalizer(new StubRegionMappingStore()).NormalizeAsync(
                Source(), raw, new InMemoryRawStorage(unsupported)));

        Assert.Equal(ExternalDataCollectionErrorCode.InvalidPayload, error.ErrorCode);
    }

    private static ExternalDataSourceDefinition Source()
        => Assert.Single(new WorldBank경지면적SourceRegistration().GetDefinitions());

    private static ExternalDataIngestionRequest Request() => new()
    {
        SourceId = WorldBank경지면적Dataset.SourceId,
        DatasetId = WorldBank경지면적Dataset.DatasetId,
        RunKey = "world-bank-test",
    };

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private const string SampleJson = """
        [
          {"page":1,"pages":1,"per_page":1000,"total":2,"sourceid":"2","lastupdated":"2026-07-13"},
          [
            {
              "indicator":{"id":"AG.LND.ARBL.HA","value":"Arable land (hectares)"},
              "country":{"id":"KR","value":"Korea, Rep."},
              "countryiso3code":"KOR",
              "date":"2023",
              "value":1571000,
              "unit":"",
              "obs_status":"",
              "decimal":0
            },
            {
              "indicator":{"id":"AG.LND.ARBL.HA","value":"Arable land (hectares)"},
              "country":{"id":"KR","value":"Korea, Rep."},
              "countryiso3code":"KOR",
              "date":"2022",
              "value":null,
              "unit":"",
              "obs_status":"",
              "decimal":0
            }
          ]
        ]
        """;

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }

    private sealed class InMemoryRawStorage(string json) : IExternalDataRawStorage
    {
        public Task<ExternalDataRawStorageResult> StoreAsync(
            ExternalDataSourceDefinition source,
            ExternalDataCollectedPayload payload,
            DateTimeOffset collectedAtUtc,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Stream> OpenReadAsync(
            외부데이터RawSnapshot snapshot,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(json)));
    }

    private sealed class StubRegionMappingStore : I외부지역MappingStore
    {
        private static readonly IReadOnlyDictionary<string, string> Regions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["KOR"] = "country:kr",
                ["USA"] = "country:us",
                ["CHN"] = "country:cn",
            };

        public Task<외부지역CodeMapping?> FindAsync(
            string sourceId,
            string externalRegionCode,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                sourceId == WorldBank경지면적Dataset.SourceId
                && Regions.TryGetValue(externalRegionCode, out var regionId)
                    ? new 외부지역CodeMapping
                    {
                        SourceId = sourceId,
                        ExternalRegionCode = externalRegionCode,
                        RegionStableId = regionId,
                        SpatialPrecisionCode = "country",
                        MappingRevision = "test",
                    }
                    : null);
    }
}
