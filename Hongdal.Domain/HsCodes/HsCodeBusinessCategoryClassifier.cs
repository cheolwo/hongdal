namespace Hongdal.Domain.HsCodes;

public sealed record HsCodeBusinessCategoryDecision(
    HsCodeBusinessCategory Category,
    string Reason);

public static class HsCodeBusinessCategoryClassifier
{
    public const string FoodReason = "HS chapter 01-24 is treated as food or food-adjacent cargo.";
    public const string GeneralCargoReason = "HS chapter is outside 01-24 and treated as general cargo.";
    public const string UnknownReason = "HS chapter could not be parsed.";

    public static HsCodeBusinessCategoryDecision Classify(string? hsCode)
    {
        var chapter = ParseChapter(hsCode);
        if (chapter is null)
        {
            return new HsCodeBusinessCategoryDecision(HsCodeBusinessCategory.Unknown, UnknownReason);
        }

        return chapter is >= 1 and <= 24
            ? new HsCodeBusinessCategoryDecision(HsCodeBusinessCategory.Food, FoodReason)
            : new HsCodeBusinessCategoryDecision(HsCodeBusinessCategory.GeneralCargo, GeneralCargoReason);
    }

    public static int? ParseChapter(string? hsCode)
    {
        if (string.IsNullOrWhiteSpace(hsCode))
        {
            return null;
        }

        var digits = new string(hsCode.Where(char.IsDigit).Take(2).ToArray());
        if (digits.Length < 2)
        {
            return null;
        }

        return int.TryParse(digits, out var chapter) && chapter is >= 1 and <= 99
            ? chapter
            : null;
    }
}
