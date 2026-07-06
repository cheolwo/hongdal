namespace Hongdal.Contracts.Common.Orderer;

public static class GroupPurchaseCategoryCode
{
    public const string GeneralCommerce = "general-commerce";
    public const string FoodColdChain = "food-cold-chain";
}

public static class GroupPurchaseTemperatureCode
{
    public const string Ambient = "ambient";
    public const string Chilled = "chilled";
    public const string Frozen = "frozen";
}

public static class GroupPurchaseLogisticsModeCode
{
    public const string Unknown = "unknown";
    public const string Lcl = "lcl";
    public const string Fcl = "fcl";
    public const string DomesticBulk = "domestic-bulk";
}

public static class GroupPurchaseCampaignStatusCode
{
    public const string Draft = "Draft";
    public const string DemandChecking = "DemandChecking";
    public const string ColdChainReview = "ColdChainReview";
    public const string ImportDecision = "ImportDecision";
}

public static class OrdererGroupOpeningApplicationStatusCode
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string PendingApproval = "PendingApproval";
    public const string ApprovedGroupReady = "ApprovedGroupReady";
    public const string Rejected = "Rejected";
}

public static class GroupPurchaseActivationPriorityCode
{
    public const string Normal = "Normal";
    public const string FoodFocused = "FoodFocused";
    public const string ColdChainFoodFocused = "ColdChainFoodFocused";
}

public sealed record HsFoodGroupPurchaseProductCard(
    string ProductCardId,
    string ProductName,
    string HsCode,
    string HsDisplayName,
    string TemperatureCode,
    string ExpectedLogisticsMode,
    decimal SuggestedTargetQuantityKg,
    decimal ExpectedUnitPrice,
    bool RequiresImportFoodReview = true,
    bool RequiresMfdsManufacturerReview = true);

public sealed record OrdererGroupOpeningApplicationDraft(
    string ApplicantOrdererId,
    HsFoodGroupPurchaseProductCard ProductCard,
    string OrdererGroupScopeKey,
    string OrdererGroupScopeName,
    decimal DesiredQuantityKg,
    decimal DesiredUnitPrice,
    bool NonBindingAgreementAccepted,
    string RequestMemo = "");

public sealed record OrdererGroupOpeningApplicationPlan(
    OrdererGroupOpeningApplicationDraft Draft,
    bool CanSubmit,
    bool IsFoodFocusedCandidate,
    string SuggestedStatus,
    IReadOnlyList<string> RequiredAdminReviewSteps,
    string Summary);

public sealed record ColdChainFoodGroupPurchaseDraft(
    string ProductName,
    string OrdererGroupScopeKey,
    decimal TargetQuantityKg,
    decimal CurrentIntentQuantityKg,
    decimal TargetUnitPrice,
    string? HsCode = null,
    string CategoryCode = GroupPurchaseCategoryCode.FoodColdChain,
    string TemperatureCode = GroupPurchaseTemperatureCode.Frozen,
    string LogisticsMode = GroupPurchaseLogisticsModeCode.Fcl,
    bool RequiresImportFoodReview = true,
    bool RequiresMfdsManufacturerReview = true,
    bool RequiresColdStorage = true,
    string Currency = "KRW");

public sealed record ColdChainFoodGroupPurchasePlan(
    ColdChainFoodGroupPurchaseDraft Draft,
    decimal DemandProgressRate,
    bool IsDemandThresholdMet,
    bool IsFclCandidate,
    bool IsHsFoodCandidate,
    string ActivationPriority,
    string SuggestedStatus,
    IReadOnlyList<string> RequiredReviewSteps,
    string CommunityCategory,
    string Summary);

public static class ColdChainFoodGroupPurchasePlanner
{
    private const decimal DemandThresholdRate = 0.7m;
    private const decimal FclFoodCandidateQuantityKg = 10000m;

    public static ColdChainFoodGroupPurchasePlan Plan(ColdChainFoodGroupPurchaseDraft draft)
    {
        Validate(draft);

        var progressRate = decimal.Round(draft.CurrentIntentQuantityKg / draft.TargetQuantityKg, 4, MidpointRounding.AwayFromZero);
        var isDemandThresholdMet = progressRate >= DemandThresholdRate;
        var isHsFoodCandidate = IsHsFoodCandidate(draft.HsCode);
        var isFclCandidate = IsFclCandidate(draft);
        var activationPriority = ResolveActivationPriority(draft, isHsFoodCandidate);
        var requiredSteps = ResolveRequiredReviewSteps(draft, isHsFoodCandidate);
        var suggestedStatus = ResolveSuggestedStatus(isDemandThresholdMet, requiredSteps);

        return new ColdChainFoodGroupPurchasePlan(
            draft,
            progressRate,
            isDemandThresholdMet,
            isFclCandidate,
            isHsFoodCandidate,
            activationPriority,
            suggestedStatus,
            requiredSteps,
            CommunityCategory: "먹거리 공동주문",
            Summary: BuildSummary(draft, progressRate, isFclCandidate));
    }

    private static void Validate(ColdChainFoodGroupPurchaseDraft draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.ProductName);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.OrdererGroupScopeKey);

        if (draft.TargetQuantityKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.TargetQuantityKg), draft.TargetQuantityKg, "Target quantity must be greater than zero.");
        }

        if (draft.CurrentIntentQuantityKg < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.CurrentIntentQuantityKg), draft.CurrentIntentQuantityKg, "Current intent quantity cannot be negative.");
        }

        if (draft.TargetUnitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.TargetUnitPrice), draft.TargetUnitPrice, "Target unit price must be greater than zero.");
        }
    }

    private static bool IsFclCandidate(ColdChainFoodGroupPurchaseDraft draft)
        => string.Equals(draft.LogisticsMode, GroupPurchaseLogisticsModeCode.Fcl, StringComparison.OrdinalIgnoreCase)
            || draft.TargetQuantityKg >= FclFoodCandidateQuantityKg;

    private static bool IsHsFoodCandidate(string? hsCode)
    {
        var digits = new string((hsCode ?? string.Empty).Where(char.IsDigit).Take(2).ToArray());
        return digits.Length == 2 &&
            int.TryParse(digits, out var chapter) &&
            chapter is >= 1 and <= 24;
    }

    private static string ResolveActivationPriority(
        ColdChainFoodGroupPurchaseDraft draft,
        bool isHsFoodCandidate)
    {
        if (!isHsFoodCandidate)
        {
            return GroupPurchaseActivationPriorityCode.Normal;
        }

        return string.Equals(draft.TemperatureCode, GroupPurchaseTemperatureCode.Frozen, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(draft.TemperatureCode, GroupPurchaseTemperatureCode.Chilled, StringComparison.OrdinalIgnoreCase) ||
            draft.RequiresColdStorage
            ? GroupPurchaseActivationPriorityCode.ColdChainFoodFocused
            : GroupPurchaseActivationPriorityCode.FoodFocused;
    }

    private static IReadOnlyList<string> ResolveRequiredReviewSteps(
        ColdChainFoodGroupPurchaseDraft draft,
        bool isHsFoodCandidate)
    {
        var steps = new List<string>();

        if (isHsFoodCandidate ||
            string.Equals(draft.CategoryCode, GroupPurchaseCategoryCode.FoodColdChain, StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("HS 식품 분류 확인");
        }

        if (draft.RequiresImportFoodReview)
        {
            steps.Add("수입식품 신고/검역 검토");
        }

        if (draft.RequiresMfdsManufacturerReview)
        {
            steps.Add("해외제조업소/수입식품 제품 조회");
        }

        if (draft.RequiresColdStorage ||
            string.Equals(draft.TemperatureCode, GroupPurchaseTemperatureCode.Frozen, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(draft.TemperatureCode, GroupPurchaseTemperatureCode.Chilled, StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("냉장/냉동 창고와 운송 가능 여부 확인");
        }

        return steps;
    }

    private static string ResolveSuggestedStatus(bool isDemandThresholdMet, IReadOnlyList<string> requiredSteps)
    {
        if (!isDemandThresholdMet)
        {
            return GroupPurchaseCampaignStatusCode.DemandChecking;
        }

        return requiredSteps.Count > 0
            ? GroupPurchaseCampaignStatusCode.ColdChainReview
            : GroupPurchaseCampaignStatusCode.ImportDecision;
    }

    private static string BuildSummary(
        ColdChainFoodGroupPurchaseDraft draft,
        decimal progressRate,
        bool isFclCandidate)
    {
        var percent = decimal.Round(progressRate * 100m, 1, MidpointRounding.AwayFromZero);
        var mode = isFclCandidate ? "FCL 후보" : "대량 공동주문 후보";
        return $"{draft.ProductName} {draft.TargetQuantityKg:N0}kg 목표, 수요 {percent:N1}% 달성, {mode}";
    }
}

public static class OrdererGroupOpeningApplicationPlanner
{
    public static OrdererGroupOpeningApplicationPlan Plan(OrdererGroupOpeningApplicationDraft draft)
    {
        Validate(draft);

        var isFoodCandidate = IsHsFoodCandidate(draft.ProductCard.HsCode);
        var reviewSteps = ResolveReviewSteps(draft, isFoodCandidate);
        var canSubmit = draft.NonBindingAgreementAccepted && isFoodCandidate;
        var status = canSubmit
            ? OrdererGroupOpeningApplicationStatusCode.PendingApproval
            : OrdererGroupOpeningApplicationStatusCode.Draft;

        return new OrdererGroupOpeningApplicationPlan(
            draft,
            canSubmit,
            isFoodCandidate,
            status,
            reviewSteps,
            BuildSummary(draft, isFoodCandidate));
    }

    private static void Validate(OrdererGroupOpeningApplicationDraft draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.ApplicantOrdererId);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.OrdererGroupScopeKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.OrdererGroupScopeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.ProductCard.ProductCardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.ProductCard.ProductName);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.ProductCard.HsCode);

        if (draft.DesiredQuantityKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.DesiredQuantityKg), draft.DesiredQuantityKg, "Desired quantity must be greater than zero.");
        }

        if (draft.DesiredUnitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.DesiredUnitPrice), draft.DesiredUnitPrice, "Desired unit price must be greater than zero.");
        }
    }

    private static bool IsHsFoodCandidate(string? hsCode)
    {
        var digits = new string((hsCode ?? string.Empty).Where(char.IsDigit).Take(2).ToArray());
        return digits.Length == 2 &&
            int.TryParse(digits, out var chapter) &&
            chapter is >= 1 and <= 24;
    }

    private static IReadOnlyList<string> ResolveReviewSteps(
        OrdererGroupOpeningApplicationDraft draft,
        bool isFoodCandidate)
    {
        var steps = new List<string>
        {
            "주문자 집단 범위 승인",
            "개설자 신청 내용 검토"
        };

        if (isFoodCandidate)
        {
            steps.Add("HS 식품 코드 확인");
        }

        if (draft.ProductCard.RequiresImportFoodReview)
        {
            steps.Add("수입식품 신고/검역 검토");
        }

        if (draft.ProductCard.RequiresMfdsManufacturerReview)
        {
            steps.Add("해외제조업소/수입식품 제품 조회");
        }

        if (draft.ProductCard.TemperatureCode is GroupPurchaseTemperatureCode.Frozen or GroupPurchaseTemperatureCode.Chilled)
        {
            steps.Add("냉장/냉동 보관 및 운송 검토");
        }

        return steps;
    }

    private static string BuildSummary(
        OrdererGroupOpeningApplicationDraft draft,
        bool isFoodCandidate)
    {
        var focus = isFoodCandidate ? "먹거리 공동주문 후보" : "일반 공동주문 후보";
        return $"{draft.OrdererGroupScopeName}에서 {draft.ProductCard.ProductName} {draft.DesiredQuantityKg:N0}kg 구매 의향으로 {focus} 개설을 신청합니다.";
    }
}
