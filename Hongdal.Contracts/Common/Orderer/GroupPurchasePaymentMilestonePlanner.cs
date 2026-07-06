namespace Hongdal.Contracts.Common.Orderer;

public static class GroupPurchasePaymentMilestoneCode
{
    public const string PickupFirstPayment = "PickupFirstPayment";
    public const string DropoffSecondPayment = "DropoffSecondPayment";
    public const string DistributionConfirmationFinalPayment = "DistributionConfirmationFinalPayment";
}

public static class GroupPurchasePaymentMilestoneStatusCode
{
    public const string Waiting = "Waiting";
    public const string Requestable = "Requestable";
    public const string Paid = "Paid";
    public const string Blocked = "Blocked";
}

public sealed record GroupPurchasePaymentMilestonePolicy(
    decimal PickupFirstPaymentRate = 0.4m,
    decimal DropoffSecondPaymentRate = 0.4m,
    decimal DistributionFinalPaymentRate = 0.2m,
    decimal DistributionConfirmationThresholdRate = 0.8m);

public sealed record GroupPurchasePaymentMilestoneDraft(
    string GroupPurchaseId,
    string OrdererId,
    decimal TotalAmount,
    bool IsPickupCompleted,
    bool IsDropoffCompleted,
    decimal DistributionConfirmationRate,
    IReadOnlySet<string>? PaidMilestoneCodes = null,
    string Currency = "KRW",
    GroupPurchasePaymentMilestonePolicy? Policy = null);

public sealed record GroupPurchasePaymentMilestoneLine(
    string MilestoneCode,
    string DisplayName,
    decimal Rate,
    decimal Amount,
    string Status,
    string DueCondition);

public sealed record GroupPurchasePaymentMilestonePlan(
    GroupPurchasePaymentMilestoneDraft Draft,
    IReadOnlyList<GroupPurchasePaymentMilestoneLine> Lines,
    decimal PaidAmount,
    decimal RequestableAmount,
    decimal RemainingAmount,
    bool IsFinalPaymentBlocked,
    string Summary);

public static class GroupPurchasePaymentMilestonePlanner
{
    public static GroupPurchasePaymentMilestonePlan Plan(GroupPurchasePaymentMilestoneDraft draft)
    {
        Validate(draft);

        var policy = NormalizePolicy(draft.Policy ?? new GroupPurchasePaymentMilestonePolicy());
        var paidMilestones = draft.PaidMilestoneCodes ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pickupAmount = RoundAmount(draft.TotalAmount * policy.PickupFirstPaymentRate);
        var dropoffAmount = RoundAmount(draft.TotalAmount * policy.DropoffSecondPaymentRate);
        var finalAmount = draft.TotalAmount - pickupAmount - dropoffAmount;

        var lines = new[]
        {
            CreateLine(
                GroupPurchasePaymentMilestoneCode.PickupFirstPayment,
                "상차 1차 지급",
                policy.PickupFirstPaymentRate,
                pickupAmount,
                draft.IsPickupCompleted,
                paidMilestones,
                "상차 완료 또는 공급자 출고 확인 후 요청"),
            CreateLine(
                GroupPurchasePaymentMilestoneCode.DropoffSecondPayment,
                "하차 2차 지급",
                policy.DropoffSecondPaymentRate,
                dropoffAmount,
                draft.IsDropoffCompleted,
                paidMilestones,
                "하차 완료 또는 집단 대표 입고지 도착 확인 후 요청"),
            CreateLine(
                GroupPurchasePaymentMilestoneCode.DistributionConfirmationFinalPayment,
                "분배 확인 최종 지급",
                policy.DistributionFinalPaymentRate,
                finalAmount,
                draft.DistributionConfirmationRate >= policy.DistributionConfirmationThresholdRate,
                paidMilestones,
                $"{policy.DistributionConfirmationThresholdRate:P0} 이상 분배 확인 후 요청")
        };

        var paidAmount = lines
            .Where(x => x.Status == GroupPurchasePaymentMilestoneStatusCode.Paid)
            .Sum(x => x.Amount);
        var requestableAmount = lines
            .Where(x => x.Status == GroupPurchasePaymentMilestoneStatusCode.Requestable)
            .Sum(x => x.Amount);
        var remainingAmount = draft.TotalAmount - paidAmount;
        var isFinalPaymentBlocked = lines.Any(x =>
            x.MilestoneCode == GroupPurchasePaymentMilestoneCode.DistributionConfirmationFinalPayment &&
            x.Status == GroupPurchasePaymentMilestoneStatusCode.Blocked);

        return new GroupPurchasePaymentMilestonePlan(
            draft,
            lines,
            paidAmount,
            requestableAmount,
            remainingAmount,
            isFinalPaymentBlocked,
            BuildSummary(requestableAmount, remainingAmount, isFinalPaymentBlocked));
    }

    private static GroupPurchasePaymentMilestoneLine CreateLine(
        string milestoneCode,
        string displayName,
        decimal rate,
        decimal amount,
        bool conditionMet,
        IReadOnlySet<string> paidMilestones,
        string dueCondition)
    {
        var status = ResolveStatus(milestoneCode, conditionMet, paidMilestones);
        return new GroupPurchasePaymentMilestoneLine(
            milestoneCode,
            displayName,
            rate,
            amount,
            status,
            dueCondition);
    }

    private static string ResolveStatus(
        string milestoneCode,
        bool conditionMet,
        IReadOnlySet<string> paidMilestones)
    {
        if (paidMilestones.Contains(milestoneCode))
        {
            return GroupPurchasePaymentMilestoneStatusCode.Paid;
        }

        return conditionMet
            ? GroupPurchasePaymentMilestoneStatusCode.Requestable
            : GroupPurchasePaymentMilestoneStatusCode.Blocked;
    }

    private static GroupPurchasePaymentMilestonePolicy NormalizePolicy(GroupPurchasePaymentMilestonePolicy policy)
    {
        if (policy.PickupFirstPaymentRate < 0 ||
            policy.DropoffSecondPaymentRate < 0 ||
            policy.DistributionFinalPaymentRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(policy), "Payment rates cannot be negative.");
        }

        var totalRate = policy.PickupFirstPaymentRate +
            policy.DropoffSecondPaymentRate +
            policy.DistributionFinalPaymentRate;
        if (totalRate != 1m)
        {
            throw new ArgumentException("Payment milestone rates must sum to 1.");
        }

        if (policy.DistributionConfirmationThresholdRate is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(policy.DistributionConfirmationThresholdRate), policy.DistributionConfirmationThresholdRate, "Distribution confirmation threshold must be between 0 and 1.");
        }

        return policy;
    }

    private static void Validate(GroupPurchasePaymentMilestoneDraft draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.GroupPurchaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.OrdererId);

        if (draft.TotalAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.TotalAmount), draft.TotalAmount, "Total amount must be greater than zero.");
        }

        if (draft.DistributionConfirmationRate is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.DistributionConfirmationRate), draft.DistributionConfirmationRate, "Distribution confirmation rate must be between 0 and 1.");
        }
    }

    private static decimal RoundAmount(decimal value)
        => decimal.Round(value, 0, MidpointRounding.AwayFromZero);

    private static string BuildSummary(
        decimal requestableAmount,
        decimal remainingAmount,
        bool isFinalPaymentBlocked)
    {
        if (requestableAmount > 0)
        {
            return $"{requestableAmount:N0}원 지급 요청 가능, 잔여 {remainingAmount:N0}원";
        }

        return isFinalPaymentBlocked
            ? $"분배 확인율이 부족해 최종 지급은 보류 중, 잔여 {remainingAmount:N0}원"
            : $"현재 지급 요청 금액 없음, 잔여 {remainingAmount:N0}원";
    }
}
