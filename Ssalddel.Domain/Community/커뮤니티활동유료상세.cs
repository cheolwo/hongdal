namespace Ssalddel.Domain.Community;

using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityActivityPaidDetail,
    SsalddelCodeLayer.Domain,
    "유료 상세, 구매 원장과 열람권의 영속 업무 상태를 정의합니다.",
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.None,
    Boundary = "상세 본문과 구매자 식별자는 공개 응답으로 직접 노출하지 않습니다.")]
public sealed class 커뮤니티활동유료상세
{
    public long Id { get; set; }
    public string 상세Id { get; set; } = string.Empty;
    public long 게시글Id { get; set; }
    public string 판매자UserId { get; set; } = string.Empty;
    public string 공개미리보기 { get; set; } = string.Empty;
    public string 상세내용 { get; set; } = string.Empty;
    public int 가격금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
    public string 판매상태 { get; set; } = "Published";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public PlatformCommunityPost 게시글 { get; set; } = null!;
    public ICollection<커뮤니티활동상세열람권> 열람권목록 { get; set; } = [];
    public ICollection<커뮤니티활동상세구매> 구매목록 { get; set; } = [];
}

public sealed class 커뮤니티활동상세열람권
{
    public long Id { get; set; }
    public string 열람권Id { get; set; } = string.Empty;
    public string 상세Id { get; set; } = string.Empty;
    public string 구매자UserId { get; set; } = string.Empty;
    public string 결제Id { get; set; } = string.Empty;
    public string 상태 { get; set; } = "Active";
    public DateTime 발급일시Utc { get; set; }
    public DateTime? 철회일시Utc { get; set; }
    public 커뮤니티활동유료상세 상세 { get; set; } = null!;
}

public sealed class 커뮤니티활동상세구매
{
    public long Id { get; set; }
    public string 구매Id { get; set; } = string.Empty;
    public string 상세Id { get; set; } = string.Empty;
    public string 구매자UserId { get; set; } = string.Empty;
    public string 판매자UserId { get; set; } = string.Empty;
    public string? 멱등성Key { get; set; }
    public int 요청금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
    public string 현재상태 { get; set; } = 커뮤니티활동상세구매상태.요청됨;
    public string? 결제Id { get; set; }
    public string? 열람권Id { get; set; }
    public DateTime 요청일시Utc { get; set; }
    public DateTime? 완료일시Utc { get; set; }
    public 커뮤니티활동유료상세 상세 { get; set; } = null!;
    public ICollection<커뮤니티활동상세구매상태이력> 상태이력 { get; set; } = [];
}

public sealed class 커뮤니티활동상세구매상태이력
{
    public long Id { get; set; }
    public string 구매Id { get; set; } = string.Empty;
    public int 순서 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string 사유Code { get; set; } = string.Empty;
    public DateTime 기록일시Utc { get; set; }
    public 커뮤니티활동상세구매 구매 { get; set; } = null!;
}
