using Ssalddel.Unity.Data;
using Ssalddel.Unity.Exhibition;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Unity.Tests;

public sealed class MarketProductBusinessSeedbedTests
{
    private readonly MarketProductSeedbedProjector projector = new();
    private readonly MarketOrderIntentCoordinator coordinator = new();

    [Fact]
    public async Task 기존마트표본의_감자_쌀_양파를같은모판에올린다()
    {
        var source = await new Simulated도심마트공개상품DataQuery().조회Async();

        var result = projector.Project(source);

        Assert.Equal(3, result.Products.Length);
        Assert.Equal(new[] { "감자", "쌀", "양파" },
            result.Products.Select(value => value.DisplayName).ToArray());
        Assert.All(result.Products, value =>
            Assert.Contains("seedbed-object:city.urban-market-building.a",
                value.SeedbedObjectStableIds));
    }

    [Fact]
    public async Task 감자만생산계보까지연결하고_쌀과양파는마트업무범위를명시한다()
    {
        var source = await new Simulated도심마트공개상품DataQuery().조회Async();
        var products = projector.Project(source).Products;

        var potato = Assert.Single(products, value => value.DisplayName == "감자");
        Assert.True(potato.HasProductionRuleConnection);
        Assert.Equal("product:potato", potato.CanonicalProductStableId);
        Assert.Contains("rule:potato-production.fixture.v1", potato.RuleStableIds);

        foreach (var product in products.Where(value => value.DisplayName is "쌀" or "양파"))
        {
            Assert.False(product.HasProductionRuleConnection);
            Assert.Empty(product.CanonicalProductStableId);
            Assert.Empty(product.RuleStableIds);
            Assert.Contains("생산 규칙은 아직 없습니다", Assert.Single(product.Limitations));
        }
    }

    [Fact]
    public async Task Simulation상품표본은_운영주문의향API를호출할수없다()
    {
        var source = await new Simulated도심마트공개상품DataQuery().조회Async();
        var products = projector.Project(source).Products;

        Assert.All(products, value => Assert.False(value.CanRequestOperationalOrderIntent));
    }

    [Fact]
    public void 운영API의모든공개상품을_품목수에상관없이동적으로변환한다()
    {
        var source = OperationalSnapshot(
            Product(41, "양파", 8, true),
            Product(42, "쌀", 6, true),
            Product(43, "배추", 0, false));

        var result = projector.Project(source);

        Assert.Equal(3, result.Products.Length);
        Assert.Equal(new long?[] { 41, 42, 43 },
            result.Products.Select(value => value.OperationalProductId).ToArray());
        Assert.All(result.Products, value =>
            Assert.Equal(MarketProductSeedbedConnectionDepthCodes.MarketBusinessOnly,
                value.ConnectionDepthCode));
        Assert.True(result.Products[0].CanRequestOperationalOrderIntent);
        Assert.False(result.Products[2].CanRequestOperationalOrderIntent);
    }

    [Fact]
    public void 서버마트API대장은_조회와비구속의향기록경계를분리한다()
    {
        var catalog = MarketProductBusinessApiCatalog.Create();

        Assert.Equal(7, catalog.Length);
        Assert.Contains(catalog, value =>
            value.RouteTemplate == MarketProductBusinessApiRoutes.PublicProducts
            && value.UnityImplementationCode
                == MarketProductBusinessApiImplementationCodes.ExistingUnityAdapter);
        Assert.Contains(catalog, value =>
            value.RouteTemplate == MarketProductBusinessApiRoutes.OrderRequests
            && value.HttpMethod == "POST"
            && value.RequiresAuthentication
            && value.RequiresPrivacyConsentEvidence);
        Assert.All(catalog, value =>
        {
            Assert.False(value.MutatesInventory);
            Assert.False(value.CreatesPayment);
        });
    }

    [Fact]
    public void 양파운영상품은_비구속주문의향을서버기록후재조회할수있다()
    {
        var product = projector.Project(OperationalSnapshot(Product(41, "양파", 8, true)))
            .Products.Single();
        var session = coordinator.CreatePreview(product, ValidDraft(2));

        Assert.Equal(36000m, session.Preview.ExpectedTotalPrice);
        Assert.False(session.Preview.CreatesInventoryReservation);
        Assert.False(session.Preview.CreatesPayment);
        Assert.False(session.Preview.CreatesConfirmedOrder);

        coordinator.RequestServerRecord(session);
        var response = ServerResponse(product, session, Guid.Parse("20bfba93-158c-46e6-a226-87d2944294e8"));
        coordinator.AcceptServerRecord(session, response);
        Assert.Equal(MarketOrderIntentPhaseCodes.AwaitingServerRefresh, session.PhaseCode);

        coordinator.ApplyServerRefresh(session, ServerResponse(
            product,
            session,
            response.주문요청Id));

        Assert.Equal(MarketOrderIntentPhaseCodes.Reconciled, session.PhaseCode);
        Assert.False(session.RefreshedResponse!.재고예약됨);
        Assert.False(session.RefreshedResponse.결제됨);
        Assert.False(session.CanonicalStateMutatedByPresentation);
    }

    [Fact]
    public void 인증과개인정보동의가없으면_주문의향서버기록을요청하지않는다()
    {
        var product = projector.Project(OperationalSnapshot(Product(42, "쌀", 6, true)))
            .Products.Single();
        var draft = ValidDraft(1);
        draft.IsAuthenticated = false;
        draft.PrivacyConsentEvidenceId = Guid.Empty;

        var session = coordinator.CreatePreview(product, draft);

        Assert.Contains("AuthenticationRequired", session.Preview.BlockingReasonCodes);
        Assert.Contains("PrivacyConsentEvidenceRequired", session.Preview.BlockingReasonCodes);
        Assert.Throws<InvalidOperationException>(() => coordinator.RequestServerRecord(session));
        Assert.Equal(MarketOrderIntentPhaseCodes.PreviewReady, session.PhaseCode);
    }

    [Fact]
    public void 공개판매가능수량을넘는의향은_서버전송전에차단한다()
    {
        var product = projector.Project(OperationalSnapshot(Product(42, "쌀", 6, true)))
            .Products.Single();

        var session = coordinator.CreatePreview(product, ValidDraft(7));

        Assert.Contains("QuantityExceedsProjectedAvailability",
            session.Preview.BlockingReasonCodes);
        Assert.Throws<InvalidOperationException>(() => coordinator.RequestServerRecord(session));
    }

    [Fact]
    public void 서버응답이재고예약이나결제를주장하면_비구속의향결과로받지않는다()
    {
        var product = projector.Project(OperationalSnapshot(Product(41, "양파", 8, true)))
            .Products.Single();
        var session = coordinator.CreatePreview(product, ValidDraft(2));
        coordinator.RequestServerRecord(session);
        var response = ServerResponse(product, session, Guid.NewGuid());
        response.재고예약됨 = true;

        var error = Assert.Throws<InvalidOperationException>(() =>
            coordinator.AcceptServerRecord(session, response));

        Assert.Equal("MarketOrderIntentUnexpectedOperationalEffect", error.Message);
        Assert.Equal(MarketOrderIntentPhaseCodes.AwaitingServerRecord, session.PhaseCode);
    }

    [Fact]
    public void 서버기록응답만으로는_최종조화완료로판정하지않는다()
    {
        var product = projector.Project(OperationalSnapshot(Product(41, "양파", 8, true)))
            .Products.Single();
        var session = coordinator.CreatePreview(product, ValidDraft(2));
        coordinator.RequestServerRecord(session);
        coordinator.AcceptServerRecord(session, ServerResponse(product, session, Guid.NewGuid()));

        Assert.Equal(MarketOrderIntentPhaseCodes.AwaitingServerRefresh, session.PhaseCode);
        Assert.Null(session.RefreshedResponse);
    }

    [Fact]
    public async Task APIUseCase는_등록응답뒤같은주문의향을반드시재조회한다()
    {
        var product = projector.Project(OperationalSnapshot(Product(42, "쌀", 6, true)))
            .Products.Single();
        var session = coordinator.CreatePreview(product, ValidDraft(1));
        var response = ServerResponse(
            product,
            session,
            Guid.Parse("90459815-f722-49da-bfa0-691a80824233"));
        var client = new RecordingOrderIntentApiClient(response);
        var useCase = new MarketOrderIntentServerUseCase(client, coordinator);

        await useCase.기록후재조회Async(session);

        Assert.Equal(1, client.CreateCallCount);
        Assert.Equal(1, client.DetailCallCount);
        Assert.Equal(response.주문요청Id, client.LastDetailOrderRequestId);
        Assert.Equal(MarketOrderIntentPhaseCodes.Reconciled, session.PhaseCode);
    }

    [Fact]
    public void 주문의향API사본은_서버의한국어JSON계약필드명을보존한다()
    {
        Assert.Equal(
            new[]
            {
                "신청개인정보동의증적Id",
                "신청출처Code",
                "클라이언트요청Id",
                "공개상품Id",
                "수량",
                "비구속주문요청확인",
                "안내버전",
            }.OrderBy(value => value),
            typeof(MarketOrderIntentCommandApiModel)
                .GetProperties()
                .Select(value => value.Name)
                .OrderBy(value => value));

        Assert.Contains(
            typeof(MarketOrderIntentResponseApiModel).GetProperties(),
            value => value.Name == "주문요청Id");
        Assert.Contains(
            typeof(MarketOrderIntentResponseApiModel).GetProperties(),
            value => value.Name == "재고예약됨");
        Assert.Contains(
            typeof(MarketOrderIntentResponseApiModel).GetProperties(),
            value => value.Name == "결제됨");
    }

    private static MarketOrderIntentDraft ValidDraft(int quantity)
        => new()
        {
            IsAuthenticated = true,
            PrivacyConsentEvidenceId = Guid.Parse("f5f50cbe-1a55-4610-b9a6-3948cd4a573d"),
            ApplicationSourceCode = "UnitySeedbed",
            ClientRequestId = Guid.Parse("f782c26f-04da-477c-9815-0731772f2c7d"),
            Quantity = quantity,
            NonBindingOrderRequestConfirmed = true,
            NoticeVersion = MarketOrderIntentCoordinator.CurrentNoticeVersion,
        };

    private static MarketOrderIntentResponseApiModel ServerResponse(
        MarketProductSeedbedItemSnapshot product,
        MarketOrderIntentSessionSnapshot session,
        Guid orderRequestId)
        => new()
        {
            주문요청Id = orderRequestId,
            공개상품Id = product.OperationalProductId!.Value,
            상품명 = product.DisplayName,
            판매단위 = product.SaleUnit,
            단가 = product.Price,
            수량 = session.Command.수량,
            합계 = product.Price * session.Command.수량,
            통화 = product.CurrencyCode,
            제출시판매가능수량 = product.ProjectedSaleAvailability,
            재고기준시각Utc = DateTimeOffset.Parse("2026-08-12T10:00:00+09:00"),
            상태코드 = "Submitted",
            안내버전 = MarketOrderIntentCoordinator.CurrentNoticeVersion,
            제출일시Utc = DateTimeOffset.Parse("2026-08-12T10:01:00+09:00"),
            재고예약됨 = false,
            결제됨 = false,
        };

    private static 도심마트공개상품DataSnapshot OperationalSnapshot(
        params 도심마트공개상품Data[] products)
        => new()
        {
            StableId = "market:urban-public",
            DataRevision = "public-products:9001",
            LegacyRevision = 9001,
            마트명 = "살뜰 도심 마트",
            ProjectionAudienceCode = 도심마트ProjectionAudienceCodes.OrdererPublic,
            ScopeKind = DataScopeKind.Global,
            Mode = DataRuntimeMode.Operational,
            GeneratedAt = DateTimeOffset.Parse("2026-08-12T10:00:00+09:00"),
            QuantityDisclosure = "판매 가능 수량 관점별 조회 결과이며 내부 재고가 아닙니다.",
            상품목록 = products,
        };

    private static 도심마트공개상품Data Product(
        long id,
        string name,
        int availability,
        bool available)
        => new()
        {
            StableId = "mart-product:" + id,
            상품명 = name,
            판매단위 = "10kg",
            판매가 = name == "양파" ? 18000m : 42000m,
            통화Code = "KRW",
            투영판매가능수량 = availability,
            투영수량단위 = "상자",
            서버판매가능여부 = available,
            QuantityMeaningCode = 도심마트QuantityMeaningCodes.ProjectedSaleAvailability,
            SourceName = "Ssalddel 마트 공개 상품 API",
            SourceHref = MarketProductBusinessApiRoutes.PublicProducts,
            EvidenceAsOf = DateTimeOffset.Parse("2026-08-12T10:00:00+09:00"),
            SourceRevision = "9001",
            EvidenceStatusCode = "Operational",
        };

    private sealed class RecordingOrderIntentApiClient : IMarketOrderIntentApiClient
    {
        private readonly MarketOrderIntentResponseApiModel response;

        public RecordingOrderIntentApiClient(MarketOrderIntentResponseApiModel value)
            => response = value;

        public int CreateCallCount { get; private set; }
        public int DetailCallCount { get; private set; }
        public Guid LastDetailOrderRequestId { get; private set; }

        public Task<MarketOrderIntentResponseApiModel> 등록Async(
            MarketOrderIntentCommandApiModel command,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CreateCallCount++;
            Assert.Equal(response.공개상품Id, command.공개상품Id);
            Assert.Equal(response.수량, command.수량);
            return Task.FromResult(response);
        }

        public Task<MarketOrderIntentResponseApiModel> 상세조회Async(
            Guid orderRequestId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DetailCallCount++;
            LastDetailOrderRequestId = orderRequestId;
            return Task.FromResult(response);
        }
    }
}
