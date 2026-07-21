using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.FoodCulture;

public sealed record OfficialFoodRecipeCollectedRecord(
    string ExternalId,
    string Name,
    string OriginalName,
    string EnglishName,
    string Summary,
    string RegionName,
    string Category,
    string ServingText,
    IReadOnlyList<string> Ingredients,
    IReadOnlyList<string> Instructions,
    IReadOnlyDictionary<string, string> Nutrition,
    IReadOnlyList<string> Tags,
    string Tips,
    string OriginalUrl,
    string ImageReferenceUrl,
    string RawPayload,
    DateTime? SourceModifiedAtUtc = null,
    DateTime? ContentExpiresAtUtc = null);

public interface IOfficialFoodRecipeRemoteSource
{
    string SourceKey { get; }

    Task<IReadOnlyList<OfficialFoodRecipeCollectedRecord>> FetchAsync(
        int maxPages,
        int maxItems,
        CancellationToken cancellationToken = default);
}

public interface IOfficialFoodRecipeArchiveService
{
    Task<IReadOnlyList<OfficialFoodRecipeSourceDto>> GetSourcesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OfficialFoodRecipeDishDto>> SearchDishesAsync(
        OfficialFoodRecipeQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OfficialFoodRecipeVariantDto>> GetVariantsAsync(
        string dishKey,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodRecipeCollectionResponse> CollectAsync(
        OfficialFoodRecipeCollectionRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class OfficialFoodRecipeArchiveService : IOfficialFoodRecipeArchiveService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AgriculturalFisheriesDbContext _db;
    private readonly IReadOnlyDictionary<string, IOfficialFoodRecipeRemoteSource> _remoteSources;
    private readonly IOfficialFoodRecipeIngredientIndexService _ingredientIndexService;
    private readonly TimeProvider _timeProvider;

    public OfficialFoodRecipeArchiveService(
        AgriculturalFisheriesDbContext db,
        IEnumerable<IOfficialFoodRecipeRemoteSource> remoteSources,
        IOfficialFoodRecipeIngredientIndexService ingredientIndexService,
        TimeProvider timeProvider)
    {
        _db = db;
        _ingredientIndexService = ingredientIndexService;
        _timeProvider = timeProvider;

        var sourceList = remoteSources.ToArray();
        var duplicate = sourceList
            .GroupBy(source => source.SourceKey, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"공식 음식 레시피 수집기 키가 중복되었습니다. SourceKey={duplicate.Key}");
        }

        _remoteSources = sourceList.ToDictionary(
            source => source.SourceKey,
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<OfficialFoodRecipeSourceDto>> GetSourcesAsync(
        CancellationToken cancellationToken = default)
        => await _db.OfficialFoodRecipeSources
            .AsNoTracking()
            .OrderBy(source => source.CountryCode)
            .ThenBy(source => source.DisplayName)
            .Select(source => new OfficialFoodRecipeSourceDto(
                source.SourceKey,
                source.Provider,
                source.DisplayName,
                source.CountryCode,
                source.LanguageCode,
                source.AccessMethod,
                source.DocumentationUrl,
                source.TermsUrl,
                source.LicenseCode,
                source.TextReusePolicy,
                source.ImageReusePolicy,
                source.AttributionTemplate,
                source.UpdateCycle,
                source.AutomationState,
                source.FullTextStorageAllowed,
                source.ImageBinaryStorageAllowed,
                source.RequiresEditorialReview,
                source.RightsVerifiedAtUtc,
                source.LastCollectedAtUtc))
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<OfficialFoodRecipeDishDto>> SearchDishesAsync(
        OfficialFoodRecipeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var dishes = _db.OfficialFoodDishes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SourceKey))
        {
            var sourceKey = query.SourceKey.Trim();
            dishes = dishes.Where(dish => dish.RecipeVariants.Any(
                variant => variant.Source != null && variant.Source.SourceKey == sourceKey));
        }

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var countryCode = query.CountryCode.Trim();
            dishes = dishes.Where(dish => dish.CountryCode == countryCode);
        }

        if (!string.IsNullOrWhiteSpace(query.RegionName))
        {
            var regionName = query.RegionName.Trim();
            dishes = dishes.Where(dish => dish.RegionName.Contains(regionName));
        }

        if (!string.IsNullOrWhiteSpace(query.ReviewState))
        {
            var reviewState = query.ReviewState.Trim();
            dishes = dishes.Where(dish => dish.ReviewState == reviewState);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var searchText = query.SearchText.Trim();
            dishes = dishes.Where(dish =>
                dish.Name.Contains(searchText)
                || dish.OriginalName.Contains(searchText)
                || dish.EnglishName.Contains(searchText)
                || dish.Summary.Contains(searchText)
                || dish.Category.Contains(searchText));
        }

        var take = Math.Clamp(query.Take, 1, 100);
        return await dishes
            .OrderBy(dish => dish.CountryCode)
            .ThenBy(dish => dish.RegionName)
            .ThenBy(dish => dish.Name)
            .Take(take)
            .Select(dish => new OfficialFoodRecipeDishDto(
                dish.DishKey,
                dish.CountryCode,
                dish.RegionName,
                dish.Name,
                dish.OriginalName,
                dish.EnglishName,
                dish.Category,
                dish.Summary,
                dish.RepresentationState,
                dish.ReviewState,
                dish.RecipeVariants.Count,
                dish.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OfficialFoodRecipeVariantDto>> GetVariantsAsync(
        string dishKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dishKey);
        var nowUtc = UtcNow();
        var variants = await _db.OfficialFoodRecipeVariants
            .AsNoTracking()
            .Include(variant => variant.Source)
            .Include(variant => variant.Dish)
            .Include(variant => variant.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                    .ThenInclude(ingredient => ingredient!.Category)
            .Where(variant => variant.Dish != null && variant.Dish.DishKey == dishKey.Trim())
            .OrderByDescending(variant => variant.LastCollectedAtUtc)
            .ToArrayAsync(cancellationToken);

        return variants.Select(variant => ToDto(variant, nowUtc)).ToArray();
    }

    public async Task<OfficialFoodRecipeCollectionResponse> CollectAsync(
        OfficialFoodRecipeCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceKey);
        if (request.MaxPages is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.MaxPages),
                "수집 페이지 수는 1~100 범위여야 합니다.");
        }

        if (request.MaxItems is < 1 or > 5000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.MaxItems),
                "수집 항목 수는 1~5000 범위여야 합니다.");
        }

        var sourceKey = request.SourceKey.Trim();
        var source = await _db.OfficialFoodRecipeSources
            .SingleOrDefaultAsync(
                item => item.SourceKey == sourceKey,
                cancellationToken)
            ?? throw new KeyNotFoundException($"등록되지 않은 공식 음식 레시피 원천입니다. SourceKey={sourceKey}");

        if (!source.FullTextStorageAllowed
            || string.Equals(
                source.AutomationState,
                OfficialFoodRecipeAutomationStates.MetadataOnly,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{source.DisplayName}은(는) 권리 확인 전 메타데이터 링크만 저장하도록 차단되어 있습니다.");
        }

        if (!_remoteSources.TryGetValue(sourceKey, out var remoteSource))
        {
            throw new InvalidOperationException(
                $"{source.DisplayName} 수집기가 등록되지 않았습니다.");
        }

        var startedAtUtc = UtcNow();
        var run = new OfficialFoodRecipeCollectionRun
        {
            SourceKey = source.SourceKey,
            QuerySummary = $"MaxPages={request.MaxPages}, MaxItems={request.MaxItems}",
            SourceUrl = source.DocumentationUrl,
            StartedAtUtc = startedAtUtc
        };
        _db.OfficialFoodRecipeCollectionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var records = await remoteSource.FetchAsync(
                request.MaxPages,
                request.MaxItems,
                cancellationToken);
            run.FetchedCount = records.Count;

            foreach (var record in records
                         .Where(record => !string.IsNullOrWhiteSpace(record.ExternalId))
                         .GroupBy(record => record.ExternalId.Trim(), StringComparer.OrdinalIgnoreCase)
                         .Select(group => group.Last()))
            {
                await UpsertAsync(source, run, record, cancellationToken);
            }

            var completedAtUtc = UtcNow();
            source.LastCollectedAtUtc = completedAtUtc;
            source.UpdatedAtUtc = completedAtUtc;
            run.StatusCode = OfficialFoodRecipeCollectionStatuses.Completed;
            run.CompletedAtUtc = completedAtUtc;
            await _db.SaveChangesAsync(cancellationToken);

            return new OfficialFoodRecipeCollectionResponse(
                run.Id,
                run.SourceKey,
                run.FetchedCount,
                run.InsertedCount,
                run.UpdatedCount,
                run.ExistingCount,
                run.StartedAtUtc,
                completedAtUtc);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await MarkFailedAsync(run, "수집이 취소되었습니다.");
            throw;
        }
        catch (Exception exception)
        {
            await MarkFailedAsync(run, Truncate(exception.Message, 4000));
            throw;
        }
    }

    private async Task UpsertAsync(
        OfficialFoodRecipeSource source,
        OfficialFoodRecipeCollectionRun run,
        OfficialFoodRecipeCollectedRecord record,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(record.ExternalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(record.Name);

        var nowUtc = UtcNow();
        var dishKey = OfficialFoodRecipeKeys.CreateDishKey(source.CountryCode, record.Name);
        var dish = _db.OfficialFoodDishes.Local
                       .SingleOrDefault(item => item.DishKey == dishKey)
                   ?? await _db.OfficialFoodDishes
                       .SingleOrDefaultAsync(item => item.DishKey == dishKey, cancellationToken);
        if (dish is null)
        {
            dish = new OfficialFoodDish
            {
                DishKey = dishKey,
                CountryCode = source.CountryCode,
                RegionName = Truncate(record.RegionName, 160),
                Name = Truncate(record.Name, 300),
                OriginalName = Truncate(record.OriginalName, 300),
                EnglishName = Truncate(record.EnglishName, 300),
                Category = Truncate(record.Category, 300),
                Summary = Truncate(record.Summary, 4000),
                RepresentationState = OfficialFoodRecipeRepresentationStates.Candidate,
                ReviewState = OfficialFoodRecipeReviewStates.PendingReview,
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc
            };
            _db.OfficialFoodDishes.Add(dish);
        }
        else
        {
            dish.RegionName = PreferLonger(dish.RegionName, record.RegionName, 160);
            dish.Category = PreferLonger(dish.Category, record.Category, 300);
            dish.Summary = PreferLonger(dish.Summary, record.Summary, 4000);
            dish.OriginalName = PreferLonger(dish.OriginalName, record.OriginalName, 300);
            dish.EnglishName = PreferLonger(dish.EnglishName, record.EnglishName, 300);
            dish.UpdatedAtUtc = nowUtc;
        }

        var recordKey = OfficialFoodRecipeKeys.CreateRecordKey(source.SourceKey, record.ExternalId);
        var checksum = OfficialFoodRecipeKeys.CreateContentChecksum(record);
        var variant = _db.OfficialFoodRecipeVariants.Local
                          .SingleOrDefault(item => item.RecordKey == recordKey)
                      ?? await _db.OfficialFoodRecipeVariants
                          .SingleOrDefaultAsync(item => item.RecordKey == recordKey, cancellationToken);
        if (variant is null)
        {
            variant = new OfficialFoodRecipeVariant
            {
                Source = source,
                Dish = dish,
                FirstCollectionRun = run,
                RecordKey = recordKey,
                ExternalId = Truncate(record.ExternalId, 200),
                FirstCollectedAtUtc = nowUtc
            };
            ApplyCollectedRecord(variant, source, record, checksum, nowUtc);
            _db.OfficialFoodRecipeVariants.Add(variant);
            await _ingredientIndexService.SynchronizeVariantAsync(
                variant,
                source.LanguageCode,
                record.Ingredients,
                nowUtc,
                force: true,
                cancellationToken);
            run.InsertedCount++;
            return;
        }

        variant.Dish = dish;
        var changed = !string.Equals(
            variant.ContentChecksum,
            checksum,
            StringComparison.Ordinal);
        ApplyCollectedRecord(variant, source, record, checksum, nowUtc);
        if (changed
            || !string.Equals(
                variant.IngredientParserVersion,
                OfficialFoodRecipeIngredientParser.ParserVersion,
                StringComparison.Ordinal))
        {
            await _ingredientIndexService.SynchronizeVariantAsync(
                variant,
                source.LanguageCode,
                record.Ingredients,
                nowUtc,
                force: true,
                cancellationToken);
        }

        if (changed)
        {
            run.UpdatedCount++;
        }
        else
        {
            run.ExistingCount++;
        }
    }

    private static void ApplyCollectedRecord(
        OfficialFoodRecipeVariant variant,
        OfficialFoodRecipeSource source,
        OfficialFoodRecipeCollectedRecord record,
        string checksum,
        DateTime nowUtc)
    {
        variant.Title = Truncate(record.Name, 300);
        variant.Summary = Truncate(record.Summary, 8000);
        variant.RegionName = Truncate(record.RegionName, 160);
        variant.Category = Truncate(record.Category, 300);
        variant.ServingText = Truncate(record.ServingText, 300);
        variant.IngredientsJson = JsonSerializer.Serialize(record.Ingredients, JsonOptions);
        variant.InstructionsJson = JsonSerializer.Serialize(record.Instructions, JsonOptions);
        variant.NutritionJson = JsonSerializer.Serialize(record.Nutrition, JsonOptions);
        variant.TagsJson = JsonSerializer.Serialize(record.Tags, JsonOptions);
        variant.Tips = Truncate(record.Tips, 8000);
        variant.OriginalUrl = Truncate(record.OriginalUrl, 1000);
        variant.ImageReferenceUrl = Truncate(record.ImageReferenceUrl, 1000);
        variant.RawPayload = record.RawPayload;
        variant.ContentChecksum = checksum;
        variant.LicenseCodeAtCollection = source.LicenseCode;
        variant.TextReusePolicyAtCollection = source.TextReusePolicy;
        variant.ImageReusePolicyAtCollection = source.ImageReusePolicy;
        variant.AttributionText = Truncate(
            source.AttributionTemplate
                .Replace("{url}", record.OriginalUrl, StringComparison.Ordinal)
                .Replace("{date}", nowUtc.ToString("yyyy-MM-dd"), StringComparison.Ordinal),
            1000);
        variant.SourceModifiedAtUtc = record.SourceModifiedAtUtc;
        variant.LastCollectedAtUtc = nowUtc;
        variant.ContentExpiresAtUtc = record.ContentExpiresAtUtc;
        variant.IsRemovedAtSource = false;
        variant.UpdatedAtUtc = nowUtc;
    }

    private async Task MarkFailedAsync(
        OfficialFoodRecipeCollectionRun run,
        string errorMessage)
    {
        _db.ChangeTracker.Clear();
        run.StatusCode = OfficialFoodRecipeCollectionStatuses.Failed;
        run.CompletedAtUtc = UtcNow();
        run.ErrorMessage = errorMessage;
        _db.OfficialFoodRecipeCollectionRuns.Attach(run);
        _db.Entry(run).State = EntityState.Modified;
        await _db.SaveChangesAsync(CancellationToken.None);
    }

    private static OfficialFoodRecipeVariantDto ToDto(
        OfficialFoodRecipeVariant variant,
        DateTime nowUtc)
    {
        var source = variant.Source
            ?? throw new InvalidOperationException("레시피 원천 관계가 없습니다.");
        var dish = variant.Dish
            ?? throw new InvalidOperationException("대표 음식 관계가 없습니다.");
        var isFresh = !variant.IsRemovedAtSource
                      && (!variant.ContentExpiresAtUtc.HasValue
                          || variant.ContentExpiresAtUtc.Value > nowUtc);

        return new OfficialFoodRecipeVariantDto(
            variant.RecordKey,
            dish.DishKey,
            source.SourceKey,
            source.Provider,
            variant.ExternalId,
            variant.Title,
            variant.Summary,
            variant.RegionName,
            variant.Category,
            variant.ServingText,
            Deserialize<string[]>(variant.IngredientsJson, []),
            Deserialize<string[]>(variant.InstructionsJson, []),
            Deserialize<Dictionary<string, string>>(variant.NutritionJson, new()),
            Deserialize<string[]>(variant.TagsJson, []),
            variant.Tips,
            variant.OriginalUrl,
            string.IsNullOrWhiteSpace(variant.ImageReferenceUrl)
                ? null
                : variant.ImageReferenceUrl,
            variant.ImageReusePolicyAtCollection,
            variant.LicenseCodeAtCollection,
            variant.AttributionText,
            variant.LastCollectedAtUtc,
            variant.ContentExpiresAtUtc,
            isFresh,
            variant.RecipeIngredients
                .OrderBy(item => item.DisplayOrder)
                .Select(ToIngredientDto)
                .ToArray());
    }

    private static OfficialFoodRecipeIngredientDto ToIngredientDto(
        OfficialFoodRecipeIngredient recipeIngredient)
    {
        var ingredient = recipeIngredient.Ingredient
            ?? throw new InvalidOperationException("표준 재료 관계가 없습니다.");
        return new OfficialFoodRecipeIngredientDto(
            ingredient.IngredientKey,
            ingredient.CanonicalName,
            ingredient.CategoryCode,
            ingredient.Category?.KoreanName ?? ingredient.CategoryCode,
            recipeIngredient.GroupName,
            recipeIngredient.OriginalText,
            recipeIngredient.SourceName,
            recipeIngredient.QuantityText,
            recipeIngredient.QuantityValue,
            recipeIngredient.QuantityMaxValue,
            recipeIngredient.UnitCode,
            recipeIngredient.UnitText,
            recipeIngredient.HouseholdMeasureText,
            recipeIngredient.PreparationNote,
            recipeIngredient.DisplayOrder,
            recipeIngredient.ParserVersion,
            recipeIngredient.ParseConfidence,
            recipeIngredient.RequiresReview);
    }

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

    private static string PreferLonger(string current, string candidate, int maxLength)
        => string.IsNullOrWhiteSpace(current) || candidate.Trim().Length > current.Trim().Length
            ? Truncate(candidate, maxLength)
            : current;

    private static string Truncate(string? value, int maxLength)
    {
        var text = value?.Trim() ?? string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength];
    }
}

public static class OfficialFoodRecipeKeys
{
    public static string CreateDishKey(string countryCode, string name)
        => Hash($"{Normalize(countryCode)}|{Normalize(name)}");

    public static string CreateRecordKey(string sourceKey, string externalId)
        => Hash($"{Normalize(sourceKey)}|{Normalize(externalId)}");

    public static string CreateIngredientKey(string languageCode, string normalizedName)
        => Hash($"{Normalize(languageCode)}|{Normalize(normalizedName)}");

    public static string CreateContentChecksum(OfficialFoodRecipeCollectedRecord record)
        => Hash(JsonSerializer.Serialize(record, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

    private static string Normalize(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormKC).Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        var previousWasSpace = false;
        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSpace = false;
            }
            else if (!previousWasSpace)
            {
                builder.Append(' ');
                previousWasSpace = true;
            }
        }

        return builder.ToString().Trim();
    }

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
