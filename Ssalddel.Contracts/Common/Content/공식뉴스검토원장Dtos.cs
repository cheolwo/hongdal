namespace Ssalddel.Contracts.Common.Content;

public sealed class 공식뉴스검토결정Request
{
    public string SourceKey { get; set; } = string.Empty;

    public string DecisionCode { get; set; } = string.Empty;

    public string DecisionNote { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public long? ExpectedRevision { get; set; }
}

public sealed record 공식뉴스검토결정이력Dto(
    string IdempotencyKey,
    string DecisionCode,
    string DecisionNote,
    string ReviewerDisplayName,
    DateTime DecidedAtUtc,
    long Revision);

public sealed record 공식뉴스검토원장Dto(
    string CandidateKey,
    string SourceKey,
    string ReviewState,
    long Revision,
    CommunityInformationCandidateDto Candidate,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<공식뉴스검토결정이력Dto> History);

public sealed record 공식뉴스검토원장목록Response(
    string? SourceKey,
    string? ReviewState,
    IReadOnlyList<공식뉴스검토원장Dto> Items);
