using System.Globalization;

namespace Ssalddel.Contracts.Common.Community;

public sealed record CommunityDisplayCountry(
    string Code,
    string KoreanName,
    string EnglishName);

/// <summary>
/// 댓글에 공개할 수 있는 ISO 3166-1 alpha-2 국가 코드를 서버와 모든 client가 같은 이름으로 해석합니다.
/// 위치 추정이나 추천·정렬에는 사용하지 않습니다.
/// </summary>
public static class CommunityDisplayCountryCatalog
{
    private static readonly string[] CommonCountryCodes =
        ["KR", "US", "CN", "JP", "CA", "GB", "AU", "DE", "FR", "VN", "TH"];

    private static readonly IReadOnlyDictionary<string, CommunityDisplayCountry> Countries =
        CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(culture =>
            {
                try
                {
                    return new RegionInfo(culture.Name);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            })
            .Where(region => region is not null)
            .GroupBy(region => region!.TwoLetterISORegionName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()!)
            .ToDictionary(
                region => region.TwoLetterISORegionName,
                region => new CommunityDisplayCountry(
                    region.TwoLetterISORegionName,
                    KoreanName(region.TwoLetterISORegionName, region.EnglishName),
                    region.EnglishName),
                StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<CommunityDisplayCountry> Common { get; } =
        CommonCountryCodes
            .Select(code => Countries.TryGetValue(code, out var country)
                ? country
                : new CommunityDisplayCountry(code, KoreanName(code, code), EnglishName(code)))
            .ToArray();

    public static CommunityDisplayCountry? Find(string? countryCode)
    {
        var normalized = NormalizeCode(countryCode);
        return normalized is not null && Countries.TryGetValue(normalized, out var country)
            ? country
            : null;
    }

    public static string? NormalizeCode(string? countryCode)
    {
        var candidate = countryCode?.Trim();
        return candidate is { Length: 2 }
               && candidate.All(char.IsAsciiLetter)
            ? candidate.ToUpperInvariant()
            : null;
    }

    private static string KoreanName(string code, string fallback)
        => code switch
        {
            "KR" => "대한민국",
            "US" => "미국",
            "CN" => "중국",
            "JP" => "일본",
            "CA" => "캐나다",
            "GB" => "영국",
            "AU" => "오스트레일리아",
            "DE" => "독일",
            "FR" => "프랑스",
            "VN" => "베트남",
            "TH" => "태국",
            _ => fallback
        };

    private static string EnglishName(string code)
        => code switch
        {
            "KR" => "South Korea",
            "US" => "United States",
            "CN" => "China",
            "JP" => "Japan",
            "CA" => "Canada",
            "GB" => "United Kingdom",
            "AU" => "Australia",
            "DE" => "Germany",
            "FR" => "France",
            "VN" => "Vietnam",
            "TH" => "Thailand",
            _ => code
        };
}
