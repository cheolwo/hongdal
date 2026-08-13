using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed class 대한민국행정동관할CodeCollector : IExternalDataCollector
{
    private readonly HttpClient httpClient;
    private readonly 대한민국행정동관할CodeOptions options;
    private readonly TimeProvider timeProvider;

    public 대한민국행정동관할CodeCollector(
        HttpClient httpClient,
        IOptions<대한민국행정동관할CodeOptions> options,
        TimeProvider timeProvider)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public bool CanCollect(ExternalDataSourceDefinition source)
        => source.SourceId == 대한민국행정동관할CodeDataset.SourceId
           && source.DatasetId == 대한민국행정동관할CodeDataset.DatasetId;

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

        var endpoint = ValidateOptions();
        using var response = await httpClient.GetAsync(
            endpoint,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw ProviderError(response.StatusCode, response.Headers.RetryAfter);
        if (response.Content.Headers.ContentLength is > 0
            && response.Content.Headers.ContentLength > options.MaxArchiveBytes)
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        byte[] bytes;
        대한민국행정동관할Archive archive;
        try
        {
            bytes = await 대한민국법정동CodeArchiveReader.ReadAllBytesAsync(
                stream,
                options.MaxArchiveBytes,
                cancellationToken);
            archive = 대한민국행정동관할CodeArchiveReader.Read(
                bytes,
                options.MaxExpandedBytes,
                options.MaxRecordCount);
        }
        catch (InvalidDataException error)
        {
            throw new ExternalDataCollectionException(
                ExternalDataCollectionErrorCode.InvalidPayload,
                innerException: error);
        }

        var retrievedAt = response.Headers.Date ?? timeProvider.GetUtcNow();
        return new ExternalDataCollectedPayload(
            new MemoryStream(bytes, writable: false),
            $"대한민국-행정기관-관할법정동-{archive.기준일}.zip",
            "application/zip",
            DateTimeOffset.ParseExact(
                archive.기준일,
                "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal),
            $"mois-jscode:{archive.기준일}:retrieved:{retrievedAt:yyyy-MM-dd}",
            archive.RecordCount);
    }

    private Uri ValidateOptions()
    {
        if (!Uri.TryCreate(options.ArchiveUrl, UriKind.Absolute, out var endpoint)
            || endpoint.Scheme != Uri.UriSchemeHttps
            || !endpoint.Host.EndsWith("mois.go.kr", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("KoreaAdministrativeJurisdictionArchiveUrlInvalid");
        if (options.MaxArchiveBytes is < 1024 or > 50 * 1024 * 1024)
            throw new InvalidOperationException("KoreaAdministrativeJurisdictionArchiveLimitInvalid");
        if (options.MaxExpandedBytes < options.MaxArchiveBytes
            || options.MaxExpandedBytes > 200 * 1024 * 1024)
            throw new InvalidOperationException("KoreaAdministrativeJurisdictionExpandedLimitInvalid");
        if (options.MaxRecordCount is < 10_000 or > 1_000_000)
            throw new InvalidOperationException("KoreaAdministrativeJurisdictionRecordLimitInvalid");
        return endpoint;
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
