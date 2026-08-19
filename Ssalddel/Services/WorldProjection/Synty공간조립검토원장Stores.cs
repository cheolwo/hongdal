using System.Text.Json;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Ssalddel.Contracts.Common.WorldProjection;
using 살뜰.Services.Options;

namespace Ssalddel.Services.WorldProjection;

public sealed class Synty공간조립검토ConcurrencyException(
    string reviewItemStableId,
    long currentRevision)
    : InvalidOperationException(
        $"Synty 공간 조립 검토 원장이 변경되었습니다. ReviewItemStableId={reviewItemStableId}, CurrentRevision={currentRevision}")
{
    public string ReviewItemStableId { get; } = reviewItemStableId;
    public long CurrentRevision { get; } = currentRevision;
}

public sealed class Synty공간조립검토원장Record
{
    [BsonId]
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string BatchStableId { get; set; } = string.Empty;
    public string BatchRevision { get; set; } = string.Empty;
    public string BatchTitle { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string ReviewStateCode { get; set; } = Synty공간조립검토상태Codes.WaitingForCapture;
    public string SnapshotHash { get; set; } = string.Empty;
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<Synty공간조립검토결정이력Record> History { get; set; } = [];
}

public sealed class Synty공간조립검토결정이력Record
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string DecisionCode { get; set; } = string.Empty;
    public List<string> IssueCodes { get; set; } = [];
    public string Note { get; set; } = string.Empty;
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerDisplayName { get; set; } = string.Empty;
    public DateTime DecidedAtUtc { get; set; }
    public long Revision { get; set; }
}

internal sealed class MongoSynty공간조립검토원장Store : ISynty공간조립검토원장Store
{
    private const string CollectionName = "synty_composition_review_ledgers";
    private readonly IMongoCollection<Synty공간조립검토원장Record> collection;
    private readonly SemaphoreSlim indexLock = new(1, 1);
    private bool indexesReady;

    public MongoSynty공간조립검토원장Store(
        IMongoClient client,
        IOptions<MongoDbOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Database))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }
        collection = client.GetDatabase(options.Value.Database.Trim())
            .GetCollection<Synty공간조립검토원장Record>(CollectionName);
    }

    public async Task<Synty공간조립검토원장Record?> 조회Async(
        string reviewItemStableId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        return await collection.Find(record => record.ReviewItemStableId == reviewItemStableId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Synty공간조립검토원장Record>> 목록Async(
        string? batchStableId,
        string? reviewStateCode,
        int take,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var filter = Builders<Synty공간조립검토원장Record>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(batchStableId))
        {
            filter &= Builders<Synty공간조립검토원장Record>.Filter.Eq(
                record => record.BatchStableId,
                batchStableId);
        }
        if (!string.IsNullOrWhiteSpace(reviewStateCode))
        {
            filter &= Builders<Synty공간조립검토원장Record>.Filter.Eq(
                record => record.ReviewStateCode,
                reviewStateCode);
        }
        return await collection.Find(filter)
            .SortByDescending(record => record.UpdatedAtUtc)
            .Limit(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> 추가Async(
        Synty공간조립검토원장Record record,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        try
        {
            await collection.InsertOneAsync(record, cancellationToken: cancellationToken);
            return true;
        }
        catch (MongoWriteException exception) when (
            exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public async Task<bool> 교체Async(
        Synty공간조립검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default)
        => (await collection.ReplaceOneAsync(
                current => current.ReviewItemStableId == record.ReviewItemStableId
                           && current.Revision == expectedRevision,
                record,
                cancellationToken: cancellationToken))
            .ModifiedCount == 1;

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
                new CreateIndexModel<Synty공간조립검토원장Record>(
                    Builders<Synty공간조립검토원장Record>.IndexKeys
                        .Ascending(record => record.BatchStableId)
                        .Ascending(record => record.ReviewStateCode)
                        .Descending(record => record.UpdatedAtUtc),
                    new CreateIndexOptions { Name = "ix_batch_state_updated" }),
                cancellationToken: cancellationToken);
            indexesReady = true;
        }
        finally
        {
            indexLock.Release();
        }
    }
}

public sealed class InMemorySynty공간조립검토원장Store : ISynty공간조립검토원장Store
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, Synty공간조립검토원장Record> records = new(StringComparer.Ordinal);
    private readonly object sync = new();

    public Task<Synty공간조립검토원장Record?> 조회Async(
        string reviewItemStableId,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            return Task.FromResult(records.TryGetValue(reviewItemStableId, out var record)
                ? Clone(record)
                : null);
        }
    }

    public Task<IReadOnlyList<Synty공간조립검토원장Record>> 목록Async(
        string? batchStableId,
        string? reviewStateCode,
        int take,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            IReadOnlyList<Synty공간조립검토원장Record> result = records.Values
                .Where(record => string.IsNullOrWhiteSpace(batchStableId)
                                 || string.Equals(record.BatchStableId, batchStableId, StringComparison.Ordinal))
                .Where(record => string.IsNullOrWhiteSpace(reviewStateCode)
                                 || string.Equals(record.ReviewStateCode, reviewStateCode, StringComparison.Ordinal))
                .OrderByDescending(record => record.UpdatedAtUtc)
                .Take(Math.Clamp(take, 1, 100))
                .Select(Clone)
                .ToArray();
            return Task.FromResult(result);
        }
    }

    public Task<bool> 추가Async(
        Synty공간조립검토원장Record record,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (records.ContainsKey(record.ReviewItemStableId))
            {
                return Task.FromResult(false);
            }
            records[record.ReviewItemStableId] = Clone(record);
            return Task.FromResult(true);
        }
    }

    public Task<bool> 교체Async(
        Synty공간조립검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (!records.TryGetValue(record.ReviewItemStableId, out var current)
                || current.Revision != expectedRevision)
            {
                return Task.FromResult(false);
            }
            records[record.ReviewItemStableId] = Clone(record);
            return Task.FromResult(true);
        }
    }

    private static Synty공간조립검토원장Record Clone(Synty공간조립검토원장Record record)
        => JsonSerializer.Deserialize<Synty공간조립검토원장Record>(
               JsonSerializer.Serialize(record, JsonOptions),
               JsonOptions)!;
}
