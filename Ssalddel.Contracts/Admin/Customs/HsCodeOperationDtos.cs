namespace Ssalddel.Contracts.Admin.Customs;

public sealed class AdminHsCodeListResponse
{
    public IReadOnlyList<AdminHsCodeEntryResponse> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}

public sealed class AdminHsCodeEntryResponse
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string NormalizedCode { get; set; } = string.Empty;

    public string KoreanName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public int BusinessCategory { get; set; }

    public string BusinessCategoryLabel { get; set; } = string.Empty;

    public string BusinessCategoryReason { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public IReadOnlyList<AdminHsCodeRiskTagResponse> RiskTags { get; set; } = [];
}

public sealed class AdminHsCodeRiskTagResponse
{
    public long Id { get; set; }

    public int TagType { get; set; }

    public string TagTypeLabel { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public int Source { get; set; }

    public string SourceLabel { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public sealed class AdminHsCodeBusinessCategoryUpdateRequest
{
    public int BusinessCategory { get; set; }

    public string Reason { get; set; } = string.Empty;
}

public sealed class AdminHsCodeRiskTagUpdateRequest
{
    public int TagType { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

