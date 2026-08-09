using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;

namespace 살뜰.Services.External.PublicData;

public enum ExternalDataCollectionErrorCode
{
    MissingCredential,
    Unauthorized,
    Forbidden,
    NotFound,
    RateLimited,
    Timeout,
    InvalidPayload,
    SourceDisabled,
    CollectorMissing,
    NormalizerMissing,
    Unexpected,
}

public sealed class ExternalDataCollectionException : Exception
{
    public ExternalDataCollectionException(
        ExternalDataCollectionErrorCode errorCode,
        bool retryable = false,
        TimeSpan? retryAfter = null,
        Exception? innerException = null)
        : base(errorCode.ToString(), innerException)
    {
        ErrorCode = errorCode;
        Retryable = retryable;
        RetryAfter = retryAfter;
    }

    public ExternalDataCollectionErrorCode ErrorCode { get; }
    public bool Retryable { get; }
    public TimeSpan? RetryAfter { get; }
}

public sealed record ExternalDataIngestionRequest
{
    public string SourceId { get; init; } = string.Empty;
    public string DatasetId { get; init; } = string.Empty;
    public string RunKey { get; init; } = Guid.NewGuid().ToString("N");
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public int MaxAttempts { get; init; } = 1;
    public bool ForceReprocess { get; init; }
}

public sealed record ExternalDataIngestionResult
{
    public string RunKey { get; init; } = string.Empty;
    public string StatusCode { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string DatasetId { get; init; } = string.Empty;
    public string SourceVersion { get; init; } = string.Empty;
    public string DataRevision { get; init; } = string.Empty;
    public int FetchedCount { get; init; }
    public int NormalizedCount { get; init; }
    public int RejectedCount { get; init; }
    public int InsertedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int ExistingCount { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
}

public sealed class ExternalDataCollectedPayload : IAsyncDisposable
{
    public ExternalDataCollectedPayload(
        Stream content,
        string originalFileName,
        string contentType,
        DateTimeOffset? evidenceAsOfUtc,
        string sourceVersion,
        int fetchedCount)
    {
        Content = content is { CanRead: true }
            ? content
            : throw new ArgumentException("ReadableContentRequired", nameof(content));
        OriginalFileName = string.IsNullOrWhiteSpace(originalFileName)
            ? "external-data.bin"
            : Path.GetFileName(originalFileName);
        ContentType = contentType?.Trim() ?? string.Empty;
        EvidenceAsOfUtc = evidenceAsOfUtc;
        SourceVersion = sourceVersion?.Trim() ?? string.Empty;
        FetchedCount = Math.Max(0, fetchedCount);
    }

    public Stream Content { get; }
    public string OriginalFileName { get; }
    public string ContentType { get; }
    public DateTimeOffset? EvidenceAsOfUtc { get; }
    public string SourceVersion { get; }
    public int FetchedCount { get; }
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}

public sealed record ExternalDataRawStorageResult(
    string ContentHashSha256,
    long ContentLength,
    string StorageContainer,
    string StorageObjectName,
    string StorageLocation);

public interface IExternalDataCollector
{
    bool CanCollect(ExternalDataSourceDefinition source);

    Task<ExternalDataCollectedPayload> CollectAsync(
        ExternalDataSourceDefinition source,
        ExternalDataIngestionRequest request,
        ExternalDataCredential? credential,
        CancellationToken cancellationToken = default);
}

public interface IExternalDataRawStorage
{
    Task<ExternalDataRawStorageResult> StoreAsync(
        ExternalDataSourceDefinition source,
        ExternalDataCollectedPayload payload,
        DateTimeOffset collectedAtUtc,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        외부데이터RawSnapshot snapshot,
        CancellationToken cancellationToken = default);
}

public sealed record ExternalDataNormalizationBatch(
    IReadOnlyCollection<외부데이터정규화Record> Records,
    int RejectedCount,
    string DataRevision);

public interface IExternalDataNormalizer
{
    bool CanNormalize(ExternalDataSourceDefinition source);

    Task<ExternalDataNormalizationBatch> NormalizeAsync(
        ExternalDataSourceDefinition source,
        외부데이터RawSnapshot rawSnapshot,
        IExternalDataRawStorage rawStorage,
        CancellationToken cancellationToken = default);
}

public interface IExternalDataRetryDelay
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}

public sealed class SystemExternalDataRetryDelay : IExternalDataRetryDelay
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        => Task.Delay(delay, cancellationToken);
}

public interface IExternalDataIngestionRuntime
{
    Task<ExternalDataIngestionResult> IngestAsync(
        ExternalDataIngestionRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class ExternalDataIngestionRuntime : IExternalDataIngestionRuntime
{
    private readonly IExternalDataSourceCatalog sourceCatalog;
    private readonly IExternalDataCredentialProvider credentialProvider;
    private readonly IExternalDataCollectionPolicy collectionPolicy;
    private readonly IReadOnlyCollection<IExternalDataCollector> collectors;
    private readonly IReadOnlyCollection<IExternalDataNormalizer> normalizers;
    private readonly IExternalDataRawStorage rawStorage;
    private readonly I외부데이터수집Store store;
    private readonly IExternalDataRetryDelay retryDelay;
    private readonly TimeProvider timeProvider;

    public ExternalDataIngestionRuntime(
        IExternalDataSourceCatalog sourceCatalog,
        IExternalDataCredentialProvider credentialProvider,
        IExternalDataCollectionPolicy collectionPolicy,
        IEnumerable<IExternalDataCollector> collectors,
        IEnumerable<IExternalDataNormalizer> normalizers,
        IExternalDataRawStorage rawStorage,
        I외부데이터수집Store store,
        IExternalDataRetryDelay retryDelay,
        TimeProvider timeProvider)
    {
        this.sourceCatalog = sourceCatalog ?? throw new ArgumentNullException(nameof(sourceCatalog));
        this.credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
        this.collectionPolicy = collectionPolicy ?? throw new ArgumentNullException(nameof(collectionPolicy));
        this.collectors = (collectors ?? []).ToArray();
        this.normalizers = (normalizers ?? []).ToArray();
        this.rawStorage = rawStorage ?? throw new ArgumentNullException(nameof(rawStorage));
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.retryDelay = retryDelay ?? throw new ArgumentNullException(nameof(retryDelay));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ExternalDataIngestionResult> IngestAsync(
        ExternalDataIngestionRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var source = sourceCatalog.GetRequired(request.SourceId, request.DatasetId);
        if (!collectionPolicy.IsEnabled(source))
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.SourceDisabled);

        var run = await store.StartRunAsync(new 외부데이터수집Run
        {
            RunKey = request.RunKey.Trim(),
            SourceId = source.SourceId,
            DatasetId = source.DatasetId,
            StartedAtUtc = timeProvider.GetUtcNow(),
        }, cancellationToken);

        try
        {
            var credential = await credentialProvider.GetAsync(
                source.SourceId,
                source.DatasetId,
                cancellationToken);
            if (source.RequiresCredential && credential is null)
                throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.MissingCredential);
            var collector = collectors.SingleOrDefault(candidate => candidate.CanCollect(source))
                ?? throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.CollectorMissing);
            var normalizer = normalizers.SingleOrDefault(candidate => candidate.CanNormalize(source))
                ?? throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.NormalizerMissing);

            await using var payload = await CollectWithRetryAsync(
                collector,
                source,
                request,
                credential,
                run,
                cancellationToken);
            var collectedAt = timeProvider.GetUtcNow();
            var stored = await rawStorage.StoreAsync(source, payload, collectedAt, cancellationToken);
            var existingRaw = await store.FindRawSnapshotAsync(
                source.SourceId,
                source.DatasetId,
                stored.ContentHashSha256,
                cancellationToken);
            if (existingRaw is not null && !request.ForceReprocess)
            {
                run.StatusCode = 외부데이터수집StatusCodes.Success;
                run.CompletedAtUtc = timeProvider.GetUtcNow();
                run.FetchedCount = payload.FetchedCount;
                run.ExistingCount = 1;
                run.SourceVersion = payload.SourceVersion;
                await store.CompleteRunAsync(run, cancellationToken);
                return Result(run);
            }

            var raw = existingRaw ?? await store.SaveRawSnapshotAsync(new 외부데이터RawSnapshot
            {
                FirstCollectionRunId = run.Id,
                SourceId = source.SourceId,
                DatasetId = source.DatasetId,
                SourceVersion = payload.SourceVersion,
                CollectedAtUtc = collectedAt,
                EvidenceAsOfUtc = payload.EvidenceAsOfUtc,
                ContentHashSha256 = stored.ContentHashSha256,
                ContentLength = stored.ContentLength,
                ContentType = payload.ContentType,
                OriginalFileName = payload.OriginalFileName,
                StorageContainer = stored.StorageContainer,
                StorageObjectName = stored.StorageObjectName,
                StorageLocation = stored.StorageLocation,
                FirstSeenAtUtc = collectedAt,
                LastSeenAtUtc = collectedAt,
            }, cancellationToken);

            var normalized = await normalizer.NormalizeAsync(source, raw, rawStorage, cancellationToken);
            ExternalDataNormalizationValidator.Validate(source, raw, normalized);
            var saveResult = await store.UpsertNormalizedAsync(normalized.Records, cancellationToken);
            run.StatusCode = normalized.RejectedCount > 0
                ? 외부데이터수집StatusCodes.Partial
                : 외부데이터수집StatusCodes.Success;
            run.CompletedAtUtc = timeProvider.GetUtcNow();
            run.FetchedCount = payload.FetchedCount;
            run.NormalizedCount = normalized.Records.Count;
            run.RejectedCount = normalized.RejectedCount;
            run.InsertedCount = saveResult.InsertedCount;
            run.UpdatedCount = saveResult.UpdatedCount;
            run.ExistingCount = saveResult.ExistingCount;
            run.SourceVersion = payload.SourceVersion;
            run.DataRevision = normalized.DataRevision;
            await store.CompleteRunAsync(run, cancellationToken);
            return Result(run);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            run.StatusCode = 외부데이터수집StatusCodes.Cancelled;
            run.CompletedAtUtc = timeProvider.GetUtcNow();
            run.ErrorCode = "Cancelled";
            run.ErrorSummary = "Collection was cancelled by the caller.";
            await store.CompleteRunAsync(run, CancellationToken.None);
            throw;
        }
        catch (Exception error)
        {
            var code = error is ExternalDataCollectionException typed
                ? typed.ErrorCode
                : ExternalDataCollectionErrorCode.Unexpected;
            run.StatusCode = 외부데이터수집StatusCodes.Failed;
            run.CompletedAtUtc = timeProvider.GetUtcNow();
            run.ErrorCode = code.ToString();
            run.ErrorSummary = SafeSummary(code);
            await store.CompleteRunAsync(run, CancellationToken.None);
            return Result(run);
        }
    }

    private async Task<ExternalDataCollectedPayload> CollectWithRetryAsync(
        IExternalDataCollector collector,
        ExternalDataSourceDefinition source,
        ExternalDataIngestionRequest request,
        ExternalDataCredential? credential,
        외부데이터수집Run run,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= request.MaxAttempts; attempt++)
        {
            run.AttemptCount = attempt;
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(request.Timeout);
            try
            {
                return await collector.CollectAsync(source, request, credential, timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (attempt == request.MaxAttempts)
                    throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.Timeout);
                await retryDelay.DelayAsync(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
            catch (ExternalDataCollectionException error) when (error.Retryable && attempt < request.MaxAttempts)
            {
                await retryDelay.DelayAsync(
                    error.RetryAfter ?? TimeSpan.FromMilliseconds(200 * attempt),
                    cancellationToken);
            }
        }

        throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.Unexpected);
    }

    private static ExternalDataIngestionResult Result(외부데이터수집Run run) => new()
    {
        RunKey = run.RunKey,
        StatusCode = run.StatusCode,
        SourceId = run.SourceId,
        DatasetId = run.DatasetId,
        SourceVersion = run.SourceVersion,
        DataRevision = run.DataRevision,
        FetchedCount = run.FetchedCount,
        NormalizedCount = run.NormalizedCount,
        RejectedCount = run.RejectedCount,
        InsertedCount = run.InsertedCount,
        UpdatedCount = run.UpdatedCount,
        ExistingCount = run.ExistingCount,
        ErrorCode = run.ErrorCode,
    };

    private static string SafeSummary(ExternalDataCollectionErrorCode code) => code switch
    {
        ExternalDataCollectionErrorCode.MissingCredential => "Required server credential is not configured.",
        ExternalDataCollectionErrorCode.Unauthorized => "Provider authentication was rejected.",
        ExternalDataCollectionErrorCode.Forbidden => "Provider access is forbidden.",
        ExternalDataCollectionErrorCode.NotFound => "Provider dataset was not found.",
        ExternalDataCollectionErrorCode.RateLimited => "Provider rate limit was reached.",
        ExternalDataCollectionErrorCode.Timeout => "Provider request timed out.",
        ExternalDataCollectionErrorCode.InvalidPayload => "Provider payload validation failed.",
        ExternalDataCollectionErrorCode.CollectorMissing => "Collector is not registered.",
        ExternalDataCollectionErrorCode.NormalizerMissing => "Normalizer is not registered.",
        _ => "External data collection failed.",
    };

    private static void Validate(ExternalDataIngestionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatasetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RunKey);
        if (request.Timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(request.Timeout));
        if (request.MaxAttempts is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(request.MaxAttempts));
    }
}

public static class ExternalDataNormalizationValidator
{
    public static void Validate(
        ExternalDataSourceDefinition source,
        외부데이터RawSnapshot raw,
        ExternalDataNormalizationBatch batch)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(raw);
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentException.ThrowIfNullOrWhiteSpace(batch.DataRevision);
        var duplicate = batch.Records.GroupBy(record => record.RecordKey, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);
        foreach (var record in batch.Records)
        {
            if (!string.Equals(record.SourceId, source.SourceId, StringComparison.Ordinal)
                || !string.Equals(record.DatasetId, source.DatasetId, StringComparison.Ordinal)
                || record.RawSnapshotId != raw.Id
                || !RegionStableIdRules.IsValid(record.RegionStableId)
                || record.EvidenceAsOfUtc == default
                || record.CollectedAtUtc == default
                || string.IsNullOrWhiteSpace(record.MetricCode)
                || string.IsNullOrWhiteSpace(record.UnitCode)
                || string.IsNullOrWhiteSpace(record.TemporalPrecisionCode)
                || string.IsNullOrWhiteSpace(record.DataRevision)
                || string.IsNullOrWhiteSpace(record.RecordKey))
                throw new ExternalDataCollectionException(ExternalDataCollectionErrorCode.InvalidPayload);
        }
    }
}
