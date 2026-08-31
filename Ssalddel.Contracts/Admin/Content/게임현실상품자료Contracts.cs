using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Admin.Content;

public static class 게임현실상품자료Codes
{
    public const string FeatureKey = "game-reality-product-curation";
    public const string Draft = "Draft";
    public const string Uncollected = "Uncollected";
    public const string Fixture = "Fixture";
    public const string Observed = "Observed";
    public const string Unconfirmed = "Unconfirmed";
    public const string Similar = "Similar";
    public const string Identical = "Identical";
    public const string Comparable = "Comparable";
}

public enum 게임현실상품자료Action
{
    CreateDraft, ReviseDraft, SubmitReview, ApproveMapping, Approve, Exclude
}

public sealed record 게임상품참조Dto(string StableId, string 이름);

/// <summary>수집 결과가 아니라 운영자가 검토할 입력 사본. null 비용은 0이 아니다.</summary>
public sealed record 현실상품후보Dto(
    string StableId,
    string 자료종류,
    string? 플랫폼,
    string? 판매자,
    string? 원천상품Id,
    string? 상품Url,
    DateTimeOffset? 관측시각,
    string? 규격,
    외부상품가격스냅샷Dto? 가격,
    decimal? 수량,
    string? 단위,
    decimal? 최소주문수량,
    string? 배송조건,
    string? 출처,
    string? 이용조건근거,
    string? 이용조건검토상태);

public sealed record 게임현실상품자료초안Dto(
    게임상품참조Dto 게임상품,
    현실상품후보Dto? 현실후보,
    string 대응종류,
    string? 대응근거,
    string 비교상태,
    string? 비교근거,
    IReadOnlyList<string> 부족조건,
    string? 제목,
    string? 요약,
    string? 출처표시,
    string? 한계);

public sealed record 게임현실상품자료Request(
    string StableId,
    string IdempotencyKey,
    long ExpectedRevision,
    게임현실상품자료Action Action,
    string 검토메모,
    게임현실상품자료초안Dto? 초안 = null);

public sealed record 게임현실상품자료이력Dto(
    string IdempotencyKey,
    string RequestHash,
    string 검토자Id,
    게임현실상품자료Action Action,
    string 검토메모,
    DateTimeOffset 검토시각,
    long Revision);

[SsalddelCodeMetadata(게임현실상품자료Codes.FeatureKey, SsalddelCodeLayer.Contract,
    "게임 상품·현실 후보·대응 검토와 제공자료 검토 상태를 분리한다.",
    StepKey = "curation-state", FlowOrder = 10,
    ExecutionStage = SsalddelCodeExecutionStage.Definition,
    Effects = SsalddelCodeEffect.None,
    Boundary = "검토 승인 사본은 수집·게시·통지·게임 상태 또는 영속 확정이 아니다.")]
public sealed record 게임현실상품자료State(
    string StableId,
    long Revision,
    게임현실상품자료초안Dto 초안,
    string 검토상태,
    bool 대응승인됨,
    IReadOnlyList<게임현실상품자료이력Dto> History);

/// <summary>저장 전 상태 후보다. 실제 게시나 외부 실행을 켜는 플래그를 제공하지 않는다.</summary>
public sealed record 게임현실상품자료Result(
    bool Prepared,
    bool Duplicate,
    string Diagnostic,
    게임현실상품자료State? State)
{
    public bool 수집실행 => false;
    public bool 실제게시 => false;
    public bool 통지발송 => false;
    public bool 게임상태변경 => false;
    public bool 영속확정 => false;
}
