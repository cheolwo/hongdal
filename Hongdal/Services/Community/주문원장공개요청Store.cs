using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 홍달.Services.Options;

namespace Hongdal.Services.Community;

public static class 원장공개요청상태
{
    public const string 승인대기 = "승인대기";
    public const string 승인 = "승인";
    public const string 거절 = "거절";
    public const string 철회 = "철회";
    public const string 만료 = "만료";
}

public static class 원장공개범위
{
    public const string 원장상세 = "원장상세";
}

public sealed record 원장공개요청기록(
    string 요청Id,
    string 주문원장Id,
    string 대상원장Id,
    string 요청자UserId,
    string 요청자표시명,
    string 승인자UserId,
    string 공개범위,
    string 사유,
    string 상태,
    DateTimeOffset 요청시각Utc,
    DateTimeOffset? 처리시각Utc,
    DateTimeOffset 만료시각Utc,
    string? 처리메모);

public interface I주문원장공개요청저장소
{
    Task<원장공개요청기록> 요청생성Async(
        원장공개요청기록 요청,
        CancellationToken cancellationToken = default);

    Task<원장공개요청기록?> 요청조회Async(
        string 요청Id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<원장공개요청기록>> 받은요청목록Async(
        string 승인자UserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> 승인된대상원장Ids조회Async(
        string 주문원장Id,
        string 요청자UserId,
        IEnumerable<string> 대상원장Ids,
        DateTimeOffset 기준시각Utc,
        CancellationToken cancellationToken = default);

    Task<원장공개요청기록?> 요청결정Async(
        string 요청Id,
        string 승인자UserId,
        bool 승인여부,
        string? 처리메모,
        DateTimeOffset 처리시각Utc,
        DateTimeOffset 승인만료시각Utc,
        CancellationToken cancellationToken = default);
}

public sealed class Mongo주문원장공개요청저장소 : I주문원장공개요청저장소
{
    private const string CollectionName = "community_ledger_disclosure_requests";
    private readonly IMongoCollection<원장공개요청문서> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public Mongo주문원장공개요청저장소(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Database))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(options.Value.Database.Trim())
            .GetCollection<원장공개요청문서>(CollectionName);
    }

    public async Task<원장공개요청기록> 요청생성Async(
        원장공개요청기록 요청,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var existing = await _collection.Find(x =>
                x.주문원장Id == 요청.주문원장Id
                && x.대상원장Id == 요청.대상원장Id
                && x.요청자UserId == 요청.요청자UserId
                && x.상태 == 원장공개요청상태.승인대기
                && x.만료시각Utc > now)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            return ToRecord(existing);
        }

        var document = ToDocument(요청);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        return ToRecord(document);
    }

    public async Task<원장공개요청기록?> 요청조회Async(
        string 요청Id,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var document = await _collection
            .Find(x => x.요청Id == 요청Id)
            .FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToRecord(document);
    }

    public async Task<IReadOnlyList<원장공개요청기록>> 받은요청목록Async(
        string 승인자UserId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var documents = await _collection
            .Find(x => x.승인자UserId == 승인자UserId)
            .SortByDescending(x => x.요청시각Utc)
            .Limit(100)
            .ToListAsync(cancellationToken);
        return documents.Select(ToRecord).ToArray();
    }

    public async Task<IReadOnlySet<string>> 승인된대상원장Ids조회Async(
        string 주문원장Id,
        string 요청자UserId,
        IEnumerable<string> 대상원장Ids,
        DateTimeOffset 기준시각Utc,
        CancellationToken cancellationToken = default)
    {
        var ids = 대상원장Ids
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (ids.Length == 0)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        await EnsureIndexesAsync(cancellationToken);
        var documents = await _collection.Find(
                Builders<원장공개요청문서>.Filter.Eq(x => x.주문원장Id, 주문원장Id)
                & Builders<원장공개요청문서>.Filter.Eq(x => x.요청자UserId, 요청자UserId)
                & Builders<원장공개요청문서>.Filter.Eq(x => x.상태, 원장공개요청상태.승인)
                & Builders<원장공개요청문서>.Filter.Gt(x => x.만료시각Utc, 기준시각Utc.UtcDateTime)
                & Builders<원장공개요청문서>.Filter.In(x => x.대상원장Id, ids))
            .Project(x => x.대상원장Id)
            .ToListAsync(cancellationToken);
        return documents.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<원장공개요청기록?> 요청결정Async(
        string 요청Id,
        string 승인자UserId,
        bool 승인여부,
        string? 처리메모,
        DateTimeOffset 처리시각Utc,
        DateTimeOffset 승인만료시각Utc,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var update = Builders<원장공개요청문서>.Update
            .Set(x => x.상태, 승인여부 ? 원장공개요청상태.승인 : 원장공개요청상태.거절)
            .Set(x => x.처리시각Utc, 처리시각Utc.UtcDateTime)
            .Set(x => x.처리메모, Clean(처리메모));
        if (승인여부)
        {
            update = update.Set(x => x.만료시각Utc, 승인만료시각Utc.UtcDateTime);
        }

        var document = await _collection.FindOneAndUpdateAsync(
            x => x.요청Id == 요청Id
                 && x.승인자UserId == 승인자UserId
                 && x.상태 == 원장공개요청상태.승인대기
                 && x.만료시각Utc > 처리시각Utc.UtcDateTime,
            update,
            new FindOneAndUpdateOptions<원장공개요청문서> { ReturnDocument = ReturnDocument.After },
            cancellationToken);
        return document is null ? null : ToRecord(document);
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (_indexesReady)
        {
            return;
        }

        await _indexLock.WaitAsync(cancellationToken);
        try
        {
            if (_indexesReady)
            {
                return;
            }

            await _collection.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<원장공개요청문서>(
                    Builders<원장공개요청문서>.IndexKeys
                        .Ascending(x => x.승인자UserId)
                        .Ascending(x => x.상태)
                        .Descending(x => x.요청시각Utc),
                    new CreateIndexOptions { Name = "ix_disclosure_owner_inbox" }),
                new CreateIndexModel<원장공개요청문서>(
                    Builders<원장공개요청문서>.IndexKeys
                        .Ascending(x => x.주문원장Id)
                        .Ascending(x => x.요청자UserId)
                        .Ascending(x => x.대상원장Id)
                        .Ascending(x => x.상태)
                        .Ascending(x => x.만료시각Utc),
                    new CreateIndexOptions { Name = "ix_disclosure_grant_lookup" })
            ], cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static 원장공개요청문서 ToDocument(원장공개요청기록 record)
        => new()
        {
            요청Id = record.요청Id,
            주문원장Id = record.주문원장Id,
            대상원장Id = record.대상원장Id,
            요청자UserId = record.요청자UserId,
            요청자표시명 = record.요청자표시명,
            승인자UserId = record.승인자UserId,
            공개범위 = record.공개범위,
            사유 = record.사유,
            상태 = record.상태,
            요청시각Utc = record.요청시각Utc.UtcDateTime,
            처리시각Utc = record.처리시각Utc?.UtcDateTime,
            만료시각Utc = record.만료시각Utc.UtcDateTime,
            처리메모 = record.처리메모
        };

    private static 원장공개요청기록 ToRecord(원장공개요청문서 document)
        => new(
            document.요청Id,
            document.주문원장Id,
            document.대상원장Id,
            document.요청자UserId,
            document.요청자표시명,
            document.승인자UserId,
            document.공개범위,
            document.사유,
            document.상태,
            AsOffset(document.요청시각Utc),
            document.처리시각Utc.HasValue ? AsOffset(document.처리시각Utc.Value) : null,
            AsOffset(document.만료시각Utc),
            document.처리메모);

    private static DateTimeOffset AsOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class 원장공개요청문서
{
    [BsonId]
    public string 요청Id { get; set; } = string.Empty;
    public string 주문원장Id { get; set; } = string.Empty;
    public string 대상원장Id { get; set; } = string.Empty;
    public string 요청자UserId { get; set; } = string.Empty;
    public string 요청자표시명 { get; set; } = string.Empty;
    public string 승인자UserId { get; set; } = string.Empty;
    public string 공개범위 { get; set; } = 원장공개범위.원장상세;
    public string 사유 { get; set; } = string.Empty;
    public string 상태 { get; set; } = 원장공개요청상태.승인대기;
    public DateTime 요청시각Utc { get; set; }
    public DateTime? 처리시각Utc { get; set; }
    public DateTime 만료시각Utc { get; set; }
    public string? 처리메모 { get; set; }
}
