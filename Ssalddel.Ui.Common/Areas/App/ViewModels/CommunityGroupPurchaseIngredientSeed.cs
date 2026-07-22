using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed record CommunityGroupPurchaseIngredientSeed
{
    public const string Route = CommunityPageRoutes.GroupPurchaseCreate;
    public const string IngredientKeyQueryName = "ingredientKey";
    public const string IngredientNameQueryName = "ingredient";
    public const string RecipeTitleQueryName = "recipe";
    public const string RecipeUrlQueryName = "recipeUrl";
    public const string RecipeSourceQueryName = "recipeSource";
    public const string RecipeQuantityQueryName = "recipeQuantity";
    public const string PriceReferenceQueryName = "priceReference";
    public const string PurchaseUnitQueryName = "purchaseUnit";
    public const string FoodNameQueryName = "food";
    public const string FoodCountryCodeQueryName = "foodCountry";
    public const string SourcingModeQueryName = "sourcingMode";

    private CommunityGroupPurchaseIngredientSeed(
        string ingredientKey,
        string ingredientName,
        string recipeTitle,
        string recipeUrl,
        string recipeSource,
        string recipeQuantity,
        string priceReference,
        string purchaseUnit,
        string foodName,
        string foodCountryCode,
        string sourcingModeCode)
    {
        IngredientKey = ingredientKey;
        IngredientName = ingredientName;
        RecipeTitle = recipeTitle;
        RecipeUrl = recipeUrl;
        RecipeSource = recipeSource;
        RecipeQuantity = recipeQuantity;
        PriceReference = priceReference;
        PurchaseUnit = purchaseUnit;
        FoodName = foodName;
        FoodCountryCode = foodCountryCode;
        SourcingModeCode = sourcingModeCode;
    }

    public string IngredientKey { get; }

    public string IngredientName { get; }

    public string RecipeTitle { get; }

    public string RecipeUrl { get; }

    public string RecipeSource { get; }

    public string RecipeQuantity { get; }

    public string PriceReference { get; }

    public string PurchaseUnit { get; }

    public string FoodName { get; }

    public string FoodCountryCode { get; }

    public string SourcingModeCode { get; }

    public bool IsGroupImportReview
        => SourcingModeCode == CommunityIngredientSourcingModeCodes.GroupImportReview;

    public string SourcingModeLabel
        => SourcingModeCode switch
        {
            CommunityIngredientSourcingModeCodes.DomesticGroupPurchase => "국내 공동구매 검토",
            CommunityIngredientSourcingModeCodes.GroupImportReview => "공동수입 검토",
            _ => "조달 경로 미정"
        };

    public string SuggestedTitle => IsGroupImportReview
        ? $"{IngredientName} 공동수입 검토 제안"
        : $"{IngredientName} 공동구매 제안";

    public string SuggestedProductKey => $"official-ingredient:{IngredientKey}";

    public string Fingerprint => string.Join('|',
        IngredientKey,
        IngredientName,
        RecipeTitle,
        RecipeUrl,
        PriceReference,
        PurchaseUnit,
        FoodName,
        FoodCountryCode,
        SourcingModeCode);

    public static CommunityGroupPurchaseIngredientSeed? Create(
        string? ingredientKey,
        string? ingredientName,
        string? recipeTitle = null,
        string? recipeUrl = null,
        string? recipeSource = null,
        string? recipeQuantity = null,
        string? priceReference = null,
        string? purchaseUnit = null,
        string? foodName = null,
        string? foodCountryCode = null,
        string? sourcingModeCode = null)
    {
        var normalizedName = Normalize(ingredientName, 160);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        var normalizedKey = Normalize(ingredientKey, 160);
        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            normalizedKey = Uri.EscapeDataString(normalizedName.ToLowerInvariant());
        }

        return new CommunityGroupPurchaseIngredientSeed(
            normalizedKey,
            normalizedName,
            Normalize(recipeTitle, 240),
            NormalizeHttpUrl(recipeUrl),
            Normalize(recipeSource, 240),
            Normalize(recipeQuantity, 160),
            Normalize(priceReference, 600),
            Normalize(purchaseUnit, 40, "kg"),
            Normalize(foodName, 240),
            NormalizeCountryCode(foodCountryCode),
            CommunityIngredientSourcingModeCodes.Normalize(sourcingModeCode));
    }

    public string ToNavigationUri()
        => BuildNavigationUri(Route);

    public string ToDemandNavigationUri()
        => BuildNavigationUri(CommunityPageRoutes.GroupPurchaseDemand);

    private string BuildNavigationUri(string route)
    {
        var parameters = new List<string>();
        AddParameter(parameters, IngredientKeyQueryName, IngredientKey);
        AddParameter(parameters, IngredientNameQueryName, IngredientName);
        AddParameter(parameters, RecipeTitleQueryName, RecipeTitle);
        AddParameter(parameters, RecipeUrlQueryName, RecipeUrl);
        AddParameter(parameters, RecipeSourceQueryName, RecipeSource);
        AddParameter(parameters, RecipeQuantityQueryName, RecipeQuantity);
        AddParameter(parameters, PriceReferenceQueryName, PriceReference);
        AddParameter(parameters, PurchaseUnitQueryName, PurchaseUnit);
        AddParameter(parameters, FoodNameQueryName, FoodName);
        AddParameter(parameters, FoodCountryCodeQueryName, FoodCountryCode);
        AddParameter(parameters, SourcingModeQueryName, SourcingModeCode);
        return $"{route}?{string.Join('&', parameters)}";
    }

    public string BuildSuggestedDescription()
    {
        var lines = new List<string>
        {
            $"{IngredientName}의 공개 가격과 활용 레시피를 확인한 뒤 공동구매 수요를 모으기 위한 초안입니다.",
            string.Empty,
            "[공공데이터 참고 근거]",
            $"재료: {IngredientName} ({IngredientKey})"
        };

        AddLine(lines, "참고 레시피", RecipeTitle);
        AddLine(lines, "둘러본 음식", FoodName);
        if (!string.IsNullOrWhiteSpace(FoodCountryCode))
        {
            lines.Add($"음식 문화 국가: {FoodCountryCode} (상품 원산지·출발국으로 자동 사용하지 않음)");
        }
        AddLine(lines, "조달 검토 방향", SourcingModeLabel);
        AddLine(lines, "레시피 재료 표기", RecipeQuantity);
        AddLine(lines, "레시피 출처", RecipeSource);
        AddLine(lines, "공공 가격 참고", PriceReference);
        AddLine(lines, "레시피 원문", RecipeUrl);
        lines.Add(string.Empty);
        lines.Add("※ 공개 가격은 국가·지역·통화·단위·유통단계와 기준일이 서로 달라 실제 구매가나 계약 조건으로 확정되지 않습니다.");
        if (IsGroupImportReview)
        {
            lines.Add("※ 공동수입 여부는 실제 상품 출발국, 최종 배송국, 통관 상태와 HS 코드를 입력한 뒤 별도 거래경로 판정에서 결정합니다.");
        }
        lines.Add("참여자는 목표 수량, 포장 규격, 공급 조건과 수령 방법을 직접 확인하고 합의해 주세요.");
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddLine(List<string> lines, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            lines.Add($"{label}: {value}");
        }
    }

    private static void AddParameter(List<string> parameters, string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }

    private static string Normalize(string? value, int maxLength, string fallback = "")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = string.Join(' ', value
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }

    private static string NormalizeHttpUrl(string? value)
    {
        var normalized = Normalize(value, 1000);
        return Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.AbsoluteUri
            : string.Empty;
    }

    private static string NormalizeCountryCode(string? value)
    {
        var normalized = Normalize(value, 2).ToUpperInvariant();
        return normalized.Length == 2 && normalized.All(character => character is >= 'A' and <= 'Z')
            ? normalized
            : string.Empty;
    }
}

public static class CommunityIngredientSourcingModeCodes
{
    public const string Unspecified = "Unspecified";
    public const string DomesticGroupPurchase = "DomesticGroupPurchase";
    public const string GroupImportReview = "GroupImportReview";

    public static string Normalize(string? value)
        => value is DomesticGroupPurchase or GroupImportReview
            ? value
            : Unspecified;
}
