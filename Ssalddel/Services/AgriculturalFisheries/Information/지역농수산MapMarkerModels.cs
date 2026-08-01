using Ssalddel.Contracts.Common.PublicData;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal sealed record 지역농수산MapNormalizedQuery(
    string CountryCode,
    IReadOnlyList<string> RelationTypeCodes,
    string? ProductName,
    DateOnly? FromDate,
    DateOnly? ToDate,
    int MaxItems);

internal sealed record 지역농수산MapSourceAggregate(
    string CodeScheme,
    string RelationTypeCode,
    string DataSourceKey,
    string ExternalCode,
    string ExternalName,
    int ObservationCount,
    DateOnly EarliestObservedDate,
    DateOnly LatestObservedDate);

internal sealed record 지역농수산MapProjectionResult(
    IReadOnlyList<RegionalAgriculturalMapMarkerDto> Markers,
    int UnresolvedObservationCount,
    int MissingAnchorRegionCount)
{
    public static 지역농수산MapProjectionResult Empty { get; } = new([], 0, 0);
}

internal static class 지역농수산MapMarkerQueryNormalizer
{
    private const int MaximumMarkerCount = 500;
    private const int MaximumProductNameLength = 200;

    public static 지역농수산MapNormalizedQuery Normalize(
        RegionalAgriculturalMapMarkerQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var countryCode = query.CountryCode.Trim().ToUpperInvariant();
        if (!RegionalAgriculturalMapCountryCodes.All.Contains(
                countryCode,
                StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"CountryCode는 {string.Join(", ", RegionalAgriculturalMapCountryCodes.All)} 중 하나여야 합니다.",
                nameof(query));
        }

        var supportedRelations = GetSupportedRelations(countryCode);
        IReadOnlyList<string> relationTypes;
        if (string.IsNullOrWhiteSpace(query.RelationTypeCode))
        {
            relationTypes = supportedRelations;
        }
        else
        {
            var relation = supportedRelations.FirstOrDefault(item => item.Equals(
                query.RelationTypeCode.Trim(),
                StringComparison.OrdinalIgnoreCase));
            relationTypes = relation is null
                ? throw new ArgumentException(
                    $"{countryCode}에서 지원하지 않는 RelationTypeCode입니다: {query.RelationTypeCode}",
                    nameof(query))
                : [relation];
        }

        if (query.FromDate.HasValue
            && query.ToDate.HasValue
            && query.FromDate.Value > query.ToDate.Value)
        {
            throw new ArgumentException("FromDate는 ToDate보다 늦을 수 없습니다.", nameof(query));
        }

        if (query.MaxItems is < 1 or > MaximumMarkerCount)
        {
            throw new ArgumentException(
                $"MaxItems는 1에서 {MaximumMarkerCount} 사이여야 합니다.",
                nameof(query));
        }

        var productName = string.IsNullOrWhiteSpace(query.ProductName)
            ? null
            : query.ProductName.Trim();
        if (productName?.Length > MaximumProductNameLength)
        {
            throw new ArgumentException(
                $"ProductName은 {MaximumProductNameLength}자를 초과할 수 없습니다.",
                nameof(query));
        }

        return new 지역농수산MapNormalizedQuery(
            countryCode,
            relationTypes,
            productName,
            query.FromDate,
            query.ToDate,
            query.MaxItems);
    }

    private static IReadOnlyList<string> GetSupportedRelations(string countryCode)
        => countryCode == RegionalAgriculturalMapCountryCodes.Korea
            ? [RegionalAgriculturalMapRelationTypeCodes.ConfirmedOrigin]
            :
            [
                RegionalAgriculturalMapRelationTypeCodes.MarketObservation,
                RegionalAgriculturalMapRelationTypeCodes.ShippingPointOrPortOfEntry
            ];
}

internal static class 지역농수산MapCodeNormalizer
{
    public static string Normalize(string? value)
        => string.Join(
            " ",
            (value ?? string.Empty)
                .Trim()
                .ToUpperInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
