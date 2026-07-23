using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.OrdererApp;

public sealed class GroupPurchaseWishBatchViewModelTests
{
    [Fact]
    public async Task 여러재료는_같은거래문맥으로_재료별개별원함을_따로저장한다()
    {
        var service = new FakeDemandService();
        var viewModel = Create(service);
        viewModel.Initialize(Products());
        viewModel.Items[0].Selected = true;
        viewModel.Items[1].Selected = true;
        viewModel.Items[0].Quantity = 10m;
        viewModel.Items[1].Quantity = 25m;
        viewModel.ApplyTransactionType(공동구매거래유형코드.B2B);
        viewModel.PriceBasis = 공동구매가격표시기준코드.부가세별도;
        viewModel.PurchasingOrganizationReference = "buyer-org-1";
        viewModel.PurchasingOrganizationName = "이웃 식당";
        viewModel.TaxInvoiceRequired = true;
        viewModel.NonBindingAgreementAccepted = true;

        var result = await viewModel.SaveAsync();

        Assert.True(result);
        Assert.Equal(2, service.SaveRequests.Count);
        Assert.Equal(2, service.PreviewRequests.Count);
        Assert.Equal(2, service.SaveRequests.Select(x => x.수요출처키).Distinct().Count());
        Assert.All(service.SaveRequests, request =>
        {
            Assert.Equal(공동구매거래유형코드.B2B, request.거래유형);
            Assert.Equal(공동구매가격표시기준코드.부가세별도, request.가격표시기준);
            Assert.Equal("buyer-org-1", request.구매조직참조키);
            Assert.True(request.세금계산서필요);
            Assert.Equal(공동구매자동수요물류방식코드.후속검토, request.물류방식);
            Assert.Equal(공동구매자동결제상태코드.미결제, request.결제상태);
        });
        Assert.Equal([10m, 25m], service.SaveRequests.Select(x => x.희망수량).ToArray());
        Assert.Equal(2, viewModel.SavedCount);
    }

    [Fact]
    public async Task 비구속동의가없으면_어떤재료도저장하지않는다()
    {
        var service = new FakeDemandService();
        var viewModel = Create(service);
        viewModel.Initialize(Products());
        viewModel.Items[0].Selected = true;

        var result = await viewModel.SaveAsync();

        Assert.False(result);
        Assert.Empty(service.PreviewRequests);
        Assert.Empty(service.SaveRequests);
        Assert.Contains("비구속", viewModel.StatusMessage);
    }

    [Fact]
    public async Task B2C저장은_조직과세금계산서조건을보내지않는다()
    {
        var service = new FakeDemandService();
        var viewModel = Create(service);
        viewModel.Initialize(Products());
        viewModel.Items[0].Selected = true;
        viewModel.NonBindingAgreementAccepted = true;

        Assert.True(await viewModel.SaveAsync());

        var request = Assert.Single(service.SaveRequests);
        Assert.Equal(공동구매거래유형코드.B2C, request.거래유형);
        Assert.Equal(공동구매가격표시기준코드.부가세포함, request.가격표시기준);
        Assert.Empty(request.구매조직참조키);
        Assert.Empty(request.구매조직표시명);
        Assert.False(request.세금계산서필요);
    }

    private static GroupPurchaseWishBatchViewModel Create(FakeDemandService service)
        => new(service, new FakeCurrentUserContext(new 현재사용자Snapshot(
            "orderer-1",
            "주문자",
            ["Orderer"],
            new 주문자집단배송권Snapshot("scope:seoul", "서울 생활권", "test"))));

    private static IReadOnlyList<HS먹거리공동구매상품카드> Products()
        =>
        [
            new("pork", "냉동 삼겹살", "0203.29", "냉동 돼지고기", 공동구매온도코드.냉동, 공동구매물류방식코드.FCL, 1000m, 8000m),
            new("sauce", "간편식 소스", "2106.90", "조제 식품", 공동구매온도코드.상온, 공동구매물류방식코드.LCL, 500m, 3000m)
        ];

    private sealed class FakeCurrentUserContext(현재사용자Snapshot user) : ISsalddel현재사용자Context
    {
        public 현재사용자Snapshot 현재사용자 { get; } = user;
    }

    private sealed class FakeDemandService : I비구속공동구매수요Service
    {
        public List<공동구매자동수요등록Command> PreviewRequests { get; } = [];
        public List<공동구매자동수요등록Command> SaveRequests { get; } = [];

        public Task<공동구매자동집단배치미리보기응답?> 수요배치미리보기Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            PreviewRequests.Add(request);
            return Task.FromResult<공동구매자동집단배치미리보기응답?>(new()
            {
                자동집단Id = $"group-{request.상품키}",
                배치유형 = 공동구매자동집단배치유형코드.신규집단
            });
        }

        public Task<공동구매자동집단사용자응답?> 비구속수요저장Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            SaveRequests.Add(request);
            return Task.FromResult<공동구매자동집단사용자응답?>(new()
            {
                자동집단Id = $"group-{request.상품키}",
                상품키 = request.상품키,
                상품명 = request.상품명,
                수요목록 =
                [
                    new 공동구매자동본인수요응답
                    {
                        수요출처키 = request.수요출처키,
                        개별원함원장Id = $"wish-{request.상품키}",
                        희망수량 = request.희망수량,
                        수량단위 = request.수량단위
                    }
                ]
            });
        }
    }
}
