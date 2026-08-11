using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed class 대한민국법정동CodeCollector : IExternalDataCollector
{
    private readonly HttpClient httpClient;
    private readonly 대한민국법정동CodeOptions options;
    private readonly TimeProvider timeProvider;

    public 대한민국법정동CodeCollector(
        HttpClient httpClient,
        IOptions<대한민국법정동CodeOptions> options,
        TimeProvider timeProvider)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public bool CanCollect(ExternalDataSourceDefinition source)
        => source.SourceId == 대한민국법정동CodeDataset.SourceId
           && source.DatasetId == 대한민국법정동CodeDataset.DatasetId;

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
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("codeseId", "법정동코드")
            ]),
        };
        using var response = await httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw ProviderError(response.StatusCode, response.Headers.RetryAfter);
        if (response.Content.Headers.ContentLength is > 0
            && response.Content.Headers.ContentLength > options.MaxArchiveBytes)
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        byte[] bytes;
        IReadOnlyList<대한민국법정동CodeRow> rows;
        try
        {
            bytes = await 대한민국법정동CodeArchiveReader.ReadAllBytesAsync(
                stream,
                options.MaxArchiveBytes,
                cancellationToken);
            rows = 대한민국법정동CodeArchiveReader.Read(
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
            "대한민국-법정동코드-전체자료.zip",
            "application/zip",
            evidenceAsOfUtc: null,
            $"moi-standard-code:retrieved:{retrievedAt:yyyy-MM-dd}",
            rows.Count);
    }

    private Uri BuildEndpoint()
    {
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("KoreaLegalDongBaseUrlMustUseHttps");
        if (options.MaxArchiveBytes is < 1024 or > 50 * 1024 * 1024)
            throw new InvalidOperationException("KoreaLegalDongArchiveLimitInvalid");
        if (options.MaxExpandedBytes < options.MaxArchiveBytes
            || options.MaxExpandedBytes > 100 * 1024 * 1024)
            throw new InvalidOperationException("KoreaLegalDongExpandedLimitInvalid");
        if (options.MaxRecordCount is < 1_000 or > 1_000_000)
            throw new InvalidOperationException("KoreaLegalDongRecordLimitInvalid");

        return new Uri(
            new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/"),
            대한민국법정동CodeDataset.DownloadPath);
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
