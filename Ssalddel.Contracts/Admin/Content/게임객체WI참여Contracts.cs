namespace Ssalddel.Contracts.Admin.Content;

public sealed record 게임객체WI참여Input(string WorldInteractionId, string? DefinitionId, string Role,
    string ContextKey, string ObjectKind, string ExtractionState, string RuleRevision, string SourceField, string ExactQuote, string ContextNote);
public sealed record 게임객체WI추출Request(string RequestId, string SourceRef, string SourceRevision, string SourceHash,
    IReadOnlyList<게임객체시각구성Input> Definitions, IReadOnlyList<게임객체WI참여Input> Relations);
public sealed record 게임객체WI참여Dto(string UseId, string SourceRevision, string SourceHash, string Freshness,
    string? DefinitionCompositionId, 게임객체WI참여Input Relation, string DecisionState, string ApplicationState);
public sealed record 게임객체WI추출Result(string Diagnostic, int DefinitionsInserted = 0, int RelationsInserted = 0, bool Duplicate = false);
public sealed record 게임객체WI조회Result(string Diagnostic, IReadOnlyList<게임객체WI참여Dto> Items);
public sealed record 게임객체WI목록항목(string WorldInteractionId, string Title, string Group, string ReviewState);
public sealed record 게임객체WI목록Result(string Diagnostic, string? SourceRevision, string? SourceHash, IReadOnlyList<게임객체WI목록항목> Items);
