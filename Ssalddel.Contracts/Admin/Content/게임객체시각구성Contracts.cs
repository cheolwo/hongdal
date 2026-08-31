using System.Text.Json.Serialization;

namespace Ssalddel.Contracts.Admin.Content;

// 역할의 반복은 서로 다른 SlotKey/ItemId로 식별한다. 위치/수량/실제 Session은 추정하지 않는다.
public sealed record 게임객체시각항목Input(string ItemId, string Role, string SlotKey,
    string? AssetVersionId = null, string? AnchorIntent = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? InventorySnapshotId = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? SelectionEvidenceJson = null);
public sealed record 게임객체시각구성Input(string DefinitionId, string DisplayName,
    string DefinitionRevision, string EvidenceRef, string EvidenceFingerprint,
    IReadOnlyList<게임객체시각항목Input> Items);
public sealed record 게임객체시각구성Request(string RequestId, long ExpectedRevision, 게임객체시각구성Input Definition);
public sealed record 게임객체시각항목Dto(게임객체시각항목Input Item, 개체시각자산판본Dto? Asset,
    string SelectionState, string ImageEvidenceState);
public sealed record 게임객체시각구성Dto(string CompositionId, long Revision, 게임객체시각구성Input Definition,
    string Kind, string ReviewState, string ApplicationState, string ReviewerId, DateTime AtUtc,
    IReadOnlyList<게임객체시각항목Dto> Items);
public sealed record 게임객체시각구성Result(string Diagnostic, 게임객체시각구성Dto? Composition = null, bool Duplicate = false);
public sealed record 게임객체시각구성목록Result(string Diagnostic, IReadOnlyList<게임객체시각구성Dto> Items);

// 판정은 검토 주체가 수행한다. 서버는 선언과 정확 파일/사본의 일치만 검사한다.
public sealed record 시각선정파일근거(string Root, string Path, string Sha256);
public sealed record 시각선정조건근거(string Condition, string State, string Reason);
public sealed record 시각자동선정근거(string SchemaVersion, string Origin, string DefinitionId,
    string DefinitionRevision, string Role, string SlotKey, string ObjectKind, string AssetKind,
    string Guid, string AssetHash, string MetaHash, string ContentVersionId, string Purpose,
    string Rationale, string ImageKind, 시각선정파일근거 Image, 시각선정파일근거 Review,
    IReadOnlyList<시각선정조건근거> Conditions, IReadOnlyList<시각선정파일근거> Dependencies);
