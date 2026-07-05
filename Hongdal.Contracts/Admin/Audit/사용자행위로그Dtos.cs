namespace Hongdal.Contracts.Admin.Audit;

public sealed class 사용자행위로그검색요청
{
    public string? AppKey { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? PhoneLast4 { get; set; }
    public string? ActionType { get; set; }
    public string? ActionName { get; set; }
    public bool? IsSuccess { get; set; }
    public string? TraceId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class 사용자행위로그목록응답
{
    public IReadOnlyList<사용자행위로그요약응답> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class 사용자행위로그요약응답
{
    public long Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string EmailMasked { get; set; } = string.Empty;
    public string PhoneLast4 { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}

public sealed class 사용자행위로그상세응답
{
    public long Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string EmailMasked { get; set; } = string.Empty;
    public string PhoneLast4 { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string MetadataJson { get; set; } = string.Empty;
}

public sealed class Trace행위로그묶음응답
{
    public string TraceId { get; set; } = string.Empty;
    public IReadOnlyList<사용자행위로그요약응답> Items { get; set; } = [];
}
