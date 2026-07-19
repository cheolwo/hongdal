using System.Security.Cryptography;
using System.Text;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Metadata;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 홍달.Services.Options;

namespace Hongdal.Services.Community;

public interface I커뮤니티원장저장소
{
    Task<커뮤니티원장Dto> 원장저장Async(
        커뮤니티원장저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<커뮤니티원장Dto?> 원장조회Async(
        string 원장Id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
        커뮤니티원장조회조건 query,
        CancellationToken cancellationToken = default);

    Task<커뮤니티원장Dto?> 원장상태변경Async(
        커뮤니티원장상태변경요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public interface I커뮤니티원장투영작업저장소
{
    Task<커뮤니티원장투영작업?> 다음작업확보Async(
        TimeSpan leaseTimeout,
        CancellationToken cancellationToken = default);

    Task 완료Async(
        string 원장Id,
        long revision,
        string? processingToken,
        CancellationToken cancellationToken = default);

    Task 실패Async(
        string 원장Id,
        long revision,
        string processingToken,
        string 오류,
        int 최대시도횟수,
        TimeSpan 기본재시도간격,
        CancellationToken cancellationToken = default);
}

public sealed record 커뮤니티원장투영작업(
    커뮤니티원장Dto 원장,
    string EventId,
    string 변경유형,
    string 변경자,
    커뮤니티원장상태변경요청? 상태변경요청,
    DateTime 발생시각Utc,
    string ProcessingToken,
    int 시도횟수);

public static class 커뮤니티원장투영상태
{
    public const string 대기 = "대기";
    public const string 처리중 = "처리중";
    public const string 재시도대기 = "재시도대기";
    public const string 완료 = "완료";
    public const string 실패 = "실패";
}

internal static class 커뮤니티원장변경유형값
{
    public const string 저장 = "저장";
    public const string 상태변경 = "상태변경";
}

[HongdalCommunityV0Module(
    HongdalCommunityV0ModuleKeys.Ledger,
    HongdalModuleKind.Persistence,
    "MongoDB community_ledgers 원본과 재시도 가능한 원장 투영 작업을 저장",
    ReleaseStage = HongdalCommunityV0ReleaseStages.ClosedLoop,
    Boundary = "원장은 참여자가 직접 합의한 진행 상태를 기록하며 플랫폼 계약 성립이나 자동 업무 실행의 근거가 아닙니다.")]
public sealed class Mongo커뮤니티원장저장소 : I커뮤니티원장저장소, I커뮤니티원장투영작업저장소
{
    private const string CollectionName = "community_ledgers";
    private readonly IMongoCollection<커뮤니티원장문서> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public Mongo커뮤니티원장저장소(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<커뮤니티원장문서>(CollectionName);
    }

    public async Task<커뮤니티원장Dto> 원장저장Async(
        커뮤니티원장저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        Validate(request);

        var now = DateTime.UtcNow;
        var 원장Id = string.IsNullOrWhiteSpace(request.원장Id)
            ? $"ledger-{Guid.NewGuid():N}"
            : request.원장Id.Trim();
        var existing = await _collection
            .Find(x => x.원장Id == 원장Id)
            .FirstOrDefaultAsync(cancellationToken);
        EnsureExpectedRevision(request.기대Revision, existing?.Revision ?? 0, 원장Id);

        var revision = (existing?.Revision ?? 0) + 1;
        var eventId = CreateEventId(원장Id, revision);

        var 문서 = new 커뮤니티원장문서
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            원장Id = 원장Id,
            커뮤니티Id = request.커뮤니티Id.Trim(),
            원장템플릿Key = request.원장템플릿Key.Trim(),
            제목 = request.제목.Trim(),
            원함 = Clean(request.원함),
            상태 = string.IsNullOrWhiteSpace(request.상태) ? 커뮤니티원장상태.초안 : request.상태.Trim(),
            현재단계Key = Clean(request.현재단계Key),
            대상OsCode = Clean(request.대상OsCode),
            대상OsName = Clean(request.대상OsName),
            생성자UserId = Clean(request.생성자UserId),
            생성자표시명 = string.IsNullOrWhiteSpace(request.생성자표시명) ? "익명 참여자" : request.생성자표시명.Trim(),
            블록목록 = BuildBlockDocuments(request, existing),
            참여자목록 = request.참여자목록.Select(ToDocument).ToArray(),
            포함원장목록 = request.포함원장목록 is null
                ? existing?.포함원장목록 ?? []
                : request.포함원장목록.Select(ToDocument).ToArray(),
            다이어그램스냅샷 = request.다이어그램스냅샷 is null ? null : ToDocument(request.다이어그램스냅샷),
            외부참조 = NormalizeDictionary(request.외부참조),
            확장속성 = NormalizeDictionary(request.확장속성),
            생성시각Utc = existing?.생성시각Utc ?? now,
            수정시각Utc = now,
            수정자 = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            Revision = revision,
            투영완료Revision = existing?.투영완료Revision ?? 0,
            투영상태 = 커뮤니티원장투영상태.대기,
            투영EventId = eventId,
            투영변경유형 = 커뮤니티원장변경유형값.저장,
            투영이전상태 = Clean(existing?.상태),
            투영변경메모 = existing is null ? "원장 생성" : "원장 저장",
            투영발생시각Utc = now,
            투영다음시도시각Utc = now.AddSeconds(5)
        };

        문서.상태이력 = existing?.상태이력?.ToList() ?? [];
        if (existing is null || !string.Equals(existing.상태, 문서.상태, StringComparison.OrdinalIgnoreCase))
        {
            문서.상태이력.Add(new 커뮤니티원장상태이력문서
            {
                EventId = eventId,
                상태 = 문서.상태,
                이전상태 = Clean(existing?.상태),
                현재단계Key = Clean(문서.현재단계Key),
                메모 = existing is null ? "원장 생성" : "원장 저장 중 상태 변경",
                변경자 = 문서.수정자,
                변경시각Utc = now
            });
        }

        try
        {
            var result = await _collection.ReplaceOneAsync(
                BuildRevisionFilter(원장Id, existing?.Revision ?? 0, existing is null),
                문서,
                new ReplaceOptions { IsUpsert = existing is null },
                cancellationToken);

            if (existing is not null && result.MatchedCount == 0)
            {
                throw CreateConcurrencyException(원장Id);
            }
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw CreateConcurrencyException(원장Id, ex);
        }

        return ToDto(문서);
    }

    public async Task<커뮤니티원장Dto?> 원장조회Async(
        string 원장Id,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(원장Id))
        {
            return null;
        }

        var 문서 = await _collection
            .Find(x => x.원장Id == 원장Id.Trim())
            .FirstOrDefaultAsync(cancellationToken);

        return 문서 is null ? null : ToDto(문서);
    }

    public async Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
        커뮤니티원장조회조건 query,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var 문서목록 = await _collection
            .Find(BuildFilter(query))
            .SortByDescending(x => x.수정시각Utc)
            .Limit(query.Limit <= 0 ? 50 : Math.Min(query.Limit, 200))
            .ToListAsync(cancellationToken);

        return 문서목록.Select(ToDto).ToArray();
    }

    public async Task<커뮤니티원장Dto?> 원장상태변경Async(
        커뮤니티원장상태변경요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(request.원장Id))
        {
            throw new InvalidOperationException("원장Id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.상태))
        {
            throw new InvalidOperationException("상태 is required.");
        }

        var now = DateTime.UtcNow;
        var 변경자 = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim();
        var 원장Id = request.원장Id.Trim();
        var existing = await _collection
            .Find(x => x.원장Id == 원장Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            return null;
        }

        EnsureExpectedRevision(request.기대Revision, existing.Revision, 원장Id);
        if (!string.IsNullOrWhiteSpace(request.이전상태)
            && !string.Equals(request.이전상태.Trim(), existing.상태, StringComparison.OrdinalIgnoreCase))
        {
            throw CreateConcurrencyException(원장Id);
        }

        var revision = existing.Revision + 1;
        var eventId = CreateEventId(원장Id, revision);
        var 상태이력 = new 커뮤니티원장상태이력문서
        {
            EventId = eventId,
            상태 = request.상태.Trim(),
            이전상태 = Clean(existing.상태),
            현재단계Key = Clean(request.현재단계Key),
            메모 = Clean(request.메모),
            변경자 = 변경자,
            변경시각Utc = now
        };

        var update = Builders<커뮤니티원장문서>.Update
            .Set(x => x.상태, request.상태.Trim())
            .Set(x => x.현재단계Key, Clean(request.현재단계Key))
            .Set(x => x.수정시각Utc, now)
            .Set(x => x.수정자, 변경자)
            .Set(x => x.Revision, revision)
            .Set(x => x.투영상태, 커뮤니티원장투영상태.대기)
            .Set(x => x.투영EventId, eventId)
            .Set(x => x.투영변경유형, 커뮤니티원장변경유형값.상태변경)
            .Set(x => x.투영이전상태, Clean(existing.상태))
            .Set(x => x.투영변경메모, Clean(request.메모))
            .Set(x => x.투영발생시각Utc, now)
            .Set(x => x.투영다음시도시각Utc, now.AddSeconds(5))
            .Set(x => x.투영시도횟수, 0)
            .Set(x => x.투영처리Token, null)
            .Set(x => x.투영처리시작시각Utc, null)
            .Set(x => x.투영마지막오류, null)
            .Push(x => x.상태이력, 상태이력);

        var updated = await _collection.FindOneAndUpdateAsync(
            BuildRevisionFilter(원장Id, existing.Revision, isNew: false),
            update,
            new FindOneAndUpdateOptions<커뮤니티원장문서>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);

        if (updated is null)
        {
            throw CreateConcurrencyException(원장Id);
        }

        return ToDto(updated);
    }

    public async Task<커뮤니티원장투영작업?> 다음작업확보Async(
        TimeSpan leaseTimeout,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var leaseExpiredAt = now.Subtract(leaseTimeout <= TimeSpan.Zero ? TimeSpan.FromMinutes(5) : leaseTimeout);
        var token = Guid.NewGuid().ToString("N");
        var builder = Builders<커뮤니티원장문서>.Filter;
        var due = builder.And(
            builder.In(x => x.투영상태, [커뮤니티원장투영상태.대기, 커뮤니티원장투영상태.재시도대기]),
            builder.Or(
                builder.Eq(x => x.투영다음시도시각Utc, null),
                builder.Lte(x => x.투영다음시도시각Utc, now)));
        var expired = builder.And(
            builder.Eq(x => x.투영상태, 커뮤니티원장투영상태.처리중),
            builder.Or(
                builder.Eq(x => x.투영처리시작시각Utc, null),
                builder.Lte(x => x.투영처리시작시각Utc, leaseExpiredAt)));

        var document = await _collection.FindOneAndUpdateAsync(
            builder.Or(due, expired),
            Builders<커뮤니티원장문서>.Update
                .Set(x => x.투영상태, 커뮤니티원장투영상태.처리중)
                .Set(x => x.투영처리Token, token)
                .Set(x => x.투영처리시작시각Utc, now)
                .Set(x => x.투영마지막오류, null)
                .Inc(x => x.투영시도횟수, 1),
            new FindOneAndUpdateOptions<커뮤니티원장문서>
            {
                Sort = Builders<커뮤니티원장문서>.Sort
                    .Ascending(x => x.투영다음시도시각Utc)
                    .Ascending(x => x.수정시각Utc),
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);

        if (document is null)
        {
            return null;
        }

        var stateRequest = string.Equals(document.투영변경유형, 커뮤니티원장변경유형값.상태변경, StringComparison.Ordinal)
            ? new 커뮤니티원장상태변경요청
            {
                원장Id = document.원장Id,
                상태 = document.상태,
                이전상태 = document.투영이전상태,
                현재단계Key = document.현재단계Key,
                메모 = document.투영변경메모,
                기대Revision = document.Revision
            }
            : null;

        return new 커뮤니티원장투영작업(
            ToDto(document),
            document.투영EventId ?? CreateEventId(document.원장Id, document.Revision),
            document.투영변경유형 ?? 커뮤니티원장변경유형값.저장,
            document.수정자,
            stateRequest,
            document.투영발생시각Utc ?? document.수정시각Utc,
            token,
            document.투영시도횟수);
    }

    public async Task 완료Async(
        string 원장Id,
        long revision,
        string? processingToken,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<커뮤니티원장문서>.Filter;
        var filter = builder.And(
            builder.Eq(x => x.원장Id, 원장Id),
            builder.Eq(x => x.Revision, revision));
        filter &= string.IsNullOrWhiteSpace(processingToken)
            ? builder.Ne(x => x.투영상태, 커뮤니티원장투영상태.처리중)
            : builder.Eq(x => x.투영처리Token, processingToken);

        var now = DateTime.UtcNow;
        await _collection.UpdateOneAsync(
            filter,
            Builders<커뮤니티원장문서>.Update
                .Set(x => x.투영완료Revision, revision)
                .Set(x => x.투영상태, 커뮤니티원장투영상태.완료)
                .Set(x => x.투영완료시각Utc, now)
                .Set(x => x.투영다음시도시각Utc, null)
                .Set(x => x.투영처리Token, null)
                .Set(x => x.투영처리시작시각Utc, null)
                .Set(x => x.투영마지막오류, null),
            cancellationToken: cancellationToken);
    }

    public async Task 실패Async(
        string 원장Id,
        long revision,
        string processingToken,
        string 오류,
        int 최대시도횟수,
        TimeSpan 기본재시도간격,
        CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(x =>
                x.원장Id == 원장Id
                && x.Revision == revision
                && x.투영처리Token == processingToken)
            .FirstOrDefaultAsync(cancellationToken);
        if (document is null)
        {
            return;
        }

        var attempts = document.투영시도횟수;
        var terminal = attempts >= Math.Max(1, 최대시도횟수);
        var baseSeconds = Math.Max(1, 기본재시도간격.TotalSeconds);
        var delaySeconds = Math.Min(3600, baseSeconds * Math.Pow(2, Math.Max(0, attempts - 1)));
        var now = DateTime.UtcNow;

        await _collection.UpdateOneAsync(
            x => x.원장Id == 원장Id
                 && x.Revision == revision
                 && x.투영처리Token == processingToken,
            Builders<커뮤니티원장문서>.Update
                .Set(x => x.투영상태, terminal ? 커뮤니티원장투영상태.실패 : 커뮤니티원장투영상태.재시도대기)
                .Set(x => x.투영다음시도시각Utc, terminal ? null : now.AddSeconds(delaySeconds))
                .Set(x => x.투영처리Token, null)
                .Set(x => x.투영처리시작시각Utc, null)
                .Set(x => x.투영마지막오류, Truncate(오류, 2000)),
            cancellationToken: cancellationToken);
    }

    private FilterDefinition<커뮤니티원장문서> BuildFilter(커뮤니티원장조회조건 query)
    {
        var builder = Builders<커뮤니티원장문서>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(query.커뮤니티Id))
        {
            filter &= builder.Eq(x => x.커뮤니티Id, query.커뮤니티Id.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.원장템플릿Key))
        {
            filter &= builder.Eq(x => x.원장템플릿Key, query.원장템플릿Key.Trim());
        }
        else if (query.원장템플릿Keys.Count > 0)
        {
            var templateKeys = query.원장템플릿Keys
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (templateKeys.Length > 0)
            {
                filter &= builder.In(x => x.원장템플릿Key, templateKeys);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.상태))
        {
            filter &= builder.Eq(x => x.상태, query.상태.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.참여자UserId))
        {
            filter &= builder.ElemMatch(x => x.참여자목록, participant => participant.UserId == query.참여자UserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.접근UserId))
        {
            var userId = query.접근UserId.Trim();
            filter &= builder.Or(
                builder.Eq(x => x.생성자UserId, userId),
                builder.ElemMatch(x => x.참여자목록, participant => participant.UserId == userId));
        }

        if (!string.IsNullOrWhiteSpace(query.포함원장Id))
        {
            filter &= builder.ElemMatch(x => x.포함원장목록, child => child.원장Id == query.포함원장Id.Trim());
        }
        else if (query.포함원장Ids.Count > 0)
        {
            var childIds = query.포함원장Ids
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (childIds.Length > 0)
            {
                filter &= builder.In<string>("포함원장목록.원장Id", childIds);
            }
        }

        return filter;
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

            var indexes = new[]
            {
                new CreateIndexModel<커뮤니티원장문서>(
                    Builders<커뮤니티원장문서>.IndexKeys.Ascending(x => x.원장Id),
                    new CreateIndexOptions { Unique = true, Name = "ux_ledger_id" }),
                new CreateIndexModel<커뮤니티원장문서>(
                    Builders<커뮤니티원장문서>.IndexKeys
                        .Ascending(x => x.커뮤니티Id)
                        .Ascending(x => x.원장템플릿Key)
                        .Ascending(x => x.상태)
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_ledger_scope_state" }),
                new CreateIndexModel<커뮤니티원장문서>(
                    Builders<커뮤니티원장문서>.IndexKeys
                        .Ascending("참여자목록.UserId")
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_ledger_participant" }),
                new CreateIndexModel<커뮤니티원장문서>(
                    Builders<커뮤니티원장문서>.IndexKeys
                        .Ascending(x => x.생성자UserId)
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_ledger_creator" }),
                new CreateIndexModel<커뮤니티원장문서>(
                    Builders<커뮤니티원장문서>.IndexKeys
                        .Ascending("포함원장목록.원장Id")
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_ledger_contained_ledger" }),
                new CreateIndexModel<커뮤니티원장문서>(
                    Builders<커뮤니티원장문서>.IndexKeys
                        .Ascending(x => x.투영상태)
                        .Ascending(x => x.투영다음시도시각Utc)
                        .Ascending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_ledger_projection_queue" })
            };

            await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static void Validate(커뮤니티원장저장요청 request)
    {
        if (string.IsNullOrWhiteSpace(request.커뮤니티Id)) throw new InvalidOperationException("커뮤니티Id is required.");
        if (string.IsNullOrWhiteSpace(request.원장템플릿Key)) throw new InvalidOperationException("원장템플릿Key is required.");
        if (string.IsNullOrWhiteSpace(request.제목)) throw new InvalidOperationException("제목 is required.");
        주문원장구성정책.저장요청검증(request);
    }

    private static 커뮤니티원장블록문서 ToDocument(커뮤니티원장블록Dto dto)
        => new()
        {
            BlockId = string.IsNullOrWhiteSpace(dto.BlockId) ? $"block-{Guid.NewGuid():N}" : dto.BlockId.Trim(),
            BlockType = dto.BlockType.Trim(),
            Title = dto.Title.Trim(),
            State = Clean(dto.State),
            담당자목록 = dto.담당자목록
                .Where(assignee => !string.IsNullOrWhiteSpace(assignee.UserId))
                .GroupBy(assignee => assignee.UserId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => ToDocument(group.Last()))
                .ToArray(),
            Data = NormalizeDictionary(dto.Data)
        };

    private static IReadOnlyList<커뮤니티원장블록문서> BuildBlockDocuments(
        커뮤니티원장저장요청 request,
        커뮤니티원장문서? existing)
    {
        var existingById = (existing?.블록목록 ?? [])
            .ToDictionary(block => block.BlockId, StringComparer.OrdinalIgnoreCase);
        return request.블록목록.Select(block =>
        {
            var document = ToDocument(block);
            if (!request.블록담당자명시적갱신여부
                && document.담당자목록.Count == 0
                && existingById.TryGetValue(document.BlockId, out var existingBlock))
            {
                document.담당자목록 = existingBlock.담당자목록;
            }

            return document;
        }).ToArray();
    }

    private static 커뮤니티원장블록담당자문서 ToDocument(커뮤니티원장블록담당자Dto dto)
        => new()
        {
            UserId = dto.UserId.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? "익명 참여자" : dto.DisplayName.Trim(),
            RoleLabel = string.IsNullOrWhiteSpace(dto.RoleLabel) ? "참여자" : dto.RoleLabel.Trim(),
            ResponsibilityType = CommunityLedgerBlockResponsibilityTypes.IsSupported(dto.ResponsibilityType)
                ? dto.ResponsibilityType
                : CommunityLedgerBlockResponsibilityTypes.Primary
        };

    private static 커뮤니티원장참여자문서 ToDocument(커뮤니티원장참여자Dto dto)
        => new()
        {
            UserId = Clean(dto.UserId),
            DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? "익명 참여자" : dto.DisplayName.Trim(),
            RoleLabel = string.IsNullOrWhiteSpace(dto.RoleLabel) ? "참여자" : dto.RoleLabel.Trim(),
            ParticipationState = string.IsNullOrWhiteSpace(dto.ParticipationState) ? "참여중" : dto.ParticipationState.Trim()
        };

    private static 커뮤니티포함원장참조문서 ToDocument(커뮤니티포함원장참조Dto dto)
        => new()
        {
            원장Id = dto.원장Id.Trim(),
            원장템플릿Key = dto.원장템플릿Key.Trim(),
            역할 = dto.역할.Trim(),
            관계유형 = string.IsNullOrWhiteSpace(dto.관계유형)
                ? CommunityLedgerRelationTypes.Contains
                : dto.관계유형.Trim(),
            필수여부 = dto.필수여부,
            표시순서 = dto.표시순서
        };

    private static 커뮤니티원장다이어그램문서 ToDocument(DiagramSnapshotDto dto)
        => new()
        {
            DiagramId = dto.DiagramId,
            DiagramName = dto.DiagramName,
            LedgerTemplateKey = dto.LedgerTemplateKey,
            WorkflowModeKey = dto.WorkflowModeKey,
            Nodes = dto.Nodes.Select(node => new 커뮤니티원장다이어그램노드문서
            {
                NodeId = node.NodeId,
                Kind = node.Kind,
                Title = node.Title,
                GroupLabel = node.GroupLabel,
                Description = node.Description,
                X = node.X,
                Y = node.Y,
                RelatedRoute = node.RelatedRoute,
                OrganizationReferences = (node.OrganizationReferences ?? [])
                    .Select(ToDocument)
                    .ToArray(),
                Data = NormalizeDictionary(node.Data)
            }).ToArray(),
            Edges = dto.Edges.Select(edge => new 커뮤니티원장다이어그램연결선문서
            {
                EdgeId = edge.EdgeId,
                FromNodeId = edge.FromNodeId,
                ToNodeId = edge.ToNodeId,
                Label = edge.Label,
                MeaningCode = edge.MeaningCode,
                Data = NormalizeDictionary(edge.Data)
            }).ToArray(),
            Metadata = NormalizeDictionary(dto.Metadata)
        };

    private static 커뮤니티원장Dto ToDto(커뮤니티원장문서 문서)
        => new()
        {
            원장Id = 문서.원장Id,
            커뮤니티Id = 문서.커뮤니티Id,
            원장템플릿Key = 문서.원장템플릿Key,
            제목 = 문서.제목,
            원함 = 문서.원함,
            상태 = 문서.상태,
            현재단계Key = 문서.현재단계Key,
            대상OsCode = 문서.대상OsCode,
            대상OsName = 문서.대상OsName,
            생성자UserId = 문서.생성자UserId,
            생성자표시명 = 문서.생성자표시명,
            블록목록 = 문서.블록목록.Select(ToDto).ToArray(),
            참여자목록 = 문서.참여자목록.Select(ToDto).ToArray(),
            포함원장목록 = 문서.포함원장목록?.Select(ToDto).OrderBy(x => x.표시순서).ToArray() ?? [],
            다이어그램스냅샷 = 문서.다이어그램스냅샷 is null ? null : ToDto(문서.다이어그램스냅샷, 문서.원장Id),
            외부참조 = 문서.외부참조,
            확장속성 = 문서.확장속성,
            상태이력 = 문서.상태이력.Select(ToDto).ToArray(),
            Revision = 문서.Revision,
            투영완료Revision = 문서.투영완료Revision,
            투영상태 = 문서.Revision <= 문서.투영완료Revision
                ? 커뮤니티원장투영상태.완료
                : 문서.투영상태,
            투영EventId = 문서.투영EventId,
            투영마지막오류 = 문서.투영마지막오류,
            생성시각Utc = 문서.생성시각Utc,
            수정시각Utc = 문서.수정시각Utc
        };

    private static 커뮤니티원장블록Dto ToDto(커뮤니티원장블록문서 문서)
        => new()
        {
            BlockId = 문서.BlockId,
            BlockType = 문서.BlockType,
            Title = 문서.Title,
            State = 문서.State,
            담당자목록 = 문서.담당자목록.Select(ToDto).ToArray(),
            Data = 문서.Data
        };

    private static 커뮤니티원장블록담당자Dto ToDto(커뮤니티원장블록담당자문서 문서)
        => new()
        {
            UserId = 문서.UserId,
            DisplayName = 문서.DisplayName,
            RoleLabel = 문서.RoleLabel,
            ResponsibilityType = 문서.ResponsibilityType
        };

    private static 커뮤니티원장참여자Dto ToDto(커뮤니티원장참여자문서 문서)
        => new()
        {
            UserId = 문서.UserId,
            DisplayName = 문서.DisplayName,
            RoleLabel = 문서.RoleLabel,
            ParticipationState = 문서.ParticipationState
        };

    private static 커뮤니티포함원장참조Dto ToDto(커뮤니티포함원장참조문서 문서)
        => new()
        {
            원장Id = 문서.원장Id,
            원장템플릿Key = 문서.원장템플릿Key,
            역할 = 문서.역할,
            관계유형 = string.IsNullOrWhiteSpace(문서.관계유형)
                ? CommunityLedgerRelationTypes.Contains
                : 문서.관계유형,
            필수여부 = 문서.필수여부,
            표시순서 = 문서.표시순서
        };

    private static 커뮤니티원장상태이력Dto ToDto(커뮤니티원장상태이력문서 문서)
        => new()
        {
            EventId = 문서.EventId,
            상태 = 문서.상태,
            이전상태 = 문서.이전상태,
            현재단계Key = 문서.현재단계Key,
            메모 = 문서.메모,
            변경자 = 문서.변경자,
            변경시각Utc = 문서.변경시각Utc
        };

    private static DiagramSnapshotDto ToDto(커뮤니티원장다이어그램문서 문서, string 원장Id)
        => new()
        {
            DiagramId = 문서.DiagramId,
            DiagramName = 문서.DiagramName,
            LedgerId = 원장Id,
            LedgerTemplateKey = 문서.LedgerTemplateKey,
            WorkflowModeKey = 문서.WorkflowModeKey,
            Nodes = 문서.Nodes.Select(node => new DiagramNodeDto
            {
                NodeId = node.NodeId,
                Kind = node.Kind,
                Title = node.Title,
                GroupLabel = node.GroupLabel,
                Description = node.Description,
                X = node.X,
                Y = node.Y,
                RelatedRoute = node.RelatedRoute,
                OrganizationReferences = (node.OrganizationReferences ?? [])
                    .Select(ToDto)
                    .ToArray(),
                Data = node.Data
            }).ToArray(),
            Edges = 문서.Edges.Select(edge => new DiagramEdgeDto
            {
                EdgeId = edge.EdgeId,
                FromNodeId = edge.FromNodeId,
                ToNodeId = edge.ToNodeId,
                Label = edge.Label,
                MeaningCode = edge.MeaningCode,
                Data = edge.Data
            }).ToArray(),
            Metadata = 문서.Metadata
        };

    private static 커뮤니티원장다이어그램업체참조문서 ToDocument(
        DiagramOrganizationReferenceDto dto)
        => new()
        {
            ReferenceId = dto.ReferenceId,
            OrganizationKey = dto.OrganizationKey,
            DisplayName = dto.DisplayName,
            RoleLabel = dto.RoleLabel,
            CountryCode = dto.CountryCode,
            OfficialWebsiteUrl = dto.OfficialWebsiteUrl,
            SourceKindCode = dto.SourceKindCode,
            SourceReferenceUrl = dto.SourceReferenceUrl,
            DirectoryStatusCode = dto.DirectoryStatusCode,
            PlatformRelationshipStatusCode = dto.PlatformRelationshipStatusCode,
            CompanySourceVerificationStatusCode = dto.CompanySourceVerificationStatusCode,
            RegulatoryVerificationStatusCode = dto.RegulatoryVerificationStatusCode,
            IsPlatformPartner = dto.IsPlatformPartner,
            CanBeSelectedForOperations = dto.CanBeSelectedForOperations,
            CapabilityCodes = (dto.CapabilityCodes ?? []).ToArray()
        };

    private static DiagramOrganizationReferenceDto ToDto(
        커뮤니티원장다이어그램업체참조문서 문서)
        => new()
        {
            ReferenceId = 문서.ReferenceId,
            OrganizationKey = 문서.OrganizationKey,
            DisplayName = 문서.DisplayName,
            RoleLabel = 문서.RoleLabel,
            CountryCode = 문서.CountryCode,
            OfficialWebsiteUrl = 문서.OfficialWebsiteUrl,
            SourceKindCode = 문서.SourceKindCode,
            SourceReferenceUrl = 문서.SourceReferenceUrl,
            DirectoryStatusCode = 문서.DirectoryStatusCode,
            PlatformRelationshipStatusCode = 문서.PlatformRelationshipStatusCode,
            CompanySourceVerificationStatusCode = 문서.CompanySourceVerificationStatusCode,
            RegulatoryVerificationStatusCode = 문서.RegulatoryVerificationStatusCode,
            IsPlatformPartner = 문서.IsPlatformPartner,
            CanBeSelectedForOperations = 문서.CanBeSelectedForOperations,
            CapabilityCodes = (문서.CapabilityCodes ?? []).ToArray()
        };

    private static Dictionary<string, string> NormalizeDictionary(IReadOnlyDictionary<string, string>? source)
        => source?
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key.Trim(), pair => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase)
           ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private static FilterDefinition<커뮤니티원장문서> BuildRevisionFilter(
        string 원장Id,
        long revision,
        bool isNew)
    {
        var builder = Builders<커뮤니티원장문서>.Filter;
        if (isNew)
        {
            return builder.Eq(x => x.원장Id, 원장Id);
        }

        var revisionFilter = revision == 0
            ? builder.Or(
                builder.Eq(x => x.Revision, 0),
                builder.Exists(x => x.Revision, false))
            : builder.Eq(x => x.Revision, revision);
        return builder.And(builder.Eq(x => x.원장Id, 원장Id), revisionFilter);
    }

    private static void EnsureExpectedRevision(long? expectedRevision, long actualRevision, string 원장Id)
    {
        if (expectedRevision.HasValue && expectedRevision.Value != actualRevision)
        {
            throw CreateConcurrencyException(원장Id);
        }
    }

    private static InvalidOperationException CreateConcurrencyException(string 원장Id, Exception? inner = null)
    {
        var suffix = string.IsNullOrWhiteSpace(원장Id) ? string.Empty : $" 원장Id={원장Id}";
        return new InvalidOperationException(
            $"원장의 현재 상태가 다른 요청에서 먼저 변경되었습니다. 최신 원장을 다시 조회한 뒤 재시도해야 합니다.{suffix}",
            inner);
    }

    private static string CreateEventId(string 원장Id, long revision)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{원장Id}:{revision}"));
        return $"ledger-{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }

    private static string Truncate(string? value, int maxLength)
        => string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Length <= maxLength ? value : value[..maxLength];

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class 커뮤니티원장조회조건
{
    public string? 커뮤니티Id { get; set; }
    public string? 원장템플릿Key { get; set; }
    public IReadOnlyList<string> 원장템플릿Keys { get; set; } = [];
    public string? 상태 { get; set; }
    public string? 참여자UserId { get; set; }
    public string? 접근UserId { get; set; }
    public string? 포함원장Id { get; set; }
    public IReadOnlyList<string> 포함원장Ids { get; set; } = [];
    public int Limit { get; set; } = 50;
}

public sealed class 커뮤니티원장저장요청
{
    public string? 원장Id { get; set; }
    public long? 기대Revision { get; set; }
    public string 커뮤니티Id { get; set; } = "platform";
    public string 원장템플릿Key { get; set; } = CommunityLedgerTemplateKeys.CargoTransport;
    public string 제목 { get; set; } = string.Empty;
    public string? 원함 { get; set; }
    public string? 상태 { get; set; }
    public string? 현재단계Key { get; set; }
    public string? 대상OsCode { get; set; }
    public string? 대상OsName { get; set; }
    public string? 생성자UserId { get; set; }
    public string? 생성자표시명 { get; set; }
    public IReadOnlyList<커뮤니티원장블록Dto> 블록목록 { get; set; } = [];
    public bool 블록담당자명시적갱신여부 { get; set; }
    public IReadOnlyList<커뮤니티원장참여자Dto> 참여자목록 { get; set; } = [];
    public IReadOnlyList<커뮤니티포함원장참조Dto>? 포함원장목록 { get; set; }
    public DiagramSnapshotDto? 다이어그램스냅샷 { get; set; }
    public IReadOnlyDictionary<string, string> 외부참조 { get; set; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> 확장속성 { get; set; } = new Dictionary<string, string>();
}

public sealed class 커뮤니티원장상태변경요청
{
    public string 원장Id { get; set; } = string.Empty;
    public long? 기대Revision { get; set; }
    public string 상태 { get; set; } = 커뮤니티원장상태.진행중;
    public string? 이전상태 { get; set; }
    public string? 현재단계Key { get; set; }
    public string? 메모 { get; set; }
}

public sealed class 커뮤니티원장Dto
{
    public string 원장Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public long 투영완료Revision { get; set; }
    public string 투영상태 { get; set; } = 커뮤니티원장투영상태.대기;
    public string? 투영EventId { get; set; }
    public string? 투영마지막오류 { get; set; }
    public string 커뮤니티Id { get; set; } = string.Empty;
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string? 원함 { get; set; }
    public string 상태 { get; set; } = 커뮤니티원장상태.초안;
    public string? 현재단계Key { get; set; }
    public string? 대상OsCode { get; set; }
    public string? 대상OsName { get; set; }
    public string? 생성자UserId { get; set; }
    public string 생성자표시명 { get; set; } = "익명 참여자";
    public IReadOnlyList<커뮤니티원장블록Dto> 블록목록 { get; set; } = [];
    public IReadOnlyList<커뮤니티원장참여자Dto> 참여자목록 { get; set; } = [];
    public IReadOnlyList<커뮤니티포함원장참조Dto> 포함원장목록 { get; set; } = [];
    public DiagramSnapshotDto? 다이어그램스냅샷 { get; set; }
    public IReadOnlyDictionary<string, string> 외부참조 { get; set; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> 확장속성 { get; set; } = new Dictionary<string, string>();
    public IReadOnlyList<커뮤니티원장상태이력Dto> 상태이력 { get; set; } = [];
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 커뮤니티원장블록Dto
{
    public string BlockId { get; set; } = string.Empty;
    public string BlockType { get; set; } = CommunityLedgerBlockTypes.Generic;
    public string Title { get; set; } = string.Empty;
    public string? State { get; set; }
    public IReadOnlyList<커뮤니티원장블록담당자Dto> 담당자목록 { get; set; } = [];
    public IReadOnlyDictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}

public sealed class 커뮤니티원장블록담당자Dto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "익명 참여자";
    public string RoleLabel { get; set; } = "참여자";
    public string ResponsibilityType { get; set; } = CommunityLedgerBlockResponsibilityTypes.Primary;
}

public sealed class 커뮤니티원장참여자Dto
{
    public string? UserId { get; set; }
    public string DisplayName { get; set; } = "익명 참여자";
    public string RoleLabel { get; set; } = "참여자";
    public string ParticipationState { get; set; } = "참여중";
}

public sealed class 커뮤니티포함원장참조Dto
{
    public string 원장Id { get; set; } = string.Empty;
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 역할 { get; set; } = string.Empty;
    public string 관계유형 { get; set; } = CommunityLedgerRelationTypes.Contains;
    public bool 필수여부 { get; set; }
    public int 표시순서 { get; set; }
}

public sealed class 커뮤니티원장상태이력Dto
{
    public string? EventId { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string? 이전상태 { get; set; }
    public string? 현재단계Key { get; set; }
    public string? 메모 { get; set; }
    public string 변경자 { get; set; } = "system";
    public DateTime 변경시각Utc { get; set; }
}

public static class 커뮤니티원장상태
{
    public const string 초안 = "초안";
    public const string 진행중 = "진행중";
    public const string 보류 = "보류";
    public const string 완료 = "완료";
    public const string 닫힘 = "닫힘";
}

public sealed class 커뮤니티원장문서
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string 원장Id { get; set; } = string.Empty;
    public string 커뮤니티Id { get; set; } = string.Empty;
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string? 원함 { get; set; }
    public string 상태 { get; set; } = 커뮤니티원장상태.초안;
    public string? 현재단계Key { get; set; }
    public string? 대상OsCode { get; set; }
    public string? 대상OsName { get; set; }
    public string? 생성자UserId { get; set; }
    public string 생성자표시명 { get; set; } = "익명 참여자";
    public IReadOnlyList<커뮤니티원장블록문서> 블록목록 { get; set; } = [];
    public IReadOnlyList<커뮤니티원장참여자문서> 참여자목록 { get; set; } = [];
    public IReadOnlyList<커뮤니티포함원장참조문서> 포함원장목록 { get; set; } = [];
    public 커뮤니티원장다이어그램문서? 다이어그램스냅샷 { get; set; }
    public Dictionary<string, string> 외부참조 { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> 확장속성 { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<커뮤니티원장상태이력문서> 상태이력 { get; set; } = [];
    public long Revision { get; set; }
    public long 투영완료Revision { get; set; }
    public string 투영상태 { get; set; } = 커뮤니티원장투영상태.대기;
    public string? 투영EventId { get; set; }
    public string? 투영변경유형 { get; set; }
    public string? 투영이전상태 { get; set; }
    public string? 투영변경메모 { get; set; }
    public DateTime? 투영발생시각Utc { get; set; }
    public int 투영시도횟수 { get; set; }
    public DateTime? 투영다음시도시각Utc { get; set; }
    public string? 투영처리Token { get; set; }
    public DateTime? 투영처리시작시각Utc { get; set; }
    public DateTime? 투영완료시각Utc { get; set; }
    public string? 투영마지막오류 { get; set; }
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
    public string 수정자 { get; set; } = "system";
}

public sealed class 커뮤니티원장블록문서
{
    public string BlockId { get; set; } = string.Empty;
    public string BlockType { get; set; } = CommunityLedgerBlockTypes.Generic;
    public string Title { get; set; } = string.Empty;
    public string? State { get; set; }
    public IReadOnlyList<커뮤니티원장블록담당자문서> 담당자목록 { get; set; } = [];
    public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class 커뮤니티원장블록담당자문서
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "익명 참여자";
    public string RoleLabel { get; set; } = "참여자";
    public string ResponsibilityType { get; set; } = CommunityLedgerBlockResponsibilityTypes.Primary;
}

public sealed class 커뮤니티원장참여자문서
{
    public string? UserId { get; set; }
    public string DisplayName { get; set; } = "익명 참여자";
    public string RoleLabel { get; set; } = "참여자";
    public string ParticipationState { get; set; } = "참여중";
}

public sealed class 커뮤니티포함원장참조문서
{
    public string 원장Id { get; set; } = string.Empty;
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 역할 { get; set; } = string.Empty;
    public string 관계유형 { get; set; } = CommunityLedgerRelationTypes.Contains;
    public bool 필수여부 { get; set; }
    public int 표시순서 { get; set; }
}

public sealed class 커뮤니티원장상태이력문서
{
    public string? EventId { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string? 이전상태 { get; set; }
    public string? 현재단계Key { get; set; }
    public string? 메모 { get; set; }
    public string 변경자 { get; set; } = "system";
    public DateTime 변경시각Utc { get; set; }
}

public sealed class 커뮤니티원장다이어그램문서
{
    public string DiagramId { get; set; } = string.Empty;
    public string DiagramName { get; set; } = string.Empty;
    public string? LedgerTemplateKey { get; set; }
    public string? WorkflowModeKey { get; set; }
    public IReadOnlyList<커뮤니티원장다이어그램노드문서> Nodes { get; set; } = [];
    public IReadOnlyList<커뮤니티원장다이어그램연결선문서> Edges { get; set; } = [];
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class 커뮤니티원장다이어그램노드문서
{
    public string NodeId { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? GroupLabel { get; set; }
    public string? Description { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string? RelatedRoute { get; set; }
    public IReadOnlyList<커뮤니티원장다이어그램업체참조문서> OrganizationReferences { get; set; } = [];
    public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class 커뮤니티원장다이어그램업체참조문서
{
    public string ReferenceId { get; set; } = string.Empty;
    public string OrganizationKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RoleLabel { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "ZZ";
    public string OfficialWebsiteUrl { get; set; } = string.Empty;
    public string SourceKindCode { get; set; } = DiagramOrganizationSourceKindCodes.ManualResearch;
    public string SourceReferenceUrl { get; set; } = string.Empty;
    public string DirectoryStatusCode { get; set; } = string.Empty;
    public string PlatformRelationshipStatusCode { get; set; } = string.Empty;
    public string CompanySourceVerificationStatusCode { get; set; } =
        DiagramOrganizationVerificationStatusCodes.VerificationRequired;
    public string RegulatoryVerificationStatusCode { get; set; } = string.Empty;
    public bool IsPlatformPartner { get; set; }
    public bool CanBeSelectedForOperations { get; set; }
    public IReadOnlyList<string> CapabilityCodes { get; set; } = [];
}

public sealed class 커뮤니티원장다이어그램연결선문서
{
    public string EdgeId { get; set; } = string.Empty;
    public string FromNodeId { get; set; } = string.Empty;
    public string ToNodeId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? MeaningCode { get; set; }
    public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
