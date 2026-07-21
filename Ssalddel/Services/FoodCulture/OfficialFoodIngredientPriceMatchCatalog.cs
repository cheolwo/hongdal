using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Services.FoodCulture;

public sealed record OfficialFoodIngredientPriceMatch(
    string CountryCode,
    string SourceKey,
    string ExternalCategoryCode,
    string ExternalItemCode,
    string ExternalItemName,
    string ExternalVariantCode,
    string ExternalVariantName,
    string MatchMethod,
    string MatchQualityCode,
    decimal MatchConfidence,
    string MappingNote,
    string SourceUrl);

public interface IOfficialFoodIngredientPriceMatchCatalog
{
    IReadOnlyList<OfficialFoodIngredientPriceMatch> Match(OfficialFoodIngredient ingredient);
}

public sealed class OfficialFoodIngredientPriceMatchCatalog(
    IFoodPriceCrosswalkCatalog domesticCrosswalkCatalog)
    : IOfficialFoodIngredientPriceMatchCatalog
{
    private const string KamisSourceUrl =
        "https://www.kamis.or.kr/customer/reference/openapi_list.do";
    private const string UsdaSourceUrl = "https://quickstats.nass.usda.gov/api";

    private static readonly HashSet<string> PriceEligibleCategories =
    [
        OfficialFoodIngredientCategoryCodes.GrainAndStarch,
        OfficialFoodIngredientCategoryCodes.LegumeAndSoy,
        OfficialFoodIngredientCategoryCodes.Vegetable,
        OfficialFoodIngredientCategoryCodes.Fruit,
        OfficialFoodIngredientCategoryCodes.Mushroom,
        OfficialFoodIngredientCategoryCodes.Seaweed,
        OfficialFoodIngredientCategoryCodes.Meat,
        OfficialFoodIngredientCategoryCodes.PoultryAndEgg,
        OfficialFoodIngredientCategoryCodes.Seafood,
        OfficialFoodIngredientCategoryCodes.Dairy,
        OfficialFoodIngredientCategoryCodes.NutAndSeed
    ];

    private static readonly IReadOnlyDictionary<string, string> KamisAliases =
        NormalizeAliases(
        [
            ("소고기", "쇠고기"),
            ("달걀", "계란"),
            ("백미", "쌀")
        ]);

    private static readonly IReadOnlyDictionary<string, UsdaCommodityMatch> UsdaMatches =
        BuildUsdaMatches();

    public IReadOnlyList<OfficialFoodIngredientPriceMatch> Match(
        OfficialFoodIngredient ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        if (!string.Equals(ingredient.LanguageCode, "ko", StringComparison.OrdinalIgnoreCase)
            || !PriceEligibleCategories.Contains(ingredient.CategoryCode)
            || string.Equals(
                ingredient.ClassificationState,
                OfficialFoodIngredientClassificationStates.PendingReview,
                StringComparison.Ordinal)
            || (ingredient.ClassificationConfidence < 0.80m
                && !string.Equals(
                    ingredient.ClassificationState,
                    OfficialFoodIngredientClassificationStates.Confirmed,
                    StringComparison.Ordinal)))
        {
            return [];
        }

        var normalizedName = OfficialFoodRecipeIngredientParser.NormalizeName(
            ingredient.NormalizedName);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return [];
        }

        var matches = new List<OfficialFoodIngredientPriceMatch>();
        AddKamisMatches(ingredient, normalizedName, matches);
        AddUsdaMatch(ingredient, normalizedName, matches);
        return matches;
    }

    private void AddKamisMatches(
        OfficialFoodIngredient ingredient,
        string normalizedName,
        ICollection<OfficialFoodIngredientPriceMatch> matches)
    {
        var lookupName = KamisAliases.GetValueOrDefault(normalizedName, normalizedName);
        var isAlias = !string.Equals(lookupName, normalizedName, StringComparison.Ordinal);
        var candidates = domesticCrosswalkCatalog.GetAll()
            .Where(entry => IsCompatibleKamisCategory(
                ingredient.CategoryCode,
                entry.AtCategoryCode))
            .Where(entry =>
                string.Equals(Normalize(entry.ProductName), lookupName, StringComparison.Ordinal)
                || string.Equals(Normalize(entry.AtItemName), lookupName, StringComparison.Ordinal))
            .GroupBy(
                entry => string.Join('|', entry.AtCategoryCode, entry.AtItemCode),
                StringComparer.Ordinal)
            .Select(group => group
                .OrderBy(entry => entry.MatchQualityCode == "ExactCommodity" ? 0 : 1)
                .ThenBy(entry => entry.HsPrefix, StringComparer.Ordinal)
                .First());

        foreach (var entry in candidates)
        {
            matches.Add(new OfficialFoodIngredientPriceMatch(
                "KR",
                OfficialFoodIngredientPublicPriceSourceKeys.Kamis,
                entry.AtCategoryCode,
                entry.AtItemCode,
                entry.AtItemName,
                string.Join(',', entry.AtVarietyCodes),
                string.Empty,
                isAlias ? "ReviewedAlias" : "ExactNormalizedName",
                isAlias ? "CommonAlias" : entry.MatchQualityCode,
                isAlias ? 0.95m : 0.99m,
                $"{entry.Note} 레시피 재료와 KAMIS 조사 품목의 원재료 수준 참고 매핑입니다.",
                KamisSourceUrl));
        }
    }

    private static void AddUsdaMatch(
        OfficialFoodIngredient ingredient,
        string normalizedName,
        ICollection<OfficialFoodIngredientPriceMatch> matches)
    {
        if (!UsdaMatches.TryGetValue(normalizedName, out var match)
            || !string.Equals(match.CategoryCode, ingredient.CategoryCode, StringComparison.Ordinal))
        {
            return;
        }

        matches.Add(new OfficialFoodIngredientPriceMatch(
            "US",
            OfficialFoodIngredientPublicPriceSourceKeys.UsdaNass,
            match.Sector,
            match.Commodity,
            match.Commodity,
            string.Empty,
            string.Empty,
            match.IsAlias ? "ReviewedAlias" : "ReviewedCommodityName",
            match.MatchQualityCode,
            match.Confidence,
            match.Note,
            UsdaSourceUrl));
    }

    private static bool IsCompatibleKamisCategory(
        string ingredientCategoryCode,
        string kamisCategoryCode)
        => kamisCategoryCode switch
        {
            "100" => ingredientCategoryCode is
                OfficialFoodIngredientCategoryCodes.GrainAndStarch
                or OfficialFoodIngredientCategoryCodes.LegumeAndSoy,
            "200" => ingredientCategoryCode is
                OfficialFoodIngredientCategoryCodes.Vegetable
                or OfficialFoodIngredientCategoryCodes.Fruit,
            "300" => ingredientCategoryCode is
                OfficialFoodIngredientCategoryCodes.LegumeAndSoy
                or OfficialFoodIngredientCategoryCodes.NutAndSeed,
            "400" => ingredientCategoryCode == OfficialFoodIngredientCategoryCodes.Fruit,
            "500" => ingredientCategoryCode is
                OfficialFoodIngredientCategoryCodes.Meat
                or OfficialFoodIngredientCategoryCodes.PoultryAndEgg
                or OfficialFoodIngredientCategoryCodes.Dairy,
            "600" => ingredientCategoryCode is
                OfficialFoodIngredientCategoryCodes.Seafood
                or OfficialFoodIngredientCategoryCodes.Seaweed,
            _ => false
        };

    private static IReadOnlyDictionary<string, UsdaCommodityMatch> BuildUsdaMatches()
    {
        var matches = new Dictionary<string, UsdaCommodityMatch>(StringComparer.Ordinal);
        Add("쌀", "RICE", "CROPS", OfficialFoodIngredientCategoryCodes.GrainAndStarch);
        Add("백미", "RICE", "CROPS", OfficialFoodIngredientCategoryCodes.GrainAndStarch, true);
        Add("옥수수", "CORN", "CROPS", OfficialFoodIngredientCategoryCodes.GrainAndStarch);
        Add("밀", "WHEAT", "CROPS", OfficialFoodIngredientCategoryCodes.GrainAndStarch);
        Add("대두", "SOYBEANS", "CROPS", OfficialFoodIngredientCategoryCodes.LegumeAndSoy);
        Add("감자", "POTATOES", "CROPS", OfficialFoodIngredientCategoryCodes.GrainAndStarch);
        Add("토마토", "TOMATOES", "CROPS", OfficialFoodIngredientCategoryCodes.Vegetable);
        Add("양파", "ONIONS", "CROPS", OfficialFoodIngredientCategoryCodes.Vegetable);
        Add("사과", "APPLES", "CROPS", OfficialFoodIngredientCategoryCodes.Fruit);
        Add("오렌지", "ORANGES", "CROPS", OfficialFoodIngredientCategoryCodes.Fruit);
        Add("포도", "GRAPES", "CROPS", OfficialFoodIngredientCategoryCodes.Fruit);
        Add("딸기", "STRAWBERRIES", "CROPS", OfficialFoodIngredientCategoryCodes.Fruit);
        Add("땅콩", "PEANUTS", "CROPS", OfficialFoodIngredientCategoryCodes.NutAndSeed);
        AddUpstream("쇠고기", "CATTLE", OfficialFoodIngredientCategoryCodes.Meat);
        AddUpstream("소고기", "CATTLE", OfficialFoodIngredientCategoryCodes.Meat, true);
        AddUpstream("돼지고기", "HOGS", OfficialFoodIngredientCategoryCodes.Meat);
        AddUpstream("닭고기", "BROILERS", OfficialFoodIngredientCategoryCodes.PoultryAndEgg);
        Add("계란", "EGGS", "ANIMALS & PRODUCTS", OfficialFoodIngredientCategoryCodes.PoultryAndEgg);
        Add("달걀", "EGGS", "ANIMALS & PRODUCTS", OfficialFoodIngredientCategoryCodes.PoultryAndEgg, true);
        Add("우유", "MILK", "ANIMALS & PRODUCTS", OfficialFoodIngredientCategoryCodes.Dairy);
        Add("메기", "CATFISH", "ANIMALS & PRODUCTS", OfficialFoodIngredientCategoryCodes.Seafood);
        Add("송어", "TROUT", "ANIMALS & PRODUCTS", OfficialFoodIngredientCategoryCodes.Seafood);
        return matches;

        void Add(
            string name,
            string commodity,
            string sector,
            string categoryCode,
            bool isAlias = false)
        {
            matches[Normalize(name)] = new UsdaCommodityMatch(
                commodity,
                sector,
                categoryCode,
                isAlias,
                "ExactCommodity",
                isAlias ? 0.95m : 0.99m,
                "USDA NASS 전국 생산자 수취가격의 원문 단위 참고값입니다. 미국 소매가격이 아니며 한국 가격과 직접 비교하지 않습니다.");
        }

        void AddUpstream(
            string name,
            string commodity,
            string categoryCode,
            bool isAlias = false)
        {
            matches[Normalize(name)] = new UsdaCommodityMatch(
                commodity,
                "ANIMALS & PRODUCTS",
                categoryCode,
                isAlias,
                "UpstreamRepresentative",
                isAlias ? 0.84m : 0.86m,
                "완제품 육류 가격이 아니라 원료 가축의 미국 생산자 수취가격입니다. 재료 구매가격으로 해석하지 않습니다.");
        }
    }

    private static IReadOnlyDictionary<string, string> NormalizeAliases(
        IEnumerable<(string Alias, string Canonical)> aliases)
        => aliases.ToDictionary(
            item => Normalize(item.Alias),
            item => Normalize(item.Canonical),
            StringComparer.Ordinal);

    private static string Normalize(string value)
        => OfficialFoodRecipeIngredientParser.NormalizeName(value);

    private sealed record UsdaCommodityMatch(
        string Commodity,
        string Sector,
        string CategoryCode,
        bool IsAlias,
        string MatchQualityCode,
        decimal Confidence,
        string Note);
}
