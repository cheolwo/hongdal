namespace Ssalddel.Contracts.Common.Community;

public static class 커뮤니티원장공개범위
{
    public const string 비공개 = "비공개";
    public const string 커뮤니티 = "커뮤니티";
    public const string 전체공개 = "전체공개";

    public static bool 공개범위인가(string? value)
        => string.Equals(value, 커뮤니티, StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, 전체공개, StringComparison.OrdinalIgnoreCase);
}

public static class 커뮤니티원장공개항목Key
{
    public const string 제목 = "summary.title";
    public const string 상태 = "summary.state";
    public const string 현재단계 = "summary.current-step";
    public const string 다이어그램구조 = "diagram.structure";

    public static string 블록제목(string blockId) => $"block:{blockId}:title";
    public static string 블록상태(string blockId) => $"block:{blockId}:state";
    public static string 블록Data(string blockId, string key) => $"block:{blockId}:data:{key}";
}

public sealed class 커뮤니티원장공개항목Response
{
    public string 항목Key { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 항목유형 { get; set; } = string.Empty;
    public bool 공개여부 { get; set; }
}

public sealed class 커뮤니티원장공개설정Response
{
    public string 원장Id { get; set; } = string.Empty;
    public string 공개범위 { get; set; } = 커뮤니티원장공개범위.비공개;
    public bool 재사용허용여부 { get; set; }
    public bool 재공유허용여부 { get; set; }
    public bool 수정가능여부 { get; set; }
    public long Revision { get; set; }
    public DateTime? 수정시각Utc { get; set; }
    public IReadOnlyList<커뮤니티원장공개항목Response> 항목목록 { get; set; } = [];
}

public sealed class 커뮤니티원장공개설정변경Request
{
    public string 공개범위 { get; set; } = 커뮤니티원장공개범위.비공개;
    public bool 재사용허용여부 { get; set; }
    public bool 재공유허용여부 { get; set; }
    public long? 기대Revision { get; set; }
    public IReadOnlyList<string> 공개항목Key목록 { get; set; } = [];
}

public sealed class 커뮤니티원장재사용Request
{
    public string? 새제목 { get; set; }
}

public sealed class 커뮤니티원장재사용Response
{
    public string 원장Id { get; set; } = string.Empty;
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 출처원장Id { get; set; } = string.Empty;
}
