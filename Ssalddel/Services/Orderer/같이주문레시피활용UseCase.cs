using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Services.Orderer;

public interface I같이주문레시피활용UseCase
{
    Task<같이주문레시피활용응답> 조회Async(
        같이주문레시피활용조회요청 request,
        CancellationToken cancellationToken = default);
}

[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[SsalddelUseCase(
    "같이 주문 식재료 활용 판단",
    Summary = "개별 주문과 같이 주문을 비교하는 주문자에게 기존 공식 레시피 DB의 식재료 활용 사례를 읽기 전용으로 제공합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Orderer)]
public sealed class 같이주문레시피활용UseCase(
    IOfficialFoodRecipeIngredientIndexService ingredientIndex) : I같이주문레시피활용UseCase
{
    public async Task<같이주문레시피활용응답> 조회Async(
        같이주문레시피활용조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);

        var ingredients = await ingredientIndex.SearchIngredientsAsync(
            new OfficialFoodIngredientQuery
            {
                SearchText = request.상품명.Trim(),
                Take = 20
            },
            cancellationToken);
        var selected = SelectIngredient(ingredients, request);
        if (selected is null)
        {
            return BaseResponse(
                request,
                같이주문레시피활용조회상태코드.일치식재료없음,
                "현재 공식 레시피 색인에서 이 상품과 일치하는 식재료를 찾지 못했습니다. 개별 주문과 같이 주문의 비용·대기시간을 기준으로 판단하세요.");
        }

        var recipes = (selected.RelatedRecipes ?? [])
            .Where(x => x.IsFreshForPublication)
            .Take(Math.Clamp(request.최대레시피수, 1, 6))
            .Select(x => new 같이주문레시피활용항목응답
            {
                DishKey = x.DishKey,
                DishName = x.DishName,
                RecipeTitle = x.RecipeTitle,
                CountryCode = x.CountryCode,
                RegionName = x.RegionName,
                Category = x.Category,
                IngredientQuantityText = x.QuantityText,
                IngredientUnitText = x.UnitText,
                PreparationNote = x.PreparationNote,
                SourceProvider = x.Provider,
                SourceUrl = x.OriginalUrl,
                SourceUpdatedAtUtc = x.LastCollectedAtUtc,
                IsFreshForPublication = x.IsFreshForPublication
            })
            .ToArray();

        var response = BaseResponse(
            request,
            recipes.Length == 0
                ? 같이주문레시피활용조회상태코드.활용레시피없음
                : 같이주문레시피활용조회상태코드.일치자료있음,
            recipes.Length == 0
                ? $"{selected.CanonicalName} 식재료는 확인했지만 현재 게시 가능한 활용 레시피가 없습니다."
                : $"{selected.CanonicalName}을 활용하는 공식 레시피 {recipes.Length}개를 찾았습니다. 개인이 실제 수령할 양과 조리 빈도를 함께 확인하세요.");
        response.일치식재료키 = selected.IngredientKey;
        response.일치식재료명 = selected.CanonicalName;
        response.활용음식 = recipes;
        response.정확한소진횟수계산가능 = false;
        response.수량판단제한 =
            "원천마다 재료 단위와 1회 제공량 표기가 달라 정확한 소진 횟수는 계산하지 않습니다. 표시된 재료량과 원문을 확인하세요.";
        response.판단도움말 =
        [
            $"개인 수령 검토량: {request.개인수령검토수량:0.####} {request.수량단위.Trim()}",
            "같이 주문 전체 모집 수량이 아니라 내가 실제로 수령할 몫을 기준으로 활용 가능성을 판단합니다.",
            "레시피는 활용 아이디어이며 보관기한·알레르기·가구 소비량을 대신 판단하지 않습니다.",
            "레시피를 보았더라도 같이 주문 참여에는 별도 동의가 필요하고 자동 가입·결제·계약은 실행되지 않습니다."
        ];
        return response;
    }

    private static OfficialFoodIngredientDto? SelectIngredient(
        IReadOnlyList<OfficialFoodIngredientDto> ingredients,
        같이주문레시피활용조회요청 request)
    {
        if (!string.IsNullOrWhiteSpace(request.식재료키))
        {
            var ingredientKey = request.식재료키.Trim();
            var byKey = ingredients.FirstOrDefault(x =>
                string.Equals(x.IngredientKey, ingredientKey, StringComparison.Ordinal));
            if (byKey is not null)
            {
                return byKey;
            }
        }

        var normalizedProductName = Normalize(request.상품명);
        return ingredients.FirstOrDefault(x =>
                   string.Equals(
                       Normalize(x.CanonicalName),
                       normalizedProductName,
                       StringComparison.OrdinalIgnoreCase))
               ?? ingredients.FirstOrDefault();
    }

    private static 같이주문레시피활용응답 BaseResponse(
        같이주문레시피활용조회요청 request,
        string statusCode,
        string message)
        => new()
        {
            상품키 = request.상품키.Trim(),
            상품명 = request.상품명.Trim(),
            개인수령검토수량 = request.개인수령검토수량,
            수량단위 = request.수량단위.Trim(),
            조회상태코드 = statusCode,
            안내 = message,
            같이주문자동전환금지 = true,
            같이주문별도동의필수 = true,
            정확한소진횟수계산가능 = false
        };

    private static void Validate(같이주문레시피활용조회요청 request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.상품키);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.상품명);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.수량단위);
        if (request.개인수령검토수량 <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "개인 수령 검토 수량은 0보다 커야 합니다.");
        }

        if (request.최대레시피수 is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "레시피 수는 1~6 범위여야 합니다.");
        }
    }

    private static string Normalize(string value)
        => string.Concat(value.Where(character => !char.IsWhiteSpace(character)))
            .ToLowerInvariant();
}
