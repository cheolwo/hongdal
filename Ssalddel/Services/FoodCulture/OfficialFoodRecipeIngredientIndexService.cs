using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.FoodCulture;

public interface IOfficialFoodRecipeIngredientIndexService
{
    Task<IReadOnlyList<OfficialFoodIngredientCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OfficialFoodIngredientDto>> SearchIngredientsAsync(
        OfficialFoodIngredientQuery query,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodIngredientIndexResponse> RebuildAsync(
        OfficialFoodIngredientIndexRequest request,
        CancellationToken cancellationToken = default);

    Task<int> SynchronizeVariantAsync(
        OfficialFoodRecipeVariant variant,
        string languageCode,
        IReadOnlyList<string> ingredientTexts,
        DateTime indexedAtUtc,
        bool force,
        CancellationToken cancellationToken = default);
}

internal sealed class OfficialFoodRecipeIngredientIndexService
    : IOfficialFoodRecipeIngredientIndexService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int BatchSize = 100;
    private const int RelatedRecipeLimit = 3;

    private readonly AgriculturalFisheriesDbContext _db;
    private readonly OfficialFoodRecipeIngredientParser _parser;
    private readonly IOfficialFoodIngredientPublicPriceService _priceService;
    private readonly TimeProvider _timeProvider;

    public OfficialFoodRecipeIngredientIndexService(
        AgriculturalFisheriesDbContext db,
        OfficialFoodRecipeIngredientParser parser,
        IOfficialFoodIngredientPublicPriceService priceService,
        TimeProvider timeProvider)
    {
        _db = db;
        _parser = parser;
        _priceService = priceService;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<OfficialFoodIngredientCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
        => await _db.OfficialFoodIngredientCategories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .Select(category => new OfficialFoodIngredientCategoryDto(
                category.CategoryCode,
                category.KoreanName,
                category.EnglishName,
                category.Description,
                category.SortOrder,
                category.Ingredients.Count))
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<OfficialFoodIngredientDto>> SearchIngredientsAsync(
        OfficialFoodIngredientQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var ingredients = _db.OfficialFoodIngredients
            .AsNoTracking()
            .Where(ingredient => ingredient.Category != null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.CategoryCode))
        {
            var categoryCode = query.CategoryCode.Trim();
            ingredients = ingredients.Where(ingredient => ingredient.CategoryCode == categoryCode);
        }

        if (!string.IsNullOrWhiteSpace(query.LanguageCode))
        {
            var languageCode = query.LanguageCode.Trim();
            ingredients = ingredients.Where(ingredient => ingredient.LanguageCode == languageCode);
        }

        if (!string.IsNullOrWhiteSpace(query.ClassificationState))
        {
            var classificationState = query.ClassificationState.Trim();
            ingredients = ingredients.Where(
                ingredient => ingredient.ClassificationState == classificationState);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var searchText = query.SearchText.Trim();
            var normalizedSearchText = OfficialFoodRecipeIngredientParser.NormalizeName(searchText);
            ingredients = ingredients.Where(ingredient =>
                ingredient.CanonicalName.Contains(searchText)
                || ingredient.NormalizedName.Contains(normalizedSearchText));
        }

        var take = Math.Clamp(query.Take, 1, 500);
        var rows = await ingredients
            .OrderByDescending(ingredient => ingredient.RecipeIngredients.Count)
            .ThenBy(ingredient => ingredient.CanonicalName)
            .Take(take)
            .Select(ingredient => new
            {
                ingredient.Id,
                Dto = new OfficialFoodIngredientDto(
                    ingredient.IngredientKey,
                    ingredient.LanguageCode,
                    ingredient.CanonicalName,
                    ingredient.NormalizedName,
                    ingredient.CategoryCode,
                    ingredient.Category!.KoreanName,
                    ingredient.ClassificationMethod,
                    ingredient.ClassificationConfidence,
                    ingredient.ClassificationState,
                    ingredient.RecipeIngredients
                        .Select(recipeIngredient => recipeIngredient.RecipeVariantId)
                        .Distinct()
                        .Count(),
                    ingredient.UpdatedAtUtc)
            })
            .ToArrayAsync(cancellationToken);
        var prices = await _priceService.GetLatestPricesAsync(
            rows.Select(row => row.Id).ToArray(),
            cancellationToken);
        var relatedRecipes = await GetRelatedRecipesAsync(
            rows.Select(row => row.Id).ToArray(),
            cancellationToken);
        return rows
            .Select(row => row.Dto with
            {
                PublicPrices = prices.GetValueOrDefault(row.Id, []),
                RelatedRecipes = relatedRecipes.GetValueOrDefault(row.Id, [])
            })
            .ToArray();
    }

    public async Task<OfficialFoodIngredientIndexResponse> RebuildAsync(
        OfficialFoodIngredientIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.MaxItems is < 1 or > 5000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.MaxItems),
                "재료 색인 대상 레시피 수는 1~5000 범위여야 합니다.");
        }

        var sourceKey = string.IsNullOrWhiteSpace(request.SourceKey)
            ? null
            : request.SourceKey.Trim();
        if (sourceKey is not null
            && !await _db.OfficialFoodRecipeSources
                .AsNoTracking()
                .AnyAsync(source => source.SourceKey == sourceKey, cancellationToken))
        {
            throw new KeyNotFoundException(
                $"등록되지 않은 공식 음식 레시피 원천입니다. SourceKey={sourceKey}");
        }

        var processedVariantCount = 0;
        var indexedIngredientCount = 0;
        long lastVariantId = 0;
        while (processedVariantCount < request.MaxItems)
        {
            var take = Math.Min(BatchSize, request.MaxItems - processedVariantCount);
            var variants = _db.OfficialFoodRecipeVariants
                .Include(variant => variant.Source)
                .Where(variant => variant.Id > lastVariantId);
            if (sourceKey is not null)
            {
                variants = variants.Where(variant =>
                    variant.Source != null && variant.Source.SourceKey == sourceKey);
            }

            if (!request.Force)
            {
                variants = variants.Where(variant =>
                    variant.IngredientParserVersion != OfficialFoodRecipeIngredientParser.ParserVersion);
            }

            var batch = await variants
                .OrderBy(variant => variant.Id)
                .Take(take)
                .ToArrayAsync(cancellationToken);
            if (batch.Length == 0)
            {
                break;
            }

            foreach (var variant in batch)
            {
                var ingredientTexts = Deserialize<string[]>(variant.IngredientsJson, []);
                indexedIngredientCount += await SynchronizeVariantAsync(
                    variant,
                    variant.Source?.LanguageCode ?? string.Empty,
                    ingredientTexts,
                    UtcNow(),
                    force: true,
                    cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            processedVariantCount += batch.Length;
            lastVariantId = batch[^1].Id;
            _db.ChangeTracker.Clear();
        }

        await _db.OfficialFoodIngredients
            .Where(ingredient =>
                ingredient.ClassificationState
                != OfficialFoodIngredientClassificationStates.Confirmed
                && !ingredient.RecipeIngredients.Any())
            .ExecuteDeleteAsync(cancellationToken);

        await _priceService.RebuildMappingsAsync(
            new OfficialFoodIngredientPriceIndexRequest(5000, request.Force),
            cancellationToken);

        var categoryCounts = await _db.OfficialFoodIngredients
            .AsNoTracking()
            .GroupBy(ingredient => ingredient.CategoryCode)
            .Select(group => new { CategoryCode = group.Key, Count = group.Count() })
            .ToDictionaryAsync(
                item => item.CategoryCode,
                item => item.Count,
                StringComparer.Ordinal,
                cancellationToken);
        var catalogIngredientCount = categoryCounts.Values.Sum();
        var pendingReviewIngredientCount = await _db.OfficialFoodIngredients
            .AsNoTracking()
            .CountAsync(
                ingredient => ingredient.ClassificationState
                              == OfficialFoodIngredientClassificationStates.PendingReview,
                cancellationToken);

        return new OfficialFoodIngredientIndexResponse(
            sourceKey,
            processedVariantCount,
            indexedIngredientCount,
            catalogIngredientCount,
            pendingReviewIngredientCount,
            categoryCounts,
            UtcNow());
    }

    public async Task<int> SynchronizeVariantAsync(
        OfficialFoodRecipeVariant variant,
        string languageCode,
        IReadOnlyList<string> ingredientTexts,
        DateTime indexedAtUtc,
        bool force,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(variant);
        ArgumentNullException.ThrowIfNull(ingredientTexts);
        if (!force
            && string.Equals(
                variant.IngredientParserVersion,
                OfficialFoodRecipeIngredientParser.ParserVersion,
                StringComparison.Ordinal))
        {
            return variant.IngredientCount;
        }

        var parsed = _parser.Parse(languageCode, ingredientTexts);
        var ingredientKeys = parsed
            .Select(item => OfficialFoodRecipeKeys.CreateIngredientKey(languageCode, item.NormalizedName))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var ingredientKeySet = ingredientKeys.ToHashSet(StringComparer.Ordinal);

        var catalog = _db.OfficialFoodIngredients.Local
            .Where(ingredient => ingredientKeySet.Contains(ingredient.IngredientKey))
            .ToDictionary(ingredient => ingredient.IngredientKey, StringComparer.Ordinal);
        if (ingredientKeys.Count > 0)
        {
            var persistedIngredients = await _db.OfficialFoodIngredients
                .Where(ingredient => ingredientKeys.Contains(ingredient.IngredientKey))
                .ToArrayAsync(cancellationToken);
            foreach (var ingredient in persistedIngredients)
            {
                catalog[ingredient.IngredientKey] = ingredient;
            }
        }

        OfficialFoodRecipeIngredient[] existingLinks;
        if (variant.Id == 0)
        {
            existingLinks = variant.RecipeIngredients.ToArray();
        }
        else
        {
            existingLinks = await _db.OfficialFoodRecipeIngredients
                .Where(item => item.RecipeVariantId == variant.Id)
                .ToArrayAsync(cancellationToken);
        }

        _db.OfficialFoodRecipeIngredients.RemoveRange(existingLinks);

        foreach (var item in parsed)
        {
            var ingredientKey = OfficialFoodRecipeKeys.CreateIngredientKey(
                languageCode,
                item.NormalizedName);
            if (!catalog.TryGetValue(ingredientKey, out var ingredient))
            {
                ingredient = new OfficialFoodIngredient
                {
                    IngredientKey = ingredientKey,
                    LanguageCode = Truncate(languageCode, 10),
                    CanonicalName = Truncate(item.CanonicalName, 300),
                    NormalizedName = Truncate(item.NormalizedName, 300),
                    CategoryCode = item.CategoryCode,
                    ClassificationMethod = item.ClassificationMethod,
                    ClassificationConfidence = item.ClassificationConfidence,
                    ClassificationState = item.ClassificationState,
                    CreatedAtUtc = indexedAtUtc,
                    UpdatedAtUtc = indexedAtUtc
                };
                _db.OfficialFoodIngredients.Add(ingredient);
                catalog.Add(ingredientKey, ingredient);
            }
            else if (!string.Equals(
                         ingredient.ClassificationState,
                         OfficialFoodIngredientClassificationStates.Confirmed,
                         StringComparison.Ordinal)
                     && (ingredient.ClassificationMethod != item.ClassificationMethod
                         || item.ClassificationConfidence >= ingredient.ClassificationConfidence))
            {
                ingredient.CategoryCode = item.CategoryCode;
                ingredient.ClassificationMethod = item.ClassificationMethod;
                ingredient.ClassificationConfidence = item.ClassificationConfidence;
                ingredient.ClassificationState = item.ClassificationState;
                ingredient.UpdatedAtUtc = indexedAtUtc;
            }

            _db.OfficialFoodRecipeIngredients.Add(new OfficialFoodRecipeIngredient
            {
                RecipeVariant = variant,
                Ingredient = ingredient,
                GroupName = Truncate(item.GroupName, 160),
                OriginalText = Truncate(item.OriginalText, 1000),
                SourceName = Truncate(item.SourceName, 300),
                QuantityText = Truncate(item.QuantityText, 100),
                QuantityValue = item.QuantityValue,
                QuantityMaxValue = item.QuantityMaxValue,
                UnitCode = Truncate(item.UnitCode, 40),
                UnitText = Truncate(item.UnitText, 80),
                HouseholdMeasureText = Truncate(item.HouseholdMeasureText, 300),
                PreparationNote = Truncate(item.PreparationNote, 300),
                DisplayOrder = item.DisplayOrder,
                ParserVersion = OfficialFoodRecipeIngredientParser.ParserVersion,
                ParseConfidence = item.ParseConfidence,
                RequiresReview = item.RequiresReview,
                CreatedAtUtc = indexedAtUtc,
                UpdatedAtUtc = indexedAtUtc
            });
        }

        variant.IngredientParserVersion = OfficialFoodRecipeIngredientParser.ParserVersion;
        variant.IngredientCount = parsed.Count;
        variant.IngredientsIndexedAtUtc = indexedAtUtc;
        return parsed.Count;
    }

    private async Task<IReadOnlyDictionary<long, IReadOnlyList<OfficialFoodIngredientRelatedRecipeDto>>>
        GetRelatedRecipesAsync(
            IReadOnlyCollection<long> ingredientIds,
            CancellationToken cancellationToken)
    {
        var ids = ingredientIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<long, IReadOnlyList<OfficialFoodIngredientRelatedRecipeDto>>();
        }

        var candidates = await _db.OfficialFoodRecipeIngredients
            .AsNoTracking()
            .Where(item => Enumerable.Contains(ids, item.IngredientId)
                           && item.RecipeVariant != null
                           && !item.RecipeVariant.IsRemovedAtSource
                           && item.RecipeVariant.Dish != null
                           && item.RecipeVariant.Source != null
                           && item.RecipeVariant.Dish.ReviewState
                           != OfficialFoodRecipeReviewStates.Excluded
                           && item.RecipeVariant.Dish.RepresentationState
                           != OfficialFoodRecipeRepresentationStates.Excluded)
            .Select(item => new RelatedRecipeCandidate(
                item.Id,
                item.IngredientId,
                item.RecipeVariantId,
                item.RecipeVariant!.DishId,
                item.RecipeVariant.Dish!.DishKey,
                item.RecipeVariant.RecordKey,
                item.RecipeVariant.Source!.SourceKey,
                item.RecipeVariant.Source.Provider,
                item.RecipeVariant.Dish.CountryCode,
                item.RecipeVariant.Dish.Name,
                item.RecipeVariant.Title,
                item.RecipeVariant.RegionName,
                item.RecipeVariant.Dish.RegionName,
                item.RecipeVariant.Category,
                item.RecipeVariant.Dish.Category,
                item.GroupName,
                item.SourceName,
                item.QuantityText,
                item.UnitText,
                item.PreparationNote,
                item.DisplayOrder,
                item.RecipeVariant.OriginalUrl,
                item.RecipeVariant.LastCollectedAtUtc,
                item.RecipeVariant.ContentExpiresAtUtc,
                item.RecipeVariant.Dish.RepresentationState,
                item.RecipeVariant.Dish.ReviewState))
            .ToArrayAsync(cancellationToken);

        var nowUtc = UtcNow();
        var candidatesByIngredient = candidates.ToLookup(candidate => candidate.IngredientId);
        return ids.ToDictionary(
            ingredientId => ingredientId,
            ingredientId => SelectRelatedRecipes(
                candidatesByIngredient[ingredientId],
                nowUtc));
    }

    private static IReadOnlyList<OfficialFoodIngredientRelatedRecipeDto> SelectRelatedRecipes(
        IEnumerable<RelatedRecipeCandidate> candidates,
        DateTime nowUtc)
    {
        var ordered = candidates
            .GroupBy(candidate => candidate.RecipeVariantId)
            .Select(group => group
                .OrderBy(candidate => candidate.DisplayOrder)
                .ThenBy(candidate => candidate.RelationId)
                .First())
            .OrderBy(candidate => candidate.ReviewState == OfficialFoodRecipeReviewStates.Approved ? 0 : 1)
            .ThenBy(candidate => candidate.RepresentationState
                                 == OfficialFoodRecipeRepresentationStates.Representative
                ? 0
                : 1)
            .ThenBy(candidate => IsFresh(candidate, nowUtc) ? 0 : 1)
            .ThenByDescending(candidate => candidate.LastCollectedAtUtc)
            .ThenBy(candidate => candidate.DishKey, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.RecipeRecordKey, StringComparer.Ordinal)
            .ToArray();

        var selected = new List<RelatedRecipeCandidate>(RelatedRecipeLimit);
        var selectedVariantIds = new HashSet<long>();
        var selectedDishIds = new HashSet<long>();
        foreach (var candidate in ordered)
        {
            if (selected.Count == RelatedRecipeLimit)
            {
                break;
            }

            if (selectedDishIds.Add(candidate.DishId))
            {
                selected.Add(candidate);
                selectedVariantIds.Add(candidate.RecipeVariantId);
            }
        }

        if (selected.Count < RelatedRecipeLimit)
        {
            foreach (var candidate in ordered)
            {
                if (selected.Count == RelatedRecipeLimit)
                {
                    break;
                }

                if (selectedVariantIds.Add(candidate.RecipeVariantId))
                {
                    selected.Add(candidate);
                }
            }
        }

        return selected.Select(candidate => new OfficialFoodIngredientRelatedRecipeDto(
                candidate.DishKey,
                candidate.RecipeRecordKey,
                candidate.SourceKey,
                candidate.Provider,
                candidate.CountryCode,
                string.IsNullOrWhiteSpace(candidate.DishName)
                    ? candidate.RecipeTitle
                    : candidate.DishName,
                candidate.RecipeTitle,
                string.IsNullOrWhiteSpace(candidate.RecipeRegionName)
                    ? candidate.DishRegionName
                    : candidate.RecipeRegionName,
                string.IsNullOrWhiteSpace(candidate.RecipeCategory)
                    ? candidate.DishCategory
                    : candidate.RecipeCategory,
                candidate.IngredientGroupName,
                candidate.IngredientSourceName,
                candidate.QuantityText,
                candidate.UnitText,
                candidate.PreparationNote,
                candidate.OriginalUrl,
                candidate.LastCollectedAtUtc,
                IsFresh(candidate, nowUtc)))
            .ToArray();
    }

    private static bool IsFresh(RelatedRecipeCandidate candidate, DateTime nowUtc)
        => !candidate.ContentExpiresAtUtc.HasValue
           || candidate.ContentExpiresAtUtc.Value > nowUtc;

    private static T Deserialize<T>(string json, T fallback)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    private static string Truncate(string? value, int maxLength)
    {
        var text = value?.Trim() ?? string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private sealed record RelatedRecipeCandidate(
        long RelationId,
        long IngredientId,
        long RecipeVariantId,
        long DishId,
        string DishKey,
        string RecipeRecordKey,
        string SourceKey,
        string Provider,
        string CountryCode,
        string DishName,
        string RecipeTitle,
        string RecipeRegionName,
        string DishRegionName,
        string RecipeCategory,
        string DishCategory,
        string IngredientGroupName,
        string IngredientSourceName,
        string QuantityText,
        string UnitText,
        string PreparationNote,
        int DisplayOrder,
        string OriginalUrl,
        DateTime LastCollectedAtUtc,
        DateTime? ContentExpiresAtUtc,
        string RepresentationState,
        string ReviewState);
}
