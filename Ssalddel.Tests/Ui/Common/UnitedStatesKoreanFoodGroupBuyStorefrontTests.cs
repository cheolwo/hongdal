using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class UnitedStatesKoreanFoodGroupBuyStorefrontTests
{
    [Fact]
    public async Task 초기화는_한국음식과첫재료를_한페이지상태로연결한다()
    {
        var detail = DishDetail();
        var foodClient = new FakeFoodClient(detail);
        using var viewModel = CreateViewModel(foodClient, new FakeGroupPurchaseService());

        var loaded = await viewModel.초기화Async();

        Assert.True(loaded);
        Assert.Equal("KR", foodClient.LastDishQuery?.CountryCode);
        Assert.Equal(detail.Dish.DishKey, viewModel.SelectedDish?.Dish.DishKey);
        Assert.Equal("ingredient:onion", viewModel.SelectedIngredient?.IngredientKey);
        Assert.Null(viewModel.HsMapping);
    }

    [Fact]
    public async Task 배치미리보기는_명시적으로선택한관세참고값과ZIP을_서버집단화에전달한다()
    {
        var detail = DishDetail();
        var foodClient = new FakeFoodClient(detail);
        var groupService = new FakeGroupPurchaseService
        {
            PreviewResponse = new 공동구매자동집단배치미리보기응답
            {
                배치유형 = 공동구매자동집단배치유형코드.기존집단,
                예상진행 = new 공동구매자동집단진행응답
                {
                    참여자수 = 4,
                    총희망수량 = 25m,
                    추가필요참여자수 = 1,
                    추가필요수량 = 5m
                }
            }
        };
        using var viewModel = CreateViewModel(foodClient, groupService);
        await viewModel.초기화Async();
        await viewModel.LoadHsReferencesAsync();
        viewModel.SelectHsReference(81);
        viewModel.UsZipCode = "10001";
        viewModel.DesiredQuantity = 4.5m;
        viewModel.TemperatureCode = "냉장";

        var previewed = await viewModel.PreviewPlacementAsync();

        Assert.True(previewed);
        Assert.NotNull(viewModel.PlacementPreview);
        Assert.Null(groupService.LastRegisteredDemand);
        var request = Assert.IsType<공동구매자동수요등록Command>(groupService.LastPreviewDemand);
        Assert.Equal("official-ingredient:ingredient:onion", request.상품키);
        Assert.Equal("0703.10.2000", request.HS코드);
        Assert.Equal("us-zcta:10001", request.배송권키);
        Assert.Equal("냉장", request.온도코드);
        Assert.Equal(공동구매자동수요물류방식코드.후속검토, request.물류방식);
        Assert.Equal(4.5m, request.희망수량);
        Assert.Equal("kg", request.수량단위);
        Assert.Equal(공동구매자동수요유형코드.관심표시, request.수요유형);
        Assert.Equal(공동구매자동결제상태코드.미결제, request.결제상태);
        Assert.DoesNotContain("account-42", request.수요출처키, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 수요참여는_결제없이같은가명출처키로_관심만등록한다()
    {
        var detail = DishDetail();
        var groupService = new FakeGroupPurchaseService
        {
            RegisterResponse = new 공동구매자동집단응답
            {
                자동집단Id = "group-us-10001",
                상품키 = "official-ingredient:ingredient:onion",
                참여자수 = 2,
                총희망수량 = 7m,
                수량단위 = "kg"
            }
        };
        using var viewModel = CreateViewModel(new FakeFoodClient(detail), groupService);
        await viewModel.초기화Async();
        await viewModel.LoadHsReferencesAsync();
        viewModel.SelectHsReference(81);
        viewModel.UsZipCode = "10001";

        var joined = await viewModel.JoinDemandPoolAsync();

        Assert.True(joined);
        Assert.Equal("group-us-10001", viewModel.RegisteredGroup?.자동집단Id);
        var request = Assert.IsType<공동구매자동수요등록Command>(groupService.LastRegisteredDemand);
        Assert.Equal(공동구매자동수요유형코드.관심표시, request.수요유형);
        Assert.Equal(공동구매자동결제상태코드.미결제, request.결제상태);
        Assert.Null(request.예약결제금액);
        Assert.Contains("reference-only", request.메모, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 등록한비구속수요는_같은출처키로철회할수있다()
    {
        var groupService = new FakeGroupPurchaseService
        {
            RegisterResponse = new 공동구매자동집단응답 { 자동집단Id = "group-us-10001" },
            WithdrawalResponse = new 공동구매자동수요철회응답
            {
                철회완료 = true,
                안내 = "withdrawn"
            }
        };
        using var viewModel = CreateViewModel(new FakeFoodClient(DishDetail()), groupService);
        await viewModel.초기화Async();
        await viewModel.LoadHsReferencesAsync();
        viewModel.SelectHsReference(81);
        viewModel.UsZipCode = "10001";
        Assert.True(await viewModel.JoinDemandPoolAsync());
        var registeredSourceKey = groupService.LastRegisteredDemand!.수요출처키;

        var withdrawn = await viewModel.WithdrawDemandAsync();

        Assert.True(withdrawn);
        Assert.Equal(registeredSourceKey, groupService.LastWithdrawnDemandSourceKey);
        Assert.Null(viewModel.RegisteredGroup);
        Assert.False(viewModel.CanWithdrawDemand);
    }

    [Fact]
    public void 화면구성은_전용경로와비구속가격통관경계를_명시한다()
    {
        var root = FindRepositoryRoot();
        var component = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "UnitedStatesKoreanFoodGroupBuyStorefront.razor"));
        var css = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "UnitedStatesKoreanFoodGroupBuyStorefront.razor.css"));
        var webRoute = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.WebApp",
            "Pages",
            "UnitedStatesKoreanFoodGroupBuyPage.razor"));

        Assert.Contains("Discover Korean food. Pool ingredient demand.", component);
        Assert.Contains("lower final price is a goal, not a guarantee", component);
        Assert.Contains("Not declaration-ready", component);
        Assert.Contains("No payment, purchase order, import declaration, or supply contract", component);
        Assert.Contains("min-height: 44px", css);
        Assert.Contains("@media (max-width: 640px)", css);
        Assert.Contains("@page \"/us/korean-food-group-buy\"", webRoute);
    }

    [Fact]
    public void 공통Ui등록은_미국공동구매상점ViewModel을포함한다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelUiCommonAppServices();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(UnitedStatesKoreanFoodGroupBuyStorefrontViewModel)
            && descriptor.ImplementationType == typeof(UnitedStatesKoreanFoodGroupBuyStorefrontViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
    }

    private static UnitedStatesKoreanFoodGroupBuyStorefrontViewModel CreateViewModel(
        FakeFoodClient foodClient,
        FakeGroupPurchaseService groupService)
        => new(
            foodClient,
            groupService,
            new FakeCurrentUserContext(new 현재사용자Snapshot(
                "account-42",
                "Buyer",
                [])));

    private static OfficialFoodDishDetailDto DishDetail()
    {
        var timestamp = new DateTime(2026, 7, 22, 3, 0, 0, DateTimeKind.Utc);
        var dish = new OfficialFoodRecipeDishDto(
            "dish:kr-onion-pancake",
            "KR",
            "전국",
            "양파전",
            "양파전",
            "Korean onion pancake",
            "전",
            "양파를 사용한 한국식 전",
            OfficialFoodRecipeRepresentationStates.Candidate,
            OfficialFoodRecipeReviewStates.PendingReview,
            1,
            timestamp);
        var ingredient = new OfficialFoodRecipeIngredientDto(
            "ingredient:onion",
            "양파",
            OfficialFoodIngredientCategoryCodes.Vegetable,
            "채소",
            "주재료",
            "양파 1개",
            "양파",
            "1개",
            1m,
            null,
            "count",
            "개",
            string.Empty,
            "채썰기",
            1,
            "test-parser",
            .98m,
            false);
        return new OfficialFoodDishDetailDto(
            dish,
            "recipe:kr-onion-pancake:1",
            OfficialFoodRecipeSourceKeys.MfdsCookRecipe,
            "식품의약품안전처",
            "양파전",
            "2인분",
            "https://foodsafety.example/kr-onion-pancake",
            "식품의약품안전처 공개 레시피",
            timestamp,
            true,
            [ingredient]);
    }

    private static OfficialFoodIngredientHsMappingResponse HsMapping()
        => new(
            "ingredient:onion",
            "양파",
            null,
            true,
            new DateTime(2026, 7, 22, 4, 0, 0, DateTimeKind.Utc),
            [
                new OfficialFoodIngredientHsCandidateDto(
                    81,
                    801,
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
                    .64m,
                    OfficialFoodIngredientHsMappingStates.Candidate,
                    "Fresh onion family candidate",
                    "Product form must be checked.",
                    ["fresh, chilled, dried, or powdered form"],
                    "USITC HTS",
                    "https://hts.usitc.gov/",
                    new DateTime(2026, 1, 1),
                    null,
                    new DateTime(2026, 7, 1),
                    new DateTime(2026, 7, 22, 4, 0, 0, DateTimeKind.Utc),
                    true,
                    false)
            ],
            ["Not ready for a customs declaration."]);

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

        throw new DirectoryNotFoundException("Ssalddel repository root was not found.");
    }

    private sealed class FakeFoodClient(OfficialFoodDishDetailDto detail)
        : IOfficialFoodIngredientDiscoveryClient
    {
        public OfficialFoodDishDiscoveryQuery? LastDishQuery { get; private set; }

        public Task<IReadOnlyList<OfficialFoodRecipeDishDto>> SearchDishesAsync(
            OfficialFoodDishDiscoveryQuery query,
            CancellationToken cancellationToken = default)
        {
            LastDishQuery = query;
            return Task.FromResult<IReadOnlyList<OfficialFoodRecipeDishDto>>([detail.Dish]);
        }

        public Task<OfficialFoodDishDetailDto?> GetDishAsync(
            string dishKey,
            CancellationToken cancellationToken = default)
            => Task.FromResult<OfficialFoodDishDetailDto?>(
                string.Equals(detail.Dish.DishKey, dishKey, StringComparison.Ordinal)
                    ? detail
                    : null);

        public Task<OfficialFoodIngredientCompanyResearchResponse> SearchCompaniesAsync(
            OfficialFoodIngredientCompanyQuery query,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<OfficialFoodIngredientHsMappingResponse> GetHsCodesAsync(
            OfficialFoodIngredientHsQuery query,
            CancellationToken cancellationToken = default)
            => Task.FromResult(HsMapping());

        public Task<IReadOnlyList<OfficialFoodIngredientDto>> SearchAsync(
            OfficialFoodIngredientQuery query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<OfficialFoodIngredientDto>>([]);
    }

    private sealed class FakeGroupPurchaseService : I공동구매실행Service
    {
        public 공동구매자동집단배치미리보기응답? PreviewResponse { get; init; }
        public 공동구매자동집단응답? RegisterResponse { get; init; }
        public 공동구매자동수요철회응답? WithdrawalResponse { get; init; }
        public 공동구매자동수요등록Command? LastPreviewDemand { get; private set; }
        public 공동구매자동수요등록Command? LastRegisteredDemand { get; private set; }
        public string? LastWithdrawnDemandSourceKey { get; private set; }

        public Task<IReadOnlyList<공동구매자동집단응답>> 자동집단목록조회Async(
            공동구매자동집단조회조건 condition,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<공동구매자동집단응답>>([]);

        public Task<공동구매자동집단배치미리보기응답?> 자동배치미리보기Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            LastPreviewDemand = request;
            return Task.FromResult(PreviewResponse);
        }

        public Task<공동구매자동집단응답?> 자동수요등록Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            LastRegisteredDemand = request;
            return Task.FromResult(RegisterResponse);
        }

        public Task<공동구매자동수요철회응답?> 자동수요철회Async(
            string demandSourceKey,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            LastWithdrawnDemandSourceKey = demandSourceKey;
            return Task.FromResult(WithdrawalResponse);
        }

        public Task<주문원장역할별조회공개Dto?> 주문원장보호조회Async(string orderLedgerId, CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장역할별조회공개Dto?>(null);

        public Task<주문원장역할별조회공개Dto?> 주문원장역할조회Async(string orderLedgerId, string viewCode, CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장역할별조회공개Dto?>(null);

        public Task<주문원장통합공개Dto?> 하위원장연결Async(string orderLedgerId, 주문하위원장연결ClientRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장통합공개Dto?>(null);

        public Task<주문원장통합공개Dto?> 하위원장분리Async(string orderLedgerId, string childLedgerId, long? expectedRevision = null, CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장통합공개Dto?>(null);

        public Task<주문원장서명상태공개Dto?> 주문원장서명상태조회Async(string orderLedgerId, CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장서명상태공개Dto?>(null);

        public Task<주문원장서명상태공개Dto?> 주문원장서명준비Async(string orderLedgerId, 주문원장서명준비ClientRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장서명상태공개Dto?>(null);

        public Task<주문원장서명상태공개Dto?> 주문원장서명등록Async(string orderLedgerId, 주문원장서명등록ClientRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장서명상태공개Dto?>(null);

        public Task<IReadOnlyList<공동구매커머스이행계획공개Dto>> 공동구매별커머스이행조회Async(string groupPurchaseId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<공동구매커머스이행계획공개Dto>>([]);

        public Task<IReadOnlyList<공동구매커머스이행계획공개Dto>> 문서번호로커머스이행조회Async(string documentManagementNumber, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<공동구매커머스이행계획공개Dto>>([]);
    }

    private sealed class FakeCurrentUserContext(현재사용자Snapshot currentUser)
        : ISsalddel현재사용자Context
    {
        public 현재사용자Snapshot 현재사용자 { get; } = currentUser;
    }
}
