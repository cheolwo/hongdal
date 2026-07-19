namespace Ssalddel.Contracts.Admin.Dispatch;

public sealed class DispatchAIJudgmentCaseCatalogDto
{
    public List<DispatchAIJudgmentCaseDto> Cases { get; set; } = [];

    public List<DispatchAIJudgmentCaseSuggestionDto> Suggestions { get; set; } = [];
}

public sealed class DispatchAIJudgmentCaseDto
{
    public string CaseId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string RelatedOS { get; set; } = string.Empty;

    public List<string> Keywords { get; set; } = [];

    public string SituationSummary { get; set; } = string.Empty;

    public string JudgmentSummary { get; set; } = string.Empty;

    public string UserDecision { get; set; } = string.Empty;

    public string BalancedDecision { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class DispatchAIJudgmentCaseSuggestionDto
{
    public string SuggestionKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string RelatedOS { get; set; } = string.Empty;

    public List<string> Keywords { get; set; } = [];

    public string SituationSummary { get; set; } = string.Empty;

    public string SuggestedJudgmentSummary { get; set; } = string.Empty;

    public string DefaultUserDecision { get; set; } = "승인";

    public string DefaultBalancedDecision { get; set; } = "운영자 승인";

    public string Source { get; set; } = string.Empty;
}

public sealed class DispatchAIJudgmentCaseCreateRequest
{
    public string Title { get; set; } = string.Empty;

    public string RelatedOS { get; set; } = string.Empty;

    public List<string> Keywords { get; set; } = [];

    public string SituationSummary { get; set; } = string.Empty;

    public string JudgmentSummary { get; set; } = string.Empty;

    public string UserDecision { get; set; } = string.Empty;

    public string BalancedDecision { get; set; } = string.Empty;

    public string? Source { get; set; }

    public bool Active { get; set; } = true;
}

public sealed class DispatchAIJudgmentCasePromoteSuggestionRequest
{
    public string? JudgmentSummary { get; set; }

    public string? UserDecision { get; set; }

    public string? BalancedDecision { get; set; }

    public bool Active { get; set; } = true;
}
