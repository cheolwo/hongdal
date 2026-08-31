namespace Ssalddel.Contracts.Admin.Content;

public sealed record 보유시각자산Input(string SurveyRevision, string InputFileHash, string SourceGroup, string? PackCode,
    string RelativePath, string Name, string AssetKind, string Guid, string AssetHash, string MetaHash,
    string? OriginVersion, string EvidenceJson, IReadOnlyList<string> ExistingCandidateIds);
public sealed record 보유시각자산반입Request(string EvidenceRef, string EvidenceHash, IReadOnlyList<보유시각자산Input> Items);
public sealed record 보유시각자산Dto(string SnapshotId, string ContentVersionId, 보유시각자산Input Metadata,
    string ReviewState, string ApplicationState, string Freshness, DateTime RegisteredAtUtc, string EvidenceRef, string EvidenceHash);
public sealed record 보유시각자산반입Result(string Diagnostic, int Inserted = 0, int Existing = 0,
    int FirstSeenGuids = 0, int AdditionalSnapshots = 0);
public sealed record 보유시각자산목록Result(string Diagnostic, int Total, IReadOnlyList<보유시각자산Dto> Items,
    IReadOnlyList<보유시각분류Input>? Classifications = null);

public sealed record 보유시각분류Input(string SnapshotId, string ContentVersionId, string TaxonomyRevision,
    string TaxonomyHash, string TaxonomyPath, string State, string FamilyId, string Traits,
    string EvidenceRef, string EvidenceHash, string Rationale);
public sealed record 보유시각분류반입Request(IReadOnlyList<보유시각분류Input> Items);
public sealed record 보유시각분류Result(string Diagnostic, int Inserted = 0, int Existing = 0);
