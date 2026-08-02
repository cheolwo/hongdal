using Ssalddel.Contracts.Common.Payments;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Community;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityActivityPaidDetail,
    SsalddelCodeLayer.Contract,
    "커뮤니티 활동 유료 상세의 등록, 구매, 열람권과 구매 상태 이력을 공유합니다.",
    FlowOrder = 10,
    Effects = SsalddelCodeEffect.None,
    Boundary = "상세 본문은 작성자 또는 활성 열람권 보유자 응답에만 포함합니다.")]
public sealed class 커뮤니티활동유료상세등록Request
{
    public long 게시글Id { get; set; }
    public string 공개미리보기 { get; set; } = string.Empty;
    public string 상세내용 { get; set; } = string.Empty;
    public int 가격금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
}

public sealed class 커뮤니티활동유료상세Response
{
    public string 상세Id { get; set; } = string.Empty;
    public long 게시글Id { get; set; }
    public string 게시글제목 { get; set; } = string.Empty;
    public string 판매자표시명 { get; set; } = string.Empty;
    public string 공개미리보기 { get; set; } = string.Empty;
    public string? 상세내용 { get; set; }
    public int 가격금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
    public string 판매상태 { get; set; } = 커뮤니티활동유료상세판매상태.판매중;
    public bool 열람가능 { get; set; }
    public string 열람근거 { get; set; } = 커뮤니티활동상세열람근거.구매필요;
}

public sealed class 커뮤니티활동상세열람권Response
{
    public string 열람권Id { get; set; } = string.Empty;
    public string 상세Id { get; set; } = string.Empty;
    public string 구매자UserId { get; set; } = string.Empty;
    public string 결제Id { get; set; } = string.Empty;
    public string 상태 { get; set; } = 커뮤니티활동상세열람권상태.활성;
    public DateTime 발급일시Utc { get; set; }
}

public sealed class 커뮤니티활동상세FakePg결제승인Request
{
    public int Amount { get; set; }
    public string 결제수단 { get; set; } = "FakePG";
    public string? IdempotencyKey { get; set; }
}

public sealed class 커뮤니티활동상세FakePg결제승인Response
{
    public string 결제Id { get; set; } = string.Empty;
    public int 결제대상유형 { get; set; } = 계약결제대상유형.커뮤니티활동상세열람;
    public int 결제제공자 { get; set; } = 계약결제제공자.FakePG;
    public string OrderId { get; set; } = string.Empty;
    public string PaymentKey { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string 통화Code { get; set; } = "KRW";
    public string 결제상태 { get; set; } = string.Empty;
    public DateTime 승인일시Utc { get; set; }
    public bool 이미완료됨 { get; set; }
    public 커뮤니티활동상세열람권Response 열람권 { get; set; } = new();
    public 커뮤니티활동상세구매WorkflowResponse 구매Workflow { get; set; } = new();
}

public sealed class 커뮤니티활동상세구매WorkflowResponse
{
    public string 구매Id { get; set; } = string.Empty;
    public string 상세Id { get; set; } = string.Empty;
    public string 구매자UserId { get; set; } = string.Empty;
    public int 요청금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
    public string 현재상태 { get; set; } = 커뮤니티활동상세구매상태.요청됨;
    public string? 결제Id { get; set; }
    public string? 열람권Id { get; set; }
    public DateTime 요청일시Utc { get; set; }
    public DateTime? 완료일시Utc { get; set; }
    public IReadOnlyList<커뮤니티활동상세구매상태이력Response> 상태이력 { get; set; } = [];
}

public sealed class 커뮤니티활동상세구매상태이력Response
{
    public int 순서 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string 사유Code { get; set; } = string.Empty;
    public DateTime 기록일시Utc { get; set; }
}

public static class 커뮤니티활동유료상세판매상태
{
    public const string 판매중 = "Published";
    public const string 판매중지 = "Paused";
}

public static class 커뮤니티활동상세열람권상태
{
    public const string 활성 = "Active";
    public const string 철회 = "Revoked";
}

public static class 커뮤니티활동상세열람근거
{
    public const string 작성자본인 = "Owner";
    public const string 구매 = "Purchase";
    public const string 구매필요 = "PurchaseRequired";
}

public static class 커뮤니티활동상세구매상태
{
    public const string 요청됨 = "Requested";
    public const string 결제승인됨 = "PaymentApproved";
    public const string 열람권발급됨 = "EntitlementGranted";
    public const string 실패 = "Failed";
    public const string 취소됨 = "Cancelled";
    public const string 환불됨 = "Refunded";
}
