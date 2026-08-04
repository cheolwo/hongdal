using System.Text.Json;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Ssalddel.Contracts.Common.Content;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Content;

public interface I공식뉴스검토원장Store
{
    Task<공식뉴스검토원장Record?> 조회Async(
        string candidateKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공식뉴스검토원장Record>> 목록Async(
        IReadOnlyCollection<string>? sourceKeys,
        string? reviewState,
        int take,
        CancellationToken cancellationToken = default);

    Task 추가Async(
        공식뉴스검토원장Record record,
        CancellationToken cancellationToken = default);

    Task<bool> 교체Async(
        공식뉴스검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default);
}

public interface I공식뉴스검토원장Service
{
    Task<공식뉴스검토원장Dto> 결정기록Async(
        string candidateKey,
        공식뉴스검토결정Request request,
        string reviewerId,
        string reviewerDisplayName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공식뉴스검토원장Dto>> 목록Async(
        IReadOnlyCollection<string>? sourceKeys,
        string? reviewState,
        int take,
        CancellationToken cancellationToken = default);
}

public sealed class 공식뉴스검토원장Service(
    ICommunityInformationCollectionService informationCollectionService,
    I공식뉴스검토원장Store store,
    TimeProvider timeProvider) : I공식뉴스검토원장Service
{
    public async Task<공식뉴스검토원장Dto> 결정기록Async(
        string candidateKey,
        공식뉴스검토결정Request request,
        string reviewerId,
        string reviewerDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalizedCandidateKey = Require(candidateKey, "candidateKey");
        var sourceKey = Require(request.SourceKey, nameof(request.SourceKey));
        var decisionCode = NormalizeDecision(request.DecisionCode);
        var idempotencyKey = Require(request.IdempotencyKey, nameof(request.IdempotencyKey), 100);
        var reviewer = Require(reviewerId, nameof(reviewerId), 200);
        var displayName = Require(reviewerDisplayName, nameof(reviewerDisplayName), 100);
        var note = NormalizeOptional(request.DecisionNote, 500);

        var existing = await store.조회Async(normalizedCandidateKey, cancellationToken);
        var duplicate = existing?.History.FirstOrDefault(item => string.Equals(
            item.IdempotencyKey,
            idempotencyKey,
            StringComparison.Ordinal));
        if (duplicate is not null)
        {
            return ToDto(existing!);
        }

        if (existing is not null
            && (!request.ExpectedRevision.HasValue
                || request.ExpectedRevision.Value != existing.Revision))
        {
            throw new 공식뉴스검토원장ConcurrencyException(existing.Revision);
        }

        if (existing is null && request.ExpectedRevision is > 0)
        {
            throw new 공식뉴스검토원장ConcurrencyException(0);
        }

        var candidate = await ResolveCandidateAsync(
            normalizedCandidateKey,
            sourceKey,
            cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (existing is null)
        {
            var created = new 공식뉴스검토원장Record
            {
                CandidateKey = normalizedCandidateKey,
                SourceKey = sourceKey,
                ReviewState = decisionCode,
                Revision = 1,
                CandidateJson = JsonSerializer.Serialize(candidate),
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                History =
                [
                    History(idempotencyKey, decisionCode, note, reviewer, displayName, now, 1)
                ]
            };
            await store.추가Async(created, cancellationToken);
            return ToDto(created);
        }

        var expectedRevision = existing.Revision;
        existing.SourceKey = sourceKey;
        existing.ReviewState = decisionCode;
        existing.CandidateJson = JsonSerializer.Serialize(candidate);
        existing.Revision++;
        existing.UpdatedAtUtc = now;
        existing.History.Add(History(
            idempotencyKey,
            decisionCode,
            note,
            reviewer,
            displayName,
            now,
            existing.Revision));
        if (!await store.교체Async(existing, expectedRevision, cancellationToken))
        {
            throw new 공식뉴스검토원장ConcurrencyException(expectedRevision);
        }

        return ToDto(existing);
    }

    public async Task<IReadOnlyList<공식뉴스검토원장Dto>> 목록Async(
        IReadOnlyCollection<string>? sourceKeys,
        string? reviewState,
        int take,
        CancellationToken cancellationToken = default)
        => (await store.목록Async(
                sourceKeys,
                NormalizeReviewState(reviewState),
                Math.Clamp(take, 1, 100),
                cancellationToken))
            .Select(ToDto)
            .ToArray();

    private async Task<CommunityInformationCandidateDto> ResolveCandidateAsync(
        string candidateKey,
        string sourceKey,
        CancellationToken cancellationToken)
    {
        var source = informationCollectionService.GetSources().FirstOrDefault(item =>
            string.Equals(item.SourceKey, sourceKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                item.SourceType,
                CommunityInformationSourceTypes.OfficialNews,
                StringComparison.Ordinal));
        if (source is null)
        {
            throw new ArgumentException("공식뉴스 검토 원천을 찾을 수 없습니다.", nameof(sourceKey));
        }

        var collected = await informationCollectionService.ReadAsync(
            new CommunityInformationCollectionQuery
            {
                SourceKey = sourceKey,
                ReviewState = CommunityInformationReviewStates.PendingReview,
                Take = 100
            },
            cancellationToken);
        if (collected.Failures.Count > 0)
        {
            throw new InvalidOperationException("공식뉴스 원천 후보를 다시 확인하지 못했습니다.");
        }

        return collected.Items.FirstOrDefault(item => string.Equals(
                   item.CandidateKey,
                   candidateKey,
                   StringComparison.Ordinal))
               ?? throw new KeyNotFoundException("검토할 공식뉴스 후보를 찾을 수 없습니다.");
    }

    private static 공식뉴스검토결정이력Record History(
        string idempotencyKey,
        string decisionCode,
        string note,
        string reviewerId,
        string reviewerDisplayName,
        DateTime now,
        long revision)
        => new()
        {
            IdempotencyKey = idempotencyKey,
            DecisionCode = decisionCode,
            DecisionNote = note,
            ReviewerId = reviewerId,
            ReviewerDisplayName = reviewerDisplayName,
            DecidedAtUtc = now,
            Revision = revision
        };

    private static string NormalizeDecision(string? value)
        => value?.Trim() switch
        {
            CommunityInformationReviewStates.Approved => CommunityInformationReviewStates.Approved,
            CommunityInformationReviewStates.Excluded => CommunityInformationReviewStates.Excluded,
            _ => throw new ArgumentException("검토 결정은 Approved 또는 Excluded만 허용합니다.", nameof(value))
        };

    private static string? NormalizeReviewState(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim() switch
            {
                CommunityInformationReviewStates.Approved => CommunityInformationReviewStates.Approved,
                CommunityInformationReviewStates.Excluded => CommunityInformationReviewStates.Excluded,
                _ => throw new ArgumentException("검토 상태는 Approved 또는 Excluded만 조회할 수 있습니다.", nameof(value))
            };

    private static string Require(string? value, string name, int maxLength = 500)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException($"{name} 값이 필요합니다.", name);
        }

        return normalized.Length <= maxLength
            ? normalized
            : throw new ArgumentException($"{name} 값이 너무 깁니다.", name);
    }

    private static string NormalizeOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }

    internal static 공식뉴스검토원장Dto ToDto(공식뉴스검토원장Record record)
    {
        var candidate = JsonSerializer.Deserialize<CommunityInformationCandidateDto>(record.CandidateJson)
                        ?? throw new InvalidDataException("공식뉴스 검토 원장의 후보 snapshot이 손상되었습니다.");
        return new 공식뉴스검토원장Dto(
            record.CandidateKey,
            record.SourceKey,
            record.ReviewState,
            record.Revision,
            candidate with { ReviewState = record.ReviewState },
            record.CreatedAtUtc,
            record.UpdatedAtUtc,
            record.History.Select(item => new 공식뉴스검토결정이력Dto(
                item.IdempotencyKey,
                item.DecisionCode,
                item.DecisionNote,
                item.ReviewerDisplayName,
                item.DecidedAtUtc,
                item.Revision)).ToArray());
    }
}

public sealed class 공식뉴스검토원장ConcurrencyException(long currentRevision)
    : InvalidOperationException($"공식뉴스 검토 원장이 변경되었습니다. CurrentRevision={currentRevision}")
{
    public long CurrentRevision { get; } = currentRevision;
}

public sealed class 공식뉴스검토원장Record
{
    [BsonId]
    public string CandidateKey { get; set; } = string.Empty;
    public string SourceKey { get; set; } = string.Empty;
    public string ReviewState { get; set; } = CommunityInformationReviewStates.PendingReview;
    public long Revision { get; set; }
    public string CandidateJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<공식뉴스검토결정이력Record> History { get; set; } = [];
}

public sealed class 공식뉴스검토결정이력Record
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string DecisionCode { get; set; } = string.Empty;
    public string DecisionNote { get; set; } = string.Empty;
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerDisplayName { get; set; } = string.Empty;
    public DateTime DecidedAtUtc { get; set; }
    public long Revision { get; set; }
}

internal sealed class Mongo공식뉴스검토원장Store : I공식뉴스검토원장Store
{
    private const string CollectionName = "official_news_review_ledgers";
    private readonly IMongoCollection<공식뉴스검토원장Record> collection;
    private readonly SemaphoreSlim indexLock = new(1, 1);
    private bool indexesReady;

    public Mongo공식뉴스검토원장Store(IMongoClient client, Microsoft.Extensions.Options.IOptions<MongoDbOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Database))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        collection = client.GetDatabase(options.Value.Database.Trim())
            .GetCollection<공식뉴스검토원장Record>(CollectionName);
    }

    public async Task<공식뉴스검토원장Record?> 조회Async(string candidateKey, CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        return await collection.Find(item => item.CandidateKey == candidateKey)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<공식뉴스검토원장Record>> 목록Async(
        IReadOnlyCollection<string>? sourceKeys,
        string? reviewState,
        int take,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var filter = Builders<공식뉴스검토원장Record>.Filter.Empty;
        if (sourceKeys is { Count: > 0 })
        {
            filter &= Builders<공식뉴스검토원장Record>.Filter.In(item => item.SourceKey, sourceKeys);
        }
        if (!string.IsNullOrWhiteSpace(reviewState))
        {
            filter &= Builders<공식뉴스검토원장Record>.Filter.Eq(item => item.ReviewState, reviewState);
        }

        return await collection.Find(filter)
            .SortByDescending(item => item.UpdatedAtUtc)
            .Limit(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task 추가Async(공식뉴스검토원장Record record, CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        await collection.InsertOneAsync(record, cancellationToken: cancellationToken);
    }

    public async Task<bool> 교체Async(
        공식뉴스검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default)
        => (await collection.ReplaceOneAsync(
            item => item.CandidateKey == record.CandidateKey && item.Revision == expectedRevision,
            record,
            cancellationToken: cancellationToken)).ModifiedCount == 1;

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (indexesReady)
        {
            return;
        }

        await indexLock.WaitAsync(cancellationToken);
        try
        {
            if (indexesReady)
            {
                return;
            }

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<공식뉴스검토원장Record>(
                    Builders<공식뉴스검토원장Record>.IndexKeys
                        .Ascending(item => item.SourceKey)
                        .Ascending(item => item.ReviewState)
                        .Descending(item => item.UpdatedAtUtc)),
                cancellationToken: cancellationToken);
            indexesReady = true;
        }
        finally
        {
            indexLock.Release();
        }
    }
}

public sealed class InMemory공식뉴스검토원장Store : I공식뉴스검토원장Store
{
    private readonly Dictionary<string, 공식뉴스검토원장Record> records =
        new(StringComparer.Ordinal);
    private readonly object sync = new();

    public Task<공식뉴스검토원장Record?> 조회Async(string candidateKey, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            return Task.FromResult(records.TryGetValue(candidateKey, out var record)
                ? Clone(record)
                : null);
        }
    }

    public Task<IReadOnlyList<공식뉴스검토원장Record>> 목록Async(
        IReadOnlyCollection<string>? sourceKeys,
        string? reviewState,
        int take,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            IReadOnlyList<공식뉴스검토원장Record> result = records.Values
                .Where(item => sourceKeys is not { Count: > 0 }
                               || sourceKeys.Contains(item.SourceKey, StringComparer.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(reviewState)
                               || string.Equals(item.ReviewState, reviewState, StringComparison.Ordinal))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(Math.Clamp(take, 1, 100))
                .Select(Clone)
                .ToArray();
            return Task.FromResult(result);
        }
    }

    public Task 추가Async(공식뉴스검토원장Record record, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (!records.TryAdd(record.CandidateKey, Clone(record)))
            {
                throw new InvalidOperationException("같은 공식뉴스 후보 검토 원장이 이미 존재합니다.");
            }
        }
        return Task.CompletedTask;
    }

    public Task<bool> 교체Async(
        공식뉴스검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (!records.TryGetValue(record.CandidateKey, out var current)
                || current.Revision != expectedRevision)
            {
                return Task.FromResult(false);
            }
            records[record.CandidateKey] = Clone(record);
            return Task.FromResult(true);
        }
    }

    private static 공식뉴스검토원장Record Clone(공식뉴스검토원장Record record)
        => JsonSerializer.Deserialize<공식뉴스검토원장Record>(
            JsonSerializer.Serialize(record))!;
}
