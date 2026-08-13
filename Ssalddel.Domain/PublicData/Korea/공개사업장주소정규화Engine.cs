using System.Text;
using System.Text.RegularExpressions;

namespace Ssalddel.Domain.PublicData.Korea;

public static partial class 공개사업장주소정규화Engine
{
    public const string RuleRevision = "kr-public-business-building-match-v1";

    public static string? NormalizeRoadAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        var text = Parentheses().Replace(address.Normalize(NormalizationForm.FormKC), " ");
        text = DetailUnit().Replace(text, " ");
        text = Whitespace().Replace(text, " ").Trim().TrimEnd(',', '·', '-').TrimEnd();
        return string.IsNullOrWhiteSpace(text) ? null : text.ToUpperInvariant();
    }

    public static string DecideStatus(string? normalizedAddressKey, int candidateCount) =>
        normalizedAddressKey is null
            ? 공개사업장연결상태Codes.주소부족
            : candidateCount switch
            {
                0 => 공개사업장연결상태Codes.건물후보없음,
                1 => 공개사업장연결상태Codes.연결됨,
                _ => 공개사업장연결상태Codes.복수후보,
            };

    [GeneratedRegex(@"\([^)]*\)", RegexOptions.CultureInvariant)]
    private static partial Regex Parentheses();

    [GeneratedRegex(@"\b(?:지하\s*)?\d+층(?:\s*[-,]?\s*\d+(?:호)?)?.*$|\b\d+호.*$", RegexOptions.CultureInvariant)]
    private static partial Regex DetailUnit();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex Whitespace();
}
