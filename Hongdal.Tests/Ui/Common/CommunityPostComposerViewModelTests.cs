using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Tests.Ui.Common;

public sealed class CommunityPostComposerViewModelTests
{
    [Fact]
    public void 글초안은_필수값과활동국가를검증한다()
    {
        var draft = new CommunityPostComposerDraftViewModel();

        Assert.NotNull(draft.Validate());

        draft.Nickname = "테스터";
        draft.Password = "secret";
        draft.Category = "자유";
        draft.WorkflowTag = "커뮤니티 신뢰";
        draft.RoleTag = "플랫폼 구성원";
        draft.Title = "공동수입 참여자를 찾습니다";
        draft.Body = "조건을 먼저 함께 확인하고 싶습니다.";
        draft.IsAuthorDisplayCountryPublic = true;
        draft.AuthorDisplayCountryCode = "K";

        Assert.Contains("ISO 알파-2", draft.Validate());

        draft.AuthorDisplayCountryCode = "KR";
        draft.AuthorDisplayCountryName = "대한민국";
        Assert.Null(draft.Validate());
    }

    [Fact]
    public async Task 임시저장은_비밀번호를제외하고_다시열때복원한다()
    {
        var store = new InMemoryDraftStore();
        using var first = CreateComposer(store);
        first.Configure("shipper", "화주");
        first.Draft.Nickname = "테스터";
        first.Draft.Password = "저장하면안됨";
        first.Draft.Title = "입고 예정 확인";
        first.Draft.Body = "업체별 입고 예정품을 확인합니다.";

        await first.SaveLocalDraftAsync();

        Assert.NotNull(store.Snapshot);
        Assert.DoesNotContain("저장하면안됨", System.Text.Json.JsonSerializer.Serialize(store.Snapshot));

        using var restored = CreateComposer(store);
        restored.Configure("shipper", "화주");
        await restored.LoadLocalDraftAsync();
        restored.Open();

        Assert.Equal("입고 예정 확인", restored.Draft.Title);
        Assert.Equal(string.Empty, restored.Draft.Password);
        Assert.Equal(CommunityComposerMessageKind.Info, restored.StatusKind);
    }

    [Fact]
    public void 수정시작은_게시글을초안에복사하고_설정을연다()
    {
        using var composer = CreateComposer(new InMemoryDraftStore());
        composer.Configure("shipper", "화주");

        composer.BeginEdit(new PlatformCommunityPostResponse
        {
            Id = 17,
            Nickname = "작성자",
            Category = "업무 질문",
            WorkflowTag = "창고·커머스 이행",
            RoleTag = "창고 관리자",
            Title = "입고 질문",
            Body = "검수 순서를 알고 싶습니다."
        });

        Assert.Equal(17, composer.EditingPostId);
        Assert.True(composer.IsOpen);
        Assert.True(composer.IsSettingsOpen);
        Assert.Equal("입고 질문", composer.Draft.Title);
        Assert.Equal(string.Empty, composer.Draft.Password);
    }

    [Fact]
    public void 운영자글쓰기는_로컬날짜와시간으로_예약발행시각을준비한다()
    {
        using var composer = CreateComposer(new InMemoryDraftStore());
        composer.Configure("platform", "운영자 정보 공유", allowScheduledPublication: true);

        composer.IsScheduledPublication = true;

        Assert.True(composer.AllowScheduledPublication);
        Assert.NotNull(composer.ScheduledPublishDateLocal);
        Assert.NotNull(composer.ScheduledPublishTimeLocal);
        Assert.True(composer.ScheduledPublishAtUtc > DateTime.UtcNow.AddMinutes(1));

        composer.Reset();

        Assert.False(composer.IsScheduledPublication);
        Assert.Null(composer.ScheduledPublishDateLocal);
        Assert.Null(composer.ScheduledPublishTimeLocal);
    }

    [Fact]
    public void 판매글은_본문없이도_상품수량가격결제정보로_요청을만든다()
    {
        var draft = new CommunityPostComposerDraftViewModel
        {
            Nickname = "햇살농원",
            Password = "secret",
            Category = "자유",
            WorkflowTag = "커뮤니티 신뢰",
            RoleTag = "생산자",
            Title = "오늘 수확한 복숭아를 판매합니다",
            IsSalesPost = true,
            SalesProductTitle = "햇복숭아 3kg 한 상자",
            SalesAvailableQuantity = 24,
            SalesQuantityUnit = "상자",
            SalesUnitPrice = 29_000,
            SalesCurrencyCode = "KRW",
            AcceptsDirectCash = true,
            AcceptsTossPayments = true,
            AllowsGroupPurchase = true
        };

        Assert.Null(draft.Validate());

        var request = draft.CreateRequest("shipper");
        var salesOffer = Assert.IsType<PlatformCommunityPostSalesOfferRequest>(request.SalesOffer);
        Assert.Equal("햇복숭아 3kg 한 상자", salesOffer.ProductTitle);
        Assert.Equal(24, salesOffer.AvailableQuantity);
        Assert.Contains(PlatformCommunitySalesPaymentMethodCodes.DirectCash, salesOffer.AcceptedPaymentMethods);
        Assert.Contains(PlatformCommunitySalesPaymentMethodCodes.TossPayments, salesOffer.AcceptedPaymentMethods);
        Assert.True(salesOffer.AllowsGroupPurchase);
        Assert.Equal(PlatformCommunityPostCategories.Sales, request.Category);
        Assert.False(request.IsReportBoardPost);
    }

    [Fact]
    public void 판매정보를_붙이면_판매게시판으로_자동분류되고_다른분류로_바뀌지않는다()
    {
        var draft = new CommunityPostComposerDraftViewModel
        {
            Category = PlatformCommunityPostCategories.ReportDispute,
            IsReportBoardPost = true
        };

        draft.IsSalesPost = true;
        draft.Category = PlatformCommunityPostCategories.General;
        draft.IsReportBoardPost = true;

        Assert.Equal(PlatformCommunityPostCategories.Sales, draft.Category);
        Assert.False(draft.IsReportBoardPost);

        var updateRequest = draft.CreateUpdateRequest();
        Assert.Equal(PlatformCommunityPostCategories.Sales, updateRequest.Category);
        Assert.False(updateRequest.IsReportBoardPost);
    }

    [Fact]
    public void 판매글은_결제방법이없으면_검증에실패한다()
    {
        var draft = new CommunityPostComposerDraftViewModel
        {
            Nickname = "판매자",
            Password = "secret",
            Category = "자유",
            WorkflowTag = "커뮤니티 신뢰",
            RoleTag = "판매자",
            Title = "판매글",
            IsSalesPost = true,
            SalesProductTitle = "상품",
            SalesAvailableQuantity = 1,
            SalesQuantityUnit = "개",
            SalesUnitPrice = 10_000,
            AcceptsDirectCash = false
        };

        Assert.Contains("결제 방법", draft.Validate());
    }

    private static CommunityPostComposerViewModel CreateComposer(
        ICommunityPostComposerDraftStore store)
    {
        var service = new PlatformCommunityService(new HttpClient(), null!);
        return new CommunityPostComposerViewModel(service, store);
    }

    private sealed class InMemoryDraftStore : ICommunityPostComposerDraftStore
    {
        public CommunityPostComposerSnapshot? Snapshot { get; private set; }

        public Task<CommunityPostComposerSnapshot?> LoadAsync(
            string appKey,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Snapshot);

        public Task SaveAsync(
            string appKey,
            CommunityPostComposerSnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            Snapshot = snapshot;
            return Task.CompletedTask;
        }

        public Task ClearAsync(
            string appKey,
            CancellationToken cancellationToken = default)
        {
            Snapshot = null;
            return Task.CompletedTask;
        }
    }
}
