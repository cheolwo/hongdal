using Ssalddel.Contracts.Common.PublicData;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal sealed record Kamis국내가격Observation(
    string SurveyDate,
    string PriceTypeCode,
    string ItemName,
    string VarietyCode,
    string SearchableText,
    decimal PriceKrwPerKg);

internal static class Kamis국내가격Aggregation
{
    internal const string RetailCode = "01";
    internal const string WholesaleCode = "02";

    internal static AtDomesticFoodPriceAggregate? Aggregate(
        IReadOnlyList<Kamis국내가격Observation> observations,
        AtDomesticFoodPriceRequest request,
        string priceTypeCode,
        string priceTypeLabel)
    {
        var allowedVarieties = ResolveAllowedVarieties(request, priceTypeCode);
        var excludedTokens = request.ExcludedNameTokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Select(token => token.Trim())
            .ToArray();
        var candidates = observations
            .Where(item => string.Equals(
                item.PriceTypeCode,
                priceTypeCode,
                StringComparison.OrdinalIgnoreCase))
            .Where(item => allowedVarieties.Count == 0 || allowedVarieties.Contains(item.VarietyCode))
            .Where(item => !excludedTokens.Any(token =>
                item.SearchableText.Contains(token, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var latestDate = candidates
            .Select(item => item.SurveyDate)
            .Where(date => !string.IsNullOrWhiteSpace(date))
            .OrderByDescending(date => date, StringComparer.Ordinal)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(latestDate))
        {
            return null;
        }

        var latestPrices = candidates
            .Where(item => string.Equals(item.SurveyDate, latestDate, StringComparison.Ordinal))
            .Select(item => item.PriceKrwPerKg)
            .Where(price => price > 0)
            .ToArray();
        if (latestPrices.Length == 0)
        {
            return null;
        }

        return new AtDomesticFoodPriceAggregate
        {
            PriceTypeCode = priceTypeCode,
            PriceTypeLabel = priceTypeLabel,
            LatestSurveyDate = latestDate,
            AverageKrwPerKg = decimal.Round(
                latestPrices.Average(),
                0,
                MidpointRounding.AwayFromZero),
            MinimumKrwPerKg = latestPrices.Min(),
            MaximumKrwPerKg = latestPrices.Max(),
            SampleCount = latestPrices.Length
        };
    }

    private static IReadOnlySet<string> ResolveAllowedVarieties(
        AtDomesticFoodPriceRequest request,
        string priceTypeCode)
    {
        var stageCodes = priceTypeCode == WholesaleCode
            ? request.WholesaleVarietyCodes
            : request.RetailVarietyCodes;
        var source = stageCodes.Count > 0 ? stageCodes : request.VarietyCodes;
        return source
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
