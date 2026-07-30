using Ssalddel.Domain.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal static class UsdaAms공개사업체DirectoryCatalog
{
    private static readonly IReadOnlyDictionary<string, string> SlugsByType =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [UsdaAms공개사업체Directory유형Codes.Agritourism] = "agritourism",
            [UsdaAms공개사업체Directory유형Codes.Csa] = "csa",
            [UsdaAms공개사업체Directory유형Codes.FarmersMarket] =
                "farmersmarket",
            [UsdaAms공개사업체Directory유형Codes.FoodHub] = "foodhub",
            [UsdaAms공개사업체Directory유형Codes.OnFarmMarket] =
                "onfarmmarket"
        };

    public static IReadOnlyList<string> NormalizeMany(
        IReadOnlyList<string> requested)
        => (requested.Count == 0
                ? UsdaAms공개사업체Directory유형Codes.All
                : requested.Select(Normalize))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    public static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        foreach (var pair in SlugsByType)
        {
            if (string.Equals(
                    pair.Key,
                    normalized,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    pair.Value,
                    normalized,
                    StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }

        throw new ArgumentException(
            $"지원하지 않는 USDA AMS directory type입니다: {value}",
            nameof(value));
    }

    public static string GetSlug(string directoryTypeCode)
    {
        var normalized = Normalize(directoryTypeCode);
        return SlugsByType[normalized];
    }
}
