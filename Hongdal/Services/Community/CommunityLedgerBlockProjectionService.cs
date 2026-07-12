using System.Text.Json;
using Hongdal.Contracts.Common.Community;
using Hongdal.Domain.Community;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Services.Community;

public interface I커뮤니티원장블록관계투영Service
{
    Task 갱신Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default);
}

public sealed class 커뮤니티원장블록관계투영Service : I커뮤니티원장블록관계투영Service
{
    private readonly HongdalContext _db;

    public 커뮤니티원장블록관계투영Service(HongdalContext db)
    {
        _db = db;
    }

    public async Task 갱신Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(원장.원장Id))
        {
            throw new InvalidOperationException("커뮤니티 원장 투영에는 원장Id가 필요합니다.");
        }

        var 원장Id = 원장.원장Id.Trim();
        await _db.커뮤니티원장블록관계투영
            .Where(x => x.커뮤니티원장Id == 원장Id)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.커뮤니티원장블록투영
            .Where(x => x.커뮤니티원장Id == 원장Id)
            .ExecuteDeleteAsync(cancellationToken);

        var 투영 = 커뮤니티원장블록관계투영Builder.생성(원장);
        if (투영.블록목록.Count == 0)
        {
            return;
        }

        await _db.커뮤니티원장블록투영.AddRangeAsync(투영.블록목록, cancellationToken);
        if (투영.관계목록.Count > 0)
        {
            await _db.커뮤니티원장블록관계투영.AddRangeAsync(투영.관계목록, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}

public static class 커뮤니티원장블록관계투영Builder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static 커뮤니티원장블록관계투영결과 생성(커뮤니티원장Dto 원장)
    {
        if (string.IsNullOrWhiteSpace(원장.원장Id))
        {
            throw new InvalidOperationException("커뮤니티 원장 투영에는 원장Id가 필요합니다.");
        }

        var now = DateTime.UtcNow;
        var blockMap = new Dictionary<string, 커뮤니티원장블록투영>(StringComparer.OrdinalIgnoreCase);
        var nodeToBlockId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sortOrder = 0;

        foreach (var block in 원장.블록목록)
        {
            var blockId = Clean(block.BlockId) ?? $"block-{sortOrder + 1}";
            if (blockMap.ContainsKey(blockId))
            {
                continue;
            }

            blockMap[blockId] = new 커뮤니티원장블록투영
            {
                커뮤니티원장Id = 원장.원장Id.Trim(),
                커뮤니티Id = Clean(원장.커뮤니티Id) ?? "platform",
                원장템플릿Key = Clean(원장.원장템플릿Key) ?? string.Empty,
                BlockId = blockId,
                BlockType = Clean(block.BlockType) ?? CommunityLedgerBlockTypes.Generic,
                Title = Clean(block.Title) ?? blockId,
                State = Clean(block.State),
                SortOrder = sortOrder++,
                속성Json = ToJson(block.Data),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
        }

        if (원장.다이어그램스냅샷 is not null)
        {
            foreach (var node in 원장.다이어그램스냅샷.Nodes)
            {
                var blockId = ResolveNodeBlockId(node);
                if (string.IsNullOrWhiteSpace(blockId))
                {
                    continue;
                }

                nodeToBlockId[node.NodeId] = blockId;
                if (!blockMap.TryGetValue(blockId, out var projection))
                {
                    projection = new 커뮤니티원장블록투영
                    {
                        커뮤니티원장Id = 원장.원장Id.Trim(),
                        커뮤니티Id = Clean(원장.커뮤니티Id) ?? "platform",
                        원장템플릿Key = Clean(원장.원장템플릿Key) ?? Clean(원장.다이어그램스냅샷.LedgerTemplateKey) ?? string.Empty,
                        BlockId = blockId,
                        BlockType = Clean(node.Kind) ?? CommunityLedgerBlockTypes.Generic,
                        Title = Clean(node.Title) ?? blockId,
                        SortOrder = sortOrder++,
                        속성Json = ToJson(node.Data),
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    };
                    blockMap[blockId] = projection;
                }

                projection.DiagramNodeId = Clean(node.NodeId);
                projection.UiSectionHint ??= Clean(node.GroupLabel);
                projection.RelatedRoute ??= Clean(node.RelatedRoute);
                projection.UpdatedAtUtc = now;
            }
        }

        var relations = 원장.다이어그램스냅샷 is null
            ? []
            : BuildDiagramRelations(원장, blockMap, nodeToBlockId, now);

        if (relations.Count == 0 && blockMap.Count > 1)
        {
            relations = BuildSequentialRelations(원장, blockMap.Values.OrderBy(x => x.SortOrder).ToArray(), now);
        }

        return new 커뮤니티원장블록관계투영결과(blockMap.Values.OrderBy(x => x.SortOrder).ToArray(), relations);
    }

    private static List<커뮤니티원장블록관계투영> BuildDiagramRelations(
        커뮤니티원장Dto 원장,
        IReadOnlyDictionary<string, 커뮤니티원장블록투영> blockMap,
        IReadOnlyDictionary<string, string> nodeToBlockId,
        DateTime now)
    {
        var relations = new List<커뮤니티원장블록관계투영>();
        if (원장.다이어그램스냅샷 is null)
        {
            return relations;
        }

        var sortOrder = 0;
        foreach (var edge in 원장.다이어그램스냅샷.Edges)
        {
            var fromBlockId = ResolveEdgeBlockId(edge.FromNodeId, nodeToBlockId);
            var toBlockId = ResolveEdgeBlockId(edge.ToNodeId, nodeToBlockId);
            if (fromBlockId is null || toBlockId is null)
            {
                continue;
            }

            if (!blockMap.TryGetValue(fromBlockId, out var fromBlock) || !blockMap.TryGetValue(toBlockId, out var toBlock))
            {
                continue;
            }

            relations.Add(new 커뮤니티원장블록관계투영
            {
                커뮤니티원장Id = 원장.원장Id.Trim(),
                관계유형 = ResolveRelationType(edge),
                Cardinality = ResolveCardinality(edge.Data),
                필수여부 = ResolveRequired(edge.Data),
                SortOrder = sortOrder++,
                FromBlockId = fromBlock.BlockId,
                ToBlockId = toBlock.BlockId,
                DiagramEdgeId = Clean(edge.EdgeId),
                Label = Clean(edge.Label),
                MeaningCode = Clean(edge.MeaningCode),
                조건식Json = ResolveConditionJson(edge.Data),
                FromBlock = fromBlock,
                ToBlock = toBlock,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        return relations;
    }

    private static List<커뮤니티원장블록관계투영> BuildSequentialRelations(
        커뮤니티원장Dto 원장,
        IReadOnlyList<커뮤니티원장블록투영> blocks,
        DateTime now)
    {
        var relations = new List<커뮤니티원장블록관계투영>();
        for (var i = 0; i < blocks.Count - 1; i++)
        {
            relations.Add(new 커뮤니티원장블록관계투영
            {
                커뮤니티원장Id = 원장.원장Id.Trim(),
                관계유형 = 원장블록관계유형.흐름,
                Cardinality = 원장블록관계Cardinality.일대일,
                SortOrder = i,
                FromBlockId = blocks[i].BlockId,
                ToBlockId = blocks[i + 1].BlockId,
                Label = "다음",
                FromBlock = blocks[i],
                ToBlock = blocks[i + 1],
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        return relations;
    }

    private static string? ResolveEdgeBlockId(string nodeId, IReadOnlyDictionary<string, string> nodeToBlockId)
        => string.IsNullOrWhiteSpace(nodeId)
            ? null
            : nodeToBlockId.TryGetValue(nodeId.Trim(), out var blockId)
                ? blockId
                : nodeId.Trim();

    private static string? ResolveNodeBlockId(DiagramNodeDto node)
        => FirstNonEmpty(
            TryGet(node.Data, "BlockId"),
            TryGet(node.Data, "blockId"),
            TryGet(node.Data, "원장블록Id"),
            node.NodeId);

    private static string ResolveRelationType(DiagramEdgeDto edge)
    {
        var raw = FirstNonEmpty(TryGet(edge.Data, "관계유형"), TryGet(edge.Data, "RelationType"), edge.MeaningCode, edge.Label);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 원장블록관계유형.흐름;
        }

        if (ContainsAny(raw, "contains", "include", "포함", "묶음"))
        {
            return 원장블록관계유형.포함;
        }

        if (ContainsAny(raw, "require", "required", "필수", "선행"))
        {
            return 원장블록관계유형.선행필수;
        }

        if (ContainsAny(raw, "handoff", "인계", "api", "os"))
        {
            return 원장블록관계유형.인계;
        }

        if (ContainsAny(raw, "reference", "참조"))
        {
            return 원장블록관계유형.참조;
        }

        return 원장블록관계유형.흐름;
    }

    private static string ResolveCardinality(IReadOnlyDictionary<string, string> data)
        => 원장블록관계Cardinality.정규화(FirstNonEmpty(
            TryGet(data, "Cardinality"),
            TryGet(data, "cardinality"),
            TryGet(data, "관계Cardinality"),
            TryGet(data, "관계수"),
            TryGet(data, "수량관계")));

    private static bool ResolveRequired(IReadOnlyDictionary<string, string> data)
    {
        var raw = FirstNonEmpty(TryGet(data, "필수여부"), TryGet(data, "Required"), TryGet(data, "required"), TryGet(data, "IsRequired"));
        return raw is not null
               && (string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(raw, "y", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(raw, "필수", StringComparison.OrdinalIgnoreCase));
    }

    private static string? ResolveConditionJson(IReadOnlyDictionary<string, string> data)
        => FirstNonEmpty(
            TryGet(data, "조건식Json"),
            TryGet(data, "ConditionJson"),
            TryGet(data, "conditionJson"),
            TryGet(data, "조건식"));

    private static string ToJson(IReadOnlyDictionary<string, string> data)
        => data.Count == 0
            ? "{}"
            : JsonSerializer.Serialize(data, JsonOptions);

    private static string? TryGet(IReadOnlyDictionary<string, string> data, string key)
        => data.TryGetValue(key, out var value) ? Clean(value) : null;

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static bool ContainsAny(string source, params string[] candidates)
        => candidates.Any(candidate => source.Contains(candidate, StringComparison.OrdinalIgnoreCase));

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record 커뮤니티원장블록관계투영결과(
    IReadOnlyList<커뮤니티원장블록투영> 블록목록,
    IReadOnlyList<커뮤니티원장블록관계투영> 관계목록);
