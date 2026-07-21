using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.FoodCulture;

public interface IOfficialFoodIngredientPublicPriceService
{
    Task<OfficialFoodIngredientPriceIndexResponse> RebuildMappingsAsync(
        OfficialFoodIngredientPriceIndexRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<long, IReadOnlyList<OfficialFoodIngredientPublicPriceDto>>>
        GetLatestPricesAsync(
            IReadOnlyCollection<long> ingredientIds,
            CancellationToken cancellationToken = default);
}

public sealed class OfficialFoodIngredientPublicPriceService(
    AgriculturalFisheriesDbContext db,
    IOfficialFoodIngredientPriceMatchCatalog matchCatalog,
    TimeProvider timeProvider)
    : IOfficialFoodIngredientPublicPriceService
{
    private const int BatchSize = 200;

    public async Task<OfficialFoodIngredientPriceIndexResponse> RebuildMappingsAsync(
        OfficialFoodIngredientPriceIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.MaxItems is < 1 or > 5000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.MaxItems),
                "공공가격 매핑 대상 재료 수는 1~5000 범위여야 합니다.");
        }

        var processedCount = 0;
        long lastIngredientId = 0;
        while (processedCount < request.MaxItems)
        {
            var take = Math.Min(BatchSize, request.MaxItems - processedCount);
            var ingredients = await db.OfficialFoodIngredients
                .Include(ingredient => ingredient.PublicPriceMappings)
                .Where(ingredient => ingredient.Id > lastIngredientId)
                .OrderBy(ingredient => ingredient.Id)
                .Take(take)
                .ToArrayAsync(cancellationToken);
            if (ingredients.Length == 0)
            {
                break;
            }

            var nowUtc = UtcNow();
            foreach (var ingredient in ingredients)
            {
                SynchronizeMappings(ingredient, request.Force, nowUtc);
            }

            await db.SaveChangesAsync(cancellationToken);
            processedCount += ingredients.Length;
            lastIngredientId = ingredients[^1].Id;
            db.ChangeTracker.Clear();
        }

        var activeMappings = db.OfficialFoodIngredientPriceMappings
            .AsNoTracking()
            .Where(mapping => mapping.IsActive);
        var mappingCount = await activeMappings.CountAsync(cancellationToken);
        var mappedIngredientCount = await activeMappings
            .Select(mapping => mapping.IngredientId)
            .Distinct()
            .CountAsync(cancellationToken);
        var koreanMappingCount = await activeMappings
            .CountAsync(mapping => mapping.CountryCode == "KR", cancellationToken);
        var unitedStatesMappingCount = await activeMappings
            .CountAsync(mapping => mapping.CountryCode == "US", cancellationToken);
        var ingredientCount = await db.OfficialFoodIngredients.CountAsync(cancellationToken);
        var mappedIngredientIds = await activeMappings
            .Select(mapping => mapping.IngredientId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
        var prices = await GetLatestPricesAsync(mappedIngredientIds, cancellationToken);
        var pricedIngredientCount = prices.Count(item => item.Value.Count > 0);
        var allPrices = prices.Values.SelectMany(item => item).ToArray();

        return new OfficialFoodIngredientPriceIndexResponse(
            processedCount,
            mappedIngredientCount,
            mappingCount,
            koreanMappingCount,
            unitedStatesMappingCount,
            Math.Max(0, ingredientCount - mappedIngredientCount),
            pricedIngredientCount,
            allPrices.Count(price => price.CountryCode == "KR"),
            allPrices.Count(price => price.CountryCode == "US"),
            UtcNow());
    }

    public async Task<IReadOnlyDictionary<long, IReadOnlyList<OfficialFoodIngredientPublicPriceDto>>>
        GetLatestPricesAsync(
            IReadOnlyCollection<long> ingredientIds,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ingredientIds);
        var ids = ingredientIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, IReadOnlyList<OfficialFoodIngredientPublicPriceDto>>();
        }

        var mappings = await db.OfficialFoodIngredientPriceMappings
            .AsNoTracking()
            .Where(mapping => mapping.IsActive && ids.Contains(mapping.IngredientId))
            .ToArrayAsync(cancellationToken);
        if (mappings.Length == 0)
        {
            return ids.ToDictionary(
                id => id,
                _ => (IReadOnlyList<OfficialFoodIngredientPublicPriceDto>)[]);
        }

        var result = ids.ToDictionary(
            id => id,
            _ => new List<OfficialFoodIngredientPublicPriceDto>());
        await AddKamisPricesAsync(mappings, result, cancellationToken);
        await AddUsdaPricesAsync(mappings, result, cancellationToken);

        return result.ToDictionary(
            item => item.Key,
            item => (IReadOnlyList<OfficialFoodIngredientPublicPriceDto>)item.Value
                .OrderBy(price => price.CountryCode, StringComparer.Ordinal)
                .ThenBy(price => MarketStageSortOrder(price.MarketStageCode))
                .ThenByDescending(price => price.ReferenceDate)
                .ToArray());
    }

    private void SynchronizeMappings(
        OfficialFoodIngredient ingredient,
        bool force,
        DateTime nowUtc)
    {
        var desired = matchCatalog.Match(ingredient)
            .ToDictionary(
                match => MappingKey(match.CountryCode, match.SourceKey),
                StringComparer.Ordinal);
        var existing = ingredient.PublicPriceMappings
            .ToDictionary(
                mapping => MappingKey(mapping.CountryCode, mapping.SourceKey),
                StringComparer.Ordinal);

        foreach (var item in desired)
        {
            var isNew = false;
            if (!existing.TryGetValue(item.Key, out var mapping))
            {
                mapping = new OfficialFoodIngredientPriceMapping
                {
                    Ingredient = ingredient,
                    CountryCode = item.Value.CountryCode,
                    SourceKey = item.Value.SourceKey,
                    MappingState = OfficialFoodIngredientPriceMappingStates.AutoMatched,
                    CreatedAtUtc = nowUtc
                };
                ingredient.PublicPriceMappings.Add(mapping);
                db.OfficialFoodIngredientPriceMappings.Add(mapping);
                isNew = true;
            }
            else if (string.Equals(
                         mapping.MappingState,
                         OfficialFoodIngredientPriceMappingStates.Confirmed,
                         StringComparison.Ordinal))
            {
                continue;
            }

            if (isNew || force || HasMaterialChanges(mapping, item.Value))
            {
                Apply(mapping, item.Value, nowUtc);
            }
        }

        foreach (var mapping in existing.Values.Where(mapping =>
                     mapping.IsActive
                     && !desired.ContainsKey(MappingKey(mapping.CountryCode, mapping.SourceKey))
                     && !string.Equals(
                         mapping.MappingState,
                         OfficialFoodIngredientPriceMappingStates.Confirmed,
                         StringComparison.Ordinal)))
        {
            if (force || mapping.MappingState == OfficialFoodIngredientPriceMappingStates.AutoMatched)
            {
                mapping.IsActive = false;
                mapping.UpdatedAtUtc = nowUtc;
            }
        }
    }

    private async Task AddKamisPricesAsync(
        IReadOnlyCollection<OfficialFoodIngredientPriceMapping> mappings,
        IDictionary<long, List<OfficialFoodIngredientPublicPriceDto>> result,
        CancellationToken cancellationToken)
    {
        var sourceMappings = mappings
            .Where(mapping => mapping.SourceKey == OfficialFoodIngredientPublicPriceSourceKeys.Kamis)
            .ToArray();
        var itemCodes = sourceMappings
            .Select(mapping => mapping.ExternalItemCode)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (itemCodes.Count == 0)
        {
            return;
        }

        var baseQuery = db.KamisPriceObservations
            .AsNoTracking()
            .Where(observation =>
                itemCodes.Contains(observation.ItemCode)
                && !observation.IsPriceMissing
                && observation.PriceKrw.HasValue);
        var latestDailyDate = await baseQuery
            .Where(observation => observation.FrequencyCode == "Daily")
            .MaxAsync(observation => (DateOnly?)observation.SurveyDate, cancellationToken);
        var latestMonthlyDate = await baseQuery
            .Where(observation => observation.FrequencyCode == "Monthly")
            .MaxAsync(observation => (DateOnly?)observation.SurveyDate, cancellationToken);
        var dailyCutoff = latestDailyDate?.AddDays(-31);
        var monthlyCutoff = latestMonthlyDate?.AddMonths(-13);

        var observations = await baseQuery
            .Where(observation =>
                (dailyCutoff.HasValue
                 && observation.FrequencyCode == "Daily"
                 && observation.SurveyDate >= dailyCutoff.Value)
                || (monthlyCutoff.HasValue
                    && observation.FrequencyCode == "Monthly"
                    && observation.SurveyDate >= monthlyCutoff.Value))
            .ToArrayAsync(cancellationToken);

        foreach (var mapping in sourceMappings)
        {
            var candidates = observations
                .Where(observation => observation.ItemCode == mapping.ExternalItemCode)
                .Where(observation => string.IsNullOrWhiteSpace(mapping.ExternalCategoryCode)
                                      || observation.CategoryCode == mapping.ExternalCategoryCode)
                .Where(observation => MatchesCodeFilter(
                    mapping.ExternalVariantCode,
                    observation.KindCode))
                .Where(observation => string.IsNullOrWhiteSpace(mapping.ExternalVariantName)
                                      || string.Equals(
                                          Normalize(observation.KindName),
                                          Normalize(mapping.ExternalVariantName),
                                          StringComparison.Ordinal))
                .ToArray();

            foreach (var productClassCode in new[] { "01", "02" })
            {
                var classCandidates = candidates
                    .Where(observation => observation.ProductClassCode == productClassCode)
                    .ToArray();
                var frequency = classCandidates.Any(observation => observation.FrequencyCode == "Daily")
                    ? "Daily"
                    : classCandidates.Any(observation => observation.FrequencyCode == "Monthly")
                        ? "Monthly"
                        : null;
                if (frequency is null)
                {
                    continue;
                }

                var frequencyCandidates = classCandidates
                    .Where(observation => observation.FrequencyCode == frequency)
                    .ToArray();
                var latestDate = frequencyCandidates.Max(observation => observation.SurveyDate);
                var samples = frequencyCandidates
                    .Where(observation => observation.SurveyDate == latestDate)
                    .Where(observation => observation.PriceKrw is > 0)
                    .ToArray();
                if (samples.Length == 0)
                {
                    continue;
                }

                var prices = samples.Select(observation => observation.PriceKrw!.Value).ToArray();
                var first = samples[0];
                result[mapping.IngredientId].Add(new OfficialFoodIngredientPublicPriceDto(
                    "KR",
                    "대한민국",
                    mapping.SourceKey,
                    "한국농수산식품유통공사 KAMIS",
                    productClassCode == "01"
                        ? OfficialFoodIngredientPriceMarketStages.Retail
                        : OfficialFoodIngredientPriceMarketStages.Wholesale,
                    productClassCode == "01" ? "전국 소매 조사가격" : "전국 도매 조사가격",
                    string.IsNullOrWhiteSpace(mapping.ExternalItemName)
                        ? first.ItemName
                        : mapping.ExternalItemName,
                    BuildKamisVarietyLabel(samples),
                    decimal.Round(prices.Average(), 0, MidpointRounding.AwayFromZero),
                    prices.Min(),
                    prices.Max(),
                    "KRW",
                    first.Unit,
                    latestDate,
                    latestDate.ToString("yyyy-MM-dd"),
                    "전국",
                    frequency,
                    samples.Length,
                    mapping.MatchQualityCode,
                    mapping.MappingNote,
                    first.SourceUrl,
                    samples.Max(observation => observation.LastSeenAtUtc)));
            }
        }
    }

    private async Task AddUsdaPricesAsync(
        IReadOnlyCollection<OfficialFoodIngredientPriceMapping> mappings,
        IDictionary<long, List<OfficialFoodIngredientPublicPriceDto>> result,
        CancellationToken cancellationToken)
    {
        var sourceMappings = mappings
            .Where(mapping => mapping.SourceKey == OfficialFoodIngredientPublicPriceSourceKeys.UsdaNass)
            .ToArray();
        var commodities = sourceMappings
            .Select(mapping => mapping.ExternalItemCode)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (commodities.Count == 0)
        {
            return;
        }

        var baseQuery = db.PriceObservations
            .AsNoTracking()
            .Where(observation =>
                commodities.Contains(observation.CommodityDesc)
                && observation.NumericValue.HasValue
                && !observation.IsSuppressed
                && observation.SourceDesc == "SURVEY"
                && observation.StatisticCategoryDesc == "PRICE RECEIVED"
                && observation.AggregationLevelDesc == "NATIONAL");
        var latestYear = await baseQuery
            .MaxAsync(observation => (int?)observation.Year, cancellationToken);
        if (!latestYear.HasValue)
        {
            return;
        }

        var observations = await baseQuery
            .Where(observation => observation.Year >= latestYear.Value - 1)
            .ToArrayAsync(cancellationToken);

        foreach (var mapping in sourceMappings)
        {
            var candidates = observations
                .Where(observation => observation.CommodityDesc == mapping.ExternalItemCode)
                .Where(observation => string.IsNullOrWhiteSpace(mapping.ExternalCategoryCode)
                                      || observation.SectorDesc == mapping.ExternalCategoryCode)
                .Where(IsConsolidatedUsdaSeries)
                .ToArray();
            if (candidates.Length == 0)
            {
                continue;
            }

            foreach (var unitGroup in candidates
                         .GroupBy(observation => observation.UnitDesc, StringComparer.Ordinal)
                         .OrderBy(group => group.Key, StringComparer.Ordinal)
                         .Take(3))
            {
                var observation = unitGroup
                    .OrderByDescending(item => ToReferenceDate(item))
                    .ThenByDescending(item => item.SourceLoadTimeUtc)
                    .First();
                var referenceDate = ToReferenceDate(observation);
                var value = observation.NumericValue!.Value;
                result[mapping.IngredientId].Add(new OfficialFoodIngredientPublicPriceDto(
                    "US",
                    "미국",
                    mapping.SourceKey,
                    "USDA National Agricultural Statistics Service",
                    OfficialFoodIngredientPriceMarketStages.ProducerReceived,
                    "전국 생산자 수취가격",
                    observation.CommodityDesc,
                    BuildUsdaClassLabel(observation),
                    value,
                    value,
                    value,
                    "USD",
                    observation.UnitDesc,
                    referenceDate,
                    string.Join(' ', observation.Year, observation.ReferencePeriodDesc).Trim(),
                    string.IsNullOrWhiteSpace(observation.CountryName)
                        ? "United States"
                        : observation.CountryName,
                    observation.FrequencyDesc,
                    1,
                    mapping.MatchQualityCode,
                    mapping.MappingNote,
                    observation.SourceUrl,
                    observation.LastSeenAtUtc));
            }
        }
    }

    private static bool IsConsolidatedUsdaSeries(UsdaNassPriceObservation observation)
        => IsAllQualifier(observation.ClassDesc)
           && IsAllQualifier(observation.UtilPracticeDesc)
           && IsAllQualifier(observation.ProductionPracticeDesc)
           && (string.IsNullOrWhiteSpace(observation.DomainDesc)
               || observation.DomainDesc == "TOTAL");

    private static bool IsAllQualifier(string value)
        => string.IsNullOrWhiteSpace(value)
           || value.StartsWith("ALL ", StringComparison.Ordinal)
           || value.Equals("ALL", StringComparison.Ordinal);

    private static DateOnly ToReferenceDate(UsdaNassPriceObservation observation)
    {
        var year = Math.Clamp(observation.Year, 1900, 2100);
        var month = int.TryParse(observation.EndCode, out var parsedMonth)
                    && parsedMonth is >= 1 and <= 12
            ? parsedMonth
            : 12;
        return new DateOnly(year, month, DateTime.DaysInMonth(year, month));
    }

    private static string BuildKamisVarietyLabel(
        IReadOnlyCollection<KamisPriceObservation> observations)
    {
        var names = observations
            .Select(observation => observation.KindName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        if (names.Length == 0)
        {
            return string.Empty;
        }

        var label = string.Join(", ", names.Take(3));
        return names.Length > 3 ? $"{label} 외 {names.Length - 3}종" : label;
    }

    private static string BuildUsdaClassLabel(UsdaNassPriceObservation observation)
    {
        var labels = new[]
            {
                observation.ClassDesc,
                observation.UtilPracticeDesc,
                observation.ProductionPracticeDesc
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return string.Join(" · ", labels);
    }

    private static void Apply(
        OfficialFoodIngredientPriceMapping mapping,
        OfficialFoodIngredientPriceMatch match,
        DateTime nowUtc)
    {
        mapping.CountryCode = match.CountryCode;
        mapping.SourceKey = match.SourceKey;
        mapping.ExternalCategoryCode = match.ExternalCategoryCode;
        mapping.ExternalItemCode = match.ExternalItemCode;
        mapping.ExternalItemName = match.ExternalItemName;
        mapping.ExternalVariantCode = match.ExternalVariantCode;
        mapping.ExternalVariantName = match.ExternalVariantName;
        mapping.MatchMethod = match.MatchMethod;
        mapping.MatchQualityCode = match.MatchQualityCode;
        mapping.MatchConfidence = match.MatchConfidence;
        mapping.MappingState = OfficialFoodIngredientPriceMappingStates.AutoMatched;
        mapping.MappingNote = match.MappingNote;
        mapping.SourceUrl = match.SourceUrl;
        mapping.IsActive = true;
        mapping.UpdatedAtUtc = nowUtc;
    }

    private static bool HasMaterialChanges(
        OfficialFoodIngredientPriceMapping mapping,
        OfficialFoodIngredientPriceMatch match)
        => mapping.CountryCode != match.CountryCode
           || mapping.SourceKey != match.SourceKey
           || mapping.ExternalCategoryCode != match.ExternalCategoryCode
           || mapping.ExternalItemCode != match.ExternalItemCode
           || mapping.ExternalItemName != match.ExternalItemName
           || mapping.ExternalVariantCode != match.ExternalVariantCode
           || mapping.ExternalVariantName != match.ExternalVariantName
           || mapping.MatchMethod != match.MatchMethod
           || mapping.MatchQualityCode != match.MatchQualityCode
           || mapping.MatchConfidence != match.MatchConfidence
           || mapping.MappingState != OfficialFoodIngredientPriceMappingStates.AutoMatched
           || mapping.MappingNote != match.MappingNote
           || mapping.SourceUrl != match.SourceUrl
           || !mapping.IsActive;

    private static string MappingKey(string countryCode, string sourceKey)
        => string.Join('|', countryCode, sourceKey);

    private static string Normalize(string value)
        => OfficialFoodRecipeIngredientParser.NormalizeName(value);

    private static bool MatchesCodeFilter(string codeFilter, string code)
        => string.IsNullOrWhiteSpace(codeFilter)
           || codeFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Contains(code, StringComparer.Ordinal);

    private static int MarketStageSortOrder(string marketStageCode)
        => marketStageCode switch
        {
            OfficialFoodIngredientPriceMarketStages.Retail => 10,
            OfficialFoodIngredientPriceMarketStages.Wholesale => 20,
            OfficialFoodIngredientPriceMarketStages.ProducerReceived => 30,
            _ => 999
        };

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
