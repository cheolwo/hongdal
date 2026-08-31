using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Admin.Content;

public static class 개체시각대응Codes
{
    public const string Feature = "entity-record-visual-correspondence";
    public const string Route = "api/v1/admin/content/entity-visual-bindings";
    public const string Policy = "서버관리자전용";
    public const string Draft = "Draft";
    public const string Pending = "PendingReview";
    public const string Approved = "Approved";
    public const string Excluded = "Excluded";
}

/// <summary>종류/대상은 등록된 서버 조회로만 해석한다. 테이블명·권한·상태를 입력받지 않는다.</summary>
public sealed record 개체시각대상Query(string Kind, string StableId, string Purpose, long? WarehouseId = null);
public sealed record 개체시각대상Dto(string Kind, string StableId, string SourceKey, string AccessScope,
    string Revision, string StateCode, string Purpose, string Representation, string DisplayName);
public sealed record 개체시각후보Dto(string VisualKey, string CatalogRevision, string CatalogFingerprint,
    string AssetFingerprint, string Fitness, string EvidenceRef, string EvidenceFingerprint,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    string? AssetVersionId = null);

/// <summary>기존 Unity 대장에서 가져올 메타데이터. 파일 본문/바이너리는 받지 않는다.</summary>
public sealed record 개체시각자산입력(string VisualKey, string CatalogRevision, string CatalogPath,
    string CatalogFingerprint, string Provider, string Pack, string PrefabPath, string PrefabGuid,
    string AssetFingerprint, string MetaFingerprint, string DisplayName, string Role,
    string EvidenceRef, string EvidenceFingerprint);
public sealed record 개체시각자산판본Dto(string AssetVersionId, 개체시각자산입력 Metadata,
    string VerificationState, DateTime RegisteredAtUtc);
public sealed record 개체시각목록Result(string Diagnostic, IReadOnlyList<개체시각자산판본Dto> Items,
    int Inserted = 0, int Existing = 0);
public enum 개체시각대응Action { SaveDraft, SubmitReview, Approve, Exclude }

[SsalddelCodeMetadata(개체시각대응Codes.Feature, SsalddelCodeLayer.Contract,
    "권한 있는 대상의 종류 기본/개별 시각 대응 검토 요청을 정의한다.", StepKey = "contract", FlowOrder = 10,
    Boundary = "Prefab 파일·원천 상태·권한을 입력받지 않으며 승인도 실제 배치 완료가 아니다.")]
public sealed record 개체시각대응Request(string BindingId, long ExpectedRevision, string IdempotencyKey,
    개체시각대응Action Action, string Note, 개체시각대상Query Target, bool TypeDefault,
    개체시각후보Dto? Candidate = null);
public sealed record 개체시각대응Dto(string BindingId, long Revision, 개체시각대상Dto Target,
    bool TypeDefault, 개체시각후보Dto? Candidate, string ReviewState, string? ReviewerId, DateTime UpdatedAtUtc);
public sealed record 개체시각대응Result(bool Success, string Diagnostic, 개체시각대응Dto? Binding = null,
    bool Duplicate = false);
public sealed record 개체시각선택Result(string Diagnostic, 개체시각대상Dto? Target = null,
    string? VisualKey = null, string? BindingId = null, bool IsFallback = false,
    IReadOnlyList<string>? Limitations = null)
{
    public bool CanApplyToScene => false;
}
public sealed record 개체시각대응목록Result(string Diagnostic, IReadOnlyList<개체시각대응Dto> Items);
public sealed record 개체시각이력Dto(long Revision, string Action, string ReviewerId, string Note, DateTime AtUtc);
public sealed record 개체시각이력Result(string Diagnostic, IReadOnlyList<개체시각이력Dto> Items);
