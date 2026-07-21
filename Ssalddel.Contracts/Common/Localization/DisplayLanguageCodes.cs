namespace Ssalddel.Contracts.Common.Localization;

public static class DisplayLanguageCodes
{
    public const string Korean = "ko-KR";
    public const string English = "en-US";

    public static IReadOnlyList<string> Supported { get; } = [Korean, English];

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
        if (candidate.StartsWith("ko", StringComparison.OrdinalIgnoreCase))
        {
            normalizedCode = Korean;
            return true;
        }

        if (candidate.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            normalizedCode = English;
            return true;
        }

        return false;
    }

    public static string ToNeutralCode(string? value)
        => Normalize(value) == English ? "en" : "ko";

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
}
