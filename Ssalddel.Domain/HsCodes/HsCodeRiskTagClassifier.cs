namespace Ssalddel.Domain.HsCodes;

public sealed record HsCodeRiskTagDecision(
    HsCodeRiskTagType TagType,
    string Label,
    string Reason);

public static class HsCodeRiskTagClassifier
{
    public static IReadOnlyList<HsCodeRiskTagDecision> Suggest(string? hsCode)
    {
        var chapter = HsCodeBusinessCategoryClassifier.ParseChapter(hsCode);
        if (chapter is null)
        {
            return [];
        }

        var normalized = Normalize(hsCode);
        var tags = new List<HsCodeRiskTagDecision>();

        if (chapter is >= 1 and <= 24)
        {
            tags.Add(new(
                HsCodeRiskTagType.Food,
                "식품 관련",
                "HS chapter 01-24 is treated as food or food-adjacent cargo."));

            tags.Add(new(
                HsCodeRiskTagType.FoodQuarantine,
                "검역/식품신고 확인",
                "Food-related HS codes may require quarantine, ingredient, label, or import notification review."));
        }

        if (chapter == 21)
        {
            tags.Add(new(
                HsCodeRiskTagType.SupplementOrPreparedFoodReview,
                "조제식품/보충제 검토",
                "Chapter 21 can include prepared food products that need ingredient and claim review."));
        }

        if (chapter is >= 28 and <= 38)
        {
            tags.Add(new(
                HsCodeRiskTagType.Chemical,
                "화학물질 확인",
                "Chemical chapters may require substance, safety, or hazardous cargo review."));
        }

        if (chapter is >= 50 and <= 63)
        {
            tags.Add(new(
                HsCodeRiskTagType.Textile,
                "섬유/의류",
                "Textile chapters often need material composition and origin checks."));
        }

        if (chapter == 85)
        {
            tags.Add(new(
                HsCodeRiskTagType.ElectricalCertification,
                "전기/인증 확인",
                "Electrical goods may require certification, radio, or product safety checks."));
        }

        if (normalized.StartsWith("8506", StringComparison.Ordinal) ||
            normalized.StartsWith("8507", StringComparison.Ordinal))
        {
            tags.Add(new(
                HsCodeRiskTagType.BatteryIncludedPossible,
                "배터리 포함 가능",
                "Battery-related HS codes need transport and safety document checks."));
        }

        if (chapter == 94)
        {
            tags.Add(new(
                HsCodeRiskTagType.Furniture,
                "가구/생활용품",
                "Furniture and fixture chapters may need material and component checks."));
        }

        if (tags.Count > 0)
        {
            tags.Add(new(
                HsCodeRiskTagType.BrokerReviewRecommended,
                "관세사 검토 권장",
                "At least one operational risk tag was detected, so broker review is recommended before agency confirmation."));
        }

        return tags;
    }

    private static string Normalize(string? hsCode)
        => new((hsCode ?? string.Empty).Where(char.IsDigit).ToArray());
}
