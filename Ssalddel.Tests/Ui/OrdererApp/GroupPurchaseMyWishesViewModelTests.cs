using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.OrdererApp;

public sealed class GroupPurchaseMyWishesViewModelTests
{
    [Fact]
    public async Task 본인원함을_불러와_자동집단별로묶고_집단없는원함은제외한다()
    {
        var summaryA = GroupSummary("group-a", participants: 3, quantity: 42m);
        var response = Response(
            Wish("wish-a1", "source-a1", "group-a", updatedMinute: 1, summary: summaryA),
            Wish(
                "wish-a2",
                "source-a2",
                "group-a",
                status: 공동구매내원함상태코드.닫힘,
                updatedMinute: 2,
                groupImportLedgerId: "import-a",
                summary: summaryA),
            Wish(
                "wish-b1",
                "source-b1",
                "group-b",
                status: 공동구매내원함상태코드.닫힘,
                updatedMinute: 5,
                summary: GroupSummary("group-b", participants: 1, quantity: 5m)),
            Wish("wish-unassigned", "source-unassigned", string.Empty, updatedMinute: 10));
        var wishesClient = new FakeWishesClient(response);
        var viewModel = Create(wishesClient, new FakeDemandService());

        var loaded = await viewModel.LoadAsync();

        Assert.True(loaded);
        Assert.Same(response, viewModel.Result);
        Assert.Equal(4, viewModel.Wishes.Count);
        Assert.Same(response.원함목록[0], viewModel.FindWish("wish-a1"));
        Assert.Equal(1, wishesClient.CallCount);

        Assert.Collection(
            viewModel.Groups,
            group =>
            {
                Assert.Equal("group-a", group.AutoGroupId);
                Assert.Equal(["wish-a2", "wish-a1"], group.Wishes.Select(wish => wish.개별원함원장Id));
                Assert.False(group.AllWishesClosed);
                Assert.Equal("import-a", group.GroupImportLedgerId);
                Assert.Same(summaryA, group.Summary);
            },
            group =>
            {
                Assert.Equal("group-b", group.AutoGroupId);
                Assert.True(group.AllWishesClosed);
            });
        Assert.Null(viewModel.FindGroup("missing-group"));
    }

    [Fact]
    public async Task 활성원함_수량수정은_동일source와Revision_현재로그인주문자와목표를보존한다()
    {
        var wish = Wish("wish-1", "source/orderer-1/soy", "group-1", revision: 17);
        wish.상품키 = "soy";
        wish.상품명 = "국산 콩";
        wish.HS코드 = "1201.90";
        wish.온도코드 = "상온";
        wish.배송권키 = "scope:seoul";
        wish.배송권명 = "서울 생활권";
        wish.수량단위 = "kg";
        wish.거래유형 = 공동구매거래유형코드.B2B;
        wish.가격표시기준 = 공동구매가격표시기준코드.부가세별도;
        wish.구매조직참조키 = "org-7";
        wish.구매조직표시명 = "이웃 두부공방";
        wish.세금계산서필요 = true;
        wish.목표참여자수 = 12;
        wish.목표수량 = 500m;

        var wishesClient = new FakeWishesClient(Response(wish));
        var demandService = new FakeDemandService();
        var viewModel = Create(wishesClient, demandService);
        Assert.True(await viewModel.LoadAsync());

        var updated = await viewModel.UpdateQuantityAsync(wish, 35m);

        Assert.True(updated);
        Assert.Equal(2, wishesClient.CallCount);
        var preview = Assert.Single(demandService.PreviewRequests);
        var saved = Assert.Single(demandService.SaveRequests);
        Assert.Same(preview, saved);
        Assert.Equal("source/orderer-1/soy", saved.수요출처키);
        Assert.Equal(17, saved.개별원함기대Revision);
        Assert.Equal("orderer-1", saved.주문자키);
        Assert.Equal("로그인 주문자", saved.주문자표시명);
        Assert.Equal(35m, saved.희망수량);
        Assert.Equal(12, saved.목표참여자수);
        Assert.Equal(500m, saved.목표수량);
        Assert.Equal("scope:seoul", saved.배송권키);
        Assert.Equal(공동구매거래유형코드.B2B, saved.거래유형);
        Assert.Equal("org-7", saved.구매조직참조키);
        Assert.Equal(공동구매자동수요물류방식코드.후속검토, saved.물류방식);
        Assert.Equal(공동구매자동수요유형코드.관심표시, saved.수요유형);
        Assert.Equal(공동구매자동결제상태코드.미결제, saved.결제상태);
        Assert.StartsWith("wish-update:", saved.요청멱등키, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Revision충돌은_서버오류를사용자오류로전파하고_재조회하지않는다()
    {
        var wish = Wish("wish-stale", "source-stale", "group-stale", revision: 8);
        var wishesClient = new FakeWishesClient(Response(wish));
        var demandService = new FakeDemandService
        {
            SaveException = new SsalddelApiException(
                "원함 수정 API 실패: HTTP 409: revision conflict",
                409,
                "원함 수정",
                """{"detail":"revision conflict"}""",
                "trace-stale")
        };
        var viewModel = Create(wishesClient, demandService);
        Assert.True(await viewModel.LoadAsync());

        var updated = await viewModel.UpdateQuantityAsync(wish, 99m);

        Assert.False(updated);
        Assert.Equal(1, wishesClient.CallCount);
        Assert.Contains("HTTP 409", viewModel.ErrorMessage);
        Assert.Contains("revision conflict", viewModel.ErrorMessage);
        Assert.Null(viewModel.Notice);
    }

    [Fact]
    public async Task 닫힌원함은_클라이언트에서수정이차단되고_서버Command를보내지않는다()
    {
        var wish = Wish(
            "wish-closed",
            "source-closed",
            "group-closed",
            status: 공동구매내원함상태코드.닫힘);
        var demandService = new FakeDemandService();
        var viewModel = Create(new FakeWishesClient(Response(wish)), demandService);
        Assert.True(await viewModel.LoadAsync());

        var updated = await viewModel.UpdateQuantityAsync(wish, 10m);

        Assert.False(updated);
        Assert.Contains("닫힌 원함", viewModel.ErrorMessage);
        Assert.Empty(demandService.PreviewRequests);
        Assert.Empty(demandService.SaveRequests);
    }

    [Fact]
    public async Task 철회성공은_동일source를철회하고_내원함을강제새로고침한다()
    {
        var active = Wish("wish-1", "source-1", "group-1");
        var closed = Wish(
            "wish-1",
            "source-1",
            "group-1",
            status: 공동구매내원함상태코드.닫힘,
            revision: 2);
        var wishesClient = new FakeWishesClient(Response(active), Response(closed));
        var demandService = new FakeDemandService();
        var viewModel = Create(wishesClient, demandService);
        Assert.True(await viewModel.LoadAsync());

        var withdrawn = await viewModel.WithdrawAsync(active);

        Assert.True(withdrawn);
        Assert.Equal(2, wishesClient.CallCount);
        var request = Assert.Single(demandService.WithdrawRequests);
        Assert.Equal("source-1", request.DemandSourceKey);
        Assert.StartsWith("wish-withdraw:", request.IdempotencyKey, StringComparison.Ordinal);
        Assert.Equal(active.Revision, request.ExpectedWishRevision);
        Assert.Equal("주문자 앱 내 원함에서 철회", request.Reason);
        Assert.Equal(공동구매내원함상태코드.닫힘, Assert.Single(viewModel.Wishes).원함상태);
        Assert.Contains("비구속 원함만 닫았습니다", viewModel.Notice);
    }

    private static GroupPurchaseMyWishesViewModel Create(
        I공동구매내원함Client wishesClient,
        I비구속공동구매수요Service demandService)
        => new(
            wishesClient,
            demandService,
            new FakeCurrentUserContext(new 현재사용자Snapshot(
                "orderer-1",
                "로그인 주문자",
                ["Orderer"])));

    private static 공동구매내원함목록응답 Response(params 공동구매내원함응답[] wishes)
        => new()
        {
            전체건수 = wishes.Length,
            활성건수 = wishes.Count(GroupPurchaseMyWishesViewModel.IsActive),
            닫힘건수 = wishes.Count(wish => !GroupPurchaseMyWishesViewModel.IsActive(wish)),
            원함목록 = wishes
        };

    private static 공동구매내원함응답 Wish(
        string ledgerId,
        string sourceKey,
        string autoGroupId,
        string status = 공동구매내원함상태코드.활성,
        long revision = 1,
        int updatedMinute = 0,
        string groupImportLedgerId = "",
        공동구매자동집단요약응답? summary = null)
        => new()
        {
            개별원함원장Id = ledgerId,
            Revision = revision,
            수요출처키 = sourceKey,
            원함상태 = status,
            상품키 = "ingredient-1",
            상품명 = "공동구매 재료",
            HS코드 = "2106.90",
            희망수량 = 5m,
            수량단위 = "kg",
            배송권키 = "scope:seoul",
            배송권명 = "서울 생활권",
            온도코드 = "상온",
            자동집단Id = autoGroupId,
            같이수입원장Id = groupImportLedgerId,
            자동집단요약 = summary,
            생성시각Utc = new DateTime(2026, 7, 23, 0, 0, 0, DateTimeKind.Utc),
            수정시각Utc = new DateTime(2026, 7, 23, 0, updatedMinute, 0, DateTimeKind.Utc)
        };

    private static 공동구매자동집단요약응답 GroupSummary(
        string autoGroupId,
        int participants,
        decimal quantity)
        => new()
        {
            자동집단Id = autoGroupId,
            참여자수 = participants,
            총희망수량 = quantity,
            수량단위 = "kg",
            목표참여자수 = 10,
            목표수량 = 100m
        };

    private sealed class FakeCurrentUserContext(현재사용자Snapshot user) : ISsalddel현재사용자Context
    {
        public 현재사용자Snapshot 현재사용자 { get; } = user;
    }

    private sealed class FakeWishesClient(params 공동구매내원함목록응답[] responses)
        : I공동구매내원함Client
    {
        private readonly Queue<공동구매내원함목록응답> _responses = new(responses);
        private 공동구매내원함목록응답? _lastResponse;

        public int CallCount { get; private set; }

        public Task<공동구매내원함목록응답?> 내원함목록조회Async(
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            if (_responses.Count > 0)
            {
                _lastResponse = _responses.Dequeue();
            }

            return Task.FromResult(_lastResponse);
        }
    }

    private sealed class FakeDemandService : I비구속공동구매수요Service
    {
        public List<공동구매자동수요등록Command> PreviewRequests { get; } = [];
        public List<공동구매자동수요등록Command> SaveRequests { get; } = [];
        public List<WithdrawRequest> WithdrawRequests { get; } = [];
        public Exception? SaveException { get; init; }

        public Task<공동구매자동집단배치미리보기응답?> 수요배치미리보기Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            PreviewRequests.Add(request);
            return Task.FromResult<공동구매자동집단배치미리보기응답?>(new()
            {
                자동집단Id = "group-1"
            });
        }

        public Task<공동구매자동집단사용자응답?> 비구속수요저장Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            SaveRequests.Add(request);
            if (SaveException is not null)
            {
                return Task.FromException<공동구매자동집단사용자응답?>(SaveException);
            }

            return Task.FromResult<공동구매자동집단사용자응답?>(new()
            {
                자동집단Id = "group-1"
            });
        }

        public Task<공동구매자동수요철회응답?> 비구속수요철회Async(
            string demandSourceKey,
            string idempotencyKey,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            WithdrawRequests.Add(new WithdrawRequest(demandSourceKey, idempotencyKey, null, reason));
            return Task.FromResult<공동구매자동수요철회응답?>(new()
            {
                수요출처키 = demandSourceKey,
                철회완료 = true
            });
        }

        public Task<공동구매자동수요철회응답?> 비구속수요철회Async(
            string demandSourceKey,
            string idempotencyKey,
            long expectedWishRevision,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            WithdrawRequests.Add(new WithdrawRequest(
                demandSourceKey,
                idempotencyKey,
                expectedWishRevision,
                reason));
            return Task.FromResult<공동구매자동수요철회응답?>(new()
            {
                수요출처키 = demandSourceKey,
                철회완료 = true
            });
        }
    }

    private sealed record WithdrawRequest(
        string DemandSourceKey,
        string IdempotencyKey,
        long? ExpectedWishRevision,
        string? Reason);
}
