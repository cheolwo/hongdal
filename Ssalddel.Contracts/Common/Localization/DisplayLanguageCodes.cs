namespace Ssalddel.Contracts.Common.Localization;

public sealed record DisplayLanguageProfile(
    string Code,
    string NeutralCode,
    string PathSegment,
    string NativeName,
    string EnglishName);

public static class DisplayLanguageCodes
{
    public const string Korean = "ko-KR";
    public const string English = "en-US";
    public const string Japanese = "ja-JP";

    public static IReadOnlyList<DisplayLanguageProfile> Profiles { get; } =
    [
        new(Korean, "ko", "ko", "한국어", "Korean"),
        new(English, "en", "en", "English", "English"),
        new(Japanese, "ja", "ja", "日本語", "Japanese")
    ];

    public static IReadOnlyList<string> Supported { get; } = Profiles
        .Select(profile => profile.Code)
        .ToArray();

    public static string Normalize(string? value, string fallback = Korean)
    {
        var normalizedFallback = TryNormalize(fallback, out var fallbackCode)
            ? fallbackCode
            : Korean;

        return TryNormalize(value, out var normalizedCode)
            ? normalizedCode
            : normalizedFallback;
    }

    public static bool TryNormalize(string? value, out string normalizedCode)
    {
        normalizedCode = Korean;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();
        var primaryLanguage = candidate
            .Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
        var profile = Profiles.FirstOrDefault(item =>
            string.Equals(item.Code, candidate, StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.NeutralCode, primaryLanguage, StringComparison.OrdinalIgnoreCase));
        if (profile is not null)
        {
            normalizedCode = profile.Code;
            return true;
        }

        return false;
    }

    public static bool TryFromPathSegment(string? value, out string languageCode)
    {
        languageCode = Korean;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var profile = Profiles.FirstOrDefault(item =>
            string.Equals(item.PathSegment, value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (profile is null)
        {
            return false;
        }

        languageCode = profile.Code;
        return true;
    }

    public static string ToNeutralCode(string? value)
        => GetProfile(value).NeutralCode;

    public static string ToPathSegment(string? value)
        => GetProfile(value).PathSegment;

    public static string NativeName(string? value)
        => GetProfile(value).NativeName;

    public static string EnglishName(string? value)
        => GetProfile(value).EnglishName;

    public static string Select(
        string? languageCode,
        string korean,
        string english,
        string? japanese = null)
        => Normalize(languageCode) switch
        {
            Japanese => japanese ?? english,
            English => english,
            _ => korean
        };

    public static bool TryResolveAcceptLanguage(string? value, out string languageCode)
    {
        languageCode = Korean;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidates = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((item, index) =>
            {
                var parts = item.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var quality = 1m;
                var qualityPart = parts.Skip(1)
                    .FirstOrDefault(part => part.StartsWith("q=", StringComparison.OrdinalIgnoreCase));
                if (qualityPart is not null
                    && !decimal.TryParse(
                        qualityPart.AsSpan(2),
                        System.Globalization.NumberStyles.AllowDecimalPoint,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out quality))
                {
                    quality = 0m;
                }

                return new { Code = parts[0], Quality = quality, Index = index };
            })
            .Where(candidate => candidate.Quality > 0m)
            .OrderByDescending(candidate => candidate.Quality)
            .ThenBy(candidate => candidate.Index);

        foreach (var candidate in candidates)
        {
            if (TryNormalize(candidate.Code, out languageCode))
            {
                return true;
            }
        }

        return false;
    }

    private static DisplayLanguageProfile GetProfile(string? value)
    {
        var normalized = Normalize(value);
        return Profiles.First(profile =>
            string.Equals(profile.Code, normalized, StringComparison.Ordinal));
    }
}
