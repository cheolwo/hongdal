namespace Ssalddel.Contracts.Common.Orderer;

public static class 공동구매품목분류코드
{
    public const string 일반커머스 = "general-commerce";
    public const string 냉장냉동먹거리 = "food-cold-chain";
}

public static class 공동구매온도코드
{
    public const string 상온 = "ambient";
    public const string 냉장 = "chilled";
    public const string 냉동 = "frozen";
}

public static class 공동구매물류방식코드
{
    public const string Unknown = "unknown";
    public const string LCL = "lcl";
    public const string FCL = "fcl";
    public const string 국내벌크 = "domestic-bulk";
}

public static class 공동구매캠페인상태코드
{
    public const string 초안 = "초안";
    public const string 수요확인 = "DemandChecking";
    public const string 콜드체인검토 = "ColdChainReview";
    public const string 수입결정 = "ImportDecision";
}

public static class 주문자집단개설신청상태코드
{
    public const string 초안 = "초안";
    public const string Submitted = "Submitted";
    public const string 승인대기 = "PendingApproval";
    public const string ApprovedGroupReady = "ApprovedGroupReady";
    public const string 반려 = "Rejected";
}

public static class 공동구매활성화우선순위코드
{
    public const string 일반 = "Normal";
    public const string 먹거리중심 = "FoodFocused";
    public const string 냉장냉동먹거리중심 = "ColdChainFoodFocused";
}

public sealed record HS먹거리공동구매상품카드(
    string 상품카드Id,
    string 상품명,
    string HS코드,
    string HS표시명,
    string 온도코드,
    string 예상물류방식,
    decimal SuggestedTargetQuantityKg,
    decimal ExpectedUnitPrice,
    bool RequiresImportFoodReview = true,
    bool RequiresMfdsManufacturerReview = true);

public sealed record 주문자집단개설신청초안(
    string ApplicantOrdererId,
    HS먹거리공동구매상품카드 상품카드,
    string 주문자집단배송권키,
    string 주문자집단배송권명,
    decimal 희망수량Kg,
    decimal DesiredUnitPrice,
    bool NonBindingAgreementAccepted,
    string Request메모 = "");

public sealed record 주문자집단개설신청계획(
    주문자집단개설신청초안 초안,
    bool CanSubmit,
    bool IsFoodFocusedCandidate,
    string 제안상태,
    IReadOnlyList<string> RequiredAdminReviewSteps,
    string 요약);

public sealed record 냉장냉동먹거리공동구매초안(
    string 상품명,
    string 주문자집단배송권키,
    decimal TargetQuantityKg,
    decimal CurrentIntentQuantityKg,
    decimal TargetUnitPrice,
    string? HS코드 = null,
    string 품목분류코드 = 공동구매품목분류코드.냉장냉동먹거리,
    string 온도코드 = 공동구매온도코드.냉동,
    string LogisticsMode = 공동구매물류방식코드.FCL,
    bool RequiresImportFoodReview = true,
    bool RequiresMfdsManufacturerReview = true,
    bool RequiresColdStorage = true,
    string Currency = "KRW");

public sealed record 냉장냉동먹거리공동구매계획(
    냉장냉동먹거리공동구매초안 초안,
    decimal DemandProgressRate,
    bool IsDemandThresholdMet,
    bool IsFclCandidate,
    bool HS먹거리후보여부,
    string 활성화우선순위,
    string 제안상태,
    IReadOnlyList<string> RequiredReviewSteps,
    string CommunityCategory,
    string 요약);

public static class 냉장냉동먹거리공동구매계획기
{
    private const decimal DemandThresholdRate = 0.7m;
    private const decimal FclFoodCandidateQuantityKg = 10000m;

    public static 냉장냉동먹거리공동구매계획 계획(냉장냉동먹거리공동구매초안 draft)
    {
        Validate(draft);

        var progressRate = decimal.Round(draft.CurrentIntentQuantityKg / draft.TargetQuantityKg, 4, MidpointRounding.AwayFromZero);
        var isDemandThresholdMet = progressRate >= DemandThresholdRate;
        var HS먹거리후보여부 = HS먹거리후보판정(draft.HS코드);
        var isFclCandidate = IsFclCandidate(draft);
        var activationPriority = Resolve활성화우선순위(draft, HS먹거리후보여부);
        var requiredSteps = ResolveRequiredReviewSteps(draft, HS먹거리후보여부);
        var suggestedStatus = Resolve제안상태(isDemandThresholdMet, requiredSteps);

        return new 냉장냉동먹거리공동구매계획(
            draft,
            progressRate,
            isDemandThresholdMet,
            isFclCandidate,
            HS먹거리후보여부,
            activationPriority,
            suggestedStatus,
            requiredSteps,
            CommunityCategory: "먹거리 공동주문",
            요약: Build요약(draft, progressRate, isFclCandidate));
    }

    private static void Validate(냉장냉동먹거리공동구매초안 draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.상품명);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.주문자집단배송권키);

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

    private static bool IsFclCandidate(냉장냉동먹거리공동구매초안 draft)
        => string.Equals(draft.LogisticsMode, 공동구매물류방식코드.FCL, StringComparison.OrdinalIgnoreCase)
            || draft.TargetQuantityKg >= FclFoodCandidateQuantityKg;

    private static bool HS먹거리후보판정(string? hsCode)
    {
        var digits = new string((hsCode ?? string.Empty).Where(char.IsDigit).Take(2).ToArray());
        return digits.Length == 2 &&
            int.TryParse(digits, out var chapter) &&
            chapter is >= 1 and <= 24;
    }

    private static string Resolve활성화우선순위(
        냉장냉동먹거리공동구매초안 draft,
        bool HS먹거리후보여부)
    {
        if (!HS먹거리후보여부)
        {
            return 공동구매활성화우선순위코드.일반;
        }

        return string.Equals(draft.온도코드, 공동구매온도코드.냉동, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(draft.온도코드, 공동구매온도코드.냉장, StringComparison.OrdinalIgnoreCase) ||
            draft.RequiresColdStorage
            ? 공동구매활성화우선순위코드.냉장냉동먹거리중심
            : 공동구매활성화우선순위코드.먹거리중심;
    }

    private static IReadOnlyList<string> ResolveRequiredReviewSteps(
        냉장냉동먹거리공동구매초안 draft,
        bool HS먹거리후보여부)
    {
        var steps = new List<string>();

        if (HS먹거리후보여부 ||
            string.Equals(draft.품목분류코드, 공동구매품목분류코드.냉장냉동먹거리, StringComparison.OrdinalIgnoreCase))
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
            string.Equals(draft.온도코드, 공동구매온도코드.냉동, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(draft.온도코드, 공동구매온도코드.냉장, StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("냉장/냉동 창고와 운송 가능 여부 확인");
        }

        return steps;
    }

    private static string Resolve제안상태(bool isDemandThresholdMet, IReadOnlyList<string> requiredSteps)
    {
        if (!isDemandThresholdMet)
        {
            return 공동구매캠페인상태코드.수요확인;
        }

        return requiredSteps.Count > 0
            ? 공동구매캠페인상태코드.콜드체인검토
            : 공동구매캠페인상태코드.수입결정;
    }

    private static string Build요약(
        냉장냉동먹거리공동구매초안 draft,
        decimal progressRate,
        bool isFclCandidate)
    {
        var percent = decimal.Round(progressRate * 100m, 1, MidpointRounding.AwayFromZero);
        var mode = isFclCandidate ? "FCL 후보" : "대량 공동주문 후보";
        return $"{draft.상품명} {draft.TargetQuantityKg:N0}kg 목표, 수요 {percent:N1}% 달성, {mode}";
    }
}

public static class 주문자집단개설신청계획기
{
    public static 주문자집단개설신청계획 계획(주문자집단개설신청초안 draft)
    {
        Validate(draft);

        var isFoodCandidate = HS먹거리후보판정(draft.상품카드.HS코드);
        var reviewSteps = ResolveReviewSteps(draft, isFoodCandidate);
        var canSubmit = draft.NonBindingAgreementAccepted && isFoodCandidate;
        var status = canSubmit
            ? 주문자집단개설신청상태코드.승인대기
            : 주문자집단개설신청상태코드.초안;

        return new 주문자집단개설신청계획(
            draft,
            canSubmit,
            isFoodCandidate,
            status,
            reviewSteps,
            Build요약(draft, isFoodCandidate));
    }

    private static void Validate(주문자집단개설신청초안 draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.ApplicantOrdererId);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.주문자집단배송권키);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.주문자집단배송권명);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.상품카드.상품카드Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.상품카드.상품명);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.상품카드.HS코드);

        if (draft.희망수량Kg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.희망수량Kg), draft.희망수량Kg, "Desired quantity must be greater than zero.");
        }

        if (draft.DesiredUnitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.DesiredUnitPrice), draft.DesiredUnitPrice, "Desired unit price must be greater than zero.");
        }
    }

    private static bool HS먹거리후보판정(string? hsCode)
    {
        var digits = new string((hsCode ?? string.Empty).Where(char.IsDigit).Take(2).ToArray());
        return digits.Length == 2 &&
            int.TryParse(digits, out var chapter) &&
            chapter is >= 1 and <= 24;
    }

    private static IReadOnlyList<string> ResolveReviewSteps(
        주문자집단개설신청초안 draft,
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

        if (draft.상품카드.RequiresImportFoodReview)
        {
            steps.Add("수입식품 신고/검역 검토");
        }

        if (draft.상품카드.RequiresMfdsManufacturerReview)
        {
            steps.Add("해외제조업소/수입식품 제품 조회");
        }

        if (draft.상품카드.온도코드 is 공동구매온도코드.냉동 or 공동구매온도코드.냉장)
        {
            steps.Add("냉장/냉동 보관 및 운송 검토");
        }

        return steps;
    }

    private static string Build요약(
        주문자집단개설신청초안 draft,
        bool isFoodCandidate)
    {
        var focus = isFoodCandidate ? "먹거리 공동주문 후보" : "일반 공동주문 후보";
        return $"{draft.주문자집단배송권명}에서 {draft.상품카드.상품명} {draft.희망수량Kg:N0}kg 구매 의향으로 {focus} 개설을 신청합니다.";
    }
}
