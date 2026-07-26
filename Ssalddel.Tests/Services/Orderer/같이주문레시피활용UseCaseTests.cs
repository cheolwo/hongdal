using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.FoodCulture;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 같이주문레시피활용UseCaseTests
{
    [Fact]
    public async Task 개인수령량과_기존공식레시피활용사례를_같이주문판단자료로반환한다()
    {
        var useCase = new 같이주문레시피활용UseCase(new FakeIngredientIndex(
        [
            new OfficialFoodIngredientDto(
                "ingredient-potato",
                "ko",
                "감자",
                "감자",
                "vegetable",
                "채소",
                "Rule",
                1m,
                "Confirmed",
                2,
                DateTime.UtcNow,
                RelatedRecipes:
                [
                    new OfficialFoodIngredientRelatedRecipeDto(
                        "dish-potato-soup",
                        "recipe-potato-soup",
                        "mfds-cookrcp01",
                        "식품의약품안전처",
                        "KR",
                        "감자수프",
                        "감자수프",
                        "전국",
                        "국",
                        "주재료",
                        "감자",
                        "감자 300g",
                        "g",
                        "껍질을 벗겨 준비",
                        "https://example.test/recipe",
                        DateTime.UtcNow,
                        true)
                ])
        ]));

        var response = await useCase.조회Async(new 같이주문레시피활용조회요청
        {
            상품키 = "product-potato",
            상품명 = "감자",
            개인수령검토수량 = 2m,
            수량단위 = "kg"
        });

        Assert.Equal(같이주문레시피활용조회상태코드.일치자료있음, response.조회상태코드);
        Assert.Equal(2m, response.개인수령검토수량);
        Assert.False(response.정확한소진횟수계산가능);
        Assert.True(response.같이주문자동전환금지);
        Assert.True(response.같이주문별도동의필수);
        var recipe = Assert.Single(response.활용음식);
        Assert.Equal("감자수프", recipe.DishName);
        Assert.Equal("감자 300g", recipe.IngredientQuantityText);
    }

    [Fact]
    public async Task 식재료가일치하지않아도_자동전환없이_판단제한을반환한다()
    {
        var useCase = new 같이주문레시피활용UseCase(new FakeIngredientIndex([]));

        var response = await useCase.조회Async(new 같이주문레시피활용조회요청
        {
            상품키 = "product-x",
            상품명 = "미색인 식재료",
            개인수령검토수량 = 1m,
            수량단위 = "kg"
        });

        Assert.Equal(
            같이주문레시피활용조회상태코드.일치식재료없음,
            response.조회상태코드);
        Assert.Empty(response.활용음식);
        Assert.True(response.같이주문자동전환금지);
    }

    private sealed class FakeIngredientIndex(
        IReadOnlyList<OfficialFoodIngredientDto> response)
        : IOfficialFoodRecipeIngredientIndexService
    {
        public Task<IReadOnlyList<OfficialFoodIngredientDto>> SearchIngredientsAsync(
            OfficialFoodIngredientQuery query,
            CancellationToken cancellationToken = default)
            => Task.FromResult(response);

        public Task<IReadOnlyList<OfficialFoodIngredientCategoryDto>> GetCategoriesAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<OfficialFoodIngredientIndexResponse> RebuildAsync(
            OfficialFoodIngredientIndexRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<int> SynchronizeVariantAsync(
            Ssalddel.Domain.FoodCulture.OfficialFoodRecipeVariant variant,
            string languageCode,
            IReadOnlyList<string> ingredientTexts,
            DateTime indexedAtUtc,
            bool force,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
