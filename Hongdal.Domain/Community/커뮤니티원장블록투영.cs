namespace Hongdal.Domain.Community;

public sealed class 커뮤니티원장블록투영
{
    public long Id { get; set; }
    public string 커뮤니티원장Id { get; set; } = string.Empty;
    public string 커뮤니티Id { get; set; } = string.Empty;
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string BlockId { get; set; } = string.Empty;
    public string BlockType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? UiSectionHint { get; set; }
    public string? DiagramNodeId { get; set; }
    public string? RelatedRoute { get; set; }
    public int SortOrder { get; set; }
    public string 속성Json { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<커뮤니티원장블록관계투영> 출력관계목록 { get; set; } = new List<커뮤니티원장블록관계투영>();
    public ICollection<커뮤니티원장블록관계투영> 입력관계목록 { get; set; } = new List<커뮤니티원장블록관계투영>();
}

public sealed class 커뮤니티원장블록관계투영
{
    public long Id { get; set; }
    public string 커뮤니티원장Id { get; set; } = string.Empty;
    public string 관계유형 { get; set; } = 원장블록관계유형.흐름;
    public string Cardinality { get; set; } = 원장블록관계Cardinality.다대다;
    public bool 필수여부 { get; set; }
    public int SortOrder { get; set; }
    public string FromBlockId { get; set; } = string.Empty;
    public string ToBlockId { get; set; } = string.Empty;
    public long FromBlockProjectionId { get; set; }
    public long ToBlockProjectionId { get; set; }
    public string? DiagramEdgeId { get; set; }
    public string? Label { get; set; }
    public string? MeaningCode { get; set; }
    public string? 조건식Json { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public 커뮤니티원장블록투영 FromBlock { get; set; } = null!;
    public 커뮤니티원장블록투영 ToBlock { get; set; } = null!;
}

public static class 원장블록관계유형
{
    public const string 흐름 = "Flow";
    public const string 포함 = "Contains";
    public const string 선행필수 = "Requires";
    public const string 인계 = "Handoff";
    public const string 참조 = "Reference";
}

public static class 원장블록관계Cardinality
{
    public const string 일대일 = "1:1";
    public const string 일대다 = "1:N";
    public const string 다대일 = "N:1";
    public const string 다대다 = "N:M";

    public static string 정규화(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 다대다;
        }

        var normalized = value.Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("대", ":", StringComparison.OrdinalIgnoreCase)
            .Replace("One", "1", StringComparison.OrdinalIgnoreCase)
            .Replace("Many", "N", StringComparison.OrdinalIgnoreCase)
            .Replace("To", ":", StringComparison.OrdinalIgnoreCase);

        return normalized.ToUpperInvariant() switch
        {
            "1:1" => 일대일,
            "1:N" => 일대다,
            "N:1" => 다대일,
            "N:M" => 다대다,
            "M:N" => 다대다,
            _ => 다대다
        };
    }
}
