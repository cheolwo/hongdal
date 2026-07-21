using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Ui.Common.Areas.App.Components.Information;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class OfficialFoodIngredientJourneyTests
{
    [Fact]
    public async Task 공개재료Client는_검색조건을인코딩하고_조회개수를제한한다()
    {
        Uri? requestedUri = null;
        var expected = Ingredient("두부");
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new[] { expected }),
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = new OfficialFoodIngredientDiscoveryClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.ssalddel.test/")
        });

        var result = await client.SearchAsync(new OfficialFoodIngredientQuery
        {
            SearchText = " 두부 요리 ",
            CategoryCode = OfficialFoodIngredientCategoryCodes.LegumeAndSoy,
            LanguageCode = "ko",
            Take = 500
        });

        Assert.Equal("두부", Assert.Single(result).CanonicalName);
        Assert.NotNull(requestedUri);
        Assert.Equal(
            "/api/v1/agricultural-fisheries/food-ingredients",
            requestedUri!.AbsolutePath);
        var query = Uri.UnescapeDataString(requestedUri.Query);
        Assert.Contains("take=50", query, StringComparison.Ordinal);
        Assert.Contains("searchText=두부 요리", query, StringComparison.Ordinal);
        Assert.Contains("categoryCode=legume-soy", query, StringComparison.Ordinal);
        Assert.Contains("languageCode=ko", query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 재료탐색ViewModel은_검색어와_대표레시피가격을함께보존한다()
    {
        var client = new FakeIngredientClient([Ingredient("두부")]);
        using var viewModel = new OfficialFoodIngredientJourneyViewModel(client)
        {
            SearchText = "두부"
        };

        var loaded = await viewModel.초기화Async();

        Assert.True(loaded);
        Assert.True(viewModel.HasIngredients);
        Assert.Equal("두부", client.LastQuery?.SearchText);
        var ingredient = Assert.Single(viewModel.Ingredients);
        Assert.Single(ingredient.PublicPrices ?? []);
        Assert.Single(ingredient.RelatedRecipes ?? []);
    }

    [Fact]
    public void 재료근거는_편집가능한공동구매초안과_딥링크를만든다()
    {
        var seed = CommunityGroupPurchaseIngredientSeed.Create(
            "ingredient:tofu",
            "두부\r\n",
            "두부전골",
            "https://foodsafety.example/recipe?id=10&lang=ko",
            "식품의약품안전처 · KR",
            "두부 200g",
            "한국 소매 4,000 KRW / kg, 2026.07.20, aT",
            "kg");

        Assert.NotNull(seed);
        Assert.Equal("두부 공동구매 제안", seed!.SuggestedTitle);
        Assert.Equal("official-ingredient:ingredient:tofu", seed.SuggestedProductKey);
        Assert.Contains("참고 레시피: 두부전골", seed.BuildSuggestedDescription(), StringComparison.Ordinal);
        Assert.Contains("실제 구매가나 계약 조건으로 확정되지 않습니다", seed.BuildSuggestedDescription(), StringComparison.Ordinal);
        var uri = Uri.UnescapeDataString(seed.ToNavigationUri());
        Assert.StartsWith("/community/group-purchase?", uri, StringComparison.Ordinal);
        Assert.Contains("ingredient=두부", uri, StringComparison.Ordinal);
        Assert.Contains("recipe=두부전골", uri, StringComparison.Ordinal);
        Assert.Contains("priceReference=한국 소매", uri, StringComparison.Ordinal);
    }

    [Fact]
    public void 재료근거는_비HTTP링크를버리고_기본구매단위를사용한다()
    {
        var seed = CommunityGroupPurchaseIngredientSeed.Create(
            "ingredient:onion",
            "양파",
            recipeUrl: "javascript:alert(1)",
            purchaseUnit: null);

        Assert.NotNull(seed);
        Assert.Empty(seed!.RecipeUrl);
        Assert.Equal("kg", seed.PurchaseUnit);
        Assert.DoesNotContain("recipeUrl=", seed.ToNavigationUri(), StringComparison.Ordinal);
    }

    [Fact]
    public void 표시책임은_가격과레시피를_개인정보없는공동구매Seed로조립한다()
    {
        var ingredient = Ingredient("두부");
        var recipe = Assert.Single(ingredient.RelatedRecipes ?? []);

        var seed = OfficialFoodIngredientPresentation.CreatePurchaseSeed(
            new OfficialFoodIngredientPurchaseSelection(ingredient, recipe));

        Assert.NotNull(seed);
        Assert.Equal("두부", seed!.IngredientName);
        Assert.Equal("건강한 두부전골", seed.RecipeTitle);
        Assert.Equal("kg", seed.PurchaseUnit);
        Assert.Contains("한국 소매", seed.PriceReference, StringComparison.Ordinal);
        Assert.Contains("식품의약품안전처 · KR", seed.RecipeSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ledger", seed.ToNavigationUri(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("https://foodsafety.example/recipe/1", true)]
    [InlineData("http://www.kamis.or.kr/price", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("/relative/recipe", false)]
    public void 원문링크표시는_HTTP계열만허용한다(string value, bool expected)
        => Assert.Equal(expected, OfficialFoodIngredientPresentation.SafeHttpUrl(value) is not null);

    [Fact]
    public void 공통Ui등록은_공개재료Client와여정ViewModel을포함한다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelUiCommonAppServices();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IOfficialFoodIngredientDiscoveryClient)
            && descriptor.ImplementationType == typeof(OfficialFoodIngredientDiscoveryClient)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(OfficialFoodIngredientJourneyViewModel)
            && descriptor.ImplementationType == typeof(OfficialFoodIngredientJourneyViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void 공식재료화면은_재료이름과_좁은폭동작영역을_실제값으로연결한다()
    {
        var componentDirectory = FindComponentDirectory();
        var card = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientCard.razor"));
        var journeyCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientJourney.razor.css"));
        var cardCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientCard.razor.css"));
        var recipeCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientRecipePanel.razor.css"));
        var searchCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientSearchPanel.razor.css"));

        Assert.Contains("IngredientName=\"@Ingredient.CanonicalName\"", card);
        Assert.Contains("@media (max-width: 900px)", journeyCss);
        Assert.Contains("@media (max-width: 640px)", cardCss);
        Assert.Contains("@media (max-width: 640px)", recipeCss);
        Assert.Contains("@media (max-width: 640px)", searchCss);
        Assert.Contains("min-height: 44px", journeyCss);
        Assert.Contains("min-height: 44px", cardCss);
        Assert.Contains("min-height: 44px", recipeCss);
        Assert.Contains("min-height: 44px", searchCss);
    }

    private static string FindComponentDirectory()
        => Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information");

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }

    private static OfficialFoodIngredientDto Ingredient(string name)
        => new(
            $"ingredient:{name}",
            "ko",
            name,
            name,
            OfficialFoodIngredientCategoryCodes.LegumeAndSoy,
            "콩·두류",
            "catalog",
            0.95m,
            OfficialFoodIngredientClassificationStates.AutoClassified,
            3,
            new DateTime(2026, 7, 21, 9, 0, 0, DateTimeKind.Utc),
            [
                new OfficialFoodIngredientPublicPriceDto(
                    "KR",
                    "한국",
                    OfficialFoodIngredientPublicPriceSourceKeys.Kamis,
                    "한국농수산식품유통공사",
                    OfficialFoodIngredientPriceMarketStages.Retail,
                    "소매",
                    name,
                    string.Empty,
                    4_000m,
                    3_500m,
                    4_500m,
                    "KRW",
                    "kg",
                    new DateOnly(2026, 7, 20),
                    "2026-07-20",
                    "전국",
                    "Daily",
                    10,
                    OfficialFoodIngredientPriceMappingStates.AutoMatched,
                    "이름 일치",
                    "https://www.kamis.or.kr",
                    new DateTime(2026, 7, 21, 8, 0, 0, DateTimeKind.Utc))
            ],
            [
                new OfficialFoodIngredientRelatedRecipeDto(
                    "dish:tofu-stew",
                    "recipe:tofu-stew:1",
                    OfficialFoodRecipeSourceKeys.MfdsCookRecipe,
                    "식품의약품안전처",
                    "KR",
                    "두부전골",
                    "건강한 두부전골",
                    "전국",
                    "국",
                    "주재료",
                    "두부",
                    "200g",
                    "g",
                    "한입 크기",
                    "https://foodsafety.example/recipe/1",
                    new DateTime(2026, 7, 21, 8, 0, 0, DateTimeKind.Utc),
                    true)
            ]);

    private sealed class FakeIngredientClient(
        IReadOnlyList<OfficialFoodIngredientDto> ingredients)
        : IOfficialFoodIngredientDiscoveryClient
    {
        public OfficialFoodIngredientQuery? LastQuery { get; private set; }

        public Task<IReadOnlyList<OfficialFoodIngredientDto>> SearchAsync(
            OfficialFoodIngredientQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(ingredients);
        }
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
