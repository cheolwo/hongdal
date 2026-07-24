using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Community;
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

    public static CommunityPostComposerSnapshot CreateCultureTransportDraft(
        OfficialFoodDishIngredientPurchaseSelection selection,
        OfficialFoodIngredientCompanyResearchResponse? companyResearch,
        OfficialFoodIngredientHsMappingResponse? hsMapping,
        DateTime savedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(selection.Dish);
        ArgumentNullException.ThrowIfNull(selection.Ingredient);

        var dish = selection.Dish;
        var ingredient = selection.Ingredient;
        var countryName = CountryName(dish.Dish.CountryCode);
        var dishWithConjunction = $"{dish.Dish.Name}{ConjunctionParticle(dish.Dish.Name)}";
        var lines = new List<string>
        {
            $"{countryName} 음식 {dish.Dish.Name}, 재료 {ingredient.CanonicalName}에 관해 이야기를 나누고 싶습니다.",
            string.Empty,
            "[공식·공개 자료에서 확인한 출발점]",
            $"음식: {dish.Dish.Name} ({dish.Dish.OriginalName})",
            $"지역·국가: {dish.Dish.RegionName} · {countryName}",
            $"선택 재료: {ingredient.CanonicalName} · {BuildRecipeIngredientText(ingredient)}",
            $"음식 자료 제공기관: {dish.Provider}",
            $"자료 확인 시각: {dish.LastCollectedAtUtc.ToUniversalTime():yyyy-MM-dd HH:mm} UTC"
        };

        var priceReference = BuildPriceReference(ingredient.PublicPrices ?? []);
        AddDraftLine(lines, "공공 가격 참고", priceReference);

        if (companyResearch is not null)
        {
            AddDraftLine(
                lines,
                "기업·상품 근거 원천",
                string.Join(" · ", companyResearch.Sources
                    .Select(source => source.Provider)
                    .Where(provider => !string.IsNullOrWhiteSpace(provider))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(3)));
            lines.Add($"관련 기업·상품 근거 후보: {companyResearch.Candidates.Count:N0}건 (거래 상대 추천·선정 결과 아님)");
        }

        if (hsMapping is not null)
        {
            var hsCandidates = hsMapping.Candidates
                .Take(3)
                .Select(candidate => $"{candidate.CountryCode} {candidate.StandardCode} {candidate.HsCode}")
                .ToArray();
            AddDraftLine(lines, "품목분류 참고 후보", string.Join(" · ", hsCandidates));
            lines.Add("품목분류 후보는 신고용 확정값이 아니며 실제 상품 형태와 거래 국가를 정한 뒤 전문 검토가 필요합니다.");
        }

        lines.AddRange(
        [
            string.Empty,
            "[함께 나누고 싶은 내용]",
            "- 이 음식은 현지에서 언제, 누구와, 어떤 방식으로 먹나요?",
            "- 원래 재료와 현지에서 구할 수 있는 대체 재료는 무엇인가요?",
            "- 구매하지 않고 정보만 나눠도 좋습니다. 구해 보고 싶다면 먼저 개별구매나 개별수입 조건을 확인할 수 있나요?",
            "- 최소수량·조건 확인·연락·통관·수령을 혼자 감당하기 부담스럽다면, 공동구매나 공동수입에서 어떤 역할을 함께 나누면 좋을까요?",
            string.Empty,
            "※ 공개 자료는 대화와 사전 검토를 위한 참고 정보입니다. 현재 판매 가능성, 거래 상대의 자격, 최종 가격, 계약 또는 수입 적격성을 보증하지 않습니다."
        ]);

        return new CommunityPostComposerSnapshot
        {
            SavedAtUtc = savedAtUtc,
            Category = CommunityBoardCatalog.Food.DisplayName,
            WorkflowTag = CultureTransportContentCatalog.FoodCultureWorkflowTag,
            RoleTag = "문화교통 참여자",
            Title = $"[문화교통][{countryName}] {dishWithConjunction} {ingredient.CanonicalName} 이야기",
            Body = string.Join(Environment.NewLine, lines),
            SharedLinkUrl = SafeHttpUrl(dish.OriginalUrl) ?? string.Empty,
            IsInterestGatheringEnabled = false,
            IsSalesPost = false
        };
    }

    public static CommunityPostComposerSnapshot CreateIndividualImportReviewDraft(
        OfficialFoodDishIngredientPurchaseSelection selection,
        OfficialFoodIngredientCompanyResearchResponse? companyResearch,
        OfficialFoodIngredientHsMappingResponse? hsMapping,
        DateTime savedAtUtc)
    {
        var cultureDraft = CreateCultureTransportDraft(
            selection,
            companyResearch,
            hsMapping,
            savedAtUtc);
        var countryName = CountryName(selection.Dish.Dish.CountryCode);
        var importQuestions = string.Join(Environment.NewLine,
        [
            string.Empty,
            "[개별수입 전에 확인하고 싶은 내용]",
            "- 실제 상품명·성분·포장 형태와 수량은 무엇인가요?",
            "- 출발 국가와 도착 국가에서 개인 반입이 허용되며 검역·신고·표시 의무가 있나요?",
            "- 배송비·세금·검사 비용과 반송 또는 폐기 위험을 누가 부담하나요?",
            "- 혼자 확인·통관·수령하기 어렵다면 누구와 어떤 책임을 나눌 수 있나요?",
            string.Empty,
            "※ 이 글은 개별수입 가능성을 함께 확인하기 위한 질문입니다. 주문, 구매 대행, 통관 신고 또는 계약을 요청하거나 자동 실행하지 않습니다."
        ]);

        return cultureDraft with
        {
            Category = CommunityBoardCatalog.InformationPrices.DisplayName,
            WorkflowTag = "개별수입 사전 확인",
            RoleTag = "정보 확인 참여자",
            Title = $"[개별수입 사전 확인] {countryName} {selection.Ingredient.CanonicalName}",
            Body = cultureDraft.Body + importQuestions
        };
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

    private static void AddDraftLine(List<string> lines, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            lines.Add($"{label}: {value}");
        }
    }

    private static string ConjunctionParticle(string value)
    {
        var lastCharacter = value.Trim().LastOrDefault();
        return lastCharacter is >= '\uAC00' and <= '\uD7A3'
               && (lastCharacter - '\uAC00') % 28 != 0
            ? "과"
            : "와";
    }
}
