using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData.WorldBank;

public sealed class WorldBank경지면적Collector : IExternalDataCollector
{
    private readonly HttpClient httpClient;
    private readonly WorldBank경지면적Options options;

    public WorldBank경지면적Collector(
        HttpClient httpClient,
        IOptions<WorldBank경지면적Options> options)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public bool CanCollect(ExternalDataSourceDefinition source)
        => source.SourceId == WorldBank경지면적Dataset.SourceId
           && source.DatasetId == WorldBank경지면적Dataset.DatasetId;

    public async Task<ExternalDataCollectedPayload> CollectAsync(
        ExternalDataSourceDefinition source,
        ExternalDataIngestionRequest request,
        ExternalDataCredential? credential,
        CancellationToken cancellationToken = default)
    {
        if (!CanCollect(source))
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.CollectorMissing);
        if (credential is not null)
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);

        var endpoint = BuildEndpoint();
        using var response = await httpClient.GetAsync(
            endpoint,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw ProviderError(response.StatusCode, response.Headers.RetryAfter);
        if (response.Content.Headers.ContentLength is > 0
            && response.Content.Headers.ContentLength > options.MaxResponseBytes)
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (bytes.Length == 0 || bytes.Length > options.MaxResponseBytes)
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);

        string sourceVersion;
        DateTimeOffset evidenceAsOfUtc;
        int fetchedCount;
        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 2)
                throw new JsonException("WorldBankEnvelopeInvalid");
            var metadata = root[0];
            var lastUpdated = metadata.TryGetProperty("lastupdated", out var lastUpdatedElement)
                ? lastUpdatedElement.GetString()
                : null;
            var sourceId = metadata.TryGetProperty("sourceid", out var sourceElement)
                ? sourceElement.GetString()
                : null;
            var observations = root[1];
            if (observations.ValueKind != JsonValueKind.Array)
                throw new JsonException("WorldBankObservationsInvalid");
            fetchedCount = metadata.TryGetProperty("total", out var totalElement)
                           && totalElement.TryGetInt32(out var total)
                ? total
                : observations.GetArrayLength();
            if (string.IsNullOrWhiteSpace(lastUpdated) || string.IsNullOrWhiteSpace(sourceId))
                throw new JsonException("WorldBankMetadataMissing");
            var latestYear = observations.EnumerateArray()
                .Where(observation => observation.ValueKind == JsonValueKind.Object
                                      && observation.TryGetProperty("value", out var value)
                                      && value.ValueKind != JsonValueKind.Null
                                      && observation.TryGetProperty("date", out _))
                .Select(observation => observation.GetProperty("date").GetString())
                .Select(value => int.TryParse(value, out var year) ? year : 0)
                .DefaultIfEmpty()
                .Max();
            if (latestYear is < 1900 or > 2200)
                throw new JsonException("WorldBankEvidenceYearMissing");
            sourceVersion = $"wdi:{sourceId}:lastupdated:{lastUpdated}";
            evidenceAsOfUtc = new DateTimeOffset(latestYear, 12, 31, 0, 0, 0, TimeSpan.Zero);
        }
        catch (JsonException error)
        {
            throw new ExternalDataCollectionException(
                ExternalDataCollectionErrorCode.InvalidPayload,
                innerException: error);
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
        return new ExternalDataCollectedPayload(
            new MemoryStream(bytes, writable: false),
            $"{WorldBank경지면적Dataset.DatasetId}.json",
            contentType,
            evidenceAsOfUtc,
            sourceVersion,
            fetchedCount);
    }

    private Uri BuildEndpoint()
    {
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("WorldBankBaseUrlMustUseHttps");
        if (options.MaxResponseBytes is < 1024 or > 50 * 1024 * 1024)
            throw new InvalidOperationException("WorldBankMaxResponseBytesInvalid");
        if (options.MostRecentValues is < 1 or > 20)
            throw new InvalidOperationException("WorldBankMostRecentValuesInvalid");

        var countryCodes = (options.CountryCodes ?? [])
            .Select(code => code?.Trim().ToUpperInvariant() ?? string.Empty)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (countryCodes.Length == 0
            || countryCodes.Any(code => code.Length != 3 || code.Any(character => character is < 'A' or > 'Z')))
            throw new InvalidOperationException("WorldBankCountryCodeInvalid");

        var normalizedBaseUri = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/");
        var countryPath = string.Join(';', countryCodes);
        var relative = $"country/{countryPath}/indicator/{WorldBank경지면적Dataset.IndicatorCode}?format=json&mrv={options.MostRecentValues}&per_page=100";
        return new Uri(normalizedBaseUri, relative);
    }

    private static ExternalDataCollectionException ProviderError(
        HttpStatusCode statusCode,
        RetryConditionHeaderValue? retryAfter)
    {
        var delay = retryAfter?.Delta;
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new(ExternalDataCollectionErrorCode.Unauthorized),
            HttpStatusCode.Forbidden => new(ExternalDataCollectionErrorCode.Forbidden),
            HttpStatusCode.NotFound => new(ExternalDataCollectionErrorCode.NotFound),
            HttpStatusCode.TooManyRequests => new(
                ExternalDataCollectionErrorCode.RateLimited,
                retryable: true,
                retryAfter: delay),
            >= HttpStatusCode.InternalServerError => new(
                ExternalDataCollectionErrorCode.Unexpected,
                retryable: true,
                retryAfter: delay),
            _ => new(ExternalDataCollectionErrorCode.InvalidPayload),
        };
    }
}
