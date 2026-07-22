using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class OrdererIngredientCardAutoGroupingViewModelTests
{
    [Fact]
    public async Task 재료카드_한번클릭은_배치미리보기와_비구속저장을_연속실행한다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, AuthenticatedOrderer());
        var product = Product();

        var result = await viewModel.JoinAsync(product);

        Assert.True(result);
        var preview = Assert.Single(service.PreviewRequests);
        var saved = Assert.Single(service.SaveRequests);
        Assert.Same(preview, saved);
        Assert.Equal(product.상품카드Id, saved.상품키);
        Assert.Equal("scope:seoul-gangnam", saved.배송권키);
        Assert.Equal("서울 강남 생활권", saved.배송권명);
        Assert.Equal(20m, saved.희망수량);
        Assert.Equal(공동구매자동수요유형코드.관심표시, saved.수요유형);
        Assert.Equal(공동구매자동결제상태코드.미결제, saved.결제상태);
        Assert.Null(saved.예약결제금액);
        Assert.Empty(saved.수령도로명주소);
        Assert.DoesNotContain("orderer-17", saved.수요출처키, StringComparison.Ordinal);

        var state = viewModel.StateFor(product.상품카드Id);
        Assert.True(state.HasActiveDemand);
        Assert.NotNull(state.PlacementPreview);
        Assert.NotNull(state.RegisteredGroup);
        Assert.Contains("기존 집단", state.Notice);
    }

    [Fact]
    public async Task 배송권이_없는계정은_자동집단을_호출하지않고_한번설정을_안내한다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(
            service,
            new 현재사용자Snapshot("orderer-17", "이웃 주문자", ["Orderer"]));
        var product = Product();

        var result = await viewModel.JoinAsync(product);

        Assert.False(result);
        Assert.Empty(service.PreviewRequests);
        Assert.Empty(service.SaveRequests);
        Assert.Contains("배송권을 한 번 설정", viewModel.StateFor(product.상품카드Id).ErrorMessage);
    }

    [Fact]
    public async Task 저장재시도는_같은멱등키와_같은사용자수요키를_유지한다()
    {
        var service = new FakeDemandService { FailFirstSave = true };
        var viewModel = CreateViewModel(service, AuthenticatedOrderer());
        var product = Product();

        Assert.False(await viewModel.JoinAsync(product));
        Assert.True(await viewModel.JoinAsync(product));

        Assert.Equal(2, service.SaveRequests.Count);
        Assert.Equal(service.SaveRequests[0].요청멱등키, service.SaveRequests[1].요청멱등키);
        Assert.Equal(service.SaveRequests[0].수요출처키, service.SaveRequests[1].수요출처키);
    }

    [Fact]
    public async Task 카드안에서_비구속참여를_철회할수있다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, AuthenticatedOrderer());
        var product = Product();
        Assert.True(await viewModel.JoinAsync(product));
        var savedSourceKey = Assert.Single(service.SaveRequests).수요출처키;

        var result = await viewModel.WithdrawAsync(product);

        Assert.True(result);
        Assert.Equal(savedSourceKey, service.WithdrawDemandSourceKey);
        Assert.StartsWith("demand-withdraw:", service.WithdrawIdempotencyKey, StringComparison.Ordinal);
        Assert.False(viewModel.StateFor(product.상품카드Id).HasActiveDemand);
        Assert.Contains("철회", viewModel.StateFor(product.상품카드Id).Notice);
    }

    private static OrdererIngredientCardAutoGroupingViewModel CreateViewModel(
        FakeDemandService service,
        현재사용자Snapshot user)
        => new(service, new FakeCurrentUserContext(user));

    private static 현재사용자Snapshot AuthenticatedOrderer()
        => new(
            "orderer-17",
            "이웃 주문자",
            ["Orderer"],
            new 주문자집단배송권Snapshot(
                "scope:seoul-gangnam",
                "서울 강남 생활권",
                "가입 온보딩"));

    private static HS먹거리공동구매상품카드 Product()
        => new(
            상품카드Id: "hs-food-0203-pork-frozen",
            상품명: "냉동 삼겹살",
            HS코드: "0203.29",
            HS표시명: "돼지고기 냉동 기타",
            온도코드: 공동구매온도코드.냉동,
            예상물류방식: 공동구매물류방식코드.FCL,
            SuggestedTargetQuantityKg: 12000m,
            ExpectedUnitPrice: 8500m);

    private sealed class FakeCurrentUserContext(현재사용자Snapshot user) : ISsalddel현재사용자Context
    {
        public 현재사용자Snapshot 현재사용자 { get; } = user;
    }

    private sealed class FakeDemandService : I비구속공동구매수요Service
    {
        public List<공동구매자동수요등록Command> PreviewRequests { get; } = [];
        public List<공동구매자동수요등록Command> SaveRequests { get; } = [];
        public bool FailFirstSave { get; init; }
        public string? WithdrawDemandSourceKey { get; private set; }
        public string? WithdrawIdempotencyKey { get; private set; }

        public Task<공동구매자동집단배치미리보기응답?> 수요배치미리보기Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            PreviewRequests.Add(request);
            return Task.FromResult<공동구매자동집단배치미리보기응답?>(new()
            {
                배치유형 = 공동구매자동집단배치유형코드.기존집단,
                자동집단Id = "auto-group-1",
                비구속안내 = "비구속 수요"
            });
        }

        public Task<공동구매자동집단사용자응답?> 비구속수요저장Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            SaveRequests.Add(request);
            if (FailFirstSave && SaveRequests.Count == 1)
            {
                throw new InvalidOperationException("일시 저장 실패");
            }

            return Task.FromResult<공동구매자동집단사용자응답?>(new()
            {
                자동집단Id = "auto-group-1",
                상품키 = request.상품키,
                상품명 = request.상품명,
                배송권키 = request.배송권키,
                배송권명 = request.배송권명,
                참여자수 = 3,
                수요건수 = 3,
                총희망수량 = 45m,
                수량단위 = request.수량단위,
                현재상태 = 공동구매자동집단상태코드.수요수집중,
                수요목록 =
                [
                    new 공동구매자동본인수요응답
                    {
                        수요출처키 = request.수요출처키,
                        희망수량 = request.희망수량,
                        수량단위 = request.수량단위
                    }
                ]
            });
        }

        public Task<공동구매자동수요철회응답?> 비구속수요철회Async(
            string demandSourceKey,
            string idempotencyKey,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            WithdrawDemandSourceKey = demandSourceKey;
            WithdrawIdempotencyKey = idempotencyKey;
            return Task.FromResult<공동구매자동수요철회응답?>(new()
            {
                수요출처키 = demandSourceKey,
                철회완료 = true,
                안내 = "철회 완료"
            });
        }
    }
}
