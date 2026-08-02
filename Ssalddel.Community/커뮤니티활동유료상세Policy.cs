using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Community;

public sealed record 커뮤니티활동유료상세PolicyDecision(
    bool 허용,
    string Code,
    string 메시지)
{
    public static 커뮤니티활동유료상세PolicyDecision Allow()
        => new(true, "Allowed", string.Empty);

    public static 커뮤니티활동유료상세PolicyDecision Reject(string code, string message)
        => new(false, code, message);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityActivityPaidDetail,
    SsalddelCodeLayer.Domain,
    "유료 상세 등록, 구매 금액과 구매 상태 전이 가능 여부를 순수 판정합니다.",
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.None,
    Boundary = "DB를 읽거나 결제와 열람권 상태를 직접 변경하지 않습니다.")]
public static class 커뮤니티활동유료상세Policy
{
    public const int 최대가격금액 = 1_000_000;

    public static 커뮤니티활동유료상세PolicyDecision 등록검증(
        long 게시글Id,
        string? 공개미리보기,
        string? 상세내용,
        int 가격금액,
        string? 통화Code)
    {
        if (게시글Id <= 0)
            return 커뮤니티활동유료상세PolicyDecision.Reject("PostRequired", "게시글Id가 필요합니다.");
        if (string.IsNullOrWhiteSpace(공개미리보기) || 공개미리보기.Trim().Length > 500)
            return 커뮤니티활동유료상세PolicyDecision.Reject("PreviewInvalid", "공개 미리보기는 1자 이상 500자 이하여야 합니다.");
        if (string.IsNullOrWhiteSpace(상세내용) || 상세내용.Trim().Length > 20_000)
            return 커뮤니티활동유료상세PolicyDecision.Reject("ContentInvalid", "상세 내용은 1자 이상 20,000자 이하여야 합니다.");
        if (가격금액 <= 0 || 가격금액 > 최대가격금액)
            return 커뮤니티활동유료상세PolicyDecision.Reject("PriceInvalid", $"가격은 1원 이상 {최대가격금액:N0}원 이하여야 합니다.");
        if (!string.Equals(통화Code?.Trim(), "KRW", StringComparison.OrdinalIgnoreCase))
            return 커뮤니티활동유료상세PolicyDecision.Reject("CurrencyUnsupported", "현재 FakePG 검증은 KRW만 지원합니다.");
        return 커뮤니티활동유료상세PolicyDecision.Allow();
    }

    public static 커뮤니티활동유료상세PolicyDecision 구매검증(
        string 판매상태,
        string 판매자UserId,
        string 구매자UserId,
        int 판매금액,
        int 요청금액)
    {
        if (!string.Equals(판매상태, 커뮤니티활동유료상세판매상태.판매중, StringComparison.Ordinal))
            return 커뮤니티활동유료상세PolicyDecision.Reject("SaleUnavailable", "현재 구매할 수 없는 활동 상세입니다.");
        if (string.Equals(판매자UserId, 구매자UserId, StringComparison.Ordinal))
            return 커뮤니티활동유료상세PolicyDecision.Reject("SelfPurchase", "작성자 본인은 자신의 상세 내용을 구매할 수 없습니다.");
        if (요청금액 != 판매금액)
            return 커뮤니티활동유료상세PolicyDecision.Reject("AmountMismatch", "결제금액이 활동 상세 가격과 다릅니다.");
        return 커뮤니티활동유료상세PolicyDecision.Allow();
    }

    public static bool 상태전이가능한가(string 현재상태, string 다음상태)
        => (현재상태, 다음상태) switch
        {
            (커뮤니티활동상세구매상태.요청됨, 커뮤니티활동상세구매상태.결제승인됨) => true,
            (커뮤니티활동상세구매상태.결제승인됨, 커뮤니티활동상세구매상태.열람권발급됨) => true,
            (커뮤니티활동상세구매상태.요청됨, 커뮤니티활동상세구매상태.실패) => true,
            (커뮤니티활동상세구매상태.결제승인됨, 커뮤니티활동상세구매상태.실패) => true,
            (커뮤니티활동상세구매상태.요청됨, 커뮤니티활동상세구매상태.취소됨) => true,
            (커뮤니티활동상세구매상태.열람권발급됨, 커뮤니티활동상세구매상태.환불됨) => true,
            _ => false
        };
}
