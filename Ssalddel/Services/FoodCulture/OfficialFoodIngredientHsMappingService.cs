using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Domain.HsCodes;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Data;

namespace Ssalddel.Services.FoodCulture;

public interface IOfficialFoodIngredientHsMappingService
{
    Task<OfficialFoodIngredientHsMappingResponse> GetOrCreateAsync(
        OfficialFoodIngredientHsQuery query,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodIngredientHsIndexResponse> RebuildAsync(
        OfficialFoodIngredientHsIndexRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class OfficialFoodIngredientHsMappingService(
    AgriculturalFisheriesDbContext archiveDb,
    SsalddelContext customsDb,
    TimeProvider timeProvider) : IOfficialFoodIngredientHsMappingService
{
    private const int CandidateLimitPerCatalog = 6;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlyList<string> Notices =
    [
        "표시된 코드는 신고용 확정값이 아니라 공식 HS 카탈로그에서 찾은 검토 후보입니다.",
        "같은 재료도 품종·가공상태·성분함량·포장·용도에 따라 다른 코드가 적용될 수 있습니다.",
        "HS 6자리 이후 세번은 국가별로 다르므로 한국 수출 HSK와 미국 수입 HTS를 각각 확인해야 합니다.",
        "실제 신고 전에는 최신 관세율표, 품목분류 결정사례와 관세사 또는 관세당국의 사전심사를 확인하세요."
    ];

    public async Task<OfficialFoodIngredientHsMappingResponse> GetOrCreateAsync(
        OfficialFoodIngredientHsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var ingredient = await FindIngredientAsync(
            query.IngredientKey,
            query.IngredientName,
            cancellationToken)
            ?? throw new KeyNotFoundException("전산화된 공식 음식 재료를 찾지 못했습니다.");
        var countryCode = NormalizeCountryCode(query.CountryCode);
        var hasMappings = await archiveDb.OfficialFoodIngredientHsMappings
            .AsNoTracking()
            .AnyAsync(mapping =>
                    mapping.IngredientId == ingredient.Id
                    && mapping.IsActive
                    && (countryCode == null || mapping.CountryCode == countryCode),
                cancellationToken);
        if (query.Refresh || !hasMappings)
        {
            await RefreshAsync(
                [ingredient],
                countryCode is null ? null : [countryCode],
                cancellationToken);
        }

        var candidates = await archiveDb.OfficialFoodIngredientHsMappings
            .AsNoTracking()
            .Where(mapping =>
                mapping.IngredientId == ingredient.Id
                && mapping.IsActive
                && mapping.MappingState != OfficialFoodIngredientHsMappingStates.Rejected
                && (countryCode == null || mapping.CountryCode == countryCode))
            .OrderBy(mapping => mapping.CountryCode)
            .ThenByDescending(mapping => mapping.MatchConfidence)
            .ThenByDescending(mapping => mapping.HsCodeLevel)
            .ThenBy(mapping => mapping.NormalizedHsCode)
            .ToArrayAsync(cancellationToken);
        var hasActiveCatalog = await HasActiveCatalogAsync(countryCode, cancellationToken);
        return new OfficialFoodIngredientHsMappingResponse(
            ingredient.IngredientKey,
            ingredient.CanonicalName,
            countryCode,
            hasActiveCatalog,
            UtcNow(),
            candidates.Select(Map).ToArray(),
            Notices);
    }

    public async Task<OfficialFoodIngredientHsIndexResponse> RebuildAsync(
        OfficialFoodIngredientHsIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var maxItems = Math.Clamp(request.MaxItems, 1, 5000);
        var countryCodes = NormalizeCountryCodes(request.CountryCodes);
        var ingredientsQuery = archiveDb.OfficialFoodIngredients
            .AsNoTracking()
            .OrderBy(ingredient => ingredient.Id)
            .AsQueryable();
        if (!request.Force)
        {
            var mappedIngredientIds = archiveDb.OfficialFoodIngredientHsMappings
                .AsNoTracking()
                .Where(mapping => mapping.IsActive)
                .Select(mapping => mapping.IngredientId);
            ingredientsQuery = ingredientsQuery.Where(ingredient =>
                !mappedIngredientIds.Contains(ingredient.Id));
        }

        var ingredients = await ingredientsQuery
            .Take(maxItems)
            .ToArrayAsync(cancellationToken);
        var catalog = await LoadCatalogAsync(countryCodes, cancellationToken);
        var refreshResult = await RefreshAsync(
            ingredients,
            countryCodes,
            catalog,
            cancellationToken);
        var mappedIngredientCount = refreshResult.ActiveMappings
            .Select(mapping => mapping.IngredientId)
            .Distinct()
            .Count();
        var countryCandidateCounts = refreshResult.ActiveMappings
            .GroupBy(mapping => string.IsNullOrWhiteSpace(mapping.CountryCode)
                ? "INTL"
                : mapping.CountryCode)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        return new OfficialFoodIngredientHsIndexResponse(
            ingredients.Length,
            mappedIngredientCount,
            refreshResult.CandidateCount,
            ingredients.Length - mappedIngredientCount,
            catalog.Select(row => row.CatalogVersionId).Distinct().Count(),
            catalog.Count,
            countryCandidateCounts,
            UtcNow());
    }

    private async Task<HsRefreshResult> RefreshAsync(
        IReadOnlyCollection<OfficialFoodIngredient> ingredients,
        IReadOnlyCollection<string>? countryCodes,
        CancellationToken cancellationToken)
    {
        var catalog = await LoadCatalogAsync(countryCodes, cancellationToken);
        return await RefreshAsync(ingredients, countryCodes, catalog, cancellationToken);
    }

    private async Task<HsRefreshResult> RefreshAsync(
        IReadOnlyCollection<OfficialFoodIngredient> ingredients,
        IReadOnlyCollection<string>? countryCodes,
        IReadOnlyList<HsCatalogRow> catalog,
        CancellationToken cancellationToken)
    {
        if (ingredients.Count == 0)
        {
            return new HsRefreshResult(0, []);
        }

        var now = UtcNow();
        var ingredientIds = ingredients.Select(ingredient => ingredient.Id).ToArray();
        var existingMappings = await archiveDb.OfficialFoodIngredientHsMappings
            .Where(mapping => Enumerable.Contains(ingredientIds, mapping.IngredientId))
            .ToArrayAsync(cancellationToken);
        var existingByKey = existingMappings.ToDictionary(
            mapping => new MappingKey(
                mapping.IngredientId,
                mapping.HsCodeCatalogVersionId,
                mapping.HsCodeEntryId));
        var generatedKeys = new HashSet<MappingKey>();

        foreach (var ingredient in ingredients)
        {
            var matches = OfficialFoodIngredientHsCandidateMatcher.Match(
                ingredient,
                catalog,
                CandidateLimitPerCatalog);
            foreach (var match in matches)
            {
                var key = new MappingKey(
                    ingredient.Id,
                    match.Catalog.CatalogVersionId,
                    match.Catalog.HsCodeEntryId);
                generatedKeys.Add(key);
                if (!existingByKey.TryGetValue(key, out var mapping))
                {
                    mapping = new OfficialFoodIngredientHsMapping
                    {
                        IngredientId = ingredient.Id,
                        HsCodeCatalogVersionId = match.Catalog.CatalogVersionId,
                        HsCodeEntryId = match.Catalog.HsCodeEntryId,
                        MappingState = OfficialFoodIngredientHsMappingStates.Candidate,
                        CreatedAtUtc = now
                    };
                    archiveDb.OfficialFoodIngredientHsMappings.Add(mapping);
                    existingByKey.Add(key, mapping);
                }

                Apply(mapping, match, now);
                if (mapping.MappingState == OfficialFoodIngredientHsMappingStates.Rejected)
                {
                    mapping.IsActive = false;
                }
                else
                {
                    mapping.IsActive = true;
                }
            }
        }

        foreach (var mapping in existingMappings.Where(mapping =>
                     mapping.IsActive
                     && mapping.MappingState == OfficialFoodIngredientHsMappingStates.Candidate
                     && (countryCodes is null
                         || countryCodes.Count == 0
                         || countryCodes.Contains(mapping.CountryCode))))
        {
            var key = new MappingKey(
                mapping.IngredientId,
                mapping.HsCodeCatalogVersionId,
                mapping.HsCodeEntryId);
            if (!generatedKeys.Contains(key))
            {
                mapping.IsActive = false;
                mapping.MappingState = OfficialFoodIngredientHsMappingStates.Superseded;
                mapping.UpdatedAtUtc = now;
                mapping.LastCheckedAtUtc = now;
            }
        }

        await archiveDb.SaveChangesAsync(cancellationToken);
        var activeMappings = generatedKeys
            .Select(key => existingByKey[key])
            .Where(mapping =>
                mapping.IsActive
                && mapping.MappingState != OfficialFoodIngredientHsMappingStates.Rejected)
            .Select(mapping => new ActiveMapping(mapping.IngredientId, mapping.CountryCode))
            .ToArray();
        return new HsRefreshResult(activeMappings.Length, activeMappings);
    }

    private async Task<IReadOnlyList<HsCatalogRow>> LoadCatalogAsync(
        IReadOnlyCollection<string>? countryCodes,
        CancellationToken cancellationToken)
    {
        var query = customsDb.HsCodeEntries
            .AsNoTracking()
            .Where(entry =>
                entry.IsActive
                && entry.CatalogVersion != null
                && entry.CatalogVersion.IsActive
                && entry.Level != HsCodeLevel.Chapter);
        if (countryCodes is { Count: > 0 })
        {
            query = query.Where(entry => countryCodes.Contains(entry.CatalogVersion!.CountryCode));
        }

        return await query
            .Select(entry => new HsCatalogRow(
                entry.CatalogVersionId,
                entry.Id,
                entry.CatalogVersion!.CountryCode,
                entry.CatalogVersion.StandardCode,
                entry.CatalogVersion.Revision,
                entry.CatalogVersion.CodeDigits,
                entry.CatalogVersion.EffectiveFrom,
                entry.CatalogVersion.EffectiveTo,
                entry.CatalogVersion.ImportedAtUtc,
                entry.CatalogVersion.SourceName,
                entry.CatalogVersion.SourceUrl,
                entry.Code,
                entry.NormalizedCode,
                (int)entry.Level,
                entry.KoreanName,
                entry.EnglishName,
                entry.Description,
                entry.SearchKeywords,
                entry.BusinessCategory))
            .ToArrayAsync(cancellationToken);
    }

    private async Task<bool> HasActiveCatalogAsync(
        string? countryCode,
        CancellationToken cancellationToken)
        => await customsDb.HsCodeCatalogVersions
            .AsNoTracking()
            .AnyAsync(version =>
                    version.IsActive
                    && (countryCode == null || version.CountryCode == countryCode),
                cancellationToken);

    private async Task<OfficialFoodIngredient?> FindIngredientAsync(
        string? ingredientKey,
        string? ingredientName,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(ingredientKey))
        {
            var key = ingredientKey.Trim();
            var byKey = await archiveDb.OfficialFoodIngredients
                .AsNoTracking()
                .FirstOrDefaultAsync(ingredient => ingredient.IngredientKey == key, cancellationToken);
            if (byKey is not null)
            {
                return byKey;
            }
        }

        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            return null;
        }

        var name = ingredientName.Trim();
        var normalizedName = OfficialFoodRecipeIngredientParser.NormalizeName(name);
        return await archiveDb.OfficialFoodIngredients
            .AsNoTracking()
            .OrderByDescending(ingredient => ingredient.CanonicalName == name)
            .ThenBy(ingredient => ingredient.Id)
            .FirstOrDefaultAsync(ingredient =>
                    ingredient.CanonicalName == name
                    || ingredient.NormalizedName == normalizedName,
                cancellationToken);
    }

    private static void Apply(
        OfficialFoodIngredientHsMapping mapping,
        HsCandidateMatch match,
        DateTime now)
    {
        var catalog = match.Catalog;
        mapping.CountryCode = catalog.CountryCode;
        mapping.JurisdictionUseCode = ResolveJurisdictionUseCode(
            catalog.CountryCode,
            catalog.CodeDigits);
        mapping.StandardCode = catalog.StandardCode;
        mapping.CatalogRevision = catalog.Revision;
        mapping.CodeDigits = catalog.CodeDigits;
        mapping.CatalogEffectiveFrom = catalog.EffectiveFrom;
        mapping.CatalogEffectiveTo = catalog.EffectiveTo;
        mapping.CatalogImportedAtUtc = catalog.ImportedAtUtc;
        mapping.HsCode = catalog.Code;
        mapping.NormalizedHsCode = catalog.NormalizedCode;
        mapping.HsCodeLevel = catalog.Level;
        mapping.KoreanName = catalog.KoreanName;
        mapping.EnglishName = catalog.EnglishName;
        mapping.Description = catalog.Description;
        mapping.MatchMethod = match.MatchMethod;
        mapping.MatchQualityCode = match.MatchQualityCode;
        mapping.MatchConfidence = match.MatchConfidence;
        mapping.MatchBasis = match.MatchBasis;
        mapping.ReviewReason = match.ReviewReason;
        mapping.RequiredProductDetailsJson = JsonSerializer.Serialize(
            match.RequiredProductDetails,
            JsonOptions);
        mapping.SourceName = catalog.SourceName;
        mapping.SourceUrl = catalog.SourceUrl;
        mapping.RequiresProfessionalReview = true;
        mapping.UpdatedAtUtc = now;
        mapping.LastCheckedAtUtc = now;
    }

    private static OfficialFoodIngredientHsCandidateDto Map(
        OfficialFoodIngredientHsMapping mapping)
        => new(
            mapping.Id,
            mapping.HsCodeEntryId,
            mapping.CountryCode,
            mapping.JurisdictionUseCode,
            mapping.StandardCode,
            mapping.CatalogRevision,
            mapping.CodeDigits,
            mapping.HsCode,
            mapping.NormalizedHsCode,
            mapping.HsCodeLevel,
            mapping.KoreanName,
            mapping.EnglishName,
            mapping.Description,
            mapping.MatchMethod,
            mapping.MatchQualityCode,
            mapping.MatchConfidence,
            mapping.MappingState,
            mapping.MatchBasis,
            mapping.ReviewReason,
            DeserializeDetails(mapping.RequiredProductDetailsJson),
            mapping.SourceName,
            mapping.SourceUrl,
            mapping.CatalogEffectiveFrom,
            mapping.CatalogEffectiveTo,
            mapping.CatalogImportedAtUtc,
            mapping.LastCheckedAtUtc,
            mapping.RequiresProfessionalReview,
            IsDeclarationReady: false);

    private static IReadOnlyList<string> DeserializeDetails(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string ResolveJurisdictionUseCode(string countryCode, int codeDigits)
    {
        if (codeDigits <= 6 || string.IsNullOrWhiteSpace(countryCode)
                            || countryCode is "INTL" or "WCO")
        {
            return OfficialFoodIngredientHsJurisdictionUseCodes.InternationalHsReference;
        }

        return countryCode switch
        {
            "KR" => OfficialFoodIngredientHsJurisdictionUseCodes.KoreaExportDeclaration,
            "US" => OfficialFoodIngredientHsJurisdictionUseCodes.UnitedStatesImportEntry,
            _ => OfficialFoodIngredientHsJurisdictionUseCodes.NationalReference
        };
    }

    private static string? NormalizeCountryCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length is < 2 or > 10
            || normalized.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new ArgumentException("국가 코드는 2~10자리 영문·숫자여야 합니다.", nameof(value));
        }

        return normalized;
    }

    private static IReadOnlyCollection<string>? NormalizeCountryCodes(
        IReadOnlyList<string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return null;
        }

        return values
            .Select(NormalizeCountryCode)
            .Where(value => value is not null)
            .Select(value => value!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private sealed record MappingKey(
        long IngredientId,
        long CatalogVersionId,
        long HsCodeEntryId);

    private sealed record ActiveMapping(long IngredientId, string CountryCode);

    private sealed record HsRefreshResult(
        int CandidateCount,
        IReadOnlyList<ActiveMapping> ActiveMappings);
}

internal static class OfficialFoodIngredientHsCandidateMatcher
{
    private static readonly IReadOnlyList<string> CommonProductDetails =
    [
        "실제 판매 상품명과 상품 사진",
        "원재료명과 성분 함량",
        "품종·생물종 또는 식물종",
        "신선·냉장·냉동·건조·분말 등 물리적 상태",
        "가열·혼합·발효 등 가공공정",
        "포장 단위와 최종 용도",
        "원산지·수출국·도착국"
    ];

    private static readonly IReadOnlyList<HsSearchProfile> Profiles =
    [
        Profile(["고춧가루", "chili powder"], ["090422"], "분쇄 여부와 매운고추 품종"),
        Profile(["말린고추", "건고추", "dried chili"], ["090421"], "건조·분쇄 여부와 품종"),
        Profile(["고추", "피망", "chili", "pepper"], ["070960", "090421", "090422"], "신선·건조·분쇄 여부와 품종"),
        Profile(["밀가루", "wheat flour"], ["1101"], "밀 이외 곡물 혼합 여부와 첨가물"),
        Profile(["쌀가루", "rice flour"], ["110290"], "쌀 함량과 혼합 곡물 여부"),
        Profile(["옥수수전분", "corn starch"], ["110812"], "변성전분 여부와 식품용 용도"),
        Profile(["감자전분", "potato starch"], ["110813"], "변성전분 여부와 식품용 용도"),
        Profile(["전분", "starch"], ["1108"], "원료 곡물·뿌리 종류와 변성 여부"),
        Profile(["쌀", "멥쌀", "찹쌀", "rice"], ["1006"], "벼·현미·정미·쇄미 여부와 품종"),
        Profile(["밀", "wheat"], ["1001"], "종자용 여부와 듀럼밀 여부"),
        Profile(["보리", "barley"], ["1003"], "종자용 여부와 가공상태"),
        Profile(["옥수수", "corn", "maize"], ["1005"], "종자용 여부와 스위트콘 여부"),
        Profile(["감자", "potato"], ["0701"], "신선·냉장·냉동 여부와 종자용 여부"),
        Profile(["고구마", "sweet potato"], ["071420"], "신선·건조·냉동 여부"),
        Profile(["마늘", "garlic"], ["070320", "071290"], "신선·냉장·건조·분말 여부"),
        Profile(["양파", "onion"], ["070310", "071220"], "신선·냉장·건조·분말 여부"),
        Profile(["당근", "carrot"], ["070610"], "신선·냉장·냉동·조제 여부"),
        Profile(["양배추", "배추", "cabbage"], ["0704"], "품종과 신선·냉장 여부"),
        Profile(["오이", "cucumber"], ["0707"], "신선·냉장·절임 여부"),
        Profile(["사과", "apple"], ["080810"], "신선·건조·조제 여부"),
        Profile(["배", "pear"], ["080830"], "신선·건조·조제 여부"),
        Profile(["바나나", "banana"], ["0803"], "플랜틴 여부와 신선·건조 여부"),
        Profile(["포도", "grape"], ["080610", "080620"], "신선·건조 여부"),
        Profile(["쇠고기", "소고기", "beef"], ["0201", "0202"], "냉장·냉동 여부, 부위, 뼈 포함 여부"),
        Profile(["돼지고기", "pork"], ["0203"], "냉장·냉동 여부, 부위, 뼈 포함 여부"),
        Profile(["닭고기", "chicken"], ["0207"], "냉장·냉동 여부, 절단 여부, 부위"),
        Profile(["달걀", "계란", "egg"], ["0407", "0408"], "껍질 포함 여부와 건조·가공 여부"),
        Profile(["우유", "milk"], ["0401", "0402"], "지방 함량, 농축·가당·분말 여부"),
        Profile(["버터", "butter"], ["0405"], "유지방 함량과 혼합 여부"),
        Profile(["치즈", "cheese"], ["0406"], "치즈 종류, 숙성·가공 여부"),
        Profile(["새우", "shrimp", "prawn"], ["0306"], "종, 냉동 여부, 껍질·조리 여부"),
        Profile(["오징어", "squid"], ["0307"], "종, 냉동·건조·조리 여부"),
        Profile(["김", "해조", "seaweed"], ["121221", "121229"], "식용 여부, 종, 건조·조미 여부"),
        Profile(["소금", "salt"], ["2501"], "식용 여부, 정제·요오드 첨가 여부"),
        Profile(["설탕", "sugar"], ["1701"], "원당·정제당 여부, 당 종류와 향미·착색 여부"),
        Profile(["간장", "soy sauce"], ["210310"], "성분 함량과 혼합·조제 여부"),
        Profile(["케첩", "ketchup"], ["210320"], "토마토 함량과 조제 형태"),
        Profile(["참기름", "sesame oil"], ["151550"], "정제 여부와 혼합유 여부"),
        Profile(["콩기름", "대두유", "soybean oil"], ["1507"], "조유·정제유 여부와 혼합 여부"),
        Profile(["올리브유", "olive oil"], ["1509"], "버진·정제 여부와 혼합 여부"),
        Profile(["커피", "coffee"], ["0901", "2101"], "원두·분쇄·인스턴트 여부와 카페인 제거 여부"),
        Profile(["차", "tea"], ["0902"], "차 종류, 발효 여부와 포장 중량"),
        Profile(["생수", "물", "water"], ["2201"], "탄산·감미·향미 첨가 여부"),
        Profile(["맥주", "beer"], ["2203"], "알코올 함량과 용기 용량"),
        Profile(["와인", "wine"], ["2204"], "원료, 알코올 함량과 용기 용량")
    ];

    public static IReadOnlyList<HsCandidateMatch> Match(
        OfficialFoodIngredient ingredient,
        IReadOnlyList<HsCatalogRow> catalog,
        int limitPerCatalog)
    {
        var ingredientText = NormalizeText(
            string.IsNullOrWhiteSpace(ingredient.NormalizedName)
                ? ingredient.CanonicalName
                : ingredient.NormalizedName);
        if (ingredientText.Length == 0 || catalog.Count == 0)
        {
            return [];
        }

        var profileMatches = Profiles
            .Select(profile => new
            {
                Profile = profile,
                Specificity = profile.Terms
                    .Select(NormalizeText)
                    .Where(term => term.Length >= 2
                        ? ingredientText.Contains(term, StringComparison.Ordinal)
                        : ingredientText == term)
                    .Select(term => term.Length)
                    .DefaultIfEmpty(0)
                    .Max()
            })
            .Where(match => match.Specificity > 0)
            .ToArray();
        var bestSpecificity = profileMatches
            .Select(match => match.Specificity)
            .DefaultIfEmpty(0)
            .Max();
        var profiles = profileMatches
            .Where(match => match.Specificity == bestSpecificity)
            .Select(match => match.Profile)
            .ToArray();
        var requiredDetails = CommonProductDetails
            .Concat(profiles.SelectMany(profile => profile.RequiredDetails))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return catalog
            .Select(row => Score(ingredientText, profiles, requiredDetails, row))
            .Where(match => match is not null)
            .Select(match => match!)
            .GroupBy(match => match.Catalog.CatalogVersionId)
            .SelectMany(group => group
                .OrderByDescending(match => match.MatchConfidence)
                .ThenBy(match => match.Catalog.Level == (int)HsCodeLevel.Subheading ? 0 : 1)
                .ThenBy(match => match.Catalog.NormalizedCode)
                .Take(Math.Clamp(limitPerCatalog, 1, 20)))
            .OrderBy(match => match.Catalog.CountryCode)
            .ThenByDescending(match => match.MatchConfidence)
            .ThenBy(match => match.Catalog.NormalizedCode)
            .ToArray();
    }

    private static HsCandidateMatch? Score(
        string ingredientText,
        IReadOnlyCollection<HsSearchProfile> profiles,
        IReadOnlyList<string> requiredDetails,
        HsCatalogRow row)
    {
        if (!IsFoodScope(row))
        {
            return null;
        }

        var code = DigitsOnly(row.NormalizedCode);
        var matchedPrefixes = profiles
            .SelectMany(profile => profile.CodePrefixes)
            .Where(prefix => code.StartsWith(prefix, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var catalogName = NormalizeText(row.KoreanName);
        var catalogEnglishName = NormalizeText(row.EnglishName);
        var catalogSearchText = NormalizeText(
            string.Join(' ', row.KoreanName, row.EnglishName, row.Description, row.SearchKeywords));
        var exactName = catalogName == ingredientText || catalogEnglishName == ingredientText;
        var textMatch = ingredientText.Length >= 2
                        && catalogSearchText.Contains(ingredientText, StringComparison.Ordinal);

        if (!exactName && matchedPrefixes.Length == 0 && !textMatch)
        {
            return null;
        }

        if (exactName)
        {
            return new HsCandidateMatch(
                row,
                "CatalogNameSearch",
                OfficialFoodIngredientHsMatchQualityCodes.ExactCatalogNameCandidate,
                0.78m,
                $"재료명 '{ingredientText}'과 카탈로그 품명이 정규화 기준으로 일치",
                "품명이 같아도 상품의 가공상태와 구성에 따라 세번이 달라질 수 있습니다.",
                requiredDetails);
        }

        if (matchedPrefixes.Length > 0)
        {
            return new HsCandidateMatch(
                row,
                "CuratedHsFamilySearch",
                OfficialFoodIngredientHsMatchQualityCodes.CuratedHsFamilyCandidate,
                0.64m,
                $"재료군 검토 접두어 {string.Join(", ", matchedPrefixes)}에 해당",
                "재료군 수준의 후보이며 실제 물품 설명과 관세율표 주·호의 용어를 대조해야 합니다.",
                requiredDetails);
        }

        return new HsCandidateMatch(
            row,
            "CatalogTextSearch",
            OfficialFoodIngredientHsMatchQualityCodes.CatalogTextCandidate,
            0.52m,
            $"카탈로그 품명·설명·검색어에 재료명 '{ingredientText}' 포함",
            "문자열 포함 검색 결과이므로 동명이물과 조제품 여부를 우선 검토해야 합니다.",
            requiredDetails);
    }

    private static bool IsFoodScope(HsCatalogRow row)
    {
        if (row.BusinessCategory == HsCodeBusinessCategory.Food)
        {
            return true;
        }

        var code = DigitsOnly(row.NormalizedCode);
        if (code.Length < 2 || !int.TryParse(code[..2], out var chapter))
        {
            return false;
        }

        return chapter is >= 1 and <= 24 || code.StartsWith("2501", StringComparison.Ordinal);
    }

    private static HsSearchProfile Profile(
        IReadOnlyList<string> terms,
        IReadOnlyList<string> codePrefixes,
        params string[] requiredDetails)
        => new(terms, codePrefixes, requiredDetails);

    private static string NormalizeText(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Normalize(NormalizationForm.FormKC).ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static string DigitsOnly(string value)
        => new(value.Where(char.IsDigit).ToArray());

    private sealed record HsSearchProfile(
        IReadOnlyList<string> Terms,
        IReadOnlyList<string> CodePrefixes,
        IReadOnlyList<string> RequiredDetails);
}

internal sealed record HsCatalogRow(
    long CatalogVersionId,
    long HsCodeEntryId,
    string CountryCode,
    string StandardCode,
    string Revision,
    int CodeDigits,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    DateTime ImportedAtUtc,
    string SourceName,
    string SourceUrl,
    string Code,
    string NormalizedCode,
    int Level,
    string KoreanName,
    string EnglishName,
    string Description,
    string SearchKeywords,
    HsCodeBusinessCategory BusinessCategory);

internal sealed record HsCandidateMatch(
    HsCatalogRow Catalog,
    string MatchMethod,
    string MatchQualityCode,
    decimal MatchConfidence,
    string MatchBasis,
    string ReviewReason,
    IReadOnlyList<string> RequiredProductDetails);
