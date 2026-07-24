using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class OfficialFoodIngredientDemandViewModelTests
{
    [Fact]
    public async Task 비로그인_탐색은_유지하지만_집단화미리보기는_호출하지않는다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, 현재사용자Snapshot.익명);
        viewModel.ApplySeed(Seed());
        viewModel.DeliveryAreaCode = "06236";

        var result = await viewModel.PreviewAsync();

        Assert.False(result);
        Assert.Contains("로그인", viewModel.ActionError);
        Assert.Empty(service.PreviewRequests);
        Assert.Empty(service.SaveRequests);
    }

    [Fact]
    public async Task 공동할인후보는_미리본뒤_별도동의해야_개별주문의향을저장한다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, AuthenticatedUser(), consent: false);
        viewModel.ApplySeed(Seed());
        viewModel.DeliveryAreaCode = "06236";

        Assert.True(await viewModel.PreviewAsync());
        Assert.False(await viewModel.RegisterAsync());
        Assert.Contains("동의", viewModel.ActionError, StringComparison.Ordinal);
        Assert.Empty(service.SaveRequests);

        viewModel.공동주문후보참여동의 = true;

        Assert.True(await viewModel.RegisterAsync());
        Assert.Single(service.SaveRequests);
    }

    [Fact]
    public async Task 탐색문맥은_stable상품키와_비식별수령권역으로_비구속수요에이어진다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, AuthenticatedUser());
        viewModel.ApplySeed(Seed(foodCountryCode: "JP"));
        viewModel.공동주문후보참여동의 = true;
        viewModel.DeliveryCountryCode = "KR";
        viewModel.DeliveryAreaCode = "06236";
        viewModel.DesiredQuantity = 2.5m;

        Assert.True(await viewModel.PreviewAsync());
        Assert.True(await viewModel.RegisterAsync());

        var preview = Assert.Single(service.PreviewRequests);
        var saved = Assert.Single(service.SaveRequests);
        Assert.Equal("official-ingredient:ingredient:onion", saved.상품키);
        Assert.Equal("양파", saved.상품명);
        Assert.Equal("delivery:kr:06236:shared-pickup", saved.배송권키);
        Assert.DoesNotContain("jp", saved.배송권키, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2.5m, saved.희망수량);
        Assert.Equal(공동구매자동수요물류방식코드.후속검토, saved.물류방식);
        Assert.Equal(공동구매거래유형코드.B2C, saved.거래유형);
        Assert.Equal(공동구매가격표시기준코드.부가세포함, saved.가격표시기준);
        Assert.Empty(saved.구매조직참조키);
        Assert.Empty(saved.구매조직표시명);
        Assert.False(saved.세금계산서필요);
        Assert.Equal(공동구매자동수요유형코드.관심표시, saved.수요유형);
        Assert.Equal(공동구매자동결제상태코드.미결제, saved.결제상태);
        Assert.Null(saved.예약결제금액);
        Assert.Null(saved.도착창고Id);
        Assert.Empty(saved.수령도로명주소);
        Assert.Empty(saved.수령상세주소);
        Assert.Empty(saved.HS코드);
        Assert.Equal(preview.수요출처키, saved.수요출처키);
        Assert.Equal(preview.요청멱등키, saved.요청멱등키);
        Assert.True(viewModel.HasActiveDemand);
    }

    [Fact]
    public async Task B2B수요는_구매조직정보가_없으면_미리보기를호출하지않는다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, AuthenticatedUser());
        viewModel.ApplySeed(Seed());
        viewModel.DeliveryAreaCode = "06236";
        viewModel.TransactionTypeCode = 공동구매거래유형코드.B2B;

        var result = await viewModel.PreviewAsync();

        Assert.False(result);
        Assert.Contains("구매 조직", viewModel.ActionError, StringComparison.Ordinal);
        Assert.Empty(service.PreviewRequests);
    }

    [Fact]
    public async Task B2B수요는_조직가격세금계산서문맥을_모든재료에전달한다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, AuthenticatedUser());
        viewModel.ApplySeeds([Seed(), Seed("TH", "ingredient:chili", "고추", "box")]);
        viewModel.공동주문후보참여동의 = true;
        viewModel.DeliveryAreaCode = "06236";
        viewModel.TransactionTypeCode = 공동구매거래유형코드.B2B;
        viewModel.PurchasingOrganizationReference = "org:market-17";
        viewModel.PurchasingOrganizationName = "이웃마트";

        Assert.True(await viewModel.PreviewAsync());
        Assert.True(await viewModel.RegisterAsync());

        Assert.Equal(2, service.SaveRequests.Count);
        Assert.All(service.SaveRequests, request =>
        {
            Assert.Equal(공동구매거래유형코드.B2B, request.거래유형);
            Assert.Equal(공동구매가격표시기준코드.부가세별도, request.가격표시기준);
            Assert.Equal("org:market-17", request.구매조직참조키);
            Assert.Equal("이웃마트", request.구매조직표시명);
            Assert.True(request.세금계산서필요);
            Assert.Equal(1, request.목표참여자수);
            Assert.Equal(30m, request.목표수량);
        });
        Assert.Equal(
            2,
            service.SaveRequests
                .Select(request => request.수요출처키)
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public async Task 저장실패_재시도는_같은멱등키를사용한다()
    {
        var service = new FakeDemandService { FailFirstSave = true };
        var viewModel = CreateViewModel(service, AuthenticatedUser());
        viewModel.ApplySeed(Seed());
        viewModel.공동주문후보참여동의 = true;
        viewModel.DeliveryAreaCode = "06236";
        Assert.True(await viewModel.PreviewAsync());

        Assert.False(await viewModel.RegisterAsync());
        Assert.True(await viewModel.RegisterAsync());

        Assert.Equal(2, service.SaveRequests.Count);
        Assert.Equal(service.SaveRequests[0].요청멱등키, service.SaveRequests[1].요청멱등키);
        Assert.Equal(service.SaveRequests[0].수요출처키, service.SaveRequests[1].수요출처키);
    }

    [Fact]
    public async Task 수량변경은_같은본인수요를_새멱등명령으로갱신하고_철회한다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, AuthenticatedUser());
        viewModel.ApplySeed(Seed());
        viewModel.공동주문후보참여동의 = true;
        viewModel.DeliveryAreaCode = "06236";
        Assert.True(await viewModel.PreviewAsync());
        Assert.True(await viewModel.RegisterAsync());
        var firstSave = Assert.Single(service.SaveRequests);

        viewModel.DesiredQuantity = 4m;
        Assert.False(viewModel.CanRegister);
        Assert.True(await viewModel.PreviewAsync());
        Assert.True(await viewModel.RegisterAsync());
        var secondSave = service.SaveRequests[1];

        Assert.Equal(firstSave.수요출처키, secondSave.수요출처키);
        Assert.NotEqual(firstSave.요청멱등키, secondSave.요청멱등키);
        Assert.Equal(4m, secondSave.희망수량);

        Assert.True(await viewModel.WithdrawAsync());
        Assert.Equal(secondSave.수요출처키, service.WithdrawDemandSourceKey);
        Assert.StartsWith("demand-withdraw:", service.WithdrawIdempotencyKey, StringComparison.Ordinal);
        Assert.False(viewModel.HasActiveDemand);
        Assert.Null(viewModel.RegisteredGroup);
    }

    [Fact]
    public async Task 여러재료는_재료별수량과온도로_각각비구속수요를저장하고함께철회한다()
    {
        var service = new FakeDemandService();
        var viewModel = CreateViewModel(service, AuthenticatedUser());
        viewModel.ApplySeeds([Seed(), Seed("TH", "ingredient:chili", "고추", "box")]);
        viewModel.공동주문후보참여동의 = true;
        viewModel.DeliveryAreaCode = "06236";
        var chili = viewModel.IngredientLines.Single(line => line.Seed.IngredientKey == "ingredient:chili");
        viewModel.UpdateLineQuantity(chili, 3m);
        viewModel.UpdateLineTemperature(chili, "냉장");

        Assert.True(await viewModel.PreviewAsync());
        Assert.True(await viewModel.RegisterAsync());

        Assert.Equal(2, service.PreviewRequests.Count);
        Assert.Equal(2, service.SaveRequests.Count);
        Assert.Equal(2, viewModel.RegisteredGroups.Count);
        Assert.Equal(2, service.SaveRequests.Select(request => request.상품키).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(2, service.SaveRequests.Select(request => request.수요출처키).Distinct(StringComparer.Ordinal).Count());
        Assert.All(service.SaveRequests, request =>
            Assert.Equal(공동구매자동수요물류방식코드.후속검토, request.물류방식));
        var savedChili = service.SaveRequests.Single(request => request.상품명 == "고추");
        Assert.Equal(3m, savedChili.희망수량);
        Assert.Equal("냉장", savedChili.온도코드);
        Assert.Equal("box", savedChili.수량단위);

        Assert.True(await viewModel.WithdrawAsync());
        Assert.Equal(2, service.WithdrawDemandSourceKeys.Count);
        Assert.False(viewModel.HasActiveDemand);
    }

    [Fact]
    public async Task 음식재료탐색에서_Api경계까지_비구속수요생명주기가이어진다()
    {
        var client = new DemandLifecycleJsonApiClient();
        var service = new 공동구매실행Service(client);
        var viewModel = new OfficialFoodIngredientDemandViewModel(
            service,
            new FakeCurrentUserContext(AuthenticatedUser()));
        viewModel.ApplySeed(Seed());
        viewModel.공동주문후보참여동의 = true;
        viewModel.DeliveryAreaCode = "06236";
        viewModel.DesiredQuantity = 3m;

        Assert.True(await viewModel.PreviewAsync());
        Assert.True(await viewModel.RegisterAsync());
        Assert.True(await viewModel.WithdrawAsync());

        Assert.Collection(
            client.Calls,
            preview =>
            {
                Assert.Equal(HttpMethod.Post, preview.Method);
                Assert.Equal(
                    "api/v1/orderer/group-purchase-auto-groups/placement-preview",
                    preview.Path);
                Assert.Null(preview.Headers);
            },
            save =>
            {
                Assert.Equal(HttpMethod.Put, save.Method);
                Assert.StartsWith(
                    "api/v1/orderer/group-purchase-auto-groups/demands/",
                    save.Path,
                    StringComparison.Ordinal);
                Assert.StartsWith(
                    "demand-save:",
                    save.Headers!["Idempotency-Key"],
                    StringComparison.Ordinal);
            },
            withdrawal =>
            {
                Assert.Equal(HttpMethod.Delete, withdrawal.Method);
                Assert.Contains("?reason=", withdrawal.Path, StringComparison.Ordinal);
                Assert.StartsWith(
                    "demand-withdraw:",
                    withdrawal.Headers!["Idempotency-Key"],
                    StringComparison.Ordinal);
            });

        var previewRequest = Assert.IsType<공동구매자동수요등록Command>(client.Calls[0].Request);
        var saveRequest = Assert.IsType<공동구매자동수요등록Command>(client.Calls[1].Request);
        Assert.Equal(previewRequest.수요출처키, saveRequest.수요출처키);
        Assert.Equal("official-ingredient:ingredient:onion", saveRequest.상품키);
        Assert.Equal("delivery:kr:06236:shared-pickup", saveRequest.배송권키);
        Assert.DoesNotContain("user-17", saveRequest.수요출처키, StringComparison.Ordinal);
        Assert.Equal(공동구매자동수요유형코드.관심표시, saveRequest.수요유형);
        Assert.Equal(공동구매자동결제상태코드.미결제, saveRequest.결제상태);
        Assert.Null(saveRequest.예약결제금액);
        Assert.Null(saveRequest.도착창고Id);
        Assert.Empty(saveRequest.수령도로명주소);
        Assert.Empty(saveRequest.수령상세주소);
        Assert.Empty(saveRequest.HS코드);
        Assert.Contains(
            Uri.EscapeDataString(saveRequest.수요출처키),
            client.Calls[1].Path,
            StringComparison.Ordinal);
        Assert.Contains(
            Uri.EscapeDataString(saveRequest.수요출처키),
            client.Calls[2].Path,
            StringComparison.Ordinal);
    }

    private static OfficialFoodIngredientDemandViewModel CreateViewModel(
        FakeDemandService service,
        현재사용자Snapshot user,
        bool consent = false)
    {
        var viewModel = new OfficialFoodIngredientDemandViewModel(
            service,
            new FakeCurrentUserContext(user))
        {
            공동주문후보참여동의 = consent
        };
        return viewModel;
    }

    private static 현재사용자Snapshot AuthenticatedUser()
        => new("user-17", "이웃 주문자", ["Orderer"]);

    private static CommunityGroupPurchaseIngredientSeed Seed(
        string foodCountryCode = "KR",
        string ingredientKey = "ingredient:onion",
        string ingredientName = "양파",
        string purchaseUnit = "kg")
        => CommunityGroupPurchaseIngredientSeed.Create(
               ingredientKey,
               ingredientName,
               $"{ingredientName}를 넣은 공식 음식",
               $"https://example.test/recipes/{Uri.EscapeDataString(ingredientKey)}",
               "공식 레시피 원천",
               "2개",
               "KRW/kg · 공개 참고값",
               purchaseUnit,
               $"{ingredientName} 음식",
               foodCountryCode,
               CommunityIngredientSourcingModeCodes.DomesticGroupPurchase)
           ?? throw new InvalidOperationException("테스트 재료 seed를 만들지 못했습니다.");

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
        public List<string> WithdrawDemandSourceKeys { get; } = [];

        public Task<공동구매자동집단배치미리보기응답?> 수요배치미리보기Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            PreviewRequests.Add(Clone(request));
            return Task.FromResult<공동구매자동집단배치미리보기응답?>(new()
            {
                정책버전 = 공동구매주문자집단화정책코드.현재버전,
                배치유형 = 공동구매자동집단배치유형코드.신규집단,
                예상진행 = new 공동구매자동집단진행응답
                {
                    참여자수 = 1,
                    총희망수량 = request.희망수량,
                    수량단위 = request.수량단위,
                    목표참여자수 = request.목표참여자수,
                    추가필요참여자수 = 4,
                    모집종료시각Utc = DateTime.UtcNow.AddDays(14)
                },
                비구속안내 = "비구속 수요"
            });
        }

        public Task<공동구매자동집단사용자응답?> 비구속수요저장Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            SaveRequests.Add(Clone(request));
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
                참여자수 = 1,
                수요건수 = 1,
                총희망수량 = request.희망수량,
                수량단위 = request.수량단위,
                모집종료시각Utc = DateTime.UtcNow.AddDays(14),
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
            WithdrawDemandSourceKeys.Add(demandSourceKey);
            return Task.FromResult<공동구매자동수요철회응답?>(new()
            {
                수요출처키 = demandSourceKey,
                철회완료 = true,
                안내 = "철회됨"
            });
        }

        private static 공동구매자동수요등록Command Clone(공동구매자동수요등록Command source)
            => new()
            {
                요청멱등키 = source.요청멱등키,
                수요출처키 = source.수요출처키,
                상품키 = source.상품키,
                상품명 = source.상품명,
                HS코드 = source.HS코드,
                온도코드 = source.온도코드,
                물류방식 = source.물류방식,
                거래유형 = source.거래유형,
                가격표시기준 = source.가격표시기준,
                구매조직참조키 = source.구매조직참조키,
                구매조직표시명 = source.구매조직표시명,
                세금계산서필요 = source.세금계산서필요,
                주문자키 = source.주문자키,
                주문자표시명 = source.주문자표시명,
                배송권키 = source.배송권키,
                배송권명 = source.배송권명,
                도착창고Id = source.도착창고Id,
                수령도로명주소 = source.수령도로명주소,
                수령상세주소 = source.수령상세주소,
                희망수량 = source.희망수량,
                수량단위 = source.수량단위,
                예약결제금액 = source.예약결제금액,
                수요유형 = source.수요유형,
                결제상태 = source.결제상태,
                메모 = source.메모,
                목표참여자수 = source.목표참여자수,
                목표수량 = source.목표수량
            };
    }

    private sealed class DemandLifecycleJsonApiClient : ISsalddelJsonApiClient
    {
        public List<ApiCall> Calls { get; } = [];

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TResponse?> SendAsync<TResponse>(
            HttpMethod method,
            string path,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TResponse?> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
        {
            Calls.Add(new ApiCall(method, path, request, null));
            return Response<TResponse>(new 공동구매자동집단배치미리보기응답
            {
                배치유형 = 공동구매자동집단배치유형코드.신규집단,
                비구속안내 = "비구속 수요"
            });
        }

        public Task<TResponse?> SendWithHeadersAsync<TResponse>(
            HttpMethod method,
            string path,
            IReadOnlyDictionary<string, string> headers,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
        {
            Calls.Add(new ApiCall(method, path, null, Copy(headers)));
            return Response<TResponse>(new 공동구매자동수요철회응답
            {
                철회완료 = true,
                안내 = "철회 완료"
            });
        }

        public Task<TResponse?> SendWithHeadersAsync<TRequest, TResponse>(
            HttpMethod method,
            string path,
            TRequest request,
            IReadOnlyDictionary<string, string> headers,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
        {
            Calls.Add(new ApiCall(method, path, request, Copy(headers)));
            return Response<TResponse>(new 공동구매자동집단사용자응답
            {
                자동집단Id = "auto-group-1"
            });
        }

        public Task SendAsync(
            HttpMethod method,
            string path,
            string operationName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SendAsync<TRequest>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private static Task<TResponse?> Response<TResponse>(object response)
            => Task.FromResult<TResponse?>((TResponse)response);

        private static IReadOnlyDictionary<string, string> Copy(
            IReadOnlyDictionary<string, string> headers)
            => new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
    }

    private sealed record ApiCall(
        HttpMethod Method,
        string Path,
        object? Request,
        IReadOnlyDictionary<string, string>? Headers);
}
