using System.Text.RegularExpressions;
using Ssalddel.Domain.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal static partial class UsdaAms공개사업체LocationParser
{
    public static LocationResult Parse(string? locationAddress)
    {
        if (string.IsNullOrWhiteSpace(locationAddress))
        {
            return LocationResult.Unparsed;
        }

        var segments = locationAddress
            .Split(',', StringSplitOptions.RemoveEmptyEntries
                        | StringSplitOptions.TrimEntries);
        for (var index = segments.Length - 1; index >= 0; index--)
        {
            if (!TryReadState(segments[index], out var stateCode))
            {
                continue;
            }

            if (index == 0)
            {
                return new LocationResult(
                    string.Empty,
                    stateCode,
                    UsdaAms공개사업체위치정밀도Codes.주);
            }

            var cityName = PlusCodeRegex()
                .Replace(segments[index - 1], string.Empty)
                .Trim();
            if (cityName.Length == 0
                || cityName.Length > 200
                || cityName.Any(char.IsDigit))
            {
                return new LocationResult(
                    string.Empty,
                    stateCode,
                    UsdaAms공개사업체위치정밀도Codes.주);
            }

            return new LocationResult(
                cityName,
                stateCode,
                UsdaAms공개사업체위치정밀도Codes.도시주);
        }

        return LocationResult.Unparsed;
    }

    private static bool TryReadState(string segment, out string stateCode)
    {
        var withoutZip = ZipRegex().Replace(segment.Trim(), string.Empty).Trim();
        if (withoutZip.Length == 2
            && StateCodes.Contains(withoutZip.ToUpperInvariant()))
        {
            stateCode = withoutZip.ToUpperInvariant();
            return true;
        }

        foreach (var state in StateNamesByLength)
        {
            if (withoutZip.EndsWith(
                    state.Key,
                    StringComparison.OrdinalIgnoreCase))
            {
                stateCode = state.Value;
                return true;
            }
        }

        stateCode = string.Empty;
        return false;
    }

    internal sealed record LocationResult(
        string CityName,
        string StateCode,
        string PrecisionCode)
    {
        public static LocationResult Unparsed { get; } = new(
            string.Empty,
            string.Empty,
            UsdaAms공개사업체위치정밀도Codes.미확인);
    }

    [GeneratedRegex(@"\s+\d{5}(?:-\d{4})?$")]
    private static partial Regex ZipRegex();

    [GeneratedRegex(
        @"^[A-Z0-9]{4,8}\+[A-Z0-9]{2,3}\s+",
        RegexOptions.IgnoreCase)]
    private static partial Regex PlusCodeRegex();

    private static readonly HashSet<string> StateCodes =
    [
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FL",
        "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME",
        "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH",
        "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI",
        "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI",
        "WY", "AS", "GU", "MP", "PR", "VI"
    ];

    private static readonly IReadOnlyDictionary<string, string> StateNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Alabama"] = "AL",
            ["Alaska"] = "AK",
            ["Arizona"] = "AZ",
            ["Arkansas"] = "AR",
            ["California"] = "CA",
            ["Colorado"] = "CO",
            ["Connecticut"] = "CT",
            ["Delaware"] = "DE",
            ["District of Columbia"] = "DC",
            ["Florida"] = "FL",
            ["Georgia"] = "GA",
            ["Hawaii"] = "HI",
            ["Idaho"] = "ID",
            ["Illinois"] = "IL",
            ["Indiana"] = "IN",
            ["Iowa"] = "IA",
            ["Kansas"] = "KS",
            ["Kentucky"] = "KY",
            ["Louisiana"] = "LA",
            ["Maine"] = "ME",
            ["Maryland"] = "MD",
            ["Massachusetts"] = "MA",
            ["Michigan"] = "MI",
            ["Minnesota"] = "MN",
            ["Mississippi"] = "MS",
            ["Missouri"] = "MO",
            ["Montana"] = "MT",
            ["Nebraska"] = "NE",
            ["Nevada"] = "NV",
            ["New Hampshire"] = "NH",
            ["New Jersey"] = "NJ",
            ["New Mexico"] = "NM",
            ["New York"] = "NY",
            ["North Carolina"] = "NC",
            ["North Dakota"] = "ND",
            ["Ohio"] = "OH",
            ["Oklahoma"] = "OK",
            ["Oregon"] = "OR",
            ["Pennsylvania"] = "PA",
            ["Rhode Island"] = "RI",
            ["South Carolina"] = "SC",
            ["South Dakota"] = "SD",
            ["Tennessee"] = "TN",
            ["Texas"] = "TX",
            ["Utah"] = "UT",
            ["Vermont"] = "VT",
            ["Virginia"] = "VA",
            ["Washington"] = "WA",
            ["West Virginia"] = "WV",
            ["Wisconsin"] = "WI",
            ["Wyoming"] = "WY",
            ["American Samoa"] = "AS",
            ["Guam"] = "GU",
            ["Northern Mariana Islands"] = "MP",
            ["Puerto Rico"] = "PR",
            ["U.S. Virgin Islands"] = "VI",
            ["Virgin Islands"] = "VI"
        };

    private static readonly IReadOnlyList<KeyValuePair<string, string>>
        StateNamesByLength = StateNames
            .OrderByDescending(item => item.Key.Length)
            .ToArray();
}
