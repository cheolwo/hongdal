using System.Globalization;
using System.Text.Json;

namespace Ssalddel.Services.External.Apify.YouTube;

internal static class ApifyYouTubeDatasetJson
{
    public static bool TryGetProperty(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    public static string? GetString(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out var value))
            {
                continue;
            }

            var result = value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }
        }

        return null;
    }

    public static string? NormalizeText(string? value, int maxLength)
    {
        var normalized = string.Join(
            ' ',
            (value ?? string.Empty).Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length == 0
            ? null
            : normalized.Length <= maxLength
                ? normalized
                : normalized[..maxLength];
    }

    public static decimal? GetNonNegativeDecimal(JsonElement element, params string[] propertyNames)
    {
        var value = GetString(element, propertyNames);
        return decimal.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed)
            && parsed >= 0
            ? parsed
            : null;
    }

    public static long? GetNonNegativeCount(JsonElement element, params string[] propertyNames)
    {
        var value = GetString(element, propertyNames);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().Replace(",", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        var multiplier = 1m;
        if (normalized.EndsWith('K'))
        {
            multiplier = 1_000m;
            normalized = normalized[..^1];
        }
        else if (normalized.EndsWith('M'))
        {
            multiplier = 1_000_000m;
            normalized = normalized[..^1];
        }
        else if (normalized.EndsWith('B'))
        {
            multiplier = 1_000_000_000m;
            normalized = normalized[..^1];
        }

        if (!decimal.TryParse(
                normalized,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed)
            || parsed < 0
            || parsed * multiplier > long.MaxValue)
        {
            return null;
        }

        return decimal.ToInt64(decimal.Round(parsed * multiplier, 0, MidpointRounding.AwayFromZero));
    }

    public static bool GetBoolean(
        JsonElement element,
        bool defaultValue,
        params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out var value))
            {
                continue;
            }

            if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                return value.GetBoolean();
            }

            if (bool.TryParse(value.ToString(), out var parsed))
            {
                return parsed;
            }
        }

        return defaultValue;
    }

    public static DateTime? GetUtcDateTime(JsonElement element, params string[] propertyNames)
    {
        var value = GetString(element, propertyNames);
        return DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed)
            ? parsed.UtcDateTime
            : null;
    }
}
