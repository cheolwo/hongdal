namespace Ssalddel.Services.Community;

/// <summary>
/// 기존의 '커뮤니티 원장'을 일반적인 기술 용어인 Business Case로 읽기 위한 계약입니다.
/// 기존 DTO와 MongoDB 필드 이름은 바꾸지 않으므로 저장 데이터와 공개 API의 호환성이 유지됩니다.
/// </summary>
public interface IBusinessCaseRecord
{
    string CaseId { get; }
    long Revision { get; }
    string CommunityId { get; }
    string CaseTemplateKey { get; }
    string Title { get; }
    string? Intent { get; }
    string Status { get; }
    string? CurrentStageKey { get; }
    IReadOnlyList<IBusinessCaseSection> Sections { get; }
    IReadOnlyList<IBusinessCaseParticipant> Participants { get; }
    IReadOnlyList<IBusinessCaseHistoryEntry> History { get; }
    DateTime CreatedAtUtc { get; }
    DateTime UpdatedAtUtc { get; }
}

/// <summary>
/// 원장 블록에 대응하는 기술 계약입니다.
/// 한 업무 건을 참여자·장소·물건·상태·증빙·인계 같은 작은 구역으로 나눠 표현합니다.
/// </summary>
public interface IBusinessCaseSection
{
    string SectionId { get; }
    string SectionType { get; }
    string Title { get; }
    string? Status { get; }
    IReadOnlyList<IBusinessCaseAssignee> Assignees { get; }
    IReadOnlyDictionary<string, string> Data { get; }
}

/// <summary>
/// 원장 블록 담당자에 대응하는 기술 계약입니다.
/// </summary>
public interface IBusinessCaseAssignee
{
    string UserId { get; }
    string DisplayName { get; }
    string RoleLabel { get; }
    string ResponsibilityType { get; }
}

/// <summary>
/// 원장 참여자에 대응하는 기술 계약입니다.
/// </summary>
public interface IBusinessCaseParticipant
{
    string? UserId { get; }
    string DisplayName { get; }
    string RoleLabel { get; }
    string ParticipationStatus { get; }
}

/// <summary>
/// 원장 상태 이력에 대응하는 기술 계약입니다.
/// 감사 로그 전체가 아니라 사용자가 확인할 수 있는 업무 상태 변경 이력을 뜻합니다.
/// </summary>
public interface IBusinessCaseHistoryEntry
{
    string? EventId { get; }
    string Status { get; }
    string? PreviousStatus { get; }
    string? CurrentStageKey { get; }
    string? Note { get; }
    string ChangedBy { get; }
    DateTime ChangedAtUtc { get; }
}

/// <summary>
/// 새 코드가 '원장 저장소' 대신 Business Case 저장소라는 기술 용어로 의존할 수 있게 하는 포트입니다.
/// 입출력의 기존 한글 요청 DTO는 직렬화 호환성을 위해 그대로 사용합니다.
/// </summary>
public interface IBusinessCaseStore
{
    Task<IBusinessCaseRecord> SaveAsync(
        커뮤니티원장저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<IBusinessCaseRecord?> GetAsync(
        string caseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IBusinessCaseRecord>> ListAsync(
        커뮤니티원장조회조건 query,
        CancellationToken cancellationToken = default);

    Task<IBusinessCaseRecord?> ChangeStatusAsync(
        커뮤니티원장상태변경요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 기존 원장 저장소를 새 Business Case 포트로 연결하는 호환 Adapter입니다.
/// 별도 저장이나 복제를 하지 않고 기존 Mongo 원본과 Event 발행 경로를 그대로 통과합니다.
/// </summary>
public sealed class BusinessCaseStoreAdapter(I커뮤니티원장저장소 원장저장소) : IBusinessCaseStore
{
    public async Task<IBusinessCaseRecord> SaveAsync(
        커뮤니티원장저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
        => await 원장저장소.원장저장Async(request, updatedBy, cancellationToken);

    public async Task<IBusinessCaseRecord?> GetAsync(
        string caseId,
        CancellationToken cancellationToken = default)
        => await 원장저장소.원장조회Async(caseId, cancellationToken);

    public async Task<IReadOnlyList<IBusinessCaseRecord>> ListAsync(
        커뮤니티원장조회조건 query,
        CancellationToken cancellationToken = default)
    {
        var 원장목록 = await 원장저장소.원장목록조회Async(query, cancellationToken);
        return 원장목록.Cast<IBusinessCaseRecord>().ToArray();
    }

    public async Task<IBusinessCaseRecord?> ChangeStatusAsync(
        커뮤니티원장상태변경요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
        => await 원장저장소.원장상태변경Async(request, updatedBy, cancellationToken);
}

/// <summary>
/// Business Case 코드에서 사용할 수 있는 상태 이름입니다.
/// 값은 기존 한글 상태와 동일하므로 DB 조회와 상태 전이 규칙은 바뀌지 않습니다.
/// </summary>
public static class BusinessCaseStatus
{
    public const string Draft = 커뮤니티원장상태.초안;
    public const string InProgress = 커뮤니티원장상태.진행중;
    public const string OnHold = 커뮤니티원장상태.보류;
    public const string Completed = 커뮤니티원장상태.완료;
    public const string Closed = 커뮤니티원장상태.닫힘;
}

public sealed partial class 커뮤니티원장Dto : IBusinessCaseRecord
{
    string IBusinessCaseRecord.CaseId => 원장Id;
    string IBusinessCaseRecord.CommunityId => 커뮤니티Id;
    string IBusinessCaseRecord.CaseTemplateKey => 원장템플릿Key;
    string IBusinessCaseRecord.Title => 제목;
    string? IBusinessCaseRecord.Intent => 원함;
    string IBusinessCaseRecord.Status => 상태;
    string? IBusinessCaseRecord.CurrentStageKey => 현재단계Key;
    IReadOnlyList<IBusinessCaseSection> IBusinessCaseRecord.Sections => 블록목록;
    IReadOnlyList<IBusinessCaseParticipant> IBusinessCaseRecord.Participants => 참여자목록;
    IReadOnlyList<IBusinessCaseHistoryEntry> IBusinessCaseRecord.History => 상태이력;
    DateTime IBusinessCaseRecord.CreatedAtUtc => 생성시각Utc;
    DateTime IBusinessCaseRecord.UpdatedAtUtc => 수정시각Utc;
}

public sealed partial class 커뮤니티원장블록Dto : IBusinessCaseSection
{
    string IBusinessCaseSection.SectionId => BlockId;
    string IBusinessCaseSection.SectionType => BlockType;
    string IBusinessCaseSection.Title => Title;
    string? IBusinessCaseSection.Status => State;
    IReadOnlyList<IBusinessCaseAssignee> IBusinessCaseSection.Assignees => 담당자목록;
    IReadOnlyDictionary<string, string> IBusinessCaseSection.Data => Data;
}

public sealed partial class 커뮤니티원장블록담당자Dto : IBusinessCaseAssignee
{
    string IBusinessCaseAssignee.UserId => UserId;
    string IBusinessCaseAssignee.DisplayName => DisplayName;
    string IBusinessCaseAssignee.RoleLabel => RoleLabel;
    string IBusinessCaseAssignee.ResponsibilityType => ResponsibilityType;
}

public sealed partial class 커뮤니티원장참여자Dto : IBusinessCaseParticipant
{
    string? IBusinessCaseParticipant.UserId => UserId;
    string IBusinessCaseParticipant.DisplayName => DisplayName;
    string IBusinessCaseParticipant.RoleLabel => RoleLabel;
    string IBusinessCaseParticipant.ParticipationStatus => ParticipationState;
}

public sealed partial class 커뮤니티원장상태이력Dto : IBusinessCaseHistoryEntry
{
    string? IBusinessCaseHistoryEntry.EventId => EventId;
    string IBusinessCaseHistoryEntry.Status => 상태;
    string? IBusinessCaseHistoryEntry.PreviousStatus => 이전상태;
    string? IBusinessCaseHistoryEntry.CurrentStageKey => 현재단계Key;
    string? IBusinessCaseHistoryEntry.Note => 메모;
    string IBusinessCaseHistoryEntry.ChangedBy => 변경자;
    DateTime IBusinessCaseHistoryEntry.ChangedAtUtc => 변경시각Utc;
}
