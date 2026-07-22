using Ssalddel.Contracts.Common.Content;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Information;

public sealed record OfficialFoodIngredientPurchaseSelection(
    OfficialFoodIngredientDto Ingredient,
    OfficialFoodIngredientRelatedRecipeDto? Recipe);

public sealed record OfficialFoodDishIngredientPurchaseSelection(
    OfficialFoodDishDetailDto Dish,
    OfficialFoodRecipeIngredientDto Ingredient,
    string SourcingModeCode);

public static class OfficialFoodIngredientPresentation
{
    public static string CountryName(string? countryCode)
        => countryCode?.Trim().ToUpperInvariant() switch
        {
            "KR" => "한국",
            "JP" => "일본",
            "GB" => "영국",
            "US" => "미국",
            "CA" => "캐나다",
            "FR" => "프랑스",
            { Length: > 0 } code => code,
            _ => "국가 미지정"
        };

    public static string ReviewStateLabel(string? reviewState)
        => reviewState switch
        {
            OfficialFoodRecipeReviewStates.Approved => "검토 완료",
            OfficialFoodRecipeReviewStates.PendingReview => "공식 원천 수집 후보",
            _ => "상태 확인 필요"
        };

    public static string CompanyRelationLabel(string? relationCode)
        => relationCode switch
        {
            OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer => "국내 제조업소",
            OfficialFoodIngredientCompanyRelationCodes.DomesticImporter => "국내 수입업체",
            OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer => "해외 제조업소",
            _ => "관계 확인 필요"
        };

    public static string CompanyVerificationLabel(string? statusCode)
        => statusCode switch
        {
            OfficialFoodIngredientCompanyVerificationStatusCodes.OfficialProductReport =>
                "공식 품목제조보고 근거",
            OfficialFoodIngredientCompanyVerificationStatusCodes.OverseasFacilityMatched =>
                "해외 제조업소 명부 대조",
            OfficialFoodIngredientCompanyVerificationStatusCodes.ImportedLabelEvidenceOnly =>
                "수입제품 표시 이력 근거",
            _ => "최신 공식 상태 재확인 필요"
        };

    public static string CompanySourceStatusLabel(string? statusCode)
        => statusCode switch
        {
            OfficialFoodIngredientCompanySourceStatusCodes.Available => "조회 완료",
            OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured => "연동 준비 필요",
            OfficialFoodIngredientCompanySourceStatusCodes.Failed => "일시 조회 실패",
            OfficialFoodIngredientCompanySourceStatusCodes.SupportingSource => "보조 확인 원천",
            _ => "상태 확인 필요"
        };

    public static CommunityGroupPurchaseIngredientSeed? CreatePurchaseSeed(
        OfficialFoodIngredientPurchaseSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(selection.Ingredient);

        var ingredient = selection.Ingredient;
        var recipe = selection.Recipe;
        return CommunityGroupPurchaseIngredientSeed.Create(
            ingredient.IngredientKey,
            ingredient.CanonicalName,
            recipe?.RecipeTitle,
            recipe?.OriginalUrl,
            recipe is null ? null : $"{recipe.Provider} · {recipe.CountryCode}",
            recipe is null ? null : BuildRecipeIngredientText(recipe),
            BuildPriceReference(ingredient.PublicPrices ?? []),
            SelectPurchaseUnit(ingredient.PublicPrices ?? [], recipe));
    }

    public static CommunityGroupPurchaseIngredientSeed? CreatePurchaseSeed(
        OfficialFoodDishIngredientPurchaseSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(selection.Dish);
        ArgumentNullException.ThrowIfNull(selection.Ingredient);

        var dish = selection.Dish;
        var ingredient = selection.Ingredient;
        return CommunityGroupPurchaseIngredientSeed.Create(
            ingredient.IngredientKey,
            ingredient.CanonicalName,
            dish.RecipeTitle,
            dish.OriginalUrl,
            $"{dish.Provider} · {dish.Dish.CountryCode}",
            BuildRecipeIngredientText(ingredient),
            BuildPriceReference(ingredient.PublicPrices ?? []),
            SelectPurchaseUnit(ingredient.PublicPrices ?? [], ingredient.UnitText),
            dish.Dish.Name,
            dish.Dish.CountryCode,
            selection.SourcingModeCode);
    }

    public static string FormatPrice(OfficialFoodIngredientPublicPriceDto price)
    {
        ArgumentNullException.ThrowIfNull(price);
        return $"{price.AveragePrice:N2} {price.CurrencyCode} / {price.Unit}";
    }

    public static string BuildPriceReference(
        IReadOnlyList<OfficialFoodIngredientPublicPriceDto> prices)
        => string.Join(" | ", prices
            .OrderBy(price => string.Equals(price.CountryCode, "KR", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(price => price.CountryCode, StringComparer.Ordinal)
            .Take(2)
            .Select(price =>
                $"{price.CountryName} {price.MarketStageName} {FormatPrice(price)}, "
                + $"{price.ReferenceDate:yyyy.MM.dd}, {price.Provider}"));

    public static string SelectPurchaseUnit(
        IReadOnlyList<OfficialFoodIngredientPublicPriceDto> prices,
        OfficialFoodIngredientRelatedRecipeDto? recipe)
        => prices
               .OrderBy(price => string.Equals(price.CountryCode, "KR", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
               .Select(price => price.Unit?.Trim())
               .FirstOrDefault(unit => !string.IsNullOrWhiteSpace(unit))
           ?? (!string.IsNullOrWhiteSpace(recipe?.UnitText) ? recipe.UnitText.Trim() : "kg");

    public static string SelectPurchaseUnit(
        IReadOnlyList<OfficialFoodIngredientPublicPriceDto> prices,
        string? recipeUnit)
        => prices
               .OrderBy(price => string.Equals(price.CountryCode, "KR", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
               .Select(price => price.Unit?.Trim())
               .FirstOrDefault(unit => !string.IsNullOrWhiteSpace(unit))
           ?? (!string.IsNullOrWhiteSpace(recipeUnit) ? recipeUnit.Trim() : "kg");

    public static string BuildRecipeIngredientText(
        OfficialFoodIngredientRelatedRecipeDto recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        return string.Join(" · ", new[]
        {
            recipe.IngredientSourceName,
            recipe.QuantityText,
            recipe.PreparationNote
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    public static string BuildRecipeIngredientText(
        OfficialFoodRecipeIngredientDto ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        return string.Join(" · ", new[]
        {
            ingredient.SourceName,
            ingredient.QuantityText,
            ingredient.PreparationNote
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    public static string? SafeHttpUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.AbsoluteUri
            : null;
}
