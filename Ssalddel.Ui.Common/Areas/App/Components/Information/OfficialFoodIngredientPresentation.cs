using Ssalddel.Contracts.Common.Content;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Information;

public sealed record OfficialFoodIngredientPurchaseSelection(
    OfficialFoodIngredientDto Ingredient,
    OfficialFoodIngredientRelatedRecipeDto? Recipe);

public static class OfficialFoodIngredientPresentation
{
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

    public static string? SafeHttpUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.AbsoluteUri
            : null;
}
