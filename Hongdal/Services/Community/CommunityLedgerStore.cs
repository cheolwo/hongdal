using Hongdal.Contracts.Common.Community;
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

public sealed class Mongo커뮤니티원장저장소 : I커뮤니티원장저장소
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

        var 문서 = new 커뮤니티원장문서
        {
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
            블록목록 = request.블록목록.Select(ToDocument).ToArray(),
            참여자목록 = request.참여자목록.Select(ToDocument).ToArray(),
            다이어그램스냅샷 = request.다이어그램스냅샷 is null ? null : ToDocument(request.다이어그램스냅샷),
            외부참조 = NormalizeDictionary(request.외부참조),
            확장속성 = NormalizeDictionary(request.확장속성),
            생성시각Utc = existing?.생성시각Utc ?? now,
            수정시각Utc = now,
            수정자 = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim()
        };

        문서.상태이력 = existing?.상태이력?.ToList() ?? [];
        if (existing is null || !string.Equals(existing.상태, 문서.상태, StringComparison.OrdinalIgnoreCase))
        {
            문서.상태이력.Add(new 커뮤니티원장상태이력문서
            {
                상태 = 문서.상태,
                메모 = existing is null ? "원장 생성" : "원장 저장 중 상태 변경",
                변경자 = 문서.수정자,
                변경시각Utc = now
            });
        }

        await _collection.ReplaceOneAsync(
            x => x.원장Id == 원장Id,
            문서,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

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
        var 상태이력 = new 커뮤니티원장상태이력문서
        {
            상태 = request.상태.Trim(),
            이전상태 = Clean(request.이전상태),
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
            .Push(x => x.상태이력, 상태이력);

        var updated = await _collection.FindOneAndUpdateAsync(
            x => x.원장Id == request.원장Id.Trim(),
            update,
            new FindOneAndUpdateOptions<커뮤니티원장문서>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);

        return updated is null ? null : ToDto(updated);
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

        if (!string.IsNullOrWhiteSpace(query.상태))
        {
            filter &= builder.Eq(x => x.상태, query.상태.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.참여자UserId))
        {
            filter &= builder.ElemMatch(x => x.참여자목록, participant => participant.UserId == query.참여자UserId.Trim());
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
                    new CreateIndexOptions { Name = "ix_ledger_participant" })
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
    }

    private static 커뮤니티원장블록문서 ToDocument(커뮤니티원장블록Dto dto)
        => new()
        {
            BlockId = string.IsNullOrWhiteSpace(dto.BlockId) ? $"block-{Guid.NewGuid():N}" : dto.BlockId.Trim(),
            BlockType = dto.BlockType.Trim(),
            Title = dto.Title.Trim(),
            State = Clean(dto.State),
            Data = NormalizeDictionary(dto.Data)
        };

    private static 커뮤니티원장참여자문서 ToDocument(커뮤니티원장참여자Dto dto)
        => new()
        {
            UserId = Clean(dto.UserId),
            DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? "익명 참여자" : dto.DisplayName.Trim(),
            RoleLabel = string.IsNullOrWhiteSpace(dto.RoleLabel) ? "참여자" : dto.RoleLabel.Trim(),
            ParticipationState = string.IsNullOrWhiteSpace(dto.ParticipationState) ? "참여중" : dto.ParticipationState.Trim()
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
            다이어그램스냅샷 = 문서.다이어그램스냅샷 is null ? null : ToDto(문서.다이어그램스냅샷, 문서.원장Id),
            외부참조 = 문서.외부참조,
            확장속성 = 문서.확장속성,
            상태이력 = 문서.상태이력.Select(ToDto).ToArray(),
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
            Data = 문서.Data
        };

    private static 커뮤니티원장참여자Dto ToDto(커뮤니티원장참여자문서 문서)
        => new()
        {
            UserId = 문서.UserId,
            DisplayName = 문서.DisplayName,
            RoleLabel = 문서.RoleLabel,
            ParticipationState = 문서.ParticipationState
        };

    private static 커뮤니티원장상태이력Dto ToDto(커뮤니티원장상태이력문서 문서)
        => new()
        {
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

    private static Dictionary<string, string> NormalizeDictionary(IReadOnlyDictionary<string, string>? source)
        => source?
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key.Trim(), pair => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase)
           ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class 커뮤니티원장조회조건
{
    public string? 커뮤니티Id { get; set; }
    public string? 원장템플릿Key { get; set; }
    public string? 상태 { get; set; }
    public string? 참여자UserId { get; set; }
    public int Limit { get; set; } = 50;
}

public sealed class 커뮤니티원장저장요청
{
    public string? 원장Id { get; set; }
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
    public IReadOnlyList<커뮤니티원장참여자Dto> 참여자목록 { get; set; } = [];
    public DiagramSnapshotDto? 다이어그램스냅샷 { get; set; }
    public IReadOnlyDictionary<string, string> 외부참조 { get; set; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> 확장속성 { get; set; } = new Dictionary<string, string>();
}

public sealed class 커뮤니티원장상태변경요청
{
    public string 원장Id { get; set; } = string.Empty;
    public string 상태 { get; set; } = 커뮤니티원장상태.진행중;
    public string? 이전상태 { get; set; }
    public string? 현재단계Key { get; set; }
    public string? 메모 { get; set; }
}

public sealed class 커뮤니티원장Dto
{
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
    public IReadOnlyList<커뮤니티원장블록Dto> 블록목록 { get; set; } = [];
    public IReadOnlyList<커뮤니티원장참여자Dto> 참여자목록 { get; set; } = [];
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
    public IReadOnlyDictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}

public sealed class 커뮤니티원장참여자Dto
{
    public string? UserId { get; set; }
    public string DisplayName { get; set; } = "익명 참여자";
    public string RoleLabel { get; set; } = "참여자";
    public string ParticipationState { get; set; } = "참여중";
}

public sealed class 커뮤니티원장상태이력Dto
{
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
    public 커뮤니티원장다이어그램문서? 다이어그램스냅샷 { get; set; }
    public Dictionary<string, string> 외부참조 { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> 확장속성 { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<커뮤니티원장상태이력문서> 상태이력 { get; set; } = [];
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
    public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class 커뮤니티원장참여자문서
{
    public string? UserId { get; set; }
    public string DisplayName { get; set; } = "익명 참여자";
    public string RoleLabel { get; set; } = "참여자";
    public string ParticipationState { get; set; } = "참여중";
}

public sealed class 커뮤니티원장상태이력문서
{
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
    public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
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
