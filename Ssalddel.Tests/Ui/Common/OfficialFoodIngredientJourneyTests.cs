using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Community;
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
    public async Task 공개음식Client는_국가별목록과_구조화재료상세를조회한다()
    {
        var detail = DishDetail();
        var requestedUris = new List<Uri>();
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUris.Add(request.RequestUri!);
            var value = request.RequestUri!.AbsolutePath.EndsWith("/food-dishes", StringComparison.Ordinal)
                ? JsonSerializer.Serialize(new[] { detail.Dish })
                : JsonSerializer.Serialize(detail);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(value, Encoding.UTF8, "application/json")
            };
        });
        var client = new OfficialFoodIngredientDiscoveryClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.ssalddel.test/")
        });

        var dishes = await client.SearchDishesAsync(new OfficialFoodDishDiscoveryQuery
        {
            CountryCode = "JP",
            SearchText = "rice",
            Take = 100
        });
        var selected = await client.GetDishAsync(detail.Dish.DishKey);

        Assert.Single(dishes);
        Assert.NotNull(selected);
        Assert.Equal("양파", Assert.Single(selected!.Ingredients).CanonicalName);
        Assert.Equal("/api/v1/agricultural-fisheries/food-dishes", requestedUris[0].AbsolutePath);
        var listQuery = Uri.UnescapeDataString(requestedUris[0].Query);
        Assert.Contains("countryCode=JP", listQuery, StringComparison.Ordinal);
        Assert.Contains("searchText=rice", listQuery, StringComparison.Ordinal);
        Assert.Contains("take=50", listQuery, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 공개재료Client는_재료관련기업근거를_인코딩해조회한다()
    {
        Uri? requestedUri = null;
        var expected = CompanyResearchResponse();
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(expected),
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = new OfficialFoodIngredientDiscoveryClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.ssalddel.test/")
        });

        var result = await client.SearchCompaniesAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientKey = " ingredient:onion ",
            IngredientName = " 양파 ",
            Take = 100
        });

        Assert.Single(result.Candidates);
        Assert.NotNull(requestedUri);
        Assert.Equal(
            "/api/v1/agricultural-fisheries/food-ingredients/companies",
            requestedUri!.AbsolutePath);
        var query = Uri.UnescapeDataString(requestedUri.Query);
        Assert.Contains("ingredientKey=ingredient:onion", query, StringComparison.Ordinal);
        Assert.Contains("ingredientName=양파", query, StringComparison.Ordinal);
        Assert.Contains("take=20", query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 공개재료Client는_재료HS후보조건을_인코딩해조회한다()
    {
        Uri? requestedUri = null;
        var expected = HsMappingResponse();
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(expected),
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = new OfficialFoodIngredientDiscoveryClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.ssalddel.test/")
        });

        var result = await client.GetHsCodesAsync(new OfficialFoodIngredientHsQuery
        {
            IngredientKey = " ingredient:onion ",
            IngredientName = " 양파 ",
            CountryCode = " US ",
            Refresh = true
        });

        Assert.Single(result.Candidates);
        Assert.Equal(
            "/api/v1/agricultural-fisheries/food-ingredients/hs-codes",
            requestedUri!.AbsolutePath);
        var query = Uri.UnescapeDataString(requestedUri.Query);
        Assert.Contains("ingredientKey=ingredient:onion", query, StringComparison.Ordinal);
        Assert.Contains("ingredientName=양파", query, StringComparison.Ordinal);
        Assert.Contains("countryCode=US", query, StringComparison.Ordinal);
        Assert.Contains("refresh=true", query, StringComparison.Ordinal);
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
    public async Task 음식탐색ViewModel은_국가음식재료선택을_한페이지상태로보존한다()
    {
        var detail = DishDetail();
        var client = new FakeIngredientClient(
            [Ingredient("두부")],
            [detail.Dish],
            detail);
        using var viewModel = new OfficialFoodIngredientJourneyViewModel(client);

        var loaded = await viewModel.초기화Async();
        var countryChanged = await viewModel.SelectCountryAsync("JP");

        Assert.True(loaded);
        Assert.True(countryChanged);
        Assert.True(viewModel.HasDishes);
        Assert.Equal("JP", viewModel.SelectedCountryCode);
        Assert.Equal("JP", client.LastDishQuery?.CountryCode);
        Assert.Equal(detail.Dish.DishKey, viewModel.SelectedDish?.Dish.DishKey);
        Assert.Equal("양파", viewModel.SelectedDishIngredient?.CanonicalName);
    }

    [Fact]
    public async Task 음식탐색ViewModel은_사용자요청시에만_선택재료기업근거를조회한다()
    {
        var detail = DishDetail();
        var client = new FakeIngredientClient(
            [Ingredient("두부")],
            [detail.Dish],
            detail,
            CompanyResearchResponse());
        using var viewModel = new OfficialFoodIngredientJourneyViewModel(client);

        await viewModel.초기화Async();
        Assert.Null(viewModel.CompanyResearch);

        var researched = await viewModel.ResearchSelectedIngredientCompaniesAsync();

        Assert.True(researched);
        Assert.NotNull(viewModel.CompanyResearch);
        Assert.Equal("ingredient:onion", client.LastCompanyQuery?.IngredientKey);
        Assert.Equal("양파", client.LastCompanyQuery?.IngredientName);
    }

    [Fact]
    public async Task 음식탐색ViewModel은_사용자요청시에만_선택재료HS후보를조회한다()
    {
        var detail = DishDetail();
        var client = new FakeIngredientClient(
            [Ingredient("두부")],
            [detail.Dish],
            detail,
            hsMapping: HsMappingResponse());
        using var viewModel = new OfficialFoodIngredientJourneyViewModel(client);

        await viewModel.초기화Async();
        Assert.Null(viewModel.HsMapping);

        var loaded = await viewModel.LoadSelectedIngredientHsCodesAsync();

        Assert.True(loaded);
        Assert.NotNull(viewModel.HsMapping);
        Assert.Equal("ingredient:onion", client.LastHsQuery?.IngredientKey);
        Assert.Equal("양파", client.LastHsQuery?.IngredientName);
        Assert.False(Assert.Single(viewModel.HsMapping!.Candidates).IsDeclarationReady);
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
        Assert.StartsWith($"{CommunityPageRoutes.GroupPurchaseCreate}?", uri, StringComparison.Ordinal);
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
    public void 여러재료묶음은_비구속수요route에서_중복없이복원된다()
    {
        var onion = CommunityGroupPurchaseIngredientSeed.Create("ingredient:onion", "양파", purchaseUnit: "kg")!;
        var chili = CommunityGroupPurchaseIngredientSeed.Create("ingredient:chili", "고추", purchaseUnit: "box")!;

        var uri = CommunityGroupPurchaseIngredientSeed.ToDemandNavigationUri([onion, chili, onion]);
        var query = new Uri($"https://example.test{uri}").Query;
        var encoded = query.Split('=', 2)[1];
        var restored = CommunityGroupPurchaseIngredientSeed.DecodeMaterialBundle(encoded);

        Assert.StartsWith(CommunityPageRoutes.GroupPurchaseDemand, uri, StringComparison.Ordinal);
        Assert.Equal(2, restored.Count);
        Assert.Collection(
            restored,
            item => Assert.Equal("ingredient:onion", item.IngredientKey),
            item =>
            {
                Assert.Equal("ingredient:chili", item.IngredientKey);
                Assert.Equal("box", item.PurchaseUnit);
            });
    }

    [Fact]
    public void 여러재료묶음은_허용길이를넘거나_손상되면복원하지않는다()
    {
        var oversized = new string('a', CommunityGroupPurchaseIngredientSeed.MaxEncodedBundleLength + 1);

        Assert.Empty(CommunityGroupPurchaseIngredientSeed.DecodeMaterialBundle(oversized));
        Assert.Empty(CommunityGroupPurchaseIngredientSeed.DecodeMaterialBundle("not-a-valid-bundle"));
    }

    [Fact]
    public void 음식재료의_공동수입선택은_문화국가와상품출발국을분리한초안을만든다()
    {
        var detail = DishDetail();
        var selection = new OfficialFoodDishIngredientPurchaseSelection(
            detail,
            Assert.Single(detail.Ingredients),
            CommunityIngredientSourcingModeCodes.GroupImportReview);

        var seed = OfficialFoodIngredientPresentation.CreatePurchaseSeed(selection);

        Assert.NotNull(seed);
        Assert.True(seed!.IsGroupImportReview);
        Assert.Equal("양파 공동수입 검토 제안", seed.SuggestedTitle);
        Assert.Equal("JP", seed.FoodCountryCode);
        Assert.Contains("상품 원산지·출발국으로 자동 사용하지 않음", seed.BuildSuggestedDescription(), StringComparison.Ordinal);
        Assert.Contains("실제 상품 출발국", seed.BuildSuggestedDescription(), StringComparison.Ordinal);
        var uri = Uri.UnescapeDataString(seed.ToNavigationUri());
        Assert.Contains("foodCountry=JP", uri, StringComparison.Ordinal);
        Assert.Contains("sourcingMode=GroupImportReview", uri, StringComparison.Ordinal);
    }

    [Fact]
    public void 문화교통초안은_국가기관근거와질문을담고_판매나거래를자동활성화하지않는다()
    {
        var detail = DishDetail();
        var selection = new OfficialFoodDishIngredientPurchaseSelection(
            detail,
            Assert.Single(detail.Ingredients),
            CommunityIngredientSourcingModeCodes.Unspecified);

        var draft = OfficialFoodIngredientPresentation.CreateCultureTransportDraft(
            selection,
            CompanyResearchResponse(),
            HsMappingResponse(),
            new DateTime(2026, 7, 23, 4, 30, 0, DateTimeKind.Utc));

        Assert.Equal(CommunityBoardCatalog.Food.DisplayName, draft.Category);
        Assert.Equal(CultureTransportContentCatalog.FoodCultureWorkflowTag, draft.WorkflowTag);
        Assert.Equal("문화교통 참여자", draft.RoleTag);
        Assert.Equal("[문화교통][일본] 양파밥과 양파 이야기", draft.Title);
        Assert.Contains("일본 농림수산성", draft.Body, StringComparison.Ordinal);
        Assert.Contains("식품의약품안전처", draft.Body, StringComparison.Ordinal);
        Assert.Contains("US HTSUS 0703.10.2000", draft.Body, StringComparison.Ordinal);
        Assert.Contains("현지에서 언제, 누구와, 어떤 방식으로", draft.Body, StringComparison.Ordinal);
        Assert.Contains("구매하지 않고 정보만 나눠도 좋습니다", draft.Body, StringComparison.Ordinal);
        Assert.Contains("먼저 개별구매나 개별수입 조건", draft.Body, StringComparison.Ordinal);
        Assert.Contains("혼자 감당하기 부담스럽다면", draft.Body, StringComparison.Ordinal);
        Assert.Contains("어떤 역할을 함께 나누면 좋을까요", draft.Body, StringComparison.Ordinal);
        Assert.Equal(detail.OriginalUrl, draft.SharedLinkUrl);
        Assert.False(draft.IsSalesPost);
        Assert.False(draft.IsInterestGatheringEnabled);
        Assert.Empty(draft.커뮤니티원장Id);
    }

    [Fact]
    public void 개별수입질문초안은_정보확인으로남고_주문이나거래를활성화하지않는다()
    {
        var detail = DishDetail();
        var selection = new OfficialFoodDishIngredientPurchaseSelection(
            detail,
            Assert.Single(detail.Ingredients),
            CommunityIngredientSourcingModeCodes.Unspecified);

        var draft = OfficialFoodIngredientPresentation.CreateIndividualImportReviewDraft(
            selection,
            CompanyResearchResponse(),
            HsMappingResponse(),
            new DateTime(2026, 7, 23, 5, 0, 0, DateTimeKind.Utc));

        Assert.Equal(CommunityBoardCatalog.InformationPrices.DisplayName, draft.Category);
        Assert.Equal("개별수입 사전 확인", draft.WorkflowTag);
        Assert.Equal("정보 확인 참여자", draft.RoleTag);
        Assert.Equal("[개별수입 사전 확인] 일본 양파", draft.Title);
        Assert.Contains("개인 반입이 허용", draft.Body, StringComparison.Ordinal);
        Assert.Contains("검역·신고·표시 의무", draft.Body, StringComparison.Ordinal);
        Assert.Contains("누구와 어떤 책임을 나눌 수 있나요", draft.Body, StringComparison.Ordinal);
        Assert.Contains("주문, 구매 대행, 통관 신고 또는 계약을 요청하거나 자동 실행하지 않습니다", draft.Body, StringComparison.Ordinal);
        Assert.False(draft.IsSalesPost);
        Assert.False(draft.IsInterestGatheringEnabled);
        Assert.Empty(draft.커뮤니티원장Id);
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
        var dishBrowser = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodDishBrowsePanel.razor"));
        var dishBrowserCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodDishBrowsePanel.razor.css"));
        var exchangePanel = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodCultureTransportPanel.razor"));
        var exchangePanelCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodCultureTransportPanel.razor.css"));
        var companyPanel = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientCompanyPanel.razor"));
        var companyPanelCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientCompanyPanel.razor.css"));
        var hsPanel = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientHsPanel.razor"));
        var hsPanelCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientHsPanel.razor.css"));
        var recipeCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientRecipePanel.razor.css"));
        var searchCss = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientSearchPanel.razor.css"));

        Assert.Contains("IngredientName=\"@Ingredient.CanonicalName\"", card);
        Assert.Contains("혼자 구입하기 부담스럽다면", card);
        Assert.Contains("같이 구매할 이웃 알아보기", card);
        Assert.Contains("혼자 준비하기 부담스럽다면", File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientRecipePanel.razor")));
        Assert.Contains("이 레시피로 같이 구매할 이웃 알아보기", File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientRecipePanel.razor")));
        Assert.Contains("@media (max-width: 900px)", journeyCss);
        Assert.Contains("@media (max-width: 640px)", cardCss);
        Assert.Contains("@media (max-width: 640px)", recipeCss);
        Assert.Contains("@media (max-width: 640px)", searchCss);
        Assert.Contains("min-height: 44px", journeyCss);
        Assert.Contains("min-height: 44px", cardCss);
        Assert.Contains("min-height: 44px", recipeCss);
        Assert.Contains("min-height: 44px", searchCss);
        Assert.Contains("음식 국가 ≠ 상품 출발국", dishBrowser);
        Assert.Contains("문화교통 · 음식에서 이동 준비까지", dishBrowser);
        Assert.Contains("OfficialFoodCultureTransportPanel", dishBrowser);
        Assert.Contains("문화교통 0.0 → 1.0 → 1.5", exchangePanel);
        Assert.Contains("공식 근거로 글 초안 만들기", exchangePanel);
        Assert.Contains("<details class=\"culture-transport__purchase-options\">", exchangePanel);
        Assert.DoesNotContain("<details class=\"culture-transport__purchase-options\" open", exchangePanel);
        Assert.Contains("구매·수입 방법 살펴보기", exchangePanel);
        Assert.Contains("먼저 혼자 알아보기", exchangePanel);
        Assert.Contains("혼자 거래하기 부담스러울 때", exchangePanel);
        Assert.Contains("같이 조건을 확인하고 역할·비용·위험 나누기", exchangePanel);
        Assert.Contains("문화교통 0.0 · 개별구매 참고", exchangePanel);
        Assert.Contains("문화교통 1.5 · 개별수입 준비", exchangePanel);
        Assert.Contains("문화교통 1.0 · 공동구매", exchangePanel);
        Assert.Contains("문화교통 1.5 · 공동수입 준비", exchangePanel);
        Assert.True(exchangePanel.IndexOf("문화교통 1.5 · 개별수입 준비", StringComparison.Ordinal)
                    < exchangePanel.IndexOf("혼자 거래하기 부담스러울 때", StringComparison.Ordinal));
        Assert.True(exchangePanel.IndexOf("혼자 거래하기 부담스러울 때", StringComparison.Ordinal)
                    < exchangePanel.IndexOf("문화교통 1.0 · 공동구매", StringComparison.Ordinal));
        Assert.Contains("같이 구매할 이웃 알아보기", exchangePanel);
        Assert.Contains("개별수입 가능성 질문 초안", exchangePanel);
        Assert.Contains("같이 수입할 이웃 알아보기", exchangePanel);
        Assert.Contains("아무것도 선택하지 않아도 됩니다", exchangePanel);
        Assert.Contains("CommunityBoardCatalog.Food", exchangePanel);
        Assert.Contains("CommunityBoardCatalog.SalesSupply", exchangePanel);
        Assert.Contains("CommunityBoardCatalog.InformationPrices", dishBrowser);
        Assert.Contains("@media (max-width: 640px)", exchangePanelCss);
        Assert.Contains("min-height: 44px", exchangePanelCss);
        Assert.Contains("OfficialFoodIngredientCompanyPanel", dishBrowser);
        Assert.Contains("OfficialFoodIngredientHsPanel", dishBrowser);
        Assert.Contains("관련 국내외 기업 조사", companyPanel);
        Assert.Contains("자동 조회하지 않습니다", companyPanel);
        Assert.Contains("자동 추천·선정·초대", companyPanel);
        Assert.Contains("@media (max-width: 640px)", dishBrowserCss);
        Assert.Contains("min-height: 44px", dishBrowserCss);
        Assert.Contains("grid-template-columns: minmax(0, 1fr)", dishBrowserCss);
        Assert.Contains("@media (max-width: 640px)", companyPanelCss);
        Assert.Contains("min-height: 44px", companyPanelCss);
        Assert.Contains("신고용 확정값 아님", hsPanel);
        Assert.Contains("한국 수출 HSK", hsPanel);
        Assert.Contains("미국 수입 HTS", hsPanel);
        Assert.Contains("@media (max-width: 640px)", hsPanelCss);
        Assert.Contains("min-height: 44px", hsPanelCss);
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

    private static OfficialFoodDishDetailDto DishDetail()
    {
        var collectedAtUtc = new DateTime(2026, 7, 21, 8, 0, 0, DateTimeKind.Utc);
        var dish = new OfficialFoodRecipeDishDto(
            "dish:jp-onion-rice",
            "JP",
            "교토",
            "양파밥",
            "玉ねぎご飯",
            "Onion rice",
            "밥",
            "양파를 넣어 함께 짓는 지역 음식",
            OfficialFoodRecipeRepresentationStates.Candidate,
            OfficialFoodRecipeReviewStates.PendingReview,
            1,
            collectedAtUtc);
        var ingredient = new OfficialFoodRecipeIngredientDto(
            "ingredient:onion",
            "양파",
            OfficialFoodIngredientCategoryCodes.Vegetable,
            "채소",
            "주재료",
            "양파 1개",
            "양파",
            "1개",
            1,
            null,
            "count",
            "개",
            string.Empty,
            "잘게 썰기",
            1,
            "test-parser",
            0.98m,
            false,
            [
                new OfficialFoodIngredientPublicPriceDto(
                    "KR",
                    "한국",
                    OfficialFoodIngredientPublicPriceSourceKeys.Kamis,
                    "한국농수산식품유통공사",
                    OfficialFoodIngredientPriceMarketStages.Retail,
                    "소매",
                    "양파",
                    string.Empty,
                    2_000m,
                    1_800m,
                    2_200m,
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
                    collectedAtUtc)
            ]);
        return new OfficialFoodDishDetailDto(
            dish,
            "recipe:jp-onion-rice:1",
            OfficialFoodRecipeSourceKeys.MaffRegionalCuisine,
            "일본 농림수산성",
            "양파밥",
            "2인분",
            "https://www.maff.go.jp/example/onion-rice",
            "출처: 일본 농림수산성",
            collectedAtUtc,
            true,
            [ingredient]);
    }

    private static OfficialFoodIngredientCompanyResearchResponse CompanyResearchResponse()
        => new(
            OfficialFoodIngredientCompanyResearchStatusCodes.Available,
            "ingredient:onion",
            "양파",
            new DateTimeOffset(2026, 7, 22, 3, 0, 0, TimeSpan.Zero),
            [
                new OfficialFoodIngredientCompanySourceDto(
                    "mfds-domestic-product-ingredient-report",
                    "식품의약품안전처",
                    "품목제조보고",
                    "대한민국",
                    "https://foodsafetykorea.go.kr",
                    OfficialFoodIngredientCompanySourceStatusCodes.Available,
                    "조회 완료",
                    true,
                    false,
                    true)
            ],
            [
                new OfficialFoodIngredientCompanyCandidateDto(
                    "organization-candidate:1",
                    "양파식품",
                    "KR",
                    "대한민국",
                    OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer,
                    OfficialFoodIngredientCompanyEvidenceCodes.DomesticProductIngredientReport,
                    "양파 원재료 제품 이력",
                    "양파 소스",
                    "소스",
                    "20010000001",
                    OfficialFoodIngredientCompanyVerificationStatusCodes.OfficialProductReport,
                    false,
                    string.Empty,
                    "mfds-domestic-product-ingredient-report",
                    "품목제조보고",
                    "https://foodsafetykorea.go.kr",
                    new DateTimeOffset(2026, 7, 22, 3, 0, 0, TimeSpan.Zero),
                    true,
                    false,
                    false)
            ],
            ["자동 선정하지 않습니다."]);

    private static OfficialFoodIngredientHsMappingResponse HsMappingResponse()
        => new(
            "ingredient:onion",
            "양파",
            null,
            true,
            new DateTime(2026, 7, 22, 4, 0, 0, DateTimeKind.Utc),
            [
                new OfficialFoodIngredientHsCandidateDto(
                    1,
                    10,
                    "US",
                    OfficialFoodIngredientHsJurisdictionUseCodes.UnitedStatesImportEntry,
                    "HTSUS",
                    "2026-r11",
                    10,
                    "0703.10.2000",
                    "0703102000",
                    10,
                    "양파",
                    "Onions",
                    "Fresh or chilled onions",
                    "CuratedHsFamilySearch",
                    OfficialFoodIngredientHsMatchQualityCodes.CuratedHsFamilyCandidate,
                    0.64m,
                    OfficialFoodIngredientHsMappingStates.Candidate,
                    "양파 재료군 후보",
                    "신선·건조 여부를 확인해야 합니다.",
                    ["신선·냉장·건조·분말 여부"],
                    "USITC HTS",
                    "https://hts.usitc.gov/",
                    new DateTime(2026, 1, 1),
                    null,
                    new DateTime(2026, 7, 1),
                    new DateTime(2026, 7, 22, 4, 0, 0, DateTimeKind.Utc),
                    true,
                    false)
            ],
            ["신고용 확정값이 아닙니다."]);

    private sealed class FakeIngredientClient(
        IReadOnlyList<OfficialFoodIngredientDto> ingredients,
        IReadOnlyList<OfficialFoodRecipeDishDto>? dishes = null,
        OfficialFoodDishDetailDto? dishDetail = null,
        OfficialFoodIngredientCompanyResearchResponse? companyResearch = null,
        OfficialFoodIngredientHsMappingResponse? hsMapping = null)
        : IOfficialFoodIngredientDiscoveryClient
    {
        public OfficialFoodIngredientQuery? LastQuery { get; private set; }

        public OfficialFoodDishDiscoveryQuery? LastDishQuery { get; private set; }

        public OfficialFoodIngredientCompanyQuery? LastCompanyQuery { get; private set; }

        public OfficialFoodIngredientHsQuery? LastHsQuery { get; private set; }

        public Task<IReadOnlyList<OfficialFoodRecipeDishDto>> SearchDishesAsync(
            OfficialFoodDishDiscoveryQuery query,
            CancellationToken cancellationToken = default)
        {
            LastDishQuery = query;
            return Task.FromResult(dishes ?? []);
        }

        public Task<OfficialFoodDishDetailDto?> GetDishAsync(
            string dishKey,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                dishDetail?.Dish.DishKey == dishKey ? dishDetail : null);

        public Task<OfficialFoodIngredientCompanyResearchResponse> SearchCompaniesAsync(
            OfficialFoodIngredientCompanyQuery query,
            CancellationToken cancellationToken = default)
        {
            LastCompanyQuery = query;
            return Task.FromResult(companyResearch ?? CompanyResearchResponse());
        }

        public Task<OfficialFoodIngredientHsMappingResponse> GetHsCodesAsync(
            OfficialFoodIngredientHsQuery query,
            CancellationToken cancellationToken = default)
        {
            LastHsQuery = query;
            return Task.FromResult(hsMapping ?? HsMappingResponse());
        }

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
