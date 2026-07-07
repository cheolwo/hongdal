namespace Hongdal.Contracts.Common.Orderer;

public static class 공동구매결제단계코드
{
    public const string 상차1차지급 = "PickupFirstPayment";
    public const string 하차2차지급 = "DropoffSecondPayment";
    public const string 분배확인최종지급 = "DistributionConfirmationFinalPayment";
}

public static class 공동구매결제단계상태코드
{
    public const string 대기 = "Waiting";
    public const string 요청가능 = "Requestable";
    public const string 지급완료 = "Paid";
    public const string 차단 = "Blocked";
}

public sealed record 공동구매결제단계정책(
    decimal 상차1차지급비율 = 0.4m,
    decimal 하차2차지급비율 = 0.4m,
    decimal 분배최종지급비율 = 0.2m,
    decimal 분배확인기준비율 = 0.8m);

public sealed record 공동구매결제단계초안(
    string 공동구매Id,
    string 주문자Id,
    decimal 총금액,
    bool 상차완료여부,
    bool 하차완료여부,
    decimal 분배확인율,
    IReadOnlySet<string>? 지급완료단계코드목록 = null,
    string 통화 = "KRW",
    공동구매결제단계정책? 정책 = null);

public sealed record 공동구매결제단계라인(
    string 단계코드,
    string 표시명,
    decimal 비율,
    decimal 금액,
    string 상태,
    string 지급조건);

public sealed record 공동구매결제단계계획(
    공동구매결제단계초안 초안,
    IReadOnlyList<공동구매결제단계라인> 라인목록,
    decimal 지급완료금액,
    decimal 요청가능금액,
    decimal 잔여금액,
    bool 최종지급차단여부,
    string 요약);

public static class 공동구매결제단계계획기
{
    public static 공동구매결제단계계획 계획(공동구매결제단계초안 draft)
    {
        Validate(draft);

        var policy = Normalize정책(draft.정책 ?? new 공동구매결제단계정책());
        var paidMilestones = draft.지급완료단계코드목록 ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pickup금액 = Round금액(draft.총금액 * policy.상차1차지급비율);
        var dropoff금액 = Round금액(draft.총금액 * policy.하차2차지급비율);
        var final금액 = draft.총금액 - pickup금액 - dropoff금액;

        var lines = new[]
        {
            CreateLine(
                공동구매결제단계코드.상차1차지급,
                "상차 1차 지급",
                policy.상차1차지급비율,
                pickup금액,
                draft.상차완료여부,
                paidMilestones,
                "상차 완료 또는 공급자 출고 확인 후 요청"),
            CreateLine(
                공동구매결제단계코드.하차2차지급,
                "하차 2차 지급",
                policy.하차2차지급비율,
                dropoff금액,
                draft.하차완료여부,
                paidMilestones,
                "하차 완료 또는 집단 대표 입고지 도착 확인 후 요청"),
            CreateLine(
                공동구매결제단계코드.분배확인최종지급,
                "분배 확인 최종 지급",
                policy.분배최종지급비율,
                final금액,
                draft.분배확인율 >= policy.분배확인기준비율,
                paidMilestones,
                $"{policy.분배확인기준비율:P0} 이상 분배 확인 후 요청")
        };

        var paid금액 = lines
            .Where(x => x.상태 == 공동구매결제단계상태코드.지급완료)
            .Sum(x => x.금액);
        var requestable금액 = lines
            .Where(x => x.상태 == 공동구매결제단계상태코드.요청가능)
            .Sum(x => x.금액);
        var remaining금액 = draft.총금액 - paid금액;
        var isFinalPaymentBlocked = lines.Any(x =>
            x.단계코드 == 공동구매결제단계코드.분배확인최종지급 &&
            x.상태 == 공동구매결제단계상태코드.차단);

        return new 공동구매결제단계계획(
            draft,
            lines,
            paid금액,
            requestable금액,
            remaining금액,
            isFinalPaymentBlocked,
            Build요약(requestable금액, remaining금액, isFinalPaymentBlocked));
    }

    private static 공동구매결제단계라인 CreateLine(
        string milestoneCode,
        string displayName,
        decimal rate,
        decimal amount,
        bool conditionMet,
        IReadOnlySet<string> paidMilestones,
        string dueCondition)
    {
        var status = Resolve상태(milestoneCode, conditionMet, paidMilestones);
        return new 공동구매결제단계라인(
            milestoneCode,
            displayName,
            rate,
            amount,
            status,
            dueCondition);
    }

    private static string Resolve상태(
        string milestoneCode,
        bool conditionMet,
        IReadOnlySet<string> paidMilestones)
    {
        if (paidMilestones.Contains(milestoneCode))
        {
            return 공동구매결제단계상태코드.지급완료;
        }

        return conditionMet
            ? 공동구매결제단계상태코드.요청가능
            : 공동구매결제단계상태코드.차단;
    }

    private static 공동구매결제단계정책 Normalize정책(공동구매결제단계정책 policy)
    {
        if (policy.상차1차지급비율 < 0 ||
            policy.하차2차지급비율 < 0 ||
            policy.분배최종지급비율 < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(policy), "Payment rates cannot be negative.");
        }

        var total비율 = policy.상차1차지급비율 +
            policy.하차2차지급비율 +
            policy.분배최종지급비율;
        if (total비율 != 1m)
        {
            throw new ArgumentException("Payment milestone rates must sum to 1.");
        }

        if (policy.분배확인기준비율 is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(policy.분배확인기준비율), policy.분배확인기준비율, "Distribution confirmation threshold must be between 0 and 1.");
        }

        return policy;
    }

    private static void Validate(공동구매결제단계초안 draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.공동구매Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.주문자Id);

        if (draft.총금액 <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.총금액), draft.총금액, "Total amount must be greater than zero.");
        }

        if (draft.분배확인율 is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.분배확인율), draft.분배확인율, "Distribution confirmation rate must be between 0 and 1.");
        }
    }

    private static decimal Round금액(decimal value)
        => decimal.Round(value, 0, MidpointRounding.AwayFromZero);

    private static string Build요약(
        decimal requestable금액,
        decimal remaining금액,
        bool isFinalPaymentBlocked)
    {
        if (requestable금액 > 0)
        {
            return $"{requestable금액:N0}원 지급 요청 가능, 잔여 {remaining금액:N0}원";
        }

        return isFinalPaymentBlocked
            ? $"분배 확인율이 부족해 최종 지급은 보류 중, 잔여 {remaining금액:N0}원"
            : $"현재 지급 요청 금액 없음, 잔여 {remaining금액:N0}원";
    }
}
