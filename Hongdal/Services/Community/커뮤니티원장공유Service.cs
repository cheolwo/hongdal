using FluentResults;
using Hongdal.Contracts.Common.Community;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 홍달.Services.Options;
using 홍달.Services.Versioning;

namespace Hongdal.Services.Community;

public interface I커뮤니티원장공유정책저장소
{
    Task<커뮤니티원장공유정책?> 조회Async(string 원장Id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<커뮤니티원장공유정책>> 공개목록조회Async(CancellationToken cancellationToken = default);
    Task<커뮤니티원장공유정책> 저장Async(
        커뮤니티원장공유정책 policy,
        long? 기대Revision,
        CancellationToken cancellationToken = default);
}

public sealed class Mongo커뮤니티원장공유정책저장소 : I커뮤니티원장공유정책저장소
{
    private const string CollectionName = "community_ledger_sharing_policies";
    private readonly IMongoCollection<커뮤니티원장공유정책문서> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public Mongo커뮤니티원장공유정책저장소(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        _collection = mongoClient
            .GetDatabase(options.Value.Database)
            .GetCollection<커뮤니티원장공유정책문서>(CollectionName);
    }

    public async Task<커뮤니티원장공유정책?> 조회Async(string 원장Id, CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var document = await _collection.Find(x => x.원장Id == 원장Id.Trim()).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToModel(document);
    }

    public async Task<IReadOnlyList<커뮤니티원장공유정책>> 공개목록조회Async(CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var documents = await _collection
            .Find(x => x.공개범위 != 커뮤니티원장공개범위.비공개)
            .SortByDescending(x => x.수정시각Utc)
            .Limit(100)
            .ToListAsync(cancellationToken);
        return documents.Select(ToModel).ToArray();
    }

    public async Task<커뮤니티원장공유정책> 저장Async(
        커뮤니티원장공유정책 policy,
        long? 기대Revision,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var existing = await _collection.Find(x => x.원장Id == policy.원장Id).FirstOrDefaultAsync(cancellationToken);
        var actualRevision = existing?.Revision ?? 0;
        if (기대Revision.HasValue && 기대Revision.Value != actualRevision)
        {
            throw new InvalidOperationException("원장 공개 설정이 다른 요청에서 먼저 변경되었습니다. 다시 불러온 뒤 저장해 주세요.");
        }

        var document = new 커뮤니티원장공유정책문서
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            원장Id = policy.원장Id,
            소유자UserId = policy.소유자UserId,
            공개범위 = policy.공개범위,
            재사용허용여부 = policy.재사용허용여부,
            재공유허용여부 = policy.재공유허용여부,
            공개항목Key목록 = policy.공개항목Key목록.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            Revision = actualRevision + 1,
            수정시각Utc = DateTime.UtcNow
        };

        var filter = existing is null
            ? Builders<커뮤니티원장공유정책문서>.Filter.Eq(x => x.원장Id, policy.원장Id)
            : Builders<커뮤니티원장공유정책문서>.Filter.And(
                Builders<커뮤니티원장공유정책문서>.Filter.Eq(x => x.원장Id, policy.원장Id),
                Builders<커뮤니티원장공유정책문서>.Filter.Eq(x => x.Revision, actualRevision));
        var result = await _collection.ReplaceOneAsync(
            filter,
            document,
            new ReplaceOptions { IsUpsert = existing is null },
            cancellationToken);
        if (existing is not null && result.MatchedCount == 0)
        {
            throw new InvalidOperationException("원장 공개 설정이 다른 요청에서 먼저 변경되었습니다. 다시 불러온 뒤 저장해 주세요.");
        }

        return ToModel(document);
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (_indexesReady) return;
        await _indexLock.WaitAsync(cancellationToken);
        try
        {
            if (_indexesReady) return;
            await _collection.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<커뮤니티원장공유정책문서>(
                    Builders<커뮤니티원장공유정책문서>.IndexKeys.Ascending(x => x.원장Id),
                    new CreateIndexOptions { Unique = true, Name = "ux_ledger_sharing_ledger_id" }),
                new CreateIndexModel<커뮤니티원장공유정책문서>(
                    Builders<커뮤니티원장공유정책문서>.IndexKeys
                        .Ascending(x => x.공개범위)
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_ledger_sharing_scope" })
            ], cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static 커뮤니티원장공유정책 ToModel(커뮤니티원장공유정책문서 document)
        => new()
        {
            원장Id = document.원장Id,
            소유자UserId = document.소유자UserId,
            공개범위 = document.공개범위,
            재사용허용여부 = document.재사용허용여부,
            재공유허용여부 = document.재공유허용여부,
            공개항목Key목록 = document.공개항목Key목록,
            Revision = document.Revision,
            수정시각Utc = document.수정시각Utc
        };
}

public interface I커뮤니티원장공유Service
{
    Task<Result<커뮤니티원장공개설정Response>> 설정조회Async(string 원장Id, string? 사용자UserId, CancellationToken cancellationToken);
    Task<Result<커뮤니티원장공개설정Response>> 설정변경Async(string 원장Id, 커뮤니티원장공개설정변경Request request, string? 사용자UserId, CancellationToken cancellationToken);
    Task<Result<커뮤니티원장재사용Response>> 재사용Async(string 원장Id, 커뮤니티원장재사용Request request, string? 사용자UserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>> 공유원장목록조회Async(string? 사용자UserId, string? 업무분류, CancellationToken cancellationToken);
    Task<커뮤니티원장공유접근판정> 접근판정Async(커뮤니티원장Dto 원장, string? 사용자UserId, CancellationToken cancellationToken);
}

public sealed class 커뮤니티원장공유Service : I커뮤니티원장공유Service
{
    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly I커뮤니티원장공유정책저장소 _정책저장소;
    private readonly IVersionFeatureFlagService _featureFlagService;

    public 커뮤니티원장공유Service(
        I커뮤니티원장저장소 원장저장소,
        I커뮤니티원장공유정책저장소 정책저장소,
        IVersionFeatureFlagService featureFlagService)
    {
        _원장저장소 = 원장저장소;
        _정책저장소 = 정책저장소;
        _featureFlagService = featureFlagService;
    }

    public async Task<Result<커뮤니티원장공개설정Response>> 설정조회Async(
        string 원장Id,
        string? 사용자UserId,
        CancellationToken cancellationToken)
    {
        var ledger = await _원장저장소.원장조회Async(원장Id, cancellationToken);
        if (ledger is null) return Fail<커뮤니티원장공개설정Response>("원장을 찾을 수 없습니다.", 404);
        if (!IsOwner(ledger, 사용자UserId)) return Fail<커뮤니티원장공개설정Response>("원장 생성자만 공개 설정을 변경할 수 있습니다.", 403);
        var policy = await _정책저장소.조회Async(ledger.원장Id, cancellationToken) ?? 커뮤니티원장공유정책.Private(ledger);
        return Result.Ok(ToSettings(ledger, policy, true));
    }

    public async Task<Result<커뮤니티원장공개설정Response>> 설정변경Async(
        string 원장Id,
        커뮤니티원장공개설정변경Request request,
        string? 사용자UserId,
        CancellationToken cancellationToken)
    {
        var ledger = await _원장저장소.원장조회Async(원장Id, cancellationToken);
        if (ledger is null) return Fail<커뮤니티원장공개설정Response>("원장을 찾을 수 없습니다.", 404);
        if (!IsOwner(ledger, 사용자UserId)) return Fail<커뮤니티원장공개설정Response>("원장 생성자만 공개 설정을 변경할 수 있습니다.", 403);

        var scope = NormalizeScope(request.공개범위);
        var availableKeys = BuildItems(ledger, []).Select(x => x.항목Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var publicKeys = scope == 커뮤니티원장공개범위.비공개
            ? []
            : request.공개항목Key목록.Where(availableKeys.Contains).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var saved = await _정책저장소.저장Async(
            new 커뮤니티원장공유정책
            {
                원장Id = ledger.원장Id,
                소유자UserId = ledger.생성자UserId ?? 사용자UserId!,
                공개범위 = scope,
                재사용허용여부 = scope != 커뮤니티원장공개범위.비공개 && request.재사용허용여부,
                재공유허용여부 = scope != 커뮤니티원장공개범위.비공개 && request.재공유허용여부,
                공개항목Key목록 = publicKeys
            },
            request.기대Revision,
            cancellationToken);
        return Result.Ok(ToSettings(ledger, saved, true));
    }

    public async Task<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>> 공유원장목록조회Async(
        string? 사용자UserId,
        string? 업무분류,
        CancellationToken cancellationToken)
    {
        var requested = string.IsNullOrWhiteSpace(업무분류) ? null : CommunityWorkClassificationCatalog.FindByWorkflowTag(업무분류);
        var policies = await _정책저장소.공개목록조회Async(cancellationToken);
        var ledgers = await Task.WhenAll(policies.Select(x => _원장저장소.원장조회Async(x.원장Id, cancellationToken)));

        return ledgers.Zip(policies)
            .Where(x => x.First is not null
                        && x.Second.재공유허용여부
                        && CanPubliclyAccess(x.Second, 사용자UserId))
            .Select(x => (Ledger: x.First!, Policy: x.Second, Classification: CommunityWorkClassificationCatalog.FindByLedgerTemplate(x.First!.원장템플릿Key)))
            .Where(x => x.Classification is not null
                        && _featureFlagService.IsEnabled(x.Classification.FeatureFlagKey)
                        && (requested is null || string.Equals(requested.Code, x.Classification.Code, StringComparison.OrdinalIgnoreCase)))
            .Select(x => ToChoice(x.Ledger, x.Classification!, x.Policy, 사용자UserId))
            .ToArray();
    }

    public async Task<Result<커뮤니티원장재사용Response>> 재사용Async(
        string 원장Id,
        커뮤니티원장재사용Request request,
        string? 사용자UserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(사용자UserId))
        {
            return Fail<커뮤니티원장재사용Response>("원장을 재사용하려면 로그인이 필요합니다.", StatusCodes.Status401Unauthorized);
        }

        var source = await _원장저장소.원장조회Async(원장Id, cancellationToken);
        if (source is null)
        {
            return Fail<커뮤니티원장재사용Response>("재사용할 원장을 찾을 수 없습니다.", StatusCodes.Status404NotFound);
        }

        var access = await 접근판정Async(source, 사용자UserId, cancellationToken);
        if (!access.재사용가능)
        {
            return Fail<커뮤니티원장재사용Response>("생성자가 재사용을 허용한 공개 원장만 가져올 수 있습니다.", StatusCodes.Status403Forbidden);
        }

        var publicKeys = access.정책.공개항목Key목록;
        var template = CommunityLedgerTemplateCatalog.Find(source.원장템플릿Key);
        var sourceTitle = IsPublic(publicKeys, 커뮤니티원장공개항목Key.제목)
            ? source.제목
            : template.DisplayName;
        var newLedgerId = $"ledger-{Guid.NewGuid():N}";
        var title = string.IsNullOrWhiteSpace(request.새제목)
            ? $"{sourceTitle} 사본"
            : request.새제목.Trim();
        if (title.Length > 160) title = title[..160];

        var saved = await _원장저장소.원장저장Async(
            new 커뮤니티원장저장요청
            {
                원장Id = newLedgerId,
                커뮤니티Id = source.커뮤니티Id,
                원장템플릿Key = source.원장템플릿Key,
                제목 = title,
                상태 = 커뮤니티원장상태.초안,
                대상OsCode = source.대상OsCode,
                대상OsName = source.대상OsName,
                생성자UserId = 사용자UserId.Trim(),
                생성자표시명 = "커뮤니티 사용자",
                블록목록 = BuildReusableBlocks(source, publicKeys),
                참여자목록 = [],
                포함원장목록 = [],
                다이어그램스냅샷 = IsPublic(publicKeys, 커뮤니티원장공개항목Key.다이어그램구조)
                    ? ClonePublicDiagram(source.다이어그램스냅샷, newLedgerId)
                    : null,
                외부참조 = new Dictionary<string, string>
                {
                    ["재사용출처원장Id"] = source.원장Id
                }
            },
            사용자UserId,
            cancellationToken);

        return Result.Ok(new 커뮤니티원장재사용Response
        {
            원장Id = saved.원장Id,
            원장템플릿Key = saved.원장템플릿Key,
            제목 = saved.제목,
            출처원장Id = source.원장Id
        });
    }

    public async Task<커뮤니티원장공유접근판정> 접근판정Async(
        커뮤니티원장Dto 원장,
        string? 사용자UserId,
        CancellationToken cancellationToken)
    {
        var direct = HasDirectAccess(원장, 사용자UserId);
        var policy = await _정책저장소.조회Async(원장.원장Id, cancellationToken) ?? 커뮤니티원장공유정책.Private(원장);
        var publicAccess = CanPubliclyAccess(policy, 사용자UserId);
        return new(direct, publicAccess, direct || (publicAccess && policy.재사용허용여부), direct || (publicAccess && policy.재공유허용여부), policy);
    }

    private static PlatformCommunityPostLedgerChoiceResponse ToChoice(
        커뮤니티원장Dto ledger,
        CommunityWorkClassificationResponse classification,
        커뮤니티원장공유정책 policy,
        string? userId)
    {
        var direct = HasDirectAccess(ledger, userId);
        var titleVisible = direct || policy.공개항목Key목록.Contains(커뮤니티원장공개항목Key.제목, StringComparer.OrdinalIgnoreCase);
        var stateVisible = direct || policy.공개항목Key목록.Contains(커뮤니티원장공개항목Key.상태, StringComparer.OrdinalIgnoreCase);
        return new()
        {
            원장Id = ledger.원장Id,
            원장템플릿Key = ledger.원장템플릿Key,
            원장템플릿명 = CommunityLedgerTemplateCatalog.Find(ledger.원장템플릿Key).DisplayName,
            제목 = titleVisible ? ledger.제목 : CommunityLedgerTemplateCatalog.Find(ledger.원장템플릿Key).DisplayName,
            상태 = stateVisible ? ledger.상태 : "공개 원장",
            현재단계 = direct ? ledger.현재단계Key ?? string.Empty : string.Empty,
            업무분류명 = classification.DisplayName,
            WorkflowTag = classification.WorkflowTag,
            내가만든원장 = IsOwner(ledger, userId),
            내접근원장여부 = direct,
            커뮤니티공유여부 = true,
            재사용허용여부 = policy.재사용허용여부,
            재공유허용여부 = policy.재공유허용여부,
            참여역할 = direct ? ResolveRole(ledger, userId) : "공개 참여자",
            수정시각Utc = ledger.수정시각Utc
        };
    }

    internal static IReadOnlyList<커뮤니티원장공개항목Response> BuildItems(커뮤니티원장Dto ledger, IReadOnlyCollection<string> publicKeys)
    {
        var items = new List<커뮤니티원장공개항목Response>
        {
            Item(커뮤니티원장공개항목Key.제목, "원장 제목", "요약", publicKeys),
            Item(커뮤니티원장공개항목Key.상태, "원장 상태", "요약", publicKeys),
            Item(커뮤니티원장공개항목Key.현재단계, "현재 단계", "요약", publicKeys),
            Item(커뮤니티원장공개항목Key.다이어그램구조, "다이어그램 구조", "다이어그램", publicKeys)
        };
        foreach (var block in ledger.블록목록)
        {
            items.Add(Item(커뮤니티원장공개항목Key.블록제목(block.BlockId), $"{block.Title} 블록", "블록", publicKeys));
            if (!string.IsNullOrWhiteSpace(block.State))
            {
                items.Add(Item(커뮤니티원장공개항목Key.블록상태(block.BlockId), $"{block.Title} 상태", "블록", publicKeys));
            }
            items.AddRange(block.Data.Keys.Select(key => Item(
                커뮤니티원장공개항목Key.블록Data(block.BlockId, key),
                $"{block.Title} · {key}",
                "블록 세부 항목",
                publicKeys)));
        }
        return items;
    }

    private static IReadOnlyList<커뮤니티원장블록Dto> BuildReusableBlocks(
        커뮤니티원장Dto ledger,
        IReadOnlyList<string> publicKeys)
        => ledger.블록목록
            .Select(block =>
            {
                var titleVisible = IsPublic(publicKeys, 커뮤니티원장공개항목Key.블록제목(block.BlockId));
                var stateVisible = IsPublic(publicKeys, 커뮤니티원장공개항목Key.블록상태(block.BlockId));
                var data = block.Data
                    .Where(item => IsPublic(publicKeys, 커뮤니티원장공개항목Key.블록Data(block.BlockId, item.Key)))
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
                return new
                {
                    HasPublicValue = titleVisible || stateVisible || data.Count > 0,
                    Block = new 커뮤니티원장블록Dto
                    {
                        BlockId = block.BlockId,
                        BlockType = block.BlockType,
                        Title = titleVisible ? block.Title : "공개 블록",
                        State = stateVisible ? block.State : null,
                        Data = data
                    }
                };
            })
            .Where(item => item.HasPublicValue)
            .Select(item => item.Block)
            .ToArray();

    private static DiagramSnapshotDto? ClonePublicDiagram(DiagramSnapshotDto? diagram, string newLedgerId)
    {
        if (diagram is null) return null;
        return new DiagramSnapshotDto
        {
            DiagramId = $"diagram-{Guid.NewGuid():N}",
            DiagramName = diagram.DiagramName,
            LedgerId = newLedgerId,
            LedgerTemplateKey = diagram.LedgerTemplateKey,
            WorkflowModeKey = diagram.WorkflowModeKey,
            Nodes = diagram.Nodes.Select(node => new DiagramNodeDto
            {
                NodeId = node.NodeId,
                Kind = node.Kind,
                Title = node.Title,
                GroupLabel = node.GroupLabel,
                X = node.X,
                Y = node.Y
            }).ToArray(),
            Edges = diagram.Edges.Select(edge => new DiagramEdgeDto
            {
                EdgeId = edge.EdgeId,
                FromNodeId = edge.FromNodeId,
                ToNodeId = edge.ToNodeId,
                Label = edge.Label,
                MeaningCode = edge.MeaningCode
            }).ToArray()
        };
    }

    private static 커뮤니티원장공개항목Response Item(string key, string name, string type, IReadOnlyCollection<string> publicKeys)
        => new() { 항목Key = key, 표시명 = name, 항목유형 = type, 공개여부 = publicKeys.Contains(key, StringComparer.OrdinalIgnoreCase) };

    private static 커뮤니티원장공개설정Response ToSettings(커뮤니티원장Dto ledger, 커뮤니티원장공유정책 policy, bool editable)
        => new()
        {
            원장Id = ledger.원장Id,
            공개범위 = policy.공개범위,
            재사용허용여부 = policy.재사용허용여부,
            재공유허용여부 = policy.재공유허용여부,
            수정가능여부 = editable,
            Revision = policy.Revision,
            수정시각Utc = policy.수정시각Utc,
            항목목록 = BuildItems(ledger, policy.공개항목Key목록)
        };

    private static string NormalizeScope(string? scope)
        => scope switch
        {
            커뮤니티원장공개범위.커뮤니티 => 커뮤니티원장공개범위.커뮤니티,
            커뮤니티원장공개범위.전체공개 => 커뮤니티원장공개범위.전체공개,
            _ => 커뮤니티원장공개범위.비공개
        };

    internal static bool HasDirectAccess(커뮤니티원장Dto ledger, string? userId)
        => !string.IsNullOrWhiteSpace(userId)
           && (IsOwner(ledger, userId)
               || ledger.참여자목록.Any(x => string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase)));

    private static bool CanPubliclyAccess(커뮤니티원장공유정책 policy, string? userId)
        => string.Equals(policy.공개범위, 커뮤니티원장공개범위.전체공개, StringComparison.OrdinalIgnoreCase)
           || (!string.IsNullOrWhiteSpace(userId)
               && string.Equals(policy.공개범위, 커뮤니티원장공개범위.커뮤니티, StringComparison.OrdinalIgnoreCase));

    private static bool IsPublic(IReadOnlyList<string> publicKeys, string key)
        => publicKeys.Contains(key, StringComparer.OrdinalIgnoreCase);

    private static bool IsOwner(커뮤니티원장Dto ledger, string? userId)
        => !string.IsNullOrWhiteSpace(userId)
           && string.Equals(ledger.생성자UserId, userId, StringComparison.OrdinalIgnoreCase);

    private static string ResolveRole(커뮤니티원장Dto ledger, string? userId)
        => IsOwner(ledger, userId)
            ? "생성자"
            : ledger.참여자목록.FirstOrDefault(x => string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase))?.RoleLabel ?? "참여자";

    private static Result<T> Fail<T>(string message, int statusCode)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", statusCode));
}

public sealed class 커뮤니티원장공유정책
{
    public string 원장Id { get; set; } = string.Empty;
    public string 소유자UserId { get; set; } = string.Empty;
    public string 공개범위 { get; set; } = 커뮤니티원장공개범위.비공개;
    public bool 재사용허용여부 { get; set; }
    public bool 재공유허용여부 { get; set; }
    public IReadOnlyList<string> 공개항목Key목록 { get; set; } = [];
    public long Revision { get; set; }
    public DateTime? 수정시각Utc { get; set; }

    public static 커뮤니티원장공유정책 Private(커뮤니티원장Dto ledger)
        => new() { 원장Id = ledger.원장Id, 소유자UserId = ledger.생성자UserId ?? string.Empty };
}

public sealed record 커뮤니티원장공유접근판정(
    bool 직접접근가능,
    bool 공개조회가능,
    bool 재사용가능,
    bool 재공유가능,
    커뮤니티원장공유정책 정책);

internal sealed class 커뮤니티원장공유정책문서
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string 원장Id { get; set; } = string.Empty;
    public string 소유자UserId { get; set; } = string.Empty;
    public string 공개범위 { get; set; } = 커뮤니티원장공개범위.비공개;
    public bool 재사용허용여부 { get; set; }
    public bool 재공유허용여부 { get; set; }
    public IReadOnlyList<string> 공개항목Key목록 { get; set; } = [];
    public long Revision { get; set; }
    public DateTime 수정시각Utc { get; set; }
}
